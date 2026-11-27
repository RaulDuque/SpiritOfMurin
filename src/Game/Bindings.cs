using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Game;

/// <summary>
/// The player's controls: every action has two inputs (a main one and a second one — a key or a mouse
/// button), as in the HTML prototype's Teclas screen. The defaults are the project's input map; changes
/// are kept in the preferences (<c>bindings</c>) and applied to the <see cref="InputMap"/> at once, so a
/// new input works the moment it is chosen. Every text that names a key is written with tokens —
/// <c>[cut]</c> is the main input, <c>[cut+]</c> both — and goes through <see cref="F"/>, so the HUD, the
/// map, the book and the combat hints follow a change.
/// </summary>
public static class Bindings
{
    /// <summary>A rebindable action: the input map name and the name shown in Portuguese.</summary>
    public readonly record struct ActionInfo(string Id, string Name);

    /// <summary>The actions shown on the Controles screen, in order.</summary>
    public static readonly ActionInfo[] Actions =
    {
        new("move_up", "Andar: cima"), new("move_down", "Andar: baixo"), new("move_left", "Andar: esquerda"), new("move_right", "Andar: direita"),
        new("cut", "Corte"), new("step", "Passo"), new("ki", "Ki (onda; esfolar com Ki)"), new("lock", "Travar no alvo / focar"),
        new("use", "Examinar / pegar"), new("heal", "Bálsamo / erva da bolsa"), new("sneak", "Andar quieto (Planície)"), new("reinforce", "Modificador do Reforço de Ki (com o corte)"),
        new("inventory", "Inventário"), new("sheet", "Ficha"), new("book", "Livro de Técnicas"), new("map", "Mapa"),
        new("menu", "Pausa"), new("fullscreen", "Tela cheia"),
    };

    private static Dictionary<string, string[]>? _defaults;

    /// <summary>Raised after any change (assign, restore), so texts can be rebuilt.</summary>
    public static event Action? Changed;

    /// <summary>Reads the defaults from the input map once and applies the saved bindings (idempotent).</summary>
    public static void Init(Prefs? prefs = null)
    {
        if (_defaults == null)
        {
            _defaults = new();
            foreach (var a in Actions) _defaults[a.Id] = Current(a.Id);
        }
        var saved = (prefs ?? Prefs.Load()).Bindings;
        foreach (var a in Actions)
            Apply(a.Id, saved.TryGetValue(a.Id, out var s) && s.Length == 2 ? s : _defaults[a.Id]);
        Changed?.Invoke();
    }

    /// <summary>The action's name in Portuguese.</summary>
    public static string NameOf(string id) => Actions.FirstOrDefault(a => a.Id == id).Name ?? id;

    /// <summary>The two inputs of an action now, encoded ("k:74", "m:1" or "").</summary>
    public static string[] Current(string id)
    {
        var ev = InputMap.HasAction(id) ? InputMap.ActionGetEvents(id) : new Godot.Collections.Array<InputEvent>();
        var codes = ev.Select(Encode).Where(c => c.Length > 0).Take(2).ToList();
        while (codes.Count < 2) codes.Add("");
        return codes.ToArray();
    }

    /// <summary>The default inputs of an action (before any change).</summary>
    public static string[] Default(string id) => _defaults != null && _defaults.TryGetValue(id, out var d) ? d : Current(id);

    /// <summary>Encodes an input event ("k:physical keycode" or "m:button"); "" for anything else.</summary>
    public static string Encode(InputEvent? e) => e switch
    {
        InputEventKey k => $"k:{(long)(k.PhysicalKeycode != Key.None ? k.PhysicalKeycode : k.Keycode)}",
        InputEventMouseButton m when m.ButtonIndex is not (MouseButton.WheelUp or MouseButton.WheelDown or MouseButton.WheelLeft or MouseButton.WheelRight) => $"m:{(long)m.ButtonIndex}",
        _ => "",
    };

    /// <summary>Decodes an input ("" → null).</summary>
    public static InputEvent? Decode(string code)
    {
        if (code.StartsWith("k:") && long.TryParse(code[2..], out long k)) return new InputEventKey { PhysicalKeycode = (Key)k, Device = -1 };
        if (code.StartsWith("m:") && long.TryParse(code[2..], out long m)) return new InputEventMouseButton { ButtonIndex = (MouseButton)m, Device = -1 };
        return null;
    }

    private static void Apply(string id, string[] codes)
    {
        if (!InputMap.HasAction(id)) InputMap.AddAction(id, 0.2f);
        InputMap.ActionEraseEvents(id);
        foreach (var c in codes) if (Decode(c) is { } e) InputMap.ActionAddEvent(id, e);
    }

    /// <summary>A short Portuguese label of an encoded input ("J", "Espaço", "clique esq.").</summary>
    public static string Label(string code)
    {
        if (code.StartsWith("m:") && long.TryParse(code[2..], out long m))
            return (MouseButton)m switch
            {
                MouseButton.Left => "clique esq.", MouseButton.Right => "clique dir.", MouseButton.Middle => "botão do meio",
                MouseButton.Xbutton1 => "botão lateral 1", MouseButton.Xbutton2 => "botão lateral 2", _ => $"botão {m}",
            };
        if (code.StartsWith("k:") && long.TryParse(code[2..], out long k))
        {
            var key = (Key)k;
            return key switch
            {
                Key.Space => "Espaço", Key.Escape => "Esc", Key.Shift => "Shift", Key.Ctrl => "Ctrl", Key.Alt => "Alt", Key.Tab => "Tab",
                Key.Enter => "Enter", Key.Backspace => "Apagar", Key.Up => "Seta cima", Key.Down => "Seta baixo", Key.Left => "Seta esq.",
                Key.Right => "Seta dir.", Key.Capslock => "Caps", Key.Kp0 or Key.Kp1 or Key.Kp2 or Key.Kp3 or Key.Kp4 or Key.Kp5 or Key.Kp6 or Key.Kp7 or Key.Kp8 or Key.Kp9 => "Num " + (k - (long)Key.Kp0),
                _ => OS.GetKeycodeString(key),
            };
        }
        return "—";
    }

    /// <summary>The label of an action's input in a slot (0 main, 1 second); "—" when empty.</summary>
    public static string Label(string id, int slot) { var c = Current(id); return c[slot].Length > 0 ? Label(c[slot]) : "—"; }

    /// <summary>The main input of an action ("J").</summary>
    public static string Key1(string id) { var c = Current(id); return c[0].Length > 0 ? Label(c[0]) : c[1].Length > 0 ? Label(c[1]) : "?"; }

    /// <summary>Both inputs of an action ("J/clique esq.").</summary>
    public static string Both(string id) { var c = Current(id).Where(x => x.Length > 0).Select(x => Label(x)).ToList(); return c.Count == 0 ? "?" : string.Join("/", c); }

    /// <summary>The four walking keys together ("WASD" when they are single letters).</summary>
    public static string Walk()
    {
        var k = new[] { "move_up", "move_left", "move_down", "move_right" }.Select(Key1).ToList();
        return k.All(x => x.Length == 1) ? string.Concat(k) : string.Join("/", k);
    }

    private static readonly Regex Token = new(@"\[(walk|[a-z_]+)(\+?)\]", RegexOptions.Compiled);

    /// <summary>Writes the current inputs into a text: <c>[cut]</c> → "J", <c>[cut+]</c> → "J/clique esq.", <c>[walk]</c> → "WASD".</summary>
    public static string F(string text) => Token.Replace(text, m =>
    {
        string id = m.Groups[1].Value;
        if (id == "walk") return Walk();
        if (!InputMap.HasAction(id)) return m.Value;
        return m.Groups[2].Value == "+" ? Both(id) : Key1(id);
    });

    /// <summary>Pairs of actions that may share an input on purpose (a modifier held together with other keys).</summary>
    private static readonly HashSet<(string, string)> Shared = new() { ("sneak", "reinforce"), ("reinforce", "sneak") };

    /// <summary>Who uses an input now: (action, slot), or null. <paramref name="except"/> ignores an action and the ones allowed to share with it.</summary>
    public static (string Id, int Slot)? Owner(string code, string except = "")
    {
        foreach (var a in Actions)
        {
            if (a.Id == except || Shared.Contains((except, a.Id))) continue;
            var c = Current(a.Id); for (int s = 0; s < 2; s++) if (c[s] == code) return (a.Id, s);
        }
        return null;
    }

    /// <summary>
    /// Gives an input to an action's slot. When another action already uses it, the two trade places (that
    /// action gets this slot's old input); it is refused when that would leave the other action with no
    /// main input. Returns the message to show, and whether it was applied.
    /// </summary>
    public static (bool Ok, string Message) Assign(string id, int slot, string code)
    {
        if (code.Length == 0) return (false, "Entrada não reconhecida.");
        var mine = Current(id);
        string old = mine[slot];
        if (old == code) return (true, $"{NameOf(id)} continua em {Label(code)}.");
        string msg = $"{NameOf(id)}: {Label(code)}.";
        if (Owner(code, id) is { } o)
        {
            var theirs = Current(o.Id);
            if (o.Slot == 0 && old.Length == 0 && theirs[1].Length == 0)
                return (false, $"{Label(code)} é a única entrada de \"{NameOf(o.Id)}\". Troque lá primeiro.");
            theirs[o.Slot] = old;
            if (theirs[0].Length == 0) { theirs[0] = theirs[1]; theirs[1] = ""; }
            if (o.Id == id) { mine = theirs; }
            else Apply(o.Id, theirs);
            msg = old.Length > 0
                ? $"{Label(code)} era de \"{NameOf(o.Id)}\": as duas trocaram ({NameOf(o.Id)} agora em {Label(old)})."
                : $"{Label(code)} era de \"{NameOf(o.Id)}\" e saiu de lá.";
        }
        if (mine[1 - slot] == code) mine[1 - slot] = old; // the same action's other slot had it: they trade
        mine[slot] = code;
        if (mine[0].Length == 0) { mine[0] = mine[1]; mine[1] = ""; }
        Apply(id, mine);
        Save();
        Changed?.Invoke();
        return (true, msg);
    }

    /// <summary>Back to the project's defaults (and saved).</summary>
    public static void Restore()
    {
        foreach (var a in Actions) Apply(a.Id, Default(a.Id));
        Save();
        Changed?.Invoke();
    }

    /// <summary>Writes the bindings into the preferences.</summary>
    public static void Save()
    {
        var p = Prefs.Load();
        p.Bindings = Actions.ToDictionary(a => a.Id, a => Current(a.Id));
        p.Save();
    }
}
