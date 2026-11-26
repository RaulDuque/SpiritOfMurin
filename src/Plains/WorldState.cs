using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Plains;

/// <summary>
/// What changed in the generated world during this play session, keyed by the generator's stable
/// ids: objects collected, animals killed (with where their carcass lies) and carcass parts taken.
/// Chunks consult it when they are built again, so leaving a chunk and coming back keeps every change.
/// Lives for the session only (saving comes in a later delivery).
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
}
