using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Campaign;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Combat;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Game;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;
using SpiritOfMurin.Vale;

namespace SpiritOfMurin.Tests;

/// <summary>
/// Pedido 14, entrega 14.0 — FOR, CON, the critical, the enemy's Vida in numbers and the damage numbers of the
/// assist mode, through the real scenes and keys. Phases, each its own run:
/// <list type="bullet">
/// <item><c>--phase=plains</c>: a Planície journey with FOR 5 and CON 5 (written by the test). The sheet lists
/// FOR first; focusing a boar shows "Vida/máx" inside its bar; a knife cut on the rooting boar takes 1 × 1,30
/// (FOR) and the bar's numbers drop by the same; with the critical roll fixed at 0 the hit is ×1,5 and marked
/// "!"; a hit taken loses 15% to CON and shows "-N"; with the numbers off nothing is shown; the assist menu
/// has the new line; the journey saves and reads FOR back.</item>
/// <item><c>--phase=vale</c>: the prologue — locking the bridge disciple shows "N/N" in its bar; a cut that
/// lands shows its number (the prologue has no sheet: no FOR, no critical).</item>
/// <item><c>--phase=oldsave</c>: Qi → Ki — a real journey written by the game before the rename
/// (<c>tests/fixtures/saves/jornada_antes_do_ki.json</c>, keys "qi") continues with its Ki, its KI points and its
/// Livro under the new names; preferences from before with the Qi control on another button keep it as Ki.</item>
/// <item><c>--phase=visual</c> (pedido 14.1): in the Vale, the Reforço de Ki puts a blue glow around the sword
/// (seen from the side) and a reinforced cut leaves a blue trail; a plain cut's trail is the larger jade crescent.
/// Pictures with <c>--shots=</c>.</item>
/// <item><c>--phase=mapa</c> (pedido 14.2): a new Planície journey — the minimap and the map (M) mark each kind of
/// place with its own picture (campfire, mat, burrow, herb, coins, marker), and the map's legend shows the same pictures.</item>
/// </list>
/// Files: <c>user://teste_atr.json</c>; preferences <c>user://teste_atr_prefs.json</c> (pass <c>--prefs=</c>).
/// Teleports and the boar's position are named in the checks.
/// </summary>
public partial class AtributosCheck : Node
{
    private const string SavePath = "user://teste_atr.json";
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private string _phase = "plains";
    private string? _shots;
    private PlainsGame _pg = null!;
    private CampaignRoot _c = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        foreach (string a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--phase=")) _phase = a["--phase=".Length..];
            if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..];
        }
        GD.Print($"window on screen {DisplayServer.WindowGetCurrentScreen()} of {DisplayServer.GetScreenCount()} ({DisplayServer.GetName()})");
        SaveGame.Erase(SavePath);
        if (Godot.FileAccess.FileExists(Prefs.Path)) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(Prefs.Path));
        Bindings.Init(); Assist.Reload(); Aim.Reload(); Aim.UseForSession(false);
        _ = _phase switch { "vale" => ValePhase(), "oldsave" => OldSavePhase(), "visual" => VisualPhase(), "mapa" => MapPhase(), _ => PlainsPhase() };
    }

    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private async Task Flush() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static void Press(Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });
    private async Task Tap(Key k) { Press(k, true); await Flush(); await Frames(2); Press(k, false); await Flush(); await Frames(2); }
    private void Finish() { int f = _results.Count(r => !r.Ok); GD.Print($"atributos {_phase}: {_results.Count - f}/{_results.Count} OK"); GetTree().Quit(f == 0 ? 0 : 1); }

    private async Task Shot(string name)
    {
        if (_shots == null || DisplayServer.GetName() == "headless") return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        DirAccess.MakeDirRecursiveAbsolute(_shots);
        GetViewport().GetTexture().GetImage().SavePng($"{_shots}/{name}.png"); // the game's own picture, never the desktop
    }

    // ------------------------------------------------------------------ the Planície
    private async Task PlainsPhase()
    {
        SaveGame.Write(SavePath, new SaveData
        {
            Region = "planicie", Phase = "planicie", Seed = 20261126, Spot = new[] { 16f, 16f }, LossApplied = true,
            Inventory = { new SavedItem { Id = "faca_pedra", Count = 1 } }, Equipped = "knife", Health = 10, Vigor = 50, Ki = 100, Level = 6, Points = 2,
            Attributes = new() { ["for"] = 5, ["con"] = 5 },
        });
        _pg = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _pg.SavePath = SavePath; _pg.ShowIntro = false; _pg.Start = PlainsGame.StartMode.Continue;
        AddChild(_pg);
        await Frames(30);
        var p = _pg.Player;
        Check("FOR 5 read from the journey: melee ×1,30", Mathf.IsEqualApprox(p.MeleeMultiplier, 1.30f), $"{p.MeleeMultiplier}");
        Check("CON 5 read from the journey: 85% of the damage taken", Mathf.IsEqualApprox(p.DamageTaken, 0.85f), $"{p.DamageTaken}");
        Check("SOR 0: 5% critical", Mathf.IsEqualApprox(p.CritChance, 0.05f), $"{p.CritChance}");

        // the sheet: FOR first, with its preview
        await Tap(Key.C); await Frames(4);
        var rows = _pg.Hud.SheetRows;
        Check("the sheet lists 7 attributes, FOR first", rows.Count == 7 && rows[0].Id == "for" && rows[0].Text.StartsWith("FOR  5"), rows.Count > 0 ? rows[0].Text : "");
        Check("the sheet's preview of FOR", _pg.Hud.SheetDetail.Contains("Dano corpo a corpo: 130% → 136%"), string.Join(" | ", _pg.Hud.SheetDetail.Split('\n').Take(2)));
        await Shot("01-ficha-com-for");
        await Tap(Key.C); await Frames(4);

        // a boar, rooting (open), focused
        var boar = _pg.World.Objects.GetChildren().OfType<Boar>().OrderBy(b => b.GlobalPosition.DistanceTo(p.GlobalPosition)).First();
        foreach (var r in _pg.World.Objects.GetChildren().OfType<Rabbit>().ToList()) _pg.World.Remove(r); // controlled: rabbits out of the blade's way
        p.GlobalPosition = boar.GlobalPosition + new Vector2(-16, 0); p.Face(Vector2.Right);
        p.GetNode<Camera2D>("Camera").ResetSmoothing(); await Frames(3);
        _pg.ToggleFocus(); await Frames(3);
        string max = DamageRules.Format(boar.HealthMax);
        Check("focused boar: Vida in numbers inside the bar", _pg.Focused == boar && _pg.Hud.FocusNumbers == $"{max}/{max}", $"'{_pg.Hud.FocusNumbers}'");

        // the numbers on, a plain cut (roll 0,99: no critical)
        Assist.Numbers = true;
        p.CritRoll = () => 0.99f;
        await Open(boar, p);
        float h0 = boar.HealthExact; int shown = _pg.Numbers.Shown.Count;
        await Tap(Key.J); await Frames(14);
        float took = h0 - boar.HealthExact;
        Check("knife cut with FOR 5: takes 1 × 1,30 (controlled: next to the boar, its guard opened)", Mathf.IsEqualApprox(took, 1.3f, 0.001f), $"tirou {took:0.###} (estado {boar.Brain.State})");
        Check("the damage number shows 1,3", _pg.Numbers.Shown.Skip(shown).Contains("1,3"), string.Join(" ", _pg.Numbers.Shown.Skip(shown)));
        await Frames(2);
        Check("the bar's numbers dropped by the same", _pg.Hud.FocusNumbers == $"{DamageRules.Format(boar.HealthExact)}/{max}", $"'{_pg.Hud.FocusNumbers}'");
        await Shot("02-barra-e-numero-de-dano");

        // the critical: roll 0
        p.CritRoll = () => 0f;
        await Frames(30);
        await Open(boar, p);
        h0 = boar.HealthExact; shown = _pg.Numbers.Shown.Count;
        if (boar.Brain.Vulnerable)
        {
            await Tap(Key.J); await Frames(6);
            await Shot("03-critico");
            await Frames(8);
            took = h0 - boar.HealthExact;
            Check("a critical cut: 1 × 1,30 × 1,5 = 1,95, marked '!'", Mathf.IsEqualApprox(took, 1.95f, 0.001f) && _pg.Numbers.Shown.Skip(shown).Any(s => s.StartsWith("2!") || s.StartsWith("1,9!") || s.StartsWith("2,0!")), $"tirou {took:0.###}; {string.Join(" ", _pg.Numbers.Shown.Skip(shown))}");
        }
        else Check("a critical cut (the boar was open)", false, $"estado {boar.Brain.State}");

        // a hit taken: 4 of damage, CON 5 → 3,4
        await Frames(60); // past the hurt invulnerability of anything before
        float life = p.Health; shown = _pg.Numbers.Shown.Count;
        p.TakeDamage(4, p.GlobalPosition + new Vector2(20, 0)); // controlled: a hit of 4
        await Frames(4);
        Check("a hit of 4 with CON 5 takes 3,4", Mathf.IsEqualApprox(life - p.Health, 3.4f, 0.001f), $"{life - p.Health:0.###}");
        Check("the damage taken shows -3,4", _pg.Numbers.Shown.Skip(shown).Contains("-3,4"), string.Join(" ", _pg.Numbers.Shown.Skip(shown)));
        await Shot("04-dano-recebido");

        // the numbers off: nothing shown
        Assist.Numbers = false;
        await Frames(60);
        shown = _pg.Numbers.Shown.Count;
        p.TakeDamage(2, p.GlobalPosition + new Vector2(20, 0)); await Frames(4);
        Check("numbers off: no number shown", _pg.Numbers.Shown.Count == shown);

        // the assist menu has the new line
        _pg.Panels.OpenPause(); _pg.Panels.Choose("assist"); await Frames(3);
        Check("Esc → Modo de assistência has 'Mostrar números de dano'", _pg.Hud.PauseRows.Any(r => r.Text.StartsWith("Mostrar números de dano")), string.Join(" | ", _pg.Hud.PauseRows.Select(r => r.Text)));
        _pg.Panels.Choose("assist_numbers"); await Frames(3);
        Check("the line turns the numbers on (and saves it)", Assist.Numbers && Prefs.Load().AssistNumbers);
        await Shot("05-menu-assistencia");
        _pg.Panels.Choose("assist_back"); _pg.Panels.Choose("back"); await Frames(3);

        // FOR survives a save
        bool saved = SaveGame.Write(SavePath, _pg.Snapshot());
        var back = SaveGame.Read(SavePath).Data;
        Check("FOR is saved with the journey", saved && back != null && back.Attributes.TryGetValue("for", out int f) && f == 5, back == null ? "sem save" : string.Join(",", back.Attributes.Select(kv => $"{kv.Key}={kv.Value}")));
        Finish();
    }

    // puts the swordsman next to the boar, facing it, and makes sure the boar is open: when it is warning or
    // charging, its guard is broken (as a guard-breaking hit would) — controlled, so the cut's damage lands
    private async Task Open(Boar boar, Player p)
    {
        p.GlobalPosition = boar.GlobalPosition + new Vector2(-16, 0); p.Face(Vector2.Right); await Frames(2);
        if (!boar.Brain.Vulnerable) boar.BreakGuard(2.5f);
        p.GlobalPosition = boar.GlobalPosition + new Vector2(-16, 0); await Frames(1);
    }

    // ------------------------------------------------------------------ Qi → Ki, files written before
    private async Task OldSavePhase()
    {
        string old = Godot.FileAccess.GetFileAsString("res://tests/fixtures/saves/jornada_antes_do_ki.json");
        using (var f = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write)) f.StoreString(old);
        // preferences from before the rename: the Qi control moved to the mouse's side button (as a player could)
        using (var f = Godot.FileAccess.Open(Prefs.Path, Godot.FileAccess.ModeFlags.Write))
            f.StoreString("{\"assist_areas\":false,\"bindings\":{\"qi\":[\"k:75\",\"m:8\"]},\"quick_slots\":{}}");
        Bindings.Init(); Assist.Reload();
        Check("the old file really has the old spelling", old.Contains("\"qi\": 100") && old.Contains("qi_wave"));
        Check("the Qi control of old preferences is the Ki control, side button kept", Bindings.Current("ki")[1] == "m:8", string.Join(",", Bindings.Current("ki")));
        _pg = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _pg.SavePath = SavePath; _pg.ShowIntro = false; _pg.Start = PlainsGame.StartMode.Continue;
        AddChild(_pg);
        await Frames(30);
        Check("the old journey continues: level 4 and its knife", _pg.Stats.Level == 4 && _pg.Inventory.Count("faca_pedra") == 1, $"Nv {_pg.Stats.Level}");
        Check("its energy came back as Ki (100)", Mathf.IsEqualApprox(_pg.Player.Ki, 100), $"{_pg.Player.Ki}");
        Check("the Livro has 'Disparo de Ki' (ki_wave), still weakened", _pg.Book.Any(b => b.Name == "Disparo de Ki" && b.Tech == "ki_wave" && b.State == "Enfraquecida"), string.Join(", ", _pg.Book.Select(b => b.Name)));
        Check("the technique state reads under the new id", _pg.Tech.State("ki_wave") == TechniqueState.Weakened, $"{_pg.Tech.State("ki_wave")}");
        Check("no 'Qi' left on screen", !_pg.Hud.ControlsText.Contains("Qi") && !_pg.Hud.Messages.Any(m => m.Contains("Qi")), _pg.Hud.ControlsText);
        bool saved = SaveGame.Write(SavePath, _pg.Snapshot());
        string now = Godot.FileAccess.GetFileAsString(SavePath);
        Check("saving again writes only the new spelling", saved && !KiMigration.IsOld(now) && now.Contains("\"ki\": 100"));
        Finish();
    }

    // ------------------------------------------------------------------ 14.1: the look of the Reforço and the cut
    private async Task VisualPhase()
    {
        _c = GD.Load<PackedScene>("res://src/Campaign/Campaign.tscn").Instantiate<CampaignRoot>();
        _c.SavePath = SavePath; _c.ShowIntro = false; _c.StartWith = "new";
        AddChild(_c);
        await Frames(40);
        var v = _c.Vale!; var p = v.Player;
        p.KiRegen = 0;
        p.GlobalPosition = ValeMap.Start + new Vector2(0, -60); p.Face(Vector2.Right); // named: the trail by the start
        p.GetNode<Camera2D>("Camera").ResetSmoothing(); await Frames(6);
        // a plain cut: the larger jade crescent
        await Tap(Key.J); await Frames(3);
        await Shot("07-corte-normal-maior");
        await Frames(40);
        // the Reforço: Shift + J held
        p.Ki = 100;
        Press(Key.Shift, true); await Flush(); await Frames(1);
        Press(Key.J, true); await Flush(); await Frames(30);
        Press(Key.J, false); await Flush(); await Frames(1); Press(Key.Shift, false); await Flush(); await Frames(10);
        p.Face(Vector2.Right); await Frames(4);
        Check("the Reforço is on (3 reinforced strikes)", p.ReinforceCharges == 3, $"{p.ReinforceCharges}");
        await Shot("08-reforco-brilho-azul");
        await Tap(Key.J); await Frames(3);
        await Shot("09-corte-reforcado-azul");
        Finish();
    }

    // ------------------------------------------------------------------ 14.2: pictures on the map
    private async Task MapPhase()
    {
        _pg = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _pg.SavePath = SavePath; _pg.ShowIntro = false; _pg.Start = PlainsGame.StartMode.New;
        AddChild(_pg);
        await Frames(40);
        var icons = _pg.LegendIcons;
        Check("the Planície's legend lists the pictures", icons.Contains(MapIcon.Campfire) && icons.Contains(MapIcon.Mat) && icons.Contains(MapIcon.Burrow) && icons.Contains(MapIcon.Herb) && icons.Contains(MapIcon.Coins));
        Check("every picture has a legend name", icons.All(i => MapIcons.Name(i).Length > 0));
        _pg.MapMarkers.Add(_pg.Player.GlobalPosition + new Vector2(120, 40)); // a marker to show its picture
        await Frames(4);
        await Shot("10-minimapa-com-icones");
        _pg.Panels.OpenMap(); await Frames(6);
        await Shot("11-mapa-com-icones-e-legenda");
        _pg.Panels.Choose("back"); await Frames(2);
        Finish();
    }

    // ------------------------------------------------------------------ the Vale
    private async Task ValePhase()
    {
        _c = GD.Load<PackedScene>("res://src/Campaign/Campaign.tscn").Instantiate<CampaignRoot>();
        _c.SavePath = SavePath; _c.ShowIntro = false; _c.StartWith = "new";
        AddChild(_c);
        await Frames(40);
        var v = _c.Vale!; var p = v.Player;
        var dis = v.Fighters.Values.Where(f => f.Type == ValeFighter.Kind.Disciple).OrderBy(f => f.GlobalPosition.DistanceTo(p.GlobalPosition)).First();
        Assist.Numbers = true;
        p.GlobalPosition = dis.GlobalPosition + new Vector2(-60, 0); p.Face(Vector2.Right); // named: near the bridge disciple
        p.GetNode<Camera2D>("Camera").ResetSmoothing(); await Frames(6);
        v.ToggleLock(); await Frames(4);
        string max = DamageRules.Format(dis.Max);
        Check("locked disciple: Vida in numbers inside the bar", v.Lock == dis && v.Hud.FocusNumbers == $"{max}/{max}", $"'{v.Hud.FocusNumbers}'");
        Check("the prologue has no sheet: no FOR, no critical, all damage taken", p.MeleeMultiplier == 1 && p.CritChance == 0 && p.DamageTaken == 1);
        // a hit that lands (controlled: through the region's own strike, from the flank)
        float hp = dis.Hp; int shown = v.Numbers.Shown.Count;
        string r = v.StrikeFighter(dis, dis.GlobalPosition + new Vector2(0, 30), 2, lamina: true, stagger: 0.5f, heavy: false, melee: true);
        await Frames(4);
        Check("a strike that lands shows its number (2)", r is "hit" or "killed" && v.Numbers.Shown.Skip(shown).Contains("2"), $"{r}; {string.Join(" ", v.Numbers.Shown.Skip(shown))}");
        await Frames(2);
        Check("the bar's numbers dropped", v.Hud.FocusNumbers == $"{DamageRules.Format(dis.Hp)}/{max}" && dis.Hp < hp, $"'{v.Hud.FocusNumbers}'");
        await Shot("06-vale-barra-com-numeros");
        Finish();
    }
}
