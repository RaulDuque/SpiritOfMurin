using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Game;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Tests;

/// <summary>
/// Full screen, the picture and the map (campaign delivery 3). Windowed runs on monitor 2:
/// <list type="bullet">
/// <item><c>--phase=measure</c>: the game picture in a 1280×720 window, a maximized window, real full screen
/// and a 1920×1080 window, with the old stretch ("keep": black borders) and the new one ("expand"); screen
/// captures of full screen before and after (<c>--shots=folder</c>).</item>
/// <item><c>--phase=click</c>: in full screen, the row under the mouse is the one a click selects and acts on
/// (inventory, sheet, pause); closing a panel by clicking does not cut.</item>
/// <item><c>--phase=map</c> then <c>--phase=map2</c>: M opens the map, X puts a marker, another chunk is
/// explored, meditating saves; after loading, the explored chunks and the marker are back.</item>
/// </list>
/// Teleports place the swordsman (controlled). Files: user://teste_tela*.json.
/// </summary>
public partial class DisplayCheck : Node
{
    private const string SavePath = "user://teste_tela.json";
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private string _phase = "measure";
    private string? _shots;
    private PlainsGame _g = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        foreach (string a in OS.GetCmdlineUserArgs()) { if (a.StartsWith("--phase=")) _phase = a["--phase=".Length..]; if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..]; }
        GD.Print($"window on screen {DisplayServer.WindowGetCurrentScreen()} of {DisplayServer.GetScreenCount()} ({DisplayServer.GetName()}), screen size {DisplayServer.ScreenGetSize(DisplayServer.WindowGetCurrentScreen())}");
        if (_phase != "map2") SaveGame.Erase(SavePath);
        _g = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _g.Seed = 20261126; _g.SavePath = SavePath; _g.ShowIntro = false;
        _g.Start = _phase == "map2" ? PlainsGame.StartMode.Continue : PlainsGame.StartMode.New;
        AddChild(_g);
        _ = _phase switch { "measure" => MeasureAll(), "click" => Click(), "map" => MapNew(), _ => MapLoad() };
    }

    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private async Task Flush() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static void Press(Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });
    private async Task Tap(Key k) { Press(k, true); await Flush(); await Frames(2); Press(k, false); await Flush(); await Frames(3); }
    private void Finish() { int f = _results.Count(r => !r.Ok); GD.Print($"display {_phase}: {_results.Count - f}/{_results.Count} OK"); DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed); GetTree().Quit(f == 0 ? 0 : 1); }

    // the game's own picture (never the rest of the desktop)
    private async Task GameShot(string name)
    {
        if (_shots == null || DisplayServer.GetName() == "headless") return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        DirAccess.MakeDirRecursiveAbsolute(_shots);
        GetViewport().GetTexture().GetImage().SavePng($"{_shots}/{name}.png");
    }

    private async Task Apply(string how)
    {
        switch (how)
        {
            case "janela 1280x720": DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed); DisplayServer.WindowSetSize(new Vector2I(1280, 720)); break;
            case "janela maximizada": DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed); await Frames(5); DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized); break;
            case "tela cheia": DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen); break;
            case "janela 1920x1080": DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed); DisplayServer.WindowSetSize(new Vector2I(1920, 1080)); break;
        }
        await Frames(40);
    }

    private async Task MeasureAll()
    {
        await Frames(30);
        foreach (var aspect in new[] { Window.ContentScaleAspectEnum.Keep, Window.ContentScaleAspectEnum.Expand })
        {
            GetWindow().ContentScaleAspect = aspect;
            DisplayManager.Instance!.OldStretch = aspect == Window.ContentScaleAspectEnum.Keep; // keep = the old rule: fixed 640 × 360
            DisplayManager.Instance.Fit();
            string tag = aspect == Window.ContentScaleAspectEnum.Keep ? "antes (640x360 fixo, keep)" : "depois (resolução da janela ÷ escala inteira)";
            foreach (var how in new[] { "janela 1280x720", "janela maximizada", "tela cheia", "janela 1920x1080" })
            {
                await Apply(how);
                DisplayManager.Instance.Fit(); await Frames(5);
                var (win, pic, scale, world) = DisplayManager.Measure(GetViewport());
                float bx = win.X - pic.Size.X, by = win.Y - pic.Size.Y;
                GD.Print($"MEDIDA {tag} | {how} | janela {win.X}x{win.Y} | jogo {pic.Size.X:0}x{pic.Size.Y:0} em ({pic.Position.X:0},{pic.Position.Y:0}) | escala {scale:0}x | mundo visível {world.X:0}x{world.Y:0} px | bordas {bx:0}+{by:0} px");
                if (aspect == Window.ContentScaleAspectEnum.Expand)
                    Check($"{tag}, {how}: the picture fills the window (borders under one game pixel), whole-number scale", bx < scale && by < scale && Mathf.IsEqualApprox(scale, Mathf.Round(scale)), $"{win.X}x{win.Y}: jogo {pic.Size.X:0}x{pic.Size.Y:0}, escala {scale}, mundo {world.X:0}x{world.Y:0}");
                // the game's own picture (never a capture of the desktop: a capture of the screen can show other windows);
                // the monitor is rebuilt from it with the measure printed above (tools/capture/monitor_compose.py)
                if (how == "tela cheia") await GameShot(aspect == Window.ContentScaleAspectEnum.Keep ? "tela-cheia-antes-jogo" : "tela-cheia-depois-jogo");
                if (how == "tela cheia" && aspect == Window.ContentScaleAspectEnum.Expand)
                    Check("full screen: the HUD follows the screen (controls line at the bottom, seed at the right)", _g.Hud.LaidOut == world, $"layout {_g.Hud.LaidOut}");
            }
        }
        Finish();
    }

    private async Task Click()
    {
        await Frames(20);
        DisplayManager.Instance?.SetFullscreen(true, save: false);
        await Frames(40);
        // a few things in the inventory
        _g.Inventory.Add("silex", 1); _g.Inventory.Add("galho_rijo", 1); _g.Inventory.Add("faca_pedra", 1);
        await Tap(Key.I);
        int target = _g.Hud.Rows.FindIndex(r => !r.IsRecipe && r.Id == "faca_pedra");
        var rect = _g.Hud.RowScreenRect("inv", target);
        Check("full screen: the inventory row has a place on screen", rect != null, rect?.ToString() ?? "none");
        int started = 0; _g.Player.ActionStarted += (_, _) => started++;
        if (rect is { } r)
        {
            Vector2 at = r.GetCenter();
            Input.ParseInputEvent(new InputEventMouseMotion { Position = at, GlobalPosition = at }); await Flush(); await Frames(3);
            Check("the row under the cursor is the one selected", _g.Hud.Selected == target, $"selected {_g.Hud.Selected}, wanted {target}");
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = at, GlobalPosition = at }); await Flush(); await Frames(2);
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false, Position = at, GlobalPosition = at }); await Flush(); await Frames(3);
            Check("a click on it acts on it (the knife is equipped)", _g.Player.Equipped == Player.Weapon.Knife, $"{_g.Player.Equipped}");
        }
        await Tap(Key.I);
        // the pause menu: click "Voltar ao jogo"; the click must not become a cut
        await Tap(Key.Escape);
        Check("Esc opens the pause menu", _g.Hud.PauseOpen);
        var back = _g.Hud.RowScreenRect("pause", 0);
        if (back is { } b)
        {
            Vector2 at = b.GetCenter();
            started = 0;
            Input.ParseInputEvent(new InputEventMouseMotion { Position = at, GlobalPosition = at }); await Flush(); await Frames(2);
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = at, GlobalPosition = at }); await Flush(); await Frames(4);
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false, Position = at, GlobalPosition = at }); await Flush(); await Frames(10);
            Check("clicking 'Voltar ao jogo' closes the menu and does not cut", !_g.Hud.PauseOpen && started == 0, $"actions {started}");
        }
        else Check("the pause row has a place on screen", false);
        // the pause menu: full screen off and on, by keyboard
        await Tap(Key.Escape);
        for (int i = 0; i < 5 && _g.Hud.PauseRows[_g.Hud.PauseSelected].Id != "fullscreen"; i++) await Tap(Key.S);
        await Tap(Key.E); await Frames(20);
        Check("the pause menu turns full screen off", !DisplayManager.Instance!.Fullscreen && _g.Hud.PauseRows[2].Text.Contains("desligada"));
        await Tap(Key.E); await Frames(20);
        Check("and back on", DisplayManager.Instance.Fullscreen);
        await Tap(Key.Escape);
        // F11
        await Tap(Key.F11); await Frames(20);
        Check("F11 toggles full screen", !DisplayManager.Instance.Fullscreen);
        Finish();
    }

    private async Task MapNew()
    {
        await Frames(20);
        await Tap(Key.M);
        Check("M opens the map", _g.Hud.MapOpen && _g.Hud.Map.Source == _g);
        Check("the map shows the explored chunk and the clearing", _g.Explored.Count >= 1, $"{_g.Explored.Count} chunk(s)");
        await GameShot("mapa-planicie");
        int markers = _g.MapMarkers.Count;
        await Tap(Key.X);
        Check("X puts a marker where the cross is", _g.MapMarkers.Count == markers + 1);
        Press(Key.D, true); await Flush(); await Frames(20); Press(Key.D, false); await Flush(); await Frames(2);
        Check("WASD pans the map", _g.Hud.Map.Center.X > _g.Player.GlobalPosition.X + 20, $"{_g.Hud.Map.Center}");
        await Tap(Key.C);
        Check("C recentres on the player", _g.Hud.Map.Center.DistanceTo(_g.Player.GlobalPosition) < 1);
        float z = _g.Hud.Map.Zoom; await Tap(Key.Equal);
        Check("+ zooms in", _g.Hud.Map.Zoom > z, $"{z:0.000} → {_g.Hud.Map.Zoom:0.000}");
        await Tap(Key.M);
        Check("M closes it", !_g.Hud.MapOpen && !_g.Player.InputLocked);
        // explore another chunk
        int before = _g.Explored.Count;
        _g.Player.GlobalPosition += new Vector2(-1100, 0); await Frames(10);
        Press(Key.A, true); await Flush(); await Frames(30); Press(Key.A, false); await Flush(); await Frames(5);
        Check("exploring reveals more of the map", _g.Explored.Count > before, $"{before} → {_g.Explored.Count}");
        // meditate: saved with the journey
        var mat = _g.World.Objects.GetChildren().OfType<MeditationMat>().First();
        _g.Player.GlobalPosition = mat.GlobalPosition + new Vector2(0, 10); await Frames(5);
        await Tap(Key.E);
        var d = SaveGame.Read(SavePath).Data!;
        Check("meditating saves the explored chunks and the marker", d.Explored.Count == _g.Explored.Count && d.Markers.Count == 1, $"{d.Explored.Count} chunks, {d.Markers.Count} marker");
        Finish();
    }

    private async Task MapLoad()
    {
        await Frames(20);
        Check("after loading, the explored chunks and the marker are back", _g.Explored.Count >= 3 && _g.MapMarkers.Count == 1, $"{_g.Explored.Count} chunks, {_g.MapMarkers.Count} marker");
        await Tap(Key.M);
        Check("the map opens with them", _g.Hud.MapOpen);
        await GameShot("mapa-planicie-depois-de-carregar");
        Finish();
    }
}
