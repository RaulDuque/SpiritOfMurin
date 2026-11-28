using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The traveller's tunic (T18, item <c>tunica_viajante</c>): worn raw linen of the same cut as the green tunic, a crossed
/// collar in a 1 px diagonal, the sash in worn jade tied in a knot (no gold buckle), rolled cuffs, a patch on the skirt
/// and an uneven hem. Light robe and jade sash, as the art bible keeps for the swordsman (§4.4, §5.1).
/// <b>Where to change it:</b>
/// <list type="bullet">
/// <item>the sash: rows 8–10 (front and back), 7–9 (profile): <c>t</c> worn jade, <c>n</c> the knot, <c>d</c> the fold.
/// To <b>widen the sash</b>, turn row 11 (front/back) or row 10 (profile) into <c>t</c> as well, in the three views;</item>
/// <item>the collar: the diagonal of <c>m</c> in rows 0–6 of the front;</item>
/// <item>the patch: the <c>mm</c> pair in rows 14–15; the hem: the last row;</item>
/// <item>the colours: <see cref="Colors"/>; the cuffs: <see cref="Cuff"/>.</item>
/// </list>
/// </summary>
public sealed class TunicaViajanteArt : MatrixRoupaArt
{
    /// <summary>The colours by letter: linen shade, base, light; worn jade sash, its fold; the knot.</summary>
    public static readonly Dictionary<char, Color> Colors = new()
    {
        ['m'] = new("#8a846f"), // sombra do linho
        ['r'] = new("#b3ad97"), // linho cru
        ['w'] = new("#d6d2bf"), // luz do linho
        ['t'] = new("#4f7a64"), // faixa em jade gasto
        ['d'] = new("#3a5a4b"), // dobra da faixa
        ['n'] = new("#6e5a40"), // nó da faixa
    };

    private static readonly string[] FrontRows =
    {
        "....mmmmrrrrrmww....",
        "...mmmmrrrrrmrrww...",
        "...mmmmrrrrmrrrww...",
        "...mmmmrrrmrrrrww...",
        "...mmmmrrmrrrrrww...",
        "...mmmmrmrrrrrrww...",
        "...mmmmmrrrrrrrww...",
        "....mmmmrrrrrrww....",
        "....ttttttttnntt....",
        "....ttttttttnntt....",
        "....dddddddddddd....",
        "....mmmmmrrrwrrr....",
        "...mmmmrmrrrwrrrw...",
        "...mmmmrmrrrwrrrw...",
        "..mmmmrrmrrrwmmrww..",
        "..mmmmrmrrrrrmmrrw..",
        "..mmmmrmrrrrrwrrrw..",
        ".mmmmrrmrrrrrwrrrww.",
        ".mmmmrrmrrrrrwrrrww.",
        ".mmmmrrmrrrrrwrrrww.",
        ".mmmrrrmmmrrrmmmrrr.",
    };

    private static readonly string[] BackRowsM =
    {
        "....mmmmrrrrrrww....",
        "...mmmmrrrrrrrrww...",
        "...mmmmrrrrrrrrww...",
        "...mmmmrrrrrrrrww...",
        "...mmmmrrrrrrrrww...",
        "...mmmmrrrrrrrrww...",
        "...mmmmrrrrrrrrww...",
        "....mmmmrrrrrrww....",
        "....tttttttttttt....",
        "....tttttttttttt....",
        "....dddddddddddd....",
        "....mmmmmrrrwrww....",
        "...mmmmrmrrrwrrww...",
        "...mmmmrmrrrwrrww...",
        "..mmmmrrmrrrwrrrww..",
        "..mmmmrmrrrrrwrrww..",
        "..mmmmrmrrrrrwrrww..",
        ".mmmmrrmrrrrrwrrrww.",
        ".mmmmrrmrrrrrwrrrww.",
        ".mmmmrrmrrrrrwrrrww.",
        ".mmmrrrmmmrrrmmmrrr.",
    };

    private static readonly string[] SideRows =
    {
        "....mmmrrrrw....",
        "...mmmmrrrrmw...",
        "...mmmmmmmrmw...",
        "...mmmmmmmrmw...",
        "...mmmmmmmrmr...",
        "...mmmmmmmrmr...",
        "...mmmmmmmrrr...",
        "....ttttttnt....",
        "....ttttttnt....",
        "....dddddddd....",
        "....mmmmmrrw....",
        "...mmmmmmrrrw...",
        "...mmmmmmrrrw...",
        "..mmmmmmmrrrww..",
        "..mmmmmmmrmmww..",
        "..mmmmmmmrmmww..",
        ".mmmmmmmmrrrrww.",
        ".mmmmmmmmrrrrww.",
        ".mmmmmmmmrrrrww.",
        ".mmmmmmmmrrrrrw.",
        ".mmmrrrmmmrrrmm.",
    };

    private static readonly Color KnotLine = new("#6e5a40");

    /// <inheritdoc />
    protected override string[] Front => FrontRows;
    /// <inheritdoc />
    protected override string[] BackRows => BackRowsM;
    /// <inheritdoc />
    protected override string[] Side => SideRows;
    /// <inheritdoc />
    protected override IReadOnlyDictionary<char, Color> Palette => Colors;
    /// <inheritdoc />
    protected override Color Cloth => Colors['r'];
    /// <inheritdoc />
    protected override Color ClothShade => Colors['m'];
    /// <inheritdoc />
    protected override Color Cuff => Colors['m'];
    /// <inheritdoc />
    protected override Color CuffSide => Colors['m'];
    /// <inheritdoc />
    protected override Color CuffLine => KnotLine;
}
