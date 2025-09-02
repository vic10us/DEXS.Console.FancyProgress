#!/usr/bin/env bash
set -euo pipefail

export TERM=xterm-truecolor

# Resolve paths relative to this script (no fragile realpath dependency)
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
export SCRIPT_DIR
SOLUTION_DIR="$( cd "$SCRIPT_DIR/../.." && pwd )"
PROJECT_DIR="$SOLUTION_DIR/src/DEXS.Console.FancyProgress.SampleConsole"
GFX_DIR="$SOLUTION_DIR/resources/gfx"

# Force a simple "$" prompt just for this script/session
export PS1="$ "

# Make sure output directory exists
mkdir -p "$GFX_DIR"

# Build quietly (no output)
dotnet build "$PROJECT_DIR" -c Release -v q >/dev/null

# Timestamped filenames
timestamp=$(date +%Y%m%d%H%M%S)
raw_cast="$GFX_DIR/demo-${timestamp}-raw.cast"
clean_cast="$GFX_DIR/demo-${timestamp}.cast"
giffile="$GFX_DIR/demo-${timestamp}.gif"

# Allow using existing cast: if env RAW_INPUT_CAST points to a file, skip recording
if [[ -n "${RAW_INPUT_CAST:-}" && -f "${RAW_INPUT_CAST}" ]]; then
  echo "Using existing cast at $RAW_INPUT_CAST"
  cp "$RAW_INPUT_CAST" "$raw_cast"
else
  # Record raw cast (only if asciinema available)
  # echo dotnet build --configuration Release --project $PROJECT_DIR
  # dotnet build --configuration Release $PROJECT_DIR
  if command -v asciinema >/dev/null 2>&1; then
    asciinema rec "$raw_cast" -c "dotnet run --no-build --configuration Release --project \"$PROJECT_DIR\""
  else
    echo "ERROR: asciinema not found in PATH. Install it (https://docs.asciinema.org/manual/cli/installation/) or set RAW_INPUT_CAST to an existing .cast file." >&2
    exit 1
  fi
fi

# Clean the raw cast using external Python helper (falls back to raw if python missing)
CLEANER_SCRIPT="$SCRIPT_DIR/clean_cast.py"
if command -v python3 >/dev/null 2>&1; then
  python3 "$CLEANER_SCRIPT" --input "$raw_cast" --output "$clean_cast" ${DISABLE_REBASE:+--disable-rebase}
else
  echo "python3 not found; copying raw cast without cleaning" >&2
  cp "$raw_cast" "$clean_cast"
fi

# Convert to GIF using kayvan/agg (expects the cleaned cast)
#if command -v docker >/dev/null 2>&1; then
#  docker run --rm -v "$GFX_DIR:/data" kayvan/agg \
#    "/data/$(basename "$clean_cast")" "/data/$(basename "$giffile")" \
#    --cols 80 --rows 3
#else
#  echo "WARNING: docker not found; skipping GIF conversion. Cast available at $clean_cast" >&2
#fi

agg --cols 80 --rows 3 $clean_cast $giffile

echo "Clean cast: $clean_cast"
echo "GIF created: $giffile"
