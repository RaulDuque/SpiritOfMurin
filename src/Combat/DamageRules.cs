using Godot;

namespace SpiritOfMurin.Combat;

/// <summary>One resolved hit of the character: how much it takes away and whether it was critical.</summary>
public readonly record struct StrikeResult(float Amount, bool Critical);

/// <summary>
/// The damage the character deals and takes (pedido 14). Pure functions, fed by the sheet
/// (<see cref="SpiritOfMurin.Progression.PlayerStats"/>) and one roll in [0, 1), so the tests can fix the roll.
/// <list type="bullet">
/// <item>outgoing = base × scale (the Reforço de Ki) × FOR (melee only); critical when roll &lt; chance, then × the critical multiplier;</item>
/// <item>incoming = amount × the fraction CON still lets through.</item>
/// </list>
/// Numbers in <c>assets/data/plains/plains_progression.json</c> (<c>for_melee_damage</c>, <c>con_damage_reduction</c>,
/// <c>con_reduction_cap</c>, <c>crit_chance</c>, <c>sor_crit_chance</c>, <c>crit_cap</c>, <c>crit_multiplier</c>).
/// </summary>
public static class DamageRules
{
    /// <summary>
    /// A hit of the character. <paramref name="strength"/> is the FOR multiplier (1 for the Ki shots),
    /// <paramref name="critChance"/> and <paramref name="critMultiplier"/> come from the sheet, <paramref name="roll"/> is in [0, 1).
    /// </summary>
    public static StrikeResult Outgoing(float baseDamage, float scale, float strength, float critChance, float critMultiplier, float roll)
    {
        float amount = Mathf.Max(0, baseDamage) * scale * strength;
        bool crit = amount > 0 && roll < critChance;
        return new StrikeResult(crit ? amount * critMultiplier : amount, crit);
    }

    /// <summary>A hit the character takes: <paramref name="taken"/> is the fraction CON lets through (1 = all).</summary>
    public static float Incoming(float amount, float taken) => Mathf.Max(0, amount) * Mathf.Clamp(taken, 0, 1);

    /// <summary>How a damage number reads on screen: whole numbers plain, fractions with one decimal (comma).</summary>
    public static string Format(float amount)
    {
        float r = Mathf.Round(amount * 10) / 10;
        return Mathf.IsEqualApprox(r, Mathf.Round(r)) ? $"{Mathf.RoundToInt(r)}" : r.ToString("0.0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
    }
}
