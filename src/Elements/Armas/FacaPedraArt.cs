using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The stone knife (T18: the second weapon of the system): a short flint blade on a wooden knob.
/// <b>Where to change it:</b> the length in <see cref="Length"/>; the blade's thickness (2 px) and the grip's size
/// (3×3) in <see cref="InHand"/>; the colours in the constants.
/// </summary>
public sealed class FacaPedraArt : ArmaArt
{
    /// <summary>The flint blade and the wooden grip.</summary>
    public static readonly Color Flint = new("#8c8a80"), Hilt = new("#5c3a1e");

    /// <inheritdoc />
    public override float Length => 0.4f;

    /// <inheritdoc />
    public override void InHand(PixelPainter p, FighterPose f)
    {
        p.Line(f.Hand.X, f.Hand.Y, f.Tip.X, f.Tip.Y, Flint, 2);
        p.Rect(f.Hand.X - 1, f.Hand.Y - 1, 3, 3, Hilt);
    }
}
