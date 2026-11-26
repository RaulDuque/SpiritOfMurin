using System;
using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Characters;

/// <summary>
/// The swordsman: walks with the move actions, cuts (sword: chain of up to three; stone knife: a
/// short cut, chain of two) and steps. The cut follows the direction the character faces, which is
/// the last direction walked. With empty hands there is no cut. Drawn in code with the HTML
/// prototype's look (the creator's approved appearance).
/// Input actions (Project Settings &gt; Input Map): move_left/right/up/down, cut, step, sneak.
/// </summary>
public partial class Player : CharacterBody2D
{
    /// <summary>What the body is doing right now.</summary>
    public enum ActionKind { None, Cut, Step }

    /// <summary>What the hands hold.</summary>
    public enum Weapon { None, Knife, Sword }

    /// <summary>How loud the character is right now (animals hear by it).</summary>
    public enum NoiseLevel { Still, Sneak, Walk, Loud }

    /// <summary>Weapon held when the scene starts (the test field: sword; the Planície: empty hands).</summary>
    [Export] public Weapon StartWeapon { get; set; } = Weapon.Sword;

    /// <summary>Weapon held now.</summary>
    public Weapon Equipped { get; set; } = Weapon.Sword;

    /// <summary>Current Vida.</summary>
    public float Health { get; private set; }

    /// <summary>Maximum Vida (from the tuning).</summary>
    public int MaxHealth => Tuning.MaxHealth;

    /// <summary>Vida still to be restored by herbs over time.</summary>
    public float PendingRegen => _regenLeft;

    /// <summary>When true the body ignores input (inventory open, fainted, intro).</summary>
    public bool InputLocked { get; set; }

    /// <summary>How loud the body is this frame.</summary>
    public NoiseLevel Noise { get; private set; } = NoiseLevel.Still;

    /// <summary>Raised every physics frame while a blade is live: feet position, direction, reach, damage, and a swing id (one per cut).</summary>
    public event Action<Vector2, Vector2, float, int, int>? BladeActive;

    /// <summary>Raised when the cut button is pressed with nothing in hand.</summary>
    public event Action? EmptyHanded;

    /// <summary>Raised when hurt: damage taken.</summary>
    public event Action<int>? Hurt;

    /// <summary>Raised when the Vida reaches zero.</summary>
    public event Action? Fainted;

    private float _invulnerable, _regenLeft, _regenRate, _hurtFlash;
    private int _swingId;

    /// <summary>Raised when an action starts: the kind and, for cuts, its number in the chain (1-3).</summary>
    public event Action<ActionKind, int>? ActionStarted;

    /// <summary>Movement, cut and step values; set in the scene from PlayerTuning.tres.</summary>
    [Export] public PlayerTuning Tuning { get; set; } = new();

    /// <summary>Unit vector the character faces; cuts and the forward step follow it.</summary>
    public Vector2 Facing { get; private set; } = Vector2.Down;

    /// <summary>The action in progress (None when free to walk).</summary>
    public ActionKind Current => _act?.Kind ?? ActionKind.None;

    /// <summary>Number of the cut in progress in the chain (0 when not cutting).</summary>
    public int CutNumber => _act is { Kind: ActionKind.Cut } a ? a.N : 0;

    /// <summary>World angle (radians) of the last cut's swing centre; equals the facing angle when it started.</summary>
    public float LastCutAngle { get; private set; }

    private sealed class Act
    {
        public ActionKind Kind;
        public float T, Startup, Active, Recovery, CancelAt, Lunge;
        public int N;
        public Vector2 StepVelocity;
        public bool Live;
        public float Duration => Kind == ActionKind.Step ? Startup : Startup + Active + Recovery;
    }

    private sealed class Slash { public Vector2 World; public float Angle, Length, Life, Max; }

    private Act? _act;
    private (ActionKind Kind, float Time)? _buffer;
    private float _walk, _time;
    private bool _moving;
    private readonly List<Slash> _slashes = new();
    private readonly List<(Vector2 World, float Life)> _ghosts = new();
    private readonly PixelPainter _paint = new();

    // the prototype's colours for the player (index.html, LOOK.player)
    private static readonly Color Robe = new("#dce5d4"), Shade = new("#9db3a0"), Trim = new("#2f7f6c"), Sash = new("#2f7f6c"),
        Skin = new("#e2b58e"), Hair = new("#1b191e"), Pants = new("#3d4a44"), Blade = new("#eef3f2"), Hilt = new("#7a5a3a"),
        Outline = new("#16140f"), Learn = new("#d4fbf6"), LearnGlow = new("#6fd8d2"), Flint = new("#8c8a80");

    /// <inheritdoc />
    public override void _Ready()
    {
        Equipped = StartWeapon;
        Health = Tuning.MaxHealth;
    }

    /// <summary>Takes damage from a hit coming from <paramref name="from"/>. Ignored while invulnerable. Returns true when it landed.</summary>
    public bool TakeDamage(int amount, Vector2 from)
    {
        if (_invulnerable > 0 || Health <= 0 || amount <= 0) return false;
        Health = Mathf.Max(0, Health - amount);
        _invulnerable = Tuning.HurtInvulnerability;
        _hurtFlash = 0.3f;
        Vector2 away = GlobalPosition - from;
        if (away.LengthSquared() > 0.01f) MoveAndCollide(away.Normalized() * Tuning.HurtKnockback);
        Hurt?.Invoke(amount);
        if (Health <= 0) Fainted?.Invoke();
        return true;
    }

    /// <summary>True while hurt-invulnerable.</summary>
    public bool Invulnerable => _invulnerable > 0;

    /// <summary>Restores Vida at once (up to the maximum).</summary>
    public void Heal(float amount) => Health = Mathf.Min(Tuning.MaxHealth, Health + amount);

    /// <summary>Restores <paramref name="amount"/> Vida gradually over <paramref name="seconds"/> (stacks with what is pending).</summary>
    public void AddRegen(float amount, float seconds)
    {
        _regenLeft += amount;
        _regenRate = Mathf.Max(_regenRate, amount / Mathf.Max(0.1f, seconds));
    }

    /// <summary>Back to full Vida, nothing pending (after fainting).</summary>
    public void Revive() { Health = Tuning.MaxHealth; _regenLeft = 0; _invulnerable = Tuning.HurtInvulnerability; _act = null; _buffer = null; }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        _time += dt;
        _invulnerable = Mathf.Max(0, _invulnerable - dt);
        _hurtFlash = Mathf.Max(0, _hurtFlash - dt);
        if (_regenLeft > 0 && Health > 0)
        {
            float r = Mathf.Min(_regenLeft, _regenRate * dt);
            _regenLeft -= r; Heal(r);
            if (Health >= Tuning.MaxHealth) _regenLeft = 0;
        }
        if (_regenLeft <= 0) _regenRate = 0;
        bool locked = InputLocked || Health <= 0;
        Vector2 move = locked ? Vector2.Zero : Input.GetVector("move_left", "move_right", "move_up", "move_down");
        bool sneaking = !locked && InputMap.HasAction("sneak") && Input.IsActionPressed("sneak");

        // a press waits in the buffer until the current action can be cancelled
        if (locked) { }
        else if (Input.IsActionJustPressed("cut"))
        {
            if (Equipped == Weapon.None) EmptyHanded?.Invoke();
            else _buffer = (ActionKind.Cut, Tuning.BufferTime);
        }
        else if (Input.IsActionJustPressed("step")) _buffer = (ActionKind.Step, Tuning.BufferTime);
        if (_buffer is { } b) _buffer = b.Time - dt > 0 ? (b.Kind, b.Time - dt) : null;

        if (_act != null) Advance(dt);
        if (_buffer is { } buf && (_act == null || _act.T >= _act.CancelAt))
        {
            bool thirdEnds = _act is { Kind: ActionKind.Cut } c && c.N >= MaxChain && buf.Kind == ActionKind.Cut;
            if (!thirdEnds) { _buffer = null; Start(buf.Kind, move); }
        }

        _moving = false;
        if (_act == null)
        {
            Velocity = move * Tuning.MoveSpeed * (sneaking ? Tuning.SneakFactor : 1f);
            if (move != Vector2.Zero) { Facing = move.Normalized(); _moving = true; _walk += dt * (sneaking ? 4f : 7f); }
        }
        else Velocity = _act.Kind == ActionKind.Step ? _act.StepVelocity : Vector2.Zero;
        MoveAndSlide();
        Noise = _act != null ? NoiseLevel.Loud : !_moving ? NoiseLevel.Still : sneaking ? NoiseLevel.Sneak : NoiseLevel.Walk;
        if (_act is { Kind: ActionKind.Cut, Live: true } live && live.T < live.Startup + live.Active)
        {
            bool knife = Equipped == Weapon.Knife;
            BladeActive?.Invoke(GlobalPosition, Facing, knife ? Tuning.KnifeReach : Tuning.SwordReach, knife ? Tuning.KnifeDamage : Tuning.SwordDamage, _swingId);
        }

        for (int i = _slashes.Count - 1; i >= 0; i--) { _slashes[i].Life -= dt; if (_slashes[i].Life <= 0) _slashes.RemoveAt(i); }
        for (int i = _ghosts.Count - 1; i >= 0; i--) { var g = _ghosts[i]; g.Life -= dt; if (g.Life <= 0) _ghosts.RemoveAt(i); else _ghosts[i] = g; }
        QueueRedraw();
    }

    private void Start(ActionKind kind, Vector2 move)
    {
        if (kind == ActionKind.Cut)
        {
            if (Equipped == Weapon.None) return;
            int n = _act is { Kind: ActionKind.Cut } prev && prev.N < MaxChain ? prev.N + 1 : 1;
            _swingId++;
            if (Equipped == Weapon.Knife)
                _act = new Act
                {
                    Kind = ActionKind.Cut, N = n, Startup = Tuning.KnifeStartup, Active = Tuning.KnifeActive,
                    Recovery = Tuning.KnifeRecovery, CancelAt = Tuning.KnifeCancelAt, Lunge = Tuning.KnifeLunge,
                };
            else
            {
                float extra = n == 3 ? Tuning.ThirdCutExtra : 0f;
                _act = new Act
                {
                    Kind = ActionKind.Cut, N = n, Startup = Tuning.CutStartup + extra, Active = Tuning.CutActive + extra,
                    Recovery = Tuning.CutRecovery + (n == 3 ? 0.1f : 0f), CancelAt = Tuning.CutCancelAt + (n == 3 ? 0.07f : 0f),
                    Lunge = n == 3 ? Tuning.ThirdCutLunge : Tuning.CutLunge,
                };
            }
            LastCutAngle = Facing.Angle();
            ActionStarted?.Invoke(kind, n);
        }
        else
        {
            // with a direction held, step that way (and face it); with none, step back from the facing
            Vector2 dir = move != Vector2.Zero ? move.Normalized() : -Facing;
            if (move != Vector2.Zero) Facing = dir;
            _act = new Act { Kind = ActionKind.Step, Startup = Tuning.StepTime, CancelAt = Tuning.StepTime * 0.85f,
                StepVelocity = dir * Tuning.StepDistance / Tuning.StepTime };
            ActionStarted?.Invoke(kind, 0);
        }
    }

    private int MaxChain => Equipped == Weapon.Knife ? Tuning.KnifeMaxChain : 3;

    /// <summary>
    /// True when a target at <paramref name="target"/> (feet) with body radius <paramref name="radius"/> is
    /// inside a cut's reach and arc from <paramref name="origin"/> facing <paramref name="dir"/>.
    /// </summary>
    public static bool InBladeArc(Vector2 origin, Vector2 dir, float reach, float arcDegrees, Vector2 target, float radius)
    {
        Vector2 v = target - origin;
        float d = v.Length();
        if (d > reach + radius) return false;
        if (d < radius + 4f) return true; // point blank
        return Mathf.Abs(Mathf.RadToDeg(dir.AngleTo(v))) <= arcDegrees;
    }

    private void Advance(float dt)
    {
        var a = _act!;
        a.T += dt;
        if (a.Kind == ActionKind.Cut && !a.Live && a.T >= a.Startup)
        {
            a.Live = true; // the blade goes live: lunge and leave the slash
            MoveAndCollide(Facing * a.Lunge);
            float side = a.N % 2 == 1 ? 55f : -55f;
            _slashes.Add(new Slash
            {
                World = GlobalPosition + (Equipped == Weapon.Knife ? new Vector2(Facing.X * 14f, -12f + Facing.Y * 10f) : new Vector2(Facing.X * 22f, -14f + Facing.Y * 16f)),
                Angle = Facing.Angle() + Mathf.DegToRad(side) + Mathf.Pi / 2f,
                Length = Equipped == Weapon.Knife ? 16f : a.N == 3 ? 44f : 34f, Life = 0.16f, Max = 0.16f,
            });
        }
        if (a.Kind == ActionKind.Step) _ghosts.Add((GlobalPosition, 0.22f));
        if (a.T >= a.Duration) _act = null;
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        // step afterimages
        foreach (var (world, life) in _ghosts)
        {
            Vector2 o = world - GlobalPosition;
            DrawRect(new Rect2(o.X - 6, o.Y - 40, 12, 40), new Color(LearnGlow, life / 0.22f * 0.25f));
        }
        // ground shadow
        var sh = new Color(8 / 255f, 10 / 255f, 8 / 255f, 0.38f);
        DrawRect(new Rect2(-11, -2, 22, 4), sh); DrawRect(new Rect2(-8, -3, 16, 6), sh);
        if (_invulnerable <= 0 || Mathf.PosMod(_time, 0.12f) < 0.08f) DrawFighter();
        if (_hurtFlash > 0) DrawRect(new Rect2(-8, -40, 16, 40), new Color(0.75f, 0.2f, 0.15f, _hurtFlash));
        // the slash: straight and clean, like the cut in the master's stone
        foreach (var s in _slashes)
        {
            Vector2 c = s.World - GlobalPosition;
            float k = Mathf.Min(1f, s.Life / s.Max * 1.5f), dx = Mathf.Cos(s.Angle), dy = Mathf.Sin(s.Angle) * 0.7f;
            for (float i = -s.Length / 2; i < s.Length / 2; i++)
                DrawRect(new Rect2(Mathf.Round(c.X + dx * i), Mathf.Round(c.Y + dy * i), 2, 2), new Color(Mathf.Abs(i) < s.Length / 3 ? Learn : LearnGlow, k));
        }
    }

    // A port of the prototype's drawFighter for the player (idle, walk, cut, step), feet at (0, 0).
    private void DrawFighter()
    {
        var p = _paint;
        float face = Facing.Angle(), fx = Mathf.Cos(face), fy = Mathf.Sin(face);
        string view = Mathf.Abs(fy) > Mathf.Abs(fx) * 0.8f ? (fy > 0 ? "down" : "up") : "side";
        int s = fx < 0 ? -1 : 1;
        float x = 0, y = 0, sy = y - 25, nx = -fy;
        Vector2 hand, tip;
        float bladeLen = Equipped == Weapon.Knife ? 0.4f : 1f;
        if (_act is { Kind: ActionKind.Cut } cut)
        {
            float k = Mathf.Clamp((cut.T - cut.Startup) / cut.Active, 0f, 1f), from = cut.N % 2 == 1 ? -75f : 75f;
            float a = face + Mathf.DegToRad(from + (-from * 2f) * k);
            hand = new Vector2(x + Mathf.Cos(a) * 11, sy + 4 + Mathf.Sin(a) * 8);
            tip = hand + new Vector2(Mathf.Cos(a) * 20, Mathf.Sin(a) * 14) * bladeLen;
        }
        else
        {
            hand = new Vector2(x + nx * 7 * (view == "side" ? -s : 1) + fx * 3, sy + 9);
            tip = hand + new Vector2(fx * 14, 9 + fy * 4) * bladeLen;
        }
        bool back = view == "up";
        void Arms()
        {
            float shX = x + (view == "side" ? fx * 2 : nx * 6), shY = sy + 2;
            p.Line(shX, shY, hand.X, hand.Y, Robe, 3);
            if (Equipped == Weapon.Sword) { p.Line(hand.X, hand.Y, tip.X, tip.Y, Blade, 1); p.Rect(hand.X - 1, hand.Y - 1, 3, 3, Hilt); }
            else if (Equipped == Weapon.Knife) { p.Line(hand.X, hand.Y, tip.X, tip.Y, Flint, 2); p.Rect(hand.X - 1, hand.Y - 1, 3, 3, Hilt); }
            p.Rect(hand.X, hand.Y, 2, 2, Skin);
        }
        if (back) Arms();
        // legs
        float wk = _moving ? Mathf.Sin(_walk) : 0f;
        p.Rect(x - 5, y - 7 - Mathf.Max(0, wk) * 2, 4, 7, Pants);
        p.Rect(x + 1, y - 7 - Mathf.Max(0, -wk) * 2, 4, 7, Pants);
        // robe, flaring to the hem
        float top = y - 27; const int rows = 21;
        int hem = _moving ? Mathf.RoundToInt(Mathf.Sin(_walk * 2)) : 0;
        for (int r = 0; r < rows; r++)
        {
            float w = 12 + Mathf.Floor(r * 4f / rows) * 2, rx = x - w / 2 + (r > rows - 6 ? hem : 0);
            p.Rect(rx, top + r, w, 1, Robe); p.Rect(rx + (s > 0 ? 0 : w - 3), top + r, 3, 1, Shade);
            if (r == 2 || r == rows - 1) p.Rect(rx + 3, top + r, w - 6, 1, r == 2 ? Trim : Shade);
        }
        p.Rect(x - 6, top + 9, 12, 2, Sash);
        if (view != "up") p.Rect(x - 1 + (view == "side" ? s * 2 : 0), top, 2, 10, Trim);
        // head
        int breath = _act == null && !_moving && Mathf.Sin(_time * 2.4f) > 0.55f ? 1 : 0;
        float hx = x, hy = y - 38 + breath;
        p.Rect(hx - 5, hy + 1, 10, 9, Skin); p.Rect(hx - 4, hy, 8, 1, Skin); p.Rect(hx - 4, hy + 10, 8, 1, Skin);
        if (view == "down")
        {
            p.Rect(hx - 5, hy, 10, 4, Hair); p.Rect(hx - 6, hy + 1, 1, 4, Hair); p.Rect(hx + 5, hy + 1, 1, 4, Hair);
            p.Rect(hx - 3, hy + 6, 1, 1, Outline); p.Rect(hx + 2, hy + 6, 1, 1, Outline);
        }
        else if (view == "up") p.Rect(hx - 5, hy, 10, 10, Hair);
        else
        {
            p.Rect(hx - 5 * s - (s < 0 ? 2 : 0), hy, 7, 7, Hair); p.Rect(hx - 5, hy, 10, 3, Hair);
            p.Rect(hx + 3 * s - (s < 0 ? 1 : 0), hy + 5, 1, 1, Outline); p.Rect(hx + 5 * s - (s < 0 ? 1 : 0), hy + 5, 1, 2, Skin);
        }
        p.Rect(hx - 2, hy - 3, 4, 3, Hair); // topknot
        float ph = Mathf.Sin(_time * 6);
        p.Line(hx, hy - 2, hx - fx * 7 + ph, hy - 1 - fy * 2 + 3, Trim, 1); // ribbon
        if (!back) Arms();
        p.Flush(this, Outline);
    }
}
