using System;
using NUnit.Framework;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps.World {
    [TestFixture]
    public class RegionAlgorithmTest {
        // A valid custom generator (concrete, parameterless ctor).
        private sealed class CustomGen : MazeGenerator {
            public override void GenerateMaze(Maze2DBuilder builder) { }
        }

        private abstract class AbstractGen : MazeGenerator { }

        [Test]
        public void BuiltIns_AreDistinct() {
            Assert.That(RegionAlgorithm.RecursiveBacktracker,
                Is.Not.EqualTo(RegionAlgorithm.Sidewinder));
            Assert.That(RegionAlgorithm.RecursiveBacktracker,
                Is.EqualTo(RegionAlgorithm.RecursiveBacktracker));
            Assert.That(RegionAlgorithm.HuntAndKill.GetHashCode(),
                Is.EqualTo(RegionAlgorithm.HuntAndKill.GetHashCode()));
        }

        [Test]
        public void Custom_AcceptsAConcreteGenerator() {
            var algo = RegionAlgorithm.Custom<CustomGen>();
            Assert.That(algo.ToString(), Is.EqualTo(nameof(CustomGen)));
        }

        [Test]
        public void Custom_RejectsNonGenerators_AndAbstractOrNull() {
            Assert.That(() => RegionAlgorithm.Custom(typeof(string)),
                Throws.ArgumentException);
            Assert.That(() => RegionAlgorithm.Custom(typeof(AbstractGen)),
                Throws.ArgumentException);
            Assert.That(() => RegionAlgorithm.Custom(null),
                Throws.InstanceOf<ArgumentNullException>());
        }
    }
}
