#!/bin/sh

set -e

find src tests maze-gen -type d \( -name bin -o -name obj \) -exec rm -rf {} +
rm -rf build
