using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Hunt-and-kill algorithm implementation.
    /// </summary>
    public class HuntAndKillMazeGenerator : MazeGenerator {
        private readonly Log _log = Log.ToConsole<HuntAndKillMazeGenerator>();
        /// <summary>
        /// Generates a maze using Hunt-and-kill algorithm in the specified
        /// layout.
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
            var currentCell = builder.PickNextCellToLink();
            while (!builder.IsFillComplete()) {
                _log.D(3, 10000, "HuntAndKillMazeGenerator.GenerateMaze()");
                if (builder.TryPickRandomNeighbor(
                        currentCell, out var nextCell, onlyUnconnected: true)) {
                    builder.Connect(currentCell, nextCell);
                    currentCell = nextCell;
                } else {
                    var hunt =
                        builder.GetPrioritizedCellsToConnect()
                               .FirstOrDefault(
                                    cell =>
                                        builder.NeighborsOf(cell)
                                               .Any(builder.IsConnected));
                    if (!hunt.IsEmpty) {
                        builder.Connect(
                            hunt,
                            builder.NeighborsOf(hunt)
                                   .Where(builder.IsConnected)
                                   .First());
                        currentCell = hunt;
                    } else {
                        // we don't have any unconnected cells with connected
                        // neighbors, meaning there are unconnected areas in 
                        // the maze field.
                        var attempts = 100;
                        do {
                            nextCell = builder.PickNextCellToLink();
                            attempts--;
                        } while (currentCell == nextCell && attempts > 0);
                        if (currentCell == nextCell && attempts == 0) {
                            break;
                        }
                        currentCell = nextCell;
                    }
                }
            }
        }
    }
}
