using Godot;
using SpiritOfMurin.Elements;

namespace SpiritOfMurin.World;

/// <summary>
/// Tall scenery with the prototype's look: a pine, a lone boulder or a clump of bamboo (the Vale and the test field).
/// The drawings are the element arts in <c>src/Elements</c> (PinheiroArt, RochaArt, BambuArt, ArbustoArt), shared with
/// the Planície's workable elements (<see cref="WorldElement"/>).
/// The origin is the foot of the object, so a y-sorted parent draws whoever stands lower in
/// front. Only the base collides (art bible 6.5: base and body), so the player can walk behind.
/// </summary>
public partial class Obstacle : StaticBody2D
{
    /// <summary>The kinds of tall scenery the base scene uses.</summary>
    public enum Kind { Pine, Boulder, Bamboo, Bush }

    /// <summary>What this obstacle is.</summary>
    [Export] public Kind Type { get; set; } = Kind.Pine;

    /// <summary>Seed for the small variations (height, sway phase).</summary>
    [Export] public int Variation { get; set; }

    private float _time;

    // base collision boxes (half width, height), the HTML prototype's BASE values
    private static readonly (float HalfWidth, float Height) PineBase = (7, 11), BoulderBase = (12, 13), BambooBase = (16, 7), BushBase = (9, 6);

    /// <inheritdoc />
    public override void _Ready()
    {
        var (hw, h) = Type switch { Kind.Pine => PineBase, Kind.Boulder => BoulderBase, Kind.Bush => BushBase, _ => BambooBase };
        AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(hw * 2, h) }, Position = new Vector2(0, -h / 2) });
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _time += (float)delta;
        if (Type != Kind.Boulder) QueueRedraw(); // pines and bamboo sway
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        // the pictures live in src/Elements (one class per look); with no data they draw the original look
        string art = Type switch { Kind.Pine => "pinheiro", Kind.Boulder => "rocha", Kind.Bush => "arbusto", _ => "bambu" };
        float wind = Type is Kind.Pine or Kind.Bush ? Plains.Wind.At(GlobalPosition) * Plains.Wind.Dir.X : 0;
        var pose = new ElementPose { Variation = Variation, Time = _time, Wind = wind, SinceHit = 99 };
        ElementArts.Get(art).Draw(this, null, pose);
    }
}
