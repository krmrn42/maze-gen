using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Renderers;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps.Areas.Evolving {
    [TestFixture, Category("Integration")]
    public class AreaDistributorIntegrationTest : Test {
        private RandomSource _random;
        public override void SetUp() {
            base.SetUp();
            _random = RandomSource.CreateFromEnv();
        }
        private static Area TestArea(int x, int y, int width, int height) =>
            Area.CreateUnpositioned(
                new Vector(x, y), new Vector(width, height), AreaType.Maze);

        [Test, Category("Integration")]
        public void AreaDistributorTest_OneTest() =>
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(), new Vector(10, 10),
                new List<Area>() { TestArea(0, 0, 4, 4) },
                maxEpochs: 10)
                .AssertAllFit(_random);

        [Test, Category("Integration")]
        public void AreaDistributorTest_SidePressure() =>
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(), new Vector(10, 10),
                new List<Area>() {
                    TestArea(1, 4, 8, 2),
                    TestArea(1, 5, 2, 4),
                    TestArea(2, 6, 2, 4),
                    TestArea(3, 7, 2, 4)
                },
                maxEpochs: 10)
                .AssertAllFit(_random);

        [Test, Category("Integration")]
        public void AreaDistributorTest_TwoTest() =>
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(),
                new Vector(10, 10),
                new List<Area>() {
                    TestArea(0, 0, 2, 2),
                    TestArea(10, 10, 2, 2)
                },
                maxEpochs: 10
                ).AssertAllFit(_random);

        [Test, Category("Integration")]
        public void AreaDistributorTest_OverlapTwo(
            [ValueSource("OverlapTwoTests")] string layout
            ) {
            var serializer = new AreaSerializer();
            var env = serializer.Deserialize(layout);
            var childAreas = env.ChildAreas.ToList();
            env.ClearChildAreas();
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(),
                env,
                childAreas,
                maxEpochs: 10)
                .AssertAllFit(_random);
        }

        public static IEnumerable<string> OverlapTwoTests() {
            yield return "Area:{11x11;0x0;False;Maze;;;[Area:{4x4;4x4;False;Maze;;;},Area:{4x4;4x4;False;Maze;;;}]}";
            yield return "Area:{16x16;0x0;False;Maze;;;[Area:{8x8;4x4;False;Maze;;;},Area:{2x2;5x9;False;Maze;;;}]}";
            yield return "Area:{16x16;0x0;False;Maze;;;[Area:{8x8;4x4;False;Maze;;;},Area:{4x4;5x7;False;Maze;;;}]}";
            yield return "Area:{16x16;0x0;False;Maze;;;[Area:{6x6;5x5;False;Maze;;;},Area:{4x4;6x6;False;Maze;;;}]}";
            yield return "Area:{11x11;0x0;False;Maze;;;[Area:{4x4;2x2;False;Maze;;;},Area:{4x4;5x5;False;Maze;;;}]}";
            yield return "Area:{11x11;0x0;False;Maze;;;[Area:{4x4;5x2;False;Maze;;;},Area:{4x4;2x5;False;Maze;;;}]}";
            yield return "Area:{11x11;0x0;False;Maze;;;[Area:{4x4;2x5;False;Maze;;;},Area:{4x4;5x2;False;Maze;;;}]}";
            yield return "Area:{11x11;0x0;False;Maze;;;[Area:{4x4;5x5;False;Maze;;;},Area:{4x4;2x2;False;Maze;;;}]}";
            yield return "Area:{11x11;0x0;False;Maze;;;[Area:{4x4;4x4;False;Maze;;;},Area:{4x4;3x3;False;Maze;;;}]}";
            yield return "Area:{11x11;0x0;False;Maze;;;[Area:{4x4;4x3;False;Maze;;;},Area:{4x4;3x4;False;Maze;;;}]}";
        }

        [Test, Category("Integration")]
        public void AreaDistributorTest_SingleMapForce(
            [ValueSource("VariousSingles")] string layout
            ) {
            var serializer = new AreaSerializer();
            var env = serializer.Deserialize(layout);
            var childAreas = env.ChildAreas.ToList();
            env.ClearChildAreas();
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(), env,
                childAreas,
                maxEpochs: 10)
                .AssertAllFit(_random);
        }

        [Test]
        public void AreaDistributorTest_SingleMapForce_Debug() {
            AreaDistributorTest_SingleMapForce("Area:{6x12;0x0;False;Maze;;;[Area:{2x2;-1x-4;False;Maze;;;}]}");
        }

        [Test, Category("Integration"), Category("Smoke")]
        public void AreaDistributorTest_CanLayout(
            [ValueSource("TestLayouts")] string layout
            ) {
            var serializer = new AreaSerializer();
            var env = serializer.Deserialize(layout);
            var childAreas = env.ChildAreas.ToList();
            env.ClearChildAreas();
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(), env,
                childAreas,
                maxEpochs: 10)
                .AssertAllFit(_random);
        }

        public static IEnumerable<string> VariousSingles() {
            // inside
            yield return "Area:{6x12;0x0;False;Maze;;;[Area:{2x2;1x2;False;Maze;;;}]}";
            yield return "Area:{6x12;0x0;False;Maze;;;[Area:{2x2;2x2;False;Maze;;;}]}";
            yield return "Area:{6x12;0x0;False;Maze;;;[Area:{2x2;3x2;False;Maze;;;}]}";
            yield return "Area:{6x12;0x0;False;Maze;;;[Area:{2x2;1x5;False;Maze;;;}]}";
            yield return "Area:{6x12;0x0;False;Maze;;;[Area:{2x2;2x5;False;Maze;;;}]}";
            yield return "Area:{6x12;0x0;False;Maze;;;[Area:{2x2;3x5;False;Maze;;;}]}";
            yield return "Area:{6x12;0x0;False;Maze;;;[Area:{2x2;1x8;False;Maze;;;}]}";
            yield return "Area:{6x12;0x0;False;Maze;;;[Area:{2x2;2x8;False;Maze;;;}]}";
            yield return "Area:{6x12;0x0;False;Maze;;;[Area:{2x2;3x8;False;Maze;;;}]}";

            // outside
            yield return "Area:{6x12;0x0;False;Maze;;;[Area:{2x2;-1x-4;False;Maze;;;}]}";
        }

        public static IEnumerable<string> TestLayouts() {
            yield return "Area:{13x8;0x0;False;Maze;;;[Area:{3x1;9x6;False;Maze;;;},Area:{2x1;1x4;False;Maze;;;}]}";
            yield return "Area:{20x20;0x0;False;Maze;;;[Area:{5x3;14x14;False;Maze;;;},Area:{1x4;-2x14;False;Maze;;;},Area:{2x4;7x-3;False;Maze;;;},Area:{1x3;2x4;False;Maze;;;}]}";
            yield return "Area:{6x14;0x0;False;Maze;;;[Area:{1x3;0x10;False;Maze;;;}]}";
            yield return "Area:{23x22;0x0;False;Maze;;;[Area:{6x2;11x10;False;Maze;;;},Area:{5x1;-1x14;False;Maze;;;},Area:{5x3;5x-3;False;Maze;;;},Area:{2x1;11x22;False;Maze;;;}]}";
            yield return "Area:{18x23;0x0;False;Maze;;;[Area:{5x5;-3x-8;False;Maze;;;},Area:{3x6;30x8;False;Maze;;;},Area:{4x4;9x3;False;Maze;;;},Area:{1x3;12x15;False;Maze;;;}]}";
            yield return "Area:{23x8;0x0;False;Maze;;;[Area:{4x1;4x7;False;Maze;;;},Area:{1x1;2x-1;False;Maze;;;}]}";
            yield return "Area:{21x13;0x0;False;Maze;;;[Area:{6x3;8x-3;False;Maze;;;},Area:{6x3;8x4;False;Maze;;;},Area:{4x2;7x2;False;Maze;;;}]}";
            yield return "Area:{16x14;0x0;False;Maze;;;[Area:{3x1;2x13;False;Maze;;;},Area:{4x3;11x-1;False;Maze;;;}]}";
            yield return "Area:{24x12;0x0;False;Maze;;;[Area:{2x1;12x7;False;Maze;;;},Area:{5x2;14x-1;False;Maze;;;},Area:{1x1;16x11;False;Maze;;;}]}";
            yield return "Area:{9x18;0x0;False;Maze;;;[Area:{2x2;2x10;False;Maze;;;},Area:{2x5;2x8;False;Maze;;;}]}";
            yield return "Area:{9x18;0x0;False;Maze;;;[Area:{2x5;2x8;False;Maze;;;},Area:{2x2;2x10;False;Maze;;;}]}";
            yield return "Area:{21x21;0x0;False;Maze;;;[Area:{6x6;-3x-3;False;Maze;;;},Area:{4x2;6x10;False;Maze;;;},Area:{2x2;9x16;False;Maze;;;},Area:{4x2;3x14;False;Maze;;;}]}";
            yield return "Area:{14x24;0x0;False;Maze;;;[Area:{2x7;-9x5;False;Maze;;;},Area:{1x7;26x5;False;Maze;;;},Area:{3x7;3x11;False;Maze;;;},Area:{3x2;5x19;False;Maze;;;},Area:{2x5;3x6;False;Maze;;;},Area:{2x2;2x19;False;Maze;;;}]}";
            yield return "Area:{33x10;0x0;False;Maze;;;[Area:{10x1;13x11;False;Maze;;;},Area:{10x1;13x-1;False;Maze;;;},Area:{8x1;19x3;False;Maze;;;},Area:{9x1;4x6;False;Maze;;;},Area:{9x2;9x3;False;Maze;;;},Area:{3x2;2x3;False;Maze;;;}]}";
            yield return "Area:{6x49;0x0;False;Maze;;;[Area:{1x11;-1x5;False;Maze;;;},Area:{1x11;5x17;False;Maze;;;},Area:{1x9;2x23;False;Maze;;;},Area:{1x13;2x14;False;Maze;;;},Area:{1x9;0x12;False;Maze;;;}]}";
            yield return "Area:{35x5;0x0;False;Maze;;;[Area:{10x1;5x-1;False;Maze;;;},Area:{8x1;23x4;False;Maze;;;},Area:{3x1;4x2;False;Maze;;;},Area:{2x1;12x2;False;Maze;;;}]}";
            yield return "Area:{42x5;0x0;False;Maze;;;[Area:{10x1;4x5;False;Maze;;;},Area:{3x1;30x1;False;Maze;;;},Area:{3x1;10x1;False;Maze;;;},Area:{1x1;2x2;False;Maze;;;}]}";
            yield return "Area:{39x5;0x0;False;Maze;;;[Area:{10x1;14x5;False;Maze;;;},Area:{9x1;5x-1;False;Maze;;;},Area:{1x1;2x2;False;Maze;;;},Area:{8x1;10x4;False;Maze;;;}]}";
            yield return "Area:{6x47;0x0;False;Maze;;;[Area:{1x12;6x12;False;Maze;;;},Area:{1x5;3x13;False;Maze;;;},Area:{1x4;3x18;False;Maze;;;},Area:{1x6;3x38;False;Maze;;;},Area:{1x2;2x2;False;Maze;;;}]}";
            yield return "Area:{6x47;0x0;False;Maze;;;[Area:{1x11;-1x28;False;Maze;;;},Area:{1x11;6x11;False;Maze;;;},Area:{1x11;6x6;False;Maze;;;},Area:{1x11;-1x6;False;Maze;;;},Area:{1x5;1x35;False;Maze;;;}]}";
            yield return "Area:{6x47;0x0;False;Maze;;;[Area:{1x14;7x10;False;Maze;;;},Area:{1x3;2x41;False;Maze;;;},Area:{1x3;3x4;False;Maze;;;},Area:{1x4;3x10;False;Maze;;;},Area:{1x9;0x25;False;Maze;;;}]}";
            yield return "Area:{5x45;0x0;False;Maze;;;[Area:{1x10;-1x5;False;Maze;;;},Area:{1x12;-2x21;False;Maze;;;},Area:{1x14;7x17;False;Maze;;;},Area:{1x1;2x40;False;Maze;;;},Area:{1x4;1x29;False;Maze;;;}]}";
            yield return "Area:{46x5;0x0;False;Maze;;;[Area:{10x1;4x-1;False;Maze;;;},Area:{7x1;29x0;False;Maze;;;},Area:{2x1;29x2;False;Maze;;;},Area:{10x1;32x2;False;Maze;;;},Area:{5x1;17x0;False;Maze;;;}]}";
            yield return "Area:{6x48;0x0;False;Maze;;;[Area:{1x14;-2x28;False;Maze;;;},Area:{1x12;6x6;False;Maze;;;},Area:{1x2;3x22;False;Maze;;;},Area:{1x7;1x14;False;Maze;;;},Area:{1x12;2x23;False;Maze;;;}]}";
            yield return "Area:{47x5;0x0;False;Maze;;;[Area:{13x1;30x-3;False;Maze;;;},Area:{11x1;24x-2;False;Maze;;;},Area:{14x1;17x7;False;Maze;;;},Area:{2x1;27x2;False;Maze;;;},Area:{4x1;4x1;False;Maze;;;}]}";
            yield return "Area:{5x45;0x0;False;Maze;;;[Area:{1x10;-2x13;False;Maze;;;},Area:{1x12;-2x7;False;Maze;;;},Area:{1x2;2x3;False;Maze;;;},Area:{1x8;0x3;False;Maze;;;},Area:{1x1;2x10;False;Maze;;;}]}";
            yield return "Area:{5x47;0x0;False;Maze;;;[Area:{1x13;7x28;False;Maze;;;},Area:{1x11;6x15;False;Maze;;;},Area:{1x3;1x2;False;Maze;;;},Area:{1x2;2x40;False;Maze;;;},Area:{1x1;2x43;False;Maze;;;}]}";
            yield return "Area:{5x36;0x0;False;Maze;;;[Area:{1x11;-2x21;False;Maze;;;},Area:{1x1;2x12;False;Maze;;;},Area:{1x2;2x15;False;Maze;;;},Area:{1x3;2x8;False;Maze;;;}]}";
            yield return "Area:{6x39;0x0;False;Maze;;;[Area:{1x11;-1x20;False;Maze;;;},Area:{1x2;3x19;False;Maze;;;},Area:{1x11;2x11;False;Maze;;;},Area:{1x5;2x20;False;Maze;;;},Area:{1x5;3x10;False;Maze;;;}]}";
            yield return "Area:{44x6;0x0;False;Maze;;;[Area:{13x1;23x-2;False;Maze;;;},Area:{13x1;7x-2;False;Maze;;;},Area:{8x1;6x2;False;Maze;;;},Area:{2x1;37x3;False;Maze;;;},Area:{10x1;29x2;False;Maze;;;}]}";
            yield return "Area:{6x42;0x0;False;Maze;;;[Area:{1x11;-1x6;False;Maze;;;},Area:{1x13;-2x21;False;Maze;;;},Area:{1x3;3x36;False;Maze;;;},Area:{1x9;5x7;False;Maze;;;},Area:{1x10;0x27;False;Maze;;;}]}";
            yield return "Area:{40x5;0x0;False;Maze;;;[Area:{12x1;8x6;False;Maze;;;},Area:{9x1;24x-1;False;Maze;;;},Area:{8x1;3x0;False;Maze;;;},Area:{2x1;23x2;False;Maze;;;}]}";
            yield return "Area:{47x7;0x0;False;Maze;;;[Area:{14x1;19x7;False;Maze;;;},Area:{8x1;9x2;False;Maze;;;},Area:{8x1;7x4;False;Maze;;;},Area:{4x1;14x2;False;Maze;;;},Area:{10x1;28x5;False;Maze;;;},Area:{2x1;35x3;False;Maze;;;}]}";
            yield return "Area:{48x6;0x0;False;Maze;;;[Area:{11x1;32x-1;False;Maze;;;},Area:{14x1;9x7;False;Maze;;;},Area:{13x1;5x7;False;Maze;;;},Area:{15x1;27x2;False;Maze;;;},Area:{3x1;5x3;False;Maze;;;}]}";
            yield return "Area:{46x6;0x0;False;Maze;;;[Area:{13x1;20x-2;False;Maze;;;},Area:{14x1;10x7;False;Maze;;;},Area:{4x1;22x2;False;Maze;;;},Area:{6x1;35x3;False;Maze;;;},Area:{7x1;17x3;False;Maze;;;}]}";
            yield return "Area:{6x39;0x0;False;Maze;;;[Area:{1x11;-1x10;False;Maze;;;},Area:{1x7;0x3;False;Maze;;;},Area:{1x10;0x11;False;Maze;;;},Area:{1x7;2x20;False;Maze;;;},Area:{1x6;3x26;False;Maze;;;}]}";
            yield return "Area:{43x42;0x0;False;Maze;;;[Area:{6x7;3x34;False;Maze;;;},Area:{9x11;11x25;False;Maze;;;}]}";
            yield return "Area:{43x42;0x0;False;Maze;;;[Area:{2x11;35x26;False;Maze;;;},Area:{12x2;34x35;False;Maze;;;}]}";
            yield return "Area:{43x42;0x0;False;Maze;;;[Area:{4x4;32x32;False;Maze;;;},Area:{2x2;34x34;False;Maze;;;}]}";
            yield return "Area:{43x42;0x0;False;Maze;;;[Area:{4x4;32x32;False;Maze;;;},Area:{4x2;34x34;False;Maze;;;}]}";
            yield return "Area:{43x42;0x0;False;Maze;;;[Area:{4x10;32x26;False;Maze;;;},Area:{4x2;34x34;False;Maze;;;}]}";
            yield return "Area:{43x42;0x0;False;Maze;;;[Area:{2x11;30x26;False;Maze;;;},Area:{12x2;29x35;False;Maze;;;}]}";
            yield return "Area:{49x26;0x0;False;Maze;;;[Area:{12x2;38x5;False;Maze;;;},Area:{4x1;1x12;False;Maze;;;},Area:{7x7;21x10;False;Maze;;;},Area:{8x6;20x3;False;Maze;;;},Area:{12x4;29x0;False;Maze;;;},Area:{15x3;17x22;False;Maze;;;},Area:{9x2;10x1;False;Maze;;;},Area:{3x5;32x18;False;Maze;;;},Area:{7x4;1x21;False;Maze;;;},Area:{13x7;36x10;False;Maze;;;},Area:{10x7;6x9;False;Maze;;;}]}";
            yield return "Area:{26x29;0x0;False;Maze;;;[Area:{7x3;10x15;False;Maze;;;},Area:{5x6;20x0;False;Maze;;;},Area:{7x7;17x5;False;Maze;;;},Area:{6x4;5x4;False;Maze;;;},Area:{4x2;21x18;False;Maze;;;},Area:{3x8;20x8;False;Maze;;;},Area:{4x5;10x11;False;Maze;;;},Area:{1x8;0x5;False;Maze;;;},Area:{3x6;21x9;False;Maze;;;}]}";
            yield return "Area:{8x45;0x0;False;Maze;;;[Area:{1x6;2x9;False;Maze;;;},Area:{1x8;2x22;False;Maze;;;},Area:{1x10;5x32;False;Maze;;;},Area:{1x9;6x14;False;Maze;;;},Area:{1x14;4x3;False;Maze;;;},Area:{1x8;2x36;False;Maze;;;}]}";
            yield return "Area:{31x47;0x0;False;Maze;;;[Area:{9x1;2x15;False;Maze;;;},Area:{5x4;3x11;False;Maze;;;},Area:{5x7;2x11;False;Maze;;;},Area:{5x10;13x14;False;Maze;;;},Area:{2x9;8x37;False;Maze;;;},Area:{3x8;27x19;False;Maze;;;},Area:{7x7;20x12;False;Maze;;;},Area:{4x2;4x13;False;Maze;;;},Area:{6x2;20x31;False;Maze;;;},Area:{9x3;9x3;False;Maze;;;},Area:{6x1;6x34;False;Maze;;;},Area:{5x2;23x6;False;Maze;;;}]}";
            yield return "Area:{7x47;0x0;False;Maze;;;[Area:{1x5;4x40;False;Maze;;;},Area:{1x7;0x12;False;Maze;;;},Area:{1x3;1x9;False;Maze;;;},Area:{1x14;5x0;False;Maze;;;},Area:{1x9;2x37;False;Maze;;;},Area:{1x6;0x29;False;Maze;;;}]}";
            yield return "Area:{48x38;0x0;False;Maze;;;[Area:{1x3;8x15;False;Maze;;;},Area:{2x7;45x11;False;Maze;;;},Area:{1x3;6x8;False;Maze;;;},Area:{10x11;4x7;False;Maze;;;},Area:{2x8;4x9;False;Maze;;;},Area:{1x10;26x13;False;Maze;;;},Area:{10x7;18x13;False;Maze;;;},Area:{10x5;35x22;False;Maze;;;},Area:{5x9;31x10;False;Maze;;;},Area:{13x6;32x10;False;Maze;;;},Area:{14x6;22x22;False;Maze;;;},Area:{3x3;26x1;False;Maze;;;},Area:{13x10;28x14;False;Maze;;;},Area:{12x8;24x4;False;Maze;;;}]}";
            yield return "Area:{31x32;0x0;False;Maze;;;[Area:{8x7;24x16;False;Maze;;;},Area:{1x6;14x9;False;Maze;;;},Area:{9x9;16x6;False;Maze;;;},Area:{1x4;18x27;False;Maze;;;},Area:{3x3;14x1;False;Maze;;;},Area:{4x7;9x18;False;Maze;;;},Area:{9x6;2x15;False;Maze;;;},Area:{7x7;0x4;False;Maze;;;},Area:{5x7;25x24;False;Maze;;;},Area:{6x9;11x20;False;Maze;;;}]}";
            yield return "Area:{45x43;0x0;False;Maze;;;[Area:{8x10;38x15;False;Maze;;;},Area:{10x4;0x0;False;Maze;;;},Area:{8x2;18x33;False;Maze;;;},Area:{1x12;0x23;False;Maze;;;},Area:{12x7;18x1;False;Maze;;;},Area:{14x7;23x20;False;Maze;;;},Area:{1x7;22x17;False;Maze;;;},Area:{8x9;11x9;False;Maze;;;},Area:{12x7;10x24;False;Maze;;;},Area:{8x5;0x8;False;Maze;;;},Area:{12x3;1x19;False;Maze;;;},Area:{7x2;20x39;False;Maze;;;},Area:{10x4;34x38;False;Maze;;;},Area:{11x12;34x1;False;Maze;;;}]}";
            yield return "Area:{49x10;0x0;False;Maze;;;[Area:{13x7;4x1;False;Maze;;;},Area:{10x7;8x1;False;Maze;;;}]}";
            yield return "Area:{49x16;0x0;False;Maze;;;[Area:{12x4;2x7;False;Maze;;;},Area:{11x3;7x-1;False;Maze;;;},Area:{13x4;19x4;False;Maze;;;},Area:{15x4;27x11;False;Maze;;;},Area:{3x4;17x10;False;Maze;;;},Area:{13x4;14x3;False;Maze;;;},Area:{6x4;24x9;False;Maze;;;},Area:{14x2;1x13;False;Maze;;;},Area:{4x3;9x4;False;Maze;;;}]}";
            yield return "Area:{49x42;0x0;False;Maze;;;[Area:{11x2;19x1;False;Maze;;;},Area:{6x8;34x31;False;Maze;;;},Area:{13x12;15x29;False;Maze;;;},Area:{5x12;15x11;False;Maze;;;},Area:{12x6;23x5;False;Maze;;;},Area:{9x2;3x17;False;Maze;;;},Area:{15x4;16x6;False;Maze;;;},Area:{9x11;15x29;False;Maze;;;},Area:{6x4;39x37;False;Maze;;;},Area:{12x9;28x22;False;Maze;;;},Area:{6x3;31x8;False;Maze;;;},Area:{9x5;27x23;False;Maze;;;},Area:{13x9;0x27;False;Maze;;;},Area:{5x11;11x17;False;Maze;;;},Area:{1x1;4x2;False;Maze;;;}]}";
            yield return "Area:{24x46;0x0;False;Maze;;;[Area:{3x9;11x20;False;Maze;;;},Area:{6x10;5x15;False;Maze;;;},Area:{7x13;5x14;False;Maze;;;},Area:{5x11;10x26;False;Maze;;;},Area:{1x4;10x18;False;Maze;;;},Area:{1x5;11x13;False;Maze;;;},Area:{6x9;10x29;False;Maze;;;},Area:{2x10;13x8;False;Maze;;;},Area:{7x13;3x17;False;Maze;;;},Area:{5x5;5x28;False;Maze;;;},Area:{5x12;18x8;False;Maze;;;}]}";
            yield return "Area:{10x10;0x0;False;Maze;;;[Area:{2x3;7x9;False;Maze;;;},Area:{3x2;7x2;False;Maze;;;},Area:{2x2;3x9;False;Maze;;;},Area:{2x3;0x5;False;Maze;;;},Area:{2x3;7x1;False;Maze;;;},Area:{6x5;0x0;False;Maze;;;}]}";
        }
    }
}
