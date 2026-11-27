using Godot;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The bush (arbusto): three leafy lobes, sometimes with red berries or small white flowers. It sways with the wind
/// and, when a body (the swordsman, a rabbit, a boar) goes through it, the leaves <b>part and lean</b> toward where
/// the body goes, then spring back (pedido 14, delivery 5). The bend comes in <see cref="ElementPose.Bend"/>
/// (worked out by <see cref="WorldElement"/> from the bodies nearby).
/// <b>Where to change it:</b> the lobes in <see cref="Lobes"/>, the greens in <see cref="Leaf"/>, how far the leaves
/// part in <see cref="PartScale"/>. Data: <c>assets/data/elements/arbustos/arbusto.json</c> (<c>contact_radius</c>).
/// </summary>
public sealed class ArbustoArt : ElementArt
{
    /// <summary>Leaf greens: dark, mid, light; berry; flower.</summary>
    public static readonly Color[] Leaf = { new("#2f4a2c"), new("#456a3a"), new("#5f8a4a"), new("#7a3a32"), new("#efe4c8") };
    /// <summary>How many px the side lobes move apart per px of bend.</summary>
    public const float PartScale = 0.8f;

    /// <summary>The three lobes: centre x, centre y, radius (the top one sways most).</summary>
    public static readonly (float X, float Y, float R)[] Lobes = { (-6, -9, 7), (6, -9, 7), (0, -14, 8) };

    /// <inheritdoc />
    public override void Draw(CanvasItem c, ElementDef? def, in ElementPose p)
    {
        int hv = Hv(p.Variation);
        Fill(c, -13, -4, 26, 5, Shadow);
        int sway = Mathf.RoundToInt(Mathf.Sin(p.Time * 1.1f + hv) * 0.6f + Mathf.Clamp(p.Wind, -1, 1.5f));
        // a passing body: every lobe leans along its way, the side lobes part around it
        float lean = Mathf.Clamp(p.Bend.X, -6, 6), dip = Mathf.Clamp(Mathf.Abs(p.Bend.Length()) * 0.4f, 0, 3), part = Mathf.Clamp(p.Bend.Length() * PartScale, 0, 4);
        var lobes = new (float X, float Y, float R)[3];
        for (int i = 0; i < 3; i++)
        {
            var (lx, ly, r) = Lobes[i];
            float push = i == 0 ? -part : i == 1 ? part : 0;
            lobes[i] = (lx + push + Mathf.Round(lean * (i == 2 ? 1f : 0.6f)) + (i == 2 ? sway : 0), ly + (i == 2 ? dip : dip * 0.5f), r);
        }
        for (int pass = 0; pass < 2; pass++)
            foreach (var (lx, ly, r) in lobes)
                for (int yy = (int)-r; yy <= r; yy++)
                {
                    float w = Mathf.Round(Mathf.Sqrt(Mathf.Max(0, r * r - yy * yy)));
                    if (pass == 0) { Fill(c, lx - w - 1, ly + yy - 1, w * 2 + 2, 3, Outline); continue; }
                    Fill(c, lx - w, ly + yy, w * 2, 1, yy < -r / 3 ? Leaf[2] : yy < r / 3 ? Leaf[1] : Leaf[0]);
                }
        float bx = Mathf.Round(lean * 0.6f);
        if (hv % 3 == 0) { Fill(c, -3 + bx, -10, 2, 2, Leaf[3]); Fill(c, 4 + bx, -13, 2, 2, Leaf[3]); }
        else if (hv % 4 == 1) { Fill(c, -5 + bx, -12, 1, 1, Leaf[4]); Fill(c, 2 + bx, -16, 1, 1, Leaf[4]); Fill(c, 6 + bx, -8, 1, 1, Leaf[4]); }
    }
}
