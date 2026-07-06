#!/bin/sh

set -e

# Forwards all arguments to `dotnet test`. Examples:
#   ./tasks/test.sh --filter "TestCategory!=Load & TestCategory!=Integration"   # what CI runs
#   ./tasks/test.sh --filter "FullyQualifiedName~Maze2DTest"
# Reproduce a seed-sensitive failure (NUnit TestContext parameters):
#   ./tasks/test.sh --filter "FullyQualifiedName~Maze2DTest" \
#       -- 'TestRunParameters.Parameter(name="SEED", value="12345")'
dotnet test maze-gen.sln -c Debug "$@"
