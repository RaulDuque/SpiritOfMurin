using System;
using System.Linq;
using System.Reflection;
using Godot;

namespace SpiritOfMurin.Tests.Unit;

/// <summary>Marks a public static method as a unit test (no arguments; throws to fail).</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class UnitTestAttribute : Attribute { }

/// <summary>Small assertion helpers for the unit tests.</summary>
public static class Assert
{
    /// <summary>Fails with <paramref name="message"/> when <paramref name="condition"/> is false.</summary>
    public static void True(bool condition, string message) { if (!condition) throw new Exception(message); }

    /// <summary>Fails when the two values differ.</summary>
    public static void Equal<T>(T expected, T actual, string what)
    {
        if (!Equals(expected, actual)) throw new Exception($"{what}: expected {expected}, got {actual}");
    }
}

/// <summary>
/// Runs every <see cref="UnitTestAttribute"/> method in this assembly and quits with 0 when all pass.
/// The project's current runner (gdUnit4's C# runner was evaluated and not installed; see the
/// 2026-11-26 report). Run: <c>godot --headless --path . res://tests/unit/UnitRunner.tscn</c>
/// </summary>
public partial class UnitRunner : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        var tests = typeof(UnitRunner).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.GetCustomAttribute<UnitTestAttribute>() != null)
            .OrderBy(m => m.DeclaringType!.Name).ThenBy(m => m.Name).ToList();
        int fails = 0;
        foreach (var m in tests)
        {
            string name = $"{m.DeclaringType!.Name}.{m.Name}";
            try { m.Invoke(null, null); GD.Print($"OK   {name}"); }
            catch (TargetInvocationException e) { fails++; GD.Print($"FAIL {name} — {e.InnerException?.Message}"); }
        }
        GD.Print($"{tests.Count - fails}/{tests.Count} OK");
        GetTree().Quit(fails == 0 && tests.Count > 0 ? 0 : 1);
    }
}
