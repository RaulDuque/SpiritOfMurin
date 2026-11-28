using System.Collections.Generic;
using Godot;
using SpiritOfMurin.Characters;

namespace SpiritOfMurin.Elements;

/// <summary>Which way the swordsman is drawn: facing the camera, his back, or in profile.</summary>
public enum FighterView { Down, Up, Side }

/// <summary>
/// Everything the swordsman's layers need about one frame (T18): the view, the side, the lean, where the hands and the
/// tip are, the walk, the hem's swing, the head's height and the clock. <see cref="Player"/> computes it once per frame
/// and hands it to the head (<see cref="CabecaArt"/>), the clothes (<see cref="RoupaArt"/>), the hat
/// (<see cref="CapaceteArt"/>) and the weapon (<see cref="ArmaArt"/>). All in the figure's pixels, (0, 0) at the feet.
/// </summary>
public sealed class FighterPose
{
    /// <summary>Facing the camera, the back or the profile.</summary>
    public FighterView View;
    /// <summary>In profile: +1 looking right, −1 looking left (also set for the other views, from the facing).</summary>
    public int S;
    /// <summary>The facing as a unit vector (x, y), and the perpendicular's x (−fy).</summary>
    public float Fx, Fy, Nx;
    /// <summary>The body's lean during a step (px).</summary>
    public int LeanX;
    /// <summary>The shoulders' height (−25 standing).</summary>
    public float ShoulderY;
    /// <summary>The front hand (the one that holds the weapon) and the weapon's tip.</summary>
    public Vector2 Hand, Tip;
    /// <summary>The walk: −1..1 through the step (0 standing), and whether the legs walk.</summary>
    public float WalkCycle;
    /// <summary>True while walking.</summary>
    public bool Moving;
    /// <summary>True during a step, a side step or a dash.</summary>
    public bool Dashing;
    /// <summary>The robe's hem swing (px, from the walk or the step).</summary>
    public int Hem;
    /// <summary>The top of the head (−41 standing, −40 on the breath).</summary>
    public int HeadY;
    /// <summary>Seconds running (the hair ribbon's flutter).</summary>
    public float Time;

    /// <summary>True when seen from behind.</summary>
    public bool Back => View == FighterView.Up;
    /// <summary>True in profile.</summary>
    public bool Profile => View == FighterView.Side;
    /// <summary>True in profile looking left: the matrices are mirrored.</summary>
    public bool Mirror => Profile && S < 0;
}

/// <summary>
/// Paints a pixel matrix (one string per row, one letter per pixel, '.' empty) through a palette: the swordsman's
/// head and robes are drawn this way. Rows from 16 down move by <c>hem</c> (the robe's skirt swinging).
/// </summary>
public static class PixelRows
{
    /// <summary>Paints <paramref name="rows"/> with its left-top corner at (<paramref name="left"/>, <paramref name="top"/>); <paramref name="skipTop"/> rows from the top are left out (a hat over the hair).</summary>
    public static void Paint(PixelPainter p, string[] rows, int left, int top, bool mirror, IReadOnlyDictionary<char, Color> palette, int hem = 0, int skipTop = 0)
    {
        for (int y = skipTop; y < rows.Length; y++)
        {
            string row = rows[y];
            for (int start = 0; start < row.Length;)
            {
                char pigment = row[start];
                int end = start + 1;
                while (end < row.Length && row[end] == pigment) end++;
                if (pigment != '.')
                {
                    int offset = mirror ? row.Length - end : start;
                    p.Rect(left + offset + (y >= 16 ? hem : 0), top + y, end - start, 1, palette[pigment]);
                }
                start = end;
            }
        }
    }
}
