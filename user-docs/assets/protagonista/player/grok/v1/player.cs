using System;
using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Combat;

namespace SpiritOfMurin.Characters;

/// <summary>
/// The swordsman — versão de teste com cores amarelas + mais detalhe visual.
/// </summary>
public partial class Player : CharacterBody2D
{
    public enum ActionKind { None, Cut, Step, Thrust, Qi, Side, Dash, Recoil }

    public readonly record struct QiShot(Vector2 Origin, Vector2 Dir, float Range, float Speed, int Damage, bool Lamina, float Stagger);

    public TechniqueSet? Tech { get; set; }
    public Func<Node2D?>? LockTarget { get; set; }

    public float Qi { get; set; }
    public float MaxQi { get; set; }
    public float QiRegen { get; set; }

    public event Action<QiShot>? QiFired;
    public event Action<string, string>? TechniqueRefused;
    public event Action? Evaded;
    public event Action<Node2D>? SideStepped;
    public event Action<Vector2>? MoveEnded;

    public const uint GapLayer = 16;

    public float BladeArcDegrees => _act is { Kind: ActionKind.Thrust } ? 20f : Tuning.HitArcDegrees;
    public float CutSlack { get; set; }
    public float ThrustSlack { get; set; }

    public (Vector2 Dir, float Radius, float HalfDeg, int Phase, float Progress)? BladeArea()
    {
        if (_act is not { Kind: ActionKind.Cut or ActionKind.Thrust } a) return null;
        bool knife = Equipped == Weapon.Knife, thrust = a.Kind == ActionKind.Thrust;
        float reach = thrust ? a.Reach : knife ? Tuning.KnifeReach : Tuning.SwordReach;
        float r = reach + (thrust ? ThrustSlack : CutSlack);
        int phase = a.T < a.Startup ? 0 : a.T < a.Startup + a.Active ? 1 : 2;
        float prog = phase == 0 ? a.T / Mathf.Max(0.001f, a.Startup) : phase == 2 ? 1 - (a.T - a.Startup - a.Active) / Mathf.Max(0.001f, a.Recovery) : 1;
        return (Facing, r, thrust ? 20f : Tuning.HitArcDegrees, phase, Mathf.Clamp(prog, 0, 1));
    }

    public bool HeavyCut => _act is { Kind: ActionKind.Cut, N: 3 };

    public void Recoil(Vector2 dir, float push)
    {
        _act = new Act { Kind = ActionKind.Recoil, Startup = 0.24f, CancelAt = 0.24f, StepVelocity = dir.Normalized() * push * 6f };
    }

    public float Lying { get; set; }
    public float TumbleAngle { get; set; }
    public float Airborne { get; set; }

    public void ReleaseGuard() { _guard = true; _buffer = null; }
    private bool _guard;

    public void Face(Vector2 dir) { if (dir.LengthSquared() > 0.0001f) Facing = dir.Normalized(); }
    public void RequestQi() { if (!InputLocked && Health > 0 && Lying <= 0) _buffer = (ActionKind.Qi, Tuning.BufferTime); }

    private readonly List<(ActionKind Kind, float T0)> _hist = new();
    private float _evade;

    public enum Weapon { None, Knife, Sword }
    public enum NoiseLevel { Still, Sneak, Walk, Loud }

    [Export] public Weapon StartWeapon { get; set; } = Weapon.Sword;
    public Weapon Equipped { get; set; } = Weapon.Sword;

    public float Health { get; private set; }
    public int MaxHealth => _maxHealth > 0 ? _maxHealth : Tuning.MaxHealth;

    public void SetMaxHealth(int max)
    {
        int old = MaxHealth;
        _maxHealth = Mathf.Max(1, max);
        if (_maxHealth > old) Health += _maxHealth - old;
        Health = Mathf.Min(Health, _maxHealth);
    }
    private int _maxHealth;

    public float MoveSpeedMultiplier { get; set; } = 1f;
    public float StepDistanceMultiplier { get; set; } = 1f;
    public float RecoveryMultiplier { get; set; } = 1f;

    public float Vigor { get; set; } = 1f;
    public float MaxVigor { get; set; }
    public float VigorRegen { get; set; }
    public float VigorRegenDelay { get; set; }
    public float StepVigorCost { get; set; }
    public float CutVigorCost { get; set; }

    public event Action? OutOfBreath;
    private float _vigorWait;

    public void RestoreVigor(float amount) => Vigor = Mathf.Min(MaxVigor, Vigor + amount);

    private bool SpendVigor(float cost)
    {
        if (MaxVigor <= 0 || cost <= 0) return true;
        if (Vigor < cost) { OutOfBreath?.Invoke(); return false; }
        Vigor -= cost; _vigorWait = VigorRegenDelay;
        return true;
    }

    public float PendingRegen => _regenLeft;
    public bool InputLocked { get; set; }
    public NoiseLevel Noise { get; private set; } = NoiseLevel.Still;

    public event Action<Vector2, Vector2, float, int, int>? BladeActive;
    public event Action? EmptyHanded;
    public event Action<int>? Hurt;
    public event Action? Fainted;

    private float _invulnerable, _regenLeft, _regenRate, _hurtFlash;
    private int _swingId;

    public event Action<ActionKind, int>? ActionStarted;

    [Export] public PlayerTuning Tuning { get; set; } = new();

    public Vector2 Facing { get; private set; } = Vector2.Down;
    public ActionKind Current => _act?.Kind ?? ActionKind.None;
    public int CutNumber => _act is { Kind: ActionKind.Cut } a ? a.N : 0;
    public float LastCutAngle { get; private set; }

    private sealed class Act
    {
        public ActionKind Kind;
        public float T, Startup, Active, Recovery, CancelAt, Lunge;
        public int N, Damage;
        public Vector2 StepVelocity, From, Dir;
        public bool Live, Lamina, Fired;
        public float Reach, Radius, A0, A1;
        public Node2D? Orbit;
        public float Duration => Kind is ActionKind.Step or ActionKind.Side or ActionKind.Dash or ActionKind.Recoil ? Startup : Startup + Active + Recovery;
    }

    private sealed class Slash { public Vector2 World; public float Angle, Length, Life, Max; }

    private Act? _act;
    private (ActionKind Kind, float Time)? _buffer;
    private float _walk, _time;
    private bool _moving;
    private readonly List<Slash> _slashes = new();
    private readonly List<(Vector2 World, float Life)> _ghosts = new();
    private readonly PixelPainter _paint = new();

    // === CORES DE TESTE (AMARELO / DOURADO) ===
    private static readonly Color Robe     = new("#f5e8c0");   // amarelo claro
    private static readonly Color Shade    = new("#d4b86a");   // sombra amarelada
    private static readonly Color Trim     = new("#c9a227");   // dourado
    private static readonly Color Sash     = new("#c9a227");   // dourado
    private static readonly Color Skin     = new("#e2b58e");
    private static readonly Color Hair     = new("#1b191e");
    private static readonly Color Pants    = new("#3d4a44");
    private static readonly Color Blade    = new("#eef3f2");
    private static readonly Color Hilt     = new("#7a5a3a");
    private static readonly Color Outline  = new("#16140f");
    private static readonly Color Learn    = new("#d4fbf6");
    private static readonly Color LearnGlow= new("#6fd8d2");
    private static readonly Color Flint    = new("#8c8a80");

    public override void _Ready()
    {
        Equipped = StartWeapon;
        Health = MaxHealth;
    }

    public bool TakeDamage(int amount, Vector2 from)
    {
        if (_evade > 0 && Health > 0) { Evaded?.Invoke(); return false; }
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

    public bool Invulnerable => _invulnerable > 0;
    public void Heal(float amount) => Health = Mathf.Min(MaxHealth, Health + amount);

    public void AddRegen(float amount, float seconds)
    {
        _regenLeft += amount;
        _regenRate = Mathf.Max(_regenRate, amount / Mathf.Max(0.1f, seconds));
    }

    public void Revive() { Health = MaxHealth; Vigor = MaxVigor; _regenLeft = 0; _invulnerable = Tuning.HurtInvulnerability; _act = null; _buffer = null; }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        _time += dt;
        _invulnerable = Mathf.Max(0, _invulnerable - dt);
        _evade = Mathf.Max(0, _evade - dt);
        if (MaxQi > 0) Qi = Mathf.Min(MaxQi, Qi + QiRegen * dt);
        _hurtFlash = Mathf.Max(0, _hurtFlash - dt);
        if (_regenLeft > 0 && Health > 0)
        {
            float r = Mathf.Min(_regenLeft, _regenRate * dt);
            _regenLeft -= r; Heal(r);
            if (Health >= MaxHealth) _regenLeft = 0;
        }
        if (_regenLeft <= 0) _regenRate = 0;
        if (MaxVigor > 0)
        {
            if (_vigorWait > 0) _vigorWait -= dt;
            else Vigor = Mathf.Min(MaxVigor, Vigor + VigorRegen * dt);
        }
        bool locked = InputLocked || Health <= 0 || Lying > 0;
        Vector2 move = locked ? Vector2.Zero : Input.GetVector("move_left", "move_right", "move_up", "move_down");
        bool sneaking = !locked && InputMap.HasAction("sneak") && Input.IsActionPressed("sneak");

        if (_guard && !Input.IsActionPressed("cut") && !Input.IsActionPressed("step")) _guard = false;

        if (locked || _guard) { }
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
            Velocity = move * Tuning.MoveSpeed * MoveSpeedMultiplier * (sneaking ? Tuning.SneakFactor : 1f);
            if (move != Vector2.Zero) { Facing = move.Normalized(); _moving = true; _walk += dt * (sneaking ? 4f : 7f); }
        }
        else if (_act.Kind is ActionKind.Step or ActionKind.Recoil) Velocity = _act.StepVelocity;
        else Velocity = Vector2.Zero;
        if (_act is { Kind: ActionKind.Recoil } rc) rc.StepVelocity *= 0.86f;
        if (_act == null || _act.Kind is not (ActionKind.Side or ActionKind.Dash)) MoveAndSlide();

        if (LockTarget?.Invoke() is { } lockedOn && IsInstanceValid(lockedOn) && _act is not { Kind: ActionKind.Side or ActionKind.Dash })
        {
            Vector2 to = lockedOn.GlobalPosition - GlobalPosition;
            if (to.LengthSquared() > 1) Facing = to.Normalized();
        }
        Noise = _act != null ? NoiseLevel.Loud : !_moving ? NoiseLevel.Still : sneaking ? NoiseLevel.Sneak : NoiseLevel.Walk;

        if (_act is { Kind: ActionKind.Cut or ActionKind.Thrust, Live: true } live && live.T < live.Startup + live.Active)
        {
            bool knife = Equipped == Weapon.Knife;
            float reach = live.Kind == ActionKind.Thrust ? live.Reach : knife ? Tuning.KnifeReach : Tuning.SwordReach;
            int dmg = live.Kind == ActionKind.Thrust ? live.Damage : knife ? Tuning.KnifeDamage : Tuning.SwordDamage * (live.N == 3 ? 2 : 1);
            BladeActive?.Invoke(GlobalPosition, Facing, reach, dmg, _swingId);
        }

        for (int i = _slashes.Count - 1; i >= 0; i--) { _slashes[i].Life -= dt; if (_slashes[i].Life <= 0) _slashes.RemoveAt(i); }
        for (int i = _ghosts.Count - 1; i >= 0; i--) { var g = _ghosts[i]; g.Life -= dt; if (g.Life <= 0) _ghosts.RemoveAt(i); else _ghosts[i] = g; }
        QueueRedraw();
    }

    private (string Kind, int Side) Rel(Vector2 move)
    {
        if (move == Vector2.Zero) return ("none", 0);
        var tg = LockTarget?.Invoke();
        Vector2 f = tg != null && IsInstanceValid(tg) ? (tg.GlobalPosition - GlobalPosition).Normalized() : Facing;
        float fwd = move.Dot(f), lat = f.X * move.Y - f.Y * move.X;
        if (Mathf.Abs(lat) > Mathf.Abs(fwd) * 1.05f) return ("lat", Math.Sign(lat));
        return (fwd > 0 ? "fwd" : "back", 0);
    }

    private bool Pay(string id, TechniqueParams p)
    {
        string name = Tech?.Data.Get(id).Name ?? id;
        bool vigorRule = MaxVigor > 0 && p.VigorCost > 0;
        if (vigorRule && Vigor < p.VigorCost) { TechniqueRefused?.Invoke(id, $"{name}: falta vigor ({Mathf.FloorToInt(Vigor)}/{p.VigorCost:0})."); return false; }
        if (p.QiCost > 0 && Qi < p.QiCost) { TechniqueRefused?.Invoke(id, $"{name}: falta Qi ({Mathf.FloorToInt(Qi)}/{p.QiCost:0})."); return false; }
        if (vigorRule) { Vigor -= p.VigorCost; _vigorWait = VigorRegenDelay; }
        Qi -= p.QiCost;
        return true;
    }

    private bool Sealed(string id) => Tech?.State(id) == TechniqueState.Sealed;

    private TechniqueParams? LaminaParams()
    {
        if (Tech == null || Sealed("lamina")) return null;
        if (Tech.Params("lamina") is { } p) return p;
        return Tech.State("side_step") switch { TechniqueState.Full => Tech.Data.Lamina.Full, TechniqueState.Weakened => Tech.Data.Lamina.Weak, _ => null };
    }

    private void Refuse(string id, string what)
    {
        string m = $"{Tech?.Data.Get(id).Name ?? id} está selada: {what}";
        if (m == _lastRefusal && _time - _lastRefusalAt < 2.5f) return;
        _lastRefusal = m; _lastRefusalAt = _time;
        TechniqueRefused?.Invoke(id, m);
    }

    private string _lastRefusal = "";
    private float _lastRefusalAt = -10;

    public bool DashNeedsShift { get; set; }

    private void Start(ActionKind kind, Vector2 move)
    {
        var rel = Rel(move);
        var target = LockTarget?.Invoke();
        if (target != null && !IsInstanceValid(target)) target = null;

        if (kind == ActionKind.Qi)
        {
            if (Tech?.Params("qi_wave") is not { } qp)
            {
                if (Sealed("qi_wave")) Refuse("qi_wave", "o Qi não responde.");
                return;
            }
            if (!Pay("qi_wave", qp)) return;
            _act = new Act { Kind = ActionKind.Qi, Startup = 0.1f, Active = 0.02f, Recovery = 0.22f * RecoveryMultiplier, CancelAt = 0.16f };
            var lp = LaminaParams();
            if (lp != null && _hist.Count >= 2)
            {
                var p1 = _hist[^1]; var p2 = _hist[^2];
                float sideTime = Tech.Params("side_step")?.Time ?? 0.16f;
                _act.Lamina = p2.Kind == ActionKind.Side && p1.Kind is ActionKind.Cut or ActionKind.Thrust
                    && p1.T0 - p2.T0 <= sideTime + lp.ComboWindow && _time - p1.T0 <= lp.ComboWindow + 0.1f;
            }
            _hist.Add((ActionKind.Qi, _time)); if (_hist.Count > 6) _hist.RemoveAt(0);
            ActionStarted?.Invoke(kind, 0);
            return;
        }

        if (kind == ActionKind.Cut && target != null && rel.Kind == "fwd" && Equipped != Weapon.None && Tech?.State("thrust") is TechniqueState.Full or TechniqueState.Weakened)
        {
            var tp = Tech.Params("thrust")!;
            if (!Pay("thrust", tp)) return;
            _swingId++;
            _act = new Act { Kind = ActionKind.Thrust, Startup = 0.09f, Active = 0.1f, Recovery = 0.28f * RecoveryMultiplier, CancelAt = 0.17f * RecoveryMultiplier, Lunge = tp.Lunge, Reach = tp.Reach, Damage = tp.Damage };
            _hist.Add((ActionKind.Thrust, _time)); if (_hist.Count > 6) _hist.RemoveAt(0);
            LastCutAngle = Facing.Angle();
            ActionStarted?.Invoke(ActionKind.Thrust, 0);
            return;
        }

        if (kind == ActionKind.Step && target != null && rel.Kind == "lat" && Tech?.State("side_step") is not (null or TechniqueState.Unknown))
        {
            if (Tech.Params("side_step") is not { } sp) Refuse("side_step", "fica o passo comum.");
            else if (Pay("side_step", sp))
            {
                Vector2 from = GlobalPosition - target.GlobalPosition;
                float r = Mathf.Clamp(from.Length(), sp.RadiusMin, sp.RadiusMax), a0 = from.Angle();
                _act = new Act { Kind = ActionKind.Side, Startup = sp.Time, CancelAt = sp.Time * 0.7f, Orbit = target, Radius = r, A0 = a0, A1 = a0 - rel.Side * Mathf.DegToRad(sp.Angle) };
                _evade = Mathf.Max(_evade, sp.Iframes);
                SideStepped?.Invoke(target);
                _hist.Add((ActionKind.Side, _time)); if (_hist.Count > 6) _hist.RemoveAt(0);
                ActionStarted?.Invoke(ActionKind.Side, 0);
                return;
            }
            else return;
        }

        bool dashKey = !DashNeedsShift || InputMap.HasAction("sneak") && Input.IsActionPressed("sneak");
        if (kind == ActionKind.Step && target == null && move != Vector2.Zero && dashKey && Tech?.State("dash") is not (null or TechniqueState.Unknown))
        {
            if (Tech.Params("dash") is not { } dp) { Refuse("dash", "fica o passo comum."); }
            else if (Pay("dash", dp))
            {
                Facing = move.Normalized();
                _act = new Act { Kind = ActionKind.Dash, Startup = dp.Time, CancelAt = dp.Time * 0.72f, From = GlobalPosition, Dir = move.Normalized(), Radius = dp.Distance };
                _evade = Mathf.Max(_evade, 0.12f);
                _hist.Add((ActionKind.Dash, _time)); if (_hist.Count > 6) _hist.RemoveAt(0);
                ActionStarted?.Invoke(ActionKind.Dash, 0);
                return;
            }
            else return;
        }

        if (kind == ActionKind.Cut)
        {
            if (Equipped == Weapon.None || !SpendVigor(CutVigorCost)) return;
            int n = _act is { Kind: ActionKind.Cut } prev && prev.N < MaxChain ? prev.N + 1 : 1;
            _swingId++;
            if (Equipped == Weapon.Knife)
                _act = new Act
                {
                    Kind = ActionKind.Cut, N = n, Startup = Tuning.KnifeStartup, Active = Tuning.KnifeActive,
                    Recovery = Tuning.KnifeRecovery * RecoveryMultiplier, CancelAt = Tuning.KnifeCancelAt * RecoveryMultiplier, Lunge = Tuning.KnifeLunge,
                };
            else
            {
                float extra = n == 3 ? Tuning.ThirdCutExtra : 0f;
                _act = new Act
                {
                    Kind = ActionKind.Cut, N = n, Startup = Tuning.CutStartup + extra, Active = Tuning.CutActive + extra,
                    Recovery = (Tuning.CutRecovery + (n == 3 ? 0.1f : 0f)) * RecoveryMultiplier, CancelAt = (Tuning.CutCancelAt + (n == 3 ? 0.07f : 0f)) * RecoveryMultiplier,
                    Lunge = n == 3 ? Tuning.ThirdCutLunge : Tuning.CutLunge,
                };
            }
            LastCutAngle = Facing.Angle();
            _hist.Add((ActionKind.Cut, _time)); if (_hist.Count > 6) _hist.RemoveAt(0);
            ActionStarted?.Invoke(kind, n);
        }
        else
        {
            if (!SpendVigor(StepVigorCost)) return;
            Vector2 dir = move != Vector2.Zero ? move.Normalized() : -Facing;
            if (move != Vector2.Zero) Facing = dir;
            _act = new Act { Kind = ActionKind.Step, Startup = Tuning.StepTime, CancelAt = Tuning.StepTime * 0.85f * RecoveryMultiplier,
                StepVelocity = dir * Tuning.StepDistance * StepDistanceMultiplier / Tuning.StepTime };
            _hist.Add((ActionKind.Step, _time)); if (_hist.Count > 6) _hist.RemoveAt(0);
            ActionStarted?.Invoke(kind, 0);
        }
    }

    private int MaxChain => Equipped == Weapon.Knife ? Tuning.KnifeMaxChain : 3;

    public static bool InBladeArc(Vector2 origin, Vector2 dir, float reach, float arcDegrees, Vector2 target, float radius)
    {
        Vector2 v = target - origin;
        float d = v.Length();
        if (d > reach + radius) return false;
        if (d < radius + 4f) return true;
        return Mathf.Abs(Mathf.RadToDeg(dir.AngleTo(v))) <= arcDegrees;
    }

    private void Glide(Vector2 to)
    {
        uint mask = CollisionMask;
        CollisionMask = mask & ~GapLayer;
        MoveAndCollide(to - GlobalPosition);
        CollisionMask = mask;
    }

    private void Advance(float dt)
    {
        var a = _act!;
        a.T += dt;
        if (a.Kind == ActionKind.Qi && !a.Fired && a.T >= a.Startup)
        {
            a.Fired = true;
            var qp = Tech?.Params("qi_wave");
            var lp = a.Lamina ? LaminaParams() : null;
            Vector2 o = GlobalPosition + new Vector2(Facing.X * 14, -12 + Facing.Y * 10);
            if (lp != null) QiFired?.Invoke(new QiShot(o, Facing, lp.Range, lp.Speed, lp.Damage, true, lp.Stagger));
            else if (qp != null) QiFired?.Invoke(new QiShot(o, Facing, qp.Range, qp.Speed, qp.Damage, false, 0));
        }
        if (a.Kind == ActionKind.Side)
        {
            float k = Mathf.Min(1, a.T / a.Startup), e = 1 - (1 - k) * (1 - k);
            Vector2 c = a.Orbit != null && IsInstanceValid(a.Orbit) ? a.Orbit.GlobalPosition : a.From;
            if (a.From == Vector2.Zero) a.From = c;
            float ang = Mathf.Lerp(a.A0, a.A1, e);
            Glide(c + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * a.Radius);
            _ghosts.Add((GlobalPosition, 0.22f));
            if (a.T >= a.Startup) { _act = null; MoveEnded?.Invoke(new Vector2(Mathf.Cos(a.A1 + Mathf.Pi / 2), Mathf.Sin(a.A1 + Mathf.Pi / 2))); }
            return;
        }
        if (a.Kind == ActionKind.Dash)
        {
            float k = Mathf.Min(1, a.T / a.Startup), e = 1 - (1 - k) * (1 - k);
            Glide(a.From + a.Dir * a.Radius * e);
            _ghosts.Add((GlobalPosition, 0.22f));
            if (a.T >= a.Startup) { _act = null; MoveEnded?.Invoke(a.Dir); }
            return;
        }
        if (a.Kind is ActionKind.Cut or ActionKind.Thrust && !a.Live && a.T >= a.Startup)
        {
            a.Live = true;
            MoveAndCollide(Facing * a.Lunge);
            float side = a.Kind == ActionKind.Thrust ? 0f : a.N % 2 == 1 ? 55f : -55f;
            _slashes.Add(new Slash
            {
                World = GlobalPosition + (Equipped == Weapon.Knife ? new Vector2(Facing.X * 14f, -12f + Facing.Y * 10f) : new Vector2(Facing.X * 22f, -14f + Facing.Y * 16f)),
                Angle = Facing.Angle() + Mathf.DegToRad(side) + Mathf.Pi / 2f,
                Length = a.Kind == ActionKind.Thrust ? 10f : Equipped == Weapon.Knife ? 16f : a.N == 3 ? 44f : 34f, Life = 0.16f, Max = 0.16f,
            });
        }
        if (a.Kind == ActionKind.Step) _ghosts.Add((GlobalPosition, 0.22f));
        if (a.Kind == ActionKind.Recoil && a.T >= a.Startup) { _act = null; return; }
        if (a.T >= a.Duration) _act = null;
    }

    public override void _Draw()
    {
        foreach (var (world, life) in _ghosts)
        {
            Vector2 o = world - GlobalPosition;
            DrawRect(new Rect2(o.X - 6, o.Y - 40, 12, 40), new Color(LearnGlow, life / 0.22f * 0.25f));
        }

        var sh = new Color(8 / 255f, 10 / 255f, 8 / 255f, 0.38f);
        DrawRect(new Rect2(-11, -2, 22, 4), sh);
        DrawRect(new Rect2(-8, -3, 16, 6), sh);

        if (Game.Assist.Areas && BladeArea() is { } ba)
        {
            float half = Mathf.DegToRad(ba.HalfDeg), ang = ba.Dir.Angle();
            var c = Game.Assist.BladeC;
            if (ba.Phase == 0) { Game.Assist.SectorEdge(this, Vector2.Zero, ang, half, ba.Radius, new Color(c, 0.8f), 1, 4); Game.Assist.Sector(this, Vector2.Zero, ang, half, Mathf.Max(1, ba.Radius * ba.Progress), new Color(c, 0.15f)); }
            else if (ba.Phase == 1) { Game.Assist.Sector(this, Vector2.Zero, ang, half, ba.Radius, new Color(c, 0.4f)); Game.Assist.SectorEdge(this, Vector2.Zero, ang, half, ba.Radius, c, 3); }
            else Game.Assist.SectorEdge(this, Vector2.Zero, ang, half, ba.Radius * (0.35f + 0.65f * ba.Progress), new Color(c, 0.45f), 1, 2);
        }

        if (Lying >= 0.99f) { DrawLying(); return; }
        if (Lying > 0) { DrawKneeling(); return; }
        if (TumbleAngle != 0 || Airborne > 0)
        {
            var t = Transform2D.Identity.Translated(new Vector2(0, 20)).Rotated(TumbleAngle).Translated(new Vector2(0, -20 - Airborne));
            DrawSetTransformMatrix(t);
            DrawFighter();
            DrawSetTransformMatrix(Transform2D.Identity);
            return;
        }
        if (_invulnerable <= 0 || Mathf.PosMod(_time, 0.12f) < 0.08f) DrawFighter();
        if (_hurtFlash > 0) DrawRect(new Rect2(-8, -40, 16, 40), new Color(0.75f, 0.2f, 0.15f, _hurtFlash));

        foreach (var s in _slashes)
        {
            Vector2 c = s.World - GlobalPosition;
            float k = Mathf.Min(1f, s.Life / s.Max * 1.5f), dx = Mathf.Cos(s.Angle), dy = Mathf.Sin(s.Angle) * 0.7f;
            for (float i = -s.Length / 2; i < s.Length / 2; i++)
                DrawRect(new Rect2(Mathf.Round(c.X + dx * i), Mathf.Round(c.Y + dy * i), 2, 2), new Color(Mathf.Abs(i) < s.Length / 3 ? Learn : LearnGlow, k));
        }
    }

    private void DrawLying()
    {
        var p = _paint;
        p.Rect(-16, -8, 26, 8, Robe);
        p.Rect(-16, -8, 26, 2, Shade);
        p.Rect(-7, -8, 3, 8, Sash);
        p.Rect(-16, -2, 26, 1, Trim);

        p.Rect(-22, -7, 7, 4, Pants);
        p.Rect(-22, -3, 7, 3, Pants);

        p.Rect(10, -10, 10, 10, Skin);
        p.Rect(10, -10, 10, 4, Hair);
        p.Rect(18, -9, 4, 5, Hair);
        p.Rect(14, -5, 1, 1, Outline);

        p.Rect(-5, -11, 10, 3, Robe);
        p.Rect(3, -10, 2, 2, Skin);

        float ph = Mathf.Sin(_time * 2.2f);
        p.Line(19, -8, 25 + ph, -1, Trim, 1);

        p.Flush(this, Outline);

        if (Mathf.Sin(_time * 1.6f) > 0.35f)
            DrawRect(new Rect2(4, -11, 1, 1), new Color(LearnGlow, 0.45f));
    }

    private void DrawKneeling()
    {
        var p = _paint;
        float k = 1 - Lying;
        float top = -19 - k * 10;

        p.Rect(-8, -6, 7, 6, Pants);
        p.Rect(1, -9, 5, 9, Pants);

        for (int r = 0; r < 15; r++)
        {
            float w = 13 + Mathf.Floor(r * 3.2f / 15) * 2;
            p.Rect(-w / 2, top + r, w, 1, Robe);
            p.Rect(-w / 2, top + r, 3, 1, Shade);
        }
        p.Rect(-7, top + 7, 14, 2, Sash);

        float hy = top - 12;
        p.Rect(-5, hy + 1, 10, 10, Skin);
        p.Rect(-5, hy, 10, 4, Hair);
        p.Rect(-2, hy - 3, 4, 3, Hair);
        p.Rect(-3, hy + 6, 1, 1, Outline);
        p.Rect(2, hy + 6, 1, 1, Outline);

        p.Line(5, top + 3, 12, -2, Robe, 3);
        p.Rect(11, -3, 2, 2, Skin);

        p.Flush(this, Outline);
    }

    private void DrawFighter()
    {
        var p = _paint;
        float face = Facing.Angle(), fx = Mathf.Cos(face), fy = Mathf.Sin(face);
        string view = Mathf.Abs(fy) > Mathf.Abs(fx) * 0.8f ? (fy > 0 ? "down" : "up") : "side";
        int s = fx < 0 ? -1 : 1;
        float x = 0, y = 0, sy = y - 27, nx = -fy;

        Vector2 hand, tip;
        float bladeLen = Equipped == Weapon.Knife ? 0.4f : 1f;

        if (_act is { Kind: ActionKind.Cut } cut)
        {
            float k = Mathf.Clamp((cut.T - cut.Startup) / cut.Active, 0f, 1f);
            float from = cut.N % 2 == 1 ? -75f : 75f;
            float a = face + Mathf.DegToRad(from + (-from * 2f) * k);
            hand = new Vector2(x + Mathf.Cos(a) * 12, sy + 5 + Mathf.Sin(a) * 9);
            tip = hand + new Vector2(Mathf.Cos(a) * 21, Mathf.Sin(a) * 15) * bladeLen;
        }
        else if (_act is { Kind: ActionKind.Thrust })
        {
            hand = new Vector2(x + fx * 14, sy + 4 + fy * 9);
            tip = hand + new Vector2(fx * 23, fy * 16) * bladeLen;
        }
        else if (_act is { Kind: ActionKind.Qi })
        {
            hand = new Vector2(x - nx * 8 * s, sy + 11);
            tip = hand + new Vector2(fx * 6, 13) * bladeLen;
            p.Rect(x + fx * 13 + nx * 3, sy + 4 + fy * 9, 3, 3, LearnGlow);
        }
        else
        {
            hand = new Vector2(x + nx * 8 * (view == "side" ? -s : 1) + fx * 3, sy + 11);
            tip = hand + new Vector2(fx * 15, 10 + fy * 4) * bladeLen;
        }

        bool back = view == "up";

        void Arms()
        {
            float shX = x + (view == "side" ? fx * 4 : nx * 7);
            float shY = sy + 4;
            p.Line(shX, shY, hand.X, hand.Y, Robe, 3);

            if (Equipped == Weapon.Sword)
            {
                p.Line(hand.X, hand.Y, tip.X, tip.Y, Blade, 1);
                p.Rect(hand.X - 1, hand.Y - 1, 3, 3, Hilt);
            }
            else if (Equipped == Weapon.Knife)
            {
                p.Line(hand.X, hand.Y, tip.X, tip.Y, Flint, 2);
                p.Rect(hand.X - 1, hand.Y - 1, 3, 3, Hilt);
            }
            p.Rect(hand.X, hand.Y, 2, 2, Skin);
        }

        if (back) Arms();

        // === LEGS ===
        float wk = _moving ? Mathf.Sin(_walk) : 0f;
        p.Rect(x - 5, y - 9 - Mathf.Max(0, wk) * 2, 4, 9, Pants);
        p.Rect(x + 1, y - 9 - Mathf.Max(0, -wk) * 2, 4, 9, Pants);

        // === ROBE (mais detalhada) ===
        float top = y - 30;
        const int rows = 24;
        int hem = _moving ? Mathf.RoundToInt(Mathf.Sin(_walk * 2)) : 0;

        for (int r = 0; r < rows; r++)
        {
            float progress = r / (float)(rows - 1);
            float w = 13 + Mathf.Floor(progress * 6) * 2;
            float rx = x - w / 2 + (r > rows - 8 ? hem : 0);

            p.Rect(rx, top + r, w, 1, Robe);

            // sombra lateral
            p.Rect(rx + (s > 0 ? 0 : w - 4), top + r, 4, 1, Shade);

            // pregas internas
            if (r > 5 && r < rows - 5 && r % 2 == 0)
                p.Rect(rx + (s > 0 ? 5 : w - 7), top + r, 2, 1, Shade);
        }

        // gola
        p.Rect(x - 6, top + 2, 12, 1, Trim);
        p.Rect(x - 5, top + 3, 10, 1, Shade);

        // faixa da cintura (mais elaborada)
        p.Rect(x - 8, top + 11, 16, 3, Sash);
        p.Rect(x - 7, top + 12, 14, 1, Shade);
        p.Rect(x - 6, top + 13, 12, 1, Trim);

        // frente da túnica
        if (view != "up")
            p.Rect(x - 1 + (view == "side" ? s * 2 : 0), top + 2, 2, 12, Trim);

        // === HEAD ===
        int breath = _act == null && !_moving && Mathf.Sin(_time * 2.4f) > 0.55f ? 1 : 0;
        float hx = x, hy = y - 41 + breath;

        // rosto
        p.Rect(hx - 5, hy + 1, 10, 11, Skin);
        p.Rect(hx - 4, hy, 8, 1, Skin);
        p.Rect(hx - 4, hy + 12, 8, 1, Skin);

        if (view == "down")
        {
            p.Rect(hx - 5, hy, 10, 5, Hair);
            p.Rect(hx - 6, hy + 1, 1, 6, Hair);
            p.Rect(hx + 5, hy + 1, 1, 6, Hair);
            p.Rect(hx - 3, hy + 7, 1, 1, Outline);
            p.Rect(hx + 2, hy + 7, 1, 1, Outline);
        }
        else if (view == "up")
        {
            p.Rect(hx - 5, hy, 10, 12, Hair);
        }
        else
        {
            p.Rect(hx - 5 * s - (s < 0 ? 2 : 0), hy, 8, 9, Hair);
            p.Rect(hx - 5, hy, 10, 4, Hair);
            p.Rect(hx + 3 * s - (s < 0 ? 1 : 0), hy + 6, 1, 1, Outline);
            p.Rect(hx + 5 * s - (s < 0 ? 1 : 0), hy + 6, 1, 2, Skin);
        }

        // coque maior
        p.Rect(hx - 3, hy - 5, 6, 5, Hair);
        p.Rect(hx - 2, hy - 6, 4, 1, Hair);
        p.Rect(hx - 1, hy - 7, 2, 1, Hair);

        // fita
        float ph = Mathf.Sin(_time * 5.5f);
        p.Line(hx, hy - 4, hx - fx * 9 + ph * 1.8f, hy - 1 - fy * 2 + 5, Trim, 1);

        if (!back) Arms();

        p.Flush(this, Outline);
    }
}