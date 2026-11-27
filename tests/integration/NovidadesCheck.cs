using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Campaign;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Game;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;
using SpiritOfMurin.Vale;

namespace SpiritOfMurin.Tests;

/// <summary>
/// Pedido 13, through the main scene and real keys and mouse. Phases, each its own run:
/// <list type="bullet">
/// <item><c>--phase=aim</c>: walking left with the cursor to the right, the cut, the knife and the Qi shot go
/// right and hit there; a remapped cut aims too; a locked target wins over the cursor and unlocking gives the
/// cursor back on the next attack; the keyboard option aims along the facing; the cursor maps to the world at
/// other window sizes.</item>
/// </list>
/// Teleports place the swordsman (named in the checks). Files: <c>user://teste_nov*.json</c>; preferences in
/// <c>user://teste_nov_prefs.json</c> (pass <c>--prefs=user://teste_nov_prefs.json</c>).
/// </summary>
public partial class NovidadesCheck : Node
{
    private const string SavePath = "user://teste_nov.json";
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private string _phase = "aim";
    private string? _shots;
    private int _slow;
    private CampaignRoot _c = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        foreach (string a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--phase=")) _phase = a["--phase=".Length..];
            if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..];
            if (a == "--lento") _slow = 30;
        }
        GD.Print($"window on screen {DisplayServer.WindowGetCurrentScreen()} of {DisplayServer.GetScreenCount()} ({DisplayServer.GetName()})");
        switch (_phase)
        {
            case "aim": Fresh(); Launch("new"); _ = AimPhase(); break;
        }
    }

    private void Fresh()
    {
        SaveGame.Erase(SavePath);
        if (Godot.FileAccess.FileExists(Prefs.Path)) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(Prefs.Path));
        Bindings.Init(); Assist.Reload(); Aim.Reload();
    }

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
    private void Finish() { int f = _results.Count(r => !r.Ok); GD.Print($"novidades {_phase}: {_results.Count - f}/{_results.Count} OK"); DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed); GetTree().Quit(f == 0 ? 0 : 1); }
    private ValeGame V => _c.Vale!;
    private Player P => V.Player;

    private async Task Shot(string name)
    {
        if (_shots == null || DisplayServer.GetName() == "headless") return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        DirAccess.MakeDirRecursiveAbsolute(_shots);
        GetViewport().GetTexture().GetImage().SavePng($"{_shots}/{name}.png");
    }

    private async Task Teleport(Vector2 at, Vector2 face) { P.GlobalPosition = at; P.Face(face); P.GetNode<Camera2D>("Camera").ResetSmoothing(); await Frames(6 + _slow); }

    // puts the cursor over a world point: world → picture (camera, zoom) → window (stretch), as a real mouse event
    private async Task PointAt(Vector2 world)
    {
        Vector2 win = GetViewport().GetFinalTransform() * (GetViewport().GetCanvasTransform() * world);
        Input.ParseInputEvent(new InputEventMouseMotion { Position = win, GlobalPosition = win });
        await Flush(); await Frames(1);
    }

    private static float AngleBetween(float a, float b) => Mathf.Abs(Mathf.Wrap(a - b, -Mathf.Pi, Mathf.Pi));

    // ------------------------------------------------------------------ 7. aim
    private async Task AimPhase()
    {
        await Frames(30);
        if (_c.Vale == null) { Check("Novo jogo starts the Vale", false); Finish(); return; }
        Check("aiming by the mouse is the default", Aim.ByMouse);
        int cuts = 0; P.ActionStarted += (k, _) => { if (k == Player.ActionKind.Cut) cuts++; };
        Vector2? shotDir = null; P.QiFired += s => shotDir = s.Dir;

        // walking left, cursor to the right, J: the cut goes right and breaks the pot on the right
        Vector2 pot = ValeMap.Pots[3];
        await Teleport(pot + new Vector2(-34, 0), Vector2.Left);
        await PointAt(pot + new Vector2(0, -8));
        float x0 = P.GlobalPosition.X;
        Press(Key.A, true); await Flush(); await Frames(10);
        bool walkedLeft = P.GlobalPosition.X < x0 - 3 && P.Facing.X < -0.9f;
        await PointAt(pot + new Vector2(0, -8));
        Press(Key.J, true); await Flush(); await Frames(3); Press(Key.J, false); await Flush();
        await Frames(4);
        await Shot("01-mira-anda-esquerda-corta-direita");
        Press(Key.A, false); await Flush(); await Frames(12);
        Check("walking left with the cursor to the right, J cuts to the right", walkedLeft && cuts == 1 && AngleBetween(P.LastCutAngle, 0) < 0.6f, $"andou para a esquerda {walkedLeft}, ângulo do corte {Mathf.RadToDeg(P.LastCutAngle):0}°");
        Check("the cut's hit area agrees: the pot on the right breaks", V.Story.PotsBroken.Contains(3), $"potes quebrados [{string.Join(",", V.Story.PotsBroken)}]");

        // the Qi shot: walking left, cursor to the right, K
        await Teleport(ValeMap.Start + new Vector2(0, -60), Vector2.Left);
        P.Qi = P.MaxQi;
        await PointAt(P.GlobalPosition + new Vector2(120, -14));
        Press(Key.A, true); await Flush(); await Frames(8);
        await PointAt(P.GlobalPosition + new Vector2(120, -14));
        await Tap(Key.K); await Frames(10);
        Press(Key.A, false); await Flush(); await Frames(10);
        Check("walking left with the cursor to the right, K shoots to the right", shotDir is { } sd && sd.X > 0.95f, $"direção {shotDir}");

        // a remapped cut (U) aims too: cursor above the swordsman
        Bindings.Assign("cut", 0, $"k:{(long)Key.U}");
        await Teleport(ValeMap.Start + new Vector2(0, -60), Vector2.Down);
        await PointAt(P.GlobalPosition + new Vector2(0, -90));
        cuts = 0; await Tap(Key.U); await Frames(20);
        Check("the remapped cut (U) aims at the cursor too (up)", cuts == 1 && AngleBetween(P.LastCutAngle, -Mathf.Pi / 2) < 0.4f, $"ângulo {Mathf.RadToDeg(P.LastCutAngle):0}°");
        Bindings.Restore();

        // the knife aims the same way
        P.Equipped = Player.Weapon.Knife;
        await PointAt(P.GlobalPosition + new Vector2(-90, -14));
        cuts = 0; await Tap(Key.J); await Frames(20);
        Check("the knife aims at the cursor (left)", cuts == 1 && AngleBetween(P.LastCutAngle, Mathf.Pi) < 0.4f, $"ângulo {Mathf.RadToDeg(P.LastCutAngle):0}°");
        P.Equipped = Player.Weapon.Sword;

        // a locked target wins over the cursor; unlocking gives the cursor back on the next attack
        var statue = V.Fighters["statue"];
        await Teleport(statue.GlobalPosition + new Vector2(0, 50), Vector2.Up);
        for (int i = 0; i < 3 && V.Lock != statue; i++) await Tap(Key.Space);
        await PointAt(P.GlobalPosition + new Vector2(0, 100)); // cursor behind the swordsman
        cuts = 0; await Tap(Key.J); await Frames(20);
        float toStatue = (statue.GlobalPosition - P.GlobalPosition).Angle();
        Check("locked on (Space) with the cursor behind: the cut goes to the target", V.Lock == statue && cuts == 1 && AngleBetween(P.LastCutAngle, toStatue) < 0.35f, $"ângulo {Mathf.RadToDeg(P.LastCutAngle):0}°, alvo {Mathf.RadToDeg(toStatue):0}°");
        await Shot("02-mira-travado-cursor-atras");
        await Tap(Key.Space);
        await PointAt(P.GlobalPosition + new Vector2(0, 100));
        cuts = 0; await Tap(Key.J); await Frames(20);
        Check("unlocked: the next cut goes back to the cursor (down)", V.Lock == null && cuts == 1 && AngleBetween(P.LastCutAngle, Mathf.Pi / 2) < 0.4f, $"ângulo {Mathf.RadToDeg(P.LastCutAngle):0}°");

        // the keyboard option: aim along the facing
        Aim.ByMouse = false;
        await Teleport(ValeMap.Start + new Vector2(0, -60), Vector2.Right);
        Press(Key.D, true); await Flush(); await Frames(6); Press(Key.D, false); await Flush(); await Frames(2);
        await PointAt(P.GlobalPosition + new Vector2(-100, -14));
        cuts = 0; await Tap(Key.J); await Frames(20);
        Check("keyboard aim (Controles): the cut follows the facing (right), not the cursor", cuts == 1 && AngleBetween(P.LastCutAngle, 0) < 0.3f, $"ângulo {Mathf.RadToDeg(P.LastCutAngle):0}°");
        Aim.ByMouse = true;

        // the cursor maps through the camera and the window at other sizes
        foreach (var how in new[] { "maximizada", "tela cheia" })
        {
            DisplayServer.WindowSetMode(how == "maximizada" ? DisplayServer.WindowMode.Maximized : DisplayServer.WindowMode.Fullscreen);
            await Frames(40);
            await Teleport(ValeMap.Start + new Vector2(0, -60), Vector2.Up);
            Vector2 target = P.GlobalPosition + Player.AimOrigin + new Vector2(70, 70); // down-right, 45°
            await PointAt(target);
            cuts = 0; await Tap(Key.J); await Frames(20);
            Check($"{how} ({DisplayServer.WindowGetSize().X}x{DisplayServer.WindowGetSize().Y}): the cursor maps to the world (cut at 45° down-right)", cuts == 1 && AngleBetween(P.LastCutAngle, Mathf.Pi / 4) < 0.12f, $"ângulo {Mathf.RadToDeg(P.LastCutAngle):0}°");
        }
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed); DisplayServer.WindowSetSize(new Vector2I(1280, 720)); await Frames(20);

        // a click that closes a panel stays in the panel (no cut)
        await Tap(Key.Escape);
        var back = V.Hud.RowScreenRect("pause", 0);
        cuts = 0;
        if (back is { } b)
        {
            Vector2 at = b.GetCenter();
            Input.ParseInputEvent(new InputEventMouseMotion { Position = at, GlobalPosition = at }); await Flush(); await Frames(2);
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = at, GlobalPosition = at }); await Flush(); await Frames(3);
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false, Position = at, GlobalPosition = at }); await Flush(); await Frames(10);
        }
        Check("the click that closes the pause menu stays in the menu (no cut)", !V.Hud.PauseOpen && cuts == 0, $"cortes {cuts}");
        Finish();
    }
}
