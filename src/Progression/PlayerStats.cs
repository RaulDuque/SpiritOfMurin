using System;
using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Progression;

/// <summary>
/// The character's level, XP, unspent points and the six attributes (VIT, INT, CON, AGI, SOR, KI),
/// with every derived number. Pure logic (no nodes), fed by <see cref="ProgressionData"/>.
/// </summary>
public sealed class PlayerStats
{
    /// <summary>Attribute ids in sheet order.</summary>
    public static readonly string[] AttributeIds = { "vit", "int", "con", "agi", "sor", "ki" };

    /// <summary>Short and long names of each attribute.</summary>
    public static readonly Dictionary<string, (string Short, string Long)> Names = new()
    {
        ["vit"] = ("VIT", "Vitalidade"), ["int"] = ("INT", "Inteligência"), ["con"] = ("CON", "Constituição"),
        ["agi"] = ("AGI", "Agilidade"), ["sor"] = ("SOR", "Sorte"), ["ki"] = ("KI", "Energia espiritual"),
    };

    private readonly ProgressionData _d;
    private readonly int _baseHealth;
    private readonly Dictionary<string, int> _attr = new();

    /// <summary>Current level (starts at 1).</summary>
    public int Level { get; private set; } = 1;
    /// <summary>XP gathered toward the next level.</summary>
    public int Xp { get; private set; }
    /// <summary>Attribute points not yet spent.</summary>
    public int Points { get; private set; }
    /// <summary>Total XP ever gained.</summary>
    public int TotalXp { get; private set; }

    /// <summary>Creates level-1 stats. <paramref name="baseHealth"/> is the Vida of a level-1 character (PlayerTuning.MaxHealth).</summary>
    public PlayerStats(ProgressionData data, int baseHealth)
    {
        _d = data; _baseHealth = baseHealth;
        foreach (var a in AttributeIds) _attr[a] = 0;
    }

    /// <summary>Points invested in an attribute.</summary>
    public int Get(string attr) => _attr.TryGetValue(attr, out int v) ? v : 0;

    /// <summary>XP needed to go from <paramref name="level"/> to the next.</summary>
    public int XpToNext(int level) => Mathf.RoundToInt(_d.XpBase * Mathf.Pow(_d.XpGrowth, level - 1));

    /// <summary>XP still missing for the next level.</summary>
    public int XpMissing => XpToNext(Level) - Xp;

    /// <summary>Adds XP; returns how many levels were gained (each gives <see cref="ProgressionData.PointsPerLevel"/>).</summary>
    public int AddXp(int amount)
    {
        if (amount <= 0) return 0;
        Xp += amount; TotalXp += amount;
        int gained = 0;
        while (Xp >= XpToNext(Level)) { Xp -= XpToNext(Level); Level++; Points += _d.PointsPerLevel; gained++; }
        return gained;
    }

    /// <summary>Spends one point on an attribute. False (and no change) when there are no points or the id is unknown.</summary>
    public bool Spend(string attr)
    {
        if (Points <= 0 || !_attr.ContainsKey(attr)) return false;
        Points--; _attr[attr]++;
        return true;
    }

    /// <summary>Restores a saved state.</summary>
    public void Restore(int level, int xp, int points, int totalXp, IDictionary<string, int> attrs)
    {
        Level = Math.Max(1, level); Xp = Math.Max(0, xp); Points = Math.Max(0, points); TotalXp = Math.Max(0, totalXp);
        foreach (var a in AttributeIds) _attr[a] = attrs.TryGetValue(a, out int v) ? Math.Max(0, v) : 0;
    }

    /// <summary>A copy of the invested points.</summary>
    public Dictionary<string, int> Attributes => new(_attr);

    // ------------------------------------------------------------------ derived values

    private float Agi(float perPoint) => Mathf.Min(_d.PerPoint.AgiCap, Get("agi") * perPoint);

    /// <summary>Maximum Vida.</summary>
    public int MaxHealth => _baseHealth + Mathf.RoundToInt(Get("vit") * _d.PerPoint.VitMaxHealth);
    /// <summary>Potency multiplier of medicinal preparations.</summary>
    public float MedicinePotency => _d.Base.MedicinePotency + Get("int") * _d.PerPoint.IntMedicinePotency;
    /// <summary>Maximum vigor.</summary>
    public float MaxVigor => _d.Base.MaxVigor + Get("con") * _d.PerPoint.ConMaxVigor;
    /// <summary>Vigor regained per second.</summary>
    public float VigorRegen => _d.Base.VigorRegen * (1 + Get("con") * _d.PerPoint.ConVigorRegen);
    /// <summary>Walking speed multiplier.</summary>
    public float MoveSpeedMultiplier => 1 + Agi(_d.PerPoint.AgiMoveSpeed);
    /// <summary>Step distance multiplier.</summary>
    public float StepDistanceMultiplier => 1 + Agi(_d.PerPoint.AgiStepDistance);
    /// <summary>Recovery time multiplier of cuts and steps (below 1 is faster).</summary>
    public float RecoveryMultiplier => 1 - Agi(_d.PerPoint.AgiRecovery);
    /// <summary>Chance of a secondary material.</summary>
    public float SecondaryChance => Mathf.Min(0.9f, _d.Base.SecondaryChance + Get("sor") * _d.PerPoint.SorSecondaryChance);
    /// <summary>Maximum Ki.</summary>
    public float MaxKi => _d.Base.MaxKi + Get("ki") * _d.PerPoint.KiMaxKi;

    /// <summary>
    /// What one more point in <paramref name="attr"/> changes, as lines "label: now → after", computed by
    /// applying the point to a copy (so the preview can never disagree with the real effect).
    /// </summary>
    public List<string> Preview(string attr, float herbRegen = 4)
    {
        var after = Clone(); after._attr[attr] = Get(attr) + 1;
        static string P(float v) => $"{v * 100:0}%";
        return attr switch
        {
            "vit" => new() { $"Vida máxima: {MaxHealth} → {after.MaxHealth}" },
            "int" => new() { $"Potência dos preparos medicinais: {P(MedicinePotency)} → {P(after.MedicinePotency)}", $"Grama Sangue-de-Coelho comum: {herbRegen * MedicinePotency:0.0} → {herbRegen * after.MedicinePotency:0.0} de Vida" },
            "con" => new() { $"Vigor máximo: {MaxVigor:0} → {after.MaxVigor:0}", $"Recuperação do vigor: {VigorRegen:0.0}/s → {after.VigorRegen:0.0}/s" },
            "agi" => new() { $"Velocidade ao andar: {P(MoveSpeedMultiplier)} → {P(after.MoveSpeedMultiplier)}", $"Distância do passo: {P(StepDistanceMultiplier)} → {P(after.StepDistanceMultiplier)}", $"Tempo de recuperação do corte e do passo: {P(RecoveryMultiplier)} → {P(after.RecoveryMultiplier)}" },
            "sor" => new() { $"Chance de material secundário (tendão na caça, uma planta ou lasca a mais na coleta): {P(SecondaryChance)} → {P(after.SecondaryChance)}" },
            "ki" => new() { $"Ki máximo: {MaxKi:0} → {after.MaxKi:0}", $"Esfolas com Ki seguidas (custo {_d.Base.KiPreserveCost:0}): {Mathf.FloorToInt(MaxKi / _d.Base.KiPreserveCost)} → {Mathf.FloorToInt(after.MaxKi / _d.Base.KiPreserveCost)}" },
            _ => new(),
        };
    }

    private PlayerStats Clone()
    {
        var c = new PlayerStats(_d, _baseHealth);
        c.Restore(Level, Xp, Points, TotalXp, _attr);
        return c;
    }
}
