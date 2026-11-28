using Godot;

namespace SpiritOfMurin.Characters;

/// <summary>
/// The Ki gathering in the palm while the charged shot is held (pedido 13, item 8): a jade orb at the front
/// hand that grows with the Ki invested, with sparks drawn inward; at the full charge it rings and pulses.
/// A child of the player drawn over the body, so the protagonist's own drawing stays untouched.
/// </summary>
public partial class KiPalm : Node2D
{
    private float _t;
    private static readonly Color Core = new("#e6fff0"), Glow = new("#6fd8d2"), Ring = new("#a6f2e2");

    /// <inheritdoc />
    public override void _Ready() { ZIndex = 1; }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        // a picture pose (T18) holds the clock too, so the sparks stand in the same place every frame
        _t = GetParent() is Player { PoseFrozen: true } fp ? fp.PoseClock : _t + (float)delta;
        QueueRedraw();
    }

    // the Reforço de Ki: Ki running up the blade during the gesture, a jade shimmer on it while charges remain,
    // and the charges as pips over the head (3, 2, 1)
    private void DrawReinforce(Player p)
    {
        Vector2 hand = p.Facing * 9 + new Vector2(0, -16), tip = hand + p.Facing * 22;
        if (p.Reinforcing)
        {
            float k = p.ReinforceProgress;
            for (int i = 0; i < 8; i++) { float u = i / 7f; if (u > k) break; DrawRect(new Rect2(hand.Lerp(tip, u) - Vector2.One, new Vector2(2, 2)), new Color(Glow, 0.9f)); }
            DrawCircle(hand.Lerp(tip, k), 2.5f, Core);
        }
        if (p.ReinforceCharges <= 0) return;
        for (int i = 0; i < 5; i++)
        {
            float u = (_t * 1.6f + i / 5f) % 1f;
            DrawRect(new Rect2(hand.Lerp(tip, u) - Vector2.One, new Vector2(2, 2)), new Color(Glow, 0.75f));
        }
        for (int i = 0; i < p.ReinforceCharges; i++)
        {
            Vector2 c = new(-6 + i * 6 - (p.ReinforceCharges - 1) * 0f, -52);
            DrawRect(new Rect2(c - new Vector2(2, 2), new Vector2(4, 4)), new Color("#16140f"));
            DrawRect(new Rect2(c - new Vector2(1, 1), new Vector2(2, 2)), Ring);
        }
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        if (GetParent() is not Player p) return;
        DrawReinforce(p);
        if (!p.Charging) return;
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
