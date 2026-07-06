using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.MapFilters;
using PlayersWorlds.Maps.Maze.PostProcessing;
using static PlayersWorlds.Maps.Maze.GeneratorOptions;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// <p>This is a helper class that allows maze generators to manage maze
    /// cells during generation.</p>
    /// <p>It is not intended to be used by the user.</p>
    /// <p>The main point is that we can't pick just random cells during the 
    /// generation process because we can have different types of areas in the
    /// maze:</p>
    /// <ul>
    /// <li> <see cref="AreaType.Fill" /> cells are not be included in the maze.
    /// They will be avoided by maze generators in all cases.
    /// </li>
    /// <li> <see cref="AreaType.Hall" /> cells are included but should have
    /// only one entrance cell. They will be avoided by generators and connected
    /// later.</li>
    /// <li> <see cref="AreaType.Cave" /> cells are included with any number of
    /// entrances. They will be processed by the generators as regular maze
    /// areas with all internal walls removed afterwards.</li>
    /// </ul>
    /// </summary>
    // TODO: What's a better name for this class?
    public class Maze2DBuilder {
        private readonly RandomSource _randomSource;
        private readonly Area _mazeArea;
        private readonly MazeGenerator _mazeGenerator;
        private readonly AreaGenerator _areaGenerator;
        private readonly MazeFillFactor _fillFactor;
        private readonly int _isFillCompleteAttempts;
        private int _isFillCompleteAttemptsMade;
        private Dictionary<Vector, HashSet<Vector>> _neighbors =
            new Dictionary<Vector, HashSet<Vector>>();
        private readonly HashSet<Vector> _cellsToConnect =
            new HashSet<Vector>();
        private readonly Dictionary<Vector, List<Vector>> _priorityCellsToConnect =
            new Dictionary<Vector, List<Vector>>();
        private readonly HashSet<Vector> _allConnectableCells =
            new HashSet<Vector>();
        private readonly HashSet<Vector> _connectedCells =
            new HashSet<Vector>();
        private readonly List<HashSet<Vector>> _cellGroups =
            new List<HashSet<Vector>>();

        /// <summary>
        /// Cells that can be connected and are not connected yet.
        /// </summary>
        /// <remarks>
        /// Used only in <see cref="AldousBroderMazeGenerator"/>.
        /// </remarks>
        internal IReadOnlyCollection<Vector> TestCellsToConnect =>
            _cellsToConnect;
        /// <summary>
        /// All connectable cells (connected or unconnected) in the
        /// lowest xy to highest xy order with row priority.
        /// </summary>
        /// <remarks>
        /// Used only in <see cref="BinaryTreeMazeGenerator"/> and 
        /// <see cref="SidewinderMazeGenerator"/>.
        /// </remarks>
        internal IReadOnlyCollection<Vector> AllCells => _allConnectableCells;
        /// <summary>
        /// Cells that are already have connections to other cells.
        /// </summary>
        /// <remarks>
        /// Used only in tests.
        /// </remarks>
        /// <testonly />
        internal IReadOnlyCollection<Vector> TestConnectedCells =>
            _connectedCells;
        /// <summary>
        /// Cells that are not yet connected, and we want to connect them first.
        /// <p>Each cell in the priority cells is associated with an area, and
        /// when any of that area cells is connected, all cells associated with
        /// that area lose their priority status.</p>
        /// </summary>
        /// <remarks>
        /// Used only in tests.
        /// </remarks>
        /// <testonly />
        internal Dictionary<Vector, List<Vector>> TestPriorityCells =>
            _priorityCellsToConnect;

        /// <summary>
        /// Contains groups of connectable cells. In case the maze field has
        /// isolated areas, some groups of cells can be isolated from others.
        /// This is the list of groups of connectable cells.
        /// </summary>
        public virtual List<HashSet<Vector>> CellGroups => _cellGroups;

        internal RandomSource Random => _randomSource;

        public Area MazeArea => _mazeArea;

        /// <summary>
        /// Creates a new instance of the <see cref="Maze2DBuilder"/> class.
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="mazeArea"><see cref="Area" /> to build the maze on.
        /// </param>
        /// <param name="mazeGenerator"></param>
        /// <param name="areaGenerator"></param>
        /// <param name="fillFactor"></param>
        public Maze2DBuilder(RandomSource randomSource,
                             Area mazeArea,
                             MazeGenerator mazeGenerator,
                             AreaGenerator areaGenerator,
                             MazeFillFactor fillFactor) {
            _randomSource = randomSource;
            _mazeArea = mazeArea;
            _mazeGenerator = mazeGenerator;
            _areaGenerator = areaGenerator;
            _fillFactor = fillFactor;
            _isFillCompleteAttempts = (int)Math.Pow(_mazeArea.Size.Area, 2);
        }

        /// <summary>
        /// A static helper method to generate a new maze on the given area.
        /// </summary>
        /// <returns>An instance of <see cref="Maze2DBuilder" /> used to
        /// generate the maze.</returns>
        /// <exception cref="ArgumentNullException">Maze generation algorithm
        /// is not specified. See <see cref="GeneratorOptions.Algorithms" />.
        /// </exception>
        /// <exception cref="ArgumentException">The provided maze generator type
        /// is not inherited from <see cref="MazeGenerator" /> or does not
        /// provide a default constructor.</exception>
        public static Maze2DBuilder BuildMaze(
            Area area,
            GeneratorOptions options = null) {

            var mazeLayer = area;
            if (options.MazeStructureStyle == MazeStructureStyle.Block) {
                // on each dimension, we start adding walls and traills until
                // we exceed area.Size
                var renderOptions = options?.MazeRendererOptions ?? Maze2DRendererOptions.SquareCells(1, 1);
                var mazeSizeValue = new int[area.Size.Dimensions]; // 0, 0
                var currentDimension = 0;
                while (currentDimension < area.Size.Dimensions) {
                    var suggestedNewSize = mazeSizeValue.ToArray();
                    suggestedNewSize[currentDimension] += 1;
                    var renderedSize = renderOptions.RenderedSize(new Vector(suggestedNewSize));
                    if (renderedSize.FitsInto(area.Size)) {
                        mazeSizeValue = suggestedNewSize;
                    } else {
                        currentDimension++;
                    }
                }
                mazeLayer = Area.CreateMaze(new Vector(mazeSizeValue), area.Tags);
            }

            var builder = CreateFromOptions(mazeLayer, options);
            builder.BuildMaze();

            if (options.MazeStructureStyle == MazeStructureStyle.Block) {
                var mazeRendererOptions =
                    options?.MazeRendererOptions ??
                    Maze2DRendererOptions.SquareCells(1, 1);
                new MazeAreaStyleConverter().ConvertMazeBorderToBlock(
                    mazeLayer, area, mazeRendererOptions);
            }

            return builder;
        }

        public static Maze2DBuilder CreateFromOptions(Area mazeArea,
                                                GeneratorOptions options) {
            if (options.RandomSource == null) {
                throw new ArgumentNullException(
                    "Please specify a RandomSource to use for maze " +
                    "generation using GeneratorOptions.RandomSource.",
                    "options.RandomSource");
            }
            if (options.AreaGeneration == GeneratorOptions.AreaGenerationMode.Auto
                && options.AreaGenerator == null) {
                throw new ArgumentNullException(
                    "Please specify an AreaGenerator to use for maze " +
                    "area generation using GeneratorOptions.AreaGenerator.");
            }
            if (options.MazeAlgorithm == null) {
                throw new ArgumentNullException(
                    "Please specify maze generation algorithm using " +
                    "GeneratorOptions.MazeAlgorithm.");
            }
            if (!typeof(MazeGenerator).IsAssignableFrom(options.MazeAlgorithm)) {
                throw new ArgumentException(
                    "Specified maze generation algorithm " +
                    $"({options.MazeAlgorithm.FullName}) is not a subtype of " +
                    "MazeGenerator.");
            }
            if (options.MazeAlgorithm.GetConstructor(Type.EmptyTypes) == null) {
                throw new ArgumentException(
                    "Specified maze generation algorithm " +
                    $"({options.MazeAlgorithm.FullName}) does not have a " +
                    "default constructor.");
            }

            var areaGenerator = options.AreaGenerator;
            var mazeGenerator =
                (MazeGenerator)Activator.CreateInstance(options.MazeAlgorithm);

            return new Maze2DBuilder(options.RandomSource,
                                     mazeArea,
                                     mazeGenerator,
                                     areaGenerator,
                                     options.FillFactor);
        }

        internal static Maze2DBuilder FromExistingMaze(Area maze) {
            return new Maze2DBuilder(RandomSource.CreateFromEnv(),
                                     maze,
                                     null,
                                     null,
                                     MazeFillFactor.Full);
        }

        internal void TestRebuildCellMaps() {
            _mazeArea.BakeChildAreas();
            RebuildCellMaps();
        }

        private void RebuildCellMaps() {
            _neighbors = new Dictionary<Vector, HashSet<Vector>>();
            var onlyMazeCells = new HashSet<Vector>();
            foreach (var cell in _mazeArea.Grid) {
                if (_mazeArea[cell].AreaType != AreaType.Maze) {
                    _neighbors.Add(cell, new HashSet<Vector>());
                    continue;
                } else {
                    onlyMazeCells.Add(cell);
                }
                var mazeNeighbors = _mazeArea.Grid.AdjacentRegion(cell)
                    // don't include diagonal neighbors.
                    .Where(nbr => nbr.Value.Where(
                        (x, i) => cell.Value[i] == x).Any())
                    // don't include unavailable neighbors.
                    .Where(nbr => _mazeArea[nbr].AreaType == AreaType.Maze)
                    .ToList();
                _neighbors.Add(cell, new HashSet<Vector>(mazeNeighbors));
            }

            // Find priority cells to connect first. _priorityCellsToConnect are
            // associated with areas they relate to. When any of the given area
            // cells is processed, all "priority cells" of this area are removed
            // from the priority list (but stay in the regular pool so they can
            // be processed as normal.
            _priorityCellsToConnect.Clear();
            foreach (var areaInfo in _mazeArea.ChildAreas) {
                var area = areaInfo;
                var mazeAreaCells = new HashSet<Vector>(areaInfo.Grid); // TODO: Not covered
                if (area.Type == AreaType.Cave) {
                    mazeAreaCells.IntersectWith(onlyMazeCells);
                    mazeAreaCells.ForEach(
                        c => _priorityCellsToConnect.Set(
                            c, mazeAreaCells.ToList()));
                } else if (area.Type == AreaType.Hall) {
                    // halls will be connected later. BUT we need to make sure
                    // halls have at least one neighbor cell connected to the
                    // maze.
                    var cells = new HashSet<Vector>(WalkInCells(area)
                        // make sure we don't include cells that belong to 
                        // any other area.
                        .Except(_mazeArea.ChildAreas
                            .Where(otherArea =>
                                area != otherArea &&
                                (otherArea.Type == AreaType.Hall ||
                                    otherArea.Type == AreaType.Fill))
                            .SelectMany(otherArea => // TODO: Not covered
                                            otherArea.Grid)));
                    cells.IntersectWith(onlyMazeCells);
                    cells.ForEach(p => _priorityCellsToConnect.Set(
                        p, cells.ToList()));
                }
            }

            _cellsToConnect.Clear();
            _allConnectableCells.Clear();
            // all cells that do not belong to fill and hall areas.
            onlyMazeCells // does this cell belong to a fill or hall child area?
                .Where(c => _mazeArea.ChildAreas
                    .Where(a => a.Grid.Contains(c))
                    .All(area => area.Type != AreaType.Fill &&
                                 area.Type != AreaType.Hall))
                .ForEach(c => {
                    _cellsToConnect.Add(c);
                    // same as _cellsToConnect but persisted over time.
                    _allConnectableCells.Add(c);
                });


            // in case the area has isolated areas, we need to find all
            // connectable groups of cells.
            var buffer = new HashSet<Vector>(_cellsToConnect);
            _cellGroups.Clear();
            if (buffer.Count > 0) {
                do {
                    var distances = DijkstraDistance.FindRaw(
                        this, buffer.First());
                    _cellGroups.Add(new HashSet<Vector>(distances.Keys));
                    buffer.ExceptWith(distances.Keys);
                } while (buffer.Count > 0);
            }
        }

        /// <summary>
        /// A helper method to generate a new maze on the given area.
        /// </summary>
        /// <exception cref="ArgumentNullException">Maze generation algorithm
        /// is not specified. See <see cref="GeneratorOptions.Algorithms" />.
        /// </exception>
        /// <exception cref="ArgumentException">The provided maze generator type
        /// is not inherited from <see cref="MazeGenerator" /> or does not
        /// provide a default constructor.</exception>
        public void BuildMaze() {
            _areaGenerator?.GenerateMazeAreas(_mazeArea);

            var unpositionedAreas = _mazeArea.ChildAreas.Where(area => area.IsPositionEmpty).ToList();
            if (unpositionedAreas.Count > 0) {
                throw new MazeBuildingException(this,
                    "Maze contains unpositioned areas: " +
                    string.Join(", ", unpositionedAreas));
            }

            _mazeArea.BakeChildAreas();
            RebuildCellMaps();

            _mazeGenerator.GenerateMaze(this);
            ApplyAreas();
            _mazeArea.X(DeadEnd.Find(this.MazeArea));
            _mazeArea.X(DijkstraDistance.FindLongestTrail(this.MazeArea));
            _mazeArea.X(this);
        }

        private IEnumerable<Vector> WalkInCells(Area area) {
            if (area.Type == AreaType.Hall) {
                // find all cells next to this hall that can be linked to the
                // hall.
                return _mazeArea.Grid
                    .SafeRegion(
                        new Vector(area.Position.Value.Select(c => c - 1)),
                        new Vector(area.Size.Value.Select(c => c + 2)))
                    // cells around this area
                    .Except(area.Grid)
                    // that have neighbors in this area.
                    .Where(c => _neighbors[c].Any(n => area.Grid.Contains(n)));
            }
            throw new InvalidOperationException(
                $"WalkInCells is applicable only to halls " +
                $"({Enum.GetName(typeof(AreaType), area.Type)} requested).");
        }

        public IEnumerable<Vector> NeighborsOf(Vector cell) => _neighbors[cell];

        /// <summary>
        /// Cells that can be connected and are not connected yet, in a
        /// priority order.
        /// </summary>
        /// <remarks>
        /// Used only in <see cref="HuntAndKillMazeGenerator"/>.
        /// </remarks>
        public IEnumerable<Vector> GetPrioritizedCellsToConnect() =>
            _priorityCellsToConnect.Keys
                    .Concat(_cellsToConnect
                            .Except(_priorityCellsToConnect.Keys));

        /// <summary>
        /// Randomly picks the next cell to be connected from a pool of
        /// available cells.
        /// </summary>
        public virtual Vector PickNextCellToLink() {
            // pick the next cell for the maze generator.
            // skip halls and filled areas.
            // prioritize cave areas over other cells to make sure they are
            // connected.
            // also make sure hall areas have at least one neighbor cell
            // connected to the maze so we can connect them later.
            Vector nextCell;
            if (_priorityCellsToConnect.Count > 0) {
                nextCell = _randomSource.RandomOf(
                    _priorityCellsToConnect.Keys,
                    _priorityCellsToConnect.Count);
            } else {
                nextCell = _randomSource.RandomOf(
                    _cellsToConnect,
                    _cellsToConnect.Count);
            }
            return nextCell;
        }

        /// <summary>
        /// Retrieves a random neighbor of the given cell, returning priority
        /// cells first.
        /// </summary>
        /// <remarks>
        /// This method does not filter by unconnected cells, i.e. returned
        /// neighbors may be already connected. Use the
        /// <see cref="TryPickRandomNeighbor(Vector, out Vector, bool, bool)"/>
        /// overload to filter.
        /// </remarks>
        /// <param name="cell">A neighbor of this cell will be returned.
        /// </param>
        /// <param name="neighbor">The returned neighbor.</param>
        /// <returns><c>true</c> if a neighbor was found, otherwise false.
        /// </returns>
        public bool TryPickRandomNeighbor(Vector cell,
                                          out Vector neighbor) =>
            TryPickRandomNeighbor(cell, out neighbor,
                onlyUnconnected: false, honorPriority: true);

        /// <summary>
        /// Retrieves a random neighbor of the given cell.
        /// </summary>
        /// <param name="cell">A neighbor of this cell will be returned.
        /// </param>
        /// <param name="neighbor">The returned neighbor.</param>
        /// <param name="honorPriority">Consider priority cells first.</param>
        /// <param name="onlyUnconnected">Only return unconnected neighbors.
        /// </param>
        /// <returns><c>true</c> if a neighbor was found, otherwise false.
        /// </returns>
        public virtual bool TryPickRandomNeighbor(Vector cell, out Vector neighbor,
                                          bool onlyUnconnected = false,
                                          bool honorPriority = true) {
            // pick the next cell for the maze generator.
            // skip halls and filled areas.
            // prioritize cave areas over other cells to make sure they are
            // connected.
            // also make sure hall areas have at least one neighbor cell
            // connected to the maze so we can connect them later.
            var cellsToConnect = honorPriority ?
                _priorityCellsToConnect.GetAll(_neighbors[cell])
                    .Select(kv => kv.Item1).ToList() :
                    new List<Vector>();
            if (cellsToConnect.Count == 0) {
                cellsToConnect =
                    (onlyUnconnected ? _cellsToConnect : _allConnectableCells)
                    .GetAll(_neighbors[cell]).ToList();
            }
            if (cellsToConnect.Count > 0) {
                neighbor = _randomSource.RandomOf(cellsToConnect);
                return true;
            } else {
                neighbor = Vector.East2D;
                return false;
            }
        }

        /// <summary>
        /// Links all cells in <see cref="AreaType.Hall" /> and
        /// <see cref="AreaType.Cave" /> areas, and removes
        /// <see cref="AreaType.Fill" /> area cells from the neighbors and
        /// links. This should be called in MazeGenerator.Generate() after the
        /// generator algorithm completes.
        /// </summary>
        public void ApplyAreas() {
            // halls were avoided during the maze generation.
            // now is the time to see if there are maze corridors next
            // to any halls, and if there are, connect them.
            foreach (var areaInfo in _mazeArea.ChildAreas) {
                var area = areaInfo;
                var areaCells = new List<Vector>(areaInfo.Grid);
                if (area.Type == AreaType.Hall) {
                    // create hall entrances.
                    var walkInCells = WalkInCells(area).ToList();
                    var entranceExists =
                        walkInCells.SelectMany(cell => _mazeArea[cell].Links())
                            .Any(linkedCell => // does this cell belong to a given child area?
                                 _mazeArea.ChildAreas
                                    .Where(a => a.Grid.Contains(linkedCell))
                                    .Any(childArea => childArea == area));
                    // entrance can already be created by an overlapping area.
                    if (entranceExists) continue;
                    var visitedWalkInCells = walkInCells
                        .Where(c => _connectedCells.Contains(c)).ToList();
                    if (visitedWalkInCells.Count == 0) {
                        Trace.TraceWarning(
                            "Hall {0} has no visited entrance cells",
                            areaInfo
                        );
                        continue;
                    }
                    var walkway = _randomSource.RandomOf(visitedWalkInCells);
                    var entrance = _neighbors[walkway]
                        .First(c => area.Grid.Contains(c));
                    Connect(walkway, entrance);
                }
            }
        }

        /// <summary>
        /// Checks if the given cell can be connected in the given direction.
        /// </summary>
        // TODO: this really means "no suitable neighbors". Perhaps the users
        //       should use a more specific check?
        public bool CanConnect(Vector cell, Vector neighbor) =>
            _allConnectableCells.Contains(cell) &&
            _allConnectableCells.Contains(neighbor) &&
            _neighbors[cell].Contains(neighbor);

        /// <summary>
        /// Check if the given cell is connected to the maze cells.
        /// </summary>
        public bool IsConnected(Vector cellPosition) {
            var connected = _connectedCells.Contains(cellPosition);
            // TODO: Prod-time assert library? I.e. Assert.That(cell.Links().Count > 0);
            return connected;
        }

        /// <summary>
        /// Connects two cells together.
        /// </summary>
        /// <param name="one">The first cell to connect.</param>
        /// <param name="another">The second cell to connect.</param>
        public void Connect(Vector one, Vector another) {
            Trace.WriteLine(string.Format("Connecting {0} to {1}.", one, another));
            // check if the cell is a neighbor.
            if (!_neighbors[one].Contains(another)) {
                throw new InvalidOperationException(
                    "Linking with non-adjacent cells is not supported yet (" +
                    $"Trying to link {one} to {another}). Neighbors: {string.Join(", ", NeighborsOf(one))}");
            }
            // this should never happen.
            if (!_neighbors[another].Contains(one)) {
                throw new InvalidProgramException(
                    "Non-mirroring neighborhood (" +
                    $"Trying to link {one} to {another}). Neighbors of one: {string.Join(", ", NeighborsOf(one))}, neighbors of another: {string.Join(", ", NeighborsOf(another))}");
            }
            // mark the cell as visited so it's not picked again in the 
            // PickNextRandomUnlinkedCell
            foreach (var position in new[] { one, another }) {
                if (_priorityCellsToConnect.ContainsKey(position)) {
                    var cellsToRemove = _priorityCellsToConnect[position];
                    cellsToRemove.ForEach(c => _priorityCellsToConnect.Remove(c));
                }
                // only the connected cell is removed from _cellsToConnect because
                // we still might want to connect other cells of the related areas.
                _cellsToConnect.Remove(position);
                _connectedCells.Add(position);
            }
            _mazeArea[one].HardLinks.Add(another);
            _mazeArea[another].HardLinks.Add(one);
        }

        public bool CellsAreLinked(Vector one, Vector another) {
            return _mazeArea[one].HardLinks.Contains(another) ||
                   _mazeArea[one].BakedLinks.Contains(another);
        }

        public bool CellHasLinks(Vector cell) {
            return _mazeArea[cell].HardLinks.Count > 0 ||
                   _mazeArea[cell].BakedLinks.Count > 0;
        }

        public ICollection<Vector> CellLinks(Vector cell) {
            return _mazeArea[cell].HardLinks
                    .Concat(_mazeArea[cell].BakedLinks)
                    .Distinct().ToList();
        }

        /// <summary>
        /// Checks if the maze is complete in accordance with the specified
        /// <see cref="GeneratorOptions"/>.
        /// </summary>
        /// <returns><c>true</c> if the maze is complete, otherwise 
        /// <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">This method has been
        /// called over maze.Size.Area ^ 3 times. Please report a bug providing
        /// the version of this library, the serialized maze, and options.
        /// </exception>
        public virtual bool IsFillComplete() {
            if (_isFillCompleteAttemptsMade >= _isFillCompleteAttempts) {
                throw new MazeBuildingException(this,
                    new StackTrace().GetFrame(1).GetType().FullName +
                    $" didn't complete in " +
                    $"{_isFillCompleteAttempts} loops. Please report a bug at" +
                    $" https://github.com/aynurin/maze-gen/issues.\n" +
                    $"Cells connected: {_connectedCells.Count} of " +
                    $"{_allConnectableCells.Count}.");
            }
            _isFillCompleteAttemptsMade--;
            if (_cellsToConnect.Count == 0) {
                return true;
            }
            if (_connectedCells.Count == 0) {
                return false;
            }
            switch (_fillFactor) {
                case MazeFillFactor.FullWidth: {
                        var minX = _connectedCells.Min(c => c.X);
                        var maxX = _connectedCells.Max(c => c.X);
                        return minX == 0 && maxX == _mazeArea.Size.X - 1;
                    }

                case MazeFillFactor.FullHeight: {
                        var minY = _connectedCells.Min(c => c.Y);
                        var maxY = _connectedCells.Max(c => c.Y);
                        return minY == 0 && maxY == _mazeArea.Size.Y - 1;
                    }

                case MazeFillFactor.Full:
                    return _cellsToConnect.Count == 0;

                default: {
                        var fillFactor =
                            _fillFactor ==
                                MazeFillFactor.Quarter ? 0.25 :
                            _fillFactor ==
                                MazeFillFactor.Half ? 0.5 :
                            _fillFactor ==
                                MazeFillFactor.ThreeQuarters ? 0.75 :
                            0.9;
                        return _cellsToConnect.Count <=
                            _allConnectableCells.Count * (1 - fillFactor);
                    }
            }
        }

        /// <summary>
        /// The requested options set is not supported.
        /// </summary>
        /// <param name="options">The <see cref="GeneratorOptions"/> to check.
        /// </param>
        /// <exception cref="ArgumentException">The options set is not
        /// supported.</exception>
        public void ThrowIfIncompatibleOptions(GeneratorOptions options) {
            if (_fillFactor != options.FillFactor) {
                throw new ArgumentException(_mazeGenerator.GetType().Name +
                " doesn't currently support fill factors other than Full");
            }
        }

        /// <inheritdoc />
        // TODO: Not covered
        public override string ToString() =>
            this.DebugString() + "\n" + _mazeArea.ToString();
    }
}
