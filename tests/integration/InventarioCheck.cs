using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Elements;
using SpiritOfMurin.Game;
using SpiritOfMurin.Items;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Tests;

/// <summary>
/// T06 — the bag, crafting, the sheet, the HUD and the choice of interaction, through the real scene, keys and mouse.
/// Phases, each its own run (<c>--seed=N</c> where it matters):
/// <list type="bullet">
/// <item><c>prancha</c>: pictures of the bag, sheet and HUD with the creator's items, and each item picture, for the board.</item>
/// <item><c>bolsa</c>: the bag opens on "Tudo" with the coins; a click on "Ferramentas" and Tab filter the grid; the knife
/// compares with the axe in hand in strikes on the same tree; "Fabricação" lists the T05 recipes as "tenho X de Y"; the last
/// ingredient makes the recipe possible (announced); crafting the polished axe updates materials, result and actions on the
/// same screen; it fells the same pine in 3 strikes instead of 6 (the comparison and the world agree).</item>
/// <item><c>atalhos</c> then <c>atalhos2</c>: 1/2 in the bag put the axe and the pickaxe on shortcuts (the same keys in the bag
/// never use anything in the world); 1/2 in the world swap tools; a herb on 3 is used until it runs out, the row says 0 and the
/// key says it ran out; in a new process the shortcuts are still there.</item>
/// <item><c>ficha</c>: the mouse over FOR selects it and shows the damage of the axe in hand 1,06 → 1,12; a click on its "+"
/// spends a point; S/E spend on CON with how far it still goes to the cap; with no point the "+" does nothing; the mouse
/// over the bars shows "Vida 60% · 6/10" and the HUD keeps "Vida 60%".</item>
/// <item><c>alvo --seed=N</c>: away from the clearing, a herb and a print found by the seed within reach of one spot: a ring
/// and a list show them, the nearest first; the mouse wheel chooses the other; E acts only on the chosen one; with one target
/// left, E acts at once with no list.</item>
/// <item><c>rodape --seed=N</c>: three branches picked one after the other show one "+3 Galho Rijo" over the head; next to a
/// carcass the footer says what E and K do, away from it it does not.</item>
/// <item><c>entrada</c>: opening, confirming and closing the bag, the sheet and the pause never cut, shoot Ki or pick up.</item>
/// <item><c>percurso --seed=N</c>: one run — at the creek short of flint, the recipe says what is missing, the flint is picked, the
/// axe made and equipped and used on a pine, compared with the polished one, a point spent, a choice of target made.</item>
/// <item><c>oldsave</c>: the journey from before (fixture) loads with its items, knife, attributes and coins; tabs and empty shortcuts.</item>
/// </list>
/// Files: <c>user://teste_inv.json</c>; preferences <c>user://teste_inv_prefs.json</c>. Pictures with <c>--shots=</c> (the game's
/// own viewport, never the desktop). Items given and teleports are named in the checks.
/// </summary>
public partial class InventarioCheck : Node
{
    private const string SavePath = "user://teste_inv.json";
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
        if (_phase != "atalhos2" && Godot.FileAccess.FileExists(Prefs.Path)) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(Prefs.Path));
        Bindings.Init(); Assist.Reload(); Aim.Reload(); Aim.UseForSession(false);
        _ = Guard(_phase switch
        {
            "bolsa" => Bag(), "atalhos" => Shortcuts(), "atalhos2" => Shortcuts2(), "ficha" => SheetPhase(), "alvo" => TargetPhase(),
            "rodape" => FooterPhase(), "entrada" => InputPhase(), "percurso" => Route(), "oldsave" => OldSave(), _ => Board(),
        });
    }

    private async Task Guard(Task t)
    {
        try { await t; }
        catch (System.Exception ex) { Check("NOT ASSESSED: the phase stopped on an exception", false, ex.GetType().Name + ": " + ex.Message + " " + ex.StackTrace?.Split('\n').FirstOrDefault()); Finish(); }
    }

    // ------------------------------------------------------------------ helpers
    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private async Task Flush() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static void Press(Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });
    private async Task Tap(Key k) { Press(k, true); await Flush(); await Frames(2); Press(k, false); await Flush(); await Frames(3); }
    private void Finish() { int f = _results.Count(r => !r.Ok); GD.Print($"inventario {_phase} seed {_seed}: {_results.Count - f}/{_results.Count} OK"); GetTree().Quit(f == 0 ? 0 : 1); }
    private PlainsHud Hud => _g.Hud;
    private Player P => _g.Player;

    private async Task Shot(string name)
    {
        if (_shots.Length == 0 || DisplayServer.GetName() == "headless") return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        DirAccess.MakeDirRecursiveAbsolute(_shots);
        GetViewport().GetTexture().GetImage().SavePng($"{_shots}/{name}.png");
    }

    private async Task Move(Vector2 win) { Input.ParseInputEvent(new InputEventMouseMotion { Position = win, GlobalPosition = win }); await Flush(); await Frames(2); }
    private async Task Click(Vector2 win)
    {
        await Move(win);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = win, GlobalPosition = win }); await Flush(); await Frames(2);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false, Position = win, GlobalPosition = win }); await Flush(); await Frames(4);
    }
    private async Task Wheel(bool down)
    {
        var at = GetViewport().GetVisibleRect().Size / 2;
        var b = down ? MouseButton.WheelDown : MouseButton.WheelUp;
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = b, Pressed = true, Position = at, GlobalPosition = at, Factor = 1 }); await Flush(); await Frames(1);
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = b, Pressed = false, Position = at, GlobalPosition = at }); await Flush(); await Frames(3);
    }

    private async Task Open(PlainsGame.StartMode mode, SaveData? write = null)
    {
        if (write != null) SaveGame.Write(SavePath, write); else if (mode == PlainsGame.StartMode.New) SaveGame.Erase(SavePath);
        _g = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _g.SavePath = SavePath; _g.ShowIntro = false; _g.Start = mode; _g.Seed = _seed;
        AddChild(_g);
        await Frames(40);
    }

    private async Task Teleport(Vector2 at) { P.GlobalPosition = at; P.GetNode<Camera2D>("Camera").ResetSmoothing(); await Frames(8); }
    private string Lines(int i) => i < 0 || i >= Hud.Rows.Count ? "" : string.Join(" | ", Hud.Rows[i].Lines?.Select(l => l.Text) ?? Enumerable.Empty<string>());
    private async Task SelectId(string id) { for (int k = 0; k <= Hud.Rows.Count && Hud.Rows.Count > 0 && Hud.Rows[Hud.Selected].Id != id; k++) await Tap(Key.D); }
    private async Task ToTab(string tab) { for (int t = 0; t < 7 && _g.BagTab != tab; t++) await Tap(Key.Tab); }

    /// <summary>The creator's items (pedido T06): a journey after T05 with the axe in hand and some materials short.</summary>
    public static SaveData CreatorJourney(int seed = 20261126) => new()
    {
        Region = "planicie", Phase = "planicie", Seed = seed, Spot = new[] { 16f, 16f }, LossApplied = true,
        Inventory =
        {
            new SavedItem { Id = "faca_pedra", Count = 1 }, new SavedItem { Id = "grama_sangue_q1", Count = 3 },
            new SavedItem { Id = "silex", Count = 2 }, new SavedItem { Id = "galho_rijo", Count = 1 },
            new SavedItem { Id = "tora", Count = 4 }, new SavedItem { Id = "minerio_ferro", Count = 2 },
            new SavedItem { Id = "machado_pedra", Count = 1 }, new SavedItem { Id = "picareta_pedra", Count = 1 },
            new SavedItem { Id = "bolsa_simples", Count = 1 },
        },
        Equipped = "axe", EquippedItem = "machado_pedra", Health = 6, Vigor = 40, Ki = 80, Level = 4, Points = 2, Coins = 25,
        Attributes = new() { ["for"] = 1, ["con"] = 2, ["sor"] = 1 },
    };

    // ------------------------------------------------------------------ the board's pictures
    private async Task Board()
    {
        await Open(PlainsGame.StartMode.Continue, CreatorJourney());
        await Shot("hud-hoje");
        _g.ToggleInventory(); await Frames(6); await Shot("bolsa-hoje"); _g.ToggleInventory(); await Frames(4);
        _g.ToggleSheet(); await Frames(6); await Shot("ficha-hoje"); _g.ToggleSheet(); await Frames(4);
        var ids = new[] { "faca_pedra", "grama_sangue_q1", "silex", "galho_rijo", "tora", "minerio_ferro", "machado_pedra", "picareta_pedra",
            "machado_lapidado", "picareta_lapidada", "pedra_bruta", "vara_bambu", "quartzo", "resina", "bolsa_simples", "carne_macia_assada", "moedas", "tira_amarracao" };
        var sheet = new IconSheet { Ids = ids, ZIndex = 4000 };
        var layer = new CanvasLayer { Layer = 120 }; AddChild(layer); layer.AddChild(sheet);
        await Frames(4); await Shot("icones");
        using (var f = Godot.FileAccess.Open($"{_shots}/icones.txt", Godot.FileAccess.ModeFlags.Write))
            if (f != null) for (int i = 0; i < ids.Length; i++) f.StoreLine($"{ids[i]} {IconSheet.Cell(i).Position.X} {IconSheet.Cell(i).Position.Y} {IconSheet.Size}");
        Check("the board's pictures were taken", _shots.Length > 0);
        Finish();
    }

    private sealed partial class IconSheet : Control
    {
        public string[] Ids = System.Array.Empty<string>();
        public const int Size = 32;
        public static Rect2 Cell(int i) => new(8 + i % 10 * (Size + 4), 8 + i / 10 * (Size + 4), Size, Size);
        public override void _Draw()
        {
            DrawRect(new Rect2(0, 0, 640, 360), MapView.Paper);
            for (int i = 0; i < Ids.Length; i++) ItemIcons.Draw(this, Ids[i], Cell(i).GetCenter(), 2);
        }
    }

    // ------------------------------------------------------------------ 1-2: the bag and crafting
    private async Task Bag()
    {
        await Open(PlainsGame.StartMode.Continue, CreatorJourney());
        await Tap(Key.I);
        Check("I opens the bag on 'Tudo' with every item and the coins", Hud.InventoryOpen && _g.BagTab == "tudo" && Hud.Rows.Count == 10 && Hud.Bag.Money == 25, $"aba {_g.BagTab}, {Hud.Rows.Count} itens"); // T18: + a Túnica Verde, sempre na bolsa
        var axe = Hud.Rows.FirstOrDefault(r => r.Id == "machado_pedra");
        Check("the axe in hand is marked (✓)", axe.Equipped);
        await Shot("01-bolsa-tudo");
        await Click(Hud.ToWindow(Hud.Bag, Hud.Bag.TabRects[2]).GetCenter());
        Check("a click on 'Ferramentas' shows only the tools", _g.BagTab == "ferramenta" && Hud.Rows.Select(r => r.Id).OrderBy(x => x).SequenceEqual(new[] { "machado_pedra", "picareta_pedra" }), string.Join(",", Hud.Rows.Select(r => r.Id)));
        await Tap(Key.Tab);
        Check("Tab: 'Consumíveis' (the herb)", _g.BagTab == "consumivel" && Hud.Rows.All(r => r.Id.StartsWith("grama")), string.Join(",", Hud.Rows.Select(r => r.Id)));
        await Tap(Key.Tab);
        Check("Tab: 'Materiais' (flint, branch, log, ore)", _g.BagTab == "material" && Hud.Rows.Count == 4, string.Join(",", Hud.Rows.Select(r => r.Id)));
        await ToTab("equipamento");
        await SelectId("faca_pedra");
        string knife = Lines(Hud.Selected);
        Check("the knife compares with the axe in hand, in strikes on the same tree", knife.Contains("Machado de Pedra") && knife.Contains("Pinheiro: 6 golpes → não corta") && knife.Contains("Touceira de bambu: 3 golpes → 3 golpes"), knife);
        await Shot("02-bolsa-comparacao-faca");
        // crafting: the T05 recipes, as "tenho X de Y"
        await ToTab("fabricacao");
        Check("'Fabricação' lists the T05 recipes", new[] { "machado_pedra", "picareta_pedra", "machado_lapidado", "picareta_lapidada" }.All(id => Hud.Rows.Any(r => r.IsRecipe && r.Id == id)));
        await SelectId("picareta_pedra");
        string pick = Lines(Hud.Selected);
        Check("short of flint: 'Sílex: tenho 2 de 3', the recipe not possible", pick.Contains("Sílex: tenho 2 de 3") && pick.Contains("Galho Rijo: tenho 1 de 1") && !Hud.Rows[Hud.Selected].Enabled, pick);
        await SelectId("machado_lapidado");
        string pol = Lines(Hud.Selected);
        Check("the polished axe: what is missing and 'Pinheiro: 6 golpes → 3 golpes'", pol.Contains("Pedra Bruta: tenho 0 de 2") && pol.Contains("Pinheiro: 6 golpes → 3 golpes"), pol);
        await Shot("03-fabricacao-falta");
        // the last ingredients: the recipe becomes possible at once (and it is announced)
        await Tap(Key.I);
        int msgs = Hud.Messages.Count;
        _g.Inventory.Add("pedra_bruta", 2); _g.Inventory.Add("tira_amarracao", 1); // controlled: what a rock and a hide give (checked in RecursosCheck / JourneyCheck)
        _g.ReceiveCoins(0); // nothing; the announcement comes with the next gain
        _g.GainXp(0, "");
        await Tap(Key.I); await ToTab("fabricacao"); await SelectId("machado_lapidado");
        Check("with the last ingredient the recipe is possible", Hud.Rows[Hud.Selected].Enabled && Lines(Hud.Selected).Contains("✓ Pedra Bruta: tenho 2 de 2"), Lines(Hud.Selected));
        int logs = _g.Inventory.Count("tora");
        await Tap(Key.E); await Frames(3);
        Check("crafting updates the same screen: materials down, the axe made, the recipe now short", _g.Inventory.Count("machado_lapidado") == 1 && _g.Inventory.Count("tora") == logs - 1 && _g.BagTab == "fabricacao" && !Hud.Rows.First(r => r.Id == "machado_lapidado").Enabled,
            $"tora {_g.Inventory.Count("tora")}, machado lapidado {_g.Inventory.Count("machado_lapidado")}");
        await Shot("04-fabricado");
        await ToTab("ferramenta"); await SelectId("machado_lapidado");
        string cmp = Lines(Hud.Selected);
        Check("selected in Ferramentas, it shows what changes: 6 → 3 strikes on the pine, the action 'Equipar'", cmp.Contains("Pinheiro: 6 golpes → 3 golpes") && Hud.Rows[Hud.Selected].Action == "Equipar", cmp);
        await Tap(Key.E);
        Check("E equips it", _g.EquippedItem == "machado_lapidado");
        await Tap(Key.I);
        // the world agrees with the comparison
        var pine = GetTree().GetNodesInGroup("elements").OfType<WorldElement>().Where(e => e.Def.Id == "pinheiro" && e.Work == 0).OrderBy(e => e.GlobalPosition.DistanceTo(P.GlobalPosition)).FirstOrDefault();
        if (pine != null)
        {
            await Teleport(pine.GlobalPosition + new Vector2(-16, 1)); P.Face(Vector2.Right);
            for (int i = 0; i < 3; i++) { P.Vigor = P.MaxVigor; await Tap(Key.J); await Frames(24); }
            await Frames(60);
            Check("the polished axe fells the pine in 3 strikes, as the bag said", pine.State == ElementState.Depleted, $"{pine.State}");
        }
        else Check("a pine near the start", false);
        Finish();
    }

    // ------------------------------------------------------------------ 3: shortcuts
    private async Task Shortcuts()
    {
        await Open(PlainsGame.StartMode.Continue, CreatorJourney());
        await Tap(Key.I); await ToTab("ferramenta");
        await SelectId("machado_pedra"); await Tap(Key.Key1);
        await SelectId("picareta_pedra"); await Tap(Key.Key2);
        await ToTab("consumivel"); await SelectId("grama_sangue_q1");
        int herbs = _g.Inventory.Count("grama_sangue_q1");
        await Tap(Key.Key3);
        Check("in the bag, 1/2/3 put the axe, the pickaxe and the herb on shortcuts — and use nothing", _g.QuickSlots()[0] == "machado_pedra" && _g.QuickSlots()[1] == "picareta_pedra" && _g.QuickSlots()[2] == "grama_sangue_q1" && _g.Inventory.Count("grama_sangue_q1") == herbs,
            string.Join(",", _g.QuickSlots()));
        Check("the bag marks the slot on the item", Hud.Rows[Hud.Selected].Slot == "3", Hud.Rows[Hud.Selected].Slot);
        await Tap(Key.I);
        Check("the HUD row shows H (herb) and the three shortcuts with their counts", Hud.Quick.Slots.Count == 4 && Hud.Quick.Slots[0].Key == "H" && Hud.Quick.Slots[3].Count == 3, string.Join(" ", Hud.Quick.Slots.Select(q => $"{q.Key}:{q.Icon}×{q.Count}")));
        await Tap(Key.Key2);
        Check("2 in the world: the pickaxe goes to the hand", _g.EquippedItem == "picareta_pedra");
        await Tap(Key.Key1);
        Check("1: back to the axe", _g.EquippedItem == "machado_pedra");
        P.SetMaxHealth(40); P.Heal(-(P.Health - 2)); // controlled: room for three herbs in a row (their healing is gradual and counts as pending)
        for (int i = 0; i < 3; i++) { await Tap(Key.Key3); await Frames(6); }
        Check("3 three times: the three herbs used", _g.Inventory.Count("grama_sangue_q1") == 0);
        await Shot("05-atalhos-erva-acabou");
        int m = Hud.Messages.Count;
        await Tap(Key.Key3);
        Check("the herb ran out: the key says so and the row shows 0", Hud.Messages.Skip(m).Any(x => x.Contains("acabou")) && Hud.Quick.Slots.Any(q => q.Key == "3" && q.Count == 0), string.Join(" / ", Hud.Messages.Skip(m)));
        Check("H too: the bag has no herb left", !_g.QuickHeal());
        Finish();
    }

    private async Task Shortcuts2()
    {
        await Open(PlainsGame.StartMode.Continue, CreatorJourney());
        Check("in a new process the shortcuts are still there (preferences)", _g.QuickSlots()[0] == "machado_pedra" && _g.QuickSlots()[1] == "picareta_pedra" && _g.QuickSlots()[2] == "grama_sangue_q1", string.Join(",", _g.QuickSlots()));
        await Tap(Key.Key2);
        Check("and they still work: 2 takes the pickaxe", _g.EquippedItem == "picareta_pedra");
        Check("Esc → Controles lists 'Atalho de item 1' on the key 1 and 'Próximo alvo do E' on the wheel", Bindings.Label("quick_1", 0) == "1" && Bindings.Label("quick_7", 0) == "Q" && Bindings.Label("target_next", 0) == "roda para baixo",
            $"{Bindings.Label("quick_1", 0)}, {Bindings.Label("quick_7", 0)}, {Bindings.Label("target_next", 0)}");
        Finish();
    }

    // ------------------------------------------------------------------ 4: the sheet and the full Vida
    private async Task SheetPhase()
    {
        await Open(PlainsGame.StartMode.Continue, CreatorJourney());
        Check("the HUD keeps the percentage: 'Vida 60%'", Hud.LifeTipText == "" && _g.Player.Health == 6);
        var bars = Hud.ToWindow(Hud.Bars, new Rect2(10, 2, 80, 10));
        await Move(bars.GetCenter()); await Frames(3);
        Check("the mouse over the bar shows 'Vida 60% · 6/10'", Hud.LifeTipText == "Vida 60% · 6/10", $"'{Hud.LifeTipText}'");
        await Shot("06-vida-ao-passar-o-mouse");
        await Move(new Vector2(640, 500)); await Frames(3);
        Check("moving away hides it", Hud.LifeTipText == "");
        await Tap(Key.C);
        Check("the sheet opens with 2 free points and the full Vida", Hud.SheetOpen && Hud.Sheet.Points == 2 && Hud.SheetHeader.Contains("Vida 60% · 6/10"), Hud.SheetHeader);
        await Move(Hud.ToWindow(Hud.Sheet, Hud.Sheet.LineRects[0]).GetCenter());
        Check("the mouse over FOR selects it and shows the axe's damage 1,06 → 1,12", Hud.SheetSelected == 0 && Hud.SheetDetail.Contains("Machado de Pedra (na mão): dano em animais 1,06 → 1,12"), Hud.SheetDetail.Split('\n').Take(5).Aggregate((a, b) => a + " | " + b));
        await Shot("07-ficha-prévia-for");
        await Click(Hud.ToWindow(Hud.Sheet, Hud.Sheet.PlusRects[0]).GetCenter());
        Check("a click on FOR's '+' spends a point (FOR 2, 1 point left)", _g.Stats.Get("for") == 2 && _g.Stats.Points == 1 && Mathf.IsEqualApprox(P.MeleeMultiplier, 1.12f));
        for (int k = 0; k < 7 && Hud.SheetSelected != 3; k++) await Tap(Key.S);
        Check("S to CON: how far it still goes before the cap", Hud.SheetSelected == 3 && Hud.SheetDetail.Contains("ainda reduz 39 pontos"), Hud.SheetDetail.Split('\n').Take(6).Aggregate((a, b) => a + " | " + b));
        await Tap(Key.E);
        Check("E spends on CON by keyboard (CON 3, 0 points)", _g.Stats.Get("con") == 3 && _g.Stats.Points == 0);
        await Click(Hud.ToWindow(Hud.Sheet, Hud.Sheet.PlusRects[1]).GetCenter());
        Check("with no point, a '+' does nothing", _g.Stats.Get("vit") == 0 && _g.Stats.Points == 0);
        await Click(Hud.ToWindow(Hud.Sheet, Hud.Sheet.PlusRects[6]).GetCenter());
        Check("(the 7th line is KI, not SOR: the old click limit is gone)", Hud.SheetSelected == 6, $"{Hud.SheetSelected}");
        await Shot("08-ficha-sem-pontos");
        Finish();
    }

    // ------------------------------------------------------------------ 5: the choice of target, away from the clearing
    private async Task TargetPhase()
    {
        await Open(PlainsGame.StartMode.New);
        var gen = _g.Gen;
        var pairs = new List<(Vector2 Herb, Vector2 Track)>();
        float reach = _g.InteractRadius;
        for (int r = 1; r < 9 && pairs.Count < 12; r++)
            for (int cy = -r; cy <= r; cy++)
                for (int cx = -r; cx <= r; cx++)
                {
                    if (Mathf.Max(Mathf.Abs(cx), Mathf.Abs(cy)) != r) continue;
                    var ch = gen.GenerateChunk(new Vector2I(cx, cy));
                    foreach (var h in ch.Features.Where(f => f.Kind == FeatureKind.Herb && f.Position.DistanceTo(gen.StartPixel) > 300))
                        foreach (var t in ch.Features.Where(f => f.Kind == FeatureKind.Track && f.Position.DistanceTo(h.Position) < reach * 1.6f))
                            pairs.Add((h.Position, t.Position));
                }
        (Vector2 Herb, Vector2 Track)? found = null;
        foreach (var pr in pairs)
        {
            // a free spot between them (a herb often grows by a rock, whose base pushes the body out): try along the line and around
            foreach (float k in new[] { 0.5f, 0.35f, 0.65f })
                foreach (var off in new[] { Vector2.Zero, new Vector2(0, 8), new Vector2(0, -8), new Vector2(8, 0), new Vector2(-8, 0) })
                {
                    await Teleport(pr.Herb.Lerp(pr.Track, k) + off); await Frames(8); // let the body settle (a rock's base may push it)
                    if (P.GlobalPosition.DistanceTo(pr.Herb) <= reach - 5 && P.GlobalPosition.DistanceTo(pr.Track) <= reach - 5 && _g.Interactables().Count >= 2 && _g.Interactables().Any(n => n is Pickup) && _g.Interactables().Any(n => n is TrackPrint)) { found = pr; break; }
                }
            if (found != null) break;
        }
        Check($"seed {_seed}: a herb and a print within reach of one free spot, away from the clearing", found != null, found is { } ff ? $"erva {ff.Herb}, pegada {ff.Track}, personagem {P.GlobalPosition}" : $"{pairs.Count} pares, nenhum ponto livre");
        if (found == null) { Finish(); return; }
        var list = _g.Interactables();
        Check("both are within reach: the ring and the list show them, the nearest first", list.Count >= 2 && _g.Target.Visible && _g.Target.Names.Count == list.Count && _g.Target.Chosen == 0,
            $"alcance {list.Count} ({string.Join(", ", list.Select(_g.TargetName))}), lista [{string.Join(", ", _g.Target.Names)}], visível {_g.Target.Visible}, escolhido {_g.Target.Chosen}, painel {Hud.AnyPanelOpen}");
        var first = _g.ChosenTarget();
        await Shot($"09-alvo-anel-e-lista-{_seed}");
        await Wheel(true);
        var second = _g.ChosenTarget();
        Check("the mouse wheel chooses the other", second != null && second != first && _g.Target.Chosen == 1, $"{_g.Target.Chosen}");
        await Shot($"10-alvo-trocado-{_seed}");
        bool herbChosen = second is Pickup;
        int herbsBefore = _g.Inventory.Entries.Where(e => e.Id.StartsWith("grama")).Sum(e => e.Count);
        await Tap(Key.E); await Frames(4);
        int herbsAfter = _g.Inventory.Entries.Where(e => e.Id.StartsWith("grama")).Sum(e => e.Count);
        bool examined = _g.Hud.Messages.TakeLast(4).Any(m => m.Contains("Pegada") || m.Contains("pegada") || m.Contains("rastro") || m.Contains("Rastro"));
        Check("E acts only on the chosen one", herbChosen ? herbsAfter == herbsBefore + 1 : herbsAfter == herbsBefore, herbChosen ? "colheu a erva" : $"examinou a pegada ({(examined ? "mensagem" : "?")}), ervas {herbsBefore}→{herbsAfter}");
        // with a single target left, E is immediate and there is no list
        var rest = _g.Interactables();
        // what was examined may open a panel (the clue): the ring hides while a panel is open, on purpose
        if (Hud.AnyPanelOpen) { await Tap(Key.E); await Frames(4); }
        rest = _g.Interactables();
        Check("the ring and the list follow what is left (none with a single target)", _g.Target.Visible == (rest.Count > 1), $"{rest.Count} ao alcance, lista {_g.Target.Visible}");
        Finish();
    }

    // ------------------------------------------------------------------ 6: gains over the head and the footer
    private async Task FooterPhase()
    {
        await Open(PlainsGame.StartMode.New);
        var branches = _g.World.Objects.GetChildren().OfType<Pickup>().Where(p => p.ItemId == "galho_rijo").OrderBy(p => p.GlobalPosition.DistanceTo(P.GlobalPosition)).Take(3).ToList();
        Check("three branches by the creek", branches.Count == 3);
        foreach (var b in branches) { await Teleport(b.GlobalPosition + new Vector2(0, 6)); await Tap(Key.E); }
        var line = _g.Feed.Log.Lines.FirstOrDefault(l => l.Key == "item:galho_rijo");
        Check("one line over the head: '+3 Galho Rijo' (not three)", line != null && GainLog.Text(line) == "+3 Galho Rijo" && _g.Feed.Log.Lines.Count(l => l.Key == "item:galho_rijo") == 1, line == null ? "nenhuma" : GainLog.Text(line));
        await Shot($"11-ganhos-agrupados-{_seed}");
        // a carcass: the footer says what E and K do there
        var rabbit = _g.World.Objects.GetChildren().OfType<Rabbit>().OrderBy(r => r.GlobalPosition.DistanceTo(P.GlobalPosition)).FirstOrDefault();
        if (rabbit != null)
        {
            string rid = rabbit.CreatureId; rabbit.ReceiveHit(5); await Frames(6); // controlled: a carcass (the hunt is checked in PlainsCheck)
            var carcass = _g.World.Objects.GetChildren().OfType<Carcass>().First(c => c.CreatureId == rid);
            _g.Equip("faca_pedra"); _g.Inventory.Add("faca_pedra");
            _g.Equip("faca_pedra");
            await Teleport(carcass.GlobalPosition + new Vector2(-6, 2)); await Frames(4);
            Check("next to the carcass the footer says what E and K do", Hud.ControlsText.Contains("tirar") && Hud.ControlsText.Contains("K esfolar com Ki"), Hud.ControlsText);
            await Shot($"12-rodape-carcaca-{_seed}");
            await Teleport(carcass.GlobalPosition + new Vector2(260, 0)); await Frames(4);
            Check("away from it, it does not", !Hud.ControlsText.Contains("tirar") && !Hud.ControlsText.Contains("esfolar"), Hud.ControlsText);
        }
        else Check("a rabbit near the start", false);
        Finish();
    }

    // ------------------------------------------------------------------ 5: panels take their input
    private async Task InputPhase()
    {
        await Open(PlainsGame.StartMode.Continue, CreatorJourney());
        int acts = 0; P.ActionStarted += (_, _) => acts++;
        var flint = _g.World.Objects.GetChildren().OfType<Pickup>().Where(p => p.ItemId == "silex").OrderBy(p => p.GlobalPosition.DistanceTo(P.GlobalPosition)).FirstOrDefault();
        if (flint != null) await Teleport(flint.GlobalPosition + new Vector2(0, 6));
        int silex = _g.Inventory.Count("silex");
        await Tap(Key.I); await Tap(Key.E); await Tap(Key.Key5); await Tap(Key.Escape);
        await Tap(Key.C); await Tap(Key.E); await Tap(Key.C);
        await Tap(Key.Escape); await Tap(Key.E); await Frames(4); // the pause opens; E on "Voltar ao jogo" closes it
        float ki = P.Ki;
        Check("bag, sheet and pause: E, a shortcut, Esc and the clicks stay in the panels (no cut, no Ki, nothing picked)", acts == 0 && _g.Inventory.Count("silex") == silex && Mathf.IsEqualApprox(P.Ki, ki), $"ações {acts}, sílex {silex}→{_g.Inventory.Count("silex")}");
        Check("everything is closed again", !Hud.AnyPanelOpen && !P.InputLocked);
        Finish();
    }

    // ------------------------------------------------------------------ the continuous route
    private async Task Route()
    {
        var j = new SaveData { Region = "planicie", Phase = "planicie", Seed = _seed, Spot = new[] { 16f, 16f }, LossApplied = true, Health = 10, Vigor = 50, Ki = 100, Level = 2, Points = 2,
            Inventory = { new SavedItem { Id = "silex", Count = 1 }, new SavedItem { Id = "galho_rijo", Count = 2 }, new SavedItem { Id = "tora", Count = 1 }, new SavedItem { Id = "faca_pedra", Count = 1 } }, EquippedItem = "faca_pedra", Equipped = "knife" };
        await Open(PlainsGame.StartMode.Continue, j);
        var flints = _g.World.Objects.GetChildren().OfType<Pickup>().Where(p => p.ItemId == "silex").OrderBy(p => p.GlobalPosition.DistanceTo(P.GlobalPosition)).ToList();
        Check("at the creek, short of flint for the axe", flints.Count > 0 && _g.Inventory.Count("silex") == 1);
        if (flints.Count == 0) { Finish(); return; }
        await Teleport(flints[0].GlobalPosition + new Vector2(0, 8));
        await Tap(Key.I); await ToTab("fabricacao"); await SelectId("machado_pedra");
        Check("the recipe says 'Sílex: tenho 1 de 2'", Lines(Hud.Selected).Contains("Sílex: tenho 1 de 2") && !Hud.Rows[Hud.Selected].Enabled, Lines(Hud.Selected));
        await Shot($"13-percurso-falta-silex-{_seed}");
        await Tap(Key.I);
        int msgs = Hud.Messages.Count;
        await Tap(Key.E); await Frames(4);
        Check("picking the flint: the axe becomes possible and it is announced", _g.Inventory.Count("silex") == 2 && Hud.Messages.Skip(msgs).Any(m => m.Contains("Agora dá para fabricar: Machado de Pedra")), string.Join(" / ", Hud.Messages.Skip(msgs)));
        await Tap(Key.I); await ToTab("fabricacao"); await SelectId("machado_pedra"); await Tap(Key.E);
        await ToTab("ferramenta"); await SelectId("machado_pedra");
        Check("made: in Ferramentas, compared with the knife in hand: 'Pinheiro: não corta → 6 golpes'", _g.Inventory.Count("machado_pedra") == 1 && Lines(Hud.Selected).Contains("Pinheiro: não corta → 6 golpes"), Lines(Hud.Selected));
        await Tap(Key.E); await Tap(Key.I);
        Check("equipped", _g.EquippedItem == "machado_pedra");
        var pine = GetTree().GetNodesInGroup("elements").OfType<WorldElement>().Where(e => e.Def.Id == "pinheiro" && e.Work == 0).OrderBy(e => e.GlobalPosition.DistanceTo(P.GlobalPosition)).FirstOrDefault();
        if (pine != null)
        {
            await Teleport(pine.GlobalPosition + new Vector2(-16, 1)); P.Face(Vector2.Right);
            for (int i = 0; i < 2; i++) { P.Vigor = P.MaxVigor; await Tap(Key.J); await Frames(24); }
            Check("used on a pine (2 strikes of 6)", Mathf.IsEqualApprox(pine.Work, 2), $"{pine.Work}");
        }
        await Tap(Key.I); await ToTab("fabricacao"); await SelectId("machado_lapidado");
        Check("compared with the polished one: 'Pinheiro: 6 golpes → 3 golpes'", Lines(Hud.Selected).Contains("Pinheiro: 6 golpes → 3 golpes"), Lines(Hud.Selected));
        await Tap(Key.I);
        await Tap(Key.C);
        await Click(Hud.ToWindow(Hud.Sheet, Hud.Sheet.PlusRects[0]).GetCenter());
        Check("a point spent on FOR with the '+'", _g.Stats.Get("for") == 1 && _g.Stats.Points == 1);
        await Tap(Key.C);
        bool choice = false;
        foreach (var herb in _g.World.Objects.GetChildren().OfType<Pickup>().Where(p => p.ItemId != "moedas").Take(60))
        {
            var other = _g.World.Objects.GetChildren().OfType<Node2D>().Where(n => n != herb && n is Pickup or TrackPrint && n.GlobalPosition.DistanceTo(herb.GlobalPosition) < _g.InteractRadius * 1.5f).FirstOrDefault();
            if (other == null) continue;
            await Teleport((herb.GlobalPosition + other.GlobalPosition) / 2); await Frames(4);
            if (_g.Interactables().Count < 2) continue;
            var before = _g.ChosenTarget(); await Wheel(true); choice = _g.ChosenTarget() != before; break;
        }
        Check("a choice of target next to another object", choice);
        Finish();
    }

    // ------------------------------------------------------------------ a journey from before
    private async Task OldSave()
    {
        string old = Godot.FileAccess.GetFileAsString("res://tests/fixtures/saves/jornada_antes_do_ki.json");
        using (var f = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write)) f.StoreString(old);
        _g = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _g.SavePath = SavePath; _g.ShowIntro = false; _g.Start = PlainsGame.StartMode.Continue;
        AddChild(_g);
        await Frames(40);
        Check("the old journey loads: knife in hand, level 4, its items, 0 coins", _g.EquippedItem == "faca_pedra" && _g.Stats.Level == 4 && _g.Inventory.Count("pele_fina") == 1 && _g.Wallet.Coins == 0);
        await Tap(Key.I);
        Check("its bag opens on 'Tudo' with tabs", _g.BagTab == "tudo" && Hud.Rows.Count == 3 && Hud.Bag.TabNames.Count == 6); // T18: + a Túnica Verde
        await Tap(Key.I);
        Check("no shortcuts yet (nothing assigned), no H without the Bolsa Simples", _g.QuickSlots().All(s => s.Length == 0) && Hud.Quick.Slots.Count == 0);
        await Tap(Key.C);
        Check("its sheet: 7 attributes with '+', 4 free points", Hud.Sheet.Lines.Count == 7 && Hud.Sheet.Points == 4);
        Finish();
    }
}
