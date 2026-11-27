using System.Collections.Generic;
using System.Linq;
using Godot;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Plains;
using SpiritOfMurin.Progression;
using SpiritOfMurin.Save;
using static SpiritOfMurin.Tests.Unit.Plains.PlainsFixtures;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>Level, attributes, proficiencies, harvest results, creature levels, save format, trails.</summary>
public static class ProgressionTests
{
    /// <summary>Progression numbers equal to plains_progression.json at the time of writing (no file I/O).</summary>
    public static ProgressionData Data() => new()
    {
        XpBase = 20, XpGrowth = 1.5f, PointsPerLevel = 2,
        Skills = new()
        {
            ["esfolar"] = new() { Name = "Esfolar", Thresholds = new[] { 1, 4, 9 } },
            ["preparo"] = new() { Name = "Preparo de Caça", Thresholds = new[] { 1, 4, 9 } },
            ["erboristeria"] = new() { Name = "Erboristeria", Thresholds = new[] { 2, 5, 10 } },
            ["rastreamento"] = new() { Name = "Rastreamento", Thresholds = new[] { 1, 3, 6 } },
        },
        Hides = new()
        {
            ["rabbit"] = new[] { "pele_rasgada", "pele_comum", "pele_fina", "pele_fina" },
            ["boar"] = new[] { "couro_rasgado", "couro_comum", "couro_grosso", "couro_grosso" },
        },
        Spiritual = new() { ["boar"] = new() { Part = "presas", Item = "presa_espiritual" } },
        Creatures = new()
        {
            ["rabbit"] = new() { Name = "Coelho de Campo", Temper = "Arisco", MinLevel = 1, MaxLevel = 2, HearingPerLevel = 0.12f, SpeedPerLevel = 0.08f },
            ["boar"] = new() { Name = "Javali da Mata", Temper = "Territorial", MinLevel = 3, MaxLevel = 5, SpeedPerLevel = 0.06f, HealthPerLevel = 1, DamagePerLevel = 0.5f },
        },
        Secondary = new() { ["rabbit"] = "tendao", ["boar"] = "tendao", ["herb"] = "extra", ["flint"] = "extra" },
    };

    /// <summary>20 XP takes level 1 to 2 and gives 2 points; the next level needs 30.</summary>
    [UnitTest]
    public static void FirstLevel_At20Xp_GivesTwoPoints()
    {
        // Arrange
        var s = new PlayerStats(Data(), 10);
        // Act
        int a = s.AddXp(19), b = s.AddXp(1);
        // Assert
        Assert.Equal(0, a, "19 XP: no level");
        Assert.Equal(1, b, "20 XP: one level");
        Assert.Equal(2, s.Level, "level");
        Assert.Equal(2, s.Points, "points");
        Assert.Equal(30, s.XpToNext(2), "XP for level 3");
        Assert.Equal(30, s.XpMissing, "missing");
    }

    /// <summary>Each attribute point changes its number, exactly as the preview said.</summary>
    [UnitTest]
    public static void EachAttribute_ChangesWhatThePreviewShows()
    {
        var expect = new Dictionary<string, string[]>
        {
            ["vit"] = new[] { "Vida máxima: 10 → 12" },
            ["int"] = new[] { "Potência dos preparos medicinais: 100% → 112%", "Grama Sangue-de-Coelho comum: 4,0 → 4,5 de Vida" },
            ["con"] = new[] { "Vigor máximo: 50 → 60", "Recuperação do vigor: 14,0/s → 15,4/s" },
            ["agi"] = new[] { "Velocidade ao andar: 100% → 104%", "Distância do passo: 100% → 105%", "Tempo de recuperação do corte e do passo: 100% → 95%" },
            ["sor"] = new[] { "Chance de material secundário (tendão na caça, uma planta ou lasca a mais na coleta): 10% → 15%" },
            ["ki"] = new[] { "Ki máximo: 100 → 115", "Esfolas com Ki seguidas (custo 15): 6 → 7" },
        };
        foreach (var attr in PlayerStats.AttributeIds)
        {
            // Arrange
            var s = new PlayerStats(Data(), 10); s.AddXp(20);
            var before = (s.MaxHealth, s.MedicinePotency, s.MaxVigor, s.MoveSpeedMultiplier, s.SecondaryChance, s.MaxKi);
            // Act
            var preview = s.Preview(attr, 4).Select(l => l.Replace('.', ',')).ToList();
            bool spent = s.Spend(attr);
            // Assert
            Assert.True(spent, $"{attr}: spent");
            for (int i = 0; i < expect[attr].Length; i++) Assert.Equal(expect[attr][i], preview[i], $"{attr} preview {i}");
            var after = (s.MaxHealth, s.MedicinePotency, s.MaxVigor, s.MoveSpeedMultiplier, s.SecondaryChance, s.MaxKi);
            Assert.True(!before.Equals(after), $"{attr}: something changed");
            Assert.Equal(1, s.Points, $"{attr}: one point left");
        }
    }

    /// <summary>No points, no spending; AGI bonuses stop at the cap.</summary>
    [UnitTest]
    public static void Spend_WithoutPoints_Fails_AndAgiIsCapped()
    {
        // Arrange
        var s = new PlayerStats(Data(), 10);
        // Act / Assert
        Assert.True(!s.Spend("vit"), "no points");
        s.Restore(9, 0, 0, 0, new Dictionary<string, int> { ["agi"] = 20 });
        Assert.True(Mathf.IsEqualApprox(s.MoveSpeedMultiplier, 1.4f) && Mathf.IsEqualApprox(s.RecoveryMultiplier, 0.6f), $"agi 20 capped: {s.MoveSpeedMultiplier}, {s.RecoveryMultiplier}");
    }

    /// <summary>Practice counts once per thing; Esfolar goes 0 → 1 at the first new carcass.</summary>
    [UnitTest]
    public static void Practice_CountsOncePerThing_EsfolarReachesIniciante()
    {
        // Arrange
        var p = new Proficiencies(Data());
        // Act
        int first = p.AddPractice("esfolar", "rabbit@1,1");
        int again = p.AddPractice("esfolar", "rabbit@1,1");
        int other = p.AddPractice("esfolar", "rabbit@2,2");
        // Assert
        Assert.Equal(1, first, "first carcass: grade 1 reached");
        Assert.Equal(-1, again, "same carcass: no practice");
        Assert.Equal(0, other, "second carcass: practice, no new grade");
        Assert.Equal(2, p.Practice("esfolar"), "practice");
        Assert.Equal(1, p.Grade("esfolar"), "grade");
        Assert.Equal(4, p.NextThreshold("esfolar"), "next threshold");
        Assert.Equal(1, p.AddPractice("rastreamento", "trail@1,1"), "rastreamento: first trail reaches Iniciante");
        Assert.Equal(-1, p.AddPractice("rastreamento", "trail@1,1"), "rastreamento: the same trail again gives nothing");
    }

    /// <summary>The hide by Esfolar grade: torn, common, intact; Ki raises it one grade from Iniciante, never from Bruto.</summary>
    [UnitTest]
    public static void Skin_GradesAndKi_GiveTheDocumentMaterials()
    {
        // Arrange
        var d = Data();
        // Act / Assert
        Assert.Equal("pele_rasgada", HarvestRules.Skin(d, "rabbit", 0, false, 100).Item, "rabbit grade 0");
        Assert.Equal("pele_comum", HarvestRules.Skin(d, "rabbit", 1, false, 100).Item, "rabbit grade 1");
        Assert.Equal("pele_fina", HarvestRules.Skin(d, "rabbit", 2, false, 100).Item, "rabbit grade 2");
        Assert.Equal("couro_rasgado", HarvestRules.Skin(d, "boar", 0, false, 100).Item, "boar grade 0");
        Assert.Equal("couro_comum", HarvestRules.Skin(d, "boar", 1, false, 100).Item, "boar grade 1");
        Assert.Equal("couro_grosso", HarvestRules.Skin(d, "boar", 2, false, 100).Item, "boar grade 2");
        var bruteKi = HarvestRules.Skin(d, "rabbit", 0, true, 100);
        Assert.True(bruteKi.Item == "pele_rasgada" && bruteKi.KiSpent == 0, "Bruto cannot guide Ki");
        var ki = HarvestRules.Skin(d, "boar", 1, true, 100);
        Assert.True(ki.Item == "couro_grosso" && ki.KiSpent == 15, $"Iniciante + Ki: {ki}");
        var noKi = HarvestRules.Skin(d, "boar", 1, true, 10);
        Assert.True(noKi.Item == "couro_comum" && noKi.KiSpent == 0, "not enough Ki");
        Assert.Equal("presa_espiritual", HarvestRules.SpiritualPart(d, "boar", "presa_javali", 3, true, 100).Item, "Mestre + Ki: spiritual tusk");
        Assert.Equal("presa_javali", HarvestRules.SpiritualPart(d, "boar", "presa_javali", 2, true, 100).Item, "Adepto: plain tusks");
    }

    /// <summary>Meat count and cooked vigor by Preparo; herb quality by Erboristeria.</summary>
    [UnitTest]
    public static void Preparo_And_Erboristeria_ChangeWhatYouGet()
    {
        // Arrange
        var d = Data();
        // Act / Assert
        Assert.Equal(1, HarvestRules.MeatCount(d, 0), "one meat");
        Assert.Equal(2, HarvestRules.MeatCount(d, 2), "two meats at Adepto");
        Assert.True(Mathf.IsEqualApprox(HarvestRules.CookedVigor(d, 25, 0), 25) && Mathf.IsEqualApprox(HarvestRules.CookedVigor(d, 25, 1), 31.25f), "cooked vigor bonus");
        Assert.Equal("grama_sangue_q0", HarvestRules.HerbItem(d, 0), "herb grade 0");
        Assert.Equal("grama_sangue_q2", HarvestRules.HerbItem(d, 2), "herb grade 2");
    }

    /// <summary>The secondary roll is fixed per seed and object; more SOR only adds objects that yield it.</summary>
    [UnitTest]
    public static void Secondary_IsDeterministic_AndSorOnlyAdds()
    {
        // Arrange
        var ids = Enumerable.Range(0, 400).Select(i => $"rabbit@{i},{-i}").ToList();
        // Act
        var low = ids.Where(id => HarvestRules.HasSecondary(SeedA, id, 0.10f)).ToHashSet();
        var high = ids.Where(id => HarvestRules.HasSecondary(SeedA, id, 0.30f)).ToHashSet();
        var again = ids.Where(id => HarvestRules.HasSecondary(SeedA, id, 0.10f)).ToHashSet();
        // Assert
        Assert.True(low.SetEquals(again), "same roll every time");
        Assert.True(low.IsSubsetOf(high) && high.Count > low.Count, $"SOR adds: {low.Count} → {high.Count}");
        Assert.True(low.Count is > 20 and < 70, $"about 10%: {low.Count}/400");
    }

    /// <summary>Creature levels stay in range and fixed; a higher level is measurably stronger but keeps the same warning and recovery.</summary>
    [UnitTest]
    public static void CreatureLevel_InRange_StrongerButSameTells()
    {
        // Arrange
        var d = Data();
        var levels = Enumerable.Range(0, 60).Select(i => HarvestRules.CreatureLevel(d, SeedA, "boar", $"boar@{i},0")).ToList();
        var bt = Boar(); var rt = Rabbit();
        // Act
        var b3 = CreatureLevels.Boar(bt, d.Creatures["boar"], 3);
        var b5 = CreatureLevels.Boar(bt, d.Creatures["boar"], 5);
        var r1 = CreatureLevels.Rabbit(rt, d.Creatures["rabbit"], 1);
        var r2 = CreatureLevels.Rabbit(rt, d.Creatures["rabbit"], 2);
        // Assert
        Assert.True(levels.All(l => l is >= 3 and <= 5) && levels.Distinct().Count() == 3, "boar levels 3-5, all used");
        Assert.Equal(levels[7], HarvestRules.CreatureLevel(d, SeedA, "boar", "boar@7,0"), "fixed level");
        Assert.Equal(bt.Health + 2, b5.Health, "level 5: +2 hits");
        Assert.Equal(bt.ContactDamage + 1, b5.ContactDamage, "level 5: +1 damage");
        Assert.True(b5.ChargeSpeed > b3.ChargeSpeed, "level 5 charges faster");
        Assert.True(b5.WarnTime == b3.WarnTime && b5.RecoverTime == b3.RecoverTime, "same warning and recovery");
        Assert.True(r2.HearWalk > r1.HearWalk && r2.DashSpeed > r1.DashSpeed && r2.AlertTime == r1.AlertTime, "rabbit level 2 hears and runs better, same freeze");
        Assert.True(HarvestRules.HuntXp(d, "boar", 5, 1) > HarvestRules.HuntXp(d, "boar", 3, 1), "more XP for a stronger boar");
        Assert.True(HarvestRules.HuntXp(d, "rabbit", 1, 5) < HarvestRules.HuntXp(d, "rabbit", 1, 1), "less XP for a weaker prey");
    }

    /// <summary>The save round-trips; damaged text or another format is refused.</summary>
    [UnitTest]
    public static void Save_RoundTrips_AndRefusesDamaged()
    {
        // Arrange
        var d = new SaveData { Seed = 777, Spot = new[] { 10f, 20f }, Collected = { "flint@4,-1" }, Dead = { ["rabbit@-9,-5"] = new[] { 1f, 2f } }, PartsTaken = { "rabbit@-9,-5#hide" },
            Inventory = { new SavedItem { Id = "pele_rasgada", Count = 1 } }, Level = 2, Points = 1, Attributes = { ["vit"] = 1 }, Practice = { ["esfolar"] = 1 }, PracticeKeys = { "esfolar:rabbit@-9,-5" } };
        // Act
        var back = SaveGame.FromJson(SaveGame.ToJson(d));
        // Assert
        Assert.True(back != null && back.Seed == 777 && back.Collected.Single() == "flint@4,-1" && back.Dead["rabbit@-9,-5"][1] == 2f && back.Inventory.Single().Count == 1
            && back.Attributes["vit"] == 1 && back.PracticeKeys.Single() == "esfolar:rabbit@-9,-5", "round trip");
        Assert.True(SaveGame.ToJson(d).Contains("\"parts_taken\""), "snake_case keys");
        Assert.True(SaveGame.FromJson("{ nope") == null, "damaged");
        Assert.True(SaveGame.FromJson("{\"game\":\"spiritofmurin\",\"format\":99,\"spot\":[0,0]}") == null, "other format");
    }

    /// <summary>The guaranteed trails start at the clearing's edge and lead to a real burrow and a real boar spot.</summary>
    [UnitTest]
    public static void Trails_FromTheClearing_LeadToRealAnimals()
    {
        foreach (int seed in new[] { SeedA, SeedB })
        {
            // Arrange
            var g = Gen(seed); int R = 26;
            var all = new List<Feature>();
            for (int ty = -R; ty <= R; ty++) for (int tx = -R; tx <= R; tx++) all.AddRange(g.FeaturesAt(tx, ty));
            var animals = all.Where(f => f.Kind is FeatureKind.Burrow or FeatureKind.Boar).ToDictionary(PlainsGenerator.CreatureIdOf);
            // Act
            var trails = all.Where(f => f.Kind == FeatureKind.Track).GroupBy(f => f.Link).ToList();
            // Assert
            foreach (var t in trails)
            {
                string creature = t.Key.Split('|')[1];
                Assert.True(animals.ContainsKey(creature), $"seed {seed}: trail to {creature} has its animal");
                var target = animals[creature].Position;
                var prints = t.OrderBy(f => int.Parse(f.Id[(f.Id.IndexOf('#') + 1)..])).ToList();
                Assert.True(prints.Last().Position.DistanceTo(target) < 30, $"seed {seed}: {t.Key} ends at the animal");
                Assert.True(prints.All(f => f.Dir.Dot((target - f.Position).Normalized()) > 0.9f), $"seed {seed}: {t.Key} points to it");
            }
            foreach (var kind in new[] { FeatureKind.Burrow, FeatureKind.Boar })
                Assert.True(trails.Any(t => t.Select(f => f.Position.DistanceTo(g.StartPixel)).Min() < (g.Settings.ClearingRadius + 1.5f) * 32
                    && animals[t.Key.Split('|')[1]].Kind == kind), $"seed {seed}: a {kind} trail starts at the clearing");
        }
    }
}
