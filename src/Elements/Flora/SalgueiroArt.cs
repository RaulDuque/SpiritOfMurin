using Godot;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The riverbank willow (salgueiro), made with <c>/create arvore "Salgueiro de margem, com galhos baixos que
/// balançam com o vento"</c> from the pine's art: a short leaning trunk, a round low crown and long hanging
/// strands that swing with the wind (the lower ones swing more). It shares the pine's work: axe notches, the fall
/// and the stump. <b>Where to change it:</b> the crown in <see cref="Crown"/>, the hanging strands in
/// <see cref="Strands"/> (how many and how long: <c>look.strands</c>, <c>look.strand_length</c>), the colours in the
/// data (<c>look.crown</c>, <c>crown_light</c>, <c>crown_dark</c>) in <c>assets/data/elements/arvores/salgueiro.json</c>,
/// the trunk in <see cref="Trunk"/>.
/// </summary>
public sealed class SalgueiroArt : ElementArt
{
    /// <summary>Bark of the willow (greyer than the pine's), its shade, the cut wood.</summary>
    public static readonly Color Bark = new("#6a5a44"), BarkShade = new("#4a3e2e"), CutWood = new("#d8c08a");

    /// <inheritdoc />
    public override void Draw(CanvasItem c, ElementDef? def, in ElementPose p)
    {
        int hv = Hv(p.Variation);
        float by = -3;
        if (p.State == ElementState.Depleted) { Fill(c, -8, by - 2, 16, 4, Shadow); Stump(c, by); return; }
        Fill(c, -18, by - 3, 36, 6, Shadow);
        if (p.State == ElementState.Falling)
        {
            Stump(c, by);
            c.DrawSetTransform(new Vector2(0, by), p.FallSide * Mathf.Min(1, p.FallT * p.FallT) * 1.5f);
            Tree(c, def, p, hv, 0);
            c.DrawSetTransform(Vector2.Zero);
            return;
        }
        Tree(c, def, p, hv, by);
        if (p.Progress > 0)
        {
            int n = Mathf.CeilToInt(p.Progress * 4);
            for (int i = 0; i < n; i++) { Fill(c, -4, by - 4 - i * 2, 3, 2, BarkShade); Fill(c, -3, by - 4 - i * 2, 2, 1, CutWood); }
        }
        if (p.Flash > 0) Fill(c, -4, by - 14, 8, 14, new Color(1, 1, 1, 0.5f * p.Flash));
    }

    private static void Tree(CanvasItem c, ElementDef? def, in ElementPose p, int hv, float by)
    {
        float lean = (hv % 3 - 1) * 2;
        Trunk(c, by, lean);
        Color mid = def?.LookC("crown", "#5f8a44") ?? new Color("#5f8a44"), hi = def?.LookC("crown_light", "#8fbf5a") ?? new Color("#8fbf5a"), dark = def?.LookC("crown_dark", "#3f6a34") ?? new Color("#3f6a34");
        Crown(c, by - 16, lean, mid, hi, dark, hv);
        Strands(c, def, p, by - 16, lean, mid, hi, dark, hv);
    }

    /// <summary>A short, thick trunk leaning to one side.</summary>
    private static void Trunk(CanvasItem c, float by, float lean)
    {
        for (int y = 0; y < 16; y++)
        {
            float x = Mathf.Round(lean * y / 16f);
            Fill(c, x - 4, by - y - 1, 8, 1, Outline);
            Fill(c, x - 3, by - y - 1, 6, 1, Bark); Fill(c, x + 1, by - y - 1, 2, 1, BarkShade);
        }
    }

    /// <summary>The round crown, wider than tall, low on the trunk.</summary>
    private static void Crown(CanvasItem c, float top, float lean, Color mid, Color hi, Color dark, int hv)
    {
        float r = 13 + hv % 3, cx = lean, cy = top - 6;
        for (int pass = 0; pass < 2; pass++)
            for (int yy = (int)-r; yy <= r * 0.6f; yy++)
            {
                float w = Mathf.Round(Mathf.Sqrt(Mathf.Max(0, r * r - yy * yy)) * 1.25f);
                if (pass == 0) { Fill(c, cx - w - 1, cy + yy - 1, w * 2 + 2, 3, Outline); continue; }
                Fill(c, cx - w, cy + yy, w * 2, 1, yy < -r / 2 ? hi : yy < r / 4 ? mid : dark);
            }
    }

    /// <summary>The hanging strands: thin lines from the crown's edge down, swinging with the wind (the low ends most).</summary>
    private static void Strands(CanvasItem c, ElementDef? def, in ElementPose p, float top, float lean, Color mid, Color hi, Color dark, int hv)
    {
        int n = (int)(def?.LookF("strands", 7) ?? 7);
        float len = def?.LookF("strand_length", 18) ?? 18, r = 13 + hv % 3;
        float wind = Mathf.Clamp(p.Wind, -1.5f, 2.5f);
        for (int i = 0; i < n; i++)
        {
            float x0 = lean - r * 1.15f + i * (r * 2.3f / Mathf.Max(1, n - 1)), y0 = top - 4 + Mathf.Abs(i - (n - 1) / 2f) * 1.2f;
            float sway = Mathf.Sin(p.Time * 1.4f + i * 0.9f + hv) * 1.2f + wind * 1.6f;
            float l = len - (i % 3) * 3;
            for (int pass = 0; pass < 2; pass++)
                for (int s = 0; s < l; s++)
                {
                    float k = s / l, x = Mathf.Round(x0 + sway * k * k), w = s < l * 0.6f ? 2 : 1;
                    if (pass == 0) { Fill(c, x - 1, y0 + s, w + 2, 1, Outline); continue; } // the outline makes the strands read over grass
                    Fill(c, x, y0 + s, w, 1, s % 4 == 0 ? hi : i % 2 == 0 ? mid : dark);
                }
        }
    }

    private static void Stump(CanvasItem c, float by)
    {
        Fill(c, -5, by - 6, 10, 7, Outline); Fill(c, -4, by - 5, 8, 5, Bark); Fill(c, 1, by - 5, 3, 5, BarkShade);
        Fill(c, -4, by - 6, 8, 1, CutWood);
    }
}
