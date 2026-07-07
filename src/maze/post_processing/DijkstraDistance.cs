using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze.PostProcessing {
    /// <summary>
    /// Dijkstra's algorithm to find the paths between cells. Used in many
    /// cases, for example to find the longest path in the maze, or the best
    /// start and end points.
    /// </summary>
    public static class DijkstraDistance {
        /// <summary>
        /// An extension object that contains the list of all cells that
        /// constitute the longest trail in the maze.
        /// </summary>
        public class LongestTrailExtension {
            /// <summary>
            /// Creates an instance of the LongestTrailExtension.
            /// </summary>
            /// <param name="longestTrail"></param>
            public LongestTrailExtension(IEnumerable<Vector> longestTrail) {
                LongestTrail = longestTrail.ToList();
            }

            /// <summary>
            /// The list of all cells that constitute the longest trail in the
            /// maze.
            /// </summary>
            public List<Vector> LongestTrail { get; private set; }
        }

        /// <summary>
        /// An extension object that denotes a longest trail.
        /// </summary>
        public class IsLongestTrailExtension { }

        /// <summary>
        /// An extension object that denotes a longest trail start.
        /// </summary>
        public class IsLongestTrailStartExtension { }

        /// <summary>
        /// An extension object that denotes a longest trail end.
        /// </summary>
        public class IsLongestTrailEndExtension { }

        /// <summary>
        /// Finds Dijkstra distances for the given cell.
        /// </summary>
        /// <param name="mazeArea">Area of the maze.</param>
        /// <param name="startingCell">The cell to start the BFS walk.</param>
        /// <returns></returns>
        public static Dictionary<Vector, int> Find(Area mazeArea, Vector startingCell) {
            var distances = new Dictionary<Vector, int> {
                { startingCell, 0 }
            };
            var stack = new Stack<Vector>();
            stack.Push(startingCell);
            Vector nextCell;
            while (stack.Count > 0) {
                nextCell = stack.Pop();
                var distance = distances[nextCell];
                foreach (var neighbor in mazeArea[nextCell].Links()) {
                    // if a maze has loops, we have to check if we are building
                    // a shorter path or a longer one.
                    if (!distances.ContainsKey(neighbor)) {
                        distances.Add(neighbor, distance + 1);
                    } else if (distance + 1 < distances[neighbor]) {
                        distances[neighbor] = distance + 1;
                    } else continue;
                    stack.Push(neighbor);
                }
            }
            return distances;
        }

        /// <summary>
        /// Finds Dijkstra distances for the given cell using connectable
        /// neighbors as opposed to actual maze links.
        /// </summary>
        /// <param name="builder">Maze builder that provides info on maze
        /// structure.</param>
        /// <param name="startingCell">The cell to start the BFS walk.</param>
        /// <returns></returns>
        public static Dictionary<Vector, int> FindRaw(Maze2DBuilder builder,
                                                      Vector startingCell) {
            var distances = new Dictionary<Vector, int> {
                { startingCell, 0 }
            };
            var stack = new Stack<Vector>();
            stack.Push(startingCell);
            Vector nextCell;
            while (stack.Count > 0) {
                nextCell = stack.Pop();
                var distance = distances[nextCell];
                foreach (var neighborXY in builder.NeighborsOf(nextCell)) {
                    if (!builder.CanConnect(nextCell, neighborXY))
                        continue;
                    // if a maze has loops, we have to check if we are building
                    // a shorter path or a longer one.
                    if (!distances.ContainsKey(neighborXY)) {
                        distances.Add(neighborXY, distance + 1);
                    } else if (distance + 1 < distances[neighborXY]) {
                        distances[neighborXY] = distance + 1;
                    } else continue;
                    stack.Push(neighborXY);
                }
            }
            return distances;
        }

        /// <summary>
        /// Finds the shortest path from the startingCell to the targetCell.
        /// </summary>
        /// <returns><c>List&lt;Cell&gt;</c> containing the solution from
        /// <paramref name="startingCell" /> to <paramref name="targetCell" />
        /// or <c>Optional&lt;List&lt;Cell&gt;&gt;.Empty</c> if the solution
        /// does not exist.</returns>        
        public static Optional<List<Vector>> Solve(Area mazeArea,
            Vector startingCell, Vector targetCell) {
            var distances = Find(mazeArea, startingCell);
            if (!distances.ContainsKey(targetCell)) {
                return Optional<List<Vector>>.Empty;
            }
            var solution = new List<Vector>() { targetCell };
            while (distances[targetCell] > 0) {
                targetCell = mazeArea[targetCell].Links()
                    .OrderBy(cell => distances[cell]).First();
                solution.Add(targetCell);
            }
            solution.Reverse();
            return solution;
        }

        public static LongestTrailExtension FindLongestTrail(Area maze) {
            maze.Grid.ThrowIfNullOrEmpty("maze.MazeCells");
            var distances = Find(maze, maze.Grid.First());
            var startingPoint = distances.OrderByDescending(kvp => kvp.Value)
                                         .Select(kvp => kvp.Key).First();
            maze[startingPoint].X(new IsLongestTrailStartExtension());
            distances = Find(maze, startingPoint);
            var targetPoint = distances.OrderByDescending(kvp => kvp.Value)
                                       .Select(kvp => kvp.Key).First();
            maze[targetPoint].X(new IsLongestTrailEndExtension());
            var solution = Solve(maze, startingPoint, targetPoint).Value;
            foreach (var cell in solution) {
                maze[cell].X(new IsLongestTrailExtension());
            }
            return new LongestTrailExtension(solution);
        }
    }
}
