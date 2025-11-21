#!/usr/bin/env python3
"""Convert the workspace logo PNG into a Windows ICO using Pillow."""

from __future__ import annotations

import argparse
import sys
from pathlib import Path
from typing import Iterable, Sequence, Tuple

from PIL import Image

SCRIPT_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPT_DIR.parent
DEFAULT_INPUT = REPO_ROOT / "Resources" / "Images" / "Logo.png"
DEFAULT_OUTPUT = REPO_ROOT / "Resources" / "Images" / "trayicon.ico"
DEFAULT_SIZES: Tuple[int, ...] = (16, 24, 32, 48, 64, 128, 256)


def _parse_sizes(values: Sequence[int]) -> Tuple[int, ...]:
    """Normalize size integers and ensure they are usable for ICO export."""
    unique = sorted({int(value) for value in values if int(value) > 0})
    if not unique:
        raise ValueError("At least one positive size is required.")
    return tuple(unique)


def _build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Convert a PNG logo into an ICO file with multiple resolutions",
    )
    parser.add_argument(
        "--input",
        "-i",
        dest="input_path",
        type=Path,
        default=DEFAULT_INPUT,
        help=f"PNG source image (default: {DEFAULT_INPUT})",
    )
    parser.add_argument(
        "--output",
        "-o",
        dest="output_path",
        type=Path,
        default=DEFAULT_OUTPUT,
        help=f"ICO output path (default: {DEFAULT_OUTPUT})",
    )
    parser.add_argument(
        "--sizes",
        nargs="+",
        type=int,
        default=DEFAULT_SIZES,
        metavar="N",
        help="Icon edge sizes in pixels (default: %(default)s)",
    )
    return parser


def _save_icon(source: Image.Image, target: Path, sizes: Iterable[int]) -> None:
    icon_sizes = [(size, size) for size in sizes]
    target.parent.mkdir(parents=True, exist_ok=True)
    source.save(target, format="ICO", sizes=icon_sizes)


def main(argv: Sequence[str] | None = None) -> int:
    parser = _build_arg_parser()
    args = parser.parse_args(argv)

    try:
        sizes = _parse_sizes(args.sizes)
    except ValueError as exc:
        parser.error(str(exc))
        return 2  # pragma: no cover

    input_path = args.input_path
    output_path = args.output_path

    if not input_path.exists():
        parser.error(f"Input file not found: {input_path}")

    with Image.open(input_path) as image:
        icon_source = image.convert("RGBA")
        _save_icon(icon_source, output_path, sizes)

    print(f"Tray icon created at {output_path} from {input_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
