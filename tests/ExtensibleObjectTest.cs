using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps {

    [TestFixture]
    internal class ExtensibleObjectTest : Test {
        private class TestObject : ExtensibleObject { }
        private class TestExtension { public int Value { get; set; } }

        [Test]
        public void CtorTest() {
            var cell = new Cell(AreaType.Cave);
            Assert.That(cell.AreaType, Is.EqualTo(AreaType.Cave));
        }

        [Test]
        public void SetExtension() {
            var o = new TestObject();
            o.X(new TestExtension { Value = 1 });
            Assert.That(o.X<TestExtension>().Value, Is.EqualTo(1));
        }

        [Test]
        public void OverrideExtension() {
            var o = new TestObject();
            o.X(new TestExtension { Value = 1 });
            o.X(new TestExtension { Value = 2 });
            Assert.That(o.X<TestExtension>().Value, Is.EqualTo(2));
        }
    }
}
