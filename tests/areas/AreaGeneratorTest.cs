using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas.Evolving;

namespace PlayersWorlds.Maps.Areas {

    [TestFixture]
    internal class AreaGeneratorTest : Test {
        private class TestAreaGenerator : AreaGenerator {
            public TestAreaGenerator(EvolvingSimulator simulator,
                                     MapAreaSystemFactory areaSystemFactory)
                                     : base(simulator, areaSystemFactory) {
            }

            protected override IEnumerable<Area> Generate(Area targetArea) {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void Ctor_DoesNotThrowIfParametersAreNull() {
            // evolving simulation is only necessary when area generator
            // generates unpositioned areas.
            Assert.DoesNotThrow(() => {
                var _ = new TestAreaGenerator(null, null);
            });
        }

        [Test]
        public void Ctor_ThrowsIfSimulatorIsNull() {
            var area = Area.CreateEnvironment(new Vector(10, 10));
            var fakeRandom = new FakeRandomSource();
            var areaSystemFactory = new Mock<MapAreaSystemFactory>(
                MockBehavior.Loose, fakeRandom);
            var generator = new Mock<AreaGenerator>(
                MockBehavior.Loose,
                null,
                areaSystemFactory.Object) {
                CallBase = true
            };
            generator.Protected()
                .Setup<IEnumerable<Area>>("Generate", area)
                .Returns(
                    new List<Area> {
                        Area.CreateUnpositioned(new Vector(1, 1), AreaType.Cave)
                    });
            Assert.Throws<ArgumentNullException>(
                () => generator.Object.GenerateMazeAreas(area));
        }

        [Test]
        public void Ctor_ThrowsIfMapAreaSystemFactoryIsNull() {
            var area = Area.CreateEnvironment(new Vector(10, 10));
            var evolvingSimulator = new Mock<EvolvingSimulator>(
                MockBehavior.Strict, 1, 1);
            var generator = new Mock<AreaGenerator>(
                MockBehavior.Loose,
                evolvingSimulator.Object,
                null) {
                CallBase = true
            };
            generator.Protected()
                .Setup<IEnumerable<Area>>("Generate", area)
                .Returns(
                    new List<Area> {
                        Area.CreateUnpositioned(new Vector(1, 1), AreaType.Cave)
                    });
            Assert.Throws<ArgumentNullException>(
                () => generator.Object.GenerateMazeAreas(area));
        }

        [Test]
        public void GenerateMazeAreas_CallsGenerate() {
            var area = Area.CreateEnvironment(new Vector(10, 10));
            var fakeRandom = new FakeRandomSource();
            var evolvingSimulator = new Mock<EvolvingSimulator>(
                MockBehavior.Strict, 1, 1);
            var areaSystemFactory = new Mock<MapAreaSystemFactory>(
                MockBehavior.Strict, fakeRandom);
            var generator = new Mock<AreaGenerator>(
                MockBehavior.Strict,
                evolvingSimulator.Object,
                areaSystemFactory.Object) {
                CallBase = true
            };
            generator.Protected()
                .SetupGet<int>("MaxAttempts").Returns(3);
            generator.Protected()
                .Setup<IEnumerable<Area>>("Generate", area)
                .Returns(Enumerable.Empty<Area>());
            generator.Object.GenerateMazeAreas(area);
            generator.VerifyAll();
        }

        [Test]
        public void GenerateMazeAreas_CallsEvolveForUnpositionedAreas() {
            var area = Area.CreateEnvironment(new Vector(10, 10));
            var fakeRandom = new FakeRandomSource();
            var evolvingSimulator = new Mock<EvolvingSimulator>(
                MockBehavior.Strict, 1, 1);
            var areaSystemFactory = new Mock<MapAreaSystemFactory>(
                MockBehavior.Strict, fakeRandom);
            var generator = new Mock<AreaGenerator>(
                MockBehavior.Strict,
                evolvingSimulator.Object,
                areaSystemFactory.Object) {
                CallBase = true
            };
            generator.Protected().SetupGet<int>("MaxAttempts").Returns(3);
            generator.Protected()
                .Setup<IEnumerable<Area>>("Generate", area)
                .Returns(
                    new List<Area> {
                        Area.CreateUnpositioned(new Vector(1, 1), AreaType.Cave)
                    });

            // when AreaGenerator creates unpositioned areas, it needs to
            // position them using the Evolving System.
            evolvingSimulator
                .Setup(x => x.Evolve(It.IsAny<MapAreasSystem>()))
                .Returns(0);
            generator.Object.GenerateMazeAreas(area);
            generator.VerifyAll();
        }

        [Test]
        public void GenerateMazeAreas_ExistingAreas() {
            var area = Area.CreateEnvironment(new Vector(10, 10));
            area.AddChildArea(Area.Create(new Vector(1, 1), new Vector(2, 2), AreaType.Cave));

            var fakeRandom = new FakeRandomSource();
            var evolvingSimulator = new Mock<EvolvingSimulator>(
                MockBehavior.Strict, 1, 1);
            var areaSystemFactory = new Mock<MapAreaSystemFactory>(
                MockBehavior.Strict, fakeRandom);
            var generator = new Mock<AreaGenerator>(
                MockBehavior.Strict,
                evolvingSimulator.Object,
                areaSystemFactory.Object) {
                CallBase = true
            };
            generator.Protected().SetupGet<int>("MaxAttempts").Returns(3);
            generator.Protected()
                .Setup<IEnumerable<Area>>("Generate", area)
                .Returns(
                    new List<Area> {
                        Area.CreateUnpositioned(new Vector(1, 1), AreaType.Cave)
                    });

            evolvingSimulator
                .Setup(x => x.Evolve(It.IsAny<MapAreasSystem>()))
                .Returns(0);
            generator.Object.GenerateMazeAreas(area);
            generator.VerifyAll();

            Assert.That(area.ChildAreas.Count, Is.EqualTo(2));
        }

        [Test]
        public void GenerateMazeAreas_Attempts() {
            var area = Area.CreateEnvironment(new Vector(10, 10));
            var attempts = 5;

            var fakeRandom = new FakeRandomSource();
            var evolvingSimulator = new Mock<EvolvingSimulator>(
                MockBehavior.Strict, 1, 1);
            var areaSystemFactory = new Mock<MapAreaSystemFactory>(
                MockBehavior.Strict, fakeRandom);
            var generator = new Mock<AreaGenerator>(
                MockBehavior.Strict,
                evolvingSimulator.Object,
                areaSystemFactory.Object) {
                CallBase = true
            };
            generator.Protected().SetupGet<int>("MaxAttempts").Returns(attempts);
            generator.Protected()
                .Setup<IEnumerable<Area>>("Generate", area)
                .Returns(
                    new List<Area> {
                        Area.CreateUnpositioned(new Vector(-1, -1), new Vector(1, 1), AreaType.Cave)
                    });

            // when AreaGenerator creates unpositioned areas, it needs to
            // position them using the Evolving System.
            evolvingSimulator
                .Setup(x => x.Evolve(It.IsAny<MapAreasSystem>()))
                .Returns(0);
            Assert.That(() => generator.Object.GenerateMazeAreas(area),
                        Throws.InstanceOf<AreaGeneratorException>());

            // ensure we made three attempts to generage and position the areas.
            generator.Protected().Verify("Generate", Times.Exactly(attempts), area);
            evolvingSimulator.Verify(s => s.Evolve(It.IsAny<MapAreasSystem>()), Times.Exactly(attempts));
        }
    }
}
