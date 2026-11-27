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
using SpiritOfMurin.Creatures;

namespace SpiritOfMurin.Tests;

/// <summary>
/// Pedido 13, through the main scene and real keys and mouse. Phases, each its own run:
/// <list type="bullet">
/// <item><c>--phase=aim</c>: walking left with the cursor to the right, the cut, the knife and the Ki shot go
/// right and hit there; a remapped cut aims too; a locked target wins over the cursor and unlocking gives the
/// cursor back on the next attack; the keyboard option aims along the facing; the cursor maps to the world at
/// other window sizes.</item>
/// <item><c>--phase=charge</c>: the secret charged Ki shot in the Vale — a tap is the plain shot; a held press
/// gathers Ki (a partial charge with proportional damage, the full 100 that holds ready and breaks the bridge
/// disciple's guard); never more than one projectile per press; less Ki than the full cost; pause, inventory
/// and losing the focus cancel without a shot and give the Ki back; a remapped key; the discovery and the
/// book page; the bridge's retreat and flank still there.</item>
/// <item><c>--phase=charge-plains</c>: in the Planície, the sealed Ki shot refuses the charge; next to a
/// carcass the button skins with Ki (no charge); at level 3 the weakened charge works.</item>
/// <item><c>--phase=reinforce</c>: the Reforço de Ki — modifier + cut with 75 Ki activates once (3 charges),
/// 74 is refused and kept, holding on does not repeat it, a new try keeps the charges left; three reinforced
/// strikes (+10%, fractional: the knife's 1 → 1.1) and the fourth normal; a miss and each cut of a combo spend
/// a charge; one break roll per strike and guard, with fixed rolls for the success and the failure; Shift
/// alone sneaks, the step still works; a remapped cut and modifier; the discovery kept by meditating.</item>
/// <item><c>--phase=bag</c>: the bag with pictures in the Planície — every item has its picture and its stack;
/// the knife is crafted by keyboard (the flint and the branch go down, the knife appears in the same bag),
/// equipped by a click (marked in use), a herb is found and used by keyboard and another by the action
/// button; window and full screen.</item>
/// <item><c>--phase=money</c> then <c>--phase=money2</c>: the coins — none at the start; the pouch by the
/// meditation mat gives +25 with E; leaving the chunk and coming back keeps it taken; receiving and spending,
/// with a short balance refused; meditating saves; in a new process the balance and the taken pouch are back.</item>
/// <item><c>--phase=oldsave</c>: a journey saved before this delivery (no coins field) loads with its items and
/// its equipment, 0 coins and the pouch still there; the awakening starts without money.</item>
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
            case "charge": Fresh(); Launch("new"); _ = ChargePhase(); break;
            case "charge-plains": Fresh(); _ = ChargePlains(); break;
            case "reinforce": Fresh(); Launch("new"); _ = ReinforcePhase(); break;
            case "bag": Fresh(); _ = BagPhase(); break;
            case "money": Fresh(); _ = MoneyPhase(); break;
            case "money2": _ = MoneyResume(); break;
            case "oldsave": Fresh(); _ = OldSavePhase(); break;
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
        Vector2? shotDir = null; P.KiFired += s => shotDir = s.Dir;

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
        Check("walking left with the cursor to the right, J cuts to the right", walkedLeft && cuts == 1 && AngleBetween(P.LastCutAngle, 0) < 0.6f, $"andou para a esquerda {walkedLeft}, cortes {cuts}, ângulo do corte {Mathf.RadToDeg(P.LastCutAngle):0}°");
        Check("the cut's hit area agrees: the pot on the right breaks", V.Story.PotsBroken.Contains(3), $"potes quebrados [{string.Join(",", V.Story.PotsBroken)}]");

        // the Ki shot: walking left, cursor to the right, K
        await Teleport(ValeMap.Start + new Vector2(0, -60), Vector2.Left);
        P.Ki = P.MaxKi;
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

    // ------------------------------------------------------------------ 8. the charged Ki shot
    private readonly List<Player.KiShot> _shots2 = new();

    private async Task HoldKi(Key key, int frames, System.Func<Task>? whileHeld = null)
    {
        Press(key, true); await Flush();
        await Frames(frames);
        if (whileHeld != null) await whileHeld();
        Press(key, false); await Flush(); await Frames(6);
    }

    private async Task ChargePhase()
    {
        await Frames(30);
        if (_c.Vale == null) { Check("Novo jogo starts the Vale", false); Finish(); return; }
        P.KiFired += s => _shots2.Add(s);
        var bookBefore = V.BookRows().Select(b => b.Name).ToList();
        Check("before the discovery the book and the commands keep the secret (no charged shot)", !bookBefore.Any(n => n.Contains("Concentrado")) && !GamePanels.Controls.Any(l => l.Contains("segurar")));
        await Teleport(ValeMap.Start + new Vector2(0, -60), Vector2.Up);
        await PointAt(P.GlobalPosition + new Vector2(0, -120));
        P.KiRegen = 0; // controlled: no regeneration while measuring the Ki

        // a tap: the plain shot (30 Ki, damage 2)
        P.Ki = 100; _shots2.Clear();
        await HoldKi(Key.K, 2); await Frames(10);
        Check("a quick tap is the plain shot (one projectile, 30 Ki, damage 2)", _shots2.Count == 1 && _shots2[0].Charge == 0 && _shots2[0].Damage == 2 && Mathf.IsEqualApprox(P.Ki, 70, 0.6f), $"{_shots2.Count} disparo(s), Ki {P.Ki:0}");
        await Frames(30);

        // a partial charge: 60 Ki available, held long → everything into the palm, damage proportional, no break
        P.Ki = 60; _shots2.Clear();
        bool wasCharging = false; float palm = 0; string kiLine = "";
        await HoldKi(Key.K, 70, async () => { wasCharging = P.Charging; palm = P.ChargeInvested; kiLine = V.Hud.KiText; await Shot("03-carga-parcial-palma"); });
        await Frames(10);
        var part = _shots2.FirstOrDefault();
        Check("held: the palm gathers Ki and the bar shows it (palma 60/100)", wasCharging && Mathf.IsEqualApprox(palm, 60, 0.6f) && kiLine.Contains("palma 60/100"), $"{kiLine}");
        Check("a partial charge (60): one projectile, damage proportional (round(6 × 0.6) = 4), no break, no extra cost", _shots2.Count == 1 && part.Damage == 4 && !part.Breaks && P.Ki < 0.6f, $"{_shots2.Count} disparo(s), dano {part.Damage}, rompe {part.Breaks}, Ki {P.Ki:0.0}");
        bool discovered = V.Story.Book.Any(b => b.Tech == "ki_charge");
        Check("the first charged shot reveals the secret technique (banner, book page with the command)", discovered && V.Hud.BannerText.Contains("Concentrado") && V.BookRows().First(b => b.Name.Contains("Concentrado")).Command.Contains("segurar K"), V.Hud.BannerText);
        await Frames(30);

        // the full charge: 100 → ready, it holds while the button stays down, one shot that breaks the guard
        P.Ki = 100; _shots2.Clear();
        bool readyHeld = false; int shotsWhileReady = -1;
        await HoldKi(Key.K, 110, async () => { await Frames(40); readyHeld = P.ChargeComplete && V.Hud.KiText.Contains("PRONTO"); shotsWhileReady = _shots2.Count; await Shot("04-carga-completa-pronta"); });
        await Frames(10);
        var full = _shots2.FirstOrDefault();
        Check("the full charge (exactly 100) holds ready while the button stays down", readyHeld && shotsWhileReady == 0);
        Check("letting go: one projectile, damage 6, breaks guards; the 100 already include the shot (Ki 0)", _shots2.Count == 1 && full.Damage == 6 && full.Breaks && P.Ki < 0.6f, $"{_shots2.Count} disparo(s), dano {full.Damage}, Ki {P.Ki:0.0}");
        await Frames(40);

        // the bridge disciple: a partial charge is parried; the full one breaks the guard and opens a window
        var dis = V.Fighters["dis_bridge"];
        await Teleport(dis.GlobalPosition + new Vector2(0, 70), Vector2.Up);
        await Frames(20);
        for (int i = 0; i < 3 && V.Lock != dis; i++) await Tap(Key.Space);
        dis.ResetToHome(); await Frames(3);
        await Teleport(dis.GlobalPosition + new Vector2(0, 45), Vector2.Up);
        dis.Face = Mathf.Pi / 2; dis.State = ValeFighter.St.Guard; // controlled: standing in guard, facing the swordsman
        P.Ki = 50; int words = V.Fx.WordsShown.Count; float hp = dis.Hp;
        await HoldKi(Key.K, 60); await Frames(25);
        Check("a partial charge against the Closed Guard is parried (no break)", dis.Hp == hp && dis.Stagger <= 0 && V.Fx.WordsShown.Skip(words).Contains("aparado"), $"vida {hp}→{dis.Hp}");
        await Frames(40);
        dis.ResetToHome(); await Frames(3);
        await Teleport(dis.GlobalPosition + new Vector2(0, 45), Vector2.Up); dis.Face = Mathf.Pi / 2; dis.State = ValeFighter.St.Guard;
        P.Ki = 100; P.Revive(); words = V.Fx.WordsShown.Count; hp = dis.Hp;
        await HoldKi(Key.K, 120); await Frames(18);
        Check("the full charge breaks the Closed Guard: damage and an open window (stagger)", dis.Hp < hp && dis.Stagger > 0.5f && V.Fx.WordsShown.Skip(words).Contains("guarda rompida"), $"vida {hp}→{dis.Hp}, aberta {dis.Stagger:0.0} s");
        await Shot("05-guarda-rompida");
        float hp2 = dis.Hp;
        await Tap(Key.J); await Frames(15);
        Check("during the window a frontal cut lands", dis.Hp < hp2, $"vida {hp2}→{dis.Hp}");
        await Tap(Key.Space);
        dis.ResetToHome(); await Frames(10);

        // less Ki than any charge: holding with 0 Ki does nothing but a refusal-free empty palm, no shot
        await Teleport(ValeMap.Start + new Vector2(0, -60), Vector2.Up);
        P.Ki = 0; _shots2.Clear();
        await HoldKi(Key.K, 60); await Frames(10);
        Check("with no Ki the palm stays empty: the shot that follows does 1 damage at most and costs nothing more", _shots2.Count <= 1 && (_shots2.Count == 0 || _shots2[0].Damage == 1) && P.Ki < 0.6f, $"{_shots2.Count} disparo(s)");

        // pause during the charge: cancelled, the Ki comes back, no shot when the game resumes
        P.Ki = 100; _shots2.Clear();
        Press(Key.K, true); await Flush(); await Frames(50);
        float invested = P.ChargeInvested;
        await Tap(Key.Escape);
        Press(Key.K, false); await Flush(); await Frames(5);
        await Tap(Key.Escape); await Frames(20);
        Check("pausing during the charge cancels it: the invested Ki goes back, no projectile on return", invested > 10 && _shots2.Count == 0 && P.Ki > 99 && !P.Charging, $"investido {invested:0}, Ki {P.Ki:0}, disparos {_shots2.Count}");
        // the inventory, the same
        P.Ki = 100; _shots2.Clear();
        Press(Key.K, true); await Flush(); await Frames(50);
        await Tap(Key.I);
        Press(Key.K, false); await Flush(); await Frames(5);
        await Tap(Key.I); await Frames(20);
        Check("opening the inventory during the charge cancels it the same way", _shots2.Count == 0 && P.Ki > 99 && !P.Charging, $"Ki {P.Ki:0}, disparos {_shots2.Count}");
        // losing the window's focus
        P.Ki = 100; _shots2.Clear();
        Press(Key.K, true); await Flush(); await Frames(50);
        P.PropagateNotification((int)Node.NotificationApplicationFocusOut);
        Press(Key.K, false); await Flush(); await Frames(20);
        Check("losing the window's focus cancels the charge (no shot, Ki back)", _shots2.Count == 0 && P.Ki > 99, $"Ki {P.Ki:0}, disparos {_shots2.Count}");

        // a remapped key (O)
        Bindings.Assign("ki", 0, $"k:{(long)Key.O}");
        P.Ki = 100; _shots2.Clear();
        await HoldKi(Key.O, 110); await Frames(10);
        Check("the remapped key (O) charges the same way", _shots2.Count == 1 && _shots2[0].Breaks, $"{_shots2.Count} disparo(s)");
        Check("the book shows the remapped command (segurar O)", V.BookRows().First(b => b.Name.Contains("Concentrado")).Command.Contains("segurar O"));
        Bindings.Restore();
        // the book page: costs and partial × break
        V.Hud.ShowBook(V.BookRows()); await Frames(2);
        for (int i = 0; i < 10 && !V.Hud.BookRows[V.Hud.BookSelected].Name.Contains("Concentrado"); i++) V.Hud.MoveBook(1);
        await Frames(3); await Shot("06-livro-disparo-concentrado");
        string page = V.Hud.BookDetail;
        Check("the book page has the costs and the difference between a partial charge and the break", page.Contains("100 Ki no total") && page.Contains("rompe a guarda") && page.Contains("carga parcial"), page.Replace("\n", " | "));
        V.Hud.ShowBook(null);

        // the meditation keeps the discovery
        await Teleport(ValeMap.Meditate + new Vector2(0, 8), Vector2.Up);
        await Tap(Key.E); await Frames(3);
        var d = SaveGame.Read(SavePath).Data;
        Check("meditating keeps the discovered technique", d?.Vale?.Book.Any(b => b.Tech == "ki_charge") == true);
        Finish();
    }

    private PlainsGame? _pg;

    private async Task ChargePlains()
    {
        // a Planície journey after the awakening: the prologue's book sealed, the charged shot found in the Vale
        var book = ValeState.StartingBook();
        book.Add(new BookEntry { Name = "Disparo de Ki Concentrado", Kind = "técnica", Tech = "ki_charge", Where = "Vale." });
        foreach (var b in book.Where(b => b.Tech.Length > 0)) b.State = "Selada";
        SaveGame.Write(SavePath, new SaveData { Region = "planicie", Phase = "planicie", Seed = 20261126, Spot = new[] { 16f, 16f }, Book = book, LossApplied = true,
            Inventory = { new SavedItem { Id = "faca_pedra", Count = 1 } }, Equipped = "knife", Health = 10, Vigor = 50, Ki = 100, Level = 2, Xp = 20 });
        _pg = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _pg.SavePath = SavePath; _pg.ShowIntro = false; _pg.Start = PlainsGame.StartMode.Continue;
        AddChild(_pg);
        await Frames(30);
        var p = _pg.Player; var shots = new List<Player.KiShot>(); p.KiFired += s => shots.Add(s);
        p.KiRegen = 0; p.Ki = 100;
        int msgs = _pg.Hud.Messages.Count;
        Press(Key.K, true); await Flush(); await Frames(60); Press(Key.K, false); await Flush(); await Frames(10);
        Check("sealed (Planície before level 3): holding the Ki button gathers nothing and shoots nothing", shots.Count == 0 && !p.Charging && p.Ki > 99 && _pg.Hud.Messages.Skip(msgs).Any(m => m.Contains("selad")), string.Join(" / ", _pg.Hud.Messages.Skip(msgs)));
        _pg.GainXp(_pg.Stats.XpMissing, "teste"); await Frames(5); // controlled: level 3
        Check("level 3: the charged shot comes back weakened with the others", _pg.Tech.State("ki_charge") == Combat.TechniqueState.Weakened);
        p.Ki = 100; p.Vigor = p.MaxVigor; shots.Clear();
        Press(Key.K, true); await Flush(); await Frames(110); Press(Key.K, false); await Flush(); await Frames(10);
        Check("the weakened full charge: one projectile, weakened damage (3), breaks", shots.Count == 1 && shots[0].Damage == 3 && shots[0].Breaks, shots.Count > 0 ? $"dano {shots[0].Damage}" : "nenhum");
        // next to a carcass the button skins (no charge): a dead rabbit's carcass
        var carcass = _pg.World.Objects.GetChildren().OfType<Carcass>().FirstOrDefault();
        if (carcass == null)
        {
            var rabbit = _pg.World.Objects.GetChildren().OfType<Rabbit>().FirstOrDefault();
            if (rabbit != null) { rabbit.ReceiveHit(99); await Frames(10); carcass = _pg.World.Objects.GetChildren().OfType<Carcass>().FirstOrDefault(); }
        }
        if (carcass != null)
        {
            p.GlobalPosition = carcass.GlobalPosition + new Vector2(-6, 2); await Frames(5);
            shots.Clear(); p.Ki = 100;
            Press(Key.K, true); await Flush(); await Frames(60); Press(Key.K, false); await Flush(); await Frames(10);
            Check("next to a carcass the Ki button skins with Ki (no charge, no shot)", shots.Count == 0 && !p.Charging && _pg.Hud.Messages.TakeLast(4).Any(m => m.Contains("Esfolar") || m.Contains("Pele")), $"Ki {p.Ki:0}, ação {_pg.KiKeyAction}, partes [{string.Join(",", carcass.Parts)}], msgs {string.Join(" / ", _pg.Hud.Messages.TakeLast(3))}");
        }
        else Check("a carcass to test the skinning", false, "nenhum coelho por perto");
        Finish();
    }

    // ------------------------------------------------------------------ 9. the Reforço de Ki
    private async Task Gesture(Key mod, Key cut, int frames)
    {
        Press(mod, true); await Flush(); await Frames(1);
        Press(cut, true); await Flush(); await Frames(frames);
        Press(cut, false); await Flush(); await Frames(1);
        Press(mod, false); await Flush(); await Frames(4);
    }

    private async Task ReinforcePhase()
    {
        await Frames(30);
        if (_c.Vale == null) { Check("Novo jogo starts the Vale", false); Finish(); return; }
        P.KiRegen = 0; // controlled: no regeneration while counting Ki
        int cuts = 0; P.ActionStarted += (k, _) => { if (k is Player.ActionKind.Cut or Player.ActionKind.Thrust) cuts++; };
        await Teleport(ValeMap.Start + new Vector2(0, -60), Vector2.Up);
        await PointAt(P.GlobalPosition + new Vector2(0, -100));

        // 75 Ki: the gesture (Shift + J held) activates once, spends 75, gives 3 charges, no cut during it
        P.Ki = 75; int msgs = V.Hud.Messages.Count;
        bool midGesture = false;
        Press(Key.Shift, true); await Flush(); await Frames(1);
        Press(Key.J, true); await Flush(); await Frames(10); midGesture = P.Reinforcing; await Shot("07-reforco-gesto");
        await Frames(50); // held well past the gesture: still one activation
        Press(Key.J, false); await Flush(); Press(Key.Shift, false); await Flush(); await Frames(5);
        Check("Shift + J with 75 Ki: the Ki runs up the blade and the Reforço activates (3 charges, 75 spent once)", midGesture && P.ReinforceCharges == 3 && P.Ki < 0.6f && cuts == 0, $"cargas {P.ReinforceCharges}, Ki {P.Ki:0.0}, cortes {cuts}");
        Check("holding the combination on did not activate it twice", V.Hud.Messages.Skip(msgs).Count(m => m.Contains("estão reforçados")) == 1);
        Check("the discovery: banner and a book page (command, cost, charges, bonus, chance)", V.Story.Book.Any(b => b.Tech == "ki_reinforce") && V.BookRows().First(b => b.Name == "Reforço de Ki").Detail.Contains("75 Ki") && V.BookRows().First(b => b.Name == "Reforço de Ki").Command.StartsWith("Shift + segurar J"), V.BookRows().First(b => b.Name == "Reforço de Ki").Command);
        Check("the HUD shows the charges (reforço 3) and the pips", V.Hud.KiText.Contains("reforço 3"), V.Hud.KiText);
        await Shot("08-reforco-cargas");
        // another try while charges remain: kept, nothing spent
        P.Ki = 100;
        await Gesture(Key.Shift, Key.J, 40);
        Check("another try with charges left keeps them and spends nothing", P.ReinforceCharges == 3 && P.Ki > 99.4f, $"cargas {P.ReinforceCharges}, Ki {P.Ki:0}");
        // 74: refused, the balance kept
        P.ReinforceCharges = 0; P.Ki = 74; msgs = V.Hud.Messages.Count;
        await Gesture(Key.Shift, Key.J, 40);
        Check("with 74 Ki: a clear refusal (falta Ki 74/75), the Ki stays 74, no charges", P.ReinforceCharges == 0 && Mathf.IsEqualApprox(P.Ki, 74, 0.01f) && V.Hud.Messages.Skip(msgs).Any(m => m.Contains("falta Ki (74/75)")), string.Join(" / ", V.Hud.Messages.Skip(msgs)));

        // three reinforced strikes (+10%) and the fourth normal, on the disciple's open flank (controlled: kept fooled)
        var dis = V.Fighters["dis_herbs"];
        async Task<float> FlankCut()
        {
            dis.Fooled = 5; dis.Face = Mathf.Pi / 2; dis.Stagger = 0; dis.State = ValeFighter.St.Guard;
            await Teleport(dis.GlobalPosition + new Vector2(36, 0), Vector2.Left);
            dis.Face = Mathf.Pi / 2;
            float before = dis.Hp;
            await PointAt(dis.GlobalPosition + new Vector2(0, -14));
            await Tap(Key.J); await Frames(30);
            return before - dis.Hp;
        }
        P.Ki = 75; await Gesture(Key.Shift, Key.J, 40);
        var hits = new List<float>();
        for (int i = 0; i < 4; i++) { hits.Add(await FlankCut()); dis.Hp = dis.Max; }
        Check("three reinforced strikes do 2.2 (sword 2 + 10%) and the fourth is normal (2)", hits.Count == 4 && Mathf.IsEqualApprox(hits[0], 2.2f, 0.01f) && Mathf.IsEqualApprox(hits[1], 2.2f, 0.01f) && Mathf.IsEqualApprox(hits[2], 2.2f, 0.01f) && Mathf.IsEqualApprox(hits[3], 2f, 0.01f) && P.ReinforceCharges == 0,
            $"danos {string.Join(" · ", hits.Select(h => h.ToString("0.##")))}");
        // the knife: 1 → 1.1 in the real damage (no rounding lost)
        P.Equipped = Player.Weapon.Knife;
        P.Ki = 75; await Gesture(Key.Shift, Key.J, 40);
        float knife = await FlankCut(); dis.Hp = dis.Max;
        Check("with the knife the bonus survives the arithmetic: 1 → 1.1 taken from the health (not rounded back to 1)", Mathf.IsEqualApprox(knife, 1.1f, 0.01f), $"dano {knife:0.###}");
        P.Equipped = Player.Weapon.Sword;
        // a miss spends a charge; a combo spends one per cut
        await Teleport(ValeMap.Start + new Vector2(0, -60), Vector2.Up);
        await PointAt(P.GlobalPosition + new Vector2(0, -100));
        int left = P.ReinforceCharges;
        await Tap(Key.J); await Frames(30);
        Check("a strike that hits nothing still spends a charge", P.ReinforceCharges == left - 1, $"{left} → {P.ReinforceCharges}");
        P.ReinforceCharges = 0; P.Ki = 75; await Gesture(Key.Shift, Key.J, 40);
        for (int i = 0; i < 3; i++) { await Tap(Key.J); await Frames(7); }
        await Frames(30);
        Check("each cut of a three-cut combo spends one charge", P.ReinforceCharges == 0, $"cargas {P.ReinforceCharges}");

        // the break: one roll per strike and guard; a fixed roll makes the success and the failure reproducible
        var bridge = V.Fighters["dis_bridge"];
        int rolls = 0;
        async Task<(bool Broke, float Dmg)> FrontCut(float roll)
        {
            P.ReinforceCharges = 0; P.Ki = 75; await Gesture(Key.Shift, Key.J, 40);
            P.ReinforceRoll = () => { rolls++; return roll; };
            bridge.ResetToHome(); await Frames(5);
            await Teleport(bridge.GlobalPosition + new Vector2(0, 40), Vector2.Up); bridge.Face = Mathf.Pi / 2; bridge.State = ValeFighter.St.Guard;
            float before = bridge.Hp; rolls = 0;
            await PointAt(bridge.GlobalPosition + new Vector2(0, -14));
            await Tap(Key.J); await Frames(8);
            return (bridge.Stagger > 0, before - bridge.Hp);
        }
        var ok = await FrontCut(0.01f);
        int rollsOk = rolls;
        await Shot("09-reforco-ruptura");
        Check("a roll under 5% (fixed 0.01): the reinforced cut breaks the Closed Guard (2.2 damage, open)", ok.Broke && Mathf.IsEqualApprox(ok.Dmg, 2.2f, 0.01f), $"rompeu {ok.Broke}, dano {ok.Dmg:0.##}");
        Check("one roll for that strike, however many frames the blade touched", rollsOk == 1, $"{rollsOk} rolagem(ns)");
        await Frames(60);
        var fail = await FrontCut(0.99f);
        Check("a roll over 5% (fixed 0.99): parried as usual (no damage, no break)", !fail.Broke && fail.Dmg < 0.01f && rolls == 1, $"rompeu {fail.Broke}, dano {fail.Dmg:0.##}, rolagens {rolls}");
        P.ReinforceRoll = () => 0.5f;
        bridge.ResetToHome();

        // Shift alone sneaks; the step still works
        await Teleport(ValeMap.Start + new Vector2(0, -60), Vector2.Up);
        P.ReinforceCharges = 0;
        float x0 = P.GlobalPosition.X;
        Press(Key.Shift, true); Press(Key.D, true); await Flush(); await Frames(20);
        bool quiet = P.Noise == Player.NoiseLevel.Sneak, moved = P.GlobalPosition.X > x0 + 3;
        Press(Key.D, false); Press(Key.Shift, false); await Flush(); await Frames(4);
        int steps = 0; P.ActionStarted += (k, _) => { if (k == Player.ActionKind.Step) steps++; };
        await Tap(Key.L); await Frames(20);
        Check("Shift alone still walks quietly and L still steps", quiet && moved && steps == 1 && P.ReinforceCharges == 0, $"quieto {quiet}, passos {steps}");
        // a remapped cut (U) and a remapped modifier (Ctrl)
        Bindings.Assign("cut", 0, $"k:{(long)Key.U}");
        Bindings.Assign("reinforce", 0, $"k:{(long)Key.Ctrl}");
        P.Ki = 75;
        await Gesture(Key.Ctrl, Key.U, 40);
        Check("with the cut on U and the modifier on Ctrl, Ctrl + U activates it", P.ReinforceCharges == 3, $"cargas {P.ReinforceCharges}");
        Check("the book shows the remapped command (Ctrl + segurar U)", V.BookRows().First(b => b.Name == "Reforço de Ki").Command.StartsWith("Ctrl + segurar U"), V.BookRows().First(b => b.Name == "Reforço de Ki").Command);
        Bindings.Restore();
        // meditating keeps the discovery
        await Teleport(ValeMap.Meditate + new Vector2(0, 8), Vector2.Up);
        await Tap(Key.E); await Frames(3);
        Check("meditating keeps the Reforço in the book", SaveGame.Read(SavePath).Data?.Vale?.Book.Any(b => b.Tech == "ki_reinforce") == true);
        Finish();
    }

    // ------------------------------------------------------------------ 10. the bag with pictures
    private PlainsGame StartPlains(StartModeArg how)
    {
        var g = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        g.Seed = 20261126; g.SavePath = SavePath; g.ShowIntro = false; g.Start = how == StartModeArg.New ? PlainsGame.StartMode.New : PlainsGame.StartMode.Continue;
        AddChild(g);
        return g;
    }

    private enum StartModeArg { New, Continue }

    private async Task ClickWin(Vector2 at)
    {
        Input.ParseInputEvent(new InputEventMouseMotion { Position = at, GlobalPosition = at }); await Flush(); await Frames(2);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = at, GlobalPosition = at }); await Flush(); await Frames(2);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false, Position = at, GlobalPosition = at }); await Flush(); await Frames(4);
    }

    private async Task BagPhase()
    {
        _pg = StartPlains(StartModeArg.New);
        await Frames(30);
        var g = _pg; var hud = g.Hud;
        // what the swordsman carries after a little gathering (controlled: given at once)
        g.Inventory.Add("silex", 2); g.Inventory.Add("galho_rijo", 2); g.Inventory.Add("grama_sangue_q1", 2);
        g.Inventory.Add("carne_macia", 1); g.Inventory.Add("carne_macia_assada", 1); g.Inventory.Add("pele_comum", 2);
        await Tap(Key.I);
        Check("I opens the bag (the picture panel)", hud.InventoryOpen && hud.Bag.Visible);
        var items = hud.Rows.Where(r => !r.IsRecipe).ToList();
        Check("every item has its picture and its stack count (flint 2, branch 2, herb 2, meat 1, hide 2)",
            items.All(r => r.Icon.Length > 0) && items.First(r => r.Id == "silex").Count == 2 && items.First(r => r.Id == "carne_macia").Count == 1 && hud.Bag.SlotRects.Count == hud.Rows.Count,
            string.Join(", ", items.Select(r => $"{r.Id}×{r.Count}")));
        await Shot("10-bolsa-preenchida");
        // the knife by keyboard: walk the selection to the recipe and press E
        for (int k = 0; k <= hud.Rows.Count && !(hud.Rows[hud.Selected].IsRecipe && hud.Rows[hud.Selected].Id == "faca_pedra"); k++) await Tap(Key.D);
        string detail = hud.Rows[hud.Selected].Detail;
        Check("the knife's recipe shows what it needs and that it can be made (Fabricar)", hud.Rows[hud.Selected].Action == "Fabricar" && hud.Rows[hud.Selected].Enabled && detail.Contains("Sílex 2/1"), detail);
        await Tap(Key.E); await Frames(3);
        Check("crafting by keyboard: flint and branch go down, the knife appears in the same bag", g.Inventory.Count("silex") == 1 && g.Inventory.Count("galho_rijo") == 1 && hud.Rows.Any(r => !r.IsRecipe && r.Id == "faca_pedra" && r.Icon == "faca_pedra"),
            $"sílex {g.Inventory.Count("silex")}, galho {g.Inventory.Count("galho_rijo")}");
        await Shot("11-bolsa-faca-fabricada");
        // equip the knife by a click on its slot
        int knife = hud.Rows.FindIndex(r => !r.IsRecipe && r.Id == "faca_pedra");
        await Frames(2);
        var rect = hud.RowScreenRect("inv", knife);
        if (rect is { } rr) await ClickWin(rr.GetCenter());
        knife = hud.Rows.FindIndex(r => !r.IsRecipe && r.Id == "faca_pedra");
        Check("a click on the knife equips it, and the bag marks it in use", g.Player.Equipped == Player.Weapon.Knife && hud.Rows[knife].Equipped, $"{g.Player.Equipped}");
        // a herb by keyboard: find it and use it
        g.Player.Heal(-6); await Frames(2);
        float hp = g.Player.Health;
        for (int k = 0; k <= hud.Rows.Count && hud.Rows[hud.Selected].Id != "grama_sangue_q1"; k++) await Tap(Key.D);
        Check("the herb's slot offers 'Usar'", hud.Rows[hud.Selected].Action == "Usar", hud.Rows[hud.Selected].Action);
        await Tap(Key.E); await Frames(3);
        Check("using the herb by keyboard: one herb fewer, the healing starts", g.Inventory.Count("grama_sangue_q1") == 1 && (g.Player.PendingRegen > 0 || g.Player.Health > hp), $"ervas {g.Inventory.Count("grama_sangue_q1")}");
        // and the other by the action button
        await Frames(2);
        int herb = hud.Rows.FindIndex(r => r.Id == "grama_sangue_q1");
        if (herb >= 0)
        {
            var hr = hud.RowScreenRect("inv", herb);
            if (hr is { } h) { Input.ParseInputEvent(new InputEventMouseMotion { Position = h.GetCenter(), GlobalPosition = h.GetCenter() }); await Flush(); await Frames(3); }
            var ft = GetViewport().GetFinalTransform(); var gt = hud.Bag.GetGlobalTransformWithCanvas();
            Vector2 btn = ft * (gt * hud.Bag.ActionRect.GetCenter());
            await ClickWin(btn);
        }
        Check("the action button (mouse) uses the other herb", g.Inventory.Count("grama_sangue_q1") == 0, $"ervas {g.Inventory.Count("grama_sangue_q1")}");
        Check("the roasted meat offers 'Comer', the raw one says to roast it, a hide is a material", hud.Rows.First(r => r.Id == "carne_macia_assada").Action == "Comer" && hud.Rows.First(r => r.Id == "carne_macia").Detail.Contains("asse na fogueira") && hud.Rows.First(r => r.Id == "pele_comum").Detail.Contains("fabricar"));
        // full screen: the bag still fits the screen
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen); await Frames(40);
        var bagRect = new Rect2(hud.Bag.Position, hud.Bag.Size);
        Check("full screen: the bag fits the screen with its slots and detail", bagRect.End.X <= hud.LaidOut.X && bagRect.End.Y <= hud.LaidOut.Y && hud.Bag.SlotRects.Count == hud.Rows.Count, $"bolsa {bagRect}, tela {hud.LaidOut}");
        await Shot("12-bolsa-tela-cheia");
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed); DisplayServer.WindowSetSize(new Vector2I(1280, 720)); await Frames(20);
        await Tap(Key.I);
        Check("I closes the bag", !hud.InventoryOpen);
        Finish();
    }

    // ------------------------------------------------------------------ 11. money
    private Pickup? Pouch(PlainsGame g) => g.World.Objects.GetChildren().OfType<Pickup>().FirstOrDefault(p => p.ItemId == "moedas");

    private async Task MoneyPhase()
    {
        _pg = StartPlains(StartModeArg.New);
        await Frames(30);
        var g = _pg;
        Check("a new journey starts with no money (0 coins)", g.Wallet.Coins == 0);
        var mat = g.World.Objects.GetChildren().OfType<MeditationMat>().First();
        var pouch = Pouch(g);
        Check("the coin pouch lies by the meditation mat, two steps east", pouch != null && pouch.GlobalPosition.DistanceTo(mat.GlobalPosition) < 90, pouch == null ? "não achada" : $"a {pouch.GlobalPosition.DistanceTo(mat.GlobalPosition):0} px da esteira");
        if (pouch == null) { Finish(); return; }
        g.Player.GlobalPosition = pouch.GlobalPosition + new Vector2(-8, 4); g.Player.GetNode<Camera2D>("Camera").ResetSmoothing(); await Frames(6);
        Check("next to it the prompt says so", g.Hud.PromptText.Contains("moedas"), g.Hud.PromptText);
        await Shot("13-bolsinha-de-moedas");
        int m = g.Hud.Messages.Count;
        await Tap(Key.E); await Frames(3);
        Check("E: +25 moedas, the balance updates at once", g.Wallet.Coins == 25 && g.Hud.Messages.Skip(m).Any(x => x.Contains("+25 moedas")) && Pouch(g) == null, string.Join(" / ", g.Hud.Messages.Skip(m)));
        await Tap(Key.I);
        Check("the bag shows the balance with its picture (Moedas: 25)", g.Hud.Bag.Money == 25);
        await Shot("14-bolsa-com-moedas");
        await Tap(Key.I);
        // leave the chunk and come back
        Vector2 back = g.Player.GlobalPosition;
        g.Player.GlobalPosition += new Vector2(-2200, 0); await Frames(20);
        g.Player.GlobalPosition = back; await Frames(20);
        Check("leaving the chunk and coming back: the pouch stays taken", Pouch(g) == null && g.Wallet.Coins == 25);
        // receive and spend (the operations merchants will use)
        g.ReceiveCoins(5, "teste"); bool paid = g.SpendCoins(12, "teste"); m = g.Hud.Messages.Count; bool refused = !g.SpendCoins(100, "teste");
        Check("receive 5 and spend 12 (25 → 18); spending 100 is refused and keeps 18", paid && refused && g.Wallet.Coins == 18 && g.Hud.Messages.Skip(m).Any(x => x.Contains("insuficientes")), $"saldo {g.Wallet.Coins}");
        // meditate: saved (the mat is found again: leaving the chunk unloaded the old node)
        mat = g.World.Objects.GetChildren().OfType<MeditationMat>().First();
        g.Player.GlobalPosition = mat.GlobalPosition + new Vector2(0, 10); await Frames(5);
        await Tap(Key.E); await Frames(3);
        var d = SaveGame.Read(SavePath).Data;
        Check("meditating saves the balance and the taken pouch", d != null && d.Coins == 18 && d.Collected.Any(c => c.StartsWith("coinpouch")), d == null ? "sem save" : $"moedas {d.Coins}");
        Finish();
    }

    private async Task MoneyResume()
    {
        _pg = StartPlains(StartModeArg.Continue);
        await Frames(30);
        Check("Continuar: the balance (18) and the taken pouch are back", _pg.Wallet.Coins == 18 && Pouch(_pg) == null, $"moedas {_pg.Wallet.Coins}");
        Finish();
    }

    private async Task OldSavePhase()
    {
        // a journey written before this delivery: the same JSON without "coins"
        var old = new SaveData { Region = "planicie", Phase = "planicie", Seed = 20261126, Spot = new[] { 16f, 16f }, LossApplied = true,
            Inventory = { new SavedItem { Id = "faca_pedra", Count = 1 }, new SavedItem { Id = "grama_sangue_q1", Count = 3 } }, Equipped = "knife", Health = 8, Vigor = 50, Ki = 100, Level = 2, Xp = 5 };
        SaveGame.Write(SavePath, old);
        string text = Godot.FileAccess.GetFileAsString(SavePath);
        var json = System.Text.Json.Nodes.JsonNode.Parse(text)!.AsObject();
        json.Remove("coins");
        using (var f = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write)) f.StoreString(json.ToJsonString());
        Check("the old save really has no coins field", !Godot.FileAccess.GetFileAsString(SavePath).Contains("\"coins\""));
        _pg = StartPlains(StartModeArg.Continue);
        await Frames(30);
        var g = _pg;
        Check("it loads: items, quantities and the equipment kept, 0 coins", g.Inventory.Count("faca_pedra") == 1 && g.Inventory.Count("grama_sangue_q1") == 3 && g.Player.Equipped == Player.Weapon.Knife && g.Wallet.Coins == 0, $"moedas {g.Wallet.Coins}");
        Check("the pouch is still there to be found", Pouch(g) != null);
        g.QueueFree(); await Frames(5);
        // the awakening: the swordsman wakes without money (the prologue's silver is lost with everything else)
        var vale = new SaveData { Region = "vale", Phase = "senior_won", Seed = 0, Spot = new[] { 100f, 100f }, Vale = new ValeState { Book = ValeState.StartingBook() },
            ValeInventory = { new SavedItem { Id = "prata", Count = 13 } }, Equipped = "sword", Health = 10, Ki = 100 };
        _pg = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _pg.Seed = 20261126; _pg.SavePath = SavePath; _pg.ShowIntro = false; _pg.Start = PlainsGame.StartMode.Awaken; _pg.AwakenFrom = vale;
        AddChild(_pg);
        await Frames(40);
        Check("the awakening in the Planície starts with 0 coins", _pg.Wallet.Coins == 0 && _pg.Inventory.Count("prata") == 0, $"moedas {_pg.Wallet.Coins}");
        Finish();
    }
}
