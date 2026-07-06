using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Maze.PostProcessing;

namespace PlayersWorlds.Maps.Renderers {
    /// <summary>
    /// Renders a <see cref="Area" /> to a string.
    /// </summary>
    public class Maze2DStringBoxRenderer : AreaToAsciiRenderer {

        private readonly Area _maze;
        private readonly int _cellInnerHeight;
        private readonly int _cellInnerWidth;
        private readonly Vector _asciiMazeSize;
        private readonly Border.Type[] _buffer;
        private readonly Dictionary<int, string> _data =
            new Dictionary<int, string>();

        /// <summary />
        public Maze2DStringBoxRenderer(Area maze,
                                      int cellInnerHeight = 1,
                                      int cellInnerWidth = 3) {
            _maze = maze; // 4x4
            _cellInnerWidth = cellInnerWidth;
            _cellInnerHeight = cellInnerHeight;
            _asciiMazeSize = new Vector(
                _maze.Size.X * (_cellInnerWidth + 1) + 1,
                _maze.Size.Y * (_cellInnerHeight + 1) + 1); // 9
            _buffer = new Border.Type[_asciiMazeSize.Area]; // 17x9
        }

        /// <summary>
        /// Also render the trail if it's defined in the maze extensions.
        /// </summary>
        /// <returns></returns>
        override public string Render() {
            var trail = _maze.X<DijkstraDistance.LongestTrailExtension>()
                            ?.LongestTrail
                        ?? new List<Vector>();
            var solutionCells = new HashSet<Vector>(trail);
            // print cells from top to bottom
            for (var y = _maze.Size.Y - 1; y >= 0; y--) {
                for (var x = 0; x < _maze.Size.X; x++) {
                    var cell = new Vector(x, y);
                    var cellData = solutionCells.Contains(cell) ?
                        Convert.ToString(trail.IndexOf(cell), 16) :
                        string.Empty;
                    PrintCell(cell, cellData);
                }
            }

            // render cells in a string buffer
            var strBuffer = new StringBuilder();
            for (var y = 0; y < _asciiMazeSize.Y; y++) {
                for (var x = 0; x < _asciiMazeSize.X; x++) {
                    var index = new Vector(x, y).ToIndex(_asciiMazeSize);
                    if (_data.ContainsKey(index)) {
                        strBuffer.Append(_data[index]);
                        x += _data[index].Length - 1;
                    } else {
                        strBuffer.Append(Border.Char(_buffer[index]));
                    }
                }
                strBuffer.Append(Environment.NewLine);
            }
            return strBuffer.ToString();
        }

        private void PrintCell(Vector cell, string cellData) {
            var asciiCoords = CellCoords.Create(_maze.Size,
                                                cell,
                                                _cellInnerWidth,
                                                _cellInnerHeight);
            if (!string.IsNullOrEmpty(cellData)) {
                _data.Add(I(asciiCoords.Center), cellData);
            }
            if (!_maze[cell].HasLinks()) return;
            _buffer[I(asciiCoords.Northeast)] |= Border.Type.X;
            _buffer[I(asciiCoords.Northwest)] |= Border.Type.X;
            _buffer[I(asciiCoords.Southeast)] |= Border.Type.X;
            _buffer[I(asciiCoords.Southwest)] |= Border.Type.X;
            if (!_maze[cell].HasLinks(cell + Vector.North2D)) {
                _buffer[I(asciiCoords.Northeast)] |= Border.Type.Left;
                _buffer[I(asciiCoords.Northwest)] |= Border.Type.Right;
                foreach (var x in asciiCoords.North) _buffer[I(x)] |= Border.Type.South;
            }
            if (!_maze[cell].HasLinks(cell + Vector.South2D)) {
                _buffer[I(asciiCoords.Southeast)] |= Border.Type.Left;
                _buffer[I(asciiCoords.Southwest)] |= Border.Type.Right;
                foreach (var x in asciiCoords.South) _buffer[I(x)] |= Border.Type.North;
            }
            if (!_maze[cell].HasLinks(cell + Vector.East2D)) {
                _buffer[I(asciiCoords.Southeast)] |= Border.Type.Top;
                _buffer[I(asciiCoords.Northeast)] |= Border.Type.Bottom;
                foreach (var x in asciiCoords.East) _buffer[I(x)] |= Border.Type.East;
            }
            if (!_maze[cell].HasLinks(cell + Vector.West2D)) {
                _buffer[I(asciiCoords.Southwest)] |= Border.Type.Top;
                _buffer[I(asciiCoords.Northwest)] |= Border.Type.Bottom;
                foreach (var x in asciiCoords.West) _buffer[I(x)] |= Border.Type.West;
            }
        }

        private int I(Vector v) => v.ToIndex(_asciiMazeSize);

        class CellCoords {
            public Vector Northwest { get; private set; }
            public Vector Southwest { get; private set; }
            public Vector Northeast { get; private set; }
            public Vector Southeast { get; private set; }
            public Vector Center { get; private set; }

            public Vector[] West { get; private set; }
            public Vector[] East { get; private set; }
            public Vector[] North { get; private set; }
            public Vector[] South { get; private set; }

            public static CellCoords Create(Vector mazeSize, Vector cellPosition, int cellInnerWidth, int cellInnerHeight) {
                var scaledCell = new {
                    x = cellPosition.X * (cellInnerWidth + 1),
                    y = (mazeSize.Y - cellPosition.Y) * (cellInnerHeight + 1)
                };
                Vector CellCoord(int dY, int dX) =>
                    new Vector(scaledCell.x + dX, scaledCell.y - dY);
                return new CellCoords {
                    Northwest = CellCoord(cellInnerHeight + 1, 0),
                    Southwest = CellCoord(0, 0),
                    Northeast = CellCoord(cellInnerHeight + 1, cellInnerWidth + 1),
                    Southeast = CellCoord(0, cellInnerWidth + 1),
                    Center = CellCoord(1, 2),
                    West = Enumerable.Range(0, cellInnerHeight).Select(i => CellCoord(i + 1, 0)).ToArray(),
                    East = Enumerable.Range(0, cellInnerHeight).Select(i => CellCoord(i + 1, cellInnerWidth + 1)).ToArray(),
                    North = Enumerable.Range(0, cellInnerWidth).Select(i => CellCoord(cellInnerHeight + 1, i + 1)).ToArray(),
                    South = Enumerable.Range(0, cellInnerWidth).Select(i => CellCoord(0, i + 1)).ToArray(),
                };
            }
        }

        private static class Border {
            [Flags]
            public enum Type {
                North = 0b000000000000100,
                East = 0b000000000001000,
                West = 0b000000000010000,
                South = 0b000000000100000,
                Exit1 = 0b000000010000000,
                Exit2 = 0b000000100000000,
                X = 0b000001000000000,
                Left = 0b000010000000000,
                Right = 0b000100000000000,
                Top = 0b001000000000000,
                Bottom = 0b010000000000000,
                Mark = 0b100000000000000,
                None = 0b000000000000000,
            }

            private static readonly Dictionary<Type, char> s_chars =
                new Dictionary<Type, char>() {
                    {Type.North, '─'},
                    {Type.South, '─'},
                    {Type.North | Type.South, '─'},
                    {Type.X | Type.Left | Type.Right, '─'},
                    {Type.West, '│'},
                    {Type.East, '│'},
                    {Type.West | Type.East, '│'},
                    {Type.X | Type.Top | Type.Bottom, '│'},

                    {Type.X | Type.Left, '╴'},
                    {Type.X | Type.Right, '╶'},
                    {Type.X | Type.Top, '╵'},
                    {Type.X | Type.Bottom, '╷'},

                    {Type.X | Type.Right | Type.Bottom, '┌'},
                    {Type.X | Type.Left | Type.Bottom, '┐'},
                    {Type.X | Type.Right | Type.Top, '└'},
                    {Type.X | Type.Left | Type.Top, '┘'},

                    {Type.X | Type.Left | Type.Right | Type.Top, '┴'},
                    {Type.X | Type.Left | Type.Right | Type.Bottom, '┬'},
                    {Type.X | Type.Right | Type.Top | Type.Bottom, '├'},
                    {Type.X | Type.Left | Type.Top | Type.Bottom, '┤'},

                    {Type.X, '┼'},
                    {Type.X | Type.Left | Type.Right | Type.Top | Type.Bottom, '┼'},
                    {Type.Mark, '*'},
                    {Type.None, ' '},
                };

            public static char Char(Type t) => s_chars[t];
        }
    }
}
