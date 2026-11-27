using Godot;

namespace SpiritOfMurin.Plains;

/// <summary>
/// One print of a trail leading to a burrow (paw prints) or a boar's spot (hoof prints). E examines it:
/// the animal, the direction and, with Rastreamento, the distance and the animal's level. With
/// <see cref="Glow"/> the print shines (the next print after the one examined, or every print at grade 3).
/// </summary>
public partial class TrackPrint : Node2D
{
    /// <summary>The print's stable id.</summary>
    public string FeatureId { get; set; } = "";
    /// <summary>The trail it belongs to (world state key: examined once).</summary>
    public string TrailId { get; set; } = "";
    /// <summary>The animal the trail leads to.</summary>
    public string CreatureId { get; set; } = "";
    /// <summary>Number of the print along the trail (0 = the start).</summary>
    public int Index { get; set; }
    /// <summary>Direction the trail leads.</summary>
    public Vector2 Dir { get; set; } = Vector2.Right;
    /// <summary>True for hoof prints (boar), false for paw prints (rabbit).</summary>
    public bool Hoof => CreatureId.StartsWith("boar");
    /// <summary>Seconds of shine left (0 = none); &lt;0 shines for good.</summary>
    public float Glow { get; set; }

    private float _time;
    private static readonly Color Mark = new("#2a2219"), MarkLight = new("#5e4a34"), Shine = new("#d4fbf6");

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _time += (float)delta;
        if (Glow > 0) Glow = Mathf.Max(0, Glow - (float)delta);
        QueueRedraw();
    }

    private void R(Vector2 p, float w, float h, Color c) => DrawRect(new Rect2(Mathf.Round(p.X), Mathf.Round(p.Y), w, h), c);

    /// <inheritdoc />
    public override void _Draw()
    {
        // prints point along the trail: two marks side by side, the front one ahead
        Vector2 f = Dir * 3f, s = new Vector2(-Dir.Y, Dir.X) * 2f;
        bool glow = Glow != 0;
        if (glow)
        {
            float a = 0.35f + 0.25f * Mathf.Sin(_time * 6);
            R(new Vector2(-6, -5), 12, 10, new Color(Shine, a * 0.5f));
        }
        Color c = glow ? Shine : Mark;
        if (Hoof)
        {
            R(-s + f - new Vector2(1, 1), 2, 3, c); R(s + f - new Vector2(1, 1), 2, 3, c);
            R(-s - f - new Vector2(1, 1), 2, 2, glow ? Shine : MarkLight); R(s - f - new Vector2(1, 1), 2, 2, glow ? Shine : MarkLight);
        }
        else
        {
            R(-s - new Vector2(1, 1), 2, 2, c); R(s + f - new Vector2(1, 1), 2, 2, c); R(f * 2 - new Vector2(0, 0), 1, 1, glow ? Shine : MarkLight);
        }
    }
}

/// <summary>The campfire at the clearing: E roasts raw meat (the fogueira recipes).</summary>
public partial class Campfire : StaticBody2D
{
    private float _time;
    private static readonly Color Stone = new("#6b6a60"), StoneDark = new("#3b3a34"), Log = new("#5a3e28"), Ember = new("#c9532e"), Flame = new("#e8a13a"), FlameHi = new("#f4dc8a"), Outline = new("#16140f");

    /// <inheritdoc />
    public override void _Ready() => AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(16, 6) }, Position = new Vector2(0, -3) });

    private PointLight2D? _light;

    /// <summary>The fire's warm light, by the night's darkness (0 day … 1 night), flickering.</summary>
    public void SetNight(float darkness, float radius, float energy)
    {
        if (_light == null) { _light = SoftLight.Make(radius, new Color("#ffb060")); _light.Position = new Vector2(0, -10); AddChild(_light); }
        _light.Energy = energy * darkness * (0.9f + 0.1f * Mathf.Sin(_time * 11));
    }

    /// <summary>The light's strength now (checks).</summary>
    public float LightEnergy => _light?.Energy ?? 0;

    /// <inheritdoc />
    public override void _Process(double delta) { _time += (float)delta; QueueRedraw(); }

    private void R(float x, float y, float w, float h, Color c) => DrawRect(new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), c);

    /// <inheritdoc />
    public override void _Draw()
    {
        for (int i = 0; i < 7; i++) { float a = i * Mathf.Tau / 7; R(Mathf.Cos(a) * 9 - 2, Mathf.Sin(a) * 4 - 4, 4, 3, i % 2 == 0 ? Stone : StoneDark); }
        R(-7, -5, 14, 2, Outline); R(-6, -5, 12, 1, Log); R(-2, -7, 4, 4, Log);
        float k = Mathf.Sin(_time * 9) * 1.2f, k2 = Mathf.Sin(_time * 13 + 1) * 1f;
        R(-4, -10 + k, 8, 6, Ember); R(-3, -13 + k2, 6, 7, Flame); R(-1, -15 + k, 3, 6, FlameHi);
        for (int i = 0; i < 2; i++) { float t = Mathf.PosMod(_time * 0.8f + i * 0.5f, 1f); R(-1 + i * 2, -18 - t * 14, 1, 1, new Color(Flame, 1 - t)); }
    }
}

/// <summary>The meditation mat by the clearing: E meditates (writes the journey, fills Ki, sets where Continuar wakes you).</summary>
public partial class MeditationMat : Node2D
{
    private float _time;
    /// <summary>Seconds of the shine after a meditation.</summary>
    public float Shine { get; set; }
    private static readonly Color Straw = new("#a3925f"), StrawDark = new("#7a6b44"), Ki = new("#6fd8d2"), Outline = new("#16140f");

    /// <inheritdoc />
    public override void _Process(double delta) { _time += (float)delta; Shine = Mathf.Max(0, Shine - (float)delta); QueueRedraw(); }

    private void R(float x, float y, float w, float h, Color c) => DrawRect(new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), c);

    /// <inheritdoc />
    public override void _Draw()
    {
        R(-11, -7, 22, 9, Outline); R(-10, -6, 20, 7, Straw);
        for (int i = -8; i < 10; i += 3) R(i, -6, 1, 7, StrawDark);
        // a thread of ki rising beside the mat
        float a = 0.25f + Shine / 1.6f * 0.7f;
        for (int i = 0; i < 6; i++) R(9 + Mathf.Round(Mathf.Sin(_time * 2 + i) * 1.5f), -10 - i * 3, 1, 2, new Color(Ki, a * (1 - i / 7f)));
    }
}
