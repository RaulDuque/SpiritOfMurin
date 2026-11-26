using System.Collections.Generic;
using System.Linq;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Items;
using SpiritOfMurin.Progression;
using SpiritOfMurin.Save;
using SpiritOfMurin.Audio;
using SpiritOfMurin.Combat;
using SpiritOfMurin.Vale;

namespace SpiritOfMurin.Plains;

/// <summary>
/// The Planície Verdejante (main scene, F5). A title menu offers Continuar (the last meditation) or
/// Novo jogo; a new journey wakes with empty hands at the clearing by the creek. The world is generated
/// by seed in chunks around the character. E picks up, examines tracks, skins and butchers (K skins with
/// Qi), cooks at the campfire and meditates on the mat (which writes the journey). I opens the inventory
/// (use, equip, craft), C the character sheet (level, XP, attributes, proficiencies), Space focuses the
/// nearest animal (name, level, temper), H uses a herb from the Bolsa Simples. In the campaign this
/// awakening follows the prologue's defeat; here it opens directly.
/// Command line (after <c>--</c>): <c>--seed=N</c>, <c>--new</c> / <c>--continue</c> (skip the title),
/// <c>--save=user://file.json</c> (another save file, for tests).
/// </summary>
public partial class PlainsGame : Node2D
{
    /// <summary>How the scene starts.</summary>
    public enum StartMode { Title, New, Continue, Awaken }

    /// <summary>For <see cref="StartMode.Awaken"/>: the prologue's journey the swordsman wakes from (its book; its items are lost).</summary>
    public SaveData? AwakenFrom { get; set; }

    /// <summary>For <see cref="StartMode.Continue"/> from the campaign: the journey already read.</summary>
    public SaveData? Preloaded { get; set; }

    /// <summary>The Livro de Técnicas (from the prologue; the techniques it learned are sealed after the awakening).</summary>
    public List<BookEntry> Book { get; private set; } = new();

    /// <summary>The prologue techniques in force (sealed until the unlock level, then weakened).</summary>
    public TechniqueSet Tech { get; private set; } = null!;

    /// <summary>Sound (optional: the scene may have none).</summary>
    public Sfx? Sfx { get; private set; }

    /// <summary>True when this journey came from the prologue (its equipment was lost once, at the awakening).</summary>
    public bool LossApplied { get; private set; }

    /// <summary>True once the weakened techniques came back.</summary>
    public bool TechniquesReturned { get; private set; }

    /// <summary>World seed for a new journey; 0 = a new random seed each time.</summary>
    [Export] public int Seed { get; set; }
    /// <summary>Rabbit tuning resource.</summary>
    [Export] public RabbitTuning? RabbitTuning { get; set; }
    /// <summary>Boar tuning resource.</summary>
    [Export] public BoarTuning? BoarTuning { get; set; }
    /// <summary>Pixels within which E reaches a thing on the ground.</summary>
    [Export] public float InteractRadius { get; set; } = 26f;
    /// <summary>Pixels within which Space focuses an animal.</summary>
    [Export] public float FocusRadius { get; set; } = 170f;
    /// <summary>Show the waking overlay (tests turn it off).</summary>
    [Export] public bool ShowIntro { get; set; } = true;
    /// <summary>Start with the title menu, or go straight to a new or a continued journey.</summary>
    [Export] public StartMode Start { get; set; } = StartMode.Title;
    /// <summary>The save file.</summary>
    [Export] public string SavePath { get; set; } = SaveGame.DefaultPath;

    /// <summary>The seed in use.</summary>
    public int SeedInUse { get; private set; }
    /// <summary>The generator (null until the journey starts).</summary>
    public PlainsGenerator Gen { get; private set; } = null!;
    /// <summary>The streamed world.</summary>
    public PlainsWorld World { get; private set; } = null!;
    /// <summary>The character.</summary>
    public Player Player { get; private set; } = null!;
    /// <summary>What the character carries.</summary>
    public Inventory Inventory { get; } = new();
    /// <summary>Items and recipes.</summary>
    public ItemCatalog Catalog { get; private set; } = null!;
    /// <summary>Progression numbers.</summary>
    public ProgressionData Data { get; private set; } = null!;
    /// <summary>Level, XP, points, attributes.</summary>
    public PlayerStats Stats { get; private set; } = null!;
    /// <summary>Proficiencies.</summary>
    public Proficiencies Skills { get; private set; } = null!;
    /// <summary>The screen layer.</summary>
    public PlainsHud Hud { get; private set; } = null!;
    /// <summary>Current Qi.</summary>
    public float Qi { get => Player.Qi; private set => Player.Qi = value; }
    /// <summary>True once a journey (new or continued) is running.</summary>
    public bool Started { get; private set; }
    /// <summary>Where the last meditation was (Continuar and fainting wake here).</summary>
    public Vector2 Spot { get; private set; }
    /// <summary>The animal in focus, or null.</summary>
    public IHittable? Focused { get; private set; }

    private readonly HashSet<string> _crafted = new();
    private int _lastSwing = -1;
    private readonly HashSet<string> _hitThisSwing = new();
    private Vector2I _lastChunk = new(int.MinValue, 0);
    private float _faintTimer;
    private SaveData? _saved;
    private SaveStatus _saveStatus;
    private bool _confirmNew;
    private float _riseHintAt, _riseClock;
    private bool _rising;

    /// <summary>True while the swordsman still lies on the ground after the awakening.</summary>
    public bool LyingDown => Player.Lying > 0;

    // ------------------------------------------------------------------ start

    /// <summary>Picks the seed: command line, then the exported value, then a fresh one.</summary>
    public static int ResolveSeed(int exported)
    {
        foreach (string a in Args())
            if (a.StartsWith("--seed=") && int.TryParse(a["--seed=".Length..], out int s) && s != 0) return s;
        if (exported != 0) return exported;
        return (int)(PlainsGenerator.Hash((int)Time.GetTicksUsec(), (int)(Time.GetUnixTimeFromSystem() % 100000), 0, 1) % 999_999u) + 1;
    }

    private static IEnumerable<string> Args() => OS.GetCmdlineUserArgs().Concat(OS.GetCmdlineArgs());

    /// <inheritdoc />
    public override void _Ready()
    {
        foreach (string a in Args())
        {
            if (a == "--new") Start = StartMode.New;
            else if (a == "--continue") Start = StartMode.Continue;
            else if (a.StartsWith("--save=")) SavePath = a["--save=".Length..];
        }
        Catalog = ItemCatalog.Load();
        Data = ProgressionData.Load();
        World = GetNode<PlainsWorld>("World");
        Player = GetNode<Player>("World/Objects/Player");
        Hud = GetNode<PlainsHud>("Hud");
        Stats = new PlayerStats(Data, Player.Tuning.MaxHealth);
        Skills = new Proficiencies(Data);
        Player.Equipped = Player.Weapon.None;
        Player.InputLocked = true;
        Player.Visible = false; // nothing of the world shows behind the title
        Sfx = GetNodeOrNull<Sfx>("Sfx");
        Tech = new TechniqueSet(TechniqueData.Load());
        Player.Tech = Tech;
        Player.LockTarget = () => Focused as Node2D;
        (_saveStatus, _saved) = Preloaded != null ? (SaveStatus.Ok, Preloaded) : SaveGame.Read(SavePath);
        Inventory.Changed += (_, _) => { if (Started) RefreshRows(); };
        if (Start == StartMode.Awaken && AwakenFrom != null) Begin(null, AwakenFrom);
        else if (Start == StartMode.Continue && _saved != null) Begin(_saved);
        else if (Start == StartMode.New || (Start == StartMode.Continue && _saved == null)) Begin(null);
        else ShowTitle();
    }

    private void ShowTitle()
    {
        _confirmNew = false;
        var opts = new List<PlainsHud.Row>();
        string msg = _saveStatus == SaveStatus.Damaged ? "A jornada salva estava danificada e não pôde ser carregada. Comece uma nova." : "";
        if (_saved != null)
        {
            string when = System.DateTime.TryParse(_saved.SavedAt, out var t) ? t.ToString("dd/MM 'às' HH:mm") : _saved.SavedAt;
            opts.Add(new PlainsHud.Row(false, "continue", $"Continuar  (meditou em {when} · Nv {_saved.Level} · semente {_saved.Seed})", "", true));
        }
        opts.Add(new PlainsHud.Row(false, "new", "Novo jogo", "", true));
        Hud.ShowTitle(opts, msg);
    }

    private void TitleChoose()
    {
        var id = Hud.TitleRows[Hud.TitleSelected].Id;
        switch (id)
        {
            case "continue": Hud.ShowTitle(null); Begin(_saved); break;
            case "new" when _saved != null && !_confirmNew:
                _confirmNew = true;
                Hud.ShowTitle(new() { new(false, "keep", "Não, manter a jornada", "", true), new(false, "erase", "Sim, começar de novo", "", true) },
                    "Começar uma nova jornada? A jornada salva será apagada.", "Novo jogo");
                break;
            case "new": Hud.ShowTitle(null); Begin(null); break;
            case "keep": ShowTitle(); break;
            case "erase": SaveGame.Erase(SavePath); _saved = null; Hud.ShowTitle(null); Begin(null); break;
        }
    }

    /// <summary>Starts the journey: a new one (null), the saved one, or the awakening after the prologue (<paramref name="awaken"/>).</summary>
    public void Begin(SaveData? d, SaveData? awaken = null)
    {
        if (Started) return;
        Started = true;
        SeedInUse = d?.Seed ?? ResolveSeed(Seed);
        Gen = new PlainsGenerator(SeedInUse, PlainsGenSettings.Load());
        var state = new WorldState();
        World.Objects = GetNode<Node2D>("World/Objects");
        World.Player = Player;
        World.RabbitTuning = RabbitTuning ?? new RabbitTuning();
        World.BoarTuning = BoarTuning ?? new BoarTuning();
        World.Progression = Data;
        World.Setup(Gen, state);
        World.CreatureSpawned += OnCreatureSpawned;
        World.CreatureDied += OnCreatureDied;
        Player.BladeActive += OnBlade;
        Player.EmptyHanded += () => Hud.Toast("Mãos vazias: fabrique uma ferramenta no inventário (I).");
        Player.OutOfBreath += () => Hud.Toast("Sem fôlego: espere o vigor voltar ou coma carne assada.");
        Player.TechniqueRefused += (_, m) => { Hud.Toast(m); Sfx?.Play("fizzle"); };
        Player.Fainted += () => { _faintTimer = 2.2f; Player.InputLocked = true; Hud.Overlay("Você desmaiou…", 1.4f); };
        Spot = Gen.StartPixel;
        Qi = Stats.MaxQi;
        if (d != null)
        {
            state.Restore(d.Collected, d.Dead.ToDictionary(kv => kv.Key, kv => new Vector2(kv.Value[0], kv.Value[1])), d.PartsTaken, d.Trails);
            Stats.Restore(d.Level, d.Xp, d.Points, d.TotalXp, d.Attributes);
            Skills.Restore(d.Practice, d.PracticeKeys);
            Inventory.Restore(d.Inventory.Select(i => (i.Id, i.Count)));
            foreach (var c in d.Crafted) _crafted.Add(c);
            Spot = new Vector2(d.Spot[0], d.Spot[1]);
            Book = d.Book ?? new();
            LossApplied = d.LossApplied; TechniquesReturned = d.TechniquesReturned;
        }
        if (awaken != null)
        {
            // the prologue's equipment and items were lost in the explosion; the book survives, sealed
            Book = (awaken.Book.Count > 0 ? awaken.Book : awaken.Vale?.Book ?? new()).Select(b => new BookEntry { Name = b.Name, Kind = b.Kind, Key = b.Key, Where = b.Where, Tech = b.Tech, State = b.Tech.Length > 0 ? "Selada" : b.State }).ToList();
            LossApplied = true;
        }
        foreach (var b in Book.Where(b => b.Tech.Length > 0))
            Tech.Set(b.Tech, b.State switch { "Selada" => TechniqueState.Sealed, "Enfraquecida" => TechniqueState.Weakened, _ => TechniqueState.Full });
        ApplyStats(fill: d == null);
        if (d != null)
        {
            Player.Equipped = d.Equipped switch { "knife" when Inventory.Count("faca_pedra") > 0 => Player.Weapon.Knife, _ => Player.Weapon.None };
            Player.Revive();
            Player.Heal(-(Player.MaxHealth - Mathf.Clamp(d.Health, 1, Player.MaxHealth)));
            Player.Vigor = Mathf.Clamp(d.Vigor, 0, Player.MaxVigor);
            Qi = Mathf.Clamp(d.Qi, 0, Stats.MaxQi);
        }
        Player.GlobalPosition = Spot;
        Player.InputLocked = false;
        Player.Visible = true;
        Player.MaxQi = Stats.MaxQi; Player.QiRegen = Data.Base.QiRegen;
        Hud.SetSeed(SeedInUse);
        World.UpdateAround(Player.GlobalPosition);
        _lastChunk = Gen.ChunkOfPixel(Player.GlobalPosition);
        Player.GetNode<Camera2D>("Camera").ResetSmoothing();
        RefreshHands(); RefreshRows(); RefreshSheet();
        if (awaken != null)
        {
            // from darkness into light: a pale glare that slowly clears, and the sounds of nature coming back
            Hud.Overlay("", 0.8f, new Color(1f, 0.97f, 0.9f), 3.2f);
            Sfx?.FadeMaster(0, 0); Sfx?.FadeMaster(1, 3.5f); Sfx?.Ambient(0.45f, 0.6f); Sfx?.Play("breath");
            Player.Lying = 1; _riseHintAt = 2.5f; // he wakes on the ground, where the blast threw him
            Hud.Toast("Você desperta na Planície Verdejante. A espada, a bolsa, tudo se perdeu na explosão.");
            Hud.Toast("Só o Livro de Técnicas ficou: as páginas do Vale estão seladas (B).");
            SaveGame.Write(SavePath, Snapshot()); // the loss and the change of region happen once: from now on Continuar is here
        }
        else
        {
            Sfx?.Ambient(0.35f, 0.5f);
            if (ShowIntro) Hud.Overlay(d == null ? "Você acorda sem nada. Planície Verdejante." : "Você desperta junto à esteira, onde meditou.", 1.6f);
        }
        GD.Print($"Planície: semente {SeedInUse} ({(d == null ? "nova jornada" : "continuação")})");
    }

    /// <summary>Applies the attributes to the body. <paramref name="fill"/> fills Vida and vigor (a new journey).</summary>
    private void ApplyStats(bool fill = false)
    {
        Player.SetMaxHealth(Stats.MaxHealth);
        Player.MoveSpeedMultiplier = Stats.MoveSpeedMultiplier;
        Player.StepDistanceMultiplier = Stats.StepDistanceMultiplier;
        Player.RecoveryMultiplier = Stats.RecoveryMultiplier;
        float oldMax = Player.MaxVigor;
        Player.MaxVigor = Stats.MaxVigor;
        Player.VigorRegen = Stats.VigorRegen;
        Player.VigorRegenDelay = Data.Base.VigorRegenDelay;
        Player.StepVigorCost = Data.Base.StepVigorCost;
        Player.CutVigorCost = Data.Base.CutVigorCost;
        if (fill) Player.Vigor = Player.MaxVigor;
        else if (Player.MaxVigor > oldMax && oldMax > 0) Player.Vigor += Player.MaxVigor - oldMax;
        Player.MaxQi = Stats.MaxQi; Player.QiRegen = Data.Base.QiRegen;
        if (fill) Qi = Stats.MaxQi;
    }

    // ------------------------------------------------------------------ loop and input

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        if (!Started) return;
        float dt = (float)delta;
        var c = Gen.ChunkOfPixel(Player.GlobalPosition);
        if (c != _lastChunk) { _lastChunk = c; World.UpdateAround(Player.GlobalPosition); }
        if (_faintTimer > 0)
        {
            _faintTimer -= dt;
            if (_faintTimer <= 0)
            {
                Player.GlobalPosition = Spot;
                Player.Revive();
                Player.InputLocked = Hud.AnyPanelOpen;
                Hud.Toast("Você volta a si junto à esteira de meditação.");
            }
        }
        if (Player.Lying > 0)
        {
            _riseClock += dt;
            if (_riseHintAt > 0 && _riseClock >= _riseHintAt) { _riseHintAt = 0; Hud.Toast("Você está caído na relva. Mova-se (WASD) para se levantar."); }
            bool wants = Input.GetVector("move_left", "move_right", "move_up", "move_down") != Vector2.Zero || Input.IsActionPressed("use");
            if (!_rising && _riseClock > 1.2f && wants && !Hud.AnyPanelOpen) { _rising = true; Sfx?.Play("breath"); }
            if (_rising) { Player.Lying = Mathf.Max(0, Player.Lying - dt / 0.9f); if (Player.Lying <= 0) { _rising = false; Hud.Toast("Você se levanta. Mãos vazias."); } }
        }
        Hud.SetHealth(Player.Health, Player.MaxHealth, Player.PendingRegen);
        Hud.SetEnergy(Player.Vigor, Player.MaxVigor, Qi, Stats.MaxQi);
        Hud.SetLevel(Stats.Level, Stats.Xp, Stats.XpToNext(Stats.Level), Stats.Points);
        UpdateFocus();
        UpdatePrompt();
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent e)
    {
        if (Hud.TitleOpen)
        {
            if (e.IsActionPressed("move_up")) Hud.MoveTitle(-1);
            else if (e.IsActionPressed("move_down")) Hud.MoveTitle(1);
            else if (e.IsActionPressed("use") || e.IsActionPressed("ui_accept")) TitleChoose();
            else if (e.IsActionPressed("ui_cancel") && _confirmNew) ShowTitle();
            else return;
            GetViewport().SetInputAsHandled();
            return;
        }
        if (!Started || _faintTimer > 0) return;
        if (Hud.BookOpen)
        {
            if (e.IsActionPressed("move_up")) Hud.MoveBook(-1);
            else if (e.IsActionPressed("move_down")) Hud.MoveBook(1);
            else if (e.IsActionPressed("book") || e.IsActionPressed("ui_cancel")) { Hud.ShowBook(null); Player.InputLocked = false; }
            else return;
            Handled(); return;
        }
        if (e.IsActionPressed("inventory") || (Hud.InventoryOpen && e.IsActionPressed("ui_cancel"))) { ToggleInventory(); Handled(); return; }
        if (e.IsActionPressed("sheet") || (Hud.SheetOpen && e.IsActionPressed("ui_cancel"))) { ToggleSheet(); Handled(); return; }
        if (Hud.InventoryOpen)
        {
            if (e.IsActionPressed("move_up")) Hud.MoveSelection(-1);
            else if (e.IsActionPressed("move_down")) Hud.MoveSelection(1);
            else if (e.IsActionPressed("use") || e.IsActionPressed("ui_accept")) ActOnSelection();
            else return;
            Handled(); return;
        }
        if (Hud.SheetOpen)
        {
            if (e.IsActionPressed("move_up")) { Hud.MoveSheet(-1); RefreshSheet(); }
            else if (e.IsActionPressed("move_down")) { Hud.MoveSheet(1); RefreshSheet(); }
            else if (e.IsActionPressed("use") || e.IsActionPressed("ui_accept")) SpendPoint(PlayerStats.AttributeIds[Hud.SheetSelected]);
            else return;
            Handled(); return;
        }
        if (e.IsActionPressed("use")) { TryInteract(false); Handled(); }
        else if (e.IsActionPressed("qi")) { QiKey(); Handled(); }
        else if (e.IsActionPressed("book")) { ToggleBook(); Handled(); }
        else if (e.IsActionPressed("lock")) { ToggleFocus(); Handled(); }
        else if (e.IsActionPressed("heal")) { QuickHeal(); Handled(); }
    }

    private void Handled() => GetViewport().SetInputAsHandled();

    /// <summary>What K will do here: "esfolar" next to a carcass (skin with Qi), otherwise "disparo" (the qi shot).</summary>
    public string QiKeyAction => NearestInteractable() is Carcass ? "esfolar" : "disparo";

    /// <summary>K: skins with Qi next to a carcass, otherwise asks for the qi shot. One press, one action.</summary>
    public void QiKey()
    {
        if (QiKeyAction == "esfolar") TryInteract(true);
        else Player.RequestQi();
    }

    /// <summary>Opens or closes the Livro de Técnicas.</summary>
    public void ToggleBook()
    {
        if (Hud.BookOpen) { Hud.ShowBook(null); Player.InputLocked = false; return; }
        if (Hud.InventoryOpen) Hud.ShowInventory(false);
        if (Hud.SheetOpen) Hud.ShowSheet(false);
        Hud.ShowBook(BookRows()); Player.InputLocked = true;
    }

    /// <summary>The book's rows, with the state of each prologue technique and its command, cost and effect in force.</summary>
    public List<PlainsHud.Row> BookRows()
    {
        var rows = new List<PlainsHud.Row>(); string last = "";
        if (Book.Count == 0) rows.Add(new PlainsHud.Row(false, "", "(vazio)", "Esta jornada começou direto na Planície: o Livro não traz páginas do Vale.", false));
        foreach (var b in Book.OrderBy(b => b.Kind == "técnica" ? 0 : b.Kind == "combinação" ? 1 : 2))
        {
            string group = b.Kind != last ? (b.Kind == "técnica" ? "Técnicas" : b.Kind == "combinação" ? "Combinações" : "Fragmentos") : "";
            last = b.Kind;
            string detail = $"{b.Name} · {b.Kind}\nComando: {(b.Key.Length > 0 ? b.Key : "—")}\n{b.Where}";
            if (b.Tech.Length > 0) detail += "\n" + TechniqueLine(b.Tech);
            rows.Add(new PlainsHud.Row(false, b.Name, b.Name + (b.State.Length > 0 ? $"  [{b.State}]" : ""), detail, b.State != "Selada", group));
        }
        return rows;
    }

    /// <summary>The state, cost and effect in force of a prologue technique (one line for the book).</summary>
    public string TechniqueLine(string id)
    {
        var st = Tech.State(id);
        if (st == TechniqueState.Sealed) return $"Selada: volta enfraquecida no nível {Tech.Data.UnlockLevel}.";
        var p = Tech.Params(id); if (p == null) return "";
        string cost = $"custo: {p.QiCost:0} Qi, {p.VigorCost:0} vigor";
        return id switch
        {
            "qi_wave" => $"{(st == TechniqueState.Weakened ? "Enfraquecida" : "Plena")} · {cost} · alcance {p.Range:0} px · dano {p.Damage}",
            "side_step" => $"{(st == TechniqueState.Weakened ? "Enfraquecida" : "Plena")} · {cost} · contorna {p.Angle:0}° · raio até {p.RadiusMax:0} px",
            "dash" => $"{(st == TechniqueState.Weakened ? "Enfraquecida" : "Plena")} · {cost} · {p.Distance:0} px",
            "thrust" => $"{(st == TechniqueState.Weakened ? "Enfraquecida" : "Plena")} · {cost} · alcance {p.Reach:0} px · dano {p.Damage}",
            "lamina" => $"{(st == TechniqueState.Weakened ? "Enfraquecida" : "Plena")} · dano {p.Damage} · alcance {p.Range:0} px",
            _ => "",
        };
    }

    // ------------------------------------------------------------------ XP and practice

    /// <summary>Gives XP with a reason (shown unless <paramref name="quiet"/>); always announces a level gained.</summary>
    public void GainXp(int amount, string reason, bool quiet = false)
    {
        if (amount <= 0) return;
        int levels = Stats.AddXp(amount);
        if (!quiet) Hud.Toast($"+{amount} XP ({reason})");
        if (levels > 0)
        {
            Hud.Toast($"Nível {Stats.Level}! +{levels * Data.PointsPerLevel} pontos de atributo: abra a ficha (C).");
            RefreshSheet();
        }
    }

    /// <summary>Practice in a skill for something new (a key that never counts twice). Returns true when it counted.</summary>
    public bool Practice(string skill, string key)
    {
        int r = Skills.AddPractice(skill, key);
        if (r < 0) return false;
        var def = Data.Skill(skill);
        string xp = Data.Xp.Practice > 0 ? $", +{Data.Xp.Practice} XP" : "";
        if (r > 0) Hud.Toast($"{def.Name}: {Data.GradeName(r)}{xp}! {Proficiencies.Benefit(skill, r)}");
        else
        {
            int? next = Skills.NextThreshold(skill);
            Hud.Toast($"{def.Name} +1 prática{xp}" + (next is { } n ? $" ({Skills.Practice(skill)}/{n} para {Data.GradeName(Skills.Grade(skill) + 1)})" : ""));
        }
        GainXp(Data.Xp.Practice, "prática", quiet: true);
        RefreshSheet();
        return true;
    }

    private void OnCreatureSpawned(Node n)
    {
        if (n is Boar b)
        {
            b.Deflected += _ => Hud.Toast("O couro grosso desvia o golpe. Espere a investida passar.");
            b.HitPlayer += _ => Hud.Toast("A investida acertou!");
        }
    }

    private void OnCreatureDied(IHittable who, Vector2 at)
    {
        if (Focused == who) Focused = null;
        int xp = HarvestRules.HuntXp(Data, who.Kind, who.Level, Stats.Level);
        string name = Data.Creatures.TryGetValue(who.Kind, out var c) ? c.Name : who.Kind;
        Hud.Toast($"{name} (Nv {who.Level}) abatido. E na carcaça: pele e carne (K esfola com Qi).");
        GainXp(xp, $"caça Nv {who.Level}");
    }

    // ------------------------------------------------------------------ hits and focus

    private void OnBlade(Vector2 origin, Vector2 dir, float reach, int damage, int swing)
    {
        if (swing != _lastSwing) { _lastSwing = swing; _hitThisSwing.Clear(); }
        foreach (var n in GetTree().GetNodesInGroup("creatures"))
        {
            if (n is not IHittable h || !h.CanBeHit || _hitThisSwing.Contains(h.CreatureId)) continue;
            if (!Player.InBladeArc(origin, dir, reach, Player.Tuning.HitArcDegrees, h.GlobalPosition, h.BodyRadius)) continue;
            _hitThisSwing.Add(h.CreatureId);
            h.ReceiveHit(damage);
        }
    }

    /// <summary>Focuses the nearest visible animal within <see cref="FocusRadius"/>, or clears the focus.</summary>
    public void ToggleFocus()
    {
        if (Focused != null) { Focused = null; return; }
        Focused = GetTree().GetNodesInGroup("creatures").OfType<IHittable>().Where(h => h.CanBeHit && IsInstanceValid((Node)h))
            .OrderBy(h => h.GlobalPosition.DistanceTo(Player.GlobalPosition)).FirstOrDefault(h => h.GlobalPosition.DistanceTo(Player.GlobalPosition) < FocusRadius);
        if (Focused == null) Hud.Toast("Nenhum animal por perto para focar.");
    }

    /// <summary>The focus line for an animal: name, level, temper.</summary>
    public string Describe(IHittable h)
    {
        var c = Data.Creatures.TryGetValue(h.Kind, out var d) ? d : new CreatureDef { Name = h.Kind };
        return $"{c.Name} · Nv {h.Level} · {c.Temper}";
    }

    private void UpdateFocus()
    {
        if (Focused != null && (!IsInstanceValid((Node)Focused) || !Focused.CanBeHit || Focused.GlobalPosition.DistanceTo(Player.GlobalPosition) > FocusRadius * 1.4f)) Focused = null;
        Hud.SetFocus(Focused == null ? null : Describe(Focused), Focused == null ? 0 : (float)Focused.HealthLeft / Mathf.Max(1, Focused.HealthMax));
    }

    // ------------------------------------------------------------------ interaction

    /// <summary>The nearest thing E can act on, or null.</summary>
    public Node2D? NearestInteractable()
    {
        Node2D? best = null; float bestD = float.MaxValue;
        foreach (var n in World.Objects.GetChildren())
        {
            if (n is not (Pickup or Carcass or TrackPrint or Campfire or MeditationMat) || n.IsQueuedForDeletion()) continue;
            var nd = (Node2D)n;
            // a print only wins when it is clearly closer (10 px): a carcass, a plant or the fire beside a
            // trail's last prints must not be shadowed by them
            // the campfire and the mat are wider than a pebble, and the fire's base is solid: reach them from a bit farther
            float reach = n is Campfire or MeditationMat ? InteractRadius + 12 : InteractRadius;
            float d = nd.GlobalPosition.DistanceTo(Player.GlobalPosition), score = d + (n is TrackPrint ? 10 : 0);
            if (d <= reach && score <= bestD) { bestD = score; best = nd; }
        }
        return best;
    }

    private bool Blade => Player.Equipped != Player.Weapon.None;

    private static string PartName(string part) => part switch { "hide" => "a pele", "meat" => "a carne", _ => "as presas" };

    private void UpdatePrompt()
    {
        var t = Hud.AnyPanelOpen ? null : NearestInteractable();
        string? text = t switch
        {
            Pickup p => $"E · pegar {Catalog.Get(p.ItemId == "grama_sangue" ? HarvestRules.HerbItem(Data, Skills.Grade("erboristeria")) : p.ItemId).Name}",
            Carcass c => !Blade ? "precisa de uma lâmina na mão" : $"E · tirar {PartName(c.Parts[0])}" + (c.Parts[0] != "meat" ? " · K com Qi" : ""),
            TrackPrint => "E · examinar a pegada",
            Campfire => "E · assar carne",
            MeditationMat => "E · meditar (grava a jornada)",
            _ => null,
        };
        Vector2 screen = t == null ? Vector2.Zero : GetViewport().GetCanvasTransform() * t.GlobalPosition;
        Hud.SetPrompt(text, screen);
    }

    private int AddItem(string id, int n = 1)
    {
        int total = Inventory.Add(id, n);
        Hud.Toast($"+{n} {Catalog.Get(id).Name}   (tem {total})");
        return total;
    }

    /// <summary>Acts on the nearest thing; <paramref name="withQi"/> is K (skin with Qi). Returns true when something happened.</summary>
    public bool TryInteract(bool withQi)
    {
        switch (NearestInteractable())
        {
            case Pickup p: return withQi ? false : TakePickup(p);
            case Carcass c: return TakePart(c, withQi);
            case TrackPrint t when !withQi: return Examine(t);
            case Campfire when !withQi: return Cook();
            case MeditationMat m when !withQi: return Meditate(m);
        }
        if (withQi) Hud.Toast("K: aplique Qi ao esfolar, junto a uma carcaça.");
        return false;
    }

    private bool TakePickup(Pickup p)
    {
        if (!World.State.MarkCollected(p.FeatureId)) return false;
        World.Remove(p);
        bool plant = p.ItemId is "grama_sangue" or "raiz_amarga";
        string item = p.ItemId == "grama_sangue" ? HarvestRules.HerbItem(Data, Skills.Grade("erboristeria")) : p.ItemId;
        AddItem(item);
        string source = plant ? "herb" : p.ItemId == "silex" ? "flint" : "";
        if (source.Length > 0 && Data.Secondary.ContainsKey(source) && HarvestRules.HasSecondary(SeedInUse, p.FeatureId, Stats.SecondaryChance))
        { Hud.Toast("Sorte: havia mais um."); AddItem(item); }
        if (plant)
        {
            Practice("erboristeria", p.FeatureId);
            GainXp(Data.Xp.HerbHarvested, "planta nova");
        }
        return true;
    }

    private bool TakePart(Carcass c, bool withQi)
    {
        if (!Blade) { Hud.Toast("Sem lâmina não dá para aproveitar a caça."); return false; }
        string part = c.Parts[0], kind = c.IsBoar ? "boar" : "rabbit";
        if (!World.State.MarkPartTaken(c.CreatureId, part)) { c.Parts.RemoveAt(0); return false; }
        c.Parts.RemoveAt(0);
        switch (part)
        {
            case "hide":
            {
                int g = Skills.Grade("esfolar");
                if (withQi && g < 1) Hud.Toast("Esfolar Bruto: ainda não sabe guiar o Qi pela lâmina (precisa de Iniciante).");
                else if (withQi && Qi < Data.Base.QiPreserveCost) Hud.Toast($"Qi insuficiente ({Data.Base.QiPreserveCost:0} para preservar).");
                var h = HarvestRules.Skin(Data, kind, g, withQi, Qi);
                Qi -= h.QiSpent;
                if (h.QiSpent > 0) Hud.Toast($"Um fio de Qi guia a lâmina (−{h.QiSpent:0} Qi): o material sai um grau acima.");
                AddItem(h.Item);
                Practice("esfolar", c.CreatureId);
                break;
            }
            case "meat":
            {
                int n = HarvestRules.MeatCount(Data, Skills.Grade("preparo"));
                AddItem(c.IsBoar ? "carne_rija" : "carne_macia", n);
                if (Data.Secondary.TryGetValue(kind, out var sec) && HarvestRules.HasSecondary(SeedInUse, c.CreatureId, Stats.SecondaryChance))
                { Hud.Toast("Sorte: um tendão inteiro."); AddItem(sec); }
                Practice("preparo", c.CreatureId);
                break;
            }
            default:
            {
                int g = Skills.Grade("esfolar");
                if (withQi && g < 3) Hud.Toast("Extração espiritual pede Esfolar Mestre.");
                var (item, spent) = HarvestRules.SpiritualPart(Data, kind, "presa_javali", g, withQi, Qi);
                Qi -= spent;
                AddItem(item);
                break;
            }
        }
        if (c.Parts.Count == 0) World.Remove(c);
        return true;
    }

    private bool Examine(TrackPrint t)
    {
        int g = Skills.Grade("rastreamento");
        bool boar = t.Hoof;
        string dir = Compass(t.Dir);
        var target = World.Objects.GetChildren().OfType<IHittable>().FirstOrDefault(h => h.CreatureId == t.CreatureId);
        Vector2 at = target?.GlobalPosition ?? (World.State.CarcassAt(t.CreatureId) ?? t.GlobalPosition + t.Dir * 200);
        string text = $"{(boar ? "Pegadas de casco, fundas" : "Pegadas pequenas, de coelho")}. Seguem para {dir}.";
        if (g >= 1) text += $" O rastro termina a uns {Mathf.RoundToInt(at.DistanceTo(t.GlobalPosition) / 32f)} passos.";
        if (g >= 2) text += $" Pela marca: {(boar ? "javali" : "coelho")} de Nv {World.LevelOf(boar ? "boar" : "rabbit", t.CreatureId)}.";
        if (World.State.CarcassAt(t.CreatureId) != null) text += " (O animal já foi caçado.)";
        Hud.Toast(text);
        // the next prints shine along the trail (from Iniciante)
        if (g >= 1)
            foreach (var p in World.Objects.GetChildren().OfType<TrackPrint>().Where(p => p.TrailId == t.TrailId && p.Index > t.Index && (g >= 3 || p.Index <= t.Index + 3)))
                p.Glow = g >= 3 ? -1f : 4f; // Mestre: the whole trail ahead keeps shining
        if (World.State.MarkTrailExamined(t.TrailId))
        {
            Practice("rastreamento", t.TrailId);
            GainXp(Data.Xp.TrailExamined, "pista nova");
        }
        return true;
    }

    /// <summary>A direction in words (8 points, screen y down).</summary>
    public static string Compass(Vector2 d)
    {
        string[] names = { "o leste", "o sudeste", "o sul", "o sudoeste", "o oeste", "o noroeste", "o norte", "o nordeste" };
        int i = Mathf.PosMod(Mathf.RoundToInt(d.Angle() / (Mathf.Pi / 4)), 8);
        return names[i];
    }

    private bool NearStation(string station) => station.Length == 0 ||
        (station == "fogueira" && World.Objects.GetChildren().OfType<Campfire>().Any(f => f.GlobalPosition.DistanceTo(Player.GlobalPosition) < 48));

    private bool Cook()
    {
        var r = Catalog.Recipes.FirstOrDefault(x => x.Station == "fogueira" && Inventory.CanCraft(x));
        if (r == null) { Hud.Toast("Nada para assar: traga Carne Macia ou Carne Rija."); return false; }
        return Craft(r.Id);
    }

    private bool Meditate(MeditationMat m)
    {
        var near = GetTree().GetNodesInGroup("creatures").OfType<Boar>().FirstOrDefault(b => b.Brain.State is BoarState.Warn or BoarState.Charge && b.GlobalPosition.DistanceTo(Player.GlobalPosition) < 200);
        if (near != null) { Hud.Toast("Não dá para meditar com um javali investindo."); return false; }
        Spot = m.GlobalPosition + new Vector2(0, 12);
        Qi = Stats.MaxQi;
        Player.RestoreVigor(Player.MaxVigor);
        m.Shine = 1.6f;
        bool ok = SaveGame.Write(SavePath, Snapshot());
        if (ok) { Hud.Toast("Meditação gravada. Qi e vigor cheios."); Hud.Toast("Se cair, volta a esta esteira. Se fechar o jogo, Continuar volta aqui."); }
        else Hud.Toast("A meditação não foi gravada: não foi possível escrever o arquivo. O jogo continua.");
        return ok;
    }

    /// <summary>The journey as it would be written now.</summary>
    public SaveData Snapshot()
    {
        var (collected, dead, parts, trails) = World.State.Snapshot();
        var (practice, keys) = Skills.Snapshot();
        return new SaveData
        {
            Seed = SeedInUse, Spot = new[] { Spot.X, Spot.Y },
            Collected = collected, Dead = dead.ToDictionary(kv => kv.Key, kv => new[] { kv.Value.X, kv.Value.Y }), PartsTaken = parts, Trails = trails,
            Inventory = Inventory.Entries.Select(e => new SavedItem { Id = e.Id, Count = e.Count }).ToList(),
            Equipped = Player.Equipped switch { Player.Weapon.Knife => "knife", Player.Weapon.Sword => "sword", _ => "none" },
            Health = Player.Health, Vigor = Player.Vigor, Qi = Qi,
            Level = Stats.Level, Xp = Stats.Xp, Points = Stats.Points, TotalXp = Stats.TotalXp, Attributes = Stats.Attributes,
            Practice = practice, PracticeKeys = keys, Crafted = _crafted.ToList(),
            Region = "planicie", Phase = "planicie", Book = Book, LossApplied = LossApplied, TechniquesReturned = TechniquesReturned,
        };
    }

    // ------------------------------------------------------------------ inventory

    /// <summary>Opens or closes the inventory (the body stops while it is open).</summary>
    public void ToggleInventory()
    {
        if (Hud.SheetOpen) Hud.ShowSheet(false);
        bool open = !Hud.InventoryOpen;
        RefreshRows();
        Hud.ShowInventory(open);
        Player.InputLocked = open;
    }

    private void RefreshHands() => Hud.SetHands(Player.Equipped switch
    {
        Player.Weapon.Knife => Catalog.Get("faca_pedra").Name,
        Player.Weapon.Sword => "espada",
        _ => "vazias",
    } + (Inventory.Count("bolsa_simples") > 0 ? " · Bolsa Simples (H)" : ""));

    /// <summary>What using an item does now, in words (shown in the inventory).</summary>
    public string Effect(ItemDef d)
    {
        var parts = new List<string>();
        if (d.RegenAmount > 0) parts.Add($"+{d.RegenAmount * (d.Medicine ? Stats.MedicinePotency : 1):0.0} Vida em {d.RegenSeconds:0} s");
        if (d.HealNow > 0) parts.Add($"+{d.HealNow * (d.Medicine ? Stats.MedicinePotency : 1):0.0} Vida na hora");
        if (d.VigorNow > 0) parts.Add($"+{HarvestRules.CookedVigor(Data, d.VigorNow, Skills.Grade("preparo")):0} vigor");
        if (d.Passive.Length > 0) parts.Add(d.Passive);
        return parts.Count == 0 ? "" : "Efeito: " + string.Join(", ", parts) + ".";
    }

    private void RefreshRows()
    {
        var rows = new List<PlainsHud.Row>();
        foreach (var (id, count) in Inventory.Entries)
        {
            var d = Catalog.Get(id);
            string tag = d.Equip == "knife" ? (Player.Equipped == Player.Weapon.Knife ? "  [na mão]" : "  [E: equipar]") : d.Usable ? "  [E: usar]" : "";
            rows.Add(new PlainsHud.Row(false, id, $"{d.Name} ×{count}{tag}", (d.Description + " " + Effect(d)).Trim(), true));
        }
        bool first = true;
        foreach (var r in Catalog.Recipes)
        {
            string inputs = string.Join(" + ", r.Inputs.Select(kv => $"{Catalog.Get(kv.Key).Name} {Inventory.Count(kv.Key)}/{kv.Value}"));
            var o = Catalog.Get(r.Output);
            string where = r.Station == "fogueira" ? " · na fogueira" : "";
            string detail = $"Ingredientes: {string.Join(", ", r.Inputs.Select(kv => $"{kv.Value} {Catalog.Get(kv.Key).Name}"))}{where}. Resultado: {r.Count} {o.Name}. {o.Description} {Effect(o)}".Trim();
            rows.Add(new PlainsHud.Row(true, r.Id, $"{(r.Count > 1 ? r.Count + "× " : "")}{o.Name}  ({inputs}){where}", detail, Inventory.CanCraft(r) && NearStation(r.Station), first ? "Fabricar" : ""));
            first = false;
        }
        Hud.SetRows(rows);
        RefreshHands();
    }

    /// <summary>Uses, equips or crafts the selected inventory row.</summary>
    public void ActOnSelection()
    {
        if (Hud.Rows.Count == 0) return;
        var row = Hud.Rows[Hud.Selected];
        if (row.IsRecipe) Craft(row.Id);
        else UseItem(row.Id);
    }

    /// <summary>Crafts a recipe by id (consumes the inputs once). Returns true when it was made.</summary>
    public bool Craft(string recipeId)
    {
        var r = Catalog.Recipes.FirstOrDefault(x => x.Id == recipeId);
        if (r == null) return false;
        if (!NearStation(r.Station)) { Hud.Toast("Isso se faz na fogueira da clareira."); return false; }
        if (!Inventory.Craft(r))
        {
            var missing = r.Inputs.Where(kv => Inventory.Count(kv.Key) < kv.Value).Select(kv => Catalog.Get(kv.Key).Name);
            Hud.Toast("Falta: " + string.Join(", ", missing));
            return false;
        }
        var o = Catalog.Get(r.Output);
        string used = string.Join(", ", r.Inputs.Select(kv => $"{Catalog.Get(kv.Key).Name} −{kv.Value}"));
        Hud.Toast($"{(r.Station == "fogueira" ? "Assou" : "Fabricou")} {r.Count} {o.Name} ({used}). {Effect(o)}".Trim());
        if (r.Station == "fogueira")
        {
            Practice("preparo", $"cook#{Stats.TotalXp}"); // every piece cooked is new meat; TotalXp grows with each cook
            GainXp(Data.Xp.MeatCooked, "carne assada");
        }
        else if (_crafted.Add(r.Id)) GainXp(Data.Xp.FirstCraft, $"primeira {o.Name}");
        if (r.Output == "bolsa_simples") Hud.Toast("A bolsa vai na cintura: H usa a melhor erva sem abrir o inventário.");
        RefreshRows();
        return true;
    }

    /// <summary>Uses or equips an item by id. Returns true when something happened.</summary>
    public bool UseItem(string id)
    {
        var d = Catalog.Get(id);
        if (Inventory.Count(id) <= 0) return false;
        if (d.Equip == "knife")
        {
            Player.Equipped = Player.Equipped == Player.Weapon.Knife ? Player.Weapon.None : Player.Weapon.Knife;
            Hud.Toast(Player.Equipped == Player.Weapon.Knife ? $"{d.Name} na mão." : "Mãos vazias.");
            RefreshHands(); RefreshRows();
            return true;
        }
        if (!d.Usable) { Hud.Toast($"{d.Name}: {d.Description}"); return false; }
        bool heals = d.RegenAmount > 0 || d.HealNow > 0;
        if (heals && Player.Health + Player.PendingRegen >= Player.MaxHealth) { Hud.Toast("A Vida já está cheia."); return false; }
        if (!heals && d.VigorNow > 0 && Player.Vigor >= Player.MaxVigor - 0.5f) { Hud.Toast("O vigor já está cheio."); return false; }
        Inventory.Remove(id);
        float pot = d.Medicine ? Stats.MedicinePotency : 1f;
        if (d.HealNow > 0) Player.Heal(d.HealNow * pot);
        if (d.RegenAmount > 0) Player.AddRegen(d.RegenAmount * pot, d.RegenSeconds);
        if (d.VigorNow > 0) Player.RestoreVigor(HarvestRules.CookedVigor(Data, d.VigorNow, Skills.Grade("preparo")));
        Hud.Toast($"Usou {d.Name}. {Effect(d)}   (resta {Inventory.Count(id)})");
        return true;
    }

    /// <summary>H: with the Bolsa Simples, uses the best herb without opening the inventory.</summary>
    public bool QuickHeal()
    {
        if (Inventory.Count("bolsa_simples") <= 0) { Hud.Toast("Sem bolsa, as ervas ficam no fundo do inventário (I). Fabrique uma Bolsa Simples."); return false; }
        var best = Inventory.Entries.Select(e => Catalog.Get(e.Id)).Where(d => d.QuickSlot).OrderByDescending(d => d.RegenAmount + d.HealNow).FirstOrDefault();
        if (best == null) { Hud.Toast("A bolsa está sem ervas."); return false; }
        return UseItem(best.Id);
    }

    // ------------------------------------------------------------------ sheet

    /// <summary>Opens or closes the character sheet.</summary>
    public void ToggleSheet()
    {
        if (Hud.InventoryOpen) Hud.ShowInventory(false);
        bool open = !Hud.SheetOpen;
        RefreshSheet();
        Hud.ShowSheet(open);
        Player.InputLocked = open;
    }

    private void RefreshSheet()
    {
        var rows = PlayerStats.AttributeIds.Select(a =>
            new PlainsHud.Row(false, a, $"{PlayerStats.Names[a].Short} {Stats.Get(a),2}   {PlayerStats.Names[a].Long}", "", Stats.Points > 0)).ToList();
        string sel = PlayerStats.AttributeIds[Mathf.Clamp(Hud.SheetSelected, 0, 5)];
        float herb = Catalog.Get(HarvestRules.HerbItem(Data, 1)).RegenAmount;
        var detail = new List<string> { Stats.Points > 0 ? $"Ao gastar 1 ponto em {PlayerStats.Names[sel].Short}:" : $"Sem pontos. Com 1 ponto em {PlayerStats.Names[sel].Short}:" };
        detail.AddRange(Stats.Preview(sel, herb).Select(l => "  " + l));
        detail.Add("");
        detail.Add("Proficiências (sobem pelo uso):");
        foreach (var s in Proficiencies.SkillIds)
        {
            int g = Skills.Grade(s); int? next = Skills.NextThreshold(s);
            string prog = next is { } n ? $"{Skills.Practice(s)}/{n} para {Data.GradeName(g + 1)}" : "grau máximo";
            detail.Add($"  {Data.Skill(s).Name} — {Data.GradeName(g)} ({prog}): {Proficiencies.Benefit(s, g)}");
        }
        string header = $"Vida {Player.MaxHealth} · Vigor {Stats.MaxVigor:0} · Qi {Stats.MaxQi:0} · XP {Stats.Xp}/{Stats.XpToNext(Stats.Level)} (faltam {Stats.XpMissing}) · Pontos: {Stats.Points}";
        Hud.SetSheet($"Ficha — Nível {Stats.Level}", header, rows, string.Join("\n", detail));
    }

    /// <summary>Spends a point on an attribute and applies it. Returns true when spent.</summary>
    public bool SpendPoint(string attr)
    {
        var change = Stats.Preview(attr, Catalog.Get(HarvestRules.HerbItem(Data, 1)).RegenAmount);
        if (!Stats.Spend(attr)) { Hud.Toast("Sem pontos para gastar: suba de nível."); return false; }
        ApplyStats();
        Hud.Toast($"{PlayerStats.Names[attr].Short} {Stats.Get(attr)} — {string.Join("; ", change)}");
        RefreshSheet(); RefreshRows();
        return true;
    }
}
