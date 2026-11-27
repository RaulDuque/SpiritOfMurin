using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Elements;

/// <summary>The life of a worked element: standing (maybe with notches or cracks), falling or breaking, used up.</summary>
public enum ElementState { Standing, Falling, Depleted }

/// <summary>Everything a drawing needs about one element at one moment (all in its local pixels, origin at the foot).</summary>
public struct ElementPose
{
    /// <summary>Seeded number for the small differences between two of the same element.</summary>
    public int Variation;
    /// <summary>Seconds running (idle sway).</summary>
    public float Time;
    /// <summary>The wind's push here (−1.5 … 2.5, along the wind; 0 when calm).</summary>
    public float Wind;
    /// <summary>0-1: the work done (notches, cracks, cut stalks).</summary>
    public float Progress;
    /// <summary>Standing, falling, used up.</summary>
    public ElementState State;
    /// <summary>0-1 through the fall or the break.</summary>
    public float FallT;
    /// <summary>The fall's side: −1 to the left, +1 to the right.</summary>
    public float FallSide;
    /// <summary>Bushes: how much and toward where a passing body bends the leaves (px).</summary>
    public Vector2 Bend;
    /// <summary>0-1: a flash right after a strike.</summary>
    public float Flash;
    /// <summary>Seconds since the last strike that worked it (large when never struck).</summary>
    public float SinceHit;
}

/// <summary>
/// The drawing of a kind of element. One class per look (<c>PinheiroArt</c>, <c>RochaArt</c>…), each in its own file
/// under <c>src/Elements/</c>: that is where its shape, palette, sizes and animation live. The data (<see cref="ElementDef"/>)
/// may pass extra numbers or colours through <c>look</c>; with no data the drawing is the original one (the Vale).
/// </summary>
public abstract class ElementArt
{
    /// <summary>The outline every shape gets (the prototype's 1 px dark outline).</summary>
    public static readonly Color Outline = new("#16140f");
    /// <summary>The soft shadow on the ground.</summary>
    public static readonly Color Shadow = new(8 / 255f, 10 / 255f, 8 / 255f, 0.32f);

    /// <summary>Draws the element on <paramref name="c"/> (local pixels, origin at its foot).</summary>
    public abstract void Draw(CanvasItem c, ElementDef? def, in ElementPose p);

    /// <summary>A whole-pixel rectangle.</summary>
    protected static void Fill(CanvasItem c, float x, float y, float w, float h, Color col) => c.DrawRect(new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), col);

    /// <summary>The variation hash the original drawings used (0-96).</summary>
    public static int Hv(int variation) => (int)((((uint)variation * 2654435761u) >> 7) % 97u);
}

/// <summary>The art classes by id. A new look is a new class registered here (the <c>/create</c> command does it).</summary>
public static class ElementArts
{
    private static readonly Dictionary<string, ElementArt> Arts = new()
    {
        ["pinheiro"] = new PinheiroArt(),
        ["bambu"] = new BambuArt(),
        ["arbusto"] = new ArbustoArt(),
        ["rocha"] = new RochaArt(),
    };

    /// <summary>The art with that id (the pine when unknown, so a typo still draws something).</summary>
    public static ElementArt Get(string id) => Arts.TryGetValue(id, out var a) ? a : Arts["pinheiro"];

    /// <summary>True when an art with that id exists.</summary>
    public static bool Has(string id) => Arts.ContainsKey(id);

    /// <summary>Every art id (the inspection scene lists them).</summary>
    public static IEnumerable<string> Ids => Arts.Keys;
}
