using Godot;

namespace SpiritOfMurin.Characters;

/// <summary>
/// Tuning values for the player's movement, cut and step. Data-driven: the values live in
/// <c>res://assets/data/PlayerTuning.tres</c> and start from the HTML prototype's
/// <c>config.js</c>, so the feel can be compared side by side.
/// </summary>
[GlobalClass]
public partial class PlayerTuning : Resource
{
    /// <summary>Walking speed in pixels per second (prototype: player_speed).</summary>
    [Export] public float MoveSpeed { get; set; } = 105f;
    /// <summary>
    /// Speed on each axis when walking diagonally, as a fraction of <see cref="MoveSpeed"/> (the creator found the plain
    /// 0.707 slow on screen, 2026-11-27): 0.8 makes the diagonal about 13% faster in distance, which feels like walking straight.
    /// </summary>
    [Export] public float DiagonalAxisSpeed { get; set; } = 0.8f;

    /// <summary>Seconds a pressed cut or step waits for the current action to allow a cancel.</summary>
    [Export] public float BufferTime { get; set; } = 0.2f;

    /// <summary>Seconds before the blade is live (cuts 1 and 2).</summary>
    [Export] public float CutStartup { get; set; } = 0.07f;

    /// <summary>Seconds the blade is live (cuts 1 and 2).</summary>
    [Export] public float CutActive { get; set; } = 0.08f;

    /// <summary>Seconds after the blade (cuts 1 and 2).</summary>
    [Export] public float CutRecovery { get; set; } = 0.24f;

    /// <summary>Seconds into a cut when the next cut or a step may cancel it (the chain window).</summary>
    [Export] public float CutCancelAt { get; set; } = 0.13f;

    /// <summary>Extra startup, active and recovery the third cut of the chain takes.</summary>
    [Export] public float ThirdCutExtra { get; set; } = 0.03f;

    /// <summary>Pixels the body moves forward when the blade goes live (cuts 1 and 2).</summary>
    [Export] public float CutLunge { get; set; } = 8f;

    /// <summary>Pixels the body moves forward on the third cut.</summary>
    [Export] public float ThirdCutLunge { get; set; } = 14f;

    /// <summary>Distance in pixels of the plain step (prototype: short_step_dist).</summary>
    [Export] public float StepDistance { get; set; } = 34f;

    /// <summary>Duration in seconds of the plain step (prototype: short_step_time).</summary>
    [Export] public float StepTime { get; set; } = 0.16f;

    /// <summary>Hit reach of the sword's blade from the feet, pixels.</summary>
    [Export] public float SwordReach { get; set; } = 34f;

    /// <summary>Damage of a sword cut.</summary>
    [Export] public int SwordDamage { get; set; } = 2;

    /// <summary>Seconds before the stone knife is live: shorter than the sword.</summary>
    [Export] public float KnifeStartup { get; set; } = 0.05f;

    /// <summary>Seconds the stone knife is live.</summary>
    [Export] public float KnifeActive { get; set; } = 0.07f;

    /// <summary>Seconds after the stone knife's cut.</summary>
    [Export] public float KnifeRecovery { get; set; } = 0.15f;

    /// <summary>Seconds into a knife cut when the next cut or a step may cancel it.</summary>
    [Export] public float KnifeCancelAt { get; set; } = 0.11f;

    /// <summary>Pixels the body moves forward on a knife cut.</summary>
    [Export] public float KnifeLunge { get; set; } = 4f;

    /// <summary>Hit reach of the stone knife from the feet, pixels (short: you must get close).</summary>
    [Export] public float KnifeReach { get; set; } = 22f;

    /// <summary>Damage of a knife cut.</summary>
    [Export] public int KnifeDamage { get; set; } = 1;

    /// <summary>Longest knife chain (the sword chains 3).</summary>
    [Export] public int KnifeMaxChain { get; set; } = 2;

    /// <summary>Half angle of the cut's arc, degrees: a target must be within it, in front.</summary>
    [Export] public float HitArcDegrees { get; set; } = 75f;

    /// <summary>Walking speed factor while sneaking (Shift): slower and much quieter.</summary>
    [Export] public float SneakFactor { get; set; } = 0.45f;

    /// <summary>Maximum Vida (life points; 2 per heart drawn).</summary>
    [Export] public int MaxHealth { get; set; } = 10;

    /// <summary>Seconds of invulnerability after being hurt.</summary>
    [Export] public float HurtInvulnerability { get; set; } = 0.7f;

    /// <summary>Pixels the body is pushed when hurt.</summary>
    [Export] public float HurtKnockback { get; set; } = 26f;
}
