using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze.PostProcessing;
using static PlayersWorlds.Maps.Maze.GeneratorOptions;

namespace PlayersWorlds.Maps.Maze {

    [TestFixture]
    internal class MazeGeneratorAreasTest : Test {
        public Area GenerateMaze(Type generatorType, List<Area> areas) =>
            MazeTestHelper.GenerateMaze(new Vector(15, 15), areas,
                new GeneratorOptions() {
                    FillFactor = MazeFillFactor.Full,
                    MazeAlgorithm = generatorType,
                    AreaGeneration = AreaGenerationMode.Manual,
                });

        [Test]
        [Repeat(10), Category("Integration")]
        public void FillAreasAreAppliedProperly(
            [ValueSource("GetAllGenerators")] Type generatorType) {
            var fill = Area.Create(new Vector(2, 4), new Vector(3, 4), AreaType.Fill);

            var maze = GenerateMaze(generatorType, new List<Area>() { fill });

            Assert.That(maze.ChildAreas, Has.Exactly(1).Items);
            var fillCells = new List<Vector>(maze.ChildAreas.First().Grid);
            Assert.That(fillCells, Has.Exactly(12).Items);

            var otherCells = maze.Grid
                .Except(fillCells)
                .ToList();

            Assert.That(otherCells, Has.Exactly(213).Items);

            // fill areas do not have links with the external cells
            Assert.That(fillCells.SelectMany(cell => maze[cell].Links().Select(link => maze[link])),
                Has.None.AnyOf(otherCells));
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void HallAreasAreAppliedProperly(
            [ValueSource("GetAllGenerators")] Type generatorType) {
            var hall = Area.Create(new Vector(8, 2), new Vector(4, 5), AreaType.Hall);
            var maze = GenerateMaze(generatorType, new List<Area>() { hall });

            var areaArea = 20;
            var areaPerimeter = 14;
            var mazeArea = 225;

            var hallCells = maze.Grid
                .SafeRegion(hall.Position, hall.Size).ToList();
            Assert.That(hallCells, Has.Exactly(areaArea).Items);

            var otherCells = maze.Grid
                .Except(hallCells)
                .ToList();

            Assert.That(otherCells, Has.Exactly(mazeArea - areaArea).Items);
            // hall areas have only one link with the external cells
            Assert.That(hallCells.SelectMany(cell => maze[cell].Links()),
                Has.Exactly(1).AnyOf(otherCells));

            var hallInnerCells = maze.Grid.SafeRegion(
                hall.Position + Vector.NorthEast2D,
                hall.Size + Vector.SouthWest2D + Vector.SouthWest2D)
                .ToList();
            Assert.That(hallInnerCells.Count(),
                Is.EqualTo(areaArea - areaPerimeter));
            // hall areas don't have walls inside the areas
            Assert.That(hallInnerCells.All(cell => maze[cell].Links().Count == 4));

            var hallEdgeCells = new HashSet<Vector>(hallCells);
            hallEdgeCells.ExceptWith(hallInnerCells);
            Assert.That(hallEdgeCells.Count(), Is.EqualTo(areaPerimeter));

            // only corners can have at least two links
            Assert.That(hallEdgeCells.Count(cell => maze[cell].Links().Count == 2),
                Is.LessThanOrEqualTo(4));
            // all other edge cells have 3 or more links
            Assert.That(hallEdgeCells.Count(cell => maze[cell].Links().Count >= 3),
                Is.GreaterThanOrEqualTo(areaPerimeter - 4));

            // exactly one of hall edge cells is connected to the external cells
            Assert.That(
                hallEdgeCells.SelectMany(cell => maze[cell].Links()),
                Has.Exactly(1).AnyOf(otherCells));
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void CaveAreasAreAppliedProperly(
            [ValueSource("GetAllGenerators")] Type generatorType) {
            var cave = Area.Create(new Vector(5, 10), new Vector(7, 3), AreaType.Cave);
            var maze = GenerateMaze(generatorType, new List<Area>() { cave });

            var areaArea = 21;
            var areaPerimeter = 16;
            var mazeArea = 225;

            Assert.That(maze.ChildAreas, Has.Exactly(1).Items);
            var caveCells = new List<Vector>(maze.ChildAreas.First().Grid);
            Assert.That(caveCells, Has.Exactly(21).Items);

            var otherCells = maze.Grid
                .Except(caveCells)
                .ToList();

            Assert.That(otherCells, Has.Exactly(mazeArea - areaArea).Items);
            // cave areas have at least one link with the external cells
            Assert.That(caveCells.SelectMany(cell => maze[cell].Links())
                                 .Intersect(otherCells).ToList(),
                        Has.Count.GreaterThanOrEqualTo(1));

            var caveInnerCells = maze.Grid.SafeRegion(
                cave.Position + Vector.NorthEast2D,
                cave.Size + Vector.SouthWest2D + Vector.SouthWest2D)
                .ToList();
            Assert.That(caveInnerCells.Count(),
                Is.EqualTo(areaArea - areaPerimeter));
            // cave areas don't have walls inside the areas
            Assert.That(caveInnerCells.All(cell => maze[cell].Links().Count == 4));

            var caveEdgeCells = new HashSet<Vector>(caveCells);
            caveEdgeCells.ExceptWith(caveInnerCells);
            Assert.That(caveEdgeCells.Count(), Is.EqualTo(areaPerimeter));
            // only corners can have at least two links
            Assert.That(caveEdgeCells.Count(cell => maze[cell].Links().Count == 2),
                Is.LessThanOrEqualTo(4));
            // all other edge cells have 3 or more links
            Assert.That(caveEdgeCells.Count(cell => maze[cell].Links().Count >= 3),
                Is.GreaterThanOrEqualTo(areaPerimeter - 4));
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void OverlappingAreasAreAppliedProperly(
            [ValueSource("GetAllGenerators")] Type generatorType,
            [ValueSource("GetAllAreaTypes")] AreaType areaType) {
            var area1 = Area.Create(new Vector(2, 3), new Vector(4, 7), areaType);
            var area2 = Area.Create(new Vector(4, 8), new Vector(7, 3), areaType);
            var maze = GenerateMaze(generatorType, new List<Area>() { area1, area2 });

            area1 = maze.ChildAreas.First();
            area2 = maze.ChildAreas.Last();

            var areaArea = 45;
            var mazeArea = 225;
            var areaCells = area1.Grid.Concat(area2.Grid).Distinct();

            Assert.That(area1.Grid, Has.Exactly(28).Items);
            Assert.That(area2.Grid, Has.Exactly(21).Items);
            Assert.That(areaCells, Has.Exactly(areaArea).Items);

            var otherCells = maze.Grid
                .Except(area1.Grid)
                .Except(area2.Grid)
                .ToList();

            Assert.That(otherCells, Has.Exactly(mazeArea - areaArea).Items);

            if (areaType == AreaType.Fill) {
                // fill areas do not have links with the external cells
                Assert.That(areaCells.SelectMany(cell => maze[cell].Links().Select(link => maze[link])),
                    Has.None.AnyOf(otherCells));
            } else {
                var innerCells = maze.Grid.SafeRegion(
                    area1.Position + Vector.NorthEast2D,
                    area1.Size + Vector.SouthWest2D + Vector.SouthWest2D)
                    .Concat(
                        maze.Grid.SafeRegion(
                            area2.Position + Vector.NorthEast2D,
                            area2.Size + Vector.SouthWest2D + Vector.SouthWest2D)
                    ).Distinct().ToList();
                // cave areas don't have walls inside the areas
                Assert.That(innerCells.All(cell => maze[cell].Links().Count == 4), string.Join(",", innerCells.Where(cell => maze[cell].Links().Count < 4)));

                var edgeCells = new HashSet<Vector>(areaCells);
                edgeCells.ExceptWith(innerCells);
                // only corners can have at least two links
                Assert.That(edgeCells.Count(cell => maze[cell].Links().Count == 2),
                    Is.LessThanOrEqualTo(6));
                // all other edge cells have 3 or more links
                Assert.That(edgeCells.Count(cell => maze[cell].Links().Count >= 3),
                    Is.GreaterThanOrEqualTo(edgeCells.Count() - 6));

            }
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void TwoMatchingAreas(
            [ValueSource("GetAllGenerators")] Type generatorType,
            [ValueSource("GetAllAreaTypes")] AreaType areaType) {
            var area1 = Area.Create(new Vector(2, 2), new Vector(2, 2), areaType);
            var area2 = Area.Create(new Vector(2, 2), new Vector(2, 2), areaType);
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(6, 6), new List<Area>() { area1, area2 },
                new GeneratorOptions() {
                    MazeAlgorithm = generatorType,
                    FillFactor = MazeFillFactor.Full,
                },
                out var builder);
            Assert.That(MazeTestHelper.IsSolveable(maze));

            var areaArea = 4;
            var mazeArea = 36;
            var expectConnected =
                areaType == AreaType.Cave ? mazeArea : mazeArea - areaArea;
            if (areaType == AreaType.Hall) {
                expectConnected += 1; // entrance cell within the call.
            }

            Assert.That(maze.ChildAreas.First().Grid, Has.Exactly(areaArea).Items);
            Assert.That(maze.ChildAreas.Last().Grid, Has.Exactly(areaArea).Items);
            Assert.That(builder.TestCellsToConnect, Is.Empty);
            Assert.That(builder.TestConnectedCells,
                Has.Exactly(expectConnected).Items);
        }

        [Test, Ignore("Debug only")]
        public void DenseWalkwaysDebug() {
            DenseWalkways(typeof(BinaryTreeMazeGenerator), AreaType.Fill);
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void DenseWalkways(
            [ValueSource("GetAllGenerators")] Type generatorType,
            [ValueSource("GetAllAreaTypes")] AreaType areaType) {
            var areas = new List<Area>() {
                Area.Create(new Vector(1, 1), new Vector(2, 2), areaType),
                Area.Create(new Vector(4, 1), new Vector(2, 3), areaType),
                Area.Create(new Vector(7, 1), new Vector(2, 2), areaType),
                Area.Create(new Vector(1, 4), new Vector(2, 3), areaType),
                Area.Create(new Vector(4, 5), new Vector(1, 2), areaType),
                Area.Create(new Vector(6, 5), new Vector(1, 2), areaType),
                Area.Create(new Vector(7, 4), new Vector(2, 1), areaType),
                Area.Create(new Vector(8, 6), new Vector(1, 1), areaType),
            };
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(10, 8), areas, new GeneratorOptions() {
                    MazeAlgorithm = generatorType,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full,
                },
                out var builder);

            var areaArea = areas.Sum(area => area.Size.Area);
            var mazeArea = maze.Size.Area;
            var expectConnected =
                areaType == AreaType.Cave ? mazeArea : mazeArea - areaArea;
            if (areaType == AreaType.Hall) {
                expectConnected += areas.Count; // entrance cell within the call.
            }

            Assert.That(builder.TestCellsToConnect, Is.Empty, builder.Random.ToString());
            Assert.That(builder.TestConnectedCells,
                Has.Exactly(expectConnected).Items, builder.Random.ToString());
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void WilsonsHalls() {
            GenerateMaze(GeneratorOptions.Algorithms.Wilsons,
                new List<Area>() {
                    Area.Create(
                        new Vector(2, 3), new Vector(4, 7), AreaType.Hall) });

            // The algorithm hung up b/c wilson's creates it's own walk paths
            // without marking the cells as visited. And if it's choosing only
            // priority cells while doing that, it will walk along one edge of
            // an area forever.
            Assert.Pass();
        }

        public void ScatteredAreasDebug() {
            ScatteredAreas(GeneratorOptions.Algorithms.Wilsons, AreaType.Cave, MazeFillFactor.Quarter);
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void ScatteredAreas(
            [ValueSource("GetAllGenerators")] Type generatorType,
            [ValueSource("GetAllAreaTypes")] AreaType areaType,
            [ValueSource("GetAllFillFactors")] MazeFillFactor fillFactor) {
            if (!MazeTestHelper.IsSupported(generatorType, fillFactor)) {
                Assert.Ignore();
            }
            if (areaType == AreaType.Fill) {
                Assert.Ignore(); // we can't test fill areas here.
            }
            if (generatorType == typeof(AldousBroderMazeGenerator) ||
                generatorType == typeof(HuntAndKillMazeGenerator) ||
                generatorType == typeof(RecursiveBacktrackerMazeGenerator)) {
                if (fillFactor == MazeFillFactor.Quarter ||
                    fillFactor == MazeFillFactor.Half ||
                    fillFactor == MazeFillFactor.ThreeQuarters ||
                    // TODO(#32): Re-enable low fill factors with probabalistic 
                    //            testing
                    fillFactor == MazeFillFactor.NinetyPercent ||
                    fillFactor == MazeFillFactor.FullWidth ||
                    fillFactor == MazeFillFactor.FullHeight) {
                    // full layouts are uninteresting for this test
                    Assert.Ignore();
                    return;
                }
            }
            var area1 = Area.Create(new Vector(2, 2), new Vector(2, 2), areaType);
            var area2 = Area.Create(new Vector(24, 24), new Vector(2, 2), areaType);
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(30, 30), new List<Area>() { area1, area2 },
                new GeneratorOptions() {
                    MazeAlgorithm = generatorType,
                },
                out _);

            var paths = DijkstraDistance.Find(maze, area1.Position);

            Assert.That(paths.ContainsKey(area2.Position));
        }

        [Test]
        [Repeat(100), Category("Integration")]
        public void ManualAndAutoAreasGeneration() {
            var options = new GeneratorOptions() {
                MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                AreaGeneration = GeneratorOptions.AreaGenerationMode.Auto,
                RandomSource = RandomSource.CreateFromEnv()
            };
            var maze = MazeTestHelper.GenerateMaze(new Vector(20, 20),
                new List<Area>() {
                    Area.Create(new Vector(2, 3),
                                new Vector(4, 7),
                                AreaType.Hall,
                                "fixed"),
                    Area.CreateUnpositioned(new Vector(2, 5), AreaType.Hall, "auto")
                }, options, out _);
            Assert.That(maze.ChildAreas.Count, Is.GreaterThan(2), $"No areas autogenerated with seed {options.RandomSource.Seed}");
        }

        public static IEnumerable<Type> GetAllGenerators() =>
            MazeTestHelper.GetAllGenerators();

        public static IEnumerable<AreaType> GetAllAreaTypes() =>
            MazeTestHelper.GetAllAreaTypes();

        public static IEnumerable<MazeFillFactor> GetAllFillFactors() =>
            MazeTestHelper.GetAllFillFactors();
    }
}
