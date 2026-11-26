using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Items;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>Factory functions for the Planície unit tests (no file I/O: the values mirror the shipped data files).</summary>
public static class PlainsFixtures
{
    /// <summary>The two seeds the delivery is tested with.</summary>
    public const int SeedA = 20261126, SeedB = 777;

    /// <summary>Generation settings equal to plains_worldgen.json at the time of writing.</summary>
    public static PlainsGenSettings Settings() => new()
    {
        PerHabitat = new Dictionary<string, HabitatChances>
        {
            ["bank"] = new() { Flint = 0.07f, Branch = 0.05f, Pine = 0.05f, Boulder = 0.035f },
            ["grass"] = new() { Flint = 0.004f, Branch = 0.008f, Pine = 0.022f, Boulder = 0.014f, Bush = 0.03f },
            ["bushes"] = new() { Branch = 0.01f, Pine = 0.01f, Boulder = 0.02f, Bush = 0.17f, Burrow = 0.035f },
            ["mud"] = new() { Boulder = 0.01f, Bush = 0.02f, Rooted = 0.035f, Boar = 0.006f },
        },
    };

    /// <summary>A generator for a seed with the fixture settings.</summary>
    public static PlainsGenerator Gen(int seed) => new(seed, Settings());

    /// <summary>The knife recipe as in plains_items.json.</summary>
    public static RecipeDef KnifeRecipe() => new() { Id = "faca_pedra", Output = "faca_pedra", Count = 1, Inputs = new() { ["silex"] = 1, ["galho_rijo"] = 1 } };

    /// <summary>Boar tuning with the shipped values.</summary>
    public static BoarTuning Boar() => new() { WarnTime = 0.8f, TrackFraction = 0.55f, ChargeSpeed = 240f, ChargeDistance = 170f, RecoverTime = 1.0f, Cooldown = 0.35f, AggroRadius = 110f, LeashRadius = 260f, ChargeHitRadius = 15f, ContactDamage = 2, Health = 5 };

    /// <summary>Rabbit tuning with the shipped values.</summary>
    public static RabbitTuning Rabbit() => new() { HearWalk = 95f, HearSneak = 26f, HearStill = 14f, HearLoud = 130f, AlertTime = 0.35f, DashSpeed = 165f, DashPause = 0.18f, HideTime = 4.5f, GrazeRadius = 44f, GrazeSpeed = 22f, Health = 1 };

    /// <summary>Fixed simulation step (60 Hz).</summary>
    public const float Dt = 1f / 60f;

    /// <summary>Moves a point toward a velocity for one step.</summary>
    public static Vector2 Move(Vector2 p, Vector2 v) => p + v * Dt;
}
