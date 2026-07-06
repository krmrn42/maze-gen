using System;
using System.Text;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps.Renderers {
    /// <summary>
    /// Renders a map to an ASCII string.
    /// </summary>
    public class AsciiRendererFactory {
        public virtual AreaToAsciiRenderer CreateRenderer(Area area) {
            if (area.X<Maze2DBuilder>() != null) {
                return new Maze2DStringBoxRenderer(area);
            } else {
                return new Map2DStringRenderer(area);
            }
        }
    }
}
