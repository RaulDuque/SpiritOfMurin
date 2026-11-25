using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Scenes;

namespace SpiritOfMurin.Tests;

/// <summary>
/// Integration check for the engine base: loads the main scene and drives it through Godot's
/// own input pipeline (Input.ParseInputEvent with keys and mouse buttons), then checks movement,
/// facing, the cut chain, the step, that the cut follows the facing and that the camera follows.
/// Run: <c>godot --headless --path . res://tests/integration/ControlsCheck.tscn</c>
/// Add <c>-- --shots=&lt;folder&gt;</c> (windowed, not headless) to save screenshots.
/// Exit code 0 when every check passes, 1 otherwise.
/// </summary>
public partial class ControlsCheck : Node
{
    private Main _main = null!;
    private Player _player = null!;
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private readonly List<(Player.ActionKind Kind, int N)> _started = new();
    private string? _shots;

    /// <inheritdoc />
    public override void _Ready()
    {
        foreach (string a in OS.GetCmdlineUserArgs()) if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..];
        _main = GD.Load<PackedScene>("res://src/Main/Main.tscn").Instantiate<Main>();
        AddChild(_main);
        _player = _main.Player;
        _player.ActionStarted += (k, n) => _started.Add((k, n));
        _ = Run();
    }

    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }

    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }

    private static void KeyEvent(Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Pressed = down });

    private static void MouseEvent(MouseButton b, bool down) => Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = b, Pressed = down, Position = new Vector2(640, 360) });

    private async Task Hold(Key k, int frames) { KeyEvent(k, true); await Frames(frames); KeyEvent(k, false); await Frames(1); }

    private async Task Tap(Key k) { KeyEvent(k, true); await Frames(2); KeyEvent(k, false); }

    private async Task Click(MouseButton b) { MouseEvent(b, true); await Frames(2); MouseEvent(b, false); }

    private async Task Shot(string name)
    {
        if (_shots == null || DisplayServer.GetName() == "headless") return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        string dir = _shots.Replace('\\', '/');
        DirAccess.MakeDirRecursiveAbsolute(dir);
        GetViewport().GetTexture().GetImage().SavePng($"{dir}/{name}.png");
    }

    private Vector2 CamCenter => _player.GetNode<Camera2D>("Camera").GetScreenCenterPosition();

    private async Task Run()
    {
        await Frames(10);
        await Shot("01-inicio");
        Vector2 p0 = _player.GlobalPosition, c0 = CamCenter;

        // ---- WASD
        await Hold(Key.D, 40);
        Vector2 p1 = _player.GlobalPosition;
        Check("D walks right", p1.X - p0.X > 50 && Mathf.Abs(p1.Y - p0.Y) < 2, $"moved {(p1 - p0).X:0} px");
        Check("walking right faces right", _player.Facing.IsEqualApprox(Vector2.Right), $"{_player.Facing}");
        await Hold(Key.W, 30);
        Vector2 p2 = _player.GlobalPosition;
        Check("W walks up", p1.Y - p2.Y > 35, $"moved {(p1 - p2).Y:0} px");
        KeyEvent(Key.A, true); KeyEvent(Key.S, true); await Frames(25); KeyEvent(Key.A, false); KeyEvent(Key.S, false); await Frames(1);
        Vector2 p3 = _player.GlobalPosition;
        Check("A+S walks down-left", p3.X < p2.X - 20 && p3.Y > p2.Y + 20, $"{p3 - p2}");
        Check("diagonal facing", _player.Facing.IsEqualApprox(new Vector2(-1, 1).Normalized()), $"{_player.Facing}");

        // ---- the cut: J, the chain, the left click, follows the facing
        await Hold(Key.D, 6); // face right
        _started.Clear();
        await Tap(Key.J); await Frames(3);
        Check("J cuts", _started.Count == 1 && _started[0] == (Player.ActionKind.Cut, 1), Describe());
        Check("the cut follows the facing (right)", Mathf.Abs(Mathf.AngleDifference(_player.LastCutAngle, 0f)) < 0.01f, $"{Mathf.RadToDeg(_player.LastCutAngle):0}°");
        await Frames(4); await Shot("02-corte-direita");
        await Frames(30);
        _started.Clear();
        for (int i = 0; i < 3; i++) { await Tap(Key.J); await Frames(7); }
        await Frames(30);
        Check("J three times chains 1-2-3", _started.Count == 3 && _started[0].N == 1 && _started[1].N == 2 && _started[2].N == 3, Describe());
        await Hold(Key.W, 6); // face up
        _started.Clear();
        await Click(MouseButton.Left); await Frames(3);
        Check("left click cuts", _started.Count == 1 && _started[0].Kind == Player.ActionKind.Cut, Describe());
        Check("the cut follows the facing (up)", Mathf.Abs(Mathf.AngleDifference(_player.LastCutAngle, -Mathf.Pi / 2)) < 0.01f, $"{Mathf.RadToDeg(_player.LastCutAngle):0}°");
        await Frames(4); await Shot("03-corte-clique-cima");
        await Frames(30);
        _started.Clear();
        for (int i = 0; i < 3; i++) { await Click(MouseButton.Left); await Frames(7); }
        await Frames(30);
        Check("three left clicks chain 1-2-3", _started.Count == 3 && _started[2].N == 3, Describe());

        // ---- the step: L with no direction steps back; right click with D steps right
        await Hold(Key.W, 4); await Frames(10);
        Vector2 s0 = _player.GlobalPosition; _started.Clear();
        await Tap(Key.L); await Frames(20);
        Vector2 s1 = _player.GlobalPosition;
        Check("L steps (back from the facing, ~34 px)", _started.Count == 1 && _started[0].Kind == Player.ActionKind.Step && s1.Y - s0.Y > 25, $"{Describe()}; moved {(s1 - s0).Y:0} px down");
        _started.Clear();
        KeyEvent(Key.D, true); await Frames(1); s0 = _player.GlobalPosition;
        await Click(MouseButton.Right); await Frames(9); KeyEvent(Key.D, false); await Frames(10);
        s1 = _player.GlobalPosition;
        Check("right click + D steps right", _started.Exists(a => a.Kind == Player.ActionKind.Step) && s1.X - s0.X > 30, $"{Describe()}; moved {(s1 - s0).X:0} px");
        await Shot("04-passo");

        // ---- the camera follows
        await Hold(Key.D, 90);
        await Frames(60);
        Vector2 pc = _player.GlobalPosition + new Vector2(0, -16), cc = CamCenter;
        Check("the camera followed the player", cc.DistanceTo(c0) > 100 && cc.DistanceTo(pc) < 12, $"camera {cc}, player {pc}");
        await Hold(Key.S, 50); await Frames(40);
        await Shot("05-camera-seguiu");

        int fails = _results.FindAll(r => !r.Ok).Count;
        GD.Print($"{_results.Count - fails}/{_results.Count} OK");
        GetTree().Quit(fails == 0 ? 0 : 1);
    }

    private string Describe() => string.Join(",", _started.ConvertAll(a => a.Kind == Player.ActionKind.Cut ? $"cut{a.N}" : "step"));
}
