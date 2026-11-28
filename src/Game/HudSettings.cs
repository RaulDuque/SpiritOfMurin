using System.Text.Json;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Game;

/// <summary>
/// The interface's times (T06), from <c>res://assets/data/plains/plains_hud.json</c>: how long a gain stays over the
/// head (and keeps adding up), how long a plain message and an important one stay on screen.
/// </summary>
public sealed class HudSettings
{
    /// <summary>Default data path.</summary>
    public const string DefaultPath = "res://assets/data/plains/plains_hud.json";
    /// <summary>Seconds a gain shows over the head and keeps adding up.</summary>
    public float GainSeconds { get; set; } = 2.5f;
    /// <summary>Seconds a plain message stays.</summary>
    public float ToastSeconds { get; set; } = 3.2f;
    /// <summary>Seconds an important message stays (level up, a recipe that became possible).</summary>
    public float ImportantSeconds { get; set; } = 6f;

    private static HudSettings? _current;
    /// <summary>The settings in use (read once).</summary>
    public static HudSettings Current => _current ??= Load();

    /// <summary>Parses the settings.</summary>
    public static HudSettings Parse(string json) => JsonSerializer.Deserialize<HudSettings>(json, PlainsGenSettings.Json) ?? new HudSettings();

    /// <summary>Loads the settings (defaults when the file is missing).</summary>
    public static HudSettings Load(string path = DefaultPath) => Godot.FileAccess.FileExists(path) ? Parse(Godot.FileAccess.GetFileAsString(path)) : new HudSettings();
}
