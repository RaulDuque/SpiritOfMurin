"""Check a 64x64 character sprite against the asset spec. Prints observations, not a verdict.

Usage: python tools/asset-pipeline/validate_sprite.py <png> [support_foot|between_feet]

Observes: size, binary alpha, every opaque colour in pal_master_64, figure height and rows,
what sits under the pivot (32, 59), outline colour inside the figure, and whether the
blade pixels form one connected piece.
"""
import sys
from pathlib import Path

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
GPL = ROOT / "assets/palettes/pal_master_64.gpl"


def palette():
    pal = {}
    for line in GPL.read_text(encoding="utf-8").splitlines():
        parts = line.split("\t")
        if len(parts) == 2 and parts[0].strip()[:1].isdigit():
            pal[tuple(int(v) for v in parts[0].split())] = parts[1].split(": ", 1)[1]
    return pal


def components(mask):
    seen = np.zeros_like(mask, bool)
    out = []
    for y, x in zip(*np.where(mask)):
        if seen[y, x]:
            continue
        stack, comp = [(y, x)], []
        seen[y, x] = True
        while stack:
            cy, cx = stack.pop(); comp.append((cy, cx))
            for dy in (-1, 0, 1):
                for dx in (-1, 0, 1):
                    ny, nx = cy + dy, cx + dx
                    if 0 <= ny < mask.shape[0] and 0 <= nx < mask.shape[1] and mask[ny, nx] and not seen[ny, nx]:
                        seen[ny, nx] = True; stack.append((ny, nx))
        out.append(comp)
    return out


def main(path, mode):
    pal = palette()
    im = Image.open(path)
    obs = [f"file: {path}", f"size: {im.size[0]}x{im.size[1]} (spec 64x64)", f"mode: {im.mode}"]
    a = np.asarray(im.convert("RGBA"))
    alpha = a[..., 3]
    vals = sorted(set(alpha.flatten().tolist()))
    obs.append(f"alpha values: {vals} (spec: only 0 and 255)")
    opaque = alpha == 255
    cols = {}
    for y, x in zip(*np.where(opaque)):
        c = tuple(int(v) for v in a[y, x, :3])
        cols[c] = cols.get(c, 0) + 1
    off = {c: n for c, n in cols.items() if c not in pal}
    obs.append(f"colours used: {len(cols)}; outside pal_master_64: {len(off)} {off if off else ''}")
    obs.append("colour counts: " + ", ".join(f"{pal.get(c, '?')} {n}" for c, n in sorted(cols.items(), key=lambda kv: -kv[1])))
    contact = (38, 44, 33)
    body = opaque & ~np.all(a[..., :3] == contact, axis=2)
    rows = np.where(body.any(1))[0]
    obs.append(f"figure rows (without contact shade): {rows.min()}-{rows.max()}; "
               f"height to the pivot row 59: {59 - rows.min() + 1} px")
    r59 = np.where(body[59])[0].tolist()
    obs.append(f"row 59 opaque columns: {r59}")
    if mode == "support_foot":
        obs.append(f"pivot column 32 at row 59 is {'opaque (foot on the pivot)' if body[59, 32] else 'EMPTY'}; "
                   f"columns 31-33 opaque: {[c for c in (31, 32, 33) if body[59, c]]}")
    else:
        left = [c for c in r59 if c < 32]; right = [c for c in r59 if c > 32]
        obs.append(f"pivot column 32 at row 59 is {'EMPTY (ground between the feet)' if not body[59, 32] else 'opaque'}; "
                   f"left foot {min(left) if left else '-'}-{max(left) if left else '-'}, "
                   f"right foot {min(right) if right else '-'}-{max(right) if right else '-'}; "
                   f"midpoint {(max(left) + min(right)) / 2 if left and right else '-'}" if left and right else "feet not found on row 59")
    below = np.where(body[60:].any(1))[0]
    obs.append(f"body pixels below row 59: rows {(below + 60).tolist() if len(below) else 'none'}")
    cr = np.where(np.all(a[..., :3] == contact, axis=2) & opaque)
    obs.append(f"contact shade pixels: {len(cr[0])} on rows {sorted(set(cr[0].tolist()))}")
    outline = np.all(a[..., :3] == (22, 20, 15), axis=2) & opaque
    inner = []
    for y, x in zip(*np.where(outline)):
        nb = [(y + dy, x + dx) for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1))]
        if all(0 <= ny < 64 and 0 <= nx < 64 and opaque[ny, nx] for ny, nx in nb):
            inner.append((int(x), int(y)))
    obs.append(f"outline pixels with no transparent 4-neighbour (inside the figure): {len(inner)}")
    steel = np.zeros_like(opaque)
    for c in ((140, 146, 146), (180, 185, 183), (214, 217, 211)):
        steel |= np.all(a[..., :3] == c, axis=2) & opaque
    comps = components(steel)
    obs.append(f"blade (steel pixels): {int(steel.sum())} px in {len(comps)} connected piece(s); "
               + "; ".join(f"cols {min(p[1] for p in c)}-{max(p[1] for p in c)} rows {min(p[0] for p in c)}-{max(p[0] for p in c)}" for c in comps))
    print("\n".join(obs))


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else "support_foot")
