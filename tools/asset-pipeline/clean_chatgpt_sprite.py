"""Clean a ChatGPT-generated character image into a 64x64 game sprite.

Reads the untouched original in assets/source/raw/ (never modified), works on
copies, and writes:
  assets/source/work/<name>_mask.png         background-removal mask (full size)
  assets/source/work/<name>_cutout_*.png     cut-out on light and dark grounds
  assets/art/characters/<name>.png           64x64, binary alpha, master palette only

Steps (spec: design/assets/specs/approval-poses-assets.md):
  1. Background: sample the flat grey on the borders, flood-fill the connected
     background from the edges, then remove enclosed holes that have the same
     flat grey (gaps between arm and torso). Everything else is kept, so the
     disciple's grey robe, the blade and the ribbon survive.
  2. Resample: each 64x64 cell takes the most frequent palette colour of the
     source pixels it covers (colours matched in CIE Lab against a per-character
     subset of pal_master_64). Scale is set by the character's height, and the
     pivot (32, 59) is placed on the support foot (swordsman) or on the ground
     point between the feet (disciple).
  3. Pixel-art rules: 1 px #16140f outline outside only, no outline colour
     inside, clean redrawn blade, 1 px contact shadow under each foot.

History (2026-11-25): the two sprites it made were rejected after the in-game comparison; the
originals stay in assets/source/raw/ and the outputs were moved to
assets/art/historico/2026-11-25-experimento-sprites/. Running it again writes fresh copies to OUT.

Usage:  python tools/asset-pipeline/clean_chatgpt_sprite.py [swordsman|disciple|all]
Requires Pillow and numpy.
"""
import sys
from collections import deque
from pathlib import Path

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
RAW = ROOT / "assets/source/raw"
WORK = ROOT / "assets/source/work"
OUT = ROOT / "assets/art/characters"
GPL = ROOT / "assets/palettes/pal_master_64.gpl"

SIZE = 64
PIVOT = (32, 59)


def load_palette():
    pal = {}
    for line in GPL.read_text(encoding="utf-8").splitlines():
        parts = line.split("\t")
        if len(parts) != 2 or not parts[0].strip()[:1].isdigit():
            continue
        rgb = tuple(int(v) for v in parts[0].split())
        name = parts[1].split(": ", 1)[1]
        pal[name] = rgb
    return pal


PAL = load_palette()
OUTLINE = PAL["contorno de ator"]
CONTACT = PAL["contato sob os pés"]

SPRITES = {
    "swordsman": dict(
        src="char_swordsman_idle_s_00.png",
        out="char_swordsman_idle_s_00.png",
        # spec ASSET-001: 46 px figure + bun (max 49), back-foot sole on row 59
        top_row=11,
        pivot="support_foot",
        colours=["contorno de ator", "pele sombra", "pele", "pele brilho",
                 "túnica sombra", "túnica tom", "túnica brilho", "faixa jade gasto sombra",
                 "faixa jade gasto", "calça sombra", "calça", "aço sombra", "aço", "fio e ponta",
                 "bronze da guarda"],
        # blade measured on the source (just past the guard -> tip), redrawn clean at 1:2
        # (~27 deg from vertical, tip down and out); tip lowered to 4 px above the sole (spec)
        blade=dict(start=(790, 690), tip=(925, 920), slope=(1, 2), tip_row=55,
                   edge_side=-1, outline="right"),
        # hand fixes after reading the 64x64 result (spec ASSET-001 checklist):
        # 1 px eyes, crossed collar as a 1 px diagonal in the tunic's dark tone, 4 px bronze guard
        pixels={
            "calça sombra": [(35, 22), (37, 22), (34, 26), (35, 26)],
            "túnica brilho": [(34, 27), (34, 28)],
            "túnica sombra": [(35, 27), (36, 28), (36, 29), (37, 30), (37, 31)],
            "bronze da guarda": [(43, 39), (44, 39), (45, 39), (46, 39)],
        },
    ),
    "disciple": dict(
        src="char_disciple_guard_s_00.png",
        out="char_disciple_guard_s_00.png",
        # spec ASSET-002: 42-43 px with hat (rows ~17-59), pivot between the feet
        top_row=17,
        pivot="between_feet",
        colours=["contorno de ator", "pele sombra", "pele", "pele brilho",
                 "corpo sombra", "corpo ardósia", "palha sombra", "palha", "palha brilho",
                 "calça sombra", "calça", "aço sombra", "aço", "fio e ponta", "bronze da guarda"],
        # blade rises at exactly 1:1 from the guard at the hip to past the left shoulder
        blade=dict(start=(628, 700), tip=(360, 445), slope=(1, 1), tip_row=None,
                   edge_side=-1, outline="both"),
        # hand fixes: 4 px bronze guard across the blade's foot, dark grip into the hand
        pixels={
            "bronze da guarda": [(35, 39), (34, 40), (33, 41), (32, 42)],
            "calça sombra": [(34, 41)],
        },
    ),
}


# ---------------------------------------------------------------- colour
def srgb_to_lab(rgb):
    c = np.asarray(rgb, dtype=float) / 255.0
    c = np.where(c > 0.04045, ((c + 0.055) / 1.055) ** 2.4, c / 12.92)
    m = np.array([[0.4124, 0.3576, 0.1805], [0.2126, 0.7152, 0.0722], [0.0193, 0.1192, 0.9505]])
    xyz = c @ m.T / np.array([0.95047, 1.0, 1.08883])
    f = np.where(xyz > 0.008856, np.cbrt(xyz), 7.787 * xyz + 16 / 116)
    return np.stack([116 * f[..., 1] - 16, 500 * (f[..., 0] - f[..., 1]), 200 * (f[..., 1] - f[..., 2])], -1)


# ---------------------------------------------------------------- step 1: mask
def background_mask(a, tol=9):
    """True where the pixel is background. a: HxWx3 int array."""
    h, w, _ = a.shape
    border = np.concatenate([a[:8].reshape(-1, 3), a[-8:].reshape(-1, 3),
                             a[:, :8].reshape(-1, 3), a[:, -8:].reshape(-1, 3)])
    bg = np.median(border, 0)
    near = np.abs(a - bg).max(2) <= tol
    ext = np.zeros((h, w), bool)
    q = deque()
    for x in range(w):
        for y in (0, h - 1):
            if near[y, x] and not ext[y, x]:
                ext[y, x] = True; q.append((y, x))
    for y in range(h):
        for x in (0, w - 1):
            if near[y, x] and not ext[y, x]:
                ext[y, x] = True; q.append((y, x))
    while q:
        y, x = q.popleft()
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            ny, nx = y + dy, x + dx
            if 0 <= ny < h and 0 <= nx < w and near[ny, nx] and not ext[ny, nx]:
                ext[ny, nx] = True; q.append((ny, nx))
    # enclosed flat-grey holes: same grey as the background, big enough to be a gap
    holes = []
    seen = ext.copy()
    for y in range(h):
        for x in range(w):
            if near[y, x] and not seen[y, x]:
                comp = []; seen[y, x] = True; q.append((y, x))
                while q:
                    cy, cx = q.popleft(); comp.append((cy, cx))
                    for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                        ny, nx = cy + dy, cx + dx
                        if 0 <= ny < h and 0 <= nx < w and near[ny, nx] and not seen[ny, nx]:
                            seen[ny, nx] = True; q.append((ny, nx))
                ys, xs = zip(*comp)
                vals = a[list(ys), list(xs)]
                if len(comp) >= 120 and vals.std(0).max() < 2.5:
                    for cy, cx in comp:
                        ext[cy, cx] = True
                    holes.append((len(comp), min(xs), min(ys), max(xs), max(ys)))
    return ext, bg, holes


# ---------------------------------------------------------------- feet
def find_feet(fg):
    """Return the two lowest foreground blobs in the bottom band: list of (x0, x1, bottom_y)."""
    h, w = fg.shape
    ys = np.where(fg.any(1))[0]
    bottom = ys.max()
    band = fg[bottom - 60: bottom + 1]
    cols = band.any(0)
    runs, x = [], 0
    while x < w:
        if cols[x]:
            x0 = x
            while x < w and cols[x]:
                x += 1
            sub = fg[:, x0:x]
            by = np.where(sub.any(1))[0].max()
            runs.append((x0, x - 1, by))
        x += 1
    runs.sort(key=lambda r: r[1] - r[0], reverse=True)
    return sorted(runs[:2])


# ---------------------------------------------------------------- step 2: resample
BLADE_ONLY = ("aço sombra", "aço", "fio e ponta")


def resample(a, fg, sx, sy, scale, names):
    """Mode of palette-matched source pixels per target cell. Steel is matched too, so the
    old blade can be found and cleared, but body pixels never take a steel colour: the blade
    is redrawn afterwards."""
    cols = np.array([PAL[n] for n in names])
    lab_pal = srgb_to_lab(cols)
    h, w = fg.shape
    idx = np.full((SIZE, SIZE), -1, int)
    for r in range(SIZE):
        y0 = sy + (r - PIVOT[1]) * scale
        for c in range(SIZE):
            x0 = sx + (c - PIVOT[0]) * scale
            ya, yb = int(round(y0)), int(round(y0 + scale))
            xa, xb = int(round(x0)), int(round(x0 + scale))
            if yb <= 0 or xb <= 0 or ya >= h or xa >= w:
                continue
            ya, xa = max(ya, 0), max(xa, 0)
            cell = fg[ya:yb, xa:xb]
            if cell.size == 0 or cell.mean() < 0.5:
                continue
            px = a[ya:yb, xa:xb][cell]
            lab = srgb_to_lab(px)
            d = ((lab[:, None, :] - lab_pal[None, :, :]) ** 2).sum(2)
            nearest = d.argmin(1)
            idx[r, c] = np.bincount(nearest, minlength=len(names)).argmax()
    return idx


def mode(values):
    """Most common value; ties go to the first one seen, so every run gives the same pixels."""
    counts = {}
    for v in values:
        counts[v] = counts.get(v, 0) + 1
    return max(counts, key=lambda v: counts[v])


def neighbours4(r, c):
    for dr, dc in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        nr, nc = r + dr, c + dc
        if 0 <= nr < SIZE and 0 <= nc < SIZE:
            yield nr, nc


# inner lines use the dark tone of the garment they sit on (art bible 3.2)
SHADOW_OF = {
    "túnica brilho": "túnica sombra", "túnica tom": "túnica sombra", "túnica sombra": "túnica sombra",
    "faixa jade gasto": "faixa jade gasto sombra", "faixa jade gasto sombra": "faixa jade gasto sombra",
    "pele brilho": "pele sombra", "pele": "pele sombra", "pele sombra": "pele sombra",
    "corpo ardósia": "corpo sombra", "corpo sombra": "corpo sombra",
    "palha brilho": "palha sombra", "palha": "palha sombra", "palha sombra": "palha sombra",
    "calça": "calça sombra", "calça sombra": "calça sombra", "neutro escuro": "calça sombra",
    "aço": "aço sombra", "aço sombra": "aço sombra", "fio e ponta": "aço sombra",
    "bronze da guarda": "calça sombra",
}


def apply_outline(idx, names):
    """Outline colour only on the outside edge; inside, an outline-coloured pixel takes the
    dark tone of the garment most of its neighbours belong to."""
    o = names.index("contorno de ator")
    solid = idx >= 0
    out = idx.copy()
    for r in range(SIZE):
        for c in range(SIZE):
            if not solid[r, c]:
                continue
            edge = any(not solid[nr, nc] for nr, nc in neighbours4(r, c)) or r in (0, SIZE - 1) or c in (0, SIZE - 1)
            if edge:
                out[r, c] = o
            elif idx[r, c] == o:
                neigh = [names[idx[nr, nc]] for nr in range(r - 1, r + 2) for nc in range(c - 1, c + 2)
                         if 0 <= nr < SIZE and 0 <= nc < SIZE and idx[nr, nc] not in (-1, o)]
                shades = [SHADOW_OF[n] for n in neigh if SHADOW_OF.get(n) in names]
                pick = mode(shades) if shades else "calça sombra"
                out[r, c] = names.index(pick)
    return out


def src_to_cell(pt, sx, sy, scale):
    """Source pixel -> sprite cell (column, row)."""
    return (int((pt[0] - sx) // scale) + PIVOT[0], int((pt[1] - sy) // scale) + PIVOT[1])


def blade_pixels(start, tip, slope, tip_row):
    """Clean blade line from start to tip at an exact dx:dy step. Returns [(c, r)] start->tip."""
    sdx, sdy = slope
    dirx = 1 if tip[0] > start[0] else -1
    diry = 1 if tip[1] > start[1] else -1
    rows = abs((tip_row if tip_row is not None else tip[1]) - start[1])
    pts = []
    for i in range(rows + 1):
        pts.append((start[0] + dirx * (i * sdx // sdy), start[1] + diry * i))
    return pts


def redraw_blade(idx, names, cfg, sx, sy, scale, log, key):
    b = cfg.get("blade")
    if not b:
        return idx, set()
    out = idx.copy()
    o = names.index("contorno de ator")
    steel = {names.index(n) for n in ("aço sombra", "aço", "fio e ponta")}
    start = src_to_cell(b["start"], sx, sy, scale)
    tip = src_to_cell(b["tip"], sx, sy, scale)
    line = blade_pixels(start, tip, b["slope"], b["tip_row"])
    # 1. clear the resampled blade: steel and outline cells within 2 px of the line
    on_body = b["outline"] == "both"
    lc = np.array(line)
    for r in range(SIZE):
        for c in range(SIZE):
            if out[r, c] in steel or out[r, c] == o:
                d = np.abs(lc - (c, r)).max(1).min()
                if d <= 2:
                    if not on_body:
                        out[r, c] = -1
                    else:
                        neigh = [names[out[nr, nc]] for nr in range(r - 2, r + 3) for nc in range(c - 2, c + 3)
                                 if 0 <= nr < SIZE and 0 <= nc < SIZE and out[nr, nc] >= 0
                                 and out[nr, nc] not in steel and out[nr, nc] != o]
                        out[r, c] = names.index(mode(neigh)) if neigh else -1
    # 2. draw: core steel, lit edge on the upper-left side, lightest pixel at the tip
    core, edge, light = names.index("aço"), names.index("fio e ponta"), names.index("fio e ponta")
    shade = names.index("aço sombra")
    blade_set = set()
    for i, (c, r) in enumerate(line):
        out[r, c] = core; blade_set.add((c, r))
        if i < len(line) - 1:
            ec = c + b["edge_side"]
            out[r, ec] = edge; blade_set.add((ec, r))
    out[line[-1][1], line[-1][0]] = light
    # 3. outline: right/lower side only over the ground; both sides over the body
    for (c, r) in sorted(blade_set):
        for dc in (-1, 1):
            nc = c + dc
            if (nc, r) in blade_set or not (0 <= nc < SIZE):
                continue
            if b["outline"] == "right" and dc < 0:
                continue
            if b["outline"] == "right" and out[r, nc] >= 0 and out[r, nc] != o:
                continue  # never paint over the figure
            out[r, nc] = o
    # tip cap
    tc, tr = line[-1]
    ty = tr + (1 if tip[1] > start[1] else -1)
    if 0 <= ty < SIZE and out[ty, tc] < 0:
        out[ty, tc] = o
    log.append(f"[{key}] blade redrawn: {len(line)} px long at {b['slope'][0]}:{b['slope'][1]}, "
               f"from cell {line[0]} to tip {line[-1]}")
    return out, blade_set


def add_contact(idx, names, feet_cells):
    """1 px contact shade under each foot (the soft ellipse is the game's)."""
    out = idx.copy()
    k = names.index("contato sob os pés") if "contato sob os pés" in names else None
    for c0, c1 in feet_cells:
        cols = [c for c in range(c0, c1 + 1) if (out[:, c] >= 0).any()]
        if not cols:
            continue
        bottom = max(np.where(out[:, c] >= 0)[0].max() for c in cols)
        r = bottom + 1
        if r < SIZE:
            for c in cols[1:-1] or cols:
                if out[r, c] < 0:
                    out[r, c] = k
    return out


def despeckle(idx, names, keep=("pele sombra", "pele", "pele brilho")):
    """A body pixel whose colour none of its 4 neighbours shares takes the most common colour
    of its 8 neighbours (art bible 5.6: no groups smaller than 2x2 that carry nothing)."""
    out = idx.copy()
    o = names.index("contorno de ator")
    keep_i = {names.index(n) for n in keep if n in names}
    for r in range(1, SIZE - 1):
        for c in range(1, SIZE - 1):
            v = idx[r, c]
            if v < 0 or v == o or v in keep_i:
                continue
            n4 = [idx[nr, nc] for nr, nc in neighbours4(r, c)]
            if v in n4 or any(x < 0 for x in n4):
                continue
            n8 = [idx[nr, nc] for nr in range(r - 1, r + 2) for nc in range(c - 1, c + 2)
                  if (nr, nc) != (r, c) and idx[nr, nc] >= 0 and idx[nr, nc] != o]
            if n8:
                out[r, c] = mode(n8)
    return out


def drop_specks(idx):
    """Remove isolated opaque pixels (no 4-neighbour) - noise from the resample."""
    out = idx.copy()
    for r in range(SIZE):
        for c in range(SIZE):
            if idx[r, c] >= 0 and all(idx[nr, nc] < 0 for nr, nc in neighbours4(r, c)):
                out[r, c] = -1
    return out


def to_image(idx, names):
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    px = img.load()
    for r in range(SIZE):
        for c in range(SIZE):
            if idx[r, c] >= 0:
                px[c, r] = PAL[names[idx[r, c]]] + (255,)
    return img


def process(key, log):
    cfg = SPRITES[key]
    names = cfg["colours"]
    src = RAW / cfg["src"]
    a = np.asarray(Image.open(src).convert("RGB")).astype(int)
    bgmask, bg, holes = background_mask(a)
    fg = ~bgmask
    WORK.mkdir(parents=True, exist_ok=True)
    stem = Path(cfg["out"]).stem
    Image.fromarray((fg * 255).astype(np.uint8)).save(WORK / f"{stem}_mask.png")
    rgba = np.dstack([a.astype(np.uint8), (fg * 255).astype(np.uint8)])
    cut = Image.fromarray(rgba, "RGBA")
    for label, ground in (("claro", (236, 232, 220)), ("escuro", (20, 22, 18))):
        g = Image.new("RGBA", cut.size, ground + (255,))
        g.alpha_composite(cut)
        g.convert("RGB").save(WORK / f"{stem}_cutout_{label}.png")

    ys, xs = np.where(fg)
    top = ys.min()
    feet = find_feet(fg)
    log.append(f"[{key}] source {src.name} {a.shape[1]}x{a.shape[0]}, background {bg.tolist()}, "
               f"figure bbox x {xs.min()}-{xs.max()} y {top}-{ys.max()}")
    log.append(f"[{key}] enclosed background holes removed: {holes or 'none'}")
    log.append(f"[{key}] feet (x0, x1, sole y): {feet}")
    if cfg["pivot"] == "support_foot":
        # the support (back) foot is the higher one; the front foot sits closer to the camera
        foot = min(feet, key=lambda f: f[2])
        sx, sole = (foot[0] + foot[1]) / 2, foot[2]
    else:
        sx = (sum((f[0] + f[1]) / 2 for f in feet)) / 2
        sole = max(f[2] for f in feet)
    rows = PIVOT[1] - cfg["top_row"] + 1
    scale = (sole + 1 - top) / rows
    sy = sole + 1 - scale / 2 - (scale / 2)  # top of row 59's cell = sole+1-scale
    sy = sole + 1 - scale
    sx = sx - scale / 2
    log.append(f"[{key}] pivot source ({sx + scale / 2:.0f}, {sole}), scale {scale:.2f} source px per sprite px, "
               f"height {rows} px (rows {cfg['top_row']}-{PIVOT[1]})")
    idx = resample(a, fg, sx, sy, scale, names)
    np.save(WORK / f"{stem}_idx_raw.npy", idx)
    idx = drop_specks(idx)
    idx = apply_outline(idx, names)
    idx = despeckle(idx, names)
    idx, blade_set = redraw_blade(idx, names, cfg, sx, sy, scale, log, key)
    # steel left anywhere off the redrawn blade becomes the garment around it
    steel = [names.index(n) for n in BLADE_ONLY]
    stray = [(r, c) for r, c in np.argwhere(np.isin(idx, steel)) if (c, r) not in blade_set]
    for r, c in stray:
        neigh = [idx[nr, nc] for nr in range(r - 1, r + 2) for nc in range(c - 1, c + 2)
                 if 0 <= nr < SIZE and 0 <= nc < SIZE and idx[nr, nc] >= 0
                 and idx[nr, nc] not in steel and (nc, nr) not in blade_set]
        if neigh:
            idx[r, c] = mode(neigh)
    log.append(f"[{key}] stray steel pixels off the blade recoloured: {len(stray)}")
    for n, rgb in cfg.get("pixels", {}).items():
        for (c, r) in rgb:
            idx[r, c] = -1 if n == "transparent" else names.index(n)
    names = names + ["contato sob os pés"]
    feet_cells = [(src_to_cell((f[0], 0), sx, sy, scale)[0], src_to_cell((f[1], 0), sx, sy, scale)[0]) for f in feet]
    idx = add_contact(idx, names, feet_cells)
    np.save(WORK / f"{stem}_idx.npy", idx)
    img = to_image(idx, names)
    OUT.mkdir(parents=True, exist_ok=True)
    img.save(OUT / cfg["out"])
    log.append(f"[{key}] wrote {OUT / cfg['out']}")
    return img


if __name__ == "__main__":
    which = sys.argv[1] if len(sys.argv) > 1 else "all"
    keys = list(SPRITES) if which == "all" else [which]
    log = []
    for k in keys:
        process(k, log)
    print("\n".join(log))
