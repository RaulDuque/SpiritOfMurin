using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>The walk's diagonal (2026-11-27, the creator found it slow): 0.8 of the speed on each axis, straight unchanged.</summary>
public static class MovementTests
{
    /// <summary>Straight moves are unchanged; a keyboard diagonal (0.707, 0.707) becomes 0.8 on each axis.</summary>
    [UnitTest]
    public static void Diagonal_PointEightPerAxis_StraightUnchanged()
    {
        // Act
        var right = Player.DiagonalScale(Vector2.Right, 0.8f);
        var diag = Player.DiagonalScale(new Vector2(1, 1).Normalized(), 0.8f);
        // Assert
        Assert.True(right.IsEqualApprox(Vector2.Right), "straight unchanged");
        Assert.True(Mathf.Abs(diag.X - 0.8f) < 0.001f && Mathf.Abs(diag.Y - 0.8f) < 0.001f, $"diagonal {diag}");
        Assert.True(Mathf.Abs(diag.Length() / right.Length() - 1.131f) < 0.002f, "about 13% more distance");
    }
}
