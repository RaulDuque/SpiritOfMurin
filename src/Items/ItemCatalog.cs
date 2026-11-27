using System.Collections.Generic;
using System.Text.Json;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Items;

/// <summary>What a tool does (pedido 14, Entrega 5): which elements it works, how much per strike, its reach and damage.</summary>
public sealed class ToolDef
{
    /// <summary>"machado", "picareta" or "faca" (matched against an element's <c>tools</c>).</summary>
    public string Kind { get; set; } = "";
    /// <summary>Work per strike (strikes needed = toughness / power).</summary>
    public float Power { get; set; } = 1;
    /// <summary>Reach of the strike (px); 0 = the weapon's own (the knife's).</summary>
    public float Reach { get; set; }
    /// <summary>Damage on animals; 0 = the weapon's own.</summary>
    public int Damage { get; set; }
}

/// <summary>One item's data (snake_case in <c>plains_items.json</c>).</summary>
public sealed class ItemDef
{
    /// <summary>Id, filled from the dictionary key.</summary>
    public string Id { get; set; } = "";
    /// <summary>Name shown to the player.</summary>
    public string Name { get; set; } = "";
    /// <summary>Short inventory text.</summary>
    public string Description { get; set; } = "";
    /// <summary>What this item puts in the hand ("knife", "axe", "pickaxe"), or empty.</summary>
    public string Equip { get; set; } = "";
    /// <summary>Its tool numbers, or null when it is not a tool.</summary>
    public ToolDef? Tool { get; set; }
    /// <summary>Life restored gradually when used.</summary>
    public int RegenAmount { get; set; }
    /// <summary>Seconds over which <see cref="RegenAmount"/> is restored.</summary>
    public float RegenSeconds { get; set; }
    /// <summary>Life restored at once when used.</summary>
    public int HealNow { get; set; }
    /// <summary>Vigor restored at once when used.</summary>
    public int VigorNow { get; set; }
    /// <summary>True for medicinal preparations (INT raises their potency).</summary>
    public bool Medicine { get; set; }
    /// <summary>True for the items the Bolsa Simples keeps at hand (quick use key).</summary>
    public bool QuickSlot { get; set; }
    /// <summary>Permanent effect of carrying the item (text), or empty.</summary>
    public string Passive { get; set; } = "";
    /// <summary>True when using the item does something (it is then consumed).</summary>
    public bool Usable => RegenAmount > 0 || HealNow > 0 || VigorNow > 0;
}

/// <summary>A crafting recipe: materials consumed once, item produced.</summary>
public sealed class RecipeDef
{
    /// <summary>Recipe id.</summary>
    public string Id { get; set; } = "";
    /// <summary>Item produced.</summary>
    public string Output { get; set; } = "";
    /// <summary>How many are produced.</summary>
    public int Count { get; set; } = 1;
    /// <summary>Materials consumed: item id → quantity.</summary>
    public Dictionary<string, int> Inputs { get; set; } = new();
    /// <summary>Place needed to make it ("fogueira"), or empty for anywhere.</summary>
    public string Station { get; set; } = "";
}

/// <summary>The items and recipes, loaded from <c>res://assets/data/plains/plains_items.json</c>.</summary>
public sealed class ItemCatalog
{
    /// <summary>Default data path.</summary>
    public const string DefaultPath = "res://assets/data/plains/plains_items.json";

    /// <summary>Items by id.</summary>
    public Dictionary<string, ItemDef> Items { get; set; } = new();

    /// <summary>Recipes in display order.</summary>
    public List<RecipeDef> Recipes { get; set; } = new();

    /// <summary>The item, or a placeholder named after the id.</summary>
    public ItemDef Get(string id) => Items.TryGetValue(id, out var d) ? d : new ItemDef { Id = id, Name = id };

    /// <summary>Parses the catalog from JSON text.</summary>
    public static ItemCatalog Parse(string json)
    {
        var c = JsonSerializer.Deserialize<ItemCatalog>(json, PlainsGenSettings.Json) ?? new ItemCatalog();
        foreach (var (id, def) in c.Items) def.Id = id;
        return c;
    }

    /// <summary>Loads the catalog from a Godot path.</summary>
    public static ItemCatalog Load(string path = DefaultPath) => Parse(Godot.FileAccess.GetFileAsString(path));
}
