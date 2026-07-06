using System;
using NUnit.Framework;

namespace PlayersWorlds.Maps.World {
    [TestFixture]
    public class RegionAddressTest {
        [Test]
        public void ToWorld_And_FromWorld_RoundTrip_2D() {
            var address = new RegionAddress(new Vector(2, -3));
            var regionSize = new Vector(42, 21);
            var local = new Vector(5, 7);

            var world = address.ToWorld(regionSize, local);
            Assert.That(world, Is.EqualTo(new Vector(2 * 42 + 5, -3 * 21 + 7)));

            var (backAddress, backLocal) =
                RegionAddress.FromWorld(world, regionSize);
            Assert.That(backAddress, Is.EqualTo(address));
            Assert.That(backLocal, Is.EqualTo(local));
        }

        [Test]
        public void FromWorld_UsesFlooredDivision_ForNegativeCoordinates() {
            var regionSize = new Vector(10, 10);
            // world (-1, -1) belongs to region (-1, -1) with local (9, 9).
            var (address, local) =
                RegionAddress.FromWorld(new Vector(-1, -1), regionSize);
            Assert.That(address, Is.EqualTo(new RegionAddress(new Vector(-1, -1))));
            Assert.That(local, Is.EqualTo(new Vector(9, 9)));
            // and it round-trips back.
            Assert.That(address.ToWorld(regionSize, local),
                Is.EqualTo(new Vector(-1, -1)));
        }

        [Test]
        public void RoundTrip_HoldsAcrossAWholeRegionSpan() {
            var regionSize = new Vector(4, 3);
            for (var wx = -6; wx <= 6; wx++) {
                for (var wy = -6; wy <= 6; wy++) {
                    var world = new Vector(wx, wy);
                    var (address, local) =
                        RegionAddress.FromWorld(world, regionSize);
                    Assert.That(local.X, Is.InRange(0, regionSize.X - 1));
                    Assert.That(local.Y, Is.InRange(0, regionSize.Y - 1));
                    Assert.That(address.ToWorld(regionSize, local),
                        Is.EqualTo(world));
                }
            }
        }

        [Test]
        public void IsNDimensional() {
            var address = new RegionAddress(new Vector(new[] { 1, 2, 3 }));
            var regionSize = new Vector(new[] { 10, 10, 10 });
            var local = new Vector(new[] { 4, 5, 6 });
            Assert.That(address.Dimensions, Is.EqualTo(3));
            Assert.That(address.ToWorld(regionSize, local),
                Is.EqualTo(new Vector(new[] { 14, 25, 36 })));
        }

        [Test]
        public void Equality_And_HashCode() {
            var a = new RegionAddress(new Vector(1, 2));
            var b = new RegionAddress(new Vector(1, 2));
            var c = new RegionAddress(new Vector(1, 3));
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a == b, Is.True);
            Assert.That(a != c, Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
            Assert.That(a.ToString(), Does.Contain("1x2"));
        }

        [Test]
        public void EmptyCoordinate_Throws() {
            Assert.That(() => new RegionAddress(Vector.Empty),
                Throws.ArgumentException);
        }

        [Test]
        public void DimensionMismatch_Throws() {
            var address = new RegionAddress(new Vector(1, 2));
            Assert.That(() => address.ToWorld(
                    new Vector(new[] { 1, 2, 3 }), new Vector(0, 0)),
                Throws.ArgumentException);
            Assert.That(() => RegionAddress.FromWorld(new Vector(1, 2),
                    new Vector(new[] { 1, 2, 3 })),
                Throws.ArgumentException);
        }
    }
}
