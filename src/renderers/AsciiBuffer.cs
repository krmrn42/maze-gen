using System;
using System.Text;

namespace PlayersWorlds.Maps.Renderers {
    internal class AsciiBuffer {
        private readonly bool _hideOverflow;
        private readonly char[][] _buffer;

        public AsciiBuffer(int cols, int rows, bool hideOverflow) {
            _hideOverflow = hideOverflow;
            _buffer = new char[rows][];
            for (var i = 0; i < rows; i++) {
                _buffer[i] = new string(' ', cols).ToCharArray();
            }
        }

        internal void PutC(int col, int row, char v) {
            if (row < 0 || row >= _buffer.Length || col < 0 || col >= _buffer[row].Length) {
                if (_hideOverflow) {
                    return;
                } else {
                    throw new InvalidOperationException($"Position {row}x{col} is off the grid {_buffer.Length}x{_buffer[row].Length}");
                }
            }
            _buffer[row][col] = v;
        }

        public override string ToString() {
            var buffer = new StringBuilder(Environment.NewLine);
            foreach (var line in _buffer) {
                var strLine = new string(line);
                if (strLine.Trim().Length > 0)
                    buffer.AppendLine(new string(line));
            }
            return Environment.NewLine + buffer.ToString() + Environment.NewLine;
        }
    }
}
