using System;
using System.Linq;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Maze.PostProcessing;
using PlayersWorlds.Maps.Serializer;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// The entry point of the region contract: a synchronous factory that hands
    /// a game one generated <see cref="RegionView"/> per <see cref="RegionAddress"/>,
    /// generating it once and thereafter reloading it from the game's
    /// <see cref="IRegionStore"/>.
    /// </summary>
    /// <remarks>
    /// The call blocks — maze generation is bounded but can take a few
    /// milliseconds to tens of milliseconds — and it is the game's job to decide
    /// <i>when</i> to call it and off which thread. There is no "endless" call:
    /// each call produces one bounded region; the endless world emerges from the
    /// game calling repeatedly for the addresses it needs. Generation is
    /// deterministic: the same world seed and address always produce the same
    /// region.
    /// </remarks>
    public sealed class World {
        private readonly IRegionStore _store;
        private readonly int _worldSeed;
        private readonly Vector _regionMazeSize;
        private readonly Maze2DRendererOptions _renderOptions;
        private readonly Type _algorithm =
            GeneratorOptions.Algorithms.RecursiveBacktracker;
        private readonly GeneratorOptions.MazeFillFactor _fillFactor =
            GeneratorOptions.MazeFillFactor.Full;

        /// <summary>
        /// Creates a world façade with <b>square</b> 1×1 Block cells — the
        /// correct default for a game whose tiles are square in world space.
        /// </summary>
        /// <param name="store">The game's persistence seam. Use a
        /// <see cref="NullRegionStore"/> to always regenerate and persist
        /// nothing.</param>
        /// <param name="worldSeed">The base seed; each region derives its own
        /// seed from this and its address, so regions are independently
        /// reproducible.</param>
        /// <param name="regionMazeSize">The region size in <i>maze</i> cells
        /// (before Block expansion). The rendered Block size is larger; read it
        /// from <see cref="RegionView.Size"/>.</param>
        public World(IRegionStore store, int worldSeed, Vector regionMazeSize)
            : this(store, worldSeed, regionMazeSize,
                   new Vector(1, 1), new Vector(1, 1)) { }

        /// <summary>
        /// Creates a world façade with explicit Block cell sizing — the client
        /// owns the cell shape. Non-square sizes stretch the region; a game
        /// wanting square tiles passes equal, square sizes (the default ctor).
        /// </summary>
        /// <param name="store">The game's persistence seam.</param>
        /// <param name="worldSeed">The base seed for per-region seeding.</param>
        /// <param name="regionMazeSize">The region size in <i>maze</i> cells.
        /// </param>
        /// <param name="cellSize">The size, in Block cells, of each maze cell's
        /// walkable interior (corridor width). Use <c>(1, 1)</c> for square.
        /// </param>
        /// <param name="wallSize">The size, in Block cells, of the walls between
        /// maze cells (wall thickness). Use <c>(1, 1)</c> for square.</param>
        public World(IRegionStore store, int worldSeed, Vector regionMazeSize,
                     Vector cellSize, Vector wallSize) {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _worldSeed = worldSeed;
            regionMazeSize.ThrowIfNotAValidSize(nameof(regionMazeSize));
            _regionMazeSize = regionMazeSize;
            cellSize.ThrowIfNotAValidSize(nameof(cellSize));
            wallSize.ThrowIfNotAValidSize(nameof(wallSize));
            _renderOptions = Maze2DRendererOptions.RectCells(cellSize, wallSize);
        }

        /// <summary>
        /// Returns the region at <paramref name="address"/>: the stored region
        /// if the store has one, otherwise a freshly generated region that is
        /// then saved. Synchronous; no streaming.
        /// </summary>
        /// <param name="address">The region's address on the region lattice.
        /// </param>
        /// <returns>The region as a read-only <see cref="RegionView"/>.</returns>
        public RegionView GetOrCreate(RegionAddress address) {
            if (_store.TryLoad(address, out var serialized) &&
                !string.IsNullOrEmpty(serialized)) {
                var loaded = new AreaSerializer().Deserialize(serialized);
                return new RegionView(address, loaded);
            }
            var region = Generate(address);
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

        private Area Generate(RegionAddress address) {
            var randomSource = RandomSource.FromSeed(SeedFor(address));
            var options = new GeneratorOptions() {
                RandomSource = randomSource,
                MazeAlgorithm = _algorithm,
                FillFactor = _fillFactor,
            };
            var builder = new GeneratedWorld(randomSource)
                .AddLayer(AreaType.Maze, _regionMazeSize)
                .OfMaze(MazeStructureStyle.Border, options)
                .MarkLongestPath()
                .MarkDeadends();

            // Capture the POIs on the Border maze BEFORE ToMap discards the
            // marker attachments; bridge them into Block coordinates after.
            var mazeArea = builder.Map();
            var entrance = mazeArea.Grid.Single(v =>
                mazeArea[v]
                    .X<DijkstraDistance.IsLongestTrailStartExtension>() != null);
            var exit = mazeArea.Grid.Single(v =>
                mazeArea[v]
                    .X<DijkstraDistance.IsLongestTrailEndExtension>() != null);
            var deadEnds = mazeArea.X<DeadEnd.DeadEndsExtension>().DeadEnds;

            builder.ToMap(_renderOptions);
            var blockMap = builder.Map();

            BakePoi(blockMap, mazeArea, entrance, PoiKind.Entrance);
            BakePoi(blockMap, mazeArea, exit, PoiKind.Exit);
            foreach (var deadEnd in deadEnds
                         .Where(de => de != entrance && de != exit)) {
                BakePoi(blockMap, mazeArea, deadEnd, PoiKind.DeadEnd);
            }
            return blockMap;
        }

        // Translates a POI on the Border maze into the Block cell at the centre
        // of that maze cell, and tags it so the POI is a first-class,
        // serializable property of the region.
        private void BakePoi(Area blockMap, Area mazeArea, Vector mazeCell,
                             PoiKind kind) {
            var mapping = new CellsMapping(
                blockMap, mazeCell - mazeArea.Position, _renderOptions);
            blockMap[mapping.CenterPosition].Tags.Add(
                new Cell.CellTag(RegionTags.For(kind)));
        }
    }
}
