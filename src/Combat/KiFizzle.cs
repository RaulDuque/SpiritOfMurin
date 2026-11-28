using Godot;

namespace SpiritOfMurin.Combat;

/// <summary>
/// Where a Ki shot gives out at the end of its range (T07): the crescent crumbles into a few fading motes and leaves
/// a short tick on the ground for a moment, so the reach of the full shot (prologue) and the weakened one (Planície,
/// level 3) can be told apart in a fight. <b>Where to change it:</b> the colours, the duration and the motes below.
/// </summary>
public partial class KiFizzle : Node2D
{
    /// <summary>How long it shows (s).</summary>
    public const float Life = 1.0f;
    /// <summary>Colours: the motes, the bright core, the ground mark.</summary>
    public static readonly Color MoteC = new("#a6f2e2"), CoreC = new("#d4fbf6"), TickC = new("#3fae9e");
    /// <summary>The shot's direction (the crescent's remnant faces it; the ground mark lies across it).</summary>
    public Vector2 Dir { get; set; } = Vector2.Right;
    private float _t;

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _t += (float)delta;
        if (_t >= Life) QueueFree(); else QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        float k = _t / Life, a = 1 - k;
        Vector2 n = new(-Dir.Y, Dir.X);
        // the crescent breaking: its pieces drift apart and fall a little
        for (int i = -6; i <= 6; i++)
        {
            float u = i / 6f;
            Vector2 p = n * i * (1 + k * 1.6f) + Dir * ((1 - u * u) * 4 + k * 3) + new Vector2(0, k * k * 10);
            DrawRect(new Rect2(p.Round() - Vector2.One, new Vector2(2, 2)), new Color(Mathf.Abs(u) < 0.4f ? CoreC : MoteC, a));
        }
        // motes rising
        for (int i = 0; i < 10; i++)
        {
            float ang = i * 0.63f + 0.2f;
            Vector2 p = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * 0.6f) * (3 + k * 14) + new Vector2(0, -k * 10);
            DrawRect(new Rect2(p.Round(), Vector2.One * (i % 3 == 0 ? 2 : 1)), new Color(MoteC, a * 0.9f));
        }
        // the mark on the ground where it gave out
        Vector2 g = new(0, 12);
        DrawLine(g - n * 7, g + n * 7, new Color(TickC, a), 2);
        DrawArc(g, 4 + k * 6, 0, Mathf.Tau, 14, new Color(TickC, a * 0.6f), 1);
    }
}
