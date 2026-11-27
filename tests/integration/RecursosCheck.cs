using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Elements;
using SpiritOfMurin.Game;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Tests;

/// <summary>
/// Pedido 14 — campaign delivery 5: trees, rocks, bamboo and bushes, with a window (monitor 2). Phases, each its own run:
/// <list type="bullet">
/// <item><c>--phase=work --seed=N</c>: from flint and branches (given by the test) the stone axe and pickaxe are made and
/// equipped; a pine takes the axe's strikes (notches, the work bar), refuses the knife and the pickaxe, falls on the
/// last strike and leaves a stump (smaller base), giving its logs once; a veined rock refuses the axe and breaks with
/// the pickaxe into rubble that does not collide; a decorative rock tells it has no vein; a bamboo clump gives way to the
/// knife; a second pine is left half cut; leaving the chunk and coming back keeps the stump, the half cut and the
/// inventory (no second drop); the logs and stones make the polished axe, which cuts in fewer strikes; meditating saves.</item>
/// <item><c>--phase=resume --seed=N</c>: Continuar — the stump, the half-cut pine and the broken rock are as they were,
/// the inventory too, and striking the stump gives nothing.</item>
/// <item><c>--phase=bush --seed=N</c>: a rabbit (the real one, moved by the test along a line) crosses a bush: the leaves
/// part toward its way and spring back; the swordsman walking through does the same; during a gust too.</item>
/// <item><c>--phase=oldsave</c>: a journey saved before this delivery (<c>tests/fixtures/saves/jornada_antes_do_ki.json</c>)
/// loads: every element standing, the knife still in hand and cutting bamboo.</item>
/// </list>
/// Files: <c>user://teste_rec5.json</c>; preferences <c>user://teste_rec5_prefs.json</c>. Pictures with <c>--shots=</c>.
/// Teleports and the items given are named in the checks.
/// </summary>
public partial class RecursosCheck : Node
{
    private const string SavePath = "user://teste_rec5.json";
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private string _phase = "work";
    private string? _shots;
    private int _seed = 20261126;
    private PlainsGame _g = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        foreach (string a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--phase=")) _phase = a["--phase=".Length..];
            if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..];
            if (a.StartsWith("--seed=") && int.TryParse(a["--seed=".Length..], out int s)) _seed = s;
        }
        GD.Print($"window on screen {DisplayServer.WindowGetCurrentScreen()} of {DisplayServer.GetScreenCount()} ({DisplayServer.GetName()})");
        if (_phase is "work" or "bush" or "oldsave") SaveGame.Erase(SavePath);
        if (_phase != "resume" && Godot.FileAccess.FileExists(Prefs.Path)) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(Prefs.Path));
        Bindings.Init(); Assist.Reload(); Aim.Reload(); Aim.UseForSession(false);
        _ = _phase switch { "resume" => Resume(), "bush" => Bush(), "oldsave" => OldSave(), _ => Work() };
    }

    // ------------------------------------------------------------------ helpers
    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private async Task Flush() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static void Press(Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });
    private async Task Tap(Key k) { Press(k, true); await Flush(); await Frames(2); Press(k, false); await Flush(); await Frames(2); }
    private void Finish() { int f = _results.Count(r => !r.Ok); GD.Print($"recursos {_phase} seed {_seed}: {_results.Count - f}/{_results.Count} OK"); GetTree().Quit(f == 0 ? 0 : 1); }
    private Player P => _g.Player;

    private async Task Shot(string name)
    {
        if (_shots == null || DisplayServer.GetName() == "headless") return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        DirAccess.MakeDirRecursiveAbsolute(_shots);
        GetViewport().GetTexture().GetImage().SavePng($"{_shots}/{name}-{_seed}.png"); // the game's picture, never the desktop
    }

    private async Task Open(PlainsGame.StartMode mode, SaveData? write = null)
    {
        if (write != null) SaveGame.Write(SavePath, write);
        _g = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _g.SavePath = SavePath; _g.ShowIntro = false; _g.Start = mode; _g.Seed = _seed;
        AddChild(_g);
        await Frames(40);
    }

    private IEnumerable<WorldElement> Elements(string family) =>
        GetTree().GetNodesInGroup("elements").OfType<WorldElement>().Where(e => e.Def.Family == family);

    // the nearest element of a kind that is standing and not yet worked, searching around the start
    private async Task<WorldElement?> Find(System.Func<WorldElement, bool> what, params string[] exclude)
    {
        for (int ring = 0; ring < 6; ring++)
        {
            var e = GetTree().GetNodesInGroup("elements").OfType<WorldElement>()
                .Where(x => what(x) && x.State == ElementState.Standing && x.Work == 0 && !exclude.Contains(x.PlaceId))
                .OrderBy(x => x.GlobalPosition.DistanceTo(P.GlobalPosition)).FirstOrDefault();
            if (e != null) return e;
            P.GlobalPosition = _g.Gen.StartPixel + new Vector2(Mathf.Cos(ring * 1.3f), Mathf.Sin(ring * 1.3f)) * (500 + ring * 300); // named: searching further
            await Frames(20);
        }
        return null;
    }

    // next to the element on dry ground (left, right or below it), facing it
    private async Task Beside(WorldElement e)
    {
        float off = e.Def.BaseHalfWidth + 9;
        Vector2 spot = e.GlobalPosition + new Vector2(-off, 1), face = Vector2.Right;
        foreach (var (o, f) in new[] { (new Vector2(-off, 1), Vector2.Right), (new Vector2(off, 1), Vector2.Left), (new Vector2(0, 14), Vector2.Up) })
        {
            var t = _g.Gen.TileOfPixel(e.GlobalPosition + o);
            if (_g.Gen.TileAt(t.X, t.Y) != TileKind.Water) { spot = e.GlobalPosition + o; face = f; break; }
        }
        P.GlobalPosition = spot; P.Face(face);
        P.GetNode<Camera2D>("Camera").ResetSmoothing(); await Frames(4);
        P.GlobalPosition = spot; P.Face(face); await Frames(2);
    }

    private async Task Strike(int times)
    {
        for (int i = 0; i < times; i++) { P.Vigor = P.MaxVigor; await Tap(Key.J); await Frames(26); }
    }

    // ------------------------------------------------------------------ work
    private async Task Work()
    {
        await Open(PlainsGame.StartMode.New);
        // controlled: the flint and branches a walk along the creek gives (the gathering itself is checked in PlainsCheck)
        _g.Inventory.Add("silex", 5); _g.Inventory.Add("galho_rijo", 3); _g.Inventory.Add("faca_pedra", 1);
        Check("the stone axe is made from 2 flint + 2 branches", _g.Craft("machado_pedra") && _g.Inventory.Count("machado_pedra") == 1 && _g.Inventory.Count("silex") == 3);
        Check("the stone pickaxe is made from 3 flint + 1 branch", _g.Craft("picareta_pedra") && _g.Inventory.Count("picareta_pedra") == 1 && _g.Inventory.Count("silex") == 0);
        _g.UseItem("machado_pedra");
        Check("the axe goes to the hand", _g.EquippedItem == "machado_pedra" && P.Equipped == Player.Weapon.Axe, _g.EquippedItem);

        // a pine: wrong tools first, then the axe
        var pine = await Find(e => e.Def.Id == "pinheiro");
        Check("a workable pine near the start", pine != null);
        if (pine == null) { Finish(); return; }
        await Beside(pine);
        Check("the prompt says what J does with the axe", _g.Hud.PromptText.Contains("cortar") && _g.Hud.PromptText.Contains("golpes"), _g.Hud.PromptText);
        _g.UseItem("faca_pedra"); await Strike(1);
        Check("the knife does not cut the pine (refused, no work)", pine.Work == 0 && _g.Hud.Messages.LastOrDefault()?.Contains("machado") == true, _g.Hud.Messages.LastOrDefault() ?? "");
        _g.UseItem("picareta_pedra"); await Frames(100); await Strike(1);
        Check("the pickaxe does not cut the pine either", pine.Work == 0);
        _g.UseItem("machado_pedra");
        await Strike(2);
        Check("two axe strikes: work 2 of 6, the notches show (progress 1/3)", Mathf.IsEqualApprox(pine.Work, 2) && Mathf.IsEqualApprox(ResourceRules.Progress(pine.Def, pine.Work), 2f / 6, 0.01f), $"trabalho {pine.Work}");
        Check("the work is in the world state", Mathf.IsEqualApprox(_g.World.State.WorkOf(pine.PlaceId), 2));
        await Shot("01-pinheiro-entalhado");
        int logs0 = _g.Inventory.Count("tora");
        await Strike(4);
        Check("the sixth strike fells the pine", pine.State is ElementState.Falling or ElementState.Depleted, $"{pine.State}");
        await Frames(10); await Shot("02-pinheiro-caindo");
        await Frames(60);
        int logs = _g.Inventory.Count("tora") - logs0;
        Check("it gives 2-3 logs, once", logs is >= 2 and <= 3, $"{logs} toras");
        Check("a stump stays: smaller base (4×5 px)", pine.State == ElementState.Depleted && pine.BaseNow == (4f, 5f), $"{pine.State} base {pine.BaseNow}");
        await Strike(1);
        Check("striking the stump gives nothing more", _g.Inventory.Count("tora") - logs0 == logs);
        await Shot("03-toco");
        string pineId = pine.PlaceId;

        // a decorative rock and a veined rock
        var deco = await Find(e => e.Def.Family == "rocha" && !e.Def.Veined);
        _g.UseItem("picareta_pedra");
        if (deco != null)
        {
            await Beside(deco); await Frames(100); await Strike(1);
            Check("a rock without vein: the pickaxe tells it has none", deco.Work == 0 && _g.Hud.Messages.LastOrDefault()?.Contains("veio") == true, _g.Hud.Messages.LastOrDefault() ?? "");
        }
        var rock = await Find(e => e.Def.Veined);
        Check("a veined rock (mineable) in reach", rock != null);
        if (rock != null)
        {
            await Beside(rock); await Shot("04-rocha-com-veio");
            _g.UseItem("machado_pedra"); await Frames(100); await Strike(1);
            Check("the axe does not break the rock", rock.Work == 0);
            _g.UseItem("picareta_pedra");
            await Strike(3);
            Check("three pickaxe strikes: cracks (work 3 of 5)", Mathf.IsEqualApprox(rock.Work, 3), $"{rock.Work}");
            await Shot("05-rocha-rachada");
            int st0 = _g.Inventory.Count("pedra_bruta");
            await Strike(2); await Frames(40);
            Check("the rock breaks: 2-3 stones, rubble does not collide", rock.State == ElementState.Depleted && _g.Inventory.Count("pedra_bruta") - st0 is >= 2 and <= 3 && rock.BaseNow == (0f, 0f), $"{rock.State}, pedras {_g.Inventory.Count("pedra_bruta") - st0}");
            await Shot("06-cascalho");
        }

        // bamboo with the knife
        var bamboo = await Find(e => e.Def.Family == "bambu");
        Check("a bamboo clump", bamboo != null);
        if (bamboo != null)
        {
            _g.UseItem("faca_pedra");
            await Beside(bamboo);
            await Shot("07-bambu");
            await Strike(1); await Frames(6);
            Check("one knife cut: a stalk gives way (1 of 3)", Mathf.IsEqualApprox(bamboo.Work, 1));
            await Shot("08-bambu-cedendo");
            int v0 = _g.Inventory.Count("vara_bambu");
            await Strike(2); await Frames(40);
            Check("three cuts: the clump is down to stubs, 2-3 rods", bamboo.State == ElementState.Depleted && _g.Inventory.Count("vara_bambu") - v0 is >= 2 and <= 3);
        }

        // a second pine left half cut, then leave the chunk and come back
        var half = await Find(e => e.Def.Id == "pinheiro", pineId);
        string halfId = half?.PlaceId ?? "";
        if (half != null)
        {
            _g.UseItem("machado_pedra"); await Beside(half); await Strike(2);
            Check("a second pine half cut (2 of 6)", Mathf.IsEqualApprox(half.Work, 2));
            Vector2 back = half.GlobalPosition + new Vector2(-20, 4);
            var inv = _g.Inventory.Entries.ToDictionary(e => e.Id, e => e.Count);
            P.GlobalPosition = back + new Vector2(4000, 0); await Frames(60); // named: far away, the chunks are freed
            bool freed = !IsInstanceValid(half) || !half.IsInsideTree();
            P.GlobalPosition = back; await Frames(60);
            var again = Elements("arvore").FirstOrDefault(e => e.PlaceId == halfId);
            var stump = Elements("arvore").FirstOrDefault(e => e.PlaceId == pineId);
            Check("leaving the chunk frees it; coming back rebuilds it", freed && again != null);
            Check("the half-cut pine is still half cut", again != null && Mathf.IsEqualApprox(again.Work, 2) && again.State == ElementState.Standing, again == null ? "sumiu" : $"{again.Work}");
            Check("the felled pine is still a stump", stump == null || stump.State == ElementState.Depleted, stump == null ? "fora do alcance" : $"{stump.State}");
            Check("nothing was given twice", _g.Inventory.Entries.All(e => inv.TryGetValue(e.Id, out int n) && n == e.Count));
        }

        // the materials' use: logs + stones + a strip → the polished axe (power 2)
        _g.Inventory.Add("tira_amarracao", 1); // controlled: the strip comes from a hide (hunting is checked in JourneyCheck)
        bool made = _g.Craft("machado_lapidado");
        Check("logs and stones make the polished axe", made && _g.Inventory.Count("machado_lapidado") == 1, $"tora {_g.Inventory.Count("tora")}, pedra {_g.Inventory.Count("pedra_bruta")}");
        if (made)
        {
            _g.UseItem("machado_lapidado");
            var third = await Find(e => e.Def.Id == "pinheiro", pineId, halfId);
            if (third != null)
            {
                await Beside(third); await Strike(3); await Frames(60);
                Check("the polished axe fells a pine in 3 strikes", third.State == ElementState.Depleted, $"{third.State}, trabalho {third.Work}");
            }
        }

        // meditate: the journey is written (the real mat, E)
        var mat = _g.World.Objects.GetChildren().OfType<MeditationMat>().FirstOrDefault();
        if (mat != null) { P.GlobalPosition = mat.GlobalPosition + new Vector2(0, 6); await Frames(6); await Tap(Key.E); await Frames(30); }
        var saved = SaveGame.Read(SavePath).Data;
        Check("meditating saved the worked elements", saved != null && saved.Worked.ContainsKey(pineId) && saved.Worked.ContainsKey(halfId), saved == null ? "sem save" : string.Join(",", saved.Worked.Keys));
        Finish();
    }

    // ------------------------------------------------------------------ resume
    private async Task Resume()
    {
        var before = SaveGame.Read(SavePath).Data;
        if (before == null) { Check("the journey of the work phase", false); Finish(); return; }
        await Open(PlainsGame.StartMode.Continue);
        var worked = before.Worked;
        Check("Continuar: the worked elements came back", worked.All(kv => Mathf.IsEqualApprox(_g.World.State.WorkOf(kv.Key), kv.Value)), string.Join(",", worked.Select(kv => $"{kv.Key}={kv.Value}")));
        Check("the inventory is as it was (logs, stones, rods, tools)", before.Inventory.All(i => _g.Inventory.Count(i.Id) == i.Count));
        Check("the item in hand came back", _g.EquippedItem == before.EquippedItem, $"{_g.EquippedItem}");
        // walk to a used-up pine: it is a stump, striking gives nothing
        var used = worked.FirstOrDefault(kv => kv.Key.StartsWith("pine") && kv.Value >= 6).Key;
        if (used != null)
        {
            var tile = used[(used.IndexOf('@') + 1)..].Split(',');
            P.GlobalPosition = new Vector2(int.Parse(tile[0]) * 32 + 16, int.Parse(tile[1]) * 32 + 40); await Frames(40);
            var stump = Elements("arvore").FirstOrDefault(e => e.PlaceId == used);
            Check("the felled pine is a stump after Continuar", stump != null && stump.State == ElementState.Depleted);
            if (stump != null)
            {
                int logs = _g.Inventory.Count("tora");
                _g.UseItem(_g.Inventory.Count("machado_pedra") > 0 ? "machado_pedra" : "machado_lapidado");
                await Beside(stump); await Strike(1);
                Check("striking it gives no more logs", _g.Inventory.Count("tora") == logs);
                await Shot("09-toco-depois-de-continuar");
            }
        }
        Finish();
    }

    // ------------------------------------------------------------------ bushes
    private async Task Bush()
    {
        await Open(PlainsGame.StartMode.New);
        var bush = Elements("arbusto").OrderBy(b => b.GlobalPosition.DistanceTo(P.GlobalPosition)).FirstOrDefault();
        Check("a bush (passable, reacts to contact)", bush != null && bush.BaseNow == (0f, 0f) && bush.Def.ContactRadius > 0);
        if (bush == null) { Finish(); return; }
        // a real rabbit, its brain paused, carried across the bush by the test (a steady, known path)
        var rabbit = _g.World.Objects.GetChildren().OfType<Rabbit>().OrderBy(r => r.GlobalPosition.DistanceTo(bush.GlobalPosition)).FirstOrDefault();
        Check("a rabbit to cross it", rabbit != null);
        if (rabbit != null)
        {
            rabbit.ProcessMode = ProcessModeEnum.Disabled;
            P.GlobalPosition = bush.GlobalPosition + new Vector2(0, 60); P.GetNode<Camera2D>("Camera").ResetSmoothing();
            Vector2 a = bush.GlobalPosition + new Vector2(-40, 2), b = bush.GlobalPosition + new Vector2(40, 2);
            float maxX = 0; bool shot = false;
            for (int i = 0; i <= 60; i++)
            {
                rabbit.GlobalPosition = a.Lerp(b, i / 60f); rabbit.Velocity = (b - a) / 1f;
                await Frames(1);
                maxX = Mathf.Max(maxX, bush.Bend.X);
                if (!shot && bush.Bend.X > 2.5f) { shot = true; await Shot("10-arbusto-coelho-passando"); }
            }
            rabbit.Velocity = Vector2.Zero;
            Check("the rabbit crossing to the right: the leaves lean right (toward its way)", maxX > 2, $"inclinação máx. {maxX:0.0} px");
            await Frames(90);
            Check("then they spring back to rest", bush.Bend.Length() < 0.6f, $"{bush.Bend.Length():0.00} px");
            rabbit.ProcessMode = ProcessModeEnum.Inherit;
        }
        // the swordsman walking through it, during a gust
        Wind.T = WindGustTime();
        P.GlobalPosition = bush.GlobalPosition + new Vector2(30, 1); await Frames(4);
        float minX = 0;
        Press(Key.A, true); await Flush();
        for (int i = 0; i < 40; i++) { await Frames(1); minX = Mathf.Min(minX, bush.Bend.X); }
        Press(Key.A, false); await Flush();
        Check("the swordsman walking left through it (in a gust): the leaves lean left", minX < -2, $"{minX:0.0} px");
        Check("the bush does not stop the walk", P.GlobalPosition.X < bush.GlobalPosition.X - 5, $"{P.GlobalPosition.X - bush.GlobalPosition.X:0} px");
        Finish();
    }

    private static float WindGustTime() => Wind.Def.GustPeriod * 0.5f;

    // ------------------------------------------------------------------ a journey from before
    private async Task OldSave()
    {
        string old = Godot.FileAccess.GetFileAsString("res://tests/fixtures/saves/jornada_antes_do_ki.json");
        using (var f = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write)) f.StoreString(old);
        _seed = 20261126;
        _g = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _g.SavePath = SavePath; _g.ShowIntro = false; _g.Start = PlainsGame.StartMode.Continue;
        AddChild(_g);
        await Frames(40);
        Check("an old journey loads: the knife in hand", _g.EquippedItem == "faca_pedra" && P.Equipped == Player.Weapon.Knife, _g.EquippedItem);
        Check("every element near it is standing (no work saved before)", Elements("arvore").Concat(Elements("rocha")).All(e => e.State == ElementState.Standing && e.Work == 0));
        var bamboo = await Find(e => e.Def.Family == "bambu");
        if (bamboo != null)
        {
            await Beside(bamboo); await Strike(1);
            Check("its knife cuts bamboo", Mathf.IsEqualApprox(bamboo.Work, 1));
        }
        Finish();
    }
}
