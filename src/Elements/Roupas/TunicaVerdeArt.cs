using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The approved robe (the creator's drawing, T18 reference): ivory tunic with jade trim, the deep-green sash at the waist
/// with its gold buckle, and jade-cuffed sleeves. Worn when nothing else is (item <c>tunica_verde</c>).
/// <b>Where to change it:</b>
/// <list type="bullet">
/// <item>the sash at the waist: rows 8–10 of each matrix (<c>t</c> = jade band, <c>g</c> = buckle, <c>d</c> = the dark
/// fold under it). To widen it, turn one more row of <c>m</c>/<c>r</c> just below into <c>d</c> or <c>t</c>, in the three
/// views alike;</item>
/// <item>the hem: the last two rows; the skirt's swing moves rows 16 and below;</item>
/// <item>the sleeves: the colours <see cref="Cloth"/>, <see cref="ClothShade"/> and the cuffs;</item>
/// <item>the colours: <see cref="Colors"/> (one letter per colour).</item>
/// </list>
/// </summary>
public sealed class TunicaVerdeArt : MatrixRoupaArt
{
    /// <summary>The colours by letter: shade (ocre), base (ivory), light, jade trim, its light, deep green, gold buckle, outline.</summary>
    public static readonly Dictionary<char, Color> Colors = new()
    {
        ['i'] = new("#17130e"), // contorno
        ['t'] = new("#2f7a53"), // faixa principal / debrum (verde jade)
        ['j'] = new("#68c28e"), // brilho verde jade da faixa e fita
        ['d'] = new("#1c4f35"), // sombra profunda da faixa e dobra (verde esmeralda escuro)
        ['m'] = new("#cbb37c"), // sombra da túnica (ocre quente)
        ['r'] = new("#f5eedc"), // túnica base (marfim dourado claro)
        ['w'] = new("#fffbf0"), // luz da túnica
        ['g'] = new("#f5cb5c"), // placa/fivela dourada
    };

    private static readonly string[] FrontRows =
    {
        "....mmtmrrrrrtww....",
        "...mmmtrrrrrrtrww...",
        "...mmmmtrrrrtrrww...",
        "...mmddtrrrrtrrww...",
        "...mmddrtrrtrrrww...",
        "...mmddrtrrtrrrww...",
        "...mmmmrrttrrrrww...",
        "....mmmmrttrrrww....",
        "....ttttttttggtt....",
        "....ttttttttggtt....",
        "....dddddddddddd....",
        "....mmmmmrrrwrtt....",
        "...mmmmrmrrrwrttw...",
        "...mmmmrmrrrwrttw...",
        "..mmmmrrmrrrwrttww..",
        "..mmmmrmrrrrrwrttw..",
        "..mmmmrmrrrrrwrttw..",
        ".mmmmrrmrrrrrwrjjww.",
        ".mmmmrrmrrrrrwrjjww.",
        ".tttttttttttttttttt.",
        ".dddddddddmmmmmmmmm.",
    };

    private static readonly string[] BackRowsM =
    {
        "....mmmmrrrrrrww....",
        "...mmmdddddddddww...",
        "...mmmmrdddddrrww...",
        "...mmddrrrrrrrrww...",
        "...mmddrrrrrrrrww...",
        "...mmddrrrrrrrrww...",
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
        ".tttttttttttttttttt.",
        ".dddddddddmmmmmmmmm.",
    };

    private static readonly string[] SideRows =
    {
        "....mmmrrrrw....",
        "...mmmmrrrrtw...",
        "...mmmmmmmrtw...",
        "...mmddmmmrtw...",
        "...mmddmmmrtr...",
        "...mmddmmmrtr...",
        "...mmmmmmmrtr...",
        "....ttttttgt....",
        "....ttttttgt....",
        "....dddddddd....",
        "....mmmmmrtw....",
        "...mmmmmmrtjw...",
        "...mmmmmmrtjw...",
        "..mmmmmmmrtjww..",
        "..mmmmmmmrtjww..",
        "..mmmmmmmrtjww..",
        ".mmmmmmmmrrtjww.",
        ".mmmmmmmmrrtjww.",
        ".mmmmmmmmrrddww.",
        ".tttttttttttttt.",
        ".dddddddmmmmmmm.",
    };

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
    protected override Color Cuff => Colors['t'];
    /// <inheritdoc />
    protected override Color CuffSide => Colors['d'];
    /// <inheritdoc />
    protected override Color CuffLine => new("#d49b27");
}
