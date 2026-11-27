using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Combat;
using SpiritOfMurin.Creatures;

namespace SpiritOfMurin.Dev;

/// <summary>
/// The showcase (vitrine) for working on the look: the protagonist in the eight directions, lying and kneeling,
/// with sword, knife and empty hands; one protagonist you control (walk, cut, step — the live animations);
/// the disciple, the senior and the statue in every pose and fallen; the boar and the rabbit. The figures are
/// the game's own classes (Player, FighterArt, Boar, Rabbit), so a change to their drawing shows here once the
/// code is rebuilt (the Painel de Testes rebuilds and reopens it on every save in "modo ao vivo").
/// Wheel: zoom · middle or right button: drag the view · R: put the controlled protagonist back · Esc: quit.
/// </summary>
public partial class Vitrine : Node2D
{
    private Camera2D _cam = null!;
    private Player _live = null!;
    private readonly Vector2 _liveHome = new(470, 165);
    private bool _drag;

    private static readonly Vector2[] Dirs =
    {
        Vector2.Down, new Vector2(1, 1).Normalized(), Vector2.Right, new Vector2(1, -1).Normalized(),
        Vector2.Up, new Vector2(-1, -1).Normalized(), Vector2.Left, new Vector2(-1, 1).Normalized(),
    };
    private static readonly string[] DirNames = { "baixo", "baixo-dir.", "direita", "cima-dir.", "cima", "cima-esq.", "esquerda", "baixo-esq." };

    /// <inheritdoc />
    public override void _Ready()
    {
        RenderingServer.SetDefaultClearColor(new Color("#3d4a33"));
        _cam = new Camera2D { Position = new Vector2(320, 240), Zoom = new Vector2(1, 1) };
        AddChild(_cam);
        _cam.MakeCurrent();
        var scene = GD.Load<PackedScene>("res://src/Player/Player.tscn");

        Label(new Vector2(10, 6), "VITRINE — protagonista (as 8 direções, com espada)", 10);
        for (int i = 0; i < 8; i++)
        {
            var p = Figure(scene, new Vector2(40 + i * 72, 78));
            p.Face(Dirs[i]);
            Label(new Vector2(16 + i * 72, 84), DirNames[i], 8);
        }
        Label(new Vector2(10, 100), "faca · mãos vazias · ajoelhado · deitado", 9);
        var knife = Figure(scene, new Vector2(40, 160)); knife.Equipped = Player.Weapon.Knife;
        var empty = Figure(scene, new Vector2(112, 160)); empty.Equipped = Player.Weapon.None;
        var kneel = Figure(scene, new Vector2(184, 160)); kneel.Lying = 0.5f;
        var lying = Figure(scene, new Vector2(270, 160)); lying.Lying = 1f;

        Label(new Vector2(330, 100), "Você controla este: andar, corte, passo, Ki (R volta)", 9);
        _live = scene.Instantiate<Player>();
        _live.GetNode<Camera2D>("Camera").Enabled = false;
        _live.Position = _liveHome; _live.CollisionMask = 0;
        AddChild(_live);

        Label(new Vector2(10, 182), "Discípulo · Sênior · Estátua — parado, andando, guarda, aviso, golpe, atordoado, ataque, caído", 9);
        AddChild(new FighterRow { Position = new Vector2(0, 0) });

        Label(new Vector2(10, 404), "Javali e coelho (parados)", 9);
        var boar = new Boar { Position = new Vector2(60, 450), HomeAt = new Vector2(60, 450), ProcessMode = ProcessModeEnum.Disabled };
        AddChild(boar);
        var rabbit = new Rabbit { Position = new Vector2(140, 450), BurrowAt = new Vector2(140, 450), ProcessMode = ProcessModeEnum.Disabled };
        AddChild(rabbit);
        Label(new Vector2(10, 470), "roda: zoom · botão direito ou do meio: arrastar · R: volta o boneco · Esc: sair", 8);
        GD.Print("Vitrine aberta.");
        // --shot=arquivo.png: saves one picture of the showcase and quits (checks)
        foreach (var a in OS.GetCmdlineUserArgs())
            if (a.StartsWith("--shot=")) _ = Shot(a["--shot=".Length..]);
    }

    private async System.Threading.Tasks.Task Shot(string path)
    {
        for (int i = 0; i < 40; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        GetViewport().GetTexture().GetImage().SavePng(path);
        GetTree().Quit();
    }

    private Player Figure(PackedScene scene, Vector2 at)
    {
        var p = scene.Instantiate<Player>();
        p.GetNode<Camera2D>("Camera").Enabled = false;
        p.Position = at; p.InputLocked = true; p.CollisionLayer = 0; p.CollisionMask = 0;
        AddChild(p);
        return p;
    }

    private void Label(Vector2 at, string text, int size)
    {
        var l = new Label { Text = text, Position = at, ZIndex = 50 };
        l.AddThemeFontSizeOverride("font_size", size);
        l.AddThemeColorOverride("font_color", new Color("#ece5d0"));
        l.AddThemeColorOverride("font_outline_color", new Color("#16140f"));
        l.AddThemeConstantOverride("outline_size", 3);
        AddChild(l);
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventKey { Pressed: true, PhysicalKeycode: Key.Escape }) GetTree().Quit();
        else if (e is InputEventKey { Pressed: true, PhysicalKeycode: Key.R }) { _live.Position = _liveHome; _live.Revive(); }
        else if (e is InputEventMouseButton mb)
        {
            if (mb.Pressed && mb.ButtonIndex == MouseButton.WheelUp) _cam.Zoom = (_cam.Zoom * 1.25f).Clamp(Vector2.One * 0.5f, Vector2.One * 8);
            else if (mb.Pressed && mb.ButtonIndex == MouseButton.WheelDown) _cam.Zoom = (_cam.Zoom * 0.8f).Clamp(Vector2.One * 0.5f, Vector2.One * 8);
            else if (mb.ButtonIndex is MouseButton.Right or MouseButton.Middle) _drag = mb.Pressed;
        }
        else if (e is InputEventMouseMotion mm && _drag) _cam.Position -= mm.Relative / _cam.Zoom;
    }

    /// <summary>The prologue's fighters in every pose, drawn with FighterArt (the same code the game uses).</summary>
    private partial class FighterRow : Node2D
    {
        private readonly PixelPainter _paint = new();
        private float _t, _walk;

        public override void _Process(double delta) { _t += (float)delta; _walk += (float)delta * 2.5f; QueueRedraw(); }

        public override void _Draw()
        {
            var looks = new[] { (FighterArt.Disciple, false), (FighterArt.Senior, true), (FighterArt.Statue, false) };
            var poses = new[] { FighterArt.Pose.Idle, FighterArt.Pose.Walk, FighterArt.Pose.Guard, FighterArt.Pose.Windup, FighterArt.Pose.Strike, FighterArt.Pose.Stagger, FighterArt.Pose.Attack };
            for (int r = 0; r < looks.Length; r++)
            {
                var (look, senior) = looks[r];
                float y = 250 + r * 58;
                for (int c = 0; c < poses.Length; c++)
                {
                    var at = new Vector2(40 + c * 70, y);
                    DrawSetTransform(at);
                    DrawRect(new Rect2(-11, -2, 22, 4), new Color(0.03f, 0.04f, 0.03f, 0.38f));
                    FighterArt.Draw(this, _paint, look, Mathf.Pi / 2, poses[c], _t, _walk, poses[c] == FighterArt.Pose.Windup, senior && poses[c] == FighterArt.Pose.Attack, false, senior);
                }
                DrawSetTransform(new Vector2(40 + poses.Length * 70 + 10, y));
                FighterArt.DrawDead(this, _paint, look, senior, false, _t);
            }
            DrawSetTransform(Vector2.Zero);
        }
    }
}
