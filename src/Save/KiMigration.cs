namespace SpiritOfMurin.Save;

/// <summary>
/// The energy was called "Qi" until 2026-11-27 and is "Ki" from then on (pedido 14). Journeys and
/// preferences written before the rename keep the old spelling in their keys and ids (<c>"qi": 100</c>,
/// the attribute <c>"qi"</c>, the techniques <c>"qi_wave"</c>, <c>"qi_charge"</c>, <c>"qi_reinforce"</c>,
/// the Livro entry "Disparo de Qi", the control <c>"qi"</c>). This rewrites them exactly the way the
/// code was rewritten (Qi → Ki, qi → ki, QI → KI), so an old file reads as if it had been written today.
/// It is idempotent: a file already in the new spelling comes back unchanged.
/// </summary>
public static class KiMigration
{
    /// <summary>True when the text still has the old spelling somewhere.</summary>
    public static bool IsOld(string text) => text.Contains("qi") || text.Contains("Qi") || text.Contains("QI");

    /// <summary>The text with the old spelling replaced (same rule as the code rename).</summary>
    public static string Migrate(string text) =>
        IsOld(text) ? text.Replace("Qi", "Ki").Replace("qi", "ki").Replace("QI", "KI") : text;
}
