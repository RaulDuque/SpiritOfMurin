using System;
using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Creatures;

/// <summary>
/// Javali da Mata: roots in the mud; warns (scrapes the hoof, lowers the head, fixes the direction),
/// charges in a straight line, then recovers — the opening. Behaviour in <see cref="BoarBrain"/>; the
/// charge's damage lands only when the boar's body actually reaches the player.
/// </summary>
public partial class Boar : CharacterBody2D, IHittable
{
    /// <summary>Tuning (BoarTuning.tres).</summary>
    public BoarTuning Tuning { get; set; } = new();
    /// <inheritdoc />
    public string CreatureId { get; set; } = "";
    /// <summary>The player it charges.</summary>
    public Player? Target { get; set; }
    /// <summary>Home in the mud, world pixels.</summary>
    public Vector2 HomeAt { get; set; }
    /// <summary>Seed for its rooting walk.</summary>
    public int Variation { get; set; }
    /// <summary>The behaviour.</summary>
    public BoarBrain Brain { get; private set; } = null!;
    /// <inheritdoc />
    public float BodyRadius => Tuning.BodyRadius;
    /// <inheritdoc />
    public bool CanBeHit => Brain.State != BoarState.Dead;
    /// <inheritdoc />
    public string Kind => "boar";
    /// <inheritdoc />
    public int Level { get; set; } = 3;
    /// <inheritdoc />
    public int HealthLeft => Mathf.CeilToInt(Brain.Health - 0.001f);
    /// <summary>Health left, exact (damage can be fractional: a reinforced knife cut takes 1.1).</summary>
    public float HealthExact => Brain.Health;
    /// <inheritdoc />
    public int HealthMax => Tuning.Health;
    /// <inheritdoc />
    public event Action<IHittable, Vector2>? Died;
    /// <summary>Raised when a hit glances off the hide (warning or charge).</summary>
    public event Action<Boar>? Deflected;
    /// <summary>Raised when the charge reaches the player.</summary>
    public event Action<Boar>? HitPlayer;

    private float _time, _hitFlash, _spark, _flinch;

    /// <summary>Seconds running (the art's animations).</summary>
    public float AnimTime => _time;
    /// <summary>0-0.2: the white flash of a hit that landed.</summary>
    public float HitFlash => _hitFlash;
    /// <summary>0-0.3: the spark of a blade glancing off the hide.</summary>
    public float Spark => _spark;
    /// <summary>Seconds of flinch left (a hit landed in the recovery).</summary>
    public float FlinchLeft => _flinch;
    /// <summary>Where the last glancing blow came from (local direction), for the streak.</summary>
    public Vector2 GlanceFrom { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        Brain = new BoarBrain(Tuning, HomeAt, Variation);
        CollisionLayer = 4; CollisionMask = 1 | Plains.PlainsWorld.WaterLayer; // deep water still blocks the animals
        AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 7 } });
        AddToGroup("creatures");
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        _time += dt; _hitFlash = Mathf.Max(0, _hitFlash - dt); _spark = Mathf.Max(0, _spark - dt); _flinch = Mathf.Max(0, _flinch - dt);
        if (Brain.State == BoarState.Dead) return;
        Vector2 player = Target?.GlobalPosition ?? new Vector2(1e6f, 1e6f);
        bool canHit = Target != null && Target.Health > 0 && !Target.Invulnerable;
        var step = Brain.Update(dt, GlobalPosition, player, canHit);
        Velocity = step.Velocity;
        if (Brain.State == BoarState.Charge)
        {
            var col = MoveAndCollide(Velocity * dt);
            if (col != null) Brain.NotifyBlocked();
        }
        else MoveAndSlide();
        if (step.HitPlayer && Target != null && Target.TakeDamage(Tuning.ContactDamage, GlobalPosition - Brain.Facing * 10)) HitPlayer?.Invoke(this);
        QueueRedraw();
    }

    /// <inheritdoc />
    /// <summary>Breaks the boar's defence (warning or charge) for a while; true when there was one to break.</summary>
    public bool BreakGuard(float seconds) { bool b = Brain.Break(seconds); if (b) _spark = 0.3f; return b; }

    /// <inheritdoc />
    public HitResult ReceiveHit(float damage) => Hit(damage, false, Vector2.Zero);

    /// <summary>A hit from <paramref name="from"/> (world): the glance streak faces it.</summary>
    public HitResult ReceiveHitFrom(float damage, Vector2 from) => Hit(damage, false, from);

    /// <summary>
    /// A hit that goes through the hide even during the warning and the charge (the weakened Lâmina, T07 decision B):
    /// the damage lands and the boar keeps charging.
    /// </summary>
    public HitResult ReceiveHitThrough(float damage) => Hit(damage, true, Vector2.Zero);

    private HitResult Hit(float damage, bool through, Vector2 from)
    {
        var r = Brain.TakeHit(damage, through);
        if (r == HitResult.Killed) Died?.Invoke(this, GlobalPosition);
        else if (r == HitResult.Damaged) { _hitFlash = 0.2f; _flinch = FaunaSettings.Current.BoarFlinchSeconds; }
        else if (r == HitResult.Deflected) { _spark = 0.2f; GlanceFrom = from == Vector2.Zero ? Vector2.Up : from - GlobalPosition; Deflected?.Invoke(this); }
        return r;
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        if (SpiritOfMurin.Game.Assist.Areas) DrawAssist(Brain.State);
        SpiritOfMurin.Elements.JavaliArt.Draw(this, SpiritOfMurin.Game.Assist.Strip); // the picture: src/Elements/Fauna/JavaliArt.cs
    }

    // the assist mode (Esc → Modo de assistência): the charge's corridor from the brain's own numbers —
    // the warning (dashed, filling), the live charge (solid), the recovery (a hollow open ring: hit it now)
    private void DrawAssist(BoarState st)
    {
        float half = Tuning.ChargeHitRadius;
        if (st == BoarState.Warn)
        {
            float k = Mathf.Clamp(Brain.StateTime / Mathf.Max(0.01f, Tuning.WarnTime), 0, 1);
            var pts = SpiritOfMurin.Game.Assist.Corridor(Vector2.Zero, Brain.Facing, Tuning.ChargeDistance, half);
            for (int i = 0; i < 4; i++) SpiritOfMurin.Game.Assist.Line(this, pts[i], pts[(i + 1) % 4], new Color(SpiritOfMurin.Game.Assist.ThreatC, 0.9f), 1, 4);
            DrawColoredPolygon(SpiritOfMurin.Game.Assist.Corridor(Vector2.Zero, Brain.Facing, Mathf.Max(1, Tuning.ChargeDistance * k), half), new Color(SpiritOfMurin.Game.Assist.ThreatC, 0.2f));
            DrawArc(Vector2.Zero, 16, 0, Mathf.Tau, 20, new Color(SpiritOfMurin.Game.Assist.GuardC, 0.8f), 2); // hits glance off now
        }
        else if (st == BoarState.Charge)
        {
            var pts = SpiritOfMurin.Game.Assist.Corridor(Vector2.Zero, Brain.Facing, Mathf.Max(8, Tuning.ChargeDistance - Brain.Travel), half);
            DrawColoredPolygon(pts, new Color(SpiritOfMurin.Game.Assist.ThreatC, 0.45f));
            for (int i = 0; i < 4; i++) DrawLine(pts[i], pts[(i + 1) % 4], SpiritOfMurin.Game.Assist.ThreatC, 3);
            DrawArc(Vector2.Zero, 16, 0, Mathf.Tau, 20, new Color(SpiritOfMurin.Game.Assist.GuardC, 0.8f), 2);
        }
        else if (st == BoarState.Recover)
            SpiritOfMurin.Game.Assist.Arc(this, Vector2.Zero, 0, Mathf.Pi, 16 + 4 * Mathf.Sin(Brain.StateTime * 8), new Color(SpiritOfMurin.Game.Assist.OpenC, 0.9f), 2, 4);
    }
}
