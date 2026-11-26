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

namespace SpiritOfMurin.Vale;

/// <summary>
/// The prologue — the Vale de Bambus — ported from the HTML prototype: the trail, the bridge guardian of
/// the Closed Guard, the half manual on a body in the bamboo, the sealed tomb, the cut in the stone and
/// the footprints that teach the side step, the inscription beyond the gap and the Silent Blade Step,
/// the camp and the Senior Swordsman. Winning against the Senior starts the ending (<see cref="SeniorFinale"/>),
/// which ends the prologue and hands over to the Planície. Meditating on the mat by the twin stones writes
/// the journey.
/// </summary>
public partial class ValeGame : Node2D, IValeHost
{
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
    private readonly List<QiWave> _waves = new();
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
        Player.MaxQi = Combat.PlayerQiMax; Player.QiRegen = Combat.PlayerQiRegen; Player.Qi = Player.MaxQi;
        Player.MaxVigor = 0;
        Player.Tech = Tech;
        Player.LockTarget = () => Lock;
        Player.BladeActive += OnBlade;
        Player.QiFired += OnQi;
        Player.SideStepped += t => { if (t is ValeFighter f && f.Type != ValeFighter.Kind.Statue) f.Fooled = f.Type == ValeFighter.Kind.Senior ? Combat.SenFooled : Combat.DisFooled; Sfx.Play("side"); };
        Player.MoveEnded += Settle;
        Player.Evaded += () => { Sfx.Play("evade"); Fx.Say(Player.GlobalPosition + new Vector2(0, -46), "passo", ValeTerrain.Learn); };
        Player.TechniqueRefused += (_, m) => { Hud.Toast(m); Sfx.Play("fizzle"); };
        Player.Fainted += () => { _dying = 1.5f; Sfx.Play("die"); Lock = null; };
        Player.ActionStarted += (k, n) => { if (k == Player.ActionKind.Cut) Sfx.Play(n == 3 ? "swing3" : "swing"); else if (k is Player.ActionKind.Step) Sfx.Play("step"); else if (k == Player.ActionKind.Qi) Sfx.Play("qi"); else if (k == Player.ActionKind.Thrust) Sfx.Play("swing"); };
        var cam = Player.GetNode<Camera2D>("Camera");
        cam.LimitLeft = 0; cam.LimitTop = 0; cam.LimitRight = ValeMap.MW * ValeMap.TS; cam.LimitBottom = ValeMap.MH * ValeMap.TS;
        Hud.SetValeMode(true);
        Player.InputLocked = true;
    }

    private void Spawn(string id, ValeFighter.Kind kind, float tx, float ty, Action<ValeFighter> set)
    {
        var f = new ValeFighter { Id = id, Type = kind, Host = this, Name = id, Position = new Vector2(tx * ValeMap.TS, ty * ValeMap.TS) };
        f.Home = f.Position;
        f.Max = f.Hp = kind switch { ValeFighter.Kind.Senior => Combat.SenHp, ValeFighter.Kind.Statue => 1, _ => Combat.DisHp };
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
            Player.GlobalPosition = new Vector2(d.Spot[0], d.Spot[1]);
            Player.Revive();
            Player.Heal(-(Player.MaxHealth - Mathf.Clamp(d.Health, 1, Player.MaxHealth)));
            Player.Qi = Mathf.Clamp(d.Qi, 0, Player.MaxQi);
            Hud.Toast("Você desperta junto às pedras gêmeas, onde meditou.");
        }
        else
        {
            Player.GlobalPosition = ValeMap.Start;
            Player.Revive();
            if (ShowIntro) Hud.Overlay("SpiritOfMurin — Vale de Bambus", 1.6f);
            Hud.Toast("Suba a trilha. O acampamento dos discípulos fica além do rio.");
        }
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
        Hud.SetEnergy(0, 0, Player.Qi, Player.MaxQi);
        Hud.SetHands($"espada · {Story.Book.Count} páginas no Livro (B)");
        Fx.Lock = Lock;
        var cam = Player.GetNode<Camera2D>("Camera");
        _shake = Mathf.Max(0, _shake - dt * 30);
        cam.Offset = _shake > 0 ? new Vector2((float)GD.RandRange(-_shake, _shake), (float)GD.RandRange(-_shake, _shake)) : Vector2.Zero;
        if (_speaker != null && IsInstanceValid(_speaker)) Hud.SpeechAt(GetViewport().GetCanvasTransform() * _speaker.GlobalPosition);
        if (Finale != null) { Finale.Update(dt); return; }
        if (_reveal > 0)
        {
            _reveal -= dt;
            if (_reveal <= 0)
            {
                Player.InputLocked = Hud.AnyPanelOpen;
                Hud.Toast("Com o alvo travado: L ou clique direito + direção para o lado do inimigo.");
                Hud.Toast("Experimente na estátua: Espaço trava nela.");
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
                if (x >= 0 && y >= 0 && x < ValeMap.MW && y < ValeMap.MH && Mathf.Pow((x - pt.X) / 8f, 2) + Mathf.Pow((y - pt.Y) / 6f, 2) <= 1) _seen[y * ValeMap.MW + x] = true;
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
            "corpse" => "E · examinar o corpo", "door" => "E · examinar o selo", "slab" => "E · examinar a marca na pedra",
            "stele" => "E · ler a inscrição", "chest" => "E · abrir o baú", "meditate" => "E · meditar (grava a jornada)", _ => null,
        };
        Hud.SetPrompt(text, GetViewport().GetCanvasTransform() * Player.GlobalPosition);
    }

    // ------------------------------------------------------------------ input

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent e)
    {
        if (Finale != null) { Finale.Input(e); GetViewport().SetInputAsHandled(); return; }
        if (Hud.StoryOpen)
        {
            if (e.IsActionPressed("use") || e.IsActionPressed("ui_accept") || e.IsActionPressed("ui_cancel") || e.IsActionPressed("cut"))
            { Hud.ShowStory(null); Player.InputLocked = _reveal > 0; GetViewport().SetInputAsHandled(); }
            return;
        }
        if (Hud.BookOpen)
        {
            if (e.IsActionPressed("move_up")) Hud.MoveBook(-1);
            else if (e.IsActionPressed("move_down")) Hud.MoveBook(1);
            else if (e.IsActionPressed("book") || e.IsActionPressed("ui_cancel")) { Hud.ShowBook(null); Player.InputLocked = false; }
            else return;
            if (Hud.BookOpen) Hud.ShowBook(BookRows());
            GetViewport().SetInputAsHandled();
            return;
        }
        if (Hud.InventoryOpen)
        {
            if (e.IsActionPressed("move_up")) Hud.MoveSelection(-1);
            else if (e.IsActionPressed("move_down")) Hud.MoveSelection(1);
            else if (e.IsActionPressed("use") || e.IsActionPressed("ui_accept")) { if (Hud.Rows.Count > 0) UseItem(Hud.Rows[Hud.Selected].Id); RefreshInventory(); }
            else if (e.IsActionPressed("inventory") || e.IsActionPressed("ui_cancel")) { Hud.ShowInventory(false); Player.InputLocked = false; }
            else return;
            GetViewport().SetInputAsHandled();
            return;
        }
        if (_reveal > 0 || _dying > 0) return;
        if (e.IsActionPressed("book")) { Hud.ShowBook(BookRows()); Player.InputLocked = true; Sfx.Play("page"); }
        else if (e.IsActionPressed("inventory")) { RefreshInventory(); Hud.ShowInventory(true); Player.InputLocked = true; Sfx.Play("page"); }
        else if (e.IsActionPressed("lock")) ToggleLock();
        else if (e.IsActionPressed("use")) { var s = NearSpot(); if (s != null) Interact(s); else return; }
        else if (e.IsActionPressed("qi")) Player.RequestQi();
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
        return new PlainsHud.Row(false, e.Id, $"{name} ×{e.Count}{(use ? "  [E: usar]" : "")}", desc, true);
    }).Prepend(new PlainsHud.Row(false, "espada", "Espada  [na mão]", ValeItems.Of("espada").Desc, true, "Equipamento")).ToList());

    /// <summary>Uses a prologue item (the balm heals, the qi stone fills the Qi). Returns true when used.</summary>
    public bool UseItem(string id)
    {
        if (Inventory.Count(id) <= 0) { if (id == "balsamo") Hud.Toast("Nenhum bálsamo."); return false; }
        if (id == "balsamo")
        {
            if (Player.Health >= Player.MaxHealth) { Hud.Toast("Sua vida já está cheia."); return false; }
            Player.Heal(4); Fx.Burst(Player.GlobalPosition + new Vector2(0, -18), 10, new Color("#7fe0a8"), 60, 0.5f, 2);
        }
        else if (id == "pedra") { Player.Qi = Player.MaxQi; Fx.Burst(Player.GlobalPosition + new Vector2(0, -18), 12, new Color("#a6f2e2"), 70, 0.5f, 2); }
        else return false;
        Inventory.Remove(id); Sfx.Play("chime"); Hud.Toast($"Usou {ValeItems.Of(id).Name}.");
        return true;
    }

    /// <summary>The book's rows: name (with the state of prologue techniques) and the page's text.</summary>
    public List<PlainsHud.Row> BookRows()
    {
        var rows = new List<PlainsHud.Row>(); string last = "";
        foreach (var b in Story.Book.OrderBy(b => b.Kind == "técnica" ? 0 : b.Kind == "combinação" ? 1 : 2))
        {
            string group = b.Kind != last ? (b.Kind == "técnica" ? "Técnicas" : b.Kind == "combinação" ? "Combinações" : "Fragmentos") : "";
            last = b.Kind;
            string state = b.State.Length > 0 ? $"  [{b.State}]" : "";
            rows.Add(new PlainsHud.Row(false, b.Name, b.Name + state, $"{b.Name} · {b.Kind}\nComando: {(b.Key.Length > 0 ? b.Key : "—")}\n{b.Where}", true, group));
        }
        return rows;
    }

    // ------------------------------------------------------------------ hits

    private void OnBlade(Vector2 origin, Vector2 dir, float reach, int damage, int swing)
    {
        if (swing != _lastSwing) { _lastSwing = swing; _hitThisSwing.Clear(); }
        bool thrust = Player.Current == Player.ActionKind.Thrust;
        float r = thrust ? reach + 10 : reach + 14;
        foreach (var f in Fighters.Values)
        {
            if (!f.Alive || _hitThisSwing.Contains(f.Id)) continue;
            if (!Player.InBladeArc(origin, dir, r, Player.BladeArcDegrees, f.GlobalPosition, f.Radius)) continue;
            _hitThisSwing.Add(f.Id);
            StrikeFighter(f, origin, damage, false, 0, Player.HeavyCut, melee: true);
            if (Player.Current == Player.ActionKind.Recoil) return; // parried: the recoil replaced the cut
        }
        foreach (var p in _pots.Where(p => IsInstanceValid(p) && !p.IsQueuedForDeletion()).ToList())
            if (Player.InBladeArc(origin, dir, r, Player.BladeArcDegrees, p.GlobalPosition, 6)) BreakPot(p);
    }

    /// <summary>A blade or a wave reached a fighter: parry, flank, hit, kill.</summary>
    public string StrikeFighter(ValeFighter f, Vector2 from, int dmg, bool lamina, float stagger, bool heavy, bool melee)
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
                if (f.Type != ValeFighter.Kind.Statue && _parryCount == 3 && !Story.LearnedSide) { Say(f, "De frente, nada passa pela Guarda Fechada."); Hud.Toast("De frente, essa guarda não cai."); }
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

    private void OnQi(Player.QiShot shot)
    {
        var w = new QiWave { Shot = shot, GlobalPosition = shot.Origin, ZIndex = 10 };
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
                    string r = StrikeFighter(f, w.Foot - w.Shot.Dir * w.Shot.Speed * 0.2f, w.Shot.Damage, w.Shot.Lamina, w.Shot.Stagger, false, melee: false);
                    if (r == "parried" && !w.Shot.Lamina) w.Stop();
                }
            }
            foreach (var p in _pots.Where(p => IsInstanceValid(p) && !p.IsQueuedForDeletion()).ToList())
                if (p.GlobalPosition.DistanceTo(w.Foot) < 16) BreakPot(p);
            if (w.Done) { w.QueueFree(); _waves.Remove(w); }
        }
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
            Story.AddBook(new BookEntry { Name = "Passo da Lâmina Silenciosa", Kind = "combinação", Key = "passo lateral → corte → Qi", Where = $"Descoberta por você: {ValeMap.AreaName(Player.GlobalPosition)}. O Qi segue o corte e quebra a guarda.", Tech = "lamina", State = "Plena" });
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
        if (Story.Once("valeHint")) Hud.Toast("Uma trilha desce para o vale, a leste.");
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
        ["lamina"] = ("A lâmina que segue o passo", "“O qi caminha atrás da lâmina que segue o passo.”"),
    };

    private void TakeQuest(string id)
    {
        if (Story.Quests.ContainsKey(id)) return;
        Story.Quests[id] = false; Hud.Toast($"Nova missão: {Quests[id].Name}");
    }

    private void DoneQuest(string id)
    {
        if (Story.Quests.TryGetValue(id, out bool done) && !done) { Story.Quests[id] = true; Hud.Toast($"Missão cumprida: {Quests[id].Name}"); }
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
                else { Story.SlabRead = true; Panel("A marca na pedra", "Um corte curvo na pedra, como um golpe que contorna algo.", "Aos pés da estátua em guarda, pegadas gravadas no chão", "saem da frente dela e vão para o lado.", "Pise onde ela pisava."); }
                break;
            case "stele":
                Story.SteleRead = true;
                Panel("Inscrição", "“A lâmina que segue o passo não faz ruído.", "O qi caminha atrás dela.”");
                Story.AddBook(new BookEntry { Name = "Inscrição da fenda", Kind = "fragmento", Where = "Além da Fenda: “O qi caminha atrás da lâmina que segue o passo.”" });
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
        Player.Revive(); Player.Qi = Player.MaxQi; // the breath settles at the master's feet
        Story.AddBook(new BookEntry { Name = "Passo Lateral", Kind = "técnica", Key = "L + lado do alvo (com alvo travado)", Where = "Túmulo da Mestra do Vale Nublado: as pegadas ao lado do corte na pedra.", Tech = "side_step", State = "Plena" });
        Story.AddBook(new BookEntry { Name = "Passo longo", Kind = "técnica", Key = "sem trava: direção + L", Where = "Nasce do passo lateral. Atravessa o que o passo curto não alcança.", Tech = "dash", State = "Plena" });
        var shortStep = Story.Book.FirstOrDefault(b => b.Name == "Passo curto"); if (shortStep != null) shortStep.Where = "Sua arte de origem: para frente ou para trás.";
        Tech.Set("side_step", TechniqueState.Full); Tech.Set("dash", TechniqueState.Full);
        Hud.Banner("Passo Lateral", 3.0f);
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
        Player.GlobalPosition = Respawn; Player.Revive(); Player.Qi = Player.MaxQi;
        Lock = null;
        foreach (var w in _waves) w.QueueFree();
        _waves.Clear();
        foreach (var f in Fighters.Values) f.ResetToHome();
        Player.GetNode<Camera2D>("Camera").ResetSmoothing();
        Hud.Toast(Respawn == ValeMap.Start ? "Você desperta no início da trilha. O que aprendeu, ficou." : "Você desperta na entrada do acampamento.");
        if (!Story.HasManual && Story.Once("valeHint")) Hud.Toast("Uma trilha desce para o vale, a leste.");
    }

    // ------------------------------------------------------------------ save

    /// <summary>The journey as it would be written now (region "vale").</summary>
    public SaveData Snapshot(string phase = "vale")
    {
        Story.Seen = new string(_seen.Select(b => b ? '1' : '0').ToArray());
        return new SaveData
        {
            Region = "vale", Phase = phase, Seed = 0,
            Spot = new[] { ValeMap.Meditate.X, ValeMap.Meditate.Y + 10 },
            Vale = Story, ValeInventory = Inventory.Entries.Select(e => new SavedItem { Id = e.Id, Count = e.Count }).ToList(),
            Book = Story.Book, Equipped = "sword", Health = Player.Health, Qi = Player.Qi, Level = 1,
        };
    }

    private void Meditate()
    {
        if (Fighters.Values.Any(f => f.Alive && f.Type != ValeFighter.Kind.Statue && f.State is ValeFighter.St.Guard or ValeFighter.St.Windup or ValeFighter.St.Strike or ValeFighter.St.Advance && f.GlobalPosition.DistanceTo(Player.GlobalPosition) < 260))
        { Hud.Toast("Não dá para meditar com um inimigo em guarda por perto."); return; }
        Player.Qi = Player.MaxQi; _decals.Meditated = 1.6f; Sfx.Play("chime");
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
