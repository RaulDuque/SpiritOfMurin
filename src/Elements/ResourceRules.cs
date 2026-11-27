using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Elements;

/// <summary>What one strike on an element did.</summary>
public enum WorkResult { NotWorkable, WrongTool, Worked, Depleted, AlreadyDepleted }

/// <summary>
/// Working an element (pedido 14, campaign delivery 5): the right tool, the work done, what it gives. Pure
/// functions (no nodes): the region keeps the work per element in <c>WorldState</c> so a tree half cut stays half
/// cut after leaving the chunk, meditating or closing the game, and a used-up one gives its materials once.
/// </summary>
public static class ResourceRules
{
    /// <summary>
    /// One strike with a tool of <paramref name="toolKind"/> and <paramref name="power"/> on an element that already
    /// took <paramref name="work"/>. Returns the result and the work after it (capped at the toughness).
    /// </summary>
    public static (WorkResult Result, float Work) Strike(ElementDef e, float work, string toolKind, float power)
    {
        if (!e.Workable) return (WorkResult.NotWorkable, work);
        if (work >= e.Toughness) return (WorkResult.AlreadyDepleted, work);
        if (!e.Tools.Contains(toolKind)) return (WorkResult.WrongTool, work);
        float after = Mathf.Min(e.Toughness, work + Mathf.Max(0.01f, power));
        return (after >= e.Toughness - 0.0001f ? WorkResult.Depleted : WorkResult.Worked, after);
    }

    /// <summary>0-1: how far the work went (the picture shows the notches or cracks from it).</summary>
    public static float Progress(ElementDef e, float work) => e.Toughness <= 0 ? 0 : Mathf.Clamp(work / e.Toughness, 0, 1);

    /// <summary>The chance of a bonus with <paramref name="sor"/> points of SOR.</summary>
    public static float BonusChance(BonusDef b, int sor) => Mathf.Min(b.Cap, b.Chance + b.PerSor * Mathf.Max(0, sor));

    /// <summary>
    /// What an element gives when used up: each drop between min and max and each bonus whose roll falls under its
    /// chance. Every roll is fixed by (seed, element place id, item), so the same element always gives the same
    /// (no reloading for a better roll) and more SOR can only add a bonus, never take one away.
    /// </summary>
    public static List<(string Item, int Count)> Yield(ElementDef e, int seed, string placeId, int sor)
    {
        var got = new List<(string, int)>();
        foreach (var d in e.Drops)
        {
            int span = Mathf.Max(0, d.Max - d.Min);
            int n = d.Min + Mathf.FloorToInt(Roll(seed, placeId, "drop:" + d.Item) * (span + 1));
            if (n > 0) got.Add((d.Item, Mathf.Min(n, d.Max)));
        }
        foreach (var b in e.Bonus)
            if (Roll(seed, placeId, "bonus:" + b.Item) < BonusChance(b, sor)) got.Add((b.Item, b.Count));
        return got;
    }

    /// <summary>A fixed number in [0, 1) for (seed, place, salt).</summary>
    public static float Roll(int seed, string placeId, string salt)
    {
        unchecked
        {
            uint h = 2166136261u ^ (uint)seed;
            foreach (char ch in placeId) { h ^= ch; h *= 16777619u; }
            h ^= '|'; h *= 16777619u;
            foreach (char ch in salt) { h ^= ch; h *= 16777619u; }
            h ^= h >> 15; h *= 2246822519u; h ^= h >> 13; h *= 3266489917u; h ^= h >> 16;
            return (h & 0xFFFFFF) / 16777216f;
        }
    }
}
