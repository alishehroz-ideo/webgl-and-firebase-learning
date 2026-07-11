#!/usr/bin/env bash
# Assemble the Firebase Hosting folder (public/) from the Unity WebGL build + our
# image assets, then deploy. Run after "BookLab > Build WebGL" in Unity.
#
# Firebase Hosting mishandles PRE-gzipped Unity files: it strips "Content-Encoding:
# gzip" from *.js, and caches encodings inconsistently across the CDN. So we ship the
# Unity payload UNcompressed (.wasm/.data/.framework.js) and let Firebase compress it
# natively over the wire — which it does reliably. Renaming (dropping .gz) also busts
# any stale cached copies. No rebuild needed.
set -e
cd "$(dirname "$0")/.."

if [ ! -d "Build/WebGL" ]; then
  echo "No Build/WebGL found. In Unity run: BookLab > Build WebGL, then re-run this."
  exit 1
fi

echo "==> Assembling public/ (keeping public/assets) ..."
rm -rf public/Build public/TemplateData public/index.html
cp -r Build/WebGL/index.html Build/WebGL/Build Build/WebGL/TemplateData public/

echo "==> Decompressing Unity payload (Firebase gzips it natively) ..."
for f in WebGL.wasm WebGL.data WebGL.framework.js; do
  if [ -f "public/Build/$f.gz" ]; then
    gzip -dc "public/Build/$f.gz" > "public/Build/$f"
    rm "public/Build/$f.gz"
  fi
done
# point index.html at the uncompressed files (strip ".gz" before each closing quote)
sed -i 's#\.gz"#"#g' public/index.html

echo "==> Deploying ..."
firebase deploy --only hosting
