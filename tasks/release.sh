#!/usr/bin/env bash
# Cut a release: bump the version, create an annotated git tag, and push it.
# CI (.github/workflows/publish.yml) packs and publishes to NuGet on the tag.
#
# Usage: tasks/release.sh <patch|minor|major>   (default: minor)
# The tag is pushed to the remote that hosts the publishing repo (krmrn42/
# maze-gen), since its GitHub Actions do the publish. Override with RELEASE_REMOTE.
set -euo pipefail

bump="${1:-minor}"
case "$bump" in
  patch|minor|major) ;;
  *) echo "Unknown bump '$bump' (use patch|minor|major)." >&2; exit 1 ;;
esac

# Pick the remote pointing at the publishing repo; fall back to origin.
remote="${RELEASE_REMOTE:-$(git remote -v | awk '/krmrn42\/maze-gen/ {print $1; exit}')}"
remote="${remote:-origin}"

if [ -n "$(git status --porcelain)" ]; then
  echo "Working tree is not clean; commit or stash before releasing." >&2
  exit 1
fi

branch="$(git rev-parse --abbrev-ref HEAD)"
if [ "$branch" != "main" ]; then
  echo "Warning: releasing from '$branch', not 'main'." >&2
fi

git fetch --tags "$remote" >/dev/null 2>&1 || true
latest="$(git tag --list 'v*' --sort=-v:refname | head -n1)"
latest="${latest:-v0.0.0}"

ver="${latest#v}"
major="${ver%%.*}"; rest="${ver#*.}"
minor="${rest%%.*}"; patch="${rest#*.}"
patch="${patch%%[-+]*}"   # drop any prerelease/build suffix

case "$bump" in
  major) major=$((major + 1)); minor=0; patch=0 ;;
  minor) minor=$((minor + 1)); patch=0 ;;
  patch) patch=$((patch + 1)) ;;
esac
new="v${major}.${minor}.${patch}"

if git rev-parse -q --verify "refs/tags/$new" >/dev/null; then
  echo "Tag $new already exists." >&2
  exit 1
fi

echo "Releasing $latest -> $new  (remote: $remote, branch: $branch)"
git tag -a "$new" -m "Release $new"
git push "$remote" "$new"
echo "Pushed $new. CI (publish.yml) will pack and publish $new to NuGet."
