using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The drawing of a thing held in the swordsman's hand (T18): a weapon or a tool, drawn along the line from the hand to
/// the tip the pose gives, plus what it shows on the body (a scabbard on the back). One class per look
/// (<see cref="EspadaArt"/>, <see cref="FacaPedraArt"/>…), each in its own file under <c>src/Elements/Armas/</c>,
/// registered in <see cref="ArmaArts"/>. The numbers of the strike (reach, damage, timing) are data, in the item's
/// <c>weapon</c> block; the drawing only says how long it looks.
/// </summary>
public abstract class ArmaArt
{
    /// <summary>How long it looks, as a fraction of the sword's line from the hand (1 = the sword, 0.4 the knife).</summary>
    public abstract float Length { get; }

    /// <summary>True for a curved blade: the Reforço de Ki's glow then follows <see cref="Spine"/> instead of the straight line.</summary>
    public virtual bool Curved => false;

    /// <summary>The point at <paramref name="t"/> (0 the hand … 1 the tip) along the blade's back; straight by default.</summary>
    public virtual Vector2 Spine(FighterPose f, float t) => f.Hand.Lerp(f.Tip, t);

    /// <summary>What it shows on the body before the arms are drawn (a scabbard on the back); nothing by default.</summary>
    public virtual void OnBody(PixelPainter p, FighterPose f) { }

    /// <summary>The thing in the hand, from <paramref name="f"/>.Hand to <paramref name="f"/>.Tip (the hand's skin is drawn over its grip afterwards).</summary>
    public abstract void InHand(PixelPainter p, FighterPose f);
}

/// <summary>The tools (axe, pickaxe) in the hand: their drawing stays in <see cref="ToolArt"/>, this only places it.</summary>
public sealed class FerramentaArmaArt : ArmaArt
{
    private readonly string _kind;

    /// <summary>A tool of the kind <paramref name="kind"/> ("machado", "picareta").</summary>
    public FerramentaArmaArt(string kind) { _kind = kind; }

    /// <inheritdoc />
    public override float Length => 0.55f;

    /// <inheritdoc />
    public override void InHand(PixelPainter p, FighterPose f) => ToolArt.Draw(p, _kind, f.Hand, f.Tip);
}

/// <summary>The weapon drawings by art id (the item's <c>weapon.art</c>). A new look is a new class registered here (<c>/create arma</c> does it).</summary>
public static class ArmaArts
{
    private static readonly Dictionary<string, ArmaArt> Arts = new()
    {
        ["espada"] = new EspadaArt(),
        ["faca_pedra"] = new FacaPedraArt(),
        ["sabre_ferro"] = new SabreFerroArt(),
        ["machado"] = new FerramentaArmaArt("machado"),
        ["picareta"] = new FerramentaArmaArt("picareta"),
    };

    /// <summary>The drawing for <paramref name="art"/>, or null (empty hands, or an unknown id).</summary>
    public static ArmaArt? Get(string? art) => art != null && Arts.TryGetValue(art, out var a) ? a : null;

    /// <summary>True when a drawing is registered for <paramref name="art"/>.</summary>
    public static bool Has(string art) => Arts.ContainsKey(art);

    /// <summary>The registered ids.</summary>
    public static IEnumerable<string> Ids => Arts.Keys;
}
