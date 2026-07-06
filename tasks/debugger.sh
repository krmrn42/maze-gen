#!/bin/sh

set -e

# Launch maze-gen under a managed (.NET) debugger for the given CLI args.
#
# In an editor: use the VS Code "Debug maze-gen" launch configs (F5) — the C#
# extension (ms-dotnettools.csharp) ships the CoreCLR debugger. That is the
# primary, supported path (Microsoft's vsdbg is licensed to VS / VS Code only).
#
# From the terminal: this script drives netcoredbg
# (https://github.com/Samsung/netcoredbg), the open-source CoreCLR debugger.
#
# Usage: ./tasks/debugger.sh run -p PlayersWorlds.Maps.Maze.Maze2DTest.SomeTest

dotnet build maze-gen.sln -c Debug
dll="maze-gen/bin/Debug/net8.0/maze-gen.dll"

if command -v netcoredbg >/dev/null 2>&1; then
  exec netcoredbg --interpreter=cli -- dotnet "$dll" "$@"
fi

echo "netcoredbg not found on PATH."
echo
echo "  Terminal debugging: install netcoredbg, then re-run this script, e.g."
echo "    ./tasks/debugger.sh run -p PlayersWorlds.Maps.Maze.Maze2DTest.SomeTest"
echo
echo "  Editor debugging:   open the repo in VS Code and press F5"
echo "    (\"Debug maze-gen: run test\" / \"Debug maze-gen: generate\")."
exit 1
