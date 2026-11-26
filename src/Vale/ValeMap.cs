using Godot;

namespace SpiritOfMurin.Vale;

/// <summary>The Vale's tile kinds (the prototype's map).</summary>
public enum VTile : byte { Grass, Rock, Water, Chasm, Bridge, Path, TombFloor, TombWall, Bamboo, Gate, Camp, Door, Tent, Stele, CutWall, Pine }

/// <summary>
/// The Vale de Bambus — the campaign's prologue — tile for tile as in the HTML prototype (index.html,
/// buildMap): the trail up the mountain, the bridge over the Rio Nublado guarded by the disciple, the
/// gap, the bamboo valley with the master's tomb and the twin stones, and the disciples' camp behind
/// the palisade. 60 × 40 tiles of 32 px. Pure data and queries, no nodes.
/// </summary>
public sealed class ValeMap
{
    /// <summary>Tile size in pixels.</summary>
    public const int TS = 32;
    /// <summary>Width and height in tiles.</summary>
    public const int MW = 60, MH = 40;

    private readonly VTile[] _t = new VTile[MW * MH];

    /// <summary>The start of the trail.</summary>
    public static readonly Vector2 Start = new(29 * TS, 35.5f * TS);
    /// <summary>The camp entry (respawn after the gate opens).</summary>
    public static readonly Vector2 CampEntry = new(29 * TS, 11.4f * TS);
    /// <summary>The master's statue.</summary>
    public static readonly Vector2 Statue = new(51 * TS, 25.4f * TS);
    /// <summary>The disciple's body with the half manual.</summary>
    public static readonly Vector2 Corpse = new(40.5f * TS, 35.3f * TS);
    /// <summary>The sealed tomb door.</summary>
    public static readonly Vector2 Door = new(51 * TS, 31.2f * TS);
    /// <summary>The cut in the stone (slab).</summary>
    public static readonly Vector2 Slab = new(51 * TS, 22.7f * TS);
    /// <summary>The inscription beyond the gap.</summary>
    public static readonly Vector2 Stele = new(17.5f * TS, 33.4f * TS);
    /// <summary>The forgotten chest beyond the gap.</summary>
    public static readonly Vector2 Chest = new(6 * TS, 27.6f * TS);
    /// <summary>The meditation mat at the foot of the west twin stone.</summary>
    public static readonly Vector2 Meditate = new(49.1f * TS, 33.3f * TS);
    /// <summary>The three footprints around the statue (angles 90°, 140°, 192°, 60 px).</summary>
    public static readonly Vector2[] Marks = { Mark(90), Mark(140), Mark(192) };
    /// <summary>Breakable pots.</summary>
    public static readonly Vector2[] Pots = { T(12, 11.5f), T(13, 11.6f), T(45, 11.4f), T(31.2f, 36.4f), T(46, 32.4f), T(25.5f, 20.8f), T(20, 10.5f), T(36, 11.6f) };
    /// <summary>Spirit herbs picked by walking over them.</summary>
    public static readonly Vector2[] Herbs = { T(40, 27.5f), T(42.5f, 26.6f), T(43.6f, 28.6f), T(7.5f, 25), T(9.5f, 22.4f), T(33.5f, 36.2f), T(26.5f, 25.6f), T(52, 34.6f) };
    /// <summary>Named places for the map, in tiles.</summary>
    public static readonly (string Name, Vector2 At)[] Landmarks =
    {
        ("Trilha da Montanha", T(29, 34)), ("Ponte do Rio Nublado", T(29, 18)), ("Acampamento", T(29, 6)), ("Vale de Bambus", T(40, 30)),
        ("Túmulo da Mestra", T(51, 25)), ("Além da Fenda", T(10, 30)),
    };

    private static Vector2 T(float x, float y) => new(x * TS, y * TS);
    private static Vector2 Mark(float deg) => Statue + new Vector2(Mathf.Cos(Mathf.DegToRad(deg)), Mathf.Sin(Mathf.DegToRad(deg))) * 60;

    /// <summary>Builds the map.</summary>
    public ValeMap()
    {
        void Set(int x, int y, VTile v) { if (x >= 0 && y >= 0 && x < MW && y < MH) _t[y * MW + x] = v; }
        void Rect(int x0, int y0, int x1, int y1, VTile v) { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) Set(x, y, v); }
        Rect(2, 2, 57, 12, VTile.Camp);
        Rect(0, 13, MW - 1, 14, VTile.Rock); Rect(27, 13, 30, 13, VTile.Gate); Rect(27, 14, 30, 14, VTile.Camp);
        Rect(0, 17, MW - 1, 19, VTile.Water); Rect(27, 17, 30, 19, VTile.Bridge);
        Rect(0, 0, MW - 1, 1, VTile.Rock); Rect(0, MH - 2, MW - 1, MH - 1, VTile.Rock); Rect(0, 0, 1, MH - 1, VTile.Rock); Rect(MW - 2, 0, MW - 1, MH - 1, VTile.Rock);
        Rect(28, 20, 29, 36, VTile.Path); Rect(30, 33, 42, 34, VTile.Path); Rect(42, 31, 43, 34, VTile.Path); Rect(43, 31, 51, 31, VTile.Path);
        Rect(21, 20, 22, MH - 3, VTile.Chasm);
        Rect(45, 21, 56, 30, VTile.TombWall); Rect(46, 22, 55, 29, VTile.TombFloor); Rect(50, 30, 51, 30, VTile.Door); Rect(50, 21, 51, 21, VTile.CutWall);
        Set(48, 32, VTile.Rock); Set(53, 32, VTile.Rock);
        Rect(7, 4, 10, 6, VTile.Tent); Rect(47, 4, 50, 6, VTile.Tent); Rect(15, 8, 17, 9, VTile.Tent); Rect(40, 8, 42, 9, VTile.Tent);
        Rect(34, 22, 36, 27, VTile.Bamboo); Rect(16, 23, 18, 25, VTile.Bamboo); Rect(38, 36, 44, 37, VTile.Bamboo);
        Rect(3, 34, 6, 37, VTile.Bamboo); Rect(4, 21, 6, 23, VTile.Bamboo); Rect(52, 33, 56, 37, VTile.Bamboo); Rect(2, 2, 4, 4, VTile.Bamboo); Rect(54, 9, 56, 11, VTile.Bamboo);
        foreach (var (x, y) in new[] { (24, 30), (33, 29), (18, 34), (39, 24), (9, 32), (19, 37), (44, 26), (31, 24), (11, 10), (36, 4), (21, 5) }) Set(x, y, VTile.Rock);
        Set(17, 32, VTile.Stele);
        foreach (var (x, y) in new[] { (25, 22), (26, 27), (32, 22), (37, 31), (34, 37), (24, 35), (11, 35), (15, 21), (19, 29), (3, 8), (5, 11), (12, 3),
            (52, 7), (56, 6), (44, 12), (33, 9), (24, 9), (46, 37), (56, 30), (47, 34) })
            if (At(x, y) is VTile.Grass or VTile.Camp) Set(x, y, VTile.Pine);
    }

    /// <summary>The tile at tile coordinates (rock outside the map).</summary>
    public VTile At(int tx, int ty) => tx < 0 || ty < 0 || tx >= MW || ty >= MH ? VTile.Rock : _t[ty * MW + tx];

    /// <summary>The tile under a world pixel.</summary>
    public VTile AtPixel(Vector2 p) => At(Mathf.FloorToInt(p.X / TS), Mathf.FloorToInt(p.Y / TS));

    /// <summary>A lone boulder: a rock tile with no rock beside it (cliffs and borders stay walls).</summary>
    public bool IsBoulder(int tx, int ty) => At(tx, ty) == VTile.Rock && At(tx - 1, ty) != VTile.Rock && At(tx + 1, ty) != VTile.Rock && At(tx, ty - 1) != VTile.Rock && At(tx, ty + 1) != VTile.Rock;

    /// <summary>True for tiles drawn standing up (y-sorted with the figures).</summary>
    public static bool IsTall(VTile t) => t is VTile.Rock or VTile.TombWall or VTile.CutWall or VTile.Bamboo or VTile.Tent or VTile.Stele or VTile.Gate or VTile.Door or VTile.Pine;

    /// <summary>The name of the area at a world point (the banner).</summary>
    public static string AreaName(Vector2 p)
    {
        float tx = p.X / TS, ty = p.Y / TS;
        if (ty < 13) return "Acampamento dos Discípulos";
        if (ty < 20) return "Ponte do Rio Nublado";
        if (tx < 21) return "Além da Fenda";
        if (tx >= 45 && tx < 57 && ty >= 21 && ty < 31) return "Túmulo da Mestra";
        if (tx > 36) return "Vale de Bambus";
        return "Trilha da Montanha";
    }
}
