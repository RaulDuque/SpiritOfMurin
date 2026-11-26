using Godot;
using SpiritOfMurin.Progression;

namespace SpiritOfMurin.Creatures;

/// <summary>
/// Applies a creature's level to its tuning: a copy of the base tuning with the per-level changes from
/// <c>plains_progression.json</c>. Only strength changes with level (hearing, speed, health, damage);
/// the timing the player learns to read — the rabbit's freeze, the boar's 0.8 s warning and its
/// recovery — stays the same at every level.
/// </summary>
public static class CreatureLevels
{
    private static int Above(CreatureDef d, int level) => Mathf.Max(0, level - d.MinLevel);

    /// <summary>A rabbit tuning for a level.</summary>
    public static RabbitTuning Rabbit(RabbitTuning baseTuning, CreatureDef d, int level)
    {
        var t = (RabbitTuning)baseTuning.Duplicate();
        float hear = 1 + d.HearingPerLevel * Above(d, level), speed = 1 + d.SpeedPerLevel * Above(d, level);
        t.HearWalk *= hear; t.HearLoud *= hear; t.HearSneak *= hear; t.HearStill *= hear;
        t.DashSpeed *= speed;
        t.Health += d.HealthPerLevel * Above(d, level);
        return t;
    }

    /// <summary>A boar tuning for a level.</summary>
    public static BoarTuning Boar(BoarTuning baseTuning, CreatureDef d, int level)
    {
        var t = (BoarTuning)baseTuning.Duplicate();
        t.ChargeSpeed *= 1 + d.SpeedPerLevel * Above(d, level);
        t.Health += d.HealthPerLevel * Above(d, level);
        t.ContactDamage += Mathf.FloorToInt(d.DamagePerLevel * Above(d, level));
        return t;
    }
}
