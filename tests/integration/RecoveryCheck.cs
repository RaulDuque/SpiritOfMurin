using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Campaign;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Combat;
using SpiritOfMurin.Game;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;
using SpiritOfMurin.Vale;

namespace SpiritOfMurin.Tests;

/// <summary>
/// The prototype's recovery (pedido 11), through the main scene and real keys and mouse. Phases, each its
/// own run (process):
/// <list type="bullet">
/// <item><c>--phase=vale</c>: Novo jogo → the Vale; the bridge's planks are drawn (not water); the minimap
/// shows while walking and hides behind panels; the map before the encounter has no clue; the bridge
/// disciple's guard parries a frontal cut and the result matches the guard arc (assist on and off); the
/// retreat writes its clue; M answers "Onde estou?" and "Última pista"; the manual updates the map and the
/// text; the book shows its pages side by side; meditating saves it all.</item>
/// <item><c>--phase=vale2</c>: Continuar keeps the missions, the clues and the last clue; the rematch from the
/// flank (side step) lands, coherent with the open arc.</item>
/// <item><c>--phase=controls</c> then <c>--phase=controls2</c>: Esc → Controles by mouse, "Alterar" on the cut,
/// U pressed: U cuts at once and the footer says U; a used key is swapped with a message; closing the panels
/// with clicks never cuts; in a new process U still cuts; "Restaurar padrões" brings J back.</item>
/// <item><c>--phase=assist</c> then <c>--phase=assist2</c>: Esc → Modo de assistência → on; saved; still on in a
/// new process; off again.</item>
/// <item><c>--phase=book</c>: the Planície book with sealed techniques (numbers of both versions), level 3
/// brings them back weakened (real numbers), a narrow panel stacks the list over the page and scrolls.</item>
/// <item><c>--phase=layout</c>: the minimap beside the seed and the bars at three window sizes; the mouse maps to
/// the corners of the picture after window, maximized and full screen.</item>
/// </list>
/// Teleports place the swordsman (named in the checks). Files: <c>user://teste_rec*.json</c>; preferences
/// in <c>user://teste_rec_prefs.json</c> (pass <c>--prefs=user://teste_rec_prefs.json</c>).
/// </summary>
public partial class RecoveryCheck : Node
{
    private const string SavePath = "user://teste_rec.json", PlainsSave = "user://teste_rec_planicie.json";
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private string _phase = "vale";
    private string? _shots;
    private int _slow;
    private CampaignRoot _c = null!;
    private PlainsGame? _pg;

    /// <inheritdoc />
    public override void _Ready()
    {
        Game.Aim.UseForSession(false); // written for aiming by facing: the real cursor over the window must not steer the cuts
        foreach (string a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--phase=")) _phase = a["--phase=".Length..];
            if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..];
            if (a == "--lento") _slow = 30;
        }
        GD.Print($"window on screen {DisplayServer.WindowGetCurrentScreen()} of {DisplayServer.GetScreenCount()} ({DisplayServer.GetName()})");
        switch (_phase)
        {
            case "vale": SaveGame.Erase(SavePath); EraseFile(Prefs.Path); Bindings.Init(); Assist.Reload(); Launch("title"); _ = Vale(); break;
            case "vale2": Launch("title"); _ = Vale2(); break;
            case "controls": SaveGame.Erase(SavePath); EraseFile(Prefs.Path); Bindings.Init(); Assist.Reload(); Launch("new"); _ = Controls(); break;
            case "controls2": Launch("new"); _ = Controls2(); break;
            case "assist": EraseFile(Prefs.Path); Assist.Reload(); SaveGame.Erase(SavePath); Launch("new"); _ = AssistOn(); break;
            case "assist2": Launch("new"); _ = AssistAgain(); break;
            case "book": _ = Book(); break;
            case "layout": SaveGame.Erase(SavePath); Launch("new"); _ = Layout(); break;
            case "video": SaveGame.Erase(SavePath); EraseFile(Prefs.Path); Bindings.Init(); Assist.Reload(); Launch("new"); _ = Video(); break;
        }
    }

    private static void EraseFile(string path) { if (Godot.FileAccess.FileExists(path)) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(path)); }

    private Vector2 _lastMotion;

    /// <inheritdoc />
    public override void _Input(InputEvent e) { if (e is InputEventMouseMotion mm) _lastMotion = mm.Position; }

    private void Launch(string how)
    {
        _c = GD.Load<PackedScene>("res://src/Campaign/Campaign.tscn").Instantiate<CampaignRoot>();
        _c.SavePath = SavePath; _c.ShowIntro = false; _c.StartWith = how;
        AddChild(_c);
    }

    // ------------------------------------------------------------------ helpers
    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private async Task Flush() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static void Press(Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });
    private async Task Tap(Key k) { Press(k, true); await Flush(); await Frames(2); Press(k, false); await Flush(); await Frames(2 + _slow / 3); }
    private async Task Chord(Key a, Key b) { Press(a, true); await Flush(); await Frames(1); Press(b, true); await Flush(); await Frames(2); Press(b, false); Press(a, false); await Flush(); await Frames(3); }
    private async Task Hold(Key k, int frames) { Press(k, true); await Flush(); await Frames(frames); Press(k, false); await Flush(); await Frames(2); }
    private void Finish() { int f = _results.Count(r => !r.Ok); GD.Print($"recovery {_phase}: {_results.Count - f}/{_results.Count} OK"); DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed); GetTree().Quit(f == 0 ? 0 : 1); }
    private ValeGame V => _c.Vale!;
    private Player P => V.Player;

    private async Task<Image?> Shot(string name)
    {
        if (DisplayServer.GetName() == "headless") return null;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        var img = GetViewport().GetTexture().GetImage();
        if (_shots != null) { DirAccess.MakeDirRecursiveAbsolute(_shots); img.SavePng($"{_shots}/{name}.png"); }
        return img;
    }

    private async Task Teleport(Vector2 at, Vector2 face) { P.GlobalPosition = at; P.Face(face); P.GetNode<Camera2D>("Camera").ResetSmoothing(); await Frames(4 + _slow); }

    // a point of a control, in window coordinates (what a mouse event carries)
    private Vector2 ToWindow(Control c, Vector2 local) => GetViewport().GetFinalTransform() * (c.GetGlobalTransformWithCanvas() * local);

    private async Task Click(Vector2 at)
    {
        Input.ParseInputEvent(new InputEventMouseMotion { Position = at, GlobalPosition = at }); await Flush(); await Frames(2);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = at, GlobalPosition = at }); await Flush(); await Frames(3);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false, Position = at, GlobalPosition = at }); await Flush(); await Frames(4 + _slow / 2);
    }

    private async Task ClickPause(string id)
    {
        int i = V.Hud.PauseRows.FindIndex(r => r.Id == id);
        var rect = V.Hud.RowScreenRect("pause", i);
        if (rect is { } r) await Click(r.GetCenter());
        else Check($"the pause row '{id}' has a place on screen", false);
    }

    private async Task StartNewGame()
    {
        await Frames(20);
        for (int i = 0; i < 40 && _c.Vale == null; i++)
        {
            if (_c.TitleHud.TitleOpen) await Tap(Key.E);
            await Frames(5);
        }
        await Frames(20);
    }

    // ------------------------------------------------------------------ the Vale: bridge, guard, retreat, clues, manual, book
    private async Task Vale()
    {
        await StartNewGame();
        Check("Novo jogo starts the Vale", _c.Vale != null);
        if (_c.Vale == null) { Finish(); return; }
        await Frames(30);
        Check("the minimap shows while walking, in the top-right corner", V.Hud.MiniMap.Visible && V.Hud.MiniMap.Source == V && V.Hud.MiniMap.Position.X > V.Hud.LaidOut.X / 2, $"{V.Hud.MiniMap.Position} {V.Hud.MiniMap.Size}");
        await Shot("01-vale-minimapa");
        // the map before the encounter: no clue yet
        await Tap(Key.M);
        Check("before the encounter, M answers 'Onde estou?' and has no clue yet", V.Hud.MapOpen && V.MapWhere.StartsWith("Trilha da Montanha") && MapView.LastClue(V) == null, V.MapWhere);
        Check("the minimap hides behind the map", !V.Hud.MiniMap.Visible);
        await Shot("02-mapa-antes-do-encontro");
        await Tap(Key.M);

        // the approach to the bridge, walking up the trail (the same approach as the prototype's shot)
        await Teleport(new Vector2(29 * ValeMap.TS, 23 * ValeMap.TS), Vector2.Up);
        await Hold(Key.W, 50);
        var dis = V.Fighters["dis_bridge"];
        Check("approaching the bridge wakes its disciple and gives the mission", dis.State != ValeFighter.St.Idle && V.Story.Quests.ContainsKey("guarda") && V.Story.QuestAt.ContainsKey("guarda"), $"{dis.State}");
        // the planks of the bridge are drawn (a water layer used to cover them)
        var img = await Shot("03-ponte-chegada");
        if (img != null)
        {
            Vector2 sp = GetViewport().GetCanvasTransform() * new Vector2(27.4f * ValeMap.TS, 18.5f * ValeMap.TS);
            var px = img.GetPixel(Mathf.Clamp((int)sp.X, 0, img.GetWidth() - 1), Mathf.Clamp((int)sp.Y, 0, img.GetHeight() - 1));
            Check("the bridge's planks are drawn over the river (wood, not water)", px.R > px.B + 0.05f, $"pixel {px.ToHtml(false)} at {sp}");
        }
        // lock and cut from the front: parried, and the guard arc says so (WouldParry is what the assist draws)
        for (int i = 0; i < 3 && V.Lock != dis; i++) await Tap(Key.Space);
        Check("Space locks on the disciple; the top shows its name and guard", V.Lock == dis && V.Hud.FocusText.Contains("Discípulo da Guarda Fechada") && V.Hud.FocusText.Contains("guarda"), V.Hud.FocusText);
        await Teleport(dis.GlobalPosition + new Vector2(0, 40), Vector2.Up); dis.Face = Mathf.Pi / 2; await Frames(2);
        bool predicted = dis.WouldParry(P.GlobalPosition);
        int words = V.Fx.WordsShown.Count;
        await Tap(Key.J); await Frames(6);
        bool parried = V.Fx.WordsShown.Skip(words).Contains("aparado");
        Check("a frontal cut is parried — inside the guard arc, as drawn", predicted && parried && dis.GuardShow > 0, $"previsto {predicted}, aparado {parried}");
        Check("the parry throws the swordsman back (recoil)", P.Current == Player.ActionKind.Recoil || P.GlobalPosition.Y > dis.GlobalPosition.Y + 40);
        await Shot("04-ponte-corte-aparado");
        // the same frontal cut with the assist mode on: same result, and the areas are drawn
        Assist.Areas = true; await Frames(30);
        await Teleport(dis.GlobalPosition + new Vector2(0, 40), Vector2.Up); dis.Face = Mathf.Pi / 2; await Frames(2);
        words = V.Fx.WordsShown.Count; predicted = dis.WouldParry(P.GlobalPosition);
        await Tap(Key.J); await Frames(2);
        await Shot("05-ponte-assistencia-ligada");
        await Frames(4);
        Check("with the assist on, the result is the same (parried, inside the drawn arc)", predicted && V.Fx.WordsShown.Skip(words).Contains("aparado"));
        Assist.Areas = false; await Frames(5);
        // a cut from outside the guard arc (controlled: the disciple held facing away) is a flank hit
        await Teleport(dis.GlobalPosition + new Vector2(40, 0), Vector2.Left); dis.Face = Mathf.Pi / 2; dis.Fooled = 1f; await Frames(1);
        predicted = dis.WouldParry(P.GlobalPosition); int hp = dis.Hp; words = V.Fx.WordsShown.Count;
        await Tap(Key.J); await Frames(6);
        Check("a cut from outside the arc lands on the flank (the drawing and the result agree)", !predicted && dis.Hp < hp && V.Fx.WordsShown.Skip(words).Contains("flanco"), $"previsto aparar {predicted}, vida {hp}→{dis.Hp}");
        await Shot("06-ponte-flanco");
        dis.Hp = dis.Max; dis.Fooled = 0;

        // the retreat: walk away down the trail until the disciple gives up — its clue goes on the map
        await Tap(Key.Space); // release the lock
        await Teleport(new Vector2(29 * ValeMap.TS, 21 * ValeMap.TS), Vector2.Down);
        await Hold(Key.S, 140);
        Check("retreating: the disciple goes back and the clue is noted ('Uma trilha desce para o vale, a leste.')",
            V.Story.Clues.Any(c => c.Id == "vale") && MapView.LastClue(V)?.Text.Contains("trilha desce para o vale") == true, $"{dis.State}, pistas {V.Story.Clues.Count}");
        await Shot("07-recuo");
        await Tap(Key.M);
        Check("M after the retreat: the last clue is the retreat's, the mission has its seal", V.Hud.MapOpen && MapView.LastClue(V)?.Text.Contains("leste") == true && V.MapNotes.Any(n => !n.Clue && n.Title == "A guarda fechada" && n.At != null && n.Toward != null));
        await Shot("08-mapa-apos-recuo");
        await Tap(Key.M);

        // the manual (teleport to the body in the bamboo)
        await Teleport(ValeMap.Corpse + new Vector2(-20, 4), Vector2.Right);
        await Tap(Key.E); await Frames(4);
        Check("the body gives the half manual", V.Story.HasManual && V.Hud.StoryOpen);
        await Shot("09-manual");
        for (int i = 0; i < 3 && V.Hud.StoryOpen; i++) await Tap(Key.E);
        await Tap(Key.M);
        var last = MapView.LastClue(V);
        Check("M after the manual: the mission 'O manual dividido' and the last clue point to the twin stones", V.MapNotes.Any(n => n.Title == "O manual dividido" && !n.Done) && last?.Text.Contains("pedras gêmeas") == true, last?.Text ?? "");
        Check("'Onde estou?' says the Vale de Bambus", V.MapWhere.StartsWith("Vale de Bambus"), V.MapWhere);
        await Shot("10-mapa-apos-manual");
        await Tap(Key.M);

        // the book: list and page side by side, the manual's fragment, the command written with the controls
        await Tap(Key.B);
        bool wide = V.Hud.LaidOut.X >= 460; // without a window (headless) the screen is too small for two columns
        Check("B opens the book with the list beside the page (wide screen)", V.Hud.BookOpen && (V.Hud.Book.SideBySide || !wide) && V.Hud.BookRows.Any(p => p.Name == "Metade de um manual" && p.Kind == "fragmento"), wide ? "" : "sem janela: tela pequena, lista empilhada");
        Check("the selected page shows the command with the current key", V.Hud.BookDetail.Contains("Comando: J → J → J"), V.Hud.BookDetail.Replace("\n", " | "));
        await Shot("11-livro");
        for (int i = 0; i < 8 && V.Hud.BookRows[V.Hud.BookSelected].Name != "Metade de um manual"; i++) await Tap(Key.S);
        await Shot("12-livro-manual");
        await Tap(Key.B);
        Check("B closes the book", !V.Hud.BookOpen && !P.InputLocked);

        // meditate: everything above goes with the journey
        await Teleport(ValeMap.Meditate + new Vector2(0, 8), Vector2.Up);
        await Tap(Key.E); await Frames(3);
        var d = SaveGame.Read(SavePath).Data;
        Check("meditating saves the missions, their seals and the clues", d?.Vale is { } vs && vs.Clues.Any(c => c.Id == "manual") && vs.QuestAt.ContainsKey("manual") && vs.Quests.ContainsKey("guarda"),
            d?.Vale == null ? "no save" : $"{d.Vale.Clues.Count} pistas, {d.Vale.Quests.Count} missões");
        Finish();
    }

    private async Task Vale2()
    {
        await Frames(20);
        // Continuar (the first option)
        for (int i = 0; i < 40 && _c.Vale == null; i++) { if (_c.TitleHud.TitleOpen) await Tap(Key.E); await Frames(5); }
        await Frames(20);
        if (_c.Vale == null) { Check("Continuar opens the Vale", false); Finish(); return; }
        await Tap(Key.M);
        var last = MapView.LastClue(V);
        Check("after Continuar, the map keeps the missions, the seals and the last clue", V.Hud.MapOpen && last?.Text.Contains("pedras gêmeas") == true && V.MapNotes.Count(n => !n.Clue) >= 2, last?.Text ?? "none");
        await Shot("13-mapa-apos-continuar");
        await Tap(Key.M);
        // the rematch: the side step (learned at the statue — controlled: learned directly), lock, step to the side, cut
        V.LearnSide(); await Frames(220);
        for (int i = 0; i < 3 && V.Hud.StoryOpen; i++) await Tap(Key.E);
        var dis = V.Fighters["dis_bridge"];
        await Teleport(dis.Home + new Vector2(0, 60), Vector2.Up);
        await Frames(30);
        for (int i = 0; i < 3 && V.Lock != dis; i++) await Tap(Key.Space);
        Assist.Areas = true;
        int hp = dis.Hp, words = V.Fx.WordsShown.Count;
        bool landed = false;
        for (int t = 0; t < 4 && !landed; t++)
        {
            if (P.Health < P.MaxHealth / 2) P.Heal(P.MaxHealth); // controlled: keep the swordsman up during the rematch
            await Chord(Key.A, Key.L); await Frames(3);
            bool fooled = dis.Fooled > 0, predicted = dis.WouldParry(P.GlobalPosition);
            if (t == 0) await Shot("14-revanche-flanco-aberto");
            await Tap(Key.J); await Frames(6);
            landed = dis.Hp < hp && V.Fx.WordsShown.Skip(words).Contains("flanco");
            if (landed) Check("the rematch: side step → the guard is fooled → the cut lands on the flank, outside the drawn arc", fooled && !predicted, $"enganado {fooled}, previsto aparar {predicted}");
            await Frames(40);
        }
        Check("the flank hit happened in the rematch", landed, $"vida {hp}→{dis.Hp}");
        await Shot("15-revanche-depois");
        Assist.Areas = false;
        Finish();
    }

    // ------------------------------------------------------------------ controls
    private async Task Controls()
    {
        await StartNewGame();
        if (_c.Vale == null) { Check("Novo jogo starts the Vale", false); Finish(); return; }
        await Teleport(ValeMap.Start + new Vector2(0, -40), Vector2.Up);
        await Tap(Key.Escape);
        Check("Esc opens the pause menu", V.Hud.PauseOpen);
        await ClickPause("controls");
        Check("a click on 'Controles' opens the screen, with each action, its key and its mouse button", V.Hud.ControlsOpen && V.Hud.Controls.ChangeRects.Count >= 8 && V.Hud.Controls.ChangeRects.Count % 2 == 0, $"{V.Hud.Controls.ChangeRects.Count / 2} de {Bindings.Actions.Length} linhas à vista");
        await Shot("20-controles");
        int cutRow = System.Array.FindIndex(Bindings.Actions, a => a.Id == "cut");
        for (int i = 0; i < cutRow; i++) await Tap(Key.Down); // the keyboard moves the selection (and keeps the line in view on a small screen)
        Check("the arrows move the selection on the Controles screen", V.Hud.Controls.Row == cutRow, $"linha {V.Hud.Controls.Row}");
        if (!V.Hud.Controls.ChangeRects.ContainsKey((cutRow, 0))) { Check("the cut's line is on screen", false); Finish(); return; }
        var cell = V.Hud.Controls.ChangeRects[(cutRow, 0)];
        await Click(ToWindow(V.Hud.Controls, new Vector2(cell.End.X - 10, cell.GetCenter().Y)));
        Check("a click on 'Alterar' (Corte) waits for the new input", V.Hud.Controls.Waiting == "cut", V.Hud.Controls.Message);
        await Shot("21-controles-esperando");
        await Tap(Key.U);
        Check("U pressed: the cut is on U at once", Bindings.Current("cut")[0] == $"k:{(long)Key.U}" && V.Hud.Controls.Waiting == null, V.Hud.Controls.Message);
        // a key another action uses: K (Qi) → they trade, with a message
        cell = V.Hud.Controls.ChangeRects[(cutRow, 0)];
        await Click(ToWindow(V.Hud.Controls, new Vector2(cell.End.X - 10, cell.GetCenter().Y)));
        await Tap(Key.K);
        Check("K (used by Qi) on the cut: a message says so and the two trade", V.Hud.Controls.Message.Contains("era de") && Bindings.Key1("qi") == "U" && Bindings.Key1("cut") == "K", V.Hud.Controls.Message);
        await Shot("22-controles-conflito");
        // back to U for the cut (Qi gets K back)
        await Click(ToWindow(V.Hud.Controls, new Vector2(cell.End.X - 10, cell.GetCenter().Y)));
        await Tap(Key.U);
        // cancel with Esc while waiting
        await Click(ToWindow(V.Hud.Controls, new Vector2(cell.End.X - 10, cell.GetCenter().Y)));
        await Tap(Key.Escape);
        Check("Esc while waiting cancels (the cut stays on U, Qi on K)", V.Hud.Controls.Waiting == null && Bindings.Key1("cut") == "U" && Bindings.Key1("qi") == "K" && V.Hud.ControlsOpen, V.Hud.Controls.Message);
        // close with clicks: Voltar → pause → Voltar ao jogo; none of the clicks cuts
        int started = 0; P.ActionStarted += (k, _) => { if (k == Player.ActionKind.Cut) started++; };
        await Click(ToWindow(V.Hud.Controls, V.Hud.Controls.ButtonRects["back"].GetCenter()));
        Check("'Voltar' returns to the pause menu", V.Hud.PauseOpen && !V.Hud.ControlsOpen);
        await ClickPause("back");
        Check("'Voltar ao jogo' closes the menu; the clicks that closed it did not cut", !V.Hud.PauseOpen && started == 0, $"cortes {started}");
        Check("the footer names the new key", V.Hud.ControlsText.Contains("U/clique esq. corte"), V.Hud.ControlsText);
        await Tap(Key.U); await Frames(10);
        Check("U cuts", started >= 1, $"cortes {started}");
        int before = started;
        await Tap(Key.J); await Frames(10);
        Check("J no longer cuts", started == before);
        var prefs = Prefs.Load();
        Check("the controls are saved in the preferences", prefs.Bindings.TryGetValue("cut", out var c) && c[0] == $"k:{(long)Key.U}");
        Finish();
    }

    private async Task Controls2()
    {
        await StartNewGame();
        if (_c.Vale == null) { Check("Novo jogo starts the Vale", false); Finish(); return; }
        await Teleport(ValeMap.Start + new Vector2(0, -40), Vector2.Up);
        int started = 0; P.ActionStarted += (k, _) => { if (k == Player.ActionKind.Cut) started++; };
        await Tap(Key.U); await Frames(10);
        Check("a new run: U still cuts (read from the preferences)", started == 1 && Bindings.Key1("cut") == "U", $"cortes {started}");
        Check("the book's command follows the change (U → U → U)", V.BookRows().First(p => p.Name == "Corte").Command == "U → U → U");
        // restore the defaults by keyboard: Esc → Controles → the button "Restaurar padrões"
        await Tap(Key.Escape);
        for (int i = 0; i < 8 && V.Hud.PauseRows[V.Hud.PauseSelected].Id != "controls"; i++) await Tap(Key.S);
        await Tap(Key.E);
        await Click(ToWindow(V.Hud.Controls, V.Hud.Controls.ButtonRects["restore"].GetCenter()));
        Check("'Restaurar padrões' brings J back to the cut", Bindings.Key1("cut") == "J" && V.Hud.Controls.Message.Contains("restaurados"));
        await Tap(Key.Escape); await Tap(Key.Escape);
        started = 0; await Tap(Key.J); await Frames(10);
        Check("J cuts again", started == 1 && !V.Hud.PauseOpen, $"cortes {started}");
        Finish();
    }

    // ------------------------------------------------------------------ assist
    private async Task AssistOn()
    {
        await StartNewGame();
        if (_c.Vale == null) { Check("Novo jogo starts the Vale", false); Finish(); return; }
        Check("the assist starts off", !Assist.Areas);
        await Tap(Key.Escape);
        await ClickPause("assist");
        Check("Esc → Modo de assistência shows 'Mostrar áreas de combate'", V.Hud.PauseRows.Any(r => r.Id == "assist_areas" && r.Text.Contains("desligado")));
        await ClickPause("assist_areas");
        Check("one click turns it on, during the game", Assist.Areas && V.Hud.PauseRows.Any(r => r.Id == "assist_areas" && r.Text.Contains("ligado")));
        await Tap(Key.Escape); await Tap(Key.Escape);
        Check("back in the game", !V.Hud.PauseOpen && !P.InputLocked);
        Check("the preference is saved", Prefs.Load().AssistAreas);
        Finish();
    }

    private async Task AssistAgain()
    {
        await StartNewGame();
        if (_c.Vale == null) { Check("Novo jogo starts the Vale", false); Finish(); return; }
        Check("a new run: the assist is still on", Assist.Areas);
        await Tap(Key.Escape);
        await ClickPause("assist"); await ClickPause("assist_areas");
        Check("and turns off", !Assist.Areas && !Prefs.Load().AssistAreas);
        Finish();
    }

    // ------------------------------------------------------------------ the book in the Planície (sealed, weakened)
    private async Task Book()
    {
        var book = ValeState.StartingBook();
        book.Add(new BookEntry { Name = "Passo Lateral", Kind = "técnica", Tech = "side_step", Where = "Túmulo da Mestra." });
        book.Add(new BookEntry { Name = "Passo longo", Kind = "técnica", Tech = "dash", Where = "Nasce do passo lateral." });
        book.Add(new BookEntry { Name = "Passo da Lâmina Silenciosa", Kind = "combinação", Tech = "lamina", Where = "Descoberta por você." });
        book.Add(new BookEntry { Name = "Metade de um manual", Kind = "fragmento", Where = "Corpo de um discípulo, no Vale de Bambus." });
        book.Add(new BookEntry { Name = "Inscrição da fenda", Kind = "fragmento", Where = "Além da Fenda." });
        foreach (var b in book.Where(b => b.Tech.Length > 0)) b.State = "Selada";
        SaveGame.Write(PlainsSave, new SaveData
        {
            Region = "planicie", Phase = "planicie", Seed = 20261126, Spot = new[] { 16f, 16f }, Book = book, LossApplied = true,
            Equipped = "none", Health = 10, Vigor = 50, Qi = 100, Level = 2, Xp = 20,
        });
        _pg = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _pg.SavePath = PlainsSave; _pg.ShowIntro = false; _pg.Start = PlainsGame.StartMode.Continue;
        AddChild(_pg);
        await Frames(30);
        _pg.ToggleBook(); await Frames(3);
        var pages = _pg.Hud.BookRows;
        Check("the Planície book keeps the Vale: techniques, the combination and the fragments", pages.Any(p => p.Name == "Passo Lateral") && pages.Any(p => p.Kind == "combinação") && pages.Count(p => p.Kind == "fragmento") == 2);
        var qi = pages.First(p => p.Name == "Disparo de Qi");
        Check("a sealed technique says so, with the numbers of both versions", qi.State == "Selada" && !qi.Enabled && qi.Detail.Contains("volta enfraquecida no nível 3") && qi.Detail.Contains("custo: 30 Qi, 0 vigor") && qi.Detail.Contains("custo: 45 Qi, 10 vigor"), qi.Detail.Replace("\n", " | "));
        var dash = pages.First(p => p.Name == "Passo longo");
        Check("the long step's command in the Planície: Shift + direção + L", dash.Command.StartsWith("Shift + direção + L"), dash.Command);
        for (int i = 0; i < 8 && _pg.Hud.BookRows[_pg.Hud.BookSelected].Name != "Disparo de Qi"; i++) await Tap(Key.S);
        await Shot("16-livro-planicie-selada");
        _pg.ToggleBook(); await Frames(2);
        _pg.GainXp(_pg.Stats.XpMissing, "teste"); await Frames(5); // controlled: the XP still missing for level 3
        _pg.ToggleBook(); await Frames(3);
        qi = _pg.Hud.BookRows.First(p => p.Name == "Disparo de Qi");
        Check("level 3: the page says weakened, with the real numbers in force and the full ones for comparison", qi.State == "Enfraquecida" && qi.Detail.Contains("Enfraquecida, em uso: custo: 45 Qi, 10 vigor · alcance 105 px · dano 1") && qi.Detail.Contains("Plena era: custo: 30 Qi"), qi.Detail.Replace("\n", " | "));
        for (int i = 0; i < 8 && _pg.Hud.BookRows[_pg.Hud.BookSelected].Name != "Disparo de Qi"; i++) await Tap(Key.S);
        await Shot("17-livro-planicie-enfraquecida");
        // a narrow panel: the list goes on top, the page below, and both scroll
        var bp = _pg.Hud.Book;
        bp.Size = new Vector2(380, 260); bp.QueueRedraw(); await Frames(3); await Shot("18-livro-estreito");
        Check("a narrow panel stacks the list over the page", !bp.SideBySide && bp.LineRects.Count > 0 && bp.LineRects.Values.All(r => r.End.Y < bp.Size.Y * 0.5f), $"{bp.LineRects.Count} linhas visíveis");
        // mouse: click a line
        int target = bp.LineRects.Keys.First(k => k != bp.Selected);
        await Click(ToWindow(bp, bp.LineRects[target].GetCenter()));
        Check("a click on a line turns to that page", bp.Selected == target);
        await Tap(Key.Escape);
        Check("Esc closes the book", !_pg.Hud.BookOpen);
        Finish();
    }

    // ------------------------------------------------------------------ layout and corners
    private async Task Layout()
    {
        await StartNewGame();
        if (_c.Vale == null) { Check("Novo jogo starts the Vale", false); Finish(); return; }
        foreach (var size in new[] { new Vector2I(1280, 720), new Vector2I(1600, 900), new Vector2I(1920, 1080) })
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed); DisplayServer.WindowSetSize(size); await Frames(40);
            var hud = V.Hud; var mm = new Rect2(hud.MiniMap.Position, hud.MiniMap.Size);
            var seed = new Rect2(hud.LaidOut.X - 156, 3, 150, 11);
            var bars = new Rect2(4, 2, 210, 52);
            bool inside = mm.Position.X >= 0 && mm.End.X <= hud.LaidOut.X && mm.End.Y < hud.LaidOut.Y - 16;
            Check($"window {size.X}x{size.Y}: the minimap stays inside, under the seed, clear of the bars and the footer", inside && mm.Position.Y >= seed.End.Y && !mm.Intersects(bars) && hud.MiniMap.Visible, $"minimapa {mm}, tela {hud.LaidOut}");
            await Shot($"19-layout-{size.X}x{size.Y}");
        }
        foreach (var how in new[] { "janela", "maximizada", "tela cheia", "janela" })
        {
            DisplayServer.WindowSetMode(how == "maximizada" ? DisplayServer.WindowMode.Maximized : how == "tela cheia" ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
            if (how == "janela") DisplayServer.WindowSetSize(new Vector2I(1280, 720));
            await Frames(45);
            Vector2I win = DisplayServer.WindowGetSize();
            Vector2 vis = GetViewport().GetVisibleRect().Size;
            bool ok = true; string detail = "";
            foreach (var corner in new[] { new Vector2(0, 0), new Vector2(win.X - 1, 0), new Vector2(0, win.Y - 1), new Vector2(win.X - 1, win.Y - 1) })
            {
                Input.ParseInputEvent(new InputEventMouseMotion { Position = corner, GlobalPosition = corner }); await Flush(); await Frames(1);
                Vector2 m = _lastMotion; // the event as the game receives it (window → picture coordinates)
                Vector2 want = new(corner.X == 0 ? 0 : vis.X, corner.Y == 0 ? 0 : vis.Y);
                bool good = Mathf.Abs(m.X - want.X) <= 1.01f && Mathf.Abs(m.Y - want.Y) <= 1.01f;
                ok &= good; detail += $"{corner}→{m.Round()} ";
            }
            Check($"{how} {win.X}x{win.Y}: the four corners of the window are the four corners of the picture", ok, detail);
            // and a click near a corner acts there: the pause menu's first row is centred, so use the book's first line (left side)
        }
        Finish();
    }

    // ------------------------------------------------------------------ the short video (run with --write-movie)
    // Esc → Controles → the cut changed to U → Modo de assistência on → the duel at the bridge with the areas shown
    private async Task Video()
    {
        _slow = 24;
        await StartNewGame();
        if (_c.Vale == null) { Check("Novo jogo starts the Vale", false); Finish(); return; }
        await Teleport(new Vector2(29 * ValeMap.TS, 25 * ValeMap.TS), Vector2.Up);
        await Frames(20);
        await Tap(Key.Escape); await Frames(20);
        await ClickPause("controls"); await Frames(25);
        int cutRow = System.Array.FindIndex(Bindings.Actions, a => a.Id == "cut");
        for (int i = 0; i < cutRow; i++) await Tap(Key.Down);
        var cell = V.Hud.Controls.ChangeRects[(cutRow, 0)];
        await Click(ToWindow(V.Hud.Controls, new Vector2(cell.End.X - 10, cell.GetCenter().Y))); await Frames(25);
        await Tap(Key.U); await Frames(40);
        Check("the cut is on U", Bindings.Key1("cut") == "U");
        await Click(ToWindow(V.Hud.Controls, V.Hud.Controls.ButtonRects["back"].GetCenter())); await Frames(20);
        await ClickPause("assist"); await Frames(20);
        await ClickPause("assist_areas"); await Frames(30);
        Check("the assist is on", Assist.Areas);
        await Tap(Key.Escape); await Frames(10); await Tap(Key.Escape); await Frames(15);
        await Hold(Key.W, 150);
        var dis = V.Fighters["dis_bridge"];
        for (int i = 0; i < 3 && V.Lock != dis; i++) await Tap(Key.Space);
        await Frames(10);
        if (P.GlobalPosition.DistanceTo(dis.GlobalPosition) > 48) await Hold(Key.W, 20);
        int words = V.Fx.WordsShown.Count;
        for (int i = 0; i < 2; i++) { await Tap(Key.U); await Frames(30); }
        Check("U cuts and the guard parries (with the areas shown)", V.Fx.WordsShown.Skip(words).Contains("aparado"));
        // let him warn, strike and recover in front of the camera
        for (int t = 0; t < 150 && dis.State != ValeFighter.St.Windup; t++) await Frames(1);
        P.Heal(P.MaxHealth);
        await Frames(70);
        await Hold(Key.S, 30);
        await Frames(30);
        Finish();
    }
}
