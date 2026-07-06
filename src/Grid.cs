using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// Represents an n-dimensional grid with indexed access using
    /// <see cref="Vector"/> coordinates.
    /// </summary>
    /// <remarks>
    /// There wasn't a need to make it n-dimensional at this stage, but I want
    /// to explore scenarios of storing more information in a map. E.g. every
    /// cell could contain data for more space, like space-associated data 
    /// (icebergs, volcanos), scaled sub spaces (Hermione's bag), or simply
    /// items in the cell.
    /// </remarks>
    public class Grid : IEnumerable<Vector> {
        private Vector _position;
        private readonly Vector _size;

        /// <summary>
        /// Gets the size of the N-dimensional array.
        /// </summary>
        public Vector Size => _size;

        public Vector Position => _position;

        internal double LowX => _position.IsEmpty ? 0 : _position.X;
        internal double HighX => LowX + Size.X;
        internal double LowY => _position.IsEmpty ? 0 : _position.Y;
        internal double HighY => LowY + Size.Y;

        /// <summary>
        /// Creates a new array with the specified size and optional initial
        /// value for each cell.
        /// </summary>
        /// <param name="position">The position of this Grid in the world or on
        /// another Grid.</param>
        /// <param name="size">The size of the map as a <see cref="Vector"/> of
        /// dimensions (rows, columns).</param>
        public Grid(Vector position, Vector size) {
            size.ThrowIfNotAValidSize();
            if (position != Vector.Empty &&
                position.Dimensions != size.Dimensions) {
                throw new ArgumentException(
                    $"The position and size of the map ({size}) must have the " +
                    $"same number of dimensions as the position ({position}).");
            }

            _position = position;
            _size = size;
        }

        internal void Reposition(Vector newPosition) {
            if (newPosition.Dimensions != _size.Dimensions) {
                throw new ArgumentException(
                    $"The position and size of the map ({_size}) must have the " +
                    $"same number of dimensions as the position ({newPosition}).");
            }
            _position = newPosition;
        }

        /// <summary>
        /// Gets a list of <see cref="Vector"/> positions of all adjacent cells
        /// to the specified position, excluding cells outside the map bounds.
        /// </summary>
        /// <param name="xy">The position of the cell to get adjacent
        /// cells for as a <see cref="Vector"/>.</param>
        /// <returns>An enumerable collection of <see cref="Vector"/> positions
        /// of adjacent cells.</returns>
        public IEnumerable<Vector> AdjacentRegion(Vector xy) {
            return SafeRegion(
                    xy + new Vector(Enumerable.Repeat(-1, _size.Dimensions)),
                    new Vector(Enumerable.Repeat(3, _size.Dimensions)))
                .Where(cell => cell != xy);
        }

        /// <summary>
        /// Retrieve all cells of a rectangular region on the map.
        /// </summary>
        /// <param name="xy">Lowest XY position of the region.</param>
        /// <param name="size">_size of the region.</param>
        /// <returns>An enumerable collection of tuples of the specified region
        /// containing the cell position as a <see cref="Vector"/> and its
        /// value.</returns>
        public IEnumerable<Vector> SafeRegion(
            Vector xy, Vector size) {
            size.ThrowIfNotAValidSize();
            if (xy.Dimensions != size.Dimensions || xy.Dimensions != _size.Dimensions) {
                throw new ArgumentException(
                    $"The position and size of the region ({size}) must have " +
                    $"the same number of dimensions as the size of the map " +
                    $"(xy={xy}, size={size}, map size={_size}).");
            }

            var xyValue = new List<int>(xy.Value);
            var sizeValue = new List<int>(size.Value);
            for (var i = 0; i < _size.Dimensions; i++) {
                var pos = _position.IsEmpty ? 0 : _position.Value[i];
                if (xyValue[i] < pos) {
                    sizeValue[i] = Math.Max(0, sizeValue[i] + xyValue[i] - pos);
                    xyValue[i] = pos;
                }
                if (xyValue[i] > pos + _size.Value[i]) {
                    sizeValue[i] = 0;
                } else if (xyValue[i] + sizeValue[i] > pos + _size.Value[i]) {
                    sizeValue[i] =
                        Math.Max(0, pos + _size.Value[i] - xyValue[i]);
                }
            }

            return Region(new Vector(xyValue), new Vector(sizeValue));
        }

        /// <summary>
        /// Retrieve all cells of a rectangular region on the map.
        /// </summary>
        /// <remarks>Retrieves any cells on the map that belong to the requested
        /// region.</remarks>
        /// <param name="xy">Lowest XY position of the region.</param>
        /// <param name="size">Size of the region.</param>
        /// <returns><see cref="Vector" />s of the specified region.</returns>
        /// <exception cref="IndexOutOfRangeException">The coordinates are out
        /// of the bounds of the map.
        /// <see cref="SafeRegion(Vector,Vector)" /> is an alternative
        /// that doesn't throw.</exception>
        public IEnumerable<Vector> Region(Vector xy, Vector size) {
            // Validate dimensions
            size.ThrowIfNotAValidSize();
            if (size.Area == 0) {
                yield break;
            }
            if (size.Dimensions != _size.Dimensions ||
                xy.Dimensions != _size.Dimensions) {
                throw new ArgumentException(
                    "Vector dimensions must match map dimensions " +
                    $"(Grid({_size}).SafeRegion({xy}, {size}))");
            }
            if (xy.Value.Select(
                (x, i) => xy.Value[i] <
                    (_position.IsEmpty ? 0 : _position.Value[i]))
                        .Any(_ => _) ||
                xy.Value.Select(
                    (x, i) => (xy.Value[i] + size.Value[i]) >
                                    (_size.Value[i] +
                                        (_position.IsEmpty ? 0 :
                                         _position.Value[i])))
                        .Any(_ => _)) {
                throw new IndexOutOfRangeException(
                    $"Can't retrieve area of size {size} at {xy} " +
                    $"in map of size {_size} located at {_position}.");
            }

            var xyValue = new List<int>(xy.Value);
            var sizeValue = new List<int>(size.Value);

            var current = new List<int>(xyValue);

            var dimension = 0;
            while (dimension < current.Count) {
                var position = new Vector(current);
                yield return position;

                // Increment coordinates, wrapping back to 0 when reaching the
                // end of a dimension
                for (dimension = 0; dimension < current.Count; dimension++) {
                    current[dimension]++;
                    if (current[dimension] <
                        (xyValue[dimension] + sizeValue[dimension])) {
                        // Complete if we haven't exceeded the end of the
                        // dimension
                        break;
                    }
                    // Move to the next dimension if we haven't reached the end
                    current[dimension] = xyValue[dimension]; // Wrap back to 0
                }
            }
        }

        /// <summary>
        /// Checks if this Grid has an overlap with another Grid.
        /// </summary>
        /// <param name="other">The other Grid to check.</param>
        /// <returns><c>true</c> if the two Grids have an overlap; otherwise,
        /// <c>false</c>.</returns>
        public bool Overlaps(Grid other) => Overlap(other).Any();

        /// <summary>
        /// Gets coordinates of cells of this Grid that overlap any cells of the
        /// <paramref name="other" /> Grid.
        /// </summary>
        /// <param name="other">The other Area to check</param>
        /// <returns><see cref="Vector" /> positions of the overlapping cells,
        /// or an empty enumerable if there is no overlap.</returns>
        public IEnumerable<Vector> Overlap(Grid other) {
            if (this == other)
                throw new InvalidOperationException("Can't compare with self");

            // Calculate the overlap rectangle coordinates
            var lowX = (int)Math.Max(this.LowX, other.LowX);
            var highX = (int)Math.Min(this.HighX, other.HighX);
            var lowY = (int)Math.Max(this.LowY, other.LowY);
            var highY = (int)Math.Min(this.HighY, other.HighY);

            // Check if there is no overlap
            if (lowX >= highX || lowY >= highY) {
                return Enumerable.Empty<Vector>();
            }

            // Calculate the size of the overlap area
            return Region(new Vector(lowX, lowY),
                new Vector(highX - lowX, highY - lowY));
        }

        /// <summary>
        /// Checks if this Area contains or touches the
        /// <paramref name="point" />.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns><c>true</c> if the given point is within this area.
        /// </returns>
        public bool Contains(Vector point) {
            return LowX <= point.X && HighX > point.X
                && LowY <= point.Y && HighY > point.Y;
        }

        /// <summary>
        /// Checks if this Area is completely within the <paramref name="other" />
        /// Grid.
        /// </summary>
        /// <param name="other">The other Grid to check.</param>
        /// <returns><c>true</c> if the inner rectangle is completely within the
        /// outer area, <c>false</c> otherwise.</returns>
        public bool FitsInto(Grid other) =>
            LowX >= other.LowX &&
            HighX <= other.HighX &&
            LowY >= other.LowY &&
            HighY <= other.HighY;

        #region IEnumerable<Vector> 
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<Vector> GetEnumerator() {
            for (var i = 0; i < _size.Area; i++) {
                yield return _position.IsEmpty ?
                    Vector.FromIndex(i, _size) :
                    _position + Vector.FromIndex(i, _size);
            }
        }
        #endregion
    }
}
