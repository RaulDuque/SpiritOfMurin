using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Tests;

/// <summary>
/// Evidence captures of the Planície (windowed, not headless):
/// <list type="bullet">
/// <item><c>--mode=wide --out=file.png</c>: one 1920×1080 picture at 1:1 pixels of the start area
/// (clearing, creek bank, bush field, mud), HUD hidden, taken from the normal main scene.</item>
/// <item><c>--mode=boar</c>: the boar encounter, scripted with real key presses — the warning, the straight
/// charge, a sidestep and the counter-attack in the recovery. Record it with Godot's movie writer:
/// <c>--write-movie out.avi</c> (or <c>out.png</c> for a frame sequence).</item>
/// </list>
/// Both take <c>--seed=N</c>.
/// </summary>
public partial class PlainsCapture : Node
{
    private PlainsGame _game = null!;
    private string _mode = "wide", _out = "user://planicie-ampla.png";
    private int _seed = 20261126;

    /// <inheritdoc />
    public override void _Ready()
    {
        foreach (string a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--mode=")) _mode = a["--mode=".Length..];
            if (a.StartsWith("--out=")) _out = a["--out=".Length..];
            if (a.StartsWith("--seed=")) _seed = int.Parse(a["--seed=".Length..]);
        }
        _game = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _game.Seed = _seed; _game.ShowIntro = false;
        _game.Start = PlainsGame.StartMode.New; _game.SavePath = "user://teste_capture.json";
        AddChild(_game);
        _ = _mode == "boar" ? Boar() : Wide();
    }

    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }

    private static void Key(Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });

    private async Task Wide()
    {
        var win = GetWindow();
        win.ContentScaleSize = new Vector2I(1920, 1080);
        win.Size = new Vector2I(1920, 1080);
        _game.Hud.Visible = false;
        var gen = _game.Gen;
        // frame the clearing, the bank to the east and both guaranteed habitats to the west
        Vector2 ts = Vector2.One * gen.Settings.TileSize;
        Vector2 center = ((gen.BushAnchor + gen.MudAnchor) / 2f * ts + gen.StartPixel + new Vector2(gen.Settings.CreekOffset * ts.X, 0)) / 2f;
        _game.World.UpdateAround(center);
        var cam = _game.Player.GetNode<Camera2D>("Camera");
        cam.PositionSmoothingEnabled = false;
        cam.Offset = center - _game.Player.GlobalPosition - cam.Position;
        await Frames(30);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        GetViewport().GetTexture().GetImage().SavePng(_out);
        GD.Print($"wide capture: seed {_seed}, centre {center}, saved {_out}");
        GetTree().Quit(0);
    }

    private async Task Boar()
    {
        await Frames(5);
        var gen = _game.Gen;
        Vector2 mud = gen.MudAnchor * gen.Settings.TileSize;
        var boar = _game.World.Objects.GetChildren().OfType<Boar>().OrderBy(b => b.HomeAt.DistanceTo(mud)).First();
        foreach (var other in _game.World.Objects.GetChildren().OfType<Boar>().Where(b => b != boar)) _game.World.Remove(other);
        _game.Player.Equipped = Characters.Player.Weapon.Knife;
        _game.Player.GlobalPosition = boar.GlobalPosition + new Vector2(-170, 0);
        Key(global::Godot.Key.D, true); await Frames(2); Key(global::Godot.Key.D, false);
        await Frames(50); // settle the camera on the boar rooting in the mud
        // walk toward it until it notices and starts its warning
        Key(global::Godot.Key.D, true);
        for (int i = 0; i < 120 && boar.Brain.State != BoarState.Warn; i++) await Frames(1);
        Key(global::Godot.Key.D, false);
        for (int i = 0; i < 120 && boar.Brain.State == BoarState.Warn; i++) await Frames(1);
        // the charge has started: walk aside
        Key(global::Godot.Key.S, true);
        for (int i = 0; i < 30 && boar.Brain.State == BoarState.Charge; i++) await Frames(1);
        Key(global::Godot.Key.S, false);
        for (int i = 0; i < 60 && boar.Brain.State == BoarState.Charge; i++) await Frames(1);
        // the opening: walk to the boar and cut twice
        for (int i = 0; i < 40 && boar.Brain.State == BoarState.Recover; i++)
        {
            Vector2 to = boar.GlobalPosition + new Vector2(-18, 0) - _game.Player.GlobalPosition;
            if (to.Length() < 6) break;
            var keys = new System.Collections.Generic.List<Key>();
            if (to.X > 3) keys.Add(global::Godot.Key.D); else if (to.X < -3) keys.Add(global::Godot.Key.A);
            if (to.Y > 3) keys.Add(global::Godot.Key.S); else if (to.Y < -3) keys.Add(global::Godot.Key.W);
            foreach (var k in keys) Key(k, true);
            await Frames(1);
            foreach (var k in keys) Key(k, false);
        }
        Key(global::Godot.Key.D, true); await Frames(1); Key(global::Godot.Key.D, false);
        int h0 = boar.Brain.Health;
        for (int c = 0; c < 2; c++) { Key(global::Godot.Key.J, true); await Frames(2); Key(global::Godot.Key.J, false); await Frames(10); }
        await Frames(50);
        GD.Print($"boar clip: seed {_seed}, boar health {h0} -> {boar.Brain.Health}, charges {boar.Brain.Charges}, player Vida {_game.Player.Health}");
        GetTree().Quit(0);
    }
}
