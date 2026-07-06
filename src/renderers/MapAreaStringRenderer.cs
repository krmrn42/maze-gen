
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Renderers {
    /// <summary>
    /// Renders a set of <see cref="Area" /> in a 2D space to a string.
    /// </summary>
    public class MapAreaStringRenderer {

        /// <summary />
        public string Render(Vector envSize,
            IEnumerable<(Area area, string label)> areas) {
            var bufferSize = new Vector(envSize.X * 2 * 2, envSize.Y * 2);
            var buffer = new AsciiBuffer(bufferSize.X, bufferSize.Y, true);
            var offset = new Vector(envSize.X / 2, envSize.Y / 2);
            DrawRect(buffer, new Vector(offset.X, offset.Y),
                envSize, "", s_mazeChars);
            // transpile room positions to reflect reversed X in Terminal
            areas.Where(area => !area.area.IsPositionEmpty).ForEach(area => DrawRect(buffer,
                new Vector(area.area.Position.X + offset.X,
                           envSize.Y - area.area.Size.Y - area.area.Position.Y + offset.Y),
                           area.area.Size,
                           area.label,
                           s_roomChars));
            return buffer.ToString() +
                Environment.NewLine +
                string.Join(Environment.NewLine,
                            areas.Where(area => area.area.IsPositionEmpty)
                                 .Select(a => "unpositioned area: " +
                                              a.label +
                                              a.area.ToString()));
        }

        private void DrawRect(AsciiBuffer buffer, Vector pos, Vector size,
            string label, char[] wallChars) {
            var ssize = new Vector(size.X * 2, size.Y);
            var spos = new Vector(pos.X * 2, pos.Y);
            buffer.PutC(spos.X, spos.Y, wallChars[2]);
            buffer.PutC(spos.X + ssize.X, spos.Y, wallChars[3]);
            buffer.PutC(spos.X, spos.Y + ssize.Y, wallChars[4]);
            buffer.PutC(spos.X + ssize.X, spos.Y + ssize.Y, wallChars[5]);
            for (var row = 1; row < ssize.Y; row++) {
                buffer.PutC(spos.X, spos.Y + row, wallChars[1]);
                buffer.PutC(spos.X + ssize.X, spos.Y + row, wallChars[1]);
            }
            for (var col = 1; col < ssize.X; col++) {
                buffer.PutC(spos.X + col, spos.Y, wallChars[0]);
                buffer.PutC(spos.X + col, spos.Y + ssize.Y, wallChars[0]);
            }

            if (label.Length > 0) {
                for (var i = 0; i < Math.Min(label.Length, ssize.X - 1); i++) {
                    buffer.PutC(spos.X + i, spos.Y + 1, label[i]);
                }
            }

        }
        private static readonly char[] s_mazeChars = new char[] { '═', '║', '╔', '╗', '╚', '╝' };
        private static readonly char[] s_roomChars = new char[] { '─', '│', '┌', '┐', '└', '┘' };
    }
}
