using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Game;

/// <summary>
/// The item shortcuts on the HUD (T06): a small row above the footer with each shortcut in use — the key, the item's
/// picture and how many are left. An empty stack shows the slot faded with "0" (and the key still says what it holds),
/// so it is clear the shortcut ran out. H (the herb, with the Bolsa Simples) comes first.
/// <b>Where to change it:</b> sizes and colours below; the slots are filled by <c>PlainsGame.RefreshQuickBar</c>; the keys
/// are the actions <c>heal</c> and <c>quick_1</c>…<c>quick_8</c> (Esc → Controles); what each slot holds is saved in the
/// preferences (<c>quick_slots</c>).
/// </summary>
public partial class QuickBar : Control
{
    /// <summary>One shortcut shown: key label, item picture, how many, the item's name.</summary>
    public readonly record struct Slot(string Key, string Icon, int Count, string Name);

    /// <summary>The shortcuts shown, in order.</summary>
    public List<Slot> Slots { get; } = new();

    /// <summary>Size of a slot (px) and the gap.</summary>
    public const int SlotPx = 22, Gap = 3;
    private static readonly Color Back = new(0.07f, 0.075f, 0.06f, 0.8f), Frame = new("#6a5e46"), Empty = new(0.3f, 0.28f, 0.24f, 0.8f), KeyC = new("#e6c86a"), CountC = new("#ece5d0"), Zero = new("#e0503a");

    /// <inheritdoc />
    public override void _Ready() => MouseFilter = MouseFilterEnum.Ignore;

    /// <summary>Replaces the shortcuts shown.</summary>
    public void Set(IEnumerable<Slot> slots) { Slots.Clear(); Slots.AddRange(slots); QueueRedraw(); }

    /// <inheritdoc />
    public override void _Draw()
    {
        var font = ThemeDB.FallbackFont;
        for (int i = 0; i < Slots.Count; i++)
        {
            var s = Slots[i];
            var r = new Rect2(i * (SlotPx + Gap), 0, SlotPx, SlotPx);
            DrawRect(r, Frame); DrawRect(r.Grow(-1), s.Count > 0 ? Back : Empty);
            if (s.Icon.Length > 0)
            {
                ItemIcons.Draw(this, s.Icon, r.GetCenter() + new Vector2(0, -1), 1);
                if (s.Count <= 0) DrawRect(r.Grow(-1), new Color(0, 0, 0, 0.45f));
            }
            DrawString(font, new Vector2(r.Position.X + 2, r.Position.Y + 8), s.Key, HorizontalAlignment.Left, -1, 7, KeyC);
            string n = s.Count.ToString();
            float w = font.GetStringSize(n, HorizontalAlignment.Left, -1, 7).X;
            DrawString(font, new Vector2(r.End.X - w - 2, r.End.Y - 2), n, HorizontalAlignment.Left, -1, 7, s.Count > 0 ? CountC : Zero);
        }
    }
}
