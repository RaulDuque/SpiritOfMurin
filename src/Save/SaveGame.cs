using System;
using System.Collections.Generic;
using System.Text.Json;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Vale;

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
/// and hunted, inventory, equipment, Vida, Ki, vigor, level, points, attributes and proficiencies.
/// </summary>
public sealed class SaveData
{
    /// <summary>Current format. Format 1 (Planície only, before the campaign) still loads, as a Planície journey.</summary>
    public const int CurrentFormat = 2;

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
    /// <summary>Ki.</summary>
    public float Ki { get; set; }
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

    // ---- the campaign (format 2) ----

    /// <summary>
    /// Campaign phase: "vale" (the prologue), "senior_won" (the Senior fell: Continuar resumes at the
    /// awakening, the safe point of the ending), "planicie" (after the awakening; also every format-1 save).
    /// </summary>
    public string Phase { get; set; } = "planicie";
    /// <summary>The prologue's story state (region "vale").</summary>
    public ValeState? Vale { get; set; }
    /// <summary>The prologue's items (region "vale"), lost at the awakening.</summary>
    public List<SavedItem> ValeInventory { get; set; } = new();
    /// <summary>The Livro de Técnicas (carried from the prologue into the Planície).</summary>
    public List<BookEntry> Book { get; set; } = new();
    /// <summary>True once the awakening took the prologue's equipment (it happens once).</summary>
    public bool LossApplied { get; set; }
    /// <summary>True once the weakened techniques came back at the unlock level (the message shows once).</summary>
    public bool TechniquesReturned { get; set; }
    /// <summary>Planície chunks explored (the map), as [x, y].</summary>
    public List<int[]> Explored { get; set; } = new();
    /// <summary>Animals already seen or tracked (T07): only these show on the map. Old saves: empty.</summary>
    public List<string> SeenAnimals { get; set; } = new();
    /// <summary>Manual map markers (world px), as [x, y].</summary>
    public List<float[]> Markers { get; set; } = new();
    /// <summary>The hour of the day (0-24) when the journey was written; -1 (older journeys) starts at the data's start hour.</summary>
    public float Hour { get; set; } = -1;
    /// <summary>The journey's money (Moedas). Journeys saved before it load with 0.</summary>
    public int Coins { get; set; }
    /// <summary>Work done on the world's elements by place id (trees cut, rocks broken; pedido 14). Older journeys: empty.</summary>
    public Dictionary<string, float> Worked { get; set; } = new();
    /// <summary>The item in hand (its id), so two axes are told apart; empty in older journeys (then <see cref="Equipped"/> decides).</summary>
    public string EquippedItem { get; set; } = "";
    /// <summary>T18: what is worn in each slot ("corpo", "cabeca" → item id; "" = the green tunic, the bare head). Null in older journeys.</summary>
    public Dictionary<string, string>? Wearing { get; set; }
    /// <summary>T18: swings that hit, per weapon drawing (the practice that reveals its techniques).</summary>
    public Dictionary<string, int> WeaponHits { get; set; } = new();
    /// <summary>T18: the weapon techniques found.</summary>
    public List<string> WeaponTechniques { get; set; } = new();
    /// <summary>Clues found in the Planície, oldest first (the map's "última pista").</summary>
    public List<SpiritOfMurin.Vale.Clue> Clues { get; set; } = new();
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
            var d = JsonSerializer.Deserialize<SaveData>(KiMigration.Migrate(text), Json); // journeys from before Qi → Ki
            if (d is not { Game: "spiritofmurin" } || d.Spot.Length != 2 || d.Format is < 1 or > SaveData.CurrentFormat) return null;
            if (d.Format == 1) { d.Region = "planicie"; d.Phase = "planicie"; d.Format = SaveData.CurrentFormat; } // migration: the journey stays in the Planície
            return d;
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
