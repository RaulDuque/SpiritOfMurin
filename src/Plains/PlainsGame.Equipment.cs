using System.Collections.Generic;
using System.Linq;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Items;
using SpiritOfMurin.Save;
using SpiritOfMurin.Vale;

namespace SpiritOfMurin.Plains;

/// <summary>
/// T18 — the equipment in the Planície: weapons with their own numbers and drawing, the robe and the hat worn in two
/// slots (decision 1: they hold back damage and weigh on the Vigor), the traveller's bundle, and the sabre's techniques
/// found by practice (decision 2). The rules are in <see cref="EquipRules"/>; the drawings in <c>src/Elements/</c>.
/// </summary>
public partial class PlainsGame
{
    /// <summary>What is worn in each slot ("corpo", "cabeca"): an item id, or "" (the green tunic, the bare head).</summary>
    public Dictionary<string, string> Wearing { get; } = new() { ["corpo"] = "", ["cabeca"] = "" };

    /// <summary>The equipment's general rules (<c>assets/data/equipamento/_regras.json</c>).</summary>
    public EquipSettings EquipData { get; private set; } = new();

    /// <summary>The weapons' own techniques (<c>assets/data/equipamento/_tecnicas_armas.json</c>).</summary>
    public Dictionary<string, WeaponTechniqueDef> WeaponTechs { get; private set; } = new();

    /// <summary>Weapon techniques found so far (ids).</summary>
    public HashSet<string> KnownWeaponTechs { get; } = new();

    /// <summary>Swings that hit something, per weapon drawing (the practice that reveals its techniques).</summary>
    public Dictionary<string, int> WeaponHits { get; } = new();

    private void LoadEquipmentData()
    {
        const string dir = ItemCatalog.EquipmentFolder;
        if (Godot.FileAccess.FileExists($"{dir}/_regras.json")) EquipData = EquipRules.ParseSettings(Godot.FileAccess.GetFileAsString($"{dir}/_regras.json"));
        if (Godot.FileAccess.FileExists($"{dir}/_tecnicas_armas.json")) WeaponTechs = EquipRules.ParseTechniques(Godot.FileAccess.GetFileAsString($"{dir}/_tecnicas_armas.json"));
    }

    /// <summary>The piece worn in <paramref name="slot"/> (the base robe for an empty body slot; null for a bare head).</summary>
    public ItemDef? WornIn(string slot)
    {
        string id = Wearing.TryGetValue(slot, out var w) ? w : "";
        if (id.Length > 0) return Catalog.Get(id);
        return slot == "corpo" && Catalog.Items.TryGetValue(EquipRules.BaseRobe, out var b) ? b : null;
    }

    /// <summary>True when <paramref name="id"/> is worn now (the base robe counts when nothing else is on the body).</summary>
    public bool IsWorn(string id)
    {
        var d = Catalog.Get(id);
        if (d.Wear == null) return false;
        return WornIn(d.Wear.Slot)?.Id == id;
    }

    /// <summary>Puts on <paramref name="id"/> (a robe or a hat), or takes it off when it is already worn. Returns true when something changed.</summary>
    public bool ToggleWear(string id)
    {
        var d = Catalog.Get(id);
        if (d.Wear == null || !Wearing.ContainsKey(d.Wear.Slot)) return false;
        string slot = d.Wear.Slot;
        if (d.Base) { if (Wearing[slot].Length == 0) return false; Wearing[slot] = ""; }      // back to the green tunic
        else if (Inventory.Count(id) <= 0) return false;
        else Wearing[slot] = Wearing[slot] == id ? "" : id;
        ApplyWear();
        var now = WornIn(slot);
        Hud.Toast(slot == "cabeca"
            ? (now == null ? "Tirou o chapéu." : $"Vestiu: {now.Name}.")
            : (Wearing[slot].Length == 0 ? $"De volta à {Catalog.Get(EquipRules.BaseRobe).Name}." : $"Vestiu: {now!.Name}."));
        return true;
    }

    /// <summary>Takes off what is in <paramref name="slot"/> (the robe goes back to the green tunic).</summary>
    public void Unwear(string slot) { if (Wearing.ContainsKey(slot)) { Wearing[slot] = ""; ApplyWear(); } }

    /// <summary>The look and the numbers of what is worn: the drawings on the body, the defense and the weight.</summary>
    public void ApplyWear()
    {
        foreach (var slot in EquipRules.Slots)
            if (Wearing[slot].Length > 0 && (Inventory.Count(Wearing[slot]) <= 0 || Catalog.Get(Wearing[slot]).Wear?.Slot != slot)) Wearing[slot] = ""; // lost or wrong: off
        Player.Outfit = WornIn("corpo")?.Wear?.Art is { Length: > 0 } a ? a : SpiritOfMurin.Elements.RoupaArts.Default;
        Player.Hat = WornIn("cabeca")?.Wear?.Art;
        ApplyStats();
    }

    /// <summary>Damage held back by the worn pieces (fraction).</summary>
    public float WornDefense => EquipRules.Defense(EquipRules.Slots.Select(WornIn), EquipData);

    /// <summary>Extra Vigor fraction of the worn pieces.</summary>
    public float WornWeight => EquipRules.Weight(EquipRules.Slots.Select(WornIn));

    // the weapon's drawing, numbers and techniques on the body (called by Equip)
    private void ApplyWeapon(ItemDef? d)
    {
        var w = d?.Weapon;
        Player.Blade = EquipRules.Blade(w);
        Player.WeaponArt = w?.Art is { Length: > 0 } art ? art : null;
        var wide = w == null ? null : WeaponTechs.Values.FirstOrDefault(t => t.Id == "meia_lua" && t.WeaponKind == w.Art && KnownWeaponTechs.Contains(t.Id));
        Player.WideCutDegrees = wide?.ArcDegrees ?? 0;
        Player.WideCutVigor = wide?.VigorExtra ?? 0;
        Player.WideCutTrail = wide?.Trail ?? 1;
    }

    // a swing of the weapon in hand hit something: practice toward its techniques
    private void PracticeWeapon()
    {
        string? art = Player.WeaponArt;
        if (art == null) return;
        WeaponHits[art] = WeaponHits.GetValueOrDefault(art) + 1;
        foreach (var t in EquipRules.Found(WeaponTechs, art, WeaponHits[art]).ToList())
        {
            if (!KnownWeaponTechs.Add(t.Id)) continue;
            Book.Add(new BookEntry { Name = t.Name, Kind = "técnica da arma", Key = $"com {Catalog.Get(EquippedItem).Name}: o 3º corte do combo", Where = $"Descoberta por você na Planície Verdejante, de tanto usar o sabre. {t.Description}", State = "Plena" });
            Hud.Toast($"Técnica descoberta: {t.Name}! {t.Description} (Livro de Técnicas)", important: true);
            ApplyWeapon(Catalog.Get(EquippedItem));
        }
    }

    // the traveller's bundle: the pieces it holds go to the bag
    private void OpenBundle()
    {
        var got = new List<string>();
        foreach (var id in EquipData.Trouxa) { AddItem(id); got.Add(Catalog.Get(id).Name); }
        Hud.Toast(Game.Bindings.F($"Uma trouxa de viajante esquecida: {string.Join(" e ", got)}. Abra a bolsa ([inventory]) para vestir e empunhar."), important: true);
        Sfx?.Play("pickup");
    }

    // the bag's lines for a piece to wear or a weapon: how it compares with what is on now (T18)
    private List<PlainsHud.BagLine> EquipCompareLines(ItemDef other)
    {
        var lines = new List<PlainsHud.BagLine>();
        if (other.Wear != null)
        {
            var now = WornIn(other.Wear.Slot);
            if (now?.Id == other.Id) { lines.Add(new(other.Wear.Slot == "cabeca" ? "Está na sua cabeça." : "Você está vestindo.", PlainsHud.BagLineKind.Head)); }
            else
            {
                lines.Add(new(now == null ? $"Cabeça descoberta → com {other.Name}:" : $"Vestindo: {now.Name} → se trocar:", PlainsHud.BagLineKind.Head));
                foreach (var c in EquipRules.CompareWear(now?.Wear, other.Wear)) lines.Add(Line(c));
            }
            if (other.Onde.Length > 0) lines.Add(new($"Onde se consegue: {other.Onde}", PlainsHud.BagLineKind.Plain));
            return lines;
        }
        if (other.Weapon != null && other.Id != EquippedItem)
        {
            var hand = EquippedItem.Length > 0 ? Catalog.Get(EquippedItem).Weapon : null;
            lines.Add(new(hand == null ? "Golpe — mãos vazias → com " + other.Name + ":" : $"Golpe — {Catalog.Get(EquippedItem).Name} → se trocar:", PlainsHud.BagLineKind.Head));
            foreach (var c in EquipRules.CompareWeapons(hand, other.Weapon, Stats.MeleeDamageMultiplier)) lines.Add(Line(c));
            var techs = other.Weapon.Techniques.Select(t => WeaponTechs.GetValueOrDefault(t)).Where(t => t != null).ToList();
            if (techs.Count > 0)
                lines.Add(new("Técnicas da arma: " + string.Join(", ", techs.Select(t => KnownWeaponTechs.Contains(t!.Id) ? t.Name : t.Playable ? "??? (descubra usando)" : "??? (ainda por descobrir)")), PlainsHud.BagLineKind.Plain));
        }
        else if (other.Weapon != null) lines.Add(new($"Alcance {other.Weapon.Reach:0} px · {other.Weapon.Damage} de dano por corte · combo de {other.Weapon.Chain}", PlainsHud.BagLineKind.Plain));
        return lines;

        static PlainsHud.BagLine Line(EquipRules.Diff c) => new($"{c.What}: {c.Now} → {c.After}", c.Better ? PlainsHud.BagLineKind.Better : c.Worse ? PlainsHud.BagLineKind.Worse : PlainsHud.BagLineKind.Plain);
    }

    // the T18 part of the journey: what is worn, the practice and the techniques found
    private void SnapshotEquipment(SaveData d)
    {
        d.Wearing = new Dictionary<string, string>(Wearing);
        d.WeaponHits = new Dictionary<string, int>(WeaponHits);
        d.WeaponTechniques = KnownWeaponTechs.OrderBy(t => t).ToList();
    }

    private void RestoreEquipment(SaveData? d)
    {
        foreach (var slot in EquipRules.Slots) Wearing[slot] = d?.Wearing != null && d.Wearing.TryGetValue(slot, out var id) ? id : "";
        WeaponHits.Clear(); KnownWeaponTechs.Clear();
        if (d != null) { foreach (var (k, v) in d.WeaponHits) WeaponHits[k] = v; foreach (var t in d.WeaponTechniques) KnownWeaponTechs.Add(t); }
        ApplyWear();
    }

    /// <summary>The bag's entries: the base robe first (it never leaves the bag), then the inventory.</summary>
    private IEnumerable<(string Id, int Count)> BagEntries()
    {
        if (Catalog.Items.TryGetValue(EquipRules.BaseRobe, out var b) && b.Base && Inventory.Count(b.Id) == 0) yield return (b.Id, 1);
        foreach (var e in Inventory.Entries) yield return e;
    }
}
