using SpiritOfMurin.Items;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>The journey's money: receiving, spending, refusing a short balance.</summary>
public static class WalletTests
{
    /// <summary>Receiving adds whole coins; nothing or a negative amount changes nothing.</summary>
    [UnitTest]
    public static void Receive_AddsCoins_IgnoresNonPositive()
    {
        // Arrange
        var w = new Wallet();
        // Act
        w.Receive(25); w.Receive(0); w.Receive(-5);
        // Assert
        Assert.Equal(25, w.Coins, "25 received");
    }

    /// <summary>Spending what is held takes it; spending more than held is refused and keeps the balance.</summary>
    [UnitTest]
    public static void Spend_TakesCoins_RefusesShortBalance()
    {
        // Arrange
        var w = new Wallet(10);
        // Act
        bool ok = w.Spend(4, out _);
        bool short_ = w.Spend(7, out string why);
        // Assert
        Assert.True(ok, "4 of 10 can be spent");
        Assert.True(!short_ && w.Coins == 6 && why.Contains("insuficientes"), "7 of 6 is refused and the balance stays 6");
    }

    /// <summary>A wallet never goes negative, whatever it is given.</summary>
    [UnitTest]
    public static void Wallet_NeverNegative()
    {
        // Arrange / Act
        var w = new Wallet(-3);
        bool zero = w.Spend(0, out _);
        // Assert
        Assert.True(w.Coins == 0 && !zero, "starts at 0 and refuses spending 0");
    }
}
