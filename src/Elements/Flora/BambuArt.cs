using Godot;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The bamboo clump (bambu): three tall green stalks with dark rings and leaves at the top that sway.
/// A clump that can be cut has a pale <b>notch mark</b> at the height of the cut on each stalk still standing; each
/// good strike cuts one stalk, which <b>gives way</b> and falls to the side, leaving a slanted stub.
/// <b>Where to change it:</b> heights and spacing in <see cref="Stalk"/>, colours in <see cref="Green"/>,
/// the fall in <see cref="Falling"/>, the stub in <see cref="Stub"/>. Data: <c>assets/data/elements/bambus/*.json</c>
/// (<c>look.stalks</c>: how many stalks, 2-4). Without data (the Vale) the drawing is the original one.
/// </summary>
public sealed class BambuArt : ElementArt
{
    /// <summary>Stalk, its light side, its rings, the leaves, the cut wood.</summary>
    public static readonly Color[] Green = { new("#5f8f4a"), new("#7fae5d"), new("#3b6332"), new("#4d7a3c"), new("#d8d29a") };

    /// <summary>How long (s) a cut stalk takes to fall.</summary>
    public const float FallSeconds = 0.6f;

    /// <inheritdoc />
    public override void Draw(CanvasItem c, ElementDef? def, in ElementPose p)
    {
        int hv = Hv(p.Variation);
        int n = def == null ? 3 : (int)def.LookF("stalks", 3);
        int cut = p.State == ElementState.Depleted ? n : Mathf.FloorToInt(p.Progress * n + 0.001f);
        for (int i = 0; i < n; i++)
        {
            int h2 = (hv * 7 + i * 13) % 17;
            float bx = -13 + i * 10 + h2 % 4, ht = 34 + h2 % 14;
            if (i < cut)
            {
                Stub(c, bx);
                // the one cut last is still falling
                if (i == cut - 1 && p.SinceHit < FallSeconds && p.State != ElementState.Depleted) Falling(c, bx, ht, h2, p.SinceHit / FallSeconds, p.FallSide);
                continue;
            }
            Stalk(c, bx, ht, h2, p.Time, def == null ? 0 : p.Wind); // the Vale's clumps keep their own sway
            if (def is { Workable: true }) Fill(c, bx, -12, 3, 1, Green[4]); // where the cut goes
        }
    }

    /// <summary>One standing stalk (the original drawing).</summary>
    private static void Stalk(CanvasItem c, float bx, float ht, int h2, float time, float wind)
    {
        Fill(c, bx + 1, -3, 5, 3, Green[2]);
        Fill(c, bx, -ht, 3, ht, Green[0]); Fill(c, bx, -ht, 1, ht, Green[1]);
        for (float n = -8; n > -ht; n -= 9) Fill(c, bx, n, 3, 1, Green[2]);
        float sway = Mathf.Round(Mathf.Sin(time * 1.5f + h2) * 1.5f + Mathf.Clamp(wind, -1, 2));
        Fill(c, bx - 4 + sway, -ht + 2, 5, 2, Green[3]); Fill(c, bx + 2 + sway, -ht + 6, 6, 2, Green[3]); Fill(c, bx - 3 + sway, -ht + 11, 4, 1, Green[3]);
    }

    /// <summary>A cut stalk giving way: it turns around the cut and drops.</summary>
    private static void Falling(CanvasItem c, float bx, float ht, int h2, float t, float side)
    {
        float a = (side == 0 ? 1 : side) * Mathf.Min(1, t * t) * 1.4f;
        c.DrawSetTransform(new Vector2(bx + 1, -12), a);
        float rest = ht - 12;
        Fill(c, -1, -rest, 3, rest, Green[0]); Fill(c, -1, -rest, 1, rest, Green[1]);
        Fill(c, -5, -rest + 2, 5, 2, Green[3]); Fill(c, 1, -rest + 6, 6, 2, Green[3]);
        c.DrawSetTransform(Vector2.Zero);
    }

    /// <summary>The stub a cut leaves: a short stalk with a slanted, pale cut.</summary>
    private static void Stub(CanvasItem c, float bx)
    {
        Fill(c, bx + 1, -3, 5, 3, Green[2]);
        Fill(c, bx - 1, -13, 5, 12, Outline);
        Fill(c, bx, -12, 3, 11, Green[0]); Fill(c, bx, -12, 1, 11, Green[1]);
        Fill(c, bx, -12, 3, 1, Green[4]); Fill(c, bx + 2, -13, 1, 1, Green[4]);
    }
}
