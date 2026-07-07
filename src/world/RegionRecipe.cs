using System;

namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// Describes <i>what fills a region's footprint</i> — the algorithm, how
    /// densely it fills, and the cell shape. A recipe is the region's content;
    /// <see cref="World"/> supplies the footprint (<c>regionSize</c>) and the
    /// default recipe, and each <see cref="World.GetOrCreate"/> call may pass
    /// its own recipe so different regions can be different kinds.
    /// </summary>
    /// <remarks>
    /// Recipes are immutable: start from an intent preset
    /// (<see cref="Maze"/>, <see cref="Corridors"/>) and
    /// layer overrides with the <c>With…</c> methods, each returning a new
    /// recipe. Cell shape defaults to <b>square</b> 1×1. (Room support —
    /// <c>Dungeon</c>/<c>Caverns</c> presets and <c>WithRooms</c> — is added
    /// next, behind this same type.)
    /// </remarks>
    public sealed class RegionRecipe {
        private static readonly Vector Square1 = new Vector(1, 1);

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

        private RegionRecipe(RegionAlgorithm algorithm, double fill,
                             Vector cellSize, Vector wallSize) {
            Algorithm = algorithm;
            Fill = fill;
            CellSize = cellSize;
            WallSize = wallSize;
        }

        /// <summary>A classic perfect maze — long winding corridors, fully
        /// connected.</summary>
        public static RegionRecipe Maze =>
            new RegionRecipe(RegionAlgorithm.RecursiveBacktracker, 1.0,
                             Square1, Square1);

        /// <summary>Straighter, corridor-biased passages.</summary>
        public static RegionRecipe Corridors =>
            new RegionRecipe(RegionAlgorithm.Sidewinder, 1.0, Square1, Square1);

        /// <summary>Returns a copy with a different algorithm.</summary>
        public RegionRecipe WithAlgorithm(RegionAlgorithm algorithm) =>
            new RegionRecipe(algorithm, Fill, CellSize, WallSize);

        /// <summary>Returns a copy with a different fill density (clamped to
        /// 0..1).</summary>
        public RegionRecipe WithFill(double fill) =>
            new RegionRecipe(Algorithm, Math.Max(0.0, Math.Min(1.0, fill)),
                             CellSize, WallSize);

        /// <summary>Returns a copy with square cells of the given side (walls
        /// and corridors equal — the usual game choice).</summary>
        public RegionRecipe WithCells(int square) =>
            WithCells(new Vector(square, square), new Vector(square, square));

        /// <summary>Returns a copy with explicit corridor and wall cell sizes
        /// (non-square deliberately stretches the region).</summary>
        public RegionRecipe WithCells(Vector cellSize, Vector wallSize) =>
            new RegionRecipe(Algorithm, Fill, cellSize, wallSize);
    }
}
