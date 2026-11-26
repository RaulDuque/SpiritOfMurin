using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Items;

namespace SpiritOfMurin.Plains;

/// <summary>
/// The Planície's screen layer: Vida (hearts and number), what the hands hold, the interaction prompt,
/// pickup messages with the item's name and quantity, the inventory with crafting, the seed in a
/// corner, the controls line and the wake / faint overlays. Draws with the prototype's colours.
/// </summary>
public partial class PlainsHud : CanvasLayer
{
    /// <summary>One line of the inventory list: an item held or a recipe.</summary>
    public readonly record struct Row(bool IsRecipe, string Id, string Text, string Detail, bool Enabled);

    private static readonly Color Text = new("#ece5d0"), Dim = new("#a39e8c"), OutlineC = new("#16140f"), Life = new("#7a3a32"),
        LifeEmpty = new("#3a2a26"), Panel = new(0.07f, 0.075f, 0.06f, 0.92f), Select = new("#c9a14a"), Disabled = new("#6b675c");

    private Control _root = null!;
    private Label _life = null!, _hands = null!, _seed = null!, _controls = null!, _prompt = null!, _invTitle = null!, _invDetail = null!, _overlayText = null!;
    private VBoxContainer _toasts = null!, _invList = null!;
    private PanelContainer _inv = null!;
    private ColorRect _overlay = null!;
    private Control _hearts = null!; // the Vida bar
    private float _health, _max = 10, _pending, _overlayTime, _overlayHold;
    private readonly List<(Label L, float T)> _toastLabels = new();

    /// <summary>True while the inventory is open.</summary>
    public bool InventoryOpen => _inv.Visible;

    /// <summary>Selected row of the inventory.</summary>
    public int Selected { get; private set; }

    /// <summary>The inventory rows currently shown.</summary>
    public List<Row> Rows { get; } = new();

    /// <summary>The last messages shown (newest last), for checks.</summary>
    public List<string> Messages { get; } = new();

    /// <summary>The prompt text currently shown ("" when none).</summary>
    public string PromptText => _prompt.Visible ? _prompt.Text : "";

    /// <summary>The seed text shown in the corner.</summary>
    public string SeedText => _seed.Text;

    private static Label MakeLabel(int size, Color c)
    {
        var l = new Label();
        l.AddThemeColorOverride("font_color", c);
        l.AddThemeColorOverride("font_outline_color", OutlineC);
        l.AddThemeConstantOverride("outline_size", 3);
        l.AddThemeFontSizeOverride("font_size", size);
        return l;
    }

    /// <inheritdoc />
    public override void _Ready()
    {
        _root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        _root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_root);

        _hearts = new Control { Position = Vector2.Zero, Size = new Vector2(130, 40), MouseFilter = Control.MouseFilterEnum.Ignore };
        _hearts.Draw += DrawLifeBar;
        _root.AddChild(_hearts);
        _life = MakeLabel(9, Text); _life.Position = new Vector2(97, 4); _root.AddChild(_life);
        _hands = MakeLabel(9, Dim); _hands.Position = new Vector2(9, 18); _root.AddChild(_hands);

        _seed = MakeLabel(8, new Color(Dim, 0.75f));
        _seed.HorizontalAlignment = HorizontalAlignment.Right;
        _seed.Position = new Vector2(640 - 156, 3); _seed.Size = new Vector2(150, 12);
        _root.AddChild(_seed);

        _controls = MakeLabel(8, new Color(Text, 0.85f));
        _controls.Position = new Vector2(6, 346);
        _controls.Text = "WASD andar · Shift andar quieto · E pegar · I inventário · J/clique esq. corte · L/clique dir. passo";
        _root.AddChild(_controls);

        _prompt = MakeLabel(8, Text); _prompt.Visible = false; _prompt.HorizontalAlignment = HorizontalAlignment.Center; _prompt.Size = new Vector2(160, 12);
        _root.AddChild(_prompt);

        _toasts = new VBoxContainer { Position = new Vector2(170, 270), Size = new Vector2(300, 70), Alignment = BoxContainer.AlignmentMode.End, MouseFilter = Control.MouseFilterEnum.Ignore };
        _root.AddChild(_toasts);

        _inv = new PanelContainer { Position = new Vector2(150, 40), Size = new Vector2(340, 250), Visible = false };
        _inv.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = Panel, BorderColor = new Color("#4e3824"), BorderWidthBottom = 1, BorderWidthTop = 1, BorderWidthLeft = 1, BorderWidthRight = 1, ContentMarginLeft = 8, ContentMarginRight = 8, ContentMarginTop = 6, ContentMarginBottom = 6 });
        var box = new VBoxContainer();
        _inv.AddChild(box);
        _invTitle = MakeLabel(10, Text); _invTitle.Text = "Inventário"; box.AddChild(_invTitle);
        _invList = new VBoxContainer(); _invList.AddThemeConstantOverride("separation", 0); box.AddChild(_invList);
        _invDetail = MakeLabel(8, Dim); _invDetail.AutowrapMode = TextServer.AutowrapMode.WordSmart; _invDetail.CustomMinimumSize = new Vector2(320, 24); box.AddChild(_invDetail);
        var help = MakeLabel(8, Dim); help.Text = "W/S escolher · E usar, equipar ou fabricar · I fechar"; box.AddChild(help);
        _root.AddChild(_inv);

        _overlay = new ColorRect { Color = new Color(0, 0, 0, 0), MouseFilter = Control.MouseFilterEnum.Ignore };
        _overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _root.AddChild(_overlay);
        _overlayText = MakeLabel(12, Text); _overlayText.HorizontalAlignment = HorizontalAlignment.Center; _overlayText.Position = new Vector2(0, 160); _overlayText.Size = new Vector2(640, 20);
        _overlayText.Modulate = new Color(1, 1, 1, 0);
        _root.AddChild(_overlayText);
    }

    /// <summary>Shows the seed in the corner.</summary>
    public void SetSeed(int seed) => _seed.Text = $"semente {seed}";

    /// <summary>Updates Vida (current, maximum, still regenerating).</summary>
    public void SetHealth(float health, int max, float pending)
    {
        _health = health; _max = max; _pending = pending;
        _life.Text = $"Vida {LifePercent(health, max)}%" + (pending > 0.05f ? " +" : "");
        _hearts.QueueRedraw();
    }

    /// <summary>Shows what the hands hold.</summary>
    public void SetHands(string text) => _hands.Text = "Mãos: " + text;

    /// <summary>Shows (or hides, with null) the interaction prompt over a screen position.</summary>
    public void SetPrompt(string? text, Vector2 screen)
    {
        _prompt.Visible = text != null;
        if (text == null) return;
        _prompt.Text = text;
        _prompt.Position = screen + new Vector2(-80, -40);
    }

    /// <summary>Adds a short message at the bottom of the screen.</summary>
    public void Toast(string text)
    {
        Messages.Add(text);
        if (Messages.Count > 40) Messages.RemoveAt(0);
        var l = MakeLabel(9, Text); l.Text = text; l.HorizontalAlignment = HorizontalAlignment.Center;
        _toasts.AddChild(l);
        _toastLabels.Add((l, 2.8f));
        while (_toastLabels.Count > 4) { _toastLabels[0].L.QueueFree(); _toastLabels.RemoveAt(0); }
    }

    /// <summary>Starts a black overlay with a line of text that fades out after <paramref name="hold"/> seconds.</summary>
    public void Overlay(string text, float hold)
    {
        _overlayText.Text = text; _overlayHold = hold; _overlayTime = 0;
        _overlay.Color = new Color(0, 0, 0, 1); _overlayText.Modulate = Colors.White;
    }

    /// <summary>Opens or closes the inventory.</summary>
    public void ShowInventory(bool open) { _inv.Visible = open; if (open) Selected = Mathf.Clamp(Selected, 0, Mathf.Max(0, Rows.Count - 1)); }

    /// <summary>Moves the selection.</summary>
    public void MoveSelection(int d) { if (Rows.Count == 0) return; Selected = Mathf.PosMod(Selected + d, Rows.Count); RefreshInventory(); }

    /// <summary>Rebuilds the inventory list from <paramref name="rows"/>.</summary>
    public void SetRows(List<Row> rows)
    {
        Rows.Clear(); Rows.AddRange(rows);
        Selected = Mathf.Clamp(Selected, 0, Mathf.Max(0, Rows.Count - 1));
        RefreshInventory();
    }

    private void RefreshInventory()
    {
        foreach (var c in _invList.GetChildren()) c.QueueFree();
        if (Rows.Count == 0) { var e = MakeLabel(9, Dim); e.Text = "(vazio: procure sílex e galhos na margem do riacho)"; _invList.AddChild(e); _invDetail.Text = ""; return; }
        bool recipeHeader = false;
        for (int i = 0; i < Rows.Count; i++)
        {
            var r = Rows[i];
            if (r.IsRecipe && !recipeHeader) { var h = MakeLabel(9, Dim); h.Text = "— Fabricar —"; _invList.AddChild(h); recipeHeader = true; }
            var l = MakeLabel(9, i == Selected ? Select : r.Enabled ? Text : Disabled);
            l.Text = (i == Selected ? "› " : "  ") + r.Text;
            _invList.AddChild(l);
        }
        _invDetail.Text = Rows[Selected].Detail;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        for (int i = _toastLabels.Count - 1; i >= 0; i--)
        {
            var (l, t) = _toastLabels[i];
            t -= dt;
            if (t <= 0) { l.QueueFree(); _toastLabels.RemoveAt(i); continue; }
            l.Modulate = new Color(1, 1, 1, Mathf.Min(1, t / 0.6f));
            _toastLabels[i] = (l, t);
        }
        if (_overlay.Color.A > 0)
        {
            _overlayTime += dt;
            float a = _overlayTime < _overlayHold ? 1 : Mathf.Max(0, 1 - (_overlayTime - _overlayHold) / 1.2f);
            _overlay.Color = new Color(0, 0, 0, a * 0.9f + (a > 0 ? 0.1f * a : 0));
            _overlayText.Modulate = new Color(1, 1, 1, a);
        }
    }

    /// <summary>Vida as a whole percentage of the maximum (rounded up, so any life left shows at least 1%).</summary>
    public static int LifePercent(float health, int max) => max <= 0 ? 0 : Mathf.Clamp(Mathf.CeilToInt(health / max * 100f - 0.001f), 0, 100);

    /// <summary>The Vida text currently shown.</summary>
    public string LifeText => _life.Text;

    private static readonly Color LifeHi = new("#b0574a"), LifeBack = new("#2b1f1a"), RegenC = new("#4c7c4a");

    // a bar in the same style as the prototype's qi bar: ink backdrop, outline, dark trough, fill with a light top line;
    // the Vida still coming from a herb shows as a green segment after the fill
    private void DrawLifeBar()
    {
        const float x = 10, y = 7, w = 82, h = 5;
        _hearts.DrawRect(new Rect2(4, 3, 126, 28), new Color(12 / 255f, 14 / 255f, 11 / 255f, 0.72f));
        _hearts.DrawRect(new Rect2(x - 1, y - 1, w + 2, h + 2), OutlineC);
        _hearts.DrawRect(new Rect2(x, y, w, h), LifeBack);
        float k = _max <= 0 ? 0 : Mathf.Clamp(_health / _max, 0, 1), fill = Mathf.Round(w * k);
        if (_pending > 0.05f)
        {
            float extra = Mathf.Round(w * Mathf.Clamp((_health + _pending) / _max, 0, 1)) - fill;
            if (extra > 0) _hearts.DrawRect(new Rect2(x + fill, y, extra, h), new Color(RegenC, 0.8f));
        }
        if (fill > 0) { _hearts.DrawRect(new Rect2(x, y, fill, h), Life); _hearts.DrawRect(new Rect2(x, y, fill, 1), LifeHi); }
    }
}
