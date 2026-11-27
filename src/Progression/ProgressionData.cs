using System.Collections.Generic;
using System.Text.Json;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Progression;

/// <summary>XP rewards by action.</summary>
public sealed class XpRewards
{
    /// <summary>XP per creature level for a hunt, by creature kind (rabbit, boar).</summary>
    public Dictionary<string, int> HuntPerLevel { get; set; } = new() { ["rabbit"] = 6, ["boar"] = 9 };
    /// <summary>XP for the first craft of each recipe.</summary>
    public int FirstCraft { get; set; } = 5;
    /// <summary>XP for the first examination of a trail.</summary>
    public int TrailExamined { get; set; } = 3;
    /// <summary>XP for each new plant harvested.</summary>
    public int HerbHarvested { get; set; } = 1;
    /// <summary>XP for each piece of meat cooked.</summary>
    public int MeatCooked { get; set; } = 2;
    /// <summary>XP for each new practice in a proficiency (skinning, butchering, a plant, a trail, a roast).</summary>
    public int Practice { get; set; } = 1;
}

/// <summary>Level-1 values of the character.</summary>
public sealed class BaseStats
{
    /// <summary>Maximum vigor.</summary>
    public float MaxVigor { get; set; } = 50;
    /// <summary>Vigor regained per second.</summary>
    public float VigorRegen { get; set; } = 14;
    /// <summary>Seconds after spending vigor before it regenerates.</summary>
    public float VigorRegenDelay { get; set; } = 0.6f;
    /// <summary>Vigor a step costs.</summary>
    public float StepVigorCost { get; set; } = 12;
    /// <summary>Vigor a cut costs.</summary>
    public float CutVigorCost { get; set; } = 3;
    /// <summary>Maximum Ki.</summary>
    public float MaxKi { get; set; } = 100;
    /// <summary>Ki regained per second (meditating fills it).</summary>
    public float KiRegen { get; set; } = 0.5f;
    /// <summary>Ki spent to preserve a material while skinning.</summary>
    public float KiPreserveCost { get; set; } = 15;
    /// <summary>Base chance of a secondary material.</summary>
    public float SecondaryChance { get; set; } = 0.10f;
    /// <summary>Base potency multiplier of medicinal preparations.</summary>
    public float MedicinePotency { get; set; } = 1.0f;
}

/// <summary>Effect of each attribute point.</summary>
public sealed class PerPoint
{
    /// <summary>VIT: maximum Vida per point.</summary>
    public float VitMaxHealth { get; set; } = 2;
    /// <summary>INT: medicinal potency per point.</summary>
    public float IntMedicinePotency { get; set; } = 0.12f;
    /// <summary>CON: maximum vigor per point.</summary>
    public float ConMaxVigor { get; set; } = 10;
    /// <summary>CON: vigor regeneration per point (fraction).</summary>
    public float ConVigorRegen { get; set; } = 0.10f;
    /// <summary>AGI: walking speed per point (fraction).</summary>
    public float AgiMoveSpeed { get; set; } = 0.04f;
    /// <summary>AGI: step distance per point (fraction).</summary>
    public float AgiStepDistance { get; set; } = 0.05f;
    /// <summary>AGI: shorter cut and step recovery per point (fraction).</summary>
    public float AgiRecovery { get; set; } = 0.05f;
    /// <summary>AGI: cap of each AGI bonus.</summary>
    public float AgiCap { get; set; } = 0.40f;
    /// <summary>SOR: secondary material chance per point.</summary>
    public float SorSecondaryChance { get; set; } = 0.05f;
    /// <summary>KI: maximum Ki per point.</summary>
    public float KiMaxKi { get; set; } = 15;
}

/// <summary>A proficiency's name and practice thresholds for grades 1, 2, 3.</summary>
public sealed class SkillDef
{
    /// <summary>Name shown to the player.</summary>
    public string Name { get; set; } = "";
    /// <summary>Practice needed for grades 1, 2 and 3.</summary>
    public int[] Thresholds { get; set; } = { 1, 4, 9 };
}

/// <summary>A creature's level range and what each level above the minimum changes.</summary>
public sealed class CreatureDef
{
    /// <summary>Name shown when focused.</summary>
    public string Name { get; set; } = "";
    /// <summary>One-word temper shown when focused.</summary>
    public string Temper { get; set; } = "";
    /// <summary>Lowest level.</summary>
    public int MinLevel { get; set; } = 1;
    /// <summary>Highest level.</summary>
    public int MaxLevel { get; set; } = 1;
    /// <summary>Hearing radius per level above the minimum (fraction).</summary>
    public float HearingPerLevel { get; set; }
    /// <summary>Running / charging speed per level above the minimum (fraction).</summary>
    public float SpeedPerLevel { get; set; }
    /// <summary>Health (hits) per level above the minimum.</summary>
    public int HealthPerLevel { get; set; }
    /// <summary>Contact damage per level above the minimum (rounded down).</summary>
    public float DamagePerLevel { get; set; }
}

/// <summary>Spiritual extraction: which part of which creature turns into what with Ki at grade 3.</summary>
public sealed class SpiritualDef
{
    /// <summary>Carcass part ("presas").</summary>
    public string Part { get; set; } = "";
    /// <summary>Item obtained.</summary>
    public string Item { get; set; } = "";
}

/// <summary>All progression numbers, from <c>res://assets/data/plains/plains_progression.json</c> (snake_case keys).</summary>
public sealed class ProgressionData
{
    /// <summary>Default data path.</summary>
    public const string DefaultPath = "res://assets/data/plains/plains_progression.json";

    /// <summary>XP from level 1 to 2.</summary>
    public float XpBase { get; set; } = 20;
    /// <summary>Growth of the XP needed per level.</summary>
    public float XpGrowth { get; set; } = 1.5f;
    /// <summary>Attribute points per level gained.</summary>
    public int PointsPerLevel { get; set; } = 2;
    /// <summary>XP rewards.</summary>
    public XpRewards Xp { get; set; } = new();
    /// <summary>XP multiplier per level of difference (creature − character).</summary>
    public float LevelDiffStep { get; set; } = 0.15f;
    /// <summary>Lowest hunt XP multiplier.</summary>
    public float LevelDiffMin { get; set; } = 0.4f;
    /// <summary>Highest hunt XP multiplier.</summary>
    public float LevelDiffMax { get; set; } = 1.6f;
    /// <summary>Level-1 values.</summary>
    public BaseStats Base { get; set; } = new();
    /// <summary>Effect of each point.</summary>
    public PerPoint PerPoint { get; set; } = new();
    /// <summary>Proficiencies by id (esfolar, preparo, erboristeria, rastreamento).</summary>
    public Dictionary<string, SkillDef> Skills { get; set; } = new();
    /// <summary>Names of grades 0-3.</summary>
    public string[] GradeNames { get; set; } = { "Bruto", "Iniciante", "Adepto", "Mestre" };
    /// <summary>Hide item by creature kind and Esfolar grade (0-3).</summary>
    public Dictionary<string, string[]> Hides { get; set; } = new();
    /// <summary>Spiritual extraction by creature kind.</summary>
    public Dictionary<string, SpiritualDef> Spiritual { get; set; } = new();
    /// <summary>Preparo de Caça grade from which a carcass gives a second piece of meat.</summary>
    public int MeatExtraGrade { get; set; } = 2;
    /// <summary>Extra vigor of cooked meat per Preparo de Caça grade of the cook (fraction).</summary>
    public float CookedVigorBonusPerGrade { get; set; } = 0.25f;
    /// <summary>Herb item by Erboristeria grade (0-3).</summary>
    public string[] HerbQuality { get; set; } = { "grama_sangue_q0", "grama_sangue_q1", "grama_sangue_q2", "grama_sangue_q3" };
    /// <summary>Creature definitions by kind (rabbit, boar).</summary>
    public Dictionary<string, CreatureDef> Creatures { get; set; } = new();
    /// <summary>Secondary material by source (rabbit, boar: item id; herb, flint: "extra" = one more of the same).</summary>
    public Dictionary<string, string> Secondary { get; set; } = new();

    /// <summary>Parses the data from JSON text.</summary>
    public static ProgressionData Parse(string json) => JsonSerializer.Deserialize<ProgressionData>(json, PlainsGenSettings.Json) ?? new ProgressionData();

    /// <summary>Loads the data from a Godot path.</summary>
    public static ProgressionData Load(string path = DefaultPath) => Parse(Godot.FileAccess.GetFileAsString(path));

    /// <summary>The skill definition, or a default named after the id.</summary>
    public SkillDef Skill(string id) => Skills.TryGetValue(id, out var s) ? s : new SkillDef { Name = id };

    /// <summary>Name of a grade (0-3).</summary>
    public string GradeName(int g) => GradeNames[System.Math.Clamp(g, 0, GradeNames.Length - 1)];
}
