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
    public int HealthLeft => Brain.Health;
    /// <inheritdoc />
    public int HealthMax => Tuning.Health;
    /// <inheritdoc />
    public event Action<IHittable, Vector2>? Died;
    /// <summary>Raised when a hit glances off the hide (warning or charge).</summary>
    public event Action<Boar>? Deflected;
    /// <summary>Raised when the charge reaches the player.</summary>
    public event Action<Boar>? HitPlayer;

    private float _time, _hitFlash, _spark;

    /// <inheritdoc />
    public override void _Ready()
    {
        Brain = new BoarBrain(Tuning, HomeAt, Variation);
        CollisionLayer = 4; CollisionMask = 1;
        AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 7 } });
        AddToGroup("creatures");
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        _time += dt; _hitFlash = Mathf.Max(0, _hitFlash - dt); _spark = Mathf.Max(0, _spark - dt);
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
    public HitResult ReceiveHit(int damage)
    {
        var r = Brain.TakeHit(damage);
        if (r == HitResult.Killed) Died?.Invoke(this, GlobalPosition);
        else if (r == HitResult.Damaged) _hitFlash = 0.2f;
        else if (r == HitResult.Deflected) { _spark = 0.15f; Deflected?.Invoke(this); }
        return r;
    }

    private static readonly Color Outline = new("#16140f"), Fur = new("#4a3a2c"), FurHi = new("#6b5640"), Dark = new("#2c2219"),
        Snout = new("#8a6a58"), Tusk = new("#e6dcc0"), Eye = new("#c9a14a"), Dust = new("#8f7a58"), Daze = new("#e6dcc0"),
        Shadow = new(8 / 255f, 10 / 255f, 8 / 255f, 0.38f), Mark = new("#33271b");

    private void R(float x, float y, float w, float h, Color c) => DrawRect(new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), c);

    /// <inheritdoc />
    public override void _Draw()
    {
        var st = Brain.State;
        if (SpiritOfMurin.Game.Assist.Areas) DrawAssist(st);
        if (st == BoarState.Dead) return;
        // fixed direction: scuffed marks on the ground along the line it is about to run
        if (Brain.DirectionLocked)
            for (int i = 1; i <= 4; i++) { Vector2 m = Brain.Facing * (14 + i * 12); R(m.X - 1, m.Y - 1, 3, 2, new Color(Mark, 0.8f - i * 0.15f)); }
        int s = Brain.Facing.X < 0 ? -1 : 1;
        bool warn = st == BoarState.Warn, charge = st == BoarState.Charge, dazed = st == BoarState.Recover;
        float headDrop = warn || charge ? 3 : 0;
        float bob = Velocity.LengthSquared() > 1 ? Mathf.Round(Mathf.Sin(_time * (charge ? 30 : 8))) : 0;
        float shake = dazed ? Mathf.Round(Mathf.Sin(_time * 18) * 1) : 0;
        R(-14, -2, 28, 5, Shadow);
        // legs (the back hoof scrapes during the warning)
        float scrape = warn ? Mathf.Round(Mathf.Sin(_time * 22) * 3) : 0;
        R(-9 * s - 1 + (s < 0 ? -2 : 0) - scrape * s, -5, 3, 5, Dark); R(-5 * s - 1, -5, 3, 5, Dark);
        R(5 * s - 1, -5 - bob, 3, 5, Dark); R(9 * s - 1 + (s < 0 ? -2 : 0), -5, 3, 5, Dark);
        // body: a heavy wedge, higher at the shoulders
        for (int pass = 0; pass < 2; pass++)
            for (int i = 0; i < 24; i++)
            {
                float x = -12 + i, h = 9 + Mathf.Min(i, 16) * 0.25f;
                float bx = s > 0 ? x : -x - 1, top = -5 - h + bob;
                if (pass == 0) R(bx - 1, top - 1, 3, h + 2, Outline);
                else { R(bx, top, 1, h, Fur); R(bx, top, 1, 2, i % 3 == 0 ? Dark : FurHi); }
            }
        // head and snout, lowered while warning and charging
        float hx = s > 0 ? 11 : -19, hy = -14 + headDrop + bob + shake;
        R(hx - 1, hy - 1, 10, 9, Outline); R(hx, hy, 8, 7, Fur);
        float sx = s > 0 ? hx + 7 : hx - 3;
        R(sx - 1, hy + 2, 6, 5, Outline); R(sx, hy + 3, 4, 3, Snout);
        R(s > 0 ? sx + 1 : sx + 1, hy + 1, 2, 3, Tusk);
        R(s > 0 ? hx + 4 : hx + 2, hy + 2, 2, 1, warn || charge ? new Color("#d05030") : Eye);
        R(s > 0 ? hx + 1 : hx + 5, hy - 2, 2, 2, Dark); // ear
        // dust from the scraping hoof, and behind the charge
        if (warn)
            for (int i = 0; i < 3; i++) { float k = Mathf.PosMod(_time * 3 + i * 0.33f, 1f); R(-12 * s - s * k * 10, -3 - k * 6, 2, 2, new Color(Dust, 1 - k)); }
        if (charge)
            for (int i = 0; i < 4; i++) { Vector2 d = -Brain.Facing * (14 + i * 6); R(d.X, d.Y - 3 - i, 3, 2, new Color(Dust, 0.7f - i * 0.15f)); }
        // dazed: little circling dots over the head (the opening)
        if (dazed)
            for (int i = 0; i < 3; i++)
            {
                float a = _time * 6 + i * Mathf.Tau / 3, dx = hx + 3 + Mathf.Cos(a) * 7, dy = hy - 7 + Mathf.Sin(a) * 2;
                R(dx - 1, dy - 1, 4, 4, Outline); R(dx, dy, 2, 2, Daze);
            }
        if (_hitFlash > 0) R(-12, -18, 24, 13, new Color(1, 1, 1, _hitFlash * 3));
        if (_spark > 0) { R(hx + 2, hy - 4, 1, 4, Tusk); R(hx, hy - 2, 5, 1, Tusk); }
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
