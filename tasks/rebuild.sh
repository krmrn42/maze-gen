#!/bin/sh

set -e

"$(dirname "$0")/clean.sh"
dotnet build maze-gen.sln -c Debug
