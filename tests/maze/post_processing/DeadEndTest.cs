using System;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps.Maze.PostProcessing {
    [TestFixture]
    public class DeadEndTest : Test {
        [Test]
        public void DeadEnd_CanFindDeadEnds() {
            var maze = MazeTestHelper.Parse("Area:{3x3;0x0;False;Maze;;[Cell:{;[0x1];},Cell:{;[2x0,1x1];},Cell:{;[1x0,2x1];},Cell:{;[0x0,1x1];},Cell:{;[1x0,0x1,1x2];},Cell:{;[2x0];},Cell:{;[1x2];},Cell:{;[1x1,0x2,2x2];},Cell:{;[1x2];}];}");
            var deadEnds = DeadEnd.Find(maze);
            Assert.That(deadEnds.DeadEnds, Is.Not.Empty);
            Assert.That(4, Is.EqualTo(deadEnds.DeadEnds.Count));
            Assert.That(deadEnds.DeadEnds.Contains(new Vector(0, 0)), Is.True, "0,0");
            Assert.That(deadEnds.DeadEnds.Contains(new Vector(2, 1)), Is.True, "2,1");
            Assert.That(deadEnds.DeadEnds.Contains(new Vector(0, 2)), Is.True, "0,2");
            Assert.That(deadEnds.DeadEnds.Contains(new Vector(2, 2)), Is.True, "2,2");
            Assert.That(maze.Count(
                cell => cell.X<DeadEnd.IsDeadEndExtension>() != null), Is.EqualTo(4));
        }
    }
}
