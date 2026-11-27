using SpiritOfMurin.Combat;
using SpiritOfMurin.Progression;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>FOR, CON and the critical hit (pedido 14), with the numbers of plains_progression.json and fixed rolls.</summary>
public static class DamageRulesTests
{
    private static void Near(float expected, float actual, string what) =>
        Assert.True(System.Math.Abs(expected - actual) < 1e-4f, $"{what}: expected {expected}, got {actual}");

    private static PlayerStats Sheet(string attr, int points)
    {
        var s = new PlayerStats(ProgressionTests.Data(), 10);
        s.Restore(1, 0, 0, 0, new System.Collections.Generic.Dictionary<string, int> { [attr] = points });
        return s;
    }

    /// <summary>Each FOR point adds 6% to melee damage; the Ki shots ignore FOR.</summary>
    [UnitTest]
    public static void For_RaisesMeleeOnly()
    {
        // Arrange
        var s = Sheet("for", 5);
        // Act
        var melee = DamageRules.Outgoing(10, 1, s.MeleeDamageMultiplier, 0, 1.5f, 0.99f);
        var ki = DamageRules.Outgoing(10, 1, 1, 0, 1.5f, 0.99f);
        // Assert
        Near(13f, melee.Amount, "10 × 1,30 with FOR 5");
        Near(10f, ki.Amount, "the Ki shot: no FOR");
    }

    /// <summary>A roll under the chance is critical (×1,5); at or over it, the hit is plain.</summary>
    [UnitTest]
    public static void Critical_ByRoll_MultipliesDamage()
    {
        // Arrange
        var s = Sheet("sor", 5); // 5% + 5 × 1% = 10%
        // Act
        var crit = DamageRules.Outgoing(4, 1, 1, s.CritChance, s.CritMultiplier, 0.09f);
        var plain = DamageRules.Outgoing(4, 1, 1, s.CritChance, s.CritMultiplier, 0.10f);
        // Assert
        Near(0.10f, s.CritChance, "chance with SOR 5");
        Assert.True(crit.Critical, "roll 0,09 < 0,10 is critical");
        Near(6f, crit.Amount, "4 × 1,5");
        Assert.True(!plain.Critical, "roll 0,10 is not critical");
        Near(4f, plain.Amount, "plain hit");
    }

    /// <summary>The critical chance stops at the cap (40%) however much SOR there is.</summary>
    [UnitTest]
    public static void CritChance_StopsAtCap()
    {
        // Act
        var s = Sheet("sor", 60);
        // Assert
        Near(0.40f, s.CritChance, "cap");
    }

    /// <summary>Each CON point takes 3% off the damage taken, down to the 45% cap.</summary>
    [UnitTest]
    public static void Con_ReducesDamageTaken_UpToCap()
    {
        // Arrange
        var five = Sheet("con", 5);
        var many = Sheet("con", 40);
        // Act
        float a = DamageRules.Incoming(10, five.DamageTakenMultiplier);
        float b = DamageRules.Incoming(10, many.DamageTakenMultiplier);
        // Assert
        Near(8.5f, a, "10 × 0,85 with CON 5");
        Near(5.5f, b, "10 × 0,55 at the cap");
    }

    /// <summary>The Reforço and FOR multiply together, before the critical.</summary>
    [UnitTest]
    public static void Reinforce_For_Critical_Stack()
    {
        // Act
        var r = DamageRules.Outgoing(10, 1.1f, 1.2f, 1, 1.5f, 0);
        // Assert
        Near(19.8f, r.Amount, "10 × 1,1 × 1,2 × 1,5");
    }

    /// <summary>Numbers on screen: whole values plain, fractions with one decimal and a comma.</summary>
    [UnitTest]
    public static void Format_WholeAndFraction()
    {
        // Assert
        Assert.Equal("150", DamageRules.Format(150), "whole");
        Assert.Equal("3,9", DamageRules.Format(3.94f), "fraction");
        Assert.Equal("1", DamageRules.Format(1.02f), "rounds to whole");
    }
}
