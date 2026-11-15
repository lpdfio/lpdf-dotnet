#!/bin/sh
# Fails unless the package in nupkg/ carries the file of the licenses of the crates the engine links, which
# their licenses ask to travel with the compiled engine. The core release attaches the file beside the engine.
# Run from the repo root after `dotnet pack Lpdf.csproj -c Release --no-build -o nupkg`.
set -eu

package=$(ls nupkg/*.nupkg)

if ! unzip -Z1 "$package" | grep -q -x 'THIRD_PARTY_LICENSES_ENGINE.txt'; then
  echo "::error::$package has no THIRD_PARTY_LICENSES_ENGINE.txt at its root. The core release carries it beside lpdf-wasi.wasm."
  exit 1
fi
echo "$package carries THIRD_PARTY_LICENSES_ENGINE.txt."
