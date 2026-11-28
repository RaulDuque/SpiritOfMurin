using Godot;
using SpiritOfMurin.Items;

namespace SpiritOfMurin.Game;

/// <summary>
/// What was just gained, over the character's head (T06, option A of the board): materials, coins and XP rise a little
/// and the same thing gained again adds up ("+3 Galho Rijo") instead of stacking lines (<see cref="GainLog"/>). The
/// lines sit above the head and fade in a couple of seconds, so the ground (prints, the boar's warnings) stays clear.
/// <b>Where to change it:</b> the colours and sizes below; how long a line adds up and shows in
/// <see cref="GainLog.Window"/> (set from <c>assets/data/plains/plains_hud.json</c>, <c>gain_seconds</c>).
/// </summary>
public partial class GainFeed : Node2D
{
    /// <summary>The body it follows.</summary>
    public Node2D? Around { get; set; }
    /// <summary>The lines and their grouping.</summary>
    public GainLog Log { get; } = new();

    /// <summary>Colours: things gained, XP, coins.</summary>
    public static readonly Color ItemC = new("#ece5d0"), XpC = new("#ffd166"), CoinC = new("#f0d27a"), OutlineC = new("#141018");
    /// <summary>Font size and the height above the feet where the lines start.</summary>
    public const int Px = 9; public const float Above = 46;

    private float _now;
    private Font _font = null!;

    /// <inheritdoc />
    public override void _Ready() { _font = ThemeDB.FallbackFont; ZIndex = 55; }

    /// <summary>Adds a gain; <paramref name="key"/> groups ("item:galho_rijo", "xp", "coins").</summary>
    public string Add(string key, string label, int amount) => Log.Add(key, label, amount, _now);

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _now += (float)delta;
        Log.Expire(_now);
        if (Around != null && IsInstanceValid(Around)) GlobalPosition = Around.GlobalPosition;
        QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        int n = Log.Lines.Count;
        for (int i = 0; i < n; i++)
        {
            var e = Log.Lines[i];
            float age = _now - e.At, k = Mathf.Clamp(age / Log.Window, 0, 1), rise = Mathf.Min(6, (_now - e.Born) * 10);
            string s = GainLog.Text(e);
            float w = _font.GetStringSize(s, HorizontalAlignment.Left, -1, Px).X;
            var at = new Vector2(-w / 2, -Above - (n - 1 - i) * (Px + 2) - rise);
            var col = e.Key == "xp" ? XpC : e.Key == "coins" ? CoinC : ItemC;
            float a = k < 0.75f ? 1 : 1 - (k - 0.75f) / 0.25f;
            DrawStringOutline(_font, at, s, HorizontalAlignment.Left, -1, Px, 3, new Color(OutlineC, a));
            DrawString(_font, at, s, HorizontalAlignment.Left, -1, Px, new Color(col, a));
        }
    }
}
