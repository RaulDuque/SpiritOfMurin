using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Campaign;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Combat;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;
using SpiritOfMurin.Vale;

namespace SpiritOfMurin.Tests;

/// <summary>
/// The campaign's prologue and its ending, through the main scene (Campaign.tscn) and real keys.
/// Phases, each its own run (process):
/// <list type="bullet">
/// <item><c>--phase=prologue</c>: Novo jogo → the Vale; the Closed Guard parries from the front; the manual,
/// the seal, the cut and the footprints teach the side step; the statue answers a flank strike; the long
/// step crosses the gap; the inscription; the Silent Blade Step; the guardian falls and the gate opens;
/// meditating writes the journey (copied aside); the Senior falls for real; the ending (victory → revival
/// → the line, advanced with E → the charge → the explosion throws the swordsman → darkness and silence);
/// the awakening in the Planície with empty hands, a sealed book, and the journey moved there once.</item>
/// <item><c>--phase=resume-vale</c>: Continuar from the meditation in the Vale restores the story.</item>
/// <item><c>--phase=resume-ending</c>: a journey closed during the ending resumes at the awakening; the loss
/// happens once (Continuar again stays in the Planície with what was gained after).</item>
/// <item><c>--phase=old-save</c>: a format-1 save (the Planície before the campaign) continues in the Planície
/// with its items, attributes and proficiencies.</item>
/// <item><c>--phase=skip</c>: with the ending already seen, Esc skips it.</item>
/// </list>
/// Teleports place the swordsman; the controlled parts (healing during the Senior fight) are named in the
/// check. Files: <c>user://teste_vale*.json</c>, preferences in <c>user://teste_vale_prefs.json</c>
/// (pass <c>--prefs=user://teste_vale_prefs.json</c>).
/// </summary>
public partial class ValeCheck : Node
{
    private const string SavePath = "user://teste_vale.json", Meditated = "user://teste_vale_meditou.json", Ending = "user://teste_vale_fim.json";
    private CampaignRoot _c = null!;
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private string _phase = "prologue";
    private string? _shots;
    private int _slow; // --lento: extra frames after each move, so a person can follow the run

    /// <inheritdoc />
    public override void _Ready()
    {
        Game.Aim.UseForSession(false); // written for aiming by facing: the real cursor over the window must not steer the cuts
        foreach (string a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--phase=")) _phase = a["--phase=".Length..];
            if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..];
            if (a == "--lento") _slow = 50;
        }
        GD.Print($"window on screen {DisplayServer.WindowGetCurrentScreen()} of {DisplayServer.GetScreenCount()} ({DisplayServer.GetName()})");
        switch (_phase)
        {
            case "prologue":
                SaveGame.Erase(SavePath); SaveGame.Erase(Prefs.Path);
                Launch("title"); _ = Prologue(); break;
            case "resume-vale": Copy(Meditated, SavePath); Launch("title"); _ = ResumeVale(); break;
            case "resume-ending": Copy(Ending, SavePath); Launch("continue"); _ = ResumeEnding(); break;
            case "old-save": WriteOldSave(); Launch("continue"); _ = OldSave(); break;
            case "skip": Copy(Meditated, SavePath); Launch("continue"); _ = Skip(); break;
            case "ending": Copy(Meditated, SavePath); Launch("continue"); _ = EndingClip(); break;
        }
    }

    private void Launch(string how)
    {
        _c = GD.Load<PackedScene>("res://src/Campaign/Campaign.tscn").Instantiate<CampaignRoot>();
        _c.SavePath = SavePath; _c.ShowIntro = false; _c.StartWith = how;
        AddChild(_c);
    }

    private static void Copy(string from, string to)
    {
        string text = Godot.FileAccess.GetFileAsString(from);
        using var f = Godot.FileAccess.Open(to, Godot.FileAccess.ModeFlags.Write); f.StoreString(text);
    }

    // ------------------------------------------------------------------ helpers
    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private async Task Flush() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static void Press(Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });
    private async Task Tap(Key k) { Press(k, true); await Flush(); await Frames(2); Press(k, false); await Flush(); await Frames(2 + _slow / 3); }
    private async Task Chord(Key a, Key b) { Press(a, true); await Flush(); await Frames(1); Press(b, true); await Flush(); await Frames(2); Press(b, false); Press(a, false); await Flush(); await Frames(2); }
    private ValeGame V => _c.Vale!;
    private Player P => V.Player;

    private async Task Shot(string name)
    {
        if (_shots == null || DisplayServer.GetName() == "headless") return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        DirAccess.MakeDirRecursiveAbsolute(_shots);
        GetViewport().GetTexture().GetImage().SavePng($"{_shots}/{name}.png");
    }

    private async Task Teleport(Vector2 at, Vector2 face)
    {
        P.GlobalPosition = at; P.Face(face); await Frames(3 + _slow);
    }

    private async Task CloseStory() { for (int i = 0; i < 3 && V.Hud.StoryOpen; i++) await Tap(Key.E); }

    // locks on a fighter (Space), releasing any other lock first
    private async Task LockOn(ValeFighter f)
    {
        if (V.Lock == f) return;
        if (V.Lock != null) await Tap(Key.Space);
        for (int i = 0; i < 3 && V.Lock != f; i++) { await Tap(Key.Space); if (V.Lock != f && V.Lock != null) await Tap(Key.Space); }
    }

    // side step (L + the key that is lateral to the target), then cut (J), then ki (K): the Silent Blade Step
    private async Task<bool> Lamina(ValeFighter f)
    {
        Vector2 to = f.GlobalPosition - P.GlobalPosition;
        Key lat = Mathf.Abs(to.X) > Mathf.Abs(to.Y) ? Key.W : Key.A;
        bool before = V.Story.ComboFound;
        float hp = f.Hp;
        await Chord(lat, Key.L);
        await Frames(4);
        await Tap(Key.J); await Frames(4);
        await Tap(Key.K); await Frames(20);
        return V.Story.ComboFound && (f.Hp < hp || !f.Alive || f.Stagger > 0);
    }

    // ------------------------------------------------------------------ the prologue
    private async Task Prologue()
    {
        await Frames(10);
        Check("the title shows only Novo jogo when there is no journey", _c.TitleHud.TitleOpen && _c.TitleHud.TitleRows.Count == 1 && _c.TitleHud.TitleRows[0].Id == "new");
        await Tap(Key.E); // the normal flow: Novo jogo from the title
        await Frames(20);
        Check("Novo jogo opens the prologue in the Vale", _c.Vale != null && _c.Plains == null && P.Equipped == Player.Weapon.Sword && P.GlobalPosition.DistanceTo(ValeMap.Start) < 4, $"{P.GlobalPosition}");
        Check("the book starts with the swordsman's art (cut, thrust, ki shot, short step)", V.Story.Book.Count == 4 && V.Tech.State("ki_wave") == TechniqueState.Full && V.Tech.State("side_step") == TechniqueState.Unknown, string.Join(", ", V.Story.Book.Select(b => b.Name)));
        await Shot("01-vale-inicio");

        // the bridge guardian parries from the front
        var bridge = V.Fighters["dis_bridge"];
        await Teleport(bridge.GlobalPosition + new Vector2(0, 60), Vector2.Up);
        await Frames(30);
        await LockOn(bridge);
        Check("Space locks on the guardian", V.Lock == bridge);
        int words = V.Fx.WordsShown.Count(w => w == "aparado"); float hp0 = bridge.Hp;
        for (int i = 0; i < 3; i++) { await Teleport(bridge.GlobalPosition + new Vector2(0, 40), Vector2.Up); await Tap(Key.J); await Frames(16); }
        Check("from the front, the Closed Guard parries (\"aparado\", no damage)", V.Fx.WordsShown.Count(w => w == "aparado") > words && bridge.Hp == hp0, $"hp {bridge.Hp}/{bridge.Max}");
        await Shot("02-guarda-apara");
        await Tap(Key.Space);
        P.Revive();

        // the manual on the body in the bamboo valley
        await Teleport(ValeMap.Corpse + new Vector2(-20, 0), Vector2.Right);
        await Frames(40);
        await Tap(Key.E);
        Check("E on the body: half a manual (panel, book, mission)", V.Story.HasManual && V.Hud.StoryOpen && V.Hud.StoryText.Contains("manual") && V.Story.Book.Any(b => b.Name == "Metade de um manual"), V.Hud.StoryText.Split('\n')[0]);
        await Shot("03-manual");
        await CloseStory();
        // the seal, the cut in the stone, the footprints
        await Teleport(ValeMap.Door + new Vector2(0, 20), Vector2.Up);
        await Tap(Key.E);
        Check("the manual completes the seal: the tomb opens", V.Story.DoorOpen, V.Hud.StoryText.Split('\n')[0]);
        await CloseStory();
        await Teleport(ValeMap.Slab + new Vector2(0, 16), Vector2.Up);
        await Tap(Key.E);
        Check("the cut in the stone and the footprints light up", V.Story.SlabRead && V.Hud.StoryText.Contains("Pise onde ela pisava"));
        await CloseStory();
        foreach (var m in ValeMap.Marks) { await Teleport(m, Vector2.Up); await Frames(4); }
        await Frames(10);
        Check("walking the footprints in order teaches the side step (and the long step)", V.Story.LearnedSide && V.Tech.State("side_step") == TechniqueState.Full && V.Tech.State("dash") == TechniqueState.Full && V.Story.Book.Any(b => b.Name == "Passo Lateral"), $"fp {V.Story.FpIdx}");
        await Shot("04-passo-lateral");
        await Frames(220);
        // the statue answers a flank strike
        var statue = V.Fighters["statue"];
        await Teleport(statue.GlobalPosition + new Vector2(0, 60), Vector2.Up);
        await LockOn(statue);
        int flank = V.Fx.WordsShown.Count(w => w == "flanco");
        await Chord(Key.A, Key.L); await Frames(6);
        await Tap(Key.J); await Frames(16);
        Check("side step around the statue, then a cut: the stone answers the flank", V.Fx.WordsShown.Count(w => w == "flanco") > flank, $"words {string.Join(",", V.Fx.WordsShown.TakeLast(3))}");
        await Tap(Key.Space);

        // the long step crosses the gap; the inscription
        await Teleport(new Vector2(23.6f * ValeMap.TS, 33 * ValeMap.TS), Vector2.Left);
        float x0 = P.GlobalPosition.X;
        await Chord(Key.A, Key.L); await Frames(20);
        Check("the long step (direction + L, no lock) crosses the gap", P.GlobalPosition.X < 21 * ValeMap.TS && V.Map.AtPixel(P.GlobalPosition) != VTile.Chasm, $"x {x0:0} → {P.GlobalPosition.X:0}");
        await Teleport(ValeMap.Stele + new Vector2(0, 18), Vector2.Up);
        await Tap(Key.E);
        Check("the inscription beyond the gap", V.Story.SteleRead && V.Story.Book.Any(b => b.Name == "Inscrição da fenda"));
        await CloseStory();
        await Teleport(new Vector2(20.2f * ValeMap.TS, 33 * ValeMap.TS), Vector2.Right);
        await Chord(Key.D, Key.L); await Frames(20);

        // the Silent Blade Step on the guardian, until he falls
        P.Revive(); P.Ki = P.MaxKi;
        bool found = false;
        for (int round = 0; round < 12 && bridge.Alive; round++)
        {
            P.Revive(); P.Ki = P.MaxKi; // controlled: the test keeps the swordsman standing
            await Teleport(bridge.GlobalPosition + new Vector2(0, 52), Vector2.Up);
            await LockOn(bridge);
            if (await Lamina(bridge)) found = true;
            await Frames(10);
            if (bridge.Alive && bridge.Stagger > 0) { await Tap(Key.J); await Frames(12); }
        }
        Check("side step → cut → ki: the Silent Blade Step breaks the guard (book, combination)", found && V.Story.ComboFound && V.Story.Book.Any(b => b.Name == "Passo da Lâmina Silenciosa"), $"guardian hp {bridge.Hp}");
        Check("the guardian falls; the camp gate opens", !bridge.Alive && V.Story.GateOpen && V.Story.Defeated.Contains("dis_bridge"));
        await Shot("05-portao-aberto");
        await Frames(40);

        // the map: M shows what was seen; X puts a marker; M closes
        await Tap(Key.M);
        Check("M opens the Vale's map (seen places only)", V.Hud.MapOpen && V.Seen(29, 34) && !V.Seen(2, 38), $"seen start {V.Seen(29, 34)}");
        await Shot("05b-mapa-vale");
        await Tap(Key.X);
        await Tap(Key.M);
        Check("a marker is placed on the map", V.MapMarkers.Count == 1 && !V.Hud.MapOpen);
        // meditate by the twin stones
        await Teleport(ValeMap.Meditate + new Vector2(0, 6), Vector2.Up);
        await Tap(Key.E);
        var (st, med) = SaveGame.Read(SavePath);
        Check("meditating writes the journey in the Vale", st == SaveStatus.Ok && med!.Region == "vale" && med.Phase == "vale" && med.Vale!.LearnedSide, $"{st}");
        Copy(SavePath, Meditated);

        // the Senior, for real
        var sen = V.Fighters["senior"];
        int rounds = 0; float minHp = sen.Hp;
        for (; rounds < 40 && sen.Alive; rounds++)
        {
            if (P.Health < 6) P.Revive(); // controlled: the test heals the swordsman between exchanges
            await Teleport(sen.GlobalPosition + new Vector2(0, 54), Vector2.Up);
            await LockOn(sen);
            await Lamina(sen);
            if (sen.Alive && sen.Stagger > 0) { await Tap(Key.J); await Frames(8); await Tap(Key.J); await Frames(12); }
            minHp = Mathf.Min(minHp, sen.Hp);
            await Frames(8);
        }
        Check("the Senior Swordsman is beaten for real (his health reaches 0 through the fight)", !sen.Alive || V.Finale != null, $"{rounds} rounds, hp {minHp}");
        var fin = V.Finale;
        Check("victory: the fight ends, the player sees it, the victory is written (safe point)", fin != null && fin.Saved && V.Hud.BannerText == "Vitória" && SaveGame.Read(SavePath).Data?.Phase == "senior_won", fin == null ? "no finale" : $"{fin.Current}");
        Copy(SavePath, Ending);
        await Shot("06-vitoria");
        for (int i = 0; i < 400 && fin != null && fin.Current < SeniorFinale.Stage.Speech; i++)
        {
            await Frames(1);
            if (fin.Current == SeniorFinale.Stage.Revive && fin.T > 1.4f && fin.T < 1.45f) await Shot("07-revive-aura");
        }
        Check("after a pause the Senior rises; his aura grows (new stance)", fin!.Seen.Contains(SeniorFinale.Stage.Revive) && sen.Scripted != null && sen.AuraPower > 0.5f, $"aura {sen.AuraPower:0.00}");
        Check("he faces the swordsman and says the line", fin.Current == SeniorFinale.Stage.Speech && V.Hud.SpeechText == SeniorFinale.Line, V.Hud.SpeechText);
        await Shot("08-fala");
        await Frames(30);
        await Tap(Key.E);
        Check("E advances the line: he draws the energy into his body", fin.Current == SeniorFinale.Stage.Charge);
        Vector2 pBefore = P.GlobalPosition;
        for (int i = 0; i < 300 && fin.Current < SeniorFinale.Stage.Explode; i++) await Frames(1);
        await Frames(8); await Shot("09-explosao");
        for (int i = 0; i < 200 && fin.Current < SeniorFinale.Stage.Dark; i++) await Frames(1);
        Check("he blows himself up: the shock wave throws the swordsman far (spinning through the air)", fin.Thrown > 400 && sen.Gone, $"thrown {fin.Thrown:0} px");
        Check("the swordsman is thrown out of the picture (the camera stays at the blast)", fin.LeftScreen, $"left the screen: {fin.LeftScreen}");
        for (int i = 0; i < 120 && fin.Current == SeniorFinale.Stage.Dark && fin.T < 1.2f; i++) await Frames(1);
        Check("darkness and silence", fin.Current == SeniorFinale.Stage.Dark && V.Hud.OverlayAlpha > 0.95f && V.Sfx.Master < 0.6f, $"overlay {V.Hud.OverlayAlpha:0.00}, master {V.Sfx.Master:0.00}");
        for (int i = 0; i < 400 && _c.Plains == null; i++) await Frames(1);
        await Frames(10);
        var pl = _c.Plains;
        Check("the awakening: the Planície, with empty hands, no items", pl != null && pl.Started && pl.Player.Equipped == Player.Weapon.None && pl.Inventory.Entries.Count == 0 && _c.Vale == null, pl == null ? "no plains" : $"{pl.Player.Equipped}, {pl.Inventory.Entries.Count} items");
        Check("the book carries what was learned, now sealed", pl != null && pl.Book.Where(b => b.Tech.Length > 0).All(b => b.State == "Selada") && pl.Book.Any(b => b.Name == "Passo Lateral") && pl.Tech.State("ki_wave") == TechniqueState.Sealed, pl == null ? "" : string.Join(", ", pl.Book.Select(b => $"{b.Name}:{b.State}")));
        var after = SaveGame.Read(SavePath).Data;
        Check("the change of region is written once (Continuar is now in the Planície)", after != null && after.Region == "planicie" && after.LossApplied && after.Book.Count > 0, after == null ? "" : $"{after.Region}/{after.Phase}");
        Check("light: a pale glare clearing, the sounds coming back", pl != null && pl.Hud.OverlayAlpha > 0.3f && pl.Sfx != null, pl == null ? "" : $"overlay {pl.Hud.OverlayAlpha:0.00}");
        await Frames(30); await Shot("10-despertar-claridade");
        await Frames(200); await Shot("11-planicie-maos-vazias");
        if (pl != null)
        {
            Check("in the Planície he wakes lying on the ground", pl.LyingDown && pl.Player.Lying >= 1);
            Press(Key.D, true); await Flush(); await Frames(80); Press(Key.D, false); await Flush(); await Frames(10);
            Check("moving makes him get up (then he walks)", !pl.LyingDown, $"lying {pl.Player.Lying:0.00}");
            await Shot("12-levantou");
            // a sealed technique cannot be used: K away from a carcass, L to the side of a focused animal
            float ki = pl.Player.Ki;
            await Tap(Key.K); await Frames(20);
            Check("K: the ki shot is sealed (refused, no Ki spent)", pl.Hud.Messages.Any(m => m.Contains("Disparo de Ki está selada")) && pl.Player.Ki >= ki - 0.01f, pl.Hud.Messages.LastOrDefault() ?? "");
        }
        Finish();
    }

    private async Task ResumeVale()
    {
        await Frames(10);
        Check("the title offers Continuar (Vale de Bambus) first", _c.TitleHud.TitleOpen && _c.TitleHud.TitleRows[0].Id == "continue" && _c.TitleHud.TitleRows[0].Text.Contains("Vale de Bambus"), _c.TitleHud.TitleRows.FirstOrDefault().Text ?? "");
        await Tap(Key.E); // Continuar
        await Frames(20);
        Check("Continuar from the meditation in the Vale", _c.Vale != null && V.Story.HasManual && V.Story.DoorOpen && V.Story.LearnedSide && V.Story.GateOpen && !V.Fighters["dis_bridge"].Alive && V.Fighters["senior"].Alive, $"{P.GlobalPosition}");
        Check("the techniques come back as they were (side step full)", V.Tech.State("side_step") == TechniqueState.Full && V.Tech.State("lamina") == TechniqueState.Full);
        Check("by the twin stones", P.GlobalPosition.DistanceTo(ValeMap.Meditate) < 20);
        Check("the map's marker and the seen places come back", V.MapMarkers.Count == 1 && V.Seen(29, 34), $"{V.MapMarkers.Count} marker(s)");
        Finish();
    }

    private async Task ResumeEnding()
    {
        await Frames(30);
        var pl = _c.Plains;
        Check("a journey closed during the ending resumes at the awakening", _c.Vale == null && pl != null && pl.Started && pl.LossApplied && pl.Player.Equipped == Player.Weapon.None && pl.Book.Any(b => b.State == "Selada"), pl == null ? "no plains" : $"{pl.Book.Count} pages");
        // gain something, meditate, continue again: the loss does not happen twice
        if (pl != null)
        {
            pl.Inventory.Add("silex", 2);
            var mat = pl.World.Objects.GetChildren().OfType<MeditationMat>().First();
            pl.Player.GlobalPosition = mat.GlobalPosition + new Vector2(0, 10); await Frames(3);
            await Tap(Key.E);
            var d = SaveGame.Read(SavePath).Data!;
            Check("after meditating there, the journey is a Planície journey (loss applied once, items kept)", d.Region == "planicie" && d.LossApplied && d.Inventory.Any(i => i.Id == "silex" && i.Count == 2), $"{d.Region}");
        }
        Finish();
    }

    private void WriteOldSave()
    {
        // a format-1 journey, as the Planície wrote it before the campaign
        string json = "{\"game\":\"spiritofmurin\",\"format\":1,\"region\":\"planicie\",\"saved_at\":\"2026-11-26T15:00:00\",\"seed\":777,\"spot\":[48,-32],"
            + "\"collected\":[],\"dead\":{},\"parts_taken\":[],\"trails\":[],\"inventory\":[{\"id\":\"faca_pedra\",\"count\":1},{\"id\":\"pele_fina\",\"count\":2}],"
            + "\"equipped\":\"knife\",\"health\":9,\"vigor\":40,\"ki\":80,\"level\":3,\"xp\":5,\"points\":1,\"total_xp\":60,\"attributes\":{\"vit\":2,\"agi\":1},"
            + "\"practice\":{\"esfolar\":4,\"rastreamento\":2},\"practice_keys\":[\"esfolar:rabbit@1,1\"],\"crafted\":[\"faca_pedra\"]}";
        using var f = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write); f.StoreString(json);
    }

    private async Task OldSave()
    {
        await Frames(20);
        var pl = _c.Plains;
        Check("a format-1 Planície journey continues in the Planície", _c.Vale == null && pl != null && pl.Started && pl.SeedInUse == 777, pl == null ? "no plains" : $"seed {pl.SeedInUse}");
        if (pl != null)
            Check("with its items, equipment, level, attributes and proficiencies", pl.Inventory.Count("pele_fina") == 2 && pl.Player.Equipped == Player.Weapon.Knife && pl.Stats.Level == 3 && pl.Stats.Get("vit") == 2 && pl.Skills.Grade("esfolar") == 2 && !pl.LossApplied,
                $"Nv {pl.Stats.Level}, vit {pl.Stats.Get("vit")}, esfolar {pl.Skills.Grade("esfolar")}");
        Finish();
    }

    private async Task Skip()
    {
        await Frames(20);
        var pr = Prefs.Load(); pr.FinaleSeen = true; pr.Save();
        var sen = V.Fighters["senior"];
        await Teleport(sen.GlobalPosition + new Vector2(0, 54), Vector2.Up);
        for (int i = 0; i < 20 && sen.Alive; i++) V.StrikeFighter(sen, sen.GlobalPosition + new Vector2(0, 30), 2, true, 1, false, false); // controlled: the fight itself is checked in the prologue phase
        await Frames(5);
        var fin = V.Finale;
        Check("with the ending seen before, it can be skipped", fin != null && fin.Skippable);
        await Frames(70);
        await Tap(Key.Escape);
        for (int i = 0; i < 300 && _c.Plains == null; i++) await Frames(1);
        Check("Esc skips to the awakening", _c.Plains != null && _c.Plains.LossApplied && fin!.Seen.Contains(SeniorFinale.Stage.Dark) && !fin.Seen.Contains(SeniorFinale.Stage.Explode), fin == null ? "" : string.Join(",", fin.Seen));
        Finish();
    }

    // for the video: from the meditation, walk to the Senior, win, watch the whole ending, take the first steps in the Planície
    private async Task EndingClip()
    {
        await Frames(20);
        var sen = V.Fighters["senior"];
        for (int round = 0; round < 40 && sen.Alive; round++)
        {
            if (P.Health < 6) P.Revive(); // controlled: the test keeps the swordsman standing
            await Teleport(sen.GlobalPosition + new Vector2(0, 54), Vector2.Up);
            await LockOn(sen);
            await Lamina(sen);
            if (sen.Alive && sen.Stagger > 0) { await Tap(Key.J); await Frames(8); await Tap(Key.J); await Frames(12); }
            await Frames(8);
        }
        var fin = V.Finale;
        for (int i = 0; i < 900 && fin != null && fin.Current != SeniorFinale.Stage.Speech; i++) await Frames(1);
        await Frames(90);
        await Tap(Key.E);
        for (int i = 0; i < 900 && _c.Plains == null; i++) await Frames(1);
        await Frames(150);
        var pl = _c.Plains;
        if (pl != null) { Press(Key.D, true); await Flush(); await Frames(45); Press(Key.D, false); Press(Key.S, true); await Flush(); await Frames(30); Press(Key.S, false); await Flush(); await Frames(40); }
        Check("the ending played to the first steps in the Planície", pl != null && pl.Started, pl == null ? "" : $"{pl.Player.GlobalPosition}");
        Finish();
    }

    private void Finish()
    {
        int fails = _results.FindAll(r => !r.Ok).Count;
        GD.Print($"vale {_phase}: {_results.Count - fails}/{_results.Count} OK");
        Engine.TimeScale = 1;
        GetTree().Quit(fails == 0 ? 0 : 1);
    }
}
