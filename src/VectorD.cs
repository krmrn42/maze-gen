using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using static PlayersWorlds.Maps.Preconditions;

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
    public struct VectorD : IEquatable<VectorD> {
        /// <summary>
        /// Minimal floating point value used across this library. In general,
        /// we are not looking for very large or high precision floating point
        /// values so we treat anything less than this value as zero.
        /// </summary>
        public const double MIN = 1E-10;
        /// <summary>
        /// Represents a 2-dimensional zero vector.
        /// </summary>
        public static readonly VectorD Zero2D = new VectorD(new double[] { 0, 0 });

        private readonly double[] _value;
        private readonly int _hashcode;
        private double _mag;


        /// <summary>
        /// Components of this vector
        /// </summary>
        // TODO: _value can't be null, make the check more assertive
        public ReadOnlyCollection<double> Value =>
            _value == null ? null : Array.AsReadOnly(_value);
        /// <summary>
        /// <c>true</c> if this vector has no components; otherwise,
        /// <c>false</c>.
        /// </summary>
        // TODO: _value can't be null, make the check more assertive
        public bool IsEmpty => _value == null || _value.Length == 0;
        /// <summary>
        /// Gets the X coordinate of a two-dimensional vector.
        /// </summary>
        /// <returns>The X coordinate.</returns>
        /// <exception cref="InvalidOperationException">
        /// X and Y are only supported in two-dimensional space.
        /// </exception>
        public double X => _value[0];
        /// <summary>
        /// Gets the Y coordinate of a two-dimensional vector.
        /// </summary>
        /// <returns>The Y coordinate.</returns>
        /// <exception cref="InvalidOperationException">
        /// X and Y are only supported in two-dimensional space.</exception>
        public double Y => _value[1];
        /// <summary>
        /// Number of components in this vector.
        /// </summary>
        public int Dimensions => _value.Length;
        /// <summary>
        /// Squared magnitude of this vector.
        /// </summary>
        /// <returns>The magnitude squared.</returns>
        public double MagnitudeSq => _value.Sum(a => Math.Abs(a) < MIN ? 0 : (a * a));
        /// <summary>
        /// Calculates the magnitude of a vector
        /// </summary>
        /// <returns>The magnitude of the vector</returns>
        public double Magnitude =>
            double.IsNaN(_mag) ? _mag = Math.Sqrt(MagnitudeSq) : _mag;

        /// <summary>
        /// Creates a new instance of the <see cref="VectorD"/> class with the
        /// given components.
        /// </summary>
        /// <param name="dimensions">The components of the vector.</param>
        /// <exception cref="ArgumentNullException"><paramref
        /// name="dimensions" /> is null.</exception>
        public VectorD(IEnumerable<double> dimensions) {
            dimensions.ThrowIfNull("dimensions");
            _value = dimensions.Select(
                    v => Math.Round(v, 9)).ToArray();
            _hashcode = _value.Length == 0 ? _value.GetHashCode() :
                ((IStructuralEquatable)_value)
                    .GetHashCode(EqualityComparer<double>.Default);
            _mag = double.NaN;
        }

        /// <summary>
        /// Creates a new two-dimensional instance of the <see cref="VectorD"/>
        /// class with the given <paramref name="x" /> and
        /// <paramref name="y" />.
        /// </summary>
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        public VectorD(double x, double y) :
            this(new double[] { x, y }) { }

        /// <summary>
        /// Creates a new instance of the <see cref="VectorD"/>
        /// class matching the given <paramref name="intVector" />.
        /// </summary>
        /// <param name="intVector">The <see cref="Vector" /> to copy.</param>
        public VectorD(Vector intVector) :
            this(intVector.Value.Select(v => (double)v)) { }

        /// <summary>
        /// Creates an zero vector with the number of dimensions specified in
        /// <paramref ref="dimensionsNumber" /> parameter.
        /// </summary>
        /// <param name="dimensionsNumber">Number of dimensions.</param>
        /// <returns>A zero vector with the specified number of dimensions.
        /// </returns>
        public static VectorD Zero(int dimensionsNumber) {
            return new VectorD(Enumerable.Repeat(0D, dimensionsNumber));
        }

        /// <summary>
        /// Rounds components of this vector to integer producing a new <see
        /// cref="Vector" /> instance.
        /// </summary>
        /// <returns>A new <see cref="Vector" />.</returns>
        public Vector RoundToInt() =>
            new Vector(_value.Select(a => (int)Math.Round(a)));

        /// <summary>
        /// Checks if all the components of this vector are zero.
        /// </summary>
        /// <returns><c>true</c> if all the elements are zero; otherwise, 
        /// <c>false</c>.</returns>
        public bool IsZero() => _value.All(v => v >= -MIN && v <= MIN);

        /// <summary>
        /// Subtracts one vector from another by subtracting each component of
        /// the right-hand-side vector from the left-hand-side vector.
        /// </summary>
        /// <param name="one">The vector to subtract from</param>
        /// <param name="another">The vector to subtract</param>
        /// <returns>A new vector with the difference</returns>
        public static VectorD operator -(VectorD one, VectorD another) =>
            ThrowIfEmptyOrApply(one, another,
                () => new VectorD(
                    one._value.Zip(another._value, (a, b) => a - b)));

        /// <summary>
        /// Add two Vectors together.
        /// </summary>
        /// <param name="one">The first Vector</param>
        /// <param name="another">The second Vector</param>
        /// <returns>A new Vector that is the sum of the two input Vectors
        /// </returns>
        public static VectorD operator +(VectorD one, VectorD another) =>
            ThrowIfEmptyOrApply(one, another,
                () => new VectorD(
                    one._value.Zip(another._value, (a, b) => a + b)));

        /// <summary>
        /// Divides a <see cref="VectorD"/> by a scalar.
        /// </summary>
        /// <param name="dividend">The <see cref="VectorD"/> to divide.</param>
        /// <param name="divisor">The scalar to divide by.</param>
        /// <returns>The quotient of the division.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the divisor
        /// less than <see cref="MIN"/>.</exception>
        public static VectorD operator /(VectorD dividend, double divisor) =>
            ThrowIfEmptyOrApply(dividend, Zero2D,
            () => new VectorD(dividend._value.Select(e => divisor < MIN ?
                throw new InvalidOperationException("Can't divide by zero") :
                e / divisor)));

        /// <summary>Multiplies a <see cref="VectorD"/> by a scalar.</summary>
        /// <param name="one">The <see cref="VectorD"/> to multiply.</param>
        /// <param name="another">The scalar to multiply by.</param>
        /// <returns>A new <see cref="VectorD"/> with each element multiplied
        /// by <paramref name="another"/>.</returns>
        public static VectorD operator *(VectorD one, double another) =>
            ThrowIfEmptyOrApply(one, VectorD.Zero2D,
            () => new VectorD(one._value.Select(e => e * another)));

        /// <summary>
        /// Applies a Hadamard product to this and <paramref name="another"/>
        /// vectors.
        /// </summary>
        /// <param name="another"></param>
        public VectorD Hadamard(VectorD another) {
            Check(Dimensions == another.Dimensions,
                "Cannot apply Hadamard product to vectors of different " +
                "dimensions");
            return new VectorD(_value.Zip(another._value, (i1, i2) => i1 * i2));

        }

        /// <summary>Checks if two Vectors are equal</summary>
        /// <param name="one">First Vector to check</param>
        /// <param name="another">Second Vector to check</param>
        /// <returns><c>true</c> if both Vectors are equal; otherwise,
        /// <c>false</c>.</returns>
        public static bool operator ==(VectorD one, VectorD another) =>
            one.Equals(another);

        /// <summary>
        /// Determines whether two specified vectors are not equal.
        /// </summary>
        /// <param name="one">The first vector to compare.</param>
        /// <param name="another">The second vector to compare.</param>
        /// <returns>
        /// <c>true</c> if the vectors are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(VectorD one, VectorD another) =>
            !one.Equals(another);

        /// <summary>
        /// Determines equality comparing all components of the given vectors by
        /// value.
        /// </summary>
        /// <param name="obj">
        /// The other <see cref="VectorD" /> to compare with this instance.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="object" /> is equal to this
        /// instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => this.Equals((VectorD)obj);

        /// <summary>
        /// Determines equality comparing all components of the given vectors by
        /// value.
        /// </summary>
        /// <param name="another">
        /// The other <see cref="VectorD" /> to compare with this instance.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="object" /> is equal to this
        /// instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(VectorD another) =>
            (this.IsEmpty && another.IsEmpty)
            || (!this.IsEmpty && !another.IsEmpty &&
                    this._value.Zip(another._value,
                                    (a, b) => Math.Abs(a - b) < MIN)
                               .All(a => a));

        /// <summary>
        /// Gets the hash code for this vector by value of its components.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => _hashcode;


        /// <summary>
        /// Returns a string of the form <c>1.23x-4.56</c> that represents the
        /// current object.
        /// </summary>
        /// <remarks>
        /// A loss of precision will occur when using the produced string in
        /// <see cref="Parse(string)"/> to re-create a new <see cref="VectorD"/>
        /// instance because while converting to string, all components will be
        /// rounded to two significant digits.
        /// </remarks>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString() => IsEmpty ? "<empty>" :
            _value.Length == 0 ? "--" :
            string.Join("x", _value.Select(v => v.ToString("F2")));

        /// <summary>
        /// Creates a new vector with the specified magnitude maintaining the 
        /// direction of this vector.
        /// </summary>
        /// <param name="newMagnitude">The new magnitude for the vector</param>
        /// <returns>A new vector with the same direction and the specified
        /// magnitude</returns>
        public VectorD WithMagnitude(double newMagnitude) {
            if (IsEmpty || IsZero()) return this;
            if (newMagnitude > -MIN && newMagnitude < MIN) return Zero2D;
            var k = newMagnitude / Magnitude;
            return new VectorD(_value.Select(a => k * a));
        }

        private static T ThrowIfEmptyOrApply<T>(VectorD one,
                                                VectorD another,
                                                Func<T> apply) {
            if (one.IsEmpty || another.IsEmpty)
                throw new InvalidOperationException(
                    "Cannot operate on an empty vector");
            return apply();
        }

        internal static VectorD Parse(string v) =>
            new VectorD(v.Trim().Split('x').Select(s => {
                if (!double.TryParse(s.Trim('P', 'S'), out var val)) {
                    throw new FormatException(
                        $"Input string was not in a correct format ({s}).");
                }
                return val;
            }));

        internal VectorD Reverse() {
            return new VectorD(_value.Select(a => -a));
        }

        internal static VectorD RandomUnit(RandomSource source, int dimensions = 2) {
            VectorD random;
            do {
                random = new VectorD(
                    source.NextBytes(dimensions)
                                .Select(a => (a % 3) - 1D));
            } while (random._value.All(v => v == 0));
            return random;
        }

        /// <summary>
        /// Checks if this vector can be used as a size, i.e. it has components,
        /// and all components are greater than zero.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void ThrowIfNotAValidSize() {
            if (_value.Length == 0 || _value.Any(i => i <= 0))
                throw new ArgumentException(
                    $"This Vector is not a valid size: {this}");
        }
    }
}
