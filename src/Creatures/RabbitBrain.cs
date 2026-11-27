using Godot;

namespace SpiritOfMurin.Creatures;

/// <summary>The rabbit's states.</summary>
public enum RabbitState { Graze, Alert, Dash1, Pause, Dash2, Hidden, Dead }

/// <summary>
/// The Coelho de Campo's behaviour, without nodes (unit-testable). It hops around its burrow; when it
/// hears the player (the hearing radius depends on how the player moves) it raises its ears, freezes,
/// then runs to the burrow in two dashes and hides there. After a while it peeks out again, so a
/// player who watched where it went can wait by the burrow, quietly.
/// </summary>
public sealed class RabbitBrain
{
    private readonly RabbitTuning _t;
    private readonly int _variation;
    private float _hopTimer;
    private int _hopN;
    private Vector2 _hopDir, _mid;

    /// <summary>Current state.</summary>
    public RabbitState State { get; private set; } = RabbitState.Graze;
    /// <summary>Seconds in the current state.</summary>
    public float StateTime { get; private set; }
    /// <summary>The burrow, world pixels.</summary>
    public Vector2 Burrow { get; }
    /// <summary>Where the rabbit faces.</summary>
    public Vector2 Facing { get; private set; } = Vector2.Right;
    /// <summary>Remaining health.</summary>
    public float Health { get; private set; }
    /// <summary>True when it can be seen and hit (not inside the burrow, not dead).</summary>
    public bool Visible => State is not (RabbitState.Hidden or RabbitState.Dead);
    /// <summary>True with the ears up (alert or running).</summary>
    public bool EarsUp => State is RabbitState.Alert or RabbitState.Dash1 or RabbitState.Pause or RabbitState.Dash2;
    /// <summary>Times it has fled to the burrow (for checks).</summary>
    public int Flights { get; private set; }

    /// <summary>Creates the brain grazing at its burrow.</summary>
    public RabbitBrain(RabbitTuning tuning, Vector2 burrow, int variation)
    {
        _t = tuning; Burrow = burrow; _variation = variation; Health = tuning.Health;
    }

    private void Enter(RabbitState s) { State = s; StateTime = 0; }

    /// <summary>Advances the behaviour. <paramref name="hearRadius"/> is how far the player's movement can be heard right now.</summary>
    public Vector2 Update(float dt, Vector2 pos, Vector2 player, float hearRadius)
    {
        StateTime += dt;
        float dPlayer = pos.DistanceTo(player);
        switch (State)
        {
            case RabbitState.Graze:
                if (dPlayer < hearRadius) { Enter(RabbitState.Alert); Facing = (player - pos).Normalized(); return Vector2.Zero; }
                return Graze(dt, pos);
            case RabbitState.Alert:
                if (StateTime < _t.AlertTime) return Vector2.Zero;
                Flights++;
                _mid = pos.Lerp(Burrow, 0.5f);
                Enter(pos.DistanceTo(Burrow) < 24 ? RabbitState.Dash2 : RabbitState.Dash1);
                return Vector2.Zero;
            case RabbitState.Dash1:
                if (pos.DistanceTo(_mid) < 4 || StateTime > 1.5f) { Enter(RabbitState.Pause); return Vector2.Zero; }
                return Run(pos, _mid);
            case RabbitState.Pause:
                if (StateTime >= _t.DashPause) Enter(RabbitState.Dash2);
                return Vector2.Zero;
            case RabbitState.Dash2:
                if (pos.DistanceTo(Burrow) < 4 || StateTime > 2.5f) { Enter(RabbitState.Hidden); return Vector2.Zero; }
                return Run(pos, Burrow);
            case RabbitState.Hidden:
                if (StateTime >= _t.HideTime) { Enter(RabbitState.Graze); _hopTimer = 1.2f; }
                return Vector2.Zero;
        }
        return Vector2.Zero;
    }

    private Vector2 Run(Vector2 pos, Vector2 to)
    {
        Vector2 d = to - pos;
        if (d.LengthSquared() < 0.01f) return Vector2.Zero;
        Facing = d.Normalized();
        // never overshoot the target in one frame (the node moves at most speed * dt)
        return Facing * _t.DashSpeed;
    }

    // short hops in seeded directions, staying near the burrow
    private Vector2 Graze(float dt, Vector2 pos)
    {
        _hopTimer -= dt;
        if (_hopTimer <= 0)
        {
            _hopN++;
            uint h = Plains.PlainsGenerator.Hash(_variation, _hopN, 1, 55);
            Vector2 back = Burrow - pos;
            if (back.Length() > _t.GrazeRadius) _hopDir = back.Normalized();
            else { float a = (h % 360) * Mathf.Pi / 180f; _hopDir = new Vector2(Mathf.Cos(a), Mathf.Sin(a)); }
            _hopTimer = 1.1f + (h >> 10) % 10 / 10f;
        }
        bool hopping = _hopTimer > 0.8f; // the first 0.3 s of each period is the hop
        if (!hopping) return Vector2.Zero;
        Facing = _hopDir;
        return _hopDir * _t.GrazeSpeed * 3f;
    }

    /// <summary>A blade reached the rabbit.</summary>
    public HitResult TakeHit(float damage)
    {
        if (!Visible) return HitResult.None;
        Health -= damage;
        if (Health <= 0) { Enter(RabbitState.Dead); return HitResult.Killed; }
        return HitResult.Damaged;
    }
}
