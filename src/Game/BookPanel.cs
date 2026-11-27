using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SpiritOfMurin.Combat;
using SpiritOfMurin.Vale;

namespace SpiritOfMurin.Game;

/// <summary>
/// One page of the Livro de Técnicas as the panel shows it: the list label, the state, the command as a
/// sequence of keycaps (written with the current controls), how it is used, the real numbers in force
/// (and those of the other version), and where it came from. <see cref="Detail"/> is the page as text.
/// </summary>
public sealed class BookPage
{
    /// <summary>Stable id (the entry's name).</summary>
    public string Id { get; init; } = "";
    /// <summary>The list label (name and state).</summary>
    public string Text { get; init; } = "";
    /// <summary>The page's name.</summary>
    public string Name { get; init; } = "";
    /// <summary>"técnica", "combinação", "fragmento".</summary>
    public string Kind { get; init; } = "";
    /// <summary>"Plena", "Selada", "Enfraquecida" or "" (the base art, fragments).</summary>
    public string State { get; init; } = "";
    /// <summary>The command: ("key", action) · ("dir", word) · ("plus") · ("then") · ("unknown").</summary>
    public List<(string Kind, string Value)> Seq { get; init; } = new();
    /// <summary>How it is used.</summary>
    public string Note { get; init; } = "";
    /// <summary>The numbers that matter to play (cost, reach, effect), one per line.</summary>
    public List<string> Numbers { get; init; } = new();
    /// <summary>Where it came from.</summary>
    public string Origin { get; init; } = "";
    /// <summary>False for a sealed technique (it cannot be used now).</summary>
    public bool Enabled { get; init; } = true;
    /// <summary>A combination found by the player (the seal "feita por você").</summary>
    public bool Made { get; init; }

    /// <summary>The command written out ("J → J → J").</summary>
    public string Command => Seq.Count == 0 ? "—" : string.Join(" ", Seq.Select(s => s.Kind switch
    {
        "key" => Bindings.Key1(s.Value), "hold" => $"segurar {Bindings.Key1(s.Value)}", "release" => "soltar", "dir" => s.Value, "plus" => "+", "then" => "→", _ => "?",
    }));

    /// <summary>The whole page as text (checks, and the page under the list on a narrow screen).</summary>
    public string Detail => $"{Name} · {Kind}{(State.Length > 0 ? " · " + State : "")}\nComando: {Command}\n{Note}\n{string.Join("\n", Numbers)}\n{Origin}".Replace("\n\n", "\n").Trim();

    /// <summary>
    /// Builds the book's pages from the saved entries, the techniques in force and (in the Vale) the story,
    /// which adds the torn page of a combination not found yet. <paramref name="plains"/> changes the long
    /// step's command (Shift + direction + step there).
    /// </summary>
    public static List<BookPage> Build(IEnumerable<BookEntry> book, TechniqueSet tech, ValeState? story, bool plains)
    {
        var pages = new List<BookPage>();
        foreach (var b in book.OrderBy(b => b.Kind == "técnica" ? 0 : b.Kind == "combinação" ? 1 : 2))
        {
            var (seq, note) = HowTo(b.Name, plains);
            var nums = b.Tech.Length > 0 ? NumbersOf(b.Tech, tech) : new List<string>();
            pages.Add(new BookPage
            {
                Id = b.Name, Name = b.Name, Kind = b.Kind, State = b.State, Seq = seq, Note = note, Numbers = nums, Origin = b.Where,
                Text = b.Name + (b.State.Length > 0 ? $"  [{b.State}]" : ""), Enabled = b.State != "Selada", Made = b.Kind == "combinação",
            });
        }
        if (story != null && story.SteleRead && !story.ComboFound)
            pages.Add(new BookPage
            {
                Id = "Página rasgada", Name = "Página rasgada", Kind = "combinação", Text = "???", Enabled = false,
                Seq = new() { ("unknown", ""), ("then", ""), ("key", "cut"), ("then", ""), ("key", "ki") },
                Note = "Algo antes do corte. Talvez um passo.",
                Origin = "A inscrição além da fenda: “O ki caminha atrás da lâmina que segue o passo.” Uma técnica que quebra a guarda. Falta o primeiro movimento.",
            });
        return pages;
    }

    private static (List<(string, string)> Seq, string Note) HowTo(string name, bool plains) => name switch
    {
        "Corte" => (new() { ("key", "cut"), ("then", ""), ("key", "cut"), ("then", ""), ("key", "cut") }, "Aperte de novo durante o golpe para encadear até três. O terceiro pesa mais."),
        "Estocada" => (new() { ("dir", "frente"), ("plus", ""), ("key", "cut") }, Bindings.F("Com o alvo travado ([lock]): a lâmina avança mais longe.")),
        "Disparo de Ki" or "Onda de Ki" => (new() { ("key", "ki") }, "Uma onda de ki lançada da palma. A guarda fechada a dissipa."),
        "Passo curto" => (new() { ("dir", "trás"), ("plus", ""), ("key", "step") }, "Para frente ou para trás em relação ao alvo. Sozinho, recua."),
        "Passo Lateral" => (new() { ("dir", "lado"), ("plus", ""), ("key", "step") }, Bindings.F("Com o alvo travado ([lock]): contorna a guarda e a cega por um instante. O flanco abre.")),
        "Passo longo" => plains
            ? (new() { ("key", "sneak"), ("plus", ""), ("dir", "direção"), ("plus", ""), ("key", "step") }, "Na Planície: sem trava, segure o andar quieto. Atravessa o que o passo curto não alcança.")
            : (new() { ("dir", "direção"), ("plus", ""), ("key", "step") }, "Sem trava: um passo longo que atravessa a fenda."),
        "Passo da Lâmina Silenciosa" => (new() { ("dir", "lado"), ("plus", ""), ("key", "step"), ("then", ""), ("key", "cut"), ("then", ""), ("key", "ki") }, "Quebra a guarda. O ki segue o corte sem ruído."),
        "Reforço de Ki" => (new() { ("key", "reinforce"), ("plus", ""), ("hold", "cut") }, Bindings.F("Segure [reinforce] e aperte [cut]: o Ki percorre a lâmina. Os próximos golpes corpo a corpo saem reforçados; cada golpe gasta uma carga, mesmo errando.")),
        "Disparo de Ki Concentrado" => (new() { ("hold", "ki"), ("then", ""), ("release", "ki") }, Bindings.F("Segure [ki]: o Ki sai da barra e se junta na palma (dá para mirar). Solte para disparar. Um toque rápido continua sendo o disparo comum.")),
        _ => (new(), ""),
    };

    // the real numbers of the version in force first, then the other one for comparison
    private static List<string> NumbersOf(string id, TechniqueSet tech)
    {
        var d = tech.Data.Get(id);
        var st = tech.State(id);
        string Line(TechniqueParams p) => id switch
        {
            "ki_wave" => $"custo: {p.KiCost:0} Ki, {p.VigorCost:0} vigor · alcance {p.Range:0} px · dano {p.Damage}",
            "side_step" => $"custo: {p.KiCost:0} Ki, {p.VigorCost:0} vigor · contorna {p.Angle:0}° · raio até {p.RadiusMax:0} px",
            "dash" => $"custo: {p.KiCost:0} Ki, {p.VigorCost:0} vigor · {p.Distance:0} px",
            "thrust" => $"custo: {p.KiCost:0} Ki, {p.VigorCost:0} vigor · alcance {p.Reach:0} px · dano {p.Damage}",
            "lamina" => $"dano {p.Damage} · alcance {p.Range:0} px · guarda quebrada {p.Stagger:0.#} s",
            "ki_reinforce" => $"custo: {p.KiCost:0} Ki, uma vez por ativação · {p.Charges} golpes reforçados · +{p.Bonus * 100:0}% de dano · {p.Chance * 100:0}% de chance de romper a guarda atingida ({p.Stagger:0.#} s aberta)",
            "ki_charge" => $"carga completa: {p.KiCost:0} Ki no total (já inclui o disparo){(p.VigorCost > 0 ? $" + {p.VigorCost:0} vigor" : "")} · dano {p.Damage} · alcance {p.Range:0} px · rompe a guarda ({p.Stagger:0.#} s aberta) | carga parcial: dano proporcional ao Ki investido, sem ruptura",
            _ => "",
        };
        return st switch
        {
            TechniqueState.Sealed => new() { $"Selada desde o despertar: volta enfraquecida no nível {tech.Data.UnlockLevel}.", "Plena (no Vale): " + Line(d.Full), $"Enfraquecida (nível {tech.Data.UnlockLevel}): " + Line(d.Weak) },
            TechniqueState.Weakened => new() { "Enfraquecida, em uso: " + Line(d.Weak), "Plena era: " + Line(d.Full) },
            TechniqueState.Full => new() { "Plena: " + Line(d.Full) },
            _ => new(),
        };
    }
}

/// <summary>
/// The Livro de Técnicas panel, in the prototype's look (a parchment scroll): on a wide screen a list on
/// the left — grouped into técnicas, combinações, fragmentos — and the selected page on the right; on a
/// narrow one the list goes on top and the page below. The page shows name, kind, state, the command as
/// keycaps (current controls), how it is used, the real numbers, and the origin. Mouse: hover or click a
/// line to turn to it, wheel to scroll the page. The game moves the selection with the walk keys and closes
/// it with the book key or Esc.
/// </summary>
public partial class BookPanel : Control
{
    /// <summary>The pages.</summary>
    public List<BookPage> Pages { get; } = new();
    /// <summary>The selected page.</summary>
    public int Selected { get; private set; }
    /// <summary>A line was clicked (the page index).</summary>
    public event Action<int>? Clicked;
    /// <summary>The selected page as text.</summary>
    public string PageText => Selected < Pages.Count ? Pages[Selected].Detail : "";
    /// <summary>True when the list and the page sit side by side (wide panel).</summary>
    public bool SideBySide => Size.X >= 420;
    /// <summary>Lines of the list's rectangles (panel coordinates), for clicks and checks.</summary>
    public Dictionary<int, Rect2> LineRects { get; } = new();

    private float _pageScroll, _pageHeight, _listScroll;
    private Rect2 _pageRect, _listRect;

    private static readonly Color Ink = MapView.Ink, Ink2 = MapView.Ink2, Jade = new("#1d5e57"), Amber = new("#8a5a1a"), Seal = MapView.Seal;

    /// <summary>Replaces the pages (keeps the selection when it still exists).</summary>
    public void SetPages(List<BookPage> pages)
    {
        string keep = Selected < Pages.Count ? Pages[Selected].Id : "";
        Pages.Clear(); Pages.AddRange(pages);
        int i = Pages.FindIndex(p => p.Id == keep);
        Selected = Mathf.Clamp(i >= 0 ? i : Selected, 0, Mathf.Max(0, Pages.Count - 1));
        QueueRedraw();
    }

    /// <summary>Turns the page by <paramref name="d"/>.</summary>
    public void Move(int d) { if (Pages.Count == 0) return; Select(Mathf.PosMod(Selected + d, Pages.Count)); }

    /// <summary>Turns to a page.</summary>
    public void Select(int i) { if (i == Selected) return; Selected = Mathf.Clamp(i, 0, Mathf.Max(0, Pages.Count - 1)); _pageScroll = 0; QueueRedraw(); }

    /// <summary>Scrolls the page text (positive: down).</summary>
    public void ScrollPage(float px) { _pageScroll = Mathf.Clamp(_pageScroll + px, 0, Mathf.Max(0, _pageHeight - _pageRect.Size.Y)); QueueRedraw(); }

    /// <inheritdoc />
    public override void _Ready() { MouseFilter = MouseFilterEnum.Stop; Resized += QueueRedraw; }

    /// <inheritdoc />
    public override void _GuiInput(InputEvent e)
    {
        if (e is InputEventMouseMotion mm)
        {
            foreach (var (i, r) in LineRects) if (r.HasPoint(mm.Position) && i != Selected) { Select(i); break; }
        }
        else if (e is InputEventMouseButton { Pressed: true } mb)
        {
            if (mb.ButtonIndex is MouseButton.WheelDown or MouseButton.WheelUp)
            {
                float d = mb.ButtonIndex == MouseButton.WheelDown ? 14 : -14;
                if (_listRect.HasPoint(mb.Position) && !SideBySide) _listScroll = Mathf.Max(0, _listScroll + d); else ScrollPage(d);
                QueueRedraw();
            }
            else if (mb.ButtonIndex == MouseButton.Left)
                foreach (var (i, r) in LineRects) if (r.HasPoint(mb.Position)) { Select(i); Clicked?.Invoke(i); break; }
            AcceptEvent();
        }
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        var font = ThemeDB.FallbackFont;
        // the scroll: rollers, parchment, edge, specks
        DrawRect(new Rect2(-6, -8, Size.X + 12, 8), MapView.Roller); DrawRect(new Rect2(-6, Size.Y, Size.X + 12, 8), MapView.Roller);
        DrawRect(new Rect2(-6, -7, Size.X + 12, 2), MapView.RollerHi); DrawRect(new Rect2(-6, Size.Y + 1, Size.X + 12, 2), MapView.RollerHi);
        DrawRect(new Rect2(Vector2.Zero, Size), MapView.Paper);
        DrawRect(new Rect2(0, 0, Size.X, 3), MapView.PaperEdge); DrawRect(new Rect2(0, Size.Y - 3, Size.X, 3), MapView.PaperEdge);
        DrawRect(new Rect2(0, 0, 3, Size.Y), MapView.PaperEdge); DrawRect(new Rect2(Size.X - 3, 0, 3, Size.Y), MapView.PaperEdge);
        for (int i = 0; i < 60; i++) { uint h = (uint)(i * 2654435761u); DrawRect(new Rect2(h % (uint)Mathf.Max(1, Size.X), (h >> 8) % (uint)Mathf.Max(1, Size.Y), 3 + h % 6, 1), MapView.PaperShade); }

        float listW = SideBySide ? Mathf.Clamp(Size.X * 0.32f, 150, 230) : Size.X - 24;
        float footer = 14;
        _listRect = SideBySide ? new Rect2(10, 28, listW, Size.Y - 28 - footer - 6) : new Rect2(12, 28, listW, Mathf.Round((Size.Y - 40) * 0.36f));
        _pageRect = SideBySide ? new Rect2(listW + 26, 10, Size.X - listW - 38, Size.Y - footer - 16) : new Rect2(12, _listRect.End.Y + 8, Size.X - 24, Size.Y - _listRect.End.Y - footer - 14);
        DrawString(font, new Vector2(12, 18), "Livro de Técnicas", HorizontalAlignment.Left, listW, 12, Ink);
        DrawRect(new Rect2(12, 23, listW - 4, 1), Ink2);
        if (SideBySide) DrawRect(new Rect2(listW + 16, 10, 1, Size.Y - footer - 20), Ink2);
        else DrawRect(new Rect2(12, _listRect.End.Y + 3, Size.X - 24, 1), Ink2);
        DrawList(font);
        DrawPage(font);
        string help = Bindings.F("[move_up]/[move_down] ou mouse: folhear · roda: rolar a página · [book] ou Esc: fechar");
        DrawString(font, new Vector2(12, Size.Y - 6), help, HorizontalAlignment.Left, Size.X - 24, 8, Ink2);
    }

    private void DrawList(Font font)
    {
        LineRects.Clear();
        if (Pages.Count == 0) { DrawString(font, _listRect.Position + new Vector2(4, 12), "(vazio)", HorizontalAlignment.Left, _listRect.Size.X, 9, Ink2); return; }
        // keep the selected line visible
        float total = 0; string last = "";
        var ys = new List<float>();
        foreach (var p in Pages) { if (p.Kind != last) { total += 11; last = p.Kind; } ys.Add(total); total += 13; }
        float sy = ys[Selected];
        if (sy - _listScroll > _listRect.Size.Y - 16) _listScroll = sy - _listRect.Size.Y + 16;
        if (sy - _listScroll < 0) _listScroll = Mathf.Max(0, sy - 11);
        _listScroll = Mathf.Clamp(_listScroll, 0, Mathf.Max(0, total - _listRect.Size.Y));
        last = "";
        float y = _listRect.Position.Y - _listScroll;
        for (int i = 0; i < Pages.Count; i++)
        {
            var p = Pages[i];
            if (p.Kind != last)
            {
                last = p.Kind;
                if (y > _listRect.Position.Y - 2 && y < _listRect.End.Y - 8)
                    DrawString(font, new Vector2(_listRect.Position.X + 4, y + 9), p.Kind == "técnica" ? "técnicas" : p.Kind == "combinação" ? "combinações" : "fragmentos", HorizontalAlignment.Left, -1, 8, Ink2);
                y += 11;
            }
            var r = new Rect2(_listRect.Position.X, y, _listRect.Size.X, 13);
            if (y >= _listRect.Position.Y - 1 && r.End.Y <= _listRect.End.Y + 1)
            {
                LineRects[i] = r;
                bool sel = i == Selected;
                if (sel) { DrawRect(r, new Color(0.11f, 0.37f, 0.34f, 0.16f)); DrawRect(new Rect2(r.Position, new Vector2(2, r.Size.Y)), Jade); }
                var col = !p.Enabled && p.Id != "Página rasgada" ? Ink2 : p.Kind == "combinação" ? Jade : p.Kind == "fragmento" ? Amber : Ink;
                DrawString(font, new Vector2(r.Position.X + 7, y + 10), p.Id == "Página rasgada" ? "???" : p.Name, HorizontalAlignment.Left, r.Size.X - 52, 9, col);
                if (p.State.Length > 0) Badge(font, new Vector2(r.End.X - 4, y + 2), p.State, small: true);
            }
            y += 13;
        }
        if (total - _listScroll > _listRect.Size.Y) DrawString(font, new Vector2(_listRect.End.X - 10, _listRect.End.Y + 2), "↓", HorizontalAlignment.Left, -1, 9, Ink2);
    }

    // a state badge, right-aligned at `at`: Plena (jade), Selada (a red seal), Enfraquecida (amber)
    private void Badge(Font font, Vector2 at, string state, bool small)
    {
        string t = small ? (state == "Enfraquecida" ? "enfraq." : state.ToLowerInvariant()) : state;
        int size = small ? 7 : 9;
        float w = font.GetStringSize(t, HorizontalAlignment.Left, -1, size).X + 6;
        var col = state == "Selada" ? Seal : state == "Enfraquecida" ? Amber : Jade;
        var r = new Rect2(at.X - w, at.Y, w, size + 3);
        DrawRect(r, col);
        DrawString(font, new Vector2(r.Position.X + 3, r.Position.Y + size + 0.5f), t, HorizontalAlignment.Left, -1, size, MapView.Paper);
    }

    private void DrawPage(Font font)
    {
        if (Selected >= Pages.Count) return;
        var p = Pages[Selected];
        float cx = _pageRect.GetCenter().X, w = _pageRect.Size.X;
        float y0 = _pageRect.Position.Y - _pageScroll, y = y0;
        bool Visible(float yy, float h) => yy >= _pageRect.Position.Y - 1 && yy + h <= _pageRect.End.Y + 1;
        void Center(string t, int size, Color c) { if (Visible(y, size + 2)) DrawString(font, new Vector2(_pageRect.Position.X, y + size), t, HorizontalAlignment.Center, w, size, c); y += size + 4; }
        void Para(string t, int size, Color c)
        {
            foreach (var l in MapView.Wrap(t, w - 16, size)) { if (Visible(y, size + 2)) DrawString(font, new Vector2(_pageRect.Position.X + 8, y + size), l, HorizontalAlignment.Center, w - 16, size, c); y += size + 3; }
        }
        Center(p.Id == "Página rasgada" ? "Página rasgada" : p.Name, 13, p.Kind == "combinação" ? Jade : Ink);
        Center(p.Kind, 8, Ink2);
        if (p.State.Length > 0 && Visible(y, 14)) { float bw = font.GetStringSize(p.State, HorizontalAlignment.Left, -1, 9).X + 6; Badge(font, new Vector2(cx + bw / 2, y), p.State, small: false); }
        if (p.State.Length > 0) y += 16;
        if (p.Seq.Count > 0)
        {
            if (Visible(y, 34)) { DrawRect(new Rect2(_pageRect.Position.X + 6, y, w - 12, 30), MapView.PaperShade); DrawSeq(font, p.Seq, cx, y + 15); }
            y += 36;
        }
        if (p.Note.Length > 0) { Para(p.Note, 9, Ink); y += 3; }
        foreach (var n in p.Numbers) Para(n, 8, n.StartsWith("Selada") ? Seal : n.StartsWith("Enfraquecida") ? Amber : Ink);
        if (p.Numbers.Count > 0) y += 3;
        if (p.Origin.Length > 0) Para(p.Origin, 8, Ink2);
        _pageHeight = y - y0;
        if (_pageHeight - _pageScroll > _pageRect.Size.Y + 2) DrawString(font, new Vector2(_pageRect.End.X - 60, _pageRect.End.Y + 9), "↓ role a página", HorizontalAlignment.Left, -1, 7, Ink2);
        if (p.Made && p.Enabled && Visible(_pageRect.End.Y - 42, 40))
        {
            float sx = _pageRect.End.X - 44, sy = _pageRect.End.Y - 42;
            DrawRect(new Rect2(sx, sy, 38, 38), Seal); DrawRect(new Rect2(sx + 3, sy + 3, 32, 32), MapView.Paper); DrawRect(new Rect2(sx + 5, sy + 5, 28, 28), Seal);
            var light = new Color("#f0dcc0");
            DrawString(font, new Vector2(sx, sy + 15), "feita", HorizontalAlignment.Center, 38, 8, light);
            DrawString(font, new Vector2(sx, sy + 23), "por", HorizontalAlignment.Center, 38, 7, light);
            DrawString(font, new Vector2(sx, sy + 31), "você", HorizontalAlignment.Center, 38, 8, light);
        }
    }

    // the keycaps of a command, centred: keys, direction marks, + and arrows (the prototype's drawSeq)
    private void DrawSeq(Font font, List<(string Kind, string Value)> seq, float cx, float y)
    {
        var widths = seq.Select(s => s.Kind switch
        {
            "key" => Mathf.Max(18, font.GetStringSize(Bindings.Key1(s.Value), HorizontalAlignment.Left, -1, 9).X + 10),
            "hold" or "release" => Mathf.Max(34, font.GetStringSize(Bindings.Key1(s.Value), HorizontalAlignment.Left, -1, 9).X + 12),
            "dir" => Mathf.Max(30, font.GetStringSize(s.Value, HorizontalAlignment.Left, -1, 8).X + 8),
            "unknown" => 22f, _ => 14f,
        }).ToList();
        float total = widths.Sum() + (widths.Count - 1) * 5, x = Mathf.Round(cx - total / 2);
        for (int i = 0; i < seq.Count; i++)
        {
            var (k, v) = seq[i]; float bw = widths[i];
            if (k == "key")
            {
                string label = Bindings.Key1(v);
                DrawRect(new Rect2(x - 1, y - 9, bw + 2, 19), Ink); DrawRect(new Rect2(x, y - 8, bw, 15), new Color("#efe4c8")); DrawRect(new Rect2(x, y + 5, bw, 2), new Color("#c9b78f"));
                DrawString(font, new Vector2(x, y + 3), label, HorizontalAlignment.Center, bw, 9, new Color("#1c1712"));
            }
            else if (k is "hold" or "release")
            {
                string label = Bindings.Key1(v);
                DrawRect(new Rect2(x - 1, y - 9, bw + 2, 19), Ink); DrawRect(new Rect2(x, y - 8, bw, 15), k == "hold" ? new Color("#d4fbf6") : new Color("#efe4c8")); DrawRect(new Rect2(x, y + 5, bw, 2), new Color("#c9b78f"));
                DrawString(font, new Vector2(x, y + 3), label, HorizontalAlignment.Center, bw, 9, new Color("#1c1712"));
                DrawString(font, new Vector2(x - 6, y + 17), k == "hold" ? "segurar" : "soltar", HorizontalAlignment.Center, bw + 12, 7, Ink2);
            }
            else if (k == "unknown") { DrawRect(new Rect2(x, y - 9, bw, 18), Ink2); DrawRect(new Rect2(x + 1, y - 8, bw - 2, 16), new Color("#b8a37a")); DrawString(font, new Vector2(x, y + 4), "?", HorizontalAlignment.Center, bw, 11, Ink); }
            else if (k == "dir")
            {
                float mx = x + bw / 2;
                if (v == "lado") { DrawRect(new Rect2(mx - 10, y - 6, 20, 2), Jade); DrawRect(new Rect2(mx + 6, y - 9, 2, 8), Jade); DrawRect(new Rect2(mx - 8, y - 9, 2, 8), Jade); }
                else if (v == "trás") { DrawRect(new Rect2(mx - 1, y - 10, 2, 10), Jade); DrawRect(new Rect2(mx - 4, y - 2, 8, 2), Jade); }
                else { DrawRect(new Rect2(mx - 1, y - 8, 2, 10), Jade); DrawRect(new Rect2(mx - 4, y - 9, 8, 2), Jade); DrawRect(new Rect2(mx - 3, y - 11, 6, 2), Jade); }
                DrawString(font, new Vector2(x, y + 10), v, HorizontalAlignment.Center, bw, 7, Ink2);
            }
            else if (k == "plus") { DrawRect(new Rect2(x + 3, y - 1, 8, 2), Ink); DrawRect(new Rect2(x + 6, y - 4, 2, 8), Ink); }
            else if (k == "then") { DrawRect(new Rect2(x + 1, y - 1, 10, 2), Ink); DrawRect(new Rect2(x + 8, y - 4, 2, 2), Ink); DrawRect(new Rect2(x + 10, y - 2, 2, 4), Ink); DrawRect(new Rect2(x + 8, y + 2, 2, 2), Ink); }
            x += bw + 5;
        }
    }
}
