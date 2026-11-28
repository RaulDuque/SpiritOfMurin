using Godot;
using SpiritOfMurin.Combat;

namespace SpiritOfMurin.Game;

/// <summary>
/// The compact Vida bar over the animal in a fight (T07, decision A): "atual/máximo" inside, over the animal's head, while
/// it is focused (Espaço) or a boar warns, charges or recovers near the swordsman; it stays a few seconds after the fight
/// (<c>near_bar_seconds</c> in <c>plains_fauna.json</c>). The bar at the top of the screen keeps showing too.
/// <b>Where to change it:</b> sizes and colours below.
/// </summary>
public partial class NearBar : Node2D
{
    /// <summary>Size (px) and how high over the feet.</summary>
    public const int W = 34, H = 7; public const float Above = 34;
    /// <summary>Colours: frame, empty, Vida, text.</summary>
    public static readonly Color FrameC = new("#16140f"), EmptyC = new("#2b1f1a"), LifeC = new("#b0403a"), TextC = new("#ece5d0");

    /// <summary>The animal shown, or null.</summary>
    public Node2D? Target { get; private set; }
    /// <summary>Its Vida now and at full.</summary>
    public float Health { get; private set; }
    /// <inheritdoc cref="Health"/>
    public float Max { get; private set; } = 1;
    /// <summary>The numbers inside ("4,7/6"), or "" when hidden.</summary>
    public string Text => Visible && Target != null ? $"{DamageRules.Format(Mathf.Max(0, Health))}/{DamageRules.Format(Max)}" : "";
    private Font _font = null!;

    /// <inheritdoc />
    public override void _Ready() { _font = ThemeDB.FallbackFont; ZIndex = 52; Visible = false; }

    /// <summary>Shows the bar over <paramref name="target"/> (null hides it).</summary>
    public void Show(Node2D? target, float health, float max)
    {
        Target = target; Health = health; Max = Mathf.Max(0.01f, max);
        Visible = target != null && IsInstanceValid(target);
        if (Visible) GlobalPosition = target!.GlobalPosition + new Vector2(0, -Above);
        QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        if (Target == null) return;
        var r = new Rect2(-W / 2f, 0, W, H);
        DrawRect(r.Grow(1), FrameC); DrawRect(r, EmptyC);
        DrawRect(new Rect2(r.Position, new Vector2(Mathf.Round(W * Mathf.Clamp(Health / Max, 0, 1)), H)), LifeC);
        string t = Text;
        float w = _font.GetStringSize(t, HorizontalAlignment.Left, -1, 7).X;
        var at = new Vector2(-w / 2, H - 1);
        DrawStringOutline(_font, at, t, HorizontalAlignment.Left, -1, 7, 2, FrameC);
        DrawString(_font, at, t, HorizontalAlignment.Left, -1, 7, TextC);
    }
}
