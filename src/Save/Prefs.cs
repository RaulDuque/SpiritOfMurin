using System.Linq;
using System.Text.Json;
using Godot;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Save;

/// <summary>
/// The player's preferences, separate from the journey (<c>user://preferencias.json</c>): whether the
/// prologue's ending was already seen (it can then be skipped), full screen, and the ground line under a
/// charge. Tests point it elsewhere with <c>--prefs=user://file.json</c>.
/// </summary>
public sealed class Prefs
{
    /// <summary>Default path.</summary>
    public const string DefaultPath = "user://preferencias.json";
    /// <summary>The ending of the prologue was seen once (it can be skipped from then on).</summary>
    public bool FinaleSeen { get; set; }
    /// <summary>Full screen.</summary>
    public bool Fullscreen { get; set; }
    /// <summary>Show the line on the ground along a fixed charge (an aid; can be reduced).</summary>
    public bool ChargeLine { get; set; } = true;
    /// <summary>Quick-use bindings: slot (1-4) → item id.</summary>
    public System.Collections.Generic.Dictionary<string, string> QuickSlots { get; set; } = new();

    /// <summary>The path in use (command line or default).</summary>
    public static string Path => OS.GetCmdlineUserArgs().Concat(OS.GetCmdlineArgs()).FirstOrDefault(a => a.StartsWith("--prefs="))?["--prefs=".Length..] ?? DefaultPath;

    /// <summary>Reads the preferences (defaults when absent or damaged).</summary>
    public static Prefs Load()
    {
        try { return FileAccess.FileExists(Path) ? JsonSerializer.Deserialize<Prefs>(FileAccess.GetFileAsString(Path), PlainsGenSettings.Json) ?? new Prefs() : new Prefs(); }
        catch (JsonException) { return new Prefs(); }
    }

    /// <summary>Writes the preferences.</summary>
    public void Save()
    {
        using var f = FileAccess.Open(Path, FileAccess.ModeFlags.Write);
        f?.StoreString(JsonSerializer.Serialize(this, PlainsGenSettings.Json));
    }
}
