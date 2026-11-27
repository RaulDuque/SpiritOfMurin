using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Plains;

/// <summary>The Planície's ambience numbers, from <c>res://assets/data/plains/plains_ambient.json</c> (snake_case).</summary>
public sealed class AmbientSettings
{
    /// <summary>Default path.</summary>
    public const string DefaultPath = "res://assets/data/plains/plains_ambient.json";
    /// <summary>A colour at an hour.</summary>
    public sealed class Key { /** <summary>Hour (0-24).</summary> */ public float Hour { get; set; } /** <summary>Colour (#rrggbb).</summary> */ public string Color { get; set; } = "#ffffff"; }
    /// <summary>The night lights.</summary>
    public sealed class Lights { /** <summary>px</summary> */ public float CampfireRadius { get; set; } = 150; /** <summary>energy</summary> */ public float CampfireEnergy { get; set; } = 1.2f; /** <summary>px</summary> */ public float PlayerRadius { get; set; } = 90; /** <summary>energy</summary> */ public float PlayerEnergy { get; set; } = 0.5f; }
    /// <summary>The wind.</summary>
    public sealed class WindDef { /** <summary>base strength</summary> */ public float Base { get; set; } = 0.25f; /** <summary>gust strength</summary> */ public float Gust { get; set; } = 1; /** <summary>s</summary> */ public float GustPeriod { get; set; } = 7; /** <summary>degrees</summary> */ public float Direction { get; set; } = 15; /** <summary>px</summary> */ public float Wavelength { get; set; } = 260; }
    /// <summary>The leaves.</summary>
    public sealed class LeavesDef { /** <summary>per second at the peak</summary> */ public float PerSecond { get; set; } = 9; /** <summary>s</summary> */ public float Life { get; set; } = 3.2f; }
    /// <summary>The birds.</summary>
    public sealed class BirdsDef { /** <summary>s</summary> */ public float Interval { get; set; } = 22; /** <summary>count</summary> */ public int Min { get; set; } = 3; /** <summary>count</summary> */ public int Max { get; set; } = 6; /** <summary>px/s</summary> */ public float Speed { get; set; } = 70; /** <summary>px</summary> */ public float Height { get; set; } = 70; }
    /// <summary>The tall grass.</summary>
    public sealed class GrassDef { /** <summary>tufts per grass tile</summary> */ public float PerGrassTile { get; set; } = 0.5f; /** <summary>tufts per bush tile</summary> */ public float PerBushTile { get; set; } = 2.5f; /** <summary>px</summary> */ public float PartRadius { get; set; } = 18; }

    /// <summary>Real seconds of a whole day.</summary>
    public float DaySeconds { get; set; } = 480;
    /// <summary>Hour a new journey (and the awakening) starts at.</summary>
    public float StartHour { get; set; } = 8;
    /// <summary>The tint through the day.</summary>
    public List<Key> Palette { get; set; } = new();
    /// <summary>The night lights.</summary>
    public Lights NightLight { get; set; } = new();
    /// <summary>The wind.</summary>
    public WindDef Wind { get; set; } = new();
    /// <summary>The leaves.</summary>
    public LeavesDef Leaves { get; set; } = new();
    /// <summary>The birds.</summary>
    public BirdsDef Birds { get; set; } = new();
    /// <summary>Speed factor in shallow water (fords).</summary>
    public float ShallowSpeed { get; set; } = 0.6f;
    /// <summary>Deep water: walking on it with Shift and Ki, or swimming (pedido 14.3).</summary>
    public DeepWaterDef DeepWater { get; set; } = new();
    /// <summary>Seconds between ripples while walking in shallow water.</summary>
    public float RippleInterval { get; set; } = 0.32f;
    /// <summary>The tall grass.</summary>
    public GrassDef TallGrass { get; set; } = new();

    /// <summary>Parses the data.</summary>
    public static AmbientSettings Parse(string json) => JsonSerializer.Deserialize<AmbientSettings>(json, PlainsGenSettings.Json) ?? new AmbientSettings();
    /// <summary>Loads the data.</summary>
    public static AmbientSettings Load(string path = DefaultPath) => Parse(Godot.FileAccess.GetFileAsString(path));

    /// <summary>The tint at an hour (0-24), between the palette's keys.</summary>
    public Color TintAt(float hour)
    {
        if (Palette.Count == 0) return Colors.White;
        hour = Mathf.PosMod(hour, 24f);
        for (int i = 0; i < Palette.Count - 1; i++)
        {
            var a = Palette[i]; var b = Palette[i + 1];
            if (hour >= a.Hour && hour <= b.Hour)
                return new Color(a.Color).Lerp(new Color(b.Color), b.Hour - a.Hour < 0.001f ? 0 : (hour - a.Hour) / (b.Hour - a.Hour));
        }
        return new Color(Palette[^1].Color);
    }
}

/// <summary>
/// The wind over the Planície: a steady breeze and gusts that travel along the ground as a wave (so the grass
/// bends in a running ripple, not all at once). Shared by the grass, the trees and the leaves. The clock is
/// advanced by the game (deterministic for checks).
/// </summary>
public static class Wind
{
    /// <summary>The wind's clock (s).</summary>
    public static float T { get; set; }
    /// <summary>The numbers in force.</summary>
    public static AmbientSettings.WindDef Def { get; set; } = new();
    /// <summary>The wind's direction (unit).</summary>
    public static Vector2 Dir => Vector2.Right.Rotated(Mathf.DegToRad(Def.Direction));

    /// <summary>The gust envelope now (0 calm … 1 peak).</summary>
    public static float Gust
    {
        get
        {
            float p = Mathf.Max(1, Def.GustPeriod), x = T / p;
            float g = Mathf.Max(0, Mathf.Sin(x * Mathf.Tau) * 0.6f + Mathf.Sin(x * Mathf.Tau * 2.3f + 1.7f) * 0.4f);
            return Mathf.Clamp(g, 0, 1);
        }
    }

    /// <summary>The push at a world point (base + the travelling gust), as a signed amount along <see cref="Dir"/>.</summary>
    public static float At(Vector2 p)
    {
        float phase = p.Dot(Dir) / Mathf.Max(30, Def.Wavelength) * Mathf.Tau;
        return Def.Base * Mathf.Sin(T * 1.3f - phase * 0.5f) + Def.Gust * Gust * (0.6f + 0.4f * Mathf.Sin(T * 2.2f - phase));
    }
}

/// <summary>
/// Day and night (a CanvasModulate over the world, not the HUD): the hour advances with the data's day
/// length, the world is tinted by the palette (a bluish but readable night), and <see cref="Darkness"/> drives
/// the campfire's light and the faint light around the swordsman. The hour is saved with the journey.
/// </summary>
public partial class DayNight : CanvasModulate
{
    /// <summary>The numbers.</summary>
    public AmbientSettings Settings { get; set; } = new();
    /// <summary>The hour (0-24).</summary>
    public float Hour { get; set; } = 8;
    /// <summary>True to stop the clock (checks set an hour and look).</summary>
    public bool Frozen { get; set; }

    /// <summary>0 at full day … 1 at deep night (from the tint's brightness).</summary>
    public float Darkness { get { var c = Settings.TintAt(Hour); float l = (c.R + c.G + c.B) / 3f; return Mathf.Clamp((0.95f - l) / 0.4f, 0, 1); } }

    /// <summary>The part of the day in words.</summary>
    public string Phase => Hour switch { < 5 => "Madrugada", < 7.5f => "Amanhecer", < 12 => "Manhã", < 17 => "Tarde", < 19.5f => "Entardecer", < 21 => "Anoitecer", _ => "Noite" };

    /// <summary>The clock ("08:30").</summary>
    public string Clock => $"{(int)Hour:00}:{(int)((Hour % 1) * 60):00}";

    /// <summary>Advances the hour and applies the tint.</summary>
    public void Advance(float dt)
    {
        if (!Frozen) Hour = Mathf.PosMod(Hour + dt * 24f / Mathf.Max(10, Settings.DaySeconds), 24f);
        Color = Settings.TintAt(Hour);
    }
}

/// <summary>A soft round light (campfire, the swordsman at night): a PointLight2D with a radial gradient.</summary>
public static class SoftLight
{
    private static GradientTexture2D? _tex;

    /// <summary>Makes a light of a radius and colour (energy set later by the night).</summary>
    public static PointLight2D Make(float radius, Color color)
    {
        _tex ??= new GradientTexture2D
        {
            Width = 128, Height = 128, Fill = GradientTexture2D.FillEnum.Radial, FillFrom = new Vector2(0.5f, 0.5f), FillTo = new Vector2(1f, 0.5f),
            Gradient = new Gradient { Colors = new[] { Colors.White, new Color(1, 1, 1, 0) }, Offsets = new[] { 0f, 1f } },
        };
        return new PointLight2D { Texture = _tex, TextureScale = radius * 2 / 128f, Color = color, Energy = 0, BlendMode = Light2D.BlendModeEnum.Add, Name = "Luz" };
    }
}

/// <summary>
/// The tall grass of a chunk: tufts on grass and bush tiles that bend with the wind (a travelling gust) and
/// part when the swordsman walks through them. Drawn over the ground, redrawn only while the chunk is near.
/// </summary>
public partial class GrassSway : Node2D
{
    /// <summary>The chunk.</summary>
    public ChunkData Data { get; set; } = null!;
    /// <summary>The generator (seed, tile size).</summary>
    public PlainsGenerator Gen { get; set; } = null!;
    /// <summary>The numbers.</summary>
    public AmbientSettings Settings { get; set; } = new();
    /// <summary>The swordsman (the grass parts around him).</summary>
    public Node2D? Player { get; set; }
    private readonly List<(Vector2 P, float H, int V)> _tufts = new();
    private float _redraw;
    private static readonly Color Blade = new("#6f8a4c"), BladeHi = new("#8ea660"), BladeDark = new("#3d4a33");

    /// <summary>The largest bend now (checks).</summary>
    public float MaxBend { get; private set; }
    /// <summary>The tufts (checks).</summary>
    public int Count => _tufts.Count;
    /// <summary>A tuft's world position (checks), or null when the chunk has none.</summary>
    public Vector2? FirstTuft => _tufts.Count == 0 ? null : GlobalPosition + _tufts[_tufts.Count / 2].P;

    /// <inheritdoc />
    public override void _Ready()
    {
        int n = Data.Size, ts = Gen.Settings.TileSize;
        for (int ly = 0; ly < n; ly++)
            for (int lx = 0; lx < n; lx++)
            {
                var k = Data[lx, ly];
                float per = k == TileKind.BushGrass ? Settings.TallGrass.PerBushTile : k == TileKind.Grass ? Settings.TallGrass.PerGrassTile : 0;
                int tx = Data.Coord.X * n + lx, ty = Data.Coord.Y * n + ly;
                for (int i = 0; i < Mathf.CeilToInt(per); i++)
                {
                    uint h = PlainsGenerator.Hash(Gen.Seed, tx * 5 + i, ty * 11, 950);
                    if ((h % 1000) / 1000f >= per - i) continue;
                    _tufts.Add((new Vector2(lx * ts + 3 + h % (uint)(ts - 6), ly * ts + 6 + (h >> 8) % (uint)(ts - 8)), 6 + (h >> 16) % 6, (int)(h & 255)));
                }
            }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _redraw -= (float)delta;
        if (_redraw > 0) return;
        _redraw = 0.05f;
        if (Player != null && IsInstanceValid(Player) && Player.GlobalPosition.DistanceTo(GlobalPosition + Vector2.One * Data.Size * Gen.Settings.TileSize / 2) > 900) return;
        QueueRedraw();
    }

    private readonly System.Collections.Generic.List<Vector2> _bodies = new();

    /// <inheritdoc />
    public override void _Draw()
    {
        float max = 0;
        float part = Settings.TallGrass.PartRadius, size = Data.Size * Gen.Settings.TileSize;
        // the bodies that part the grass: the swordsman and the animals (pedido 14: the rabbit too), in this chunk's pixels
        _bodies.Clear();
        if (Player != null && IsInstanceValid(Player)) _bodies.Add(Player.GlobalPosition - GlobalPosition);
        foreach (var n in GetTree().GetNodesInGroup("creatures"))
            if (n is Node2D c && IsInstanceValid(c))
            {
                Vector2 q = c.GlobalPosition - GlobalPosition;
                if (q.X > -part && q.Y > -part && q.X < size + part && q.Y < size + part) _bodies.Add(q);
            }
        foreach (var (p, h, v) in _tufts)
        {
            float w = Wind.At(GlobalPosition + p);
            float bend = w * 2.2f * Wind.Dir.X;
            foreach (var me in _bodies)
            {
                Vector2 away = p - me;
                float d = away.Length();
                if (d < part) bend += Mathf.Sign(away.X == 0 ? 1 : away.X) * (part - d) / part * 4f; // parted by the passing body
            }
            max = Mathf.Max(max, Mathf.Abs(bend));
            for (int b = -1; b <= 1; b++)
            {
                float bh = h - Mathf.Abs(b) * 2, bx = p.X + b * 2;
                for (int y = 0; y < bh; y++)
                {
                    float k = y / bh, off = Mathf.Round(bend * k * k + b * k);
                    DrawRect(new Rect2(bx + off, p.Y - y, 1, 1), y > bh - 2 ? BladeHi : b == 1 ? BladeDark : Blade);
                }
            }
        }
        MaxBend = max;
    }
}

/// <summary>Leaves blown off in the gusts, drifting with the wind across the view (world space, above the objects).</summary>
public partial class LeafFx : Node2D
{
    /// <summary>The numbers.</summary>
    public AmbientSettings Settings { get; set; } = new();
    /// <summary>Where the view is (the swordsman).</summary>
    public Node2D? Around { get; set; }
    private sealed class Leaf { public Vector2 P, V; public float Life, Spin; public Color C; }
    private readonly List<Leaf> _leaves = new();
    private float _acc;
    private uint _rng = 7;
    private float Rnd() { _rng ^= _rng << 13; _rng ^= _rng >> 17; _rng ^= _rng << 5; return (_rng & 0xFFFF) / 65536f; }
    /// <summary>Leaves in the air (checks).</summary>
    public int Count => _leaves.Count;

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        if (Around != null && IsInstanceValid(Around))
        {
            _acc += Settings.Leaves.PerSecond * Wind.Gust * dt;
            while (_acc >= 1)
            {
                _acc -= 1;
                Vector2 c = Around.GlobalPosition;
                _leaves.Add(new Leaf { P = c + new Vector2(-380 + Rnd() * 200, -220 + Rnd() * 420), V = Wind.Dir * (40 + Rnd() * 50) + new Vector2(0, 10 + Rnd() * 14), Life = Settings.Leaves.Life, Spin = Rnd() * 6,
                    C = Rnd() < 0.5f ? new Color("#7a9a4a") : Rnd() < 0.5f ? new Color("#a8a04a") : new Color("#5f8a3a") });
            }
        }
        for (int i = _leaves.Count - 1; i >= 0; i--)
        {
            var l = _leaves[i];
            l.Life -= dt; l.Spin += dt * 5;
            l.P += (l.V + Wind.Dir * Wind.Gust * 60) * dt + new Vector2(0, Mathf.Sin(l.Spin) * 12 * dt);
            if (l.Life <= 0) _leaves.RemoveAt(i);
        }
        QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        foreach (var l in _leaves)
        {
            float a = Mathf.Min(1, l.Life);
            bool flat = Mathf.Sin(l.Spin) > 0;
            DrawRect(new Rect2(l.P.Round(), flat ? new Vector2(3, 1) : new Vector2(2, 2)), new Color(l.C, a));
        }
    }
}

/// <summary>Flocks of birds crossing the sky by day, flapping, each with its shadow running on the ground below.</summary>
public partial class Birds : Node2D
{
    /// <summary>The numbers.</summary>
    public AmbientSettings Settings { get; set; } = new();
    /// <summary>Where the view is.</summary>
    public Node2D? Around { get; set; }
    /// <summary>The day/night (no birds at night).</summary>
    public DayNight? Sky { get; set; }
    private sealed class Bird { public Vector2 Ground, V; public float Flap, Height; }
    private readonly List<Bird> _birds = new();
    private float _next = 4;
    private uint _rng = 11;
    private float Rnd() { _rng ^= _rng << 13; _rng ^= _rng >> 17; _rng ^= _rng << 5; return (_rng & 0xFFFF) / 65536f; }
    /// <summary>Birds in the sky (checks).</summary>
    public int Count => _birds.Count;

    /// <summary>Sends a flock now (checks; the game does it on a timer).</summary>
    public void Flock()
    {
        if (Around == null || !IsInstanceValid(Around)) return;
        int n = Settings.Birds.Min + (int)(Rnd() * (Settings.Birds.Max - Settings.Birds.Min + 1));
        bool fromLeft = Rnd() < 0.5f;
        Vector2 c = Around.GlobalPosition, start = c + new Vector2(fromLeft ? -420 : 420, -120 + Rnd() * 160);
        Vector2 v = new Vector2(fromLeft ? 1 : -1, -0.15f + Rnd() * 0.3f).Normalized() * Settings.Birds.Speed;
        for (int i = 0; i < n; i++)
            _birds.Add(new Bird { Ground = start + new Vector2(-Mathf.Sign(v.X) * i * 14, (i % 2 == 0 ? 1 : -1) * i * 7), V = v, Flap = Rnd() * 6, Height = Settings.Birds.Height + Rnd() * 20 });
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        bool day = Sky == null || Sky.Darkness < 0.4f;
        _next -= dt;
        if (_next <= 0) { _next = Settings.Birds.Interval * (0.6f + Rnd() * 0.8f); if (day) Flock(); }
        for (int i = _birds.Count - 1; i >= 0; i--)
        {
            var b = _birds[i];
            b.Ground += b.V * dt; b.Flap += dt * 9;
            if (Around == null || !IsInstanceValid(Around) || b.Ground.DistanceTo(Around.GlobalPosition) > 700) _birds.RemoveAt(i);
        }
        QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        foreach (var b in _birds)
        {
            // the shadow on the ground (where the bird really is), then the bird up in the air
            DrawRect(new Rect2(b.Ground.Round() - new Vector2(3, 1), new Vector2(6, 2)), new Color(0.03f, 0.04f, 0.03f, 0.3f));
            Vector2 p = (b.Ground - new Vector2(0, b.Height)).Round();
            int wing = Mathf.Sin(b.Flap) > 0 ? -2 : 1;
            var ink = new Color("#2a2420");
            DrawRect(new Rect2(p.X - 1, p.Y, 2, 2), ink);
            DrawRect(new Rect2(p.X - 4, p.Y + wing, 3, 1), ink); DrawRect(new Rect2(p.X + 1, p.Y + wing, 3, 1), ink);
            DrawRect(new Rect2(p.X - 5, p.Y + wing + (wing < 0 ? 1 : -1), 1, 1), ink); DrawRect(new Rect2(p.X + 4, p.Y + wing + (wing < 0 ? 1 : -1), 1, 1), ink);
        }
    }
}

/// <summary>Ripples in shallow water: rings that spread around the swordsman's feet while he wades.</summary>
public partial class Ripples : Node2D
{
    private readonly List<(Vector2 P, float Life)> _rings = new();
    /// <summary>Rings spreading now (checks).</summary>
    public int Count => _rings.Count;
    /// <summary>All the ripples made so far (checks).</summary>
    public int Made { get; private set; }

    /// <summary>Starts a ripple at a world point.</summary>
    public void Add(Vector2 at) { _rings.Add((at, 0.8f)); Made++; }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        for (int i = _rings.Count - 1; i >= 0; i--) { var r = _rings[i]; r.Life -= (float)delta; if (r.Life <= 0) _rings.RemoveAt(i); else _rings[i] = r; }
        QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        foreach (var (p, life) in _rings)
        {
            float k = 1 - life / 0.8f, rad = 4 + k * 12;
            var c = new Color("#c8e0dc", 0.7f * (1 - k));
            for (int i = 0; i < 14; i++) { float a = i * Mathf.Tau / 14; DrawRect(new Rect2(p + new Vector2(Mathf.Cos(a) * rad, Mathf.Sin(a) * rad * 0.45f), Vector2.One), c); }
        }
    }
}
