using SpiritOfMurin.Combat;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>The technique states and their two versions.</summary>
public static class TechniqueTests
{
    private static TechniqueData Data() => new()
    {
        UnlockLevel = 3,
        QiWave = new() { Name = "Disparo de Qi", Full = new() { QiCost = 30, Range = 210, Damage = 2 }, Weak = new() { QiCost = 45, VigorCost = 10, Range = 105, Damage = 1 } },
        SideStep = new() { Name = "Passo Lateral", Full = new() { Angle = 95 }, Weak = new() { QiCost = 15, VigorCost = 12, Angle = 55 } },
    };

    /// <summary>Unknown and sealed give no parameters; full and weakened give their own version.</summary>
    [UnitTest]
    public static void States_GiveTheRightVersion()
    {
        // Arrange
        var t = new TechniqueSet(Data());
        // Act / Assert
        Assert.True(t.Params("qi_wave") == null, "unknown: none");
        t.Set("qi_wave", TechniqueState.Sealed);
        Assert.True(t.Params("qi_wave") == null, "sealed: none");
        t.Set("qi_wave", TechniqueState.Full);
        Assert.Equal(210f, t.Params("qi_wave")!.Range, "full range");
        t.Set("qi_wave", TechniqueState.Weakened);
        var w = t.Params("qi_wave")!;
        Assert.True(w.Range < 210 && w.Damage < 2 && w.QiCost > 30 && w.VigorCost > 0, "weakened: shorter, weaker, dearer, costs vigor");
    }

    /// <summary>The weakened side step covers less ground and costs more Qi than the full one.</summary>
    [UnitTest]
    public static void WeakSideStep_LessGround_MoreQi()
    {
        // Arrange
        var d = Data();
        // Act / Assert
        Assert.True(d.SideStep.Weak.Angle < d.SideStep.Full.Angle && d.SideStep.Weak.QiCost > d.SideStep.Full.QiCost, "less ground, more Qi");
    }
}
