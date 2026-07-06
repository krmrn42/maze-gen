using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PlayersWorlds.Maps;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Areas {
    [TestFixture]
    public class BasicAreaGeneratorTest : Test {

        [Test]
        public void MinSizeTooLargeDoesNotProduceAreas() {
            var env = Area.CreateEnvironment(new Vector(10, 10));
            var gen = new BasicAreaGenerator(
                new FakeRandomSource(),
                env,
                new AreaType[] { AreaType.Cave },
                new string[] { "tag1" },
                1,
                new Vector(20, 20),
                new Vector(20, 20),
                new List<Area>());
            gen.GenerateMazeAreas(env);

            Assert.That(env.ChildAreas, Is.Empty);
        }

        [Test]
        public void TwoAreas() {
            var env = Area.CreateEnvironment(new Vector(10, 10));
            var gen = new BasicAreaGenerator(
                new FakeRandomSource(),
                env,
                new AreaType[] { AreaType.Cave },
                new string[] { "tag1", "tag2" },
                2,
                new Vector(2, 3),
                new Vector(5, 5),
                new List<Area>());

            gen.GenerateMazeAreas(env);

            Assert.That(env.ChildAreas, Is.Not.Empty);
        }

        [Test]
        public void MaxAreas() {
            var env = Area.CreateEnvironment(new Vector(10, 10));
            var gen = new BasicAreaGenerator(
                new FakeRandomSource(),
                env,
                new AreaType[] { AreaType.Cave },
                new string[] { "tag1", "tag2" },
                100,
                new Vector(2, 3),
                new Vector(5, 5),
                new List<Area>());

            gen.GenerateMazeAreas(env);

            Assert.That(env.ChildAreas, Has.Count.GreaterThan(2));
            Assert.That(IsAValidLayout(env));
        }

        [Test]
        public void AreasNoOverlap() {
            var env = Area.CreateEnvironment(new Vector(10, 10));
            var noOverlapArea = Area.Create(new Vector(1, 1),
                                            new Vector(4, 4),
                                            AreaType.Hall,
                                            "tag1");
            var gen = new BasicAreaGenerator(
                new FakeRandomSource(),
                env,
                new AreaType[] { AreaType.Cave },
                new string[] { "tag1", "tag2" },
                100,
                new Vector(2, 3),
                new Vector(5, 5),
                new List<Area>() { noOverlapArea });

            gen.GenerateMazeAreas(env);

            Assert.That(env.ChildAreas, Has.Count.GreaterThan(2));
            Assert.That(env.ChildAreas.All(
                a => !a.Grid.Overlaps(noOverlapArea.Grid)));
            Assert.That(IsAValidLayout(env));
        }

        private bool IsAValidLayout(Area area) =>
            !area.ChildAreas.Any(
                a => area.ChildAreas.Any(
                    b => a != b && a.Grid.Overlaps(b.Grid)))
            && area.ChildAreas.All(a => a.Grid.FitsInto(area.Grid));
    }
}
