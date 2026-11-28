using System.Linq;
using SpiritOfMurin.Elements;
using SpiritOfMurin.Items;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>T06: the bag's tabs, "tenho X de Y", tools compared in strikes, gains that add up, the choice of target.</summary>
public static class BagRulesTests
{
    // equal to the data at the time of writing (no file I/O)
    private static ElementDef Pine() => new() { Name = "Pinheiro", Family = "arvore", Toughness = 6, Tools = { "machado" } };
    private static ElementDef Bamboo() => new() { Name = "Touceira de bambu", Family = "bambu", Toughness = 3, Tools = { "faca", "machado" } };
    private static ElementDef Rock() => new() { Name = "Rocha com veio de quartzo", Family = "rocha", Toughness = 5, Tools = { "picareta" } };
    private static readonly ToolDef Knife = new() { Kind = "faca", Power = 1 }, Axe = new() { Kind = "machado", Power = 1, Reach = 22 }, Polished = new() { Kind = "machado", Power = 2, Reach = 24 };

    /// <summary>"Tudo" shows every item; a tab only its category; "Fabricação" none; no category reads as material.</summary>
    [UnitTest]
    public static void Tabs_FilterByCategory()
    {
        // Arrange
        var axe = new ItemDef { Category = "ferramenta" }; var flint = new ItemDef();
        // Assert
        Assert.True(BagRules.InTab(axe, "tudo") && BagRules.InTab(axe, "ferramenta") && !BagRules.InTab(axe, "material"), "axe");
        Assert.True(BagRules.InTab(flint, "material") && !BagRules.InTab(flint, "fabricacao"), "flint without category is a material");
        Assert.Equal("fabricacao", BagRules.Tabs[^1].Id, "Fabricação is the last tab");
    }

    /// <summary>A recipe's ingredients read "Sílex: tenho 2 de 3", with the shortfall flagged.</summary>
    [UnitTest]
    public static void Needs_HaveOfNeeded()
    {
        // Arrange
        var c = new ItemCatalog(); c.Items["silex"] = new ItemDef { Id = "silex", Name = "Sílex" }; c.Items["galho_rijo"] = new ItemDef { Id = "galho_rijo", Name = "Galho Rijo" };
        var r = new RecipeDef { Inputs = { ["silex"] = 3, ["galho_rijo"] = 1 } };
        // Act
        var n = BagRules.Needs(r, id => id == "silex" ? 2 : 1, c);
        // Assert
        Assert.Equal("Sílex: tenho 2 de 3", n[0].Line, "flint line");
        Assert.True(!n[0].Ok && n[1].Ok, "flint short, branch fine");
    }

    /// <summary>The polished axe against the stone one: the pine 6 → 3 strikes (better), the knife cannot fell it.</summary>
    [UnitTest]
    public static void CompareTools_InStrikes()
    {
        // Act
        var pol = BagRules.CompareTools(Axe, 22, 1.06f, Polished, 24, 1.06f, new[] { Pine(), Bamboo(), Rock() });
        var knife = BagRules.CompareTools(Axe, 22, 1.06f, Knife, 22, 1.06f, new[] { Pine(), Bamboo(), Rock() });
        // Assert
        var p = pol.First(x => x.What == "Pinheiro");
        Assert.True(p.Hand == "6 golpes" && p.Other == "3 golpes" && p.Better, $"pine {p.Hand} → {p.Other}");
        Assert.True(pol.Any(x => x.What == "Alcance" && x.Other == "24 px"), "reach shown when it changes");
        Assert.True(!pol.Any(x => x.What.StartsWith("Rocha")), "a rock neither axe works is left out");
        var kp = knife.First(x => x.What == "Pinheiro");
        Assert.True(kp.Other == "não corta" && kp.Worse, "the knife cannot fell the pine");
        Assert.True(knife.First(x => x.What == "Touceira de bambu").Other == "3 golpes", "the knife cuts bamboo in 3");
    }

    /// <summary>The same gain within the window adds up into one line; after it, a new line starts.</summary>
    [UnitTest]
    public static void GainLog_AddsUpWithinWindow()
    {
        // Arrange
        var g = new GainLog { Window = 2.5f };
        // Act
        g.Add("item:galho_rijo", "Galho Rijo", 1, 0); g.Add("item:galho_rijo", "Galho Rijo", 1, 1); string t = g.Add("item:galho_rijo", "Galho Rijo", 1, 2);
        g.Add("xp", "XP", 3, 2);
        string later = g.Add("item:galho_rijo", "Galho Rijo", 1, 6);
        // Assert
        Assert.Equal("+3 Galho Rijo", t, "three branches in a row");
        Assert.Equal("+1 Galho Rijo", later, "after the window, a new line");
    }

    /// <summary>The nearest is chosen; the wheel moves; a choice stays while within reach; leaving it goes back to the nearest.</summary>
    [UnitTest]
    public static void TargetChoice_WheelAndKeep()
    {
        // Arrange
        var c = new TargetChoice();
        // Act / Assert
        c.Update(new[] { "erva", "pegada" }); Assert.Equal("erva", c.Chosen, "nearest first");
        c.Step(1); Assert.Equal("pegada", c.Chosen, "wheel: next");
        c.Update(new[] { "erva", "carcaca", "pegada" }); Assert.Equal("pegada", c.Chosen, "the choice stays while within reach");
        c.Update(new[] { "erva", "carcaca" }); Assert.Equal("erva", c.Chosen, "gone: back to the nearest");
        c.Step(-1); Assert.Equal("carcaca", c.Chosen, "wheel back wraps around");
    }
}
