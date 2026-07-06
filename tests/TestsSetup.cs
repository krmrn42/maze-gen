using System;
using System.Diagnostics;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PlayersWorlds.Maps;

[SetUpFixture]
public class TestsSetup {
    [OneTimeSetUp]
    public void RunBeforeAnyTests() {
        if (TestContext.Parameters.Exists("SEED")) {
            RandomSource.EnvRandomSeed = int.Parse(TestContext.Parameters["SEED"]);
        }
        if (TestContext.Parameters.Exists("DEBUG")) {
            Log.DebugLoggingLevel = TestLog.DebugLevel = int.Parse(TestContext.Parameters["DEBUG"]);
        }
        if (Log.DebugLoggingLevel > 0) {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests() {
        if (Log.DebugLoggingLevel > 0) {
            Trace.Flush();
        }
    }
}
