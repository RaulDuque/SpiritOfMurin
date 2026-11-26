using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritOfMurin.Items;

/// <summary>
/// What the character carries: item id → quantity, in the order first picked up. Pure logic,
/// no nodes, so it is unit-testable.
/// </summary>
public sealed class Inventory
{
    private readonly List<string> _order = new();
    private readonly Dictionary<string, int> _counts = new();

    /// <summary>Raised after any change: item id and its new quantity.</summary>
    public event Action<string, int>? Changed;

    /// <summary>Quantity held of an item (0 when none).</summary>
    public int Count(string id) => _counts.TryGetValue(id, out int n) ? n : 0;

    /// <summary>Items held, in pickup order, with quantities above zero.</summary>
    public IReadOnlyList<(string Id, int Count)> Entries => _order.Where(id => Count(id) > 0).Select(id => (id, Count(id))).ToList();

    /// <summary>Adds items; returns the new quantity.</summary>
    public int Add(string id, int n = 1)
    {
        if (n <= 0) return Count(id);
        if (!_counts.ContainsKey(id)) { _order.Add(id); _counts[id] = 0; }
        _counts[id] += n;
        Changed?.Invoke(id, _counts[id]);
        return _counts[id];
    }

    /// <summary>Removes items if there are enough; returns false (and changes nothing) otherwise.</summary>
    public bool Remove(string id, int n = 1)
    {
        if (n <= 0 || Count(id) < n) return false;
        _counts[id] -= n;
        Changed?.Invoke(id, _counts[id]);
        return true;
    }

    /// <summary>Replaces everything with a saved list (pickup order kept).</summary>
    public void Restore(IEnumerable<(string Id, int Count)> entries)
    {
        _order.Clear(); _counts.Clear();
        foreach (var (id, n) in entries) if (n > 0 && !_counts.ContainsKey(id)) { _order.Add(id); _counts[id] = n; }
        Changed?.Invoke("", 0);
    }

    /// <summary>True when every input of the recipe is held.</summary>
    public bool CanCraft(RecipeDef r) => r.Inputs.All(kv => Count(kv.Key) >= kv.Value);

    /// <summary>Consumes the recipe's inputs once and adds its output. Returns false and changes nothing when short of materials.</summary>
    public bool Craft(RecipeDef r)
    {
        if (!CanCraft(r)) return false;
        foreach (var (id, n) in r.Inputs) Remove(id, n);
        Add(r.Output, r.Count);
        return true;
    }
}
