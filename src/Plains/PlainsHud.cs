using System;
using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Game;

namespace SpiritOfMurin.Plains;

/// <summary>
/// The Planície's screen layer, drawn with the prototype's colours:
/// <list type="bullet">
/// <item>the bars in the corner: Vida with its percentage and the pending herb heal in green, vigor and Qi,
/// in the style of the prototype's qi bar; the level and the XP missing; what the hands hold;</item>
/// <item>the focused creature's name, level, temper and health at the top;</item>
/// <item>the interaction prompt, pickup messages, the seed, the controls line;</item>
/// <item>three panels that share one look: the inventory (I), the character sheet (C) and the title menu;</item>
/// <item>the wake / faint overlay.</item>
/// </list>
/// </summary>
public partial class PlainsHud : CanvasLayer
{
    /// <summary>One selectable line of a panel. <paramref name="Group"/> starts a titled block before it.</summary>
    public readonly record struct Row(bool IsRecipe, string Id, string Text, string Detail, bool Enabled, string Group = "");

    private static readonly Color Text = new("#ece5d0"), Dim = new("#a39e8c"), OutlineC = new("#16140f"), Life = new("#7a3a32"), LifeHi = new("#b0574a"),
        LifeBack = new("#2b1f1a"), RegenC = new("#4c7c4a"), Panel = new(0.07f, 0.075f, 0.06f, 0.94f), Select = new("#c9a14a"), Disabled = new("#6b675c"),
        Vig = new("#b8923a"), VigHi = new("#e6c878"), VigBack = new("#2a2418"), QiC = new("#3fae9e"), QiHi = new("#a6f2e2"), QiBack = new("#1f2a28"),
        Threat = new("#b0403a");

    /// <summary>A panel with a title, a list of rows, a detail block and a help line.</summary>
    private sealed class Menu
    {
        public PanelContainer Box = null!;
        public Label Title = null!, Header = null!, Detail = null!, Help = null!;
        public VBoxContainer List = null!;
        public readonly List<Row> Rows = new();
        public int Selected;
        public string Empty = "";
    }

    private Control _root = null!, _bars = null!, _focusBar = null!, _footer = null!;
    private Label _life = null!, _vigor = null!, _qi = null!, _level = null!, _hands = null!, _seed = null!, _controls = null!, _prompt = null!, _overlayText = null!, _focus = null!;
    private VBoxContainer _toasts = null!;
    private Menu _inv = null!, _sheet = null!, _title = null!, _story = null!, _pause = null!;
    private Vector2 _laidOut;

    /// <summary>The world map panel.</summary>
    public MapView Map { get; private set; } = null!;
    /// <summary>The corner minimap (shown while exploring, hidden behind panels).</summary>
    public MapView MiniMap { get; private set; } = null!;
    /// <summary>The Controles screen (Esc → Controles).</summary>
    public ControlsPanel Controls { get; private set; } = null!;
    /// <summary>The Livro de Técnicas panel.</summary>
    public BookPanel Book { get; private set; } = null!;
    /// <summary>The HUD's root control (panels added by the game hang from it).</summary>
    public Control Root => _root;
    private string _controlsTemplate = "", _valeTemplate = "", _qiHint = "[qi] Qi";
    /// <summary>True while the map is open.</summary>
    public bool MapOpen => Map.Visible;
    /// <summary>True while the pause menu is open.</summary>
    public bool PauseOpen => _pause.Box.Visible;
    /// <summary>The pause menu's rows.</summary>
    public List<Row> PauseRows => _pause.Rows;
    /// <summary>Selected pause row.</summary>
    public int PauseSelected => _pause.Selected;

    /// <summary>A row was clicked with the mouse: the panel ("inv", "sheet", "title", "book", "pause") and the row index.</summary>
    public event Action<string, int>? RowClicked;

    /// <summary>The screen rectangle (in window coordinates) of a row of a panel, for mouse checks; null when not shown.</summary>
    public Rect2? RowScreenRect(string panel, int index)
    {
        var m = MenuOf(panel);
        if (m == null || !m.Box.Visible) return null;
        foreach (var c in m.List.GetChildren())
            if (c is Label l && l.HasMeta("row") && (int)l.GetMeta("row") == index && !l.IsQueuedForDeletion())
            {
                var ft = GetViewport().GetFinalTransform();
                Vector2 a = ft * l.GetGlobalTransformWithCanvas().Origin, b = ft * (l.GetGlobalTransformWithCanvas().Origin + l.Size);
                return new Rect2(a, b - a); // window coordinates: what a mouse event carries
            }
        return null;
    }

    private Menu? MenuOf(string name) => name switch { "inv" => _inv, "sheet" => _sheet, "title" => _title, "pause" => _pause, "story" => _story, _ => null };
    private string NameOf(Menu m) => m == _inv ? "inv" : m == _sheet ? "sheet" : m == _title ? "title" : m == _pause ? "pause" : "story";
    private Label _banner = null!, _speech = null!;
    private float _bannerT, _speechT;
    private bool _valeMode;
    private ColorRect _overlay = null!;
    private float _health, _max = 10, _pending, _vig, _maxVig, _qiNow, _maxQi, _focusFrac, _overlayTime, _overlayHold;
    private readonly List<(Label L, float T)> _toastLabels = new();

    /// <summary>True while the inventory is open.</summary>
    public bool InventoryOpen => _inv.Box.Visible;
    /// <summary>True while the character sheet is open.</summary>
    public bool SheetOpen => _sheet.Box.Visible;
    /// <summary>True while the title menu is shown.</summary>
    public bool TitleOpen => _title.Box.Visible;
    /// <summary>True while any panel is open.</summary>
    public bool AnyPanelOpen => InventoryOpen || SheetOpen || TitleOpen || StoryOpen || BookOpen || PauseOpen || MapOpen || ControlsOpen;
    /// <summary>True while the Controles screen is open.</summary>
    public bool ControlsOpen => Controls.Visible;

    /// <summary>True while a story panel (a page, a seal, an inscription) is shown.</summary>
    public bool StoryOpen => _story.Box.Visible;
    /// <summary>True while the Livro de Técnicas is open.</summary>
    public bool BookOpen => Book.Visible;
    /// <summary>The story panel's title and text.</summary>
    public string StoryText => _story.Title.Text + "\n" + _story.Detail.Text;
    /// <summary>The book's pages.</summary>
    public List<BookPage> BookRows => Book.Pages;
    /// <summary>Selected book page.</summary>
    public int BookSelected => Book.Selected;
    /// <summary>The book's selected page as text (name, state, command, numbers, origin).</summary>
    public string BookDetail => Book.PageText;
    /// <summary>The area banner text ("" when none).</summary>
    public string BannerText => _banner.Visible ? _banner.Text : "";
    /// <summary>The speech bubble text ("" when none).</summary>
    public string SpeechText => _speech.Visible ? _speech.Text : "";

    /// <summary>Selected row of the inventory.</summary>
    public int Selected => _inv.Selected;
    /// <summary>The inventory rows currently shown.</summary>
    public List<Row> Rows => _inv.Rows;
    /// <summary>Selected row of the sheet.</summary>
    public int SheetSelected => _sheet.Selected;
    /// <summary>The sheet rows (the six attributes).</summary>
    public List<Row> SheetRows => _sheet.Rows;
    /// <summary>The sheet's detail text (the selected attribute's preview and the proficiencies).</summary>
    public string SheetDetail => _sheet.Detail.Text;
    /// <summary>The sheet's header (level, XP, points).</summary>
    public string SheetHeader => _sheet.Header.Text;
    /// <summary>Selected option of the title menu.</summary>
    public int TitleSelected => _title.Selected;
    /// <summary>The title options.</summary>
    public List<Row> TitleRows => _title.Rows;
    /// <summary>The title menu's message line.</summary>
    public string TitleMessage => _title.Header.Text;

    /// <summary>The last messages shown (newest last), for checks.</summary>
    public List<string> Messages { get; } = new();
    /// <summary>The prompt text currently shown ("" when none).</summary>
    public string PromptText => _prompt.Visible ? _prompt.Text : "";
    /// <summary>The seed text shown in the corner.</summary>
    public string SeedText => _seed.Text;
    /// <summary>The Vida text currently shown.</summary>
    public string LifeText => _life.Text;
    /// <summary>The level / XP line.</summary>
    public string LevelText => _level.Text;
    /// <summary>The focused creature's line ("" when none).</summary>
    public string FocusText => _focus.Visible ? _focus.Text : "";

    private static Label MakeLabel(int size, Color c)
    {
        var l = new Label();
        l.AddThemeColorOverride("font_color", c);
        l.AddThemeColorOverride("font_outline_color", OutlineC);
        l.AddThemeConstantOverride("outline_size", 3);
        l.AddThemeFontSizeOverride("font_size", size);
        return l;
    }

    private Label Add(Label l, Vector2 pos) { l.Position = pos; _root.AddChild(l); return l; }

    /// <summary>The screen size the layout was made for (the viewport grows with the window: "expand").</summary>
    public Vector2 LaidOut => _laidOut;

    // puts every element where it belongs for the real screen size (the base is 640 × 360; a bigger window shows more)
    private void Layout(Vector2 sz)
    {
        _laidOut = sz;
        float W = sz.X, H = sz.Y;
        _focus.Position = new Vector2(W / 2 - 150, 3); _focusBar.Position = new Vector2(W / 2 - 61, 17);
        _seed.Position = new Vector2(W - 156, 3);
        // the minimap sits under the seed, in the top-right corner, a little bigger on a bigger screen
        var mm = new Vector2(Mathf.Round(Mathf.Clamp(W * 0.18f, 104, 170)), Mathf.Round(Mathf.Clamp(H * 0.2f, 66, 110)));
        MiniMap.Size = mm; MiniMap.Position = new Vector2(W - mm.X - 4, 15);
        // the footer: a dark strip across the bottom so the shortcuts read over any ground
        _footer.Position = new Vector2(0, H - 15); _footer.Size = new Vector2(W, 15);
        _controls.Position = new Vector2(6, H - 14); _controls.Size = new Vector2(W - 12, 12);
        _toasts.Position = new Vector2(W / 2 - 200, H - 98);
        _banner.Position = new Vector2(0, 52); _banner.Size = new Vector2(W, 20);
        _overlayText.Position = new Vector2(0, H / 2 - 20); _overlayText.Size = new Vector2(W, 20);
        foreach (var m in new[] { _inv, _sheet, _title, _story, _pause })
            m.Box.Position = new Vector2(Mathf.Round((W - m.Box.Size.X) / 2), Mathf.Round(Mathf.Max(18, (H - m.Box.Size.Y) / 2 - 6)));
        Map.Size = new Vector2(Mathf.Min(W - 40, 900), H - 46); Map.Position = new Vector2(Mathf.Round((W - Map.Size.X) / 2), 22);
        Book.Size = new Vector2(Mathf.Min(W - 40, 820), H - 46); Book.Position = new Vector2(Mathf.Round((W - Book.Size.X) / 2), 22);
        Controls.Size = new Vector2(Mathf.Min(W - 24, 560), H - 34); Controls.Position = new Vector2(Mathf.Round((W - Controls.Size.X) / 2), 14);
    }

    private Menu MakeMenu(string title, string help, Vector2 pos, Vector2 size)
    {
        var m = new Menu();
        m.Box = new PanelContainer { Position = pos, Size = size, Visible = false };
        m.Box.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = Panel, BorderColor = new Color("#4e3824"), BorderWidthBottom = 1, BorderWidthTop = 1, BorderWidthLeft = 1, BorderWidthRight = 1, ContentMarginLeft = 8, ContentMarginRight = 8, ContentMarginTop = 6, ContentMarginBottom = 6 });
        var box = new VBoxContainer(); box.AddThemeConstantOverride("separation", 2);
        m.Box.AddChild(box);
        m.Title = MakeLabel(10, Text); m.Title.Text = title; box.AddChild(m.Title);
        m.Header = MakeLabel(8, Dim); m.Header.AutowrapMode = TextServer.AutowrapMode.WordSmart; m.Header.CustomMinimumSize = new Vector2(size.X - 16, 0); box.AddChild(m.Header);
        m.List = new VBoxContainer(); m.List.AddThemeConstantOverride("separation", 0); box.AddChild(m.List);
        m.Detail = MakeLabel(8, Dim); m.Detail.AutowrapMode = TextServer.AutowrapMode.WordSmart; m.Detail.CustomMinimumSize = new Vector2(size.X - 16, 20); box.AddChild(m.Detail);
        m.Help = MakeLabel(8, Dim); m.Help.Text = help; box.AddChild(m.Help);
        _root.AddChild(m.Box);
        return m;
    }

    /// <inheritdoc />
    public override void _Ready()
    {
        _root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        _root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_root);

        _bars = new Control { Size = new Vector2(220, 56), MouseFilter = Control.MouseFilterEnum.Ignore };
        _bars.Draw += DrawBars;
        _root.AddChild(_bars);
        _life = Add(MakeLabel(8, Text), new Vector2(97, 0));
        _vigor = Add(MakeLabel(8, Dim), new Vector2(97, 10));
        _qi = Add(MakeLabel(8, Dim), new Vector2(97, 20));
        _level = Add(MakeLabel(8, Text), new Vector2(9, 31));
        _hands = Add(MakeLabel(8, Dim), new Vector2(9, 41));

        _focus = Add(MakeLabel(9, Text), new Vector2(170, 3)); _focus.Size = new Vector2(300, 12); _focus.HorizontalAlignment = HorizontalAlignment.Center; _focus.Visible = false;
        _focusBar = new Control { Position = new Vector2(259, 17), Size = new Vector2(122, 6), Visible = false, MouseFilter = Control.MouseFilterEnum.Ignore };
        _focusBar.Draw += () =>
        {
            _focusBar.DrawRect(new Rect2(0, 0, 122, 6), OutlineC); _focusBar.DrawRect(new Rect2(1, 1, 120, 4), LifeBack);
            _focusBar.DrawRect(new Rect2(1, 1, Mathf.Round(120 * _focusFrac), 4), Threat);
        };
        _root.AddChild(_focusBar);

        _seed = Add(MakeLabel(8, new Color(Dim, 0.75f)), new Vector2(640 - 156, 3));
        _seed.HorizontalAlignment = HorizontalAlignment.Right; _seed.Size = new Vector2(150, 12);

        _footer = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        _footer.Draw += () => { _footer.DrawRect(new Rect2(Vector2.Zero, _footer.Size), new Color(0.05f, 0.055f, 0.045f, 0.82f)); _footer.DrawRect(new Rect2(0, 0, _footer.Size.X, 1), new Color("#4e3824")); };
        _root.AddChild(_footer);
        _controls = Add(MakeLabel(8, Text), new Vector2(6, 346));
        _controls.ClipText = true;
        _controlsTemplate = "[walk] andar · [sneak] quieto · [use] pegar/examinar · [inventory] inventário · [sheet] ficha · [book] livro · [map] mapa · [lock] focar · {qi} · [heal] erva · [cut+] corte · [step+] passo · [menu] pausa";
        _valeTemplate = "[walk] andar · [lock] travar · [cut+] corte · [step+] passo · [qi] Qi · [use] examinar · [heal] bálsamo · [inventory] inventário · [book] livro · [map] mapa · [menu] pausa";
        _controls.Text = ControlsLine();
        MiniMap = new MapView { Compact = true, Visible = false, MouseFilter = Control.MouseFilterEnum.Ignore, Name = "MiniMap", TextureFilter = CanvasItem.TextureFilterEnum.Nearest };
        _root.AddChild(MiniMap);
        Bindings.Changed += RefreshTexts;
        if (DisplayManager.Instance != null) DisplayManager.Instance.Notice += Toast;
        if (DisplayManager.Embedded && !DisplayManager.EmbedNoticeShown) { DisplayManager.EmbedNoticeShown = true; Callable.From(() => Toast(DisplayManager.EmbedNotice)).CallDeferred(); }

        _prompt = Add(MakeLabel(8, Text), Vector2.Zero); _prompt.Visible = false; _prompt.HorizontalAlignment = HorizontalAlignment.Center; _prompt.Size = new Vector2(200, 12);

        _toasts = new VBoxContainer { Position = new Vector2(120, 262), Size = new Vector2(400, 78), Alignment = BoxContainer.AlignmentMode.End, MouseFilter = Control.MouseFilterEnum.Ignore };
        _root.AddChild(_toasts);

        _inv = MakeMenu("Inventário", "W/S escolher · E usar, equipar ou fabricar · I fechar", new Vector2(130, 30), new Vector2(380, 290));
        _inv.Empty = "(vazio: procure sílex e galhos na margem do riacho)";
        _sheet = MakeMenu("Ficha", "W/S escolher atributo · E gastar 1 ponto · C fechar", new Vector2(110, 22), new Vector2(420, 310));
        _title = MakeMenu("SpiritOfMurin — Planície Verdejante", "W/S escolher · E ou Enter confirmar", new Vector2(170, 110), new Vector2(300, 130));
        _story = MakeMenu("", "E, Enter ou Esc: continuar", new Vector2(130, 90), new Vector2(380, 150));
        _story.Title.AddThemeColorOverride("font_color", new Color("#d4fbf6"));
        Book = new BookPanel { Visible = false, Name = "Book" };
        Book.Clicked += i => RowClicked?.Invoke("book", i);
        _root.AddChild(Book);
        _pause = MakeMenu("Pausa", "W/S ou mouse escolher · E, Enter ou clique confirmar · Esc voltar", new Vector2(200, 90), new Vector2(240, 160));
        Map = new MapView { Visible = false, Position = new Vector2(20, 20), Size = new Vector2(600, 320), MouseFilter = Control.MouseFilterEnum.Stop, TextureFilter = CanvasItem.TextureFilterEnum.Nearest };
        _root.AddChild(Map);
        Controls = new ControlsPanel { Visible = false, Name = "Controles" };
        _root.AddChild(Controls);
        _banner = Add(MakeLabel(12, Text), new Vector2(0, 52)); _banner.HorizontalAlignment = HorizontalAlignment.Center; _banner.Size = new Vector2(640, 20); _banner.Visible = false;
        _speech = Add(MakeLabel(9, new Color("#ece5d0")), Vector2.Zero); _speech.HorizontalAlignment = HorizontalAlignment.Center; _speech.Size = new Vector2(300, 12); _speech.Visible = false;
        _speech.AutowrapMode = TextServer.AutowrapMode.WordSmart; _speech.CustomMinimumSize = new Vector2(300, 0);

        _overlay = new ColorRect { Color = new Color(0, 0, 0, 0), MouseFilter = Control.MouseFilterEnum.Ignore };
        _overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _root.AddChild(_overlay);
        _overlayText = Add(MakeLabel(12, Text), new Vector2(0, 160)); _overlayText.HorizontalAlignment = HorizontalAlignment.Center; _overlayText.Size = new Vector2(640, 20);
        _overlayText.Modulate = new Color(1, 1, 1, 0);
        // the panels sit above the world's words (speech, area banner, messages) and under the fade overlay
        foreach (Control c in new Control[] { _inv.Box, _sheet.Box, _story.Box, _pause.Box, Map, Book, Controls })
            _root.MoveChild(c, _overlay.GetIndex());
        _root.MoveChild(_title.Box, -1);
        RefreshTexts();
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        // the controls and the display outlive the scene: let go of them
        Bindings.Changed -= RefreshTexts;
        if (DisplayManager.Instance != null) DisplayManager.Instance.Notice -= Toast;
    }

    // ------------------------------------------------------------------ corner

    /// <summary>Shows the seed in the corner.</summary>
    public void SetSeed(int seed) => _seed.Text = $"semente {seed}";

    /// <summary>Vida as a whole percentage of the maximum (rounded up, so any life left shows at least 1%).</summary>
    public static int LifePercent(float health, int max) => max <= 0 ? 0 : Mathf.Clamp(Mathf.CeilToInt(health / max * 100f - 0.001f), 0, 100);

    /// <summary>Updates Vida (current, maximum, still regenerating).</summary>
    public void SetHealth(float health, int max, float pending)
    {
        _health = health; _max = max; _pending = pending;
        _life.Text = $"Vida {LifePercent(health, max)}%" + (pending > 0.05f ? " +" : "");
        _bars.QueueRedraw();
    }

    /// <summary>Updates vigor and Qi.</summary>
    public void SetEnergy(float vigor, float maxVigor, float qi, float maxQi)
    {
        _vig = vigor; _maxVig = maxVigor; _qiNow = qi; _maxQi = maxQi;
        _vigor.Text = maxVigor > 0 ? $"Vigor {Mathf.FloorToInt(vigor)}/{maxVigor:0}" : "";
        _qi.Text = $"Qi {Mathf.FloorToInt(qi)}/{maxQi:0}";
        _bars.QueueRedraw();
    }

    /// <summary>Updates the level line.</summary>
    /// <summary>What K does here, in the controls line (it changes next to a carcass).</summary>
    public void SetQiKeyHint(string hint)
    {
        _qiHint = hint;
        if (_valeMode) return;
        string t = ControlsLine();
        if (_controls.Text != t) _controls.Text = t;
    }

    // the footer's shortcuts, written with the current controls
    private string ControlsLine() => Bindings.F(_valeMode ? _valeTemplate : _controlsTemplate.Replace("{qi}", _qiHint));

    // a control changed: every line that names a key is written again
    private void RefreshTexts()
    {
        _controls.Text = ControlsLine();
        _inv.Help.Text = Bindings.F("[move_up]/[move_down] escolher · [use] ou clique: usar, equipar, fabricar · [inventory] fechar");
        _sheet.Help.Text = Bindings.F("[move_up]/[move_down] escolher atributo · [use] ou clique: gastar 1 ponto · [sheet] fechar");
        _title.Help.Text = Bindings.F("[move_up]/[move_down] escolher · [use] ou Enter confirmar");
        _story.Help.Text = Bindings.F("[use], Enter ou Esc: continuar");
        _pause.Help.Text = Bindings.F("[move_up]/[move_down] ou mouse escolher · [use], Enter ou clique confirmar · Esc voltar");
    }

    /// <summary>The controls line now.</summary>
    public string ControlsText => _controls.Text;

    /// <summary>The prologue's corner: no vigor bar, no level line (the Vale has neither).</summary>
    public void SetValeMode(bool vale) { _valeMode = vale; _level.Visible = !vale; _controls.Text = ControlsLine(); }

    /// <summary>Shows a story panel: a title and lines (a page of the manual, the seal, the inscription). Null hides it.</summary>
    public void ShowStory(string? title, IEnumerable<string>? lines = null)
    {
        _story.Box.Visible = title != null;
        if (title == null) return;
        _story.Title.Text = title; _story.Rows.Clear(); Refresh(_story);
        _story.Detail.Text = lines == null ? "" : string.Join("\n", lines);
    }

    /// <summary>Opens (with rows) or closes (null) the Livro de Técnicas.</summary>
    public void ShowBook(List<BookPage>? pages)
    {
        Book.Visible = pages != null;
        if (pages != null) Book.SetPages(pages);
    }

    /// <summary>Moves the book selection.</summary>
    public void MoveBook(int d) => Book.Move(d);

    /// <summary>Shows the area banner for a while.</summary>
    public void Banner(string text, float seconds = 2.4f) { _banner.Text = text; _banner.Visible = true; _bannerT = seconds; }

    /// <summary>Shows a speech line over a screen point for a while (null hides it).</summary>
    public void Speech(string? text, Vector2 screen, float seconds = 3.4f)
    {
        _speech.Visible = text != null;
        if (text == null) return;
        _speech.Text = text; _speechT = seconds;
        _speech.Position = new Vector2(Mathf.Clamp(screen.X - 150, 4, Mathf.Max(4, _laidOut.X - 304)), Mathf.Max(30, screen.Y - 74));
    }

    /// <summary>Moves the speech bubble over a screen point (it follows the speaker).</summary>
    public void SpeechAt(Vector2 screen) { if (_speech.Visible) _speech.Position = new Vector2(Mathf.Clamp(screen.X - 150, 4, Mathf.Max(4, _laidOut.X - 304)), Mathf.Max(30, screen.Y - 74)); }

    public void SetLevel(int level, int xp, int need, int points) =>
        _level.Text = $"Nv {level} · XP {xp}/{need} (faltam {need - xp})" + (points > 0 ? Bindings.F($" · {points} ponto{(points > 1 ? "s" : "")} ([sheet])") : "");

    /// <summary>Shows what the hands hold.</summary>
    public void SetHands(string text) => _hands.Text = "Mãos: " + text;

    /// <summary>Shows (null hides) the focused creature's line and health.</summary>
    public void SetFocus(string? text, float healthFraction)
    {
        _focus.Visible = _focusBar.Visible = text != null;
        if (text == null) return;
        _focus.Text = text; _focusFrac = Mathf.Clamp(healthFraction, 0, 1); _focusBar.QueueRedraw();
    }

    private void Bar(float x, float y, float w, float h, float frac, Color back, Color fill, Color hi)
    {
        _bars.DrawRect(new Rect2(x - 1, y - 1, w + 2, h + 2), OutlineC);
        _bars.DrawRect(new Rect2(x, y, w, h), back);
        float f = Mathf.Round(w * Mathf.Clamp(frac, 0, 1));
        if (f > 0) { _bars.DrawRect(new Rect2(x, y, f, h), fill); _bars.DrawRect(new Rect2(x, y, f, 1), hi); }
    }

    // bars in the style of the prototype's qi bar: ink backdrop, outline, dark trough, fill with a light top line;
    // the Vida still coming from a herb shows as a green segment after the fill
    private void DrawBars()
    {
        const float x = 10, w = 82;
        _bars.DrawRect(new Rect2(4, 2, 210, 52), new Color(12 / 255f, 14 / 255f, 11 / 255f, 0.72f));
        float k = _max <= 0 ? 0 : Mathf.Clamp(_health / _max, 0, 1);
        Bar(x, 5, w, 5, k, LifeBack, Life, LifeHi);
        if (_pending > 0.05f && _max > 0)
        {
            float fill = Mathf.Round(w * k), extra = Mathf.Round(w * Mathf.Clamp((_health + _pending) / _max, 0, 1)) - fill;
            if (extra > 0) _bars.DrawRect(new Rect2(x + fill, 5, extra, 5), new Color(RegenC, 0.8f));
        }
        if (_maxVig > 0) Bar(x, 15, w, 3, _vig / _maxVig, VigBack, Vig, VigHi);
        Bar(x, 24, w, 4, _maxQi <= 0 ? 0 : _qiNow / _maxQi, QiBack, QiC, QiHi);
    }

    // ------------------------------------------------------------------ messages

    /// <summary>Shows (or hides, with null) the interaction prompt over a screen position.</summary>
    public void SetPrompt(string? text, Vector2 screen)
    {
        _prompt.Visible = text != null;
        if (text == null) return;
        _prompt.Text = text;
        _prompt.Position = screen + new Vector2(-100, -40);
    }

    /// <summary>Adds a short message at the bottom of the screen.</summary>
    public void Toast(string text)
    {
        Messages.Add(text);
        if (Messages.Count > 500) Messages.RemoveAt(0);
        var l = MakeLabel(9, Text); l.Text = text; l.HorizontalAlignment = HorizontalAlignment.Center;
        l.AutowrapMode = TextServer.AutowrapMode.WordSmart; l.CustomMinimumSize = new Vector2(400, 0);
        _toasts.AddChild(l);
        _toastLabels.Add((l, 3.2f));
        while (_toastLabels.Count > 4) { _toastLabels[0].L.QueueFree(); _toastLabels.RemoveAt(0); }
    }

    /// <summary>Starts a black overlay with a line of text that fades out after <paramref name="hold"/> seconds.</summary>
    public void Overlay(string text, float hold) => Overlay(text, hold, Colors.Black);

    /// <summary>Starts an overlay of a colour (black: darkness; near white: the flash, the light of waking) that fades out after <paramref name="hold"/> seconds.</summary>
    public void Overlay(string text, float hold, Color color, float fade = 1.2f)
    {
        _overlayText.Text = text; _overlayHold = hold; _overlayTime = 0; _overlayFade = fade; _overlayColor = color;
        _overlay.Color = new Color(color, 1); _overlayText.Modulate = Colors.White;
    }

    /// <summary>Opacity of the overlay now (0 when gone).</summary>
    public float OverlayAlpha => _overlay.Color.A;

    private float _overlayFade = 1.2f;
    private Color _overlayColor = Colors.Black;

    // ------------------------------------------------------------------ panels

    private void Fill(Menu m, List<Row> rows)
    {
        m.Rows.Clear(); m.Rows.AddRange(rows);
        m.Selected = Mathf.Clamp(m.Selected, 0, Mathf.Max(0, m.Rows.Count - 1));
        Refresh(m);
    }

    private void Refresh(Menu m)
    {
        foreach (var c in m.List.GetChildren()) c.QueueFree();
        if (m.Rows.Count == 0) { var e = MakeLabel(9, Dim); e.Text = m.Empty; m.List.AddChild(e); if (m != _sheet) m.Detail.Text = ""; return; }
        for (int i = 0; i < m.Rows.Count; i++)
        {
            var r = m.Rows[i];
            if (r.Group.Length > 0) { var h = MakeLabel(9, Dim); h.Text = $"— {r.Group} —"; m.List.AddChild(h); }
            var l = MakeLabel(9, i == m.Selected ? Select : r.Enabled ? Text : Disabled);
            l.Text = (i == m.Selected ? "› " : "  ") + r.Text;
            l.MouseFilter = Control.MouseFilterEnum.Stop;
            l.SetMeta("row", i);
            int index = i; var menu = m;
            l.GuiInput += ev =>
            {
                // the row under the cursor is the one selected and the one a click acts on (Godot maps the
                // click through the window's scaling, so this holds at any window size)
                if (ev is InputEventMouseMotion && menu.Selected != index) { menu.Selected = index; Callable.From(() => Refresh(menu)).CallDeferred(); }
                else if (ev is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
                {
                    menu.Selected = index;
                    GetViewport().SetInputAsHandled();
                    RowClicked?.Invoke(NameOf(menu), index);
                }
            };
            m.List.AddChild(l);
        }
        if (m != _sheet) m.Detail.Text = m.Rows[m.Selected].Detail;
    }

    private static void Move(Menu m, int d) { if (m.Rows.Count > 0) m.Selected = Mathf.PosMod(m.Selected + d, m.Rows.Count); }

    /// <summary>Opens (with rows) or closes (null) the pause menu.</summary>
    public void ShowPause(List<Row>? rows) { _pause.Box.Visible = rows != null; if (rows != null) { _pause.Selected = Mathf.Clamp(_pause.Selected, 0, rows.Count - 1); Fill(_pause, rows); } }
    /// <summary>Moves the pause selection.</summary>
    public void MovePause(int d) { Move(_pause, d); Refresh(_pause); }

    /// <summary>Opens the map on a region (null closes it).</summary>
    public void ShowMap(IMapSource? source, float zoom)
    {
        Map.Visible = source != null;
        if (source == null) return;
        Map.Source = source; Map.Center = source.MapPlayer; Map.Zoom = zoom;
    }

    /// <summary>Opens or closes the inventory.</summary>
    public void ShowInventory(bool open) { _inv.Box.Visible = open; if (open) Refresh(_inv); }
    /// <summary>Moves the inventory selection.</summary>
    public void MoveSelection(int d) { Move(_inv, d); Refresh(_inv); }
    /// <summary>Rebuilds the inventory list.</summary>
    public void SetRows(List<Row> rows) => Fill(_inv, rows);

    /// <summary>Opens or closes the character sheet.</summary>
    public void ShowSheet(bool open) { _sheet.Box.Visible = open; if (open) Refresh(_sheet); }
    /// <summary>Moves the sheet selection.</summary>
    public void MoveSheet(int d) { Move(_sheet, d); Refresh(_sheet); }
    /// <summary>Rebuilds the sheet: header, attribute rows and the detail text (preview + proficiencies).</summary>
    public void SetSheet(string title, string header, List<Row> rows, string detail)
    {
        _sheet.Title.Text = title; _sheet.Header.Text = Bindings.F(header); Fill(_sheet, rows); _sheet.Detail.Text = Bindings.F(detail);
    }

    /// <summary>Shows the title menu with its options and a message line (null hides it). The corner and the controls line hide behind it.</summary>
    public void ShowTitle(List<Row>? options, string message = "", string title = "SpiritOfMurin — Planície Verdejante")
    {
        _title.Box.Visible = options != null;
        foreach (var c in new Control[] { _bars, _life, _vigor, _qi, _level, _hands, _controls, _footer }) c.Visible = options == null;
        _titleShown = options != null;
        if (options == null) return;
        _title.Title.Text = title; _title.Header.Text = message; _title.Selected = 0; Fill(_title, options);
    }
    /// <summary>Moves the title selection.</summary>
    public void MoveTitle(int d) { Move(_title, d); Refresh(_title); }

    private bool _titleShown;

    /// <summary>Shows the corner minimap for a region (null hides it).</summary>
    public void SetMiniMap(IMapSource? source, float zoom)
    {
        MiniMap.Source = source;
        if (source != null) { MiniMap.Zoom = zoom; MiniMap.Center = source.MapPlayer; }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        Vector2 size = GetViewport().GetVisibleRect().Size;
        if (size != _laidOut) Layout(size);
        // the minimap: while exploring, never over a panel, the title or a dark overlay
        bool mm = MiniMap.Source != null && !_titleShown && !AnyPanelOpen && _overlay.Color.A < 0.5f;
        if (MiniMap.Visible != mm) MiniMap.Visible = mm;
        for (int i = _toastLabels.Count - 1; i >= 0; i--)
        {
            var (l, t) = _toastLabels[i];
            t -= dt;
            if (t <= 0) { l.QueueFree(); _toastLabels.RemoveAt(i); continue; }
            l.Modulate = new Color(1, 1, 1, Mathf.Min(1, t / 0.6f));
            _toastLabels[i] = (l, t);
        }
        if (_bannerT > 0) { _bannerT -= dt; _banner.Modulate = new Color(1, 1, 1, Mathf.Min(1, _bannerT / 0.6f)); if (_bannerT <= 0) _banner.Visible = false; }
        if (_speechT > 0) { _speechT -= dt; if (_speechT <= 0) _speech.Visible = false; }
        if (_overlay.Color.A > 0)
        {
            _overlayTime += dt;
            float a = _overlayTime < _overlayHold ? 1 : Mathf.Max(0, 1 - (_overlayTime - _overlayHold) / Mathf.Max(0.05f, _overlayFade));
            _overlay.Color = new Color(_overlayColor, a);
            _overlayText.Modulate = new Color(1, 1, 1, a);
        }
    }
}
