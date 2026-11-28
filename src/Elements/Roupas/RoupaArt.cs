using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The drawing of a robe on the swordsman (T18): the body of the robe in the three views and the two sleeves. One class
/// per look (<see cref="TunicaVerdeArt"/>, <c>TunicaViajanteArt</c>…), each in its own file under
/// <c>src/Elements/Roupas/</c>, registered in <see cref="RoupaArts"/>. The arm's bones (shoulder, hand) come from the
/// pose; the robe only dresses them. The hand's skin is the body's, drawn after the sleeve.
/// </summary>
public abstract class RoupaArt
{
    /// <summary>The arm's outline colour in profile (the same dark brown as the figure's outline).</summary>
    public static readonly Color ArmOutline = new("#17130e");

    /// <summary>The robe body: the matrix of the view, at the chest, with the skirt's swing.</summary>
    public abstract void Body(PixelPainter p, FighterPose f);

    /// <summary>The sleeve of the arm behind (the free hand), from <paramref name="sh"/> to <paramref name="hand"/>.</summary>
    public abstract void BackSleeve(PixelPainter p, FighterPose f, Vector2 sh, Vector2 hand);

    /// <summary>The sleeve of the arm in front (the weapon's hand), from <paramref name="sh"/> to <paramref name="hand"/>.</summary>
    public abstract void FrontSleeve(PixelPainter p, FighterPose f, Vector2 sh, Vector2 hand);
}

/// <summary>
/// A robe drawn from three pixel matrices (front, back, profile) and a palette, with cloth sleeves: what the approved
/// green tunic does. A new robe of the same cut copies a subclass and changes the rows and colours.
/// </summary>
public abstract class MatrixRoupaArt : RoupaArt
{
    /// <summary>The robe seen from the front (20 wide), from behind (20) and in profile looking right (16).</summary>
    protected abstract string[] Front { get; }
    /// <summary>The robe from behind.</summary>
    protected abstract string[] BackRows { get; }
    /// <summary>The robe in profile.</summary>
    protected abstract string[] Side { get; }
    /// <summary>The colours by letter.</summary>
    protected abstract IReadOnlyDictionary<char, Color> Palette { get; }
    /// <summary>Sleeve cloth, its shade, the cuff (front/back view), the cuff in profile and its gold line.</summary>
    protected abstract Color Cloth { get; }
    /// <summary>The sleeve's shade.</summary>
    protected abstract Color ClothShade { get; }
    /// <summary>The cuff seen from the front or the back.</summary>
    protected abstract Color Cuff { get; }
    /// <summary>The cuff in profile.</summary>
    protected abstract Color CuffSide { get; }
    /// <summary>The thin line at the end of the cuff in profile.</summary>
    protected abstract Color CuffLine { get; }

    /// <inheritdoc />
    public override void Body(PixelPainter p, FighterPose f)
    {
        int left = f.Profile ? -8 : -10;
        PixelRows.Paint(p, f.Back ? BackRows : f.Profile ? Side : Front, left + f.LeanX, -27, f.Mirror, Palette, f.Hem);
    }

    /// <inheritdoc />
    public override void BackSleeve(PixelPainter p, FighterPose f, Vector2 sh, Vector2 hand)
    {
        if (f.Profile)
        {
            p.Line(sh.X, sh.Y, hand.X, hand.Y, ArmOutline, 4);
            p.Line(sh.X, sh.Y, hand.X, hand.Y, ClothShade, 2);
            p.Rect(hand.X - 1, hand.Y - 1, 2, 2, CuffSide);
        }
        else
        {
            p.Line(sh.X, sh.Y, hand.X, hand.Y, ClothShade, 4);
            p.Line(sh.X, sh.Y, hand.X, hand.Y, Cloth, 2);
            p.Rect(hand.X - 1, hand.Y - 2, 3, 2, Cuff);
        }
    }

    /// <inheritdoc />
    public override void FrontSleeve(PixelPainter p, FighterPose f, Vector2 sh, Vector2 hand)
    {
        if (f.Profile)
        {
            // a dark outline separates the arm from the robe in profile
            p.Line(sh.X, sh.Y, hand.X, hand.Y, ArmOutline, 5);
            p.Line(sh.X, sh.Y, hand.X, hand.Y, ClothShade, 3);
            p.Line(sh.X, sh.Y, hand.X, hand.Y, Cloth, 2);
            p.Rect(hand.X - 2, hand.Y - 3, 4, 2, CuffSide);
            p.Rect(hand.X - 2, hand.Y - 1, 4, 1, CuffLine);
        }
        else
        {
            p.Line(sh.X, sh.Y, hand.X, hand.Y, ClothShade, 4);
            p.Line(sh.X, sh.Y, hand.X, hand.Y, Cloth, 2);
            float midX = Mathf.Round((sh.X + hand.X) * 0.5f), midY = Mathf.Round((sh.Y + hand.Y) * 0.5f);
            p.Rect(midX - 1, midY + 1, 3, 2, ClothShade); // the elbow's fold
            p.Rect(hand.X - 1, hand.Y - 2, 3, 2, Cuff);
        }
    }
}

/// <summary>The robe drawings by art id (the item's <c>wear.art</c>). A new look is a new class registered here (<c>/create roupa</c> does it).</summary>
public static class RoupaArts
{
    /// <summary>The robe the swordsman wears with nothing else on: the approved green tunic.</summary>
    public const string Default = "tunica_verde";

    private static readonly Dictionary<string, RoupaArt> Arts = new()
    {
        [Default] = new TunicaVerdeArt(),
        ["tunica_viajante"] = new TunicaViajanteArt(),
    };

    /// <summary>The robe drawn for <paramref name="art"/>; the green tunic for an unknown or empty id.</summary>
    public static RoupaArt Get(string? art) => art != null && Arts.TryGetValue(art, out var a) ? a : Arts[Default];

    /// <summary>True when a drawing is registered for <paramref name="art"/>.</summary>
    public static bool Has(string art) => Arts.ContainsKey(art);

    /// <summary>The registered ids.</summary>
    public static IEnumerable<string> Ids => Arts.Keys;
}
