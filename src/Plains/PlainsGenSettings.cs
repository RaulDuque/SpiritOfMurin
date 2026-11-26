using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpiritOfMurin.Plains;

/// <summary>Per-tile chances (0-1) of each object in one habitat.</summary>
public sealed class HabitatChances
{
    /// <summary>Flint lying on the ground.</summary>
    public float Flint { get; set; }
    /// <summary>A hard branch lying on the ground.</summary>
    public float Branch { get; set; }
    /// <summary>A pine tree.</summary>
    public float Pine { get; set; }
    /// <summary>A lone boulder (may carry a herb beside it).</summary>
    public float Boulder { get; set; }
    /// <summary>A bush.</summary>
    public float Bush { get; set; }
    /// <summary>A rabbit burrow, with its rabbit.</summary>
    public float Burrow { get; set; }
    /// <summary>A patch of earth the boar rooted up (may carry a bitter root).</summary>
    public float Rooted { get; set; }
    /// <summary>A boar's home spot.</summary>
    public float Boar { get; set; }
}

/// <summary>
/// The Planície generation rules, read from <c>res://assets/data/plains/plains_worldgen.json</c>
/// (snake_case keys: this class is the wire-format boundary). Distances are in 32 px tiles.
/// </summary>
public sealed class PlainsGenSettings
{
    /// <summary>Default path of the data file.</summary>
    public const string DefaultPath = "res://assets/data/plains/plains_worldgen.json";

    /// <summary>Side of a chunk in tiles.</summary>
    public int ChunkTiles { get; set; } = 16;
    /// <summary>Tile size in pixels.</summary>
    public int TileSize { get; set; } = 32;
    /// <summary>Chunks loaded around the player's chunk (Chebyshev distance).</summary>
    public int LoadRadius { get; set; } = 2;
    /// <summary>Chunks farther than this from the player's chunk are freed.</summary>
    public int UnloadRadius { get; set; } = 3;
    /// <summary>Radius of the empty waking clearing, in tiles.</summary>
    public float ClearingRadius { get; set; } = 3.5f;
    /// <summary>How far east of the start the creek runs, in tiles.</summary>
    public float CreekOffset { get; set; } = 7f;
    /// <summary>Half width of the creek water, in tiles.</summary>
    public float CreekHalfWidth { get; set; } = 1.2f;
    /// <summary>Width of the bank on each side of the water, in tiles.</summary>
    public float BankWidth { get; set; } = 3f;
    /// <summary>Amplitude of the creek's long meander, in tiles.</summary>
    public float CreekMeander1 { get; set; } = 5f;
    /// <summary>Frequency of the long meander (per tile).</summary>
    public float CreekFreq1 { get; set; } = 0.045f;
    /// <summary>Amplitude of the short meander, in tiles.</summary>
    public float CreekMeander2 { get; set; } = 2f;
    /// <summary>Frequency of the short meander (per tile).</summary>
    public float CreekFreq2 { get; set; } = 0.13f;
    /// <summary>Distance between fords along the creek, in tiles.</summary>
    public int FordSpacing { get; set; } = 28;
    /// <summary>Length of each ford, in tiles.</summary>
    public int FordLength { get; set; } = 3;
    /// <summary>Frequency of the habitat noise.</summary>
    public float HabitatScale { get; set; } = 0.07f;
    /// <summary>Noise value above which a tile is mud.</summary>
    public float MudThreshold { get; set; } = 0.72f;
    /// <summary>Noise value above which a tile is bush field.</summary>
    public float BushThreshold { get; set; } = 0.66f;
    /// <summary>Minimum distance of the guaranteed habitats from the start, in tiles.</summary>
    public float AnchorDistanceMin { get; set; } = 12f;
    /// <summary>Maximum distance of the guaranteed habitats from the start, in tiles.</summary>
    public float AnchorDistanceMax { get; set; } = 16f;
    /// <summary>Radius of the guaranteed habitat blobs, in tiles.</summary>
    public float AnchorRadius { get; set; } = 6.5f;
    /// <summary>Direction of the guaranteed bush field (degrees; 0 = east, 90 = south).</summary>
    public float BushAnchorAngle { get; set; } = 215f;
    /// <summary>Direction of the guaranteed mud (degrees; 0 = east, 90 = south).</summary>
    public float MudAnchorAngle { get; set; } = 145f;
    /// <summary>Seeded variation of both anchor angles, ± degrees.</summary>
    public float AnchorAngleJitter { get; set; } = 20f;
    /// <summary>Object chances per habitat: keys bank, grass, bushes, mud.</summary>
    public Dictionary<string, HabitatChances> PerHabitat { get; set; } = new();
    /// <summary>Chance of a Grama Sangue-de-Coelho beside each boulder.</summary>
    public float HerbNearBoulder { get; set; } = 0.75f;
    /// <summary>Chance of a Raiz Amarga in each rooted patch.</summary>
    public float RootInRooted { get; set; } = 0.8f;
    /// <summary>Chance that an animal away from the start leaves a trail of prints.</summary>
    public float TrailChance { get; set; } = 0.6f;
    /// <summary>Shortest random trail, in tiles.</summary>
    public float TrailMinTiles { get; set; } = 4f;
    /// <summary>Longest random trail, in tiles.</summary>
    public float TrailMaxTiles { get; set; } = 7f;
    /// <summary>Pixels between prints.</summary>
    public float TrailSpacing { get; set; } = 22f;

    /// <summary>The JSON options for the data files: snake_case keys, comments and a "_doc" block ignored.</summary>
    public static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        ReadCommentHandling = JsonCommentHandling.Skip,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>Parses settings from JSON text.</summary>
    public static PlainsGenSettings Parse(string json) => JsonSerializer.Deserialize<PlainsGenSettings>(json, Json) ?? new PlainsGenSettings();

    /// <summary>Loads settings from a Godot path (res:// or user://).</summary>
    public static PlainsGenSettings Load(string path = DefaultPath) => Parse(Godot.FileAccess.GetFileAsString(path));

    /// <summary>The chances for a habitat, or zero chances when the data file leaves it out.</summary>
    public HabitatChances For(Habitat h) => PerHabitat.TryGetValue(h switch
    {
        Habitat.Bank => "bank", Habitat.Bushes => "bushes", Habitat.Mud => "mud", _ => "grass",
    }, out var c) ? c : new HabitatChances();
}
