using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Vale;

/// <summary>
/// Builds the Vale's ground, its tall pieces (y-sorted with the figures) and its colliders from a
/// <see cref="ValeMap"/>, with the prototype's drawing (index.html drawGround / drawTall). The gate and
/// the tomb door are separate bodies so they can open. Gaps (the chasm) sit on
/// <see cref="Characters.Player.GapLayer"/>: walls for walking, crossable by a side step or a dash.
/// </summary>
public static class ValeTerrain
{
    internal static readonly Color Ground = new("#4c5a3e"), Ground2 = new("#526142"), Tuft = new("#65774c"), TuftDark = new("#3d4a33"), Bloom = new("#8e9a6c"),
        Path = new("#86704f"), Path2 = new("#77623f"), Pebble = new("#9c8763"), Camp = new("#77684a"), Camp2 = new("#6b5d41"),
        RockTop = new("#8a877a"), RockHi = new("#a3a091"), RockFace = new("#5d5b51"), RockDark = new("#3b3a34"),
        Water = new("#2c5a5e"), Water2 = new("#336669"), WaterHi = new("#78aca7"), Chasm = new("#090a0b"), ChasmFace = new("#2a2924"), ChasmHi = new("#4a473e"),
        Bridge = new("#7a5a3a"), Bridge2 = new("#5d432d"), Rail = new("#4b3524"), TFloor = new("#3a3a3f"), TFloor2 = new("#323237"), TLine = new("#28282c"),
        TWall = new("#6c6b72"), TWallHi = new("#83828a"), TWallFace = new("#48474e"), Bamboo = new("#5f8f4a"), BambooHi = new("#7fae5d"), BambooDark = new("#3b6332"), Leaf = new("#4d7a3c"),
        Tent = new("#8f3d2c"), Tent2 = new("#74301f"), TentRoof = new("#c4ab80"), Learn = new("#d4fbf6"), LearnGlow = new("#6fd8d2"), Amber = new("#e2a640"), AmberHi = new("#ffd27a"),
        Outline = new("#16140f");

    /// <summary>The prototype's hash (index.html hash).</summary>
    public static uint H(int x, int y)
    {
        unchecked
        {
            int h = x * 374761393 + y * 668265263;
            uint u = (uint)h;
            u = (u ^ (u >> 13)) * 1274126177u;
            return u ^ (u >> 16);
        }
    }

    /// <summary>The bodies built for the world, so the scene can open the gate and the door.</summary>
    public sealed class Built
    {
        /// <summary>The palisade gate (removed when it opens).</summary>
        public StaticBody2D Gate = null!;
        /// <summary>The sealed tomb door (removed when it opens).</summary>
        public StaticBody2D Door = null!;
        /// <summary>The ground drawing (redrawn when the gate opens).</summary>
        public ValeGround Ground = null!;
        /// <summary>Tall pieces by tile (the gate and door pieces redraw when they open).</summary>
        public readonly Dictionary<Vector2I, ValeTall> Tall = new();
    }

    /// <summary>Builds everything under <paramref name="ground"/> (z −10) and <paramref name="ysorted"/>.</summary>
    public static Built Build(ValeMap map, Node2D ground, Node2D ysorted, Node2D bodies, ValeState state)
    {
        var b = new Built();
        b.Ground = new ValeGround { Map = map, State = state, ZIndex = -10, Name = "ValeGround" };
        ground.AddChild(b.Ground);
        var water = new ValeWater { Map = map, ZIndex = -9, Name = "ValeWater" };
        ground.AddChild(water);
        for (int ty = 0; ty < ValeMap.MH; ty++)
            for (int tx = 0; tx < ValeMap.MW; tx++)
            {
                var t = map.At(tx, ty);
                if (!ValeMap.IsTall(t)) continue;
                var n = new ValeTall { Map = map, State = state, Tx = tx, Ty = ty, Kind = t, Position = new Vector2(tx * ValeMap.TS + 16, (ty + 1) * ValeMap.TS - 1), Name = $"Tall_{tx}_{ty}" };
                ysorted.AddChild(n);
                b.Tall[new Vector2I(tx, ty)] = n;
            }
        // colliders: full tiles merged by row; tall pieces that only touch the ground at their base get a base box
        var solid = new StaticBody2D { Name = "Solid" };
        var gap = new StaticBody2D { Name = "Gap", CollisionLayer = Characters.Player.GapLayer, CollisionMask = 0 };
        b.Gate = new StaticBody2D { Name = "Gate" };
        b.Door = new StaticBody2D { Name = "Door" };
        void Box(StaticBody2D body, float x, float y, float w, float h) =>
            body.AddChild(new CollisionShape2D { Position = new Vector2(x + w / 2, y + h / 2), Shape = new RectangleShape2D { Size = new Vector2(w, h) } });
        for (int ty = 0; ty < ValeMap.MH; ty++)
        {
            int runStart = -1; StaticBody2D? runBody = null;
            for (int tx = 0; tx <= ValeMap.MW; tx++)
            {
                var t = tx < ValeMap.MW ? map.At(tx, ty) : VTile.Grass;
                StaticBody2D? body = null;
                if (tx < ValeMap.MW)
                {
                    if (t is VTile.Rock && !map.IsBoulder(tx, ty) || t is VTile.TombWall or VTile.CutWall or VTile.Tent or VTile.Stele or VTile.Water) body = solid;
                    else if (t == VTile.Chasm) body = gap;
                    else if (t == VTile.Gate) body = b.Gate;
                    else if (t == VTile.Door) body = b.Door;
                }
                if (body != runBody)
                {
                    if (runBody != null) Box(runBody, runStart * ValeMap.TS, ty * ValeMap.TS, (tx - runStart) * ValeMap.TS, ValeMap.TS);
                    runBody = body; runStart = tx;
                }
                if (tx >= ValeMap.MW) continue;
                float X = tx * ValeMap.TS, Y = ty * ValeMap.TS;
                if (t == VTile.Pine) Box(solid, X + 16 - 7, Y + 32 - 11, 14, 11);
                else if (t == VTile.Bamboo) Box(solid, X, Y + 32 - 7, 32, 7);
                else if (t == VTile.Rock && map.IsBoulder(tx, ty)) Box(solid, X + 16 - 12, Y + 32 - 13, 24, 13);
            }
        }
        bodies.AddChild(solid); bodies.AddChild(gap); bodies.AddChild(b.Gate); bodies.AddChild(b.Door);
        return b;
    }
}

/// <summary>The Vale's ground: grass, path, camp, chasm, bridge and tomb floor (drawn once; redrawn when the gate opens).</summary>
public partial class ValeGround : Node2D
{
    /// <summary>The map.</summary>
    public ValeMap Map { get; set; } = null!;
    /// <summary>The story state (gate open).</summary>
    public ValeState State { get; set; } = null!;

    private void R(float x, float y, float w, float h, Color c) => DrawRect(new Rect2(x, y, w, h), c);

    private static VTile Under(VTile t, int ty) => t switch
    {
        VTile.TombWall or VTile.CutWall or VTile.Door => VTile.TombFloor,
        VTile.Tent or VTile.Gate => VTile.Camp,
        _ when ValeMap.IsTall(t) => ty < 13 ? VTile.Camp : VTile.Grass,
        _ => t,
    };

    /// <inheritdoc />
    public override void _Draw()
    {
        const int TS = ValeMap.TS;
        for (int ty = 0; ty < ValeMap.MH; ty++)
            for (int tx = 0; tx < ValeMap.MW; tx++)
            {
                var t = Under(Map.At(tx, ty), ty);
                float X = tx * TS, Y = ty * TS;
                uint hv = ValeTerrain.H(tx, ty);
                switch (t)
                {
                    case VTile.Grass:
                        R(X, Y, TS, TS, ValeTerrain.Ground);
                        if (hv % 3 == 0) R(X + hv % 18, Y + (hv >> 6) % 20, 10 + hv % 8, 6 + (hv >> 3) % 6, ValeTerrain.Ground2);
                        for (int i = 0; i < 5; i++)
                        {
                            uint h2 = ValeTerrain.H(tx * 7 + i, ty * 13 - i); if (h2 % 3 == 2) continue;
                            float px = X + h2 % 29, py = Y + 3 + (h2 >> 5) % 27;
                            R(px - 1, py + 3, 5, 1, ValeTerrain.TuftDark);
                            R(px, py, 1, 3, ValeTerrain.Tuft); R(px + 2, py + 1, 1, 2, ValeTerrain.Tuft); R(px - 1, py + 1, 1, 2, ValeTerrain.Tuft);
                        }
                        if (hv % 13 == 0) { R(X + 12, Y + 20, 2, 2, ValeTerrain.Bloom); R(X + 13, Y + 19, 1, 1, new Color("#d6d8c4")); }
                        break;
                    case VTile.Path:
                        R(X, Y, TS, TS, ValeTerrain.Path);
                        for (int i = 0; i < 4; i++) { uint h2 = ValeTerrain.H(tx + i * 31, ty); R(X + h2 % 30, Y + (h2 >> 6) % 30, 2, 1, ValeTerrain.Path2); }
                        R(X + hv % 28, Y + (hv >> 8) % 28, 2, 2, ValeTerrain.Pebble);
                        bool G(int a, int bb) { var n = Map.At(tx + a, ty + bb); return n is VTile.Grass or VTile.Pine or VTile.Bamboo or VTile.Rock; }
                        for (int i = 0; i < TS; i += 2)
                        {
                            uint h3 = ValeTerrain.H(tx * 31 + i, ty * 17); float d = 1 + h3 % 4;
                            if (G(0, -1)) R(X + i, Y, 2, d, ValeTerrain.Ground);
                            if (G(0, 1)) R(X + i, Y + TS - d, 2, d, ValeTerrain.Ground);
                            if (G(-1, 0)) R(X, Y + i, d, 2, ValeTerrain.Ground);
                            if (G(1, 0)) R(X + TS - d, Y + i, d, 2, ValeTerrain.Ground);
                        }
                        break;
                    case VTile.Camp:
                        R(X, Y, TS, TS, (hv & 1) == 1 ? ValeTerrain.Camp : ValeTerrain.Camp2);
                        R(X + hv % 26, Y + (hv >> 7) % 26, 3, 1, ValeTerrain.Path2);
                        break;
                    case VTile.Chasm:
                        R(X, Y, TS, TS, ValeTerrain.Chasm);
                        if (Map.At(tx, ty - 1) != VTile.Chasm) { R(X, Y, TS, 10, ValeTerrain.ChasmFace); R(X, Y, TS, 2, ValeTerrain.ChasmHi); }
                        if (Map.At(tx - 1, ty) != VTile.Chasm) R(X, Y, 3, TS, ValeTerrain.ChasmFace);
                        if (Map.At(tx + 1, ty) != VTile.Chasm) R(X + TS - 3, Y, 3, TS, ValeTerrain.ChasmFace);
                        break;
                    case VTile.Bridge:
                        R(X, Y, TS, TS, ValeTerrain.Bridge);
                        for (int i = 0; i < TS; i += 6) R(X, Y + i, TS, 1, ValeTerrain.Bridge2);
                        if (Map.At(tx - 1, ty) != VTile.Bridge) R(X, Y, 3, TS, ValeTerrain.Rail);
                        if (Map.At(tx + 1, ty) != VTile.Bridge) R(X + TS - 3, Y, 3, TS, ValeTerrain.Rail);
                        break;
                    case VTile.TombFloor:
                        R(X, Y, TS, TS, ((tx + ty) & 1) == 1 ? ValeTerrain.TFloor : ValeTerrain.TFloor2);
                        R(X, Y, TS, 1, ValeTerrain.TLine); R(X, Y, 1, TS, ValeTerrain.TLine);
                        break;
                }
            }
        if (State.GateOpen) // the broken palisade lies aside
        {
            float X = 27 * TS, Y = 13 * TS; var c = new Color("#5a3f28");
            R(X - 14, Y + 20, 18, 4, c); R(X + 4 * TS - 2, Y + 14, 16, 4, c); R(X + 4 * TS + 2, Y + 22, 12, 3, c);
        }
    }
}

/// <summary>The Rio Nublado: animated water with a rock edge where it meets land.</summary>
public partial class ValeWater : Node2D
{
    /// <summary>The map.</summary>
    public ValeMap Map { get; set; } = null!;
    private float _t;

    /// <inheritdoc />
    public override void _Process(double delta) { _t += (float)delta; QueueRedraw(); }

    /// <inheritdoc />
    public override void _Draw()
    {
        const int TS = ValeMap.TS;
        for (int ty = 16; ty <= 20; ty++)
            for (int tx = 0; tx < ValeMap.MW; tx++)
            {
                var t = Map.At(tx, ty);
                if (t is not (VTile.Water or VTile.Bridge)) continue;
                float X = tx * TS, Y = ty * TS; uint hv = ValeTerrain.H(tx, ty);
                if (t == VTile.Bridge) { DrawRect(new Rect2(X, Y, TS, TS), ValeTerrain.Water); continue; }
                DrawRect(new Rect2(X, Y, TS, TS), ((tx + ty) & 1) == 1 ? ValeTerrain.Water : ValeTerrain.Water2);
                int ph = Mathf.FloorToInt(_t * 2 + hv % 5);
                if (ph % 3 == 0) DrawRect(new Rect2(X + hv % 20, Y + 8 + (hv >> 4) % 16, 7, 1), ValeTerrain.WaterHi);
                if (ph % 4 == 1) DrawRect(new Rect2(X + (hv >> 3) % 22, Y + 4 + (hv >> 9) % 20, 5, 1), ValeTerrain.WaterHi);
                if (Map.At(tx, ty - 1) is not (VTile.Water or VTile.Bridge)) { DrawRect(new Rect2(X, Y, TS, 5), ValeTerrain.RockFace); DrawRect(new Rect2(X, Y + 5, TS, 1), ValeTerrain.WaterHi); }
            }
    }
}

/// <summary>One standing tile (pine, boulder, rock wall, tomb wall, bamboo, tent, stele, gate, door), origin at its foot.</summary>
public partial class ValeTall : Node2D
{
    /// <summary>The map.</summary>
    public ValeMap Map { get; set; } = null!;
    /// <summary>The story state (gate, door, slab, stele).</summary>
    public ValeState State { get; set; } = null!;
    /// <summary>Tile coordinates.</summary>
    public int Tx { get; set; }
    /// <summary>Tile coordinates.</summary>
    public int Ty { get; set; }
    /// <summary>What it is.</summary>
    public VTile Kind { get; set; }
    private float _t;

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _t += (float)delta;
        if (Kind is VTile.Pine or VTile.Bamboo or VTile.Stele || Kind == VTile.CutWall && Tx == 51) QueueRedraw();
    }

    private void R(float x, float y, float w, float h, Color c) => DrawRect(new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), c);

    /// <inheritdoc />
    public override void _Draw()
    {
        const int TS = ValeMap.TS;
        float X = -16, Y = -TS + 1; // the tile's top-left, relative to the foot
        uint hv = ValeTerrain.H(Tx, Ty);
        var t = Kind;
        if (t == VTile.Pine)
        {
            float cx0 = X + 16, by = Y + TS - 3;
            var sh = new Color(8 / 255f, 10 / 255f, 8 / 255f, 0.32f);
            R(cx0 - 15, by - 3, 30, 6, sh); R(cx0 - 11, by - 5, 22, 9, sh);
            R(cx0 - 3, by - 12, 6, 13, ValeTerrain.Outline); R(cx0 - 2, by - 12, 4, 12, new Color("#5a3e28")); R(cx0 + 1, by - 12, 1, 12, new Color("#3e2a1a"));
            int sway = Mathf.RoundToInt(Mathf.Sin(_t * 1.2f + hv) * 1);
            for (int pass = 0; pass < 2; pass++)
                for (int i = 0; i < 4; i++)
                {
                    float w = 17 - i * 3 + hv % 3, top = by - 24 - i * 11; const int rows = 15;
                    for (int r = 0; r < rows; r++)
                    {
                        float rw = Mathf.Max(1, Mathf.Round(w * (r + 1) / rows)), xx = cx0 - rw + (i > 1 ? sway : 0), yy = top + r;
                        if (pass == 0) { R(xx - 1, yy - 1, rw * 2 + 2, 3, ValeTerrain.Outline); continue; }
                        R(xx, yy, rw * 2, 1, new Color("#2f5a3a")); R(xx + rw, yy, rw, 1, new Color("#23462d"));
                        R(xx + 1, yy, Mathf.Max(1, Mathf.Floor(rw / 3)), 1, new Color("#4c7c4a"));
                        if (r == rows - 1) R(xx, yy, rw * 2, 1, new Color("#1d3a25"));
                    }
                }
            return;
        }
        if (t == VTile.Rock && Map.IsBoulder(Tx, Ty))
        {
            float cx0 = X + 16, by = Y + TS - 2, ht = 24 + hv % 6, hw = 13 + hv % 4;
            R(cx0 - hw - 2, by - 3, hw * 2 + 4, 5, new Color(8 / 255f, 10 / 255f, 8 / 255f, 0.35f));
            for (int pass = 0; pass < 2; pass++)
                for (int r = 0; r < ht; r++)
                {
                    float k = (r - ht * 0.55f) / (ht * 0.55f), w = Mathf.Round(hw * Mathf.Sqrt(Mathf.Max(0, 1 - k * k * (k < 0 ? 1 : 0.35f)))), yy = by - ht + r;
                    if (pass == 0) { R(cx0 - w - 1, yy - 1, w * 2 + 2, 3, ValeTerrain.Outline); continue; }
                    R(cx0 - w, yy, w * 2, 1, r < ht * 0.25f ? ValeTerrain.RockHi : r < ht * 0.65f ? ValeTerrain.RockTop : ValeTerrain.RockFace);
                    R(cx0 + w - 3, yy, 3, 1, r < ht * 0.65f ? ValeTerrain.RockFace : ValeTerrain.RockDark);
                }
            R(cx0 - 4 + hv % 5, by - ht + 8, 1, 6, ValeTerrain.RockDark); R(cx0 - 3 + hv % 5, by - ht + 13, 4, 1, ValeTerrain.RockDark); R(cx0 - 7, by - ht + 4, 5, 1, ValeTerrain.RockHi);
            return;
        }
        bool wallish(VTile k) => k is VTile.TombWall or VTile.CutWall or VTile.Door;
        if (t is VTile.Rock or VTile.TombWall or VTile.CutWall || t == VTile.Door && !State.DoorOpen)
        {
            bool wall = wallish(t);
            Color top = wall ? ValeTerrain.TWall : ValeTerrain.RockTop, hi = wall ? ValeTerrain.TWallHi : ValeTerrain.RockHi, face = wall ? ValeTerrain.TWallFace : ValeTerrain.RockFace;
            bool sameS = wall ? wallish(Map.At(Tx, Ty + 1)) : Map.At(Tx, Ty + 1) == VTile.Rock;
            bool sameN = wall ? wallish(Map.At(Tx, Ty - 1)) : Map.At(Tx, Ty - 1) == VTile.Rock;
            R(X, Y - 10, TS, TS, top);
            if (!sameN) R(X, Y - 10, TS, 2, hi);
            if (wall) { if (((Tx + Ty) & 1) == 1) R(X + 15, Y - 8, 1, TS - 4, ValeTerrain.TWallFace); R(X, Y + 6, TS, 1, ValeTerrain.TWallFace); }
            else { R(X + hv % 20 + 4, Y - 4 + (hv >> 5) % 14, 6, 1, ValeTerrain.RockDark); R(X + (hv >> 3) % 20 + 2, Y + 2 + (hv >> 7) % 10, 1, 4, ValeTerrain.RockDark); R(X + (hv >> 2) % 22 + 3, Y - 6 + (hv >> 9) % 8, 4, 1, hi); }
            if (!sameS)
            {
                R(X, Y + TS - 10, TS, 10, face); R(X, Y + TS - 1, TS, 1, ValeTerrain.RockDark);
                if (wall) { for (int i = (Ty & 1) * 4; i < TS; i += 8) R(X + i, Y + TS - 10, 1, 10, ValeTerrain.TLine); R(X, Y + TS - 5, TS, 1, ValeTerrain.TLine); }
            }
            if (t == VTile.Door)
            {
                R(X + (Tx == 50 ? 4 : 0), Y - 6, 28, TS + 4, new Color("#55545b"));
                if (Tx == 50) R(X + TS - 1, Y - 6, 1, TS + 4, ValeTerrain.TLine);
                if (Tx == 51)
                {
                    var c = State.HasManual ? ValeTerrain.AmberHi : ValeTerrain.Amber;
                    R(X - 6, Y + 4, 6, 2, c); R(X - 8, Y + 6, 2, 6, c); R(X - 6, Y + 12, 6, 2, c); R(X, Y + 4, 6, 10, new Color("#3e3d43"));
                }
            }
            if (t == VTile.CutWall && Tx == 51)
            {
                float X2 = X - TS; bool lit = State.SlabRead || State.LearnedSide; float k = lit ? 0.6f + 0.4f * Mathf.Sin(_t * 3) : 0;
                for (int i = 0; i < 40; i++)
                {
                    float u = i / 39f, px = X2 + 12 + i, py = Y - 4 + Mathf.Floor(u * 16 - Mathf.Sin(u * Mathf.Pi) * 9);
                    R(px, py, 1, 2, lit ? (k > 0.8f ? ValeTerrain.Learn : ValeTerrain.LearnGlow) : new Color("#4b4a51"));
                }
                if (lit) R(X2 + 10, Y - 7, 44, 24, new Color(111 / 255f, 216 / 255f, 210 / 255f, 0.18f));
            }
        }
        else if (t == VTile.Bamboo)
        {
            for (int i = 0; i < 3; i++)
            {
                uint h2 = ValeTerrain.H(Tx * 3 + i, Ty); float bx = X + 3 + i * 10 + h2 % 4, ht = 34 + h2 % 14;
                R(bx + 1, Y + TS - 3, 5, 3, ValeTerrain.BambooDark);
                R(bx, Y + TS - ht, 3, ht, ValeTerrain.Bamboo); R(bx, Y + TS - ht, 1, ht, ValeTerrain.BambooHi);
                for (float n = Y + TS - 8; n > Y + TS - ht; n -= 9) R(bx, n, 3, 1, ValeTerrain.BambooDark);
                float sway = Mathf.Round(Mathf.Sin(_t * 1.5f + h2) * 1.5f);
                R(bx - 4 + sway, Y + TS - ht + 2, 5, 2, ValeTerrain.Leaf); R(bx + 2 + sway, Y + TS - ht + 6, 6, 2, ValeTerrain.Leaf); R(bx - 3 + sway, Y + TS - ht + 11, 4, 1, ValeTerrain.Leaf);
            }
        }
        else if (t == VTile.Tent)
        {
            bool n = Map.At(Tx, Ty - 1) == VTile.Tent, so = Map.At(Tx, Ty + 1) == VTile.Tent;
            R(X, Y - 8, TS, TS + 8, ValeTerrain.Tent);
            for (int i = 0; i < TS; i += 8) R(X + i, Y - 8, 3, TS + 8, ValeTerrain.Tent2);
            if (!n) { R(X, Y - 12, TS, 5, ValeTerrain.TentRoof); R(X, Y - 8, TS, 1, new Color("#9e8964")); }
            if (!so) { R(X, Y + TS - 8, TS, 8, new Color("#5a2418")); if ((Tx & 1) == 0) R(X + 10, Y + TS - 14, 12, 14, new Color("#1e1410")); }
        }
        else if (t == VTile.Stele)
        {
            R(X + 6, Y - 25, 20, TS + 23, ValeTerrain.Outline); R(X + 7, Y - 24, 18, TS + 21, new Color("#7c7a70")); R(X + 7, Y - 24, 18, 2, new Color("#9a978b")); R(X + 7, Y + TS - 8, 18, 5, new Color("#56544c"));
            float lit = 0.6f + 0.4f * Mathf.Sin(_t * 2.5f);
            var c = State.SteleRead ? ValeTerrain.LearnGlow : lit > 0.8f ? ValeTerrain.Learn : ValeTerrain.LearnGlow;
            for (int r = 0; r < 5; r++) for (int cc = 0; cc < 2; cc++) R(X + 10 + cc * 7, Y - 18 + r * 7, 4, 2 + ((r + cc) & 1), c);
            R(X + 26, Y + 6, 7, 12, ValeTerrain.Outline); R(X + 27, Y + 7, 5, 9, ValeTerrain.Amber); R(X + 28, Y + 9 + (Mathf.FloorToInt(_t * 6) & 1), 3, 4, ValeTerrain.AmberHi);
            R(X + 18, Y - 2, 24, 28, new Color(226 / 255f, 166 / 255f, 64 / 255f, 0.14f));
        }
        else if (t == VTile.Gate && !State.GateOpen)
        {
            for (int i = 0; i < 4; i++)
            {
                float sx = X + 2 + i * 8;
                R(sx - 1, Y - 22, 7, TS + 20, ValeTerrain.Outline); R(sx, Y - 20, 5, TS + 18, new Color("#6d4f33")); R(sx, Y - 20, 1, TS + 18, new Color("#8a6844")); R(sx + 1, Y - 23, 3, 3, new Color("#6d4f33"));
            }
            R(X, Y - 10, TS, 3, new Color("#4a3422")); R(X, Y + 10, TS, 3, new Color("#4a3422"));
        }
    }
}
