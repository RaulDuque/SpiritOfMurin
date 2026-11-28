using Godot;

namespace SpiritOfMurin.Game;

/// <summary>
/// The direction hint drawn on the ground when a print is examined (T07): a faint cone from the print toward where the
/// animal (or its burrow) is, as wide as the Rastreamento grade allows — wide and vague at first, narrow at Mestre.
/// It fades in <c>track_hint_seconds</c> (<c>plains_fauna.json</c>). <b>Where to change it:</b> colours and length below.
/// </summary>
public partial class TrackHint : Node2D
{
    /// <summary>Length of the cone (px).</summary>
    public const float Length = 70;
    /// <summary>Colours: the cone's fill and its edges.</summary>
    public static readonly Color FillC = new(0.83f, 0.98f, 0.96f, 0.16f), EdgeC = new(0.83f, 0.98f, 0.96f, 0.7f);
    /// <summary>The cone: centre angle and half width (radians).</summary>
    public (float Center, float Half) Sector { get; set; }
    /// <summary>Seconds it shows.</summary>
    public float Life { get; set; } = 2.5f;
    private float _t;

    /// <inheritdoc />
    public override void _Ready() => ZIndex = -4;

    /// <inheritdoc />
    public override void _Process(double delta) { _t += (float)delta; if (_t >= Life) QueueFree(); else QueueRedraw(); }

    /// <inheritdoc />
    public override void _Draw()
    {
        float a = 1 - Mathf.Clamp(_t / Life, 0, 1);
        var (c, h) = Sector;
        var pts = new Vector2[10];
        pts[0] = Vector2.Zero;
        for (int i = 0; i <= 8; i++) { float ang = c - h + 2 * h * i / 8; pts[i + 1] = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * 0.7f) * Length; }
        DrawColoredPolygon(pts, new Color(FillC, FillC.A * a));
        DrawLine(Vector2.Zero, pts[1], new Color(EdgeC, EdgeC.A * a), 1);
        DrawLine(Vector2.Zero, pts[9], new Color(EdgeC, EdgeC.A * a), 1);
    }
}
