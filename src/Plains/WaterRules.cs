namespace SpiritOfMurin.Plains;

/// <summary>Where the character is with respect to deep water (pedido 14.3).</summary>
public enum WaterState { None, Walk, Swim }

/// <summary>
/// Deep water in the Planície (pedido 14.3, decided by the creator): holding Shift over deep water the
/// character walks on it, spending Ki every second; without Shift, or with no Ki left, they swim, slowly.
/// After sinking for lack of Ki, walking on the water again needs at least <see cref="DeepWaterDef.RiseMinKi"/>
/// (so the slow Ki regeneration does not make them bob up and down every frame). The prologue's river still
/// blocks (only the Planície uses this). Numbers in <c>assets/data/plains/plains_ambient.json</c> (<c>deep_water</c>).
/// </summary>
public static class WaterRules
{
    /// <summary>
    /// One frame: the state over the water and the Ki left. <paramref name="deep"/> is true when the feet are on
    /// deep water, <paramref name="shift"/> when the water-walk key (Shift) is held, <paramref name="now"/> the state of the last frame.
    /// </summary>
    public static (WaterState State, float Ki) Step(bool deep, bool shift, float ki, float dt, WaterState now, DeepWaterDef d)
    {
        if (!deep) return (WaterState.None, ki);
        bool canRise = now == WaterState.Swim ? ki >= d.RiseMinKi : ki > 0;
        if (!shift || !canRise) return (WaterState.Swim, ki);
        float left = ki - d.WalkKiPerSecond * dt;
        return left > 0 ? (WaterState.Walk, left) : (WaterState.Swim, 0);
    }
}

/// <summary>Deep water numbers (<c>deep_water</c> in plains_ambient.json).</summary>
public sealed class DeepWaterDef
{
    /// <summary>Ki spent per second while walking on the water.</summary>
    public float WalkKiPerSecond { get; set; } = 10;
    /// <summary>Speed factor while swimming.</summary>
    public float SwimSpeed { get; set; } = 0.45f;
    /// <summary>Ki needed to rise and walk on the water again after sinking for lack of Ki.</summary>
    public float RiseMinKi { get; set; } = 10;
}
