using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Characters;

/// <summary>
/// Collects pixel rectangles and lines, then draws them the way the HTML prototype does:
/// a 1 px outline around every shape first, the colours on top. Coordinates are local to the
/// canvas item, with (0, 0) at the figure's feet.
/// </summary>
public sealed class PixelPainter
{
    private readonly List<(Rect2 Rect, Color Color)> _ops = new();
    private readonly List<(Rect2 Rect, Color Color)> _glow = new();

    /// <summary>
    /// Adds a glow line: no outline, drawn between the outline pass and the colour pass, so it shows around
    /// the shapes and under them (the Reforço de Ki's light around the blade). Translucent colours blend.
    /// </summary>
    public void GlowLine(float x0f, float y0f, float x1f, float y1f, Color c, int w = 1)
    {
        int n0 = _ops.Count;
        Line(x0f, y0f, x1f, y1f, c, w);
        _glow.AddRange(_ops.GetRange(n0, _ops.Count - n0));
        _ops.RemoveRange(n0, _ops.Count - n0);
    }

    /// <summary>Adds a filled rectangle (rounded to whole pixels).</summary>
    public void Rect(float x, float y, float w, float h, Color c) =>
        _ops.Add((new Rect2(Mathf.Round(x), Mathf.Round(y), w, h), c));

    /// <summary>Adds a pixel line (Bresenham) of the given width.</summary>
    public void Line(float x0f, float y0f, float x1f, float y1f, Color c, int w = 1)
    {
        int x0 = Mathf.RoundToInt(x0f), y0 = Mathf.RoundToInt(y0f), x1 = Mathf.RoundToInt(x1f), y1 = Mathf.RoundToInt(y1f);
        int dx = Mathf.Abs(x1 - x0), dy = -Mathf.Abs(y1 - y0), sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;
        for (int n = 0; n < 200; n++)
        {
            Rect(x0 - (w >> 1), y0 - (w >> 1), w, w, c);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    /// <summary>Draws the outline pass, the glow pass, then the colour pass (all in <paramref name="tint"/> when given: a hit flash), onto <paramref name="item"/>, and clears.</summary>
    public void Flush(CanvasItem item, Color outline, Color? tint = null)
    {
        foreach (var (r, _) in _ops) item.DrawRect(new Rect2(r.Position - Vector2.One, r.Size + Vector2.One * 2), outline);
        if (tint == null) foreach (var (r, c) in _glow) item.DrawRect(r, c);
        foreach (var (r, c) in _ops) item.DrawRect(r, tint ?? c);
        _ops.Clear(); _glow.Clear();
    }
}
