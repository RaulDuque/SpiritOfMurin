using SpiritOfMurin.Combat;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>The technique states and their two versions.</summary>
public static class TechniqueTests
{
    private static TechniqueData Data() => new()
    {
        UnlockLevel = 3,
        KiWave = new() { Name = "Disparo de Ki", Full = new() { KiCost = 30, Range = 210, Damage = 2 }, Weak = new() { KiCost = 45, VigorCost = 10, Range = 105, Damage = 1 } },
        SideStep = new() { Name = "Passo Lateral", Full = new() { Angle = 95 }, Weak = new() { KiCost = 15, VigorCost = 12, Angle = 55 } },
    };

    /// <summary>Unknown and sealed give no parameters; full and weakened give their own version.</summary>
    [UnitTest]
    public static void States_GiveTheRightVersion()
    {
        // Arrange
        var t = new TechniqueSet(Data());
        // Act / Assert
        Assert.True(t.Params("ki_wave") == null, "unknown: none");
        t.Set("ki_wave", TechniqueState.Sealed);
        Assert.True(t.Params("ki_wave") == null, "sealed: none");
        t.Set("ki_wave", TechniqueState.Full);
        Assert.Equal(210f, t.Params("ki_wave")!.Range, "full range");
        t.Set("ki_wave", TechniqueState.Weakened);
        var w = t.Params("ki_wave")!;
        Assert.True(w.Range < 210 && w.Damage < 2 && w.KiCost > 30 && w.VigorCost > 0, "weakened: shorter, weaker, dearer, costs vigor");
    }

    /// <summary>The weakened side step covers less ground and costs more Ki than the full one.</summary>
    [UnitTest]
    public static void WeakSideStep_LessGround_MoreKi()
    {
        // Arrange
        var d = Data();
        // Act / Assert
        Assert.True(d.SideStep.Weak.Angle < d.SideStep.Full.Angle && d.SideStep.Weak.KiCost > d.SideStep.Full.KiCost, "less ground, more Ki");
    }
}
