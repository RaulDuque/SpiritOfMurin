using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>Deep water (pedido 14.3): Shift + Ki walks on it, otherwise swim; sinking needs Ki to rise again.</summary>
public static class WaterRulesTests
{
    private static readonly DeepWaterDef D = new() { WalkKiPerSecond = 10, SwimSpeed = 0.45f, RiseMinKi = 10 };

    /// <summary>On land nothing changes; on deep water without Shift the character swims and keeps the Ki.</summary>
    [UnitTest]
    public static void Land_None_DeepWithoutShift_Swims()
    {
        // Act
        var land = WaterRules.Step(false, true, 50, 0.1f, WaterState.None, D);
        var swim = WaterRules.Step(true, false, 50, 0.1f, WaterState.None, D);
        // Assert
        Assert.Equal(WaterState.None, land.State, "land");
        Assert.Equal(50f, land.Ki, "no Ki spent on land");
        Assert.Equal(WaterState.Swim, swim.State, "no Shift: swim");
        Assert.Equal(50f, swim.Ki, "swimming spends no Ki");
    }

    /// <summary>Shift with Ki walks on the water, spending 10 per second.</summary>
    [UnitTest]
    public static void Shift_WithKi_WalksAndSpends()
    {
        // Act
        var r = WaterRules.Step(true, true, 50, 0.5f, WaterState.None, D);
        // Assert
        Assert.Equal(WaterState.Walk, r.State, "walk");
        Assert.Equal(45f, r.Ki, "0,5 s × 10/s");
    }

    /// <summary>Running out of Ki sinks to swimming at 0; a little regenerated Ki does not lift the swimmer back.</summary>
    [UnitTest]
    public static void KiRunsOut_Sinks_AndNeedsRiseMinToWalkAgain()
    {
        // Act
        var sink = WaterRules.Step(true, true, 0.5f, 0.1f, WaterState.Walk, D);
        var stillSwim = WaterRules.Step(true, true, 9.9f, 0.1f, WaterState.Swim, D);
        var rises = WaterRules.Step(true, true, 10f, 0.1f, WaterState.Swim, D);
        // Assert
        Assert.Equal(WaterState.Swim, sink.State, "Ki ran out: swim");
        Assert.Equal(0f, sink.Ki, "Ki at 0");
        Assert.Equal(WaterState.Swim, stillSwim.State, "9,9 Ki is not enough to rise");
        Assert.Equal(WaterState.Walk, rises.State, "10 Ki rises again");
    }
}
