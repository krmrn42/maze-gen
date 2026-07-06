#!/bin/sh

# NOT YET MIGRATED off Mono. Uses the Mono log profiler + mprof-report against a
# `maze-gen.exe` that the net8.0 build no longer produces. Rework needed:
# `dotnet-trace` (or dotnet run + BenchmarkDotNet) over `mazegen perfrun`.

set -e

mono --profile=log:calls,alloc,output=output.mlpd,maxframes=4,calldepth=5 build/Debug/mazegen/maze-gen.exe perfrun
mprof-report --out=build/perf.txt output.mlpd