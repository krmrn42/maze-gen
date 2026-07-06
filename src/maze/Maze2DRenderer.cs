using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayersWorlds.Maps.MapFilters;

namespace PlayersWorlds.Maps.Maze {

    /// <summary>
    /// Renders a maze <see cref="Area" /> to a string.
    /// </summary>
    public class Maze2DRenderer {
        private readonly Area _maze;
        private readonly Maze2DRendererOptions _options;
        private readonly List<Map2DFilter> _filters = new List<Map2DFilter>();

        internal static Area CreateMapForMaze(
            Area maze, Maze2DRendererOptions options) =>
                Area.Create(maze.Position, options.RenderedSize(maze.Size),
                Areas.AreaType.Environment, maze.Tags);

        /// <summary />
        public Maze2DRenderer(Area maze, Maze2DRendererOptions options) {
            maze.ThrowIfNull("maze");
            options.ThrowIfNull("options");

            _maze = maze;
            _options = options;
        }

        /// <summary>
        /// Applies the specified <see cref="Map2DFilter" />s while rendering.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Maze2DRenderer With(Map2DFilter filter) {
            _filters.Add(filter);
            return this;
        }

        /// <summary>
        /// Renders a <see cref="Area" /> to a string.
        /// </summary>
        public void Render(Area map) {
            if (!_options.RenderedSize(_maze.Size).FitsInto(map.Size)) {
                throw new ArgumentException("Map does not fit the maze.");
            }
            foreach (var cell in _maze.Grid) {
                var mapping = new CellsMapping(map, cell - _maze.Position, _options);
                if (_maze[cell].HasLinks()) {
                    mapping.CenterCells.Where(c => map.Grid.Contains(c)).ForEach(c => map[c].Tags.Add(Cell.CellTag.MazeTrail));
                }
                if (_maze[cell].HasLinks(cell + Vector.North2D)) {
                    mapping.NCells.Where(c => map.Grid.Contains(c)).ForEach(c => map[c].Tags.Add(Cell.CellTag.MazeTrail));
                }
                if (_maze[cell].HasLinks(cell + Vector.East2D)) {
                    mapping.ECells.Where(c => map.Grid.Contains(c)).ForEach(c => map[c].Tags.Add(Cell.CellTag.MazeTrail));
                }
                if (_maze[cell].HasLinks(cell + Vector.South2D)) {
                    mapping.SCells.Where(c => map.Grid.Contains(c)).ForEach(c => map[c].Tags.Add(Cell.CellTag.MazeTrail));
                }
                if (_maze[cell].HasLinks(cell + Vector.West2D)) {
                    mapping.WCells.Where(c => map.Grid.Contains(c)).ForEach(c => map[c].Tags.Add(Cell.CellTag.MazeTrail));
                }
            }
            foreach (var filter in _filters) {
                filter.Render(map);
            }
        }

        internal class CellsMapping {
            private readonly Area _map;
            private readonly Vector _mazeCell;
            private readonly Maze2DRendererOptions _options;
            private readonly Vector[] _size = new Vector[9];
            private readonly Vector[] _position = new Vector[9];
            private const int NW = 0, N = 1, NE = 2, W = 3, CENTER = 4, E = 5, SW = 6, S = 7, SE = 8;

            public CellsMapping(Area map, Vector mazeCell, Maze2DRendererOptions options) {
                map.ThrowIfNull(nameof(map));
                mazeCell.ThrowIfEmpty(nameof(mazeCell));
                options.ThrowIfNull(nameof(options));

                _map = map;
                _mazeCell = mazeCell;
                _options = options;

                _size[SW] = _options.WallSize(_mazeCell);
                _position[SW] = _options.SWPosition(_mazeCell);
                _size[CENTER] = _options.TrailSize(_mazeCell);
                _position[CENTER] = _position[SW] + _size[SW];
                _size[NE] = _options.WallSize(
                    _mazeCell + Vector.NorthEast2D);
                _position[NE] = _position[CENTER] + _size[CENTER];

                _size[NW] = new Vector(_size[SW].X, _size[NE].Y);
                _position[NW] = new Vector(_position[SW].X, _position[NE].Y);
                _size[N] = new Vector(_size[CENTER].X, _size[NE].Y);
                _position[N] = new Vector(_position[CENTER].X, _position[NE].Y);
                _size[W] = new Vector(_size[SW].X, _size[CENTER].Y);
                _position[W] = new Vector(_position[SW].X, _position[CENTER].Y);
                _size[E] = new Vector(_size[NE].X, _size[CENTER].Y);
                _position[E] = new Vector(_position[NE].X, _position[CENTER].Y);
                _size[S] = new Vector(_size[CENTER].X, _size[SW].Y);
                _position[S] = new Vector(_position[CENTER].X, _position[SW].Y);
                _size[SE] = new Vector(_size[NE].X, _size[SW].Y);
                _position[SE] = new Vector(_position[NE].X, _position[SW].Y);

                for (var i = 0; i < _position.Length; i++) {
                    _position[i] = _position[i] + _map.Position;
                }
            }

            // y │    N
            //   │  W C E
            //   │    S
            //   └─────────
            //           x

            public Vector CenterPosition => _position[CENTER];
            public Vector CenterSize => _size[CENTER];
            public Vector NWPosition => _position[NW];
            public Vector NWSize => _size[NW];
            public Vector NPosition => _position[N];
            public Vector NSize => _size[N];
            public Vector NEPosition => _position[NE];
            public Vector NESize => _size[NE];
            public Vector WPosition => _position[W];
            public Vector WSize => _size[W];
            public Vector EPosition => _position[E];
            public Vector ESize => _size[E];
            public Vector SWPosition => _position[SW];
            public Vector SWSize => _size[SW];
            public Vector SPosition => _position[S];
            public Vector SSize => _size[S];
            public Vector SEPosition => _position[SE];
            public Vector SESize => _size[SE];

            public IEnumerable<Vector> CenterCells =>
                _map.Grid.Region(_position[CENTER], _size[CENTER]);

            public IEnumerable<Vector> NWCells =>
                _map.Grid.Region(_position[NW], _size[NW]);

            public IEnumerable<Vector> NCells =>
                _map.Grid.Region(_position[N], _size[N]);

            public IEnumerable<Vector> NECells =>
                _map.Grid.Region(_position[NE], _size[NE]);

            public IEnumerable<Vector> WCells =>
                _map.Grid.Region(_position[W], _size[W]);

            public IEnumerable<Vector> ECells =>
                _map.Grid.Region(_position[E], _size[E]);

            public IEnumerable<Vector> SWCells =>
                _map.Grid.Region(_position[SW], _size[SW]);

            public IEnumerable<Vector> SCells =>
                _map.Grid.Region(_position[S], _size[S]);

            public IEnumerable<Vector> SECells =>
                _map.Grid.Region(_position[SE], _size[SE]);


        }

        /// <summary>
        /// Maze rendering options.
        /// </summary>
        public class Maze2DRendererOptions {
            internal Vector TrailCellSize { get; }
            internal Vector WallCellSize { get; }

            internal Vector WallSize(Vector _) =>
                WallCellSize;

            internal Vector TrailSize(Vector _) =>
                TrailCellSize;

            internal Vector SWPosition(Vector mazeCellPosition) {
                var trailPart = new Vector(
                        TrailCellSize.X * mazeCellPosition.X,
                        TrailCellSize.Y * mazeCellPosition.Y);
                var wallPart = new Vector(
                        WallCellSize.X * mazeCellPosition.X,
                        WallCellSize.Y * mazeCellPosition.Y);
                return trailPart + wallPart;
            }

            internal Vector RenderedSize(Vector mazeSize) {
                return SWPosition(mazeSize) + WallCellSize;
            }

            /// <summary>
            /// Creates new maze rendering options with the specified walls and
            /// trails sizes.
            /// </summary>
            /// <param name="trailCellSize">Size of trail cells.</param>
            /// <param name="wallCellSize">Size of wall cells.</param>
            /// <exception cref="ArgumentException"></exception>
            public Maze2DRendererOptions(
                Vector trailCellSize,
                Vector wallCellSize) {
                trailCellSize.ThrowIfNotAValidSize(nameof(trailCellSize));
                wallCellSize.ThrowIfNotAValidSize(nameof(wallCellSize));
                if (trailCellSize.Dimensions != wallCellSize.Dimensions) {
                    throw new ArgumentException("Wall and trail cell sizes " +
                        "must have the same number of dimensions.");
                }
                if (trailCellSize.Area == 0 ||
                    wallCellSize.Area == 0) {
                    throw new ArgumentException("Zero and negative wall and " +
                        "trail sizes are not supported.");
                }
                TrailCellSize = trailCellSize;
                WallCellSize = wallCellSize;
            }

            /// <summary>
            /// Creates an instance of <see cref="Maze2DRendererOptions" /> with
            /// square wall and trail cell sizes.
            /// </summary>
            public static Maze2DRendererOptions SquareCells(
                int trailCellSize,
                int wallCellSize)
                => new Maze2DRendererOptions(
                    new Vector(trailCellSize, trailCellSize),
                    new Vector(wallCellSize, wallCellSize)
                );

            /// <summary>
            /// Creates an instance of <see cref="Maze2DRendererOptions" /> with
            /// rectangular wall and trail cell sizes.
            /// </summary>
            public static Maze2DRendererOptions RectCells(
                Vector trailCellSize,
                Vector wallCellSize)
                => new Maze2DRendererOptions(
                    trailCellSize,
                    wallCellSize
                );

            /// <summary>
            /// Creates an instance of <see cref="Maze2DRendererOptions" /> with
            /// rectangular wall and trail cell sizes.
            /// </summary>
            public static Maze2DRendererOptions RectCells(
                int cellWidth,
                int cellHeight)
                => new Maze2DRendererOptions(
                    new Vector(cellWidth, cellHeight),
                    new Vector(cellWidth, cellHeight)
                );
        }
    }
}
