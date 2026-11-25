using Godot;

namespace SpiritOfMurin.World;

/// <summary>
/// Tall scenery with the prototype's look: a pine, a lone boulder or a clump of bamboo.
/// The origin is the foot of the object, so a y-sorted parent draws whoever stands lower in
/// front. Only the base collides (art bible 6.5: base and body), so the player can walk behind.
/// </summary>
public partial class Obstacle : StaticBody2D
{
    /// <summary>The kinds of tall scenery the base scene uses.</summary>
    public enum Kind { Pine, Boulder, Bamboo }

    /// <summary>What this obstacle is.</summary>
    [Export] public Kind Type { get; set; } = Kind.Pine;

    /// <summary>Seed for the small variations (height, sway phase).</summary>
    [Export] public int Variation { get; set; }

    private float _time;

    // base collision boxes (half width, height), the HTML prototype's BASE values
    private static readonly (float HalfWidth, float Height) PineBase = (7, 11), BoulderBase = (12, 13), BambooBase = (16, 7);

    /// <inheritdoc />
    public override void _Ready()
    {
        var (hw, h) = Type switch { Kind.Pine => PineBase, Kind.Boulder => BoulderBase, _ => BambooBase };
        AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(hw * 2, h) }, Position = new Vector2(0, -h / 2) });
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _time += (float)delta;
        if (Type != Kind.Boulder) QueueRedraw(); // pines and bamboo sway
    }

    private static readonly Color Outline = new("#16140f"), Shadow = new(8 / 255f, 10 / 255f, 8 / 255f, 0.32f);

    /// <inheritdoc />
    public override void _Draw()
    {
        int hv = (int)((((uint)Variation * 2654435761u) >> 7) % 97u);
        switch (Type)
        {
            case Kind.Pine: DrawPine(hv); break;
            case Kind.Boulder: DrawBoulder(hv); break;
            default: DrawBamboo(hv); break;
        }
    }

    private void Fill(float x, float y, float w, float h, Color c) => DrawRect(new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), c);

    private void DrawPine(int hv)
    {
        float by = -3;
        Fill(-15, by - 3, 30, 6, Shadow); Fill(-11, by - 5, 22, 9, Shadow);
        Fill(-3, by - 12, 6, 13, Outline); Fill(-2, by - 12, 4, 12, new Color("#5a3e28")); Fill(1, by - 12, 1, 12, new Color("#3e2a1a"));
        int sway = Mathf.RoundToInt(Mathf.Sin(_time * 1.2f + hv));
        for (int pass = 0; pass < 2; pass++)
            for (int i = 0; i < 4; i++)
            {
                float w = 17 - i * 3 + hv % 3, top = by - 24 - i * 11; const int rows = 15;
                for (int r = 0; r < rows; r++)
                {
                    float rw = Mathf.Max(1, Mathf.Round(w * (r + 1) / rows)), xx = -rw + (i > 1 ? sway : 0), yy = top + r;
                    if (pass == 0) { Fill(xx - 1, yy - 1, rw * 2 + 2, 3, Outline); continue; }
                    Fill(xx, yy, rw * 2, 1, new Color("#2f5a3a")); Fill(xx + rw, yy, rw, 1, new Color("#23462d"));
                    Fill(xx + 1, yy, Mathf.Max(1, Mathf.Floor(rw / 3)), 1, new Color("#4c7c4a"));
                    if (r == rows - 1) Fill(xx, yy, rw * 2, 1, new Color("#1d3a25"));
                }
            }
    }

    private void DrawBoulder(int hv)
    {
        float by = -2, ht = 24 + hv % 6, hw = 13 + hv % 4;
        Color hi = new("#a3a091"), top = new("#8a877a"), face = new("#5d5b51"), dark = new("#3b3a34");
        Fill(-hw - 2, by - 3, hw * 2 + 4, 5, new Color(8 / 255f, 10 / 255f, 8 / 255f, 0.35f));
        for (int pass = 0; pass < 2; pass++)
            for (int r = 0; r < ht; r++)
            {
                float k = (r - ht * 0.55f) / (ht * 0.55f), w = Mathf.Round(hw * Mathf.Sqrt(Mathf.Max(0, 1 - k * k * (k < 0 ? 1 : 0.35f))));
                float yy = by - ht + r;
                if (pass == 0) { Fill(-w - 1, yy - 1, w * 2 + 2, 3, Outline); continue; }
                Fill(-w, yy, w * 2, 1, r < ht * 0.25f ? hi : r < ht * 0.65f ? top : face);
                Fill(w - 3, yy, 3, 1, r < ht * 0.65f ? face : dark);
            }
        Fill(-4 + hv % 5, by - ht + 8, 1, 6, dark); Fill(-3 + hv % 5, by - ht + 13, 4, 1, dark); Fill(-7, by - ht + 4, 5, 1, hi);
    }

    private void DrawBamboo(int hv)
    {
        Color bamboo = new("#5f8f4a"), bambooHi = new("#7fae5d"), bambooDark = new("#3b6332"), leaf = new("#4d7a3c");
        for (int i = 0; i < 3; i++)
        {
            int h2 = (hv * 7 + i * 13) % 17;
            float bx = -13 + i * 10 + h2 % 4, ht = 34 + h2 % 14;
            Fill(bx + 1, -3, 5, 3, bambooDark);
            Fill(bx, -ht, 3, ht, bamboo); Fill(bx, -ht, 1, ht, bambooHi);
            for (float n = -8; n > -ht; n -= 9) Fill(bx, n, 3, 1, bambooDark);
            float sway = Mathf.Round(Mathf.Sin(_time * 1.5f + h2) * 1.5f);
            Fill(bx - 4 + sway, -ht + 2, 5, 2, leaf); Fill(bx + 2 + sway, -ht + 6, 6, 2, leaf); Fill(bx - 3 + sway, -ht + 11, 4, 1, leaf);
        }
    }
}
