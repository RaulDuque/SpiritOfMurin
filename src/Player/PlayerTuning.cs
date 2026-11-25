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
}
