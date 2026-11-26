using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Creatures;

/// <summary>
/// A hunted animal lying on the ground. Each part (meat, hide) is taken once with E and a blade in hand;
/// the world state remembers what was taken. The node disappears when nothing is left.
/// </summary>
public partial class Carcass : Node2D
{
    /// <summary>Id of the animal it was (world state key).</summary>
    public string CreatureId { get; set; } = "";

    /// <summary>True for a boar (drawn bigger and darker), false for a rabbit.</summary>
    public bool IsBoar { get; set; }

    /// <summary>Item ids the carcass yields, in the order they are taken.</summary>
    public List<string> Parts { get; set; } = new();

    private static readonly Color Outline = new("#16140f"), Fur = new("#8a7a5e"), FurLight = new("#b8a98a"), BoarFur = new("#4a3a2c"),
        BoarDark = new("#2c2219"), Tusk = new("#e6dcc0"), Blood = new("#7a3a32");

    private void R(float x, float y, float w, float h, Color c) => DrawRect(new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), c);

    /// <inheritdoc />
    public override void _Draw()
    {
        R(-10, -1, 20, 3, new Color(Blood, 0.55f));
        if (IsBoar)
        {
            R(-14, -9, 28, 10, Outline); R(-13, -8, 26, 8, BoarFur); R(-13, -3, 26, 3, BoarDark); R(12, -6, 3, 2, Tusk);
            R(-8, 0, 3, 2, BoarDark); R(4, 0, 3, 2, BoarDark);
        }
        else
        {
            R(-7, -5, 14, 6, Outline); R(-6, -4, 12, 4, Fur); R(-6, -2, 12, 2, FurLight); R(5, -6, 2, 3, Fur); R(-7, -3, 2, 2, FurLight);
        }
    }
}
