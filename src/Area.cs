using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.MapFilters;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Renderers;
using PlayersWorlds.Maps.Serializer;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// Represents an area of arbitrary cells in an N-space.
    /// </summary>
    public class Area : ExtensibleObject, IReadOnlyCollection<Cell> {
        private readonly bool _isPositionFixed;
        private readonly List<Cell> _cells;
        private readonly Grid _grid;
        private readonly AreaType _areaType;
        private readonly string[] _tags;
        private readonly List<Area> _childAreas;

        /// <summary>
        /// Position of this area in the target map.
        /// </summary>
        /// <remarks>
        /// If the area is applied to a maze, then it's the cells of the maze;
        /// and if the area is applied to a map, then it's the cells of the map.
        /// </remarks>
        /// <remarks>
        /// There should be a clear separation in code between
        /// positioned areas and non-positioned areas, so we will
        /// throw here without letting the consumer to check if
        /// the area is positioned or not.
        /// </remarks>
        public Vector Position {
            get {
                if (_grid.Position.IsEmpty) {
                    throw new InvalidOperationException(
                        "Position is not initialized. " +
                        "Check if IsPositionFixed == true.");
                }
                return _grid.Position;
            }
        }

        internal bool IsPositionFixed => _isPositionFixed;
        internal bool IsPositionEmpty => _grid.Position.IsEmpty;


        /// <summary>
        /// Used by other algorithms to identify if the area can be entered
        /// by the player or not.
        /// </summary>
        public AreaType Type => _areaType;

        /// <summary>
        /// Allows to specify any data significant for further use. These tags
        /// can be populated by the user and will be retained in the resulting
        /// map.
        /// </summary>
        public string[] Tags => _tags;

        /// <summary>
        /// Size of the map in cells.
        /// </summary>
        public Vector Size => _grid.Size;

        /// <summary>
        /// A readonly access to the map cells.
        /// </summary>
        public Grid Grid => _grid;

        public int Count => _cells.Count;

        public bool IsHollow =>
            _areaType == AreaType.Hall || _areaType == AreaType.Cave;

        public ReadOnlyCollection<Area> ChildAreas => _childAreas.AsReadOnly();

        /// <summary>
        /// Gets the value of a cell at the specified <see cref="Vector"/>
        /// position.
        /// </summary>
        /// <param name="xy">The position of the cell as a <see cref="Vector"/>.
        /// </param>
        /// <returns>The value of the cell at the specified position.</returns>
        /// <exception cref="IndexOutOfRangeException">The position is outside
        /// the map bounds.</exception>
        public Cell this[Vector xy] => _cells[(_grid.Position.IsEmpty ? xy : (xy - _grid.Position)).ToIndex(_grid.Size)];

        public static Area Create(Vector position,
                                  Vector size,
                                  AreaType areaType,
                                  params string[] tags) =>
            new Area(position, size,
                     /*isPositionFixed=*/true, areaType,
                     /*childAreas=*/null, tags);

        public static Area CreateUnpositioned(Vector size,
                                              AreaType areaType,
                                              params string[] tags) =>
            new Area(Vector.Empty, size,
                     /*isPositionFixed=*/false, areaType,
                     /*childAreas=*/null, tags);

        public static Area CreateUnpositioned(Vector initialPosition,
                                              Vector size,
                                              AreaType areaType,
                                              params string[] tags) =>
            new Area(initialPosition, size,
                     /*isPositionFixed=*/false, areaType,
                     /*childAreas=*/null, tags);

        public static Area CreateEnvironment(Vector size,
                                             params string[] tags) =>
            new Area(Vector.Zero(size.Value.Count), size,
                     /*isPositionFixed=*/false,
                     AreaType.Environment,
                     /*childAreas=*/null,
                     tags);

        // TODO: Misleading, this does not actually create a maze.
        public static Area CreateMaze(Vector size,
                                             params string[] tags) =>
            new Area(Vector.Zero(size.Value.Count), size,
                     /*isPositionFixed=*/false,
                     AreaType.Maze,
                     /*childAreas=*/null,
                     tags);

        public Area ShallowCopy(AreaType? areaType = null,
                                IEnumerable<Cell> cells = null,
                                IEnumerable<Area> childAreas = null,
                                IEnumerable<string> tags = null) =>
            new Area(_grid,
                     cells ?? _cells,
                     _isPositionFixed,
                     areaType ?? _areaType,
                     childAreas ?? _childAreas,
                     tags == null ? _tags : tags.ToArray());

        /// <summary>
        /// Creates a new instance of Area.
        /// </summary>
        /// <param name="position">Position of this area.</param>
        /// <param name="size">Size of the area.</param>
        /// <param name="isPositionFixed"><c>true</c> if the position is fixed,
        /// <c>false</c> otherwise.</param>
        /// <param name="areaType">Area type.</param>
        /// <param name="childAreas">Child areas.</param>
        /// <param name="tags">Tags to assign to this area.</param>
        /// <exception cref="ArgumentException"></exception>
        // TODO: Area constructors seem to be redundant.
        internal Area(Vector position, Vector size, bool isPositionFixed,
                    AreaType areaType,
                    IEnumerable<Area> childAreas,
                    IEnumerable<string> tags) {
            if (position.IsEmpty && isPositionFixed) {
                throw new ArgumentException("Position is not initialized.");
            }
            position.ThrowIfNull(nameof(position));
            _areaType = areaType;
            _isPositionFixed = isPositionFixed;
            _childAreas = childAreas == null ?
                new List<Area>() :
                new List<Area>(childAreas);
            _tags = tags?.ToArray() ?? new string[0];
            _grid = new Grid(position, size);
            _cells = Enumerable.Range(0, size.Area)
                               .Select(_ => new Cell(areaType))
                .ToList();
            BakeChildAreas();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        internal Area(Grid grid, IEnumerable<Cell> cells, bool isPositionFixed,
                    AreaType areaType,
                    IEnumerable<Area> childAreas = null,
                    string[] tags = null) {
            grid.ThrowIfNull(nameof(grid));
            cells.ThrowIfNull(nameof(cells));
            if (grid.Position.IsEmpty && isPositionFixed) {
                throw new ArgumentException("Position is not initialized.");
            }
            _areaType = areaType;
            _isPositionFixed = isPositionFixed;
            _childAreas = childAreas == null ?
                new List<Area>() :
                new List<Area>(childAreas);
            if (tags == null) {
                _tags = new string[0];
            } else {
                _tags = new string[tags.Length];
                Array.Copy(tags, _tags, tags.Length);
            }
            _grid = new Grid(grid.Position, grid.Size);
            _cells = new List<Cell>(cells);
            if (_cells.Count != _grid.Size.Area) {
                throw new ArgumentException(
                    "The number of cells does not match the area size.");
            }
            BakeChildAreas();
        }

        // TODO: Why is this called 3-4 times?
        public void BakeChildAreas() {
            // check if the cell belongs to a Hall or Cave.
            // check if two cells belong to the same Hall or Cave area.
            // make sure the cell is available; unavailable cells cannot be
            //      visited.
            var childAreas = _childAreas.Where(a => !a.IsPositionEmpty)
                                        .ToList();

            // Add all None-Areas to unavailable cells;
            // Remove all non-None-Areas from unavailable cells.
            // Force all Fill areas to be unavailable.
            var unavailableCells = new HashSet<Vector>(
                _areaType == AreaType.None ? _grid : Enumerable.Empty<Vector>());
            // the reason we do this in three steps is because areas can be
            // overlapping. I.e. if there is a hall area over none area, and
            // then covered by a fill area, the fill area cells have to be
            // marked unavailable no matter the order in the collection.
            foreach (var area in childAreas.Where(a => a.Type == AreaType.None)) {
                foreach (var cell in area._grid) {
                    unavailableCells.Add(cell);
                }
            }
            foreach (var area in childAreas.Where(a => a.Type != AreaType.None)) {
                unavailableCells.ExceptWith(area._grid.Select(cell => cell));
            }
            foreach (var area in childAreas.Where(a => a.Type == AreaType.Fill)) {
                foreach (var cell in area._grid) {
                    unavailableCells.Add(cell);
                }
            }

            // cell types logic:
            //   if there are only Environment cells - they are available.
            //   if there are only Maze cells - they are available.
            //   if all cells are of any other type - they are unavailable.
            //   if there are cells of both Maze and Environment or None types,
            //      only Maze cells are used as neighbors.

            HashSet<Vector> neighborCells;
            var mazeCells = _cells
                .Select((c, i) => (cell: c, index: Vector.FromIndex(i, Size)))
                .Where(c => c.cell.AreaType == AreaType.Maze)
                .Select(c => c.index)
                .ToList();
            if (mazeCells.Count > 0) {
                neighborCells = new HashSet<Vector>(mazeCells);
            } else {
                neighborCells = new HashSet<Vector>(_grid);
            }
            neighborCells.ExceptWith(unavailableCells);

            foreach (var cell in _grid) {
                if (unavailableCells.Contains(cell)) {
                    this[cell].Bake(
                        Enumerable.Empty<Vector>(), Enumerable.Empty<Vector>());
                    continue;
                }
                var envNeighbors = _grid.AdjacentRegion(cell)
                    // don't include diagonal neighbors.
                    .Where(nbr => nbr.Value.Where(
                        (x, i) => cell.Value[i] == x).Any())
                    // don't include unavailable neighbors.
                    .Where(nbr => neighborCells.Contains(nbr))
                    .Where(nbr => neighborCells.Contains(cell))
                    .ToList();
                var envLinks = envNeighbors
                    // both cell and neighbor belong to the same hollow area.
                    .Where(nbr => childAreas.Any(a => a.IsHollow &&
                                                      a.Grid.Contains(cell) &&
                                                      a.Grid.Contains(nbr)));
                this[cell].Bake(envNeighbors, envLinks);
            }
        }

        public void Reposition(Vector newPosition) {
            if (_isPositionFixed)
                throw new InvalidOperationException("Position is fixed");
            _grid.Reposition(newPosition);
        }

        public void AddChildArea(Area area) {
            // TODO: Do we need this check?
            // if (!area.Grid.FitsInto(Grid)) {
            //     throw new ArgumentException("Area does not fit into this area");
            // }
            _childAreas.Add(area);
        }

        public void ClearChildAreas() {
            _childAreas.Clear();
        }

        /// <summary>
        /// Produces a string representation of this Area.
        /// </summary>
        /// <returns>A <see cref="string" /> of the form
        /// "P{Position};S{Size};{Type}".</returns>
        public override string ToString() =>
            $"{_areaType}:{_grid.Position};{_grid.Size}" +
            (_tags.Length == 0 ? "" : $";{string.Join(",", _tags)}");

        /// <summary>
        /// Renders this area using a renderer created by the provided factory.
        /// </summary>
        public string Render(AsciiRendererFactory rendererFactory) {
            return rendererFactory.CreateRenderer(this).Render();
        }

        public IEnumerator<Cell> GetEnumerator() {
            return _cells.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _cells.GetEnumerator();
        }
    }
}
