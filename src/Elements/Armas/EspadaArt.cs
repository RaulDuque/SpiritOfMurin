using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The prologue's sword (T18: the first weapon of the system, the creator's approved drawing): polished blade with a
/// pale-gold edge, gold guard and pommel, dark wooden grip; the scabbard across the back when facing the camera.
/// <b>Where to change it:</b> the blade's length in <see cref="Length"/> (1 = the reference line from the hand);
/// the edge in the second line of <see cref="InHand"/>; the guard's width in the third (±2 px across); the grip in
/// the fourth; the scabbard in <see cref="OnBody"/>; the colours in the constants.
/// </summary>
public sealed class EspadaArt : ArmaArt
{
    /// <summary>Blade, its edge, the grip, the guard and pommel, the scabbard.</summary>
    public static readonly Color Blade = new("#f7f9fc"), BladeEdge = new("#d6c7a1"), Hilt = new("#5c3a1e"), Gold = new("#f5cb5c"), Scabbard = new("#3d2817");

    /// <inheritdoc />
    public override float Length => 1f;

    /// <inheritdoc />
    public override void OnBody(PixelPainter p, FighterPose f)
    {
        if (f.View != FighterView.Down) return; // the scabbard across the back shows only from the front
        p.Line(f.LeanX + 4, f.ShoulderY - 3, f.LeanX - 6, f.ShoulderY + 11, Scabbard, 2);
        p.Rect(f.LeanX + 4, f.ShoulderY - 4, 2, 2, Gold);
    }

    /// <inheritdoc />
    public override void InHand(PixelPainter p, FighterPose f)
    {
        Vector2 hand = f.Hand, tip = f.Tip;
        p.Line(hand.X, hand.Y, tip.X, tip.Y, Blade, 1);
        Vector2 dir = (tip - hand).Normalized();
        Vector2 perp = new(-dir.Y, dir.X);
        p.Line(hand.X + dir.X * 3 + perp.X, hand.Y + dir.Y * 3 + perp.Y,
               tip.X - dir.X * 2 + perp.X, tip.Y - dir.Y * 2 + perp.Y, BladeEdge, 1);
        p.Line(hand.X + dir.X * 2 - perp.X * 2, hand.Y + dir.Y * 2 - perp.Y * 2,
               hand.X + dir.X * 2 + perp.X * 2, hand.Y + dir.Y * 2 + perp.Y * 2, Gold, 1);
        p.Line(hand.X - dir.X * 2, hand.Y - dir.Y * 2, hand.X + dir.X, hand.Y + dir.Y, Hilt, 2);
        p.Rect(hand.X - dir.X * 3, hand.Y - dir.Y * 3, 1, 1, Gold);
    }
}
