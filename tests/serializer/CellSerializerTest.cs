using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Serializer {

    [TestFixture]
    internal class CellSerializerTest : Test {
        [Test]
        public void CanSerializeEmptyCell() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var cell = new Vector(1, 1);
            var actual = new CellSerializer(AreaType.None).Serialize(env[cell]);
            var expected = "Cell:{Environment;;}";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanDeserializeEmptyCell() {
            var serializer = new CellSerializer(AreaType.None);
            var cell = serializer.Deserialize("Cell:{Environment;;}");
            Assert.That(cell.AreaType, Is.EqualTo(AreaType.Environment));
            Assert.That(cell.HardLinks.Count, Is.EqualTo(0));
            Assert.That(cell.Tags.Count, Is.EqualTo(0));
        }

        [Test]
        public void CanSerializeCellWithLinks() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var cell = new Vector(1, 1);
            env[cell].HardLinks.Add(cell + Vector.North2D);
            env[cell + Vector.North2D].HardLinks.Add(cell);
            env[cell].HardLinks.Add(cell + Vector.East2D);
            env[cell + Vector.East2D].HardLinks.Add(cell);
            var actual = new CellSerializer(AreaType.None).Serialize(env[cell]);
            var expected = "Cell:{Environment;[1x2,2x1];}";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanDeserializeCellWithLinks() {
            var serializer = new CellSerializer(AreaType.None);
            var cell = serializer.Deserialize("Cell:{Environment;[1x2,2x1];}");
            var cellXY = new Vector(1, 1);
            Assert.That(cell.HardLinks.Count, Is.EqualTo(2));
            Assert.That(cell.HardLinks.Contains(cellXY + Vector.North2D), Is.True);
            Assert.That(cell.HardLinks.Contains(cellXY + Vector.East2D), Is.True);
            Assert.That(cell.Tags.Count, Is.EqualTo(0));
        }

        [Test]
        public void CanSerializeCellWithTags() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var cell = new Vector(1, 1);
            env[cell].Tags.Add(new Cell.CellTag("foo"));
            env[cell].Tags.Add(new Cell.CellTag("bar"));
            var actual = new CellSerializer(AreaType.None).Serialize(env[cell]);
            var expected = "Cell:{Environment;;[foo,bar]}";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanDeserializeCellWithTags() {
            var serializer = new CellSerializer(AreaType.None);
            var cell = serializer.Deserialize("Cell:{Environment;;[foo,bar]}");
            Assert.That(cell.HardLinks.Count, Is.EqualTo(0));
            Assert.That(cell.Tags.Count, Is.EqualTo(2));
            Assert.That(cell.Tags.Contains(new Cell.CellTag("foo")), Is.True);
            Assert.That(cell.Tags.Contains(new Cell.CellTag("bar")), Is.True);
        }

        [Test]
        public void CanSerializeCellWithLinksAndTags() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var cell = new Vector(1, 1);
            env[cell].HardLinks.Add(cell + Vector.North2D);
            env[cell].HardLinks.Add(cell + Vector.East2D);
            env[cell].Tags.Add(new Cell.CellTag("foo"));
            env[cell].Tags.Add(new Cell.CellTag("bar"));
            var actual = new CellSerializer(AreaType.None).Serialize(env[cell]);
            var expected = "Cell:{Environment;[1x2,2x1];[foo,bar]}";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanDeserializeCellWithLinksAndTags() {
            var serializer = new CellSerializer(AreaType.None);
            var cell = serializer.Deserialize("Cell:{Environment;[1x2,2x1];[foo,bar]}");
            var cellXY = new Vector(1, 1);
            Assert.That(cell.HardLinks.Count, Is.EqualTo(2));
            Assert.That(cell.HardLinks.Contains(cellXY + Vector.North2D), Is.True);
            Assert.That(cell.HardLinks.Contains(cellXY + Vector.East2D), Is.True);
            Assert.That(cell.Tags.Count, Is.EqualTo(2));
            Assert.That(cell.Tags.Contains(new Cell.CellTag("foo")), Is.True);
            Assert.That(cell.Tags.Contains(new Cell.CellTag("bar")), Is.True);
        }
    }
}
