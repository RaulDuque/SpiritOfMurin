using System.Collections.Generic;
using System.Linq;
using SpiritOfMurin.Combat;

namespace SpiritOfMurin.Vale;

/// <summary>One page of the Livro de Técnicas.</summary>
public sealed class BookEntry
{
    /// <summary>Name shown.</summary>
    public string Name { get; set; } = "";
    /// <summary>"técnica", "combinação" or "fragmento".</summary>
    public string Kind { get; set; } = "técnica";
    /// <summary>How it is done (keys).</summary>
    public string Key { get; set; } = "";
    /// <summary>Where it came from / what it says.</summary>
    public string Where { get; set; } = "";
    /// <summary>The technique id it stands for (thrust, qi_wave, side_step, dash, lamina), or empty (base art, fragments).</summary>
    public string Tech { get; set; } = "";
    /// <summary>"Plena", "Selada" or "Enfraquecida" for prologue techniques; empty otherwise.</summary>
    public string State { get; set; } = "";
}

/// <summary>
/// The prologue's story state, saved with the journey: what was found, opened, read and learned, who
/// was defeated, what was picked up, the missions and the Livro de Técnicas.
/// </summary>
public sealed class ValeState
{
    /// <summary>The half manual was taken from the body.</summary>
    public bool HasManual { get; set; }
    /// <summary>The tomb door is open.</summary>
    public bool DoorOpen { get; set; }
    /// <summary>The cut in the stone was read.</summary>
    public bool SlabRead { get; set; }
    /// <summary>Footprints walked in order so far (0-3).</summary>
    public int FpIdx { get; set; }
    /// <summary>The side step was learned at the statue.</summary>
    public bool LearnedSide { get; set; }
    /// <summary>The camp gate is open (the bridge disciple fell).</summary>
    public bool GateOpen { get; set; }
    /// <summary>The Silent Blade Step was found.</summary>
    public bool ComboFound { get; set; }
    /// <summary>The inscription beyond the gap was read.</summary>
    public bool SteleRead { get; set; }
    /// <summary>The chest beyond the gap was opened.</summary>
    public bool ChestOpen { get; set; }
    /// <summary>Defeated enemies by id (dis_bridge, dis_herbs, dis_gap, dis_camp).</summary>
    public List<string> Defeated { get; set; } = new();
    /// <summary>Herbs picked, by index.</summary>
    public List<int> HerbsTaken { get; set; } = new();
    /// <summary>Pots broken, by index.</summary>
    public List<int> PotsBroken { get; set; } = new();
    /// <summary>Missions: id → done.</summary>
    public Dictionary<string, bool> Quests { get; set; } = new();
    /// <summary>Times defeated.</summary>
    public int Deaths { get; set; }
    /// <summary>One-time hints already shown.</summary>
    public List<string> Flags { get; set; } = new();
    /// <summary>Map tiles seen (row-major 60 × 40, as a string of 0/1).</summary>
    public string Seen { get; set; } = "";
    /// <summary>The Livro de Técnicas.</summary>
    public List<BookEntry> Book { get; set; } = new();

    /// <summary>The book a new journey starts with: the swordsman's own art.</summary>
    public static List<BookEntry> StartingBook() => new()
    {
        new() { Name = "Corte", Kind = "técnica", Key = "J / clique esquerdo", Where = "Sua arte de origem. Encadeia até três golpes." },
        new() { Name = "Estocada", Kind = "técnica", Key = "frente + J (com alvo travado)", Where = "Sua arte de origem: com o alvo travado, avança a lâmina.", Tech = "thrust", State = "Plena" },
        new() { Name = "Disparo de Qi", Kind = "técnica", Key = "K", Where = "Sua arte de origem: uma onda de qi lançada da palma.", Tech = "qi_wave", State = "Plena" },
        new() { Name = "Passo curto", Kind = "técnica", Key = "L / clique direito", Where = "Sua arte de origem: só para frente ou para trás." },
    };

    /// <summary>Adds a page unless one with the same name exists.</summary>
    public void AddBook(BookEntry e) { if (!Book.Any(b => b.Name == e.Name)) Book.Add(e); }

    /// <summary>Marks a one-time flag; returns true the first time.</summary>
    public bool Once(string flag) { if (Flags.Contains(flag)) return false; Flags.Add(flag); return true; }

    /// <summary>The technique states the book implies.</summary>
    public void ApplyTo(TechniqueSet set)
    {
        foreach (var e in Book.Where(b => b.Tech.Length > 0))
            set.Set(e.Tech, e.State switch { "Selada" => TechniqueState.Sealed, "Enfraquecida" => TechniqueState.Weakened, _ => TechniqueState.Full });
    }
}
