using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Maze.PostProcessing;
using PlayersWorlds.Maps.Serializer;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps.Examples {
    /// <summary>
    /// Worked example for the Phase-0 region contract (see
    /// <c>openspec/changes/maze-gen-mazzzze-phase0-contract</c>). It documents
    /// two maze-gen behaviours the façade / store seam depends on:
    /// <list type="number">
    /// <item>the <b>corrected POIs</b> — after the S6 fix, the longest-path
    /// entrance and exit resolve to two <i>distinct</i> cells;</item>
    /// <item>the <b>lossless round-trip</b> — <see cref="AreaSerializer"/>
    /// preserves the maze structure (cells + hard links), so a stored region
    /// reloads identically and its POIs can be <i>recomputed</i> from the
    /// reloaded structure (POI markers are extension attachments and are not
    /// themselves serialized — a deterministic recompute is the contract).
    /// </item>
    /// </list>
    /// </summary>
    [TestFixture]
    public class Phase0RegionContractExample : Test {
        [Test]
        public void CorrectedPois_AndLosslessRoundTrip() {
            // 1. Generate a region (a Border maze) and mark its longest path.
            var world = new GeneratedWorld(RandomSource)
                .AddLayer(AreaType.Maze, new Vector(12, 12))
                .OfMaze(MazeStructureStyle.Border, new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.RecursiveBacktracker,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full
                })
                .MarkLongestPath();
            var region = world.Map();

            // 2. Corrected POIs: entrance and exit are two DISTINCT cells.
            //    (Before the S6 fix the end marker was written onto the start
            //    cell, so entrance == exit.)
            var entrance = region.Grid.Single(v =>
                region[v].X<DijkstraDistance.IsLongestTrailStartExtension>() != null);
            var exit = region.Grid.Single(v =>
                region[v].X<DijkstraDistance.IsLongestTrailEndExtension>() != null);
            Assert.That(entrance, Is.Not.EqualTo(exit),
                "entrance and exit POIs must be distinct cells");

            var originalTrail =
                region.X<DijkstraDistance.LongestTrailExtension>().LongestTrail;
            Assert.That(originalTrail.First(), Is.EqualTo(entrance));
            Assert.That(originalTrail.Last(), Is.EqualTo(exit));

            // 3. Lossless round-trip through the store-seam serializer.
            var serializer = new AreaSerializer();
            var reloaded = serializer.Deserialize(serializer.Serialize(region));

            // Structure survives: same size and the exact same hard links.
            Assert.That(reloaded.Size, Is.EqualTo(region.Size));
            var originalLinks = region.Grid.Sum(v => region[v].HardLinks.Count);
            var reloadedLinks = reloaded.Grid.Sum(v => reloaded[v].HardLinks.Count);
            Assert.That(reloadedLinks, Is.EqualTo(originalLinks));

            // POI markers are NOT serialized -- the reloaded region has none...
            Assert.That(reloaded.Grid.Any(v =>
                reloaded[v].X<DijkstraDistance.IsLongestTrailStartExtension>() != null),
                Is.False);

            // ...they are recomputed from the reloaded structure, and because
            // generation is deterministic the recomputed trail has the same
            // length as the original.
            var recomputed = DijkstraDistance.FindLongestTrail(reloaded);
            Assert.That(recomputed.LongestTrail.Count,
                Is.EqualTo(originalTrail.Count));
            var reEntrance = reloaded.Grid.Single(v =>
                reloaded[v].X<DijkstraDistance.IsLongestTrailStartExtension>() != null);
            var reExit = reloaded.Grid.Single(v =>
                reloaded[v].X<DijkstraDistance.IsLongestTrailEndExtension>() != null);
            Assert.That(reEntrance, Is.Not.EqualTo(reExit));
        }
    }
}
