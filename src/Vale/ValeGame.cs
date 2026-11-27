using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SpiritOfMurin.Audio;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Combat;
using SpiritOfMurin.Items;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;
using SpiritOfMurin.Game;

namespace SpiritOfMurin.Vale;

/// <summary>
/// The prologue — the Vale de Bambus — ported from the HTML prototype: the trail, the bridge guardian of
/// the Closed Guard, the half manual on a body in the bamboo, the sealed tomb, the cut in the stone and
/// the footprints that teach the side step, the inscription beyond the gap and the Silent Blade Step,
/// the camp and the Senior Swordsman. Winning against the Senior starts the ending (<see cref="SeniorFinale"/>),
/// which ends the prologue and hands over to the Planície. Meditating on the mat by the twin stones writes
/// the journey.
/// </summary>
public partial class ValeGame : Node2D, IValeHost, IMapSource
{
    // ------------------------------------------------------------------ map (campaign delivery 3)

    /// <inheritdoc />
    public List<Vector2> MapMarkers { get; } = new();
    /// <inheritdoc />
    public Vector2 MapPlayer => Player.GlobalPosition;
    /// <inheritdoc />
    public Vector2 MapFacing => Player.Facing;
    /// <inheritdoc />
    public string MapTitle => "Vale de Bambus";
    /// <inheritdoc />
    public string MapLegend => "só o que você já viu · ciano: esteira de meditação\nselo vermelho: onde a pista surgiu · ✕ marcadores";
    /// <inheritdoc />
    public string MapWhere => $"{ValeMap.AreaName(Player.GlobalPosition)}. {WhereDetail()}";
    /// <inheritdoc />
    public IReadOnlyList<MapNote> MapNotes => _notes;
    private readonly List<MapNote> _notes = new();

    private string WhereDetail()
    {
        Vector2 p = Player.GlobalPosition;
        string dir = Facing8(Player.Facing);
        if (Map.AtPixel(p) == VTile.Bridge) return $"Sobre a ponte, olhando para {dir}.";
        if (p.Y < 13 * ValeMap.TS) return $"Dentro do acampamento, olhando para {dir}.";
        if (p.Y < 17 * ValeMap.TS) return $"Na margem norte do rio, olhando para {dir}.";
        if (p.Y < 21 * ValeMap.TS) return $"Na margem sul do rio, perto da ponte, olhando para {dir}.";
        return $"Olhando para {dir}.";
    }

    /// <summary>A facing in words (norte, nordeste…): up on the screen is north.</summary>
    public static string Facing8(Vector2 f)
    {
        if (f.LengthSquared() < 0.0001f) return "o sul";
        int i = Mathf.PosMod(Mathf.RoundToInt(Mathf.Atan2(f.Y, f.X) / (Mathf.Pi / 4)), 8);
        return new[] { "o leste", "o sudeste", "o sul", "o sudoeste", "o oeste", "o noroeste", "o norte", "o nordeste" }[i];
    }

    private static readonly Dictionary<string, Vector2> QuestDefaultAt = new()
    {
        ["guarda"] = new Vector2(29, 15.8f) * ValeMap.TS, ["manual"] = ValeMap.Corpse, ["senior"] = ValeMap.CampEntry, ["lamina"] = ValeMap.Stele,
    };

    // the notebook in the order things happened: the missions (with their seals) and the clues
    private void RebuildNotes()
    {
        _notes.Clear();
        foreach (var (id, done) in Story.Quests)
        {
            if (!Quests.TryGetValue(id, out var q)) continue;
            Vector2 at = Story.QuestAt.TryGetValue(id, out var a) && a.Length == 2 ? new Vector2(a[0], a[1]) : QuestDefaultAt.GetValueOrDefault(id, Player.GlobalPosition);
            _notes.Add(new MapNote(q.Name, q.Desc, done, at, QuestToward(id), false));
        }
        foreach (var c in Story.Clues)
            _notes.Add(new MapNote("Pista", c.Text, false, c.At.Length == 2 ? new Vector2(c.At[0], c.At[1]) : null, c.Toward.Length == 2 ? new Vector2(c.Toward[0], c.Toward[1]) : null, true));
    }

    private static Vector2? QuestToward(string id) => id switch { "guarda" or "manual" => new Vector2(51, 26) * ValeMap.TS, "senior" => new Vector2(29, 7) * ValeMap.TS, _ => null };

    /// <summary>Notes a clue in the map's notebook (once per id) and says so.</summary>
    public void AddClue(string id, string text, Vector2 at, Vector2? toward = null)
    {
        if (Story.Clues.Exists(c => c.Id == id)) return;
        Story.Clues.Add(new Clue { Id = id, Text = text, At = new[] { at.X, at.Y }, Toward = toward is { } t ? new[] { t.X, t.Y } : System.Array.Empty<float>() });
        RebuildNotes();
        Hud.Toast(Bindings.F($"Pista anotada no mapa ([map])."));
    }

    private ImageTexture? _seenTex;
    private int _seenVersion = -1, _seenTexVersion = -2;

    private static Color MapPaperColor(VTile t) => t switch
    {
        VTile.Path => new Color("#9a7d55"), VTile.Camp => new Color("#b59c72"), VTile.Water => new Color("#5f8a8a"), VTile.Chasm => new Color("#2a2620"),
        VTile.Bridge => new Color("#7a5a3a"), VTile.Rock => new Color("#6f6a5c"), VTile.TombWall or VTile.CutWall or VTile.Door => new Color("#55545a"),
        VTile.TombFloor => new Color("#8d8a84"), VTile.Bamboo => new Color("#6f8f55"), VTile.Tent => new Color("#8f4a36"), VTile.Stele => new Color("#6f6a5c"),
        VTile.Gate => new Color("#6d4f33"), VTile.Pine => new Color("#4c6a48"), _ => new Color("#b3a882"),
    };
    /// <summary>The pause menu and the map.</summary>
    public GamePanels Panels { get; private set; } = null!;


    /// <inheritdoc />
    public void DrawMap(MapView v)
    {
        // one texel per tile, only what was seen (the prototype's map colours on parchment)
        if (_seenTex == null || _seenTexVersion != _seenVersion)
        {
            var img = Image.CreateEmpty(ValeMap.MW, ValeMap.MH, false, Image.Format.Rgba8);
            for (int ty = 0; ty < ValeMap.MH; ty++)
                for (int tx = 0; tx < ValeMap.MW; tx++)
                {
                    if (!Seen(tx, ty)) continue;
                    var t = Map.At(tx, ty); if (t == VTile.Gate && Story.GateOpen) t = VTile.Camp;
                    img.SetPixel(tx, ty, MapPaperColor(t));
                }
            if (_seenTex == null) _seenTex = ImageTexture.CreateFromImage(img); else _seenTex.Update(img);
            _seenTexVersion = _seenVersion;
        }
        v.WorldTexture(Vector2.Zero, new Vector2(ValeMap.MW, ValeMap.MH) * ValeMap.TS, _seenTex);
        foreach (var (name, at) in ValeMap.Landmarks)
            if (Seen(Mathf.FloorToInt(at.X / ValeMap.TS), Mathf.FloorToInt(at.Y / ValeMap.TS))) v.WorldLabel(at, name, MapView.Ink);
        if (Seen(Mathf.FloorToInt(ValeMap.Meditate.X / ValeMap.TS), Mathf.FloorToInt(ValeMap.Meditate.Y / ValeMap.TS))) v.WorldDot(ValeMap.Meditate, new Color("#6fd8d2"), 3);
    }

    /// <summary>The save file.</summary>
    [Export] public string SavePath { get; set; } = SaveGame.DefaultPath;
    /// <summary>Show the opening overlay (tests turn it off).</summary>
    [Export] public bool ShowIntro { get; set; } = true;

    /// <inheritdoc />
    public Player Player { get; private set; } = null!;
    /// <inheritdoc />
    public ValeCombat Combat { get; private set; } = null!;
    /// <inheritdoc />
    public ValeState Story { get; private set; } = new();
    /// <inheritdoc />
    public bool PlayerDown => _dying > 0 || Player.Health <= 0;
    /// <summary>The map.</summary>
    public ValeMap Map { get; } = new();
    /// <summary>The screen layer.</summary>
    public PlainsHud Hud { get; private set; } = null!;
    /// <summary>Sound.</summary>
    public Sfx Sfx { get; private set; } = null!;
    /// <summary>Effects.</summary>
    public ValeFx Fx { get; private set; } = null!;
    /// <summary>The damage numbers of the assist mode.</summary>
    public CombatText Numbers { get; private set; } = null!;
    /// <summary>The prologue's items.</summary>
    public Inventory Inventory { get; } = new();
    /// <summary>Enemies and the statue by id.</summary>
    public Dictionary<string, ValeFighter> Fighters { get; } = new();
    /// <summary>The locked target.</summary>
    public ValeFighter? Lock { get; private set; }
    /// <summary>The techniques in force.</summary>
    public TechniqueSet Tech { get; private set; } = null!;
    /// <summary>Where the player wakes after falling.</summary>
    public Vector2 Respawn { get; private set; } = ValeMap.Start;
    /// <summary>The ending, once the Senior falls.</summary>
    public SeniorFinale? Finale { get; private set; }
    /// <summary>Raised when the ending is over: the journey to carry into the Planície.</summary>
    public event Action<SaveData>? PrologueEnded;

    private Node2D _world = null!, _ground = null!, _bodies = null!;
    private ValeDecals _decals = null!;
    private ValeTerrain.Built _built = null!;
    private readonly List<ValePot> _pots = new();
    private readonly List<ValeHerb> _herbs = new();
    private readonly List<ValeDrop> _drops = new();
    private readonly List<KiWave> _waves = new();
    private ValeChest _chest = null!;
    private ValeCorpse _corpse = null!;
    private float _dying, _reveal, _hitstopReal, _shake;
    private int _parryCount, _flankHits, _lastSwing = -1;
    private readonly HashSet<string> _hitThisSwing = new();
    private string _area = "";
    private readonly bool[] _seen = new bool[ValeMap.MW * ValeMap.MH];
    private Vector2I _seenAt = new(-1, -1);
    private ValeFighter? _speaker;

    // ------------------------------------------------------------------ setup

    /// <inheritdoc />
    public override void _Ready()
    {
        Combat = ValeCombat.Load();
        Tech = new TechniqueSet(TechniqueData.Load());
        _ground = GetNode<Node2D>("Ground");
        _world = GetNode<Node2D>("World");
        _bodies = GetNode<Node2D>("Bodies");
        Player = GetNode<Player>("World/Player");
        Hud = GetNode<PlainsHud>("Hud");
        Sfx = GetNode<Sfx>("Sfx");
        Fx = new ValeFx { ZIndex = 20, Name = "Fx" }; AddChild(Fx);
        Numbers = new CombatText { Name = "NumerosDeDano" }; AddChild(Numbers);
        _decals = new ValeDecals { Story = Story, ZIndex = -5, Name = "Decals" }; _ground.AddChild(_decals);
        _built = ValeTerrain.Build(Map, _ground, _world, _bodies, Story);
        for (int i = 0; i < ValeMap.Pots.Length; i++) { var p = new ValePot { Index = i, Position = ValeMap.Pots[i], Name = $"Pot{i}" }; _world.AddChild(p); _pots.Add(p); }
        for (int i = 0; i < ValeMap.Herbs.Length; i++) { var h = new ValeHerb { Index = i, Position = ValeMap.Herbs[i], Name = $"Herb{i}" }; _world.AddChild(h); _herbs.Add(h); }
        _chest = new ValeChest { Position = ValeMap.Chest, Name = "Chest" }; _world.AddChild(_chest);
        _corpse = new ValeCorpse { Position = ValeMap.Corpse, Name = "Corpse" }; _world.AddChild(_corpse);
        Spawn("dis_bridge", ValeFighter.Kind.Disciple, 29, 15.8f, f => { f.Bridge = true; f.Leash = 300; f.Drops = new[] { ("faixa", 1), ("balsamo", 1) }; });
        Spawn("senior", ValeFighter.Kind.Senior, 29, 7, f => f.Drops = new[] { ("pingente", 1), ("prata", 5) });
        Spawn("statue", ValeFighter.Kind.Statue, ValeMap.Statue.X / ValeMap.TS, ValeMap.Statue.Y / ValeMap.TS, _ => { });
        Spawn("dis_herbs", ValeFighter.Kind.Disciple, 41.5f, 25.6f, f => { f.AggroR = 120; f.Leash = 240; f.Line = "Essas ervas são da seita. Volte por onde veio."; f.Drops = new[] { ("faixa", 1), ("erva", 2), ("pedra", 1) }; });
        Spawn("dis_gap", ValeFighter.Kind.Disciple, 9.5f, 27.4f, f => { f.AggroR = 130; f.Leash = 240; f.Line = "Ninguém atravessa a fenda. Como você chegou aqui?"; f.Drops = new[] { ("faixa", 1), ("balsamo", 1), ("prata", 3) }; });
        Spawn("dis_camp", ValeFighter.Kind.Disciple, 44.5f, 10.6f, f => { f.AggroR = 110; f.Leash = 200; f.Line = "O sênior não precisa de você. Eu bastarei."; f.Drops = new[] { ("faixa", 1), ("pedra", 1) }; });
        // the swordsman of the prologue
        Player.Equipped = Player.Weapon.Sword;
        Player.CollisionMask = 1 | Player.GapLayer;
        Player.SetMaxHealth(Combat.PlayerHealth);
        Player.MaxKi = Combat.PlayerKiMax; Player.KiRegen = Combat.PlayerKiRegen; Player.Ki = Player.MaxKi;
        Player.MaxVigor = 0;
        Player.Tech = Tech;
        Player.SecretFound += OnSecret;
        Player.ReinforceResult += (ok, m) => { Hud.Toast(m); Sfx.Play(ok ? "chime" : "fizzle"); };
        Player.ChargeCancelled += back => { if (back > 0.5f) Hud.Toast($"A concentração se desfaz: {back:0} de Ki voltam à barra."); };
        Player.CutSlack = Combat.CutSlack; Player.ThrustSlack = Combat.ThrustSlack; // the prototype's hit test is a little wider than the blade
        Player.LockTarget = () => Lock;
        Player.BladeActive += OnBlade;
        Player.Hurt += a => Numbers.Taken(Player.GlobalPosition + new Vector2(8, -44), a);
        Player.KiFired += OnKi;
        Player.SideStepped += t => { if (t is ValeFighter f && f.Type != ValeFighter.Kind.Statue) f.Fooled = f.Type == ValeFighter.Kind.Senior ? Combat.SenFooled : Combat.DisFooled; Sfx.Play("side"); };
        Player.MoveEnded += Settle;
        Player.Evaded += () => { Sfx.Play("evade"); Fx.Say(Player.GlobalPosition + new Vector2(0, -46), "passo", ValeTerrain.Learn); };
        Player.TechniqueRefused += (_, m) => { Hud.Toast(m); Sfx.Play("fizzle"); };
        Player.Fainted += () => { _dying = 1.5f; Sfx.Play("die"); Lock = null; };
        Player.ActionStarted += (k, n) => { if (k == Player.ActionKind.Cut) Sfx.Play(n == 3 ? "swing3" : "swing"); else if (k is Player.ActionKind.Step) Sfx.Play("step"); else if (k == Player.ActionKind.Ki) Sfx.Play("ki"); else if (k == Player.ActionKind.Thrust) Sfx.Play("swing"); };
        var cam = Player.GetNode<Camera2D>("Camera");
        cam.LimitLeft = 0; cam.LimitTop = 0; cam.LimitRight = ValeMap.MW * ValeMap.TS; cam.LimitBottom = ValeMap.MH * ValeMap.TS;
        Hud.SetValeMode(true);
        Player.InputLocked = true;
        Panels = new GamePanels(Hud, Player, this, 0.15f);
        Hud.SetMiniMap(this, 0.085f);
        Hud.RowClicked += (panel, i) => { if (panel == "inv") { UseItem(Hud.Rows[i].Id); RefreshInventory(); } else if (panel == "book") Hud.ShowBook(BookRows()); };
    }

    private void Spawn(string id, ValeFighter.Kind kind, float tx, float ty, Action<ValeFighter> set)
    {
        var f = new ValeFighter { Id = id, Type = kind, Host = this, Name = id, Position = new Vector2(tx * ValeMap.TS, ty * ValeMap.TS) };
        f.Home = f.Position;
        f.Max = kind switch { ValeFighter.Kind.Senior => Combat.SenHp, ValeFighter.Kind.Statue => 1, _ => Combat.DisHp }; f.Hp = f.Max;
        set(f);
        _world.AddChild(f);
        Fighters[id] = f;
    }

    /// <summary>Starts the prologue: a new journey (null) or the saved one (region "vale").</summary>
    public void Begin(SaveData? d)
    {
        Story = d?.Vale ?? new ValeState { Book = ValeState.StartingBook() };
        if (Story.Book.Count == 0) Story.Book = ValeState.StartingBook();
        _decals.Story = Story; _built.Ground.State = Story;
        foreach (var t in _built.Tall.Values) t.State = Story;
        Story.ApplyTo(Tech);
        if (d != null)
        {
            Inventory.Restore(d.ValeInventory.Select(i => (i.Id, i.Count)));
            foreach (var id in Story.Defeated) if (Fighters.TryGetValue(id, out var f)) f.SetDead();
            foreach (int i in Story.HerbsTaken) _herbs[i].QueueFree();
            foreach (int i in Story.PotsBroken) _pots[i].QueueFree();
            if (Story.GateOpen) { _built.Gate.QueueFree(); Respawn = ValeMap.CampEntry; }
            if (Story.DoorOpen) _built.Door.QueueFree();
            _chest.Open = Story.ChestOpen; _corpse.WithPage = !Story.HasManual;
            for (int i = 0; i < Story.Seen.Length && i < _seen.Length; i++) _seen[i] = Story.Seen[i] == '1';
            foreach (var mk in Story.Markers) if (mk.Length == 2) MapMarkers.Add(new Vector2(mk[0], mk[1]));
            Player.GlobalPosition = new Vector2(d.Spot[0], d.Spot[1]);
            Player.Revive();
            Player.Heal(-(Player.MaxHealth - Mathf.Clamp(d.Health, 1, Player.MaxHealth)));
            Player.Ki = Mathf.Clamp(d.Ki, 0, Player.MaxKi);
            Hud.Toast("Você desperta junto às pedras gêmeas, onde meditou.");
        }
        else
        {
            Player.GlobalPosition = ValeMap.Start;
            Player.Revive();
            if (ShowIntro) Hud.Overlay("SpiritOfMurin — Vale de Bambus", 1.6f);
            Hud.Toast("Suba a trilha. O acampamento dos discípulos fica além do rio.");
        }
        _seenVersion++;
        RebuildNotes();
        Player.Face(Vector2.Up);
        Player.GetNode<Camera2D>("Camera").ResetSmoothing();
        _built.Ground.QueueRedraw();
        Player.InputLocked = false;
        Sfx.Ambient(0.35f, 0.25f);
        GD.Print($"Vale: {(d == null ? "novo jogo" : "continuação")}");
    }

    // ------------------------------------------------------------------ loop

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        if (_hitstopReal > 0) { _hitstopReal -= (float)delta / Mathf.Max(0.01f, (float)Engine.TimeScale); if (_hitstopReal <= 0) Engine.TimeScale = 1; }
        Hud.SetHealth(Player.Health, Player.MaxHealth, Player.PendingRegen);
        Hud.SetEnergy(0, 0, Player.Ki, Player.MaxKi);
        Hud.SetCharge(Player.ChargeInvested, Player.ChargeCost);
        Hud.SetReinforce(Player.ReinforceCharges);
        Hud.SetHands(Bindings.F($"espada · {Story.Book.Count} páginas no Livro ([book])"));
        Fx.Lock = Lock;
        // the locked target at the top, as in the prototype: name, health and what its guard is doing
        if (Lock != null && IsInstanceValid(Lock) && Lock.Alive)
        {
            string name = Lock.Type switch { ValeFighter.Kind.Senior => "Espadachim Sênior", ValeFighter.Kind.Statue => "Estátua da Mestra", _ => "Discípulo da Guarda Fechada" };
            string stance = Lock.Type == ValeFighter.Kind.Statue ? "pedra em guarda" : Lock.Stagger > 0 ? "guarda quebrada" : Lock.Type == ValeFighter.Kind.Senior && Lock.AttackStance ? "postura de ataque"
                : Lock.Fooled > 0 ? "flanco aberto" : Lock.State == ValeFighter.St.Recover ? "recuperando" : "guarda fechada";
            if (Lock.Type == ValeFighter.Kind.Statue) Hud.SetFocus($"{name} · {stance}", 1);
            else Hud.SetFocus($"{name} · {stance}", Lock.Hp, Lock.Max);
        }
        else Hud.SetFocus(null, 0);
        var cam = Player.GetNode<Camera2D>("Camera");
        _shake = Mathf.Max(0, _shake - dt * 30);
        cam.Offset = _shake > 0 ? new Vector2((float)GD.RandRange(-_shake, _shake), (float)GD.RandRange(-_shake, _shake)) : Vector2.Zero;
        if (_speaker != null && IsInstanceValid(_speaker)) Hud.SpeechAt(GetViewport().GetCanvasTransform() * _speaker.GlobalPosition);
        Panels.Process(dt);
        if (Finale != null) { Finale.Update(dt); return; }
        if (_reveal > 0)
        {
            _reveal -= dt;
            if (_reveal <= 0)
            {
                Player.InputLocked = Hud.AnyPanelOpen;
                Hud.Toast(Bindings.F("Com o alvo travado: [step+] + direção para o lado do inimigo."));
                Hud.Toast(Bindings.F("Experimente na estátua: [lock] trava nela."));
            }
            return;
        }
        if (_dying > 0)
        {
            _dying -= dt;
            if (_dying <= 0) RespawnPlayer();
            return;
        }
        if (Lock != null && (!IsInstanceValid(Lock) || !Lock.Alive || Lock.GlobalPosition.DistanceTo(Player.GlobalPosition) > 330)) Lock = null;
        UpdateWaves(dt);
        // bodies never overlap
        foreach (var f in Fighters.Values)
        {
            if (!f.Alive && f.Type != ValeFighter.Kind.Statue) continue;
            Vector2 d = Player.GlobalPosition - f.GlobalPosition; float min = f.Type == ValeFighter.Kind.Statue ? 20 : 22;
            if (d.Length() < min && d.Length() > 0.01f && Player.Current is not (Player.ActionKind.Side or Player.ActionKind.Dash)) Player.MoveAndCollide(d.Normalized() * (min - d.Length()));
        }
        // drops and herbs are picked by walking over them
        for (int i = _drops.Count - 1; i >= 0; i--)
        {
            var it = _drops[i];
            if (it.Age > 0.45f && it.GlobalPosition.DistanceTo(Player.GlobalPosition) < 18) { Gain(it.Item, it.N); Fx.Burst(it.GlobalPosition + new Vector2(0, -6), 6, ValeTerrain.AmberHi, 50, 0.3f, 1); it.QueueFree(); _drops.RemoveAt(i); }
        }
        foreach (var h in _herbs.Where(h => IsInstanceValid(h) && !h.IsQueuedForDeletion()).ToList())
            if (h.GlobalPosition.DistanceTo(Player.GlobalPosition) < 16) { Story.HerbsTaken.Add(h.Index); Gain("erva", 1); Fx.Burst(h.GlobalPosition + new Vector2(0, -4), 8, new Color("#9fe08a"), 40, 0.4f, 1); h.QueueFree(); }
        UpdateFootprints();
        // the map remembers where you have walked
        var pt = new Vector2I(Mathf.FloorToInt(Player.GlobalPosition.X / ValeMap.TS), Mathf.FloorToInt(Player.GlobalPosition.Y / ValeMap.TS));
        if (pt != _seenAt)
        {
            _seenAt = pt;
            for (int y = pt.Y - 6; y <= pt.Y + 6; y++) for (int x = pt.X - 8; x <= pt.X + 8; x++)
                if (x >= 0 && y >= 0 && x < ValeMap.MW && y < ValeMap.MH && Mathf.Pow((x - pt.X) / 8f, 2) + Mathf.Pow((y - pt.Y) / 6f, 2) <= 1 && !_seen[y * ValeMap.MW + x]) { _seen[y * ValeMap.MW + x] = true; _seenVersion++; }
        }
        string an = ValeMap.AreaName(Player.GlobalPosition);
        if (an != _area) { _area = an; Hud.Banner(an); }
        if (!Story.LearnedSide && Player.GlobalPosition.X < 24 * ValeMap.TS && Player.GlobalPosition.X > 22.6f * ValeMap.TS && Player.GlobalPosition.Y > 20 * ValeMap.TS && Story.Once("gap")) Hud.Toast("Larga demais para atravessar.");
        UpdatePrompt();
    }

    /// <summary>The map tiles seen so far.</summary>
    public bool Seen(int tx, int ty) => tx >= 0 && ty >= 0 && tx < ValeMap.MW && ty < ValeMap.MH && _seen[ty * ValeMap.MW + tx];

    private void UpdatePrompt()
    {
        var s = NearSpot();
        string? text = s switch
        {
            "corpse" => "[use] · examinar o corpo", "door" => "[use] · examinar o selo", "slab" => "[use] · examinar a marca na pedra",
            "stele" => "[use] · ler a inscrição", "chest" => "[use] · abrir o baú", "meditate" => "[use] · meditar (grava a jornada)", _ => null,
        };
        Hud.SetPrompt(text == null ? null : Bindings.F(text), GetViewport().GetCanvasTransform() * Player.GlobalPosition);
    }

    // ------------------------------------------------------------------ input

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent e)
    {
        if (Finale != null) { Finale.Input(e); GetViewport().SetInputAsHandled(); return; }
        if (!Hud.StoryOpen || Hud.PauseOpen || Hud.MapOpen || Panels.OwnsStory) { if (Panels.Input(e)) { GetViewport().SetInputAsHandled(); return; } }
        if (Hud.StoryOpen)
        {
            if (e.IsActionPressed("use") || e.IsActionPressed("ui_accept") || e.IsActionPressed("ui_cancel") || e.IsActionPressed("cut"))
            { Hud.ShowStory(null); Player.InputLocked = _reveal > 0; Player.ReleaseGuard(); GetViewport().SetInputAsHandled(); }
            return;
        }
        if (Hud.BookOpen)
        {
            if (e.IsActionPressed("move_up")) Hud.MoveBook(-1);
            else if (e.IsActionPressed("move_down")) Hud.MoveBook(1);
            else if (e.IsActionPressed("book") || e.IsActionPressed("ui_cancel")) { Hud.ShowBook(null); Player.InputLocked = false; Player.ReleaseGuard(); }
            else return;
            if (Hud.BookOpen) Hud.ShowBook(BookRows());
            GetViewport().SetInputAsHandled();
            return;
        }
        if (Hud.InventoryOpen)
        {
            if (e.IsActionPressed("move_up") || e.IsActionPressed("ui_up")) Hud.MoveSelection(-1);
            else if (e.IsActionPressed("move_down") || e.IsActionPressed("ui_down")) Hud.MoveSelection(1);
            else if (e.IsActionPressed("move_left") || e.IsActionPressed("ui_left")) Hud.MoveSelectionSide(-1);
            else if (e.IsActionPressed("move_right") || e.IsActionPressed("ui_right")) Hud.MoveSelectionSide(1);
            else if (e.IsActionPressed("use") || e.IsActionPressed("ui_accept")) { if (Hud.Rows.Count > 0) UseItem(Hud.Rows[Hud.Selected].Id); RefreshInventory(); }
            else if (e.IsActionPressed("inventory") || e.IsActionPressed("ui_cancel")) { Hud.ShowInventory(false); Player.InputLocked = false; Player.ReleaseGuard(); }
            else return;
            GetViewport().SetInputAsHandled();
            return;
        }
        if (_reveal > 0 || _dying > 0) return;
        if (e.IsActionPressed("book")) { Hud.ShowBook(BookRows()); Player.InputLocked = true; Sfx.Play("page"); }
        else if (e.IsActionPressed("inventory")) { RefreshInventory(); Hud.ShowInventory(true); Player.InputLocked = true; Sfx.Play("page"); }
        else if (e.IsActionPressed("lock")) ToggleLock();
        else if (e.IsActionPressed("use")) { var s = NearSpot(); if (s != null) Interact(s); else return; }
        else if (e.IsActionPressed("ki")) Player.BeginKi();
        else if (e.IsActionPressed("heal")) UseItem("balsamo");
        else return;
        GetViewport().SetInputAsHandled();
    }

    /// <summary>Locks on the nearest living fighter within 240 px (or the statue), or releases the lock.</summary>
    public void ToggleLock()
    {
        if (Lock != null) { Lock = null; return; }
        Lock = Fighters.Values.Where(f => f.Alive).OrderBy(f => f.GlobalPosition.DistanceTo(Player.GlobalPosition)).FirstOrDefault(f => f.GlobalPosition.DistanceTo(Player.GlobalPosition) < 240);
        if (Lock == null && Story.Once("nolock")) Hud.Toast("Nenhum alvo por perto.");
    }

    // ------------------------------------------------------------------ items, book

    private void Gain(string id, int n)
    {
        int total = Inventory.Add(id, n);
        Hud.Toast($"+{n} {ValeItems.Of(id).Name}   (tem {total})");
        Sfx.Play("pickup");
    }

    private void RefreshInventory() => Hud.SetRows(Inventory.Entries.Select(e =>
    {
        var (name, desc) = ValeItems.Of(e.Id);
        bool use = e.Id is "balsamo" or "pedra";
        return new PlainsHud.Row(false, e.Id, $"{name} ×{e.Count}{(use ? Bindings.F("  [[use]: usar]") : "")}", Bindings.F(desc), true, Icon: e.Id, Count: e.Count, Action: use ? "Usar" : "", Name: name);
    }).Prepend(new PlainsHud.Row(false, "espada", "Espada  [na mão]", ValeItems.Of("espada").Desc, false, "Equipamento", Icon: "espada", Count: 1, Equipped: true, Action: "Na mão", Name: "Espada")).ToList());

    /// <summary>Uses a prologue item (the balm heals, the ki stone fills the Ki). Returns true when used.</summary>
    public bool UseItem(string id)
    {
        if (Inventory.Count(id) <= 0) { if (id == "balsamo") Hud.Toast("Nenhum bálsamo."); return false; }
        if (id == "balsamo")
        {
            if (Player.Health >= Player.MaxHealth) { Hud.Toast("Sua vida já está cheia."); return false; }
            Player.Heal(4); Fx.Burst(Player.GlobalPosition + new Vector2(0, -18), 10, new Color("#7fe0a8"), 60, 0.5f, 2);
        }
        else if (id == "pedra") { Player.Ki = Player.MaxKi; Fx.Burst(Player.GlobalPosition + new Vector2(0, -18), 12, new Color("#a6f2e2"), 70, 0.5f, 2); }
        else return false;
        Inventory.Remove(id); Sfx.Play("chime"); Hud.Toast($"Usou {ValeItems.Of(id).Name}.");
        return true;
    }

    /// <summary>The book's pages: name, state, command, numbers and origin (the torn page while the combination is missing).</summary>
    public List<BookPage> BookRows() => BookPage.Build(Story.Book, Tech, Story, plains: false);

    // ------------------------------------------------------------------ hits

    private void OnBlade(Vector2 origin, Vector2 dir, float reach, int damage, int swing)
    {
        if (swing != _lastSwing) { _lastSwing = swing; _hitThisSwing.Clear(); }
        bool thrust = Player.Current == Player.ActionKind.Thrust;
        float r = reach + (thrust ? Player.ThrustSlack : Player.CutSlack);
        foreach (var f in Fighters.Values)
        {
            if (!f.Alive || _hitThisSwing.Contains(f.Id)) continue;
            if (!Player.InBladeArc(origin, dir, r, Player.BladeArcDegrees, f.GlobalPosition, f.Radius)) continue;
            _hitThisSwing.Add(f.Id);
            // the Reforço de Ki: +10% damage (fractional, no rounding) and one break roll against the guard met
            var hit = Player.ResolveStrike(damage, melee: true); // the Reforço (the prologue has no sheet: no FOR, no critical)
            float dmg = hit.Amount;
            bool breaks = f.Type != ValeFighter.Kind.Statue && f.WouldParry(origin) && Player.RollReinforceBreak();
            string res = StrikeFighter(f, origin, dmg, breaks, breaks ? Tech.Data.KiReinforce.Full.Stagger : 0, Player.HeavyCut, melee: true, crit: hit.Critical);
            if (breaks && res is "hit" or "killed") Fx.Say(f.GlobalPosition + new Vector2(0, -58), "guarda rompida", ValeTerrain.Learn, 1.2f);
            if (Player.Current == Player.ActionKind.Recoil) return; // parried: the recoil replaced the cut
        }
        foreach (var p in _pots.Where(p => IsInstanceValid(p) && !p.IsQueuedForDeletion()).ToList())
            if (Player.InBladeArc(origin, dir, r, Player.BladeArcDegrees, p.GlobalPosition, 6)) BreakPot(p);
    }

    /// <summary>A blade or a wave reached a fighter: parry, flank, hit, kill.</summary>
    public string StrikeFighter(ValeFighter f, Vector2 from, float dmg, bool lamina, float stagger, bool heavy, bool melee, bool crit = false)
    {
        Vector2 mid = (f.GlobalPosition + from) / 2 + new Vector2(0, -18);
        bool guarded = f.Guarding;
        string r = f.Strike(from, dmg, lamina, stagger, heavy);
        switch (r)
        {
            case "parried":
                _parryCount++;
                if (f.Type != ValeFighter.Kind.Statue) Fx.Say(mid + new Vector2(0, -12), "aparado", new Color("#ece5d0"), 0.5f);
                Fx.Burst(mid, 9, f.Type == ValeFighter.Kind.Statue ? new Color("#b9b5a6") : new Color("#fff3c4"), 120, 0.25f, 1);
                Fx.AddRing(mid, new Color("#fff3c4"));
                Sfx.Play(f.Type == ValeFighter.Kind.Statue ? "stone" : "parry"); Hitstop(0.045f);
                if (melee) Player.Recoil(Player.GlobalPosition - f.GlobalPosition, Combat.ParryPush);
                if (f.Type != ValeFighter.Kind.Statue && _parryCount == 3 && !Story.LearnedSide) { Say(f, "De frente, nada passa pela Guarda Fechada."); Hud.Toast("De frente, essa guarda não cai."); AddClue("frente", "De frente, essa guarda não cai: ela apara tudo o que vem pela frente.", f.GlobalPosition); }
                return r;
            case "statue":
                if (guarded)
                {
                    Sfx.Play("chime"); Fx.Burst(mid, 10, ValeTerrain.Learn, 110, 0.4f, 1); Fx.AddRing(mid, ValeTerrain.LearnGlow, 0.3f);
                    Fx.Say(f.GlobalPosition + new Vector2(0, -50), "flanco", ValeTerrain.Learn, 0.8f);
                    if (Story.Once("statueFlank")) Hud.Toast("A pedra responde ao golpe no flanco.");
                }
                return r;
        }
        if (f.Type != ValeFighter.Kind.Statue && r is "hit" or "flank" or "killed") Numbers.Dealt(f.GlobalPosition + new Vector2(0, -46), new StrikeResult(dmg, crit));
        if (r == "flank")
        {
            _flankHits++;
            Fx.Say(f.GlobalPosition + new Vector2(0, -50), "flanco", ValeTerrain.Learn, 0.8f);
            if (Story.Once("flankWhy")) Hud.Toast("A guarda ficou olhando para onde você estava. O flanco estava aberto.");
        }
        Hitstop(Combat.Hitstop * (heavy || lamina ? 1.6f : 1)); Shake(heavy || lamina ? 1.4f : 1);
        Sfx.Play("hit"); Fx.Burst(mid, 12, ValeTerrain.Learn, 130, 0.3f, 2); Fx.Burst(mid, 6, Colors.White, 60, 0.2f, 1);
        if (r == "killed") KillFighter(f);
        return r;
    }

    private void Hitstop(float seconds)
    {
        if (seconds <= 0) return;
        Engine.TimeScale = 0.05f;
        _hitstopReal = Mathf.Max(_hitstopReal, seconds);
    }

    private void Shake(float k) => _shake = Mathf.Max(_shake, Combat.Shake * k);

    private void KillFighter(ValeFighter f)
    {
        Fx.Burst(f.GlobalPosition + new Vector2(0, -16), 24, ValeTerrain.Learn, 150, 0.6f, 2);
        if (Lock == f) Lock = null;
        if (!Story.Defeated.Contains(f.Id)) Story.Defeated.Add(f.Id);
        if (f.Type == ValeFighter.Kind.Disciple)
        {
            Drop(f.GlobalPosition, f.Drops);
            if (f.Bridge)
            {
                Story.GateOpen = true; Respawn = ValeMap.CampEntry; Sfx.Play("gate");
                if (IsInstanceValid(_built.Gate)) _built.Gate.QueueFree();
                _built.Ground.QueueRedraw();
                Hud.Toast("O guardião cai. O portão do acampamento se abre.");
                AddClue("portao", "O portão do acampamento se abriu. Além dele, o Espadachim Sênior espera.", f.GlobalPosition, new Vector2(29, 7) * ValeMap.TS);
                DoneQuest("guarda"); TakeQuest("senior");
            }
        }
        else if (f.Type == ValeFighter.Kind.Senior)
        {
            foreach (var (id, n) in f.Drops) Gain(id, n);
            DoneQuest("senior");
            foreach (var w in _waves) w.QueueFree();
            _waves.Clear();
            Lock = null;
            Finale = new SeniorFinale(this, f);
        }
    }

    private void Drop(Vector2 at, (string Id, int N)[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            float a = i / (float)list.Length * Mathf.Tau + 0.6f;
            var d = new ValeDrop { Item = list[i].Id, N = list[i].N, Position = at + new Vector2(Mathf.Cos(a) * 6, Mathf.Sin(a) * 4), V = new Vector2(Mathf.Cos(a) * 60, Mathf.Sin(a) * 40) };
            _world.AddChild(d); _drops.Add(d);
        }
    }

    private void BreakPot(ValePot p)
    {
        Story.PotsBroken.Add(p.Index); Sfx.Play("pot");
        Fx.Burst(p.GlobalPosition + new Vector2(0, -8), 12, new Color("#a0643c"), 90, 0.4f, 2);
        Drop(p.GlobalPosition, new[] { (ValeTerrain.H(p.Index, 3) % 10 < 6 ? "prata" : "erva", 1) });
        p.QueueFree();
    }

    private void OnKi(Player.KiShot shot)
    {
        var w = new KiWave { Shot = shot, GlobalPosition = shot.Origin, ZIndex = 10 };
        AddChild(w); _waves.Add(w);
        if (shot.Lamina) TriggerLamina();
    }

    private void UpdateWaves(float dt)
    {
        foreach (var w in _waves.ToList())
        {
            w.Advance(dt);
            var t = Map.AtPixel(w.Foot);
            if (t is not (VTile.Water or VTile.Chasm or VTile.Grass or VTile.Path or VTile.Camp or VTile.Bridge or VTile.TombFloor) && !(t == VTile.Gate && Story.GateOpen) && !(t == VTile.Door && Story.DoorOpen))
            { w.Stop(); Fx.Burst(w.GlobalPosition, 6, new Color("#3fae9e"), 60, 0.25f, 1); }
            foreach (var f in Fighters.Values)
            {
                if (!f.Alive || w.HitSet.Contains(f.Id)) continue;
                if (f.GlobalPosition.DistanceTo(w.Foot) < (w.Shot.Lamina ? 22 : 16))
                {
                    w.HitSet.Add(f.Id);
                    bool guarded = f.Guarding;
                    string r = StrikeFighter(f, w.Foot - w.Shot.Dir * w.Shot.Speed * 0.2f, w.Shot.Damage, w.Shot.Lamina || w.Shot.Breaks, w.Shot.Stagger, false, melee: false);
                    if (w.Shot.Breaks && guarded && f.Type != ValeFighter.Kind.Statue && r is "hit" or "killed")
                    {
                        Fx.Say(f.GlobalPosition + new Vector2(0, -58), "guarda rompida", ValeTerrain.Learn, 1.2f);
                        if (Story.Once("chargeBreak")) Hud.Toast("O Ki concentrado rompe a guarda. Ataque agora!");
                    }
                    if (r == "parried" && !w.Shot.Lamina && !w.Shot.Breaks) w.Stop();
                }
            }
            foreach (var p in _pots.Where(p => IsInstanceValid(p) && !p.IsQueuedForDeletion()).ToList())
                if (p.GlobalPosition.DistanceTo(w.Foot) < 16) BreakPot(p);
            if (w.Done) { w.QueueFree(); _waves.Remove(w); }
        }
    }

    // the first charged shot reveals the secret technique: the banner and a page in the book
    private void OnSecret(string id)
    {
        if (id == "ki_reinforce")
        {
            if (Story.Book.Any(b => b.Tech == "ki_reinforce")) return;
            Story.AddBook(new BookEntry { Name = Tech.Data.KiReinforce.Name, Kind = "técnica", Key = "modificador + corte", Where = $"Descoberta por você: {ValeMap.AreaName(Player.GlobalPosition)}. O Ki percorre a lâmina.", Tech = "ki_reinforce", State = "Plena" });
            Tech.Set("ki_reinforce", TechniqueState.Full);
            Hud.Banner(Tech.Data.KiReinforce.Name, 2.6f);
            Sfx.Play("learn");
            return;
        }
        if (id != "ki_charge" || Story.Book.Any(b => b.Tech == "ki_charge")) return;
        Story.AddBook(new BookEntry { Name = Tech.Data.KiCharge.Name, Kind = "técnica", Key = "segurar Ki, soltar", Where = $"Descoberta por você: {ValeMap.AreaName(Player.GlobalPosition)}. O Ki se junta na palma enquanto o botão fica preso.", Tech = "ki_charge", State = "Plena" });
        Tech.Set("ki_charge", TechniqueState.Full);
        Hud.Banner(Tech.Data.KiCharge.Name, 2.8f);
        Hud.Toast(Bindings.F("Técnica secreta descoberta: segurar [ki] concentra o Ki; soltar dispara. Veja o Livro ([book])."));
        Sfx.Play("learn");
    }

    private void TriggerLamina()
    {
        bool first = !Story.ComboFound;
        Story.ComboFound = true;
        Hitstop(0.22f); Shake(1.5f); Sfx.Play("combo");
        for (int i = 0; i < 18; i++) { float a = i / 18f * Mathf.Tau; Fx.Particle(Player.GlobalPosition + new Vector2(0, -14), new Vector2(Mathf.Cos(a) * 140, Mathf.Sin(a) * 100), ValeTerrain.Learn, 0.5f, 2); }
        Hud.Banner("Passo da Lâmina Silenciosa", 2.6f);
        if (first)
        {
            Story.AddBook(new BookEntry { Name = "Passo da Lâmina Silenciosa", Kind = "combinação", Key = "passo lateral → corte → Ki", Where = $"Descoberta por você: {ValeMap.AreaName(Player.GlobalPosition)}. O Ki segue o corte e quebra a guarda.", Tech = "lamina", State = "Plena" });
            Tech.Set("lamina", TechniqueState.Full);
        }
        DoneQuest("lamina");
    }

    /// <inheritdoc />
    public void HitPlayer(ValeFighter by)
    {
        if (PlayerDown || Finale != null) return;
        if (Player.TakeDamage(Combat.EnemyDamage, by.GlobalPosition))
        {
            Hitstop(0.09f); Shake(1.6f); Sfx.Play("hurt");
            Fx.Burst(Player.GlobalPosition + new Vector2(0, -18), 10, new Color("#e8834c"), 120, 0.3f, 2);
        }
    }

    /// <inheritdoc />
    public void Say(ValeFighter who, string line)
    {
        _speaker = who;
        Hud.Speech(line, GetViewport().GetCanvasTransform() * who.GlobalPosition);
    }

    /// <inheritdoc />
    public void OnAggro(ValeFighter who) { if (who.Bridge) TakeQuest("guarda"); }

    /// <inheritdoc />
    public void OnFlee(ValeFighter who)
    {
        if (Story.HasManual) return;
        Say(who, "Fuja, então. Pela frente, ninguém passa por mim.");
        if (Story.Once("valeHint")) { Hud.Toast("Uma trilha desce para o vale, a leste."); AddClue("vale", "Uma trilha desce para o vale, a leste. (O discípulo da ponte: \"Pela frente, ninguém passa por mim.\")", Player.GlobalPosition, new Vector2(40, 30) * ValeMap.TS); }
    }

    /// <inheritdoc />
    public void EnemySlash(ValeFighter who, bool big)
    {
        Fx.EnemySlash(who.GlobalPosition + new Vector2(Mathf.Cos(who.Face) * 26, -14 + Mathf.Sin(who.Face) * 18), who.Face, big);
        Sfx.Play("swing");
    }

    // ------------------------------------------------------------------ story

    private static readonly Dictionary<string, (string Name, string Desc)> Quests = new()
    {
        ["guarda"] = ("A guarda fechada", "O discípulo da ponte apara tudo o que vem de frente. Deve haver um jeito."),
        ["manual"] = ("O manual dividido", "A anotação na margem fala do túmulo da mestra, entre as pedras gêmeas."),
        ["senior"] = ("O Espadachim Sênior", "Além do portão, o mestre do acampamento espera."),
        ["lamina"] = ("A lâmina que segue o passo", "“O ki caminha atrás da lâmina que segue o passo.”"),
    };

    private void TakeQuest(string id)
    {
        if (Story.Quests.ContainsKey(id)) return;
        Story.Quests[id] = false;
        Vector2 at = id == "guarda" && Fighters.TryGetValue("dis_bridge", out var b) ? b.GlobalPosition : Player.GlobalPosition;
        Story.QuestAt[id] = new[] { at.X, at.Y };
        RebuildNotes();
        Hud.Toast(Bindings.F($"Nova missão: {Quests[id].Name}  ([map]: mapa)"));
    }

    private void DoneQuest(string id)
    {
        if (Story.Quests.TryGetValue(id, out bool done) && !done) { Story.Quests[id] = true; RebuildNotes(); Hud.Toast($"Missão cumprida: {Quests[id].Name}"); }
    }

    /// <summary>The spot E would examine, or null.</summary>
    public string? NearSpot()
    {
        Vector2 p = Player.GlobalPosition;
        foreach (var (id, at, r) in new (string, Vector2, float)[] { ("corpse", ValeMap.Corpse, 34), ("door", ValeMap.Door, 36), ("slab", ValeMap.Slab, 34), ("stele", ValeMap.Stele, 34), ("chest", ValeMap.Chest, 30), ("meditate", ValeMap.Meditate, 26) })
        {
            if (p.DistanceTo(at) > r) continue;
            if (id == "door" && Story.DoorOpen || id == "chest" && Story.ChestOpen) continue;
            return id;
        }
        return null;
    }

    private void Panel(string title, params string[] lines) { Hud.ShowStory(title, lines); Player.InputLocked = true; Sfx.Play("page"); }

    /// <summary>Examines a spot (the body, the seal, the cut, the inscription, the chest, the mat).</summary>
    public void Interact(string id)
    {
        switch (id)
        {
            case "corpse":
                if (!Story.HasManual)
                {
                    Story.HasManual = true; _corpse.WithPage = false;
                    Panel("Metade de um manual", "“...a lâmina não vence a guarda: contorna-a. Entra pelo flanco, onde a guarda não olha.”", "A página que ensinava o passo foi arrancada.", "Na margem, à mão: “O resto ficou no túmulo da mestra, entre as pedras gêmeas.”");
                    Story.AddBook(new BookEntry { Name = "Metade de um manual", Kind = "fragmento", Where = "Corpo de um discípulo, no Vale de Bambus. Falta a página do passo." });
                    TakeQuest("manual");
                    AddClue("manual", "A anotação na margem do manual: \"O resto ficou no túmulo da mestra, entre as pedras gêmeas.\"", ValeMap.Corpse, new Vector2(51, 26) * ValeMap.TS);
                }
                else Panel("O discípulo", "Ele não carrega mais nada.");
                break;
            case "door":
                if (Story.HasManual)
                {
                    Story.DoorOpen = true; Sfx.Play("gate");
                    if (IsInstanceValid(_built.Door)) _built.Door.QueueFree();
                    foreach (var t in _built.Tall.Values.Where(t => t.Kind == VTile.Door)) t.QueueRedraw();
                    Panel("O selo do túmulo", "O símbolo partido no manual completa o selo.", "A porta de pedra se abre.");
                }
                else Panel("O selo do túmulo", "Um selo partido ao meio. Falta a outra metade.");
                break;
            case "slab":
                if (Story.LearnedSide) Panel("A marca na pedra", "Agora você entende onde ela pisava: fora da frente da guarda.");
                else { Story.SlabRead = true; AddClue("pegadas", "Aos pés da estátua, pegadas saem da frente dela e vão para o lado. Pise onde ela pisava.", ValeMap.Slab, ValeMap.Statue); Panel("A marca na pedra", "Um corte curvo na pedra, como um golpe que contorna algo.", "Aos pés da estátua em guarda, pegadas gravadas no chão", "saem da frente dela e vão para o lado.", "Pise onde ela pisava."); }
                break;
            case "stele":
                Story.SteleRead = true;
                AddClue("inscricao", "\"O ki caminha atrás da lâmina que segue o passo.\" (inscrição além da fenda)", ValeMap.Stele);
                Panel("Inscrição", "“A lâmina que segue o passo não faz ruído.", "O ki caminha atrás dela.”");
                Story.AddBook(new BookEntry { Name = "Inscrição da fenda", Kind = "fragmento", Where = "Além da Fenda: “O ki caminha atrás da lâmina que segue o passo.”" });
                if (!Story.ComboFound) TakeQuest("lamina");
                break;
            case "chest":
                Story.ChestOpen = true; _chest.Open = true; _chest.QueueRedraw(); Sfx.Play("gate");
                Panel("Um baú esquecido", "Alguém escondeu isto além da fenda, onde ninguém atravessa.");
                Drop(ValeMap.Chest + new Vector2(0, 10), new[] { ("balsamo", 2), ("pedra", 1), ("prata", 8) });
                break;
            case "meditate":
                Meditate();
                break;
        }
    }

    private void UpdateFootprints()
    {
        if (!Story.SlabRead || Story.LearnedSide) return;
        for (int i = 0; i < ValeMap.Marks.Length; i++)
        {
            if (Player.GlobalPosition.DistanceTo(ValeMap.Marks[i]) > 11) continue;
            if (i == Story.FpIdx)
            {
                Story.FpIdx++; Sfx.Play("step"); Fx.Burst(ValeMap.Marks[i], 8, ValeTerrain.LearnGlow, 50, 0.4f, 1);
                if (Story.FpIdx == 3) LearnSide();
            }
            else if (i > Story.FpIdx) { Story.FpIdx = 0; Sfx.Play("fizzle"); if (Story.Once("fpwrong")) Hud.Toast("Não. Ela começava de frente para a estátua."); }
        }
    }

    /// <summary>The side step is learned at the master's statue (and the long step with it).</summary>
    public void LearnSide()
    {
        Story.LearnedSide = true; _reveal = 3.2f; Sfx.Play("learn");
        Player.InputLocked = true;
        Player.Revive(); Player.Ki = Player.MaxKi; // the breath settles at the master's feet
        Story.AddBook(new BookEntry { Name = "Passo Lateral", Kind = "técnica", Key = "L + lado do alvo (com alvo travado)", Where = "Túmulo da Mestra do Vale Nublado: as pegadas ao lado do corte na pedra.", Tech = "side_step", State = "Plena" });
        Story.AddBook(new BookEntry { Name = "Passo longo", Kind = "técnica", Key = "sem trava: direção + L", Where = "Nasce do passo lateral. Atravessa o que o passo curto não alcança.", Tech = "dash", State = "Plena" });
        var shortStep = Story.Book.FirstOrDefault(b => b.Name == "Passo curto"); if (shortStep != null) shortStep.Where = "Sua arte de origem: para frente ou para trás.";
        Tech.Set("side_step", TechniqueState.Full); Tech.Set("dash", TechniqueState.Full);
        Hud.Banner("Passo Lateral", 3.0f);
        AddClue("flanco", "A guarda olha para onde você estava. Com o alvo travado, o passo lateral abre o flanco.", ValeMap.Statue, new Vector2(29, 16) * ValeMap.TS);
        Fx.Burst(ValeMap.Statue + new Vector2(0, -20), 24, ValeTerrain.Learn, 90, 0.9f, 2);
        DoneQuest("manual");
    }

    // a side step or a dash never ends over the gap
    private void Settle(Vector2 dir)
    {
        bool Gap(Vector2 p) => Map.AtPixel(p) == VTile.Chasm;
        if (!Gap(Player.GlobalPosition)) return;
        for (int i = 2; i <= 80; i += 2)
            foreach (int s in new[] { 1, -1 })
            {
                Vector2 to = Player.GlobalPosition + dir * i * s;
                if (!Gap(to) && Map.AtPixel(to) is not (VTile.Rock or VTile.Water)) { Player.GlobalPosition = to; return; }
            }
        Player.GlobalPosition = Respawn;
    }

    private void RespawnPlayer()
    {
        Story.Deaths++;
        Player.GlobalPosition = Respawn; Player.Revive(); Player.Ki = Player.MaxKi;
        Lock = null;
        foreach (var w in _waves) w.QueueFree();
        _waves.Clear();
        foreach (var f in Fighters.Values) f.ResetToHome();
        Player.GetNode<Camera2D>("Camera").ResetSmoothing();
        Hud.Toast(Respawn == ValeMap.Start ? "Você desperta no início da trilha. O que aprendeu, ficou." : "Você desperta na entrada do acampamento.");
        if (!Story.HasManual && Story.Once("valeHint")) { Hud.Toast("Uma trilha desce para o vale, a leste."); AddClue("vale", "Uma trilha desce para o vale, a leste.", Player.GlobalPosition, new Vector2(40, 30) * ValeMap.TS); }
    }

    // ------------------------------------------------------------------ save

    /// <summary>The journey as it would be written now (region "vale").</summary>
    public SaveData Snapshot(string phase = "vale")
    {
        Story.Seen = new string(_seen.Select(b => b ? '1' : '0').ToArray());
        Story.Markers = MapMarkers.Select(m => new[] { m.X, m.Y }).ToList();
        return new SaveData
        {
            Region = "vale", Phase = phase, Seed = 0,
            Spot = new[] { ValeMap.Meditate.X, ValeMap.Meditate.Y + 10 },
            Vale = Story, ValeInventory = Inventory.Entries.Select(e => new SavedItem { Id = e.Id, Count = e.Count }).ToList(),
            Book = Story.Book, Equipped = "sword", Health = Player.Health, Ki = Player.Ki, Level = 1,
        };
    }

    private void Meditate()
    {
        if (Fighters.Values.Any(f => f.Alive && f.Type != ValeFighter.Kind.Statue && f.State is ValeFighter.St.Guard or ValeFighter.St.Windup or ValeFighter.St.Strike or ValeFighter.St.Advance && f.GlobalPosition.DistanceTo(Player.GlobalPosition) < 260))
        { Hud.Toast("Não dá para meditar com um inimigo em guarda por perto."); return; }
        Player.Ki = Player.MaxKi; _decals.Meditated = 1.6f; Sfx.Play("chime");
        bool ok = SaveGame.Write(SavePath, Snapshot());
        if (ok) { Hud.Toast("Meditação gravada."); Hud.Toast("Se cair em duelo, volta à trilha. Se fechar o jogo, volta a esta pedra."); }
        else Hud.Toast("A meditação não foi gravada: não foi possível escrever o arquivo.");
    }

    /// <summary>Writes a save at a phase of the ending (the safe point) — called by the finale.</summary>
    public bool WritePhase(string phase) => SaveGame.Write(SavePath, Snapshot(phase));

    /// <summary>The ending is over: hand the journey to the campaign.</summary>
    public void EndPrologue() => PrologueEnded?.Invoke(Snapshot("senior_won"));

    // for the finale
    internal void FinaleShake(float k) => Shake(k);
    internal Node2D World => _world;
}
