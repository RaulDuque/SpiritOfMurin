using System.Text.Json;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Vale;

/// <summary>The prologue's combat numbers, from <c>res://assets/data/vale_combat.json</c> (snake_case).</summary>
public sealed class ValeCombat
{
    /// <summary>Default path.</summary>
    public const string DefaultPath = "res://assets/data/vale_combat.json";
    /// <summary>The swordsman's Vida.</summary>
    public int PlayerHealth { get; set; } = 10;
    /// <summary>The swordsman's Qi.</summary>
    public float PlayerQiMax { get; set; } = 100;
    /// <summary>Qi per second.</summary>
    public float PlayerQiRegen { get; set; } = 22;
    /// <summary>Degrees each side of a guard's facing that parry.</summary>
    public float GuardHalf { get; set; } = 65;
    /// <summary>Pixels a parry throws the player back.</summary>
    public float ParryPush { get; set; } = 34;
    /// <summary>Disciple health.</summary>
    public int DisHp { get; set; } = 12;
    /// <summary>Disciple turn speed, degrees/s.</summary>
    public float DisTurn { get; set; } = 300;
    /// <summary>Seconds a disciple loses track after a side step.</summary>
    public float DisFooled { get; set; } = 0.38f;
    /// <summary>Disciple telegraph, s.</summary>
    public float DisWindup { get; set; } = 0.5f;
    /// <summary>Disciple reach, px.</summary>
    public float DisReach { get; set; } = 64;
    /// <summary>Senior reach, px.</summary>
    public float SenReach { get; set; } = 70;
    /// <summary>Degrees each side of the facing an enemy strike reaches.</summary>
    public float StrikeHalf { get; set; } = 40;
    /// <summary>Degrees each side for the Senior's third, wide strike.</summary>
    public float StrikeHalfBig { get; set; } = 75;
    /// <summary>Extra reach of the Senior's third strike, px.</summary>
    public float StrikeBigExtra { get; set; } = 6;
    /// <summary>Slack added to the reach when checking the strike against the player, px (the body's size).</summary>
    public float StrikeSlack { get; set; } = 8;
    /// <summary>Seconds a strike lasts (the lunge).</summary>
    public float StrikeTime { get; set; } = 0.14f;
    /// <summary>Seconds into the strike when the blade is live.</summary>
    public float StrikeLiveAt { get; set; } = 0.04f;
    /// <summary>Pixels the prologue adds to a cut's reach when testing a hit.</summary>
    public float CutSlack { get; set; } = 14;
    /// <summary>Pixels the prologue adds to a thrust's reach when testing a hit.</summary>
    public float ThrustSlack { get; set; } = 10;
    /// <summary>Disciple cooldown, s.</summary>
    public float DisCooldown { get; set; } = 1.1f;
    /// <summary>Disciple recovery, s.</summary>
    public float DisRecover { get; set; } = 0.55f;
    /// <summary>Senior health.</summary>
    public int SenHp { get; set; } = 20;
    /// <summary>Senior turn speed, degrees/s.</summary>
    public float SenTurn { get; set; } = 330;
    /// <summary>Seconds the Senior loses track after a side step.</summary>
    public float SenFooled { get; set; } = 0.26f;
    /// <summary>Seconds in the closed guard before the attack stance.</summary>
    public float SenClosedTime { get; set; } = 3.4f;
    /// <summary>Senior telegraph, s.</summary>
    public float SenWindup { get; set; } = 0.38f;
    /// <summary>Seconds open after his three-strike combo.</summary>
    public float SenComboRecover { get; set; } = 0.8f;
    /// <summary>Senior advance speed, px/s.</summary>
    public float SenSpeed { get; set; } = 85;
    /// <summary>Damage of an enemy hit, in Vida.</summary>
    public int EnemyDamage { get; set; } = 2;
    /// <summary>World freeze on a landed hit, s.</summary>
    public float Hitstop { get; set; } = 0.07f;
    /// <summary>Screen shake, px.</summary>
    public float Shake { get; set; } = 3;

    /// <summary>Parses the data.</summary>
    public static ValeCombat Parse(string json) => JsonSerializer.Deserialize<ValeCombat>(json, PlainsGenSettings.Json) ?? new ValeCombat();

    /// <summary>Loads the data.</summary>
    public static ValeCombat Load(string path = DefaultPath) => Parse(Godot.FileAccess.GetFileAsString(path));
}
