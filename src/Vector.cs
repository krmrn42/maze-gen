using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// A coordinate on a map, or a size of an object on the map.
    /// </summary>
    /// <remarks>
    /// This class is not supposed to be used for vector maths.
    /// See <a href="https://www.nuget.org/packages/System.Numerics.Vectors/"
    /// title="System.Numerics.Vectors on NuGet">System.Numerics.Vectors</a>
    /// for that.
    /// </remarks>
    public readonly struct Vector : IEquatable<Vector> {
        /// <summary>
        /// Empty instance of <see cref="Vector" />.
        /// </summary>
        public static readonly Vector Empty = new Vector();
        /// <summary>
        /// Represents a 2-dimensional zero vector.
        /// </summary>
        public static readonly Vector Zero2D = new Vector(0, 0);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the top-left corner on the
        /// euclidean plane (-1, 1).
        /// </summary>
        public static readonly Vector NorthWest2D = new Vector(-1, 1);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the top on the
        /// euclidean plane (0, 1).
        /// </summary>
        public static readonly Vector North2D = new Vector(0, 1);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the top-right on the
        /// euclidean plane (1, 1).
        /// </summary>
        public static readonly Vector NorthEast2D = new Vector(1, 1);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the left on the
        /// euclidean plane (-1, 0).
        /// </summary>
        public static readonly Vector West2D = new Vector(-1, 0);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the right on the
        /// euclidean plane (1, 0).
        /// </summary>
        public static readonly Vector East2D = new Vector(1, 0);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the bottom-left on the
        /// euclidean plane (-1, -1).
        /// </summary>
        public static readonly Vector SouthWest2D = new Vector(-1, -1);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the bottom on the
        /// euclidean plane (0, -1).
        /// </summary>
        public static readonly Vector South2D = new Vector(0, -1);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the bottom-right on the
        /// euclidean plane (1, -1).
        /// </summary>
        public static readonly Vector SouthEast2D = new Vector(1, -1);

        private readonly int[] _value;
        private readonly int _hashcode;

        /// <summary>
        /// Components of this vector.
        /// </summary>
        internal ReadOnlyCollection<int> Value =>
            IsEmpty ? new List<int>().AsReadOnly() : Array.AsReadOnly(_value);
        /// <summary>
        /// The vector is not empty only if it was initialized with components.
        /// </summary>
        // TODO: _value can't be null, make the check more assertive
        public bool IsEmpty => _value == null || _value.Length == 0;
        /// <summary>
        /// Returns a first component of a non-empty vector.
        /// </summary>
        public int X => !IsEmpty ? _value[0] :
            throw new InvalidOperationException(
                "X is only supported in non-empty vectors");
        /// <summary>
        /// Returns a second component of a non-empty vector.
        /// </summary>
        public int Y => !IsEmpty && _value.Length > 1 ? _value[1] :
            throw new InvalidOperationException(
                "Y is only supported in non-empty two+ dimensional space");
        /// <summary>
        /// Number of components in this vector.
        /// </summary>
        public int Dimensions => _value.Length;
        /// <summary>
        /// A product of the components of this vector.
        /// </summary>
        public int Area => _value.Aggregate((a, b) => a * b);
        /// <summary>
        /// Squared magnitude of this vector.
        /// </summary>
        public int MagnitudeSq => _value.Sum(a => a * a);

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector"/> struct
        /// representing a 2-dimensional vector.
        /// </summary>
        /// <param name="x">The X coordinate (horizontal component) of the
        /// vector.</param>
        /// <param name="y">The Y coordinate (vertical component) of the
        /// vector.</param>
        public Vector(int x, int y) : this(new int[] { x, y }) { }

        /// <summary>
        /// Creates a new vector with the specified components.
        /// </summary>
        /// <param name="dimensions">Components of the vector</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dimensions" /> is null.</exception>
        public Vector(IEnumerable<int> dimensions) {
            dimensions.ThrowIfNull(nameof(dimensions));
            _value = dimensions.ToArray();
            _hashcode = _value.Length == 0 ? _value.GetHashCode() :
                ((IStructuralEquatable)_value)
                    .GetHashCode(EqualityComparer<int>.Default);
        }

        /// <summary>
        /// Creates an zero vector with the number of dimensions specified in
        /// <paramref ref="dimensionsNumber" /> parameter.
        /// </summary>
        /// <param name="dimensionsNumber">Number of dimensions.</param>
        /// <returns>A zero vector with the specified number of dimensions.
        /// </returns>
        public static Vector Zero(int dimensionsNumber) {
            return new Vector(Enumerable.Repeat(0, dimensionsNumber));
        }

        /// <summary>
        /// Checks if this vector can be used as a size, i.e. it has components,
        /// and all components are greater than zero.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void ThrowIfNotAValidSize(string paramName = null) {
            if (_value.Length == 0 || _value.Any(i => i < 0))
                throw new ArgumentException($"This Vector is not a valid size: {this}", paramName);
        }

        private static T ThrowIfEmptyOrApply<T>(Vector one, Vector another, Func<Vector, Vector, T> apply) {
            if (one.IsEmpty || another.IsEmpty)
                throw new InvalidOperationException("Cannot operate on an empty vector");
            return apply(one, another);
        }

        /// <summary>
        /// <see cref="Equals(Vector)"/>.
        /// </summary>
        public static bool operator ==(Vector one, Vector another) =>
            one.Equals(another);

        /// <summary>
        /// <see cref="Equals(Vector)"/>.
        /// </summary>
        public static bool operator !=(Vector one, Vector another) =>
            !one.Equals(another);

        /// <summary>
        /// Add two Vectors together.
        /// </summary>
        /// <param name="one">The first Vector</param>
        /// <param name="another">The second Vector</param>
        /// <returns>A new Vector that is the sum of the two input Vectors
        /// </returns>
        public static Vector operator +(Vector one, Vector another) =>
            ThrowIfEmptyOrApply(one, another,
                (a, b) => new Vector(
                    one._value.Zip(another._value, (x1, x2) => x1 + x2)));

        /// <summary>
        /// Subtracts one vector from another by subtracting each component of
        /// the right-hand-side vector from the left-hand-side vector.
        /// </summary>
        /// <param name="one">The vector to subtract from</param>
        /// <param name="another">The vector to subtract</param>
        /// <returns>A new vector with the difference</returns>
        public static Vector operator -(Vector one, Vector another) =>
            ThrowIfEmptyOrApply(one, another,
                (a, b) => new Vector(
                    one._value.Zip(another._value, (x1, x2) => x1 - x2)));

        /// <summary>
        /// Checks if a region of size <paramref name="container"/> fits a
        /// region of the size of this vector.
        /// </summary>
        // TODO: Rename to AllComponentsAreLessOrEqual? Or something like that?
        public bool FitsInto(Vector container) =>
            _value.Zip(container._value, (a, b) => a <= b).All(b => b);

        /// <summary>
        /// Checks if this point is inside a region positioned at
        /// <paramref name="position"/> of size <paramref name="size"/>.
        /// </summary>
        /// <param name="position">Position of the container</param>
        /// <param name="size">Size of the container</param>
        /// <returns><c>true</c> if the point fits in the container, otherwise
        /// <c>false</c></returns>
        public bool IsIn(Vector position, Vector size) =>
            position._value.Zip(size._value, (p, s) => new int[] { p, p + s })
                           .Zip(_value, (ps, v) => v >= ps[0] && v < ps[1])
                           .All(isIn => isIn);

        /// <summary>
        /// <see cref="Equals(Vector)"/>.
        /// </summary>
        public override bool Equals(object obj) => this.Equals((Vector)obj);

        /// <summary>
        /// Returns a hash code of this vector based on its components. If no
        /// components, returns the hash code of the empty int[].
        /// </summary>
        /// <returns></returns>
        // TODO: _value can't be null, make the check more assertive
        public override int GetHashCode() =>
            _value == null ? base.GetHashCode() : _hashcode;

        /// <summary>
        /// Returns a string representation of this vector of the form
        /// <c>x,y</c>.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => IsEmpty ? "<empty>" : _value.Length == 0 ? "-0-" : string.Join("x", _value);

        /// <summary>
        /// Checks if two vectors have the same components.
        /// </summary>
        public bool Equals(Vector another) =>
            (this.IsEmpty && another.IsEmpty)
            || (!this.IsEmpty && !another.IsEmpty && this._value.SequenceEqual(another._value));

        /// <summary>
        /// Calculates the linear index of a <see cref="Vector"/> position
        /// within a space with the specified dimensions.
        /// </summary>
        /// <param name="spaceDimensions">A <see cref="Vector"/>
        /// representing the size of the space in (e.g., [width, height]).
        /// </param>
        /// <returns>The linear index of the <see cref="Vector"/> position
        /// within the space.</returns>
        /// <exception cref="ArgumentException">Throws if the
        /// <see cref="Vector"/> dimension does not match the provided
        /// dimensions array, or any of the coordinates exceed the space
        /// size.</exception>
        public int ToIndex(Vector spaceDimensions) {
            if (_value.Length != spaceDimensions._value.Length) {
                throw new ArgumentException("Vector dimensions do not match array dimensions");
            }

            var index = 0;
            var multiplier = 1;
            for (var i = 0; i < _value.Length; i++) {
                if (_value[i] >= spaceDimensions._value[i]) {
                    throw new IndexOutOfRangeException(
                        $"Vector dimension {i} ({string.Join("x", _value)}) is larger than" +
                        $" space dimension {i} ({string.Join("x", spaceDimensions._value)})");
                }
                index += _value[i] * multiplier;
                multiplier *= spaceDimensions._value[i];
            }
            return index;
        }

        /// <summary>
        /// Converts a linear index within a space with the specified dimensions
        /// to a corresponding <see cref="Vector"/> position.
        /// </summary>
        /// <param name="index">The linear index within the space.</param>
        /// <param name="spaceDimensions">A <see cref="Vector"/> representing
        /// the size of the space (e.g., [width, height]).</param>
        /// <returns>The corresponding <see cref="Vector"/> position within the
        /// space.</returns>
        /// <exception cref="ArgumentException">Throws if the provided index is
        /// out of the bounds for the specified space.</exception>
        public static Vector FromIndex(int index, Vector spaceDimensions) {
            var coordinates = new int[spaceDimensions._value.Length];
            var remaining = index;
            for (var i = 0; i < spaceDimensions._value.Length; i++) {
                coordinates[i] = remaining % spaceDimensions._value[i];
                remaining /= spaceDimensions._value[i];
            }
            if (remaining > 0) {
                throw new ArgumentException("Index does not fit into space dimensions");
            }
            return new Vector(coordinates);
        }

        /// <summary>
        /// Parses a string of a form <c>1x2</c> into a <see cref="Vector"/>.
        /// </summary>
        /// <param name="serialized">Serialized representation of a vector
        /// </param>
        /// <returns>A new instance of <see cref="Vector"/></returns>
        public static Vector Parse(string serialized) {
            return new Vector(serialized.Split('x').Select(int.Parse));
        }

        public void ThrowIfEmpty(string paramName) {
            if (this.IsEmpty) {
                throw new InvalidOperationException(paramName + " is empty");
            }
        }

        /// <summary>
        /// Compares two <see cref="Vector"/>s based on their components so that
        /// components with lower coordinates come first.
        /// </summary>
        public class NorthEastComparer : IComparer<Vector> {
            /// <inheritdoc />
            public int Compare(Vector a, Vector b) {
                var cmp = a.Y - b.Y;
                if (cmp == 0) {
                    cmp = a.X - b.X;
                }
                return cmp;
            }
        }
    }
}
