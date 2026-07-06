using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Aldous-Broder algorithm implementation.
    /// </summary>
    public class AldousBroderMazeGenerator : MazeGenerator {
        private readonly Log _log = Log.ToConsole<AldousBroderMazeGenerator>();
        /// <summary>
        /// Generates a maze using Aldous-Broder algorithm in the specified
        /// layout.
        /// </summary>
        /// <remarks>
        /// Aldous-Broder's algorithm walks the maze via random neighbors. It
        /// does not stretch far by picking random maze cells. Some of the side
        /// effects is that if there are scattered areas, it has a high chance
        /// of not connecting all areas with sparce maze fill factors.
        /// </remarks>
        /// <param name="builder"><see cref="Maze2DBuilder" /> instance for
        /// the maze to be generated.</param>
        override public void GenerateMaze(Maze2DBuilder builder) {
            var currentCell = builder.PickNextCellToLink();
            var idleLoops = 0;
            var maxIdleLoops = builder.AllCells.Count * 100;
            while (!builder.IsFillComplete()) {
                _log.D(3, 1000, "AldousBroderMazeGenerator.GenerateMaze()");
                if (idleLoops >= maxIdleLoops) {
                    // A maze can have isolated maze areas do to Areas
                    // layout, so if we are stuck, we need to try picking a new
                    // starting point.
                    currentCell = builder.PickNextCellToLink();
                }
                if (builder.TryPickRandomNeighbor(currentCell, out var next)) {
                    if (!builder.IsConnected(next) ||
                        !builder.IsConnected(currentCell)) {
                        builder.Connect(currentCell, next);
                        idleLoops = 0;
                    } else {
                        idleLoops++;
                    }
                    currentCell = next;
                } else {
                    throw new NotImplementedException(
                        $"Investigate TryPickRandomNeighbor returning " +
                        $"empty for neighbors of {currentCell} in maze:\n" +
                        builder.ToString());
                }
            }
        }
    }
}
