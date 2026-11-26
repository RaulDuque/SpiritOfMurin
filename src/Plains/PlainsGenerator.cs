using System;
using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Plains;

/// <summary>What a ground tile is.</summary>
public enum TileKind { Grass, Bank, Water, Ford, BushGrass, Mud }

/// <summary>The habitat a tile belongs to; decides which objects and animals it holds.</summary>
public enum Habitat { Grass, Bank, Water, Bushes, Mud }

/// <summary>The kinds of generated objects.</summary>
public enum FeatureKind { Flint, Branch, Pine, Boulder, Bush, Burrow, Rooted, Herb, Root, Boar }

/// <summary>
/// One generated object. <see cref="Id"/> is stable for a seed: the same kind on the same tile
/// always has the same id, whatever order the chunks were generated in, so world state (what was
/// collected, which animal died) can key on it.
/// </summary>
/// <param name="Id">Stable id, "kind@tx,ty".</param>
/// <param name="Kind">What it is.</param>
/// <param name="Tile">The tile that owns it (decides its chunk).</param>
/// <param name="Position">World position in pixels (its foot).</param>
/// <param name="Variation">Seeded number for small visual variations.</param>
public readonly record struct Feature(string Id, FeatureKind Kind, Vector2I Tile, Vector2 Position, int Variation);

/// <summary>The ground and objects of one chunk.</summary>
public sealed class ChunkData
{
    /// <summary>Chunk coordinates.</summary>
    public Vector2I Coord { get; init; }
    /// <summary>Side in tiles.</summary>
    public int Size { get; init; }
    /// <summary>Tiles, row by row (index = y * Size + x, local coordinates).</summary>
    public TileKind[] Tiles { get; init; } = Array.Empty<TileKind>();
    /// <summary>Objects whose owning tile is inside this chunk.</summary>
    public List<Feature> Features { get; } = new();
    /// <summary>The tile at local coordinates.</summary>
    public TileKind this[int lx, int ly] => Tiles[ly * Size + lx];
}

/// <summary>
/// Generates the Planície Verdejante from a seed. Every query is a pure function of
/// (seed, world tile), never of what was generated before, so chunks match across their borders and
/// come out identical in any visiting order. The waking spot is tile (0, 0); a creek runs to its east
/// with flint and branches on the bank; a bush field with burrows and a mud flat with boars and
/// rooted earth are guaranteed within reach to the west. Handmade places (towns, tombs) can later be
/// stamped in through <see cref="AddStamp"/>, which claims tiles before the random rules run.
/// </summary>
public sealed class PlainsGenerator
{
    /// <summary>The world seed.</summary>
    public int Seed { get; }

    /// <summary>The rules in use.</summary>
    public PlainsGenSettings Settings { get; }

    /// <summary>Centre of the guaranteed bush field, in tiles.</summary>
    public Vector2 BushAnchor { get; }

    /// <summary>Centre of the guaranteed mud flat, in tiles.</summary>
    public Vector2 MudAnchor { get; }

    /// <summary>The waking spot in world pixels (centre of tile 0, 0).</summary>
    public Vector2 StartPixel => new(Settings.TileSize * 0.5f, Settings.TileSize * 0.5f);

    private readonly float _phase1, _phase2;
    private readonly int _fordPhase;
    private readonly Dictionary<Vector2I, List<(FeatureKind Kind, Vector2 Offset)>> _forced = new();
    private readonly Dictionary<Vector2I, TileKind> _stampTiles = new();

    /// <summary>Creates a generator for a seed and a rule set.</summary>
    public PlainsGenerator(int seed, PlainsGenSettings settings)
    {
        Seed = seed;
        Settings = settings;
        _phase1 = Rand01(0, 0, 101) * Mathf.Tau;
        _phase2 = Rand01(0, 0, 102) * Mathf.Tau;
        _fordPhase = (int)(Hash(seed, 0, 0, 103) % (uint)Math.Max(1, settings.FordSpacing));
        BushAnchor = Anchor(settings.BushAnchorAngle, 104);
        MudAnchor = Anchor(settings.MudAnchorAngle, 106);
        PlaceStartGuarantees();
    }

    // ------------------------------------------------------------------ hashing and noise

    /// <summary>Deterministic 32-bit hash of a seed, a tile and a salt.</summary>
    public static uint Hash(int seed, int x, int y, int salt)
    {
        unchecked
        {
            uint h = (uint)seed * 0x9E3779B1u ^ (uint)x * 0x85EBCA77u ^ (uint)y * 0xC2B2AE3Du ^ (uint)salt * 0x27D4EB2Fu;
            h ^= h >> 15; h *= 0x2C1B3C6Du; h ^= h >> 12; h *= 0x297A2D39u; h ^= h >> 15;
            return h;
        }
    }

    private float Rand01(int x, int y, int salt) => (Hash(Seed, x, y, salt) >> 8) / 16777216f;

    /// <summary>Smooth value noise in [0, 1) at a world position (in noise units).</summary>
    public float Noise(float x, float y, int salt)
    {
        int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y);
        float fx = x - ix, fy = y - iy;
        fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
        float a = Rand01(ix, iy, salt), b = Rand01(ix + 1, iy, salt), c = Rand01(ix, iy + 1, salt), d = Rand01(ix + 1, iy + 1, salt);
        return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
    }

    private Vector2 Anchor(float angleDeg, int salt)
    {
        float ang = Mathf.DegToRad(angleDeg + (Rand01(0, 0, salt) * 2 - 1) * Settings.AnchorAngleJitter);
        float dist = Mathf.Lerp(Settings.AnchorDistanceMin, Settings.AnchorDistanceMax, Rand01(0, 0, salt + 1));
        return new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * dist;
    }

    // ------------------------------------------------------------------ ground

    /// <summary>Centre line of the creek (in tiles, x) at a tile row. Passes <c>CreekOffset</c> at row 0.</summary>
    public float CreekX(int ty)
    {
        var s = Settings;
        // the meander is calmer near the waking spot, so the creek never swings into the start area
        float calm = 0.35f + 0.65f * Mathf.Clamp(Mathf.Abs(ty) / 30f, 0f, 1f);
        return s.CreekOffset + calm * (s.CreekMeander1 * (Mathf.Sin(ty * s.CreekFreq1 + _phase1) - Mathf.Sin(_phase1))
                                     + s.CreekMeander2 * (Mathf.Sin(ty * s.CreekFreq2 + _phase2) - Mathf.Sin(_phase2)));
    }

    /// <summary>True on the rows where the creek has a ford (shallow, walkable).</summary>
    public bool IsFordRow(int ty) => ((ty - _fordPhase) % Settings.FordSpacing + Settings.FordSpacing) % Settings.FordSpacing < Settings.FordLength;

    private float StartDistance(int tx, int ty) => new Vector2(tx + 0.5f, ty + 0.5f).Length();

    /// <summary>The ground at a world tile.</summary>
    public TileKind TileAt(int tx, int ty)
    {
        if (_stampTiles.TryGetValue(new Vector2I(tx, ty), out var stamped)) return stamped;
        var s = Settings;
        float dx = Mathf.Abs(tx + 0.5f - CreekX(ty));
        if (dx < s.CreekHalfWidth) return IsFordRow(ty) ? TileKind.Ford : TileKind.Water;
        if (dx < s.CreekHalfWidth + s.BankWidth) return TileKind.Bank;
        if (StartDistance(tx, ty) < s.ClearingRadius + 2f) return TileKind.Grass;
        var p = new Vector2(tx + 0.5f, ty + 0.5f);
        float edge = 0.8f + 0.4f * Noise(tx * 0.35f, ty * 0.35f, 7);
        if (p.DistanceTo(MudAnchor) < s.AnchorRadius * edge) return TileKind.Mud;
        if (p.DistanceTo(BushAnchor) < s.AnchorRadius * edge) return TileKind.BushGrass;
        // away from the start, the habitats follow the noise
        float far = Mathf.Clamp((StartDistance(tx, ty) - s.AnchorDistanceMax - s.AnchorRadius) / 6f, 0f, 1f);
        if (far > 0)
        {
            if (Noise(tx * s.HabitatScale, ty * s.HabitatScale, 1) * far > s.MudThreshold * far + (1 - far)) return TileKind.Mud;
            if (Noise(tx * s.HabitatScale, ty * s.HabitatScale, 2) * far > s.BushThreshold * far + (1 - far)) return TileKind.BushGrass;
        }
        return TileKind.Grass;
    }

    /// <summary>The habitat a tile kind belongs to.</summary>
    public static Habitat HabitatOf(TileKind k) => k switch
    {
        TileKind.Water or TileKind.Ford => Habitat.Water,
        TileKind.Bank => Habitat.Bank,
        TileKind.BushGrass => Habitat.Bushes,
        TileKind.Mud => Habitat.Mud,
        _ => Habitat.Grass,
    };

    /// <summary>The habitat at a world tile.</summary>
    public Habitat HabitatAt(int tx, int ty) => HabitatOf(TileAt(tx, ty));

    /// <summary>True for the ground a walker can stand on (everything but deep water).</summary>
    public static bool IsWalkableGround(TileKind k) => k != TileKind.Water;

    // ------------------------------------------------------------------ objects

    /// <summary>
    /// Claims a tile for a handmade place: its ground and, optionally, objects. Claimed tiles skip the
    /// random rules. Call before generating any chunk (the generator stays a pure function of its inputs).
    /// </summary>
    public void AddStamp(Vector2I tile, TileKind ground, IEnumerable<(FeatureKind Kind, Vector2 Offset)>? objects = null)
    {
        _stampTiles[tile] = ground;
        _forced[tile] = new List<(FeatureKind, Vector2)>(objects ?? Array.Empty<(FeatureKind, Vector2)>());
    }

    private void Force(int tx, int ty, FeatureKind kind, Vector2 offset)
    {
        var key = new Vector2I(tx, ty);
        if (!_forced.TryGetValue(key, out var list)) _forced[key] = list = new List<(FeatureKind, Vector2)>();
        list.Add((kind, offset));
    }

    // The first materials, a herb by a stone, burrows and a boar with its rooted earth, always within reach.
    private void PlaceStartGuarantees()
    {
        int BankTile(int ty) => Mathf.FloorToInt(CreekX(ty) - Settings.CreekHalfWidth - 1f);
        Vector2 mid = new(16, 20);
        Force(BankTile(-1), -1, FeatureKind.Flint, mid);
        Force(BankTile(-4), -4, FeatureKind.Flint, mid);
        Force(BankTile(2), 2, FeatureKind.Branch, mid);
        Force(BankTile(-6) - 1, -6, FeatureKind.Branch, mid);
        Force(BankTile(-7), -7, FeatureKind.Pine, new Vector2(16, 26));
        Force(BankTile(5), 5, FeatureKind.Boulder, new Vector2(10, 26));
        Force(BankTile(5), 5, FeatureKind.Herb, new Vector2(26, 28));
        var b = new Vector2I(Mathf.RoundToInt(BushAnchor.X), Mathf.RoundToInt(BushAnchor.Y));
        Force(b.X, b.Y, FeatureKind.Burrow, mid);
        Force(b.X + 3, b.Y + 1, FeatureKind.Burrow, mid);
        Force(b.X - 2, b.Y - 2, FeatureKind.Bush, new Vector2(16, 26));
        Force(b.X + 1, b.Y - 2, FeatureKind.Bush, new Vector2(16, 26));
        Force(b.X - 3, b.Y + 2, FeatureKind.Boulder, new Vector2(10, 26));
        Force(b.X - 3, b.Y + 2, FeatureKind.Herb, new Vector2(26, 28));
        var m = new Vector2I(Mathf.RoundToInt(MudAnchor.X), Mathf.RoundToInt(MudAnchor.Y));
        Force(m.X, m.Y, FeatureKind.Boar, mid);
        Force(m.X + 2, m.Y + 1, FeatureKind.Rooted, mid);
        Force(m.X + 2, m.Y + 1, FeatureKind.Root, new Vector2(20, 18));
        Force(m.X - 2, m.Y + 2, FeatureKind.Rooted, mid);
        Force(m.X - 2, m.Y + 2, FeatureKind.Root, new Vector2(12, 22));
        Force(m.X + 1, m.Y - 2, FeatureKind.Rooted, mid);
    }

    private static string Short(FeatureKind k) => k.ToString().ToLowerInvariant();

    /// <summary>The objects whose owning tile is (tx, ty).</summary>
    public IEnumerable<Feature> FeaturesAt(int tx, int ty)
    {
        int ts = Settings.TileSize;
        var origin = new Vector2(tx * ts, ty * ts);
        int variation = (int)(Hash(Seed, tx, ty, 30) % 1000);
        if (_forced.TryGetValue(new Vector2I(tx, ty), out var forced))
        {
            foreach (var (kind, off) in forced)
                yield return new Feature($"{Short(kind)}@{tx},{ty}", kind, new Vector2I(tx, ty), origin + off, variation);
            yield break;
        }
        var tile = TileAt(tx, ty);
        if (!IsWalkableGround(tile) || tile == TileKind.Ford || StartDistance(tx, ty) < Settings.ClearingRadius) yield break;
        var ch = Settings.For(HabitatOf(tile));
        float r = Rand01(tx, ty, 10);
        FeatureKind? pick = null;
        foreach (var (kind, chance) in new[] {
            (FeatureKind.Flint, ch.Flint), (FeatureKind.Branch, ch.Branch), (FeatureKind.Pine, ch.Pine), (FeatureKind.Boulder, ch.Boulder),
            (FeatureKind.Bush, ch.Bush), (FeatureKind.Burrow, ch.Burrow), (FeatureKind.Rooted, ch.Rooted), (FeatureKind.Boar, ch.Boar) })
        {
            if (r < chance) { pick = kind; break; }
            r -= chance;
        }
        if (pick is not { } k) yield break;
        var pos = origin + new Vector2(6 + Rand01(tx, ty, 11) * 14, 14 + Rand01(tx, ty, 12) * 14);
        yield return new Feature($"{Short(k)}@{tx},{ty}", k, new Vector2I(tx, ty), pos, variation);
        if (k == FeatureKind.Boulder && Rand01(tx, ty, 13) < Settings.HerbNearBoulder)
            yield return new Feature($"herb@{tx},{ty}", FeatureKind.Herb, new Vector2I(tx, ty), pos + new Vector2(15, 3), variation);
        if (k == FeatureKind.Rooted && Rand01(tx, ty, 14) < Settings.RootInRooted)
            yield return new Feature($"root@{tx},{ty}", FeatureKind.Root, new Vector2I(tx, ty), pos + new Vector2(3, -2), variation);
    }

    // ------------------------------------------------------------------ chunks

    /// <summary>The chunk that owns a world tile.</summary>
    public Vector2I ChunkOfTile(int tx, int ty) => new(Mathf.FloorToInt((float)tx / Settings.ChunkTiles), Mathf.FloorToInt((float)ty / Settings.ChunkTiles));

    /// <summary>The chunk under a world pixel.</summary>
    public Vector2I ChunkOfPixel(Vector2 p) => ChunkOfTile(Mathf.FloorToInt(p.X / Settings.TileSize), Mathf.FloorToInt(p.Y / Settings.TileSize));

    /// <summary>The world tile under a world pixel.</summary>
    public Vector2I TileOfPixel(Vector2 p) => new(Mathf.FloorToInt(p.X / Settings.TileSize), Mathf.FloorToInt(p.Y / Settings.TileSize));

    /// <summary>Generates one chunk: its tiles and the objects its tiles own.</summary>
    public ChunkData GenerateChunk(Vector2I chunk)
    {
        int n = Settings.ChunkTiles;
        var data = new ChunkData { Coord = chunk, Size = n, Tiles = new TileKind[n * n] };
        for (int ly = 0; ly < n; ly++)
            for (int lx = 0; lx < n; lx++)
            {
                int tx = chunk.X * n + lx, ty = chunk.Y * n + ly;
                data.Tiles[ly * n + lx] = TileAt(tx, ty);
                data.Features.AddRange(FeaturesAt(tx, ty));
            }
        return data;
    }
}
