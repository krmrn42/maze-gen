using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PlayersWorlds.Maps {
    [TestFixture]
    public class VectorTest : Test {
        [Test]
        public void Vector_IsInitialized() {
            var p = new Vector(2, 5);
            Assert.That(p.X, Is.EqualTo(2));
            Assert.That(p.Y, Is.EqualTo(5));
        }

        [Test]
        public void Size_NotInitialized() {
            Assert.Throws<InvalidOperationException>(() => Assert.That(Vector.Empty.X, Is.EqualTo(2)));
            Assert.Throws<InvalidOperationException>(() => Assert.That(Vector.Empty.Y, Is.EqualTo(2)));
        }

        [Test]
        public void Vector_EqualityIsCheckedByValue() {
            var p1 = new Vector(1, 2);
            var p2 = new Vector(1, 2);
            Assert.That(p1, Is.EqualTo(p2));
            Assert.That(p1 == p2, Is.True);
            Assert.That(p1.Equals(p2), Is.True);
            Assert.That(p1.Equals((object)p2), Is.True);
            Assert.That(Vector.Empty, Is.Not.EqualTo(p2));
        }

        [Test]
        public void Vector_GetHashCodeIsDerivedFromValue() {
            var p1 = new Vector(1, 2);
            var p2 = new Vector(1, 2);
            Assert.That(p1.GetHashCode(), Is.EqualTo(p2.GetHashCode()));
            Assert.DoesNotThrow(() => Vector.Empty.GetHashCode());
        }

        [Test]
        public void Vector_ToStringFormat() {
            var p1 = new Vector(new int[] { 1, 2, 2, 3 });
            Assert.That(p1.ToString(), Is.EqualTo("1x2x2x3"));
            Assert.That(Vector.Empty.ToString(), Is.EqualTo("<empty>"));
        }

        [Test]
        public void Vector_ZeroVectorIsNotEmpty() {
            var p = new Vector(new int[] { 0 });
            Assert.That(p, Is.Not.EqualTo(Vector.Empty));
        }

        [Test]
        public void Vector_EmptyVectorIsEmpty() {
            var p = new Vector();
            Assert.That(p, Is.EqualTo(Vector.Empty));
        }

        [Test]
        public void Vector_EmptyIsEmpty() {
            Assert.That(Vector.Empty, Is.EqualTo(Vector.Empty));
        }

        [Test]
        public void Vector_NotEqualToNull() {
            var p0 = new Vector(3, 2);
            var p1 = new Vector();

            Assert.That(p1.Value, Is.Empty);
            Assert.That(p1 != p0, Is.True);
            Assert.That(p0 != p1, Is.True);
        }

        [Test]
        public void Vector_CanAddAndSubtract() {
            var p0 = new Vector(3, 2);
            var p1 = new Vector(1, 2);
            var s1 = new Vector(3, 4);

            Assert.That(new Vector(4, 4), Is.EqualTo(p0 + p1));
            Assert.That(new Vector(2, 0), Is.EqualTo(p0 - p1));
            Assert.That(new Vector(6, 6), Is.EqualTo(p0 + s1));
            Assert.That(new Vector(0, -2), Is.EqualTo(p0 - s1));
            Assert.Throws<InvalidOperationException>(() => Assert.That(Vector.Empty + p1, Is.EqualTo(new Vector(4, 4))));
            Assert.Throws<InvalidOperationException>(() => Assert.That(Vector.Empty + s1, Is.EqualTo(new Vector(4, 4))));
        }

        [Test]
        public void Vector_ConstructorChecksArguments() {
            Assert.Throws<ArgumentNullException>(() => new Vector(null));
        }

        [Test]
        public void Vector_ThrowsIfNotAValidSize() {
            Assert.Throws<ArgumentException>(() => new Vector(new int[] { 0, 1, 2, 3, -1 }).ThrowIfNotAValidSize());
            Assert.Throws<ArgumentException>(() => new Vector(new int[] { 1, -1 }).ThrowIfNotAValidSize());
            Assert.DoesNotThrow(() => new Vector(new int[] { 1 }).ThrowIfNotAValidSize());
        }

        [Test]
        public void Vector_SnappedForce() {
            Assert.That(new Vector(-4, -2), Is.EqualTo(new VectorD(new double[] { 2, 1 }).WithMagnitude(-5).RoundToInt()));
            Assert.That(new Vector(-4, -2), Is.EqualTo(new VectorD(new double[] { -2, -1 }).WithMagnitude(5).RoundToInt()));
            Assert.That(new Vector(4, 2), Is.EqualTo(new VectorD(new double[] { -2, -1 }).WithMagnitude(-5).RoundToInt()));
            Assert.That(new Vector(4, 2), Is.EqualTo(new VectorD(new double[] { 2, 1 }).WithMagnitude(5).RoundToInt()));
            Assert.That(new Vector(2, 1), Is.EqualTo(new VectorD(new double[] { 10, 5 }).WithMagnitude(2.5).RoundToInt()));
            Assert.That(new Vector(0, 0), Is.EqualTo(new VectorD(new double[] { -8, 4 }).WithMagnitude(0).RoundToInt()));
            Assert.That(new Vector(0, 1), Is.EqualTo(new VectorD(new double[] { 0, -10 }).WithMagnitude(-1).RoundToInt()));
            Assert.That(new Vector(0, 0), Is.EqualTo(new VectorD(new double[] { 0, 0 }).WithMagnitude(1044).RoundToInt()));
            Assert.That(new Vector(566, 566), Is.EqualTo(new VectorD(new double[] { -3, -3 }).WithMagnitude(-800).RoundToInt()));
        }

        [Test]
        public void Vector_ToIndex() {
            Assert.That(new Vector(1, 2).ToIndex(new Vector(3, 4)), Is.EqualTo(7));
            Assert.That(new Vector(2, 3).ToIndex(new Vector(3, 4)), Is.EqualTo(11));
            Assert.Throws<IndexOutOfRangeException>(() => new Vector(5, 2).ToIndex(new Vector(4, 3)));
        }

        [Test]
        public void Vector_FromIndex() {
            Assert.That(Vector.FromIndex(2, new Vector(3, 3)), Is.EqualTo(new Vector(2, 0)));
            Assert.That(Vector.FromIndex(5, new Vector(3, 3)), Is.EqualTo(new Vector(2, 1)));
        }

        [Test]
        public void VectorD_Parse() {
            Assert.That(new VectorD(0.3, -1), Is.EqualTo(VectorD.Parse("S0.3x-1")));
        }

        [Test]
        public void FitsInto() {
            Assert.That(new Vector(1, 2).FitsInto(new Vector(2, 2)), Is.True);
            Assert.That(new Vector(10, 10).FitsInto(new Vector(20, 20)), Is.True);
            Assert.That(new Vector(10, 10).FitsInto(new Vector(2, 2)), Is.False);
        }

        [Test]
        public void IsIn() {
            Assert.That(new Vector(-1, 2).IsIn(new Vector(-2, -2), new Vector(5, 5)), Is.True);
            Assert.That(new Vector(2, 2).IsIn(new Vector(2, 2), new Vector(2, 2)), Is.True);
            Assert.That(new Vector(3, 2).IsIn(new Vector(2, 2), new Vector(2, 2)), Is.True);
            Assert.That(new Vector(2, 3).IsIn(new Vector(2, 2), new Vector(2, 2)), Is.True);
            Assert.That(new Vector(3, 3).IsIn(new Vector(2, 2), new Vector(2, 2)), Is.True);
            Assert.That(new Vector(4, 4).IsIn(new Vector(2, 2), new Vector(2, 2)), Is.False);
            Assert.That(new Vector(1, 1).IsIn(new Vector(2, 2), new Vector(2, 2)), Is.False);
        }

        [Test]
        public void ToIndex_Matches_ToIndex() {
            var vector = new Vector(new int[] { 2, 0 });
            var one = vector.ToIndex(new Vector(10, 10));
            var another = vector.ToIndex(new Vector(new int[] { 10, 10 }));
            Assert.That(one, Is.EqualTo(another));
        }

        [Test]
        public void FromIndex_Matches_FromIndex() {
            var index = 75;
            var one = Vector.FromIndex(index, new Vector(10, 10));
            var another = Vector.FromIndex(index, new Vector(new int[] { 10, 10 }));
            Assert.That(one, Is.EqualTo(another));
        }

        [Test]
        public void ToIndex_ShouldCalculateCorrectIndex_1D() {
            var vector = new Vector(new int[] { 3 });
            Assert.That(vector.ToIndex(new Vector(new int[] { 10 })), Is.EqualTo(3));
            Assert.That(vector.ToIndex(new Vector(new int[] { 5 })), Is.EqualTo(3));
        }

        [Test]
        public void ToIndex_ShouldCalculateCorrectIndex_2D() {
            var vector = new Vector(2, 3);
            Assert.That(vector.ToIndex(new Vector(new int[] { 10, 5 })), Is.EqualTo(32));
            Assert.That(vector.ToIndex(new Vector(new int[] { 5, 6 })), Is.EqualTo(17));
        }

        [Test]
        public void ToIndex_ShouldCalculateCorrectIndex_3D() {
            var vector = new Vector(new int[] { 1, 4, 2 });
            Assert.That(vector.ToIndex(new Vector(new int[] { 10, 5, 3 })), Is.EqualTo(141));
            Assert.That(vector.ToIndex(new Vector(new int[] { 6, 7, 3 })), Is.EqualTo(109));
        }

        [Test]
        public void ToIndex_FromIndex_RoundTripConversion() {
            var dimensions = new int[] { 10, 5, 3 };
            for (var x = 0; x < dimensions[0]; x++) {
                for (var y = 0; y < dimensions[1]; y++) {
                    for (var z = 0; z < dimensions[2]; z++) {
                        var vector = new Vector(new int[] { x, y, z });
                        var index = vector.ToIndex(new Vector(dimensions));
                        var convertedVector = Vector.FromIndex(index, new Vector(dimensions));
                        Assert.That(convertedVector, Is.EqualTo(vector));
                    }
                }
            }
        }

        [Test]
        public void ToIndex_FromIndex_Valid5D() {
            var dimensions = new int[] { 1, 2, 3, 2, 1 };
            var space = new Vector(dimensions);
            var array = new Vector[space.Area];
            var counter = 0;
            for (var x = 0; x < dimensions[0]; x++) {
                for (var y = 0; y < dimensions[1]; y++) {
                    for (var z = 0; z < dimensions[2]; z++) {
                        for (var a = 0; a < dimensions[3]; a++) {
                            for (var b = 0; b < dimensions[4]; b++) {
                                var vector = new Vector(new int[] { x, y, z, a, b });
                                var index = vector.ToIndex(new Vector(dimensions));
                                array[index] = vector;
                                counter++;
                                var convertedVector = Vector.FromIndex(index, new Vector(dimensions));
                                Assert.That(convertedVector, Is.EqualTo(vector));
                            }
                        }
                    }
                }
            }
            Assert.That(array.Distinct().Count(), Is.EqualTo(array.Count()));
        }

        [Test]
        public void ToIndex_ShouldThrowException_DimensionOverflow() {
            var vector = new Vector(3, 5);
            Assert.That(() => vector.ToIndex(new Vector(new int[] { 5, 3 })), Throws.InstanceOf<IndexOutOfRangeException>());
        }

        [Test]
        public void ToIndex_ShouldThrowException_DimensionMismatch() {
            var vector = new Vector(1, 2);
            Assert.That(() => vector.ToIndex(new Vector(new int[] { 10 })), Throws.ArgumentException);
        }

        [Test]
        public void FromIndex_ShouldThrowException_DimensionMismatch() {
            Assert.That(() => Vector.FromIndex(28, new Vector(new int[] { 10 })), Throws.ArgumentException);
        }

        [Test]
        public void NorthEastComparer_ComparesCorrectly() {
            var actual = new List<Vector> { new Vector(1, 2), new Vector(3, 4) };
            actual.Sort(new Vector.NorthEastComparer());
            var expected = new List<Vector> { new Vector(1, 2), new Vector(3, 4) };
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
