using Godot;
using SpiritOfMurin.World;

namespace SpiritOfMurin.Scenes;

/// <summary>
/// The engine base's main scene: a test field with the prototype's look, a few pines, boulders
/// and bamboo placed deterministically from the field's seed, the player and a following camera
/// held inside the field. The seed-generated Planície Verdejante replaces the field next.
/// </summary>
public partial class Main : Node2D
{
    /// <summary>The player instance, for checks.</summary>
    public Characters.Player Player => GetNode<Characters.Player>("World/Player");

    /// <inheritdoc />
    public override void _Ready()
    {
        var ground = GetNode<Ground>("Ground");
        var world = GetNode<Node2D>("World");
        int ts = ground.TileSize;
        // scenery: every few tiles, a hash decides pine / boulder / bamboo / nothing (never on the trail
        // nor in the clearing around the start)
        Vector2 start = Player.Position;
        for (int ty = 1; ty < ground.SizeInTiles.Y - 1; ty += 2)
            for (int tx = 1; tx < ground.SizeInTiles.X - 1; tx += 2)
            {
                uint h = Ground.Hash(tx * 11 + ground.Seed, ty * 5);
                if (h % 7 > 1 || ground.IsTrail(tx, ty) || ground.IsTrail(tx + 1, ty) || ground.IsTrail(tx - 1, ty)) continue;
                var at = new Vector2(tx * ts + 16 + (int)(h >> 8) % 9 - 4, (ty + 1) * ts);
                if (at.DistanceTo(start) < 110) continue;
                var kind = ((h >> 4) % 3) switch { 0 => Obstacle.Kind.Pine, 1 => Obstacle.Kind.Boulder, _ => Obstacle.Kind.Bamboo };
                world.AddChild(new Obstacle { Type = kind, Variation = (int)(h % 1000), Position = at, Name = $"{kind}_{tx}_{ty}" });
            }
        // the camera stays inside the field
        var cam = Player.GetNode<Camera2D>("Camera");
        cam.LimitLeft = 0; cam.LimitTop = 0;
        cam.LimitRight = (int)ground.SizePixels.X; cam.LimitBottom = (int)ground.SizePixels.Y;
        // field borders
        var border = new StaticBody2D { Name = "Border" };
        Vector2 size = ground.SizePixels;
        foreach (var (pos, ext) in new[] {
            (new Vector2(size.X / 2, -8), new Vector2(size.X, 16)), (new Vector2(size.X / 2, size.Y + 8), new Vector2(size.X, 16)),
            (new Vector2(-8, size.Y / 2), new Vector2(16, size.Y)), (new Vector2(size.X + 8, size.Y / 2), new Vector2(16, size.Y)) })
            border.AddChild(new CollisionShape2D { Position = pos, Shape = new RectangleShape2D { Size = ext } });
        AddChild(border);
    }
}
