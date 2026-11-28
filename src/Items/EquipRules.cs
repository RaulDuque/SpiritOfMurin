using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Items;

/// <summary>A weapon's own technique (T18): its name, the weapon it works with, whether it is in the game yet, how it is found and its numbers.</summary>
public sealed class WeaponTechniqueDef
{
    /// <summary>Id, filled from the key.</summary>
    public string Id { get; set; } = "";
    /// <summary>Name shown in the Livro de Técnicas.</summary>
    public string Name { get; set; } = "";
    /// <summary>The weapon's art it works with ("sabre_ferro").</summary>
    public string WeaponKind { get; set; } = "";
    /// <summary>"jogavel" or "futura".</summary>
    public string State { get; set; } = "futura";
    /// <summary>What it does, in words.</summary>
    public string Description { get; set; } = "";
    /// <summary>How it is found.</summary>
    public DiscoverDef Discover { get; set; } = new();
    /// <summary>Meia-Lua: the last cut's half angle (degrees).</summary>
    public float ArcDegrees { get; set; }
    /// <summary>Meia-Lua: the extra Vigor of the last cut.</summary>
    public float VigorExtra { get; set; }
    /// <summary>Meia-Lua: the trail's length multiplier.</summary>
    public float Trail { get; set; } = 1;
    /// <summary>True when the technique exists in the game now.</summary>
    public bool Playable => State == "jogavel";
}

/// <summary>How a weapon technique is found: hits landed with the weapon.</summary>
public sealed class DiscoverDef
{
    /// <summary>Cuts that hit, with the weapon in hand; 0 = not discoverable yet.</summary>
    public int Cuts { get; set; }
}

/// <summary>The equipment's general rules (<c>assets/data/equipamento/_regras.json</c>).</summary>
public sealed class EquipSettings
{
    /// <summary>The most the worn pieces together hold back of the damage (fraction), on top of CON.</summary>
    public float DefenseCap { get; set; } = 0.4f;
    /// <summary>What the traveller's bundle holds (item ids).</summary>
    public List<string> Trouxa { get; set; } = new();
}

/// <summary>
/// The equipment's pure rules (T18): what is worn in each slot, how defense and weight add up (decision 1: defense and
/// weight on the Vigor), the blade's numbers for the body, the bag's comparison lines and the weapon techniques found
/// by practice (decision 2). No Godot nodes: unit-tested in <c>tests/unit/Plains/EquipRulesTests.cs</c>.
/// </summary>
public static class EquipRules
{
    /// <summary>The two wear slots: the robe and the head.</summary>
    public static readonly string[] Slots = { "corpo", "cabeca" };

    /// <summary>The robe worn when nothing else is (the creator's green tunic).</summary>
    public const string BaseRobe = "tunica_verde";

    /// <summary>Parses the rules file.</summary>
    public static EquipSettings ParseSettings(string json) => JsonSerializer.Deserialize<EquipSettings>(json, PlainsGenSettings.Json) ?? new EquipSettings();

    /// <summary>Parses the weapon techniques file (keys starting with "_" are skipped).</summary>
    public static Dictionary<string, WeaponTechniqueDef> ParseTechniques(string json)
    {
        var all = new Dictionary<string, WeaponTechniqueDef>();
        using var doc = JsonDocument.Parse(json);
        foreach (var p in doc.RootElement.EnumerateObject())
        {
            if (p.Name.StartsWith("_")) continue;
            var t = p.Value.Deserialize<WeaponTechniqueDef>(PlainsGenSettings.Json) ?? new WeaponTechniqueDef();
            t.Id = p.Name;
            all[p.Name] = t;
        }
        return all;
    }

    /// <summary>Damage held back by the worn pieces together (fraction), capped.</summary>
    public static float Defense(IEnumerable<ItemDef?> worn, EquipSettings s) =>
        System.Math.Clamp(worn.Where(w => w?.Wear != null).Sum(w => w!.Wear!.Defense), 0f, s.DefenseCap);

    /// <summary>The extra Vigor fraction the worn pieces add to a cut and a step.</summary>
    public static float Weight(IEnumerable<ItemDef?> worn) => System.Math.Max(0f, worn.Where(w => w?.Wear != null).Sum(w => w!.Wear!.Weight));

    /// <summary>The damage taken multiplier: CON's (<paramref name="conMultiplier"/>) and then the clothes'.</summary>
    public static float DamageTaken(float conMultiplier, float defense) => conMultiplier * (1 - defense);

    /// <summary>A cut's or a step's Vigor cost with the clothes' weight.</summary>
    public static float VigorCost(float baseCost, float weight) => baseCost * (1 + weight);

    /// <summary>The body's numbers for a weapon (null when it has none).</summary>
    public static Characters.Player.BladeStats? Blade(WeaponDef? w) => w == null ? null : new(w.Reach, w.Damage, w.Chain, w.Startup, w.Active, w.Recovery, w.CancelAt, w.Lunge,
        w.ThirdExtra, w.ThirdRecovery, w.ThirdCancel, w.ThirdLunge, System.Math.Max(1, w.ThirdMult), w.Kind == "curta");

    /// <summary>One comparison line of the bag: what, now, after, and whether it gets better or worse.</summary>
    public readonly record struct Diff(string What, string Now, string After, bool Better, bool Worse);

    /// <summary>
    /// How a weapon compares with the one in hand (null = empty hands): reach (px, the same number the strike uses),
    /// damage of a cut, cuts in the chain, the damage of the chain and the time of one cut.
    /// </summary>
    public static List<Diff> CompareWeapons(WeaponDef? now, WeaponDef other, float damageMultiplier = 1)
    {
        var l = new List<Diff>();
        void Add(string what, float a, float b, string fmt, bool higherBetter)
        {
            string A = now == null ? "—" : a.ToString(fmt), B = b.ToString(fmt);
            bool better = now == null || (higherBetter ? b > a : b < a), worse = now != null && (higherBetter ? b < a : b > a);
            if (now != null && System.Math.Abs(a - b) < 0.0001f) { better = worse = false; }
            l.Add(new Diff(what, A, B, better, worse));
        }
        Add("Alcance (px)", now?.Reach ?? 0, other.Reach, "0", true);
        Add("Dano por corte", (now?.Damage ?? 0) * damageMultiplier, other.Damage * damageMultiplier, "0.#", true);
        Add("Cortes no combo", now?.Chain ?? 0, other.Chain, "0", true);
        Add("Dano do combo", ChainDamage(now) * damageMultiplier, ChainDamage(other) * damageMultiplier, "0.#", true);
        Add("Tempo de um corte (s)", now == null ? 0 : now.Startup + now.Active + now.Recovery, other.Startup + other.Active + other.Recovery, "0.00", false);
        return l;
    }

    /// <summary>The damage of a whole chain (the last cut of a chain of 3 or more multiplied).</summary>
    public static float ChainDamage(WeaponDef? w)
    {
        if (w == null) return 0;
        float d = 0;
        for (int n = 1; n <= w.Chain; n++) d += w.Damage * (w.Chain >= 3 && n == w.Chain ? System.Math.Max(1, w.ThirdMult) : 1);
        return d;
    }

    /// <summary>How a piece to wear compares with the one worn in its slot: defense (%) and weight (% more Vigor per cut and step).</summary>
    public static List<Diff> CompareWear(WearDef? now, WearDef other)
    {
        float da = (now?.Defense ?? 0) * 100, db = other.Defense * 100, wa = (now?.Weight ?? 0) * 100, wb = other.Weight * 100;
        return new()
        {
            new("Defesa", $"{da:0}%", $"{db:0}%", db > da, db < da),
            new("Peso (vigor por corte e passo)", $"+{wa:0}%", $"+{wb:0}%", wb < wa, wb > wa),
        };
    }

    /// <summary>The playable techniques of a weapon art whose discovery count was reached with <paramref name="cuts"/> hits.</summary>
    public static IEnumerable<WeaponTechniqueDef> Found(IReadOnlyDictionary<string, WeaponTechniqueDef> all, string weaponArt, int cuts) =>
        all.Values.Where(t => t.Playable && t.WeaponKind == weaponArt && t.Discover.Cuts > 0 && cuts >= t.Discover.Cuts);
}
