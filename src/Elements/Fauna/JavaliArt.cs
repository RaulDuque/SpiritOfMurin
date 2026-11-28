using Godot;
using SpiritOfMurin.Creatures;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The Javali da Mata's drawing (T07, element standard): a heavy wedge of a body, the head that drops when it warns,
/// the scraping hoof and its <b>dust</b>, the red eye, the dazed dots of the recovery, and the signs of each contact —
/// the <b>blade glancing off</b> the hide (a white streak and a spark on the tusk), the body <b>flinching</b> back when a
/// hit lands, and the <b>charge strip</b> on the ground along the fixed direction.
/// <b>Where to change it:</b> colours in the constants below; the body in <see cref="Body"/>; the dust in <see cref="Dust"/>;
/// the strip in <see cref="Strip"/> (on/short/off in Esc → Modo de assistência); the glance in <see cref="Glance"/>.
/// Times and sizes of the flinch are in <c>assets/data/plains/plains_fauna.json</c>; the behaviour is
/// <c>src/Creatures/BoarBrain.cs</c> and its numbers <c>assets/data/plains/BoarTuning.tres</c>.
/// </summary>
public static class JavaliArt
{
    /// <summary>Colours: fur, light fur, dark, snout, tusk, eye, eye when angry, dust, ground marks, the outline, the shadow, dazed dots, the strip.</summary>
    public static readonly Color Fur = new("#4a3a2c"), FurHi = new("#6b5640"), Dark = new("#2c2219"), Snout = new("#8a6a58"), Tusk = new("#e6dcc0"),
        Eye = new("#c9a14a"), EyeAngry = new("#d05030"), DustC = new("#8f7a58"), Mark = new("#33271b"), Outline = new("#16140f"),
        Shadow = new(8 / 255f, 10 / 255f, 8 / 255f, 0.38f), Daze = new("#e6dcc0"), StripEdge = new(0.82f, 0.59f, 0.31f, 0.9f), StripFill = new(0.69f, 0.44f, 0.23f, 0.14f);

    private static void R(CanvasItem c, float x, float y, float w, float h, Color col) => c.DrawRect(new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), col);

    /// <summary>Draws the boar <paramref name="b"/> on itself (local pixels, origin at its feet).</summary>
    public static void Draw(Boar b, int stripMode)
    {
        var st = b.Brain.State;
        if (st == BoarState.Dead) return;
        if (st is BoarState.Warn or BoarState.Charge) Strip(b, stripMode);
        // fixed direction: scuffed marks on the ground along the line it is about to run
        if (b.Brain.DirectionLocked)
            for (int i = 1; i <= 4; i++) { Vector2 m = b.Brain.Facing * (14 + i * 12); R(b, m.X - 1, m.Y - 1, 3, 2, new Color(Mark, 0.8f - i * 0.15f)); }
        Body(b, st);
    }

    /// <summary>
    /// The charge strip (T07, decision A): two dashed earth-coloured edges and a faint fill, as long as the charge
    /// really runs (what is left of it while charging) and as wide as it hits. <paramref name="mode"/>: 0 full, 1 short (the first 60 px), 2 off.
    /// </summary>
    public static void Strip(Boar b, int mode)
    {
        float len = StripLength(b, mode);
        if (len < 4) return;
        Vector2 f = b.Brain.Facing, n = new(-f.Y, f.X) , start = f * 6;
        float half = b.Tuning.ChargeHitRadius;
        var fill = new[] { start + n * half, start + f * len + n * half, start + f * len - n * half, start - n * half };
        b.DrawColoredPolygon(fill, StripFill);
        b.DrawDashedLine(start + n * half, start + f * len + n * half, StripEdge, 1, 4);
        b.DrawDashedLine(start - n * half, start + f * len - n * half, StripEdge, 1, 4);
    }

    /// <summary>How long the strip is drawn now (px from the boar's feet): what is left of the charge; 0 when off or not locked.</summary>
    public static float StripLength(Boar b, int mode)
    {
        if (mode >= 2 || !b.Brain.DirectionLocked && b.Brain.State != BoarState.Charge) return 0;
        if (b.Brain.State is not (BoarState.Warn or BoarState.Charge)) return 0;
        float len = b.Tuning.ChargeDistance - (b.Brain.State == BoarState.Charge ? b.Brain.Travel : 0);
        return mode == 1 ? Mathf.Min(len, 60) : Mathf.Max(0, len);
    }

    /// <summary>The body, the head, the legs; the warning's dust; the recovery's dazed dots; a hit's flash and flinch.</summary>
    private static void Body(Boar b, BoarState st)
    {
        int s = b.Brain.Facing.X < 0 ? -1 : 1;
        bool warn = st == BoarState.Warn, charge = st == BoarState.Charge, dazed = st == BoarState.Recover;
        float t = b.AnimTime, headDrop = warn || charge ? 3 : 0;
        float bob = b.Velocity.LengthSquared() > 1 ? Mathf.Round(Mathf.Sin(t * (charge ? 30 : 8))) : 0;
        float shake = dazed ? Mathf.Round(Mathf.Sin(t * 18) * 1) : 0;
        // the flinch: the body pushed back (away from its facing) and a little lower when a hit lands
        float fk = b.FlinchLeft > 0 ? b.FlinchLeft / Mathf.Max(0.01f, FaunaSettings.Current.BoarFlinchSeconds) : 0;
        float fx = -s * Mathf.Round(FaunaSettings.Current.BoarFlinchPx * fk), fy = Mathf.Round(fk);
        R(b, -14, -2, 28, 5, Shadow);
        float scrape = warn ? Mathf.Round(Mathf.Sin(t * 22) * 3) : 0;
        R(b, fx - 9 * s - 1 + (s < 0 ? -2 : 0) - scrape * s, -5, 3, 5, Dark); R(b, fx - 5 * s - 1, -5, 3, 5, Dark);
        R(b, fx + 5 * s - 1, -5 - bob, 3, 5, Dark); R(b, fx + 9 * s - 1 + (s < 0 ? -2 : 0), -5, 3, 5, Dark);
        for (int pass = 0; pass < 2; pass++)
            for (int i = 0; i < 24; i++)
            {
                float x = -12 + i, h = 9 + Mathf.Min(i, 16) * 0.25f - fy;
                float bx = fx + (s > 0 ? x : -x - 1), top = -5 - h + bob;
                if (pass == 0) R(b, bx - 1, top - 1, 3, h + 2, Outline);
                else { R(b, bx, top, 1, h, Fur); R(b, bx, top, 1, 2, i % 3 == 0 ? Dark : FurHi); }
            }
        float hx = fx + (s > 0 ? 11 : -19), hy = -14 + headDrop + bob + shake + fy;
        R(b, hx - 1, hy - 1, 10, 9, Outline); R(b, hx, hy, 8, 7, Fur);
        float sx = s > 0 ? hx + 7 : hx - 3;
        R(b, sx - 1, hy + 2, 6, 5, Outline); R(b, sx, hy + 3, 4, 3, Snout);
        R(b, sx + 1, hy + 1, 2, 3, Tusk);
        R(b, s > 0 ? hx + 4 : hx + 2, hy + 2, 2, 1, warn || charge ? EyeAngry : Eye);
        R(b, s > 0 ? hx + 1 : hx + 5, hy - 2, 2, 2, Dark); // ear
        Dust(b, st, s, t);
        if (dazed)
            for (int i = 0; i < 3; i++)
            {
                float a = t * 6 + i * Mathf.Tau / 3, dx = hx + 3 + Mathf.Cos(a) * 7, dy = hy - 7 + Mathf.Sin(a) * 2;
                R(b, dx - 1, dy - 1, 4, 4, Outline); R(b, dx, dy, 2, 2, Daze);
            }
        if (b.HitFlash > 0) R(b, fx - 12, -18, 24, 13, new Color(1, 1, 1, b.HitFlash * 3));
        if (b.Spark > 0) Glance(b, hx, hy);
    }

    /// <summary>The dust of the scraping hoof (warning) and behind the charge.</summary>
    public static void Dust(Boar b, BoarState st, int s, float t)
    {
        if (st == BoarState.Warn)
            for (int i = 0; i < 3; i++) { float k = Mathf.PosMod(t * 3 + i * 0.33f, 1f); R(b, -12 * s - s * k * 10, -3 - k * 6, 2, 2, new Color(DustC, 1 - k)); }
        if (st == BoarState.Charge)
            for (int i = 0; i < 4; i++) { Vector2 d = -b.Brain.Facing * (14 + i * 6); R(b, d.X, d.Y - 3 - i, 3, 2, new Color(DustC, 0.7f - i * 0.15f)); }
    }

    /// <summary>The blade glancing off the hide: a spark on the tusk and a white streak along the side that was struck.</summary>
    public static void Glance(Boar b, float hx, float hy)
    {
        float k = Mathf.Clamp(b.Spark / 0.2f, 0, 1);
        R(b, hx + 2, hy - 4, 1, 4, Tusk); R(b, hx, hy - 2, 5, 1, Tusk);
        Vector2 from = b.GlanceFrom.LengthSquared() > 0.01f ? b.GlanceFrom.Normalized() : Vector2.Up;
        Vector2 at = from * 10 + new Vector2(0, -10), along = new(-from.Y, from.X);
        b.DrawLine(at - along * 6, at + along * 6, new Color(1, 1, 1, k), 1);
        b.DrawLine(at - along * 3 + from, at + along * 4 + from, new Color(1, 0.95f, 0.7f, k * 0.8f), 1);
    }
}
