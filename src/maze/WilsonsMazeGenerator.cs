using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Maze.PostProcessing;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Wilson's algorithm implementation.
    /// </summary>
    /// <remarks>
    /// Wilson's rarely checks for priority cells, so it might not connect all
    /// areas.</remarks>
    public class WilsonsMazeGenerator : MazeGenerator {
        private readonly Log _log = Log.ToConsole<WilsonsMazeGenerator>();
        /// <summary>
        /// Generates a maze using Wilson's algorithm in the
        /// specified layout.
        /// </summary>
        /// <param name="builder"><see cref="Maze2DBuilder" /> instance for
        /// the maze to be generated.</param>
        override public void GenerateMaze(Maze2DBuilder builder) {
            // mark a random cell as connected
            // TODO: According to the algorithm, we need to mark an initial cell
            //       to make sure the algorithm concludes without creating one
            //       single path spanning the whole maze. 
            //       Every next pass tries to find a cell that's already been
            //       marked as connected (e.g., the initial cell).
            //       Consider a setup when areas isolate one maze area from
            //       another, making it impossible to find an already connected
            //       cell. In this case, we need to poke a cell in every
            //       isolated area.

            var visitedCells = new HashSet<Vector>();
            foreach (var cellGroup in builder.CellGroups) {
                visitedCells.Add(builder.Random.RandomOf(cellGroup, cellGroup.Count));
            }

            while (!builder.IsFillComplete()) {
                _log.D(3, 1000, "WilsonsMazeGenerator.GenerateMaze() 1");
                var walkPath = new List<Vector>();
                var nextCell = builder.PickNextCellToLink(); // 4x3

                if (visitedCells.Contains(nextCell))
                    continue;

                while (!visitedCells.Contains(nextCell)) {
                    _log.D(3, 10000, "WilsonsMazeGenerator.GenerateMaze() 2");
                    var containsAt = walkPath.IndexOf(nextCell);
                    if (containsAt >= 0)
                        walkPath.RemoveRange(containsAt + 1, walkPath.Count - containsAt - 1);
                    else walkPath.Add(nextCell);
                    builder.TryPickRandomNeighbor(nextCell, out nextCell, honorPriority: false); //3x3, 4x3
                }
                ;

                if (!nextCell.IsEmpty) {
                    walkPath.Add(nextCell);
                }

                for (var i = 0; i < walkPath.Count - 1; i++) {
                    builder.Connect(walkPath[i], walkPath[i + 1]);
                    visitedCells.Add(walkPath[i]);
                }
                visitedCells.Add(walkPath.Last());
            }
        }
    }
}
