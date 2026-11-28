using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The worn iron sabre (T18, item <c>sabre_ferro</c>): a single-edged blade a little longer than the sword, curved by
/// one pixel, grey steel with a lighter edge nicked in two places, a dark iron guard (no gold), a long dark grip and a
/// pixel of rust by the guard. The scabbard crosses the back as the sword's does, with an iron tip.
/// <b>Where to change it:</b>
/// <list type="bullet">
/// <item><b>the blade's length</b>: <see cref="Length"/> (1 = the sword; 1.05 now). The strike's reach is data
/// (<c>assets/data/equipamento/armas/sabre_ferro.json</c>, <c>reach</c>);</item>
/// <item>the curve: <see cref="Bend"/> (px off the straight line) and <see cref="BendAt"/> (where it is deepest);</item>
/// <item>the edge and its nicks: <see cref="InHand"/> (<c>Nicks</c>); the guard (±2 px across, 2 thick), the grip
/// (3 px behind the hand) and the pommel below it;</item>
/// <item>the colours: the constants.</item>
/// </list>
/// </summary>
public sealed class SabreFerroArt : ArmaArt
{
    /// <summary>Steel core, the edge and tip, the iron guard and pommel, the grip, the rust, the scabbard.</summary>
    public static readonly Color Core = new("#b9b8ae"), Edge = new("#dcd8c8"), Iron = new("#4a4640"), Grip = new("#3b2a1f"), Rust = new("#6e5844"), Scabbard = new("#3d2817");

    /// <summary>How far (px) the blade's back bows from the straight line, and where along it (0 hand … 1 tip).</summary>
    public const float Bend = 1f, BendAt = 0.55f;

    /// <summary>Where the edge is nicked (fractions of the blade).</summary>
    private static readonly float[] Nicks = { 0.4f, 0.7f };

    /// <inheritdoc />
    public override float Length => 1.05f;

    /// <inheritdoc />
    public override bool Curved => true;

    // the side the back of the blade bows to (the same on the screen whichever way the body faces)
    private static Vector2 BackSide(FighterPose f, Vector2 d) => new Vector2(d.Y, -d.X) * f.S;

    /// <inheritdoc />
    public override Vector2 Spine(FighterPose f, float t)
    {
        Vector2 d = (f.Tip - f.Hand).LengthSquared() > 0.01f ? (f.Tip - f.Hand).Normalized() : Vector2.Right;
        float k = t <= BendAt ? t / BendAt : (1 - t) / (1 - BendAt);
        return f.Hand.Lerp(f.Tip, t) + BackSide(f, d) * Bend * Mathf.Round(k * 4) / 4;
    }

    // a run of the edge between two fractions of the blade, split at the bend so it follows the curve
    private void EdgeRun(PixelPainter p, FighterPose f, float a, float b, Vector2 off)
    {
        if (b <= a) return;
        float m = Mathf.Clamp(BendAt, a, b);
        Run(p, f, a, m, off);
        Run(p, f, m, b, off);
    }

    private void Run(PixelPainter p, FighterPose f, float u, float v, Vector2 off)
    {
        if (v - u <= 0.001f) return;
        Vector2 s = Spine(f, u) + off, e = Spine(f, v) + off;
        p.Line(s.X, s.Y, e.X, e.Y, Edge, 1);
    }

    /// <inheritdoc />
    public override void OnBody(PixelPainter p, FighterPose f)
    {
        if (f.View != FighterView.Down) return;
        p.Line(f.LeanX + 4, f.ShoulderY - 3, f.LeanX - 6, f.ShoulderY + 11, Scabbard, 2);
        p.Rect(f.LeanX + 4, f.ShoulderY - 4, 2, 2, Iron);
        p.Rect(f.LeanX - 7, f.ShoulderY + 11, 2, 1, Iron);
    }

    /// <inheritdoc />
    public override void InHand(PixelPainter p, FighterPose f)
    {
        Vector2 hand = f.Hand, tip = f.Tip;
        Vector2 d = (tip - hand).LengthSquared() > 0.01f ? (tip - hand).Normalized() : Vector2.Right;
        Vector2 edge = -BackSide(f, d); // the edge is on the belly of the curve
        Vector2 k = Spine(f, BendAt);
        // the steel: two straight runs meeting at the bend (one clean step, no shimmer between frames)
        p.Line(hand.X + d.X * 2, hand.Y + d.Y * 2, k.X, k.Y, Core, 1);
        p.Line(k.X, k.Y, tip.X, tip.Y, Core, 1);
        // the edge, one pixel off the steel along the nearest axis (a clean run, no checkerboard), broken at the nicks
        Vector2 off = Mathf.Abs(edge.X) > Mathf.Abs(edge.Y) ? new Vector2(Mathf.Sign(edge.X), 0) : new Vector2(0, Mathf.Sign(edge.Y));
        float len = Mathf.Max(1, (tip - hand).Length());
        float from = 3 / len;
        foreach (float n in Nicks)
        {
            EdgeRun(p, f, from, n - 1.2f / len, off);
            from = n + 1.2f / len;
        }
        EdgeRun(p, f, from, 1 - 2 / len, off);
        p.Rect(tip.X, tip.Y, 1, 1, Edge); // the lightest pixel is the tip
        // the iron guard: a block 4 across and 2 thick
        for (int s = 2; s <= 3; s++)
            p.Line(hand.X + d.X * s - edge.X * 2, hand.Y + d.Y * s - edge.Y * 2, hand.X + d.X * s + edge.X * 2, hand.Y + d.Y * s + edge.Y * 2, Iron, 1);
        p.Rect(hand.X + d.X * 4, hand.Y + d.Y * 4, 1, 1, Rust);
        // the long grip behind the hand and the pommel
        p.Line(hand.X - d.X * 3, hand.Y - d.Y * 3, hand.X + d.X, hand.Y + d.Y, Grip, 2);
        p.Rect(hand.X - d.X * 4, hand.Y - d.Y * 4, 1, 1, Iron);
    }
}
