using System.Collections.Generic;
using System.Linq;
using Godot;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Save;
using SpiritOfMurin.Vale;

namespace SpiritOfMurin.Campaign;

/// <summary>
/// The game's main scene (F5): the title menu, then the campaign. Novo jogo starts the prologue in the
/// Vale de Bambus; Continuar resumes the journey where it was written — in the Vale, at the awakening
/// (when the game was closed during the prologue's ending) or in the Planície (also every save from
/// before the campaign). When the prologue ends, the Planície wakes the swordsman with empty hands.
/// Command line (after <c>--</c>): <c>--new</c> / <c>--continue</c> skip the title,
/// <c>--save=user://file.json</c> uses another journey file (test profiles), <c>--seed=N</c> fixes the
/// Planície's seed.
/// </summary>
public partial class CampaignRoot : Node
{
    /// <summary>The journey file.</summary>
    [Export] public string SavePath { get; set; } = SaveGame.DefaultPath;
    /// <summary>Show the opening overlays (tests turn them off).</summary>
    [Export] public bool ShowIntro { get; set; } = true;

    /// <summary>The prologue scene while it runs.</summary>
    public ValeGame? Vale { get; private set; }
    /// <summary>The Planície scene while it runs.</summary>
    public PlainsGame? Plains { get; private set; }
    /// <summary>The title layer.</summary>
    public PlainsHud TitleHud { get; private set; } = null!;
    /// <summary>What the title found.</summary>
    public SaveStatus SaveStatus { get; private set; }
    /// <summary>The journey read at start (null when none).</summary>
    public SaveData? Saved { get; private set; }

    private bool _confirmNew;
    private string _start = "title";

    /// <summary>How to start: "title" (default), "new" or "continue" (tests; the command line overrides).</summary>
    [Export] public string StartWith { get => _start; set => _start = value; }

    /// <inheritdoc />
    public override void _Ready()
    {
        foreach (string a in OS.GetCmdlineUserArgs().Concat(OS.GetCmdlineArgs()))
        {
            if (a == "--new") _start = "new";
            else if (a == "--continue") _start = "continue";
            else if (a.StartsWith("--save=")) SavePath = a["--save=".Length..];
        }
        TitleHud = GetNode<PlainsHud>("TitleHud");
        TitleHud.RowClicked += (panel, i) => { if (panel == "title" && TitleHud.Visible) Choose(TitleHud.TitleRows[i].Id); };
        (SaveStatus, Saved) = SaveGame.Read(SavePath);
        if (_start == "new") NewGame();
        else if (_start == "continue" && Saved != null) Continue();
        else ShowTitle();
    }

    private static string Describe(SaveData d)
    {
        string when = System.DateTime.TryParse(d.SavedAt, out var t) ? t.ToString("dd/MM 'às' HH:mm") : d.SavedAt;
        string where = d.Region == "vale" ? (d.Phase == "senior_won" ? "fim do prólogo" : "Vale de Bambus") : $"Planície · Nv {d.Level}";
        return $"Continuar  ({where} · {when})";
    }

    private void ShowTitle()
    {
        _confirmNew = false;
        var opts = new List<PlainsHud.Row>();
        if (Saved != null) opts.Add(new PlainsHud.Row(false, "continue", Describe(Saved), "", true));
        opts.Add(new PlainsHud.Row(false, "new", "Novo jogo", "", true));
        string msg = SaveStatus == SaveStatus.Damaged ? "A jornada salva estava danificada e não pôde ser carregada. Comece uma nova." : "";
        TitleHud.Visible = true;
        TitleHud.ShowTitle(opts, msg, "SpiritOfMurin");
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent e)
    {
        if (!TitleHud.Visible || !TitleHud.TitleOpen) return;
        if (e.IsActionPressed("move_up")) TitleHud.MoveTitle(-1);
        else if (e.IsActionPressed("move_down")) TitleHud.MoveTitle(1);
        else if (e.IsActionPressed("use") || e.IsActionPressed("ui_accept")) Choose(TitleHud.TitleRows[TitleHud.TitleSelected].Id);
        else if (e.IsActionPressed("ui_cancel") && _confirmNew) ShowTitle();
        else return;
        GetViewport().SetInputAsHandled();
    }

    private void Choose(string id)
    {
        switch (id)
        {
            case "continue": Continue(); break;
            case "new" when Saved != null && !_confirmNew:
                _confirmNew = true;
                TitleHud.ShowTitle(new() { new(false, "keep", "Não, manter a jornada", "", true), new(false, "erase", "Sim, começar de novo", "", true) },
                    "Começar uma nova jornada? A jornada salva será apagada.", "Novo jogo");
                break;
            case "new": NewGame(); break;
            case "keep": ShowTitle(); break;
            case "erase": SaveGame.Erase(SavePath); Saved = null; NewGame(); break;
        }
    }

    /// <summary>Novo jogo: the prologue in the Vale.</summary>
    public void NewGame()
    {
        TitleHud.ShowTitle(null); TitleHud.Visible = false;
        StartVale(null);
    }

    /// <summary>Continuar: the region and phase the journey was written in.</summary>
    public void Continue()
    {
        TitleHud.ShowTitle(null); TitleHud.Visible = false;
        var d = Saved!;
        if (d.Region == "vale" && d.Phase == "senior_won") Awaken(d); // the safe point of the ending
        else if (d.Region == "vale") StartVale(d);
        else StartPlains(d);
    }

    private void StartVale(SaveData? d)
    {
        Vale = GD.Load<PackedScene>("res://src/Vale/Vale.tscn").Instantiate<ValeGame>();
        Vale.SavePath = SavePath; Vale.ShowIntro = ShowIntro;
        AddChild(Vale);
        Vale.PrologueEnded += OnPrologueEnded;
        Vale.Begin(d);
    }

    private void OnPrologueEnded(SaveData vale)
    {
        var v = Vale;
        Vale = null;
        v?.QueueFree();
        Engine.TimeScale = 1;
        Awaken(vale);
    }

    /// <summary>The awakening in the Planície, after the prologue (or resuming a journey closed during its ending).</summary>
    public void Awaken(SaveData vale)
    {
        Plains = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        Plains.SavePath = SavePath; Plains.ShowIntro = ShowIntro;
        Plains.Start = PlainsGame.StartMode.Awaken; Plains.AwakenFrom = vale;
        AddChild(Plains);
    }

    private void StartPlains(SaveData d)
    {
        Plains = GD.Load<PackedScene>("res://src/Plains/Plains.tscn").Instantiate<PlainsGame>();
        Plains.SavePath = SavePath; Plains.ShowIntro = ShowIntro;
        Plains.Start = PlainsGame.StartMode.Continue; Plains.Preloaded = d;
        AddChild(Plains);
    }
}
