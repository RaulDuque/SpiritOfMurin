using System;
using System.Collections.Generic;

namespace SpiritOfMurin.Progression;

/// <summary>
/// Proficiencies that grow by use: Esfolar, Preparo de Caça, Erboristeria and Rastreamento. Each
/// practice is keyed by what was practised on (a carcass part, a plant, a trail), so the same thing
/// never counts twice — reopening a carcass or re-examining a track gives nothing.
/// </summary>
public sealed class Proficiencies
{
    /// <summary>Skill ids in sheet order.</summary>
    public static readonly string[] SkillIds = { "esfolar", "preparo", "erboristeria", "rastreamento" };

    private readonly ProgressionData _d;
    private readonly Dictionary<string, int> _practice = new();
    private readonly HashSet<string> _keys = new();

    /// <summary>Creates all skills at grade 0.</summary>
    public Proficiencies(ProgressionData data) { _d = data; foreach (var s in SkillIds) _practice[s] = 0; }

    /// <summary>Practice gathered in a skill.</summary>
    public int Practice(string skill) => _practice.TryGetValue(skill, out int p) ? p : 0;

    /// <summary>Current grade (0-3) of a skill.</summary>
    public int Grade(string skill)
    {
        int p = Practice(skill), g = 0;
        foreach (int t in _d.Skill(skill).Thresholds) if (p >= t) g++;
        return Math.Min(g, 3);
    }

    /// <summary>Practice needed for the next grade, or null at the top.</summary>
    public int? NextThreshold(string skill)
    {
        int g = Grade(skill);
        var th = _d.Skill(skill).Thresholds;
        return g < th.Length ? th[g] : null;
    }

    /// <summary>
    /// Adds one practice for <paramref name="key"/> (e.g. "esfolar:rabbit@-9,-5"). Returns the grade
    /// reached when it went up, 0 when it counted without a new grade, -1 when the key already counted.
    /// </summary>
    public int AddPractice(string skill, string key)
    {
        if (!_keys.Add(skill + ":" + key)) return -1;
        int before = Grade(skill);
        _practice[skill] = Practice(skill) + 1;
        int after = Grade(skill);
        return after > before ? after : 0;
    }

    /// <summary>True when that key already gave practice in that skill.</summary>
    public bool HasPracticed(string skill, string key) => _keys.Contains(skill + ":" + key);

    /// <summary>Saved form: practice per skill and the keys already counted.</summary>
    public (Dictionary<string, int> Practice, List<string> Keys) Snapshot() => (new(_practice), new(_keys));

    /// <summary>Restores a saved state.</summary>
    public void Restore(IDictionary<string, int> practice, IEnumerable<string> keys)
    {
        foreach (var s in SkillIds) _practice[s] = practice.TryGetValue(s, out int v) ? Math.Max(0, v) : 0;
        _keys.Clear(); foreach (var k in keys) _keys.Add(k);
    }

    /// <summary>What a grade of a skill lets the player do (one line, shown in the sheet).</summary>
    public static string Benefit(string skill, int grade) => (skill, Math.Clamp(grade, 0, 3)) switch
    {
        ("esfolar", 0) => "Bruto: a pele sai rasgada (só serve para tiras).",
        ("esfolar", 1) => "Iniciante: pele comum. Com Qi (K na carcaça), preserva um grau acima.",
        ("esfolar", 2) => "Adepto: couro intacto (Pele Fina, Couro Grosso).",
        ("esfolar", _) => "Mestre: extração espiritual com Qi (Presa Espiritual do javali).",
        ("preparo", 0) => "Bruto: uma carne por caça; a carne assada recupera o vigor base.",
        ("preparo", 1) => "Iniciante: a carne que você assar recupera +25% de vigor.",
        ("preparo", 2) => "Adepto: duas carnes por caça; +50% de vigor ao assar.",
        ("preparo", _) => "Mestre: duas carnes; +75% de vigor ao assar.",
        ("erboristeria", 0) => "Bruto: a erva sai amassada (cura 3).",
        ("erboristeria", 1) => "Iniciante: cortada no talo (cura 4).",
        ("erboristeria", 2) => "Adepto: bem colhida (cura 5).",
        ("erboristeria", _) => "Mestre: seiva viva (cura 6, mais rápido).",
        ("rastreamento", 0) => "Bruto: a pegada diz o animal e a direção.",
        ("rastreamento", 1) => "Iniciante: diz a distância e a próxima pegada brilha.",
        ("rastreamento", 2) => "Adepto: revela o nível do animal pela pegada.",
        ("rastreamento", _) => "Mestre: a trilha inteira à frente fica brilhando.",
        _ => "",
    };
}
