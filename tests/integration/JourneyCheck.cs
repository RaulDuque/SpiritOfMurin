using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Progression;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Tests;

/// <summary>
/// The first progression of the Planície, end to end, through real keys. Two runs, two processes:
/// <list type="bullet">
/// <item><c>--phase=new</c>: a new journey. Knife; examine the trail from the clearing and follow it to
/// the burrow; focus the rabbit (level); hunt it; skin at Esfolar 0 (torn hide) and butcher; cook at the
/// campfire and eat (vigor); cut strips; the first level; a second rabbit at Esfolar 1 (common hide); the
/// bag and H; a third rabbit skinned with Ki (K); the sheet's preview and each attribute's effect;
/// meditate (writes the journey) and record what must come back.</item>
/// <item><c>--phase=resume</c>: the game opened again, the title menu, Continuar by key, and everything
/// compared with what was recorded; used carcasses and collected objects stay gone; meditating again does
/// not multiply anything.</item>
/// </list>
/// Run: <c>godot --path . res://tests/integration/JourneyCheck.tscn -- --phase=new --seed=N</c>, then
/// <c>-- --phase=resume</c>. Uses <c>user://teste_jornada.json</c>. Add <c>--shots=folder</c> (windowed).
/// </summary>
public partial class JourneyCheck : Node
{
    private const string SavePath = "user://teste_jornada.json", ExpectPath = "user://teste_jornada_esperado.json";
    private PlainsGame _game = null!;
    private Player _player = null!;
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private string? _shots;
    private string _phase = "new";
    private int _seed = 20261126, _stuck;

    /// <inheritdoc />
    public override void _Ready()
    {
        Game.Aim.UseForSession(false); // written for aiming by facing: the real cursor over the window must not steer the cuts
        foreach (string a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..];
            if (a.StartsWith("--seed=")) _seed = int.Parse(a["--seed=".Length..]);
            if (a.StartsWith("--phase=")) _phase = a["--phase=".Length..];
        }
        if (_phase == "new") { SaveGame.Erase(SavePath); SaveGame.Erase(ExpectPath); }
        _game = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _game.Seed = _seed; _game.ShowIntro = false; _game.SavePath = SavePath;
        _game.Start = _phase == "new" ? PlainsGame.StartMode.New : PlainsGame.StartMode.Title;
        AddChild(_game);
        _player = _game.Player;
        GD.Print($"window on screen {DisplayServer.WindowGetCurrentScreen()} of {DisplayServer.GetScreenCount()} ({DisplayServer.GetName()})");
        _ = _phase == "new" ? NewJourney() : Resume();
    }

    // ------------------------------------------------------------------ helpers

    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private async Task Flush() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static void Key(Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });
    private async Task Tap(Key k) { Key(k, true); await Flush(); await Frames(2); Key(k, false); await Flush(); await Frames(1); }
    private bool Recent(string s, int n = 6) => _game.Hud.Messages.TakeLast(n).Any(m => m.Contains(s));
    private string Last(int n = 3) => string.Join(" / ", _game.Hud.Messages.TakeLast(n));
    private IEnumerable<T> Nodes<T>() where T : Node => _game.World.Objects.GetChildren().OfType<T>().Where(n => !n.IsQueuedForDeletion());
    private int Count(string id) => _game.Inventory.Count(id);

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
        Key k = Mathf.Abs(face.X) >= Mathf.Abs(face.Y) ? (face.X >= 0 ? Godot.Key.D : Godot.Key.A) : (face.Y >= 0 ? Godot.Key.S : Godot.Key.W);
        for (int t = 0; t < 3; t++)
        {
            Key(k, true); await Flush(); await Frames(2); Key(k, false); await Flush(); await Frames(1);
            if (_player.Facing.Dot(face.Normalized()) > 0.9f) break;
        }
    }

    private async Task WalkToward(Vector2 target, bool sneak, int frames)
    {
        Vector2 to = target - _player.GlobalPosition, before = _player.GlobalPosition;
        var keys = new List<Key>();
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

    private async Task<bool> PickUp(string pickupItem)
    {
        var p = Nodes<Pickup>().Where(x => x.ItemId == pickupItem).OrderBy(x => x.GlobalPosition.DistanceTo(_player.GlobalPosition)).FirstOrDefault();
        if (p == null) return false;
        _player.GlobalPosition = p.GlobalPosition + new Vector2(0, 3);
        await Frames(3);
        await Tap(Godot.Key.E);
        return true;
    }

    private async Task SelectRow(Func<PlainsHud.Row, bool> match)
    {
        for (int i = 0; i <= _game.Hud.Rows.Count && !(_game.Hud.Rows.Count > 0 && match(_game.Hud.Rows[_game.Hud.Selected])); i++) await Tap(Godot.Key.S);
    }

    private async Task SelectAttr(string attr)
    {
        int want = Array.IndexOf(PlayerStats.AttributeIds, attr);
        for (int i = 0; i < 7 && _game.Hud.SheetSelected != want; i++) await Tap(Godot.Key.S);
    }

    // wait by the burrow, still; when the rabbit peeks out, sneak close and cut (knife in hand)
    private async Task<bool> Hunt(Rabbit r)
    {
        string id = r.CreatureId;
        await Teleport(r.BurrowAt + new Vector2(-34, 0), Vector2.Right);
        for (int i = 0; i < 60 * 16; i++)
        {
            if (!IsInstanceValid(r) || r.Brain.State == RabbitState.Dead) return _game.World.State.CarcassAt(id) != null;
            if (r.Brain.Visible)
            {
                Vector2 to = r.GlobalPosition - _player.GlobalPosition;
                if (Player.InBladeArc(_player.GlobalPosition, _player.Facing, _player.Tuning.KnifeReach, _player.Tuning.HitArcDegrees, r.GlobalPosition, r.BodyRadius))
                { await Tap(Godot.Key.J); await Frames(6); continue; }
                if (to.Length() > 12) { await WalkToward(r.GlobalPosition, true, 3); continue; }
            }
            await Frames(1);
        }
        return _game.World.State.CarcassAt(id) != null;
    }

    private async Task<Carcass?> GoToCarcass(string id)
    {
        await Frames(3);
        var c = Nodes<Carcass>().FirstOrDefault(x => x.CreatureId == id);
        if (c != null) { _player.GlobalPosition = c.GlobalPosition + new Vector2(-6, 2); await Frames(3); }
        return c;
    }

    private Rabbit? NextRabbit(Vector2 from) => Nodes<Rabbit>().Where(r => r.Brain.State != RabbitState.Dead && _game.World.State.CarcassAt(r.CreatureId) == null)
        .OrderBy(r => r.BurrowAt.DistanceTo(from)).FirstOrDefault();

    // ------------------------------------------------------------------ phase 1: a new journey

    private async Task NewJourney()
    {
        await Frames(10);
        var inv = _game.Inventory; var stats = _game.Stats; var skills = _game.Skills;
        Check("new journey: level 1, empty hands, full bars", stats.Level == 1 && _player.Equipped == Player.Weapon.None && _game.Ki == stats.MaxKi && _player.Vigor == _player.MaxVigor,
            $"Nv {stats.Level}, Ki {_game.Ki}/{stats.MaxKi}, vigor {_player.Vigor}/{_player.MaxVigor}");
        Check("the HUD shows the level and how much is missing", _game.Hud.LevelText.StartsWith("Nv 1 · XP 0/20 (faltam 20)"), _game.Hud.LevelText);
        await Shot("01-nova-jornada");

        // knife
        await PickUp("silex"); await PickUp("galho_rijo");
        Check("first craft is worth XP; the knife is made once", _game.Craft("faca_pedra") && !_game.Craft("faca_pedra") && Count("faca_pedra") == 1 && stats.TotalXp == 5, $"XP {stats.TotalXp}");
        _game.UseItem("faca_pedra");

        // the trail from the clearing
        var first = Nodes<TrackPrint>().Where(t => t.CreatureId.StartsWith("rabbit")).OrderBy(t => t.GlobalPosition.DistanceTo(_game.Gen.StartPixel)).First();
        string trail = first.TrailId, rabbitId = first.CreatureId;
        Check("a trail of paw prints starts at the clearing", first.GlobalPosition.DistanceTo(_game.Gen.StartPixel) < 5 * 32, $"{trail} at {first.GlobalPosition.DistanceTo(_game.Gen.StartPixel) / 32:0.0} tiles, to {rabbitId}");
        _player.GlobalPosition = first.GlobalPosition + new Vector2(0, 3); await Frames(3);
        int xp0 = stats.TotalXp;
        await Tap(Godot.Key.E);
        Check("E on a print: animal and direction; first look gives Rastreamento and XP", Recent("Pegadas pequenas, de coelho. Seguem para o") && skills.Grade("rastreamento") == 1 && stats.TotalXp == xp0 + _game.Data.Xp.TrailExamined + _game.Data.Xp.Practice, $"XP {xp0} → {stats.TotalXp}; {Last(4)}");
        await Shot("02-pegada-examinada");
        int xp1 = stats.TotalXp, pr1 = skills.Practice("rastreamento");
        await Tap(Godot.Key.E);
        var glowing = Nodes<TrackPrint>().Where(t => t.TrailId == trail && t.Glow != 0).Select(t => t.Index).ToList();
        Check("looking again: no XP, no practice; now (Iniciante) the distance and the next prints shine", stats.TotalXp == xp1 && skills.Practice("rastreamento") == pr1 && Recent("termina a uns") && glowing.Count > 0,
            $"glowing {string.Join(",", glowing)}; {Last(1)}");
        // follow the prints by walking
        var prints = Nodes<TrackPrint>().Where(t => t.TrailId == trail).OrderBy(t => t.Index).ToList();
        foreach (var p in prints.Skip(1))
            for (int i = 0; i < 40 && _player.GlobalPosition.DistanceTo(p.GlobalPosition) > 10; i++) await WalkToward(p.GlobalPosition, false, 3);
        var rabbit = Nodes<Rabbit>().FirstOrDefault(r => r.CreatureId == rabbitId);
        Check("following the prints by walking leads to the burrow", rabbit != null && _player.GlobalPosition.DistanceTo(rabbit.BurrowAt) < 40, rabbit == null ? "no rabbit" : $"{_player.GlobalPosition.DistanceTo(rabbit.BurrowAt):0} px from the burrow");
        await Shot("03-seguiu-as-pegadas");

        // focus: level
        if (rabbit != null && !rabbit.Brain.Visible) for (int i = 0; i < 400 && !rabbit.Brain.Visible; i++) await Frames(1);
        await Tap(Godot.Key.Space);
        Check("Space focuses the animal: name, level, temper", rabbit != null && _game.Hud.FocusText == $"Coelho de Campo · Nv {rabbit.Level} · Arisco", _game.Hud.FocusText);
        await Shot("04-foco-nivel");
        await Tap(Godot.Key.Space);

        // first hunt, Esfolar 0
        bool hunted = rabbit != null && await Hunt(rabbit);
        Check("the first hunt", hunted && Recent("XP (caça Nv"), Last(3));
        var carcass = await GoToCarcass(rabbitId);
        await Tap(Godot.Key.E);
        Check("Esfolar Bruto: torn hide; Esfolar goes 0 → 1", Count("pele_rasgada") == 1 && skills.Grade("esfolar") == 1 && Recent("Esfolar: Iniciante"), Last(3));
        await Tap(Godot.Key.E);
        Check("Preparo de Caça: the meat", Count("carne_macia") >= 1 && skills.Grade("preparo") == 1, $"carne {Count("carne_macia")}, tendão {Count("tendao")}");
        await Frames(2);
        int before = inv.Entries.Sum(e => e.Count);
        await Tap(Godot.Key.E);
        Check("the carcass is empty and gone; E again gives nothing", !Nodes<Carcass>().Any(c => c.CreatureId == rabbitId) && inv.Entries.Sum(e => e.Count) == before, $"items {before} → {inv.Entries.Sum(e => e.Count)}");

        // cook and eat
        var fire = Nodes<Campfire>().First();
        await Teleport(fire.GlobalPosition + new Vector2(-24, 6), Vector2.Right);
        await Tap(Godot.Key.E);
        Check("E at the campfire roasts the meat (once)", Count("carne_macia_assada") == 1 && Recent("Assou 1 Carne Macia Assada"), Last(3));
        for (int i = 0; i < 3; i++) { await Tap(Godot.Key.L); await Frames(12); }
        float vig = _player.Vigor;
        Check("steps spend vigor (and it shows)", vig <= _player.MaxVigor - 30 && _game.Hud.LevelText.Length > 0, $"vigor {vig:0}/{_player.MaxVigor}");
        await Shot("05-vigor-gasto");
        _game.UseItem("carne_macia_assada");
        float gainExpected = HarvestRules.CookedVigor(_game.Data, 25, skills.Grade("preparo"));
        Check("eating the roast restores vigor (Preparo bonus)", _player.Vigor >= vig + gainExpected - 1 && Count("carne_macia_assada") == 0, $"{vig:0} → {_player.Vigor:0} (+{gainExpected:0})");
        await Frames(120);
        Check("vigor comes back by itself", _player.Vigor > vig + gainExpected, $"{_player.Vigor:0}");

        // strips, first level
        int lv = stats.Level;
        Check("strips from the torn hide: 2, consuming it once", _game.Craft("tiras_pele") && Count("tira_amarracao") == 2 && Count("pele_rasgada") == 0 && !_game.Craft("tiras_pele"), $"tiras {Count("tira_amarracao")}");
        Check("the first level up (2 points)", stats.Level >= 2 && stats.Points == 2 * (stats.Level - 1) && Recent("Nível 2!", 10), $"Nv {stats.Level}, pontos {stats.Points}, XP total {stats.TotalXp}; {_game.Hud.LevelText}");
        await Shot("06-primeiro-nivel");

        // second rabbit at Esfolar 1: common hide
        var r2 = NextRabbit(rabbit?.BurrowAt ?? _game.Gen.StartPixel);
        bool h2 = r2 != null && await Hunt(r2);
        if (h2) { await GoToCarcass(r2!.CreatureId); await Tap(Godot.Key.E); await Tap(Godot.Key.E); }
        Check("a second rabbit at Esfolar Iniciante gives common hide", h2 && Count("pele_comum") == 1, $"pele comum {Count("pele_comum")}");
        Check("the bag: common hide + 2 strips, once", _game.Craft("bolsa_pele") && Count("bolsa_simples") == 1 && Count("tira_amarracao") == 0 && Count("pele_comum") == 0, Last(2));
        await PickUp("grama_sangue");
        string herb = inv.Entries.Select(e => e.Id).First(id => id.StartsWith("grama_sangue"));
        _player.TakeDamage(3, _player.GlobalPosition + Vector2.Right);
        await Frames(50);
        float hp = _player.Health;
        await Tap(Godot.Key.H);
        await Frames(90);
        Check("H uses the herb from the bag (heals over time)", _player.Health > hp && Recent($"Usou {_game.Catalog.Get(herb).Name}"), $"{hp} → {_player.Health:0.0}; {Last(1)}");

        // third rabbit skinned with Ki
        var r3 = NextRabbit(r2?.BurrowAt ?? _game.Gen.StartPixel);
        bool h3 = r3 != null && await Hunt(r3);
        float ki = _game.Ki;
        if (h3) { await GoToCarcass(r3!.CreatureId); await Tap(Godot.Key.K); }
        Check("K at Esfolar Iniciante guides Ki: fine hide, −15 Ki", h3 && Count("pele_fina") == 1 && _game.Ki <= ki - 14, $"pele fina {Count("pele_fina")}, Ki {ki:0} → {_game.Ki:0}");
        await Shot("07-esfolar-com-ki");

        // the sheet: preview before spending, and each attribute's effect
        _game.GainXp(stats.XpToNext(stats.Level) + stats.XpToNext(stats.Level + 1), "teste: pontos para os seis atributos"); // controlled: enough points for all six
        await Tap(Godot.Key.C);
        Check("C opens the sheet", _game.Hud.SheetOpen && _game.Hud.SheetHeader.Contains("Pontos:"), _game.Hud.SheetHeader);
        await SelectAttr("vit");
        Check("before spending, the sheet shows the concrete change", _game.Hud.SheetDetail.Contains($"Vida máxima: {_player.MaxHealth} → {_player.MaxHealth + 2}") && _game.Hud.SheetDetail.Contains("Esfolar — Iniciante"), _game.Hud.SheetDetail.Split('\n')[1]);
        await Shot("08-ficha-previa");
        int mh = _player.MaxHealth; await Tap(Godot.Key.E);
        Check("VIT: maximum Vida +2 (the HUD keeps the %)", _player.MaxHealth == mh + 2 && _game.Hud.LifeText.StartsWith("Vida "), $"{mh} → {_player.MaxHealth}; {_game.Hud.LifeText}");
        await SelectAttr("int");
        float pot = stats.MedicinePotency; await Tap(Godot.Key.E);
        Check("INT: medicinal potency up (the herb heals more)", stats.MedicinePotency > pot && _game.Effect(_game.Catalog.Get("grama_sangue_q1")).Contains($"+{4 * stats.MedicinePotency:0.0} Vida"), _game.Effect(_game.Catalog.Get("grama_sangue_q1")));
        await SelectAttr("con");
        float mv = _player.MaxVigor; await Tap(Godot.Key.E);
        Check("CON: maximum vigor +10", _player.MaxVigor == mv + 10, $"{mv} → {_player.MaxVigor}");
        await SelectAttr("sor");
        float sc = stats.SecondaryChance; await Tap(Godot.Key.E);
        Check("SOR: secondary chance up", stats.SecondaryChance > sc, $"{sc:P0} → {stats.SecondaryChance:P0}");
        await SelectAttr("ki");
        float mq = stats.MaxKi; await Tap(Godot.Key.E);
        Check("KI: maximum Ki +15", stats.MaxKi == mq + 15, $"{mq} → {stats.MaxKi}");
        await Tap(Godot.Key.C);
        // AGI measured on the body: walking speed while W is held, before and after the point
        async Task<float> WalkSpeed()
        {
            await Teleport(_game.Gen.StartPixel, Vector2.Up);
            Key(Godot.Key.W, true); await Flush(); await Frames(10);
            float v = _player.Velocity.Length();
            Key(Godot.Key.W, false); await Flush(); await Frames(2);
            return v;
        }
        float v0 = await WalkSpeed();
        await Tap(Godot.Key.C); await SelectAttr("agi"); await Tap(Godot.Key.E); await Tap(Godot.Key.C);
        float v1 = await WalkSpeed();
        Check("AGI: walking speed +4%", Mathf.Abs(v1 / v0 - 1.04f) < 0.005f && Mathf.IsEqualApprox(_player.MoveSpeedMultiplier, 1.04f), $"{v0:0.0} → {v1:0.0} px/s");

        // meditate: write the journey
        var mat = Nodes<MeditationMat>().First();
        await Teleport(mat.GlobalPosition + new Vector2(0, 10), Vector2.Up);
        await Tap(Godot.Key.E);
        var (status, saved) = SaveGame.Read(SavePath);
        Check("E on the mat meditates and writes the journey", status == SaveStatus.Ok && saved != null && Recent("Meditação gravada"), $"{status}; {Last(2)}");
        await Shot("09-meditou");
        // what must come back is exactly what was written (kept aside, since the resume run meditates again)
        if (saved != null) using (var f = Godot.FileAccess.Open(ExpectPath, Godot.FileAccess.ModeFlags.Write)) f.StoreString(SaveGame.ToJson(saved));
        Finish();
    }

    // ------------------------------------------------------------------ phase 2: the game opened again

    private async Task Resume()
    {
        await Frames(5);
        var expect = SaveGame.FromJson(Godot.FileAccess.GetFileAsString(ExpectPath));
        Check("the title offers Continuar first", _game.Hud.TitleOpen && _game.Hud.TitleRows.Count == 2 && _game.Hud.TitleRows[0].Id == "continue" && _game.Hud.TitleRows[0].Text.Contains("Nv "), _game.Hud.TitleRows.FirstOrDefault().Text ?? "");
        await Shot("10-titulo-continuar");
        await Tap(Godot.Key.E);
        await Frames(10);
        if (expect == null) { Check("the recorded expectation exists", false); Finish(); return; }
        var now = _game.Snapshot();
        static string Inv(SaveData d) => string.Join(",", d.Inventory.Select(i => $"{i.Id}:{i.Count}"));
        Check("Continuar: the same seed and the meditation spot", _game.Started && _game.SeedInUse == expect.Seed && _player.GlobalPosition.DistanceTo(new Vector2(expect.Spot[0], expect.Spot[1])) < 2, $"semente {_game.SeedInUse}, {_player.GlobalPosition}");
        Check("the same inventory, nothing multiplied", Inv(now) == Inv(expect), Inv(now));
        Check("the same level, XP, points and attributes", now.Level == expect.Level && now.Xp == expect.Xp && now.Points == expect.Points && now.Attributes.OrderBy(kv => kv.Key).SequenceEqual(expect.Attributes.OrderBy(kv => kv.Key)),
            $"Nv {now.Level}, XP {now.Xp}, pontos {now.Points}, {string.Join(" ", now.Attributes.Select(kv => kv.Key + kv.Value))}");
        Check("the same proficiencies", now.Practice.OrderBy(kv => kv.Key).SequenceEqual(expect.Practice.OrderBy(kv => kv.Key)) && now.PracticeKeys.Count == expect.PracticeKeys.Count, string.Join(" ", now.Practice.Select(kv => $"{kv.Key}:{kv.Value}")));
        Check("the same Vida, Ki, vigor and knife in hand", Mathf.Abs(now.Health - expect.Health) < 0.01f && Mathf.Abs(now.Ki - expect.Ki) < 1 && Mathf.Abs(now.Vigor - expect.Vigor) < 1 && now.Equipped == "knife" && _player.MaxHealth == _game.Stats.MaxHealth,
            $"Vida {now.Health}/{_player.MaxHealth}, Ki {now.Ki:0}, vigor {now.Vigor:0}, {now.Equipped}");
        var loaded = Nodes<Pickup>().Select(p => p.FeatureId).ToHashSet();
        Check("collected objects stay collected", expect.Collected.All(id => !loaded.Contains(id)) && expect.Collected.Count > 0, $"{expect.Collected.Count} collected");
        var carcasses = Nodes<Carcass>().Select(c => c.CreatureId).ToHashSet();
        var used = expect.Dead.Keys.Where(id => expect.PartsTaken.Count(p => p.StartsWith(id + "#")) >= 2).ToList();
        Check("used carcasses are not recreated; hunted animals do not come back", used.Count >= 2 && used.All(id => !carcasses.Contains(id)) && !Nodes<Rabbit>().Any(r => expect.Dead.ContainsKey(r.CreatureId)), $"{used.Count} carcasses used");
        // the third rabbit was only skinned (with Ki): its carcass comes back with the meat alone
        var partial = expect.Dead.Keys.Where(id => expect.PartsTaken.Count(p => p.StartsWith(id + "#")) == 1).ToList();
        var back = Nodes<Carcass>().Where(c => partial.Contains(c.CreatureId)).ToList();
        Check("a half-used carcass comes back with only what is left", partial.Count == 1 && back.Count == 1 && back[0].Parts.SequenceEqual(new[] { "meat" }), $"{string.Join(",", partial)}: {string.Join(",", back.SelectMany(c => c.Parts))}");
        var t = Nodes<TrackPrint>().FirstOrDefault(p => expect.Trails.Contains(p.TrailId));
        if (t != null)
        {
            _player.GlobalPosition = t.GlobalPosition + new Vector2(0, 3); await Frames(3);
            int xp = _game.Stats.TotalXp; await Tap(Godot.Key.E);
            Check("a trail examined before the save gives no XP again", _game.Stats.TotalXp == xp && Recent("Pegadas"), Last(1));
        }
        await Shot("11-continuou");
        var mat = Nodes<MeditationMat>().First();
        _player.GlobalPosition = mat.GlobalPosition + new Vector2(0, 10); await Frames(3);
        await Tap(Godot.Key.E);
        var again = SaveGame.Read(SavePath).Data;
        Check("meditating again writes the same goods (no duplication)", again != null && Inv(again) == Inv(expect) && again.Collected.Count == expect.Collected.Count && again.PartsTaken.Count == expect.PartsTaken.Count, again == null ? "no save" : Inv(again));
        Finish();
    }

    private void Finish()
    {
        int fails = _results.FindAll(r => !r.Ok).Count;
        GD.Print($"journey {_phase}, seed {_seed}: {_results.Count - fails}/{_results.Count} OK");
        GetTree().Quit(fails == 0 ? 0 : 1);
    }
}
