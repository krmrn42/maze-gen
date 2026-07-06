using System.Linq;

namespace PlayersWorlds.Maps.MapFilters {

    /// <summary>
    /// A <see cref="Area" /> filter that detects area edges and places another
    /// cell type around the edges.
    /// </summary>
    /// <remarks>
    /// Use this filter to create outlines around edges of specific cell type.
    /// E.g., to draw walls around trails.
    /// </remarks>
    public class Map2DOutline : Map2DFilter {
        private readonly Cell.CellTag[] _cellType;
        private readonly Cell.CellTag _outlineType;
        private readonly Vector _outlineCellSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map2DOutline" /> class.
        /// </summary>
        /// <param name="cellType">Type of cells around which to draw outline.
        /// </param>
        /// <param name="outlineType">Outline cell type</param>
        /// <param name="outlineCellSize">Minimal cell size of the outline.
        /// </param>
        public Map2DOutline(Cell.CellTag[] cellType,
                            Cell.CellTag outlineType,
                            Vector outlineCellSize) {
            _cellType = cellType;
            _outlineType = outlineType;
            _outlineCellSize = outlineCellSize;
        }

        /// <summary>
        /// Apply the filter to the specified <see cref="Area" />.
        /// </summary>
        /// <param name="map">The map to apply the filter to.</param>
        override public void Render(Area map) {
            foreach (var cell in map.Grid) {
                if (map[cell].Tags.Any(t => _cellType.Contains(t)) ||
                    map[cell].Tags.Contains(_outlineType)) continue;
                var setOutline =
                    map.Grid.SafeRegion(
                        cell - _outlineCellSize,
                        _outlineCellSize + _outlineCellSize + new Vector(1, 1))
                        .Any(c => c != cell &&
                            map[c].Tags.Any(t => _cellType.Contains(t)));
                if (setOutline) {
                    foreach (var tag in _cellType)
                        if (map[cell].Tags.Contains(tag))
                            map[cell].Tags.Remove(tag);
                    map[cell].Tags.Add(_outlineType);
                }
            }
        }
    }
}
