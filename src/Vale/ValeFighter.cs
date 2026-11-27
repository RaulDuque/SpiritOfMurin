using Godot;
using SpiritOfMurin.Game;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Combat;

namespace SpiritOfMurin.Vale;

/// <summary>What a Vale fighter needs from the scene.</summary>
public interface IValeHost
{
    /// <summary>The swordsman.</summary>
    Player Player { get; }
    /// <summary>The combat numbers.</summary>
    ValeCombat Combat { get; }
    /// <summary>The story state.</summary>
    ValeState Story { get; }
    /// <summary>True while the player is down (fallen, respawning).</summary>
    bool PlayerDown { get; }
    /// <summary>A fighter's strike reached the player.</summary>
    void HitPlayer(ValeFighter by);
    /// <summary>A fighter speaks (speech bubble).</summary>
    void Say(ValeFighter who, string line);
    /// <summary>A fighter noticed the player for the first time.</summary>
    void OnAggro(ValeFighter who);
    /// <summary>The bridge disciple saw the player run away before the manual was found.</summary>
    void OnFlee(ValeFighter who);
    /// <summary>A strike's slash, for drawing.</summary>
    void EnemySlash(ValeFighter who, bool big);
    /// <summary>The fighter the player has locked on (null when none).</summary>
    ValeFighter? Lock { get; }
}

/// <summary>
/// An enemy of the prologue, ported from the prototype (index.html makeEnemy / updateEnemy): the
/// Disciple of the Closed Guard (parries anything from the front, turns fast, telegraphs a strike),
/// the Senior Swordsman (a closed guard that changes into an attack stance with a three-strike combo
/// and hyper armour, then an opening) and the master's stone statue (a target for practising the side
/// step: it parries from the front and answers a flank strike).
/// </summary>
public partial class ValeFighter : CharacterBody2D
{
    /// <summary>The kinds of fighter.</summary>
    public enum Kind { Disciple, Senior, Statue }

    /// <summary>The AI states.</summary>
    public enum St { Idle, Return, Guard, Advance, Windup, Strike, Recover, Flinch, Stagger, Dead }

    /// <summary>What it is.</summary>
    public Kind Type { get; set; }
    /// <summary>Stable id (dis_bridge, dis_herbs, dis_gap, dis_camp, senior, statue).</summary>
    public string Id { get; set; } = "";
    /// <summary>Where it stands when idle.</summary>
    public Vector2 Home { get; set; }
    /// <summary>Health points.</summary>
    public int Hp { get; set; }
    /// <summary>Full health.</summary>
    public int Max { get; set; }
    /// <summary>Facing angle, radians.</summary>
    public float Face { get; set; } = Mathf.Pi / 2;
    /// <summary>AI state.</summary>
    public St State { get; set; } = St.Idle;
    /// <summary>Seconds in the state.</summary>
    public float T { get; set; }
    /// <summary>"closed" guard or "attack" stance (the Senior).</summary>
    public bool AttackStance { get; set; }
    /// <summary>Seconds it still loses track of the player (after a side step).</summary>
    public float Fooled { get; set; }
    /// <summary>Seconds the guard stays broken.</summary>
    public float Stagger { get; set; }
    /// <summary>The bridge guardian (opens the gate when it falls).</summary>
    public bool Bridge { get; set; }
    /// <summary>Distance at which a non-bridge disciple notices the player.</summary>
    public float AggroR { get; set; } = 120;
    /// <summary>Distance from home after which it gives up.</summary>
    public float Leash { get; set; } = 300;
    /// <summary>What it says when it first sees the player.</summary>
    public string Line { get; set; } = "";
    /// <summary>Items it drops: id, quantity.</summary>
    public (string Id, int N)[] Drops { get; set; } = System.Array.Empty<(string, int)>();
    /// <summary>True while it lives.</summary>
    public bool Alive => State != St.Dead;
    /// <summary>True once it saw the player and spoke.</summary>
    public bool Said { get; set; }
    /// <summary>Parries in a row (two makes it strike back sooner).</summary>
    public int Parries { get; set; }
    /// <summary>Seconds the guard flash shows after a parry.</summary>
    public float GuardShow { get; set; }
    /// <summary>Hit flash.</summary>
    public float Flash { get; set; }
    /// <summary>Knockback velocity.</summary>
    public Vector2 Knock { get; set; }
    /// <summary>Strike number in the Senior's combo (0-2).</summary>
    public int Combo { get; private set; }
    /// <summary>Body radius.</summary>
    public float Radius => 8;
    /// <summary>The scene.</summary>
    public IValeHost Host { get; set; } = null!;
    /// <summary>A pose set by a cutscene (the AI stops while it is set).</summary>
    public FighterArt.Pose? Scripted { get; set; }
    /// <summary>Strength of the qi aura around the body (0-1; the ending).</summary>
    public float AuraPower { get; set; }
    /// <summary>Hidden by a cutscene (gone in the explosion).</summary>
    public bool Gone { get; set; }

    /// <summary>Brings it back for a cutscene: full health, standing, scripted pose.</summary>
    public void ScriptedRevive(FighterArt.Pose pose) { Hp = Max; State = St.Idle; Scripted = pose; Knock = Vector2.Zero; Stagger = 0; }

    private float _cd = 0.8f, _wt, _rt, _rtTotal, _stanceT, _walk, _lunge, _time;

    /// <summary>True when the next or current strike is the Senior's third, wide one.</summary>
    public bool BigStrike => Type == Kind.Senior && AttackStance && Combo == 2;

    /// <summary>
    /// The area the strike tests against the player: the radius (reach plus the body's slack) and the
    /// half-angle each side of the facing, in radians. The ground warning and the assist mode draw it.
    /// </summary>
    public (float Reach, float Half) StrikeArea()
    {
        var c = Host.Combat;
        bool big = BigStrike;
        float reach = (Type == Kind.Senior ? c.SenReach : c.DisReach) + (big ? c.StrikeBigExtra : 0) + c.StrikeSlack;
        return (reach, Mathf.DegToRad(big ? c.StrikeHalfBig : c.StrikeHalf));
    }

    /// <summary>The guard's half-angle in radians (hits from inside it are parried while <see cref="Guarding"/>).</summary>
    public float GuardHalfRad => Mathf.DegToRad(Host.Combat.GuardHalf);

    /// <summary>True when a hit coming from <paramref name="from"/> would be parried now (the same test as <see cref="Strike"/>).</summary>
    public bool WouldParry(Vector2 from) => Guarding && Mathf.Abs(Mathf.Wrap(Face - (from - GlobalPosition).Angle(), -Mathf.Pi, Mathf.Pi)) <= GuardHalfRad;

    /// <summary>0..1 through the warning (windup), 0 when not warning.</summary>
    public float WarnProgress => State == St.Windup ? Mathf.Clamp(T / Mathf.Max(0.01f, _wt), 0, 1) : 0;

    /// <summary>0..1 of the recovery still left (1 just after the strike), 0 when not recovering.</summary>
    public float RecoverLeft => State == St.Recover && _rtTotal > 0 ? Mathf.Clamp(1 - T / _rtTotal, 0, 1) : 0;
    private bool _struck;
    private int _rolls;
    private readonly PixelPainter _paint = new();

    /// <inheritdoc />
    public override void _Ready()
    {
        CollisionLayer = 4; CollisionMask = 1 | Player.GapLayer;
        AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 7 } });
        AddToGroup("vale_fighters");
    }

    /// <summary>True while the guard faces where hits come from (it parries from the front).</summary>
    public bool Guarding => Alive && Stagger <= 0 && State != St.Flinch && !(Type == Kind.Senior && AttackStance);

    /// <summary>The Senior's hyper armour: in the attack stance, hits do not interrupt him.</summary>
    public bool HyperArmor => Type == Kind.Senior && AttackStance && State != St.Recover;

    /// <summary>Back to home with full health (the player fell).</summary>
    public void ResetToHome()
    {
        if (!Alive || Type == Kind.Statue) return;
        GlobalPosition = Home; Hp = Max; State = St.Idle; AttackStance = false; _stanceT = 0; Fooled = 0; Parries = 0; Stagger = 0; Face = Mathf.Pi / 2; _cd = 0.8f; Knock = Vector2.Zero;
    }

    private float Rnd() { _rolls++; return (Plains.PlainsGenerator.Hash(Id.GetHashCode(), _rolls, 0, 31) >> 8) / 16777216f; }

    private void Enter(St s) { State = s; T = 0; }

    private void StartWindup(float wt) { Enter(St.Windup); _wt = wt; }

    private static float TurnToward(float a, float b, float max)
    {
        float d = Mathf.Wrap(b - a, -Mathf.Pi, Mathf.Pi);
        return Mathf.Abs(d) <= max ? b : a + Mathf.Sign(d) * max;
    }

    private void Move(Vector2 delta) { if (delta != Vector2.Zero) MoveAndCollide(delta); }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        _time += dt;
        Flash = Mathf.Max(0, Flash - dt);
        if (GuardShow > 0) GuardShow -= dt;
        QueueRedraw();
        if (!Alive) return;
        if (Scripted != null) return;
        if (Knock != Vector2.Zero) { Move(Knock * dt); Knock *= 0.82f; if (Mathf.Abs(Knock.X) + Mathf.Abs(Knock.Y) < 4) Knock = Vector2.Zero; }
        if (Type == Kind.Statue) return;
        var c = Host.Combat; var p = Host.Player;
        float d = GlobalPosition.DistanceTo(p.GlobalPosition), want = (p.GlobalPosition - GlobalPosition).Angle();
        T += dt;
        if (Fooled > 0) Fooled -= dt;
        float turn = Mathf.DegToRad(Type == Kind.Senior ? c.SenTurn : c.DisTurn) * dt;
        void Track(float k = 1) { if (Fooled <= 0) Face = TurnToward(Face, want, turn * k); }
        void WalkTo(Vector2 to, float spd) { Vector2 dir = (to - GlobalPosition).Normalized(); Move(dir * spd * dt); _walk += dt * spd / 20; }
        bool leashOut = Type == Kind.Disciple ? p.GlobalPosition.DistanceTo(Home) > Leash : p.GlobalPosition.Y > 13.2f * ValeMap.TS;

        if (Stagger > 0) { Stagger -= dt; if (Stagger <= 0) { Enter(St.Guard); _cd = 0.4f; } return; }
        bool down = Host.PlayerDown;
        if (down && State is not (St.Idle or St.Return)) Enter(St.Return);

        switch (State)
        {
            case St.Idle:
                Face = TurnToward(Face, Mathf.Pi / 2, turn);
                if (!down && (Bridge && d < 190 && p.GlobalPosition.Y < 21 * ValeMap.TS || Type == Kind.Disciple && !Bridge && d < AggroR || Type == Kind.Senior && d < 230 && p.GlobalPosition.Y < 13 * ValeMap.TS))
                {
                    Enter(St.Guard); _cd = 0.9f;
                    if (!Said)
                    {
                        Said = true;
                        Host.Say(this, Bridge ? "Sua lâmina não passa pela minha guarda." : Type == Kind.Senior ? "Você passou pelo meu discípulo. Mostre de onde vem esse passo." : Line);
                        Host.OnAggro(this);
                    }
                }
                break;
            case St.Return:
                WalkTo(Home, 80); Face = (Home - GlobalPosition).Angle();
                if (GlobalPosition.DistanceTo(Home) < 6) { Enter(St.Idle); Hp = Max; AttackStance = false; }
                break;
            case St.Guard:
            {
                Track();
                if (leashOut) { Enter(St.Return); if (Bridge && !down) Host.OnFlee(this); break; }
                if (Type == Kind.Senior) { _stanceT += dt; if (_stanceT >= c.SenClosedTime) { AttackStance = true; Enter(St.Advance); Combo = 0; break; } }
                float keep = Type == Kind.Senior ? 56 : 52;
                if (d > keep + 16) WalkTo(p.GlobalPosition, Type == Kind.Senior ? 55 : 45);
                else if (d < keep - 14) { float a = want + Mathf.Pi; Move(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 30 * dt); }
                _cd -= dt;
                float reach = Type == Kind.Senior ? 70 : c.DisReach;
                if (Parries >= 2) { Parries = 0; StartWindup((Type == Kind.Senior ? c.SenWindup : c.DisWindup) * 0.6f); }
                else if (_cd <= 0 && d < reach + 6) StartWindup(Type == Kind.Senior ? c.SenWindup : c.DisWindup);
                break;
            }
            case St.Advance:
                Track(); WalkTo(p.GlobalPosition, c.SenSpeed);
                if (leashOut) { Enter(St.Return); AttackStance = false; break; }
                if (d < 72) StartWindup(c.SenWindup);
                else if (T > 2.5f) { AttackStance = false; Enter(St.Guard); _stanceT = 0; }
                break;
            case St.Windup:
                Track(0.8f);
                if (T >= _wt) { Enter(St.Strike); _struck = false; _lunge = Type == Kind.Senior && AttackStance ? new[] { 40f, 40f, 62f }[Combo] : 46; }
                break;
            case St.Strike:
            {
                float dur = c.StrikeTime;
                Move(new Vector2(Mathf.Cos(Face), Mathf.Sin(Face)) * _lunge / dur * dt);
                if (!_struck && T >= c.StrikeLiveAt)
                {
                    _struck = true;
                    var (reach, half) = StrikeArea();
                    Host.EnemySlash(this, BigStrike);
                    Vector2 toP = p.GlobalPosition - GlobalPosition;
                    if (toP.Length() <= reach && Mathf.Abs(Mathf.Wrap(toP.Angle() - Face, -Mathf.Pi, Mathf.Pi)) <= half) Host.HitPlayer(this);
                }
                if (T >= dur)
                {
                    if (Type == Kind.Senior && AttackStance && Combo < 2) { Combo++; StartWindup(c.SenWindup * (Combo == 1 ? 0.7f : 1.3f)); }
                    else { Enter(St.Recover); _rt = Type == Kind.Senior && AttackStance ? c.SenComboRecover : c.DisRecover; _rtTotal = _rt; }
                }
                break;
            }
            case St.Recover:
                if (!AttackStance) Track();
                if (T >= _rt)
                {
                    Enter(St.Guard);
                    _cd = (Type == Kind.Senior ? 0.6f : c.DisCooldown) * (0.8f + Rnd() * 0.5f);
                    if (Type == Kind.Senior && AttackStance) { AttackStance = false; _stanceT = 0; }
                }
                break;
            case St.Flinch:
                if (T >= 0.32f) { Enter(St.Guard); _cd = Mathf.Min(_cd, 0.35f); }
                break;
        }
    }

    /// <summary>
    /// A blade or a qi shot reached it. Returns "parried", "flank", "hit", "killed" or "statue" (a flank
    /// strike on the statue, which only answers).
    /// </summary>
    public string Strike(Vector2 from, int dmg, bool lamina, float stagger, bool heavy)
    {
        if (!lamina && WouldParry(from))
        {
            Parries++; GuardShow = 0.7f;
            return "parried";
        }
        bool flank = !lamina && Guarding;
        if (Type == Kind.Statue) return flank ? "statue" : "statue";
        Hp -= dmg; Flash = 0.12f;
        Vector2 back = (GlobalPosition - from).Normalized(); Knock = back * 160;
        if (lamina) { Stagger = stagger; Enter(St.Stagger); }
        else if (!HyperArmor) Enter(St.Flinch);
        if (Hp <= 0) { Hp = 0; Enter(St.Dead); return "killed"; }
        return flank ? "flank" : "hit";
    }

    // the assist mode: the same numbers as the tests, three phases told apart by shape
    private void DrawAssist()
    {
        Vector2 o = Vector2.Zero;
        if (Guarding)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(_time * 6);
            Assist.Sector(this, o, Face, GuardHalfRad, 34, new Color(Assist.GuardC, 0.10f));
            Assist.SectorEdge(this, o, Face, GuardHalfRad, 34, new Color(Assist.GuardC, 0.85f), 2);
            // the flank: the rest of the circle, dashed jade; brighter (and thicker) while it is fooled
            float open = Fooled > 0 ? 0.95f : 0.45f + 0.15f * pulse;
            Assist.Arc(this, o, Face + Mathf.Pi, Mathf.Pi - GuardHalfRad, 34, new Color(Assist.OpenC, open), Fooled > 0 ? 2 : 1, 4);
        }
        else if (Alive) Assist.Arc(this, o, 0, Mathf.Pi, 34, new Color(Assist.OpenC, 0.9f), 2, 4); // no guard at all: open everywhere
        var (reach, half) = StrikeArea();
        if (State == St.Windup)
        {
            // aviso: dashed outline of the whole area, a fill growing from the body
            Assist.SectorEdge(this, o, Face, half, reach, new Color(Assist.ThreatC, 0.9f), 1, 4);
            Assist.Sector(this, o, Face, half, Mathf.Max(1, reach * WarnProgress), new Color(Assist.ThreatC, 0.22f));
        }
        else if (State == St.Strike)
        {
            // golpe ativo: solid, thick edge
            Assist.Sector(this, o, Face, half, reach, new Color(Assist.ThreatC, 0.45f));
            Assist.SectorEdge(this, o, Face, half, reach, Assist.ThreatC, 3);
        }
        else if (State == St.Recover)
        {
            // recuperação: a thin hollow outline that shrinks — the opening
            Assist.SectorEdge(this, o, Face, half, reach * (0.35f + 0.65f * RecoverLeft), new Color(Assist.ThreatC, 0.55f), 1, 2);
        }
    }

    /// <summary>Kills it outright (a saved defeat).</summary>
    public void SetDead() { Hp = 0; Enter(St.Dead); }

    /// <inheritdoc />
    public override void _Draw()
    {
        var look = Type switch { Kind.Senior => FighterArt.Senior, Kind.Statue => FighterArt.Statue, _ => FighterArt.Disciple };
        if (Gone) { DrawRect(new Rect2(-18, -3, 36, 6), new Color("#1d1712", 0.7f)); return; } // the scorched ground
        if (!Alive) { FighterArt.DrawDead(this, _paint, look, Type == Kind.Senior, false, _time); return; }
        if (Scripted is { } sp)
        {
            // the aura: rings of red-ochre qi that grow with the power
            if (AuraPower > 0)
                for (int ring = 0; ring < 3; ring++)
                {
                    float rad = 14 + ring * 7 + AuraPower * 10 + Mathf.Sin(_time * 8 + ring) * 2;
                    var col = new Color(ring == 0 ? "#e8834c" : "#c8502a", (0.25f + 0.35f * AuraPower) * (1 - ring * 0.25f));
                    for (int i = 0; i < 20; i++) { float a = i * Mathf.Tau / 20 + _time * (1 + ring); DrawRect(new Rect2(Mathf.Cos(a) * rad, -22 + Mathf.Sin(a) * rad * 0.7f, 2, 2), col); }
                }
            DrawRect(new Rect2(-11, -2, 22, 4), new Color(8 / 255f, 10 / 255f, 8 / 255f, 0.38f));
            FighterArt.Draw(this, _paint, look, Face, sp, _time, _walk, false, AuraPower > 0.05f, Flash > 0, Type == Kind.Senior);
            return;
        }
        // the telegraph wedge on the ground: the strike's real area (StrikeArea), filling as the strike nears
        if (State == St.Windup)
        {
            float k = WarnProgress;
            var (reach, half) = StrikeArea();
            var col = new Color("#c8502a", 0.18f + 0.25f * k);
            for (int i = 0; i < 11; i++)
            {
                float u = i / 10f, ang = Face - half + 2 * half * u;
                for (float r = 14; r < reach * k; r += 6) DrawRect(new Rect2(Mathf.Cos(ang) * r - 1, Mathf.Sin(ang) * r - 1, 2, 2), col);
            }
        }
        if (Assist.Areas && Type != Kind.Statue) DrawAssist();
        // the guard, as in the prototype: an arc in front while it is the locked target (or just parried);
        // once the side step is known, the rest of the body shows as an open dashed arc while it is fooled
        bool locked = Host.Lock == this;
        if (Guarding && (locked || GuardShow > 0) && !(Assist.Areas && Type != Kind.Statue))
        {
            float lit = Mathf.Max(0, GuardShow);
            Assist.Arc(this, new Vector2(0, -2), Face, GuardHalfRad, 30, new Color("#ece5d0", 0.22f + lit * 0.8f), lit > 0 ? 2 : 1);
            if (locked && (Fooled > 0 || Type == Kind.Statue) && Host.Story.LearnedSide)
                Assist.Arc(this, new Vector2(0, -2), Face + Mathf.Pi, Mathf.Pi - GuardHalfRad - 0.08f, 30, Type == Kind.Statue ? new Color("#6fd8d2", 0.35f) : Assist.OpenC, 1, 3);
        }
        DrawRect(new Rect2(-11, -2, 22, 4), new Color(8 / 255f, 10 / 255f, 8 / 255f, 0.38f));
        var pose = State switch
        {
            St.Windup => FighterArt.Pose.Windup,
            St.Strike => FighterArt.Pose.Strike,
            St.Stagger or St.Flinch => FighterArt.Pose.Stagger,
            St.Advance => FighterArt.Pose.Walk,
            St.Return => FighterArt.Pose.Walk,
            St.Guard or St.Recover => Type == Kind.Senior && AttackStance ? FighterArt.Pose.Attack : FighterArt.Pose.Guard,
            _ => Type == Kind.Statue ? FighterArt.Pose.Guard : FighterArt.Pose.Idle,
        };
        FighterArt.Draw(this, _paint, look, Face, pose, _time, _walk, State == St.Windup, Type == Kind.Senior && AttackStance, Flash > 0, Type == Kind.Senior);
        // the guard flash after a parry: a short arc in front
        if (GuardShow > 0)
        {
            var col = new Color("#fff3c4", GuardShow / 0.7f);
            for (int i = -4; i <= 4; i++) { float ang = Face + i * 0.18f; DrawRect(new Rect2(Mathf.Cos(ang) * 16 - 1, -16 + Mathf.Sin(ang) * 10, 2, 2), col); }
        }
        if (Stagger > 0) for (int i = 0; i < 3; i++) { float ang = _time * 6 + i * Mathf.Tau / 3; DrawRect(new Rect2(Mathf.Cos(ang) * 7, -48 + Mathf.Sin(ang) * 2, 2, 2), new Color("#d4fbf6")); }
    }
}
