#!/bin/sh

set -e

# Code coverage on .NET 8 via coverlet (`dotnet test --collect`) + ReportGenerator.
# Exclusions live in tasks/coverlet.runsettings (mirrors the old AltCover filters).
#
# Usage:
#   ./tasks/coverage.sh                                          # unit tests (CI set)
#   ./tasks/coverage.sh "TestCategory!=Load"                     # custom filter
#
# Outputs (under build/coverage/):
#   report/Summary.txt   text summary (printed below)
#   report/index.html    browsable HTML report
#   report/Cobertura.xml Cobertura for CI / the Coverage Gutters extension
#
# Coverage goal is >=98%; the per-file test-class audit below prints
# `MISSING TEST CLASS FOR ...` for any src file lacking a matching *Test.cs.

filter="${1:-TestCategory!=Load & TestCategory!=Integration}"
out="build/coverage"

rm -rf "$out"
dotnet tool restore

dotnet test maze-gen.sln -c Debug \
  --filter "$filter" \
  --settings tasks/coverlet.runsettings \
  --results-directory "$out/raw"

dotnet tool run reportgenerator \
  "-reports:$out/raw/**/coverage.cobertura.xml" \
  "-targetdir:$out/report" \
  "-reporttypes:TextSummary;Html;Cobertura"

# One test class per source file (see CLAUDE.md conventions).
for cs_file in $(find src -name '*.cs'); do
  test_file=$(echo "$cs_file" | sed -e 's/\.cs$/Test.cs/' -e 's/^src/tests/')
  [ ! -e "$test_file" ] && printf '%s\n' "MISSING TEST CLASS FOR $cs_file"
done

cat "$out/report/Summary.txt"
