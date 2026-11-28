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
/// <item>G: a gust of wind on / off · A: a body crossing the bush · wheel: zoom · Esc: quit.</item>
/// <item>The swordsman (T18): Tab the weapon · R the robe · C the hat · Z the pose (rest, walk, the three cuts, thrust,
/// step, Ki, Reforço) · Q / E turn the big figure through the eight directions · Space plays the pose (the cut runs,
/// the legs walk). The row below shows the eight directions in the same pose.</item>
/// </list>
/// Command line: <c>-- --elemento=pinheiro --estado=meio --arma=sabre --roupa=tunica_viajante --capacete=chapeu_bambu
/// --pose=corte2 --dir=3 --shot=user://x.png</c> opens straight on it (and saves a picture).
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
    private static readonly (string Name, Player.Weapon W, float Reach, string? Art)[] Tools =
    {
        ("espada", Player.Weapon.Sword, 0, "espada"), ("faca", Player.Weapon.Knife, 0, "faca_pedra"), ("sabre", Player.Weapon.Sword, 0, "sabre_ferro"),
        ("machado", Player.Weapon.Axe, 22, null), ("picareta", Player.Weapon.Pickaxe, 20, null), ("mãos vazias", Player.Weapon.None, 0, null),
    };

    // ---- the swordsman (T18): robe, hat, pose and direction
    private readonly List<string> _robes = RoupaArts.Ids.ToList();
    private readonly List<string?> _hats = new List<string?> { null }.Concat(CapaceteArts.Ids.Where(h => !h.StartsWith("prancha_"))).ToList();
    private int _robe, _hat, _pose, _dir;
    private bool _play;
    private float _playT;
    private Player _big = null!;

    /// <summary>The poses of the swordsman's preview: name and the frozen state.</summary>
    private static readonly (string Name, Player.PoseShot Pose)[] Poses =
    {
        ("repouso", new(Player.ActionKind.None, 0, 0, 0, false, 0.9f, false)),
        ("caminhada", new(Player.ActionKind.None, 0, 0, Mathf.Pi / 2, true, 0.9f, false)),
        ("corte1", new(Player.ActionKind.Cut, 1, 0.3f, 0, false, 0.9f, false)),
        ("corte2", new(Player.ActionKind.Cut, 2, 0.7f, 0, false, 0.9f, false)),
        ("corte3", new(Player.ActionKind.Cut, 3, 0.8f, 0, false, 0.9f, false)),
        ("estocada", new(Player.ActionKind.Thrust, 1, 0.5f, 0, false, 0.9f, false)),
        ("passo", new(Player.ActionKind.Step, 0, 0.5f, 0, false, 0.9f, false)),
        ("ki", new(Player.ActionKind.Ki, 0, 0.5f, 0, false, 0.9f, false)),
        ("reforco", new(Player.ActionKind.Cut, 2, 0.3f, 0, false, 0.9f, true)),
    };

    private static readonly Vector2[] Eight =
    {
        Vector2.Down, new Vector2(-1, 1).Normalized(), Vector2.Left, new Vector2(-1, -1).Normalized(),
        Vector2.Up, new Vector2(1, -1).Normalized(), Vector2.Right, new Vector2(1, 1).Normalized(),
    };
    private static readonly string[] EightNames = { "frente", "frente-esquerda", "esquerda", "costas-esquerda", "costas", "costas-direita", "direita", "frente-direita" };

    /// <inheritdoc />
    public override void _Ready()
    {
        RenderingServer.SetDefaultClearColor(new Color("#4a5a3a"));
        _cam = new Camera2D { Position = new Vector2(320, 180) };
        AddChild(_cam); _cam.MakeCurrent();
        AddChild(new GroundPatch());
        _cat = ElementCatalog.Load();
        _defs = _cat.All.Values.OrderBy(d => d.Family).ThenBy(d => d.Id).ToList();
        _title = MakeLabel(new Vector2(10, 6), 11); _info = MakeLabel(new Vector2(10, 226), 8);
        var scene = GD.Load<PackedScene>("res://src/Player/Player.tscn");
        Player Figure(Vector2 at, float scale)
        {
            var p = scene.Instantiate<Player>();
            p.GetNode<Camera2D>("Camera").Enabled = false;
            p.Position = at; p.Scale = Vector2.One * scale; p.InputLocked = true; p.CollisionLayer = 0; p.CollisionMask = 0;
            AddChild(p); _figures.Add(p);
            return p;
        }
        for (int k = 0; k < 8; k++) Figure(new Vector2(296 + k * 46, 352), 1); // the eight directions in the same pose
        _big = Figure(new Vector2(530, 206), 3);                                // the big one, turned with Q / E
        foreach (var a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--elemento=")) _i = Mathf.Max(0, _defs.FindIndex(d => d.Id == a["--elemento=".Length..]));
            if (a.StartsWith("--estado=")) _state = Mathf.Max(0, System.Array.IndexOf(States, a["--estado=".Length..]));
            if (a.StartsWith("--ferramenta=")) _tool = Mathf.Max(0, System.Array.FindIndex(Tools, t => t.Name == a["--ferramenta=".Length..]));
            if (a.StartsWith("--arma=")) _tool = Mathf.Max(0, System.Array.FindIndex(Tools, t => t.Name == a["--arma=".Length..] || t.Art == a["--arma=".Length..]));
            if (a.StartsWith("--roupa=")) _robe = Mathf.Max(0, _robes.IndexOf(a["--roupa=".Length..]));
            if (a.StartsWith("--capacete=")) _hat = Mathf.Max(0, _hats.IndexOf(a["--capacete=".Length..]));
            if (a.StartsWith("--pose=")) _pose = Mathf.Max(0, System.Array.FindIndex(Poses, q => q.Name == a["--pose=".Length..]));
            if (a.StartsWith("--dir=") && int.TryParse(a["--dir=".Length..], out int dd)) _dir = Mathf.PosMod(dd, 8);
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
        Dress();
        _title.Text = $"INSPEÇÃO — {d.Name} ({d.Id}) · família {d.Family} · arte {d.Art} · estado: {States[_state]}";
        _info.Text = $"dados: assets/data/elements/…/{d.Id}.json · desenho: src/Elements/…/{char.ToUpperInvariant(d.Art[0])}{d.Art[1..]}Art.cs\n"
            + (d.Workable ? $"aguenta {d.Toughness} golpes de força 1 · ferramentas: {string.Join(", ", d.Tools)} · dá: {string.Join(", ", d.Drops.Select(x => $"{x.Min}-{x.Max} {x.Item}"))}" : "decorativo (não se trabalha)")
            + $"\n{Look()}"
            + $"\n← → elemento · ↑ ↓ estado · J golpe · G rajada {(_gust ? "LIGADA" : "desligada")} · A corpo\nTab arma · R roupa · C chapéu · Z pose · Q/E gira · Espaço anima · Esc sai";
    }

    // the swordsman's pieces and where each drawing lives (to edit by hand)
    private string Look()
    {
        var t = Tools[_tool];
        string w = t.Art != null ? $"{t.Name} → src/Elements/Armas/{Art(t.Art)}Art.cs" : t.W is Player.Weapon.Axe or Player.Weapon.Pickaxe ? $"{t.Name} → src/Elements/Tools/ToolArt.cs" : t.Name;
        string h = _hats[_hat] is { } hat ? $"{hat} → src/Elements/Capacetes/{Art(hat)}Art.cs" : "sem chapéu";
        return $"arma: {w}\nroupa:{_robes[_robe]} → src/Elements/Roupas/{Art(_robes[_robe])}Art.cs · chapéu: {h}\npose: {Poses[_pose].Name} · figura grande: {EightNames[_dir]} · cabeça e cabelo → src/Elements/Corpo/CabecaArt.cs";
    }

    /// <summary>The class name of an art id: <c>sabre_ferro</c> → <c>SabreFerro</c> (the file is <c>SabreFerroArt.cs</c>).</summary>
    public static string Art(string id) => string.Concat(id.Split('_').Select(p => p.Length == 0 ? p : char.ToUpperInvariant(p[0]) + p[1..]));

    // puts the chosen pieces and pose on every figure
    private void Dress()
    {
        var t = Tools[_tool];
        var pose = Poses[_pose].Pose;
        if (_play)
        {
            float k = Mathf.PosMod(_playT, 1f);
            pose = pose with { Progress = k, WalkPhase = pose.Moving ? _playT * Mathf.Tau : pose.WalkPhase, Time = 0.9f + _playT };
        }
        for (int i = 0; i < _figures.Count; i++)
        {
            var p = _figures[i];
            p.Equipped = t.W; p.ToolReach = t.Reach; p.WeaponArt = t.Art; p.Outfit = _robes[_robe]; p.Hat = _hats[_hat];
            p.FreezePose(p == _big ? Eight[_dir] : Eight[i], pose);
        }
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
            case Key.R: _robe = (_robe + 1) % _robes.Count; Show(); break;
            case Key.C: _hat = (_hat + 1) % _hats.Count; Show(); break;
            case Key.Z: _pose = (_pose + 1) % Poses.Length; Show(); break;
            case Key.Q: _dir = (_dir + 7) % 8; Show(); break;
            case Key.E: _dir = (_dir + 1) % 8; Show(); break;
            case Key.Space: _play = !_play; _playT = 0; Show(); break;
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
        if (_play) { _playT += (float)delta * 0.8f; Dress(); }
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
            DrawRect(new Rect2(410, 60, 230, 160), new Color("#56703f"));
            DrawRect(new Rect2(276, 316, 364, 44), new Color("#56703f"));
        }
    }
}
