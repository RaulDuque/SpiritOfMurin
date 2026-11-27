using System;
using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Game;

/// <summary>
/// Esc → Controles: every action with its name in Portuguese, its main input and its second one (a key or
/// a mouse button), each with an "Alterar" button. Choosing Alterar waits for the next key or mouse button
/// and gives it to that action at once (Esc cancels); when another action already used that input the two
/// trade places and a line says so. "Restaurar padrões" brings the project's controls back. Mouse: hover
/// and click; keyboard: up/down choose the line, left/right the column, E or Enter change, Esc back.
/// While it is open, it takes every input, so nothing reaches the world.
/// </summary>
public partial class ControlsPanel : Control
{
    /// <summary>The panel closed (back to the pause menu).</summary>
    public event Action? Closed;
    /// <summary>"Lista completa de comandos" was chosen.</summary>
    public event Action? ListRequested;

    /// <summary>Selected line (actions, then the three buttons).</summary>
    public int Row { get; private set; }
    /// <summary>Selected column (0 main, 1 second).</summary>
    public int Col { get; private set; }
    /// <summary>The action waiting for a new input (null when not waiting).</summary>
    public string? Waiting { get; private set; }
    /// <summary>The slot waiting.</summary>
    public int WaitingSlot { get; private set; }
    /// <summary>The message line (the result of the last change, or what to do).</summary>
    public string Message { get; private set; } = "";
    /// <summary>Rectangles of the "Alterar" buttons (panel coordinates) by (action index, slot), for clicks and checks.</summary>
    public Dictionary<(int Row, int Col), Rect2> ChangeRects { get; } = new();
    /// <summary>Rectangles of the bottom buttons by id ("restore", "list", "back").</summary>
    public Dictionary<string, Rect2> ButtonRects { get; } = new();

    private static readonly string[] Buttons = { "aim", "restore", "list", "back" };
    private int Rows => Bindings.Actions.Length + Buttons.Length;
    private float _scroll;

    /// <summary>Opens the screen.</summary>
    public void Open() { Visible = true; Waiting = null; Row = 0; Col = 0; Message = Bindings.F("Escolha \"Alterar\" e aperte a nova tecla ou botão. Esc cancela."); QueueRedraw(); }

    /// <summary>Starts waiting for a new input for an action's slot.</summary>
    public void BeginChange(int row, int slot)
    {
        if (row < 0 || row >= Bindings.Actions.Length) return;
        Row = row; Col = slot; Waiting = Bindings.Actions[row].Id; WaitingSlot = slot;
        Message = $"Aperte a nova entrada para \"{Bindings.Actions[row].Name}\" ({(slot == 0 ? "principal" : "segunda")}). Esc cancela.";
        QueueRedraw();
    }

    private void Close() { Visible = false; Waiting = null; Closed?.Invoke(); }

    private void Act(int row)
    {
        if (row < Bindings.Actions.Length) { BeginChange(row, Col); return; }
        switch (Buttons[row - Bindings.Actions.Length])
        {
            case "aim":
                Aim.ByMouse = !Aim.ByMouse;
                Message = Aim.ByMouse ? "Mira pelo mouse: sem alvo travado, o golpe e o Qi vão para o cursor." : "Mira pela direção do personagem (só teclado): o golpe vai para onde você anda.";
                break;
            case "restore": Bindings.Restore(); Message = "Controles restaurados para os padrões."; break;
            case "list": ListRequested?.Invoke(); break;
            case "back": Close(); break;
        }
        QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Ready() { MouseFilter = MouseFilterEnum.Stop; }

    /// <inheritdoc />
    public override void _Input(InputEvent e)
    {
        if (!Visible) return;
        if (Waiting != null)
        {
            bool press = e is InputEventKey { Pressed: true, Echo: false } || e is InputEventMouseButton { Pressed: true } mb0 && mb0.ButtonIndex is not (MouseButton.WheelUp or MouseButton.WheelDown or MouseButton.WheelLeft or MouseButton.WheelRight);
            if (press)
            {
                if (e is InputEventKey { PhysicalKeycode: Key.Escape } or InputEventKey { Keycode: Key.Escape }) { Message = "Troca cancelada."; Waiting = null; }
                else
                {
                    var (ok, msg) = Bindings.Assign(Waiting, WaitingSlot, Bindings.Encode(e));
                    Message = msg; Waiting = null;
                    if (!ok) Message = "Não trocou: " + msg;
                }
                QueueRedraw();
            }
            GetViewport().SetInputAsHandled();
            return;
        }
        if (e is InputEventKey { Pressed: true } k)
        {
            if (k.IsActionPressed("ui_cancel") || k.IsActionPressed("menu") || k.PhysicalKeycode == Key.Escape) Close();
            else if (k.IsActionPressed("ui_up") || k.IsActionPressed("move_up")) { Row = Mathf.PosMod(Row - 1, Rows); }
            else if (k.IsActionPressed("ui_down") || k.IsActionPressed("move_down")) { Row = Mathf.PosMod(Row + 1, Rows); }
            else if (k.IsActionPressed("ui_left") || k.IsActionPressed("move_left")) Col = 0;
            else if (k.IsActionPressed("ui_right") || k.IsActionPressed("move_right")) Col = 1;
            else if (k.IsActionPressed("ui_accept") || k.IsActionPressed("use")) Act(Row);
            QueueRedraw();
            GetViewport().SetInputAsHandled();
        }
        else if (e is InputEventKey) GetViewport().SetInputAsHandled(); // mouse buttons go on to the panel's own GUI input (clicks), and the game ignores them while it is open
    }

    /// <inheritdoc />
    public override void _GuiInput(InputEvent e)
    {
        if (Waiting != null) return;
        if (e is InputEventMouseMotion mm)
        {
            foreach (var ((r, c), rect) in ChangeRects) if (rect.HasPoint(mm.Position) && (r != Row || c != Col)) { Row = r; Col = c; QueueRedraw(); }
            for (int i = 0; i < Buttons.Length; i++) if (ButtonRects.TryGetValue(Buttons[i], out var br) && br.HasPoint(mm.Position) && Row != Bindings.Actions.Length + i) { Row = Bindings.Actions.Length + i; QueueRedraw(); }
        }
        else if (e is InputEventMouseButton { Pressed: true } mb)
        {
            if (mb.ButtonIndex == MouseButton.WheelDown) { _scroll += 12; QueueRedraw(); }
            else if (mb.ButtonIndex == MouseButton.WheelUp) { _scroll = Mathf.Max(0, _scroll - 12); QueueRedraw(); }
            else if (mb.ButtonIndex == MouseButton.Left)
            {
                foreach (var ((r, c), rect) in ChangeRects) if (rect.HasPoint(mb.Position)) { Col = c; BeginChange(r, c); AcceptEvent(); return; }
                for (int i = 0; i < Buttons.Length; i++) if (ButtonRects.TryGetValue(Buttons[i], out var br) && br.HasPoint(mb.Position)) { Row = Bindings.Actions.Length + i; Act(Row); AcceptEvent(); return; }
            }
            AcceptEvent();
        }
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        var font = ThemeDB.FallbackFont;
        Color bg = new(0.07f, 0.075f, 0.06f, 0.97f), text = new("#ece5d0"), dim = new("#a39e8c"), sel = new("#c9a14a"), frame = new("#4e3824"), jade = new("#6fd8d2"), warn = new("#e8834c");
        DrawRect(new Rect2(Vector2.Zero, Size), bg);
        DrawRect(new Rect2(Vector2.Zero, Size), frame, false, 1);
        DrawString(font, new Vector2(10, 14), "Controles", HorizontalAlignment.Left, -1, 11, text);
        var msgLines = MapView.Wrap(Message, Size.X - 20, 8);
        for (int i = 0; i < Math.Min(2, msgLines.Count); i++)
            DrawString(font, new Vector2(10, 26 + i * 10), msgLines[i], HorizontalAlignment.Left, Size.X - 20, 8, Waiting != null ? jade : Message.StartsWith("Não") ? warn : dim);
        float nameW = Mathf.Round(Size.X * 0.42f), colW = Mathf.Round((Size.X - nameW - 24) / 2);
        float top = 50, rowH = 12, listH = Size.Y - top - 44;
        DrawString(font, new Vector2(10, top - 3), "Ação", HorizontalAlignment.Left, -1, 8, dim);
        DrawString(font, new Vector2(10 + nameW, top - 3), "Principal", HorizontalAlignment.Left, -1, 8, dim);
        DrawString(font, new Vector2(14 + nameW + colW, top - 3), "Segunda (tecla ou mouse)", HorizontalAlignment.Left, -1, 8, dim);
        DrawRect(new Rect2(8, top, Size.X - 16, 1), frame);
        // keep the selected line in view
        float selY = Mathf.Min(Row, Bindings.Actions.Length - 1) * rowH;
        if (selY - _scroll > listH - rowH) _scroll = selY - listH + rowH;
        if (selY - _scroll < 0) _scroll = selY;
        _scroll = Mathf.Clamp(_scroll, 0, Mathf.Max(0, Bindings.Actions.Length * rowH - listH));
        ChangeRects.Clear();
        for (int i = 0; i < Bindings.Actions.Length; i++)
        {
            float y = top + 3 + i * rowH - _scroll;
            if (y < top || y + rowH > top + listH + 2) continue;
            var a = Bindings.Actions[i];
            bool rowSel = i == Row;
            if (rowSel) DrawRect(new Rect2(8, y, Size.X - 16, rowH), new Color(sel, 0.12f));
            DrawString(font, new Vector2(12, y + 9), (rowSel ? "› " : "  ") + a.Name, HorizontalAlignment.Left, nameW - 6, 8, rowSel ? sel : text);
            for (int s = 0; s < 2; s++)
            {
                float x = 10 + nameW + s * (colW + 4);
                bool cell = rowSel && Col == s, waiting = Waiting == a.Id && WaitingSlot == s;
                string label = waiting ? "aperte…" : Bindings.Label(a.Id, s);
                var chip = new Rect2(x, y + 1, colW - 50, rowH - 2);
                DrawRect(chip, waiting ? new Color(jade, 0.25f) : new Color(0, 0, 0, 0.35f));
                DrawRect(chip, cell ? sel : frame, false, 1);
                DrawString(font, new Vector2(chip.Position.X + 3, y + 9), label, HorizontalAlignment.Left, chip.Size.X - 4, 8, waiting ? jade : text);
                var btn = new Rect2(chip.End.X + 3, y + 1, 44, rowH - 2);
                DrawRect(btn, cell ? new Color(sel, 0.85f) : new Color(frame, 0.9f));
                DrawString(font, new Vector2(btn.Position.X, y + 9), "Alterar", HorizontalAlignment.Center, btn.Size.X, 8, cell ? new Color("#16140f") : text);
                ChangeRects[(i, s)] = new Rect2(chip.Position, new Vector2(btn.End.X - chip.Position.X, chip.Size.Y));
            }
        }
        // the buttons at the bottom
        ButtonRects.Clear();
        string[] labels = { Aim.ByMouse ? "Mira: pelo mouse" : "Mira: direção do personagem", "Restaurar padrões", "Lista completa de comandos", "Voltar" };
        float bx = 10, by = Size.Y - 34;
        DrawRect(new Rect2(8, by - 4, Size.X - 16, 1), frame);
        for (int i = 0; i < Buttons.Length; i++)
        {
            float w = font.GetStringSize(labels[i], HorizontalAlignment.Left, -1, 9).X + 14;
            var r = new Rect2(bx, by, w, 14);
            bool s = Row == Bindings.Actions.Length + i;
            DrawRect(r, s ? new Color(sel, 0.85f) : new Color(frame, 0.9f));
            DrawString(font, new Vector2(r.Position.X, by + 10), labels[i], HorizontalAlignment.Center, w, 9, s ? new Color("#16140f") : text);
            ButtonRects[Buttons[i]] = r;
            bx += w + 8;
        }
        DrawString(font, new Vector2(10, Size.Y - 8), Bindings.F("setas ou [move_up]/[move_down]: linha · esquerda/direita: coluna · E ou Enter: alterar · Esc: voltar · as teclas ficam salvas"), HorizontalAlignment.Left, Size.X - 20, 7, dim);
    }
}
