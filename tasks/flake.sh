#!/bin/sh

set -e

dotnet build maze-gen.sln -c Debug

while dotnet test maze-gen.sln -c Debug --no-build "$@"; do :; done
