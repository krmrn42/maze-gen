using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// Describes <i>what fills a region's footprint</i> — the algorithm, how
    /// densely it fills, the cell shape, and any rooms. A recipe is the region's
    /// content; <see cref="World"/> supplies the footprint (<c>regionSize</c>)
    /// and the default recipe, and each <see cref="World.GetOrCreate"/> call may
    /// pass its own recipe so different regions can be different kinds.
    /// </summary>
    /// <remarks>
    /// Recipes are immutable: start from an intent preset (<see cref="Maze"/>,
    /// <see cref="Corridors"/>, <see cref="Dungeon"/>, <see cref="Caverns"/>)
    /// and layer overrides with the <c>With…</c> methods, each returning a new
    /// recipe. Cell shape defaults to <b>square</b> 1×1.
    /// </remarks>
    public sealed class RegionRecipe {
        private static readonly Vector Square1 = new Vector(1, 1);
        private static readonly RoomRequest[] NoRooms = new RoomRequest[0];

        private readonly RoomRequest[] _rooms;

        /// <summary>The generation algorithm.</summary>
        public RegionAlgorithm Algorithm { get; }

        /// <summary>How densely the algorithm fills the footprint, 0..1
        /// (1 = fully connected).</summary>
        public double Fill { get; }

        /// <summary>The size, in Block cells, of each cell's walkable interior
        /// (corridor width).</summary>
        public Vector CellSize { get; }

        /// <summary>The size, in Block cells, of the walls between cells (wall
        /// thickness).</summary>
        public Vector WallSize { get; }

        internal IReadOnlyList<RoomRequest> Rooms => _rooms;

        private RegionRecipe(RegionAlgorithm algorithm, double fill,
                             Vector cellSize, Vector wallSize,
                             RoomRequest[] rooms) {
            Algorithm = algorithm;
            Fill = fill;
            CellSize = cellSize;
            WallSize = wallSize;
            _rooms = rooms;
        }

        /// <summary>A classic perfect maze — long winding corridors, fully
        /// connected, no rooms.</summary>
        public static RegionRecipe Maze =>
            new RegionRecipe(RegionAlgorithm.RecursiveBacktracker, 1.0,
                             Square1, Square1, NoRooms);

        /// <summary>Straighter, corridor-biased passages, no rooms.</summary>
        public static RegionRecipe Corridors =>
            new RegionRecipe(RegionAlgorithm.Sidewinder, 1.0, Square1, Square1,
                             NoRooms);

        /// <summary>Corridors linking several walled halls.</summary>
        public static RegionRecipe Dungeon =>
            Maze.WithRooms(6, new Vector(6, 6), new Vector(10, 10),
                           RoomKind.Hall);

        /// <summary>Organic caverns — larger, less regular open rooms.
        /// </summary>
        public static RegionRecipe Caverns =>
            new RegionRecipe(RegionAlgorithm.AldousBroder, 1.0, Square1, Square1,
                             NoRooms)
                .WithRooms(7, new Vector(5, 5), new Vector(11, 11),
                           RoomKind.Cave);

        /// <summary>Returns a copy with a different algorithm.</summary>
        public RegionRecipe WithAlgorithm(RegionAlgorithm algorithm) =>
            new RegionRecipe(algorithm, Fill, CellSize, WallSize, _rooms);

        /// <summary>Returns a copy with a different fill density (clamped to
        /// 0..1).</summary>
        public RegionRecipe WithFill(double fill) =>
            new RegionRecipe(Algorithm, Math.Max(0.0, Math.Min(1.0, fill)),
                             CellSize, WallSize, _rooms);

        /// <summary>Returns a copy with square cells of the given side (walls
        /// and corridors equal — the usual game choice).</summary>
        public RegionRecipe WithCells(int square) =>
            WithCells(new Vector(square, square), new Vector(square, square));

        /// <summary>Returns a copy with explicit corridor and wall cell sizes
        /// (non-square deliberately stretches the region).</summary>
        public RegionRecipe WithCells(Vector cellSize, Vector wallSize) =>
            new RegionRecipe(Algorithm, Fill, cellSize, wallSize, _rooms);

        /// <summary>
        /// Returns a copy that also auto-places <paramref name="count"/> rooms
        /// of <paramref name="kind"/>, each sized between
        /// <paramref name="minSize"/> and <paramref name="maxSize"/> (in world
        /// Block cells — the same unit as <c>regionSize</c>), tagged with
        /// <paramref name="tags"/>. Call it more than once to mix kinds. Rooms
        /// are placed best-effort without overlapping.
        /// </summary>
        /// <remarks>
        /// Rooms snap to the underlying maze grid (pitch = cell + wall, i.e. 2
        /// world cells for square 1×1 cells), so a room smaller than ~3 world
        /// cells collapses to a single corridor cell and is not visible as a
        /// room. Use ≥3 (and a footprint with room to place them).
        /// </remarks>
        /// <param name="count">How many rooms of this kind to place.</param>
        /// <param name="minSize">Minimum room size, in world cells.</param>
        /// <param name="maxSize">Maximum room size, in world cells.</param>
        /// <param name="kind">The room's structural kind.</param>
        /// <param name="tags">Open-ended semantic tags (e.g. "armory").</param>
        public RegionRecipe WithRooms(int count, Vector minSize, Vector maxSize,
                                      RoomKind kind = RoomKind.Hall,
                                      params string[] tags) {
            if (count < 0) {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            minSize.ThrowIfNotAValidSize(nameof(minSize));
            maxSize.ThrowIfNotAValidSize(nameof(maxSize));
            var request = new RoomRequest(count, minSize, maxSize, kind,
                tags ?? new string[0]);
            return new RegionRecipe(Algorithm, Fill, CellSize, WallSize,
                _rooms.Concat(new[] { request }).ToArray());
        }
    }
}
