using System;
using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Creatures;

/// <summary>Something a cut can reach.</summary>
public interface IHittable
{
    /// <summary>World state id.</summary>
    string CreatureId { get; }
    /// <summary>Feet position.</summary>
    Vector2 GlobalPosition { get; }
    /// <summary>Body radius for the blade test.</summary>
    float BodyRadius { get; }
    /// <summary>False when hidden or dead.</summary>
    bool CanBeHit { get; }
    /// <summary>Applies a blade hit.</summary>
    HitResult ReceiveHit(float damage);
    /// <summary>Raised when it dies, with where it fell.</summary>
    event Action<IHittable, Vector2>? Died;
    /// <summary>Kind for data lookups ("rabbit", "boar").</summary>
    string Kind { get; }
    /// <summary>Its level (fixed for the seed and its id).</summary>
    int Level { get; }
    /// <summary>Remaining hits it can take.</summary>
    int HealthLeft { get; }
    /// <summary>Hits it had at full health.</summary>
    int HealthMax { get; }
    /// <summary>Health left, exact (damage can be fractional: FOR, the Reforço, a critical).</summary>
    float HealthExact { get; }
}

/// <summary>
/// Coelho de Campo: grazes by its burrow, hears the player by how they move, raises its ears, runs to
/// the burrow in two dashes and hides; peeks out again later. Behaviour in <see cref="RabbitBrain"/>.
/// </summary>
public partial class Rabbit : CharacterBody2D, IHittable
{
    /// <summary>Tuning (RabbitTuning.tres).</summary>
    public RabbitTuning Tuning { get; set; } = new();
    /// <inheritdoc />
    public string CreatureId { get; set; } = "";
    /// <summary>The player it listens for.</summary>
    public Player? Target { get; set; }
    /// <summary>Its burrow, world pixels.</summary>
    public Vector2 BurrowAt { get; set; }
    /// <summary>Seed for its hops.</summary>
    public int Variation { get; set; }
    /// <summary>The behaviour.</summary>
    public RabbitBrain Brain { get; private set; } = null!;
    /// <inheritdoc />
    public float BodyRadius => Tuning.BodyRadius;
    /// <inheritdoc />
    public bool CanBeHit => Brain.Visible;
    /// <inheritdoc />
    public string Kind => "rabbit";
    /// <inheritdoc />
    public int Level { get; set; } = 1;
    /// <inheritdoc />
    public int HealthLeft => Mathf.CeilToInt(Brain.Health - 0.001f);
    /// <summary>Health left, exact (damage can be fractional: a reinforced knife cut takes 1.1).</summary>
    public float HealthExact => Brain.Health;
    /// <inheritdoc />
    public int HealthMax => Tuning.Health;
    /// <inheritdoc />
    public event Action<IHittable, Vector2>? Died;

    private float _time, _hitFlash, _dive;
    private bool _wasVisible = true;

    /// <summary>Seconds running (the art's animations).</summary>
    public float AnimTime => _time;
    /// <summary>0-0.2: the white flash of a hit.</summary>
    public float HitFlash => _hitFlash;
    /// <summary>Seconds left of the dive into the burrow (0 = not diving).</summary>
    public float DiveLeft => _dive;
    /// <summary>True from the moment it starts diving into the burrow until it is out again (checks).</summary>
    public bool Dived { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        Brain = new RabbitBrain(Tuning, BurrowAt, Variation);
        CollisionLayer = 4; CollisionMask = 1 | Plains.PlainsWorld.WaterLayer; // deep water still blocks the animals
        AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 4 } });
        AddToGroup("creatures");
    }

    /// <summary>How far the target can be heard right now.</summary>
    public float HearRadius => Target == null ? 0 : Target.Noise switch
    {
        Player.NoiseLevel.Still => Tuning.HearStill,
        Player.NoiseLevel.Sneak => Tuning.HearSneak,
        Player.NoiseLevel.Walk => Tuning.HearWalk,
        _ => Tuning.HearLoud,
    };

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        _time += dt; _hitFlash = Mathf.Max(0, _hitFlash - dt); _dive = Mathf.Max(0, _dive - dt);
        if (Brain.State == RabbitState.Dead) return;
        Vector2 player = Target?.GlobalPosition ?? new Vector2(1e6f, 1e6f);
        Velocity = Brain.Update(dt, GlobalPosition, player, HearRadius);
        MoveAndSlide();
        // into the burrow: a short dive instead of vanishing (T07)
        if (_wasVisible && !Brain.Visible && Brain.State == RabbitState.Hidden) { _dive = FaunaSettings.Current.RabbitDiveSeconds; Dived = true; }
        if (Brain.Visible && !_wasVisible) Dived = false;
        _wasVisible = Brain.Visible;
        Visible = Brain.Visible || _dive > 0;
        QueueRedraw();
    }

    /// <inheritdoc />
    public HitResult ReceiveHit(float damage)
    {
        var r = Brain.TakeHit(damage);
        if (r == HitResult.Killed) Died?.Invoke(this, GlobalPosition);
        else if (r == HitResult.Damaged) _hitFlash = 0.2f;
        return r;
    }

    /// <inheritdoc />
    public override void _Draw() => SpiritOfMurin.Elements.CoelhoArt.Draw(this); // the picture: src/Elements/Fauna/CoelhoArt.cs

}
