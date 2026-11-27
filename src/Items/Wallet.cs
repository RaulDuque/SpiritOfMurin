using System;

namespace SpiritOfMurin.Items;

/// <summary>
/// The journey's money (pedido 13, item 11): a whole number of coins, provisionally called "Moedas". It is
/// the base the villages, towns and merchants will use later: <see cref="Receive"/> adds coins,
/// <see cref="Spend"/> takes them or refuses when the balance is short. Shops and prices come with the villages.
/// </summary>
public sealed class Wallet
{
    /// <summary>The coins held (never negative).</summary>
    public int Coins { get; private set; }

    /// <summary>Raised after the balance changes, with the new balance.</summary>
    public event Action<int>? Changed;

    /// <summary>Creates a wallet with a starting balance (negative values become 0).</summary>
    public Wallet(int coins = 0) { Coins = Math.Max(0, coins); }

    /// <summary>Adds coins (amounts of 0 or less do nothing). Returns the new balance.</summary>
    public int Receive(int amount)
    {
        if (amount <= 0) return Coins;
        Coins += amount;
        Changed?.Invoke(Coins);
        return Coins;
    }

    /// <summary>
    /// Takes coins. Returns false — and leaves the balance as it was — when the amount is not positive or the
    /// balance is short; <paramref name="reason"/> says why, in words for the player.
    /// </summary>
    public bool Spend(int amount, out string reason)
    {
        if (amount <= 0) { reason = "Valor inválido."; return false; }
        if (amount > Coins) { reason = $"Moedas insuficientes: tem {Coins}, precisa de {amount}."; return false; }
        Coins -= amount;
        reason = "";
        Changed?.Invoke(Coins);
        return true;
    }

    /// <summary>Sets the balance (loading a journey; the awakening's loss).</summary>
    public void Set(int coins) { Coins = Math.Max(0, coins); Changed?.Invoke(Coins); }
}
