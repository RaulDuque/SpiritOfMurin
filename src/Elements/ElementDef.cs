using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Elements;

/// <summary>A material an element gives when it is used up: <c>min</c> to <c>max</c> of an item (fixed for the seed and the element).</summary>
public sealed class DropDef
{
    /// <summary>Item id (plains_items.json).</summary>
    public string Item { get; set; } = "";
    /// <summary>Fewest given.</summary>
    public int Min { get; set; } = 1;
    /// <summary>Most given.</summary>
    public int Max { get; set; } = 1;
}

/// <summary>A bonus material: given with <c>chance</c> + <c>per_sor</c> × SOR (capped at <c>cap</c>), rolled once per element.</summary>
public sealed class BonusDef
{
    /// <summary>Item id.</summary>
    public string Item { get; set; } = "";
    /// <summary>Base chance (0-1).</summary>
    public float Chance { get; set; }
    /// <summary>Chance added per SOR point.</summary>
    public float PerSor { get; set; }
    /// <summary>Highest chance.</summary>
    public float Cap { get; set; } = 0.9f;
    /// <summary>How many when it comes.</summary>
    public int Count { get; set; } = 1;
}

/// <summary>
/// One element of the world (pedido 14, element standard): a tree, a rock, a bamboo clump, a bush. Read from
/// <c>assets/data/elements/&lt;family folder&gt;/&lt;id&gt;.json</c> (one file per element, snake_case keys).
/// The picture is the art named by <see cref="Art"/> (<c>src/Elements/**/&lt;Art&gt;Art.cs</c>, registered in
/// <see cref="ElementArts"/>); the behaviour is shared by the family (<see cref="WorldElement"/>).
/// See <c>docs/elementos/PADRAO.md</c>.
/// </summary>
public sealed class ElementDef
{
    /// <summary>Stable id (also the file name). Saves key on the world feature, not on this id.</summary>
    public string Id { get; set; } = "";
    /// <summary>Name shown to the player.</summary>
    public string Name { get; set; } = "";
    /// <summary>Family: "arvore", "rocha", "bambu", "arbusto".</summary>
    public string Family { get; set; } = "";
    /// <summary>Art id (which drawing class).</summary>
    public string Art { get; set; } = "";
    /// <summary>Where the generator may place it: habitat ("grass", "bank", "bushes", "mud") → weight among its family.</summary>
    public Dictionary<string, float> Habitats { get; set; } = new();
    /// <summary>Half width (px) of the base that collides while standing; 0 = never collides (bushes).</summary>
    public float BaseHalfWidth { get; set; }
    /// <summary>Height (px) of the colliding base.</summary>
    public float BaseHeight { get; set; }
    /// <summary>Half width and height (px) of the base after it is used up (a stump); 0 = nothing collides.</summary>
    public float DepletedHalfWidth { get; set; }
    /// <summary>Height (px) of the used-up base.</summary>
    public float DepletedHeight { get; set; }
    /// <summary>Work it takes: strikes of power 1 (a better tool takes fewer). 0 = cannot be worked (decor).</summary>
    public float Toughness { get; set; }
    /// <summary>Tool kinds that work it ("machado", "picareta", "faca").</summary>
    public List<string> Tools { get; set; } = new();
    /// <summary>What the game says when the tool in hand does not work it.</summary>
    public string Refusal { get; set; } = "";
    /// <summary>What it leaves when used up: "toco" (stump), "cascalho" (rubble), "talos" (cut stalks), "nada".</summary>
    public string Leaves { get; set; } = "nada";
    /// <summary>Materials given once, when used up.</summary>
    public List<DropDef> Drops { get; set; } = new();
    /// <summary>Bonus materials (SOR raises their chance).</summary>
    public List<BonusDef> Bonus { get; set; } = new();
    /// <summary>XP when used up.</summary>
    public int Xp { get; set; }
    /// <summary>Bushes: how far (px) a passing body bends the leaves; 0 = no contact reaction.</summary>
    public float ContactRadius { get; set; }
    /// <summary>Rocks: true when the rock shows a vein and can be mined (decorative rocks have it false).</summary>
    public bool Veined { get; set; }
    /// <summary>Free notes for the art (colours, sizes), read by the art class; keys are the art's own.</summary>
    public Dictionary<string, JsonElement> Look { get; set; } = new();

    /// <summary>True when a tool can work it.</summary>
    public bool Workable => Toughness > 0 && Tools.Count > 0;

    /// <summary>A number from <see cref="Look"/>, or <paramref name="fallback"/>.</summary>
    public float LookF(string key, float fallback) => Look.TryGetValue(key, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetSingle() : fallback;
    /// <summary>A colour from <see cref="Look"/> ("#rrggbb"), or <paramref name="fallback"/>.</summary>
    public Color LookC(string key, string fallback) => new(Look.TryGetValue(key, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString()! : fallback);
}

/// <summary>
/// Every element in <c>res://assets/data/elements/</c> (all sub-folders), by id. Adding an element is adding a
/// file there (and, for a new picture, an art class): see <c>docs/elementos/PADRAO.md</c> and <c>/create</c>.
/// </summary>
public sealed class ElementCatalog
{
    /// <summary>Default folder.</summary>
    public const string DefaultFolder = "res://assets/data/elements";

    /// <summary>Elements by id.</summary>
    public Dictionary<string, ElementDef> All { get; } = new();

    /// <summary>The element, or null.</summary>
    public ElementDef? Get(string id) => All.TryGetValue(id, out var d) ? d : null;

    /// <summary>Adds an element parsed from JSON text (the file name is its id when the text has none).</summary>
    public ElementDef Add(string json, string idFromFile = "")
    {
        var d = JsonSerializer.Deserialize<ElementDef>(json, PlainsGenSettings.Json) ?? new ElementDef();
        if (d.Id.Length == 0) d.Id = idFromFile;
        All[d.Id] = d;
        return d;
    }

    /// <summary>The elements of a family, in id order (stable).</summary>
    public List<ElementDef> Family(string family) => All.Values.Where(e => e.Family == family).OrderBy(e => e.Id, System.StringComparer.Ordinal).ToList();

    /// <summary>
    /// Picks the element of <paramref name="family"/> for a habitat with a roll in [0, 1) (fixed by the seed and the
    /// tile, so the same place always gets the same element). Null when none of the family grows there.
    /// </summary>
    public ElementDef? Pick(string family, string habitat, float roll, System.Func<ElementDef, bool>? where = null)
    {
        var options = Family(family).Where(e => (where == null || where(e)) && e.Habitats.TryGetValue(habitat, out float w) && w > 0).ToList();
        float total = options.Sum(e => e.Habitats[habitat]);
        if (total <= 0) return null;
        float r = roll * total;
        foreach (var e in options) { r -= e.Habitats[habitat]; if (r < 0) return e; }
        return options[^1];
    }

    /// <summary>Loads every <c>.json</c> under <paramref name="folder"/> (recursively).</summary>
    public static ElementCatalog Load(string folder = DefaultFolder)
    {
        var c = new ElementCatalog();
        void Walk(string dir)
        {
            using var d = DirAccess.Open(dir);
            if (d == null) return;
            foreach (var sub in d.GetDirectories()) Walk($"{dir}/{sub}");
            foreach (var f in d.GetFiles())
                if (f.EndsWith(".json")) c.Add(Godot.FileAccess.GetFileAsString($"{dir}/{f}"), f[..^5]);
        }
        Walk(folder);
        return c;
    }
}
