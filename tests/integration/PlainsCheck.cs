using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Tests;

/// <summary>
/// Integration check for the Planície's first stretch, through Godot's own input (keys and mouse):
/// wake empty-handed, pick up flint and branch with E (once each), leave the chunk and come back (the
/// pickup stays taken), craft and equip the stone knife from the inventory (I), cut with J, startle a
/// rabbit and watch it run to its burrow, wait there quietly and hunt it, take meat and hide once each,
/// face a boar (warning → straight charge → recovery), dodge by walking aside, hit it in the opening,
/// take a charge, heal with Grama Sangue-de-Coelho over time and find Raiz Amarga on rooted earth.
/// Long moves between places are teleports (the walking is covered by the controls check); every action
/// is a real key or click. Run: <c>godot --headless --path . res://tests/integration/PlainsCheck.tscn -- --seed=N</c>
/// (add <c>--shots=&lt;folder&gt;</c>, windowed, for screenshots). Exit 0 when every check passes.
/// </summary>
public partial class PlainsCheck : Node
{
    private PlainsGame _game = null!;
    private Player _player = null!;
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private readonly List<(Player.ActionKind Kind, int N)> _started = new();
    private string? _shots;
    private int _seed = 20261126;

    /// <inheritdoc />
    public override void _Ready()
    {
        Game.Aim.UseForSession(false); // written for aiming by facing: the real cursor over the window must not steer the cuts
        foreach (string a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..];
            if (a.StartsWith("--seed=")) _seed = int.Parse(a["--seed=".Length..]);
        }
        _game = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _game.Seed = _seed; _game.ShowIntro = false;
        _game.Start = PlainsGame.StartMode.New; _game.SavePath = "user://teste_plainscheck.json";
        AddChild(_game);
        _player = _game.Player;
        _player.ActionStarted += (k, n) => _started.Add((k, n));
        GD.Print($"window on screen {DisplayServer.WindowGetCurrentScreen()} of {DisplayServer.GetScreenCount()} ({DisplayServer.GetName()})");
        _ = Run();
    }

    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }

    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }

    // Parsed events are buffered until the next main-loop iteration; when two physics steps run in one
    // iteration, a press and its release could be flushed together and never be seen by the body. Waiting a
    // process frame after each event makes sure it was flushed before the physics frames that read it.
    private async Task Flush() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

    private static void Key(Godot.Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });

    private async Task Tap(Godot.Key k) { Key(k, true); await Flush(); await Frames(2); Key(k, false); await Flush(); await Frames(1); }

    private async Task Click(MouseButton b)
    {
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = b, Pressed = true, Position = new Vector2(640, 360) });
        await Flush(); await Frames(2);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = b, Pressed = false, Position = new Vector2(640, 360) });
        await Flush(); await Frames(1);
    }

    // holds the 8-way keys toward a point for a few frames (Shift held when sneaking); when blocked, slides sideways
    private async Task WalkToward(Vector2 target, bool sneak, int frames)
    {
        Vector2 to = target - _player.GlobalPosition, before = _player.GlobalPosition;
        var keys = new List<Godot.Key>();
        if (to.X > 6) keys.Add(Godot.Key.D); else if (to.X < -6) keys.Add(Godot.Key.A);
        if (to.Y > 6) keys.Add(Godot.Key.S); else if (to.Y < -6) keys.Add(Godot.Key.W);
        if (_stuck > 2) { keys.Clear(); keys.Add(_stuck % 2 == 0 ? Godot.Key.W : Godot.Key.S); }
        if (sneak) Key(Godot.Key.Shift, true);
        foreach (var k in keys) Key(k, true);
        await Flush(); await Frames(frames);
        foreach (var k in keys) Key(k, false);
        if (sneak) Key(Godot.Key.Shift, false);
        await Flush();
        _stuck = _player.GlobalPosition.DistanceTo(before) < 0.5f && keys.Count > 0 ? _stuck + 1 : Mathf.Max(0, _stuck - 1);
        if (_stuck > 8) _stuck = 0;
    }

    private int _stuck;

    private async Task Hold(Godot.Key k, int frames) { Key(k, true); await Flush(); await Frames(frames); Key(k, false); await Flush(); await Frames(1); }

    private async Task Shot(string name)
    {
        if (_shots == null || DisplayServer.GetName() == "headless") return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        string dir = _shots.Replace('\\', '/');
        DirAccess.MakeDirRecursiveAbsolute(dir);
        GetViewport().GetTexture().GetImage().SavePng($"{dir}/{name}.png");
    }

    private async Task Teleport(Vector2 p, Vector2 face)
    {
        _player.GlobalPosition = p;
        await Frames(3);
        // face the way with a 1-frame tap (a tiny step) of the matching key
        Godot.Key k = Mathf.Abs(face.X) >= Mathf.Abs(face.Y) ? (face.X >= 0 ? Godot.Key.D : Godot.Key.A) : (face.Y >= 0 ? Godot.Key.S : Godot.Key.W);
        for (int tries = 0; tries < 3; tries++)
        {
            Key(k, true); await Flush(); await Frames(2); Key(k, false); await Flush(); await Frames(1);
            if (_player.Facing.Dot(face.Normalized()) > 0.9f) break;
        }
    }

    private IEnumerable<T> Nodes<T>() where T : Node => _game.World.Objects.GetChildren().OfType<T>().Where(n => !n.IsQueuedForDeletion());

    private Pickup? NearestPickup(string item, Vector2 from) => Nodes<Pickup>().Where(p => p.ItemId == item).OrderBy(p => p.GlobalPosition.DistanceTo(from)).FirstOrDefault();

    // one action can show several messages (the item, then practice and XP): look at the last few
    private bool LastMessage(string contains) => _game.Hud.Messages.TakeLast(5).Any(m => m.Contains(contains));

    private string _lastPick = "";

    // moves the inventory selection with W/S until a row matches (gives up after a full turn)
    private async Task SelectRow(Predicate<PlainsHud.Row> match, Godot.Key key = Godot.Key.S)
    {
        for (int i = 0; i <= _game.Hud.Rows.Count && !(_game.Hud.Rows.Count > 0 && match(_game.Hud.Rows[_game.Hud.Selected])); i++)
            await Tap(key);
    }

    private async Task<bool> PickUp(string item)
    {
        var p = NearestPickup(item, _player.GlobalPosition);
        if (p == null) { _lastPick = "none loaded"; return false; }
        _player.GlobalPosition = p.GlobalPosition + new Vector2(0, 3);
        await Frames(3);
        var near = _game.NearestInteractable();
        _lastPick = $"target {p.FeatureId}, nearest {(near as Pickup)?.FeatureId}";
        await Tap(Godot.Key.E);
        return true;
    }

    private async Task Run()
    {
        await Frames(10);
        var inv = _game.Inventory;

        // ---- 1. waking: empty hands, the seed in the corner
        Check("wakes with empty hands", _player.Equipped == Player.Weapon.None && inv.Entries.Count == 0, $"{_player.Equipped}");
        Check("full Vida at the start", _player.Health == _player.MaxHealth, $"{_player.Health}/{_player.MaxHealth}");
        Check("the seed is shown", _game.Hud.SeedText == $"semente {_seed}", _game.Hud.SeedText);
        await Shot("01-despertar");
        _started.Clear();
        await Tap(Godot.Key.J); await Frames(10);
        Check("J with empty hands does not cut", _started.Count == 0 && LastMessage("Mãos vazias"), _game.Hud.Messages.LastOrDefault() ?? "");

        // ---- 2. flint and branch, with E, once each
        var flint = NearestPickup("silex", _game.Gen.StartPixel);
        string flintId = flint?.FeatureId ?? "";
        Vector2 flintAt = flint?.GlobalPosition ?? Vector2.Zero;
        Check("flint lies near the start", flint != null && flintAt.DistanceTo(_game.Gen.StartPixel) < 10 * 32, $"{flintId} at {flintAt}");
        await PickUp("silex");
        Check("E picks up flint", inv.Count("silex") == 1 && LastMessage("Sílex") && LastMessage("tem 1"), _game.Hud.Messages.LastOrDefault() ?? "");
        await Shot("02-pegou-silex");
        int before = inv.Count("silex");
        await Tap(Godot.Key.E); await Frames(2);
        bool sameSpotGone = !Nodes<Pickup>().Any(p => p.FeatureId == flintId);
        Check("the flint is gone; E again does not duplicate", sameSpotGone && inv.Count("silex") == before, $"silex {inv.Count("silex")}");
        await PickUp("galho_rijo");
        Check("E picks up a hard branch", inv.Count("galho_rijo") == 1 && LastMessage("Galho Rijo"), (_game.Hud.Messages.LastOrDefault() ?? "") + "; " + _lastPick);

        // ---- 3. leave the chunk and come back: the pickup stays taken, the rest is there again
        var chunk = _game.Gen.ChunkOfPixel(flintAt);
        var othersInChunk = Nodes<Pickup>().Where(p => _game.Gen.ChunkOfPixel(p.GlobalPosition) == chunk && _game.Gen.ChunkOfTile(_game.Gen.TileOfPixel(p.GlobalPosition).X, _game.Gen.TileOfPixel(p.GlobalPosition).Y) == chunk).Select(p => p.FeatureId).ToHashSet();
        int builds = _game.World.ChunkBuilds;
        await Teleport(flintAt + new Vector2(0, 6 * 512), Vector2.Down);
        await Frames(5);
        Check("far away, the chunk is freed", !_game.World.LoadedChunks.Contains(chunk), $"chunk {chunk}");
        await Teleport(flintAt + new Vector2(-40, 0), Vector2.Right);
        await Frames(5);
        var nowIds = Nodes<Pickup>().Select(p => p.FeatureId).ToList();
        Check("back again, the chunk was rebuilt", _game.World.LoadedChunks.Contains(chunk) && _game.World.ChunkBuilds > builds, $"builds {builds}→{_game.World.ChunkBuilds}");
        Check("the collected flint did not come back", !nowIds.Contains(flintId), flintId);
        Check("the other objects of the chunk came back", othersInChunk.All(nowIds.Contains), $"{othersInChunk.Count} objects");
        Check("no object node is duplicated", nowIds.Count == nowIds.Distinct().Count(), $"{nowIds.Count} pickups");
        // walk across a chunk border and back, by keys
        float borderX = Mathf.Floor(_player.GlobalPosition.X / 512f) * 512f;
        await Teleport(new Vector2(borderX + 30, _player.GlobalPosition.Y), Vector2.Left);
        var c0 = _game.Gen.ChunkOfPixel(_player.GlobalPosition);
        await Hold(Godot.Key.A, 30);
        var c1 = _game.Gen.ChunkOfPixel(_player.GlobalPosition);
        await Hold(Godot.Key.D, 30);
        Check("walking crosses a chunk border and back", c1 != c0 && _game.Gen.ChunkOfPixel(_player.GlobalPosition) == c0, $"{c0} → {c1} → {_game.Gen.ChunkOfPixel(_player.GlobalPosition)}");
        Check("inventory unchanged by the trip", inv.Count("silex") == 1 && inv.Count("galho_rijo") == 1, $"silex {inv.Count("silex")}, galho {inv.Count("galho_rijo")}");

        // ---- 4. inventory: craft the knife once, equip it
        await Tap(Godot.Key.I); await Frames(2);
        Check("I opens the inventory", _game.Hud.InventoryOpen);
        await SelectRow(r => r.IsRecipe && r.Id == "faca_pedra");
        await Shot("03-inventario-receita");
        await Tap(Godot.Key.E); await Frames(2);
        Check("E on the recipe crafts the knife", inv.Count("faca_pedra") == 1 && inv.Count("silex") == 0 && inv.Count("galho_rijo") == 0, $"faca {inv.Count("faca_pedra")}");
        await SelectRow(r => r.IsRecipe && r.Id == "faca_pedra");
        await Tap(Godot.Key.E); await Frames(2);
        Check("crafting again without materials does nothing", inv.Count("faca_pedra") == 1 && LastMessage("Falta"), _game.Hud.Messages.LastOrDefault() ?? "");
        await SelectRow(r => !r.IsRecipe && r.Id == "faca_pedra", Godot.Key.W);
        await Tap(Godot.Key.E); await Frames(2);
        Check("E on the knife equips it", _player.Equipped == Player.Weapon.Knife, $"{_player.Equipped}");
        await Shot("04-faca-equipada");
        await Tap(Godot.Key.I); await Frames(2);
        Check("I closes the inventory", !_game.Hud.InventoryOpen);
        _started.Clear();
        for (int i = 0; i < 3; i++) { await Tap(Godot.Key.J); await Frames(5); }
        await Frames(20);
        Check("J cuts with the knife (short chain: never a third cut)", _started.Count >= 2 && _started[1].N == 2 && _started.All(a => a.N <= _player.Tuning.KnifeMaxChain), Describe());
        await Tap(Godot.Key.J); await Frames(5); await Shot("05-corte-faca"); await Frames(20);
        _started.Clear();
        await Click(MouseButton.Left); await Frames(25);
        Vector2 s0 = _player.GlobalPosition;
        await Click(MouseButton.Right); await Frames(20);
        Check("left click cuts with the knife, right click steps", _started.Count == 2 && _started[0].Kind == Player.ActionKind.Cut && _started[1].Kind == Player.ActionKind.Step && s0.DistanceTo(_player.GlobalPosition) > 20, $"{Describe()}; moved {s0.DistanceTo(_player.GlobalPosition):0} px");
        s0 = _player.GlobalPosition;
        await Tap(Godot.Key.L); await Frames(20);
        Check("L steps", s0.DistanceTo(_player.GlobalPosition) > 20, $"moved {s0.DistanceTo(_player.GlobalPosition):0} px");

        // ---- 5. the rabbit: hears steps, ears up, runs to the burrow
        var rabbit = Nodes<Rabbit>().OrderBy(r => r.GlobalPosition.DistanceTo(_player.GlobalPosition)).FirstOrDefault();
        Check("a rabbit lives by a burrow", rabbit != null && _game.Gen.HabitatAt(_game.Gen.TileOfPixel(rabbit.BurrowAt).X, _game.Gen.TileOfPixel(rabbit.BurrowAt).Y) == Habitat.Bushes, rabbit?.CreatureId ?? "none");
        if (rabbit != null)
        {
            foreach (var other in Nodes<Rabbit>().Where(r => r != rabbit && r.BurrowAt.DistanceTo(rabbit.BurrowAt) < 400)) _game.World.Remove(other);
            await Teleport(rabbit.GlobalPosition + new Vector2(-130, 0), Vector2.Right);
            await Frames(20);
            bool alerted = false, earsShot = false;
            // walk (not sneak) toward it, steering around bushes, until it hears the steps
            for (int i = 0; i < 80 && rabbit.Brain.State == RabbitState.Graze; i++) await WalkToward(rabbit.GlobalPosition, false, 3);
            for (int i = 0; i < 200 && rabbit.Brain.State != RabbitState.Hidden; i++)
            {
                alerted |= rabbit.Brain.EarsUp;
                if (rabbit.Brain.State == RabbitState.Alert && !earsShot) { earsShot = true; await Shot("06-coelho-orelhas"); }
                await Frames(1);
            }
            Check("walking close, the rabbit raises its ears and runs", alerted && rabbit.Brain.Flights >= 1, $"flights {rabbit.Brain.Flights}");
            Check("it hides in its burrow", rabbit.Brain.State == RabbitState.Hidden && rabbit.GlobalPosition.DistanceTo(rabbit.BurrowAt) < 6, $"{rabbit.Brain.State}, {rabbit.GlobalPosition.DistanceTo(rabbit.BurrowAt):0} px from it");

            // wait by the burrow, still; when it peeks out, sneak (Shift) close and cut
            await Teleport(rabbit.BurrowAt + new Vector2(-34, 0), Vector2.Right);
            bool hunted = false;
            for (int i = 0; i < 60 * 14 && !hunted; i++)
            {
                if (!IsInstanceValid(rabbit) || rabbit.Brain.State == RabbitState.Dead) { hunted = true; break; }
                if (rabbit.Brain.Visible)
                {
                    Vector2 to = rabbit.GlobalPosition - _player.GlobalPosition;
                    if (Player.InBladeArc(_player.GlobalPosition, _player.Facing, _player.Tuning.KnifeReach, _player.Tuning.HitArcDegrees, rabbit.GlobalPosition, rabbit.BodyRadius))
                    { await Tap(Godot.Key.J); await Frames(6); continue; }
                    if (to.Length() > 12) { await WalkToward(rabbit.GlobalPosition, true, 3); continue; }
                }
                await Frames(1);
            }
            Check("waiting by the burrow and sneaking, the knife hunts the rabbit", hunted && _game.World.State.CarcassAt(rabbit.CreatureId) != null, hunted ? "carcass on the ground" : $"state {rabbit.Brain.State}");
            await Frames(3);
            var carcass = Nodes<Carcass>().FirstOrDefault(c => c.CreatureId == rabbit.CreatureId);
            if (carcass != null)
            {
                await Teleport(carcass.GlobalPosition + new Vector2(-8, 2), Vector2.Right);
                await Shot("07-carcaca-coelho");
                await Tap(Godot.Key.E); await Tap(Godot.Key.E); await Tap(Godot.Key.E); await Frames(3);
                Check("the carcass gives meat and hide, once each", inv.Count("carne_macia") == 1 && inv.Count("pele_rasgada") == 1 && !Nodes<Carcass>().Any(c => c.CreatureId == carcass.CreatureId), $"carne {inv.Count("carne_macia")}, pele rasgada {inv.Count("pele_rasgada")} (Esfolar Bruto)");
            }
            else Check("the carcass gives meat and hide, once each", false, "no carcass");
        }

        // ---- 6. the boar: warning, straight charge, recovery; dodge by walking aside, hit in the opening
        var boar = Nodes<Boar>().OrderBy(b => b.GlobalPosition.DistanceTo(_game.Gen.StartPixel)).FirstOrDefault();
        Check("a boar roots in the mud", boar != null && _game.Gen.HabitatAt(_game.Gen.TileOfPixel(boar.HomeAt).X, _game.Gen.TileOfPixel(boar.HomeAt).Y) == Habitat.Mud, boar?.CreatureId ?? "none");
        var roots = Nodes<Pickup>().Where(p => p.ItemId == "raiz_amarga").ToList();
        if (boar != null)
        {
            Check("Raiz Amarga grows on the rooted earth near the boar", roots.Any(r => r.GlobalPosition.DistanceTo(boar.HomeAt) < 120), $"{roots.Count} roots");
            foreach (var other in Nodes<Boar>().Where(b => b != boar && b.HomeAt.DistanceTo(boar.HomeAt) < 600)) _game.World.Remove(other);
            await Teleport(boar.GlobalPosition + new Vector2(-95, 0), Vector2.Right);
            float hp0 = _player.Health;
            for (int i = 0; i < 120 && boar.Brain.State != BoarState.Warn; i++) await Frames(1);
            Check("the boar warns (hoof, head down)", boar.Brain.State == BoarState.Warn);
            // measured in engine physics ticks (an awaited frame can cover more than one tick when the window lags)
            ulong warnStart = Engine.GetPhysicsFrames() - (ulong)Mathf.RoundToInt(boar.Brain.StateTime * Engine.PhysicsTicksPerSecond);
            bool lockedShot = false;
            for (int i = 0; i < 300 && boar.Brain.State == BoarState.Warn; i++)
            {
                await Frames(1);
                if (boar.Brain.DirectionLocked && !lockedShot) { lockedShot = true; await Shot("08-javali-aviso"); }
            }
            float warn = (Engine.GetPhysicsFrames() - warnStart) / (float)Engine.PhysicsTicksPerSecond;
            Check("the warning lasts the tuned time (0.8 s)", Mathf.Abs(warn - _game.World.BoarTuning.WarnTime) < 0.1f, $"{warn:0.00} s");
            // a hit during the charge glances off
            Vector2 chargeStart = boar.GlobalPosition;
            Key(Godot.Key.S, true); await Flush();
            await Frames(4); await Shot("09-javali-investida");
            for (int i = 0; i < 40 && boar.Brain.State == BoarState.Charge; i++) await Frames(1);
            Key(Godot.Key.S, false);
            Check("walking aside dodges the straight charge", _player.Health == hp0 && boar.Brain.State == BoarState.Recover, $"Vida {_player.Health}, {boar.Brain.State}");
            Check("the charge ran straight", Mathf.Abs(boar.GlobalPosition.Y - chargeStart.Y) < 2 && Mathf.Abs(boar.GlobalPosition.X - chargeStart.X) > 60, $"{chargeStart} → {boar.GlobalPosition}");
            await Shot("10-javali-recuperacao");
            // counter-attack in the opening
            float bh = boar.Brain.Health;
            await Teleport(boar.GlobalPosition + new Vector2(0, 16), Vector2.Up);
            string keys = string.Join("", new[] { "move_up", "move_down", "move_left", "move_right", "sneak", "cut" }.Where(a => Input.IsActionPressed(a)));
            string atJ = $"held [{keys}] {boar.Brain.State} {boar.Brain.StateTime:0.00}s, dist {boar.GlobalPosition.DistanceTo(_player.GlobalPosition):0}, facing {_player.Facing}";
            if (boar.Brain.State == BoarState.Recover) { await Tap(Godot.Key.J); await Frames(8); }
            Check("the counter-attack lands during the recovery", boar.Brain.Health < bh, $"health {bh} → {boar.Brain.Health}; at J: {atJ}");
            // stand in the line: the charge hits when it reaches the body (first let it walk back home, calm)
            await Teleport(boar.HomeAt + new Vector2(-300, 0), Vector2.Left);
            for (int i = 0; i < 60 * 12 && boar.Brain.State != BoarState.Rooting; i++) await Frames(1);
            await Teleport(boar.GlobalPosition + new Vector2(-90, 0), Vector2.Right);
            float hp1 = _player.Health; float minDist = 999; string hitBy = "";
            foreach (var other in Nodes<Boar>()) other.HitPlayer += b => hitBy = $"{b.CreatureId} at {b.GlobalPosition.DistanceTo(_player.GlobalPosition):0} px";
            for (int i = 0; i < 300 && _player.Health == hp1; i++)
            {
                if (boar.Brain.State == BoarState.Charge) minDist = Mathf.Min(minDist, boar.GlobalPosition.DistanceTo(_player.GlobalPosition));
                await Frames(1);
            }
            Check("standing in the line, the charge hurts on contact", _player.Health == hp1 - boar.Tuning.ContactDamage, $"Vida {hp1} → {_player.Health}, closest during charge {minDist:0} px, hit by {hitBy}");
            await Frames(2);
            Check("the Vida bar shows the percentage", _game.Hud.LifeText.StartsWith($"Vida {PlainsHud.LifePercent(_player.Health, _player.MaxHealth)}%") && _player.Health < _player.MaxHealth, _game.Hud.LifeText);
            await Shot("11-javali-acertou");

            // the boar does not follow to the clearing
            await Teleport(_game.Gen.StartPixel, Vector2.Down);
            await Frames(30);
            Check("the boar does not follow to the clearing", boar.GlobalPosition.DistanceTo(_player.GlobalPosition) > 120, $"{boar.GlobalPosition.DistanceTo(_player.GlobalPosition):0} px");

            // ---- 6b. focus shows its level; hunt it down in its openings; its materials (brief: meat, hide, tusks)
            Vector2 boarHome = boar.HomeAt;
            await Teleport(boar.GlobalPosition + new Vector2(-120, 0), Vector2.Right);
            await Tap(Godot.Key.Space);
            Check("Space on the boar: name, level (3-5), temper", _game.Hud.FocusText == $"Javali da Mata · Nv {boar.Level} · Territorial" && boar.Level is >= 3 and <= 5, _game.Hud.FocusText);
            await Tap(Godot.Key.Space);
            _player.Revive();
            for (int round = 0; round < 12 && IsInstanceValid(boar) && boar.Brain.State != BoarState.Dead; round++)
            {
                if (_player.Health < 4) _player.Revive();
                await Teleport(boar.GlobalPosition + new Vector2(-90, 0), Vector2.Right);
                for (int i = 0; i < 200 && boar.Brain.State is not (BoarState.Charge or BoarState.Dead); i++) await Frames(1);
                Key(Godot.Key.S, true); await Flush();
                for (int i = 0; i < 60 && boar.Brain.State == BoarState.Charge; i++) await Frames(1);
                Key(Godot.Key.S, false); await Flush();
                for (int hit = 0; hit < 2 && IsInstanceValid(boar) && boar.Brain.State == BoarState.Recover; hit++)
                {
                    _player.GlobalPosition = boar.GlobalPosition + new Vector2(0, 14); await Frames(1);
                    await Teleport(boar.GlobalPosition + new Vector2(0, 14), Vector2.Up);
                    await Tap(Godot.Key.J); await Frames(8);
                }
            }
            var boarCarcass = Nodes<Carcass>().FirstOrDefault(c => c.CreatureId == boar.CreatureId);
            Check("the boar falls after its openings", boarCarcass != null, $"rounds; carcass {(boarCarcass != null)}");
            if (boarCarcass != null)
            {
                string hide = _game.Data.Hides["boar"][_game.Skills.Grade("esfolar")];
                _player.GlobalPosition = boarCarcass.GlobalPosition + new Vector2(-8, 2); await Frames(3);
                await Tap(Godot.Key.E); await Tap(Godot.Key.E); await Tap(Godot.Key.E); await Tap(Godot.Key.E);
                Check("the boar gives hide (by Esfolar), Carne Rija and tusks, once each", inv.Count(hide) == 1 && inv.Count("carne_rija") >= 1 && inv.Count("presa_javali") == 1 && !Nodes<Carcass>().Any(c => c.CreatureId == boar.CreatureId),
                    $"{hide} {inv.Count(hide)}, carne rija {inv.Count("carne_rija")}, presas {inv.Count("presa_javali")}");
            }
            _player.Revive(); await Frames(50); _player.TakeDamage(2, _player.GlobalPosition + Vector2.Right); await Frames(45); // Revive leaves a moment of invulnerability

            // ---- 7. herbs: Grama Sangue-de-Coelho heals over time; Raiz Amarga by the rooted earth
            await Teleport(_game.Gen.StartPixel, Vector2.Down);
            await Frames(30);
            await PickUp("grama_sangue");
            string herbId = inv.Entries.Select(e => e.Id).FirstOrDefault(id => id.StartsWith("grama_sangue")) ?? "grama_sangue_q0";
            Check("E picks up Grama Sangue-de-Coelho by a stone", inv.Count(herbId) >= 1 && LastMessage("Grama Sangue-de-Coelho"), $"{herbId}: {string.Join(" / ", _game.Hud.Messages.TakeLast(3))}");
            float hurt = _player.Health;
            await Tap(Godot.Key.I);
            int herbsBefore = inv.Count(herbId);
            float regen = _game.Catalog.Get(herbId).RegenAmount * _game.Stats.MedicinePotency;
            await SelectRow(r => !r.IsRecipe && r.Id == herbId);
            await Tap(Godot.Key.E);
            await Tap(Godot.Key.I);
            await Frames(60);
            float mid = _player.Health;
            await Frames(330);
            float end = _player.Health;
            Check("the herb restores Vida over time", inv.Count(herbId) == herbsBefore - 1 && mid > hurt && mid < end && end >= Mathf.Min(_player.MaxHealth, hurt + regen - 0.1f), $"{hurt} → {mid:0.0} (1 s) → {end:0.0} (6.5 s), herb {regen:0.0}");
            await Shot("12-cura-erva");
            var root = roots.Where(IsInstanceValid).OrderBy(r => r.GlobalPosition.DistanceTo(boarHome)).FirstOrDefault();
            if (root != null)
            {
                await Teleport(root.GlobalPosition + new Vector2(-10, 4), Vector2.Right);
                await Tap(Godot.Key.E);
                Check("E picks up Raiz Amarga, name and quantity shown", inv.Count("raiz_amarga") == 1 && LastMessage("Raiz Amarga") && LastMessage("tem 1"), _game.Hud.Messages.LastOrDefault() ?? "");
                await Shot("13-raiz-amarga");
            }
            else Check("E picks up Raiz Amarga", false, "no root loaded");
        }

        int fails = _results.FindAll(r => !r.Ok).Count;
        GD.Print($"seed {_seed}: {_results.Count - fails}/{_results.Count} OK");
        GetTree().Quit(fails == 0 ? 0 : 1);
    }

    private string Describe() => string.Join(",", _started.ConvertAll(a => a.Kind == Player.ActionKind.Cut ? $"cut{a.N}" : "step"));
}
