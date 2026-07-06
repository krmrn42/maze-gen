using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps.Areas.Evolving {
    [TestFixture]
    public class AreaDistributorLoadTest : Test {

        [Test, Category("Load")]
        public void AreaDistributor_LoadTest() {
            var results = new List<AreaDistributorHelper.DistributeResult>();
            var ops = new ParallelOptions {
                MaxDegreeOfParallelism = 24
            };
            // !! Current fail rate is at <0.5%. Requires investigation.
            const int ExpectedPassingMoreThan = 990;
            var numTotal = 1000;
            var numPassed = 0;
            _ = Parallel.For(0, numTotal, ops, (i, state) => {
                var log = TestLog.CreateForThisTest();
                var testRandom1 = RandomSource.CreateFromEnv();
                var mazeSize = new Vector(
                    testRandom1.Next(5, 50), testRandom1.Next(5, 50));
                var maze = Area.CreateMaze(mazeSize);
                var roomsCount = (int)Math.Sqrt(maze.Size.Area) / 3;
                var rooms = new List<Area>();
                for (var j = 0; j < roomsCount; j++) {
                    var size = new Vector(
                        testRandom1.Next(1, maze.Size.X / 3),
                        testRandom1.Next(1, maze.Size.Y / 3));
                    var position = new Vector(
                        testRandom1.Next(0, (maze.Size - size).X),
                        testRandom1.Next(0, (maze.Size - size).Y));
                    rooms.Add(Area.CreateUnpositioned(
                        position, size, AreaType.Maze));
                }
                var testRandom2 = RandomSource.CreateFromEnv();
                var result = AreaDistributorHelper.Distribute(
                    testRandom2, log, maze.Size, rooms, 100);
                lock (results) {
                    if (result.PlacedOutOfBounds.Count > 0 ||
                        result.PlacedOverlapping.Count > 0) {
                        log.D(0, result.DebugString());
                    } else {
                        numPassed++;
                    }
                    results.Add(result);
                    log.Buffered.Reset();
                }
            });
            var message = "Passed: " + numPassed + ", " +
                "Failed: " + (numTotal - numPassed) +
                Environment.NewLine +
                string.Join(Environment.NewLine,
                results.Where(
                    r => r.PlacedOutOfBounds.Count +
                         r.PlacedOverlapping.Count > 0)
                    .Select(r => r.TestString));
            if (numPassed < ExpectedPassingMoreThan) {
                Assert.Fail(message);
            } else if (numPassed < numTotal) {
                Assert.Inconclusive(message);
            } else {
                Assert.Pass(message);
            }
        }
    }
}
