#!/bin/sh

set -e

dotnet run --project maze-gen -c Debug -- run "$@"
