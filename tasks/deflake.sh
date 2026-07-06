#!/bin/sh

set -e

dotnet build maze-gen.sln -c Debug

until dotnet test maze-gen.sln -c Debug --no-build "$@"; do :; done
