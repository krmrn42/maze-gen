using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Maze {

    [TestFixture]
    public class Maze2DBuilderTest : Test {
        [Test]
        public void BuildsCorrectCellsCollections() {
            var maze = Area.CreateMaze(new Vector(5, 5));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(builder.TestCellsToConnect, Has.Exactly(25).Items);
            Assert.That(builder.TestPriorityCells, Has.Exactly(0).Items);
        }

        [Test]
        public void BuildsCorrectCellsCollectionsWithHallAreas() {
            var maze = Area.CreateMaze(new Vector(6, 6));
            maze.AddChildArea(
                Area.Create(new Vector(2, 3),
                            new Vector(3, 2),
                            AreaType.Hall,
                            "hall"));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            var priorityCells = new List<Vector>() {
                new Vector(2, 2),
                new Vector(3, 2),
                new Vector(4, 2),
                new Vector(1, 3),
                new Vector(5, 3),
                new Vector(1, 4),
                new Vector(5, 4),
                new Vector(2, 5),
                new Vector(3, 5),
                new Vector(4, 5),
            };

            Assert.That(builder.TestCellsToConnect, Has.Exactly(30).Items);
            Assert.That(builder.TestPriorityCells, Has.Exactly(10).Items);
            Assert.That(new List<Vector>(builder.TestPriorityCells.Keys),
                Is.EqualTo(priorityCells));
            Assert.That(builder.TestPriorityCells.First().Value,
                Is.EqualTo(priorityCells));

            foreach (var areaInfo in maze.ChildAreas) {
                if (areaInfo.Type == AreaType.Hall || areaInfo.Type == AreaType.Fill) {
                    var selectableCells = builder.TestCellsToConnect.Intersect(areaInfo.Grid).ToList();
                    if (selectableCells.Count > 0) {
                        Assert.Fail("Hall cells are in the cellsToConnect collection or have links.");
                    }
                }
            }
        }

        [Test]
        public void BuildsCorrectCellsCollectionsWithHallAreasAtTheEdge() {
            var maze = Area.CreateMaze(new Vector(6, 6));
            maze.AddChildArea(Area.Create(new Vector(3, 4),
                                     new Vector(3, 2),
                                     AreaType.Hall,
                                     "hall"));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            var priorityCells = new List<Vector>() {
                new Vector(3, 3),
                new Vector(4, 3),
                new Vector(5, 3),
                new Vector(2, 4),
                new Vector(2, 5),
            };

            Assert.That(builder.TestCellsToConnect, Has.Exactly(30).Items);
            Assert.That(builder.TestPriorityCells, Has.Exactly(5).Items);
            Assert.That(new List<Vector>(builder.TestPriorityCells.Keys),
                Is.EqualTo(priorityCells));
            Assert.That(builder.TestPriorityCells.First().Value,
                Is.EqualTo(priorityCells));
        }

        [Test]
        public void PicksPriorityCellsWhenAvailable() {
            var maze = Area.CreateMaze(new Vector(6, 6));
            maze.AddChildArea(Area.Create(new Vector(3, 4),
                                     new Vector(3, 2),
                                     AreaType.Hall,
                                     "hall"));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            var priorityCells = new List<Vector>() {
                new Vector(3, 3),
                new Vector(4, 3),
                new Vector(5, 3),
                new Vector(2, 4),
                new Vector(2, 5),
            };

            Assert.That(builder.TestCellsToConnect, Has.Exactly(30).Items);
            Assert.That(builder.TestPriorityCells, Has.Exactly(5).Items);

            var pickedCells = new HashSet<Vector>();
            for (var i = 0; i < 1000; i++) {
                var cell = builder.PickNextCellToLink();
                pickedCells.Add(cell);
                Assert.That(cell, Is.AnyOf(priorityCells));
            }

            Assert.That(pickedCells, Has.Exactly(5).Items);

            foreach (var cell in priorityCells) {
                if (maze.Grid.Contains(cell + Vector.East2D)) {
                    builder.Connect(cell, cell + Vector.East2D);
                }
                if (maze.Grid.Contains(cell + Vector.North2D)) {
                    builder.Connect(cell, cell + Vector.North2D);
                }
            }

            for (var i = 0; i < 1000; i++) {
                Assert.That(builder.PickNextCellToLink(),
                    Is.Not.AnyOf(priorityCells));
            }
        }

        [Test]
        public void PickRandomNeighborToLink() {
            var maze = Area.CreateMaze(new Vector(6, 6));
            maze.AddChildArea(Area.Create(new Vector(3, 4),
                                     new Vector(3, 2),
                                     AreaType.Hall,
                                     "hall"));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            // hall area neighbors are never chosen;
            for (var i = 0; i < 1000; i++) {
                builder.TryPickRandomNeighbor(new Vector(1, 5), out var randomNeighbor);
                Assert.That(randomNeighbor, Is.Not.Null);
                Assert.That(builder.TestPriorityCells.ContainsKey(randomNeighbor), Is.True);
                Assert.That(randomNeighbor.X, Is.AnyOf(0, 2));
            }

            // hall area neighbors are never chosen;
            for (var i = 0; i < 1000; i++) {
                builder.TryPickRandomNeighbor(new Vector(1, 5), out var randomNeighbor);
                Assert.That(randomNeighbor, Is.Not.Null);
                Assert.That(randomNeighbor.X, Is.Not.EqualTo(3));
            }
        }

        [Test]
        public void MarkConnected() {
            var maze = Area.CreateMaze(new Vector(3, 3));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            var connectedCells = new List<Vector>() {
                new Vector(0, 0),
                new Vector(1, 0),
                new Vector(1, 1),
                new Vector(1, 2)
            };

            for (var i = 0; i < connectedCells.Count - 1; i++) {
                builder.Connect(connectedCells[i], connectedCells[i + 1]);
            }

            Assert.That(builder.TestCellsToConnect, Has.Exactly(5).Items);
            Assert.That(builder.TestPriorityCells, Has.Exactly(0).Items);
            Assert.That(builder.TestConnectedCells, Has.Exactly(4).Items);

            connectedCells.ForEach(c =>
                Assert.That(builder.IsConnected(c)));
        }


        [Test]
        public void IterateUnlinkedCells_IteratesAllAvailableCells() {
            var maze = Area.CreateMaze(new Vector(6, 6));
            maze.AddChildArea(Area.Create(new Vector(3, 4),
                                     new Vector(3, 2),
                                     AreaType.Hall,
                                     "hall"));
            maze.AddChildArea(Area.Create(new Vector(1, 1),
                                     new Vector(2, 3),
                                     AreaType.Fill,
                                     "hall"));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            var priorityCells = new List<Vector>() {
                new Vector(3, 3),
                new Vector(4, 3),
                new Vector(5, 3),
                new Vector(2, 4),
                new Vector(2, 5),
            };

            var unavailableCells = new List<Vector>() {
                new Vector(1, 1),
                new Vector(2, 1),
                new Vector(1, 2),
                new Vector(2, 2),
                new Vector(1, 3),
                new Vector(2, 3),
            };


            Assert.That(builder.TestCellsToConnect, Has.Exactly(24).Items);
            Assert.That(builder.TestPriorityCells, Has.Exactly(5).Items);

            var iterate = builder.AllCells.ToList();
            var cellsOrder = maze.Grid.ToList();

            Assert.That(iterate, Is.SupersetOf(priorityCells));
            Assert.That(iterate, Has.None.AnyOf(unavailableCells));

            // check if the order of cells in IterateCells matches the 
            // order of cells in maze.Grid
            var allCellsIndex = -1;
            var iterateIdx = 0;
            for (; iterateIdx < iterate.Count; iterateIdx++) {
                var thisCellIndex =
                    cellsOrder.IndexOf(iterate[iterateIdx], allCellsIndex + 1);
                Assert.That(thisCellIndex, Is.GreaterThan(allCellsIndex));
                allCellsIndex = thisCellIndex;
            }
        }

        [Test]
        public void IsFillComplete_NoCells() {
            var maze = Area.CreateMaze(new Vector(5, 5));
            maze.AddChildArea(Area.Create(new Vector(0, 0),
                                     new Vector(5, 5),
                                     AreaType.Fill,
                                     "fill"));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Full() {
            var maze = Area.CreateMaze(new Vector(2, 2));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 0),
                new Vector(1, 0));
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 0),
                new Vector(0, 1));
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 1),
                new Vector(1, 1));
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Full_WithHall() {
            var maze = Area.CreateMaze(new Vector(5, 5));
            maze.AddChildArea(Area.Create(new Vector(1, 1),
                                     new Vector(3, 3),
                                     AreaType.Hall,
                                     "hall"));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(builder.IsFillComplete(), Is.False);
            var areaCells = maze.Grid
                    .SafeRegion(new Vector(1, 1), new Vector(3, 3));
            var cellsToConnect = maze.Grid.ToHashSet();
            cellsToConnect.ExceptWith(areaCells);
            foreach (var cell in cellsToConnect) {
                if (maze.Grid.Contains(cell + Vector.East2D)) {
                    builder.Connect(cell, cell + Vector.East2D);
                }
                if (maze.Grid.Contains(cell + Vector.North2D)) {
                    builder.Connect(cell, cell + Vector.North2D);
                }
            }
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Full_WithFill() {
            var maze = Area.CreateMaze(new Vector(5, 5));
            maze.AddChildArea(Area.Create(new Vector(1, 1),
                                          new Vector(3, 3),
                                          AreaType.Fill,
                                          "hall"));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(builder.IsFillComplete(), Is.False);
            var areaCells = maze.Grid
                    .SafeRegion(new Vector(1, 1), new Vector(3, 3));
            var cellsToConnect = maze.Grid.ToHashSet();
            cellsToConnect.ExceptWith(areaCells);
            foreach (var cell in cellsToConnect) {
                if (cellsToConnect.Contains(cell + Vector.East2D))
                    builder.Connect(cell, cell + Vector.East2D);
                if (cellsToConnect.Contains(cell + Vector.North2D))
                    builder.Connect(cell, cell + Vector.North2D);
            }
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Full_WithCave() {
            var maze = Area.CreateMaze(new Vector(5, 5));
            maze.AddChildArea(Area.Create(new Vector(1, 1),
                                     new Vector(3, 3),
                                     AreaType.Cave,
                                     "hall"));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(builder.IsFillComplete(), Is.False);
            var areaCells = maze.Grid
                    .SafeRegion(new Vector(1, 1), new Vector(3, 3));

            for (var i = 0; i < 4; i++) {
                builder.Connect(new Vector(i, 0), new Vector(i, 0) + Vector.East2D);
                builder.Connect(new Vector(i, 4), new Vector(i, 4) + Vector.East2D);
                builder.Connect(new Vector(0, i), new Vector(0, i) + Vector.North2D);
                builder.Connect(new Vector(4, i), new Vector(4, i) + Vector.North2D);
            }

            maze.Grid
                    .SafeRegion(new Vector(1, 1), new Vector(2, 2))
                    .ForEach(c => {
                        builder.Connect(c, c + Vector.East2D);
                        builder.Connect(c, c + Vector.North2D);
                    });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(new Vector(2, 3), new Vector(2, 3) + Vector.East2D);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_FullWidth() {
            var maze = Area.CreateMaze(new Vector(2, 2));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    FillFactor = GeneratorOptions.MazeFillFactor.FullWidth,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 0),
                new Vector(0, 1));
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 0),
                new Vector(1, 0));
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_FullHeight() {
            var maze = Area.CreateMaze(new Vector(2, 2));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    FillFactor = GeneratorOptions.MazeFillFactor.FullHeight,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 0),
                new Vector(1, 0));
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 0),
                new Vector(0, 1));
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Quarter() {
            var maze = Area.CreateMaze(new Vector(2, 2));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    FillFactor = GeneratorOptions.MazeFillFactor.Quarter,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 0),
                new Vector(1, 0));
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Half() {
            var maze = Area.CreateMaze(new Vector(2, 2));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    FillFactor = GeneratorOptions.MazeFillFactor.Half,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 0),
                new Vector(1, 0));
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_ThreeQuarters() {
            var maze = Area.CreateMaze(new Vector(2, 2));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    FillFactor = GeneratorOptions.MazeFillFactor.ThreeQuarters,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 0),
                new Vector(1, 0));
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 0),
                new Vector(0, 1));
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_NinetyPercent() {
            var maze = Area.CreateMaze(new Vector(2, 2));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    FillFactor = GeneratorOptions.MazeFillFactor.NinetyPercent,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 0),
                new Vector(1, 0));
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 0),
                new Vector(0, 1));
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                new Vector(0, 1),
                new Vector(1, 1));
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void ApplyAreas() {
            var maze = Area.CreateMaze(new Vector(5, 5));
            maze.AddChildArea(Area.Create(new Vector(1, 1),
                                     new Vector(3, 3),
                                     AreaType.Hall,
                                     "hall"));
            var walkway = new List<Vector>() {
                new Vector(1, 0),
                new Vector(0, 0)
            };
            var entrance = new Vector(1, 1);
            var areaCells = maze.Grid
                    .SafeRegion(new Vector(1, 1), new Vector(3, 3));

            Assert.That(maze.Grid.Where(c => maze[c].HasLinks()), Has.No.Member(walkway[0]));

            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();
            builder.Connect(walkway[0], walkway[1]);
            // this should connect entrance to walkway[0]
            builder.ApplyAreas();

            var connectedCells = areaCells
                .Where(c => builder.MazeArea[c].HasLinks())
                .ToList();

            Assert.That(maze.Grid.Where(c => builder.MazeArea[c].HasLinks()), Has.Member(entrance));
            Assert.That(maze.Grid.Where(c => builder.MazeArea[c].HasLinks()), Has.Member(walkway[0]));
            Assert.That(maze[entrance].Links(), Has.Member(walkway[0]));
        }

        [Test]
        public void OverlappingAreas_ProduceValidPriorityCells() {
            var maze = Area.CreateMaze(new Vector(15, 15));
            var area1 = Area.Create(new Vector(2, 3), new Vector(4, 7), AreaType.Hall);
            var area2 = Area.Create(new Vector(4, 8), new Vector(7, 3), AreaType.Hall);
            maze.AddChildArea(area1);
            maze.AddChildArea(area2);
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(builder.TestPriorityCells.Keys.Intersect(area1.Grid), Is.Empty);
            Assert.That(builder.TestCellsToConnect.Intersect(area1.Grid), Is.Empty);
            Assert.That(builder.TestPriorityCells.Keys.Intersect(area2.Grid), Is.Empty);
            Assert.That(builder.TestCellsToConnect.Intersect(area2.Grid), Is.Empty);

            foreach (var areaInfo in maze.ChildAreas) {
                if (areaInfo.Type == AreaType.Hall || areaInfo.Type == AreaType.Fill) {
                    var selectableCells = builder.TestCellsToConnect.Intersect(areaInfo.Grid).ToList();
                    if (selectableCells.Count > 0) {
                        Assert.Fail("Hall cells are in the cellsToConnect collection or have links.");
                    }
                }
            }
        }

        [Test]
        public void Maze2DBuilder_CellsNeighborsAreValid() {
            var rows = 5;
            var cols = 5;
            var map = Area.CreateMaze(new Vector(cols, rows));
            var builder = Maze2DBuilder.CreateFromOptions(map,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(map.Count, Is.EqualTo(rows * cols), "Wrong number of cells.");
            for (var x = 0; x < cols; x++) {
                for (var y = 0; y < rows; y++) {
                    var cell = new Vector(x, y);
                    var neighbors = map.Grid.Where(c =>
                        (c.X == cell.X && Math.Abs(c.Y - cell.Y) == 1) ||
                        (c.Y == cell.Y && Math.Abs(c.X - cell.X) == 1))
                            .ToList();
                    var nonNeighbors = map.Grid
                        .Where(c => c != cell && !neighbors.Contains(c))
                        .ToList();
                    if (neighbors.Count != builder.NeighborsOf(cell).Count()) {
                        TestLog.CreateForThisTest().D(5, $"Cell {cell.X},{cell.Y} has {neighbors.Count} neighbors. It should have {builder.NeighborsOf(cell).Count()} neighbors.");
                    }
                    Assert.That(neighbors.Count, Is.EqualTo(builder.NeighborsOf(cell).Count()));
                    Assert.That(neighbors.All(c => builder.NeighborsOf(cell).Contains(c)), Is.True);
                    Assert.That(nonNeighbors.Any(c => builder.NeighborsOf(cell).Contains(c)), Is.False);
                }
            }
        }

        [Test]
        public void LinksAreMutual() {
            var env = Area.CreateMaze(new Vector(5, 5));
            var builder = Maze2DBuilder.CreateFromOptions(env,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            var a = new Vector(2, 1);
            var b = new Vector(2, 2);

            builder.Connect(a, b);
            Assert.That(env[a].HardLinks, Has.Count.EqualTo(1));
            Assert.That(env[b].HardLinks, Has.Count.EqualTo(1));
            Assert.That(env[a].HasLinks(a + Vector.North2D), Is.True);
            Assert.That(env[b].HasLinks(b + Vector.South2D), Is.True);
            Assert.That(env[a + Vector.North2D], Is.EqualTo(env[b]));
            Assert.That(env[b + Vector.South2D], Is.EqualTo(env[a]));

            env[b].HardLinks.Remove(a);
            env[a].HardLinks.Remove(b);
            Assert.That(env[a].HasLinks(a + Vector.North2D), Is.False);
            Assert.That(env[b].HasLinks(b + Vector.South2D), Is.False);
            Assert.That(env[a].HardLinks, Has.Count.EqualTo(0));
            Assert.That(env[b].HardLinks, Has.Count.EqualTo(0));
        }

        [Test]
        public void LinksOnlyWithNeighbors() {
            var env = Area.CreateMaze(new Vector(5, 5));
            var builder = Maze2DBuilder.CreateFromOptions(env,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();
            var a = new Vector(2, 1);
            var b = new Vector(2, 2);
            var c = new Vector(2, 3);

            Assert.That(() => builder.Connect(a, c), Throws.InstanceOf<InvalidOperationException>());
        }

        [Test]
        public void DoubleLinkingThrowsError() {
            var env = Area.CreateMaze(new Vector(5, 5));
            var builder = Maze2DBuilder.CreateFromOptions(env,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();
            var a = new Vector(1, 2);
            var b = new Vector(2, 2);
            builder.Connect(a, b);
            Assert.Throws<InvalidOperationException>(() => builder.Connect(a, a));
        }

        [Test]
        public void CanConnect() {
            var maze = Area.CreateMaze(new Vector(10, 10));
            maze.AddChildArea(Area.Create(new Vector(1, 2),
                                     new Vector(3, 2),
                                     AreaType.Hall,
                                     "hall"));
            maze.AddChildArea(Area.Create(new Vector(6, 1),
                                     new Vector(2, 4),
                                     AreaType.Fill,
                                     "fill"));
            maze.AddChildArea(Area.Create(new Vector(1, 6),
                                     new Vector(2, 2),
                                     AreaType.Cave,
                                     "cave"));
            var builder = Maze2DBuilder.CreateFromOptions(maze,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Wilsons,
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.TestRebuildCellMaps();

            Assert.That(
                builder.CanConnect(
                    new Vector(0, 0),
                    new Vector(0, 0) + Vector.East2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    new Vector(0, 2),
                    new Vector(0, 2) + Vector.East2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    new Vector(2, 1),
                    new Vector(2, 1) + Vector.North2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    new Vector(2, 2),
                    new Vector(2, 2) + Vector.East2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    new Vector(3, 2),
                    new Vector(3, 2) + Vector.East2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    new Vector(4, 2),
                    new Vector(4, 2) + Vector.East2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    new Vector(5, 2),
                    new Vector(5, 2) + Vector.East2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    new Vector(0, 6),
                    new Vector(0, 6) + Vector.East2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    new Vector(1, 5),
                    new Vector(1, 5) + Vector.North2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    new Vector(1, 9),
                    new Vector(1, 9) + Vector.South2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    new Vector(2, 6),
                    new Vector(2, 6) + Vector.East2D),
                Is.True);
        }
    }
}
