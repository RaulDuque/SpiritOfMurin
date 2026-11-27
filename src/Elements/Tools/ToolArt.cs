using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The tools in the swordsman's hand (pedido 14, Entrega 5): the axe and the pickaxe, drawn from the hand to the
/// tip the pose gives (the same line the knife and the sword use), with the prototype's outline.
/// <b>Where to change them:</b> the handle in <see cref="Handle"/>, the axe's head in the "machado" case, the
/// pickaxe's head in the "picareta" case, the colours in the constants below. The bag's icons are in
/// <c>src/Game/ItemIcons.cs</c> (cases <c>machado_*</c>, <c>picareta_*</c>).
/// </summary>
public static class ToolArt
{
    /// <summary>Colours: handle, its shade, the stone head, its light edge, the binding.</summary>
    public static readonly Color Wood = new("#7a5a3a"), WoodShade = new("#5a3e28"), Stone = new("#8c8a80"), StoneEdge = new("#d6d4ca"), Binding = new("#c8b48a");

    /// <summary>Draws the tool <paramref name="kind"/> ("machado", "picareta") from <paramref name="hand"/> to <paramref name="tip"/>.</summary>
    public static void Draw(PixelPainter p, string kind, Vector2 hand, Vector2 tip)
    {
        Vector2 d = (tip - hand).LengthSquared() > 0.01f ? (tip - hand).Normalized() : Vector2.Right;
        Vector2 n = new(-d.Y, d.X);
        Handle(p, hand, tip);
        Vector2 h = tip; // the head sits at the far end
        switch (kind)
        {
            case "machado": // a wedge of stone on one side
                p.Line(h.X - n.X * 1, h.Y - n.Y * 1, h.X + n.X * 4, h.Y + n.Y * 4, Stone, 3);
                p.Line(h.X + n.X * 4 - d.X, h.Y + n.Y * 4 - d.Y, h.X + n.X * 4 + d.X, h.Y + n.Y * 4 + d.Y, StoneEdge, 1);
                p.Rect(h.X - d.X * 2, h.Y - d.Y * 2, 2, 2, Binding);
                break;
            case "picareta": // a point on each side, the long one forward
                p.Line(h.X - n.X * 3, h.Y - n.Y * 3, h.X + n.X * 5, h.Y + n.Y * 5, Stone, 2);
                p.Rect(h.X + n.X * 5, h.Y + n.Y * 5, 1, 1, StoneEdge);
                p.Rect(h.X - d.X * 2, h.Y - d.Y * 2, 2, 2, Binding);
                break;
        }
    }

    /// <summary>The wooden handle with its shaded side.</summary>
    private static void Handle(PixelPainter p, Vector2 hand, Vector2 tip)
    {
        p.Line(hand.X, hand.Y, tip.X, tip.Y, Wood, 2);
        p.Line(hand.X, hand.Y + 1, tip.X, tip.Y + 1, WoodShade, 1);
    }
}
