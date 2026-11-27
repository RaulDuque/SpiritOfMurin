using Godot;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Progression;

/// <summary>
/// What a hunt, a plant or a fire gives, from the proficiency grades, the attributes and the seed.
/// Pure functions; every chance is rolled deterministically from (seed, object id), so saving and
/// loading never rerolls a result.
/// </summary>
public static class HarvestRules
{
    /// <summary>Result of skinning: item obtained, Ki spent and the grade the material came out at.</summary>
    public readonly record struct Hide(string Item, float KiSpent, int EffectiveGrade);

    /// <summary>
    /// Skins a carcass of <paramref name="kind"/> ("rabbit", "boar"). With <paramref name="useKi"/>, from
    /// Esfolar grade 1 and with enough Ki, the material comes out one grade higher (capped at 3).
    /// </summary>
    public static Hide Skin(ProgressionData d, string kind, int esfolar, bool useKi, float ki)
    {
        int g = Mathf.Clamp(esfolar, 0, 3);
        float spent = 0;
        if (useKi && g >= 1 && ki >= d.Base.KiPreserveCost) { g = Mathf.Min(3, g + 1); spent = d.Base.KiPreserveCost; }
        string[] table = d.Hides.TryGetValue(kind, out var t) ? t : new[] { "pele_rasgada" };
        return new Hide(table[Mathf.Min(g, table.Length - 1)], spent, g);
    }

    /// <summary>
    /// Takes a spiritual-compatible part (the boar's tusks). At Esfolar grade 3 with Ki it becomes the
    /// spiritual item; otherwise the plain one. Returns the item and the Ki spent.
    /// </summary>
    public static (string Item, float KiSpent) SpiritualPart(ProgressionData d, string kind, string plainItem, int esfolar, bool useKi, float ki)
    {
        if (useKi && esfolar >= 3 && ki >= d.Base.KiPreserveCost && d.Spiritual.TryGetValue(kind, out var sp))
            return (sp.Item, d.Base.KiPreserveCost);
        return (plainItem, 0);
    }

    /// <summary>Pieces of meat a carcass gives at a Preparo de Caça grade.</summary>
    public static int MeatCount(ProgressionData d, int preparo) => preparo >= d.MeatExtraGrade ? 2 : 1;

    /// <summary>Vigor restored by a cooked meat, for the Preparo grade of the one who cooked it.</summary>
    public static float CookedVigor(ProgressionData d, float baseVigor, int preparo) => baseVigor * (1 + d.CookedVigorBonusPerGrade * Mathf.Clamp(preparo, 0, 3));

    /// <summary>The herb item a harvest gives at an Erboristeria grade.</summary>
    public static string HerbItem(ProgressionData d, int erboristeria) => d.HerbQuality[Mathf.Clamp(erboristeria, 0, d.HerbQuality.Length - 1)];

    /// <summary>The 0-1 roll for a secondary material of an object (fixed for a seed and an id).</summary>
    public static float SecondaryRoll(int seed, string id)
    {
        uint h = 2166136261u;
        foreach (char c in id) h = (h ^ c) * 16777619u;
        return (PlainsGenerator.Hash(seed, (int)h, 7, 4242) >> 8) / 16777216f;
    }

    /// <summary>True when the object yields its secondary material at this chance.</summary>
    public static bool HasSecondary(int seed, string id, float chance) => SecondaryRoll(seed, id) < chance;

    /// <summary>XP multiplier of a hunt by level difference (creature − character).</summary>
    public static float XpMultiplier(ProgressionData d, int creatureLevel, int playerLevel) =>
        Mathf.Clamp(1 + d.LevelDiffStep * (creatureLevel - playerLevel), d.LevelDiffMin, d.LevelDiffMax);

    /// <summary>XP of a hunt.</summary>
    public static int HuntXp(ProgressionData d, string kind, int creatureLevel, int playerLevel) =>
        Mathf.Max(1, Mathf.RoundToInt((d.Xp.HuntPerLevel.TryGetValue(kind, out int x) ? x : 5) * creatureLevel * XpMultiplier(d, creatureLevel, playerLevel)));

    /// <summary>The level of a creature, fixed for a seed and its id, within its kind's range.</summary>
    public static int CreatureLevel(ProgressionData d, int seed, string kind, string id)
    {
        if (!d.Creatures.TryGetValue(kind, out var c)) return 1;
        int span = Mathf.Max(1, c.MaxLevel - c.MinLevel + 1);
        return c.MinLevel + (int)(SecondaryRoll(seed, "lvl:" + id) * span) % span;
    }
}
