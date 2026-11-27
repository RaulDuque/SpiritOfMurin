using System.Linq;
using System.Text.Json;
using Godot;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Save;

/// <summary>
/// The player's preferences, separate from the journey (<c>user://preferencias.json</c>): whether the
/// prologue's ending was already seen (it can then be skipped), full screen, the ground line under a
/// charge, the assist mode and the controls. Tests point it elsewhere with <c>--prefs=user://file.json</c>.
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
    /// <summary>Aim attacks at the mouse cursor (true) or along the character's facing (keyboard only).</summary>
    public bool AimMouse { get; set; } = true;
    /// <summary>Assist mode: show the combat areas (guard arc, live blade, announced attacks).</summary>
    public bool AssistAreas { get; set; }
    /// <summary>Assist mode: show the damage numbers of each hit, dealt and taken.</summary>
    public bool AssistNumbers { get; set; }
    /// <summary>Controls: action → [main input, second input] ("k:physical keycode", "m:mouse button" or "").</summary>
    public System.Collections.Generic.Dictionary<string, string[]> Bindings { get; set; } = new();
    /// <summary>Quick-use bindings: slot (1-4) → item id.</summary>
    public System.Collections.Generic.Dictionary<string, string> QuickSlots { get; set; } = new();

    /// <summary>The path in use (command line or default).</summary>
    public static string Path
    {
        get
        {
            var args = OS.GetCmdlineUserArgs().Concat(OS.GetCmdlineArgs()).ToArray();
            string? given = args.FirstOrDefault(a => a.StartsWith("--prefs="))?["--prefs=".Length..];
            if (given != null) return given;
            // a check scene never reads or writes the player's own preferences (controls, assist, full screen)
            return args.Any(a => a.StartsWith("res://tests/")) ? "user://teste_padrao_prefs.json" : DefaultPath;
        }
    }

    /// <summary>Reads the preferences (defaults when absent or damaged).</summary>
    public static Prefs Load()
    {
        // preferences from before Qi → Ki keep the control "qi": read it as "ki"
        try { return FileAccess.FileExists(Path) ? JsonSerializer.Deserialize<Prefs>(KiMigration.Migrate(FileAccess.GetFileAsString(Path)), PlainsGenSettings.Json) ?? new Prefs() : new Prefs(); }
        catch (JsonException) { return new Prefs(); }
    }

    /// <summary>Writes the preferences.</summary>
    public void Save()
    {
        using var f = FileAccess.Open(Path, FileAccess.ModeFlags.Write);
        f?.StoreString(JsonSerializer.Serialize(this, PlainsGenSettings.Json));
    }
}
