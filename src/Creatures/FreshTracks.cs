using System.Collections.Generic;
using System.Linq;
using Godot;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Creatures;

/// <summary>
/// Where an animal leaves its next print (T07, pure): every <c>spacing</c> pixels along the path it actually walked,
/// pointing the way it was going at that moment (its movement over the last few updates, not the chord since the last
/// print) — so the prints follow the turns. Teleports (a jump larger than
/// <see cref="JumpPx"/>, e.g. a chunk reload) restart the trail without a print.
/// </summary>
public sealed class TrackStepper
{
    /// <summary>A jump this big restarts the trail (px).</summary>
    public const float JumpPx = 120;
    private Vector2? _last;
    private readonly Queue<Vector2> _recent = new();
    /// <summary>How many updates back the direction is read from.</summary>
    public const int RecentSteps = 6;
    /// <summary>Pixels between prints.</summary>
    public float Spacing { get; }
    /// <summary>Prints left so far (the next print's index).</summary>
    public int Count { get; private set; }

    /// <summary>A stepper for prints <paramref name="spacing"/> pixels apart.</summary>
    public TrackStepper(float spacing) => Spacing = Mathf.Max(2, spacing);

    /// <summary>The animal is at <paramref name="pos"/>: returns the print to leave (place and direction), or null.</summary>
    public (Vector2 At, Vector2 Dir)? Step(Vector2 pos)
    {
        if (_last is not { } l || l.DistanceTo(pos) > JumpPx || (_recent.Count > 0 && _recent.Last().DistanceTo(pos) > JumpPx))
        { _last = pos; _recent.Clear(); _recent.Enqueue(pos); return null; }
        _recent.Enqueue(pos);
        while (_recent.Count > RecentSteps + 1) _recent.Dequeue();
        float d = l.DistanceTo(pos);
        if (d < Spacing) return null;
        Vector2 way = pos - _recent.Peek();
        Vector2 dir = way.LengthSquared() > 1 ? way.Normalized() : (pos - l) / d;
        _last = pos; Count++;
        return (pos, dir);
    }
}

/// <summary>
/// Leaves fresh prints behind the moving animals (T07): rabbits and boars, while visible, on the real path; each print
/// lasts <c>track_seconds</c> and fades; the oldest go first above <c>track_max</c>. The trails generated with the world
/// (the clearing's) are left untouched. Values in <c>assets/data/plains/plains_fauna.json</c>.
/// </summary>
public partial class FreshTracks : Node
{
    /// <summary>Where the prints go (the world's objects).</summary>
    public Node2D Parent { get; set; } = null!;
    /// <summary>The settings (tests inject their own).</summary>
    public FaunaSettings Settings { get; set; } = FaunaSettings.Current;
    private readonly Dictionary<string, TrackStepper> _steppers = new();
    private readonly Queue<TrackPrint> _prints = new();

    /// <summary>The fresh prints alive (oldest first).</summary>
    public IEnumerable<TrackPrint> Prints => _prints.Where(p => IsInstanceValid(p) && !p.IsQueuedForDeletion());

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        foreach (var n in GetTree().GetNodesInGroup("creatures"))
        {
            if (n is not (Rabbit or Boar) || n is not IHittable h || !h.CanBeHit) continue;
            if (n is Rabbit r && !r.Brain.Visible) continue;
            var body = (Node2D)n;
            bool hoof = n is Boar;
            if (!_steppers.TryGetValue(h.CreatureId, out var st))
                _steppers[h.CreatureId] = st = new TrackStepper(hoof ? Settings.TrackSpacingBoar : Settings.TrackSpacingRabbit);
            if (st.Step(body.GlobalPosition) is not { } step) continue;
            Leave(h.CreatureId, st.Count, step.At, step.Dir);
        }
    }

    private void Leave(string creature, int index, Vector2 at, Vector2 dir)
    {
        while (_prints.Count > 0 && (!IsInstanceValid(_prints.Peek()) || _prints.Count >= Settings.TrackMax))
        {
            var old = _prints.Dequeue();
            if (IsInstanceValid(old)) old.QueueFree();
        }
        var p = new TrackPrint
        {
            FeatureId = $"fresh@{creature}#{index}", TrailId = "fresh@" + creature, CreatureId = creature, Index = index,
            Dir = dir, Position = Parent.ToLocal(at), ZIndex = -5, Fresh = true, Life = Settings.TrackSeconds,
            Name = new string($"fresh_{creature}_{index}".Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray()),
        };
        Parent.AddChild(p); _prints.Enqueue(p);
    }
}
