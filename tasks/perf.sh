#!/bin/sh

set -e

# Performance baseline on .NET 8. Runs the `perfrun` workload (every maze
# TestFixture instantiated and each [Test] invoked by reflection) under
# dotnet-trace, and reports wall-clock time.
#
# Usage: ./tasks/perf.sh
#
# Outputs (under build/perf/):
#   perfrun.nettrace           raw EventPipe trace (open in PerfView / VS)
#   perfrun.speedscope.json    flame graph — drop into https://speedscope.app
#   perfrun.log                captured stdout of the run

out="build/perf"
rm -rf "$out"
mkdir -p "$out"

dotnet tool restore
dotnet build maze-gen.sln -c Release

dll="maze-gen/bin/Release/net8.0/maze-gen.dll"

start=$(date +%s)
# `dotnet dotnet-trace` (not `dotnet tool run`) so the child-launch `--` is
# forwarded to the tool rather than consumed by the SDK.
dotnet dotnet-trace collect \
  --output "$out/perfrun.nettrace" \
  --format Speedscope \
  -- dotnet "$dll" perfrun | tee "$out/perfrun.log"
end=$(date +%s)

echo "----------------------------------------------------------------"
echo "perfrun wall-clock: $((end - start))s"
echo "trace:      $out/perfrun.nettrace"
echo "flamegraph: $out/perfrun.speedscope.json  (open at https://speedscope.app)"
