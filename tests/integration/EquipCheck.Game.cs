using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Elements;
using SpiritOfMurin.Game;
using SpiritOfMurin.Items;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Tests;

/// <summary>
/// T18 — the equipment through the real Planície (each phase its own run, <c>--seed=N</c>):
/// <list type="bullet">
/// <item><c>bolsa</c>: a new journey; the traveller's bundle under the pine; I opens the bag; the sabre and the tunic compare
/// with what is on; each is equipped on its own (icon and mark); the hat is crafted and worn; off again: the green tunic, the
/// hair and the bun back, the knife and the T06 shortcuts still working.</item>
/// <item><c>combate</c>: the sabre in hand — the reach in the bag is the strike's; the free aim by the mouse and the locked
/// target turn the cut; the Reforço de Ki glows on the sabre; 12 hits reveal the Meia-Lua, whose last cut sweeps wide.</item>
/// <item><c>salvar</c> then <c>continuar</c>: equip the set, meditate (the journey is written), close; Continuar brings the set back.</item>
/// <item><c>oldsave</c>: <c>tests/fixtures/saves/jornada_antes_do_ki.json</c> loads with the green tunic, bare head and the knife.</item>
/// </list>
/// Files: <c>user://teste_equip.json</c> (never the creator's journey).
/// </summary>
public partial class EquipCheck
{
    private const string SavePath = "user://teste_equip.json";
    private const string Fixture = "res://tests/fixtures/saves/jornada_antes_do_ki.json";
    private PlainsGame _g = null!;
    private int _seed = 20261126;
    private string _shots = "";
    private Player P => _g.Player;
    private PlainsHud Hud => _g.Hud;

    private async Task PhysFrames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private async Task Flush() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static void KeyEv(Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });
    private async Task Tap(Key k) { KeyEv(k, true); await Flush(); await PhysFrames(2); KeyEv(k, false); await Flush(); await PhysFrames(1); }
    private async Task Chord(params Key[] keys) { foreach (var k in keys) { KeyEv(k, true); await Flush(); await PhysFrames(1); } await PhysFrames(2); foreach (var k in keys.Reverse()) KeyEv(k, false); await Flush(); await PhysFrames(2); }
    private string Lines(int i) => i < 0 || i >= Hud.Rows.Count ? "" : string.Join(" | ", Hud.Rows[i].Lines?.Select(l => l.Text) ?? Enumerable.Empty<string>());
    private async Task SelectId(string id) { for (int k = 0; k <= Hud.Rows.Count && Hud.Rows.Count > 0 && Hud.Rows[Hud.Selected].Id != id; k++) await Tap(Key.D); }
    private async Task ToTab(string tab) { for (int t = 0; t < 7 && _g.BagTab != tab; t++) await Tap(Key.Tab); }
    private PlainsHud.Row? RowOf(string id) => Hud.Rows.Cast<PlainsHud.Row?>().FirstOrDefault(r => r!.Value.Id == id);

    private async Task Open(PlainsGame.StartMode mode, SaveData? write = null)
    {
        if (write != null) SaveGame.Write(SavePath, write); else if (mode == PlainsGame.StartMode.New) SaveGame.Erase(SavePath);
        _g = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _g.SavePath = SavePath; _g.ShowIntro = false; _g.Start = mode; _g.Seed = _seed;
        AddChild(_g);
        await PhysFrames(40);
    }

    private async Task Teleport(Vector2 at) { P.GlobalPosition = at; P.GetNode<Camera2D>("Camera").ResetSmoothing(); await PhysFrames(8); }

    private async Task Shot(string name)
    {
        if (_shots.Length == 0) return;
        DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(_shots));
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        GetViewport().GetTexture().GetImage().SavePng($"{_shots}/{name}.png"); // the game's own picture, never the desktop
    }

    // where the bundle is for this seed (the generator's fixed place under the pine)
    private Vector2? BundleSpot()
    {
        for (int ty = -9; ty <= -5; ty++)
            for (int tx = -30; tx <= 30; tx++)
                foreach (var f in _g.Gen.FeaturesAt(tx, ty))
                    if (f.Kind == FeatureKind.Bundle) return f.Position;
        return null;
    }

    /// <summary>A journey with the pieces in the bag (and the sabre in hand when asked), for the phases that start past the bundle.</summary>
    private SaveData WithSet(bool sabreInHand, bool wear = false) => new()
    {
        Region = "planicie", Phase = "planicie", Seed = _seed, Spot = new[] { 16f, 16f }, LossApplied = true, Health = 10, Vigor = 60, Ki = 100, Level = 3, Points = 0,
        Inventory = { new SavedItem { Id = "faca_pedra", Count = 1 }, new SavedItem { Id = "sabre_ferro", Count = 1 }, new SavedItem { Id = "tunica_viajante", Count = 1 }, new SavedItem { Id = "chapeu_bambu", Count = 1 } },
        Equipped = sabreInHand ? "sword" : "knife", EquippedItem = sabreInHand ? "sabre_ferro" : "faca_pedra",
        Wearing = wear ? new() { ["corpo"] = "tunica_viajante", ["cabeca"] = "chapeu_bambu" } : null,
        Book = { new Vale.BookEntry { Name = "Reforço de Ki", Kind = "técnica", Tech = "ki_reinforce", State = "Plena" } },
    };

    // ------------------------------------------------------------------ bolsa
    private async Task BagPhase()
    {
        await Open(PlainsGame.StartMode.New);
        Check("a new journey wears the green tunic, bare head, empty hands", P.Outfit == RoupaArts.Default && P.Hat == null && P.HeldArt == null, $"{P.Outfit} / {P.Hat} / {P.HeldArt}");
        var spot = BundleSpot();
        Check($"seed {_seed}: the traveller's bundle is under the pine by the creek", spot != null, spot?.ToString() ?? "not generated");
        if (spot == null) return;
        await Teleport(spot.Value + new Vector2(-10, 4));
        var bundle = _g.World.Objects.GetChildren().OfType<Pickup>().FirstOrDefault(p => p.ItemId == "trouxa_viajante");
        Check("the bundle is in the world, within reach of E", bundle != null && _g.Interactables().Contains(bundle), bundle == null ? "no node" : $"{_g.Interactables().Count} within reach");
        await Shot("01-trouxa");
        while (_g.Interactables().FirstOrDefault() is not Pickup { ItemId: "trouxa_viajante" } && _g.Interactables().Count > 1) { await Tap(Key.E); } // other things first
        await Tap(Key.E);
        Check("E takes it: the sabre and the traveller's tunic go to the bag", _g.Inventory.Count("sabre_ferro") == 1 && _g.Inventory.Count("tunica_viajante") == 1, string.Join(",", _g.Inventory.Entries.Select(e => e.Id)));
        // the bag
        await Tap(Key.I);
        await ToTab("equipamento");
        Check("I opens the bag; 'Equipamento' lists the green tunic (worn), the sabre and the tunic, each with its icon",
            Hud.InventoryOpen && RowOf("tunica_verde") is { Equipped: true } && RowOf("sabre_ferro") is { Icon: "sabre_ferro" } && RowOf("tunica_viajante") is { Icon: "tunica_viajante", Equipped: false },
            string.Join(",", Hud.Rows.Select(r => $"{r.Id}{(r.Equipped ? "✓" : "")}")));
        await SelectId("sabre_ferro");
        string sabre = Lines(Hud.Selected);
        Check("the sabre compares with empty hands: reach 38 px, 2 per cut, 3 cuts; its techniques still hidden", sabre.Contains("Alcance (px): — → 38") && sabre.Contains("Cortes no combo: — → 3") && sabre.Contains("???"), sabre);
        await Shot("02-bolsa-sabre");
        await Tap(Key.E);
        Check("E equips the sabre: marked in the bag, in the hand, drawn as the sabre", RowOf("sabre_ferro") is { Equipped: true } && P.Equipped == Player.Weapon.Sword && P.HeldArt == "sabre_ferro" && P.Blade?.Reach == 38, $"{P.Equipped} {P.HeldArt}");
        await SelectId("tunica_viajante");
        string tunic = Lines(Hud.Selected);
        Check("the tunic compares with the green one: defense 0% → 8%, weight +0% → +10%", tunic.Contains("Vestindo: Túnica Verde") && tunic.Contains("Defesa: 0% → 8%") && tunic.Contains("Peso (vigor por corte e passo): +0% → +10%"), tunic);
        float cutBefore = P.CutVigorCost, dmgBefore = P.DamageTaken;
        await Tap(Key.E);
        Check("E puts on the tunic: marked, the green one unmarked, drawn on the body", RowOf("tunica_viajante") is { Equipped: true } && RowOf("tunica_verde") is { Equipped: false } && P.Outfit == "tunica_viajante", P.Outfit);
        Check("decision 1: the tunic holds back 8% of the damage and costs 10% more Vigor per cut", Mathf.IsEqualApprox(P.DamageTaken, dmgBefore * 0.92f) && Mathf.IsEqualApprox(P.CutVigorCost, cutBefore * 1.1f), $"dano {dmgBefore:0.000}→{P.DamageTaken:0.000}, corte {cutBefore:0.0}→{P.CutVigorCost:0.0}");
        // the hat: crafted (controlled materials: what bamboo and a hide give, checked in RecursosCheck / JourneyCheck)
        _g.Inventory.Add("vara_bambu", 3); _g.Inventory.Add("tira_amarracao", 1);
        await ToTab("fabricacao"); await SelectId("chapeu_bambu");
        Check("'Fabricação' offers the bamboo hat with what it takes", Hud.Rows[Hud.Selected].Id == "chapeu_bambu" && Hud.Rows[Hud.Selected].Enabled && Lines(Hud.Selected).Contains("Vara de Bambu: tenho 3 de 3"), Lines(Hud.Selected));
        await Tap(Key.E);
        Check("the hat is made", _g.Inventory.Count("chapeu_bambu") == 1);
        await ToTab("equipamento"); await SelectId("chapeu_bambu");
        Check("the hat compares with the bare head: defense 0% → 3%", Lines(Hud.Selected).Contains("Cabeça descoberta") && Lines(Hud.Selected).Contains("Defesa: 0% → 3%"), Lines(Hud.Selected));
        await Tap(Key.E);
        Check("E puts on the hat: marked, on the head (it hides the bun and the band)", RowOf("chapeu_bambu") is { Equipped: true } && P.Hat == "chapeu_bambu" && CapaceteArts.Get(P.Hat)!.HairCover == CabecaArt.BandRows, P.Hat ?? "");
        await Shot("03-bolsa-conjunto");
        await Tap(Key.I); await PhysFrames(6);
        await Shot("04-conjunto-equipado");
        // off again
        await Tap(Key.I); await ToTab("equipamento");
        await SelectId("chapeu_bambu"); await Tap(Key.E);
        Check("E on the worn hat takes it off: the bare head (bun, band and ribbon drawn again)", P.Hat == null && RowOf("chapeu_bambu") is { Equipped: false });
        await SelectId("tunica_viajante"); await Tap(Key.E);
        Check("E on the worn tunic takes it off: back to the green tunic", P.Outfit == RoupaArts.Default && RowOf("tunica_verde") is { Equipped: true } && Mathf.IsEqualApprox(P.CutVigorCost, cutBefore), P.Outfit);
        // the stone knife made at the creek (controlled materials: flint and a branch, picked in InventarioCheck's route)
        _g.Inventory.Add("silex", 1); _g.Inventory.Add("galho_rijo", 1);
        await ToTab("fabricacao"); await SelectId("faca_pedra"); await Tap(Key.E);
        await ToTab("equipamento"); await SelectId("faca_pedra"); await Tap(Key.E);
        Check("the stone knife still works as before (short blade, tool 'faca', the tuning's numbers)", P.Equipped == Player.Weapon.Knife && P.HeldArt == "faca_pedra" && _g.ToolInHand?.Kind == "faca" && P.Blade?.Reach == P.Tuning.KnifeReach && P.Blade?.Chain == P.Tuning.KnifeMaxChain, $"{P.Equipped} {P.HeldArt}");
        // T06: the sabre on shortcut 1 (chosen in the bag), then 1 in the world puts it in the hand
        await SelectId("sabre_ferro"); await Tap(Key.Key1);
        await Tap(Key.I);
        await Tap(Key.Key1);
        Check("T06 shortcut 1 puts the sabre in the hand", P.HeldArt == "sabre_ferro", P.HeldArt ?? "");
    }

    // ------------------------------------------------------------------ combate
    private async Task CombatPhase()
    {
        var j = WithSet(true);
        j.WeaponHits = new() { ["sabre_ferro"] = 11 }; // controlled: one hit from the Meia-Lua
        await Open(PlainsGame.StartMode.Continue, j);
        Check("the journey brings the sabre in hand", P.HeldArt == "sabre_ferro" && P.Blade?.Reach == 38);
        // the reach the bag shows is the strike's
        await Tap(Key.I); await ToTab("equipamento");
        float shown = _g.Catalog.Get("sabre_ferro").Weapon!.Reach;
        await Tap(Key.I);
        Aim.UseForSession(false);
        await Tap(Key.J);
        await PhysFrames(4);
        var area = P.BladeArea();
        Check("the reach in the bag (38 px) is the reach of the cut in play", area != null && Mathf.IsEqualApprox(area.Value.Radius, shown + P.CutSlack), area == null ? "no cut" : $"bolsa {shown}, golpe {area.Value.Radius}");
        await PhysFrames(30);
        // free aim by the mouse
        Aim.UseForSession(true);
        var vp = GetViewport();
        Vector2 screenTo = vp.GetVisibleRect().Size / 2 + new Vector2(-120, 0); // to the left of the body
        var mm = new InputEventMouseMotion { Position = screenTo, GlobalPosition = screenTo };
        Input.ParseInputEvent(mm); await Flush(); await PhysFrames(2);
        await Tap(Key.J); await PhysFrames(3);
        Check("free aim: the cut with the sabre goes toward the cursor (left)", P.Facing.X < -0.7f, P.Facing.ToString());
        await PhysFrames(30);
        Aim.UseForSession(false);
        // the locked target: a boar of the world (moved next to the body, its brain calm)
        var boar = _g.World.Objects.GetChildren().OfType<Creatures.Boar>().FirstOrDefault()
            ?? GetTree().GetNodesInGroup("creatures").OfType<Creatures.Boar>().FirstOrDefault();
        if (boar != null)
        {
            boar.GlobalPosition = P.GlobalPosition + new Vector2(0, 30);
            await PhysFrames(4);
            await Tap(Key.Space);
            await Tap(Key.J); await PhysFrames(3);
            Check("locked target: the cut turns to the boar (below)", P.Facing.Y > 0.7f, P.Facing.ToString());
            await PhysFrames(30);
            if (_g.Focused != null) await Tap(Key.Space);
        }
        else Check("NOT ASSESSED: no boar in the world to lock on", false);
        // the Reforço de Ki with the sabre (Shift held + cut)
        _g.Player.Ki = 100;
        KeyEv(Key.Shift, true); await Flush(); await PhysFrames(1);
        KeyEv(Key.J, true); await Flush(); await PhysFrames(40);
        KeyEv(Key.J, false); KeyEv(Key.Shift, false); await Flush(); await PhysFrames(4);
        Check("the Reforço de Ki is active with the sabre (charges ready)", P.ReinforceCharges > 0, $"cargas {P.ReinforceCharges}");
        await Shot("05-reforco-sabre");
        // the Meia-Lua: one more hit with the sabre on a bamboo or a tree of the world
        var target = GetTree().GetNodesInGroup("elements").OfType<WorldElement>().Where(e => e.State == ElementState.Standing && e.Def.Family != "arbusto")
            .OrderBy(e => e.GlobalPosition.DistanceTo(P.GlobalPosition)).FirstOrDefault();
        Check("an element of the world to strike", target != null);
        if (target == null) return;
        await Teleport(target.GlobalPosition + new Vector2(-18, 2));
        P.Face(Vector2.Right);
        int before = _g.WeaponHits.GetValueOrDefault("sabre_ferro");
        for (int i = 0; i < 3 && !_g.KnownWeaponTechs.Contains("meia_lua"); i++) { await Tap(Key.J); await PhysFrames(14); }
        Check("12 hits with the sabre reveal the Meia-Lua (announced, in the Livro)", _g.KnownWeaponTechs.Contains("meia_lua") && _g.Book.Any(b => b.Name == "Meia-Lua") && Hud.Messages.Any(m => m.Contains("Meia-Lua")), $"acertos {before}→{_g.WeaponHits.GetValueOrDefault("sabre_ferro")}");
        Check("the sabre now sweeps 120° each side on its last cut", Mathf.IsEqualApprox(P.WideCutDegrees, 120), P.WideCutDegrees.ToString());
        await PhysFrames(30);
        P.RestoreVigor(999);
        bool wide = false;
        for (int i = 0; i < 3; i++) { await Tap(Key.J); for (int k = 0; k < 6; k++) { await PhysFrames(1); if (P.WideCut && Mathf.IsEqualApprox(P.CutArcDegrees, 120)) wide = true; } }
        Check("the third cut of the combo is the Meia-Lua's wide sweep", wide);
        await Shot("06-meia-lua");
    }

    // ------------------------------------------------------------------ salvar / continuar
    private async Task SavePhase()
    {
        await Open(PlainsGame.StartMode.Continue, WithSet(false));
        _g.Equip("sabre_ferro"); _g.ToggleWear("tunica_viajante"); _g.ToggleWear("chapeu_bambu");
        var mat = _g.World.Objects.GetChildren().OfType<MeditationMat>().FirstOrDefault();
        Check("the meditation mat is there", mat != null);
        if (mat == null) return;
        await Teleport(mat.GlobalPosition + new Vector2(-8, 4));
        while (_g.Interactables().FirstOrDefault() is not MeditationMat && _g.Interactables().Count > 1) await Tap(Key.E);
        await Tap(Key.E);
        var (st, d) = SaveGame.Read(SavePath);
        Check("meditating writes the journey with what is worn and in hand", st == SaveStatus.Ok && d!.EquippedItem == "sabre_ferro" && d.Wearing?["corpo"] == "tunica_viajante" && d.Wearing?["cabeca"] == "chapeu_bambu",
            d == null ? st.ToString() : $"{d.EquippedItem} {string.Join(",", d.Wearing ?? new())}");
    }

    private async Task ResumePhase()
    {
        if (!Godot.FileAccess.FileExists(SavePath)) { Check("NOT ASSESSED: run --phase=salvar first (no journey written)", false); return; }
        await Open(PlainsGame.StartMode.Continue);
        Check("Continuar brings the sabre in hand, the tunic and the hat on", P.HeldArt == "sabre_ferro" && P.Outfit == "tunica_viajante" && P.Hat == "chapeu_bambu", $"{P.HeldArt} {P.Outfit} {P.Hat}");
        await Tap(Key.I); await ToTab("equipamento");
        Check("…and the bag marks the three", RowOf("sabre_ferro") is { Equipped: true } && RowOf("tunica_viajante") is { Equipped: true } && RowOf("chapeu_bambu") is { Equipped: true });
        await Tap(Key.I);
        await Shot("07-continuar");
    }

    // ------------------------------------------------------------------ oldsave
    private async Task OldSavePhase()
    {
        string old = Godot.FileAccess.GetFileAsString(Fixture);
        Check("the fixture is there", old.Length > 0);
        using (var f = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write)) f.StoreString(old);
        await Open(PlainsGame.StartMode.Continue);
        Check("a journey from before T18 wears the green tunic and no hat", P.Outfit == RoupaArts.Default && P.Hat == null && _g.Wearing.Values.All(v => v.Length == 0));
        Check("…and its knife is back in the hand, as saved ('knife')", P.Equipped == Player.Weapon.Knife && P.HeldArt == "faca_pedra" && _g.EquippedItem == "faca_pedra", $"{P.Equipped} {_g.EquippedItem}");
        await Tap(Key.I);
        Check("…its bag shows the green tunic as worn", RowOf("tunica_verde") is { Equipped: true });
        await Tap(Key.I);
    }
    // ------------------------------------------------------------------ video (run with --write-movie: the game's own frames)
    /// <summary>A short take for the report: the bag opens, the tunic, the hat and the sabre go on one by one, then the sabre's combo and the Reforço.</summary>
    private async Task VideoPhase()
    {
        await Open(PlainsGame.StartMode.Continue, WithSet(false));
        var target = GetTree().GetNodesInGroup("elements").OfType<WorldElement>().Where(e => e.State == ElementState.Standing && e.Def.Family == "bambu")
            .OrderBy(e => e.GlobalPosition.DistanceTo(P.GlobalPosition)).FirstOrDefault();
        if (target != null) { await Teleport(target.GlobalPosition + new Vector2(-24, 2)); P.Face(Vector2.Right); }
        await PhysFrames(30);
        await Tap(Key.I); await PhysFrames(20);
        await ToTab("equipamento"); await PhysFrames(15);
        foreach (var id in new[] { "tunica_viajante", "chapeu_bambu", "sabre_ferro" })
        {
            await SelectId(id); await PhysFrames(30);
            await Tap(Key.E); await PhysFrames(35);
        }
        await Tap(Key.I); await PhysFrames(25);
        P.Face(Vector2.Right);
        for (int i = 0; i < 3; i++) { await Tap(Key.J); await PhysFrames(9); }
        await PhysFrames(30);
        _g.Player.Ki = 100;
        KeyEv(Key.Shift, true); await Flush(); await PhysFrames(1);
        KeyEv(Key.J, true); await Flush(); await PhysFrames(40);
        KeyEv(Key.J, false); KeyEv(Key.Shift, false); await Flush(); await PhysFrames(15);
        for (int i = 0; i < 3; i++) { await Tap(Key.J); await PhysFrames(9); }
        await PhysFrames(40);
        Check("the take went through: the set on and the sabre used", P.HeldArt == "sabre_ferro" && P.Outfit == "tunica_viajante" && P.Hat == "chapeu_bambu");
    }
    // ------------------------------------------------------------------ mapa (the test map, Painel → "Mapa de testes")
    /// <summary>The test map: every item of the catalogue in the bag, one of each weapon, robe, hat and tool shown on the ground, E picks one up; its own file.</summary>
    private async Task TestMapPhase()
    {
        SaveGame.Erase(SavePath);
        _g = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _g.SavePath = SavePath; _g.ShowIntro = false; _g.Start = PlainsGame.StartMode.New; _g.Seed = PlainsGame.TestMapSeed; _g.TestMap = true;
        AddChild(_g);
        await PhysFrames(40);
        var want = _g.Catalog.Items.Values.Where(d => !d.Base && d.Id is not ("trouxa_viajante" or "moedas")).Select(d => d.Id).ToList();
        var missing = want.Where(id => _g.Inventory.Count(id) <= 0).ToList();
        Check($"every item of the catalogue is in the bag ({want.Count} items, read from the catalogue)", want.Count > 0 && missing.Count == 0, missing.Count == 0 ? "" : "faltam: " + string.Join(", ", missing));
        var shown = _g.World.Objects.GetChildren().OfType<Pickup>().Where(p => p.IconItem.Length > 0).ToList();
        int pieces = _g.Catalog.Items.Values.Count(d => !d.Base && (d.Weapon != null || d.Wear != null || d.Tool != null));
        Check($"the showcase lays one of each weapon, robe, hat and tool ({pieces})", shown.Count == pieces && new[] { "sabre_ferro", "tunica_viajante", "chapeu_bambu", "faca_pedra" }.All(id => shown.Any(p => p.ItemId == id)), string.Join(",", shown.Select(p => p.ItemId)));
        Check("the weapons' playable techniques come known (the Meia-Lua)", _g.KnownWeaponTechs.Contains("meia_lua"));
        var hat = shown.First(p => p.ItemId == "chapeu_bambu");
        await Teleport(hat.GlobalPosition + new Vector2(0, 8));
        await Shot("08-mapa-de-testes");
        int before = _g.Inventory.Count("chapeu_bambu");
        for (int i = 0; i < 6 && IsInstanceValid(hat) && !hat.IsQueuedForDeletion(); i++) { if (_g.Interactables().FirstOrDefault() == hat) { await Tap(Key.E); break; } await Wheel(); }
        Check("E picks a piece of the showcase", _g.Inventory.Count("chapeu_bambu") == before + 1, $"{before} → {_g.Inventory.Count("chapeu_bambu")}");
        Check("the test map writes nowhere but its own file", _g.SavePath == SavePath);
    }

    private async Task Wheel()
    {
        var at = GetViewport().GetVisibleRect().Size / 2;
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.WheelDown, Pressed = true, Position = at, GlobalPosition = at, Factor = 1 }); await Flush(); await PhysFrames(1);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.WheelDown, Pressed = false, Position = at, GlobalPosition = at }); await Flush(); await PhysFrames(3);
    }
}
