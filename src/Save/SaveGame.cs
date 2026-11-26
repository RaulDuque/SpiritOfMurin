using System;
using System.Collections.Generic;
using System.Text.Json;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Save;

/// <summary>One inventory line in the save.</summary>
public sealed class SavedItem
{
    /// <summary>Item id.</summary>
    public string Id { get; set; } = "";
    /// <summary>Quantity.</summary>
    public int Count { get; set; }
}

/// <summary>
/// The Planície journey as written when meditating (format 1 of the Godot game; snake_case JSON in
/// <c>user://</c>). It holds what the creator asked to keep: seed, meditation spot, what was collected
/// and hunted, inventory, equipment, Vida, Qi, vigor, level, points, attributes and proficiencies.
/// </summary>
public sealed class SaveData
{
    /// <summary>Current format.</summary>
    public const int CurrentFormat = 1;

    /// <summary>Always "spiritofmurin".</summary>
    public string Game { get; set; } = "spiritofmurin";
    /// <summary>Format version.</summary>
    public int Format { get; set; } = CurrentFormat;
    /// <summary>Region of the journey ("planicie").</summary>
    public string Region { get; set; } = "planicie";
    /// <summary>When it was written (ISO 8601, local time).</summary>
    public string SavedAt { get; set; } = "";
    /// <summary>World seed.</summary>
    public int Seed { get; set; }
    /// <summary>Meditation spot (world pixels): where Continuar wakes the character.</summary>
    public float[] Spot { get; set; } = { 0, 0 };
    /// <summary>Ids of collected objects.</summary>
    public List<string> Collected { get; set; } = new();
    /// <summary>Dead animals and where their carcass lies.</summary>
    public Dictionary<string, float[]> Dead { get; set; } = new();
    /// <summary>Carcass parts already taken ("creature#part").</summary>
    public List<string> PartsTaken { get; set; } = new();
    /// <summary>Trails already examined.</summary>
    public List<string> Trails { get; set; } = new();
    /// <summary>Inventory in pickup order.</summary>
    public List<SavedItem> Inventory { get; set; } = new();
    /// <summary>Weapon in hand ("none", "knife", "sword").</summary>
    public string Equipped { get; set; } = "none";
    /// <summary>Vida.</summary>
    public float Health { get; set; }
    /// <summary>Vigor.</summary>
    public float Vigor { get; set; }
    /// <summary>Qi.</summary>
    public float Qi { get; set; }
    /// <summary>Level.</summary>
    public int Level { get; set; } = 1;
    /// <summary>XP toward the next level.</summary>
    public int Xp { get; set; }
    /// <summary>Unspent attribute points.</summary>
    public int Points { get; set; }
    /// <summary>All XP ever gained.</summary>
    public int TotalXp { get; set; }
    /// <summary>Points invested per attribute.</summary>
    public Dictionary<string, int> Attributes { get; set; } = new();
    /// <summary>Practice per proficiency.</summary>
    public Dictionary<string, int> Practice { get; set; } = new();
    /// <summary>What already gave practice (so it never counts twice).</summary>
    public List<string> PracticeKeys { get; set; } = new();
    /// <summary>Recipes already made once (first-craft XP).</summary>
    public List<string> Crafted { get; set; } = new();
}

/// <summary>What reading the save found.</summary>
public enum SaveStatus { None, Ok, Damaged }

/// <summary>Writes and reads the journey file. Writing reads the file back and only reports success when it matches.</summary>
public static class SaveGame
{
    /// <summary>Default save path.</summary>
    public const string DefaultPath = "user://jornada_planicie.json";

    private static readonly JsonSerializerOptions Json = new(PlainsGenSettings.Json) { WriteIndented = true };

    /// <summary>Serialises a journey.</summary>
    public static string ToJson(SaveData d) => JsonSerializer.Serialize(d, Json);

    /// <summary>Parses a journey; null when the text is not a valid journey of the current format.</summary>
    public static SaveData? FromJson(string text)
    {
        try
        {
            var d = JsonSerializer.Deserialize<SaveData>(text, Json);
            return d is { Game: "spiritofmurin", Format: SaveData.CurrentFormat } && d.Spot.Length == 2 ? d : null;
        }
        catch (JsonException) { return null; }
    }

    /// <summary>Writes the journey and reads it back. Returns false when it could not be written or read back the same.</summary>
    public static bool Write(string path, SaveData d)
    {
        d.SavedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        string text = ToJson(d);
        using (var f = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write))
        {
            if (f == null) return false;
            if (!f.StoreString(text)) return false;
        }
        return Godot.FileAccess.FileExists(path) && Godot.FileAccess.GetFileAsString(path) == text;
    }

    /// <summary>Reads the journey: status and data (null unless Ok).</summary>
    public static (SaveStatus Status, SaveData? Data) Read(string path)
    {
        if (!Godot.FileAccess.FileExists(path)) return (SaveStatus.None, null);
        var d = FromJson(Godot.FileAccess.GetFileAsString(path));
        return d == null ? (SaveStatus.Damaged, null) : (SaveStatus.Ok, d);
    }

    /// <summary>Deletes the journey file (new game confirmed).</summary>
    public static void Erase(string path)
    {
        if (Godot.FileAccess.FileExists(path)) Godot.DirAccess.RemoveAbsolute(Godot.ProjectSettings.GlobalizePath(path));
    }
}
