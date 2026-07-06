#!/bin/sh

# NOT YET MIGRATED off Mono. This script still uses AltCover/ReportGenerator/
# Gendarme via `mono` and the old packages.config layout, which no longer exist
# after the net8.0 migration. Rework needed: `dotnet test --collect:"XPlat Code
# Coverage"` (coverlet) + ReportGenerator as a dotnet tool. See CLAUDE.md.

set -e

AltCover="$(realpath ./packages/altcover.8.8.74/tools/net472/AltCover.exe)"
ReportGenerator="$(realpath ./packages/ReportGenerator.5.3.5/tools/net47/ReportGenerator.exe)"
gendarme="$(realpath ./packages/altcode.gendarme.2023.12.27.19054/tools/gendarme.exe)"
nunit="$(realpath ./packages/NUnit.ConsoleRunner.3.17.0/tools/nunit3-console.exe)"

# Check if --where parameter is provided
if [ "$#" -gt 0 ]; then
  # Extract the --where parameter
  where_param="$1"
  shift
else
  # Default to "Category!=Load AND Category!=Integration"
  where_param="Category!=Load AND Category!=Integration"
fi

echo $where_param
cd build/Debug/tests
mono $AltCover --assemblyFilter=Moq --typeFilter=PlayersWorlds.Maps.Areas.Evolving.VectorDistanceForceProducer --typeFilter=PlayersWorlds.Maps.Maze.MazeBuildingException --methodFilter=GetEnumerator --pathFilter=./src/renderers --outputDirectory=__Instrumented
mono $AltCover Runner --recorderDirectory __Instrumented --cobertura=../../coverage.cobertura.xml --executable "$nunit" -- __Instrumented/PlayersWorlds.Maps.Tests.dll --framework=mono-4.0 --where="$where_param"
cd ../../..
mono $ReportGenerator -reports:build/coverage.cobertura.xml -targetdir:build/coverage -reporttypes:TextSummary -assemblyfilters:-AltCover.Monitor\;-PlayersWorlds.Maps.Tests
mono $nunit build/Debug/tests/PlayersWorlds.Maps.Tests.dll --framework=mono-4.0 --where="$where_param"
for cs_file in $(find src/ -name '*.cs'); do test_file=$(echo $cs_file | sed -e 's/.cs$/Test.cs/' -e 's/^src/tests/'); [ ! -e "$test_file" ] && printf '%s\n' "MISSING TEST CLASS FOR $cs_file"; done
mono $gendarme build/Debug/PlayersWorlds.Maps/PlayersWorlds.Maps.dll --html build/gendarme.html || true
cat build/coverage/Summary.txt
