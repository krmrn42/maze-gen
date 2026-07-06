using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas.Evolving;

namespace PlayersWorlds.Maps.Areas {
    namespace PlayersWorlds.Maps.Areas {

        [TestFixture]
        internal class AreaGeneratorExceptionTest : Test {
            private Mock<AreaGenerator> CreateGeneratorMock() {
                var generatorMock = new Mock<AreaGenerator>(
                    new Mock<EvolvingSimulator>(MockBehavior.Loose, 1, 1).Object,
                    new Mock<MapAreaSystemFactory>(
                        MockBehavior.Loose, new FakeRandomSource()).Object);
                return generatorMock;
            }
            [Test]
            public void WithMessage() {
                var ex = new AreaGeneratorException(CreateGeneratorMock().Object, "message");
                Assert.That(ex.Generator, Is.Not.Null);
            }
            [Test]
            public void WithMessageAndInnerException() {
                var ex = new AreaGeneratorException(CreateGeneratorMock().Object, new Exception("message"));
                Assert.That(ex.Generator, Is.Not.Null);
            }
            [Test]
            public void MessageIncludesGeneratorInfo() {
                var areaGeneratorMock = CreateGeneratorMock();
                areaGeneratorMock.Setup(x => x.ToString()).Returns("generator");
                var ex = new AreaGeneratorException(areaGeneratorMock.Object, "message");
                Assert.That(ex.Message, Is.EqualTo("message\ngenerator"));
                areaGeneratorMock.Verify(x => x.ToString(), Times.Once);
            }
        }
    }
}
