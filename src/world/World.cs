using System;
using System.Linq;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Maze.PostProcessing;
using PlayersWorlds.Maps.Serializer;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// A persistent, seeded space of regions — created once (a game-start
    /// event) and then asked for one region at a time as the player moves (a
    /// positioning / environment-loading event).
    /// </summary>
    /// <remarks>
    /// The world holds what every region shares and inherits: the seed, the
    /// store, the region footprint (<c>regionSize</c>), and a default recipe.
    /// Each <see cref="GetOrCreate"/> may pass its own <see cref="RegionRecipe"/>
    /// so different regions can be different kinds. Regions tile a <b>uniform
    /// lattice</b>: every region is <c>regionSize</c> Block cells, which is
    /// exactly what <see cref="RegionView.Size"/> reports and the pitch
    /// <see cref="RegionAddress.ToWorld"/> uses.
    ///
    /// The call blocks (bounded — a few to tens of ms); the game decides when to
    /// call it and off which thread. Generation is deterministic: same world
    /// seed + address always produce the same region.
    /// </remarks>
    public sealed class World {
        private readonly IRegionStore _store;
        private readonly int _worldSeed;
        private readonly Vector _regionSize;
        private readonly RegionRecipe _defaultRecipe;

        /// <summary>
        /// Creates a world.
        /// </summary>
        /// <param name="store">The game's persistence seam. Use a
        /// <see cref="NullRegionStore"/> to always regenerate and persist
        /// nothing.</param>
        /// <param name="worldSeed">The base seed; each region derives its own
        /// seed from this and its address, so regions are independently
        /// reproducible.</param>
        /// <param name="regionSize">The region's footprint in the world, in
        /// Block cells — the uniform lattice pitch. This is exactly what
        /// <see cref="RegionView.Size"/> reports; the number of corridors/rooms
        /// within it is derived from the recipe's cell sizing.</param>
        /// <param name="defaultRecipe">What a region is generated as unless a
        /// <see cref="GetOrCreate"/> call overrides it. Defaults to
        /// <see cref="RegionRecipe.Maze"/>.</param>
        public World(IRegionStore store, int worldSeed, Vector regionSize,
                     RegionRecipe defaultRecipe = null) {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _worldSeed = worldSeed;
            regionSize.ThrowIfNotAValidSize(nameof(regionSize));
            _regionSize = regionSize;
            _defaultRecipe = defaultRecipe ?? RegionRecipe.Maze;
        }

        /// <summary>
        /// Returns the region at <paramref name="address"/>: the stored region
        /// if the store has one, otherwise a freshly generated region (using
        /// <paramref name="recipe"/>, or the world's default) that is then
        /// saved. Synchronous; no streaming.
        /// </summary>
        /// <param name="address">The region's address on the lattice.</param>
        /// <param name="recipe">The recipe for this region if it must be
        /// generated. Ignored if the region is already stored (a region's kind
        /// is fixed when it is first created). Defaults to the world's default
        /// recipe.</param>
        /// <returns>The region as a read-only <see cref="RegionView"/>.</returns>
        public RegionView GetOrCreate(RegionAddress address,
                                      RegionRecipe recipe = null) {
            if (_store.TryLoad(address, out var serialized) &&
                !string.IsNullOrEmpty(serialized)) {
                var loaded = new AreaSerializer().Deserialize(serialized);
                return new RegionView(address, loaded);
            }
            var region = Generate(address, recipe ?? _defaultRecipe);
            _store.Save(address, new AreaSerializer().Serialize(region));
            return new RegionView(address, region);
        }

        private int SeedFor(RegionAddress address) {
            var seed = _worldSeed;
            foreach (var component in address.Value.Value) {
                seed = unchecked(seed * 397 + component);
            }
            return seed;
        }

        private Area Generate(RegionAddress address, RegionRecipe recipe) {
            var randomSource = RandomSource.FromSeed(SeedFor(address));
            var renderOptions =
                Maze2DRendererOptions.RectCells(recipe.CellSize, recipe.WallSize);
            var mazeCells =
                DeriveMazeCells(_regionSize, recipe.CellSize, recipe.WallSize);
            var options = new GeneratorOptions() {
                RandomSource = randomSource,
                MazeAlgorithm = recipe.Algorithm.GeneratorType,
                FillFactor = MapFill(recipe.Fill),
                // We place rooms ourselves (below), so the builder carves around
                // the existing areas rather than distributing its own.
                AreaGeneration = GeneratorOptions.AreaGenerationMode.Manual,
            };
            var builder =
                new GeneratedWorld(randomSource).AddLayer(AreaType.Maze, mazeCells);
            foreach (var room in recipe.Rooms) {
                var min = ClampToGrid(
                    ToMazeCells(room.MinSize, recipe.CellSize, recipe.WallSize),
                    mazeCells);
                var max = ClampToGrid(
                    ToMazeCells(room.MaxSize, recipe.CellSize, recipe.WallSize),
                    mazeCells);
                builder = builder.WithAreas(new[] { AreaTypeOf(room.Kind) },
                    room.Tags, room.Count, min, max);
            }
            builder = builder
                .OfMaze(MazeStructureStyle.Border, options)
                .MarkLongestPath()
                .MarkDeadends();

            // Capture POIs on the Border maze before the Block conversion drops
            // the marker attachments; bridge them into Block coordinates after.
            var mazeArea = builder.Map();
            var entrance = mazeArea.Grid.Single(v =>
                mazeArea[v]
                    .X<DijkstraDistance.IsLongestTrailStartExtension>() != null);
            var exit = mazeArea.Grid.Single(v =>
                mazeArea[v]
                    .X<DijkstraDistance.IsLongestTrailEndExtension>() != null);
            var deadEnds = mazeArea.X<DeadEnd.DeadEndsExtension>().DeadEnds;

            // Render into a footprint-sized canvas so RegionView.Size ==
            // regionSize exactly; any remainder past the maze stays impassable.
            var blockMap = Area.Create(Vector.Zero(_regionSize.Dimensions),
                _regionSize, AreaType.Environment);
            new MazeAreaStyleConverter()
                .ConvertMazeBorderToBlock(mazeArea, blockMap, renderOptions);

            BakePoi(blockMap, mazeArea, entrance, PoiKind.Entrance, renderOptions);
            BakePoi(blockMap, mazeArea, exit, PoiKind.Exit, renderOptions);
            foreach (var deadEnd in deadEnds
                         .Where(de => de != entrance && de != exit)) {
                BakePoi(blockMap, mazeArea, deadEnd, PoiKind.DeadEnd,
                        renderOptions);
            }
            return blockMap;
        }

        // Largest maze-cell grid whose Block rendering fits inside the region
        // footprint: footprint = wall + N·(cell + wall) per dimension.
        private static Vector DeriveMazeCells(Vector footprint, Vector cellSize,
                                              Vector wallSize) {
            var cells = new int[footprint.Dimensions];
            for (var i = 0; i < footprint.Dimensions; i++) {
                var pitch = cellSize.Value[i] + wallSize.Value[i];
                var n = (footprint.Value[i] - wallSize.Value[i]) / pitch;
                if (n < 1) {
                    throw new ArgumentException(
                        $"regionSize {footprint} is too small for the cell/wall " +
                        "sizing; each dimension must fit at least one cell.");
                }
                cells[i] = n;
            }
            return new Vector(cells);
        }

        private static AreaType AreaTypeOf(RoomKind kind) => kind switch {
            RoomKind.Hall => AreaType.Hall,
            RoomKind.Cave => AreaType.Cave,
            RoomKind.Blocked => AreaType.Fill,
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

        // Rooms are sized in world (Block) cells; snap each dimension to the
        // maze grid (one maze cell renders to cell + wall Block cells).
        private static Vector ToMazeCells(Vector worldSize, Vector cellSize,
                                          Vector wallSize) {
            var cells = new int[worldSize.Dimensions];
            for (var i = 0; i < worldSize.Dimensions; i++) {
                var pitch = cellSize.Value[i] + wallSize.Value[i];
                cells[i] = Math.Max(1, worldSize.Value[i] / pitch);
            }
            return new Vector(cells);
        }

        // Keeps a room's maze-cell size within the maze grid so the distributor
        // never gets an empty placement range for an oversized room.
        private static Vector ClampToGrid(Vector size, Vector mazeCells) {
            var clamped = new int[size.Dimensions];
            for (var i = 0; i < size.Dimensions; i++) {
                clamped[i] = Math.Max(1, Math.Min(size.Value[i], mazeCells.Value[i]));
            }
            return new Vector(clamped);
        }

        private static GeneratorOptions.MazeFillFactor MapFill(double fill) {
            if (fill >= 0.95) return GeneratorOptions.MazeFillFactor.Full;
            if (fill >= 0.825) return GeneratorOptions.MazeFillFactor.NinetyPercent;
            if (fill >= 0.625) return GeneratorOptions.MazeFillFactor.ThreeQuarters;
            if (fill >= 0.375) return GeneratorOptions.MazeFillFactor.Half;
            return GeneratorOptions.MazeFillFactor.Quarter;
        }

        // Translates a POI on the Border maze into the Block cell at the centre
        // of that maze cell, and tags it so the POI is a first-class,
        // serializable property of the region.
        private static void BakePoi(Area blockMap, Area mazeArea, Vector mazeCell,
                                    PoiKind kind,
                                    Maze2DRendererOptions renderOptions) {
            var mapping = new CellsMapping(
                blockMap, mazeCell - mazeArea.Position, renderOptions);
            blockMap[mapping.CenterPosition].Tags.Add(
                new Cell.CellTag(RegionTags.For(kind)));
        }
    }
}
