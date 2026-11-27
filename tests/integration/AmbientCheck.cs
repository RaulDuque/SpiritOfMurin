using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;
using SpiritOfMurin.World;

namespace SpiritOfMurin.Tests;

/// <summary>
/// The Planície's ambience (pedido 13 = campaign delivery 4), with a window on monitor 2:
/// <list type="bullet">
/// <item><c>--phase=day --seed=N</c>: midday — the tint, the HUD clock and the running hour; the gusts bend the
/// grass and blow leaves; the grass parts around the swordsman; a flock with shadows; the ford slows and
/// ripples; deep water is swum (slowly, no attacks) or walked on with Shift and Ki (pedido 14.3); borders blended across chunks; varied pines and rocks; the y-sorted layers.</item>
/// <item><c>--phase=night --seed=N</c>: 23h — a bluish but legible night, the campfire's light and the faint
/// light around the swordsman; meditating saves the hour.</item>
/// <item><c>--phase=resume --seed=N</c>: Continuar brings the saved hour back.</item>
/// </list>
/// Teleports place the swordsman; the clock is set and frozen for the shots (named in the checks).
/// </summary>
public partial class AmbientCheck : Node
{
    private string SavePath => $"user://teste_amb_{_seed}.json";
    private readonly List<(string Name, bool Ok, string Detail)> _results = new();
    private string _phase = "day";
    private int _seed = 20261126;
    private string? _shots;
    private PlainsGame _g = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Game.Aim.UseForSession(false);
        foreach (string a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--phase=")) _phase = a["--phase=".Length..];
            if (a.StartsWith("--seed=")) _seed = int.Parse(a["--seed=".Length..]);
            if (a.StartsWith("--shots=")) _shots = a["--shots=".Length..];
        }
        if (_phase != "resume") SaveGame.Erase(SavePath);
        _g = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        _g.Seed = _seed; _g.SavePath = SavePath; _g.ShowIntro = false;
        _g.Start = _phase == "resume" ? PlainsGame.StartMode.Continue : PlainsGame.StartMode.New;
        AddChild(_g);
        _ = _phase switch { "night" => Night(), "resume" => Resume(), _ => Day() };
    }

    private void Check(string name, bool ok, string detail = "") { _results.Add((name, ok, detail)); GD.Print($"{(ok ? "OK  " : "FAIL")} {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
    private async Task Frames(int n) { for (int i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private async Task Flush() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static void Press(Key k, bool down) => Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = k, Keycode = k, Pressed = down });
    private void Finish() { int f = _results.Count(r => !r.Ok); GD.Print($"ambient {_phase} seed {_seed}: {_results.Count - f}/{_results.Count} OK"); GetTree().Quit(f == 0 ? 0 : 1); }
    private Player P => _g.Player;

    private async Task<Image?> Shot(string name)
    {
        if (DisplayServer.GetName() == "headless") return null;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        var img = GetViewport().GetTexture().GetImage();
        if (_shots != null) { DirAccess.MakeDirRecursiveAbsolute(_shots); img.SavePng($"{_shots}/{name}.png"); }
        return img;
    }

    private async Task Teleport(Vector2 at) { P.GlobalPosition = at; P.GetNode<Camera2D>("Camera").ResetSmoothing(); await Frames(8); }

    // a tile of a kind near the start, searching outward
    private Vector2I? FindTile(TileKind k, int radius = 60)
    {
        var start = _g.Gen.TileOfPixel(_g.Gen.StartPixel);
        for (int r = 1; r < radius; r++)
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != r) continue;
                    if (_g.Gen.TileAt(start.X + dx, start.Y + dy) == k) return new Vector2I(start.X + dx, start.Y + dy);
                }
        return null;
    }

    private Vector2 Center(Vector2I t) => (t + new Vector2(0.5f, 0.5f)) * _g.Gen.Settings.TileSize;

    // average brightness (0-1) of a box of the picture
    private static float Luma(Image img, Rect2I box)
    {
        double sum = 0; int n = 0;
        for (int y = box.Position.Y; y < box.End.Y; y += 2) for (int x = box.Position.X; x < box.End.X; x += 2) { var c = img.GetPixel(x, y); sum += 0.299 * c.R + 0.587 * c.G + 0.114 * c.B; n++; }
        return n == 0 ? 0 : (float)(sum / n);
    }

    private async Task Day()
    {
        await Frames(30);
        var sky = _g.Sky;
        sky.Frozen = true; sky.Hour = 12; await Frames(3);
        Check("midday: the world is not tinted (white) and the HUD clock says the hour", sky.Color.IsEqualApprox(Colors.White) && _g.Hud.TimeText.Contains("12:00"), $"{sky.Color} · {_g.Hud.TimeText}");
        sky.Frozen = false; float h0 = sky.Hour; await Frames(120);
        Check("the hour runs (2 s of play = 0.1 h with a 480 s day)", Mathf.IsEqualApprox(sky.Hour - h0, 0.1f, 0.02f), $"{h0:0.000} → {sky.Hour:0.000}");
        sky.Frozen = true; sky.Hour = 12;
        await Shot($"dia-{_seed}");

        // the wind: calm and gusts, the grass bending in a travelling wave, the leaves
        var gusts = new List<float>();
        for (float t = 0; t < 14; t += 0.25f) { Wind.T = t; gusts.Add(Wind.Gust); }
        Check("the wind has calms and gusts (the gust envelope goes from near 0 to near 1)", gusts.Min() < 0.1f && gusts.Max() > 0.7f, $"{gusts.Min():0.00}…{gusts.Max():0.00}");
        float calm = 0, peak = 0;
        for (float t = 0; t < 14; t += 0.05f) { Wind.T = t; if (Wind.Gust < 0.05f && calm == 0) calm = t; if (Wind.Gust > 0.95f && peak == 0) peak = t; }
        var grass = _g.World.GetChildren().SelectMany(n => n.GetChildren()).OfType<GrassSway>().Where(gs => gs.Count > 0).OrderBy(gs => gs.GlobalPosition.DistanceTo(P.GlobalPosition)).FirstOrDefault();
        Check("the chunks have tall grass", grass != null, grass == null ? "nenhum" : $"{grass.Count} tufos");
        if (grass != null)
        {
            var who = grass.Player; grass.Player = null; // the wind alone (no parting around the body)
            Wind.T = calm; grass.QueueRedraw(); await Flush(); await Frames(2); float bendCalm = grass.MaxBend;
            Wind.T = peak; grass.QueueRedraw(); await Flush(); await Frames(2); float bendGust = grass.MaxBend;
            Check("a gust bends the grass more than the calm", bendGust > bendCalm + 0.8f, $"calma {bendCalm:0.0}, rajada {bendGust:0.0}");
            grass.Player = who;
        }
        Wind.T = peak; int leavesBefore = _g.Leaves.Count; await Frames(60);
        Check("a gust blows leaves across the view", _g.Leaves.Count > leavesBefore, $"{_g.Leaves.Count} folhas");
        // the grass parts around the swordsman (in the calm): stand right beside a tuft
        if (grass?.FirstTuft is { } tuft)
        {
            Wind.T = calm;
            await Teleport(tuft + new Vector2(5, 0));
            grass.QueueRedraw(); await Flush(); await Frames(3);
            Check("walking through, the grass parts around the swordsman", grass.MaxBend > 1.2f, $"curvatura {grass.MaxBend:0.0}");
            await Shot($"capim-{_seed}");
        }
        // birds with shadows
        P.GlobalPosition = _g.Gen.StartPixel; await Frames(4);
        _g.Flocks.Flock(); await Frames(30);
        Check("a flock of birds crosses the sky by day (each with its shadow)", _g.Flocks.Count >= 3, $"{_g.Flocks.Count} pássaros");
        await Shot($"passaros-{_seed}");

        // the ford: slower, rippling; deep water blocks
        var ford = FindTile(TileKind.Ford, 80);
        Check("a ford (shallow water) near the start", ford != null);
        if (ford is { } ft)
        {
            await Teleport(Center(ft));
            Press(Key.W, true); await Flush(); await Frames(8);
            float vFord = P.Velocity.Length(); bool shallowFactor = Mathf.IsEqualApprox(P.TerrainSpeed, _g.Ambient.ShallowSpeed);
            await Frames(30);
            Press(Key.W, false); await Flush(); await Frames(3);
            await Teleport(_g.Gen.StartPixel);
            Press(Key.W, true); await Flush(); await Frames(8); float vGrass = P.Velocity.Length(); Press(Key.W, false); await Flush(); await Frames(3);
            Check("in the ford the swordsman walks slower (×0.6)", shallowFactor && vFord > 0 && Mathf.IsEqualApprox(vFord / vGrass, _g.Ambient.ShallowSpeed, 0.05f), $"vau {vFord:0} px/s, relva {vGrass:0} px/s");
            Check("and leaves ripples behind", _g.Ripples.Made > 0, $"{_g.Ripples.Made} ondinhas");
        }
        var deep = FindTile(TileKind.Water, 80);
        if (deep is { } dt2)
        {
            // stand on the land next to the deep water and walk into it
            Vector2I land = dt2;
            foreach (var d in new[] { Vector2I.Left, Vector2I.Right, Vector2I.Up, Vector2I.Down })
                if (_g.Gen.TileAt(dt2.X + d.X, dt2.Y + d.Y) is TileKind.Bank or TileKind.Grass) { land = dt2 + d; break; }
            await Teleport(Center(land));
            Vector2I dir = dt2 - land;
            Key k = dir.X > 0 ? Key.D : dir.X < 0 ? Key.A : dir.Y > 0 ? Key.S : Key.W;
            // pedido 14.3: without Shift the swordsman swims in, slowly, and cannot attack
            P.Ki = P.MaxKi;
            Press(k, true); await Flush(); await Frames(50);
            var now = _g.Gen.TileOfPixel(P.GlobalPosition);
            float vSwim = P.Velocity.Length();
            bool swimming = P.OnWater == WaterState.Swim;
            Press(k, false); await Flush(); await Frames(3);
            Check("deep water (darker): the swordsman swims in, slowly (×0.45), without Shift", swimming && _g.Gen.TileAt(now.X, now.Y) == TileKind.Water && Mathf.IsEqualApprox(P.TerrainSpeed, _g.Ambient.DeepWater.SwimSpeed), $"tile {now}, estado {P.OnWater}, {vSwim:0} px/s");
            int acts = 0; P.ActionStarted += (_, _) => acts++;
            Press(Key.J, true); await Flush(); await Frames(3); Press(Key.J, false); await Flush(); await Frames(6);
            Check("swimming, the cut does nothing", acts == 0);
            await Shot($"agua-nadando-{_seed}");
            // Shift: walks on the water, spending Ki (10/s)
            float ki0 = P.Ki;
            Press(Key.Shift, true); await Flush(); await Frames(60);
            bool walking = P.OnWater == WaterState.Walk;
            await Shot($"agua-andando-sobre-{_seed}");
            Press(Key.Shift, false); await Flush(); await Frames(2);
            float spent = ki0 - P.Ki;
            Check("holding Shift, the swordsman walks on the water and Ki drains (about 10/s)", walking && spent > 7 && spent < 13, $"estado {P.OnWater}, Ki gasto em ~1 s: {spent:0.0}");
            // no Ki: sinks and swims
            P.Ki = 0.5f;
            Press(Key.Shift, true); await Flush(); await Frames(20);
            Check("with no Ki left, Shift does not hold: the swordsman swims", P.OnWater == WaterState.Swim && P.Ki < 1f, $"estado {P.OnWater}, Ki {P.Ki:0.0}");
            Press(Key.Shift, false); await Flush(); await Frames(2);
            // the animals are still blocked by deep water
            var rb = _g.World.Objects.GetChildren().OfType<SpiritOfMurin.Creatures.Rabbit>().FirstOrDefault();
            Check("the animals still collide with deep water", rb == null || (rb.CollisionMask & PlainsWorld.WaterLayer) != 0);
            await Teleport(Center(land));
        }
        // blended borders, the same across chunk edges (read through the generator)
        int fringe = _g.World.GetChildren().SelectMany(n => n.GetChildren()).OfType<ChunkGround>().Sum(c => c.FringePixels);
        Check("terrain borders are blended (dithered fringes), not hard 32 px edges", fringe > 500, $"{fringe} pixels de transição");
        // variety: pines and rocks of different shapes
        var obs = _g.World.Objects.GetChildren().OfType<Obstacle>().ToList();
        int Hv(Obstacle o) => (int)((((uint)o.Variation * 2654435761u) >> 7) % 97u);
        int pineShapes = obs.Where(o => o.Type == Obstacle.Kind.Pine).Select(o => Hv(o) % 3).Distinct().Count();
        int rockShapes = obs.Where(o => o.Type == Obstacle.Kind.Boulder).Select(o => Hv(o) % 3).Distinct().Count();
        Check("pines of 3–5 tiers and rocks round, flat or in groups (at least two shapes each)", pineShapes >= 2 && rockShapes >= 2, $"pinheiros {pineShapes} formas, pedras {rockShapes} formas");
        // the layers: objects y-sorted, the grass under them, the birds and the leaves over
        var objects = _g.World.Objects;
        Check("y-sort: objects sorted by their feet; grass on the ground layer; birds and leaves above", objects.YSortEnabled && _g.Flocks.ZIndex > 0 && _g.Leaves.ZIndex > 0, $"ysort {objects.YSortEnabled}");
        Finish();
    }

    private async Task Night()
    {
        await Frames(30);
        var sky = _g.Sky;
        sky.Frozen = true; sky.Hour = 23; await Frames(3);
        var fire = _g.World.Objects.GetChildren().OfType<Campfire>().First();
        await Teleport(fire.GlobalPosition + new Vector2(-40, 30));
        await Frames(10);
        Check("23h: the night tint (bluish), the HUD says Noite · 23:00", sky.Darkness > 0.8f && sky.Color.B > sky.Color.R && _g.Hud.TimeText.Contains("Noite · 23:00"), $"{sky.Color} · {_g.Hud.TimeText}");
        Check("the campfire lights its surroundings at night", fire.LightEnergy > 1f, $"energia {fire.LightEnergy:0.00}");
        Check("a faint light around the swordsman keeps the fight readable", _g.PlayerLight.Energy > 0.4f, $"energia {_g.PlayerLight.Energy:0.00}");
        var img = await Shot($"noite-{_seed}");
        if (img != null)
        {
            var vp = GetViewport().GetVisibleRect().Size;
            float around = Luma(img, new Rect2I((int)(vp.X / 2 - 60), (int)(vp.Y / 2 - 60), 120, 120));
            float corner = Luma(img, new Rect2I(10, (int)vp.Y - 70, 60, 50));
            Check("legible: around the swordsman and the fire the picture is bright enough, darker far away", around > 0.22f && around > corner, $"perto {around:0.00}, longe {corner:0.00}");
        }
        sky.Hour = 18.5f; await Frames(3);
        await Shot($"entardecer-{_seed}");
        Check("dusk: a warm tint between day and night", sky.Color.R > sky.Color.B && sky.Darkness > 0 && sky.Darkness < 0.8f, $"{sky.Color}");
        sky.Hour = 23;
        // meditate: the hour goes with the journey
        var mat = _g.World.Objects.GetChildren().OfType<MeditationMat>().First();
        P.GlobalPosition = mat.GlobalPosition + new Vector2(0, 10); await Frames(5);
        Press(Key.E, true); await Flush(); await Frames(2); Press(Key.E, false); await Flush(); await Frames(5);
        var d = SaveGame.Read(SavePath).Data;
        Check("meditating saves the hour of the night", d != null && Mathf.IsEqualApprox(d.Hour, 23, 0.05f), d == null ? "sem save" : $"hora {d.Hour:0.00}");
        Finish();
    }

    private async Task Resume()
    {
        await Frames(30);
        Check("Continuar brings the saved hour back (the night it was meditated in)", Mathf.IsEqualApprox(_g.Sky.Hour, 23, 0.1f), $"hora {_g.Sky.Hour:0.00}");
        Finish();
    }
}
