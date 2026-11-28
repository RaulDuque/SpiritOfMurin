using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Elements;

/// <summary>
/// The woven bamboo hat (T18, item <c>chapeu_bambu</c>): a low flat-topped crown of dry green bamboo, a narrow level brim
/// (16 px, the eyes stay in sight) and a deep-green cord. It hides the bun and its band
/// (<see cref="HairCover"/> = <see cref="CabecaArt.BandRows"/>, the creator's decision 3): no ribbon while it is worn; the side hair
/// shows under the brim, and the whole head comes back when it is taken off.
/// <b>Where to change it:</b>
/// <list type="bullet">
/// <item>the brim: row 4 of each matrix (<c>k</c> ends, <c>m</c>/<c>l</c> top face) and row 5 (the shadow under it).
/// To lower the brim, add one more row of crown (copy row 2) above the cord: everything below moves down a pixel;</item>
/// <item>the crown: rows 0–2; the cord: row 3 (<c>d</c>, light <c>t</c>);</item>
/// <item>how much hair it hides: <see cref="HairCover"/>; the colours: <see cref="Colors"/>.</item>
/// </list>
/// The matrices are 16 wide and centred on the body (left −8), so the profile mirrors in place.
/// </summary>
public class ChapeuBambuArt : CapaceteArt
{
    /// <summary>Bamboo shade, base, light; the cord and its light.</summary>
    public static readonly Dictionary<char, Color> Colors = new()
    {
        ['k'] = new("#5c6340"), // sombra do bambu
        ['m'] = new("#7f8757"), // bambu seco esverdeado
        ['l'] = new("#98a06a"), // luz do trançado
        ['d'] = new("#1c4f35"), // cordão (verde da faixa)
        ['t'] = new("#2f7a53"), // brilho do cordão
    };

    /// <summary>The hat from the front.</summary>
    public static readonly string[] Front =
    {
        ".....mlllmm.....",
        "....kmlllmmk....",
        "....kkmmmmkk....",
        "....ddddtddd....",
        "kmmmmllllllmmmmk",
        ".kkkkkkkkkkkkkk.",
    };

    /// <summary>The hat from behind.</summary>
    public static readonly string[] BackRows =
    {
        ".....mmllmm.....",
        "....kmmllmmk....",
        "....kkmmmmkk....",
        "....dddddddd....",
        "kmmmmmmmmmmmmmmk",
        ".kkkkkkkkkkkkkk.",
    };

    /// <summary>The hat in profile, looking right (the brim reaches 3 px past the face).</summary>
    public static readonly string[] Side =
    {
        ".....mllmm......",
        "....kmllmmk.....",
        "....kkmmmkk.....",
        "....dddtddd.....",
        ".kmmmmllllmmmmmk",
        "..kkkkkkkkkkkkk.",
    };

    /// <inheritdoc />
    public override int HairCover => CabecaArt.BandRows; // decisão 3 do criador (2026-11-28): cobre o coque e a faixa; sem fita sob o chapéu

    /// <inheritdoc />
    public override void Draw(PixelPainter p, FighterPose f) =>
        PixelRows.Paint(p, f.Back ? BackRows : f.Profile ? Side : Front, -8 + f.LeanX, f.HeadY, f.Mirror, Colors);
}

/// <summary>
/// Board-only variants of the bamboo hat (T18, decision 3), registered by the picture check, never in the game:
/// covering the bun and its band (no ribbon), or the bun showing above the crown.
/// </summary>
public sealed class ChapeuBambuVarianteArt : ChapeuBambuArt
{
    private readonly int _cover;
    private readonly bool _bunOver;

    /// <summary>A variant hiding <paramref name="cover"/> rows of the head; <paramref name="bunOver"/> paints the bun again over the crown.</summary>
    public ChapeuBambuVarianteArt(int cover, bool bunOver) { _cover = cover; _bunOver = bunOver; }

    /// <inheritdoc />
    public override int HairCover => _cover;

    /// <inheritdoc />
    public override void Draw(PixelPainter p, FighterPose f)
    {
        base.Draw(p, f);
        if (!_bunOver) return;
        string[] head = f.Back ? CabecaArt.BackRows : f.Profile ? CabecaArt.Side : CabecaArt.Front;
        PixelRows.Paint(p, new[] { head[0], head[1], head[2] }, -6 + f.LeanX, f.HeadY - 2, f.Mirror, CabecaArt.Palette);
    }
}
