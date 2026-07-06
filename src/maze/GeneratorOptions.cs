using System;
using System.Collections.Generic;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// <see cref="MazeGenerator"/> options.
    /// </summary>
    public class GeneratorOptions {
        /// <summary>
        /// How much to fill the maze. <see
        /// cref="Maze2DBuilder.IsFillComplete() "/>
        /// implementation for details.
        /// </summary>
        public MazeFillFactor FillFactor { get; set; }
        /// <summary>
        /// How (and if) to generate areas in the maze.
        /// </summary>
        public AreaGenerationMode AreaGeneration { get; set; }
        /// <summary>
        /// When <see cref="AreaGeneration"/> is set to
        /// <see cref="AreaGenerationMode.Auto"/>, this generator will be used to
        /// generate areas.
        /// </summary>
        public AreaGenerator AreaGenerator { get; set; }
        /// <summary>
        /// Algorithm to use when generating the maze. Has to be a type derived
        /// from <see cref="MazeGenerator"/>.
        /// </summary>
        public Type MazeAlgorithm { get; set; }
        /// <summary>
        /// A source of random numbers to use when generating the maze.
        /// </summary>
        public RandomSource RandomSource { get; set; }
        public MazeStructureStyle MazeStructureStyle { get; set; }
        public Maze2DRenderer.Maze2DRendererOptions MazeRendererOptions { get; set; }

        /// <summary>
        /// How much to fill the maze. <see
        /// cref="Maze2DBuilder.IsFillComplete()"/>
        /// implementation for details.
        /// </summary>
        public enum MazeFillFactor {
            /// <summary>
            /// All cells of the maze will be visited.
            /// </summary>
            Full,
            /// <summary>
            /// The algorithm stops as soon as it visits <c>x = 0</c> and
            /// <c>x = maze.Size.X - 1</c>.
            /// </summary>
            FullWidth,
            /// <summary>
            /// The algorithm stops as soon as it visits <c>y = 0</c> and
            /// <c>x = maze.Size.Y - 1</c>.
            /// </summary>
            FullHeight,
            /// <summary>
            /// The algorithm stops as soon as it visits at least 25% of maze
            /// cells.
            /// </summary>
            Quarter,
            /// <summary>
            /// The algorithm stops as soon as it visits at least 50% of maze
            /// cells.
            /// </summary>
            Half,
            /// <summary>
            /// The algorithm stops as soon as it visits at least 75% of maze
            /// cells.
            /// </summary>
            ThreeQuarters,
            /// <summary>
            /// The algorithm stops as soon as it visits at least 90% of maze
            /// cells.
            /// </summary>
            NinetyPercent
        }

        /// <summary>
        /// How to generate various areas in the maze.
        /// </summary>
        public enum AreaGenerationMode {
            /// <summary>
            /// Specify the areas manually in
            /// <see cref="Area"/>.
            /// </summary>
            Manual,
            /// <summary>
            /// Use a generator to generate the areas. The generator can be
            /// specified in <see cref="GeneratorOptions.MazeAlgorithm" />.
            /// </summary>
            Auto
        }

        /// <summary>
        /// Maze generation algorithms implemented in this library.
        /// </summary>
        public static class Algorithms {
            /// <summary>
            /// <see cref="RecursiveBacktrackerMazeGenerator" />.
            /// </summary>
            public static readonly Type Default =
                typeof(RecursiveBacktrackerMazeGenerator);
            /// <summary>
            /// <see cref="AldousBroderMazeGenerator" />.
            /// </summary>
            public static readonly Type AldousBroder =
                typeof(AldousBroderMazeGenerator);
            /// <summary>
            /// <see cref="HuntAndKillMazeGenerator" />.
            /// </summary>
            public static readonly Type HuntAndKill =
                typeof(HuntAndKillMazeGenerator);
            /// <summary>
            /// <see cref="RecursiveBacktrackerMazeGenerator" />.
            /// </summary>
            public static readonly Type RecursiveBacktracker =
                typeof(RecursiveBacktrackerMazeGenerator);
            /// <summary>
            /// <see cref="SidewinderMazeGenerator" />.
            /// </summary>
            public static readonly Type Sidewinder =
                typeof(SidewinderMazeGenerator);
            /// <summary>
            /// <see cref="WilsonsMazeGenerator" />.
            /// </summary>
            public static readonly Type Wilsons =
                typeof(WilsonsMazeGenerator);
            /// <summary>
            /// <see cref="BinaryTreeMazeGenerator" />.
            /// </summary>
            public static readonly Type BinaryTree =
                typeof(BinaryTreeMazeGenerator);
        }
    }
}
