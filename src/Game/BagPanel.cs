using System;
using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Game;

/// <summary>
/// The bag (inventory), in the HTML prototype's look: a parchment with a grid of slots, each with the item's
/// picture (<see cref="ItemIcons"/>), the stack count and a mark on the equipment in use; below, the recipes
/// as slots with the result's picture. The selected slot shows its name, a short description and the
/// actions possible right now (equip, use, eat, craft — with what is missing). The money (Moedas) shows in
/// the header. Mouse: hover selects, a click acts (the main action), the action button too. Keyboard: the
/// game moves the selection (up/down by rows, left/right one by one) and acts with the use key.
/// </summary>
public partial class BagPanel : Control
{
    /// <summary>The rows the bag shows (items, then recipes), shared with the HUD.</summary>
    public List<PlainsHud.Row> Rows { get; set; } = new();
    /// <summary>Reads the selected index.</summary>
    public Func<int> GetSelected { get; set; } = () => 0;
    /// <summary>A slot was hovered (select it).</summary>
    public event Action<int>? Hovered;
    /// <summary>A slot or its action button was clicked (act on it).</summary>
    public event Action<int>? Clicked;
    /// <summary>Slots per row.</summary>
    public int Columns { get; private set; } = 8;
    /// <summary>The slots' rectangles (panel coordinates), by row index.</summary>
    public Dictionary<int, Rect2> SlotRects { get; } = new();
    /// <summary>The action button's rectangle (panel coordinates), or an empty rect.</summary>
    public Rect2 ActionRect { get; private set; }
    /// <summary>The money shown in the header (null hides it).</summary>
    public int? Money { get; set; }
    /// <summary>The title.</summary>
    public string Title { get; set; } = "Bolsa";
    /// <summary>What to say when there is nothing.</summary>
    public string Empty { get; set; } = "";

    private static readonly Color Ink = MapView.Ink, Ink2 = MapView.Ink2, Jade = new("#1d5e57"), SlotBg = new("#c9b68d"), SlotHi = new("#e9dcb8"), Off = new("#8a7a5e");

    /// <inheritdoc />
    public override void _Ready() { MouseFilter = MouseFilterEnum.Stop; Resized += QueueRedraw; }

    /// <inheritdoc />
    public override void _Process(double delta) { if (Visible) QueueRedraw(); }

    /// <inheritdoc />
    public override void _GuiInput(InputEvent e)
    {
        if (e is InputEventMouseMotion mm)
        {
            foreach (var (i, r) in SlotRects) if (r.HasPoint(mm.Position) && i != GetSelected()) { Hovered?.Invoke(i); break; }
        }
        else if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mb)
        {
            if (ActionRect.HasPoint(mb.Position)) { Clicked?.Invoke(GetSelected()); AcceptEvent(); return; }
            foreach (var (i, r) in SlotRects) if (r.HasPoint(mb.Position)) { Hovered?.Invoke(i); Clicked?.Invoke(i); break; }
            AcceptEvent();
        }
        else if (e is InputEventMouseButton) AcceptEvent();
    }

    /// <summary>The index one row down (or up) in the grid; past the end it goes to the top of the next column, so every slot is reached.</summary>
    public int Vertical(int i, int d)
    {
        int n = Rows.Count; if (n == 0) return 0;
        int c = Columns;
        if (d > 0) { int next = i + c; if (next < n) return next; int col = i % c + 1; return col < c && col < n ? col : 0; }
        int prev = i - c; if (prev >= 0) return prev;
        int colUp = i % c - 1; if (colUp < 0) colUp = Math.Min(c, n) - 1;
        int last = colUp; while (last + c < n) last += c; return last;
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        var font = ThemeDB.FallbackFont;
        DrawRect(new Rect2(-6, -8, Size.X + 12, 8), MapView.Roller); DrawRect(new Rect2(-6, Size.Y, Size.X + 12, 8), MapView.Roller);
        DrawRect(new Rect2(-6, -7, Size.X + 12, 2), MapView.RollerHi); DrawRect(new Rect2(-6, Size.Y + 1, Size.X + 12, 2), MapView.RollerHi);
        DrawRect(new Rect2(Vector2.Zero, Size), MapView.Paper);
        DrawRect(new Rect2(0, 0, Size.X, 3), MapView.PaperEdge); DrawRect(new Rect2(0, Size.Y - 3, Size.X, 3), MapView.PaperEdge);
        DrawRect(new Rect2(0, 0, 3, Size.Y), MapView.PaperEdge); DrawRect(new Rect2(Size.X - 3, 0, 3, Size.Y), MapView.PaperEdge);
        DrawString(font, new Vector2(12, 17), Title, HorizontalAlignment.Left, -1, 12, Ink);
        if (Money is { } m)
        {
            float w = font.GetStringSize($"Moedas: {m}", HorizontalAlignment.Left, -1, 10).X;
            ItemIcons.Draw(this, "moedas", new Vector2(Size.X - w - 26, 12), 1);
            DrawString(font, new Vector2(Size.X - w - 14, 16), $"Moedas: {m}", HorizontalAlignment.Left, -1, 10, Ink);
        }
        DrawRect(new Rect2(12, 22, Size.X - 24, 1), Ink2);

        int sz = Size.X >= 420 ? 40 : 34;
        Columns = Math.Max(4, (int)((Size.X - 24) / sz));
        float gx = 12, y = 30;
        SlotRects.Clear();
        int sel = GetSelected();
        int firstRecipe = Rows.FindIndex(r => r.IsRecipe);
        int items = firstRecipe < 0 ? Rows.Count : firstRecipe;
        if (Rows.Count == 0 || items == 0) DrawString(font, new Vector2(gx, y + 14), Empty.Length > 0 ? Empty : "(vazia)", HorizontalAlignment.Left, Size.X - 24, 9, Ink2);
        // the items: a grid that fills row by row
        for (int i = 0; i < items; i++)
        {
            var r = new Rect2(gx + (i % Columns) * sz, y + (i / Columns) * sz, sz - 2, sz - 2);
            DrawSlot(font, i, r, sel == i);
        }
        if (items > 0) y += ((items - 1) / Columns + 1) * sz;
        if (firstRecipe >= 0)
        {
            if (items == 0) y += 18;
            DrawString(font, new Vector2(gx, y + 10), "Fabricar", HorizontalAlignment.Left, -1, 9, Ink2);
            y += 14;
            for (int i = firstRecipe; i < Rows.Count; i++)
            {
                int k = i - firstRecipe;
                var r = new Rect2(gx + (k % Columns) * sz, y + (k / Columns) * sz, sz - 2, sz - 2);
                DrawSlot(font, i, r, sel == i);
            }
            y += ((Rows.Count - firstRecipe - 1) / Columns + 1) * sz;
        }
        // the selected slot: name, description, the actions possible now
        ActionRect = new Rect2();
        float dy = y + 6;
        DrawRect(new Rect2(12, dy - 2, Size.X - 24, 1), Ink2);
        if (sel >= 0 && sel < Rows.Count)
        {
            var row = Rows[sel];
            string name = row.Name.Length > 0 ? row.Name : row.Text;
            DrawString(font, new Vector2(12, dy + 12), name + (row.Count > 1 && !row.IsRecipe ? $"  ×{row.Count}" : "") + (row.Equipped ? "  — em uso" : ""), HorizontalAlignment.Left, Size.X - 24, 11, row.Equipped ? Jade : Ink);
            float ly = dy + 18;
            foreach (var line in MapView.Wrap(row.Detail, Size.X - 140, 9))
            {
                if (ly > Size.Y - 30) break;
                DrawString(font, new Vector2(12, ly + 9), line, HorizontalAlignment.Left, Size.X - 140, 9, Ink2); ly += 11;
            }
            if (row.Action.Length > 0)
            {
                string label = Bindings.F($"{row.Action}  [[use]]");
                float bw = font.GetStringSize(label, HorizontalAlignment.Left, -1, 10).X + 16;
                ActionRect = new Rect2(Size.X - bw - 14, dy + 4, bw, 18);
                bool can = row.Enabled && !row.Equipped;
                DrawRect(ActionRect, can ? Jade : Off);
                DrawString(font, new Vector2(ActionRect.Position.X, ActionRect.Position.Y + 13), label, HorizontalAlignment.Center, bw, 10, MapView.Paper);
                if (!row.Enabled && row.Need.Length > 0)
                    DrawString(font, new Vector2(Size.X - 130, dy + 34), row.Need, HorizontalAlignment.Left, 118, 8, MapView.Seal);
            }
        }
        DrawString(font, new Vector2(12, Size.Y - 7), Bindings.F("setas ou [walk]: escolher · [use] ou clique: agir · [inventory] ou Esc: fechar"), HorizontalAlignment.Left, Size.X - 24, 8, Ink2);
    }

    private void DrawSlot(Font font, int i, Rect2 r, bool sel)
    {
        SlotRects[i] = r;
        var row = Rows[i];
        DrawRect(r, sel ? Jade : Ink2);
        DrawRect(r.Grow(-2), sel ? SlotHi : row.IsRecipe && !row.Enabled ? new Color(SlotBg, 0.55f) : SlotBg);
        float s = r.Size.X >= 38 ? 2 : 1.6f;
        if (row.Icon.Length > 0) ItemIcons.Draw(this, row.Icon, r.GetCenter() + new Vector2(0, -2), s);
        if (row.IsRecipe)
        {
            if (!row.Enabled) DrawRect(r.Grow(-2), new Color(MapView.Paper, 0.45f));
            DrawString(font, new Vector2(r.Position.X + 3, r.Position.Y + 10), "+", HorizontalAlignment.Left, -1, 10, row.Enabled ? Jade : Off);
        }
        if (row.Count > 1)
        {
            string n = row.Count.ToString();
            float w = font.GetStringSize(n, HorizontalAlignment.Left, -1, 9).X;
            DrawString(font, new Vector2(r.End.X - w - 3, r.End.Y - 3) + Vector2.One, n, HorizontalAlignment.Left, -1, 9, SlotHi);
            DrawString(font, new Vector2(r.End.X - w - 3, r.End.Y - 3), n, HorizontalAlignment.Left, -1, 9, Ink);
        }
        if (row.Equipped)
        {
            // the equipment in use: a jade corner and a small "E"
            DrawRect(new Rect2(r.Position + Vector2.One * 2, new Vector2(10, 10)), Jade);
            DrawString(font, new Vector2(r.Position.X + 4, r.Position.Y + 11), "✓", HorizontalAlignment.Left, -1, 8, MapView.Paper);
        }
    }
}
