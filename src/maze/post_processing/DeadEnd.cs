using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze.PostProcessing {
    /// <summary>
    /// Find dead ends in the maze.
    /// </summary>
    public static class DeadEnd {
        /// <summary>
        /// An extension object that contains the list of all found dead ends.
        /// </summary>
        public class DeadEndsExtension {
            /// <summary>
            /// Creates an instance of the DeadEndsExtension.
            /// </summary>
            /// <param name="deadEnds"></param>
            public DeadEndsExtension(IEnumerable<Vector> deadEnds) {
                DeadEnds = deadEnds.ToList();
            }

            /// <summary>
            /// The list of all found dead ends.
            /// </summary>
            public List<Vector> DeadEnds { get; private set; }
        }

        /// <summary>
        /// An extension object that denotes a dead end.
        /// </summary>
        public class IsDeadEndExtension { }

        /// <summary>
        /// Find dead ends in the maze.
        /// </summary>
        public static DeadEndsExtension Find(Area maze) {
            var deadEnds = maze.Grid.Where(cell => maze[cell].Links().Count == 1)
                .ToList();
            foreach (var cell in deadEnds) {
                maze[cell].X(new IsDeadEndExtension());
            }
            return new DeadEndsExtension(deadEnds);
        }
    }
}
