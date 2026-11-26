using Godot;

namespace SpiritOfMurin.Creatures;

/// <summary>
/// Coelho de Campo tuning. Data-driven: values live in <c>res://assets/data/plains/RabbitTuning.tres</c>.
/// The rabbit hears the player within a radius that depends on how the player moves.
/// </summary>
[GlobalClass]
public partial class RabbitTuning : Resource
{
    /// <summary>Hearing radius when the player walks, pixels.</summary>
    [Export] public float HearWalk { get; set; } = 95f;
    /// <summary>Hearing radius when the player sneaks (Shift), pixels.</summary>
    [Export] public float HearSneak { get; set; } = 26f;
    /// <summary>Hearing radius when the player stands still, pixels.</summary>
    [Export] public float HearStill { get; set; } = 14f;
    /// <summary>Hearing radius during a step or a cut, pixels.</summary>
    [Export] public float HearLoud { get; set; } = 130f;
    /// <summary>Seconds the rabbit freezes with its ears up before running.</summary>
    [Export] public float AlertTime { get; set; } = 0.35f;
    /// <summary>Running speed, pixels per second (faster than the player's walk).</summary>
    [Export] public float DashSpeed { get; set; } = 165f;
    /// <summary>Pause between the two dashes to the burrow, seconds.</summary>
    [Export] public float DashPause { get; set; } = 0.18f;
    /// <summary>Seconds hidden in the burrow before peeking out again.</summary>
    [Export] public float HideTime { get; set; } = 4.5f;
    /// <summary>How far from the burrow it grazes, pixels.</summary>
    [Export] public float GrazeRadius { get; set; } = 44f;
    /// <summary>Hop speed while grazing, pixels per second.</summary>
    [Export] public float GrazeSpeed { get; set; } = 22f;
    /// <summary>Body radius for being hit, pixels.</summary>
    [Export] public float BodyRadius { get; set; } = 6f;
    /// <summary>Hits before dying.</summary>
    [Export] public int Health { get; set; } = 1;
}
