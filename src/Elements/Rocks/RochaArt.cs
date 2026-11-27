using Godot;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The rock (rocha): a round boulder, a flat slab, or a rock with small stones around it (by variation).
/// A rock that can be mined shows its <b>vein</b> (pale lines with glints of quartz); a decorative rock has none.
/// The pickaxe opens <b>cracks</b> that grow with the work; used up, it leaves rubble.
/// <b>Where to change it:</b> the shapes in <see cref="Boulder"/>, the greys in <see cref="Greys"/>, the vein in
/// <see cref="Vein"/> (colour <c>look.vein</c> in the data), the cracks in <see cref="Cracks"/>, the rubble in
/// <see cref="Rubble"/>, the rust stains in <see cref="Rust"/> (<c>look.rust</c>). Data: <c>assets/data/elements/rochas/*.json</c>.
/// </summary>
public sealed class RochaArt : ElementArt
{
    /// <summary>Greys of the stone: light top, top, face, dark side.</summary>
    public static readonly Color[] Greys = { new("#a3a091"), new("#8a877a"), new("#5d5b51"), new("#3b3a34") };
    /// <summary>The cracks' colour.</summary>
    public static readonly Color Crack = new("#24231f");

    /// <inheritdoc />
    public override void Draw(CanvasItem c, ElementDef? def, in ElementPose p)
    {
        int hv = Hv(p.Variation);
        if (p.State == ElementState.Depleted) { Rubble(c, hv, 1); return; }
        if (p.State == ElementState.Falling)
        {
            // breaking: the stone sinks and splits, pieces spread
            Rubble(c, hv, p.FallT);
            if (p.FallT < 0.5f) Boulder(c, hv, 1 - p.FallT * 1.6f);
            return;
        }
        var (top, hw) = Boulder(c, hv, 1);
        if (def != null && def.Look.ContainsKey("rust")) Rust(c, hv, top, hw, def.LookC("rust", "#8a4a2a")); // stains of a rusty rock (look.rust)
        if (def is { Veined: true }) Vein(c, hv, top, hw, def.LookC("vein", "#e6efe8"), p.Time);
        if (p.Progress > 0) Cracks(c, hv, top, hw, p.Progress);
        if (p.Flash > 0) Fill(c, -hw, top, hw * 2, -top - 2, new Color(1, 1, 1, 0.35f * p.Flash));
    }

    /// <summary>The boulder (the original drawing). Returns the top y and the half width, for the vein and the cracks.</summary>
    private static (float Top, float HalfWidth) Boulder(CanvasItem c, int hv, float size)
    {
        int shape = hv % 3;
        if (shape == 2 && size >= 1) { Small(c, -14, 1, 8, 6); Small(c, 13, 2, 6, 5); }
        float by = -2, ht = (24 + hv % 6) * (shape == 1 ? 0.62f : shape == 2 ? 0.85f : 1f) * size, hw = (13 + hv % 4) * (shape == 1 ? 1.3f : 1f) * Mathf.Max(0.6f, size);
        Fill(c, -hw - 2, by - 3, hw * 2 + 4, 5, new Color(8 / 255f, 10 / 255f, 8 / 255f, 0.35f));
        for (int pass = 0; pass < 2; pass++)
            for (int r = 0; r < ht; r++)
            {
                float k = (r - ht * 0.55f) / (ht * 0.55f), w = Mathf.Round(hw * Mathf.Sqrt(Mathf.Max(0, 1 - k * k * (k < 0 ? 1 : 0.35f))));
                float yy = Mathf.Round(by - ht) + r; // round the top once: rounding each row put two rows on one pixel when ht ended in ,5 (striped slabs)
                if (pass == 0) { Fill(c, -w - 1, yy - 1, w * 2 + 2, 3, Outline); continue; }
                Fill(c, -w, yy, w * 2, 1, r < ht * 0.25f ? Greys[0] : r < ht * 0.65f ? Greys[1] : Greys[2]);
                Fill(c, w - 3, yy, 3, 1, r < ht * 0.65f ? Greys[2] : Greys[3]);
            }
        if (size >= 1) { Fill(c, -4 + hv % 5, by - ht + 8, 1, 6, Greys[3]); Fill(c, -3 + hv % 5, by - ht + 13, 4, 1, Greys[3]); Fill(c, -7, by - ht + 4, 5, 1, Greys[0]); }
        return (by - ht, hw);
    }

    // a small stone of a group (or a piece of rubble)
    private static void Small(CanvasItem c, float ox, float by, float hw, float ht)
    {
        Fill(c, ox - hw - 1, by - 2, hw * 2 + 2, 3, new Color(8 / 255f, 10 / 255f, 8 / 255f, 0.3f));
        for (int pass = 0; pass < 2; pass++)
            for (int r = 0; r < ht; r++)
            {
                float k = (r - ht * 0.5f) / (ht * 0.5f), w = Mathf.Round(hw * Mathf.Sqrt(Mathf.Max(0, 1 - k * k * 0.6f)));
                float yy = Mathf.Round(by - ht) + r;
                if (pass == 0) { Fill(c, ox - w - 1, yy - 1, w * 2 + 2, 3, Outline); continue; }
                Fill(c, ox - w, yy, w * 2, 1, r < ht * 0.3f ? Greys[0] : r < ht * 0.7f ? Greys[1] : Greys[2]);
            }
    }

    /// <summary>The vein that tells a mineable rock: two pale lines across the face and glints that blink.</summary>
    private static void Vein(CanvasItem c, int hv, float top, float hw, Color vein, float time)
    {
        float y0 = top + 7 + hv % 4;
        for (int i = 0; i < (int)(hw * 1.4f); i++)
        {
            float x = -hw * 0.7f + i, y = y0 + Mathf.Round(Mathf.Sin(i * 0.45f + hv) * 1.5f) + i * 0.25f;
            Fill(c, x, y, 1, 1, vein);
            if (i % 3 == 0) Fill(c, x, y + 1, 1, 1, new Color(vein, 0.55f));
        }
        for (int g = 0; g < 3; g++)
        {
            float on = Mathf.Sin(time * 2.2f + g * 2.1f + hv);
            if (on > 0.55f) Fill(c, -hw * 0.5f + g * hw * 0.5f, y0 + 2 + g, 1, 1, Colors.White);
        }
    }

    /// <summary>Rust stains on the face (<c>look.rust</c> in the data): a few blotches, lower and darker than the vein.</summary>
    private static void Rust(CanvasItem c, int hv, float top, float hw, Color rust)
    {
        for (int k = 0; k < 4; k++)
        {
            float x = -hw * 0.6f + (k * 7 + hv) % (int)(hw * 1.2f), y = top + 10 + (k * 5 + hv) % 9;
            Fill(c, x, y, 3, 2, rust); Fill(c, x + 1, y + 2, 2, 1, new Color(rust, 0.7f));
        }
    }

    /// <summary>The pickaxe's cracks: zigzags from the top, longer and more of them with the work done.</summary>
    private static void Cracks(CanvasItem c, int hv, float top, float hw, float progress)
    {
        int n = 1 + Mathf.FloorToInt(progress * 3);
        for (int k = 0; k < n; k++)
        {
            float x = -hw * 0.5f + k * hw * 0.45f + hv % 3, y = top + 3;
            int len = (int)(4 + progress * 14);
            for (int s = 0; s < len; s++) { Fill(c, x, y, 1, 1, Crack); y += 1; x += (s + k + hv) % 3 - 1; }
        }
    }

    /// <summary>What is left of a broken rock: low stones on the ground (<paramref name="spread"/> 0-1 while it breaks).</summary>
    private static void Rubble(CanvasItem c, int hv, float spread)
    {
        float s = Mathf.Clamp(spread, 0, 1);
        Small(c, -9 - 4 * s, 2, 5, 4); Small(c, 6 + 5 * s, 3, 4, 3); Small(c, -1, 4, 6, 4);
        if (hv % 2 == 0) Small(c, 12 + 3 * s, -1, 3, 3);
    }
}
