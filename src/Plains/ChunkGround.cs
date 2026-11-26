using Godot;

namespace SpiritOfMurin.Plains;

/// <summary>
/// Draws one chunk's ground once, with the prototype's look: grass, the creek bank (sand and pebbles),
/// water with a ford, the darker bush-field grass and the mud flat, plus the ground marks that belong to
/// objects (burrow mouths, rooted earth, hoof prints). Every detail is hashed from the world tile, so a
/// chunk redraws identically and its edges meet the neighbours'.
/// </summary>
public partial class ChunkGround : Node2D
{
    /// <summary>The chunk to draw.</summary>
    public ChunkData Data { get; set; } = null!;

    /// <summary>The generator (for neighbour tiles at the edges and hashing).</summary>
    public PlainsGenerator Gen { get; set; } = null!;

    private static readonly Color Ground = new("#4c5a3e"), Ground2 = new("#526142"), Tuft = new("#65774c"), TuftDark = new("#3d4a33"),
        Bloom = new("#8e9a6c"), Sand = new("#8f8466"), Sand2 = new("#7f755a"), Pebble = new("#a3a091"), Water = new("#2f5560"),
        Water2 = new("#3b6a73"), WaterHi = new("#8fb3b0"), Shallow = new("#557f7c"), Stone = new("#8a877a"), BushGround = new("#3f4d34"),
        BushGround2 = new("#46553a"), Mud = new("#4a3d2c"), Mud2 = new("#463a2a"), MudWet = new("#5a4b37"), Rooted = new("#5e4630"),
        RootedDark = new("#33271b"), Hole = new("#16140f"), Mound = new("#6b5a3e");

    /// <inheritdoc />
    public override void _Draw()
    {
        int n = Data.Size, ts = Gen.Settings.TileSize;
        for (int ly = 0; ly < n; ly++)
            for (int lx = 0; lx < n; lx++)
            {
                int tx = Data.Coord.X * n + lx, ty = Data.Coord.Y * n + ly;
                DrawTile(Data[lx, ly], tx, ty, lx * ts, ly * ts, ts);
            }
        // ground marks of objects
        Vector2 origin = new(Data.Coord.X * n * ts, Data.Coord.Y * n * ts);
        foreach (var f in Data.Features)
        {
            Vector2 p = f.Position - origin;
            if (f.Kind == FeatureKind.Burrow) DrawBurrow(p);
            else if (f.Kind == FeatureKind.Rooted) DrawRooted(p, f.Variation);
            else if (f.Kind == FeatureKind.Boar) DrawHoofPrints(p, f.Variation);
        }
    }

    private void R(float x, float y, float w, float h, Color c) => DrawRect(new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), c);

    private void DrawTile(TileKind k, int tx, int ty, float X, float Y, int ts)
    {
        uint h = PlainsGenerator.Hash(Gen.Seed, tx, ty, 900);
        switch (k)
        {
            case TileKind.Water:
            case TileKind.Ford:
            {
                bool ford = k == TileKind.Ford;
                R(X, Y, ts, ts, ford ? Shallow : Water);
                R(X + h % 22, Y + (h >> 5) % 28, 8, 1, ford ? WaterHi : Water2);
                R(X + (h >> 10) % 24, Y + (h >> 15) % 28, 5, 1, WaterHi);
                if (ford) for (int i = 0; i < 3; i++) { uint g = PlainsGenerator.Hash(Gen.Seed, tx * 3 + i, ty, 901); R(X + g % 26, Y + (g >> 8) % 26, 5, 3, Stone); }
                // bank edge: a lighter line where the water meets land
                if (!IsWater(tx - 1, ty)) R(X, Y, 2, ts, Sand2);
                if (!IsWater(tx + 1, ty)) R(X + ts - 2, Y, 2, ts, Sand2);
                return;
            }
            case TileKind.Bank:
                R(X, Y, ts, ts, (h & 3) == 0 ? Sand2 : Sand);
                for (int i = 0; i < 3; i++) { uint g = PlainsGenerator.Hash(Gen.Seed, tx * 3 + i, ty, 902); R(X + g % 29, Y + (g >> 8) % 29, 2, 2, (g & 1) == 0 ? Pebble : Sand2); }
                if (h % 5 == 0) Tufts(tx, ty, X, Y, 1);
                return;
            case TileKind.Mud:
                R(X, Y, ts, ts, (h & 3) == 0 ? Mud2 : Mud);
                R(X + h % 18, Y + (h >> 6) % 24, 10, 3, MudWet);
                R(X + (h >> 11) % 26, Y + (h >> 16) % 26, 4, 2, RootedDark);
                return;
            case TileKind.BushGrass:
                R(X, Y, ts, ts, (h & 3) == 0 ? BushGround2 : BushGround);
                Tufts(tx, ty, X, Y, 4);
                return;
            default:
                R(X, Y, ts, ts, (h & 3) == 0 ? Ground2 : Ground);
                Tufts(tx, ty, X, Y, 3);
                if (h % 23 == 0) R(X + 12, Y + 14, 2, 2, Bloom);
                return;
        }
    }

    private bool IsWater(int tx, int ty) { var k = Gen.TileAt(tx, ty); return k is TileKind.Water or TileKind.Ford; }

    private void Tufts(int tx, int ty, float X, float Y, int count)
    {
        for (int i = 0; i < count; i++)
        {
            uint g = PlainsGenerator.Hash(Gen.Seed, tx * 3 + i, ty * 7, 903);
            float gx = X + 2 + g % 26, gy = Y + (g >> 8) % 28;
            R(gx, gy, 1, 3, Tuft); R(gx + 2, gy + 1, 1, 2, TuftDark); R(gx - 2, gy + 1, 1, 2, Tuft);
        }
    }

    private void DrawBurrow(Vector2 p)
    {
        R(p.X - 9, p.Y - 5, 18, 7, Mound); R(p.X - 7, p.Y - 7, 14, 3, Mound);
        R(p.X - 5, p.Y - 4, 10, 5, Hole); R(p.X - 3, p.Y - 5, 6, 1, Hole);
        // small prints leading in
        R(p.X + 9, p.Y + 4, 2, 1, RootedDark); R(p.X + 13, p.Y + 6, 2, 1, RootedDark); R(p.X + 17, p.Y + 4, 2, 1, RootedDark);
    }

    private void DrawRooted(Vector2 p, int v)
    {
        R(p.X - 11, p.Y - 6, 22, 11, Rooted);
        for (int i = 0; i < 6; i++)
        {
            uint g = PlainsGenerator.Hash(v, i, 0, 904);
            R(p.X - 10 + g % 18, p.Y - 5 + (g >> 8) % 9, 3 + (g >> 4) % 3, 2, (g & 1) == 0 ? RootedDark : MudWet);
        }
    }

    private void DrawHoofPrints(Vector2 p, int v)
    {
        for (int i = 0; i < 5; i++)
        {
            uint g = PlainsGenerator.Hash(v, i, 1, 905);
            float x = p.X - 24 + i * 11 + g % 4, y = p.Y + 6 + (g >> 6) % 8 - (i % 2) * 5;
            R(x, y, 2, 2, RootedDark); R(x + 3, y, 2, 2, RootedDark);
        }
    }
}
