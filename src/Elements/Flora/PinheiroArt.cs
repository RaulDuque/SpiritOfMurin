using Godot;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The pine (pinheiro): a brown trunk and 3 to 5 tiers of needles that bend with the wind, sometimes with a smaller
/// twin beside it. <b>Where to change it:</b> the tiers (width, height, how many) in <see cref="DrawPineAt"/>, the
/// greens in <see cref="Greens"/>, the trunk in <see cref="Trunk"/>, the notches of the axe in <see cref="Notches"/>,
/// the stump in <see cref="Stump"/>. Data (<c>assets/data/elements/arvores/pinheiro.json</c>, <c>look</c>):
/// <c>trunk_extra</c> (px of extra trunk height by variation, 0-4), <c>lean</c> (px each tier may lean).
/// Without data (the Vale) the drawing is the original one.
/// </summary>
public sealed class PinheiroArt : ElementArt
{
    /// <summary>Trunk colours: bark, shade, cut wood (the notches and the stump), its ring.</summary>
    public static readonly Color Bark = new("#5a3e28"), BarkShade = new("#3e2a1a"), CutWood = new("#c9a26a"), CutRing = new("#8a6a3e");

    // three palettes of needles: base, shade, highlight, lower edge
    private static (Color, Color, Color, Color) Greens(int hv) => (hv / 3 % 3) switch
    {
        0 => (new Color("#2f5a3a"), new Color("#23462d"), new Color("#4c7c4a"), new Color("#1d3a25")),
        1 => (new Color("#365f34"), new Color("#284a28"), new Color("#5a8a48"), new Color("#1f3a20")),
        _ => (new Color("#2a5440"), new Color("#1f4232"), new Color("#447a5a"), new Color("#18342a")),
    };

    /// <inheritdoc />
    public override void Draw(CanvasItem c, ElementDef? def, in ElementPose p)
    {
        int hv = Hv(p.Variation);
        float by = -3;
        if (p.State == ElementState.Depleted) { Fill(c, -7, by - 2, 14, 4, Shadow); Stump(c, by); return; } // a stump casts a small shadow
        Fill(c, -15, by - 3, 30, 6, Shadow); Fill(c, -11, by - 5, 22, 9, Shadow);
        int extra = def == null ? 0 : (int)(hv % 5 * def.LookF("trunk_extra", 0) / 4);
        float lean = def == null ? 0 : (hv / 7 % 3 - 1) * def.LookF("lean", 0);
        if (p.State == ElementState.Falling)
        {
            Stump(c, by);
            // the tree turns around its foot, faster and faster, and the canopy hits the ground
            float a = p.FallSide * Mathf.Min(1, p.FallT * p.FallT) * 1.5f;
            c.DrawSetTransform(new Vector2(0, by), a);
            DrawTree(c, def, p, hv, 0, extra, lean, notches: false);
            c.DrawSetTransform(Vector2.Zero);
            return;
        }
        DrawTree(c, def, p, hv, by, extra, lean, notches: true);
        if (p.Flash > 0) Fill(c, -3, by - 12 - extra, 6, 12 + extra, new Color(1, 1, 1, 0.5f * p.Flash));
    }

    private void DrawTree(CanvasItem c, ElementDef? def, in ElementPose p, int hv, float by, int extra, float lean, bool notches)
    {
        int tiers = 3 + hv % 3;
        var (g0, g1, g2, g3) = Greens(hv);
        if (hv % 7 == 0) DrawPineAt(c, p, -14, by + 2, 3, 0.7f, g0, g1, g2, g3, hv + 3, 0, 0);
        DrawPineAt(c, p, 0, by, tiers, 1f, g0, g1, g2, g3, hv, extra, lean);
        if (notches && p.Progress > 0) Notches(c, by, p.Progress, extra);
    }

    /// <summary>The trunk: outline, bark and its shaded side.</summary>
    private static void Trunk(CanvasItem c, float ox, float by, int extra)
    {
        Fill(c, ox - 3, by - 12 - extra, 6, 13 + extra, Outline); Fill(c, ox - 2, by - 12 - extra, 4, 12 + extra, Bark); Fill(c, ox + 1, by - 12 - extra, 1, 12 + extra, BarkShade);
    }

    // one pine: the trunk and its tiers, the upper ones bending with the wind (the gust running over the ground)
    private static void DrawPineAt(CanvasItem c, in ElementPose p, float ox, float by, int tiers, float scale, Color g0, Color g1, Color g2, Color g3, int hv, int extra, float lean)
    {
        float windPush = Mathf.Clamp(p.Wind, -1.5f, 2.5f);
        Trunk(c, ox, by, extra);
        for (int pass = 0; pass < 2; pass++)
            for (int i = 0; i < tiers; i++)
            {
                int sway = Mathf.RoundToInt(Mathf.Sin(p.Time * 1.2f + hv) * 0.6f + windPush * (i + 1) / tiers + lean * i);
                float w = (17 - i * 3 + hv % 3) * scale, top = Mathf.Round(by - extra - (24 + i * 11) * scale); int rows = (int)(15 * scale); // top rounded once (no doubled rows at ,5)
                for (int r = 0; r < rows; r++)
                {
                    float rw = Mathf.Max(1, Mathf.Round(w * (r + 1) / rows)), xx = ox - rw + (i > 0 ? sway : 0), yy = top + r;
                    if (pass == 0) { Fill(c, xx - 1, yy - 1, rw * 2 + 2, 3, Outline); continue; }
                    Fill(c, xx, yy, rw * 2, 1, g0); Fill(c, xx + rw, yy, rw, 1, g1);
                    Fill(c, xx + 1, yy, Mathf.Max(1, Mathf.Floor(rw / 3)), 1, g2);
                    if (r == rows - 1) Fill(c, xx, yy, rw * 2, 1, g3);
                }
            }
    }

    /// <summary>The axe's notches: up to four light chips in the trunk, more with the work done.</summary>
    private static void Notches(CanvasItem c, float by, float progress, int extra)
    {
        int n = Mathf.CeilToInt(progress * 4);
        for (int i = 0; i < n; i++)
        {
            float y = by - 4 - i * 2;
            Fill(c, -3, y, 3, 2, BarkShade); Fill(c, -2, y, 2, 1, CutWood);
        }
        if (progress > 0.6f) Fill(c, -1, by - 10, 1, 7, BarkShade); // the trunk is close to giving
    }

    /// <summary>What an axe leaves: a short trunk with the cut wood on top.</summary>
    private static void Stump(CanvasItem c, float by)
    {
        Fill(c, -4, by - 6, 8, 7, Outline); Fill(c, -3, by - 5, 6, 5, Bark); Fill(c, 1, by - 5, 2, 5, BarkShade);
        Fill(c, -3, by - 6, 6, 1, CutWood); Fill(c, -1, by - 6, 2, 1, CutRing);
    }
}
