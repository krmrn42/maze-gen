using System.Collections.Generic;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Serializer {
    [TestFixture]
    public class AreaSerializerTest {
        private AreaSerializer _serializer;

        [SetUp]
        public void SetUp() {
            _serializer = new AreaSerializer();
        }

        [Test]
        public void CanSerializeAndDeserializeAnEmptyArea() {
            var area = Area.Create(new Vector(0, 0), new Vector(1, 1), AreaType.Maze);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            Assert.That(deserializedArea.ChildAreas, Is.Empty);
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithCells() {
            var area = Area.Create(new Vector(0, 0), new Vector(2, 2), AreaType.Maze);
            var tag1 = new Cell.CellTag("tag1");
            var tag2 = new Cell.CellTag("tag2");
            area[new Vector(0, 0)].Tags.Add(tag1);
            area[new Vector(1, 1)].Tags.Add(tag2);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(2, 2)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            Assert.That(deserializedArea.ChildAreas, Is.Empty);
            Assert.That(deserializedArea[new Vector(0, 0)].Tags, Is.EquivalentTo(new[] { tag1 }));
            Assert.That(deserializedArea[new Vector(1, 1)].Tags, Is.EquivalentTo(new[] { tag2 }));
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithChildAreas() {
            var childArea1 = Area.Create(new Vector(0, 0), new Vector(1, 1), AreaType.Maze);
            var childArea2 = Area.Create(new Vector(1, 1), new Vector(1, 1), AreaType.Maze);
            var area = Area.Create(new Vector(0, 0), new Vector(2, 2), AreaType.Maze);
            area.AddChildArea(childArea1);
            area.AddChildArea(childArea2);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(2, 2)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            var childAreas = new List<Area>(deserializedArea.ChildAreas);
            Assert.That(childAreas.Count, Is.EqualTo(2));
            Assert.That(childAreas[0].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[0].Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(childAreas[0].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[0].Tags, Is.Empty);
            Assert.That(childAreas[1].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[1].Position, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[1].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[1].Tags, Is.Empty);
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithPosition() {
            var area = Area.Create(new Vector(1, 2), new Vector(2, 2), AreaType.Maze);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(2, 2)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(1, 2)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            Assert.That(deserializedArea.ChildAreas, Is.Empty);
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithTags() {
            var area = Area.Create(new Vector(0, 0), new Vector(1, 1), AreaType.Maze, "tag1", "tag2");
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.EquivalentTo(new[] { "tag1", "tag2" }));
            Assert.That(deserializedArea.ChildAreas, Is.Empty);
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithUnpositionedChildAreas() {
            var childArea1 = Area.CreateUnpositioned(new Vector(1, 1), AreaType.Maze);
            var childArea2 = Area.CreateUnpositioned(new Vector(1, 1), AreaType.Maze);
            var area = Area.Create(new Vector(0, 0), new Vector(2, 2), AreaType.Maze);
            area.AddChildArea(childArea1);
            area.AddChildArea(childArea2);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(2, 2)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            var childAreas = new List<Area>(deserializedArea.ChildAreas);
            Assert.That(childAreas.Count, Is.EqualTo(2));
            Assert.That(childAreas[0].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[0].IsPositionEmpty, Is.True);
            Assert.That(childAreas[0].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[0].Tags, Is.Empty);
            Assert.That(childAreas[1].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[1].IsPositionEmpty, Is.True);
            Assert.That(childAreas[1].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[1].Tags, Is.Empty);
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithPositionedChildAreas() {
            var childArea1 = Area.Create(new Vector(1, 1), new Vector(1, 1), AreaType.Maze);
            var childArea2 = Area.Create(new Vector(2, 2), new Vector(1, 1), AreaType.Maze);
            var area = Area.Create(new Vector(0, 0), new Vector(3, 3), AreaType.Maze);
            area.AddChildArea(childArea1);
            area.AddChildArea(childArea2);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(3, 3)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            var childAreas = new List<Area>(deserializedArea.ChildAreas);
            Assert.That(childAreas.Count, Is.EqualTo(2));
            Assert.That(childAreas[0].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[0].Position, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[0].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[0].Tags, Is.Empty);
            Assert.That(childAreas[1].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[1].Position, Is.EqualTo(new Vector(2, 2)));
            Assert.That(childAreas[1].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[1].Tags, Is.Empty);
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithMixedChildAreas() {
            var childArea1 = Area.Create(new Vector(1, 1), new Vector(1, 1), AreaType.Maze);
            var childArea2 = Area.CreateUnpositioned(new Vector(1, 1), AreaType.Maze);
            var area = Area.Create(new Vector(0, 0), new Vector(3, 3), AreaType.Maze);
            area.AddChildArea(childArea1);
            area.AddChildArea(childArea2);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(3, 3)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            var childAreas = new List<Area>(deserializedArea.ChildAreas);
            Assert.That(childAreas.Count, Is.EqualTo(2));
            Assert.That(childAreas[0].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[0].Position, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[0].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[0].Tags, Is.Empty);
            Assert.That(childAreas[1].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[1].IsPositionEmpty, Is.True);
            Assert.That(childAreas[1].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[1].Tags, Is.Empty);
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithLinkedCells() {
            var area = Area.CreateMaze(new Vector(5, 5));
            area[new Vector(0, 0)].HardLinks.Add(new Vector(0, 1));
            area[new Vector(0, 1)].HardLinks.Add(new Vector(0, 0));
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(
                deserializedArea[new Vector(0, 0)].HasLinks(new Vector(0, 1)));
        }

        // D5 trap (docs/COMPONENT-REVIEW.md): the store seam behind
        // IRegionStore MUST round-trip through AreaSerializer, which is
        // lossless, and MUST NOT use GeneratedWorld.Serialize() /
        // Area.ToString(), which return a short debug label that drops all
        // cells and links and does not deserialize back into an Area.
        [Test]
        public void AreaSerializerIsLossless_ToStringIsADebugLabelTrap() {
            var area = Area.CreateMaze(new Vector(5, 5));
            area[new Vector(0, 0)].HardLinks.Add(new Vector(0, 1));
            area[new Vector(0, 1)].HardLinks.Add(new Vector(0, 0));

            // Lossless: links survive a full AreaSerializer round trip -- this
            // is the path the store seam uses.
            var roundTripped =
                _serializer.Deserialize(_serializer.Serialize(area));
            Assert.That(
                roundTripped[new Vector(0, 0)].HasLinks(new Vector(0, 1)),
                Is.True);

            // The trap: Area.ToString() (exactly what GeneratedWorld.Serialize()
            // returns) is a debug label carrying no cell/link data...
            var debugLabel = area.ToString();
            Assert.That(debugLabel, Does.Not.Contain("Cell"));
            // ...and it is not even a valid serialized Area, so feeding it to
            // the store's deserializer fails loudly rather than silently
            // reconstructing an empty, link-less region.
            Assert.That(() => _serializer.Deserialize(debugLabel),
                Throws.Exception,
                "GeneratedWorld.Serialize()/Area.ToString() must not be fed " +
                "to the store seam; it is a lossy debug label, not a format.");
        }
    }
}
