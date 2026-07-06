using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.MapFilters;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Maze.PostProcessing;
using PlayersWorlds.Maps.Renderers;
using PlayersWorlds.Maps.Serializer;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps {
    // TODO: We have two Maze2DRendererTest.cs files.
    [TestFixture]
    public class Maze2DRendererTest : Test {
        private Area _maze;

        [SetUp]
        override public void SetUp() {
            base.SetUp();
            _maze = MazeTestHelper.Parse("Area:{4x4;0x0;False;Maze;;[Cell:{;[1x0,0x1];},Cell:{;[0x0,2x0,1x1];},Cell:{;[1x0,3x0];},Cell:{;[2x0,3x1];},Cell:{;[0x0,1x1,0x2];},Cell:{;[1x0,0x1];},Cell:{;;},Cell:{;[3x0];},Cell:{;[0x1,0x3];},Cell:{;;},Cell:{;[2x3,3x2];},Cell:{;[2x2];},Cell:{;[0x2,1x3];},Cell:{;[0x3,2x3];},Cell:{;[1x3,2x2];},Cell:{;;}];}");
        }

        [Test]
        public void CanRenderAMapWithSmoothCorners() {
            var log = TestLog.CreateForThisTest();
            var mazeRenderingOptions = new Maze2DRendererOptions(
                new Vector(2, 1), new Vector(2, 1));
            var map = CreateMapForMaze(_maze, mazeRenderingOptions);

            new Maze2DRenderer(_maze, mazeRenderingOptions)
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail }, Cell.CellTag.MazeWall, new Vector(1, 1)))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner, new Vector(1, 1)))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner }, Cell.CellTag.MazeWall, new Vector(1, 1)))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid }, true, Cell.CellTag.MazeWall, 4, 4))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall, Cell.CellTag.MazeWallCorner }, false, Cell.CellTag.MazeTrail, 3, 3))
                .Render(map);
            var expected =
                "0▓▓▓▓▓▓▓▓▓▓▓▓00000\n" +
                "0▓░░░░░░░░░░▓▓0000\n" +
                "0▓░░▒▓▓▓▓▒░░▒▓▓▓▓0\n" +
                "0▓░░▓▓▓▓▓▓░░░░░░▓0\n" +
                "0▓░░▒▓▓▓▓▓▓▓▓▓▓▓▓0\n" +
                "0▓░░░░░░▓▓▓▓▓▓░░▓0\n" +
                "0▓░░░░░░▒▓▓▓▓▒░░▓0\n" +
                "0▓░░░░░░░░░░░░░░▓0\n" +
                "0▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓0\n";
            var actual = map.Render(new AsciiRendererFactory());
            log.D(5, expected);
            log.D(5, actual);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanRenderAMapWithVoids() {
            var log = TestLog.CreateForThisTest();
            var mazeRenderingOptions = new Maze2DRendererOptions(
                new Vector(3, 2), new Vector(2, 1));
            var map = CreateMapForMaze(_maze, mazeRenderingOptions);

            new Maze2DRenderer(_maze, mazeRenderingOptions)
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail }, Cell.CellTag.MazeWall, new Vector(1, 1)))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner, new Vector(1, 1)))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner }, Cell.CellTag.MazeWall, new Vector(1, 1)))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid }, true, Cell.CellTag.MazeWall, 4, 4))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall, Cell.CellTag.MazeWallCorner }, false, Cell.CellTag.MazeTrail, 3, 3))
                .Render(map);
            var expected =
                "0▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓000000\n" +
                "0▓░░░░░░░░░░░░░▓000000\n" +
                "0▓░░░░░░░░░░░░░▓▓00000\n" +
                "0▓░░░▒▓▓▓▓▓▒░░░▒▓▓▓▓▓0\n" +
                "0▓░░░▓▓▓▓▓▓▓░░░░░░░░▓0\n" +
                "0▓░░░▓▓▓▓▓▓▓░░░░░░░░▓0\n" +
                "0▓░░░▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓0\n" +
                "0▓░░░░░░░░▓00000▓░░░▓0\n" +
                "0▓░░░░░░░░▓▓000▓▓░░░▓0\n" +
                "0▓░░░░░░░░▒▓▓▓▓▓▒░░░▓0\n" +
                "0▓░░░░░░░░░░░░░░░░░░▓0\n" +
                "0▓░░░░░░░░░░░░░░░░░░▓0\n" +
                "0▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓0\n";
            var actual = map.Render(new AsciiRendererFactory());
            log.D(5, _maze.Render(new AsciiRendererFactory()));
            log.D(5, expected);
            log.D(5, actual);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanRenderAMapWithFilledAreas() {
            var log = TestLog.CreateForThisTest();
            // The priority between hard links and filled areas is up to the
            // rendering engine. In this implementation, the filled areas do not
            // take priority over hard links (hard links are hard). So we need
            // to adjust the maze to make it render the way it is expected.
            _maze = MazeTestHelper.Parse("Area:{4x4;0x0;False;Maze;;[Cell:{;[1x0,0x1];},Cell:{;[0x0,2x0];},Cell:{;[1x0,3x0];},Cell:{;[2x0,3x1];},Cell:{;[0x0,0x2];},Cell:{;;},Cell:{;;},Cell:{;[3x0];},Cell:{;[0x1,0x3];},Cell:{;;},Cell:{;[2x3,3x2];},Cell:{;[2x2];},Cell:{;[0x2,1x3];},Cell:{;[0x3,2x3];},Cell:{;[1x3,2x2];},Cell:{;;}];}");
            _maze.AddChildArea(
                Area.Create(
                    new Vector(1, 1), new Vector(1, 1), AreaType.Fill));
            var builder = Maze2DBuilder.CreateFromOptions(_maze, new GeneratorOptions() {
                MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                RandomSource = RandomSource.CreateFromEnv()
            });
            builder.TestRebuildCellMaps();
            builder.ApplyAreas();
            var mazeRenderingOptions = new Maze2DRendererOptions(
                new Vector(2, 1), new Vector(2, 1));
            var map = CreateMapForMaze(_maze, mazeRenderingOptions);

            new Maze2DRenderer(_maze, mazeRenderingOptions)
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail }, Cell.CellTag.MazeWall, new Vector(1, 1)))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner, new Vector(1, 1)))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner }, Cell.CellTag.MazeWall, new Vector(1, 1)))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid }, true, Cell.CellTag.MazeWall, 5, 5))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall, Cell.CellTag.MazeWallCorner }, false, Cell.CellTag.MazeTrail, 3, 3))
                .Render(map);

            var expected =
                "0▓▓▓▓▓▓▓▓▓▓▓▓00000\n" +
                "0▓░░░░░░░░░░▓▓0000\n" +
                "0▓░░▒▓▓▓▓▒░░▒▓▓▓▓0\n" +
                "0▓░░▓▓00▓▓░░░░░░▓0\n" +
                "0▓░░▓0000▓▓▓▓▓▓▓▓0\n" +
                "0▓░░▓▓000000▓▓░░▓0\n" +
                "0▓░░▒▓▓▓▓▓▓▓▓▒░░▓0\n" +
                "0▓░░░░░░░░░░░░░░▓0\n" +
                "0▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓0\n";
            var actual = map.Render(new AsciiRendererFactory());
            log.D(5, _maze.Render(new AsciiRendererFactory()));
            log.D(5, expected);
            log.D(5, actual);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanRenderAMapWithHallAreas() {
            var log = TestLog.CreateForThisTest();
            _maze.AddChildArea(
                Area.Create(
                    new Vector(0, 0), new Vector(4, 2), AreaType.Hall));
            var builder = Maze2DBuilder.CreateFromOptions(_maze, new GeneratorOptions() {
                MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                RandomSource = RandomSource.CreateFromEnv()
            });
            builder.TestRebuildCellMaps();
            builder.ApplyAreas();
            var mazeRenderingOptions = new Maze2DRendererOptions(
                new Vector(2, 1), new Vector(2, 1));
            var map = CreateMapForMaze(_maze, mazeRenderingOptions);

            new Maze2DRenderer(_maze, mazeRenderingOptions)
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail }, Cell.CellTag.MazeWall, new Vector(1, 1)))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner, new Vector(1, 1)))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner }, Cell.CellTag.MazeWall, new Vector(1, 1)))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid }, true, Cell.CellTag.MazeWall, 5, 5))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall, Cell.CellTag.MazeWallCorner }, false, Cell.CellTag.MazeTrail, 3, 3))
                .Render(map);

            var expected =
                "0▓▓▓▓▓▓▓▓▓▓▓▓00000\n" +
                "0▓░░░░░░░░░░▓▓0000\n" +
                "0▓░░▒▓▓▓▓▒░░▒▓▓▓▓0\n" +
                "0▓░░▓▓▓▓▓▓░░░░░░▓0\n" +
                "0▓░░▒▓▓▓▓▓▓▓▓▓▓▓▓0\n" +
                "0▓░░░░░░░░░░░░░░▓0\n" +
                "0▓░░░░░░░░░░░░░░▓0\n" +
                "0▓░░░░░░░░░░░░░░▓0\n" +
                "0▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓0\n";
            log.D(5, _maze.Render(new AsciiRendererFactory()));
            log.D(5, expected);
            var actual = map.Render(new AsciiRendererFactory());
            log.D(5, actual);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Maze2DStringBoxRenderer_CanConvertToAscii() {
            var log = TestLog.CreateForThisTest();
            var expected =
                "┌───────────┐    \n" +
                "│           │    \n" +
                "│   ┌───┐   └───┐\n" +
                "│   │   │       │\n" +
                "│   └───┼───┬───┤\n" +
                "│       │   │   │\n" +
                "│   ┼   └───┘   │\n" +
                "│               │\n" +
                "└───────────────┘\n";
            var area = _maze.ShallowCopy();
            area.X(new Maze2DBuilder(RandomSource.CreateFromEnv(),
                                     area, null, null,
                                     GeneratorOptions.MazeFillFactor.Full));
            log.D(5, area.Render(new AsciiRendererFactory()));
            Assert.That(area.Render(new AsciiRendererFactory()), Is.EqualTo(expected));
        }

        [Test]
        public void Maze2DStringBoxRenderer_CanConvertToAsciiWithData() {
            var log = TestLog.CreateForThisTest();
            var expected =
                "┌───────────┐    \n" +
                "│ 4   3   2 │    \n" +
                "│   ┌───┐   └───┐\n" +
                "│ 5 │   │ 1   0 │\n" +
                "│   └───┼───┬───┤\n" +
                "│ 6     │   │ b │\n" +
                "│   ┼   └───┘   │\n" +
                "│ 7   8   9   a │\n" +
                "└───────────────┘\n";
            var area = _maze.ShallowCopy();
            area.X(new Maze2DBuilder(RandomSource.CreateFromEnv(),
                                     area, null, null,
                                     GeneratorOptions.MazeFillFactor.Full));
            area.X(DeadEnd.Find(_maze));
            area.X(DijkstraDistance.FindLongestTrail(_maze));
            log.D(5, area.Render(new AsciiRendererFactory()));
            Assert.That(area.Render(new AsciiRendererFactory()), Is.EqualTo(expected));
        }

        [Test]
        public void Maze2DToMap2DConverter_ThrowsIfInvalidOptions() {
            Assert.Throws<ArgumentException>(() => Maze2DRendererOptions.RectCells(new Vector(1, 2), new Vector(3, 0)));
            Assert.Throws<ArgumentException>(() => Maze2DRendererOptions.RectCells(new Vector(1, -2), new Vector(3, 4)));
            Assert.Throws<ArgumentException>(() => Maze2DRendererOptions.SquareCells(1, -2));
            Assert.Throws<ArgumentException>(() => Maze2DRendererOptions.SquareCells(0, 2));
            var mazeRenderingOptions = new Maze2DRendererOptions(
                new Vector(1, 1), new Vector(2, 2));
            Assert.DoesNotThrow(() => new Maze2DRendererOptions(
                new Vector(1, 1), new Vector(2, 2)));
            Assert.Throws<ArgumentException>(() => new Maze2DRendererOptions(
                new Vector(1, 1), new Vector(2, 0)));
        }

        [Test]
        public void CellsMapping_ValidMapping() {
            // The goal is to make sure the cellMapping *Cells properties return
            // valid groups of cells
            // how to validate? make sure x,y matches expected values for all
            // returned cells
            // Assert.XYIn(expected, actual)
            // Assert.XIn([1, 2], cellMapping.NWCells.Select(cell => CellPosition(map, cell)))
            // Assert.YIn([1, 2], cellMapping.NWCells.Select(cell => CellPosition(map, cell)))

            var mazeToMapOptions = new Maze2DRendererOptions(
                new Vector(1, 1), new Vector(2, 2));
            var map = CreateMapForMaze(_maze, mazeToMapOptions);
            var mazeToMap = new Maze2DRenderer(_maze, mazeToMapOptions);
            var cellMapping = new CellsMapping(map, _maze.Grid.First(), mazeToMapOptions);

            Assert.That(cellMapping.SWPosition, Is.EqualTo(new Vector(0, 0)), "SWPosition");
            Assert.That(cellMapping.SWSize, Is.EqualTo(new Vector(2, 2)), "SWSize");
            Assert.That(cellMapping.CenterPosition, Is.EqualTo(new Vector(2, 2)), "CenterPosition");
            Assert.That(cellMapping.CenterSize, Is.EqualTo(new Vector(1, 1)), "CenterSize");
            Assert.That(cellMapping.NEPosition, Is.EqualTo(new Vector(3, 3)), "NEPosition");
            Assert.That(cellMapping.NESize, Is.EqualTo(new Vector(2, 2)), "NESize");

            Assert.That(cellMapping.NWPosition, Is.EqualTo(new Vector(0, 3)), "NWPosition");
            Assert.That(cellMapping.NWSize, Is.EqualTo(new Vector(2, 2)), "NWSize");
            Assert.That(cellMapping.NPosition, Is.EqualTo(new Vector(2, 3)), "NPosition");
            Assert.That(cellMapping.NSize, Is.EqualTo(new Vector(1, 2)), "NSize");
            Assert.That(cellMapping.WPosition, Is.EqualTo(new Vector(0, 2)), "WPosition");
            Assert.That(cellMapping.WSize, Is.EqualTo(new Vector(2, 1)), "WSize");
            Assert.That(cellMapping.EPosition, Is.EqualTo(new Vector(3, 2)), "EPosition");
            Assert.That(cellMapping.ESize, Is.EqualTo(new Vector(2, 1)), "ESize");
            Assert.That(cellMapping.SPosition, Is.EqualTo(new Vector(2, 0)), "SPosition");
            Assert.That(cellMapping.SSize, Is.EqualTo(new Vector(1, 2)), "SSize");
            Assert.That(cellMapping.SEPosition, Is.EqualTo(new Vector(3, 0)), "SEPosition");
            Assert.That(cellMapping.SESize, Is.EqualTo(new Vector(2, 2)), "SESize");

            Assert.That(cellMapping.CenterCells.ToList(), Has.Count.EqualTo(1), "CenterCells Count");
            Assert.That(cellMapping.CenterCells.Select(cell => cell.X), Has.All.AnyOf(2), "CenterCells.X");
            Assert.That(cellMapping.CenterCells.Select(cell => cell.Y), Has.All.AnyOf(2), "CenterCells.Y");

            Assert.That(cellMapping.NWCells.ToList(), Has.Count.EqualTo(4), "NWCells Count");
            Assert.That(cellMapping.NWCells.Select(cell => cell.X), Has.All.AnyOf(0, 1), "NWCells.X");
            Assert.That(cellMapping.NWCells.Select(cell => cell.Y), Has.All.AnyOf(3, 4), "NWCells.Y");

            Assert.That(cellMapping.NCells.ToList(), Has.Count.EqualTo(2), "NCells Count");
            Assert.That(cellMapping.NCells.Select(cell => cell.X), Has.All.AnyOf(2), "NCells.X");
            Assert.That(cellMapping.NCells.Select(cell => cell.Y), Has.All.AnyOf(3, 4), "NCells.Y");

            Assert.That(cellMapping.NECells.ToList(), Has.Count.EqualTo(4), "NECells Count");
            Assert.That(cellMapping.NECells.Select(cell => cell.X), Has.All.AnyOf(3, 4), "NECells.X");
            Assert.That(cellMapping.NECells.Select(cell => cell.Y), Has.All.AnyOf(3, 4), "NECells.Y");

            Assert.That(cellMapping.WCells.ToList(), Has.Count.EqualTo(2), "WCells Count");
            Assert.That(cellMapping.WCells.Select(cell => cell.X), Has.All.AnyOf(0, 1), "WCells.X");
            Assert.That(cellMapping.WCells.Select(cell => cell.Y), Has.All.AnyOf(2), "WCells.Y");

            Assert.That(cellMapping.ECells.ToList(), Has.Count.EqualTo(2), "ECells Count");
            Assert.That(cellMapping.ECells.Select(cell => cell.X), Has.All.AnyOf(3, 4), "ECells.X");
            Assert.That(cellMapping.ECells.Select(cell => cell.Y), Has.All.AnyOf(2), "ECells.Y");

            Assert.That(cellMapping.SWCells.ToList(), Has.Count.EqualTo(4), "SWCells Count");
            Assert.That(cellMapping.SWCells.Select(cell => cell.X), Has.All.AnyOf(0, 1), "SWCells.X");
            Assert.That(cellMapping.SWCells.Select(cell => cell.Y), Has.All.AnyOf(0, 1), "SWCells.Y");

            Assert.That(cellMapping.SCells.ToList(), Has.Count.EqualTo(2), "SCells Count");
            Assert.That(cellMapping.SCells.Select(cell => cell.X), Has.All.AnyOf(2), "SCells.X");
            Assert.That(cellMapping.SCells.Select(cell => cell.Y), Has.All.AnyOf(0, 1), "SCells.Y");

            Assert.That(cellMapping.SECells.ToList(), Has.Count.EqualTo(4), "SECells Count");
            Assert.That(cellMapping.SECells.Select(cell => cell.X), Has.All.AnyOf(3, 4), "SECells.X");
            Assert.That(cellMapping.SECells.Select(cell => cell.Y), Has.All.AnyOf(0, 1), "SECells.Y");
        }
    }
}
