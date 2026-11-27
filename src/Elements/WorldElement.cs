using System;
using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Elements;

/// <summary>
/// An element standing in the Planície (pedido 14, campaign delivery 5): a tree, a rock, a bamboo clump or a bush,
/// with its data (<see cref="Def"/>), its place id (stable for the seed, the key of its saved state) and its
/// drawing (<see cref="ElementArts"/>). The origin is the foot, so the y-sorted layer draws whoever stands lower in
/// front and the swordsman can pass behind a canopy; only the base collides, and the base changes with the state
/// (standing → the stump or the rubble → nothing), at the same moment as the picture.
/// <list type="bullet">
/// <item>Workable elements take strikes (<see cref="Strike"/>): notches, cracks or cut stalks show the work; the last
/// strike makes it fall or break and raises <see cref="UsedUp"/> once (the region gives the materials).</item>
/// <item>Bushes bend away from the bodies that cross them (<see cref="Movers"/>) and spring back.</item>
/// </list>
/// </summary>
public partial class WorldElement : StaticBody2D
{
    /// <summary>The element's data.</summary>
    public ElementDef Def { get; set; } = new();
    /// <summary>Stable id of this place (the world feature id), the key of its state in the journey.</summary>
    public string PlaceId { get; set; } = "";
    /// <summary>Seeded number for the small visual differences.</summary>
    public int Variation { get; set; }
    /// <summary>Work done so far (strikes of power 1).</summary>
    public float Work { get; set; }
    /// <summary>Now: standing, falling (or breaking), used up.</summary>
    public ElementState State { get; private set; }
    /// <summary>The bodies that bend a bush when they pass (the region sets it: the swordsman and the animals).</summary>
    public Func<IEnumerable<CharacterBody2D>>? Movers { get; set; }
    /// <summary>Raised once, by the strike that uses the element up.</summary>
    public event Action<WorldElement>? UsedUp;

    /// <summary>How long the fall or the break lasts (s).</summary>
    public float FallSeconds => Def.Family switch { "arvore" => 0.9f, "bambu" => BambuArt.FallSeconds, _ => 0.45f };

    /// <summary>The bend a passing body gives a bush now (px), for checks and the drawing.</summary>
    public Vector2 Bend => _bend;
    /// <summary>The largest bend this bush has had (checks).</summary>
    public float MaxBend { get; private set; }

    private float _time, _fallT, _flash, _sinceHit = 99, _fallSide = 1;
    private Vector2 _bend, _bendVel;
    private CollisionShape2D? _shape;

    /// <summary>Spring of the bush leaves: stiffness and damping.</summary>
    public const float BendStiffness = 60f, BendDamping = 9f;

    /// <inheritdoc />
    public override void _Ready()
    {
        AddToGroup("elements");
        State = Def.Workable && Work >= Def.Toughness ? ElementState.Depleted : ElementState.Standing;
        _time = Variation * 0.37f;
        UpdateShape();
    }

    /// <summary>The base that collides for the current state.</summary>
    private void UpdateShape()
    {
        float hw = State == ElementState.Standing ? Def.BaseHalfWidth : Def.DepletedHalfWidth;
        float h = State == ElementState.Standing ? Def.BaseHeight : Def.DepletedHeight;
        if (hw <= 0 || h <= 0) { _shape?.QueueFree(); _shape = null; return; }
        if (_shape == null) { _shape = new CollisionShape2D(); AddChild(_shape); }
        _shape.Shape = new RectangleShape2D { Size = new Vector2(hw * 2, h) };
        _shape.Position = new Vector2(0, -h / 2);
    }

    /// <summary>The colliding base now: half width and height (0 when nothing collides).</summary>
    public (float HalfWidth, float Height) BaseNow => _shape?.Shape is RectangleShape2D r ? (r.Size.X / 2, r.Size.Y) : (0, 0);

    /// <summary>
    /// A strike of a tool of <paramref name="toolKind"/> and <paramref name="power"/>, from <paramref name="from"/>
    /// (the striker's position: the tree falls away from it). Returns what happened.
    /// </summary>
    public WorkResult Strike(string toolKind, float power, Vector2 from)
    {
        if (State != ElementState.Standing) return WorkResult.AlreadyDepleted;
        var (result, work) = ResourceRules.Strike(Def, Work, toolKind, power);
        if (result is WorkResult.Worked or WorkResult.Depleted)
        {
            Work = work; _flash = 1; _sinceHit = 0;
            _fallSide = from.X <= GlobalPosition.X ? 1 : -1;
            if (result == WorkResult.Depleted)
            {
                State = ElementState.Falling; _fallT = 0;
                UpdateShape(); // the stump's base from now on: the canopy no longer blocks
                UsedUp?.Invoke(this);
            }
        }
        QueueRedraw();
        return result;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _time += dt; _sinceHit += dt;
        _flash = Mathf.Max(0, _flash - dt * 6);
        if (State == ElementState.Falling)
        {
            _fallT += dt / FallSeconds;
            if (_fallT >= 1) { State = ElementState.Depleted; UpdateShape(); QueueRedraw(); } // one last drawing: the stump
        }
        if (Def.ContactRadius > 0) UpdateBend(dt);
        if (State != ElementState.Depleted || Def.Family == "arbusto") QueueRedraw();
    }

    // a bush: the bodies within the radius push the leaves along their way; a spring brings them back
    private void UpdateBend(float dt)
    {
        Vector2 target = Vector2.Zero;
        if (Movers != null)
            foreach (var b in Movers())
            {
                if (!IsInstanceValid(b)) continue;
                Vector2 d = b.GlobalPosition - GlobalPosition;
                float dist = d.Length();
                if (dist > Def.ContactRadius || b.Velocity.LengthSquared() < 4) continue;
                float k = 1 - dist / Def.ContactRadius;
                // along the way the body goes, a little away from it
                target += (b.Velocity.Normalized() * 5f + (d.LengthSquared() > 0.01f ? -d.Normalized() * 2f : Vector2.Zero)) * k;
            }
        target = target.LimitLength(6);
        Vector2 acc = (target - _bend) * BendStiffness - _bendVel * BendDamping;
        _bendVel += acc * dt; _bend += _bendVel * dt;
        MaxBend = Mathf.Max(MaxBend, _bend.Length());
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        var pose = new ElementPose
        {
            Variation = Variation, Time = _time, Wind = Plains.Wind.At(GlobalPosition) * Plains.Wind.Dir.X, Progress = ResourceRules.Progress(Def, Work),
            State = State, FallT = Mathf.Clamp(_fallT, 0, 1), FallSide = _fallSide, Bend = _bend, Flash = _flash, SinceHit = _sinceHit,
        };
        ElementArts.Get(Def.Art).Draw(this, Def, pose);
        if (State == ElementState.Standing && Def.Workable && Work > 0 && _sinceHit < BarSeconds) DrawProgress(pose.Progress);
    }

    /// <summary>How long (s) the work bar stays after a strike.</summary>
    public const float BarSeconds = 3f;
    /// <summary>Colours of the work bar: frame, empty, done.</summary>
    public static readonly Color BarFrame = new("#16140f"), BarEmpty = new("#3a3428"), BarDone = new("#e6c86a");

    /// <summary>Height (px above the foot) of the work bar: data <c>look.bar_y</c>, else by family.</summary>
    public float BarY => Def.LookF("bar_y", Def.Family switch { "arvore" => 78, "bambu" => 54, _ => 36 });

    // a small bar above the element while it is being worked: how much is done
    private void DrawProgress(float progress)
    {
        float w = 24, y = -BarY, a = Mathf.Clamp((BarSeconds - _sinceHit) / 0.5f, 0, 1);
        DrawRect(new Rect2(-w / 2 - 1, y - 1, w + 2, 5), new Color(BarFrame, a));
        DrawRect(new Rect2(-w / 2, y, w, 3), new Color(BarEmpty, a));
        DrawRect(new Rect2(-w / 2, y, Mathf.Round(w * progress), 3), new Color(BarDone, a));
    }
}
