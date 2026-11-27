using System.Linq;
using Godot;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Game;

/// <summary>
/// Window and full screen for every scene (autoload "Display"). F11 — or the pause menu — toggles real
/// full screen; the choice is saved in the preferences and applied at start. The picture keeps whole-number
/// pixel scaling (crisp pixel art) and, with the "expand" aspect, fills the window: a bigger window shows
/// more of the world instead of black borders. <c>--estica-antiga</c> restores the old "keep" aspect
/// (the black borders) for before/after comparisons.
/// </summary>
public partial class DisplayManager : Node
{
    /// <summary>True when the game runs inside the editor's Game tab (F5 with "embed" on): the editor sets the
    /// window's size and full screen is not available there.</summary>
    public static bool Embedded => Engine.IsEmbeddedInEditor();

    /// <summary>A message for the player (F11 refused inside the editor); the HUD shows it.</summary>
    public event System.Action<string>? Notice;

    /// <summary>True once the "inside the editor" notice was shown (once per run).</summary>
    public static bool EmbedNoticeShown { get; set; }

    /// <summary>What to tell the player when the game runs inside the editor (F11 cannot work there).</summary>
    public const string EmbedNotice = "O jogo está dentro do editor (aba Jogo): ali a tela cheia não funciona. Para F11, desmarque \"Integrar Jogo na Próxima Execução\" no menu ⋮ da aba Jogo.";

    /// <summary>The last measure written to the diagnostics file.</summary>
    public string LastMeasure { get; private set; } = "";

    /// <summary>True when full screen is on.</summary>
    public bool Fullscreen => DisplayServer.WindowGetMode() is DisplayServer.WindowMode.Fullscreen or DisplayServer.WindowMode.ExclusiveFullscreen;

    /// <summary>The scene tree's display manager (null when the autoload is not there, e.g. a bare test).</summary>
    public static DisplayManager? Instance { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        Instance = this;
        Bindings.Init();
        var args = OS.GetCmdlineUserArgs().Concat(OS.GetCmdlineArgs()).ToArray();
        var win = GetWindow();
        OldStretch = args.Contains("--estica-antiga");
        win.ContentScaleAspect = OldStretch ? Window.ContentScaleAspectEnum.Keep : Window.ContentScaleAspectEnum.Expand;
        win.SizeChanged += () => { Fit(); Callable.From(Report).CallDeferred(); };
        Fit();
        bool forcedWindow = args.Contains("--windowed") || args.Contains("--janela");
        if (!forcedWindow && !Embedded && Prefs.Load().Fullscreen) SetFullscreen(true, save: false);
        Callable.From(Report).CallDeferred();
    }

    /// <summary>
    /// Writes what the game occupies now to the output and to <c>user://diagnostico_tela.txt</c>: how it runs
    /// (inside the editor or in its own window), the window mode, the window and screen sizes, the scale,
    /// the world seen and the margins. It is how an F5 run is measured.
    /// </summary>
    public void Report()
    {
        var vp = GetViewport();
        var (window, picture, scale, world) = Measure(vp);
        int screen = DisplayServer.WindowGetCurrentScreen();
        Vector2I scr = DisplayServer.ScreenGetSize(screen);
        float left = picture.Position.X, top = picture.Position.Y, right = window.X - picture.End.X, bottom = window.Y - picture.End.Y;
        LastMeasure = $"TELA modo={(Embedded ? "incorporado ao editor (aba Game)" : "janela própria")} janela_modo={DisplayServer.WindowGetMode()} " +
            $"janela={window.X}x{window.Y} monitor={scr.X}x{scr.Y} desenhado={picture.Size.X:0}x{picture.Size.Y:0} em ({left:0},{top:0}) " +
            $"escala={scale:0.##} mundo={world.X:0}x{world.Y:0} margens esq={left:0} dir={right:0} cima={top:0} baixo={bottom:0}";
        GD.Print(LastMeasure);
        using var f = FileAccess.Open("user://diagnostico_tela.txt", FileAccess.ModeFlags.Write);
        f?.StoreLine(LastMeasure);
    }

    /// <inheritdoc />
    public override void _Notification(int what)
    {
        // the controls and the preferences are written when the game closes, too
        if (what == NotificationWMCloseRequest) Bindings.Save();
    }

    /// <inheritdoc />
    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed("fullscreen")) { Toggle(); GetViewport().SetInputAsHandled(); }
    }

    /// <summary>True when the old stretch (fixed 640 × 360, black borders) is in force (comparisons).</summary>
    public bool OldStretch { get; set; }

    /// <summary>The smallest world the screen shows, in game pixels (the base resolution).</summary>
    public static readonly Vector2I Base = new(640, 360);

    /// <summary>
    /// Makes the picture fill the window with whole pixels: the scale is the largest whole number that
    /// still shows at least the base 640 × 360, and the game's resolution becomes the window divided by it
    /// (1600 × 900 → 2×, 800 × 450; 1920 × 1080 → 3×, 640 × 360). What is left is less than one game pixel.
    /// </summary>
    public void Fit()
    {
        var win = GetWindow();
        if (OldStretch) { win.ContentScaleSize = Base; return; }
        Vector2I s = win.Size;
        if (s.X <= 0 || s.Y <= 0) return;
        int k = Mathf.Max(1, Mathf.Min(s.X / Base.X, s.Y / Base.Y));
        var size = new Vector2I(Mathf.Max(Base.X / 2, s.X / k), Mathf.Max(Base.Y / 2, s.Y / k));
        if (win.ContentScaleSize != size) win.ContentScaleSize = size;
    }

    /// <summary>Switches full screen on or off and saves the preference.</summary>
    public void Toggle()
    {
        if (Embedded)
        {
            Notice?.Invoke(EmbedNotice);
            return;
        }
        SetFullscreen(!Fullscreen);
    }

    /// <summary>Sets full screen (borderless, the whole monitor) or back to a window.</summary>
    public void SetFullscreen(bool on, bool save = true)
    {
        DisplayServer.WindowSetMode(on ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
        if (!on && DisplayServer.WindowGetSize().X < 640) DisplayServer.WindowSetSize(new Vector2I(1280, 720));
        if (save) { var p = Prefs.Load(); p.Fullscreen = on; p.Save(); }
    }

    /// <summary>What the game occupies in the window now: window size, the game picture (scaled), the whole-number scale and the world area seen (base pixels).</summary>
    public static (Vector2I Window, Rect2 Picture, float Scale, Vector2 World) Measure(Viewport vp)
    {
        var win = vp.GetWindow();
        Vector2I size = win.Size;
        var t = vp.GetFinalTransform();
        float scale = t.X.X;
        Vector2 world = vp.GetVisibleRect().Size;
        var picture = new Rect2(t.Origin, world * scale);
        return (size, picture, scale, world);
    }
}
