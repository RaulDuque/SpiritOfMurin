using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Plains;

/// <summary>
/// What changed in the generated world during this play session, keyed by the generator's stable
/// ids: objects collected, animals killed (with where their carcass lies) and carcass parts taken.
/// Chunks consult it when they are built again, so leaving a chunk and coming back keeps every change.
/// Saved when meditating (see <c>SaveGame</c>).
/// </summary>
public sealed class WorldState
{
    private readonly HashSet<string> _collected = new();
    private readonly Dictionary<string, Vector2> _dead = new();
    private readonly HashSet<string> _partsTaken = new();

    /// <summary>True when the object with this id was collected.</summary>
    public bool IsCollected(string id) => _collected.Contains(id);

    /// <summary>Marks an object collected. Returns false when it already was (no second pickup).</summary>
    public bool MarkCollected(string id) => _collected.Add(id);

    /// <summary>Number of objects collected so far.</summary>
    public int CollectedCount => _collected.Count;

    /// <summary>Records an animal's death and where its carcass lies.</summary>
    public void MarkDead(string creatureId, Vector2 carcassAt) => _dead.TryAdd(creatureId, carcassAt);

    /// <summary>Where the dead animal's carcass lies, or null when it is alive.</summary>
    public Vector2? CarcassAt(string creatureId) => _dead.TryGetValue(creatureId, out var p) ? p : null;

    /// <summary>True when that part (e.g. "carne_macia") was already taken from that carcass.</summary>
    public bool IsPartTaken(string creatureId, string part) => _partsTaken.Contains(creatureId + "#" + part);

    /// <summary>Marks a carcass part taken. Returns false when it already was.</summary>
    public bool MarkPartTaken(string creatureId, string part) => _partsTaken.Add(creatureId + "#" + part);

    private readonly HashSet<string> _trails = new();
    private readonly Dictionary<string, float> _work = new();

    /// <summary>
    /// The work done on an element of the world (a tree cut, a rock broken; pedido 14, Entrega 5), by its place id.
    /// A used-up element keeps its full work, so it comes back as a stump and never gives its materials again.
    /// </summary>
    public float WorkOf(string placeId) => _work.TryGetValue(placeId, out float w) ? w : 0;

    /// <summary>Records the work done on an element.</summary>
    public void SetWork(string placeId, float work) { if (work > 0) _work[placeId] = work; else _work.Remove(placeId); }

    /// <summary>Saved form of the work on elements.</summary>
    public Dictionary<string, float> WorkSnapshot() => new(_work);

    /// <summary>Restores the work on elements (replacing it).</summary>
    public void RestoreWork(IDictionary<string, float>? work) { _work.Clear(); if (work != null) foreach (var (k, v) in work) if (v > 0) _work[k] = v; }

    /// <summary>Marks a trail examined. Returns false when it already was (no second practice or XP).</summary>
    public bool MarkTrailExamined(string trailId) => _trails.Add(trailId);

    /// <summary>True when the trail was examined.</summary>
    public bool IsTrailExamined(string trailId) => _trails.Contains(trailId);

    /// <summary>Saved form of everything this state holds.</summary>
    public (List<string> Collected, Dictionary<string, Vector2> Dead, List<string> Parts, List<string> Trails) Snapshot() =>
        (new(_collected), new(_dead), new(_partsTaken), new(_trails));

    /// <summary>Restores a saved state (replacing everything).</summary>
    public void Restore(IEnumerable<string> collected, IDictionary<string, Vector2> dead, IEnumerable<string> parts, IEnumerable<string> trails)
    {
        _collected.Clear(); foreach (var c in collected) _collected.Add(c);
        _dead.Clear(); foreach (var (k, v) in dead) _dead[k] = v;
        _partsTaken.Clear(); foreach (var p in parts) _partsTaken.Add(p);
        _trails.Clear(); foreach (var t in trails) _trails.Add(t);
    }
}
