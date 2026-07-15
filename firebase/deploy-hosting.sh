#!/usr/bin/env bash
# Assemble a Firebase Hosting folder from a Unity WebGL build, then deploy it.
# Usage:  deploy-hosting.sh [task1|task2]      (default: task1)
#
#   task1  ->  Build/WebGL-Task1  ->  public/        ->  target "task1"  (adeeb-booklab-07111926.web.app)
#   task2  ->  Build/WebGL-Task2  ->  public-task2/  ->  target "task2"  (adeeb-booklab-task2.web.app)
#
# ONE-TIME setup for task2 (needs `firebase login`):
#   firebase hosting:sites:create adeeb-booklab-task2
#   (the task1/task2 -> site mapping already lives in .firebaserc)
#
# Why we decompress: Firebase Hosting strips "Content-Encoding: gzip" from *.js and caches
# encodings inconsistently across its CDN. So we ship the Unity payload UNcompressed and let
# Firebase gzip it natively over the wire — which it does reliably. Renaming (dropping .gz)
# also busts any stale cached copies. No rebuild needed.
set -e
cd "$(dirname "$0")/.."

TASK="${1:-task1}"
case "$TASK" in
  task1) BUILD_DIR="Build/WebGL-Task1"; PUBLIC_DIR="public";       TARGET="task1" ;;
  task2) BUILD_DIR="Build/WebGL-Task2"; PUBLIC_DIR="public-task2"; TARGET="task2" ;;
  *) echo "usage: $0 [task1|task2]"; exit 1 ;;
esac

if [ ! -d "$BUILD_DIR" ]; then
  echo "No $BUILD_DIR found. In Unity run the matching 'BookLab > Build WebGL - ...' menu item, then re-run this."
  exit 1
fi

echo "==> [$TASK] Assembling $PUBLIC_DIR/ from $BUILD_DIR ..."
mkdir -p "$PUBLIC_DIR"
rm -rf "$PUBLIC_DIR/Build" "$PUBLIC_DIR/TemplateData" "$PUBLIC_DIR/index.html"
cp -r "$BUILD_DIR/index.html" "$BUILD_DIR/Build" "$BUILD_DIR/TemplateData" "$PUBLIC_DIR/"

echo "==> Decompressing Unity payload (Firebase gzips it natively) ..."
# Prefix-agnostic: Unity names the files after the build folder (WebGL.* for task1,
# WebGL-Task2.* for task2), so just decompress every .gz that's there.
for gz in "$PUBLIC_DIR"/Build/*.gz; do
  [ -e "$gz" ] || continue
  gzip -dc "$gz" > "${gz%.gz}"
  rm "$gz"
done
# point index.html at the uncompressed files (strip ".gz" before each closing quote)
sed -i 's#\.gz"#"#g' "$PUBLIC_DIR/index.html"

echo "==> Deploying hosting target: $TARGET ..."
firebase deploy --only "hosting:$TARGET"
