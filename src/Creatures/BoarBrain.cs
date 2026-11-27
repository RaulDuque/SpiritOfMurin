using Godot;

namespace SpiritOfMurin.Creatures;

/// <summary>The boar's states.</summary>
public enum BoarState { Rooting, Warn, Charge, Recover, Return, Dead }

/// <summary>What a hit did to a creature.</summary>
public enum HitResult { None, Damaged, Deflected, Killed }

/// <summary>
/// The Javali da Mata's behaviour, without nodes (unit-testable). It roots around its home in the mud;
/// when the player comes close it warns (scrapes the hoof, lowers the head; the head follows the player
/// for <see cref="BoarTuning.TrackFraction"/> of the warning, then the direction is fixed), charges in a
/// straight line and must recover. Hits during the warning and the charge glance off the thick hide;
/// the recovery is the opening.
/// </summary>
public sealed class BoarBrain
{
    /// <summary>One update's output.</summary>
    /// <param name="Velocity">Desired velocity, pixels per second.</param>
    /// <param name="HitPlayer">True on the frame the charge reaches the player.</param>
    public readonly record struct Step(Vector2 Velocity, bool HitPlayer);

    private readonly BoarTuning _t;
    private readonly int _variation;
    private bool _hitThisCharge;
    private float _travel, _cooldown, _extraRecover, _pause;
    private int _wanderN;
    private Vector2 _wanderTarget;

    /// <summary>Current state.</summary>
    public BoarState State { get; private set; } = BoarState.Rooting;
    /// <summary>Seconds in the current state.</summary>
    public float StateTime { get; private set; }
    /// <summary>Direction of the (coming) charge; also where the boar faces.</summary>
    public Vector2 Facing { get; private set; } = Vector2.Left;
    /// <summary>Remaining health.</summary>
    public float Health { get; private set; }
    /// <summary>Home spot in the mud, world pixels.</summary>
    public Vector2 Home { get; }
    /// <summary>Pixels already run in the current charge.</summary>
    public float Travel => _travel;
    /// <summary>True once the warning has fixed the charge direction.</summary>
    public bool DirectionLocked => State == BoarState.Warn && StateTime >= _t.WarnTime * _t.TrackFraction;
    /// <summary>True while hits deal damage.</summary>
    public bool Vulnerable => State is BoarState.Rooting or BoarState.Recover or BoarState.Return;
    /// <summary>Number of charges started (for checks).</summary>
    public int Charges { get; private set; }

    /// <summary>Creates the brain at home with full health.</summary>
    public BoarBrain(BoarTuning tuning, Vector2 home, int variation)
    {
        _t = tuning; Home = home; _variation = variation; Health = tuning.Health;
        _wanderTarget = home;
    }

    private void Enter(BoarState s) { State = s; StateTime = 0; }

    /// <summary>Advances the behaviour by <paramref name="dt"/> seconds.</summary>
    public Step Update(float dt, Vector2 pos, Vector2 player, bool playerCanBeHit)
    {
        StateTime += dt;
        Vector2 toPlayer = player - pos;
        bool playerNear = toPlayer.Length() < _t.AggroRadius && player.DistanceTo(Home) < _t.LeashRadius;
        switch (State)
        {
            case BoarState.Dead:
                return new Step(Vector2.Zero, false);
            case BoarState.Rooting:
                _cooldown -= dt;
                if (playerNear && _cooldown <= 0) { StartWarn(toPlayer); return new Step(Vector2.Zero, false); }
                return new Step(Wander(dt, pos), false);
            case BoarState.Warn:
                if (!DirectionLocked && toPlayer.LengthSquared() > 1) Facing = toPlayer.Normalized();
                if (StateTime >= _t.WarnTime) { Enter(BoarState.Charge); _travel = 0; _hitThisCharge = false; Charges++; }
                return new Step(Vector2.Zero, false);
            case BoarState.Charge:
            {
                _travel += _t.ChargeSpeed * dt;
                bool hit = false;
                if (!_hitThisCharge && playerCanBeHit && toPlayer.Length() < _t.ChargeHitRadius) { hit = true; _hitThisCharge = true; }
                if (_travel >= _t.ChargeDistance) { EnterRecover(); return new Step(Vector2.Zero, hit); }
                return new Step(Facing * _t.ChargeSpeed, hit);
            }
            case BoarState.Recover:
                if (StateTime >= _t.RecoverTime + _extraRecover)
                {
                    _cooldown = _t.Cooldown;
                    Enter(pos.DistanceTo(Home) > _t.LeashRadius * 0.6f ? BoarState.Return : BoarState.Rooting);
                }
                return new Step(Vector2.Zero, false);
            case BoarState.Return:
                if (playerNear && pos.DistanceTo(Home) < _t.LeashRadius * 0.6f) { StartWarn(toPlayer); return new Step(Vector2.Zero, false); }
                if (pos.DistanceTo(Home) < 8) { Enter(BoarState.Rooting); return new Step(Vector2.Zero, false); }
                Facing = (Home - pos).Normalized();
                return new Step(Facing * _t.ReturnSpeed, false);
        }
        return new Step(Vector2.Zero, false);
    }

    private void StartWarn(Vector2 toPlayer)
    {
        Enter(BoarState.Warn);
        if (toPlayer.LengthSquared() > 1) Facing = toPlayer.Normalized();
    }

    private void EnterRecover() { Enter(BoarState.Recover); _extraRecover = 0; }

    // slow rooting walk between seeded spots around home, with pauses
    private Vector2 Wander(float dt, Vector2 pos)
    {
        if (_pause > 0) { _pause -= dt; return Vector2.Zero; }
        Vector2 d = _wanderTarget - pos;
        if (d.Length() < 3)
        {
            _wanderN++;
            uint h = Plains.PlainsGenerator.Hash(_variation, _wanderN, 0, 77);
            float ang = (h % 360) * Mathf.Pi / 180f, r = 8 + (h >> 9) % 28;
            _wanderTarget = Home + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r;
            _pause = 0.8f + (h >> 17) % 15 / 10f;
            return Vector2.Zero;
        }
        Facing = d.Normalized();
        return Facing * _t.WanderSpeed;
    }

    /// <summary>The charge ran into something solid: it ends there and the recovery starts.</summary>
    public void NotifyBlocked() { if (State == BoarState.Charge) EnterRecover(); }

    /// <summary>
    /// A guard-breaking hit (the full charged Qi shot, a reinforced strike that rolled the break): the warning
    /// or the charge stops and the boar stays open for <paramref name="seconds"/> more, so the hit that follows lands.
    /// Returns true when there was a defence to break.
    /// </summary>
    public bool Break(float seconds)
    {
        if (State is not (BoarState.Warn or BoarState.Charge)) return false;
        EnterRecover(); _extraRecover = seconds;
        return true;
    }

    /// <summary>A blade reached the boar. Damage only lands when it is open (rooting, recovering, walking home).</summary>
    public HitResult TakeHit(float damage)
    {
        if (State == BoarState.Dead) return HitResult.None;
        if (!Vulnerable) return HitResult.Deflected;
        Health -= damage;
        if (Health <= 0) { Enter(BoarState.Dead); return HitResult.Killed; }
        if (State == BoarState.Recover) _extraRecover += _t.HitFlinch;
        else { Enter(BoarState.Warn); }
        return HitResult.Damaged;
    }
}
