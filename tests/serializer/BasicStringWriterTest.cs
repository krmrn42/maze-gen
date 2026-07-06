using NUnit.Framework;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps.Serializer {

    [TestFixture]
    internal class BasicStringWriterTest : Test {
        [Test]
        public void CanWriteEmptyObject() {
            var actual = new BasicStringWriter()
                .WriteObjectStart(this.GetType())
                .WriteObjectEnd();
            var expected = "BasicStringWriterTest:{}";
            Assert.That(actual, Is.EqualTo(expected));
        }
        [Test]
        public void CanWriteSimpleObject() {
            var actual = new BasicStringWriter()
                .WriteObjectStart(this.GetType())
                .WriteValue("foo")
                .WriteValue("bar")
                .WriteEnumerable(new string[] { "baz", "fuz" })
                .WriteObjectEnd();
            var expected = "BasicStringWriterTest:{foo;bar;[baz,fuz]}";
            Assert.That(actual, Is.EqualTo(expected));
        }
        [Test]
        public void CanWriteOnlyEnumerable() {
            var actual = new BasicStringWriter()
                .WriteObjectStart(this.GetType())
                .WriteEnumerable(new string[] { "baz", "fuz" })
                .WriteObjectEnd();
            var expected = "BasicStringWriterTest:{[baz,fuz]}";
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
