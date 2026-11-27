using System.Linq;
using SpiritOfMurin.Elements;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>Working the world's elements (pedido 14, Entrega 5): the right tool, the work, the materials, SOR and the bonus.</summary>
public static class ResourceRulesTests
{
    // equal to assets/data/elements/rochas/rocha_quartzo.json at the time of writing (no file I/O)
    private static ElementDef Rock() => new()
    {
        Id = "rocha_quartzo", Family = "rocha", Toughness = 5, Tools = { "picareta" },
        Drops = { new DropDef { Item = "pedra_bruta", Min = 2, Max = 3 } },
        Bonus = { new BonusDef { Item = "quartzo", Chance = 0.12f, PerSor = 0.04f, Cap = 0.6f, Count = 1 } },
    };

    /// <summary>The wrong tool does no work; the right one adds its power; the last strike uses it up; after that nothing.</summary>
    [UnitTest]
    public static void Strike_RightTool_Works_WrongTool_Refused_ThenDepleted()
    {
        // Arrange
        var e = Rock();
        // Act
        var wrong = ResourceRules.Strike(e, 0, "machado", 1);
        var one = ResourceRules.Strike(e, 0, "picareta", 1);
        var strong = ResourceRules.Strike(e, 3, "picareta", 2);
        var after = ResourceRules.Strike(e, 5, "picareta", 1);
        // Assert
        Assert.Equal(WorkResult.WrongTool, wrong.Result, "axe on rock");
        Assert.Equal(0f, wrong.Work, "no work from the wrong tool");
        Assert.Equal(WorkResult.Worked, one.Result, "pickaxe works");
        Assert.Equal(1f, one.Work, "work 1");
        Assert.Equal(WorkResult.Depleted, strong.Result, "3 + 2 = 5: used up");
        Assert.Equal(WorkResult.AlreadyDepleted, after.Result, "a used-up rock takes no more");
    }

    /// <summary>A decorative element (toughness 0) is not workable by any tool.</summary>
    [UnitTest]
    public static void Decor_IsNotWorkable()
    {
        // Act
        var r = ResourceRules.Strike(new ElementDef { Family = "rocha", Toughness = 0 }, 0, "picareta", 1);
        // Assert
        Assert.Equal(WorkResult.NotWorkable, r.Result, "decor");
    }

    /// <summary>The same rock always gives the same: drops within min-max, fixed by seed and place.</summary>
    [UnitTest]
    public static void Yield_IsFixedForSeedAndPlace()
    {
        // Act
        var a = ResourceRules.Yield(Rock(), 20261126, "boulder@3,-1", 0);
        var b = ResourceRules.Yield(Rock(), 20261126, "boulder@3,-1", 0);
        int stones = a.First(x => x.Item == "pedra_bruta").Count;
        // Assert
        Assert.True(a.SequenceEqual(b), "same seed and place: same yield");
        Assert.True(stones is >= 2 and <= 3, $"2-3 stones, got {stones}");
    }

    /// <summary>
    /// SOR and the bonus, reproducible: over 400 fixed places, the share of quartz follows 12% + 4% per SOR point
    /// (within 4 points), and raising SOR never takes a quartz away from a place that had one.
    /// </summary>
    [UnitTest]
    public static void Sor_RaisesBonus_Monotonic_AndNearTheChance()
    {
        // Arrange
        var rock = Rock();
        var places = Enumerable.Range(0, 400).Select(i => $"boulder@{i % 20},{i / 20}").ToList();
        int Count(int sor) => places.Count(p => ResourceRules.Yield(rock, 777, p, sor).Any(x => x.Item == "quartzo"));
        // Act
        int s0 = Count(0), s5 = Count(5), s20 = Count(20);
        bool never = places.All(p => !ResourceRules.Yield(rock, 777, p, 0).Any(x => x.Item == "quartzo") || ResourceRules.Yield(rock, 777, p, 5).Any(x => x.Item == "quartzo"));
        // Assert
        Assert.True(System.Math.Abs(s0 / 400f - 0.12f) < 0.04f, $"SOR 0: {s0}/400 ≈ 12%");
        Assert.True(System.Math.Abs(s5 / 400f - 0.32f) < 0.04f, $"SOR 5: {s5}/400 ≈ 32%");
        Assert.True(System.Math.Abs(s20 / 400f - 0.60f) < 0.04f, $"SOR 20: {s20}/400 ≈ 60% (cap)");
        Assert.True(never, "more SOR never removes a bonus");
        Assert.Equal(0.32f, ResourceRules.BonusChance(rock.Bonus[0], 5), "chance with SOR 5");
    }

    /// <summary>The catalog picks by habitat weight with a fixed roll, and nothing where the family does not grow.</summary>
    [UnitTest]
    public static void Catalog_PicksByHabitatWeight()
    {
        // Arrange
        var c = new ElementCatalog();
        c.Add("{\"id\":\"pedra\",\"family\":\"rocha\",\"habitats\":{\"grass\":2}}");
        c.Add("{\"id\":\"rocha_quartzo\",\"family\":\"rocha\",\"habitats\":{\"grass\":1}}");
        // Act
        var low = c.Pick("rocha", "grass", 0.1f);
        var high = c.Pick("rocha", "grass", 0.9f);
        var none = c.Pick("rocha", "mud", 0.5f);
        // Assert
        Assert.Equal("pedra", low!.Id, "first 2/3 of the rolls");
        Assert.Equal("rocha_quartzo", high!.Id, "last 1/3");
        Assert.True(none == null, "no rock grows in mud in this catalog");
    }
}
