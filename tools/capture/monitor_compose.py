"""Evidence images without capturing the desktop.

A capture of the screen can show other windows (the test runs beside the person's own work), so the
full-monitor evidence is rebuilt from the game's own picture and the measure the game printed:

  python tools/capture/monitor_compose.py monitor GAME.png OUT.png --monitor 1600x900 --at 160,90 --scale 2 --caption "..."
      the monitor (black), the game's picture scaled by whole pixels at its measured place, and a caption
      strip under it (outside the monitor area)
  python tools/capture/monitor_compose.py caption IN.png OUT.png --caption "..."
      adds a caption strip under an image (real captures of the editor)
  python tools/capture/monitor_compose.py side LEFT.png RIGHT.png OUT.png --left "..." --right "..." [--crop-left x,y,w,h]
      two pictures side by side at the same height, each with its title (prototype × Godot)

Needs Pillow.
"""
import sys

from PIL import Image, ImageDraw, ImageFont


def font(size):
    for name in ("consola.ttf", "cour.ttf", "DejaVuSansMono.ttf"):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            pass
    return ImageFont.load_default()


def opt(args, name, default=None):
    return args[args.index(name) + 1] if name in args else default


def with_caption(img, text, size=22):
    lines = text.split("|")
    h = 14 + len(lines) * (size + 6)
    out = Image.new("RGB", (img.width, img.height + h), (24, 22, 18))
    out.paste(img, (0, 0))
    d = ImageDraw.Draw(out)
    f = font(size)
    for i, line in enumerate(lines):
        d.text((12, img.height + 8 + i * (size + 6)), line.strip(), fill=(236, 229, 208), font=f)
    return out


def main():
    a = sys.argv[1:]
    mode = a[0]
    if mode == "monitor":
        game, out = Image.open(a[1]).convert("RGB"), a[2]
        mw, mh = (int(v) for v in opt(a, "--monitor").split("x"))
        x, y = (int(v) for v in opt(a, "--at", "0,0").split(","))
        k = int(opt(a, "--scale", "1"))
        mon = Image.new("RGB", (mw, mh), (0, 0, 0))
        mon.paste(game.resize((game.width * k, game.height * k), Image.NEAREST), (x, y))
        with_caption(mon, opt(a, "--caption", "")).save(out)
    elif mode == "caption":
        with_caption(Image.open(a[1]).convert("RGB"), opt(a, "--caption", "")).save(a[2])
    elif mode == "side":
        left, right, out = Image.open(a[1]).convert("RGB"), Image.open(a[2]).convert("RGB"), a[3]
        if "--crop-left" in a:
            cx, cy, cw, ch = (int(v) for v in opt(a, "--crop-left").split(","))
            left = left.crop((cx, cy, cx + cw, cy + ch))
        h = max(left.height, right.height) * 2
        left = left.resize((left.width * h // left.height, h), Image.NEAREST)
        right = right.resize((right.width * h // right.height, h), Image.NEAREST)
        title = 40
        img = Image.new("RGB", (left.width + right.width + 16, h + title), (24, 22, 18))
        img.paste(left, (0, title)); img.paste(right, (left.width + 16, title))
        d = ImageDraw.Draw(img); f = font(24)
        d.text((10, 8), opt(a, "--left", "Protótipo HTML"), fill=(236, 229, 208), font=f)
        d.text((left.width + 26, 8), opt(a, "--right", "Godot"), fill=(236, 229, 208), font=f)
        img.save(out)


if __name__ == "__main__":
    main()
