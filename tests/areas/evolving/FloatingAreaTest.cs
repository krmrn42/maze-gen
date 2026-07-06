using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Areas.Evolving {

    [TestFixture]
    internal class FloatingAreaTest : Test {
        [Test]
        public void Parse() {
            var data = "P0.53x7.5;S-3.01x0.01";
            var expectedPosition = new VectorD(0.53D, 7.5D);
            var expectedSize = new VectorD(-3.01D, 0.01D);
            var floatingArea = FloatingArea.Parse(data);
            Assert.That(floatingArea.Position, Is.EqualTo(expectedPosition));
            Assert.That(floatingArea.Size, Is.EqualTo(expectedSize));
        }

        [Test]
        public void ToStringIsValid() {
            var data = "P0.53x7.50;S-3.01x0.01";
            var floatingArea = FloatingArea.Parse(data);
            Assert.That(floatingArea.ToString(), Is.EqualTo(data));
        }

        [Test]
        public void Overlaps() {
            var area1 = FloatingArea.FromMapArea(Area.Create(
                new Vector(0, 0), new Vector(10, 10), AreaType.Maze),
                Vector.Zero2D);
            var area2 = FloatingArea.FromMapArea(Area.Create(
                new Vector(5, 5), new Vector(10, 10), AreaType.Maze),
                Vector.Zero2D);
            Assert.That(area1.Overlaps(area2), Is.True);
            Assert.Throws<InvalidOperationException>(() => area1.Overlaps(area1));
        }

        [Test]
        public void Contains() {
            var area1 = FloatingArea.FromMapArea(Area.Create(
                new Vector(0, 0), new Vector(10, 10), AreaType.Maze),
                Vector.Zero2D);
            var area2 = FloatingArea.FromMapArea(Area.Create(
                new Vector(5, 5), new Vector(10, 10), AreaType.Maze),
                Vector.Zero2D);
            Assert.That(area1.Contains(new VectorD(0.5D, 0.5D)), Is.True);
            Assert.That(area2.Contains(new VectorD(1.5D, 1.5D)), Is.False);
        }

        [Test]
        public void CenterIsValid() {
            Assert.That(
                FloatingArea.Unlinked(new VectorD(0, 0), new VectorD(4, 4))
                .Center, Is.EqualTo(new VectorD(2, 2)));
            Assert.That(
                FloatingArea.Unlinked(new VectorD(3, 3), new VectorD(4, 4))
                .Center, Is.EqualTo(new VectorD(5, 5)));
            Assert.That(
                FloatingArea.Unlinked(new VectorD(0, 0), new VectorD(5, 5))
                .Center, Is.EqualTo(new VectorD(2.5, 2.5)));
            Assert.That(
                FloatingArea.Unlinked(new VectorD(3, 3), new VectorD(5, 5))
                .Center, Is.EqualTo(new VectorD(5.5, 5.5)));
        }

        [Test, Category("Integration")]
        public void DistanceTo(
            [ValueSource("AreaPairs")]
            (FloatingArea[], VectorD, bool) parameters) {
            var (d, overlap) =
                parameters.Item1[0].DistanceTo(parameters.Item1[1]);
            Assert.That(d, Is.EqualTo(parameters.Item2));
            Assert.That(overlap, Is.EqualTo(parameters.Item3));
        }

        [Test]
        public void DistanceToDebug() {
            DistanceTo(AreaPairs().ElementAt(2));
        }

        public static IEnumerable<(FloatingArea[], VectorD, bool)> AreaPairs() {
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P4x1;S2x2") },
                new VectorD(-1, 0), false);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P3x1;S2x2") },
                new VectorD(0, 0), false);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P2x1;S2x2") },
                new VectorD(1, 0), true);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P1x1;S2x2") },
                new VectorD(0, 0), true);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P0x1;S2x2") },
                new VectorD(-1, 0), true);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P-1x1;S2x2") },
                new VectorD(0, 0), false);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P-2x1;S2x2") },
                new VectorD(1, 0), false);

            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P4x2;S2x2") },
                new VectorD(-1, 1), false);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P3x2;S2x2") },
                new VectorD(0, 1), false);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P2x2;S2x2") },
                new VectorD(1, 1), true);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P1x2;S2x2") },
                new VectorD(0, 1), true);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P0x2;S2x2") },
                new VectorD(-1, 1), true);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P-1x2;S2x2") },
                new VectorD(0, 1), false);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P1x1;S2x2"),
                    FloatingArea.Parse("P-2x2;S2x2") },
                new VectorD(1, 1), false);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P5x5;S4x4"),
                    FloatingArea.Parse("P2x2;S4x4") },
                new VectorD(-1, -1), true);
            yield return (
                new FloatingArea[] {
                    FloatingArea.Parse("P6x6;S4x4"),
                    FloatingArea.Parse("P1x1;S4x4") },
                new VectorD(1, 1), false);
        }

    }
}
