using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps {
    public class Test {
        private readonly Dictionary<string, Stopwatch> _runningTests =
            new Dictionary<string, Stopwatch>();

        private RandomSource _randomSource;

        protected RandomSource RandomSource => _randomSource;

        protected int DebugLevel => TestLog.DebugLevel;

        [SetUp]
        public virtual void SetUp() {
            if (TestLog.DebugLevel > 1) {
                TestContext.Progress.WriteLine("Running : " +
                    TestContext.CurrentContext.Test.FullName + "...");
            }
            _runningTests.Add(TestContext.CurrentContext.Test.ID,
                            Stopwatch.StartNew());
            _randomSource = RandomSource.CreateFromEnv();
            TestSetup();
        }

        [TearDown]
        public virtual void TearDown() {
            TestTearDown();
            var sw = _runningTests[TestContext.CurrentContext.Test.ID];
            _runningTests.Remove(TestContext.CurrentContext.Test.ID);
            sw.Stop();
            var duration = sw.Elapsed;
            if (TestLog.DebugLevel > 0 && duration.TotalMilliseconds > 200) {
                TestContext.Progress.WriteLine("Completed : " +
                    TestContext.CurrentContext.Test.FullName + ": " +
                    duration.ToString());
            }
        }

        protected virtual void TestSetup() { }
        protected virtual void TestTearDown() { }

        protected Area Env10(params string[] tags) =>
            Area.CreateMaze(new Vector(10, 10), tags);

        protected Area Hall(int x, int y, int w, int h) =>
            Area.Create(new Vector(x, y), new Vector(w, h), AreaType.Hall);
    }
}
