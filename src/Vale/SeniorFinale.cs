using Godot;
using SpiritOfMurin.Combat;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Vale;

/// <summary>
/// The end of the prologue, after the Senior Swordsman falls for real:
/// <list type="number">
/// <item>Victory: the fight ends and the player sees it (banner, chime, the body on the ground).</item>
/// <item>After a short pause he rises: a new stance, a qi aura that grows until it moves dust, leaves and cloth.</item>
/// <item>He faces the swordsman and speaks; E advances.</item>
/// <item>He draws the energy into his own body and blows himself up; the shock wave throws the swordsman far.</item>
/// <item>Darkness and silence, a faint heartbeat; then the campaign wakes the player in the Planície.</item>
/// </list>
/// The victory is written at once (phase "senior_won"): closing the game anywhere in the ending resumes at
/// the awakening. Once seen, the ending can be skipped with Esc or Enter (preference "finale_seen").
/// </summary>
public sealed class SeniorFinale
{
    /// <summary>The steps of the ending.</summary>
    public enum Stage { Victory, Revive, Speech, Charge, Explode, Dark, Done }

    /// <summary>The line the Senior says.</summary>
    public const string Line = "Você acha que pode me derrotar? Pois vou garantir que não.";

    private readonly ValeGame _g;
    private readonly ValeFighter _sen;
    private readonly Prefs _prefs;
    private float _t, _fly, _spin = 1;
    private bool _landed;
    private Vector2 _flyFrom, _flyTo;
    private bool _saved, _heart1, _heart2;
    private uint _rng = 7;

    /// <summary>Current stage.</summary>
    public Stage Current { get; private set; } = Stage.Victory;
    /// <summary>Seconds in the stage.</summary>
    public float T => _t;
    /// <summary>True when the ending may be skipped (it was seen before).</summary>
    public bool Skippable { get; }
    /// <summary>True when the victory was written to the journey.</summary>
    public bool Saved => _saved;
    /// <summary>Stages entered, in order (for checks).</summary>
    public System.Collections.Generic.List<Stage> Seen { get; } = new();
    /// <summary>True once the thrown swordsman left the picture.</summary>
    public bool LeftScreen { get; private set; }

    /// <summary>Distance the swordsman was thrown, px.</summary>
    public float Thrown { get; private set; }

    /// <summary>Starts the ending (the Senior just fell).</summary>
    public SeniorFinale(ValeGame g, ValeFighter senior)
    {
        _g = g; _sen = senior; _prefs = Prefs.Load();
        Skippable = _prefs.FinaleSeen;
        g.Player.InputLocked = true;
        g.Hud.Banner("Vitória", 2.4f);
        g.Sfx.Play("win");
        g.Sfx.Ambient(0.15f, 0);
        Seen.Add(Stage.Victory);
        _saved = g.WritePhase("senior_won");
        if (Skippable) g.Hud.Toast("Esc ou Enter: pular o fim do prólogo.");
    }

    private float Rnd() { _rng ^= _rng << 13; _rng ^= _rng >> 17; _rng ^= _rng << 5; return (_rng & 0xFFFF) / 65536f; }

    private void Enter(Stage s) { Current = s; _t = 0; Seen.Add(s); }

    /// <summary>Input during the ending: E advances the speech; Esc / Enter skip once it was seen.</summary>
    public void Input(InputEvent e)
    {
        if (Current == Stage.Speech && (e.IsActionPressed("use") || e.IsActionPressed("ui_accept")) && _t > 0.4f) { StartCharge(); return; }
        if (Skippable && (e.IsActionPressed("ui_cancel") || e.IsActionPressed("ui_accept") || e.IsActionPressed("menu")) && Current < Stage.Dark)
        {
            _g.Hud.Speech(null, Vector2.Zero);
            Enter(Stage.Dark); _g.Hud.Overlay("", 99, Colors.Black, 0.1f); _g.Sfx.FadeMaster(0, 0.4f);
            _t = 2.2f; // skip to the end of the darkness
        }
    }

    private void StartCharge()
    {
        Enter(Stage.Charge); _g.Hud.Speech(null, Vector2.Zero); _g.Sfx.Play("charge");
        _sen.Scripted = FighterArt.Pose.Windup;
    }

    /// <summary>Advances the ending.</summary>
    public void Update(float dt)
    {
        _t += dt;
        Vector2 at = _sen.GlobalPosition, p = _g.Player.GlobalPosition;
        switch (Current)
        {
            case Stage.Victory:
                if (_t >= 2.0f)
                {
                    Enter(Stage.Revive);
                    _sen.ScriptedRevive(FighterArt.Pose.Stagger);
                    _g.Sfx.Play("rumble");
                }
                break;
            case Stage.Revive:
            {
                float k = Mathf.Clamp(_t / 2.6f, 0, 1);
                _sen.AuraPower = k * 0.7f;
                if (_t > 0.8f) _sen.Scripted = FighterArt.Pose.Attack; // the stance changes
                _sen.Face = (p - at).Angle();
                // dust swirls at his feet, leaves are blown outward, cloth strips flap
                if (Rnd() < 0.6f + k) { float a = Rnd() * Mathf.Tau; _g.Fx.Particle(at + new Vector2(Mathf.Cos(a) * 18, Mathf.Sin(a) * 8), new Vector2(-Mathf.Sin(a), Mathf.Cos(a) * 0.5f) * 60 * k + new Vector2(0, -20), new Color("#8f7a58"), 0.8f, 2); }
                if (Rnd() < 0.3f * k) { float a = Rnd() * Mathf.Tau; _g.Fx.Particle(at + new Vector2(0, -16), new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (90 + 60 * k), Rnd() < 0.5f ? new Color("#8fb35a") : new Color("#b5a54e"), 1.2f, 2); }
                if (Rnd() < 0.25f * k) _g.Fx.Particle(at + new Vector2((Rnd() - 0.5f) * 14, -20 - Rnd() * 20), new Vector2((Rnd() - 0.5f) * 40, -30), Rnd() < 0.5f ? new Color("#6e3027") : new Color("#c79e4e"), 0.6f, 1);
                _g.FinaleShake(0.3f + k * 0.5f);
                if (_t >= 2.6f) { Enter(Stage.Speech); _sen.Scripted = FighterArt.Pose.Guard; _g.Say(_sen, Line); }
                break;
            }
            case Stage.Speech:
                _sen.Face = (p - at).Angle(); _g.Player.Face(at - p);
                _g.Hud.Speech(Line, _g.GetViewport().GetCanvasTransform() * at, 1);
                if (_t >= 6.0f) StartCharge(); // the line also advances by itself
                break;
            case Stage.Charge:
            {
                float k = Mathf.Clamp(_t / 2.0f, 0, 1);
                _sen.AuraPower = 0.7f + k * 0.6f; _sen.Flash = Mathf.FloorToInt(_t * 12) % 2 == 1 && k > 0.6f ? 0.05f : 0;
                // the energy is drawn into his body
                for (int i = 0; i < 3; i++) { float a = Rnd() * Mathf.Tau, r = 60 + Rnd() * 40; Vector2 from = at + new Vector2(Mathf.Cos(a) * r, -16 + Mathf.Sin(a) * r * 0.7f); _g.Fx.Particle(from, (at + new Vector2(0, -16) - from) * 2.2f, new Color("#e8834c"), 0.45f, 2); }
                _g.FinaleShake(0.6f + k * 1.4f);
                if (_t >= 2.0f)
                {
                    Enter(Stage.Explode);
                    _g.Sfx.Play("explode");
                    _g.Hud.Overlay("", 0.08f, new Color(1, 0.96f, 0.86f), 0.5f); // the flash
                    _g.Fx.AddRing(at + new Vector2(0, -10), new Color("#fff3c4"), 0.9f, 140);
                    _g.Fx.AddRing(at + new Vector2(0, -10), new Color("#e8834c"), 1.1f, 90);
                    _g.Fx.Burst(at + new Vector2(0, -14), 60, new Color("#e8834c"), 260, 0.9f, 2);
                    _g.Fx.Burst(at + new Vector2(0, -14), 30, new Color("#fff3c4"), 200, 0.6f, 2);
                    _sen.Gone = true; _sen.Scripted = null; _sen.AuraPower = 0;
                    Vector2 away = (p - at).LengthSquared() > 1 ? (p - at).Normalized() : Vector2.Down;
                    _flyFrom = p; _flyTo = p + away * 560; _fly = 0; _spin = away.X < 0 ? -1 : 1;
                    // the camera stays where the blast was: the swordsman is thrown out of the picture
                    var cam = _g.Player.GetNode<Camera2D>("Camera");
                    Vector2 camAt = cam.GetScreenCenterPosition();
                    cam.TopLevel = true; cam.PositionSmoothingEnabled = false; cam.GlobalPosition = camAt;
                    _g.FinaleShake(4);
                }
                break;
            }
            case Stage.Explode:
            {
                // the shock wave throws the swordsman far away: spinning, up and out of the picture, over the mountains
                var pl = _g.Player;
                if (!_landed)
                {
                    _fly = Mathf.Min(1, _fly + dt / 1.4f);
                    float e = 1 - (1 - _fly) * (1 - _fly);
                    pl.GlobalPosition = _flyFrom.Lerp(_flyTo, e); // nothing stops him: he is in the air
                    Thrown = pl.GlobalPosition.DistanceTo(_flyFrom);
                    pl.Airborne = e * e * 260; // he keeps rising
                    pl.TumbleAngle = _spin * _fly * Mathf.Tau * 2.5f;
                    if (Rnd() < 0.6f) _g.Fx.Particle(pl.GlobalPosition + new Vector2(0, -20 - pl.Airborne), new Vector2((Rnd() - 0.5f) * 30, 10), new Color("#8f7a58"), 0.5f, 2);
                    var view = _g.GetViewport().GetVisibleRect();
                    Vector2 onScreen = _g.GetViewport().GetCanvasTransform() * (pl.GlobalPosition + new Vector2(0, -20 - pl.Airborne));
                    if (!view.Grow(40).HasPoint(onScreen)) LeftScreen = true;
                    if (_fly >= 1) { _landed = true; pl.Visible = false; }
                }
                if (_t >= 2.0f)
                {
                    Enter(Stage.Dark);
                    _g.Hud.Overlay("", 99, Colors.Black, 0.1f);
                    _g.Sfx.FadeMaster(0, 0.8f); // silence
                    _g.Sfx.Ambient(0, 0);
                }
                break;
            }
            case Stage.Dark:
                if (_t >= 1.6f && !_heart1) { _heart1 = true; _g.Sfx.FadeMaster(0.5f, 0.2f); _g.Sfx.Play("heart"); }
                if (_t >= 2.6f && !_heart2) { _heart2 = true; _g.Sfx.Play("heart"); }
                if (_t >= 3.4f)
                {
                    Enter(Stage.Done);
                    if (!_prefs.FinaleSeen) { _prefs.FinaleSeen = true; _prefs.Save(); }
                    _g.EndPrologue();
                }
                break;
        }
    }
}
