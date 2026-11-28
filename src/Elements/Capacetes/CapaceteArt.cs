using System.Collections.Generic;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The drawing of a hat or helmet on the swordsman (T18), drawn over the head (<see cref="CabecaArt"/>). One class per
/// look, each in its own file under <c>src/Elements/Capacetes/</c>, registered in <see cref="CapaceteArts"/>. A hat says
/// how much of the hair it covers (<see cref="HairCover"/>): the head then leaves those rows out, so the bun never pokes
/// through and comes back whole when the hat is taken off.
/// </summary>
public abstract class CapaceteArt
{
    /// <summary>Rows of the head's top left out under the hat: 0 none, <see cref="CabecaArt.BunRows"/> the bun, <see cref="CabecaArt.BandRows"/> the bun and its band (and the ribbon).</summary>
    public virtual int HairCover => CabecaArt.BunRows;

    /// <summary>The hat over the head, in the pose's view (the head's top is at <paramref name="f"/>.HeadY).</summary>
    public abstract void Draw(PixelPainter p, FighterPose f);
}

/// <summary>The hat drawings by art id (the item's <c>wear.art</c>). A new look is a new class registered here (<c>/create capacete</c> does it).</summary>
public static class CapaceteArts
{
    private static readonly Dictionary<string, CapaceteArt> Arts = new()
    {
        ["chapeu_bambu"] = new ChapeuBambuArt(),
    };

    /// <summary>Registers a drawing under <paramref name="art"/> (the picture checks use it for board-only variants).</summary>
    public static void Register(string art, CapaceteArt a) => Arts[art] = a;

    /// <summary>The drawing for <paramref name="art"/>, or null (bare head, or an unknown id).</summary>
    public static CapaceteArt? Get(string? art) => art != null && Arts.TryGetValue(art, out var a) ? a : null;

    /// <summary>True when a drawing is registered for <paramref name="art"/>.</summary>
    public static bool Has(string art) => Arts.ContainsKey(art);

    /// <summary>The registered ids.</summary>
    public static IEnumerable<string> Ids => Arts.Keys;
}
