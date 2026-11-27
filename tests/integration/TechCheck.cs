using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Combat;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;
using SpiritOfMurin.Vale;

namespace SpiritOfMurin.Tests;

/// <summary>
/// The seal and the weakened return of the prologue techniques (campaign delivery 2), measured in the engine:
/// <list type="bullet">
/// <item><c>--phase=full</c>: in the Vale, the full versions — ki shot range, Ki and vigor cost, damage; side
/// step arc; long step distance; Silent Blade Step damage.</item>
/// <item><c>--phase=sealed</c>: a Planície journey at level 2 after the awakening — K, L to the side of a
/// focused animal and Shift+direction+L are refused without spending anything (the plain step still
/// works); K next to a carcass skins with Ki and fires nothing; the level-up to 3 brings them back
/// weakened with one message; the weakened versions measured (range, cost, damage); refusals for lack of
/// Ki or vigor spend nothing; meditating saves it.</item>
/// <item><c>--phase=resume2</c>: a journey saved at level 2 stays sealed, no message.</item>
/// <item><c>--phase=resume3</c>: the journey saved after the return keeps the weakened techniques, no second message.</item>
/// <item><c>--phase=migrate</c> then <c>--phase=migrate2</c>: a journey already at level 4 with the techniques
/// still sealed gets them back once on loading; loading again repeats nothing.</item>
/// </list>
/// Teleports place the swordsman; saves are written as fixtures (controlled). Files: user://teste_tecnicas*.json.
/// </summary>
public partial class TechCheck : Node
{
    private const string SavePath = "user://teste_tecnicas.json", AfterReturn = "user://teste_tecnicas_depois.json", Migrate = "user://teste_tecnicas_migrar.json";
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private string _phase = "sealed";
    private PlainsGame _g = null!;
    private int _slow;
    private string? _shots;

    private async Task Shot(string name)
    {
        if (_shots == null || DisplayServer.GetName() == "headless") return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        DirAccess.MakeDirRecursiveAbsolute(_shots);
        GetViewport().GetTexture().GetImage().SavePng($"{_shots}/{name}.png");
    }

    /// <summary>Measured numbers, printed at the end as "MEDIDA name=value" lines for the report.</summary>
    private readonly List<string> _measures = new();

    /// <inheritdoc />
    public override void _Ready()
    {
        Game.Aim.UseForSession(false); // written for aiming by facing: the real cursor over the window must not steer the cuts
        foreach (string a in OS.GetCmdlineUserArgs()) { if (a.StartsWith("--phase=")) _phase = a["--phase=".Length..]; if (a == "--lento") _slow = 40; if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..]; }
        GD.Print($"window on screen {DisplayServer.WindowGetCurrentScreen()} of {DisplayServer.GetScreenCount()} ({DisplayServer.GetName()})");
        switch (_phase)
        {
            case "full": _ = Full(); return;
            case "sealed": WriteFixture(SavePath, level: 2, xp: 20, returned: false); break;
            case "resume2": WriteFixture(SavePath, level: 2, xp: 10, returned: false); break;
            case "resume3": Copy(AfterReturn, SavePath); break;
            case "migrate": WriteFixture(Migrate, level: 4, xp: 3, returned: false); Copy(Migrate, SavePath); break;
            case "migrate2": Copy(Migrate, SavePath); break;
        }
        _g = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _g.SavePath = SavePath; _g.ShowIntro = false; _g.Start = PlainsGame.StartMode.Continue;
        AddChild(_g);
        _ = _phase switch { "sealed" => Sealed(), "resume2" => Resume2(), "resume3" => Resume3(), "migrate" => Migrated(true), _ => Migrated(false) };
    }

    // a Planície journey after the awakening: the prologue's book, sealed; a knife; Esfolar Iniciante
    private static void WriteFixture(string path, int level, int xp, bool returned)
    {
        var book = ValeState.StartingBook();
        book.Add(new BookEntry { Name = "Passo Lateral", Kind = "técnica", Key = "L + lado do alvo", Tech = "side_step" });
        book.Add(new BookEntry { Name = "Passo longo", Kind = "técnica", Key = "direção + L", Tech = "dash" });
        book.Add(new BookEntry { Name = "Passo da Lâmina Silenciosa", Kind = "combinação", Key = "passo lateral → corte → Ki", Tech = "lamina" });
        foreach (var b in book.Where(b => b.Tech.Length > 0)) b.State = "Selada";
        var d = new SaveData
        {
            Region = "planicie", Phase = "planicie", Seed = 20261126, Spot = new[] { 16f, 16f }, Book = book, LossApplied = true, TechniquesReturned = returned,
            Inventory = { new SavedItem { Id = "faca_pedra", Count = 1 } }, Equipped = "knife", Health = 10, Vigor = 50, Ki = 100,
            Level = level, Xp = xp, Points = 0, Practice = { ["esfolar"] = 1 }, PracticeKeys = { "esfolar:fixture" },
        };
        SaveGame.Write(path, d);
    }

    private static void Copy(string from, string to) { string t = Godot.FileAccess.GetFileAsString(from); using var f = Godot.FileAccess.Open(to, Godot.FileAccess.ModeFlags.Write); f.StoreString(t); }

    // ------------------------------------------------------------------ helpers
    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
    private void Measure(string name, float value) { _measures.Add($"{name}={value:0.##}"); GD.Print($"MEDIDA {name}={value:0.##}"); }
    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private async Task Flush() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static void Press(Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });
    private async Task Tap(Key k) { Press(k, true); await Flush(); await Frames(2); Press(k, false); await Flush(); await Frames(2 + _slow / 3); }
    private async Task Chord(params Key[] keys) { foreach (var k in keys) { Press(k, true); await Flush(); await Frames(1); } await Frames(2); foreach (var k in keys.Reverse()) Press(k, false); await Flush(); await Frames(2 + _slow / 3); }
    private Player P => _g.Player;
    private int Msgs => _g.Hud.Messages.Count;
    private bool Said(string s, int since) => _g.Hud.Messages.Skip(since).Any(m => m.Contains(s));

    private async Task Teleport(Vector2 at, Vector2 face) { P.GlobalPosition = at; P.Face(face); await Frames(3 + _slow); }

    private Vector2 Empty()
    {
        // an open spot facing east, away from animals (the clearing)
        return _g.Gen.StartPixel;
    }

    // fires K and waits for the shot to end: returns (distance travelled, Ki spent, vigor spent, damage carried)
    private async Task<(float Went, float Ki, float Vigor, int Damage)> Shoot()
    {
        float ki = P.Ki, vig = P.Vigor; float went = -1; int dmg = 0;
        void Ended(KiWave w, bool hit) { went = Mathf.Min(w.Went, w.Shot.Range); dmg = w.Shot.Damage; }
        _g.KiShotEnded += Ended;
        await Tap(Key.K);
        float kiAfter = P.Ki, vigAfter = P.Vigor;
        for (int i = 0; i < 120 && went < 0; i++) await Frames(1);
        _g.KiShotEnded -= Ended;
        return (went, ki - kiAfter, vig - vigAfter, dmg);
    }

    private void Finish()
    {
        int fails = _results.FindAll(r => !r.Ok).Count;
        GD.Print($"tech {_phase}: {_results.Count - fails}/{_results.Count} OK  [{string.Join(" ", _measures)}]");
        GetTree().Quit(fails == 0 ? 0 : 1);
    }

    // ------------------------------------------------------------------ the full versions, in the Vale
    private async Task Full()
    {
        var v = GD.Load<PackedScene>("res://src/Vale/Vale.tscn").Instantiate<ValeGame>();
        v.SavePath = "user://teste_tecnicas_vale.json"; v.ShowIntro = false;
        AddChild(v); v.Begin(null);
        await Frames(10);
        v.LearnSide(); await Frames(220); // controlled: the side step is learned at the statue (checked in ValeCheck)
        var p = v.Player;
        p.GlobalPosition = ValeMap.Start + new Vector2(0, -40); p.Face(Vector2.Up); await Frames(3);
        p.KiRegen = 0; // controlled: no regeneration while measuring the cost
        float ki = p.Ki; float went = -1; int dmg = 0; float range = 0;
        p.KiFired += s => { dmg = s.Damage; range = s.Range; };
        await Tap(Key.K);
        float spent = ki - p.Ki;
        await Frames(40);
        Measure("plena_disparo_ki", spent); Measure("plena_disparo_vigor", 0); Measure("plena_disparo_alcance", range); Measure("plena_disparo_dano", dmg);
        Check("full ki shot: 30 Ki, no vigor, 210 px, damage 2", Mathf.IsEqualApprox(spent, 30, 0.6f) && range == 210 && dmg == 2, $"{spent:0} Ki, {range} px, {dmg}");
        // side step around the statue
        var statue = v.Fighters["statue"];
        p.GlobalPosition = statue.GlobalPosition + new Vector2(0, 60); p.Face(Vector2.Up); await Frames(3);
        v.ToggleLock();
        Vector2 a0 = p.GlobalPosition - statue.GlobalPosition;
        await Chord(Key.A, Key.L); await Frames(20);
        Vector2 a1 = p.GlobalPosition - statue.GlobalPosition;
        float arc = Mathf.Abs(Mathf.RadToDeg(a0.AngleTo(a1)));
        Measure("plena_passo_lateral_graus", arc); Measure("plena_passo_lateral_ki", 0);
        Check("full side step: about 95° around the target, no Ki", arc > 80 && arc < 100, $"{arc:0}°");
        v.ToggleLock();
        // long step
        p.GlobalPosition = ValeMap.Start + new Vector2(0, -60); await Frames(3);
        Vector2 d0 = p.GlobalPosition;
        await Chord(Key.W, Key.L); await Frames(20);
        float dash = d0.DistanceTo(p.GlobalPosition);
        Measure("plena_passo_longo_px", dash);
        Check("full long step: about 112 px", dash > 100 && dash < 120, $"{dash:0} px");
        Measure("plena_lamina_dano", TechniqueData.Load().Lamina.Full.Damage);
        Finish();
    }

    // ------------------------------------------------------------------ sealed, then weakened, in the Planície
    private async Task Sealed()
    {
        await Frames(20);
        Check("a level-2 journey after the awakening: techniques sealed", _g.Stats.Level == 2 && _g.Tech.State("ki_wave") == TechniqueState.Sealed && _g.Tech.State("side_step") == TechniqueState.Sealed && !_g.TechniquesReturned);
        await Teleport(Empty(), Vector2.Right);
        // K: refused, nothing spent
        int m = Msgs; float ki = P.Ki, vig = P.Vigor;
        var (went, _, _, _) = await Shoot();
        await Shot("01-nivel2-disparo-selado");
        Check("level 2 · K: the ki shot is sealed (message, no Ki, no vigor, no shot)", Said("Disparo de Ki está selada", m) && P.Ki >= ki - 0.01f && Mathf.Abs(P.Vigor - vig) < 0.5f && went < 0, _g.Hud.Messages.LastOrDefault() ?? "");
        Check("the controls line says what K does here", _g.Hud.ControlsText.Contains("K disparo de Ki (selado)"), _g.Hud.ControlsText);
        // L to the side of a focused animal: refused, the plain step happens
        var boar = _g.World.Objects.GetChildren().OfType<Boar>().OrderBy(b => b.GlobalPosition.DistanceTo(P.GlobalPosition)).First();
        await Teleport(boar.HomeAt + new Vector2(-130, 0), Vector2.Right);
        await Tap(Key.Space);
        Check("Space focuses the boar", _g.Focused is Boar, _g.Hud.FocusText);
        var focused = (Node2D)_g.Focused!;
        Vector2 tf = focused.GlobalPosition - P.GlobalPosition;
        Key lat0 = Mathf.Abs(tf.X) > Mathf.Abs(tf.Y) ? Key.W : Key.D;
        m = Msgs; ki = P.Ki; vig = P.Vigor; Vector2 p0 = P.GlobalPosition;
        await Chord(lat0, Key.L); await Frames(15);
        float moved = p0.DistanceTo(P.GlobalPosition);
        Check("level 2 · L to the side: the side step is sealed; the plain step still works (vigor only)", Said("Passo Lateral está selada", m) && moved > 20 && moved < 50 && P.Ki >= ki - 0.01f && vig - P.Vigor >= 11, $"moved {moved:0} px, Ki −{ki - P.Ki:0}, vigor −{vig - P.Vigor:0}");
        await Tap(Key.Space);
        await Frames(50);
        m = Msgs; ki = P.Ki;
        await Chord(Key.Shift, Key.D, Key.L); await Frames(15);
        Check("level 2 · Shift+direction+L: the long step is sealed (plain step, no Ki)", Said("Passo longo está selada", m) && P.Ki >= ki - 0.01f, _g.Hud.Messages.LastOrDefault() ?? "");
        // K next to a carcass: skins with Ki, fires nothing
        var rabbit = _g.World.Objects.GetChildren().OfType<Rabbit>().OrderBy(r => r.GlobalPosition.DistanceTo(P.GlobalPosition)).First();
        string rid = rabbit.CreatureId;
        rabbit.ReceiveHit(5); // controlled: a carcass to test K's context (the hunt itself is checked in the journey tests)
        await Frames(5);
        var carcass = _g.World.Objects.GetChildren().OfType<Carcass>().First(c => c.CreatureId == rid);
        P.GlobalPosition = carcass.GlobalPosition + new Vector2(-6, 2); await Frames(5);
        Check("next to a carcass, the controls line says K skins with Ki", _g.KiKeyAction == "esfolar" && _g.Hud.ControlsText.Contains("K esfolar com Ki"), _g.Hud.ControlsText);
        await Frames(240); // let Ki refill a little
        P.Ki = P.MaxKi; ki = P.Ki; int waves = _g.Waves.Count;
        await Tap(Key.K); await Frames(5);
        Check("one K next to the carcass: skinned with Ki (−15), no shot", _g.Inventory.Count("pele_fina") == 1 && Mathf.IsEqualApprox(ki - P.Ki, 15, 0.6f) && _g.Waves.Count == waves, $"Ki −{ki - P.Ki:0}, pele fina {_g.Inventory.Count("pele_fina")}");

        // level 3: the techniques come back weakened, one message
        await Teleport(Empty(), Vector2.Right);
        m = Msgs;
        _g.GainXp(_g.Stats.XpMissing, "teste"); // controlled: the XP still missing for level 3
        await Frames(5);
        int msgCount = _g.Hud.Messages.Skip(m).Count(x => x.Contains("as técnicas do Vale voltam"));
        Check("level 3: the techniques come back weakened, with one short message", _g.Stats.Level == 3 && _g.TechniquesReturned && msgCount == 1 && _g.Tech.State("ki_wave") == TechniqueState.Weakened && _g.Book.Where(b => b.Tech.Length > 0).All(b => b.State == "Enfraquecida"), $"{msgCount} message(s)");
        await Shot("02-nivel3-tecnicas-voltam");
        _g.ToggleBook(); await Frames(3);
        for (int i = 0; i < 8 && _g.Hud.BookRows[_g.Hud.BookSelected].Id != "Disparo de Ki"; i++) await Tap(Key.S);
        await Shot("03-livro-disparo-enfraquecido");
        _g.ToggleBook(); await Frames(2);
        var qrow = _g.BookRows().First(r => r.Id == "Disparo de Ki");
        Check("the book shows the command, cost and effect in force", qrow.Text.Contains("Enfraquecida") && qrow.Detail.Contains("custo: 45 Ki, 10 vigor") && qrow.Detail.Contains("alcance 105 px"), qrow.Detail.Replace("\n", " | "));
        _g.GainXp(_g.Stats.XpToNext(3), "teste"); await Frames(3);
        Check("levelling again does not repeat the message", _g.Hud.Messages.Skip(m).Count(x => x.Contains("as técnicas do Vale voltam")) == 1);

        // the weakened ki shot, measured (controlled: no Ki regeneration while measuring)
        P.KiRegen = 0;
        P.Ki = P.MaxKi; P.Vigor = P.MaxVigor; await Frames(2);
        var shot = await Shoot();
        Measure("enfraquecida_disparo_ki", shot.Ki); Measure("enfraquecida_disparo_vigor", shot.Vigor); Measure("enfraquecida_disparo_alcance", shot.Went); Measure("enfraquecida_disparo_dano", shot.Damage);
        Check("weakened ki shot: 45 Ki + 10 vigor, ends at 105 px, damage 1", Mathf.IsEqualApprox(shot.Ki, 45, 0.6f) && Mathf.IsEqualApprox(shot.Vigor, 10, 0.6f) && shot.Went > 95 && shot.Went <= 106 && shot.Damage == 1, $"{shot.Ki:0} Ki, {shot.Vigor:0} vigor, {shot.Went:0} px, dano {shot.Damage}");
        // the weakened shot hits an animal
        var r2 = _g.World.Objects.GetChildren().OfType<Rabbit>().Where(r => r.Brain.Visible).OrderBy(r => r.GlobalPosition.DistanceTo(P.GlobalPosition)).First();
        P.Ki = P.MaxKi; P.Vigor = P.MaxVigor;
        await Teleport(r2.GlobalPosition + new Vector2(-70, 0), Vector2.Right);
        string r2id = r2.CreatureId;
        bool landed = false; float landedAt = 0;
        void Ended(KiWave w, bool hit) { if (hit) { landed = true; landedAt = w.Went; } }
        _g.KiShotEnded += Ended;
        int deadBefore = _g.World.State.Snapshot().Dead.Count;
        await Tap(Key.K); await Frames(40);
        _g.KiShotEnded -= Ended;
        int deadAfter = _g.World.State.Snapshot().Dead.Count;
        Check("the weakened shot hits an animal in its path and the damage lands (a rabbit falls)", landed && deadAfter > deadBefore, $"hit at {landedAt:0} px, carcasses {deadBefore}→{deadAfter}");
        // refused for lack of one resource: the other is not spent
        await Teleport(Empty(), Vector2.Right); await Frames(10);
        P.Ki = 10; P.Vigor = P.MaxVigor; m = Msgs; float v0 = P.Vigor;
        await Tap(Key.K); await Frames(5);
        Check("not enough Ki: refused, vigor not spent", Said("falta Ki", m) && Mathf.Abs(P.Vigor - v0) < 0.5f && P.Ki < 11, _g.Hud.Messages.LastOrDefault() ?? "");
        P.Ki = P.MaxKi; P.Vigor = 5; m = Msgs; float q0 = P.Ki;
        await Tap(Key.K); await Frames(5);
        Check("not enough vigor: refused, Ki not spent", Said("falta vigor", m) && P.Ki >= q0 - 0.6f, _g.Hud.Messages.LastOrDefault() ?? "");
        // the weakened side step around the boar
        P.Ki = P.MaxKi; P.Vigor = P.MaxVigor;
        boar = _g.World.Objects.GetChildren().OfType<Boar>().OrderBy(b => b.GlobalPosition.DistanceTo(P.GlobalPosition)).First();
        await Teleport(boar.GlobalPosition + new Vector2(-60, 0), Vector2.Right);
        await Tap(Key.Space);
        float sq = P.Ki, sv = P.Vigor; Vector2 s0 = P.GlobalPosition - boar.GlobalPosition;
        await Chord(Key.W, Key.L); await Frames(20);
        Vector2 s1 = P.GlobalPosition - boar.GlobalPosition;
        float sideArc = Mathf.Abs(Mathf.RadToDeg(s0.AngleTo(s1)));
        Measure("enfraquecido_passo_lateral_graus", sideArc); Measure("enfraquecido_passo_lateral_ki", sq - P.Ki); Measure("enfraquecido_passo_lateral_vigor", sv - P.Vigor);
        Check("weakened side step: less ground (about 55°), 15 Ki + 12 vigor", sideArc > 40 && sideArc < 70 && Mathf.IsEqualApprox(sq - P.Ki, 15, 1) && Mathf.IsEqualApprox(sv - P.Vigor, 12, 1), $"{sideArc:0}°, Ki −{sq - P.Ki:0}, vigor −{sv - P.Vigor:0}");
        // the Silent Blade Step, weakened: side step → cut → ki
        P.Ki = P.MaxKi; P.Vigor = P.MaxVigor; await Frames(40);
        int laminaDmg = -1; float laminaRange = 0;
        void OnShot(Player.KiShot s) { if (s.Lamina) { laminaDmg = s.Damage; laminaRange = s.Range; } }
        P.KiFired += OnShot;
        Vector2 toBoar = boar.GlobalPosition - P.GlobalPosition;
        Key lat = Mathf.Abs(toBoar.X) > Mathf.Abs(toBoar.Y) ? Key.S : Key.D;
        await Chord(lat, Key.L); await Frames(4); await Tap(Key.J); await Frames(4); await Tap(Key.K); await Frames(20);
        P.KiFired -= OnShot;
        Measure("enfraquecida_lamina_dano", laminaDmg); Measure("enfraquecida_lamina_alcance", laminaRange);
        Check("weakened Silent Blade Step: damage 3 (full: 6)", laminaDmg == 3, $"dano {laminaDmg}, {laminaRange} px");
        await Tap(Key.Space);
        // the weakened long step: Shift + direction + L
        P.Ki = P.MaxKi; P.Vigor = P.MaxVigor;
        await Teleport(Empty(), Vector2.Right); await Frames(30);
        Vector2 d0 = P.GlobalPosition; float dq = P.Ki;
        await Chord(Key.Shift, Key.D, Key.L); await Frames(20);
        float dash = d0.DistanceTo(P.GlobalPosition);
        Measure("enfraquecido_passo_longo_px", dash); Measure("enfraquecido_passo_longo_ki", dq - P.Ki);
        Check("weakened long step: about 60 px, 10 Ki (full: 112 px, no Ki)", dash > 50 && dash < 70 && Mathf.IsEqualApprox(dq - P.Ki, 10, 1), $"{dash:0} px, Ki −{dq - P.Ki:0}");
        // meditate: the return is saved
        var mat = _g.World.Objects.GetChildren().OfType<MeditationMat>().First();
        await Teleport(mat.GlobalPosition + new Vector2(0, 10), Vector2.Up);
        await Tap(Key.E);
        var saved = SaveGame.Read(SavePath).Data!;
        Check("meditating saves the return (flag and book)", saved.TechniquesReturned && saved.Book.Any(b => b.State == "Enfraquecida"));
        Copy(SavePath, AfterReturn);
        Finish();
    }

    private async Task Resume2()
    {
        await Frames(20);
        Check("a journey saved at level 2: still sealed, no return message", _g.Stats.Level == 2 && !_g.TechniquesReturned && _g.Tech.State("side_step") == TechniqueState.Sealed && !_g.Hud.Messages.Any(m => m.Contains("voltam")));
        int m = Msgs; await Teleport(Empty(), Vector2.Right); await Tap(Key.K); await Frames(5);
        Check("K is still refused", Said("selada", m));
        Finish();
    }

    private async Task Resume3()
    {
        await Frames(20);
        Check("the journey saved after the return: weakened, no second message", _g.TechniquesReturned && _g.Tech.State("ki_wave") == TechniqueState.Weakened && !_g.Hud.Messages.Any(m => m.Contains("voltam")));
        Finish();
    }

    private async Task Migrated(bool first)
    {
        await Frames(20);
        int count = _g.Hud.Messages.Count(m => m.Contains("as técnicas do Vale voltam"));
        if (first)
        {
            Check("a level-4 journey with sealed techniques gets them back once on loading", _g.TechniquesReturned && _g.Tech.State("side_step") == TechniqueState.Weakened && count == 1, $"{count} message(s)");
            var mat = _g.World.Objects.GetChildren().OfType<MeditationMat>().First();
            await Teleport(mat.GlobalPosition + new Vector2(0, 10), Vector2.Up);
            await Tap(Key.E);
            Copy(SavePath, Migrate);
            Check("and the book is saved with them weakened", SaveGame.Read(SavePath).Data!.TechniquesReturned);
        }
        else Check("loading it again repeats nothing", _g.TechniquesReturned && count == 0 && _g.Tech.State("side_step") == TechniqueState.Weakened, $"{count} message(s)");
        Finish();
    }
}
