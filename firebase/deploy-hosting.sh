#!/usr/bin/env bash
# Assemble the Firebase Hosting folder (public/) from the Unity WebGL build + our
# image assets, then deploy. Run after "BookLab > Build WebGL" in Unity.
#
# Firebase Hosting keeps "Content-Encoding: gzip" for binary files (.wasm/.data) but
# will NOT keep it for the pre-gzipped framework (it manages JS compression itself).
# Fix: ship the framework UNcompressed and let Firebase gzip it natively over the wire;
# keep wasm/data pre-gzipped (served with Content-Encoding via firebase.json). Result:
# every file loads correctly, no rebuild needed.
set -e
cd "$(dirname "$0")/.."

if [ ! -d "Build/WebGL" ]; then
  echo "No Build/WebGL found. In Unity run: BookLab > Build WebGL, then re-run this."
  exit 1
fi

echo "==> Assembling public/ (keeping public/assets) ..."
rm -rf public/Build public/TemplateData public/index.html
cp -r Build/WebGL/index.html Build/WebGL/Build Build/WebGL/TemplateData public/

if [ -f "public/Build/WebGL.framework.js.gz" ]; then
  echo "==> Decompressing framework so Firebase serves + gzips it as normal JS ..."
  gzip -dc public/Build/WebGL.framework.js.gz > public/Build/WebGL.framework.js
  rm public/Build/WebGL.framework.js.gz
  sed -i 's#WebGL.framework.js.gz#WebGL.framework.js#' public/index.html
fi

echo "==> Deploying ..."
firebase deploy --only hosting
