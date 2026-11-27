using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Tests;

/// <summary>
/// The F5 flow played by keys only — no teleports, no calls into the game's methods, no XP handed out:
/// title → Novo jogo → flint and branch → the knife from the inventory → the trail from the clearing,
/// examined and followed → wait by the burrow, sneak and cut → hide and meat → back to the campfire:
/// roast, step, eat → strips → the sheet (C) and a point → the mat: meditate. It reports what happened,
/// it does not assert. Run windowed with <c>--write-movie file.avi</c> to record it; <c>--shots=folder</c>
/// saves stills. Uses <c>user://teste_percurso.json</c>. Take <c>--seed=N</c>.
/// </summary>
public partial class PlainsRoute : Node
{
    private PlainsGame _game = null!;
    private string? _shots;
    private int _seed = 20261126, _stuck;
    private readonly List<string> _log = new();

    /// <inheritdoc />
    public override void _Ready()
    {
        Game.Aim.UseForSession(false); // written for aiming by facing: the real cursor over the window must not steer the cuts
        foreach (string a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--seed=")) _seed = int.Parse(a["--seed=".Length..]);
            if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..].Replace('\\', '/');
        }
        _game = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _game.Seed = _seed; _game.Start = PlainsGame.StartMode.Title; _game.SavePath = "user://teste_percurso.json";
        SaveGame.Erase(_game.SavePath);
        AddChild(_game);
        _ = Route();
    }

    private Player P => _game.Player;
    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private async Task Flush() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static void Key(Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });
    private async Task Tap(Key k) { Key(k, true); await Flush(); await Frames(2); Key(k, false); await Flush(); await Frames(3); }
    private IEnumerable<T> Nodes<T>() where T : Node => _game.World.Objects.GetChildren().OfType<T>().Where(n => !n.IsQueuedForDeletion());
    private string Msgs(int n) => string.Join(" / ", _game.Hud.Messages.TakeLast(n));
    private void Note(string s) { _log.Add(s); GD.Print("PERCURSO " + s); }

    private async Task Shot(string name)
    {
        if (_shots == null || DisplayServer.GetName() == "headless") return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        DirAccess.MakeDirRecursiveAbsolute(_shots);
        GetViewport().GetTexture().GetImage().SavePng($"{_shots}/{name}.png");
    }

    // holds the 8-way keys toward a point (Shift to sneak); slides sideways when blocked
    private async Task Step(Vector2 target, bool sneak = false, int frames = 4)
    {
        Vector2 to = target - P.GlobalPosition, before = P.GlobalPosition;
        var keys = new List<Key>();
        if (to.X > 5) keys.Add(Godot.Key.D); else if (to.X < -5) keys.Add(Godot.Key.A);
        if (to.Y > 5) keys.Add(Godot.Key.S); else if (to.Y < -5) keys.Add(Godot.Key.W);
        if (_stuck > 2) { keys.Clear(); keys.Add(_stuck % 2 == 0 ? Godot.Key.W : Godot.Key.S); keys.Add(_stuck % 4 < 2 ? Godot.Key.A : Godot.Key.D); }
        if (sneak) Key(Godot.Key.Shift, true);
        foreach (var k in keys) Key(k, true);
        await Flush(); await Frames(frames);
        foreach (var k in keys) Key(k, false);
        if (sneak) Key(Godot.Key.Shift, false);
        await Flush();
        _stuck = P.GlobalPosition.DistanceTo(before) < 0.5f && keys.Count > 0 ? _stuck + 1 : Math.Max(0, _stuck - 1);
        if (_stuck > 10) _stuck = 0;
    }

    private async Task WalkTo(Vector2 target, float near = 8, int max = 400)
    {
        for (int i = 0; i < max && P.GlobalPosition.DistanceTo(target) > near; i++) await Step(target);
    }

    private async Task PickNearest(string pickupItem)
    {
        var p = Nodes<Pickup>().Where(x => x.ItemId == pickupItem).OrderBy(x => x.GlobalPosition.DistanceTo(P.GlobalPosition)).FirstOrDefault();
        if (p == null) { Note($"sem {pickupItem} por perto"); return; }
        await WalkTo(p.GlobalPosition, 10);
        await Tap(Godot.Key.E);
        Note($"E em {pickupItem}: {Msgs(1)}");
    }

    // moves the selection to the matching row; false (and nothing selected for E) when the row is not there
    private async Task<bool> InvSelect(Func<PlainsHud.Row, bool> match)
    {
        if (!_game.Hud.Rows.Any(match)) { Note("linha não encontrada no inventário"); return false; }
        for (int i = 0; i <= _game.Hud.Rows.Count && !match(_game.Hud.Rows[_game.Hud.Selected]); i++) await Tap(Godot.Key.S);
        return true;
    }

    private async Task Route()
    {
        await Frames(20);
        Note($"título: {string.Join(" | ", _game.Hud.TitleRows.Select(r => r.Text))}");
        await Shot("01-titulo");
        await Tap(Godot.Key.E); // Novo jogo (the only option without a save)
        await Frames(110);
        Note($"novo jogo, semente {_game.SeedInUse}; {_game.Hud.LifeText}; {_game.Hud.LevelText}");
        await Shot("02-despertar");
        await PickNearest("silex");
        await PickNearest("galho_rijo");
        await Tap(Godot.Key.I);
        if (await InvSelect(r => r.IsRecipe && r.Id == "faca_pedra")) await Tap(Godot.Key.E);
        Note($"fabricar: {Msgs(2)}");
        if (await InvSelect(r => !r.IsRecipe && r.Id == "faca_pedra")) await Tap(Godot.Key.E);
        await Shot("03-faca-no-inventario");
        await Tap(Godot.Key.I);
        Note($"mãos: {P.Equipped}");

        var start = Nodes<TrackPrint>().Where(t => t.CreatureId.StartsWith("rabbit")).OrderBy(t => t.GlobalPosition.DistanceTo(_game.Gen.StartPixel)).First();
        await WalkTo(start.GlobalPosition, 6);
        await Tap(Godot.Key.E);
        Note($"E na primeira pegada: {Msgs(3)}");
        await Tap(Godot.Key.E);
        Note($"E de novo: {Msgs(1)}");
        await Shot("04-pegada");
        foreach (var p in Nodes<TrackPrint>().Where(t => t.TrailId == start.TrailId).OrderBy(t => t.Index).Skip(1).ToList()) await WalkTo(p.GlobalPosition, 10, 60);
        var rabbit = Nodes<Rabbit>().FirstOrDefault(r => r.CreatureId == start.CreatureId);
        if (rabbit == null) { Note("o coelho da trilha não está carregado"); GetTree().Quit(1); return; }
        Note($"fim da trilha: {P.GlobalPosition.DistanceTo(rabbit.BurrowAt):0} px da toca; o coelho está {rabbit.Brain.State}");
        await Shot("05-fim-da-trilha");
        await Tap(Godot.Key.Space);
        Note($"Espaço: {_game.Hud.FocusText}");
        await Tap(Godot.Key.Space);

        await WalkTo(rabbit.BurrowAt + new Vector2(-34, 0), 6, 200);
        for (int i = 0; i < 60 * 25 && IsInstanceValid(rabbit) && rabbit.Brain.State != RabbitState.Dead; i++)
        {
            if (rabbit.Brain.Visible)
            {
                if (Player.InBladeArc(P.GlobalPosition, P.Facing, P.Tuning.KnifeReach, P.Tuning.HitArcDegrees, rabbit.GlobalPosition, rabbit.BodyRadius)) { await Tap(Godot.Key.J); await Frames(6); continue; }
                if (P.GlobalPosition.DistanceTo(rabbit.GlobalPosition) > 12) { await Step(rabbit.GlobalPosition, true, 3); continue; }
            }
            await Frames(1);
        }
        Note($"caça: {Msgs(2)}");
        var c = Nodes<Carcass>().FirstOrDefault();
        if (c != null)
        {
            await WalkTo(c.GlobalPosition, 8);
            await Tap(Godot.Key.E); Note($"E na carcaça (pele): {Msgs(2)}");
            await Tap(Godot.Key.E); Note($"E na carcaça (carne): {Msgs(2)}");
        }
        await Shot("06-caca-aproveitada");

        var fire = Nodes<Campfire>().First();
        await WalkTo(fire.GlobalPosition + new Vector2(-22, 6), 8, 600);
        Note($"chegou à fogueira: {P.GlobalPosition.DistanceTo(fire.GlobalPosition):0} px; na tela: '{_game.Hud.PromptText}'");
        await Tap(Godot.Key.E);
        Note($"E na fogueira: {Msgs(3)}");
        await Tap(Godot.Key.L); await Frames(15); await Tap(Godot.Key.L); await Frames(15);
        Note($"dois passos: vigor {P.Vigor:0}/{P.MaxVigor:0}");
        await Tap(Godot.Key.I);
        if (await InvSelect(r => !r.IsRecipe && r.Id == "carne_macia_assada")) await Tap(Godot.Key.E);
        Note($"comer: {Msgs(1)}; vigor {P.Vigor:0}");
        if (await InvSelect(r => r.IsRecipe && r.Id == "tiras_pele")) await Tap(Godot.Key.E);
        Note($"tiras: {Msgs(3)}");
        await Shot("07-inventario");
        await Tap(Godot.Key.I);
        Note($"nível {_game.Stats.Level}, pontos {_game.Stats.Points}, XP {_game.Stats.Xp}/{_game.Stats.XpToNext(_game.Stats.Level)}; HUD: {_game.Hud.LevelText}");
        await Tap(Godot.Key.C);
        await Shot("08-ficha");
        Note($"ficha: {_game.Hud.SheetDetail.Split('\n').Skip(1).FirstOrDefault()}");
        await Tap(Godot.Key.E); // VIT, the first row
        Note($"gastou 1 ponto: {Msgs(1)}");
        await Tap(Godot.Key.C);
        var mat = Nodes<MeditationMat>().First();
        await WalkTo(mat.GlobalPosition + new Vector2(0, 8), 6, 300);
        await Tap(Godot.Key.E);
        Note($"E na esteira: {Msgs(2)}");
        await Shot("09-meditou");
        await Frames(60);
        Note($"arquivo: {SaveGame.Read(_game.SavePath).Status}");
        GetTree().Quit(0);
    }
}
