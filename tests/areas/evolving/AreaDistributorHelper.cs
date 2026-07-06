using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Renderers;

namespace PlayersWorlds.Maps.Areas.Evolving {
    internal class AreaDistributorHelper {
        internal static DistributeResult Distribute(
            RandomSource random,
            TestLog log,
            Vector mapSize,
            IEnumerable<Area> areas,
            int maxEpochs = -1) => Distribute(
                random, log, Area.CreateMaze(mapSize), areas, maxEpochs);

        internal static DistributeResult Distribute(
            RandomSource random,
            TestLog log,
            Area env,
            IEnumerable<Area> areas,
            int maxEpochs = -1) {

            var debugLevel = 0;
            if (TestContext.Parameters.Exists("DEBUG")) {
                debugLevel = int.Parse(TestContext.Parameters["DEBUG"]);
            }
            if (TestContext.Parameters.Exists("EPOCHS")) {
                maxEpochs = int.Parse(TestContext.Parameters["EPOCHS"]);
            }

            var managedAreas = areas.ToList();
            var originalCopy = managedAreas
                .Select(x => x.ShallowCopy())
                .ToList();

            if (debugLevel >= 4) {
                log.Buffered.D(4, new MapAreaStringRenderer().Render(env.Size, managedAreas.Select(a => (a, ""))));
                log.Buffered.Flush();
            }

            var result = new DistributeResult {
                Log = log,
                MaxEpochs = maxEpochs,
                OriginalAreas = originalCopy,
                OriginalOverlapping =
                    originalCopy
                        .Where(
                            a => originalCopy.Any(
                                    b => a != b &&
                                         a.Grid.Overlaps(b.Grid)))
                        .ToList(),
                OriginalOutOfBounds =
                    originalCopy
                        .Where(
                            area => !area.Grid.FitsInto(env.Grid))
                        .ToList(),
                PlacedAreas = managedAreas
            };

            new EvolvingSimulator(maxEpochs, 20).Evolve(
                new MapAreaSystemFactory(random).Create(env, managedAreas));

            result.PlacedOverlapping =
                result.PlacedAreas
                    .Where(a =>
                        result.PlacedAreas.Any(
                            b => a != b &&
                                 a.Grid.Overlaps(b.Grid)))
                    .ToList();
            result.PlacedOutOfBounds =
                result.PlacedAreas
                    .Where(block => !block.Grid.FitsInto(env.Grid))
                    .ToList();

            result.TestString = $"yield return \"{env.Size}: " +
                string.Join(" ", result.OriginalAreas.Select(area =>
                    $"P{area.Position};S{area.Size}")) +
                    "\"; // " + random.ToString();

            if (debugLevel >= 4) {
                log.Buffered.D(4, new MapAreaStringRenderer().Render(env.Size, managedAreas.Select(a => (a, ""))));
                log.Buffered.Flush();
            }

            return result;
        }

        internal class DistributeResult {
            public TestLog Log { get; set; }
            public List<Area> OriginalOutOfBounds { get; set; }
            public List<Area> OriginalOverlapping { get; set; }
            public List<Area> OriginalAreas { get; set; }
            public List<Area> PlacedOutOfBounds { get; set; }
            public List<Area> PlacedOverlapping { get; set; }
            public List<Area> PlacedAreas { get; set; }
            public int MaxEpochs { get; set; }
            public string TestString { get; set; }

            internal void AssertAllFit(RandomSource random) {
                if (PlacedOutOfBounds.Count > 0 || PlacedOverlapping.Count > 0) {
                    Log?.Buffered.Flush();
                }
                Assert.That(PlacedOutOfBounds, Is.Empty,
                    "Out Of Bounds: " + string.Join(", ",
                        PlacedOutOfBounds.Select(area => $"P{area.Position};S{area.Size} ({random})")));
                Assert.That(PlacedOverlapping, Is.Empty,
                    "Overlapping: " + string.Join(", ",
                        PlacedOverlapping.Select(area => $"P{area.Position};S{area.Size} ({random})")));
            }

            internal void AssertDoesNotFit(RandomSource random) {
                if (PlacedOutOfBounds.Count > 0 || PlacedOverlapping.Count > 0) {
                    Log?.Buffered.Flush();
                }
                Assert.That(PlacedOutOfBounds.Concat(PlacedOverlapping), Is.Not.Empty,
                    "Out Of Bounds: " + string.Join(", ",
                        PlacedOutOfBounds.Select(area => $"P{area.Position};S{area.Size} ({random})") +
                    ". Overlapping: " + string.Join(", ",
                        PlacedOverlapping.Select(area => $"P{area.Position};S{area.Size} ({random})"))));
            }
        }
    }
}
