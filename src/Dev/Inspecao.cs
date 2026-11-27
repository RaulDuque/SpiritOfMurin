using System.Collections.Generic;
using System.Linq;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Elements;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Dev;

/// <summary>
/// The inspection scene (pedido 14, item 3): one element of the world at the game's real size, in every state, plus the
/// swordsman in four directions holding each tool. It uses the game's own classes (<see cref="WorldElement"/>,
/// <see cref="Player"/>) and data (<c>assets/data/elements</c>), so an edit to an art class or a JSON shows here once the
/// code is rebuilt (the Painel de Testes rebuilds and reopens it).
/// <list type="bullet">
/// <item>← / →: the element · ↑ / ↓: the state (inteiro, meio trabalhado, quase no fim, caindo, esgotado) · J: one strike with the right tool;</item>
/// <item>G: a gust of wind on / off · A: a body crossing the bush · Tab: the tool in the swordsman's hand · wheel: zoom · Esc: quit.</item>
/// </list>
/// Command line: <c>-- --elemento=pinheiro --estado=meio --shot=user://x.png</c> opens straight on it (and saves a picture).
/// </summary>
public partial class Inspecao : Node2D
{
    private ElementCatalog _cat = new();
    private List<ElementDef> _defs = new();
    private int _i, _state, _tool;
    private readonly List<WorldElement> _shown = new();
    private readonly List<Player> _figures = new();
    private Label _title = null!, _info = null!;
    private Camera2D _cam = null!;
    private bool _gust;
    private CharacterBody2D? _walker;
    private float _walkT = -1, _again;

    private static readonly string[] States = { "inteiro", "meio", "quase", "caindo", "esgotado" };
    private static readonly (string Name, Player.Weapon W, float Reach)[] Tools =
        { ("espada", Player.Weapon.Sword, 0), ("faca", Player.Weapon.Knife, 0), ("machado", Player.Weapon.Axe, 22), ("picareta", Player.Weapon.Pickaxe, 20), ("mãos vazias", Player.Weapon.None, 0) };

    /// <inheritdoc />
    public override void _Ready()
    {
        RenderingServer.SetDefaultClearColor(new Color("#4a5a3a"));
        _cam = new Camera2D { Position = new Vector2(320, 180) };
        AddChild(_cam); _cam.MakeCurrent();
        AddChild(new GroundPatch());
        _cat = ElementCatalog.Load();
        _defs = _cat.All.Values.OrderBy(d => d.Family).ThenBy(d => d.Id).ToList();
        _title = MakeLabel(new Vector2(10, 6), 11); _info = MakeLabel(new Vector2(10, 300), 8);
        var scene = GD.Load<PackedScene>("res://src/Player/Player.tscn");
        Vector2[] dirs = { Vector2.Down, Vector2.Right, Vector2.Up, Vector2.Left };
        for (int k = 0; k < 4; k++)
        {
            var p = scene.Instantiate<Player>();
            p.GetNode<Camera2D>("Camera").Enabled = false;
            p.Position = new Vector2(430 + k * 48, 250); p.InputLocked = true; p.CollisionLayer = 0; p.CollisionMask = 0;
            AddChild(p); p.Face(dirs[k]); _figures.Add(p);
        }
        foreach (var a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--elemento=")) _i = Mathf.Max(0, _defs.FindIndex(d => d.Id == a["--elemento=".Length..]));
            if (a.StartsWith("--estado=")) _state = Mathf.Max(0, System.Array.IndexOf(States, a["--estado=".Length..]));
            if (a.StartsWith("--ferramenta=")) _tool = Mathf.Max(0, System.Array.FindIndex(Tools, t => t.Name == a["--ferramenta=".Length..]));
        }
        Show();
        foreach (var a in OS.GetCmdlineUserArgs())
            if (a.StartsWith("--shot=")) _ = Shot(a["--shot=".Length..]);
    }

    // the element in three variations, side by side, in the chosen state
    private void Show()
    {
        foreach (var e in _shown) e.QueueFree();
        _shown.Clear();
        if (_defs.Count == 0) { _title.Text = "Nenhum elemento em assets/data/elements."; return; }
        var d = _defs[_i];
        for (int v = 0; v < 3; v++)
        {
            float work = _state switch { 1 => d.Toughness * 0.5f, 2 => d.Toughness * 0.85f, 4 => d.Toughness, _ => 0 };
            var e = new WorldElement { Def = d, PlaceId = $"inspecao#{v}", Variation = 101 + v * 337, Work = d.Workable ? work : 0, Position = new Vector2(90 + v * 110, 220), Movers = Bodies };
            AddChild(e); _shown.Add(e);
            if (_state == 3 && d.Workable) { e.Work = d.Toughness - 0.5f; e.Strike(d.Tools[0], 1, e.Position + new Vector2(-20, 0)); }
        }
        var t = Tools[_tool];
        foreach (var p in _figures) { p.Equipped = t.W; p.ToolReach = t.Reach; }
        _title.Text = $"INSPEÇÃO — {d.Name} ({d.Id}) · família {d.Family} · arte {d.Art} · estado: {States[_state]}";
        _info.Text = $"dados: assets/data/elements/…/{d.Id}.json · desenho: src/Elements/…/{char.ToUpperInvariant(d.Art[0])}{d.Art[1..]}Art.cs\n"
            + (d.Workable ? $"aguenta {d.Toughness} golpes de força 1 · ferramentas: {string.Join(", ", d.Tools)} · dá: {string.Join(", ", d.Drops.Select(x => $"{x.Min}-{x.Max} {x.Item}"))}" : "decorativo (não se trabalha)")
            + $"\nna mão do personagem: {t.Name} (Tab troca)\n← → elemento · ↑ ↓ estado · J golpe · G rajada {(_gust ? "LIGADA" : "desligada")} · A corpo atravessando · roda: zoom · Esc sai";
    }

    private IEnumerable<CharacterBody2D> Bodies() { if (_walker != null) yield return _walker; }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent ev)
    {
        if (ev is InputEventMouseButton mb && mb.Pressed)
        {
            if (mb.ButtonIndex == MouseButton.WheelUp) _cam.Zoom *= 1.15f;
            if (mb.ButtonIndex == MouseButton.WheelDown) _cam.Zoom /= 1.15f;
            _cam.Zoom = _cam.Zoom.Clamp(Vector2.One, Vector2.One * 6);
        }
        if (ev is not InputEventKey { Pressed: true, Echo: false } k) return;
        switch (k.PhysicalKeycode)
        {
            case Key.Right: _i = (_i + 1) % Mathf.Max(1, _defs.Count); Show(); break;
            case Key.Left: _i = (_i - 1 + _defs.Count) % Mathf.Max(1, _defs.Count); Show(); break;
            case Key.Down: _state = (_state + 1) % States.Length; Show(); break;
            case Key.Up: _state = (_state - 1 + States.Length) % States.Length; Show(); break;
            case Key.Tab: _tool = (_tool + 1) % Tools.Length; Show(); break;
            case Key.G: _gust = !_gust; Show(); break;
            case Key.J: foreach (var e in _shown) if (e.Def.Workable) e.Strike(e.Def.Tools[0], 1, e.Position + new Vector2(-20, 0)); break;
            case Key.A: StartWalker(); break;
            case Key.Escape: GetTree().Quit(); break;
        }
    }

    // a plain body crossing the elements from left to right (what a rabbit does to a bush)
    private void StartWalker()
    {
        _walker?.QueueFree();
        _walker = new CharacterBody2D { Position = new Vector2(40, 222), Velocity = new Vector2(90, 0) };
        AddChild(_walker); _walkT = 0;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        Wind.T = _gust ? Wind.Def.GustPeriod * 0.5f : 0; // a gust held, or the calm
        // "caindo": the fall plays again every 2 s
        _again += (float)delta;
        if (_state == 3 && _again > 2) { _again = 0; Show(); }
        if (_walker != null && _walkT >= 0)
        {
            _walkT += (float)delta;
            _walker.Position += _walker.Velocity * (float)delta;
            if (_walker.Position.X > 400) { _walker.QueueFree(); _walker = null; _walkT = -1; }
        }
        QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        if (_walker != null) DrawCircle(_walker.Position + new Vector2(0, -4), 4, new Color("#efe4c8")); // the crossing body
    }

    private async System.Threading.Tasks.Task Shot(string path)
    {
        for (int i = 0; i < (_state == 3 ? 28 : 50); i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); // "caindo": mid-fall
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        GetViewport().GetTexture().GetImage().SavePng(path); // the scene's own picture, never the desktop
        GetTree().Quit();
    }

    private Label MakeLabel(Vector2 at, int size)
    {
        var l = new Label { Position = at, ZIndex = 50 };
        l.AddThemeFontSizeOverride("font_size", size);
        l.AddThemeColorOverride("font_color", new Color("#ece5d0"));
        l.AddThemeColorOverride("font_outline_color", new Color("#16140f"));
        l.AddThemeConstantOverride("outline_size", 3);
        AddChild(l);
        return l;
    }

    // a strip of the Planície's ground behind the elements (grass, bank, a little water), so the colours read as in the game
    private sealed partial class GroundPatch : Node2D
    {
        public override void _Draw()
        {
            DrawRect(new Rect2(0, 120, 400, 140), new Color("#56703f"));
            DrawRect(new Rect2(0, 230, 400, 30), new Color("#b9a77a"));
            DrawRect(new Rect2(0, 250, 400, 10), new Color("#22434e"));
            DrawRect(new Rect2(410, 200, 220, 80), new Color("#56703f"));
        }
    }
}
