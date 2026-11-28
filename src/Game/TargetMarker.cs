using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Game;

/// <summary>
/// The target of E when several things are within reach (T06, option C of the board): a ring on the ground under the
/// chosen one and, beside the character, a short list of what is within reach with the chosen one marked ("▸ erva (E)")
/// and how to change it (the mouse wheel, actions <c>target_next</c>/<c>target_prev</c>). With a single target nothing
/// extra shows (the usual prompt is enough).
/// <b>Where to change it:</b> the ring in <see cref="_Draw"/>, the list's look in <see cref="DrawList"/>, the colours below.
/// The choice itself is <c>src/Items/BagRules.cs</c> (<c>TargetChoice</c>); the candidates come from <c>PlainsGame.Interactables</c>.
/// </summary>
public partial class TargetMarker : Node2D
{
    /// <summary>Colours: the ring, the chosen line, the others, the panel.</summary>
    public static readonly Color RingC = new("#6fd8d2"), ChosenC = new("#6fd8d2"), OtherC = new("#ece5d0"), PanelC = new(0.08f, 0.075f, 0.06f, 0.85f), HelpC = new("#a39e8c");

    /// <summary>Names of the targets within reach, nearest first.</summary>
    public List<string> Names { get; } = new();
    /// <summary>The chosen one's index.</summary>
    public int Chosen { get; private set; }
    /// <summary>Where the chosen target stands (world).</summary>
    public Vector2 At { get; private set; }
    /// <summary>The body the list stands beside.</summary>
    public Node2D? Around { get; set; }

    private float _t;
    private Font _font = null!;

    /// <inheritdoc />
    public override void _Ready() { _font = ThemeDB.FallbackFont; ZIndex = 50; }

    /// <summary>Shows the ring and the list (nothing when fewer than two targets).</summary>
    public void Show(IReadOnlyList<string> names, int chosen, Vector2 at)
    {
        Names.Clear(); Names.AddRange(names); Chosen = chosen; At = at; Visible = names.Count > 1; QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Process(double delta) { _t += (float)delta; if (Visible) QueueRedraw(); }

    /// <inheritdoc />
    public override void _Draw()
    {
        if (Names.Count < 2) return;
        // the ring under the chosen one, breathing a little
        float r = 9 + Mathf.Sin(_t * 5) * 0.8f;
        DrawArc(At + new Vector2(0, 1), r, 0, Mathf.Tau, 24, new Color(0, 0, 0, 0.5f), 3);
        DrawSetTransform(At + new Vector2(0, 1), 0, new Vector2(1, 0.45f));
        DrawArc(Vector2.Zero, r, 0, Mathf.Tau, 28, RingC, 1.5f);
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);
        DrawList();
    }

    /// <summary>The short list to the right of the character.</summary>
    private void DrawList()
    {
        if (Around == null || !IsInstanceValid(Around)) return;
        const int px = 8;
        var lines = new List<string>();
        for (int i = 0; i < Names.Count; i++) lines.Add((i == Chosen ? "▸ " : "   ") + Names[i] + (i == Chosen ? Bindings.F(" ([use])") : ""));
        string help = "roda do mouse: trocar";
        float w = 0; foreach (var l in lines) w = Mathf.Max(w, _font.GetStringSize(l, HorizontalAlignment.Left, -1, px).X);
        w = Mathf.Max(w, _font.GetStringSize(help, HorizontalAlignment.Left, -1, 7).X) + 10;
        var o = Around.GlobalPosition + new Vector2(18, -40);
        DrawRect(new Rect2(o, new Vector2(w, lines.Count * 10 + 14)), PanelC);
        for (int i = 0; i < lines.Count; i++) DrawString(_font, o + new Vector2(5, 10 + i * 10), lines[i], HorizontalAlignment.Left, -1, px, i == Chosen ? ChosenC : OtherC);
        DrawString(_font, o + new Vector2(5, 10 + lines.Count * 10 + 1), help, HorizontalAlignment.Left, -1, 7, HelpC);
    }
}
