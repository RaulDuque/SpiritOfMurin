using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Combat;

/// <summary>
/// The prototype's fighter drawing (index.html drawFighter / drawDead), for the other figures of the
/// prologue: the disciple of the Closed Guard, the Senior Swordsman and the master's statue. Feet at
/// (0, 0); every shape gets the 1 px outline of the prototype.
/// </summary>
public static class FighterArt
{
    /// <summary>The colours and proportions of one kind of figure (the prototype's LOOK).</summary>
    public sealed record Look(Color Robe, Color Shade, Color Trim, Color Sash, Color Skin, Color Hair, Color Pants, Color Blade, Color Hilt,
        int H, int Wide, bool Hat = false, bool LongHair = false, bool Pads = false, bool Bun = false);

    /// <summary>The disciple: grey-green robe, straw hat.</summary>
    public static readonly Look Disciple = new(new("#7f846c"), new("#585d4b"), new("#3d4136"), new("#a28f58"), new("#d0a07a"), new("#26221e"), new("#3a3a30"), new("#cbd1d3"), new("#4a3a2a"), 0, 0, Hat: true);
    /// <summary>The Senior Swordsman: taller, wider, dark red robe, gold trim, long hair, shoulder pads.</summary>
    public static readonly Look Senior = new(new("#6e3027"), new("#4a1f1a"), new("#c79e4e"), new("#24130f"), new("#cc9a74"), new("#131116"), new("#2a1714"), new("#e1e8ea"), new("#2a1a10"), 5, 2, LongHair: true, Pads: true);
    /// <summary>The master's statue: grey stone, hair in a bun.</summary>
    public static readonly Look Statue = new(new("#8d9089"), new("#6d706a"), new("#7b7e77"), new("#777a73"), new("#9b9e97"), new("#63665f"), new("#6a6d66"), new("#abaea7"), new("#6a6d66"), 0, 0, Bun: true);

    /// <summary>Outline colours.</summary>
    public static readonly Color Outline = new("#16140f"), Threat = new("#c8502a"), ThreatHi = new("#e8834c"), LearnGlow = new("#6fd8d2");

    /// <summary>The poses.</summary>
    public enum Pose { Idle, Walk, Guard, Windup, Strike, Stagger, Attack }

    /// <summary>Draws a fighter at (0, 0) facing <paramref name="face"/> (radians).</summary>
    public static void Draw(CanvasItem ci, PixelPainter p, Look L, float face, Pose pose, float t, float walk, bool telegraph, bool aura, bool flash, bool senior)
    {
        float x = 0, y = 0, fx = Mathf.Cos(face), fy = Mathf.Sin(face);
        string view = Mathf.Abs(fy) > Mathf.Abs(fx) * 0.8f ? (fy > 0 ? "down" : "up") : "side";
        int s = fx < 0 ? -1 : 1;
        float sy = y - 25 - L.H, nx = -fy;
        Vector2 Pf(float k) => new(fx * k, fy * k * 0.7f);
        Vector2 hand, tip; Vector2? hand2 = null; Color? glow = null;
        switch (pose)
        {
            case Pose.Guard:
                hand = new Vector2(x + Pf(7).X, sy + 5 + Pf(7).Y);
                tip = new Vector2(hand.X + nx * 13 * s + fx * 2, hand.Y - 7); hand2 = new Vector2(hand.X - nx * 4, hand.Y + 1);
                hand = new Vector2(hand.X - nx * 9 * s, hand.Y + 6);
                break;
            case Pose.Windup:
                hand = new Vector2(x - fx * 7 + nx * 5, sy - 9 - fy * 3); tip = new Vector2(hand.X - fx * 12, hand.Y - 13); glow = Threat;
                break;
            case Pose.Attack:
                hand = new Vector2(x + fx * 2, sy - 14); tip = new Vector2(hand.X - fx * 6 + nx * 8, hand.Y - 16); hand2 = new Vector2(x - fx * 2, sy - 12);
                break;
            case Pose.Strike:
                hand = new Vector2(x + Pf(12).X, sy + 3 + Pf(12).Y); tip = new Vector2(hand.X + fx * 22, hand.Y + fy * 15);
                break;
            case Pose.Stagger:
                hand = new Vector2(x + nx * 7, sy + 10); tip = new Vector2(hand.X + fx * 6, hand.Y + 14);
                break;
            default:
                hand = new Vector2(x + nx * 7 * (view == "side" ? -s : 1) + fx * 3, sy + 9); tip = new Vector2(hand.X + fx * 14, hand.Y + 9 + fy * 4);
                break;
        }
        bool back = view == "up";
        void Arms()
        {
            float shX = x + (view == "side" ? fx * 2 : nx * 6), shY = sy + 2;
            p.Line(shX, shY, hand.X, hand.Y, L.Robe, 3);
            if (hand2 is { } h2) p.Line(x - nx * 6, shY, h2.X, h2.Y, L.Robe, 3);
            p.Line(hand.X, hand.Y, tip.X, tip.Y, glow ?? L.Blade, glow != null ? 2 : 1);
            if (senior) p.Line(hand.X + (tip.X - hand.X) * 0.55f, hand.Y + (tip.Y - hand.Y) * 0.55f, tip.X, tip.Y, glow ?? L.Blade, 2);
            p.Rect(hand.X - 1, hand.Y - 1, 3, 3, L.Hilt);
            p.Rect(hand.X, hand.Y, 2, 2, L.Skin);
            if (hand2 is { } h) p.Rect(h.X, h.Y, 2, 2, L.Skin);
        }
        if (back) Arms();
        float wk = pose == Pose.Walk ? Mathf.Sin(walk) : 0;
        float lean = pose == Pose.Stagger ? Mathf.Round(Mathf.Sin(t * 18) * 2) : 0;
        p.Rect(x - 5, y - 7 - Mathf.Max(0, wk) * 2, 4, 7, L.Pants);
        p.Rect(x + 1, y - 7 - Mathf.Max(0, -wk) * 2, 4, 7, L.Pants);
        float top = y - 27 - L.H; int rows = 21 + L.H;
        int hem = pose == Pose.Walk ? Mathf.RoundToInt(Mathf.Sin(walk * 2)) : 0;
        for (int r = 0; r < rows; r++)
        {
            float w = 12 + L.Wide * 2 + Mathf.Floor(r * 4f / rows) * 2, rx = x - w / 2 + lean * (1 - (float)r / rows) + (r > rows - 6 ? hem : 0);
            p.Rect(rx, top + r, w, 1, L.Robe); p.Rect(rx + (s > 0 ? 0 : w - 3), top + r, 3, 1, L.Shade);
            if (r == 2 || r == rows - 1) p.Rect(rx + 3, top + r, w - 6, 1, r == 2 ? L.Trim : L.Shade);
        }
        p.Rect(x - 6 - L.Wide + lean, top + 9 + L.H, 12 + L.Wide * 2, 2, L.Sash);
        if (view != "up") p.Rect(x - 1 + (view == "side" ? s * 2 : 0) + lean, top, 2, 10, L.Trim);
        if (L.Pads) { p.Rect(x - 10 - L.Wide + lean, top, 6, 4, L.Trim); p.Rect(x + 4 + L.Wide + lean, top, 6, 4, L.Trim); }
        int breath = (pose is Pose.Idle or Pose.Guard) && !L.Bun && Mathf.Sin(t * 2.4f) > 0.55f ? 1 : 0;
        float hx = x + lean, hy = y - 38 - L.H + breath;
        p.Rect(hx - 5, hy + 1, 10, 9, L.Skin); p.Rect(hx - 4, hy, 8, 1, L.Skin); p.Rect(hx - 4, hy + 10, 8, 1, L.Skin);
        if (view == "down")
        {
            p.Rect(hx - 5, hy, 10, 4, L.Hair); p.Rect(hx - 6, hy + 1, 1, 4, L.Hair); p.Rect(hx + 5, hy + 1, 1, 4, L.Hair);
            p.Rect(hx - 3, hy + 6, 1, 1, Outline); p.Rect(hx + 2, hy + 6, 1, 1, Outline);
            if (L.LongHair) { p.Rect(hx - 7, hy + 2, 2, 14, L.Hair); p.Rect(hx + 5, hy + 2, 2, 14, L.Hair); }
        }
        else if (view == "up") { p.Rect(hx - 5, hy, 10, 10, L.Hair); if (L.LongHair) p.Rect(hx - 4, hy + 8, 8, 14, L.Hair); }
        else
        {
            p.Rect(hx - 5 * s - (s < 0 ? 2 : 0), hy, 7, 7, L.Hair); p.Rect(hx - 5, hy, 10, 3, L.Hair);
            p.Rect(hx + 3 * s - (s < 0 ? 1 : 0), hy + 5, 1, 1, Outline); p.Rect(hx + 5 * s - (s < 0 ? 1 : 0), hy + 5, 1, 2, L.Skin);
            if (L.LongHair) p.Rect(hx - 6 * s - (s < 0 ? 1 : 0), hy + 3, 2, 15, L.Hair);
        }
        if (L.Hat) { p.Rect(hx - 11, hy + 1, 22, 3, new Color("#b69858")); p.Rect(hx - 7, hy - 2, 14, 3, new Color("#c6a866")); p.Rect(hx - 3, hy - 4, 6, 2, new Color("#d4b676")); }
        else if (L.Bun) { p.Rect(hx - 7, hy, 3, 4, L.Hair); p.Rect(hx + 4, hy, 3, 4, L.Hair); p.Rect(hx - 1, hy - 3, 3, 3, L.Trim); }
        else p.Rect(hx - 2, hy - 3, 4, 3, L.Hair);
        if (!back) Arms();
        Color outline = Outline;
        if (telegraph) outline = Mathf.FloorToInt(t * 16) % 2 == 1 ? ThreatHi : Threat;
        else if (aura) outline = Mathf.FloorToInt(t * 8) % 2 == 1 ? Threat : new Color("#7a2a18");
        p.Flush(ci, outline, flash ? Colors.White : null);
    }

    /// <summary>Draws a fallen fighter at (0, 0); <paramref name="withPage"/> shows the glowing torn page.</summary>
    public static void DrawDead(CanvasItem ci, PixelPainter p, Look L, bool senior, bool withPage, float t)
    {
        ci.DrawRect(new Rect2(-16, -3, 32, 6), new Color(8 / 255f, 10 / 255f, 8 / 255f, 0.35f));
        p.Rect(-14, -7, 22, 7, L.Robe); p.Rect(-14, -7, 22, 2, L.Shade); p.Rect(-6, -7, 2, 7, L.Sash);
        p.Rect(8, -8, 8, 8, L.Skin); p.Rect(8, -8, 8, 3, L.Hair);
        p.Line(-18, 4, 2, 1, L.Blade, 1);
        if (L.Hat) { p.Rect(14, -14, 12, 3, new Color("#b69858")); p.Rect(17, -16, 6, 2, new Color("#c6a866")); }
        if (senior) p.Rect(-18, -9, 6, 4, L.Trim);
        p.Flush(ci, Outline);
        if (withPage)
        {
            bool gl = Mathf.FloorToInt(t * 3) % 2 == 1;
            ci.DrawRect(new Rect2(-4, -12, 6, 5), gl ? new Color("#d4fbf6") : LearnGlow);
            ci.DrawRect(new Rect2(-3, -10, 4, 1), new Color("#5d8f8b"));
        }
    }
}
