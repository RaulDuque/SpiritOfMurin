using System;
using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Game;

/// <summary>
/// The character sheet (C), in the bag's parchment (T06, option A of the board): the title with the free points, the
/// full numbers ("Vida 60% · 6/10 · Vigor … · Ki …"), the seven attributes each with a <b>"+" button at the end of the
/// line</b>, and beside them the preview of the selected attribute's next point (computed by the game from the real
/// formulas) and the proficiencies. Hover selects a line, a click on its "+" spends a point; W/S and E/Enter do the same.
/// With no free point the "+" are faded and do nothing.
/// <b>Where to change it:</b> the lines in <see cref="_Draw"/>, the "+" in <see cref="PlusRect"/>, the colours below. The
/// rows and the preview text come from <c>PlainsGame.RefreshSheet</c>; the numbers from <c>PlayerStats.Preview</c>.
/// </summary>
public partial class SheetPanel : Control
{
    /// <summary>One attribute line: short name, value, long name.</summary>
    public readonly record struct Line(string Short, int Value, string Long);

    /// <summary>The title ("Ficha — Nível 4").</summary>
    public string Title { get; set; } = "Ficha";
    /// <summary>The numbers line under the title.</summary>
    public string Header { get; set; } = "";
    /// <summary>Free points.</summary>
    public int Points { get; set; }
    /// <summary>The attribute lines.</summary>
    public List<Line> Lines { get; } = new();
    /// <summary>The preview of the selected attribute (first lines) and the proficiencies.</summary>
    public string Detail { get; set; } = "";
    /// <summary>Reads the selected line.</summary>
    public Func<int> GetSelected { get; set; } = () => 0;
    /// <summary>A line was hovered (select it).</summary>
    public event Action<int>? Hovered;
    /// <summary>A line's "+" was clicked.</summary>
    public event Action<int>? PlusClicked;
    /// <summary>The "+" buttons' rectangles (panel coordinates), by line.</summary>
    public Dictionary<int, Rect2> PlusRects { get; } = new();
    /// <summary>The lines' rectangles (panel coordinates).</summary>
    public Dictionary<int, Rect2> LineRects { get; } = new();

    private static readonly Color Ink = MapView.Ink, Ink2 = MapView.Ink2, Jade = new("#1d5e57"), SelBack = new(0.114f, 0.369f, 0.341f, 0.18f), PreviewBack = new(0.79f, 0.71f, 0.55f, 0.45f);
    /// <summary>Height of an attribute line (px).</summary>
    public const int LineH = 19;

    /// <inheritdoc />
    public override void _Ready() { MouseFilter = MouseFilterEnum.Stop; Resized += QueueRedraw; }

    /// <inheritdoc />
    public override void _Process(double delta) { if (Visible) QueueRedraw(); }

    /// <summary>The "+" of line <paramref name="i"/>.</summary>
    public static Rect2 PlusRect(int i) => new(226, 44 + i * LineH + 1, 15, 15);

    /// <inheritdoc />
    public override void _GuiInput(InputEvent e)
    {
        if (e is InputEventMouseMotion mm)
        {
            foreach (var (i, r) in LineRects) if (r.HasPoint(mm.Position) && i != GetSelected()) { Hovered?.Invoke(i); break; }
        }
        else if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mb)
        {
            foreach (var (i, r) in PlusRects) if (r.Grow(2).HasPoint(mb.Position)) { Hovered?.Invoke(i); PlusClicked?.Invoke(i); break; }
            AcceptEvent();
        }
        else if (e is InputEventMouseButton) AcceptEvent();
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
        DrawString(font, new Vector2(12, 17), $"Pontos livres: {Points}", HorizontalAlignment.Right, Size.X - 24, 11, Points > 0 ? Jade : Ink2);
        DrawString(font, new Vector2(12, 31), Header, HorizontalAlignment.Left, Size.X - 24, 9, Ink2);
        DrawRect(new Rect2(12, 36, Size.X - 24, 1), Ink2);
        int sel = GetSelected();
        PlusRects.Clear(); LineRects.Clear();
        for (int i = 0; i < Lines.Count; i++)
        {
            var l = Lines[i];
            float y = 44 + i * LineH;
            LineRects[i] = new Rect2(12, y, 236, LineH - 2);
            if (i == sel) DrawRect(LineRects[i], SelBack);
            DrawString(font, new Vector2(18, y + 12), l.Short, HorizontalAlignment.Left, -1, 10, Ink);
            DrawString(font, new Vector2(48, y + 12), l.Value.ToString(), HorizontalAlignment.Left, -1, 10, Ink);
            DrawString(font, new Vector2(66, y + 12), l.Long, HorizontalAlignment.Left, 150, 9, Ink2);
            var p = PlusRect(i); PlusRects[i] = p;
            DrawRect(p, Points > 0 ? (i == sel ? Jade : Ink2) : new Color(Ink2, 0.3f));
            DrawString(font, new Vector2(p.Position.X, p.Position.Y + 12), "+", HorizontalAlignment.Center, p.Size.X, 12, Points > 0 ? MapView.Paper : new Color(MapView.Paper, 0.6f));
        }
        // the preview (first block of the detail) and the proficiencies (after the blank line)
        float px = 262, pw = Size.X - 274, py = 44;
        string[] parts = Detail.Split("\n\n", 2);
        DrawRect(new Rect2(px, py, pw, 128), PreviewBack);
        float ly = py + 4;
        foreach (var raw in parts[0].Split('\n'))
            foreach (var line in MapView.Wrap(raw.Trim(), pw - 12, 9))
            {
                if (ly > py + 120) break;
                bool head = ly == py + 4;
                DrawString(font, new Vector2(px + 6, ly + 10), line, HorizontalAlignment.Left, pw - 12, head ? 10 : 9, head ? Ink : raw.StartsWith("  ") ? Jade : Ink); ly += head ? 13 : 11;
            }
        if (parts.Length > 1)
        {
            ly = 44 + Lines.Count * LineH + 8;
            foreach (var raw in parts[1].Split('\n'))
                foreach (var line in MapView.Wrap(raw, Size.X - 30, 8))
                {
                    if (ly > Size.Y - 22) break;
                    DrawString(font, new Vector2(14, ly + 8), line, HorizontalAlignment.Left, Size.X - 30, 8, Ink2); ly += 10;
                }
        }
        DrawString(font, new Vector2(12, Size.Y - 7), Bindings.F("[move_up]/[move_down] ou mouse: atributo · [use], Enter ou clique no +: gastar 1 ponto · [sheet] fechar"), HorizontalAlignment.Left, Size.X - 24, 8, Ink2);
    }
}
