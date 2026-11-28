using System.Collections.Generic;
using System.Linq;
using Godot;
using SpiritOfMurin.Elements;

namespace SpiritOfMurin.Items;

/// <summary>
/// The rules behind the bag (T06), without nodes, so they are unit-tested: the tabs and what each holds, a
/// recipe's ingredients as "tenho X de Y", and how a tool compares with the one in hand, in the terms a player
/// feels (strikes to fell the same tree, to break the same rock). The bag's drawing is <c>src/Game/BagPanel.cs</c>;
/// the rows it shows are built by <c>PlainsGame.RefreshRows</c>.
/// </summary>
public static class BagRules
{
    /// <summary>The bag's tabs, in order: id and the name shown. "fabricacao" lists the recipes.</summary>
    public static readonly (string Id, string Name)[] Tabs =
    {
        ("tudo", "Tudo"), ("equipamento", "Equipamentos"), ("ferramenta", "Ferramentas"), ("consumivel", "Consumíveis"), ("material", "Materiais"), ("fabricacao", "Fabricação"),
    };

    /// <summary>The tab of a category id (items without one are materials).</summary>
    public static string CategoryOf(ItemDef d) => d.Category.Length > 0 ? d.Category : "material";

    /// <summary>True when an item shows in a tab ("tudo" shows every item; "fabricacao" none).</summary>
    public static bool InTab(ItemDef d, string tab) => tab == "tudo" || (tab != "fabricacao" && CategoryOf(d) == tab);

    /// <summary>One ingredient of a recipe: what it is, how many are carried, how many it takes.</summary>
    public readonly record struct Need(string Item, string Name, int Have, int Needed)
    {
        /// <summary>True when there is enough.</summary>
        public bool Ok => Have >= Needed;
        /// <summary>"Sílex: tenho 2 de 3".</summary>
        public string Line => $"{Name}: tenho {Have} de {Needed}";
    }

    /// <summary>The ingredients of a recipe with what is carried now.</summary>
    public static List<Need> Needs(RecipeDef r, System.Func<string, int> have, ItemCatalog c) =>
        r.Inputs.Select(kv => new Need(kv.Key, c.Get(kv.Key).Name, have(kv.Key), kv.Value)).ToList();

    /// <summary>Strikes a tool needs to use up an element; null when it cannot work it.</summary>
    public static int? Strikes(ElementDef e, ToolDef? t) =>
        t == null || !e.Workable || !e.Tools.Contains(t.Kind) ? null : Mathf.CeilToInt(e.Toughness / Mathf.Max(0.01f, t.Power) - 0.0001f);

    /// <summary>"6 golpes", "não corta" / "não quebra".</summary>
    public static string StrikesText(ElementDef e, int? n) => n is { } k ? $"{k} golpe{(k == 1 ? "" : "s")}" : e.Family == "rocha" ? "não quebra" : "não corta";

    /// <summary>One line of a comparison: what, with the hand's thing, with the other.</summary>
    public readonly record struct Compare(string What, string Hand, string Other, bool Better, bool Worse);

    /// <summary>
    /// How <paramref name="other"/> compares with the tool or weapon in hand, on every workable element whose result
    /// changes or that one of them works (one line per element name), then reach and damage on animals.
    /// <paramref name="handDamage"/>/<paramref name="otherDamage"/> already include FOR.
    /// </summary>
    public static List<Compare> CompareTools(ToolDef? hand, float handReach, float handDamage, ToolDef? other, float otherReach, float otherDamage, IEnumerable<ElementDef> elements)
    {
        var rows = new List<Compare>();
        foreach (var e in elements.Where(e => e.Workable).GroupBy(e => e.Name).Select(g => g.First()))
        {
            int? a = Strikes(e, hand), b = Strikes(e, other);
            if (a == null && b == null) continue;
            bool better = b != null && (a == null || b < a), worse = a != null && (b == null || b > a);
            rows.Add(new Compare(e.Name, StrikesText(e, a), StrikesText(e, b), better, worse));
        }
        if (!Mathf.IsEqualApprox(handReach, otherReach)) rows.Add(new Compare("Alcance", $"{handReach:0} px", $"{otherReach:0} px", otherReach > handReach, otherReach < handReach));
        rows.Add(new Compare("Dano em animais", Num(handDamage), Num(otherDamage), otherDamage > handDamage + 0.001f, otherDamage < handDamage - 0.001f));
        return rows;
    }

    private static string Num(float v) => v.ToString("0.00", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")); // hundredths: FOR changes damage by 6% a point
}

/// <summary>
/// Gains shown over the character (T06): the same thing gained again within a short window adds up
/// ("+1 Galho Rijo" then "+1" → "+2 Galho Rijo") instead of stacking lines. Pure: the clock is passed in.
/// </summary>
public sealed class GainLog
{
    /// <summary>One line: what (key), its label, the total and when it was last added.</summary>
    public sealed class Entry { public string Key = "", Label = ""; public int Amount; public float At, Born; }

    /// <summary>Seconds a line stays open to add to (and shows).</summary>
    public float Window { get; set; } = 2.5f;
    /// <summary>The lines now, oldest first.</summary>
    public List<Entry> Lines { get; } = new();

    /// <summary>Adds <paramref name="amount"/> of <paramref name="key"/> at time <paramref name="now"/>; returns the line's text.</summary>
    public string Add(string key, string label, int amount, float now)
    {
        Lines.RemoveAll(l => now - l.At > Window);
        var e = Lines.FirstOrDefault(l => l.Key == key);
        if (e == null) { e = new Entry { Key = key, Label = label, Born = now }; Lines.Add(e); }
        e.Amount += amount; e.At = now;
        return Text(e);
    }

    /// <summary>"+3 Galho Rijo".</summary>
    public static string Text(Entry e) => $"+{e.Amount} {e.Label}";

    /// <summary>Drops the lines older than the window.</summary>
    public void Expire(float now) => Lines.RemoveAll(l => now - l.At > Window);
}

/// <summary>
/// Which of the things within reach E acts on (T06): the nearest at first; the mouse wheel moves along the list
/// (nearest first). When the list changes, the chosen one stays chosen while it is still within reach.
/// </summary>
public sealed class TargetChoice
{
    /// <summary>The candidates now, nearest first (ids).</summary>
    public List<string> Ids { get; private set; } = new();
    /// <summary>The chosen one's index in <see cref="Ids"/>.</summary>
    public int Index { get; private set; }
    private string _chosen = "";
    private bool _picked;

    /// <summary>The chosen id, or "" when nothing is within reach.</summary>
    public string Chosen => Ids.Count == 0 ? "" : Ids[Mathf.Clamp(Index, 0, Ids.Count - 1)];

    /// <summary>Updates the candidates (nearest first). A choice made with the wheel stays while it is within reach.</summary>
    public void Update(IReadOnlyList<string> nearestFirst)
    {
        Ids = nearestFirst.ToList();
        int keep = _picked ? Ids.IndexOf(_chosen) : -1;
        if (keep < 0) { _picked = false; Index = 0; } else Index = keep;
        _chosen = Chosen;
    }

    /// <summary>Moves the choice (+1 next, −1 previous), wrapping around.</summary>
    public void Step(int d)
    {
        if (Ids.Count < 2) return;
        Index = ((Index + d) % Ids.Count + Ids.Count) % Ids.Count;
        _chosen = Chosen; _picked = true;
    }
}
