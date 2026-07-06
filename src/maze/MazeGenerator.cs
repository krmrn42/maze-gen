using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Areas.Evolving;
using PlayersWorlds.Maps.Maze.PostProcessing;
using PlayersWorlds.Maps.Renderers;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Base class for all maze generator algorithms. Also contains helper
    /// methods for maze generation.
    /// </summary>
    public abstract class MazeGenerator {
        /// <summary>
        /// When implemented in a derived class, generates a new maze.
        /// </summary>
        /// <param name="builder"><see cref="Maze2DBuilder" /> instance for
        /// the maze to be generated.</param>
        public abstract void GenerateMaze(Maze2DBuilder builder);
    }
}
