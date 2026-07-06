using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace PlayersWorlds.Maps {
    [TestFixture]
    public class VectorDTest : Test {
        [Test]
        public void IsInitialized() {
            var p = new VectorD(2.5, -5.1);
            Assert.That(p.X, Is.EqualTo(2.5).Within(VectorD.MIN));
            Assert.That(p.Y, Is.EqualTo(-5.1).Within(VectorD.MIN));
        }

        [Test]
        public void EqualityIsCheckedByValue() {
            var p1 = new VectorD(1.2, -2.8);
            var p2 = new VectorD(1.2, -2.8);
            Assert.That(p1, Is.EqualTo(p2));
            Assert.That(p1 == p2, Is.True);
            Assert.That(p1.Equals(p2), Is.True);
            Assert.That(p1.Equals((object)p2), Is.True);
        }

        [Test]
        public void GetHashCodeIsDerivedFromValue() {
            var p1 = new VectorD(1, 2);
            var p2 = new VectorD(1, 2);
            Assert.That(p1.GetHashCode(), Is.EqualTo(p2.GetHashCode()));
        }

        [Test]
        public void ToStringFormat() {
            var p1 = new VectorD(new double[] { 1, 2, 2, 3 });
            Assert.That(p1.ToString(), Is.EqualTo("1.00x2.00x2.00x3.00"));
        }

        [Test]
        public void InitializedEmpty() {
            var p1 = new VectorD();

            Assert.That(p1.Value, Is.Null, "Value should be null");
            Assert.That(p1.IsEmpty, Is.True, "IsEmpty should be true");
            Assert.That(p1 != VectorD.Zero2D, Is.True, "p1!= VectorD.Zero2D");
            Assert.That(VectorD.Zero2D != p1, Is.True, "VectorD.Zero2D!= p1");
        }

        [Test]
        public void InitializedAsNull() {
            IEnumerable<double> n = null;
            Assert.Throws<ArgumentNullException>(() => new VectorD(n));
        }

        [Test]
        public void IsZero() {
            var p1 = new VectorD(1, 2);
            var p2 = new VectorD(0, 0);
            Assert.That(p2.IsEmpty, Is.False);
            Assert.That(p2.IsZero(), Is.True);
            Assert.That(p1.IsZero(), Is.False);
        }

        [Test]
        public void CanAddAndSubtract() {
            var p0 = new VectorD(3.5, -2.6);
            var p1 = new VectorD(1.1, 2.3);
            var s1 = new VectorD(-3.0, 4.7);

            Assert.That(new VectorD(4.6, -0.3), Is.EqualTo(p0 + p1));
            Assert.That(new VectorD(2.4, -4.9), Is.EqualTo(p0 - p1));
            Assert.That(new VectorD(0.5, 2.1), Is.EqualTo(p0 + s1));
            Assert.That(new VectorD(6.5, -7.3), Is.EqualTo(p0 - s1));

            Assert.Throws<InvalidOperationException>(() => { var _ = p0 + new VectorD(); });
        }

        [Test]
        public void ConstructorChecksArguments() {
            IEnumerable<double> n = null;
            Assert.Throws<ArgumentNullException>(() => new VectorD(n));
        }

        [Test]
        public void SnappedForce() {
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
        public void VectorD_Parse() {
            Assert.That(new VectorD(0.3, -1), Is.EqualTo(VectorD.Parse("S0.3x-1")));
            Assert.Throws<FormatException>(() => VectorD.Parse("wrong format"));
        }
    }
}
