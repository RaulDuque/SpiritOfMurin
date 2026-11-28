using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The swordsman's head (T18): the face, the hair, the bun on top with its jade band, and the ribbon that flutters from
/// the band. It is the layer under the hat. The pictures are the 12×14 matrices below (one letter per pixel, colours in
/// <see cref="Palette"/>).
/// <b>Where to change it:</b> the bun is in rows 0–2 of each matrix, the band in row 3, the face in the rows below;
/// the ribbon's length and flutter in <see cref="Draw"/> (<c>tail</c>); the colours in <see cref="Palette"/>.
/// A hat hides the top <see cref="BunRows"/> rows (the bun) or also the band (<see cref="BandRows"/>), as its
/// <c>CapaceteArt.HairCover</c> says.
/// </summary>
public static class CabecaArt
{
    /// <summary>Rows of the bun (hidden by a hat that covers the bun) and of the bun plus the band.</summary>
    public const int BunRows = 3, BandRows = 4;

    /// <summary>The colours by letter: outline/eyes, hair, its volume and light, skin (shade, base, light), the band (jade, its light).</summary>
    public static readonly Dictionary<char, Color> Palette = new()
    {
        ['i'] = new("#17130e"), // contorno / olhos
        ['h'] = new("#1c181d"), // cabelo escuro
        ['v'] = new("#3b333d"), // volume do cabelo
        ['a'] = new("#5c505e"), // brilho no topo do cabelo
        ['s'] = new("#c48b6c"), // sombra de pele
        ['p'] = new("#e2b58e"), // pele base
        ['l'] = new("#f3cca4"), // luz da pele
        ['t'] = new("#2f7a53"), // faixa do coque (verde jade)
        ['j'] = new("#68c28e"), // brilho verde jade da faixa e da fita
    };

    /// <summary>The head seen from the front.</summary>
    public static readonly string[] Front =
    {
        "......hh....",
        ".....hvvh...",
        ".....hhaa...",
        "....ttjtt...",
        "..hhvvvvhh..",
        ".hvvahhhhvh.",
        ".hvhhlphhvh.",
        ".hhhplllphh.",
        ".hhpplllphh.",
        ".hhisssipph.",
        ".hhplpllpvh.",
        ".hhsppppph..",
        "..hsspppsh..",
        "....spps....",
    };

    /// <summary>The head seen from behind.</summary>
    public static readonly string[] BackRows =
    {
        "......hh....",
        ".....hvvh...",
        ".....hvah...",
        "....ttjtt...",
        "..hhvvvvhh..",
        ".hvvavvhvvh.",
        ".hvahhvvhvh.",
        ".hhvhvvhhvh.",
        ".hhvhvhhhvh.",
        ".hhvhhvhhvh.",
        ".hhhhvvhhvh.",
        "..hhhvhhvh..",
        "...hhhhvh...",
        ".....hhh....",
    };

    /// <summary>The head in profile (looking right; mirrored for the left).</summary>
    public static readonly string[] Side =
    {
        ".....hh.....",
        "....hvah....",
        "....hvvh....",
        "...ttjjtt...",
        "..hhvvvvhh..",
        ".hvvavhhvvh.",
        ".hvahhhhhlp.",
        ".hvhhsppplp.",
        ".hvhhpppllp.",
        ".hhhhsppih..",
        "..hvhppppll.",
        "..hhhssppp..",
        "...hhssppp..",
        ".....sspp...",
    };

    /// <summary>
    /// Draws the ribbon and the head. <paramref name="hiddenRows"/> rows from the top are left out (0 bare head,
    /// <see cref="BunRows"/> the bun under a hat, <see cref="BandRows"/> the bun and the band); the ribbon is left
    /// out with the band.
    /// </summary>
    public static void Draw(PixelPainter p, FighterPose f, int hiddenRows = 0)
    {
        int hy = f.HeadY;
        if (hiddenRows < BandRows)
        {
            float speedFactor = f.Moving ? 1.5f : 1f;
            float flutter = Mathf.Sin(f.Time * 6f * speedFactor);
            float flutterTip = Mathf.Sin(f.Time * 6f * speedFactor - 1.1f);
            float tail = f.Profile ? -f.S * 9 : f.Back ? 8 : -8;
            p.Line(f.LeanX, hy + 3, f.LeanX + tail / 2, hy + 4 + flutter * 0.5f, Palette['t'], 1);
            p.Line(f.LeanX + tail / 2, hy + 4 + flutter * 0.5f, f.LeanX + tail + flutterTip, hy + 7, Palette['j'], 1);
        }
        PixelRows.Paint(p, f.Back ? BackRows : f.Profile ? Side : Front, -6 + f.LeanX, hy, f.Mirror, Palette, 0, Mathf.Clamp(hiddenRows, 0, 14));
    }
}
