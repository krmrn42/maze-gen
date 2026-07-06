#!/bin/sh

# NOT YET MIGRATED off Mono. Uses the Mono soft-debugger agent against a
# `maze-gen.exe` that the net8.0 build no longer produces. Rework needed:
# `dotnet run` under vsdbg/netcoredbg (the VS Code C# debugger attaches directly).

set -e

msbuild

mono --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555 ./build/Debug/mazegen/maze-gen.exe run -p "$@"