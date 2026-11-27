using System;
using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Combat;

namespace SpiritOfMurin.Characters;

/// <summary>
/// The swordsman: walks with the move actions, cuts (sword: chain of up to three; stone knife: a
/// short cut, chain of two) and steps. The cut follows the direction the character faces, which is
/// the last direction walked. With empty hands there is no cut. Drawn in code using pixel-run
/// matrices with Jade Green &amp; Imperial Gold palette, slimmed side profile, and dual-arm walk swing.
/// Input actions (Project Settings &gt; Input Map): move_left/right/up/down, cut, step, sneak.
/// </summary>
public partial class Player : CharacterBody2D
{
    /// <summary>What the body is doing right now.</summary>
    public enum ActionKind { None, Cut, Step, Thrust, Qi, Side, Dash, Recoil }

    /// <summary>A qi shot leaving the hand: where from, which way, how far and fast, its damage, and whether it is the Silent Blade Step.</summary>
    public readonly record struct QiShot(Vector2 Origin, Vector2 Dir, float Range, float Speed, int Damage, bool Lamina, float Stagger);

    /// <summary>The prologue techniques and their state (full, sealed, weakened); null = none known (the plain body).</summary>
    public TechniqueSet? Tech { get; set; }

    /// <summary>The locked target (Space), or null. Provided by the scene (Vale: an enemy; Planície: the focused animal).</summary>
    public Func<Node2D?>? LockTarget { get; set; }

    /// <summary>Current Qi.</summary>
    public float Qi { get; set; }

    /// <summary>Maximum Qi.</summary>
    public float MaxQi { get; set; }

    /// <summary>Qi regained per second.</summary>
    public float QiRegen { get; set; }

    /// <summary>Raised when a qi shot (or the Silent Blade Step) leaves the hand.</summary>
    public event Action<QiShot>? QiFired;

    /// <summary>Raised when a technique is refused: technique id and the message (sealed, not enough Qi or vigor).</summary>
    public event Action<string, string>? TechniqueRefused;

    /// <summary>Raised when a hit passes through the side step's untouchable moment.</summary>
    public event Action? Evaded;

    /// <summary>Raised when a side step starts around a target (the scene makes it lose track).</summary>
    public event Action<Node2D>? SideStepped;

    /// <summary>Raised when a side step or a dash ends (the scene can move the body off a gap).</summary>
    public event Action<Vector2>? MoveEnded;

    /// <summary>Collision layer of gaps a side step or a dash may cross (bit value).</summary>
    public const uint GapLayer = 16;

    /// <summary>Half angle, degrees, of the blade's arc for the current action (the thrust is narrow).</summary>
    public float BladeArcDegrees => _act is { Kind: ActionKind.Thrust } ? 20f : Tuning.HitArcDegrees;

    /// <summary>Pixels a region adds to a cut's reach when it tests a hit (the prologue tests a little wider); the assist mode draws with it.</summary>
    public float CutSlack { get; set; }
    /// <summary>Pixels a region adds to a thrust's reach when it tests a hit.</summary>
    public float ThrustSlack { get; set; }

    /// <summary>
    /// The blade's area now, as the hit test sees it: (origin, facing, radius, half-angle in degrees, phase)
    /// with phase 0 = startup (warning), 1 = live, 2 = recovery; null when not cutting or thrusting.
    /// </summary>
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

    /// <summary>True during the third cut of the chain (heavier hit).</summary>
    public bool HeavyCut => _act is { Kind: ActionKind.Cut, N: 3 };

    /// <summary>Starts a parry recoil: the body is thrown back along <paramref name="dir"/>.</summary>
    public void Recoil(Vector2 dir, float push)
    {
        _act = new Act { Kind = ActionKind.Recoil, Startup = 0.24f, CancelAt = 0.24f, StepVelocity = dir.Normalized() * push * 6f };
    }

    /// <summary>
    /// Lying on the ground (after the explosion, at the awakening). 1 = flat; values between 0 and 1 draw
    /// the getting up (kneeling). The body takes no input while lying.
    /// </summary>
    public float Lying { get; set; }

    /// <summary>Thrown through the air: rotation of the body in radians (0 = upright) and height above the ground, px.</summary>
    public float TumbleAngle { get; set; }

    /// <summary>Height above the ground while thrown, px.</summary>
    public float Airborne { get; set; }

    /// <summary>
    /// Ignores cut and step presses until the mouse buttons and the keys are released: the press that
    /// closed a panel never becomes an attack, a step or a use in the world.
    /// </summary>
    public void ReleaseGuard() { _guard = true; _buffer = null; }

    private bool _guard;

    /// <summary>Turns the body to face a direction (scenes and cutscenes).</summary>
    public void Face(Vector2 dir) { if (dir.LengthSquared() > 0.0001f) Facing = dir.Normalized(); }

    // ---- free aim by the mouse (pedido 13, item 7) ----
    private Vector2 _mouseView;
    private bool _mouseSeen;

    /// <summary>Where the aim starts: the chest, a little above the feet.</summary>
    public static readonly Vector2 AimOrigin = new(0, -14);

    /// <inheritdoc />
    public override void _Input(InputEvent e)
    {
        // the cursor as the game sees it (window → picture coordinates, already through the stretch)
        if (e is InputEventMouse m) { _mouseView = m.Position; _mouseSeen = true; }
    }

    /// <summary>The cursor in the world (through the camera and the zoom), or null when aiming by facing or before the mouse moved.</summary>
    public Vector2? AimWorld => !Game.Aim.ByMouse || !_mouseSeen ? null : GetCanvasTransform().AffineInverse() * _mouseView;

    /// <summary>The direction an attack would go now: the locked target, else the cursor, else the facing.</summary>
    public Vector2 AimDirection
    {
        get
        {
            if (LockTarget?.Invoke() is { } t && IsInstanceValid(t)) { Vector2 to = t.GlobalPosition - GlobalPosition; if (to.LengthSquared() > 1) return to.Normalized(); }
            if (AimWorld is { } w) { Vector2 d = w - (GlobalPosition + AimOrigin); if (d.LengthSquared() > 4) return d.Normalized(); }
            return Facing;
        }
    }

    // turns the body to where the attack goes (the pose, the blade, the shot and the hit area all read Facing)
    private void TurnToAim() => Facing = AimDirection;

    /// <summary>Asks for a qi shot (K). It waits in the buffer like a cut or a step.</summary>
    public void RequestQi() { if (!InputLocked && Health > 0 && Lying <= 0) _buffer = (ActionKind.Qi, Tuning.BufferTime); }

    private readonly List<(ActionKind Kind, float T0)> _hist = new();
    private float _evade;

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
    public int MaxHealth => _maxHealth > 0 ? _maxHealth : Tuning.MaxHealth;

    /// <summary>Sets the maximum Vida (VIT). Raising it adds the difference to the current Vida.</summary>
    public void SetMaxHealth(int max)
    {
        int old = MaxHealth;
        _maxHealth = Mathf.Max(1, max);
        if (_maxHealth > old) Health += _maxHealth - old;
        Health = Mathf.Min(Health, _maxHealth);
    }

    private int _maxHealth;

    /// <summary>Walking speed multiplier (AGI). 1 = the tuning's speed.</summary>
    public float MoveSpeedMultiplier { get; set; } = 1f;

    /// <summary>Step distance multiplier (AGI).</summary>
    public float StepDistanceMultiplier { get; set; } = 1f;

    /// <summary>Recovery time multiplier of cuts and steps (AGI; below 1 is faster).</summary>
    public float RecoveryMultiplier { get; set; } = 1f;

    /// <summary>Current vigor (physical stamina; steps and cuts spend it).</summary>
    public float Vigor { get; set; } = 1f;

    /// <summary>Maximum vigor (CON). 0 disables the vigor rule (the sword test field).</summary>
    public float MaxVigor { get; set; }

    /// <summary>Vigor regained per second after <see cref="VigorRegenDelay"/>.</summary>
    public float VigorRegen { get; set; }

    /// <summary>Seconds after spending vigor before it comes back.</summary>
    public float VigorRegenDelay { get; set; }

    /// <summary>Vigor a step costs.</summary>
    public float StepVigorCost { get; set; }

    /// <summary>Vigor a cut costs.</summary>
    public float CutVigorCost { get; set; }

    /// <summary>Raised when a cut or a step was refused for lack of vigor.</summary>
    public event Action? OutOfBreath;

    private float _vigorWait;

    /// <summary>Restores vigor at once (up to the maximum).</summary>
    public void RestoreVigor(float amount) => Vigor = Mathf.Min(MaxVigor, Vigor + amount);

    private bool SpendVigor(float cost)
    {
        if (MaxVigor <= 0 || cost <= 0) return true;
        if (Vigor < cost) { OutOfBreath?.Invoke(); return false; }
        Vigor -= cost; _vigorWait = VigorRegenDelay;
        return true;
    }

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

    // Paleta base: Verde Jade + Amarelo/Dourado Imperial e Marfim Quente
    private static readonly Color
        Robe = new("#f5eedc"),         // túnica marfim dourado claro
        Shade = new("#cbb37c"),        // sombra ocre quente
        Trim = new("#2f7a53"),         // debrum e detalhes verde jade
        Sash = new("#1c4f35"),         // faixa principal verde esmeralda profundo
        SashLight = new("#68c28e"),    // brilho verde jade claro da fita e faixa
        Scabbard = new("#3d2817"),     // bainha marrom escuro
        Skin = new("#e2b58e"),         // pele
        SkinShade = new("#c48b6c"),    // sombra de pele (mão traseira)
        Hair = new("#1c181d"),         // cabelo escuro quente
        Pants = new("#3b352c"),        // calça marrom-carvão
        Blade = new("#f7f9fc"),        // lâmina polida
        BladeEdge = new("#d6c7a1"),    // fio dourado pálido
        Hilt = new("#5c3a1e"),         // cabo de madeira escura
        Gold = new("#f5cb5c"),         // guarda e fivela dourada
        GoldDeep = new("#d49b27"),     // detalhes amarelo-dourado
        Outline = new("#17130e"),      // contorno marrom-preto quente
        Learn = new("#e6fff0"),        // centro do risco de Qi jade claro
        LearnGlow = new("#4eb87d"),    // brilho de Qi verde esmeralda
        Flint = new("#8c8a80");        // faca de pedra

    /// <inheritdoc />
    public override void _Ready()
    {
        Equipped = StartWeapon;
        Health = MaxHealth;
    }

    /// <summary>Takes damage from a hit coming from <paramref name="from"/>. Ignored while invulnerable. Returns true when it landed.</summary>
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

    /// <summary>True while hurt-invulnerable.</summary>
    public bool Invulnerable => _invulnerable > 0;

    /// <summary>Restores Vida at once (up to the maximum).</summary>
    public void Heal(float amount) => Health = Mathf.Min(MaxHealth, Health + amount);

    /// <summary>Restores <paramref name="amount"/> Vida gradually over <paramref name="seconds"/> (stacks with what is pending).</summary>
    public void AddRegen(float amount, float seconds)
    {
        _regenLeft += amount;
        _regenRate = Mathf.Max(_regenRate, amount / Mathf.Max(0.1f, seconds));
    }

    /// <summary>Back to full Vida, nothing pending (after fainting).</summary>
    public void Revive() { Health = MaxHealth; Vigor = MaxVigor; _regenLeft = 0; _invulnerable = Tuning.HurtInvulnerability; _act = null; _buffer = null; }

    /// <inheritdoc />
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
            if (move != Vector2.Zero) { Facing = move.Normalized(); _moving = true; _walk += dt * (sneaking ? 4.5f : 8f); }
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

    /// <summary>When true the long step needs Shift held (the Planície: L + direction stays the plain step).</summary>
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
            TurnToAim();
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
            TurnToAim();
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

    /// <inheritdoc />
    public override void _Draw()
    {
        // Step afterimages (silhouette-shaped trailing glow in Jade Qi)
        foreach (var (world, life) in _ghosts)
        {
            Vector2 o = world - GlobalPosition;
            float alpha = life / 0.22f * 0.30f;
            DrawRect(new Rect2(o.X - 5, o.Y - 26, 10, 24), new Color(LearnGlow, alpha));
            DrawRect(new Rect2(o.X - 4, o.Y - 37, 8, 11), new Color(LearnGlow, alpha));
            DrawRect(new Rect2(o.X - 2, o.Y - 40, 4, 3), new Color(Learn, alpha * 1.2f));
        }
        // Ground shadow
        var sh = new Color(10 / 255f, 8 / 255f, 6 / 255f, 0.38f);
        DrawRect(new Rect2(-11, -2, 22, 4), sh); DrawRect(new Rect2(-8, -3, 16, 6), sh);
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
        p.Rect(-15, -7, 24, 7, Robe); p.Rect(-15, -7, 24, 2, Shade); p.Rect(-6, -7, 3, 7, Sash); p.Rect(-15, -2, 24, 1, Trim);
        p.Rect(-21, -6, 6, 3, Pants); p.Rect(-21, -3, 6, 3, Pants);
        p.Rect(9, -9, 9, 9, Skin); p.Rect(9, -9, 9, 3, Hair); p.Rect(17, -8, 3, 4, Hair);
        p.Rect(13, -5, 1, 1, Outline);
        float ph = Mathf.Sin(_time * 2);
        p.Line(18, -7, 23 + ph, -2, Trim, 1);
        p.Rect(-4, -10, 8, 3, Robe);
        p.Flush(this, Outline);
        if (Mathf.Sin(_time * 1.6f) > 0.4f) DrawRect(new Rect2(3, -10, 1, 1), new Color(LearnGlow, 0.4f));
    }

    private void DrawKneeling()
    {
        var p = _paint;
        float x = 0, y = 0, k = 1 - Lying;
        float top = y - 18 - k * 9;
        p.Rect(x - 7, y - 5, 6, 5, Pants); p.Rect(x + 1, y - 8, 4, 8, Pants);
        for (int r = 0; r < 14; r++) { float w = 12 + Mathf.Floor(r * 3f / 14) * 2; p.Rect(x - w / 2, top + r, w, 1, Robe); p.Rect(x - w / 2, top + r, 3, 1, Shade); }
        p.Rect(x - 6, top + 6, 12, 3, Sash);
        float hy = top - 11;
        p.Rect(x - 5, hy + 1, 10, 9, Skin); p.Rect(x - 5, hy, 10, 4, Hair); p.Rect(x - 2, hy - 3, 4, 3, Hair);
        p.Rect(x - 3, hy + 6, 1, 1, Outline); p.Rect(x + 2, hy + 6, 1, 1, Outline);
        p.Line(x + 5, top + 2, x + 11, y - 1, Robe, 3); p.Rect(x + 10, y - 2, 2, 2, Skin);
        p.Flush(this, Outline);
    }

    // Pixel matrices (12x14 head; 20x21 front/back robe; slim 16x21 side robe)
    private static readonly string[] CabecaFrente =
    {
        "......hh....",
        ".....hvvh...",
        ".....hhaa...",
        "....ttjtt...",
        "..hhvvvvhh..",
        ".hvvahhhhvh.",
        ".hvhhlphhvh.",
        ".hhhplllphh.",
        ".hhpplllphh.",
        ".hhisssipph.",
        ".hhplpllpvh.",
        ".hhsppppph..",
        "..hsspppsh..",
        "....spps....",
    };

    private static readonly string[] CabecaCostas =
    {
        "......hh....",
        ".....hvvh...",
        ".....hvah...",
        "....ttjtt...",
        "..hhvvvvhh..",
        ".hvvavvhvvh.",
        ".hvahhvvhvh.",
        ".hhvhvvhhvh.",
        ".hhvhvhhhvh.",
        ".hhvhhvhhvh.",
        ".hhhhvvhhvh.",
        "..hhhvhhvh..",
        "...hhhhvh...",
        ".....hhh....",
    };

    private static readonly string[] CabecaPerfil =
    {
        ".....hh.....",
        "....hvah....",
        "....hvvh....",
        "...ttjjtt...",
        "..hhvvvvhh..",
        ".hvvavhhvvh.",
        ".hvahhhhhlp.",
        ".hvhhsppplp.",
        ".hvhhpppllp.",
        ".hhhhsppih..",
        "..hvhppppll.",
        "..hhhssppp..",
        "...hhssppp..",
        ".....sspp...",
    };

    private static readonly string[] TunicaFrente =
    {
        "....mmtmrrrrrtww....",
        "...mmmtrrrrrrtrww...",
        "...mmmmtrrrrtrrww...",
        "...mmddtrrrrtrrww...",
        "...mmddrtrrtrrrww...",
        "...mmddrtrrtrrrww...",
        "...mmmmrrttrrrrww...",
        "....mmmmrttrrrww....",
        "....ttttttttggtt....",
        "....ttttttttggtt....",
        "....dddddddddddd....",
        "....mmmmmrrrwrtt....",
        "...mmmmrmrrrwrttw...",
        "...mmmmrmrrrwrttw...",
        "..mmmmrrmrrrwrttww..",
        "..mmmmrmrrrrrwrttw..",
        "..mmmmrmrrrrrwrttw..",
        ".mmmmrrmrrrrrwrjjww.",
        ".mmmmrrmrrrrrwrjjww.",
        ".tttttttttttttttttt.",
        ".dddddddddmmmmmmmmm.",
    };

    private static readonly string[] TunicaCostas =
    {
        "....mmmmrrrrrrww....",
        "...mmmdddddddddww...",
        "...mmmmrdddddrrww...",
        "...mmddrrrrrrrrww...",
        "...mmddrrrrrrrrww...",
        "...mmddrrrrrrrrww...",
        "...mmmmrrrrrrrrww...",
        "....mmmmrrrrrrww....",
        "....tttttttttttt....",
        "....tttttttttttt....",
        "....dddddddddddd....",
        "....mmmmmrrrwrww....",
        "...mmmmrmrrrwrrww...",
        "...mmmmrmrrrwrrww...",
        "..mmmmrrmrrrwrrrww..",
        "..mmmmrmrrrrrwrrww..",
        "..mmmmrmrrrrrwrrww..",
        ".mmmmrrmrrrrrwrrrww.",
        ".mmmmrrmrrrrrwrrrww.",
        ".tttttttttttttttttt.",
        ".dddddddddmmmmmmmmm.",
    };

    private static readonly string[] TunicaPerfil =
    {
        "....mmmrrrrw....",
        "...mmmmrrrrtw...",
        "...mmmmmmmrtw...",
        "...mmddmmmrtw...",
        "...mmddmmmrtr...",
        "...mmddmmmrtr...",
        "...mmmmmmmrtr...",
        "....ttttttgt....",
        "....ttttttgt....",
        "....dddddddd....",
        "....mmmmmrtw....",
        "...mmmmmmrtjw...",
        "...mmmmmmrtjw...",
        "..mmmmmmmrtjww..",
        "..mmmmmmmrtjww..",
        "..mmmmmmmrtjww..",
        ".mmmmmmmmrrtjww.",
        ".mmmmmmmmrrtjww.",
        ".mmmmmmmmrrddww.",
        ".tttttttttttttt.",
        ".dddddddmmmmmmm.",
    };

    // Paleta Verde Jade + Amarelo/Dourado Imperial e Marfim Quente
    private static readonly Dictionary<char, Color> Pigmentos = new()
    {
        ['i'] = new("#17130e"), // contorno / olhos
        ['h'] = new("#1c181d"), // cabelo escuro
        ['v'] = new("#3b333d"), // volume do cabelo
        ['a'] = new("#5c505e"), // brilho no topo do cabelo
        ['s'] = new("#c48b6c"), // sombra de pele
        ['p'] = new("#e2b58e"), // pele base
        ['l'] = new("#f3cca4"), // luz da pele
        ['t'] = new("#2f7a53"), // faixa principal / debrum (verde jade)
        ['j'] = new("#68c28e"), // brilho verde jade da faixa e fita
        ['d'] = new("#1c4f35"), // sombra profunda da faixa e dobra (verde esmeralda escuro)
        ['m'] = new("#cbb37c"), // sombra da túnica (ocre quente)
        ['r'] = new("#f5eedc"), // túnica base (marfim dourado claro)
        ['w'] = new("#fffbf0"), // luz da túnica
        ['b'] = new("#4a4236"), // calça média
        ['g'] = new("#f5cb5c"), // placa/fivela dourada
        ['k'] = new("#1c1915"), // botas escuras
    };

    private void PintarLinhas(string[] rows, int left, int top, bool mirror, int hem = 0)
    {
        for (int y = 0; y < rows.Length; y++)
        {
            string row = rows[y];
            for (int start = 0; start < row.Length;)
            {
                char pigment = row[start];
                int end = start + 1;
                while (end < row.Length && row[end] == pigment) end++;
                if (pigment != '.')
                {
                    int offset = mirror ? row.Length - end : start;
                    _paint.Rect(left + offset + (y >= 16 ? hem : 0), top + y, end - start, 1, Pigmentos[pigment]);
                }
                start = end;
            }
        }
    }

    private void DrawFighter()
    {
        var p = _paint;
        float face = Facing.Angle(), fx = Mathf.Cos(face), fy = Mathf.Sin(face);
        string view = Mathf.Abs(fy) > Mathf.Abs(fx) * 0.8f ? (fy > 0 ? "down" : "up") : "side";
        int s = fx < 0 ? -1 : 1;

        bool dashing = _act is { Kind: ActionKind.Step or ActionKind.Side or ActionKind.Dash };
        float walkCycle = _moving ? Mathf.Sin(_walk) : 0f;
        int leanX = dashing ? Mathf.RoundToInt(fx * 2f) : 0;

        float x = 0, y = 0, sy = y - 25, nx = -fy;
        Vector2 hand, tip;
        float bladeLen = Equipped == Weapon.Knife ? 0.4f : 1f;

        if (_act is { Kind: ActionKind.Cut } cut)
        {
            float k = Mathf.Clamp((cut.T - cut.Startup) / cut.Active, 0f, 1f), from = cut.N % 2 == 1 ? -75f : 75f;
            float a = face + Mathf.DegToRad(from + (-from * 2f) * k);
            hand = new Vector2(x + leanX + Mathf.Cos(a) * 11, sy + 4 + Mathf.Sin(a) * 8);
            tip = hand + new Vector2(Mathf.Cos(a) * 20, Mathf.Sin(a) * 14) * bladeLen;
        }
        else if (_act is { Kind: ActionKind.Thrust })
        {
            hand = new Vector2(x + fx * 12, sy + 3 + fy * 8);
            tip = hand + new Vector2(fx * 22, fy * 15) * bladeLen;
        }
        else if (_act is { Kind: ActionKind.Qi })
        {
            hand = new Vector2(x - nx * 7 * s, sy + 9);
            tip = hand + new Vector2(fx * 6, 12) * bladeLen;
            p.Rect(x + fx * 11 + nx * 3, sy + 3 + fy * 8, 3, 3, LearnGlow);
            p.Rect(x + fx * 12 + nx * 3, sy + 4 + fy * 8, 1, 1, Learn);
        }
        else if (dashing)
        {
            hand = new Vector2(x + leanX - fx * 6 + nx * 6 * (view == "side" ? -s : 1), sy + 8);
            tip = hand + new Vector2(-fx * 13, 8) * bladeLen;
        }
        else if (view == "side")
        {
            float armSwing = walkCycle * 2.5f;
            hand = new Vector2(x + leanX - s * 1 + armSwing, sy + 9);
            tip = hand + new Vector2(s * 14, 7) * bladeLen;
        }
        else
        {
            float armSwing = walkCycle * 1.5f;
            hand = new Vector2(x + leanX + nx * 7 + fx * 3, sy + 9 + armSwing);
            tip = hand + new Vector2(fx * 14, 9 + fy * 4 + armSwing * 0.5f) * bladeLen;
        }

        // Bainha diagonal nas costas quando está de frente com espada
        if (Equipped == Weapon.Sword && view == "down")
        {
            p.Line(x + leanX + 4, sy - 3, x + leanX - 6, sy + 11, Scabbard, 2);
            p.Rect(x + leanX + 4, sy - 4, 2, 2, Gold);
        }

        bool back = view == "up";

        // Segundo braço (oposto ao braço da espada): balança em oposição aos passos
        void BackArm()
        {
            if (view == "side")
            {
                float backShX = x + leanX + s * 2f;
                float backShY = sy + 2f;
                float oppSwing = -walkCycle * 5.5f;
                float backHandX = backShX + oppSwing + (_moving ? s * 1.5f : s * 2.5f);
                float backHandY = sy + 8.5f - Mathf.Abs(walkCycle) * 1.2f;

                p.Line(backShX, backShY, backHandX, backHandY, Outline, 4);
                p.Line(backShX, backShY, backHandX, backHandY, Shade, 2);
                p.Rect(backHandX - 1, backHandY - 1, 2, 2, Sash);
                p.Rect(backHandX, backHandY, 2, 2, SkinShade);
            }
            else
            {
                float oppShX = x + leanX - nx * 6;
                float oppShY = sy + 2;
                float oppSwing = -walkCycle * 2.2f;
                float oppHandX = oppShX - nx * 1f - fx * 1f;
                float oppHandY = sy + 9f + oppSwing;

                p.Line(oppShX, oppShY, oppHandX, oppHandY, Shade, 4);
                p.Line(oppShX, oppShY, oppHandX, oppHandY, Robe, 2);
                p.Rect(oppHandX - 1, oppHandY - 2, 3, 2, Trim);
                p.Rect(oppHandX, oppHandY, 2, 2, Skin);
            }
        }

        void FrontArm()
        {
            float shX = x + leanX + (view == "side" ? -s * 1f : nx * 6), shY = sy + 2;

            if (view == "side")
            {
                // Contorno escuro para separar o braço da túnica no perfil
                p.Line(shX, shY, hand.X, hand.Y, Outline, 5);
                p.Line(shX, shY, hand.X, hand.Y, Shade, 3);
                p.Line(shX, shY, hand.X, hand.Y, Robe, 2);
                p.Rect(hand.X - 2, hand.Y - 3, 4, 2, Sash);
                p.Rect(hand.X - 2, hand.Y - 1, 4, 1, GoldDeep);
            }
            else
            {
                p.Line(shX, shY, hand.X, hand.Y, Shade, 4);
                p.Line(shX, shY, hand.X, hand.Y, Robe, 2);
                float midX = Mathf.Round((shX + hand.X) * 0.5f), midY = Mathf.Round((shY + hand.Y) * 0.5f);
                p.Rect(midX - 1, midY + 1, 3, 2, Shade);
                p.Rect(hand.X - 1, hand.Y - 2, 3, 2, Trim);
            }

            if (Equipped == Weapon.Sword)
            {
                p.Line(hand.X, hand.Y, tip.X, tip.Y, Blade, 1);
                Vector2 dir = (tip - hand).Normalized();
                Vector2 perp = new(-dir.Y, dir.X);
                p.Line(hand.X + dir.X * 3 + perp.X, hand.Y + dir.Y * 3 + perp.Y,
                       tip.X - dir.X * 2 + perp.X, tip.Y - dir.Y * 2 + perp.Y, BladeEdge, 1);
                p.Line(hand.X + dir.X * 2 - perp.X * 2, hand.Y + dir.Y * 2 - perp.Y * 2,
                       hand.X + dir.X * 2 + perp.X * 2, hand.Y + dir.Y * 2 + perp.Y * 2, Gold, 1);
                p.Line(hand.X - dir.X * 2, hand.Y - dir.Y * 2, hand.X + dir.X, hand.Y + dir.Y, Hilt, 2);
                p.Rect(hand.X - dir.X * 3, hand.Y - dir.Y * 3, 1, 1, Gold);
            }
            else if (Equipped == Weapon.Knife)
            {
                p.Line(hand.X, hand.Y, tip.X, tip.Y, Flint, 2);
                p.Rect(hand.X - 1, hand.Y - 1, 3, 3, Hilt);
            }
            p.Rect(hand.X, hand.Y, 2, 2, Skin);
        }

        if (view == "side" || back) BackArm();
        if (back) FrontArm();

        float stride = walkCycle;
        int legSpread = dashing ? 2 : 0;
        int legShift = view == "side" ? Mathf.RoundToInt(stride * 2f) : 0;
        void Boot(int left, float lift)
        {
            int top = -8 - Mathf.RoundToInt(lift * 2);
            p.Rect(left, top, 4, 7, Pants);
            p.Rect(left, top + 3, 4, 1, Shade);
            p.Rect(left - 1, top + 6, 6, 2, Pigmentos['k']);
            p.Rect(left + 1, top + 6, 2, 1, GoldDeep);
        }
        if (view == "side")
        {
            Boot(-4 - legSpread - legShift, Mathf.Max(0, stride));
            Boot(-2 + legSpread + legShift, Mathf.Max(0, -stride));
        }
        else
        {
            Boot(-6 - legSpread, Mathf.Max(0, stride));
            Boot(2 + legSpread, Mathf.Max(0, -stride));
        }

        bool profile = view == "side", mirror = profile && s < 0;
        int hem = _moving ? Mathf.RoundToInt(Mathf.Sin(_walk * 2)) : (dashing ? -Mathf.RoundToInt(fx) : 0);
        int robeLeft = profile ? -8 : -10;
        PintarLinhas(back ? TunicaCostas : profile ? TunicaPerfil : TunicaFrente, robeLeft + leanX, -27, mirror, hem);

        int breath = _act == null && !_moving && Mathf.Sin(_time * 2.4f) > 0.55f ? 1 : 0;
        int hy = -41 + breath;
        float speedFactor = _moving ? 1.5f : 1f;
        float flutter = Mathf.Sin(_time * 6f * speedFactor);
        float flutterTip = Mathf.Sin(_time * 6f * speedFactor - 1.1f);
        float tail = profile ? -s * 9 : back ? 8 : -8;

        p.Line(leanX, hy + 3, leanX + tail / 2, hy + 4 + flutter * 0.5f, Trim, 1);
        p.Line(leanX + tail / 2, hy + 4 + flutter * 0.5f, leanX + tail + flutterTip, hy + 7, Pigmentos['j'], 1);

        PintarLinhas(back ? CabecaCostas : profile ? CabecaPerfil : CabecaFrente, -6 + leanX, hy, mirror);

        if (!back)
        {
            if (view != "side") BackArm();
            FrontArm();
        }
        p.Flush(this, Outline);
    }
}