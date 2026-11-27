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

    private float _time, _hitFlash;

    /// <inheritdoc />
    public override void _Ready()
    {
        Brain = new RabbitBrain(Tuning, BurrowAt, Variation);
        CollisionLayer = 4; CollisionMask = 1;
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
        _time += dt; _hitFlash = Mathf.Max(0, _hitFlash - dt);
        if (Brain.State == RabbitState.Dead) return;
        Vector2 player = Target?.GlobalPosition ?? new Vector2(1e6f, 1e6f);
        Velocity = Brain.Update(dt, GlobalPosition, player, HearRadius);
        MoveAndSlide();
        Visible = Brain.Visible;
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

    private static readonly Color Outline = new("#16140f"), Fur = new("#8a7a5e"), FurLight = new("#b8a98a"), Belly = new("#d8ccb0"),
        EarIn = new("#c99a8a"), Eye = new("#16140f"), Shadow = new(8 / 255f, 10 / 255f, 8 / 255f, 0.35f);

    private void R(float x, float y, float w, float h, Color c) => DrawRect(new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), c);

    /// <inheritdoc />
    public override void _Draw()
    {
        if (!Brain.Visible) return;
        int s = Brain.Facing.X < 0 ? -1 : 1;
        bool running = Brain.State is RabbitState.Dash1 or RabbitState.Dash2;
        bool hop = Velocity.LengthSquared() > 1 && !running;
        float lift = hop ? -Mathf.Abs(Mathf.Sin(_time * 14f)) * 2 : 0;
        float stretch = running ? 3 : 0;
        R(-6, -1, 12, 3, Shadow);
        // body (outline then colour), mirrored by facing
        float bx = -5 - stretch / 2, by = -7 + lift;
        R(bx - 1, by - 1, 10 + stretch + 2, 7 + 2, Outline);
        R(bx, by, 10 + stretch, 7, Fur); R(bx, by + 5, 10 + stretch, 2, Belly); R(bx + (s > 0 ? 0 : 8 + stretch), by + 1, 2, 2, FurLight);
        // head
        float hx = s > 0 ? bx + 7 + stretch : bx - 4, hy = by - 3;
        R(hx - 1, hy - 1, 6 + 2, 5 + 2, Outline); R(hx, hy, 6, 5, Fur);
        R(hx + (s > 0 ? 4 : 1), hy + 1, 1, 1, Eye);
        // ears: up when alert (the tell), laid back otherwise
        if (Brain.EarsUp)
        {
            R(hx + 1 - 1, hy - 7, 2 + 2, 7, Outline); R(hx + 3 - 1, hy - 8, 2 + 2, 8, Outline);
            R(hx + 1, hy - 6, 2, 6, Fur); R(hx + 3, hy - 7, 2, 7, FurLight); R(hx + 3, hy - 6, 1, 4, EarIn);
        }
        else
        {
            float ex = s > 0 ? hx - 4 : hx + 4;
            R(ex - 1, hy - 1, 6 + 2, 2 + 2, Outline); R(ex, hy, 6, 2, Fur); R(ex + 1, hy, 3, 1, EarIn);
        }
        // tail
        R(s > 0 ? bx - 2 : bx + 10 + stretch, by + 1, 2, 2, Belly);
        if (_hitFlash > 0) R(bx, by, 10, 7, new Color(1, 1, 1, _hitFlash * 3));
    }
}
