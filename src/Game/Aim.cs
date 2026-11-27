using SpiritOfMurin.Save;

namespace SpiritOfMurin.Game;

/// <summary>
/// How attacks are aimed without a locked target (pedido 13, item 7): by the mouse — the swordsman turns to
/// the cursor when a cut, a knife cut or a Qi shot starts — or by the direction the character faces, for
/// whoever plays with the keyboard only (Esc → Controles). With a target locked, the target always wins.
/// Saved in the preferences (<c>aim_mouse</c>).
/// </summary>
public static class Aim
{
    private static bool? _mouse;

    /// <summary>True when attacks aim at the cursor (the default).</summary>
    public static bool ByMouse
    {
        get => _mouse ??= Prefs.Load().AimMouse;
        set { _mouse = value; var p = Prefs.Load(); p.AimMouse = value; p.Save(); }
    }

    /// <summary>Sets the aim for this run only, without saving (checks written for aiming by facing).</summary>
    public static void UseForSession(bool byMouse) => _mouse = byMouse;

    /// <summary>Forgets the cached value (tests that switch preference files).</summary>
    public static void Reload() => _mouse = null;
}
