using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Vale;

/// <summary>
/// The prologue's effects layer (index.html: parts, fx, trails): spark bursts, words over a hit
/// ("aparado", "flanco", "passo"), rings, enemy slashes, and the lock brackets on the target. Drawn
/// above the figures. The ground marks (footprints, step trails, the cut left on the ground) live in
/// <see cref="ValeDecals"/>.
/// </summary>
public partial class ValeFx : Node2D
{
    private sealed class Part { public Vector2 P, V; public float Life, Max, Size; public Color C; }
    private sealed class Word { public Vector2 P; public string S = ""; public float Life, Max; public Color C; }
    private sealed class Ring { public Vector2 P; public float Life, Max, R; public Color C; }
    private sealed class ESlash { public Vector2 P; public float Ang, Life, Max; public bool Big; }

    private readonly List<Part> _parts = new();
    private readonly List<Word> _words = new();
    private readonly List<Ring> _rings = new();
    private readonly List<ESlash> _slashes = new();
    private uint _rng = 99;
    private float _t;
    private Font? _font;

    /// <summary>The locked target (corner brackets), or null.</summary>
    public Node2D? Lock { get; set; }

    /// <summary>Words shown so far (newest last), for checks.</summary>
    public List<string> WordsShown { get; } = new();

    private float Rnd() { _rng ^= _rng << 13; _rng ^= _rng >> 17; _rng ^= _rng << 5; return (_rng & 0xFFFF) / 65536f; }

    /// <summary>A burst of <paramref name="n"/> sparks.</summary>
    public void Burst(Vector2 at, int n, Color c, float spd = 90, float life = 0.35f, float size = 2)
    {
        for (int i = 0; i < n; i++)
        {
            float a = Rnd() * Mathf.Tau, v = spd * (0.4f + Rnd() * 0.8f);
            _parts.Add(new Part { P = at, V = new Vector2(Mathf.Cos(a) * v, Mathf.Sin(a) * v * 0.75f), Life = life, Max = life, C = c, Size = size });
        }
    }

    /// <summary>A particle with a given velocity (dust, leaves, embers).</summary>
    public void Particle(Vector2 at, Vector2 v, Color c, float life, float size = 1) => _parts.Add(new Part { P = at, V = v, Life = life, Max = life, C = c, Size = size });

    /// <summary>A short word over a point.</summary>
    public void Say(Vector2 at, string s, Color c, float life = 0.6f) { _words.Add(new Word { P = at, S = s, Life = life, Max = life, C = c }); WordsShown.Add(s); }

    /// <summary>An expanding ring.</summary>
    public void AddRing(Vector2 at, Color c, float life = 0.15f, float r = 14) => _rings.Add(new Ring { P = at, C = c, Life = life, Max = life, R = r });

    /// <summary>An enemy's slash.</summary>
    public void EnemySlash(Vector2 at, float ang, bool big) => _slashes.Add(new ESlash { P = at, Ang = ang, Big = big, Life = 0.18f, Max = 0.18f });

    /// <inheritdoc />
    public override void _Ready() => _font = ThemeDB.FallbackFont;

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _t += dt;
        for (int i = _parts.Count - 1; i >= 0; i--) { var q = _parts[i]; q.P += q.V * dt; q.V *= 0.9f; q.Life -= dt; if (q.Life <= 0) _parts.RemoveAt(i); }
        for (int i = _words.Count - 1; i >= 0; i--) { _words[i].Life -= dt; _words[i].P += new Vector2(0, -14 * dt); if (_words[i].Life <= 0) _words.RemoveAt(i); }
        for (int i = _rings.Count - 1; i >= 0; i--) { _rings[i].Life -= dt; if (_rings[i].Life <= 0) _rings.RemoveAt(i); }
        for (int i = _slashes.Count - 1; i >= 0; i--) { _slashes[i].Life -= dt; if (_slashes[i].Life <= 0) _slashes.RemoveAt(i); }
        QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        foreach (var q in _parts) DrawRect(new Rect2(q.P.Round(), new Vector2(q.Size, q.Size)), new Color(q.C, Mathf.Min(1, q.Life / q.Max * 1.5f)));
        foreach (var r in _rings)
        {
            float k = 1 - r.Life / r.Max, rad = r.R * (0.6f + k);
            for (int i = 0; i < 16; i++) { float a = i * Mathf.Tau / 16; DrawRect(new Rect2(r.P + new Vector2(Mathf.Cos(a) * rad, Mathf.Sin(a) * rad * 0.7f), new Vector2(2, 2)), new Color(r.C, 1 - k)); }
        }
        foreach (var e in _slashes)
        {
            float k = e.Life / e.Max, len = e.Big ? 52 : 36;
            Color c = new(e.Big ? "#e8834c" : "#c8502a", k);
            for (float i = -len / 2; i < len / 2; i++)
            {
                float a = e.Ang + Mathf.Pi / 2, bend = (1 - 4 * i * i / (len * len)) * len * 0.18f;
                Vector2 p = e.P + new Vector2(Mathf.Cos(a) * i, Mathf.Sin(a) * i * 0.7f) + new Vector2(Mathf.Cos(e.Ang), Mathf.Sin(e.Ang)) * bend;
                DrawRect(new Rect2(p.Round(), new Vector2(2, 2)), c);
            }
        }
        if (_font != null)
            foreach (var w in _words)
            {
                var col = new Color(w.C, Mathf.Min(1, w.Life / w.Max * 2));
                Vector2 at = w.P.Round() + new Vector2(-w.S.Length * 2.5f, 0);
                DrawString(_font, at + Vector2.One, w.S, HorizontalAlignment.Left, -1, 9, new Color("#16140f", col.A));
                DrawString(_font, at, w.S, HorizontalAlignment.Left, -1, 9, col);
            }
        if (Lock != null && IsInstanceValid(Lock))
        {
            Vector2 c = Lock.GlobalPosition + new Vector2(0, -20);
            var col = new Color("#e8834c");
            float s = 16 + Mathf.Sin(_t * 6) * 1.5f;
            foreach (var (dx, dy) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) })
            {
                Vector2 o = c + new Vector2(dx * s, dy * s * 1.4f);
                DrawRect(new Rect2(o.X - (dx > 0 ? 4 : 0), o.Y, 5, 1), col);
                DrawRect(new Rect2(o.X, o.Y - (dy > 0 ? 4 : 0), 1, 5), col);
            }
        }
    }
}

/// <summary>The prologue's ground marks: the three footprints by the statue, the step marks the player leaves, the meditation mat.</summary>
public partial class ValeDecals : Node2D
{
    /// <summary>The story state (slab read, footprints walked, side step learned).</summary>
    public ValeState Story { get; set; } = null!;
    /// <summary>Seconds of the mat's shine after a meditation.</summary>
    public float Meditated { get; set; }
    private readonly List<(Vector2 P, float Life)> _feet = new();
    private float _t;

    /// <summary>Leaves a step mark.</summary>
    public void Foot(Vector2 p) => _feet.Add((p, 1.4f));

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        float dt = (float)delta; _t += dt; Meditated = Mathf.Max(0, Meditated - dt);
        for (int i = _feet.Count - 1; i >= 0; i--) { var f = _feet[i]; f.Life -= dt; if (f.Life <= 0) _feet.RemoveAt(i); else _feet[i] = f; }
        QueueRedraw();
    }

    private void R(float x, float y, float w, float h, Color c) => DrawRect(new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), c);

    /// <inheritdoc />
    public override void _Draw()
    {
        // the meditation mat at the foot of the west twin stone
        Vector2 m = ValeMap.Meditate;
        R(m.X - 9, m.Y - 3, 18, 7, ValeTerrain.Outline); R(m.X - 7, m.Y - 4, 14, 9, ValeTerrain.Outline);
        R(m.X - 8, m.Y - 2, 16, 5, new Color("#8f8466")); R(m.X - 6, m.Y - 3, 12, 7, new Color("#8f8466"));
        R(m.X - 6, m.Y - 2, 12, 3, new Color("#b0a382")); R(m.X - 4, m.Y - 2, 6, 1, new Color("#c4b893"));
        float k = Meditated > 0 ? Meditated / 1.6f : 0.25f;
        for (int i = 0; i < 4; i++) R(m.X + 10 + Mathf.Round(Mathf.Sin(_t * 2 + i) * 1.5f), m.Y - 6 - i * 4, 1, 3, new Color("#a6f2e2", 0.35f + 0.5f * k));
        // footprints in the tomb
        bool lit = Story.SlabRead || Story.LearnedSide;
        for (int i = 0; i < ValeMap.Marks.Length; i++)
        {
            bool on = Story.LearnedSide || i < Story.FpIdx, next = lit && !Story.LearnedSide && i == Story.FpIdx;
            bool pulse = next && Mathf.FloorToInt(_t * 4) % 2 == 1;
            Color c = on ? ValeTerrain.Learn : lit ? (pulse ? ValeTerrain.Learn : ValeTerrain.LearnGlow) : new Color("#4c4c53");
            Vector2 p = ValeMap.Marks[i];
            float a = Mathf.DegToRad(i == 0 ? 90 : i == 1 ? 140 : 192) + Mathf.Pi / 2;
            float ox = Mathf.Round(Mathf.Cos(a) * 3), oy = Mathf.Round(Mathf.Sin(a) * 3);
            R(p.X - 4 + ox, p.Y - 3 + oy, 3, 5, c); R(p.X + 1 - ox, p.Y - 2 - oy, 3, 5, c);
            R(p.X - 4 + ox, p.Y - 5 + oy, 1, 1, c); R(p.X + 2 - ox, p.Y - 4 - oy, 1, 1, c);
            if (lit) R(p.X - 7, p.Y - 7, 14, 12, new Color(111 / 255f, 216 / 255f, 210 / 255f, 0.15f));
        }
        foreach (var (p, life) in _feet)
        {
            var c = new Color(ValeTerrain.LearnGlow, life / 1.4f * 0.85f);
            R(p.X - 3, p.Y - 1, 2, 3, c); R(p.X + 1, p.Y - 2, 2, 3, c);
        }
    }
}
