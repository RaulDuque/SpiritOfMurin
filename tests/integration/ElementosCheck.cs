using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Elements;
using SpiritOfMurin.Items;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Tests;

/// <summary>
/// The element standard's gate (pedido 14, items 2 and 4): every element in <c>assets/data/elements</c> — the set is
/// read from the folder, never listed here, so an element added by <c>/create</c> is covered without touching this file.
/// <list type="bullet">
/// <item><c>--phase=catalogo</c> (an isolated scene, no world): each element file loads; its id matches its file; its
/// art exists; it grows somewhere; a workable one names real items, takes strikes of its own tool until used up, refuses
/// another tool, falls and gives materials that exist in the item catalog; a decorative one takes no work.</item>
/// <item><c>--phase=mundo --seed=N</c>: in the Planície every element family appears around the start, and every
/// workable element found answers its tool there (named: the swordsman is placed next to it).</item>
/// </list>
/// An empty folder is a failure ("NOT ASSESSED"), never a pass. Pictures with <c>--shots=</c>.
/// </summary>
public partial class ElementosCheck : Node2D
{
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private string _phase = "catalogo", _shots = "";
    private int _seed = 20261126;

    /// <inheritdoc />
    public override void _Ready()
    {
        foreach (string a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--phase=")) _phase = a["--phase=".Length..];
            if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..];
            if (a.StartsWith("--seed=") && int.TryParse(a["--seed=".Length..], out int s)) _seed = s;
        }
        _ = Guard(_phase == "mundo" ? World() : Catalog());
    }

    // an exception inside a phase must fail the run, never leave the window open (a check that did not run is no pass)
    private async Task Guard(Task t)
    {
        try { await t; }
        catch (System.Exception ex) { Check("NOT ASSESSED: the phase stopped on an exception", false, ex.GetType().Name + ": " + ex.Message); Finish(); }
    }

    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private void Finish() { int f = _results.Count(r => !r.Ok); GD.Print($"elementos {_phase}: {_results.Count - f}/{_results.Count} OK"); GetTree().Quit(f == 0 ? 0 : 1); }

    // every file under the folder, with the id its name gives
    private static List<(string File, string Id)> Files()
    {
        var list = new List<(string, string)>();
        void Walk(string dir)
        {
            using var d = DirAccess.Open(dir);
            if (d == null) return;
            foreach (var sub in d.GetDirectories()) Walk($"{dir}/{sub}");
            foreach (var f in d.GetFiles()) if (f.EndsWith(".json")) list.Add(($"{dir}/{f}", f[..^5]));
        }
        Walk(ElementCatalog.DefaultFolder);
        return list;
    }

    // ------------------------------------------------------------------ the catalogue, isolated
    private async Task Catalog()
    {
        RenderingServer.SetDefaultClearColor(new Color("#56703f"));
        var files = Files();
        var cat = ElementCatalog.Load();
        var items = ItemCatalog.Load();
        if (files.Count == 0) { Check("NOT ASSESSED: no element file in assets/data/elements", false); Finish(); return; }
        Check($"{files.Count} element files found", files.Count == cat.All.Count, string.Join(", ", files.Select(f => f.Id)));
        float x = 40;
        foreach (var (file, idFromFile) in files)
        {
            var d = cat.Get(idFromFile);
            if (d == null) { Check($"{idFromFile}: loads", false, file); continue; }
            Check($"{d.Id}: the id matches the file name (stable id)", d.Id == idFromFile, file);
            Check($"{d.Id}: its art '{d.Art}' exists", ElementArts.Has(d.Art));
            Check($"{d.Id}: grows somewhere (habitats)", d.Habitats.Values.Any(w => w > 0));
            var e = new WorldElement { Def = d, PlaceId = $"teste#{d.Id}", Variation = 7, Position = new Vector2(x, 200) };
            AddChild(e); x += 70;
            await Frames(2);
            if (!d.Workable) { Check($"{d.Id}: decorative, takes no work", e.Strike("machado", 1, Vector2.Zero) == WorkResult.NotWorkable); continue; }
            Check($"{d.Id}: drops and bonus are real items", d.Drops.Concat(d.Bonus.Select(b => new DropDef { Item = b.Item })).All(i => items.Items.ContainsKey(i.Item)),
                string.Join(", ", d.Drops.Select(i => i.Item).Concat(d.Bonus.Select(b => b.Item))));
            string other = new[] { "machado", "picareta", "faca", "pá" }.First(t => !d.Tools.Contains(t));
            Check($"{d.Id}: refuses a tool it does not take ({other})", e.Strike(other, 1, Vector2.Zero) == WorkResult.WrongTool && e.Work == 0);
            bool used = false; e.UsedUp += _ => used = true;
            int strikes = 0;
            while (e.State == ElementState.Standing && strikes < 50) { e.Strike(d.Tools[0], 1, e.Position + new Vector2(-20, 0)); strikes++; }
            await Frames(70);
            var got = ResourceRules.Yield(d, _seed, e.PlaceId, 0);
            Check($"{d.Id}: {d.Tools[0]} uses it up in {Mathf.CeilToInt(d.Toughness)} strikes, it falls and gives materials once", used && strikes == Mathf.CeilToInt(d.Toughness) && e.State == ElementState.Depleted && got.Count > 0,
                $"{strikes} golpes, {e.State}, {string.Join(", ", got.Select(g => $"{g.Count} {g.Item}"))}");
        }
        if (_shots.Length > 0)
        {
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            DirAccess.MakeDirRecursiveAbsolute(_shots);
            GetViewport().GetTexture().GetImage().SavePng($"{_shots}/catalogo-esgotados.png");
        }
        Finish();
    }

    // ------------------------------------------------------------------ in the generated world
    private async Task World()
    {
        var g = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        g.SavePath = "user://teste_elem.json"; g.ShowIntro = false; g.Start = PlainsGame.StartMode.New; g.Seed = _seed;
        SaveGame.Erase(g.SavePath);
        AddChild(g);
        await Frames(40);
        // what was seen, as plain data (chunks are freed while walking: never keep the nodes)
        var seen = new Dictionary<string, (string Family, Vector2 At, string Place)>();
        for (int ring = 0; ring < 8; ring++)
        {
            foreach (var e in GetTree().GetNodesInGroup("elements").OfType<WorldElement>()) seen.TryAdd(e.Def.Id, (e.Def.Family, e.GlobalPosition, e.PlaceId));
            g.Player.GlobalPosition = g.Gen.StartPixel + new Vector2(Mathf.Cos(ring * 0.8f), Mathf.Sin(ring * 0.8f)) * (300 + ring * 250); // named: walking around the start
            await Frames(25);
        }
        var cat = ElementCatalog.Load();
        foreach (var fam in cat.All.Values.Select(d => d.Family).Distinct())
            Check($"family '{fam}' appears around the start (seed {_seed})", seen.Values.Any(v => v.Family == fam), string.Join(", ", seen.Where(kv => kv.Value.Family == fam).Select(kv => kv.Key)));
        foreach (var d in cat.All.Values.Where(d => d.Workable))
        {
            if (!seen.TryGetValue(d.Id, out var v)) { Check($"{d.Id}: found in the world around the start", false, "não apareceu nas voltas"); continue; }
            var tile = g.Gen.TileOfPixel(v.At);
            string habitat = g.Gen.HabitatAt(tile.X, tile.Y).ToString().ToLowerInvariant();
            Check($"{d.Id}: stands in one of its habitats ({habitat})", d.Habitats.TryGetValue(habitat, out float w) && w > 0);
            g.Player.GlobalPosition = v.At + new Vector2(0, 40); // named: next to it
            g.Player.GetNode<Camera2D>("Camera").ResetSmoothing(); await Frames(30);
            var e = GetTree().GetNodesInGroup("elements").OfType<WorldElement>().FirstOrDefault(x => x.PlaceId == v.Place);
            if (e == null) { Check($"{d.Id}: rebuilt when coming back", false); continue; }
            var r = e.Strike(d.Tools[0], 1, g.Player.GlobalPosition);
            Check($"{d.Id}: answers its tool in the world", r is WorkResult.Worked or WorkResult.Depleted, $"{r}");
            if (_shots.Length > 0)
            {
                g.Player.GlobalPosition = v.At + new Vector2(-24, 2); g.Player.GetNode<Camera2D>("Camera").ResetSmoothing(); await Frames(10);
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                DirAccess.MakeDirRecursiveAbsolute(_shots);
                GetViewport().GetTexture().GetImage().SavePng($"{_shots}/{d.Id}-no-mundo-{_seed}.png");
            }
        }
        Finish();
    }
}
