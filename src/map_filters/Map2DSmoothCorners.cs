using System.Linq;

namespace PlayersWorlds.Maps.MapFilters {

    /// <summary>
    /// A <see cref="Area" /> filter that detects corner cells to allow placing
    /// different types of cells in the corners.
    /// </summary>
    /// <remarks>
    /// Use this filter to mark the corners of different areas on the
    /// map to allow easier automated placement of map objects in corners.
    /// </remarks>
    public class Map2DSmoothCorners : Map2DFilter {
        private readonly Cell.CellTag _cellType;
        private readonly Cell.CellTag _cornerType;
        private readonly Vector _cornerCellSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map2DSmoothCorners" />
        /// class.
        /// </summary>
        /// <param name="cellType">The type of cells where to look for
        /// corners.</param>
        /// <param name="cornerType">The type of cells to put in the corners.
        /// </param>
        /// <param name="cornerCellSize">Size of the corner cells block.</param>
        public Map2DSmoothCorners(Cell.CellTag cellType,
                              Cell.CellTag cornerType,
                              Vector cornerCellSize) {
            _cellType = cellType;
            _cornerType = cornerType;
            _cornerCellSize = cornerCellSize;
        }

        /// <summary>
        /// Apply the filter to the specified <see cref="Area" />.
        /// </summary>
        /// <param name="map">The map to apply the filter to.</param>
        override public void Render(Area map) {
            foreach (var cell in map.Grid) {
                if (map[cell].Tags.Contains(_cellType) ||
                    map[cell].Tags.Contains(_cornerType)) continue;
                var a = map.Grid.SafeRegion(
                            new Vector(cell.X - _cornerCellSize.X, cell.Y),
                            new Vector(_cornerCellSize.X * 2 + 1, 1))
                            .Count(c => map[c].Tags.Contains(_cellType));
                var b = map.Grid.SafeRegion(
                            new Vector(cell.X, cell.Y - _cornerCellSize.Y),
                            new Vector(1, _cornerCellSize.Y * 2 + 1))
                            .Count(c => map[c].Tags.Contains(_cellType));
                var setOutline = a > 0 && b > 0;
                if (setOutline) {
                    if (map[cell].Tags.Contains(_cellType))
                        map[cell].Tags.Remove(_cellType);
                    map[cell].Tags.Add(_cornerType);
                }
            }
            // for (var y = 0; y < map.Size.Y; y++) {
            //     for (var x = 0; x < map.Size.X; x++) {
            //         var cell = map[map.Position + new Vector(x, y)];
            //         if (cell.Tags.Contains(_cellType) ||
            //             cell.Tags.Contains(_cornerType)) continue;
            //         var a = map.Grid.SafeRegion(
            //                     new Vector(x - _cornerCellSize.X, y),
            //                     new Vector(_cornerCellSize.X * 2 + 1, 1))
            //                    .Count(c => map[c].Tags.Contains(_cellType));
            //         var b = map.Grid.SafeRegion(
            //                     new Vector(x, y - _cornerCellSize.Y),
            //                     new Vector(1, _cornerCellSize.Y * 2 + 1))
            //                    .Count(c => map[c].Tags.Contains(_cellType));
            //         var setOutline = a > 0 && b > 0;
            //         if (setOutline) {
            //             if (cell.Tags.Contains(_cellType))
            //                 cell.Tags.Remove(_cellType);
            //             cell.Tags.Add(_cornerType);
            //         }
            //     }
            // }
        }
    }
}
