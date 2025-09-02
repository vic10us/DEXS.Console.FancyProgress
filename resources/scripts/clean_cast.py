#!/usr/bin/env python3
"""Clean an asciinema .cast recording by removing unwanted control-only events.

Features:
  * Removes specific escape sequence output events (alternate screen/app mode + cursor show)\n
  * Trims trailing pure newline output events ("\n", "\r", "\r\n")
  * Re-bases timestamps to start at 0 (unless --disable-rebase provided)
  * Falls back to copying raw file unchanged if filtering would produce an empty event list

Usage:
  python clean_cast.py --input raw.cast --output cleaned.cast [--disable-rebase]

Environment variables (optional):
  EXTRA_FILTER: comma-separated additional exact output strings to remove.

Exit codes:
  0 on success, non-zero on error.
"""
from __future__ import annotations

import argparse
import json
import os
import shutil
import sys
from pathlib import Path
from typing import List


DEFAULT_UNWANTED = {"\u001b[?1h\u001b=", "\u001b[?25h"}


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description="Clean an asciinema .cast recording")
    p.add_argument("--input", "-i", required=True, help="Path to raw .cast file")
    p.add_argument("--output", "-o", required=True, help="Path to write cleaned .cast file")
    p.add_argument("--disable-rebase", action="store_true", help="Do not rebase timestamps to start at 0")
    return p.parse_args()


def load_lines(path: Path) -> List[str]:
    try:
        return path.read_text(encoding="utf-8").splitlines()
    except FileNotFoundError:
        sys.exit(f"Input file not found: {path}")
    except Exception as e:
        sys.exit(f"Failed reading {path}: {e}")


def main() -> int:
    args = parse_args()
    in_path = Path(args.input)
    out_path = Path(args.output)

    lines = load_lines(in_path)
    if not lines:
        sys.exit("Empty cast file")

    header = lines[0]
    # Validate header JSON object
    try:
        json.loads(header)
    except Exception:
        print("Warning: header line not valid JSON object", file=sys.stderr)

    extra = {s for s in os.environ.get("EXTRA_FILTER", "").split(",") if s}
    unwanted = DEFAULT_UNWANTED | extra

    events_raw = lines[1:]
    parsed = []
    for ln in events_raw:
        ln = ln.strip()
        if not ln:
            continue
        try:
            evt = json.loads(ln)
        except json.JSONDecodeError:
            # skip malformed
            continue
        if not (isinstance(evt, list) and len(evt) == 3):
            continue
        t, kind, data = evt
        if kind == 'o' and data in unwanted:
            continue
        parsed.append(evt)

    # Trim trailing newline-only events
    while parsed and parsed[-1][1] == 'o' and parsed[-1][2] in ('\n', '\r', '\r\n'):
        parsed.pop()

    if not parsed:
        # Nothing left; preserve original file instead of creating empty one
        shutil.copyfile(in_path, out_path)
        return 0

    if not args.disable_rebase:
        first_time = parsed[0][0]
        try:
            base = float(first_time)
        except Exception:
            base = 0.0
        for evt in parsed:
            try:
                evt[0] = round(float(evt[0]) - base, 6)
            except Exception:
                pass

    with out_path.open('w', encoding='utf-8') as f:
        f.write(header.rstrip('\n') + '\n')
        for evt in parsed:
            f.write(json.dumps(evt, ensure_ascii=False) + '\n')
    return 0


if __name__ == "__main__":  # pragma: no cover
    sys.exit(main())
