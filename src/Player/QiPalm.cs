using Godot;

namespace SpiritOfMurin.Characters;

/// <summary>
/// The Qi gathering in the palm while the charged shot is held (pedido 13, item 8): a jade orb at the front
/// hand that grows with the Qi invested, with sparks drawn inward; at the full charge it rings and pulses.
/// A child of the player drawn over the body, so the protagonist's own drawing stays untouched.
/// </summary>
public partial class QiPalm : Node2D
{
    private float _t;
    private static readonly Color Core = new("#e6fff0"), Glow = new("#6fd8d2"), Ring = new("#a6f2e2");

    /// <inheritdoc />
    public override void _Ready() { ZIndex = 1; }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _t += (float)delta;
        if (GetParent() is Player p && (p.Charging || Visible)) QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        if (GetParent() is not Player p || !p.Charging) return;
        float k = Mathf.Clamp(p.ChargeInvested / Mathf.Max(1, p.ChargeCost), 0, 1);
        Vector2 hand = p.Facing * 11 + new Vector2(0, -17);
        float r = 2 + 6 * k;
        // sparks pulled in toward the palm
        for (int i = 0; i < 6; i++)
        {
            float a = _t * 3 + i * Mathf.Tau / 6, d = 14 - (_t * 20 + i * 5) % 12;
            DrawRect(new Rect2(hand + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * d - Vector2.One, new Vector2(2, 2)), new Color(Glow, 0.5f + 0.4f * k));
        }
        DrawCircle(hand, r + 2, new Color(Glow, 0.35f));
        DrawCircle(hand, r, new Color(Glow, 0.8f));
        DrawCircle(hand, Mathf.Max(1, r * 0.5f), Core);
        if (p.ChargeComplete)
        {
            float pulse = 1 + 0.25f * Mathf.Sin(_t * 12);
            DrawArc(hand, (r + 5) * pulse, 0, Mathf.Tau, 24, Ring, 2);
            DrawArc(hand, (r + 9) * pulse, 0, Mathf.Tau, 24, new Color(Ring, 0.4f), 1);
        }
    }
}
