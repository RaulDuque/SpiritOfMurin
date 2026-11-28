using System.Linq;
using SpiritOfMurin.Items;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>T18: the equipment's rules — defense and weight (decision 1), the blade's numbers, the bag's comparison and the techniques found by practice (decision 2).</summary>
public static class EquipRulesTests
{
    // equal to the data at the time of writing (no file I/O)
    private static ItemDef Robe(float def, float weight) => new() { Wear = new WearDef { Slot = "corpo", Art = "x", Defense = def, Weight = weight } };
    private static ItemDef Hat(float def, float weight) => new() { Wear = new WearDef { Slot = "cabeca", Art = "y", Defense = def, Weight = weight } };
    private static WeaponDef Sword() => new() { Art = "espada", Reach = 34, Damage = 2, Chain = 3, Startup = 0.07f, Active = 0.08f, Recovery = 0.24f, CancelAt = 0.13f, Lunge = 8, ThirdExtra = 0.03f, ThirdRecovery = 0.1f, ThirdCancel = 0.07f, ThirdLunge = 14, ThirdMult = 2 };
    private static WeaponDef Sabre() => new() { Art = "sabre_ferro", Reach = 38, Damage = 2, Chain = 3, Startup = 0.08f, Active = 0.09f, Recovery = 0.26f, CancelAt = 0.14f, Lunge = 8, ThirdMult = 2 };

    /// <summary>The traveller's tunic and the hat add their defense and weight; nothing worn adds nothing; the cap holds.</summary>
    [UnitTest]
    public static void DefenseAndWeight_AddUpAndCap()
    {
        // Arrange
        var s = new EquipSettings { DefenseCap = 0.4f };
        // Act
        float d = EquipRules.Defense(new[] { Robe(0.08f, 0.1f), Hat(0.03f, 0.02f) }, s), w = EquipRules.Weight(new[] { Robe(0.08f, 0.1f), Hat(0.03f, 0.02f) });
        // Assert
        Assert.True(System.Math.Abs(d - 0.11f) < 1e-5, $"defense 0.11, got {d}");
        Assert.True(System.Math.Abs(w - 0.12f) < 1e-5, $"weight 0.12, got {w}");
        Assert.Equal(0f, EquipRules.Defense(new ItemDef?[] { null, new ItemDef() }, s), "nothing worn holds nothing back");
        Assert.Equal(0.4f, EquipRules.Defense(new[] { Robe(0.3f, 0), Hat(0.3f, 0) }, s), "capped at defense_cap");
    }

    /// <summary>CON's multiplier comes first, the clothes' after; the weight raises a cut's and a step's Vigor.</summary>
    [UnitTest]
    public static void DamageAndVigor_FollowTheClothes()
    {
        // Act / Assert
        Assert.True(System.Math.Abs(EquipRules.DamageTaken(0.9f, 0.1f) - 0.81f) < 1e-5, "0.9 × (1 − 0.1)");
        Assert.Equal(1f, EquipRules.DamageTaken(1, 0), "no CON, nothing worn: full damage");
        Assert.True(System.Math.Abs(EquipRules.VigorCost(10, 0.12f) - 11.2f) < 1e-4, "10 vigor × 1.12");
        Assert.Equal(10f, EquipRules.VigorCost(10, 0), "the green tunic weighs nothing");
    }

    /// <summary>The prologue's sword as data has the tuning's numbers, so the body cuts exactly as before T18.</summary>
    [UnitTest]
    public static void SwordData_GivesTheOldStrike()
    {
        // Act
        var b = EquipRules.Blade(Sword())!;
        // Assert
        Assert.Equal(34f, b.Reach, "reach = SwordReach");
        Assert.Equal(2, b.Damage, "damage = SwordDamage");
        Assert.Equal(3, b.Chain, "three cuts");
        Assert.Equal(2, b.ThirdMult, "the third cut doubles");
        Assert.Equal(8f, EquipRules.ChainDamage(Sword()), "2 + 2 + 4");
        Assert.True(EquipRules.Blade(null) == null, "empty hands: no blade numbers");
    }

    /// <summary>The bag compares the sabre with the sword in hand: more reach, same damage, a slower cut; with empty hands everything is better.</summary>
    [UnitTest]
    public static void CompareWeapons_ShowsReachAndTime()
    {
        // Act
        var l = EquipRules.CompareWeapons(Sword(), Sabre());
        var empty = EquipRules.CompareWeapons(null, Sabre());
        // Assert
        var reach = l.First(x => x.What.StartsWith("Alcance"));
        Assert.True(reach.Now == "34" && reach.After == "38" && reach.Better, "reach 34 → 38, better");
        var dmg = l.First(x => x.What == "Dano por corte");
        Assert.True(!dmg.Better && !dmg.Worse, "same damage is neither");
        Assert.True(l.First(x => x.What.StartsWith("Tempo")).Worse, "the sabre's cut is slower");
        Assert.True(empty.All(x => x.Better && x.Now == "—"), "from empty hands every line improves");
    }

    /// <summary>A piece to wear compares defense and weight with what is worn (null = bare head).</summary>
    [UnitTest]
    public static void CompareWear_DefenseUpWeightUp()
    {
        // Act
        var l = EquipRules.CompareWear(new WearDef(), Robe(0.08f, 0.1f).Wear!);
        // Assert
        Assert.True(l[0].Now == "0%" && l[0].After == "8%" && l[0].Better, "defense 0% → 8%");
        Assert.True(l[1].Now == "+0%" && l[1].After == "+10%" && l[1].Worse, "weight +0% → +10%, worse");
        Assert.Equal("0%", EquipRules.CompareWear(null, Hat(0.03f, 0.02f).Wear!)[0].Now, "bare head");
    }

    /// <summary>The Meia-Lua is found after its count of hits with the sabre; future techniques never are; other weapons do not count.</summary>
    [UnitTest]
    public static void Techniques_FoundByPractice()
    {
        // Arrange
        var all = EquipRules.ParseTechniques("""
            { "_doc": {}, "meia_lua": { "name": "Meia-Lua", "weapon_kind": "sabre_ferro", "state": "jogavel", "discover": { "cuts": 12 }, "arc_degrees": 120 },
              "saque_viajante": { "name": "Saque", "weapon_kind": "sabre_ferro", "state": "futura", "discover": { "cuts": 0 } } }
            """);
        // Act / Assert
        Assert.Equal(2, all.Count, "the _doc key is skipped");
        Assert.Equal(0, EquipRules.Found(all, "sabre_ferro", 11).Count(), "11 hits: not yet");
        Assert.Equal("meia_lua", EquipRules.Found(all, "sabre_ferro", 12).Single().Id, "12 hits: the Meia-Lua");
        Assert.Equal(0, EquipRules.Found(all, "espada", 99).Count(), "the sword does not reveal the sabre's techniques");
        Assert.Equal(120f, all["meia_lua"].ArcDegrees, "the sweep's half angle");
    }

    /// <summary>A piece of equipment's file becomes an item: a weapon goes in the hand, a robe is worn, the category defaults.</summary>
    [UnitTest]
    public static void ParseEquipment_SetsEquipAndCategory()
    {
        // Act
        var sabre = ItemCatalog.ParseEquipment("""{ "id": "sabre_ferro", "name": "Sabre", "weapon": { "art": "sabre_ferro", "kind": "longa", "reach": 38 } }""");
        var knife = ItemCatalog.ParseEquipment("""{ "id": "faca_pedra", "equip": "knife", "weapon": { "kind": "curta" } }""");
        var robe = ItemCatalog.ParseEquipment("""{ "id": "tunica_viajante", "wear": { "slot": "corpo", "art": "tunica_viajante", "defense": 0.08 } }""");
        // Assert
        Assert.True(sabre.Equip == "weapon" && sabre.Category == "equipamento" && sabre.Weapon!.Reach == 38, "the sabre is a weapon");
        Assert.Equal("knife", knife.Equip, "the knife keeps its own equip (the T06 tool behaviour)");
        Assert.True(robe.Equip == "wear" && robe.Wear!.Slot == "corpo", "the robe is worn on the body");
    }
}
