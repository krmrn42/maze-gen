using System;
using NUnit.Framework;
using PlayersWorlds.Maps.Renderers;

namespace PlayersWorlds.Maps.MapFilters {

    [TestFixture]
    internal class Map2DEraseSpotsTest : Test {

        [Test]
        public void BackslashEraseSpots() {
            var log = TestLog.CreateForThisTest();
            var map = Map2DTest.Parse(Map2DTest.Backslash, Map2DTest.Tags);
            log.D(5, map.ToString());

            new Map2DEraseSpots(new[] {
                Cell.CellTag.MazeWall,
                Cell.CellTag.MazeWallCorner
            }, false, Cell.CellTag.MazeTrail, 3, 3)
                .Render(map);

            var expected =
                "▓▓░░░\n" +
                "░▓▓░░\n" +
                "░░▓▓░\n" +
                "░░░▓▓\n" +
                "░░░░▓\n";
            log.D(5, expected);
            log.D(5, map.ToString());
            var actual = map.Render(new AsciiRendererFactory());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void VariousSpotsEraseSpots() {
            var log = TestLog.CreateForThisTest();
            var emptyMap =
                "░░░░░\n" +
                "░░░░░\n" +
                "░░░░░\n" +
                "░░░░░\n" +
                "░░░░░\n";
            var spots = new string[] {
                Map2DTest.Spot2x2A, Map2DTest.Spot2x2B, Map2DTest.Spot2x2C,
                Map2DTest.Spot2x2D, Map2DTest.Spot2x2E, Map2DTest.Spot2x2F
            };
            foreach (var spot in spots) {
                var map = Map2DTest.Parse(spot, Map2DTest.Tags);
                log.D(5, map.ToString());

                new Map2DEraseSpots(new[] {
                    Cell.CellTag.MazeWall,
                    Cell.CellTag.MazeWallCorner
                }, false, Cell.CellTag.MazeTrail, 2, 2)
                    .Render(map);

                var expected = emptyMap;
                log.D(5, expected);
                log.D(5, map.ToString());
                var actual = map.Render(new AsciiRendererFactory());
                Assert.That(actual, Is.EqualTo(expected));
            }
        }

        [Test]
        public void Spot1x3SpotsEraseSpots() {
            var log = TestLog.CreateForThisTest();
            var map = Map2DTest.Parse(Map2DTest.Spot1x3, Map2DTest.Tags);
            log.D(5, map.ToString());

            new Map2DEraseSpots(new[] {
                Cell.CellTag.MazeWall,
                Cell.CellTag.MazeWallCorner
            }, false, Cell.CellTag.MazeTrail, 2, 2)
                .Render(map);

            var expected = Map2DTest.Spot1x3;
            log.D(5, expected);
            log.D(5, map.ToString());
            var actual = map.Render(new AsciiRendererFactory());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SmoothBoxVoidBgEraseSpots() {
            var log = TestLog.CreateForThisTest();
            var map = Map2DTest.Parse(Map2DTest.SmoothBoxVoidBg, Map2DTest.Tags);
            log.D(5, map.ToString());

            new Map2DEraseSpots(new[] {
                Cell.CellTag.MazeVoid
            }, true, Cell.CellTag.MazeWall, 3, 3)
                .Render(map);
            var expected =
                "▒▓▓▓▒\n" +
                "▓▓▓▓▓\n" +
                "▓▓▓▓▓\n" +
                "▓▓▓▓▓\n" +
                "▒▓▓▓▓\n";
            log.D(5, expected);
            log.D(5, map.ToString());
            var actual = map.Render(new AsciiRendererFactory());
            Assert.That(actual, Is.EqualTo(expected));
        }
        [Test]
        public void SmoothBoxEraseSpots() {
            var log = TestLog.CreateForThisTest();
            var map = Map2DTest.Parse(Map2DTest.SmoothBox, Map2DTest.Tags);
            log.D(5, map.ToString());

            new Map2DEraseSpots(new[] {
                Cell.CellTag.MazeTrail
            }, true, Cell.CellTag.MazeWall, 3, 3)
                .Render(map);
            var expected =
                "▒▓▓▓▒\n" +
                "▓▓▓▓▓\n" +
                "▓▓▓▓▓\n" +
                "▓▓▓▓▓\n" +
                "▒▓▓▓▓\n";
            log.D(5, expected);
            log.D(5, map.ToString());
            var actual = map.Render(new AsciiRendererFactory());
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
