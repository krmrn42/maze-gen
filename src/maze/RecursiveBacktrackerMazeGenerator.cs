using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Recursive Backtracker algorithm implementation.
    /// </summary>
    public class RecursiveBacktrackerMazeGenerator : MazeGenerator {
        private readonly Log _log = Log.ToConsole<RecursiveBacktrackerMazeGenerator>();
        /// <summary>
        /// Generates a maze using Recursive Backtracker algorithm in the
        /// specified layout.
        /// </summary>
        /// <remarks>
        /// Just like Aldous-Broder's algorithm, this algorithm walks the maze
        /// in a modest way without trying to reach far. So in the same way it
        /// has a high chance of not connecting all areas with sparce maze fill 
        /// factors.
        /// </remarks>
        /// <param name="builder"><see cref="Maze2DBuilder" /> instance for
        /// the maze to be generated.</param>
        override public void GenerateMaze(Maze2DBuilder builder) {
            var stack = new Stack<Vector>();
            stack.Push(builder.PickNextCellToLink());
            while (!builder.IsFillComplete() && stack.Count > 0) {
                _log.D(3, 1000, "RecursiveBacktrackerMazeGenerator.GenerateMaze()");
                var currentCell = stack.Peek();
                if (builder.TryPickRandomNeighbor(
                        currentCell, out var nextCell, true)) {
                    builder.Connect(currentCell, nextCell);
                    stack.Push(nextCell);
                } else {
                    stack.Pop();
                }
            }
        }
    }
}
