using NUnit.Framework;
using PlayersWorlds.Maps.Renderers;

namespace PlayersWorlds.Maps.MapFilters {

    [TestFixture]
    internal class Map2DClassifyWallsTest : Test {

        private static string Classify(string input) {
            var map = Map2DTest.Parse(input, Map2DTest.Tags);
            new Map2DClassifyWalls().Render(map);
            return map.Render(new AsciiRendererFactory());
        }

        [Test]
        public void HorizontalStraightGetsAxisX() {
            var input =
                "░░░░░\n" +
                "░░░░░\n" +
                "▓▓▓▓▓\n" +
                "░░░░░\n" +
                "░░░░░\n";
            var expected =
                "░░░░░\n" +
                "░░░░░\n" +
                "═════\n" +
                "░░░░░\n" +
                "░░░░░\n";
            Assert.That(Classify(input), Is.EqualTo(expected));
        }

        [Test]
        public void VerticalStraightGetsAxisY() {
            var input =
                "░░▓░░\n" +
                "░░▓░░\n" +
                "░░▓░░\n" +
                "░░▓░░\n" +
                "░░▓░░\n";
            var expected =
                "░░║░░\n" +
                "░░║░░\n" +
                "░░║░░\n" +
                "░░║░░\n" +
                "░░║░░\n";
            Assert.That(Classify(input), Is.EqualTo(expected));
        }

        [Test]
        public void TJunctionFlatWallKeepsThroughAxis_BranchGetsPerpendicularAxis() {
            var input =
                "░░░░░\n" +
                "░░░░░\n" +
                "▓▓▓▓▓\n" +
                "░░▓░░\n" +
                "░░▓░░\n";
            var expected =
                "░░░░░\n" +
                "░░░░░\n" +
                "═════\n" +
                "░░║░░\n" +
                "░░║░░\n";
            Assert.That(Classify(input), Is.EqualTo(expected));
        }

        [Test]
        public void TrueCornerGetsCornerTag() {
            var input =
                "░░░░░\n" +
                "░░░░░\n" +
                "░▓▓▓░\n" +
                "░▓░░░\n" +
                "░▓░░░\n";
            var expected =
                "░░░░░\n" +
                "░░░░░\n" +
                "░▒══░\n" +
                "░║░░░\n" +
                "░║░░░\n";
            Assert.That(Classify(input), Is.EqualTo(expected));
        }
    }
}
