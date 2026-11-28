using Godot;
using SpiritOfMurin.Creatures;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>T07: reading a print gets more precise with Rastreamento and never points the wrong way.</summary>
public static class TrackingTests
{
    /// <summary>For many prints and directions, every grade's sector contains the real direction, and grade 3 is narrower than grade 0.</summary>
    [UnitTest]
    public static void Sector_ContainsReal_NarrowsWithGrade()
    {
        // Arrange
        var s = new FaunaSettings();
        bool all = true;
        // Act
        for (int i = 0; i < 64; i++)
        {
            var real = Vector2.FromAngle(i * 0.37f);
            for (int g = 0; g <= 3; g++) all &= TrackingRules.Contains(TrackingRules.Sector(real, g, $"print#{i}", s), real);
        }
        var g0 = TrackingRules.Sector(Vector2.Right, 0, "p", s); var g3 = TrackingRules.Sector(Vector2.Right, 3, "p", s);
        // Assert
        Assert.True(all, "the real direction is always inside the hint");
        Assert.True(g3.Half < g0.Half / 4, $"grade 3 ({Mathf.RadToDeg(g3.Half * 2):0}°) much narrower than grade 0 ({Mathf.RadToDeg(g0.Half * 2):0}°)");
        Assert.True(TrackingRules.Words(TrackingRules.Sector(Vector2.Up, 0, "x", s), 0).StartsWith("mais ou menos"), "grade 0 is vague");
    }

    /// <summary>The prints follow the real path: one every spacing pixels, each pointing the way the animal went — also after a turn; a teleport leaves none.</summary>
    [UnitTest]
    public static void Stepper_FollowsPathAndTurns()
    {
        // Arrange: walk 60 px east, then 60 px south, in 1 px steps
        var st = new TrackStepper(14);
        var prints = new System.Collections.Generic.List<(Vector2 At, Vector2 Dir)>();
        Vector2 p = Vector2.Zero;
        // Act
        st.Step(p);
        for (int i = 0; i < 60; i++) { p += Vector2.Right; if (st.Step(p) is { } a) prints.Add(a); }
        for (int i = 0; i < 60; i++) { p += Vector2.Down; if (st.Step(p) is { } a) prints.Add(a); }
        var jump = st.Step(p + new Vector2(500, 0));
        // Assert
        Assert.True(prints.Count >= 7 && prints.Count <= 9, $"about 120/14 prints ({prints.Count})");
        Assert.True(prints[0].Dir.Dot(Vector2.Right) > 0.99f, "the first prints point east");
        Assert.True(prints[^1].Dir.Dot(Vector2.Down) > 0.99f, "after the turn they point south");
        Assert.True(jump == null, "a teleport restarts the trail without a print");
    }
}
