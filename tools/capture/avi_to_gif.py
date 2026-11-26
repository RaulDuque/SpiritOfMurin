"""Turns a Godot movie-writer AVI (MJPEG) into an animated GIF, keeping pixel art crisp.

Usage: python tools/capture/avi_to_gif.py in.avi out.gif [--scale 0.5] [--every 1] [--fps 30]
Needs Pillow. Reads the AVI's 00dc/00db JPEG frames directly (no ffmpeg needed).
"""
import io
import struct
import sys

from PIL import Image


def frames(path):
    data = open(path, "rb").read()
    i = 0
    while True:
        i = min((j for j in (data.find(b"00dc", i), data.find(b"00db", i)) if j >= 0), default=-1)
        if i < 0:
            return
        size = struct.unpack("<I", data[i + 4:i + 8])[0]
        chunk = data[i + 8:i + 8 + size]
        if chunk[:2] == b"\xff\xd8":
            yield Image.open(io.BytesIO(chunk)).convert("RGB")
        i += 8 + size


def main():
    args = sys.argv[1:]
    src, dst = args[0], args[1]
    scale = float(args[args.index("--scale") + 1]) if "--scale" in args else 0.5
    every = int(args[args.index("--every") + 1]) if "--every" in args else 1
    fps = float(args[args.index("--fps") + 1]) if "--fps" in args else 30
    out = []
    for n, im in enumerate(frames(src)):
        if n % every:
            continue
        if scale != 1:
            im = im.resize((int(im.width * scale), int(im.height * scale)), Image.NEAREST)
        out.append(im.quantize(colors=128, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE))
    if not out:
        sys.exit("no frames found")
    out[0].save(dst, save_all=True, append_images=out[1:], duration=int(1000 * every / fps), loop=0, optimize=True)
    print(f"{len(out)} frames -> {dst}")


if __name__ == "__main__":
    main()
