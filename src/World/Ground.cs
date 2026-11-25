using Godot;

namespace SpiritOfMurin.World;

/// <summary>
/// The test field's ground, drawn once: the prototype's grass (two tones, tufts, small blooms)
/// and a dirt trail, in 32 px tiles. A placeholder for the seed-generated Planície Verdejante.
/// </summary>
public partial class Ground : Node2D
{
    /// <summary>Field size in tiles.</summary>
    [Export] public Vector2I SizeInTiles { get; set; } = new(48, 34);

    /// <summary>Tile size in pixels (art bible 8.4: 32×32).</summary>
    [Export] public int TileSize { get; set; } = 32;

    /// <summary>Seed for the tuft pattern, so every run draws the same field.</summary>
    [Export] public int Seed { get; set; } = 1;

    /// <summary>Field size in pixels.</summary>
    public Vector2 SizePixels => SizeInTiles * TileSize;

    /// <summary>Deterministic hash used for small variations (the prototype's hash()).</summary>
    public static uint Hash(int x, int y)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263);
            h = (h ^ (h >> 13)) * 1274126177u;
            return h ^ (h >> 16);
        }
    }

    /// <summary>True where the dirt trail runs (an S-shaped band across the field).</summary>
    public bool IsTrail(int tx, int ty) => (tx - (SizeInTiles.X / 2 + Mathf.RoundToInt(Mathf.Sin(ty * 0.35f) * 3))) is >= -1 and <= 0; // two tiles wide

    /// <inheritdoc />
    public override void _Draw()
    {
        Color ground = new("#4c5a3e"), ground2 = new("#526142"), tuft = new("#65774c"), tuftDark = new("#3d4a33"), bloom = new("#8e9a6c");
        Color path = new("#86704f"), path2 = new("#77623f"), pebble = new("#9c8763");
        for (int ty = 0; ty < SizeInTiles.Y; ty++)
            for (int tx = 0; tx < SizeInTiles.X; tx++)
            {
                uint h = Hash(tx + Seed * 101, ty);
                float X = tx * TileSize, Y = ty * TileSize;
                if (IsTrail(tx, ty))
                {
                    DrawRect(new Rect2(X, Y, TileSize, TileSize), path);
                    DrawRect(new Rect2(X + h % 20, Y + (h >> 5) % 24, 6, 3), path2);
                    DrawRect(new Rect2(X + (h >> 9) % 26, Y + (h >> 13) % 26, 2, 2), pebble);
                    continue;
                }
                DrawRect(new Rect2(X, Y, TileSize, TileSize), (h & 3) == 0 ? ground2 : ground);
                for (int i = 0; i < 3; i++)
                {
                    uint g = Hash(tx * 3 + i, ty * 7 + Seed);
                    float gx = X + g % 28, gy = Y + (g >> 8) % 28;
                    DrawRect(new Rect2(gx, gy, 1, 3), tuft); DrawRect(new Rect2(gx + 2, gy + 1, 1, 2), tuftDark); DrawRect(new Rect2(gx - 2, gy + 1, 1, 2), tuft);
                }
                if (h % 23 == 0) DrawRect(new Rect2(X + 12, Y + 14, 2, 2), bloom);
            }
    }
}
