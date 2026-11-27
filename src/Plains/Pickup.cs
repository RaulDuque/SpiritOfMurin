using Godot;

namespace SpiritOfMurin.Plains;

/// <summary>
/// Something lying in the world that E picks up once: flint, a hard branch, a herb, a bitter root.
/// The origin is its foot (it sits in the y-sorted world). It has no collision.
/// </summary>
public partial class Pickup : Node2D
{
    /// <summary>The generator's stable id (the world state keys on it).</summary>
    public string FeatureId { get; set; } = "";

    /// <summary>Item given when picked up.</summary>
    public string ItemId { get; set; } = "";

    /// <summary>What it looks like.</summary>
    public FeatureKind Kind { get; set; }

    /// <summary>Seeded variation for the drawing.</summary>
    public int Variation { get; set; }

    private float _time;

    private static readonly Color Outline = new("#16140f"), FlintLight = new("#b9b6a8"), FlintMid = new("#8c8a80"), FlintDark = new("#5d5b51"),
        Wood = new("#7a5a3a"), WoodDark = new("#4e3824"), Leaf = new("#4c7c4a"), LeafHi = new("#65a05a"), Vein = new("#b0403a"),
        RootSkin = new("#a08350"), RootDark = new("#6e5530"), Glint = new("#eef3f2");

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _time += (float)delta;
        if (Kind is FeatureKind.Herb or FeatureKind.Flint or FeatureKind.CoinPouch) QueueRedraw(); // herb sways, flint and coins glint
    }

    private void R(float x, float y, float w, float h, Color c) => DrawRect(new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), c);

    private void O(float x, float y, float w, float h) => R(x - 1, y - 1, w + 2, h + 2, Outline);

    /// <inheritdoc />
    public override void _Draw()
    {
        switch (Kind)
        {
            case FeatureKind.Flint:
                O(-5, -4, 10, 4); O(-2, -6, 5, 2);
                R(-5, -4, 10, 4, FlintMid); R(-2, -6, 5, 2, FlintLight); R(-4, -2, 4, 1, FlintDark); R(2, -3, 3, 1, FlintLight);
                if (Mathf.PosMod(_time + Variation * 0.37f, 2.4f) < 0.15f) R(0, -6, 1, 1, Glint);
                break;
            case FeatureKind.CoinPouch: // a small tied leather pouch with a coin peeking out
                O(-5, -7, 10, 7); O(-2, -9, 4, 2);
                R(-5, -7, 10, 7, new Color("#8f6f4a")); R(-4, -7, 8, 1, new Color("#a8845a")); R(-2, -9, 4, 2, new Color("#a8845a")); R(-3, -6, 6, 1, new Color("#5a3a24"));
                R(3, -3, 3, 2, new Color("#f0d27a")); R(3, -3, 3, 1, new Color("#fff2c0"));
                if (Mathf.PosMod(_time + Variation * 0.29f, 2.0f) < 0.15f) R(4, -4, 1, 1, Glint);
                break;
            case FeatureKind.Branch:
                O(-9, -3, 18, 2); O(3, -6, 2, 3);
                R(-9, -3, 18, 2, Wood); R(-9, -2, 18, 1, WoodDark); R(3, -6, 2, 3, Wood); R(-6, -4, 2, 1, Wood);
                break;
            case FeatureKind.Herb:
            {
                int sway = Mathf.RoundToInt(Mathf.Sin(_time * 2f + Variation) * 0.7f);
                for (int pass = 0; pass < 2; pass++)
                    for (int i = -2; i <= 2; i++)
                    {
                        float x = i * 2.5f + (i == 0 ? sway : 0), h = 7 - Mathf.Abs(i) * 1.5f;
                        if (pass == 0) { O(x, -h, 2, h); continue; }
                        R(x, -h, 2, h, i % 2 == 0 ? Leaf : LeafHi);
                        R(x, -h + 1, 1, h - 2, Vein); // the red veins that give it the name
                    }
                break;
            }
            case FeatureKind.Root:
                // a pale forked root half out of the turned earth, with a small sprout on top
                O(-4, -4, 8, 4); O(-7, -1, 3, 1); O(4, -1, 4, 1); O(-1, -8, 2, 4);
                R(-4, -4, 8, 4, RootSkin); R(-4, -1, 8, 1, RootDark); R(-7, -1, 3, 1, RootSkin); R(4, -1, 4, 1, RootSkin);
                R(-1, -8, 2, 4, Leaf); R(1, -8, 2, 1, LeafHi); R(-2, -3, 1, 1, RootDark);
                break;
        }
    }
}
