using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Combat;

namespace SpiritOfMurin.Game;

/// <summary>
/// The damage numbers of the assist mode (Esc → Modo de assistência → Mostrar números de dano, pedido 14):
/// every hit that lands shows how much it took away, floating up from the one who was hit.
/// <list type="bullet">
/// <item>damage you deal: pale bone, above the target; a critical hit is larger, gold, with "!" ("crítico" the first time);</item>
/// <item>damage you take: red-ochre with a minus sign, above the character.</item>
/// </list>
/// The numbers are the ones the combat code subtracted (after FOR, the Reforço, the critical and CON).
/// Colours and sizes are the constants below; one node per region, in the world layer (it follows the camera).
/// </summary>
public partial class CombatText : Node2D
{
    /// <summary>Colours: dealt (bone), critical (gold), taken (red-ochre), the outline.</summary>
    public static readonly Color DealtC = new("#f3ecd8"), CritC = new("#ffd166"), TakenC = new("#e0503a"), OutlineC = new("#141018");
    /// <summary>Font sizes (px at the base resolution) and how long a number stays.</summary>
    public const int Size = 9, CritSize = 12; public const float Life = 0.9f, Rise = 22;

    private sealed class Pop { public Vector2 P; public string S = ""; public Color C; public int Px; public float T; }
    private readonly List<Pop> _pops = new();
    private Font _font = null!;
    private bool _critExplained;

    /// <summary>Every text shown (tests read it).</summary>
    public List<string> Shown { get; } = new();

    /// <inheritdoc />
    public override void _Ready() { _font = ThemeDB.FallbackFont; ZIndex = 60; }

    /// <summary>A hit the character dealt at <paramref name="at"/> (the target's head). Nothing when the mode is off or nothing was taken.</summary>
    public void Dealt(Vector2 at, StrikeResult r)
    {
        if (!Assist.Numbers || r.Amount <= 0) return;
        string s = DamageRules.Format(r.Amount) + (r.Critical ? "!" : "");
        if (r.Critical && !_critExplained) { _critExplained = true; s += " crítico"; }
        Add(at, s, r.Critical ? CritC : DealtC, r.Critical ? CritSize : Size);
    }

    /// <summary>A hit the character took (the amount after CON), shown at <paramref name="at"/>.</summary>
    public void Taken(Vector2 at, float amount)
    {
        if (!Assist.Numbers || amount <= 0) return;
        Add(at, "-" + DamageRules.Format(amount), TakenC, CritSize - 1); // a little larger: red over the hurt flash
    }

    private void Add(Vector2 at, string s, Color c, int px)
    {
        // two numbers on the same spot in the same moment stack instead of overlapping
        foreach (var p in _pops) if (p.T < 0.15f && p.P.DistanceTo(at) < 8) at.Y -= px + 1;
        _pops.Add(new Pop { P = at, S = s, C = c, Px = px }); Shown.Add(s);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (_pops.Count == 0) return;
        float dt = (float)delta / Mathf.Max(0.05f, (float)Engine.TimeScale); // real time: hitstop does not freeze the numbers
        for (int i = _pops.Count - 1; i >= 0; i--) { _pops[i].T += dt; if (_pops[i].T >= Life) _pops.RemoveAt(i); }
        QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        foreach (var p in _pops)
        {
            float k = p.T / Life;
            var at = p.P + new Vector2(0, -Rise * Mathf.Sqrt(k));
            float w = _font.GetStringSize(p.S, HorizontalAlignment.Left, -1, p.Px).X;
            var o = at - new Vector2(w / 2, 0);
            var col = new Color(p.C, k < 0.7f ? 1 : 1 - (k - 0.7f) / 0.3f);
            DrawStringOutline(_font, o, p.S, HorizontalAlignment.Left, -1, p.Px, 3, new Color(OutlineC, col.A));
            DrawString(_font, o, p.S, HorizontalAlignment.Left, -1, p.Px, col);
        }
    }
}
