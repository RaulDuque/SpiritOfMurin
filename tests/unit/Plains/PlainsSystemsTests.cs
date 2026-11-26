using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Items;
using SpiritOfMurin.Plains;
using static SpiritOfMurin.Tests.Unit.Plains.PlainsFixtures;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>Inventory and crafting, world state, the rabbit, the boar, the blade test.</summary>
public static class PlainsSystemsTests
{
    /// <summary>Crafting the knife consumes one flint and one branch, once.</summary>
    [UnitTest]
    public static void CraftKnife_ConsumesMaterialsOnce()
    {
        // Arrange
        var inv = new Inventory(); var r = KnifeRecipe();
        inv.Add("silex"); inv.Add("galho_rijo");
        // Act
        bool first = inv.Craft(r);
        bool second = inv.Craft(r);
        // Assert
        Assert.True(first, "first craft");
        Assert.True(!second, "second craft without materials must fail");
        Assert.Equal(0, inv.Count("silex"), "flint left");
        Assert.Equal(0, inv.Count("galho_rijo"), "branch left");
        Assert.Equal(1, inv.Count("faca_pedra"), "knives");
    }

    /// <summary>Crafting with a material missing changes nothing.</summary>
    [UnitTest]
    public static void CraftKnife_MissingBranch_ChangesNothing()
    {
        // Arrange
        var inv = new Inventory(); inv.Add("silex", 2);
        // Act
        bool ok = inv.Craft(KnifeRecipe());
        // Assert
        Assert.True(!ok, "craft must fail");
        Assert.Equal(2, inv.Count("silex"), "flint untouched");
        Assert.Equal(0, inv.Count("faca_pedra"), "no knife");
    }

    /// <summary>Collecting is once per id; a carcass part is taken once.</summary>
    [UnitTest]
    public static void WorldState_CollectAndCarcassParts_AreOnce()
    {
        // Arrange
        var s = new WorldState();
        // Act / Assert
        Assert.True(s.MarkCollected("flint@4,-1"), "first pickup");
        Assert.True(!s.MarkCollected("flint@4,-1"), "second pickup refused");
        s.MarkDead("rabbit@-10,-8", new Vector2(1, 2));
        Assert.True(s.MarkPartTaken("rabbit@-10,-8", "carne_macia"), "meat once");
        Assert.True(!s.MarkPartTaken("rabbit@-10,-8", "carne_macia"), "meat twice refused");
        Assert.True(s.MarkPartTaken("rabbit@-10,-8", "pele_fina"), "hide once");
        Assert.Equal(new Vector2(1, 2), s.CarcassAt("rabbit@-10,-8"), "carcass position kept");
    }

    /// <summary>The rabbit hears walking, raises its ears, runs to the burrow in two dashes and hides.</summary>
    [UnitTest]
    public static void Rabbit_HearsWalking_FleesToBurrowAndHides()
    {
        // Arrange
        var t = Rabbit(); var burrow = new Vector2(0, 0);
        var b = new RabbitBrain(t, burrow, 3);
        Vector2 pos = new(30, 0), player = new(100, 0);
        // Act: player walking 70 px away (inside HearWalk)
        bool sawAlert = false, sawPause = false; float t1 = 0;
        for (int i = 0; i < 300 && b.State != RabbitState.Hidden; i++, t1 += Dt)
        {
            pos = Move(pos, b.Update(Dt, pos, player, t.HearWalk));
            sawAlert |= b.State == RabbitState.Alert && b.EarsUp;
            sawPause |= b.State == RabbitState.Pause;
        }
        // Assert
        Assert.True(sawAlert, "ears up / alert first");
        Assert.True(sawPause, "two dashes (pause between)");
        Assert.Equal(RabbitState.Hidden, b.State, "hidden in the burrow");
        Assert.True(pos.DistanceTo(burrow) < 5, $"ended at the burrow ({pos})");
        Assert.True(!b.Visible, "not visible while hidden");
        Assert.True(b.TakeHit(1) == HitResult.None, "cannot be hit inside the burrow");
    }

    /// <summary>A sneaking player at the same distance is not heard.</summary>
    [UnitTest]
    public static void Rabbit_SneakingPlayer_IsNotHeard()
    {
        // Arrange
        var t = Rabbit(); var b = new RabbitBrain(t, Vector2.Zero, 3);
        Vector2 pos = Vector2.Zero, player = new(40, 0);
        // Act
        for (int i = 0; i < 120; i++) pos = Move(pos, b.Update(Dt, pos, player, t.HearSneak));
        // Assert (it may hop, but never alerts while the player stays farther than HearSneak)
        Assert.True(b.State == RabbitState.Graze || pos.DistanceTo(player) < t.HearSneak, $"state {b.State}");
        Assert.Equal(0, b.Flights, "no flight");
    }

    /// <summary>After hiding, the rabbit peeks out again; one knife hit kills it.</summary>
    [UnitTest]
    public static void Rabbit_ComesBackOut_AndOneHitKills()
    {
        // Arrange
        var t = Rabbit(); var b = new RabbitBrain(t, Vector2.Zero, 3);
        Vector2 pos = new(10, 0), far = new(5000, 0);
        b.Update(Dt, pos, new Vector2(20, 0), t.HearLoud);
        for (int i = 0; i < 120; i++) pos = Move(pos, b.Update(Dt, pos, far, 0));
        Assert.Equal(RabbitState.Hidden, b.State, "hidden");
        // Act
        for (int i = 0; i < (int)(t.HideTime / Dt) + 5; i++) b.Update(Dt, pos, far, 0);
        // Assert
        Assert.True(b.Visible, "peeked out");
        Assert.Equal(HitResult.Killed, b.TakeHit(1), "one knife hit");
    }

    private static (BoarBrain B, Vector2 Pos) WarnedBoar(out BoarTuning t)
    {
        t = Boar();
        var b = new BoarBrain(t, Vector2.Zero, 5);
        Vector2 pos = Vector2.Zero;
        b.Update(Dt, pos, new Vector2(80, 0), true);
        return (b, pos);
    }

    /// <summary>Warning lasts WarnTime; the head tracks, then the direction is fixed before the charge.</summary>
    [UnitTest]
    public static void Boar_WarnsForWarnTime_ThenChargesInFixedDirection()
    {
        // Arrange
        var (b, pos) = WarnedBoar(out var t);
        Assert.Equal(BoarState.Warn, b.State, "player in range starts the warning");
        // Act: during the warning the player moves down; after the lock the direction must stop following
        float time = 0; Vector2 player = new(80, 0); Vector2 lockedDir = Vector2.Zero;
        while (b.State == BoarState.Warn && time < 3)
        {
            player += new Vector2(0, 40 * Dt);
            b.Update(Dt, pos, player, true); time += Dt;
            if (b.DirectionLocked && lockedDir == Vector2.Zero) lockedDir = b.Facing;
        }
        // Assert
        Assert.True(Mathf.Abs(time - t.WarnTime) < 0.05f, $"warning lasted {time:0.00} s");
        Assert.Equal(BoarState.Charge, b.State, "charges after the warning");
        Assert.True(lockedDir != Vector2.Zero && b.Facing.IsEqualApprox(lockedDir), "direction fixed at the lock");
        Assert.True(Mathf.Abs(b.Facing.AngleTo((player - pos).Normalized())) > 0.05f, "no longer tracking the player");
    }

    /// <summary>Stepping aside avoids the straight charge; then the boar recovers for RecoverTime and hits land.</summary>
    [UnitTest]
    public static void Boar_SidestepAvoidsCharge_RecoveryIsTheOpening()
    {
        // Arrange
        var (b, pos) = WarnedBoar(out var t);
        Vector2 player = new(80, 0);
        Assert.Equal(HitResult.Deflected, b.TakeHit(1), "warning: hide deflects");
        while (b.State == BoarState.Warn) b.Update(Dt, pos, player, true);
        Assert.Equal(HitResult.Deflected, b.TakeHit(1), "charge: hide deflects");
        // Act: the player steps 30 px to the side as the charge starts
        player += new Vector2(0, 30);
        bool hit = false; float recover = 0;
        while (b.State == BoarState.Charge) { var s = b.Update(Dt, pos, player, true); hit |= s.HitPlayer; pos = Move(pos, s.Velocity); }
        Assert.True(!hit, "the sidestep avoided the charge");
        Assert.True(pos.X > 150, $"charged in a straight line ({pos})");
        Assert.True(Mathf.Abs(pos.Y) < 0.5f, "straight: no turning");
        Assert.Equal(BoarState.Recover, b.State, "recovering");
        Assert.Equal(HitResult.Damaged, b.TakeHit(1), "recovery: the hit lands");
        while (b.State == BoarState.Recover && recover < 5) { b.Update(Dt, pos, new Vector2(5000, 0), true); recover += Dt; }
        // Assert
        Assert.True(Mathf.Abs(recover - (t.RecoverTime + t.HitFlinch)) < 0.06f, $"recovery lasted {recover:0.00} s");
        Assert.Equal(t.Health - 1, b.Health, "health after one hit");
    }

    /// <summary>Standing in the line, the charge hits once when the body reaches the player.</summary>
    [UnitTest]
    public static void Boar_ChargeHitsPlayerInLine_OnlyOnContact()
    {
        // Arrange
        var (b, pos) = WarnedBoar(out var t);
        Vector2 player = new(80, 0);
        while (b.State == BoarState.Warn) b.Update(Dt, pos, player, true);
        // Act
        int hits = 0; float distAtHit = -1;
        while (b.State == BoarState.Charge)
        {
            var s = b.Update(Dt, pos, player, true);
            if (s.HitPlayer) { hits++; distAtHit = pos.DistanceTo(player); }
            pos = Move(pos, s.Velocity);
        }
        // Assert
        Assert.Equal(1, hits, "one hit per charge");
        Assert.True(distAtHit >= 0 && distAtHit < t.ChargeHitRadius, $"hit at {distAtHit:0.0} px (contact)");
    }

    /// <summary>The boar dies after Health hits during its openings.</summary>
    [UnitTest]
    public static void Boar_DiesAfterHealthHitsInOpenings()
    {
        // Arrange
        var t = Boar(); var b = new BoarBrain(t, Vector2.Zero, 1);
        HitResult last = HitResult.None; int n = 0;
        // Act: hit it only while vulnerable (rooting, then each recovery)
        Vector2 pos = Vector2.Zero, player = new(60, 0);
        for (int i = 0; i < 6000 && b.State != BoarState.Dead; i++)
        {
            if (b.Vulnerable && b.State == BoarState.Recover) { last = b.TakeHit(1); n++; }
            var s = b.Update(Dt, pos, player, false);
            pos = Move(pos, s.Velocity);
            if (b.State == BoarState.Charge) player = pos + b.Facing * 60;
        }
        // Assert
        Assert.Equal(BoarState.Dead, b.State, "dead");
        Assert.Equal(HitResult.Killed, last, "last hit kills");
        Assert.True(n >= t.Health, $"took {n} hits");
    }

    /// <summary>The blade reaches what is in front and within reach, not behind nor beyond.</summary>
    [UnitTest]
    public static void BladeArc_ReachesFrontOnly()
    {
        // Arrange
        Vector2 o = Vector2.Zero, right = Vector2.Right;
        // Act / Assert
        Assert.True(Player.InBladeArc(o, right, 22, 75, new Vector2(25, 0), 6), "in front, within reach + radius");
        Assert.True(!Player.InBladeArc(o, right, 22, 75, new Vector2(40, 0), 6), "too far for the knife");
        Assert.True(Player.InBladeArc(o, right, 34, 75, new Vector2(40, 0), 6), "the sword reaches farther");
        Assert.True(!Player.InBladeArc(o, right, 22, 75, new Vector2(-20, 0), 6), "behind");
    }

    /// <summary>The Vida bar's percentage: exact at full and empty, rounded up so a sliver of life never reads 0%.</summary>
    [UnitTest]
    public static void LifePercent_RoundsUpAndClamps()
    {
        // Arrange / Act / Assert
        Assert.Equal(100, PlainsHud.LifePercent(10, 10), "full");
        Assert.Equal(80, PlainsHud.LifePercent(8, 10), "8 of 10");
        Assert.Equal(1, PlainsHud.LifePercent(0.05f, 10), "a sliver");
        Assert.Equal(0, PlainsHud.LifePercent(0, 10), "empty");
        Assert.Equal(100, PlainsHud.LifePercent(12, 10), "over the maximum clamps");
    }
}
