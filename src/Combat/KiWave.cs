using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Combat;

/// <summary>
/// A ki shot in flight (the Disparo de Ki, or the Silent Blade Step's wave). It travels straight from
/// the palm until its range runs out or the scene stops it; the scene tests what it touches every
/// frame. Drawn as the prototype's crescent of ki; the lamina version is brighter and leaves sparks.
/// </summary>
public partial class KiWave : Node2D
{
    /// <summary>The shot it carries.</summary>
    public Player.KiShot Shot { get; set; }
    /// <summary>Pixels travelled.</summary>
    public float Went { get; private set; }
    /// <summary>Ids already hit (one hit per target).</summary>
    public HashSet<string> HitSet { get; } = new();
    /// <summary>True when it should be removed.</summary>
    public bool Done => Went >= Shot.Range;
    /// <summary>Stops the wave where it is (it hit a wall or a guard).</summary>
    public void Stop() => Went = float.MaxValue;
    /// <summary>The point that touches targets (the wave flies at chest height; this is its foot on the ground).</summary>
    public Vector2 Foot => GlobalPosition + new Vector2(0, 12);

    private readonly List<(Vector2 P, float Life)> _sparks = new();
    private static readonly Color Ki = new("#3fae9e"), KiHi = new("#a6f2e2"), Learn = new("#d4fbf6");

    /// <summary>Moves the wave one step.</summary>
    public void Advance(float dt)
    {
        Vector2 v = Shot.Dir * Shot.Speed * dt;
        GlobalPosition += v; Went += v.Length();
        if (Shot.Lamina) _sparks.Add((GlobalPosition, 0.3f));
        for (int i = _sparks.Count - 1; i >= 0; i--) { var s = _sparks[i]; s.Life -= dt; if (s.Life <= 0) _sparks.RemoveAt(i); else _sparks[i] = s; }
        QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        foreach (var (p, life) in _sparks) DrawRect(new Rect2((p - GlobalPosition).Round(), Vector2.One), new Color(KiHi, life / 0.3f));
        float ang = Shot.Dir.Angle(), k = 1 - Mathf.Clamp(Went / Mathf.Max(1, Shot.Range), 0, 1) * 0.6f;
        int half = Shot.Lamina ? 9 : 7;
        for (int i = -half; i <= half; i++)
        {
            float u = i / (float)half, a = ang + Mathf.Pi / 2;
            float bend = (1 - u * u) * (Shot.Lamina ? 6 : 4);
            Vector2 p = new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.7f) * i + Shot.Dir * bend;
            DrawRect(new Rect2(p.Round() - Vector2.One, new Vector2(3, 3)), new Color(Ki, k));
            DrawRect(new Rect2(p.Round(), new Vector2(2, 2)), new Color(Mathf.Abs(u) < 0.5f ? (Shot.Lamina ? Learn : KiHi) : Ki, k));
        }
    }
}
