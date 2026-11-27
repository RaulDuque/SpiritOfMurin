using Godot;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Game;

/// <summary>
/// The assist mode (Esc → Modo de assistência → Mostrar áreas de combate / Mostrar números de dano). With the areas on, the fights draw what
/// the combat code tests: the arc a guard parries, the area a blade covers while it is live, and the
/// direction of an announced attack. Every drawing reads the same numbers the hit tests use, so what is
/// seen is what happens. Three phases are told apart by shape and motion, not only colour:
/// <list type="bullet">
/// <item><b>aviso</b> (warning): a dashed outline that fills from the body outward as the strike nears;</item>
/// <item><b>golpe ativo</b> (live): a solid filled area with a thick edge;</item>
/// <item><b>recuperação</b> (recovery): a hollow, thin outline that shrinks — the opening.</item>
/// </list>
/// The flank opening is a dashed jade arc around the rest of the body. With the mode off, the game's own
/// cues stay (pose, the short parry arc, the hoof dust, the hit reaction).
/// </summary>
public static class Assist
{
    private static bool? _areas;

    /// <summary>True when the combat areas are shown (saved in the preferences).</summary>
    public static bool Areas
    {
        get => _areas ??= Prefs.Load().AssistAreas;
        set { _areas = value; var p = Prefs.Load(); p.AssistAreas = value; p.Save(); }
    }

    private static bool? _numbers;

    /// <summary>True when the damage numbers are shown (dealt and taken, saved in the preferences). See <see cref="CombatText"/>.</summary>
    public static bool Numbers
    {
        get => _numbers ??= Prefs.Load().AssistNumbers;
        set { _numbers = value; var p = Prefs.Load(); p.AssistNumbers = value; p.Save(); }
    }

    /// <summary>A short summary for the pause menu: what the assist mode shows now.</summary>
    public static string Summary => (Areas, Numbers) switch
    {
        (true, true) => "áreas e números de dano visíveis",
        (true, false) => "áreas de combate visíveis",
        (false, true) => "números de dano visíveis",
        _ => "desligado",
    };

    /// <summary>Forgets the cached values (tests that switch preference files).</summary>
    public static void Reload() { _areas = null; _numbers = null; }

    /// <summary>Colours: guard (bone), opening (jade), threat (red-ochre), the player's blade (pale jade).</summary>
    public static readonly Color GuardC = new("#ece5d0"), OpenC = new("#6fd8d2"), ThreatC = new("#e0503a"), BladeC = new("#a6f2e2");

    /// <summary>A sector (pie slice) from <paramref name="at"/>: centre angle, half-angle and radius, filled.</summary>
    public static void Sector(CanvasItem c, Vector2 at, float ang, float half, float r, Color col, int segs = 18)
    {
        var pts = new Vector2[segs + 2];
        pts[0] = at;
        for (int i = 0; i <= segs; i++) { float a = ang - half + 2 * half * i / segs; pts[i + 1] = at + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r; }
        c.DrawColoredPolygon(pts, col);
    }

    /// <summary>The outline of a sector; dashed when <paramref name="dash"/> &gt; 0 (dash length in px).</summary>
    public static void SectorEdge(CanvasItem c, Vector2 at, float ang, float half, float r, Color col, float width = 1, float dash = 0)
    {
        Vector2 a0 = at + new Vector2(Mathf.Cos(ang - half), Mathf.Sin(ang - half)) * r, a1 = at + new Vector2(Mathf.Cos(ang + half), Mathf.Sin(ang + half)) * r;
        Line(c, at, a0, col, width, dash); Line(c, at, a1, col, width, dash);
        Arc(c, at, ang, half, r, col, width, dash);
    }

    /// <summary>An arc; dashed when <paramref name="dash"/> &gt; 0.</summary>
    public static void Arc(CanvasItem c, Vector2 at, float ang, float half, float r, Color col, float width = 1, float dash = 0)
    {
        if (dash <= 0) { c.DrawArc(at, r, ang - half, ang + half, 24, col, width); return; }
        float step = dash / Mathf.Max(4, r);
        for (float a = ang - half; a < ang + half; a += step * 2)
            c.DrawArc(at, r, a, Mathf.Min(a + step, ang + half), 3, col, width);
    }

    /// <summary>A line; dashed when <paramref name="dash"/> &gt; 0.</summary>
    public static void Line(CanvasItem c, Vector2 a, Vector2 b, Color col, float width = 1, float dash = 0)
    {
        if (dash <= 0) { c.DrawLine(a, b, col, width); return; }
        c.DrawDashedLine(a, b, col, width, dash);
    }

    /// <summary>A corridor (a straight charge): from <paramref name="at"/> along <paramref name="dir"/>, length and half-width.</summary>
    public static Vector2[] Corridor(Vector2 at, Vector2 dir, float length, float halfWidth)
    {
        Vector2 n = new(-dir.Y, dir.X);
        return new[] { at + n * halfWidth, at + dir * length + n * halfWidth, at + dir * length - n * halfWidth, at - n * halfWidth };
    }
}
