using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Elements;
using SpiritOfMurin.Game;
using SpiritOfMurin.Items;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Tests;

/// <summary>
/// T18 — the swordsman's look, through the game's own <see cref="Player"/> drawn in the viewport. Phases, each its own run:
/// <list type="bullet">
/// <item><c>retrato</c>: the body frozen in reproducible poses (standing, two walk frames, the three cuts at the start
/// and the middle of the swing, the thrust, the step, the Ki gesture, the Reforço's glow) in the eight directions, holding
/// each thing (sword, knife, axe, pickaxe, empty hands). <c>--out=DIR</c> saves one PNG per sheet;
/// <c>--ref=DIR</c> compares every sheet pixel by pixel with the one of the same name there (and writes
/// <c>&lt;nome&gt;_diff.png</c> beside the new one when a pixel differs). A missing reference sheet is NOT ASSESSED.</item>
/// </list>
/// Pictures are the game's own viewport (never the desktop). Preferences: the tests' file (<c>Prefs.Path</c>).
/// </summary>
public partial class EquipCheck : Node2D
{
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private string _phase = "retrato", _out = "", _ref = "";

    /// <summary>The eight directions, from facing the camera clockwise as seen on screen (down, down-left, left …).</summary>
    public static readonly (string Name, Vector2 Dir)[] Dirs =
    {
        ("s", Vector2.Down), ("so", new Vector2(-1, 1).Normalized()), ("o", Vector2.Left), ("no", new Vector2(-1, -1).Normalized()),
        ("n", Vector2.Up), ("ne", new Vector2(1, -1).Normalized()), ("l", Vector2.Right), ("se", new Vector2(1, 1).Normalized()),
    };

    /// <summary>The reproducible poses: name and the frozen state (all with the same clock, so the sash and the breath match).</summary>
    public static readonly (string Name, Player.PoseShot Pose)[] Poses =
    {
        ("repouso", new(Player.ActionKind.None, 0, 0, 0, false, 0.9f, false)),
        ("passoA", new(Player.ActionKind.None, 0, 0, Mathf.Pi / 2, true, 0.9f, false)),
        ("passoB", new(Player.ActionKind.None, 0, 0, Mathf.Pi * 1.5f, true, 0.9f, false)),
        ("corte1_ini", new(Player.ActionKind.Cut, 1, 0, 0, false, 0.9f, false)),
        ("corte1", new(Player.ActionKind.Cut, 1, 0.3f, 0, false, 0.9f, false)),
        ("corte2", new(Player.ActionKind.Cut, 2, 0.7f, 0, false, 0.9f, false)),
        ("corte3", new(Player.ActionKind.Cut, 3, 0.8f, 0, false, 0.9f, false)),
        ("estocada", new(Player.ActionKind.Thrust, 1, 0.5f, 0, false, 0.9f, false)),
        ("esquiva", new(Player.ActionKind.Step, 0, 0.5f, 0, false, 0.9f, false)),
        ("ki", new(Player.ActionKind.Ki, 0, 0.5f, 0, false, 0.9f, false)),
        ("reforco", new(Player.ActionKind.None, 0, 0, 0, false, 0.9f, true)),
        ("reforco_corte", new(Player.ActionKind.Cut, 2, 0.3f, 0, false, 0.9f, true)),
    };

    /// <summary>What the hands hold in the sheets.</summary>
    public static readonly (string Name, Player.Weapon W, float Reach)[] Held =
    {
        ("espada", Player.Weapon.Sword, 0), ("faca", Player.Weapon.Knife, 0), ("machado", Player.Weapon.Axe, 22),
        ("picareta", Player.Weapon.Pickaxe, 20), ("maos", Player.Weapon.None, 0),
    };

    /// <summary>A whole look: what the hands hold (and its reach for tools), the weapon's drawing, the robe and the hat.</summary>
    public sealed record Look(string Name, Player.Weapon W, float Reach = 0, string? WeaponArt = null, string Outfit = RoupaArts.Default, string? Hat = null);

    /// <summary>Puts <paramref name="look"/> on <paramref name="p"/>.</summary>
    public static void Dress(Player p, Look look)
    {
        p.Equipped = look.W; p.ToolReach = look.Reach; p.WeaponArt = look.WeaponArt; p.Outfit = look.Outfit; p.Hat = look.Hat;
    }

    /// <summary>The board's looks (T18): the approved default, the first set, each piece alone, and the two other ways a hat may meet the bun.</summary>
    public static readonly Look[] BoardLooks =
    {
        new("padrao", Player.Weapon.Sword),
        new("conjunto", Player.Weapon.Sword, 0, "sabre_ferro", "tunica_viajante", "chapeu_bambu"),
        new("so_tunica", Player.Weapon.Sword, 0, null, "tunica_viajante"),
        new("so_chapeu", Player.Weapon.Sword, 0, null, RoupaArts.Default, "chapeu_bambu"),
        new("so_sabre", Player.Weapon.Sword, 0, "sabre_ferro"),
        new("chapeu_cobre_faixa", Player.Weapon.Sword, 0, "sabre_ferro", "tunica_viajante", "prancha_cobre_faixa"),
        new("chapeu_coque_fora", Player.Weapon.Sword, 0, "sabre_ferro", "tunica_viajante", "prancha_coque_fora"),
    };

    /// <summary>The board's columns: direction and pose (front, back, both profiles at rest; the cut, the grip in profile, the back mid-swing, the Reforço).</summary>
    public static readonly (string Name, Vector2 Dir, Player.PoseShot Pose)[] BoardCols =
    {
        ("frente", Vector2.Down, Poses[0].Pose), ("costas", Vector2.Up, Poses[0].Pose), ("perfil_dir", Vector2.Right, Poses[0].Pose),
        ("perfil_esq", Vector2.Left, Poses[0].Pose), ("frente_corte", Vector2.Down, Poses[4].Pose), ("perfil_corte", Vector2.Right, Poses[5].Pose),
        ("costas_corte", Vector2.Up, Poses[3].Pose), ("reforco", new Vector2(1, 1).Normalized(), Poses[11].Pose),
    };

    private const int RowsPerSheet = 4, CellW = 72, CellH = 84, Left = 36, Top = 70;
    private static readonly Color Ground = new("#4a5a3a");

    /// <inheritdoc />
    public override void _Ready()
    {
        foreach (string a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--phase=")) _phase = a["--phase=".Length..];
            if (a.StartsWith("--out=")) _out = a["--out=".Length..];
            if (a.StartsWith("--ref=")) _ref = a["--ref=".Length..];
            if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..];
            if (a.StartsWith("--seed=") && int.TryParse(a["--seed=".Length..], out int sd)) _seed = sd;
        }
        if (Godot.FileAccess.FileExists(Prefs.Path)) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(Prefs.Path)); // the tests' preferences, fresh
        Bindings.Init(); Assist.Reload(); Aim.Reload(); Aim.UseForSession(false);
        Assist.Areas = false; // the blade's area is not part of the look
        bool game = _phase is "bolsa" or "combate" or "salvar" or "continuar" or "oldsave" or "video" or "mapa";
        if (!game)
        {
            RenderingServer.SetDefaultClearColor(Ground);
            var cam = new Camera2D { Position = new Vector2(320, 180) };
            AddChild(cam); cam.MakeCurrent();
        }
        _ = Guard(_phase switch
        {
            "prancha" => Board(), "sabre" => SabrePhase(), "conjunto" => SetPhase(), "catalogo" => CatalogPhase(),
            "bolsa" => BagPhase(), "combate" => CombatPhase(), "salvar" => SavePhase(), "continuar" => ResumePhase(), "oldsave" => OldSavePhase(), "video" => VideoPhase(), "mapa" => TestMapPhase(),
            "retrato" => Portrait(),
            _ => Unknown(),
        });
    }

    // ------------------------------------------------------------------ sabre (pictures of the frozen body)

    /// <summary>One figure drawn alone in <paramref name="look"/>, facing <paramref name="dir"/> in <paramref name="pose"/>, and its picture.</summary>
    private async Task<(Image Img, Player P)> Alone(Look look, Vector2 dir, Player.PoseShot pose, Vector2 at)
    {
        var p = GD.Load<PackedScene>("res://src/Player/Player.tscn").Instantiate<Player>();
        p.GetNode<Camera2D>("Camera").Enabled = false;
        p.Position = at; p.InputLocked = true; p.CollisionLayer = 0; p.CollisionMask = 0; p.StartWeapon = look.W;
        AddChild(p); Dress(p, look); p.FreezePose(dir, pose);
        var img = await Snap();
        return (img, p);
    }

    private static int Count(Image img, Color c)
    {
        int n = 0; uint want = c.ToRgba32();
        for (int y = 0; y < img.GetHeight(); y++) for (int x = 0; x < img.GetWidth(); x++) if (img.GetPixel(x, y).ToRgba32() == want) n++;
        return n;
    }

    private static bool Near(Image img, Vector2 at, Color c, int r)
    {
        for (int y = -r; y <= r; y++)
            for (int x = -r; x <= r; x++)
            {
                int px = Mathf.RoundToInt(at.X) + x, py = Mathf.RoundToInt(at.Y) + y;
                if (px < 0 || py < 0 || px >= img.GetWidth() || py >= img.GetHeight()) continue;
                if (img.GetPixel(px, py).ToRgba32() == c.ToRgba32()) return true;
            }
        return false;
    }

    /// <summary>
    /// The sabre on the body, frame by frame: the grip stays in the hand through the three cuts in the eight directions;
    /// seen from behind the robe covers the hand but the blade's tip still shows; the Reforço's glow follows the curve.
    /// </summary>
    private async Task SabrePhase()
    {
        var look = new Look("sabre", Player.Weapon.Sword, 0, "sabre_ferro", "tunica_viajante", "chapeu_bambu");
        Vector2 at = new(320, 200);
        int frames = 0, gripOk = 0;
        var misses = new List<string>();
        foreach (int n in new[] { 1, 2, 3 })
            foreach (float k in new[] { 0f, 0.25f, 0.5f, 0.75f, 1f })
                foreach (var d in Dirs)
                {
                    var (img, p) = await Alone(look, d.Dir, new(Player.ActionKind.Cut, n, k, 0, false, 0.9f, false), at);
                    frames++;
                    Vector2 hand = at + p.HandPoint;
                    bool back = d.Name is "n" or "no" or "ne" && Mathf.Abs(d.Dir.Y) > Mathf.Abs(d.Dir.X) * 0.8f;
                    // the skin of the hand over the grip, the dark grip right behind it (when the robe does not cover them, from behind)
                    bool ok = back || (Near(img, hand + Vector2.One * 0.5f, new Color("#e2b58e"), 1) && Near(img, hand, SabreFerroArt.Grip, 4));
                    if (ok) gripOk++; else if (misses.Count < 6) misses.Add($"corte{n} k{k} {d.Name}");
                    p.QueueFree(); await Frames(1);
                }
        Check($"the grip follows the hand through the combo: {gripOk}/{frames} frames (3 cuts × 5 moments × 8 directions)", gripOk == frames, string.Join("; ", misses));
        // from behind: the arms go under the robe, the tip shows
        // from behind at rest the approved sword hangs wholly under the robe; the sabre must do the same (no blade drawn over the back)
        var (bimg, bp) = await Alone(look, Vector2.Up, Poses[0].Pose, at);
        int restSteel = Count(bimg, SabreFerroArt.Core) + Count(bimg, SabreFerroArt.Edge);
        bp.QueueFree(); await Frames(1);
        var (simg, sp) = await Alone(new Look("espada", Player.Weapon.Sword), Vector2.Up, Poses[0].Pose, at);
        int swordSteel = Count(simg, EspadaArt.Blade) + Count(simg, EspadaArt.BladeEdge);
        sp.QueueFree(); await Frames(1);
        Check("from behind at rest the sabre hangs under the robe, as the approved sword does", restSteel == 0 && swordSteel == 0, $"sabre {restSteel} px, espada {swordSteel} px");
        var (cimg, cp) = await Alone(look, Vector2.Up, new(Player.ActionKind.Cut, 1, 0.3f, 0, false, 0.9f, false), at);
        int steel = 0;
        for (int y = 0; y < cimg.GetHeight(); y++) for (int x = 0; x < cimg.GetWidth(); x++) { var c = cimg.GetPixel(x, y).ToRgba32(); if (c == SabreFerroArt.Core.ToRgba32() || c == SabreFerroArt.Edge.ToRgba32()) steel++; }
        Check("from behind mid-cut at least 8 px of the blade show (the robe covers the hand, not the blade)", steel >= 8, $"{steel} px de aço");
        cp.QueueFree(); await Frames(1);
        // the Reforço's glow along the curve
        var glowPose = new Player.PoseShot(Player.ActionKind.None, 0, 0, 0, false, 0.9f, true);
        var (gimg, gp) = await Alone(look, Vector2.Right, glowPose, at);
        var art = (SabreFerroArt)ArmaArts.Get("sabre_ferro")!;
        int lit = 0, probes = 0;
        foreach (float t in new[] { 0.35f, 0.5f, 0.65f, 0.8f, 0.95f })
        {
            probes++;
            Vector2 s = at + gp.HandPoint.Lerp(gp.TipPoint, t);
            bool blue = false;
            for (int y = -3; y <= 3 && !blue; y++) for (int x = -3; x <= 3 && !blue; x++)
            {
                var c = gimg.GetPixel(Mathf.RoundToInt(s.X) + x, Mathf.RoundToInt(s.Y) + y);
                blue = c.B > c.R + 0.12f && c.B > c.G + 0.04f;
            }
            if (blue) lit++;
        }
        Check($"the Reforço's blue glow runs along the sabre: {lit}/{probes} points of the blade", lit == probes);
        gp.QueueFree(); await Frames(1);
        _ = art;
    }

    /// <summary>The approved set's looks (each piece alone and together), saved or compared like <c>retrato</c>; the bare head after the hat is the approved one.</summary>
    private async Task SetPhase()
    {
        var scene = GD.Load<PackedScene>("res://src/Player/Player.tscn");
        if (_out.Length > 0) DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(_out));
        var looks = new[]
        {
            new Look("conjunto", Player.Weapon.Sword, 0, "sabre_ferro", "tunica_viajante", "chapeu_bambu"),
            new Look("so_tunica", Player.Weapon.Sword, 0, null, "tunica_viajante"),
            new Look("so_chapeu", Player.Weapon.Sword, 0, null, RoupaArts.Default, "chapeu_bambu"),
            new Look("so_sabre", Player.Weapon.Sword, 0, "sabre_ferro"),
            new Look("chapeu_tirado", Player.Weapon.Sword), // after the hat comes off: must be the approved head
        };
        int sheets = 0, compared = 0;
        foreach (var look in looks)
            for (int s = 0; s * RowsPerSheet < Poses.Length; s++)
            {
                var rows = Poses.Skip(s * RowsPerSheet).Take(RowsPerSheet).ToList();
                string name = $"{look.Name}_{s + 1}";
                var made = new List<Player>();
                for (int r = 0; r < rows.Count; r++)
                    for (int c = 0; c < Dirs.Length; c++)
                    {
                        var p = scene.Instantiate<Player>();
                        p.GetNode<Camera2D>("Camera").Enabled = false;
                        p.Position = new Vector2(Left + c * CellW, Top + r * CellH);
                        p.InputLocked = true; p.CollisionLayer = 0; p.CollisionMask = 0; p.StartWeapon = look.W;
                        AddChild(p);
                        if (look.Name == "chapeu_tirado") { Dress(p, look with { Hat = "chapeu_bambu" }); p.FreezePose(Dirs[c].Dir, rows[r].Pose); await Frames(1); }
                        Dress(p, look); p.FreezePose(Dirs[c].Dir, rows[r].Pose);
                        made.Add(p);
                    }
                var img = await Snap();
                foreach (var p in made) p.QueueFree();
                await Frames(1);
                sheets++;
                if (_out.Length > 0) img.SavePng($"{_out}/{name}.png");
                string refName = look.Name == "chapeu_tirado" ? $"espada_{s + 1}" : name; // the bare head is compared with the approved sword sheets
                string refDir = look.Name == "chapeu_tirado" ? _baseRef : _ref;
                if (refDir.Length == 0) continue;
                string refPath = $"{refDir}/{refName}.png";
                if (!Godot.FileAccess.FileExists(refPath)) { Check($"NOT ASSESSED: {name} has no reference sheet", false, refPath); continue; }
                compared++;
                var (diff, map) = Compare(Image.LoadFromFile(ProjectSettings.GlobalizePath(refPath)), img);
                Check($"{name}: identical to {refPath}", diff == 0, $"{diff} px differ");
                if (diff > 0 && map != null && _out.Length > 0) map.SavePng($"{_out}/{name}_diff.png");
            }
        Check($"set sheets drawn: {sheets}", sheets == looks.Length * Mathf.CeilToInt(Poses.Length / (float)RowsPerSheet));
        if (_ref.Length == 0 && _out.Length == 0) Check("NOT ASSESSED: neither --out nor --ref was given", false);
    }


    // ------------------------------------------------------------------ catalogo (what /create arma|roupa|capacete makes)

    private sealed partial class IconPair : Node2D
    {
        public string Id = "";
        public override void _Draw()
        {
            DrawRect(new Rect2(0, 0, 120, 60), new Color("#d8c7a0"));
            ItemIcons.Draw(this, Id, new Vector2(30, 30), 2);
            ItemIcons.Draw(this, "__sem_icone__", new Vector2(90, 30), 2);
        }
    }

    /// <summary>
    /// Every piece in <c>assets/data/equipamento/</c> (read from the folder, no list to keep): its file loads, the id is the
    /// file's name, its drawing is registered, it has an icon of its own (not the plain bundle), its numbers make sense,
    /// it says where to get it, a recipe or the bundle gives it (or it is the base robe / a prologue weapon), and it is
    /// drawn on the swordsman in the eight directions (saved with <c>--out</c>). An empty folder is NOT ASSESSED.
    /// </summary>
    private async Task CatalogPhase()
    {
        var pieces = ItemCatalog.LoadEquipment();
        Check($"equipment files found: {pieces.Count}", pieces.Count > 0, pieces.Count == 0 ? "NOT ASSESSED: no file in assets/data/equipamento" : string.Join(", ", pieces.Select(p => p.Id)));
        if (pieces.Count == 0) return;
        var cat = ItemCatalog.Load();
        string rules = Godot.FileAccess.GetFileAsString(ItemCatalog.EquipmentFolder + "/_regras.json");
        var bundle = EquipRules.ParseSettings(rules).Trouxa;
        if (_out.Length > 0) DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(_out));
        var files = new List<string>();
        void Walk(string dir) { using var d = DirAccess.Open(dir); if (d == null) return; foreach (var f in d.GetFiles()) if (f.EndsWith(".json") && !f.StartsWith("_")) files.Add($"{dir}/{f}"); foreach (var sub in d.GetDirectories()) Walk($"{dir}/{sub}"); }
        Walk(ItemCatalog.EquipmentFolder);
        foreach (var path in files)
        {
            var d = ItemCatalog.ParseEquipment(Godot.FileAccess.GetFileAsString(path));
            string file = path.GetFile().GetBaseName(), folder = path.GetBaseDir().GetFile();
            var problems = new List<string>();
            if (d.Id != file) problems.Add($"id '{d.Id}' ≠ arquivo '{file}'");
            if (d.Name.Length == 0) problems.Add("sem name");
            if (d.Onde.Length == 0) problems.Add("sem 'onde' (onde se consegue)");
            if (folder == "armas")
            {
                var w = d.Weapon;
                if (w == null) problems.Add("arma sem bloco weapon");
                else
                {
                    if (!ArmaArts.Has(w.Art)) problems.Add($"desenho '{w.Art}' não registrado em ArmaArts");
                    if (w.Reach <= 0 || w.Damage <= 0 || w.Chain <= 0 || w.Startup <= 0 || w.Active <= 0 || w.Recovery <= 0) problems.Add("números do golpe zerados");
                    if (w.Kind is not ("longa" or "curta")) problems.Add($"kind '{w.Kind}'");
                }
            }
            else if (folder is "roupas" or "capacetes")
            {
                var wr = d.Wear;
                string slot = folder == "roupas" ? "corpo" : "cabeca";
                if (wr == null) problems.Add("sem bloco wear");
                else
                {
                    if (wr.Slot != slot) problems.Add($"slot '{wr.Slot}' (esperado {slot})");
                    if (slot == "corpo" ? !RoupaArts.Has(wr.Art) : !CapaceteArts.Has(wr.Art)) problems.Add($"desenho '{wr.Art}' não registrado");
                    if (wr.Defense < 0 || wr.Weight < 0) problems.Add("defesa ou peso negativos");
                }
            }
            else problems.Add($"pasta '{folder}' (use armas, roupas ou capacetes)");
            bool gotten = d.Base || bundle.Contains(d.Id) || cat.Recipes.Any(r => r.Output == d.Id) || d.Onde.Contains("prólogo");
            if (!gotten) problems.Add("nenhuma receita nem a trouxa dão esta peça");
            if (!cat.Items.ContainsKey(d.Id)) problems.Add("não entrou no catálogo da bolsa");
            // the icon: its own picture, not the plain bundle of unknown ids
            var pair = new IconPair { Id = d.Id, Position = new Vector2(500, 10) };
            AddChild(pair);
            var img = await Snap();
            pair.QueueFree(); await Frames(1);
            var a = img.GetRegion(new Rect2I(500 + 10, 10 + 10, 40, 40)); var b = img.GetRegion(new Rect2I(500 + 70, 10 + 10, 40, 40));
            if (Compare(a, b).Diff == 0) problems.Add($"sem ícone próprio (falta um case \"{d.Id}\" em src/Game/ItemIcons.cs)");
            Check($"{folder}/{file}.json: loads, drawn, icon, numbers, where to get it", problems.Count == 0, problems.Count == 0 ? (d.Onde) : string.Join("; ", problems));
            // the preview on the swordsman: eight directions at rest and mid-cut
            var look = folder == "armas" ? new Look(d.Id, d.Weapon?.Kind == "curta" ? Player.Weapon.Knife : Player.Weapon.Sword, 0, d.Weapon?.Art)
                : folder == "roupas" ? new Look(d.Id, Player.Weapon.Sword, 0, null, d.Wear?.Art ?? RoupaArts.Default)
                : new Look(d.Id, Player.Weapon.Sword, 0, null, RoupaArts.Default, d.Wear?.Art);
            var made = new List<Player>();
            var scene = GD.Load<PackedScene>("res://src/Player/Player.tscn");
            for (int r = 0; r < 2; r++)
                for (int c = 0; c < Dirs.Length; c++)
                {
                    var p = scene.Instantiate<Player>();
                    p.GetNode<Camera2D>("Camera").Enabled = false;
                    p.Position = new Vector2(Left + c * CellW, Top + r * CellH); p.InputLocked = true; p.CollisionLayer = 0; p.CollisionMask = 0; p.StartWeapon = look.W;
                    AddChild(p); Dress(p, look); p.FreezePose(Dirs[c].Dir, Poses[r == 0 ? 0 : 5].Pose); made.Add(p);
                }
            var prev = await Snap();
            foreach (var p in made) p.QueueFree();
            await Frames(1);
            if (_out.Length > 0) prev.SavePng($"{_out}/previa_{d.Id}.png");
        }
    }

    private string _baseRef = "res://production/qa/evidence/2026-11-28_t18-equipamento/antes";

    private Task Unknown() { Check($"NOT ASSESSED: unknown phase '{_phase}'", false); return Task.CompletedTask; }

    private async Task Guard(Task t)
    {
        try { await t; }
        catch (System.Exception ex) { Check("NOT ASSESSED: the phase stopped on an exception", false, ex.GetType().Name + ": " + ex.Message + " " + ex.StackTrace?.Split('\n').FirstOrDefault()); }
        Finish();
    }

    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
    private void Finish() { int f = _results.Count(r => !r.Ok); GD.Print($"equip {_phase}: {_results.Count - f}/{_results.Count} OK"); GetTree().Quit(f == 0 && _results.Count > 0 ? 0 : 1); }
    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); }

    /// <summary>Lays the figures of one sheet (rows of poses × the eight directions) and returns them.</summary>
    private List<Player> Lay(PackedScene scene, (string Name, Player.Weapon W, float Reach) held, IReadOnlyList<(string Name, Player.PoseShot Pose)> rows)
    {
        var made = new List<Player>();
        for (int r = 0; r < rows.Count; r++)
            for (int c = 0; c < Dirs.Length; c++)
            {
                var p = scene.Instantiate<Player>();
                p.GetNode<Camera2D>("Camera").Enabled = false;
                p.Position = new Vector2(Left + c * CellW, Top + r * CellH);
                p.InputLocked = true; p.CollisionLayer = 0; p.CollisionMask = 0; p.StartWeapon = held.W;
                AddChild(p);
                p.Equipped = held.W; p.ToolReach = held.Reach;
                p.FreezePose(Dirs[c].Dir, rows[r].Pose);
                made.Add(p);
            }
        return made;
    }

    private async Task<Image> Snap()
    {
        await Frames(3);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        return GetViewport().GetTexture().GetImage();
    }

    /// <summary>
    /// The board (T18): one strip per look (<see cref="BoardLooks"/>), the columns of <see cref="BoardCols"/>, saved as
    /// <c>&lt;look&gt;.png</c> (640 × 84, the game's own pixels at 1×) in <c>--out</c>.
    /// </summary>
    private async Task Board()
    {
        if (_out.Length == 0) { Check("NOT ASSESSED: the board needs --out", false); return; }
        CapaceteArts.Register("prancha_cobre_faixa", new ChapeuBambuVarianteArt(CabecaArt.BandRows, false));
        CapaceteArts.Register("prancha_coque_fora", new ChapeuBambuVarianteArt(0, true));
        DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(_out));
        var scene = GD.Load<PackedScene>("res://src/Player/Player.tscn");
        int saved = 0;
        foreach (var look in BoardLooks)
        {
            var made = new List<Player>();
            for (int c = 0; c < BoardCols.Length; c++)
            {
                var p = scene.Instantiate<Player>();
                p.GetNode<Camera2D>("Camera").Enabled = false;
                p.Position = new Vector2(Left + c * CellW + 8, Top);
                p.InputLocked = true; p.CollisionLayer = 0; p.CollisionMask = 0; p.StartWeapon = look.W;
                AddChild(p); Dress(p, look);
                p.FreezePose(BoardCols[c].Dir, BoardCols[c].Pose);
                made.Add(p);
            }
            var img = await Snap();
            foreach (var p in made) p.QueueFree();
            await Frames(1);
            img.GetRegion(new Rect2I(0, Top - 62, 640, CellH)).SavePng($"{_out}/{look.Name}.png");
            saved++;
        }
        Check($"board strips saved: {saved}/{BoardLooks.Length}", saved == BoardLooks.Length);
    }

    private async Task Portrait()
    {
        var scene = GD.Load<PackedScene>("res://src/Player/Player.tscn");
        if (_out.Length > 0) DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(_out));
        int sheets = 0, compared = 0;
        foreach (var held in Held)
            for (int s = 0; s * RowsPerSheet < Poses.Length; s++)
            {
                var rows = Poses.Skip(s * RowsPerSheet).Take(RowsPerSheet).ToList();
                string name = $"{held.Name}_{s + 1}";
                var figs = Lay(scene, held, rows);
                var img = await Snap();
                foreach (var f in figs) f.QueueFree();
                await Frames(1);
                sheets++;
                if (_out.Length > 0) img.SavePng($"{_out}/{name}.png");
                if (_ref.Length == 0) continue;
                string refPath = $"{_ref}/{name}.png";
                if (!Godot.FileAccess.FileExists(refPath)) { Check($"NOT ASSESSED: {name} has no reference sheet", false, refPath); continue; }
                var want = Image.LoadFromFile(ProjectSettings.GlobalizePath(refPath));
                compared++;
                var (diff, map) = Compare(want, img);
                Check($"{name}: identical to the reference ({string.Join(", ", rows.Select(r => r.Name))} × 8 directions)", diff == 0, diff == 0 ? "0 px differ" : $"{diff} px differ");
                if (diff > 0 && map != null && _out.Length > 0) map.SavePng($"{_out}/{name}_diff.png");
            }
        Check($"sheets drawn: {sheets} ({Held.Length} held × {Poses.Length} poses × 8 directions)", sheets == Held.Length * Mathf.CeilToInt(Poses.Length / (float)RowsPerSheet));
        if (_ref.Length > 0) Check($"sheets compared with {_ref}: {compared}/{sheets}", compared == sheets);
        else if (_out.Length == 0) Check("NOT ASSESSED: neither --out nor --ref was given", false);
    }

    /// <summary>Counts the pixels that differ and paints them red over a faded copy of the new picture.</summary>
    public static (int Diff, Image? Map) Compare(Image want, Image got)
    {
        if (want.GetSize() != got.GetSize()) return (int.MaxValue, null);
        want.Convert(Image.Format.Rgba8); got.Convert(Image.Format.Rgba8);
        var map = Image.CreateEmpty(got.GetWidth(), got.GetHeight(), false, Image.Format.Rgba8);
        int diff = 0;
        for (int y = 0; y < got.GetHeight(); y++)
            for (int x = 0; x < got.GetWidth(); x++)
            {
                Color a = want.GetPixel(x, y), b = got.GetPixel(x, y);
                bool same = a.ToRgba32() == b.ToRgba32();
                if (!same) diff++;
                map.SetPixel(x, y, same ? b.Lerp(Colors.White, 0.7f) : Colors.Red);
            }
        return (diff, map);
    }
}
