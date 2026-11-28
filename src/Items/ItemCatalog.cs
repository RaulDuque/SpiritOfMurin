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

/// <summary>
/// A weapon's data (T18), in <c>assets/data/equipamento/armas/&lt;id&gt;.json</c>: the drawing in the hand, the kind of
/// blade, the strike's numbers (reach, damage, chain, timing) and its own technique. The prologue's sword and the stone
/// knife are the first two; their numbers are the ones the tuning had.
/// </summary>
public sealed class WeaponDef
{
    /// <summary>The drawing in the hand (<c>src/Elements/Armas/</c>, registered in <c>ArmaArts</c>).</summary>
    public string Art { get; set; } = "";
    /// <summary>"longa" (sword-like: three cuts, the thrust) or "curta" (knife-like: the short cut, also a tool).</summary>
    public string Kind { get; set; } = "longa";
    /// <summary>Reach of the cut (px).</summary>
    public float Reach { get; set; }
    /// <summary>Damage of a cut.</summary>
    public int Damage { get; set; }
    /// <summary>Cuts in the chain.</summary>
    public int Chain { get; set; } = 3;
    /// <summary>Seconds before the cut is live.</summary>
    public float Startup { get; set; }
    /// <summary>Seconds the cut is live.</summary>
    public float Active { get; set; }
    /// <summary>Seconds of recovery after it.</summary>
    public float Recovery { get; set; }
    /// <summary>Seconds into the cut when the next one can start.</summary>
    public float CancelAt { get; set; }
    /// <summary>The body's lunge on a cut (px).</summary>
    public float Lunge { get; set; }
    /// <summary>The last cut of a chain of 3 or more: extra startup and live time (s).</summary>
    public float ThirdExtra { get; set; }
    /// <summary>The last cut: extra recovery (s).</summary>
    public float ThirdRecovery { get; set; }
    /// <summary>The last cut: later chaining (s).</summary>
    public float ThirdCancel { get; set; }
    /// <summary>The last cut's lunge (px).</summary>
    public float ThirdLunge { get; set; }
    /// <summary>The last cut's damage multiplier.</summary>
    public int ThirdMult { get; set; } = 1;
    /// <summary>The weapon's own techniques (ids in <c>assets/data/equipamento/_tecnicas_armas.json</c>), discovered in play.</summary>
    public List<string> Techniques { get; set; } = new();
}

/// <summary>
/// Something worn (T18), in <c>assets/data/equipamento/roupas|capacetes/&lt;id&gt;.json</c>: where it goes, its drawing,
/// how much damage it holds back and how much it weighs (decision 1: weight raises the Vigor a cut and a step cost).
/// </summary>
public sealed class WearDef
{
    /// <summary>"corpo" (robe) or "cabeca" (hat).</summary>
    public string Slot { get; set; } = "";
    /// <summary>The drawing (<c>src/Elements/Roupas/</c> or <c>src/Elements/Capacetes/</c>).</summary>
    public string Art { get; set; } = "";
    /// <summary>Fraction of the damage it holds back (0.05 = 5%), added to the others up to the cap in <c>plains_progression.json</c>.</summary>
    public float Defense { get; set; }
    /// <summary>Fraction added to the Vigor a cut and a step cost (0.1 = +10%).</summary>
    public float Weight { get; set; }
}

/// <summary>One item's data (snake_case in <c>plains_items.json</c>).</summary>
public sealed class ItemDef
{
    /// <summary>Id, filled from the dictionary key.</summary>
    public string Id { get; set; } = "";
    /// <summary>Name shown to the player.</summary>
    public string Name { get; set; } = "";
    /// <summary>The bag's tab (T06): "equipamento", "ferramenta", "consumivel" or "material".</summary>
    public string Category { get; set; } = "";
    /// <summary>Short inventory text.</summary>
    public string Description { get; set; } = "";
    /// <summary>What this item puts in the hand ("knife", "axe", "pickaxe"), or empty.</summary>
    public string Equip { get; set; } = "";
    /// <summary>Its tool numbers, or null when it is not a tool.</summary>
    public ToolDef? Tool { get; set; }
    /// <summary>Its weapon data (T18), or null.</summary>
    public WeaponDef? Weapon { get; set; }
    /// <summary>What it is when worn (T18), or null.</summary>
    public WearDef? Wear { get; set; }
    /// <summary>T18: the robe the swordsman always has (the green tunic): never leaves the bag, worn when nothing else is.</summary>
    public bool Base { get; set; }
    /// <summary>T18: where the player gets it (shown in the bag and in the guides), or empty.</summary>
    public string Onde { get; set; } = "";
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

    /// <summary>Loads the catalog from a Godot path, with the equipment (T18) from <paramref name="equipment"/>.</summary>
    public static ItemCatalog Load(string path = DefaultPath, string equipment = EquipmentFolder)
    {
        var c = Parse(Godot.FileAccess.GetFileAsString(path));
        foreach (var d in LoadEquipment(equipment)) c.Items[d.Id] = d;
        return c;
    }

    /// <summary>Where the equipment lives: one JSON per piece in <c>armas/</c>, <c>roupas/</c>, <c>capacetes/</c>.</summary>
    public const string EquipmentFolder = "res://assets/data/equipamento";

    /// <summary>Parses one piece of equipment (its JSON file: the item's own fields plus <c>id</c>).</summary>
    public static ItemDef ParseEquipment(string json)
    {
        var d = JsonSerializer.Deserialize<ItemDef>(json, PlainsGenSettings.Json) ?? new ItemDef();
        if (d.Category.Length == 0) d.Category = "equipamento";
        if (d.Weapon != null && d.Equip.Length == 0) d.Equip = d.Weapon.Kind == "curta" ? "knife" : "weapon";
        if (d.Wear != null && d.Equip.Length == 0) d.Equip = "wear";
        return d;
    }

    /// <summary>Every piece of equipment under <paramref name="folder"/> (recursively; files starting with "_" are skipped).</summary>
    public static List<ItemDef> LoadEquipment(string folder = EquipmentFolder)
    {
        var all = new List<ItemDef>();
        void Walk(string dir)
        {
            using var d = Godot.DirAccess.Open(dir);
            if (d == null) return;
            foreach (var f in d.GetFiles())
                if (f.EndsWith(".json") && !f.StartsWith("_")) all.Add(ParseEquipment(Godot.FileAccess.GetFileAsString($"{dir}/{f}")));
            foreach (var sub in d.GetDirectories()) Walk($"{dir}/{sub}");
        }
        Walk(folder);
        return all;
    }
}
