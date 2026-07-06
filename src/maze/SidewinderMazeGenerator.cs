using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static PlayersWorlds.Maps.Maze.GeneratorOptions;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Sidewinder algorithm implementation.
    /// </summary>
    public class SidewinderMazeGenerator : MazeGenerator {
        /// <summary>
        /// Generates a maze using Sidewinder algorithm in the
        /// specified layout.
        /// </summary>
        /// <param name="builder"><see cref="Maze2DBuilder" /> instance for
        /// the maze to be generated.</param>
        override public void GenerateMaze(Maze2DBuilder builder) {
            builder.ThrowIfIncompatibleOptions(new GeneratorOptions() {
                FillFactor = MazeFillFactor.Full,
            });
            var cellStates = builder.Random.NextBytes(builder.AllCells.Count);
            var currentY = 0;
            var run = new List<Vector>();
            var i = 0;
            foreach (var currentCell in builder.AllCells) {
                if (currentCell.Y != currentY) {
                    run.Clear();
                    currentY = currentCell.Y;
                }

                // we don't check if the cell is already connected
                // because Sidewinder moves through cells in order. A cell
                // might have already been connected from the lower left cells,
                // but we still need to work through the right top cells.

                if (builder.CanConnect(currentCell, currentCell + Vector.North2D))
                    run.Add(currentCell);

                var canConnectNorth = run.Count > 0;
                var runCandidate = Vector.Empty;
                if (canConnectNorth) {
                    runCandidate = run[cellStates[i] % run.Count];
                    // see SidewinderMazeGeneratorTest.ArchShapedAreasLeftExit
                    // in case the random run cell cannot be connected to its
                    // north neighbor, we can try picking any run cell that can:
                    if (!builder.CanConnect(runCandidate, runCandidate + Vector.North2D)) {
                        runCandidate = builder.Random.RandomOf(run
                            .Where(cell =>
                                builder.CanConnect(cell, cell + Vector.North2D))
                            .ToList());
                        canConnectNorth = !runCandidate.IsEmpty;
                    }
                }

                var connectEast = cellStates[i] % 2 == 0 || !canConnectNorth;
                var canConnectEast =
                    builder.CanConnect(currentCell, currentCell + Vector.East2D);

                var cellsToLink = new Vector[2];

                // if the random was to connect east, we will try to connect
                // east
                // if there are no adjacent cells to the east, we will
                // try to connect north
                // if we cannot connect east nor can connect north, we will
                // continue with the lowest leftmost unconnected cell.
                if (connectEast && canConnectEast) {
                    cellsToLink[0] = currentCell;
                    cellsToLink[1] = builder.NeighborsOf(currentCell).Contains(currentCell + Vector.East2D) ? currentCell + Vector.East2D : Vector.Empty;
                } else if (canConnectNorth) {
                    cellsToLink[0] = runCandidate;
                    cellsToLink[1] = builder.NeighborsOf(runCandidate).Contains(runCandidate + Vector.North2D) ? runCandidate + Vector.North2D : Vector.Empty;
                    run.Clear();
                } else if (!builder.MazeArea[currentCell].HasLinks()) {
                    // This link is not connected, and won't be connected
                    // because of this maze geometry. Let's connect it to
                    // any other cell so that it's not left out.
                    if (builder.TryPickRandomNeighbor(
                            currentCell, out var cellToLink)) {
                        cellsToLink[0] = currentCell;
                        cellsToLink[1] = cellToLink;
                    } else {
                        // this cell doesn't have any neighbors we can
                        // connect. Perhaps it is surrounded by walls,
                        // filled areas, or halls.
                        Trace.TraceWarning(
                                $"SidewinderMazeGenerator could not find any " +
                                $"neighbors to connect {currentCell} to.");
                    }
                }
                if (cellsToLink.All(c => !c.IsEmpty)) {
                    builder.Connect(cellsToLink[0], cellsToLink[1]);
                }
                i++;
            }
        }
    }
}
