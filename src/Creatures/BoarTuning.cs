using Godot;

namespace SpiritOfMurin.Creatures;

/// <summary>
/// Javali da Mata tuning. Data-driven: values live in <c>res://assets/data/plains/BoarTuning.tres</c>
/// (first balance pass: 0.8 s warning, ~1 s recovery, as in the Planície brief).
/// </summary>
[GlobalClass]
public partial class BoarTuning : Resource
{
    /// <summary>Seconds of warning (scraping the hoof, head lowered) before the charge.</summary>
    [Export] public float WarnTime { get; set; } = 0.8f;
    /// <summary>Fraction of the warning during which the head still follows the player; after it the direction is fixed.</summary>
    [Export] public float TrackFraction { get; set; } = 0.55f;
    /// <summary>Charge speed, pixels per second.</summary>
    [Export] public float ChargeSpeed { get; set; } = 240f;
    /// <summary>Longest charge, pixels (it stops sooner when it hits something).</summary>
    [Export] public float ChargeDistance { get; set; } = 170f;
    /// <summary>Seconds the boar needs to recover after a charge: the opening for the counter-attack.</summary>
    [Export] public float RecoverTime { get; set; } = 1.0f;
    /// <summary>Seconds between the end of the recovery and the next warning.</summary>
    [Export] public float Cooldown { get; set; } = 0.35f;
    /// <summary>The boar notices the player within this distance, pixels.</summary>
    [Export] public float AggroRadius { get; set; } = 110f;
    /// <summary>Beyond this distance from home it gives up and goes back, pixels.</summary>
    [Export] public float LeashRadius { get; set; } = 260f;
    /// <summary>Distance at which the charge hits the player, pixels (feet to feet).</summary>
    [Export] public float ChargeHitRadius { get; set; } = 15f;
    /// <summary>Life damage the charge deals.</summary>
    [Export] public int ContactDamage { get; set; } = 2;
    /// <summary>Hits (of 1 damage) the boar takes before dying.</summary>
    [Export] public int Health { get; set; } = 5;
    /// <summary>Wandering speed while rooting in the mud, pixels per second.</summary>
    [Export] public float WanderSpeed { get; set; } = 16f;
    /// <summary>Walking speed when going back home, pixels per second.</summary>
    [Export] public float ReturnSpeed { get; set; } = 50f;
    /// <summary>Body radius for being hit, pixels.</summary>
    [Export] public float BodyRadius { get; set; } = 10f;
    /// <summary>Seconds the boar flinches after a hit during the recovery (adds to the recovery).</summary>
    [Export] public float HitFlinch { get; set; } = 0.15f;
}
