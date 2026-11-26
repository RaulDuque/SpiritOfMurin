using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Game;

/// <summary>
/// The pause menu (Esc or P), the map (M) and the controls list, the same in every region. The pause
/// menu has: back to the game, the map, full screen on/off (also F11), the controls and quit. Closing any
/// panel makes the body ignore the press that closed it (a click never turns into a cut).
/// </summary>
public sealed class GamePanels
{
    private readonly PlainsHud _hud;
    private readonly Player _player;
    private readonly IMapSource _map;
    private readonly float _zoom;

    /// <summary>The full controls list (shown from the pause menu).</summary>
    public static readonly string[] Controls =
    {
        "WASD ou setas: andar · Shift: andar quieto (na Planície)",
        "J ou clique esquerdo: corte · L ou clique direito: passo",
        "Espaço: travar no alvo / focar animal · K: Qi (esfolar com Qi junto à carcaça)",
        "E: pegar, examinar, interagir · H: bálsamo ou erva da bolsa",
        "I ou Tab: inventário · C: ficha · B: Livro de Técnicas · M: mapa",
        "Esc ou P: pausa · F11: tela cheia",
        "Técnicas do Vale: frente + J com trava (estocada) · L + lado com trava (passo lateral)",
        "direção + L sem trava no Vale, Shift + direção + L na Planície (passo longo)",
    };

    /// <summary>Creates the panels for a scene.</summary>
    public GamePanels(PlainsHud hud, Player player, IMapSource map, float zoom)
    {
        _hud = hud; _player = player; _map = map; _zoom = zoom;
        _hud.RowClicked += (panel, i) => { if (panel == "pause") Choose(_hud.PauseRows[i].Id); };
    }

    private List<PlainsHud.Row> PauseRows() => new()
    {
        new(false, "back", "Voltar ao jogo", "", true),
        new(false, "map", "Mapa (M)", "", true),
        new(false, "fullscreen", $"Tela cheia: {(DisplayManager.Instance?.Fullscreen == true ? "ligada" : "desligada")} (F11)", "", true),
        new(false, "controls", "Controles", "", true),
        new(false, "quit", "Sair do jogo", "", true),
    };

    /// <summary>Opens the pause menu.</summary>
    public void OpenPause() { _hud.ShowPause(PauseRows()); _player.InputLocked = true; }

    /// <summary>Opens the map.</summary>
    public void OpenMap() { _hud.ShowPause(null); _hud.ShowMap(_map, _zoom); _player.InputLocked = true; }

    private void CloseAll()
    {
        _hud.ShowPause(null); _hud.ShowMap(null, 0); _hud.ShowStory(null);
        _player.InputLocked = _hud.AnyPanelOpen;
        _player.ReleaseGuard();
    }

    /// <summary>Acts on a pause row.</summary>
    public void Choose(string id)
    {
        switch (id)
        {
            case "back": CloseAll(); break;
            case "map": OpenMap(); break;
            case "fullscreen": DisplayManager.Instance?.Toggle(); _hud.ShowPause(PauseRows()); break;
            case "controls": _hud.ShowPause(null); _hud.ShowStory("Controles", Controls); _controlsFromPause = true; break;
            case "quit": _player.GetTree().Quit(); break;
        }
    }

    private bool _controlsFromPause;

    /// <summary>Handles the pause menu, the map and the controls list. Returns true when the event was theirs.</summary>
    public bool Input(InputEvent e)
    {
        if (_hud.PauseOpen)
        {
            if (e.IsActionPressed("move_up")) _hud.MovePause(-1);
            else if (e.IsActionPressed("move_down")) _hud.MovePause(1);
            else if (e.IsActionPressed("use") || e.IsActionPressed("ui_accept")) Choose(_hud.PauseRows[_hud.PauseSelected].Id);
            else if (e.IsActionPressed("menu") || e.IsActionPressed("ui_cancel")) CloseAll();
            else return e is InputEventKey or InputEventMouseButton; // nothing else reaches the world while paused
            return true;
        }
        if (_controlsFromPause && _hud.StoryOpen)
        {
            if (e.IsActionPressed("use") || e.IsActionPressed("ui_accept") || e.IsActionPressed("ui_cancel") || e.IsActionPressed("menu")) { _hud.ShowStory(null); _controlsFromPause = false; OpenPause(); return true; }
            return e is InputEventKey;
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

    /// <summary>Pans the map with WASD while it is open.</summary>
    public void Process(float dt)
    {
        if (!_hud.MapOpen) return;
        Vector2 v = Godot.Input.GetVector("move_left", "move_right", "move_up", "move_down");
        if (v != Vector2.Zero) _hud.Map.Center += v * dt * 240 / _hud.Map.Zoom;
    }
}
