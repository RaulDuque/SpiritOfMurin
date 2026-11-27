using System.Collections.Generic;
using System.Text.Json;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Combat;

/// <summary>The state of a technique in the Livro de Técnicas.</summary>
public enum TechniqueState { Unknown, Full, Sealed, Weakened }

/// <summary>One version (full or weakened) of a technique. Unused fields stay 0.</summary>
public sealed class TechniqueParams
{
    /// <summary>Ki spent per use.</summary>
    public float KiCost { get; set; }
    /// <summary>Vigor spent per use (ignored where there is no vigor).</summary>
    public float VigorCost { get; set; }
    /// <summary>Damage in points.</summary>
    public int Damage { get; set; }
    /// <summary>Ki wave / lamina: distance travelled, px.</summary>
    public float Range { get; set; }
    /// <summary>Ki wave / lamina: speed, px/s.</summary>
    public float Speed { get; set; }
    /// <summary>Dash: distance, px.</summary>
    public float Distance { get; set; }
    /// <summary>Side step / dash: duration, s.</summary>
    public float Time { get; set; }
    /// <summary>Side step: degrees around the locked target.</summary>
    public float Angle { get; set; }
    /// <summary>Side step: untouchable seconds.</summary>
    public float Iframes { get; set; }
    /// <summary>Side step: smallest orbit radius, px.</summary>
    public float RadiusMin { get; set; }
    /// <summary>Side step: largest orbit radius, px.</summary>
    public float RadiusMax { get; set; }
    /// <summary>Thrust: reach, px.</summary>
    public float Reach { get; set; }
    /// <summary>Thrust: body lunge, px.</summary>
    public float Lunge { get; set; }
    /// <summary>Lamina: seconds allowed between side step → cut → ki.</summary>
    public float ComboWindow { get; set; }
    /// <summary>Lamina / charged shot: seconds the guard stays broken.</summary>
    public float Stagger { get; set; }
    /// <summary>Charged shot: seconds the button must be held before the charge starts (a shorter press is the plain shot).</summary>
    public float Hold { get; set; }
    /// <summary>Charged shot: Ki drawn into the palm per second while charging.</summary>
    public float Drain { get; set; }
    /// <summary>Reinforce: melee strikes reinforced by one activation.</summary>
    public int Charges { get; set; }
    /// <summary>Reinforce: extra damage of a reinforced strike, as a fraction (0.10 = +10%).</summary>
    public float Bonus { get; set; }
    /// <summary>Reinforce: chance (0-1) that a reinforced strike breaks the guard it meets, rolled once per strike and guard.</summary>
    public float Chance { get; set; }
}

/// <summary>A technique's name and its two versions.</summary>
public sealed class TechniqueDef
{
    /// <summary>Name in the book.</summary>
    public string Name { get; set; } = "";
    /// <summary>Full version (the prologue).</summary>
    public TechniqueParams Full { get; set; } = new();
    /// <summary>Weakened version (after the seal, from the unlock level).</summary>
    public TechniqueParams Weak { get; set; } = new();
}

/// <summary>The prologue techniques, from <c>res://assets/data/combat_techniques.json</c> (snake_case).</summary>
public sealed class TechniqueData
{
    /// <summary>Default path.</summary>
    public const string DefaultPath = "res://assets/data/combat_techniques.json";
    /// <summary>Ids of the techniques the seal covers.</summary>
    public static readonly string[] SealedIds = { "thrust", "ki_wave", "side_step", "dash", "lamina", "ki_charge" };

    /// <summary>Plains level at which sealed techniques come back weakened.</summary>
    public int UnlockLevel { get; set; } = 3;
    /// <summary>Ki wave.</summary>
    public TechniqueDef KiWave { get; set; } = new();
    /// <summary>Side step.</summary>
    public TechniqueDef SideStep { get; set; } = new();
    /// <summary>Dash (passo longo).</summary>
    public TechniqueDef Dash { get; set; } = new();
    /// <summary>Thrust (estocada).</summary>
    public TechniqueDef Thrust { get; set; } = new();
    /// <summary>The Silent Blade Step combination.</summary>
    public TechniqueDef Lamina { get; set; } = new();
    /// <summary>The secret charged Ki shot (hold the Ki button): ki_cost is the full charge's total cost.</summary>
    public TechniqueDef KiCharge { get; set; } = new();
    /// <summary>The Reforço de Ki (modifier + cut): hold = the gesture's seconds.</summary>
    public TechniqueDef KiReinforce { get; set; } = new();

    /// <summary>The definition by id.</summary>
    public TechniqueDef Get(string id) => id switch
    {
        "ki_wave" => KiWave, "side_step" => SideStep, "dash" => Dash, "thrust" => Thrust, "lamina" => Lamina, "ki_charge" => KiCharge, "ki_reinforce" => KiReinforce, _ => new TechniqueDef { Name = id },
    };

    /// <summary>Parses the data.</summary>
    public static TechniqueData Parse(string json) => JsonSerializer.Deserialize<TechniqueData>(json, PlainsGenSettings.Json) ?? new TechniqueData();

    /// <summary>Loads the data from a Godot path.</summary>
    public static TechniqueData Load(string path = DefaultPath) => Parse(Godot.FileAccess.GetFileAsString(path));
}

/// <summary>
/// What the body may do right now with each prologue technique: its state and the parameters in force
/// (full or weakened). Unknown and sealed techniques have no parameters.
/// </summary>
public sealed class TechniqueSet
{
    private readonly Dictionary<string, TechniqueState> _state = new();
    /// <summary>The data both versions come from.</summary>
    public TechniqueData Data { get; }

    /// <summary>Creates a set with every technique unknown.</summary>
    public TechniqueSet(TechniqueData data) { Data = data; }

    /// <summary>State of a technique.</summary>
    public TechniqueState State(string id) => _state.TryGetValue(id, out var s) ? s : TechniqueState.Unknown;

    /// <summary>Sets the state of a technique.</summary>
    public void Set(string id, TechniqueState s) => _state[id] = s;

    /// <summary>The parameters in force, or null when it cannot be used (unknown or sealed).</summary>
    public TechniqueParams? Params(string id) => State(id) switch
    {
        TechniqueState.Full => Data.Get(id).Full,
        TechniqueState.Weakened => Data.Get(id).Weak,
        _ => null,
    };

    /// <summary>All states (for saving).</summary>
    public Dictionary<string, TechniqueState> All => new(_state);
}
