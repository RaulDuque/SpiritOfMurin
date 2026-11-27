using SpiritOfMurin.Save;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>Qi → Ki (pedido 14): journeys written before the rename load with every value in its new place.</summary>
public static class KiMigrationTests
{
    // The relevant parts of a format-2 journey written before the rename (same keys and ids as
    // tests/fixtures/saves/jornada_antes_do_ki.json, which is a real file written by the game then).
    private const string OldJourney = """
    {
      "game": "spiritofmurin", "format": 2, "region": "planicie", "seed": 20261126, "spot": [48, -32],
      "qi": 85, "level": 4,
      "attributes": { "vit": 1, "int": 0, "con": 0, "agi": 0, "sor": 0, "qi": 2 },
      "phase": "planicie",
      "book": [ { "name": "Disparo de Qi", "kind": "técnica", "key": "K", "where": "uma onda de qi lançada da palma.", "tech": "qi_wave", "state": "Enfraquecida" } ]
    }
    """;

    /// <summary>The energy, the attribute and the technique of an old journey come back under the Ki names.</summary>
    [UnitTest]
    public static void OldJourney_LoadsKiValues()
    {
        // Act
        var d = SaveGame.FromJson(OldJourney);
        // Assert
        Assert.True(d != null, "the old journey still loads");
        Assert.Equal(85f, d!.Ki, "the energy carried over");
        Assert.Equal(2, (d.Attributes.TryGetValue("ki", out int k) ? k : 0), "the KI attribute kept its 2 points");
        Assert.Equal("ki_wave", d.Book[0].Tech, "the technique id renamed");
        Assert.Equal("Disparo de Ki", d.Book[0].Name, "the Livro entry renamed");
    }

    /// <summary>A text already in the new spelling is returned unchanged (the migration is idempotent).</summary>
    [UnitTest]
    public static void Migrate_IsIdempotent()
    {
        // Arrange
        string once = KiMigration.Migrate(OldJourney);
        // Act
        string twice = KiMigration.Migrate(once);
        // Assert
        Assert.True(!KiMigration.IsOld(once), "no old spelling left");
        Assert.Equal(once, twice, "second pass changes nothing");
    }
}
