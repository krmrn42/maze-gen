using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.MapFilters {

    /// <summary>
    /// A <see cref="Area" /> filter that detects blocks of specific type
    /// less then a certain width and height, and replaces them with another
    /// cell type.
    /// </summary>
    internal class Map2DEraseSpots : Map2DFilter {
        private readonly Cell.CellTag[] _spotTypes;
        private readonly bool _includeVoids;
        private readonly Cell.CellTag _fillType;
        private readonly int _maxSpotWidth;
        private readonly int _maxSpotHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map2DEraseSpots" />
        /// class.
        /// </summary>
        /// <param name="spotTypes">Cells types to treat as spots.</param>
        /// <param name="includeVoids"><c>true</c> to also treat undefined cell
        /// types as spots.</param>
        /// <param name="fillType">Cell type to replace the spots.</param>
        /// <param name="maxSpotWidth">Maximum block width to treat as a spot.
        /// </param>
        /// <param name="maxSpotHeight">Maximum block height to treat as a spot.
        /// </param>
        public Map2DEraseSpots(Cell.CellTag[] spotTypes,
                             bool includeVoids,
                             Cell.CellTag fillType,
                             int maxSpotWidth,
                             int maxSpotHeight) {
            _spotTypes = spotTypes;
            _includeVoids = includeVoids;
            _fillType = fillType;
            _maxSpotWidth = maxSpotWidth;
            _maxSpotHeight = maxSpotHeight;
        }

        /// <summary>
        /// Apply the filter to the specified <see cref="Area" />.
        /// </summary>
        /// <param name="map">The map to apply the filter to.</param>
        override public void Render(Area map) {
            var visited = new bool[map.Size.Area];
            foreach (var xy in map.Grid) {
                var cellData = map[xy];
                if (visited[(xy - map.Position).ToIndex(map.Size)]) continue;

                // look for all spots of the given type
                // if a cell matches a spot type, dfs to find the
                // entire spot
                // if the spot size is larger than the max spot size, then
                // keep the spot in place.
                // if the spot is smaller, erase it.
                // mark all spot cells as visited

                if (!CellIsASpot(cellData)) {
                    visited[(xy - map.Position).ToIndex(map.Size)] = true;
                    continue;
                }

                var dfsStack = new Stack<Vector>();
                dfsStack.Push(xy);

                var spotCells = new List<Cell> { cellData };
                var minX = int.MaxValue;
                var maxX = int.MinValue;
                var minY = int.MaxValue;
                var maxY = int.MinValue;

                while (dfsStack.Count > 0) {
                    var current = dfsStack.Pop();
                    if (minX > current.X) minX = current.X;
                    if (maxX < current.X) maxX = current.X;
                    if (minY > current.Y) minY = current.Y;
                    if (maxY < current.Y) maxY = current.Y;

                    foreach (var nextXy in map.Grid.AdjacentRegion(current)) {
                        if (!CellIsASpot(map[nextXy])) continue;
                        if (visited[(nextXy - map.Position).ToIndex(map.Size)] == false) {
                            visited[(nextXy - map.Position).ToIndex(map.Size)] = true;
                            spotCells.Add(map[nextXy]);
                            dfsStack.Push(nextXy);
                        }
                    }
                }

                if (maxX - minX < _maxSpotWidth && maxY - minY < _maxSpotHeight) {
                    foreach (var spotCell in spotCells) {
                        _spotTypes.ForEach(tag => spotCell.Tags.Remove(tag));
                        spotCell.Tags.Add(_fillType);
                    }
                }
            }
        }

        private bool CellIsASpot(Cell cell) =>
            _includeVoids && cell.Tags.Count() == 0 ||
            cell.Tags.Any(t => _spotTypes.Contains(t));
    }
}
