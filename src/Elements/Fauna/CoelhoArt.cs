using Godot;
using SpiritOfMurin.Creatures;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The Coelho de Campo's drawing (T07, element standard): body, head, tail and the <b>ears</b> — laid back while it
/// grazes, raised when it hears you, always at the back of the head for the side it faces; the body <b>crouches</b>
/// (lower, wider) in the instant before it dashes off; it stretches while running; and it <b>dives</b> into the burrow
/// (shrinking into the hole, with a puff of dust) instead of vanishing.
/// <b>Where to change it:</b> colours in the constants below; the ears in <see cref="Ears"/>; the crouch and the run in
/// <see cref="Draw"/>; the dive in <see cref="Dive"/>. How long the dive takes is in
/// <c>assets/data/plains/plains_fauna.json</c> (<c>rabbit_dive_seconds</c>); hearing and speed in
/// <c>assets/data/plains/RabbitTuning.tres</c>; the behaviour in <c>src/Creatures/RabbitBrain.cs</c>.
/// </summary>
public static class CoelhoArt
{
    /// <summary>Colours: outline, fur, light fur, belly, inside of the ear, eye, shadow, dust.</summary>
    public static readonly Color Outline = new("#16140f"), Fur = new("#8a7a5e"), FurLight = new("#b8a98a"), Belly = new("#d8ccb0"),
        EarIn = new("#c99a8a"), Eye = new("#16140f"), Shadow = new(8 / 255f, 10 / 255f, 8 / 255f, 0.35f), DustC = new("#b9a77a");

    private static void R(CanvasItem c, float x, float y, float w, float h, Color col) => c.DrawRect(new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), col);

    /// <summary>Draws the rabbit <paramref name="r"/> on itself (local pixels, origin at its feet).</summary>
    public static void Draw(Rabbit r)
    {
        if (r.DiveLeft > 0) { Dive(r); return; }
        if (!r.Brain.Visible) return;
        int s = r.Brain.Facing.X < 0 ? -1 : 1;
        var st = r.Brain.State;
        bool running = st is RabbitState.Dash1 or RabbitState.Dash2, crouch = st == RabbitState.Alert;
        bool hop = r.Velocity.LengthSquared() > 1 && !running;
        float t = r.AnimTime;
        float lift = hop ? -Mathf.Abs(Mathf.Sin(t * 14f)) * 2 : 0;
        float stretch = running ? 3 : 0;
        // the crouch before the dash: lower and a little wider (the "it is about to run" frame)
        float low = crouch ? 2 : 0, wide = crouch ? 1 : 0;
        R(r, -6, -1, 12, 3, Shadow);
        float bx = -5 - stretch / 2 - wide / 2, by = -7 + lift + low;
        float bw = 10 + stretch + wide, bh = 7 - low * 0.5f;
        R(r, bx - 1, by - 1, bw + 2, bh + 2, Outline);
        R(r, bx, by, bw, bh, Fur); R(r, bx, by + bh - 2, bw, 2, Belly); R(r, bx + (s > 0 ? 0 : bw - 2), by + 1, 2, 2, FurLight);
        float hx = s > 0 ? bx + bw - 3 : bx - 4, hy = by - 3 + low * 0.5f;
        R(r, hx - 1, hy - 1, 8, 7, Outline); R(r, hx, hy, 6, 5, Fur);
        R(r, hx + (s > 0 ? 4 : 1), hy + 1, 1, 1, Eye);
        Ears(r, hx, hy, s, r.Brain.EarsUp);
        R(r, s > 0 ? bx - 2 : bx + bw, by + 1, 2, 2, Belly);
        if (r.HitFlash > 0) R(r, bx, by, bw, bh, new Color(1, 1, 1, r.HitFlash * 3));
    }

    /// <summary>The ears: raised (the tell) at the back of the head for the side it faces, or laid back along the body.</summary>
    public static void Ears(Rabbit r, float hx, float hy, int s, bool up)
    {
        if (up)
        {
            // facing right the head's back is its left side (hx+1, hx+3); facing left, its right side (hx+2, hx+4)
            float e = s > 0 ? hx + 1 : hx + 2;
            R(r, e - 1, hy - 7, 4, 7, Outline); R(r, e + 1, hy - 8, 4, 8, Outline);
            R(r, e, hy - 6, 2, 6, Fur); R(r, e + 2, hy - 7, 2, 7, FurLight); R(r, s > 0 ? e + 2 : e, hy - 6, 1, 4, EarIn);
        }
        else
        {
            float ex = s > 0 ? hx - 4 : hx + 4;
            R(r, ex - 1, hy - 1, 8, 4, Outline); R(r, ex, hy, 6, 2, Fur); R(r, ex + 1, hy, 3, 1, EarIn);
        }
    }

    /// <summary>The dive: the body shrinks down into the burrow's mouth, a little dust puffs out.</summary>
    public static void Dive(Rabbit r)
    {
        float k = 1 - r.DiveLeft / Mathf.Max(0.01f, FaunaSettings.Current.RabbitDiveSeconds); // 0 → 1
        Vector2 hole = r.ToLocal(r.BurrowAt);
        float w = Mathf.Round(10 * (1 - k)), h = Mathf.Round(6 * (1 - k));
        Vector2 at = hole * k + new Vector2(0, -3 * (1 - k));
        R(r, hole.X - 4, hole.Y - 1, 8, 3, new Color(0.13f, 0.1f, 0.07f, 0.9f)); // the burrow's mouth
        if (w >= 1 && h >= 1) { R(r, at.X - w / 2 - 1, at.Y - h - 1, w + 2, h + 2, Outline); R(r, at.X - w / 2, at.Y - h, w, h, Fur); }
        for (int i = 0; i < 4; i++)
        {
            float a = i * 1.6f, d = 3 + k * 7;
            R(r, hole.X + Mathf.Cos(a) * d, hole.Y - 2 - k * 4 + Mathf.Sin(a) * 2, 2, 2, new Color(DustC, 1 - k));
        }
    }
}
