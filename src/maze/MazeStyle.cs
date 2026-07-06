using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Hunt-and-kill algorithm implementation.
    /// </summary>
    public enum MazeStructureStyle {
        /// <summary>
        /// All cells of the generated maze are trails, with borders defined
        /// via <see cref="Cell.HardLinks" /> or the lack of thereof between
        /// cells.
        /// </summary>
        Border,
        /// <summary>
        /// Each cell of the generated maze is either a wall or a trail.
        /// </summary>
        Block,
    }
}
