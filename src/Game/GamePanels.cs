using System.Collections.Generic;
using System.Linq;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Game;

/// <summary>
/// The pause menu (Esc or P), the map (M), the Controles screen and the assist mode, the same in every
/// region. The pause menu has: back to the game, the map, full screen on/off (also F11), Controles (change
/// any input), Modo de assistência (show the combat areas), the full list of commands, and quit. Closing
/// any panel makes the body ignore the press that closed it (a click never turns into a cut).
/// </summary>
public sealed class GamePanels
{
    private readonly PlainsHud _hud;
    private readonly Player _player;
    private readonly IMapSource _map;
    private readonly float _zoom;
    private string _pauseMode = "main";

    /// <summary>The full list of commands, written with the current controls (shown from the pause menu).</summary>
    public static string[] Controls => new[]
    {
        "[walk] (ou setas): andar · [sneak]: andar quieto (na Planície)",
        "[cut+]: corte (aperte de novo para encadear) · [step+]: passo",
        "[lock]: travar no alvo / focar animal · [ki]: Ki (esfolar com Ki junto à carcaça)",
        "[use]: pegar, examinar, interagir · [heal]: bálsamo ou erva da bolsa",
        "[inventory+]: inventário · [sheet]: ficha · [book]: Livro de Técnicas · [map]: mapa",
        "[menu+]: pausa · [fullscreen]: tela cheia",
        "Técnicas do Vale: frente + [cut] com trava (estocada) · lado + [step] com trava (passo lateral)",
        "direção + [step] sem trava no Vale, [sneak] + direção + [step] na Planície (passo longo)",
        "Passo da Lâmina Silenciosa: passo lateral → [cut] → [ki]",
        "No mapa: [walk] mover · roda ou +/− zoom · C centralizar · X ou clique: marcador",
    }.Select(Bindings.F).ToArray();

    /// <summary>Creates the panels for a scene.</summary>
    public GamePanels(PlainsHud hud, Player player, IMapSource map, float zoom)
    {
        _hud = hud; _player = player; _map = map; _zoom = zoom;
        _hud.RowClicked += (panel, i) => { if (panel == "pause") Choose(_hud.PauseRows[i].Id); };
        _hud.Controls.Closed += () => { OpenPause(); };
        _hud.Controls.ListRequested += () => { _hud.Controls.Visible = false; _hud.ShowStory("Lista de comandos", Controls); _listFrom = "controls"; };
    }

    private List<PlainsHud.Row> PauseRows() => _pauseMode == "assist" ? new()
    {
        new(false, "assist_areas", $"Mostrar áreas de combate: {(Assist.Areas ? "ligado" : "desligado")}", "", true),
        new(false, "assist_back", "Voltar", "", true),
    } : new()
    {
        new(false, "back", "Voltar ao jogo", "", true),
        new(false, "map", Bindings.F("Mapa ([map])"), "", true),
        new(false, "fullscreen", DisplayManager.Embedded ? "Tela cheia: indisponível dentro do editor" : Bindings.F($"Tela cheia: {(DisplayManager.Instance?.Fullscreen == true ? "ligada" : "desligada")} ([fullscreen])"), "", true),
        new(false, "controls", "Controles (trocar teclas e mouse)", "", true),
        new(false, "assist", $"Modo de assistência ({(Assist.Areas ? "áreas de combate visíveis" : "desligado")})", "", true),
        new(false, "commands", "Lista de comandos", "", true),
        new(false, "quit", "Sair do jogo", "", true),
    };

    /// <summary>Opens the pause menu.</summary>
    public void OpenPause() { _pauseMode = "main"; _hud.ShowPause(PauseRows()); _player.InputLocked = true; }

    /// <summary>Opens the map.</summary>
    public void OpenMap() { _hud.ShowPause(null); _hud.ShowMap(_map, _zoom); _player.InputLocked = true; }

    /// <summary>Opens the Controles screen.</summary>
    public void OpenControls() { _hud.ShowPause(null); _hud.Controls.Open(); _player.InputLocked = true; }

    private void CloseAll()
    {
        _hud.ShowPause(null); _hud.ShowMap(null, 0); _hud.ShowStory(null); _hud.Controls.Visible = false;
        _player.InputLocked = _hud.AnyPanelOpen;
        _player.ReleaseGuard();
    }

    private string _listFrom = "";

    /// <summary>True while the story panel shows the list of commands (the panels close it, not the region).</summary>
    public bool OwnsStory => _listFrom.Length > 0 && _hud.StoryOpen;

    /// <summary>Acts on a pause row.</summary>
    public void Choose(string id)
    {
        switch (id)
        {
            case "back": CloseAll(); break;
            case "map": OpenMap(); break;
            case "fullscreen": DisplayManager.Instance?.Toggle(); _hud.ShowPause(PauseRows()); break;
            case "controls": OpenControls(); break;
            case "commands": _hud.ShowPause(null); _hud.ShowStory("Lista de comandos", Controls); _listFrom = "pause"; break;
            case "assist": _pauseMode = "assist"; _hud.ShowPause(PauseRows()); break;
            case "assist_areas": Assist.Areas = !Assist.Areas; _hud.ShowPause(PauseRows()); break;
            case "assist_back": _pauseMode = "main"; _hud.ShowPause(PauseRows()); break;
            case "quit": Bindings.Save(); _player.GetTree().Quit(); break;
        }
    }

    /// <summary>Handles the pause menu, the map and the lists. Returns true when the event was theirs.</summary>
    public bool Input(InputEvent e)
    {
        if (_hud.ControlsOpen) return true; // the Controles screen takes its own input
        if (_hud.PauseOpen)
        {
            if (e.IsActionPressed("move_up") || e.IsActionPressed("ui_up")) _hud.MovePause(-1);
            else if (e.IsActionPressed("move_down") || e.IsActionPressed("ui_down")) _hud.MovePause(1);
            else if (e.IsActionPressed("use") || e.IsActionPressed("ui_accept")) Choose(_hud.PauseRows[_hud.PauseSelected].Id);
            else if (e.IsActionPressed("menu") || e.IsActionPressed("ui_cancel"))
            {
                if (_pauseMode != "main") { _pauseMode = "main"; _hud.ShowPause(PauseRows()); }
                else CloseAll();
            }
            else return e is InputEventKey or InputEventMouseButton; // nothing else reaches the world while paused
            return true;
        }
        if (_listFrom.Length > 0 && _hud.StoryOpen)
        {
            if (e.IsActionPressed("use") || e.IsActionPressed("ui_accept") || e.IsActionPressed("ui_cancel") || e.IsActionPressed("menu") || e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            {
                _hud.ShowStory(null);
                if (_listFrom == "controls") _hud.Controls.Open(); else OpenPause();
                _listFrom = "";
                return true;
            }
            return e is InputEventKey or InputEventMouseButton;
        }
        if (_hud.MapOpen)
        {
            var m = _hud.Map;
            if (e.IsActionPressed("map") || e.IsActionPressed("ui_cancel") || e.IsActionPressed("menu")) CloseAll();
            else if (e is InputEventKey { Pressed: true } k && k.PhysicalKeycode == Key.C) m.Center = _map.MapPlayer;
            else if (e is InputEventKey { Pressed: true } k2 && k2.PhysicalKeycode == Key.X) m.ToggleMarker(m.Center);
            else if (e is InputEventKey { Pressed: true } k3 && k3.PhysicalKeycode is Key.Equal or Key.KpAdd or Key.Pageup) m.ZoomBy(1.25f);
            else if (e is InputEventKey { Pressed: true } k4 && k4.PhysicalKeycode is Key.Minus or Key.KpSubtract or Key.Pagedown) m.ZoomBy(0.8f);
            else return e is InputEventKey;
            return true;
        }
        if (_hud.AnyPanelOpen) return false; // other panels handle themselves
        if (e.IsActionPressed("menu")) { OpenPause(); return true; }
        if (e.IsActionPressed("map")) { OpenMap(); return true; }
        return false;
    }

    /// <summary>Pans the map with the walk keys while it is open.</summary>
    public void Process(float dt)
    {
        if (!_hud.MapOpen) return;
        Vector2 v = Godot.Input.GetVector("move_left", "move_right", "move_up", "move_down");
        if (v != Vector2.Zero) _hud.Map.Center += v * dt * 240 / _hud.Map.Zoom;
    }
}
