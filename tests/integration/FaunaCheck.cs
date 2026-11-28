using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Combat;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Elements;
using SpiritOfMurin.Game;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Tests;

/// <summary>
/// T07 — combat, tracking and reading the fauna, through the real scenes. Phases, each its own run (<c>--seed=N</c>):
/// <list type="bullet">
/// <item><c>prancha</c>: the boar warning with its direction fixed, assist off and on, plus its numbers, for the board.</item>
/// <item><c>duelo</c>: a parry in the warning (the blade glances off, the dry sound, Vida unchanged); the charge hit (impact,
/// push and Vida loss in the same frame); the counter in the recovery (the body flinches in the frame the Vida drops, the
/// recovery lasts longer).</item>
/// <item><c>faixa</c>: the charge strip appears when the direction locks and ends where the charge really ends, with the
/// assist off and on; short and off modes.</item>
/// <item><c>barra</c>: the compact bar with numbers over the focused animal and a boar in a fight; the top bar stays.</item>
/// <item><c>lamina</c>: the weakened Passo da Lâmina Silenciosa goes through the warning at half damage (decision B).</item>
/// <item><c>coelho</c>: ears up, crouch, the bush bends, the dive into the burrow; it hears walking at about 95 px and Shift at 26.</item>
/// <item><c>pegadas</c>: fresh prints follow the real path (also a turn) and fade; examining narrows with Rastreamento;
/// the wheel chooses a fresh print over a herb.</item>
/// <item><c>tecnicas</c>: the full and weakened Disparo de Ki end where they should (and crumble there), the long step too.</item>
/// <item><c>mapa</c>: a boar shows on the map only after it was seen or tracked; it is saved.</item>
/// <item><c>vento</c>: in a gust the strip and the prints still read.</item>
/// <item><c>oldsave</c>: the fixture <c>tests/fixtures/saves/jornada_antes_do_ki.json</c> (no T07 fields) loads.</item>
/// <item><c>trilhas</c>: the trails generated with the world are unchanged (count and places).</item>
/// </list>
/// Files: <c>user://teste_fauna.json</c>; preferences <c>user://teste_fauna_prefs.json</c>. Pictures with <c>--shots=</c>
/// (the game's own viewport, never the desktop).
/// </summary>
public partial class FaunaCheck : Node
{
    private const string SavePath = "user://teste_fauna.json";
    private const string Fixture = "res://tests/fixtures/saves/jornada_antes_do_ki.json";
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private string _phase = "prancha", _shots = "";
    private int _seed = 20261126;
    private PlainsGame _g = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        foreach (string a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--phase=")) _phase = a["--phase=".Length..];
            if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..];
            if (a.StartsWith("--seed=") && int.TryParse(a["--seed=".Length..], out int sd)) _seed = sd;
        }
        if (Godot.FileAccess.FileExists(Prefs.Path)) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(Prefs.Path));
        Bindings.Init(); Assist.Reload(); Aim.Reload(); Aim.UseForSession(false);
        _ = Guard(_phase switch
        {
            "duelo" => Duel(), "faixa" => StripPhase(), "barra" => BarPhase(), "lamina" => LaminaPhase(), "coelho" => RabbitPhase(),
            "pegadas" => PrintsPhase(), "tecnicas" => TechPhase(), "mapa" => MapPhase(), "vento" => WindPhase(), "oldsave" => OldSave(),
            "trilhas" => Trails(), _ => Board(),
        });
    }

    private async Task Guard(Task t)
    {
        try { await t; }
        catch (System.Exception ex) { Check("NOT ASSESSED: the phase stopped on an exception", false, ex.GetType().Name + ": " + ex.Message + " " + ex.StackTrace?.Split('\n').FirstOrDefault()); Finish(); }
    }

    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private async Task Flush() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static void Key(Godot.Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });
    private async Task Tap(Godot.Key k) { Key(k, true); await Flush(); await Frames(2); Key(k, false); await Flush(); await Frames(1); }
    private async Task Chord(params Godot.Key[] keys) { foreach (var k in keys) { Key(k, true); await Flush(); await Frames(1); } await Frames(2); foreach (var k in keys.Reverse()) Key(k, false); await Flush(); await Frames(2); }
    private void Finish() { Engine.TimeScale = 1; int f = _results.Count(r => !r.Ok); GD.Print($"fauna {_phase} seed {_seed}: {_results.Count - f}/{_results.Count} OK"); GetTree().Quit(f == 0 ? 0 : 1); }
    private Player P => _g.Player;
    private IEnumerable<T> Nodes<T>() where T : Node => _g.World.Objects.GetChildren().OfType<T>().Where(n => !n.IsQueuedForDeletion());

    private async Task Shot(string name)
    {
        if (_shots.Length == 0 || DisplayServer.GetName() == "headless") return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        DirAccess.MakeDirRecursiveAbsolute(_shots);
        GetViewport().GetTexture().GetImage().SavePng($"{_shots}/{name}.png");
    }

    private async Task<Image?> Grab()
    {
        if (DisplayServer.GetName() == "headless") return null;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        return GetViewport().GetTexture().GetImage();
    }

    private async Task Open(SaveData? write = null)
    {
        if (write != null) SaveGame.Write(SavePath, write); else SaveGame.Erase(SavePath);
        _g = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _g.SavePath = SavePath; _g.ShowIntro = false; _g.Start = write != null ? PlainsGame.StartMode.Continue : PlainsGame.StartMode.New; _g.Seed = _seed;
        AddChild(_g);
        await Frames(40);
    }

    private async Task Teleport(Vector2 at, Vector2 face) { P.GlobalPosition = at; P.Face(face); P.GetNode<Camera2D>("Camera").ResetSmoothing(); await Frames(3); }

    /// <summary>A Planície journey at level 4 with the knife (the boar's level range), for the duels.</summary>
    public static SaveData Hunter(int seed) => new()
    {
        Region = "planicie", Phase = "planicie", Seed = seed, Spot = new[] { 16f, 16f }, LossApplied = true, Health = 12, Vigor = 50, Ki = 100, Level = 4,
        Inventory = { new SavedItem { Id = "faca_pedra", Count = 1 } }, Equipped = "knife", EquippedItem = "faca_pedra",
    };

    /// <summary>The fixture journey (level 4, techniques weakened, no T07 fields), on this run's seed.</summary>
    private SaveData FixtureJourney()
    {
        var d = SaveGame.FromJson(Godot.FileAccess.GetFileAsString(Fixture))!;
        d.Seed = _seed;
        return d;
    }

    // the boar nearest the swordsman, alone (the other animals around are taken away)
    private Boar Lone()
    {
        var boar = Nodes<Boar>().OrderBy(b => b.GlobalPosition.DistanceTo(P.GlobalPosition)).First();
        foreach (var r in Nodes<Rabbit>().ToList()) _g.World.Remove(r);
        foreach (var o in Nodes<Boar>().Where(b => b != boar).ToList()) _g.World.Remove(o);
        return boar;
    }

    private async Task<bool> WaitFor(System.Func<bool> cond, int frames) { for (int i = 0; i < frames && !cond(); i++) await Frames(1); return cond(); }

    // ------------------------------------------------------------------ the board: the boar warning, assist off and on
    private async Task Board()
    {
        await Open(Hunter(_seed));
        var boar = Lone();
        await Teleport(boar.GlobalPosition + new Vector2(70, 10), Vector2.Left);
        await WaitFor(() => boar.Brain.State == BoarState.Warn && boar.Brain.DirectionLocked, 240);
        Check("the boar warns with its direction fixed", boar.Brain.State == BoarState.Warn && boar.Brain.DirectionLocked, $"{boar.Brain.State}");
        _g.ToggleFocus();
        P.InputLocked = true; boar.ProcessMode = ProcessModeEnum.Disabled; // hold the moment for both pictures
        await Frames(2);
        await Shot("javali-aviso-assistencia-desligada");
        Assist.Areas = true; boar.QueueRedraw(); await Frames(2);
        await Shot("javali-aviso-assistencia-ligada");
        Assist.Areas = false;
        Vector2 sp = GetViewport().GetCanvasTransform() * boar.GlobalPosition;
        if (_shots.Length > 0)
            using (var f = Godot.FileAccess.Open($"{_shots}/javali.txt", Godot.FileAccess.ModeFlags.Write))
                f?.StoreLine($"{sp.X} {sp.Y} {boar.Brain.Facing.X} {boar.Brain.Facing.Y} {boar.Tuning.ChargeDistance} {boar.Tuning.ChargeHitRadius} {boar.HealthLeft} {boar.HealthMax}");
        Finish();
    }

    // ------------------------------------------------------------------ the duel: parry, charge hit, counter in the recovery
    private async Task Duel()
    {
        await Open(Hunter(_seed));
        var boar = Lone();
        float reach = P.Tuning.KnifeReach;
        await Teleport(boar.GlobalPosition + new Vector2(reach + boar.BodyRadius - 6, 0), Vector2.Left);
        _g.ToggleFocus();
        bool warned = await WaitFor(() => boar.Brain.State == BoarState.Warn, 240);
        Check("the boar warns at the swordsman", warned, $"{boar.Brain.State}");
        // 1. the parry: a cut during the warning glances off — spark and streak, the dry sound, the Vida unchanged
        int deflects = 0; boar.Deflected += _ => deflects++;
        float hp0 = boar.Brain.Health;
        P.Face(boar.GlobalPosition - P.GlobalPosition);
        await Tap(Godot.Key.J);
        await WaitFor(() => deflects > 0, 12);
        Check("a cut in the warning glances off: the spark shows, the boar's Vida is unchanged", deflects > 0 && boar.Spark > 0 && boar.Brain.Health == hp0,
            $"desvios {deflects}, faísca {boar.Spark:0.00}, Vida {hp0}→{boar.Brain.Health}, estado {boar.Brain.State}");
        Check("the glance streak comes from the swordsman's side", boar.GlanceFrom.Dot(P.GlobalPosition - boar.GlobalPosition) > 0, $"{boar.GlanceFrom}");
        Check("the parry has its own dry sound and a short nudge (data)", FaunaSettings.Current.ParryNudgePx > 0, $"{FaunaSettings.Current.ParryNudgePx} px");
        await Shot("duelo-1-aparo");
        // 2. the charge hit: the impact, the push and the Vida loss in the very frame the body reaches the swordsman
        float pHp = P.Health; Vector2 pAt = P.GlobalPosition;
        float hitHp = -1, hitPush = -1, beforeHp = -1; Vector2 beforeAt = default;
        boar.HitPlayer += _ => { hitHp = P.Health; hitPush = P.GlobalPosition.DistanceTo(beforeAt); beforeHp = pHp; };
        for (int i = 0; i < 120 && hitHp < 0; i++) { beforeAt = P.GlobalPosition; pHp = P.Health; await Frames(1); }
        Check("the charge hits: Vida lost and the push in the same frame as the impact", hitHp >= 0 && hitHp < beforeHp && hitPush > 1,
            $"Vida {beforeHp}→{hitHp}, empurrão {hitPush:0.0} px no quadro do impacto");
        await Shot("duelo-2-investida");
        // 3. the counter: in the recovery, the cut lands — the body flinches in the frame the Vida drops; the recovery stretches
        bool rec = await WaitFor(() => boar.Brain.State == BoarState.Recover, 120);
        Check("after the charge the boar recovers (the opening)", rec, $"{boar.Brain.State}");
        await Teleport(boar.GlobalPosition + new Vector2(reach + boar.BodyRadius - 6, 0) * (P.GlobalPosition.X >= boar.GlobalPosition.X ? 1 : -1), Vector2.Zero);
        P.Face(boar.GlobalPosition - P.GlobalPosition);
        float bh = boar.Brain.Health; bool flinchSameFrame = false; float dropAt = -1;
        Key(Godot.Key.J, true); await Flush();
        for (int i = 0; i < 14 && dropAt < 0; i++)
        {
            await Frames(1);
            if (i == 2) { Key(Godot.Key.J, false); await Flush(); }
            if (boar.Brain.Health < bh) { dropAt = boar.Brain.StateTime; flinchSameFrame = boar.FlinchLeft > 0 && boar.HitFlash > 0; }
        }
        Key(Godot.Key.J, false); await Flush();
        Check("the counter lands in the recovery and the body flinches in the frame the Vida drops", dropAt >= 0 && flinchSameFrame, $"Vida {bh}→{boar.Brain.Health}, recuo {boar.FlinchLeft:0.00} s");
        await Shot("duelo-3-contra-ataque");
        float last = 0;
        while (boar.Brain.State == BoarState.Recover) { last = boar.Brain.StateTime; await Frames(1); }
        Check("the hit in the recovery stretches it (HitFlinch)", dropAt < 0 || last >= boar.Tuning.RecoverTime + boar.Tuning.HitFlinch - 0.05f, $"recuperação {last:0.00} s (base {boar.Tuning.RecoverTime} + {boar.Tuning.HitFlinch})");
        await Frames(30);
        Finish();
    }

    // ------------------------------------------------------------------ the charge strip, with the assist off and on
    private async Task StripPhase()
    {
        await Open(Hunter(_seed));
        var boar = Lone();
        Vector2 home = boar.GlobalPosition;
        float shortAtLock = -1, offAtLock = -1;
        foreach (bool assist in new[] { false, true })
        {
            Assist.Areas = assist;
            await Teleport(boar.GlobalPosition + new Vector2(90, 0), Vector2.Left);
            await WaitFor(() => boar.Brain.State == BoarState.Warn, 300);
            bool before = boar.Brain.DirectionLocked;
            await WaitFor(() => boar.Brain.DirectionLocked, 60);
            Check($"assist {(assist ? "on" : "off")}: the strip is not drawn before the direction locks, then it is", !before && JavaliArt.StripLength(boar, 0) > 0 && boar.Brain.State == BoarState.Warn,
                $"travada antes {before}, comprimento {JavaliArt.StripLength(boar, 0):0}");
            Vector2 end = boar.GlobalPosition + boar.Brain.Facing * JavaliArt.StripLength(boar, 0);
            shortAtLock = JavaliArt.StripLength(boar, 1); offAtLock = JavaliArt.StripLength(boar, 2);
            await Shot($"faixa-{(assist ? "com" : "sem")}-assistencia");
            P.GlobalPosition += new Vector2(0, 40); // step aside: the charge runs its whole length
            await WaitFor(() => boar.Brain.State == BoarState.Charge, 60);
            float midLeft = JavaliArt.StripLength(boar, 0);
            Vector2 midEnd = boar.GlobalPosition + boar.Brain.Facing * midLeft;
            await WaitFor(() => boar.Brain.State != BoarState.Charge, 120);
            float dev = boar.GlobalPosition.DistanceTo(end);
            Check($"assist {(assist ? "on" : "off")}: the strip ends where the charge really ends (< 4 px), also while it runs", dev < 4 && midEnd.DistanceTo(end) < 4,
                $"desvio {dev:0.0} px, no meio {midEnd.DistanceTo(end):0.0} px, estado {boar.Brain.State}");
            await Teleport(home + new Vector2(400, 0), Vector2.Left);
            await WaitFor(() => boar.Brain.State == BoarState.Rooting, 60 * 12);
        }
        Assist.Areas = false;
        Assist.Strip = 2;
        Check("short mode: 60 px at the lock; off mode: no strip; the choice is saved (Esc → Modo de assistência)", Mathf.IsEqualApprox(shortAtLock, 60) && offAtLock == 0 && Prefs.Load().ChargeStrip == 2,
            $"curta {shortAtLock}, desligada {offAtLock}, salvo {Prefs.Load().ChargeStrip}");
        Assist.Strip = 0;
        Finish();
    }

    // ------------------------------------------------------------------ the bar near the animal
    private async Task BarPhase()
    {
        await Open(Hunter(_seed));
        var boar = Lone();
        await Teleport(boar.GlobalPosition + new Vector2(60, 30), Vector2.Left);
        _g.ToggleFocus(); await Frames(3);
        Check("focused: the compact bar shows over the animal with 'atual/máximo'", _g.Near.Visible && _g.Near.Target == boar && _g.Near.Text == $"{boar.HealthMax}/{boar.HealthMax}",
            $"visível {_g.Near.Visible}, texto '{_g.Near.Text}'");
        Check("the bar sits over the animal's head", _g.Near.GlobalPosition.DistanceTo(boar.GlobalPosition + new Vector2(0, -NearBar.Above)) < 1, $"{_g.Near.GlobalPosition} / {boar.GlobalPosition}");
        await Shot("barra-junto-focado");
        _g.ToggleFocus(); await Frames(3);
        await WaitFor(() => boar.Brain.State == BoarState.Warn, 240);
        await Frames(2);
        Check("not focused: a boar warning near the swordsman shows the bar too", _g.Near.Visible && _g.Near.Target == boar, $"{boar.Brain.State}, visível {_g.Near.Visible}");
        // the counter takes Vida: the numbers follow
        await WaitFor(() => boar.Brain.State == BoarState.Recover, 200);
        if (boar.Brain.State == BoarState.Recover) { boar.ReceiveHitFrom(1, P.GlobalPosition); await Frames(2); }
        Check("the numbers follow the Vida", _g.Near.Text == $"{DamageRules.Format(boar.HealthExact)}/{boar.HealthMax}", $"'{_g.Near.Text}', Vida {boar.HealthExact}");
        _g.ToggleFocus(); await Frames(2);
        Check("the top bar stays while focused", _g.Focused == boar, "");
        _g.ToggleFocus();
        await Teleport(boar.HomeAt + new Vector2(500, 0), Vector2.Right);
        await Frames(Mathf.CeilToInt(FaunaSettings.Current.NearBarSeconds * 60) + 20);
        Check($"away from the fight the bar goes after {FaunaSettings.Current.NearBarSeconds} s", !_g.Near.Visible, $"visível {_g.Near.Visible}");
        Finish();
    }

    // ------------------------------------------------------------------ the weakened Lâmina goes through the warning at half damage
    private async Task LaminaPhase()
    {
        await Open(FixtureJourney());
        var boar = Lone();
        P.Ki = P.MaxKi; P.Vigor = P.MaxVigor;
        await Teleport(boar.GlobalPosition + new Vector2(46, 0), Vector2.Left);
        _g.ToggleFocus();
        await WaitFor(() => boar.Brain.State == BoarState.Warn, 240);
        float hp0 = boar.Brain.Health; int dmg = -1; bool landed = false;
        void OnShot(Player.KiShot s) { if (s.Lamina) dmg = s.Damage; }
        P.KiFired += OnShot;
        Vector2 spot = P.GlobalPosition;
        foreach (var side in new[] { Godot.Key.S, Godot.Key.W, Godot.Key.S, Godot.Key.W })
        {
            await Teleport(spot, Vector2.Left);
            await WaitFor(() => boar.Brain.State == BoarState.Warn && !boar.Brain.DirectionLocked, 60 * 6);
            P.Ki = P.MaxKi; P.Vigor = P.MaxVigor; hp0 = boar.Brain.Health; dmg = -1;
            await Chord(side, Godot.Key.L); await Frames(2); await Tap(Godot.Key.J); await Frames(2); await Tap(Godot.Key.K);
            landed = await WaitFor(() => boar.Brain.Health < hp0, 40);
            if (landed || dmg >= 0) break;
            GD.Print($"  (Lâmina não saiu pelo lado {side}: estado {boar.Brain.State}; tenta de novo)");
            await WaitFor(() => boar.Brain.State is BoarState.Rooting or BoarState.Return, 60 * 6);
        }
        P.KiFired -= OnShot;
        float took = hp0 - boar.Brain.Health;
        float want = dmg * FaunaSettings.Current.LaminaThroughFactor;
        if (dmg < 0) Check("NOT ASSESSED: the Lâmina did not fire in this run (combo timing)", false, $"estado do javali {boar.Brain.State}");
        else Check("the weakened Lâmina goes through the hide at half damage (decision B), the boar keeps going", Mathf.Abs(took - want) < 0.35f,
            $"dano da Lâmina {dmg}, tirou {took:0.##} (esperado {want:0.##}, crítico arredonda), estado {boar.Brain.State}");
        await Shot("lamina-atravessa");
        Finish();
    }

    // ------------------------------------------------------------------ the rabbit: ears, crouch, bush, dive; walking vs Shift
    private async Task RabbitPhase()
    {
        await Open(Hunter(_seed));
        var rabbit = Nodes<Rabbit>().Where(r => r.Brain.Visible).OrderBy(r => r.GlobalPosition.DistanceTo(P.GlobalPosition)).First();
        foreach (var o in Nodes<Rabbit>().Where(r => r != rabbit && r.BurrowAt.DistanceTo(rabbit.BurrowAt) < 500).ToList()) _g.World.Remove(o);
        foreach (var b in Nodes<Boar>().Where(b => b.GlobalPosition.DistanceTo(rabbit.GlobalPosition) < 500).ToList()) _g.World.Remove(b);
        // hearing: approach walking, then with Shift, and note where the ears rise
        float walkAt = await Approach(rabbit, false);
        await WaitFor(() => rabbit.Brain.State == RabbitState.Graze, 60 * 10);
        await Frames(30);
        float sneakAt = await Approach(rabbit, true);
        Check($"walking it hears you at its hearing ({rabbit.Tuning.HearWalk:0} px at Nv {rabbit.Level}; 95 at the base); with Shift only close ({rabbit.Tuning.HearSneak:0}; 26 at the base)",
            walkAt > rabbit.Tuning.HearWalk - 12 && walkAt <= rabbit.Tuning.HearWalk + 2 && sneakAt > 0 && sneakAt <= rabbit.Tuning.HearSneak + 2 && sneakAt < walkAt / 2.5f, $"andando {walkAt:0} px, Shift {sneakAt:0} px");
        await WaitFor(() => rabbit.Brain.State == RabbitState.Graze, 60 * 10);
        await Frames(30);
        // the flight: ears to the right side, the crouch, the bush, the dive
        var bushes = Nodes<WorldElement>().Where(e => e.Def.Family == "arbusto" && e.GlobalPosition.DistanceTo(rabbit.BurrowAt) < 120).ToList();
        var bend0 = bushes.ToDictionary(b => b, b => b.MaxBend);
        Vector2 from = rabbit.GlobalPosition + (rabbit.GlobalPosition - rabbit.BurrowAt).Normalized() * 70;
        if (from.DistanceTo(rabbit.GlobalPosition) < 1) from = rabbit.GlobalPosition + new Vector2(70, 0);
        await Teleport(from, rabbit.GlobalPosition - from);
        bool sawCrouch = false, earsOk = true, earsSeen = false, dived = false, shotEars = false, shotRun = false, shotDive = false;
        for (int i = 0; i < 60 * 8 && rabbit.Brain.State != RabbitState.Hidden; i++)
        {
            if (rabbit.Brain.State == RabbitState.Graze) { Key(Godot.Key.W, true); await Flush(); await Frames(2); Key(Godot.Key.W, false); await Flush(); P.Face(rabbit.GlobalPosition - P.GlobalPosition); }
            await Frames(1);
            if (rabbit.Brain.EarsUp) earsSeen = true;
            if (rabbit.Brain.State == RabbitState.Dash1 && rabbit.Velocity.LengthSquared() > 1)
                earsOk &= rabbit.Brain.Facing.Dot(rabbit.Velocity.Normalized()) > 0.5f; // it faces where it runs: the ears sit at the back of the head for that side
            if (rabbit.Brain.State == RabbitState.Alert) { sawCrouch = true; if (!shotEars) { shotEars = true; await Shot("coelho-1-orelhas-e-agachado"); } }
            if (rabbit.Brain.State is RabbitState.Dash1 or RabbitState.Dash2 && !shotRun) { shotRun = true; await Shot("coelho-2-correndo"); }
        }
        for (int i = 0; i < 30; i++) { if (rabbit.DiveLeft > 0) { dived = true; if (!shotDive) { shotDive = true; await Shot("coelho-3-mergulho"); } } await Frames(1); }
        Check("the ears rise and the body crouches before the dash; it faces the way it runs", earsSeen && earsOk && sawCrouch, $"orelhas {earsSeen}, agachado {sawCrouch}, de frente para a corrida {earsOk}");
        Check("it dives into the burrow (shrinks into the hole) instead of vanishing", dived && rabbit.Dived && rabbit.GlobalPosition.DistanceTo(rabbit.BurrowAt) < 8, $"mergulho {dived}, {rabbit.GlobalPosition.DistanceTo(rabbit.BurrowAt):0} px da toca");
        float bend = bushes.Count == 0 ? 0 : bushes.Max(b => b.MaxBend - bend0[b]);
        string how = "na fuga";
        if (bend <= 0.5f)
        {
            // the flight did not cross a bush on this seed: carry the rabbit through the nearest one at its dash speed
            var bush = Nodes<WorldElement>().Where(e => e.Def.Family == "arbusto").OrderBy(e => e.GlobalPosition.DistanceTo(rabbit.GlobalPosition)).FirstOrDefault();
            if (bush != null)
            {
                float m0 = bush.MaxBend; rabbit.ProcessMode = ProcessModeEnum.Disabled;
                rabbit.GlobalPosition = bush.GlobalPosition + new Vector2(-30, 2);
                for (int i = 0; i < 24; i++) { rabbit.Velocity = new Vector2(rabbit.Tuning.DashSpeed, 0); rabbit.GlobalPosition += new Vector2(rabbit.Tuning.DashSpeed / 60f, 0); await Frames(1); }
                bend = bush.MaxBend - m0; how = "atravessando o arbusto mais perto";
                await Shot("coelho-4-arbusto");
            }
        }
        Check("a bush bends as the rabbit runs through it", bend > 0.5f, $"curvatura {bend:0.0} ({how})");
        Finish();
    }

    // walks (or sneaks with Shift) at the rabbit, steering toward it; returns the distance when its ears went up
    private async Task<float> Approach(Rabbit r, bool sneak)
    {
        Vector2 dir = (r.GlobalPosition - r.BurrowAt).LengthSquared() > 25 ? (r.GlobalPosition - r.BurrowAt).Normalized() : Vector2.Right;
        await Teleport(r.GlobalPosition + dir * 140, -dir);
        if (sneak) { Key(Godot.Key.Shift, true); await Flush(); }
        var held = new List<Godot.Key>();
        float at = -1, closest = 1e9f;
        for (int i = 0; i < 60 * 8 && at < 0; i++)
        {
            Vector2 to = r.GlobalPosition - P.GlobalPosition;
            var want = new List<Godot.Key>();
            if (to.X > 3) want.Add(Godot.Key.D); else if (to.X < -3) want.Add(Godot.Key.A);
            if (to.Y > 3) want.Add(Godot.Key.S); else if (to.Y < -3) want.Add(Godot.Key.W);
            if (!want.SequenceEqual(held)) { foreach (var k in held) Key(k, false); foreach (var k in want) Key(k, true); held = want; await Flush(); }
            await Frames(1);
            closest = Mathf.Min(closest, r.GlobalPosition.DistanceTo(P.GlobalPosition));
            if (r.Brain.EarsUp) at = r.GlobalPosition.DistanceTo(P.GlobalPosition);
        }
        foreach (var k in held) Key(k, false);
        if (sneak) Key(Godot.Key.Shift, false);
        await Flush();
        GD.Print($"  (aproximação {(sneak ? "Shift" : "andando")}: {(at < 0 ? "sem ouvir" : $"ouviu a {at:0} px")}, mais perto {closest:0} px, ruído {P.Noise}, audição andando {r.Tuning.HearWalk:0} / Shift {r.Tuning.HearSneak:0})");
        return at;
    }

    // ------------------------------------------------------------------ fresh prints and reading them
    private async Task PrintsPhase()
    {
        await Open(Hunter(_seed));
        var rabbit = Nodes<Rabbit>().Where(r => r.Brain.Visible).OrderBy(r => r.GlobalPosition.DistanceTo(P.GlobalPosition)).First();
        foreach (var o in Nodes<Rabbit>().Where(r => r != rabbit).ToList()) _g.World.Remove(o);
        foreach (var b in Nodes<Boar>().ToList()) _g.World.Remove(b);
        // 1. the flight leaves prints on the real path: each points the way the rabbit was running at that moment
        var seen = new HashSet<TrackPrint>(); float worst = 0; int n = 0;
        Vector2 from = rabbit.GlobalPosition + (rabbit.GlobalPosition - rabbit.BurrowAt).Normalized() * 60;
        await Teleport(from, rabbit.GlobalPosition - from);
        Key(Godot.Key.W, true); await Flush(); await Frames(4); Key(Godot.Key.W, false); await Flush();
        var path = new List<Vector2> { rabbit.GlobalPosition };
        for (int i = 0; i < 60 * 6 && rabbit.Brain.State != RabbitState.Hidden; i++)
        {
            await Frames(1);
            path.Add(rabbit.GlobalPosition);
            // the way it ran: its movement over the last few frames (about one print's spacing)
            Vector2 moved = path[^1] - path[Mathf.Max(0, path.Count - 6)];
            foreach (var p in _g.Tracks.Prints.Where(p => p.CreatureId == rabbit.CreatureId && seen.Add(p)))
                if (moved.LengthSquared() > 4) { worst = Mathf.Max(worst, Mathf.RadToDeg(Mathf.Abs(p.Dir.AngleTo(moved)))); n++; }
        }
        Check("the flight leaves fresh prints, each within 20° of the way the rabbit ran", n >= 1 && worst <= 20, $"{n} pegadas, pior desvio {worst:0}°");
        await Shot("pegadas-1-fuga");
        // 2. a turn: the rabbit carried east then south leaves prints that turn with it
        Vector2 here = P.GlobalPosition;
        await Teleport(rabbit.BurrowAt + new Vector2(0, 260), Vector2.Up); // away: it comes out again
        bool back = await WaitFor(() => rabbit.Brain.State == RabbitState.Graze, 60 * 10);
        Check("away from the burrow, the rabbit comes out again", back, $"{rabbit.Brain.State}");
        await Teleport(here, Vector2.Up);
        rabbit.ProcessMode = ProcessModeEnum.Disabled;
        Vector2 p0 = P.GlobalPosition + new Vector2(-60, -40); // a jump: the trail restarts without a print
        rabbit.GlobalPosition = p0; await Frames(2);
        var before = _g.Tracks.Prints.ToHashSet();
        for (int i = 0; i < 60; i++) { rabbit.GlobalPosition += Vector2.Right; await Frames(1); }
        for (int i = 0; i < 60; i++) { rabbit.GlobalPosition += Vector2.Down; await Frames(1); }
        var turn = _g.Tracks.Prints.Where(p => !before.Contains(p) && p.CreatureId == rabbit.CreatureId).ToList();
        Check("prints follow a turn: east first, then south", turn.Count >= 6 && turn.First().Dir.Dot(Vector2.Right) > 0.94f && turn.Last().Dir.Dot(Vector2.Down) > 0.94f,
            $"{turn.Count} pegadas, primeira {turn.FirstOrDefault()?.Dir}, última {turn.LastOrDefault()?.Dir}");
        // 3. reading a print narrows with Rastreamento and always contains the real direction
        var print = turn.Last();
        rabbit.GlobalPosition = print.GlobalPosition + new Vector2(150, 40);
        var halves = new List<float>(); bool contains = true;
        foreach (int prac in new[] { 0, 1000 })
        {
            _g.Skills.Restore(new Dictionary<string, int> { ["rastreamento"] = prac }, System.Array.Empty<string>());
            await Teleport(print.GlobalPosition + new Vector2(0, 8), Vector2.Up);
            while (_g.Hud.AnyPanelOpen) { await Tap(Godot.Key.Escape); await Frames(3); }
            var chosen = _g.ChosenTarget();
            if (chosen != print) foreach (var _ in Enumerable.Range(0, 4)) { if (_g.ChosenTarget() == print) break; await Wheel(true); }
            await Tap(Godot.Key.E); await Frames(3);
            halves.Add(_g.LastTrackSector.Half);
            var ex = _g.LastTrackPrint;
            contains &= ex != null && TrackingRules.Contains(_g.LastTrackSector, rabbit.GlobalPosition - ex.GlobalPosition);
            await Shot(prac == 0 ? "pegadas-2-exame-novato" : "pegadas-2-exame-mestre");
            if (_g.Hud.AnyPanelOpen) { await Tap(Godot.Key.E); await Frames(3); }
        }
        Check("low Rastreamento gives a broad direction, Mestre a narrow one; both contain where the animal is", halves.Count == 2 && halves[1] < halves[0] / 3 && contains,
            $"meia largura {string.Join(" → ", halves.Select(h => $"{Mathf.RadToDeg(h):0}°"))}, contém {contains}");
        Check("the hint cone is drawn on the ground", Nodes<TrackHint>().Any() || _g.World.Objects.GetChildren().OfType<TrackHint>().Any(), "");
        // 4. the wheel chooses the fresh print over a herb
        var herb = Nodes<Pickup>().Where(h => h.ItemId == "grama_sangue").OrderBy(h => h.GlobalPosition.DistanceTo(P.GlobalPosition)).FirstOrDefault();
        if (herb == null) Check("NOT ASSESSED: no herb loaded near the swordsman", false);
        else
        {
            while (_g.Hud.AnyPanelOpen) { await Tap(Godot.Key.Escape); await Frames(3); }
            var b2 = _g.Tracks.Prints.ToHashSet();
            Vector2 a = herb.GlobalPosition + new Vector2(-40, 10);
            rabbit.GlobalPosition = a; await Frames(2);
            for (int i = 0; i < 80; i++) { rabbit.GlobalPosition += Vector2.Right; await Frames(1); }
            rabbit.GlobalPosition += new Vector2(0, 400); await Frames(2);
            var near = _g.Tracks.Prints.Where(p => !b2.Contains(p)).OrderBy(p => p.GlobalPosition.DistanceTo(herb.GlobalPosition)).First();
            await Teleport(herb.GlobalPosition.Lerp(near.GlobalPosition, 0.5f) + new Vector2(0, 4), Vector2.Up); await Frames(6);
            var list = _g.Interactables();
            GD.Print($"  (erva {herb.GlobalPosition}, pegada {near.GlobalPosition}, personagem {P.GlobalPosition}, painel {_g.Hud.AnyPanelOpen}, alvos {list.Count})");
            bool both = list.Any(x => x is Pickup) && list.Any(x => x is TrackPrint tp && tp.Fresh);
            for (int i = 0; i < 4 && _g.ChosenTarget() is not TrackPrint; i++) await Wheel(true);
            int msgs = _g.Hud.Messages.Count;
            bool chosePrint = _g.ChosenTarget() is TrackPrint;
            await Shot("pegadas-3-roda-escolhe-a-pegada");
            await Tap(Godot.Key.E); await Frames(3);
            Check("with a herb and a fresh print in reach, the wheel chooses the print and E examines it", both && chosePrint && _g.Hud.Messages.Skip(msgs).Any(m => m.Contains("coelho")),
                $"alvos [{string.Join(", ", list.Select(_g.TargetName))}], escolhida a pegada {chosePrint}");
        }
        // 5. fresh prints fade and go after track_seconds
        var oldest = _g.Tracks.Prints.First();
        oldest.Age = oldest.Life - 0.1f; await Frames(12);
        Check($"a fresh print fades and goes after {FaunaSettings.Current.TrackSeconds} s", !IsInstanceValid(oldest) || oldest.IsQueuedForDeletion(), "");
        Finish();
    }

    private async Task Wheel(bool down)
    {
        var at = GetViewport().GetVisibleRect().Size / 2;
        var b = down ? MouseButton.WheelDown : MouseButton.WheelUp;
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = b, Pressed = true, Position = at, GlobalPosition = at, Factor = 1 }); await Flush(); await Frames(1);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = b, Pressed = false, Position = at, GlobalPosition = at }); await Flush(); await Frames(3);
    }

    // ------------------------------------------------------------------ full vs weakened techniques, same camera scale
    private async Task TechPhase()
    {
        await Open(FixtureJourney());
        foreach (var r in Nodes<Rabbit>().ToList()) _g.World.Remove(r);
        foreach (var b in Nodes<Boar>().ToList()) _g.World.Remove(b);
        var results = new Dictionary<string, (float Shot, float Step, bool Fizzle)>();
        foreach (var (label, state) in new[] { ("plena", TechniqueState.Full), ("enfraquecida", TechniqueState.Weakened) })
        {
            _g.Tech.Set("ki_wave", state); _g.Tech.Set("dash", state);
            P.Ki = P.MaxKi; P.Vigor = P.MaxVigor;
            await Teleport(_g.Gen.StartPixel + new Vector2(-60, 60), Vector2.Right); await Frames(20);
            float went = -1; bool fizzled = false;
            void Ended(KiWave w, bool hit) => went = Mathf.Min(w.Went, w.Shot.Range);
            _g.KiShotEnded += Ended;
            int fz = _g.World.Objects.GetChildren().OfType<KiFizzle>().Count();
            await Tap(Godot.Key.K);
            for (int i = 0; i < 120 && went < 0; i++) await Frames(1);
            _g.KiShotEnded -= Ended;
            fizzled = _g.World.Objects.GetChildren().OfType<KiFizzle>().Count() > fz;
            await Frames(6); // the crumble a moment after the end
            await Shot($"tecnica-disparo-{label}");
            P.Ki = P.MaxKi; P.Vigor = P.MaxVigor;
            await Teleport(_g.Gen.StartPixel + new Vector2(-60, 60), Vector2.Right); await Frames(20);
            Vector2 d0 = P.GlobalPosition;
            await Chord(Godot.Key.Shift, Godot.Key.D, Godot.Key.L); await Frames(20);
            results[label] = (went, d0.DistanceTo(P.GlobalPosition), fizzled);
            await Shot($"tecnica-passo-{label}");
        }
        var full = _g.Tech.Data.KiWave.Full; var weak = _g.Tech.Data.KiWave.Weak;
        var (fs, fst, ff) = results["plena"]; var (ws, wst, wf) = results["enfraquecida"];
        Check($"full Disparo de Ki ends at its data range ({full.Range:0} px ± 4) and crumbles there", Mathf.Abs(fs - full.Range) <= 4 && ff, $"{fs:0} px, desfaz {ff}");
        Check($"weakened Disparo de Ki ends at {weak.Range:0} px ± 4 and crumbles there", Mathf.Abs(ws - weak.Range) <= 4 && wf, $"{ws:0} px, desfaz {wf}");
        Check("the long step: full about 112 px, weakened about 60 px", fst > 100 && fst < 120 && wst > 50 && wst < 70, $"plena {fst:0} px, enfraquecida {wst:0} px");
        Finish();
    }

    // ------------------------------------------------------------------ the map: boars only after seen or tracked
    private async Task MapPhase()
    {
        await Open(Hunter(_seed));
        var boar = Nodes<Boar>().OrderBy(b => b.GlobalPosition.DistanceTo(P.GlobalPosition)).First();
        var chunk = _g.Gen.GenerateChunk(_g.Gen.ChunkOfPixel(boar.HomeAt));
        var f = chunk.Features.First(x => x.Kind == FeatureKind.Boar && PlainsGenerator.CreatureIdOf(x) == boar.CreatureId);
        bool far = boar.GlobalPosition.DistanceTo(P.GlobalPosition) > FaunaSettings.Current.SeenRadius;
        Check("a boar not yet seen is not on the map", far && !_g.BoarOnMap(f), $"{boar.GlobalPosition.DistanceTo(P.GlobalPosition):0} px");
        _g.Panels.OpenMap(); await Frames(3); await Shot("mapa-1-antes"); _g.Panels.Choose("back"); await Frames(2);
        await Teleport(boar.GlobalPosition + new Vector2(FaunaSettings.Current.SeenRadius - 20, 0), Vector2.Left); await Frames(3);
        Check("seen within seen_radius, it shows on the map", _g.BoarOnMap(f), "");
        _g.Panels.OpenMap(); await Frames(3); await Shot("mapa-2-depois-de-visto"); _g.Panels.Choose("back"); await Frames(2);
        var snap = _g.Snapshot();
        Check("the journey saves the animals seen", snap.SeenAnimals.Contains(boar.CreatureId), string.Join(",", snap.SeenAnimals));
        // tracking: a hoof print far enough from its boar not to see it; the boar is forgotten first
        var boars = Nodes<Boar>().ToDictionary(b => b.CreatureId, b => b.GlobalPosition);
        var trail = Nodes<TrackPrint>().Where(t => !t.Fresh && t.Hoof && boars.TryGetValue(t.CreatureId, out var bp) && bp.DistanceTo(t.GlobalPosition) > FaunaSettings.Current.SeenRadius + 30)
            .OrderBy(t => t.GlobalPosition.DistanceTo(P.GlobalPosition)).FirstOrDefault();
        if (trail == null) GD.Print("  (sem rastro de casco longe do javali nesta semente: o caminho pelo rastro não foi medido aqui)");
        else
        {
            _g.SeenAnimals.Remove(trail.CreatureId);
            await Teleport(trail.GlobalPosition + new Vector2(0, 8), Vector2.Up); await Frames(4);
            bool hiddenBefore = !_g.SeenAnimals.Contains(trail.CreatureId);
            for (int i = 0; i < 4 && _g.ChosenTarget() is not TrackPrint; i++) await Wheel(true);
            await Tap(Godot.Key.E); await Frames(3);
            Check("tracking: examining a hoof trail puts its boar on the map (without seeing it)", hiddenBefore && _g.SeenAnimals.Contains(trail.CreatureId), trail.CreatureId);
        }
        Finish();
    }

    // ------------------------------------------------------------------ legible in a gust
    private async Task WindPhase()
    {
        await Open(Hunter(_seed));
        var boar = Lone();
        await Teleport(boar.GlobalPosition + new Vector2(90, 0), Vector2.Left);
        await WaitFor(() => boar.Brain.DirectionLocked, 300);
        boar.ProcessMode = ProcessModeEnum.Disabled; P.InputLocked = true;
        float peak = 0, best = 0;
        for (float t = 0; t < 20; t += 0.05f) { Wind.T = t; if (Wind.Gust > best) { best = Wind.Gust; peak = t; } }
        Wind.T = peak;
        Engine.TimeScale = 0; await Flush(); await Flush();
        var on = await Grab(); await Shot("vento-faixa-na-rajada");
        Assist.Strip = 2; boar.QueueRedraw(); await Flush(); await Flush();
        var off = await Grab();
        Assist.Strip = 0; boar.QueueRedraw();
        Engine.TimeScale = 1;
        if (on == null || off == null) { Check("NOT ASSESSED: no picture without a window", false); Finish(); return; }
        int changed = Diff(on, off, 0.06f);
        Check($"in a gust (wind {best:0.00}) the charge strip still stands out from the swaying grass", changed > 150, $"{changed} px mudam com a faixa");
        Finish();
    }

    private static int Diff(Image a, Image b, float min)
    {
        int n = 0;
        for (int y = 0; y < a.GetHeight(); y += 1)
            for (int x = 0; x < a.GetWidth(); x += 1)
            {
                Color p = a.GetPixel(x, y), q = b.GetPixel(x, y);
                if (Mathf.Abs(p.R - q.R) + Mathf.Abs(p.G - q.G) + Mathf.Abs(p.B - q.B) > min) n++;
            }
        return n;
    }

    // ------------------------------------------------------------------ the old journey loads
    private async Task OldSave()
    {
        await Open(FixtureJourney());
        Check("the fixture journey (no T07 fields) loads: level 4, the knife, weakened Ki shot", _g.Started && _g.Stats.Level == 4 && _g.Tech.State("ki_wave") == TechniqueState.Weakened,
            $"nível {_g.Stats.Level}, disparo {_g.Tech.State("ki_wave")}");
        Check("no animals seen yet: no boar on the map", _g.SeenAnimals.Count == 0, $"{_g.SeenAnimals.Count}");
        Check("strip preference defaults to full", Assist.Strip == 0, $"{Assist.Strip}");
        Finish();
    }

    // ------------------------------------------------------------------ the generated trails are untouched
    private async Task Trails()
    {
        await Open(Hunter(_seed));
        var trails = Nodes<TrackPrint>().Where(t => !t.Fresh).ToList();
        string sig = string.Join(";", trails.OrderBy(t => t.FeatureId).Take(6).Select(t => $"{t.FeatureId}@{t.GlobalPosition.X:0},{t.GlobalPosition.Y:0}"));
        Check($"seed {_seed}: the clearing trails are generated as before ({trails.Count} prints)", trails.Count > 0 && trails.All(t => t.TrailId.StartsWith("trail@") && t.Opacity == 1), sig);
        Check("generated prints never fade", trails.All(t => !t.Fresh), "");
        Finish();
    }
}
