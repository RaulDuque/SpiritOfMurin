using System.Text.Json;
using Godot;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Creatures;

/// <summary>
/// How the fauna and the fights read (T07), from <c>res://assets/data/plains/plains_fauna.json</c>: how long a fresh
/// print stays, how precise tracking gets per grade, the parry's nudge, the boar's flinch, the rabbit's dive, how much
/// of the Lâmina gets through the hide, when the bar beside an animal shows.
/// </summary>
public sealed class FaunaSettings
{
    /// <summary>Default data path.</summary>
    public const string DefaultPath = "res://assets/data/plains/plains_fauna.json";
    /// <summary>Seconds a fresh print stays (fading).</summary>
    public float TrackSeconds { get; set; } = 90;
    /// <summary>Px between a rabbit's fresh prints.</summary>
    public float TrackSpacingRabbit { get; set; } = 14;
    /// <summary>Px between a boar's fresh prints.</summary>
    public float TrackSpacingBoar { get; set; } = 20;
    /// <summary>Most fresh prints at once.</summary>
    public int TrackMax { get; set; } = 220;
    /// <summary>Full width (degrees) of the direction hint per Rastreamento grade 0-3.</summary>
    public float[] TrackSectorDegrees { get; set; } = { 120, 75, 40, 20 };
    /// <summary>Seconds the direction hint stays on the ground.</summary>
    public float TrackHintSeconds { get; set; } = 2.5f;
    /// <summary>Px the swordsman is nudged back when the hide parries.</summary>
    public float ParryNudgePx { get; set; } = 2;
    /// <summary>Seconds of the boar's flinch when a hit lands.</summary>
    public float BoarFlinchSeconds { get; set; } = 0.25f;
    /// <summary>Px of the boar's flinch.</summary>
    public float BoarFlinchPx { get; set; } = 3;
    /// <summary>Seconds of the rabbit's dive into the burrow.</summary>
    public float RabbitDiveSeconds { get; set; } = 0.35f;
    /// <summary>Share of the Lâmina's damage that gets through the hide during the warning and the charge.</summary>
    public float LaminaThroughFactor { get; set; } = 0.5f;
    /// <summary>Seconds the bar beside the animal stays after the fight.</summary>
    public float NearBarSeconds { get; set; } = 3;
    /// <summary>Px within which a warning, charging or recovering boar counts as a fight.</summary>
    public float CombatRadius { get; set; } = 220;
    /// <summary>Px within which an animal seen goes on the map.</summary>
    public float SeenRadius { get; set; } = 180;

    private static FaunaSettings? _current;
    /// <summary>The settings in use (read once).</summary>
    public static FaunaSettings Current => _current ??= Load();
    /// <summary>Parses the settings.</summary>
    public static FaunaSettings Parse(string json) => JsonSerializer.Deserialize<FaunaSettings>(json, PlainsGenSettings.Json) ?? new FaunaSettings();
    /// <summary>Loads the settings (defaults when missing).</summary>
    public static FaunaSettings Load(string path = DefaultPath) => Godot.FileAccess.FileExists(path) ? Parse(Godot.FileAccess.GetFileAsString(path)) : new FaunaSettings();
}

/// <summary>
/// Reading a print (T07): the direction hint gets narrower with Rastreamento, and always contains the real direction.
/// Pure: the "vagueness" offset comes from the print's id, so the same print always says the same.
/// </summary>
public static class TrackingRules
{
    /// <summary>The hint for a print: centre angle (radians) and half width, for a real direction and a grade.</summary>
    public static (float Center, float Half) Sector(Vector2 real, int grade, string printId, FaunaSettings s)
    {
        float full = Mathf.DegToRad(s.TrackSectorDegrees[Mathf.Clamp(grade, 0, s.TrackSectorDegrees.Length - 1)]);
        float half = full / 2;
        // the centre wanders inside the sector by up to 60% of the half width, fixed per print: vague, never wrong
        uint h = 2166136261u; foreach (char c in printId) { h ^= c; h *= 16777619u; }
        float k = ((h >> 8) & 0xFFFF) / 65535f * 2 - 1;
        return (real.Angle() + k * half * 0.6f, half);
    }

    /// <summary>True when the sector contains <paramref name="real"/>.</summary>
    public static bool Contains((float Center, float Half) s, Vector2 real) => Mathf.Abs(Mathf.AngleDifference(s.Center, real.Angle())) <= s.Half + 0.0001f;

    /// <summary>The hint in words: grade 0 says a quarter ("mais ou menos para o norte"), grade 1+ the 8 points, grade 3 adds "com precisão".</summary>
    public static string Words((float Center, float Half) s, int grade)
    {
        var d = Vector2.FromAngle(s.Center);
        if (grade <= 0)
        {
            string[] four = { "o leste", "o sul", "o oeste", "o norte" };
            return "mais ou menos para " + four[Mathf.PosMod(Mathf.RoundToInt(d.Angle() / (Mathf.Pi / 2)), 4)];
        }
        string eight = PlainsGame.Compass(d);
        return grade >= 3 ? $"para {eight} (com precisão)" : $"para {eight}";
    }
}
