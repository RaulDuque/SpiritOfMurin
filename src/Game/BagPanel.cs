using System;
using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Game;

/// <summary>
/// The bag (inventory), in the HTML prototype's look (T06, option A of the board
/// <c>user-docs/relatorios/2026-11-27-t06-prancha-bolsa.html</c>): a parchment with the money in the header, a row of
/// <b>tabs</b> (Tudo, Equipamentos, Ferramentas, Consumíveis, Materiais, Fabricação), a grid of slots — each with the
/// item's picture (<see cref="ItemIcons"/>), the stack count and a ✓ on the one in hand — and, below, the selected
/// slot's detail: name, description, the extra lines (the ingredients as "tenho X de Y", the comparison with the
/// tool in hand), the shortcut that holds it, and the action button.
/// <b>Where to change it:</b> the tabs in <see cref="DrawTabs"/>, the slots in <see cref="DrawSlot"/>, the detail in
/// <see cref="DrawDetail"/>, the colours below. The rows are built by <c>PlainsGame.RefreshRows</c>; the tab list and the
/// comparison rules are <c>src/Items/BagRules.cs</c>.
/// Mouse: hover selects, a click acts; a click on a tab opens it. Keyboard (handled by the game): arrows or WASD
/// choose, E acts, Tab changes the tab, 1-6/Q/R put the item on a shortcut.
/// </summary>
public partial class BagPanel : Control
{
    /// <summary>The rows the bag shows (the current tab's items, or the recipes in Fabricação), shared with the HUD.</summary>
    public List<PlainsHud.Row> Rows { get; set; } = new();
    /// <summary>Reads the selected index.</summary>
    public Func<int> GetSelected { get; set; } = () => 0;
    /// <summary>A slot was hovered (select it).</summary>
    public event Action<int>? Hovered;
    /// <summary>A slot or its action button was clicked (act on it).</summary>
    public event Action<int>? Clicked;
    /// <summary>A tab was clicked (its index).</summary>
    public event Action<int>? TabClicked;
    /// <summary>Slots per row.</summary>
    public int Columns { get; private set; } = 8;
    /// <summary>The slots' rectangles (panel coordinates), by row index.</summary>
    public Dictionary<int, Rect2> SlotRects { get; } = new();
    /// <summary>The tabs' rectangles (panel coordinates), by tab index.</summary>
    public Dictionary<int, Rect2> TabRects { get; } = new();
    /// <summary>The action button's rectangle (panel coordinates), or an empty rect.</summary>
    public Rect2 ActionRect { get; private set; }
    /// <summary>The money shown in the header (null hides it).</summary>
    public int? Money { get; set; }
    /// <summary>The title.</summary>
    public string Title { get; set; } = "Bolsa";
    /// <summary>What to say when there is nothing.</summary>
    public string Empty { get; set; } = "";
    /// <summary>The tabs' names, in order (empty: no tab row).</summary>
    public List<string> TabNames { get; } = new();
    /// <summary>The open tab.</summary>
    public int ActiveTab { get; set; }

    /// <summary>Colours: ink, faded ink, jade (selection, in hand), slot, selected slot, disabled, fine, missing, better, worse.</summary>
    public static readonly Color Ink = MapView.Ink, Ink2 = MapView.Ink2, Jade = new("#1d5e57"), SlotBg = new("#c9b68d"), SlotHi = new("#e9dcb8"), Off = new("#8a7a5e"),
        OkC = new("#2f7a3a"), MissC = MapView.Seal, BetterC = new("#1d5e57"), WorseC = MapView.Seal;
    /// <summary>Size of a slot (px, at the game's 640×360).</summary>
    public const int SlotSize = 40;

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
            foreach (var (t, r) in TabRects) if (r.HasPoint(mb.Position)) { TabClicked?.Invoke(t); AcceptEvent(); return; }
            if (ActionRect.HasPoint(mb.Position)) { Clicked?.Invoke(GetSelected()); AcceptEvent(); return; }
            foreach (var (i, r) in SlotRects) if (r.HasPoint(mb.Position)) { Hovered?.Invoke(i); Clicked?.Invoke(i); break; }
            AcceptEvent();
        }
        else if (e is InputEventMouseButton) AcceptEvent(); // the wheel and other buttons stay in the bag
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
            string t = $"Moedas: {m}";
            float w = font.GetStringSize(t, HorizontalAlignment.Left, -1, 10).X;
            ItemIcons.Draw(this, "moedas", new Vector2(Size.X - w - 26, 12), 1);
            DrawString(font, new Vector2(Size.X - w - 14, 16), t, HorizontalAlignment.Left, -1, 10, Ink);
        }
        float y = 22;
        if (TabNames.Count > 0) { DrawTabs(font, 12, 24); y = 41; }
        DrawRect(new Rect2(12, y, Size.X - 24, 1), Ink2);

        int sz = SlotSize;
        Columns = Math.Max(4, (int)((Size.X - 24) / sz));
        float gx = 12; y += 5;
        SlotRects.Clear();
        int sel = GetSelected();
        if (Rows.Count == 0) DrawString(font, new Vector2(gx, y + 14), Empty.Length > 0 ? Empty : "(vazia)", HorizontalAlignment.Left, Size.X - 24, 9, Ink2);
        for (int i = 0; i < Rows.Count; i++)
            DrawSlot(font, i, new Rect2(gx + (i % Columns) * sz, y + (i / Columns) * sz, sz - 2, sz - 2), sel == i);
        if (Rows.Count > 0) y += ((Rows.Count - 1) / Columns + 1) * sz;
        DrawDetail(font, y + 6, sel);
        DrawString(font, new Vector2(12, Size.Y - 7), Bindings.F("[bag_tab] ou clique: aba · setas ou [walk]: escolher · [use] ou clique: agir · 1-6, Q, R: pôr no atalho · [inventory] ou Esc: fechar"),
            HorizontalAlignment.Left, Size.X - 24, 8, Ink2);
    }

    // the tabs: the open one in jade
    private void DrawTabs(Font font, float x, float y)
    {
        TabRects.Clear();
        for (int t = 0; t < TabNames.Count; t++)
        {
            float w = font.GetStringSize(TabNames[t], HorizontalAlignment.Left, -1, 9).X + 12;
            var r = new Rect2(x, y, w, 14);
            TabRects[t] = r;
            bool on = t == ActiveTab;
            DrawRect(r, on ? Jade : new Color(Ink2, 0.25f));
            DrawString(font, new Vector2(x + 6, y + 10), TabNames[t], HorizontalAlignment.Left, -1, 9, on ? MapView.Paper : Ink);
            x += w + 3;
        }
    }

    // the selected slot: name, description, the extra lines, the shortcut, the action
    private void DrawDetail(Font font, float dy, int sel)
    {
        ActionRect = new Rect2();
        DrawRect(new Rect2(12, dy - 2, Size.X - 24, 1), Ink2);
        if (sel < 0 || sel >= Rows.Count) return;
        var row = Rows[sel];
        string name = row.Name.Length > 0 ? row.Name : row.Text;
        DrawString(font, new Vector2(12, dy + 12), name + (row.Count > 1 && !row.IsRecipe ? $"  ×{row.Count}" : "") + (row.Equipped ? "  — na mão" : ""),
            HorizontalAlignment.Left, Size.X - 150, 11, row.Equipped ? Jade : Ink);
        float ly = dy + 18, textW = Size.X - 150;
        int descLines = 0;
        foreach (var line in MapView.Wrap(row.Detail, textW, 9))
        {
            if (++descLines > 2 || ly > Size.Y - 30) break;
            DrawString(font, new Vector2(12, ly + 9), line, HorizontalAlignment.Left, textW, 9, Ink2); ly += 11;
        }
        if (row.Lines is { Count: > 0 } lines)
        {
            ly += 3;
            foreach (var l in lines)
            {
                if (ly > Size.Y - 24) break;
                var (col, x) = l.Kind switch
                {
                    PlainsHud.BagLineKind.Head => (Ink, 12f), PlainsHud.BagLineKind.Ok => (OkC, 16f), PlainsHud.BagLineKind.Missing => (MissC, 16f),
                    PlainsHud.BagLineKind.Better => (BetterC, 16f), PlainsHud.BagLineKind.Worse => (WorseC, 16f), _ => (Ink2, 16f),
                };
                DrawString(font, new Vector2(x, ly + 9), l.Text, HorizontalAlignment.Left, Size.X - 160, 9, col);
                ly += 11;
            }
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
            {
                float ny = dy + 34;
                foreach (var nl in MapView.Wrap(row.Need, 124, 8)) { DrawString(font, new Vector2(Size.X - 136, ny), nl, HorizontalAlignment.Left, 124, 8, MapView.Seal); ny += 10; }
            }
        }
        if (!row.IsRecipe)
        {
            string slot = row.Slot.Length > 0 ? $"No atalho {row.Slot}" : "1-6, Q ou R: pôr num atalho";
            DrawString(font, new Vector2(Size.X - 136, dy + (row.Need.Length > 0 ? 62 : 36)), slot, HorizontalAlignment.Left, 124, 8, row.Slot.Length > 0 ? Jade : Ink2);
        }
    }

    private void DrawSlot(Font font, int i, Rect2 r, bool sel)
    {
        SlotRects[i] = r;
        var row = Rows[i];
        DrawRect(r, sel ? Jade : Ink2);
        DrawRect(r.Grow(-2), sel ? SlotHi : row.IsRecipe && !row.Enabled ? new Color(SlotBg, 0.55f) : SlotBg);
        if (row.Icon.Length > 0) ItemIcons.Draw(this, row.Icon, r.GetCenter() + new Vector2(0, -2), 2);
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
        if (row.Slot.Length > 0) DrawString(font, new Vector2(r.End.X - 8, r.Position.Y + 10), row.Slot, HorizontalAlignment.Left, -1, 8, Jade);
        if (row.Equipped)
        {
            DrawRect(new Rect2(r.Position + Vector2.One * 2, new Vector2(10, 10)), Jade);
            DrawString(font, new Vector2(r.Position.X + 4, r.Position.Y + 11), "✓", HorizontalAlignment.Left, -1, 8, MapView.Paper);
        }
    }
}
