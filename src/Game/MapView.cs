using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Game;

/// <summary>What a region gives the map: its explored ground, its places, the player and the manual markers.</summary>
public interface IMapSource
{
    /// <summary>Draws the explored ground and the known places, using <paramref name="v"/>.ToScreen for world → map.</summary>
    void DrawMap(MapView v);
    /// <summary>The player's position (world px).</summary>
    Vector2 MapPlayer { get; }
    /// <summary>The player's facing.</summary>
    Vector2 MapFacing { get; }
    /// <summary>Manual markers (world px), saved with the journey.</summary>
    List<Vector2> MapMarkers { get; }
    /// <summary>A short legend.</summary>
    string MapLegend { get; }
    /// <summary>The map's title.</summary>
    string MapTitle { get; }
}

/// <summary>
/// The world map panel (M or the pause menu). Pan with WASD, zoom with the mouse wheel or + / −, C
/// recentres on the player, X (or a left click) puts or removes a marker where the cross is (or under the
/// cursor). The region draws its own explored ground (<see cref="IMapSource"/>); the view draws the frame,
/// the player arrow, the markers and the legend.
/// </summary>
public partial class MapView : Control
{
    /// <summary>The region shown.</summary>
    public IMapSource? Source { get; set; }
    /// <summary>World point at the centre of the panel.</summary>
    public Vector2 Center { get; set; }
    /// <summary>Map pixels per world pixel.</summary>
    public float Zoom { get; set; } = 0.1f;
    /// <summary>Smallest zoom.</summary>
    public float MinZoom { get; set; } = 0.02f;
    /// <summary>Largest zoom.</summary>
    public float MaxZoom { get; set; } = 0.5f;

    private static readonly Color Paper = new(0.09f, 0.1f, 0.08f, 0.96f), Frame = new("#4e3824"), Ink = new("#ece5d0"), Dim = new("#a39e8c"), Mark = new("#d05030"), Me = new("#d4fbf6");
    private float _t;

    /// <summary>World → screen (panel) coordinates.</summary>
    public Vector2 ToScreen(Vector2 world) => Size / 2 + (world - Center) * Zoom;

    /// <summary>Screen (panel) → world coordinates.</summary>
    public Vector2 ToWorld(Vector2 screen) => Center + (screen - Size / 2) / Zoom;

    /// <summary>A filled rectangle in world units.</summary>
    public void WorldRect(Vector2 topLeft, Vector2 size, Color c)
    {
        Vector2 a = ToScreen(topLeft), b = size * Zoom;
        if (a.X > Size.X || a.Y > Size.Y || a.X + b.X < 0 || a.Y + b.Y < 0) return;
        DrawRect(new Rect2(a, new Vector2(Mathf.Max(1, b.X), Mathf.Max(1, b.Y))), c);
    }

    /// <summary>A small label at a world point.</summary>
    public void WorldLabel(Vector2 world, string text, Color c)
    {
        Vector2 p = ToScreen(world);
        if (p.X < 0 || p.Y < 0 || p.X > Size.X || p.Y > Size.Y) return;
        DrawString(ThemeDB.FallbackFont, p + new Vector2(-text.Length * 2.2f, -4), text, HorizontalAlignment.Left, -1, 8, c);
    }

    /// <summary>A dot (place) at a world point.</summary>
    public void WorldDot(Vector2 world, Color c, float r = 2)
    {
        Vector2 p = ToScreen(world);
        if (p.X < -4 || p.Y < -4 || p.X > Size.X + 4 || p.Y > Size.Y + 4) return;
        DrawRect(new Rect2(p - new Vector2(r + 1, r + 1), new Vector2(r * 2 + 2, r * 2 + 2)), new Color("#16140f"));
        DrawRect(new Rect2(p - new Vector2(r, r), new Vector2(r * 2, r * 2)), c);
    }

    /// <summary>Zooms by a factor, keeping the centre.</summary>
    public void ZoomBy(float k) => Zoom = Mathf.Clamp(Zoom * k, MinZoom, MaxZoom);

    /// <summary>Puts a marker at a world point, or removes one within 24 map pixels of it.</summary>
    public bool ToggleMarker(Vector2 world)
    {
        if (Source == null) return false;
        var list = Source.MapMarkers;
        int near = list.FindIndex(m => (m - world).Length() * Zoom < 24 * 0.5f + 6);
        if (near >= 0) { list.RemoveAt(near); return false; }
        list.Add(world);
        return true;
    }

    /// <inheritdoc />
    public override void _Process(double delta) { if (Visible) { _t += (float)delta; QueueRedraw(); } }

    /// <inheritdoc />
    public override void _GuiInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb && mb.Pressed)
        {
            if (mb.ButtonIndex == MouseButton.WheelUp) ZoomBy(1.25f);
            else if (mb.ButtonIndex == MouseButton.WheelDown) ZoomBy(0.8f);
            else if (mb.ButtonIndex == MouseButton.Left) ToggleMarker(ToWorld(mb.Position));
            AcceptEvent();
        }
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), Paper);
        if (Source != null)
        {
            Source.DrawMap(this);
            // the player: an arrow toward the facing, blinking softly
            Vector2 p = ToScreen(Source.MapPlayer), f = Source.MapFacing.LengthSquared() > 0 ? Source.MapFacing.Normalized() : Vector2.Down;
            var col = Mathf.Sin(_t * 5) > -0.3f ? Me : new Color(Me, 0.5f);
            Vector2 side = new(-f.Y, f.X);
            for (int i = 0; i < 7; i++) { float k = i / 6f; DrawLine(p + f * 6 - side * 4 * k - f * 8 * k, p + f * 6 + side * 4 * k - f * 8 * k, col, 1.5f); }
            foreach (var m in Source.MapMarkers)
            {
                Vector2 q = ToScreen(m);
                DrawLine(q - new Vector2(4, 4), q + new Vector2(4, 4), Mark, 2); DrawLine(q + new Vector2(-4, 4), q + new Vector2(4, -4), Mark, 2);
            }
            DrawString(ThemeDB.FallbackFont, new Vector2(8, 14), Source.MapTitle, HorizontalAlignment.Left, -1, 10, Ink);
            int y = 0;
            foreach (var line in Source.MapLegend.Split('\n')) { DrawString(ThemeDB.FallbackFont, new Vector2(8, Size.Y - 34 + y), line, HorizontalAlignment.Left, -1, 8, Dim); y += 10; }
        }
        // the cross at the centre (where X puts a marker)
        Vector2 c = Size / 2;
        DrawLine(c - new Vector2(5, 0), c + new Vector2(5, 0), new Color(Ink, 0.5f)); DrawLine(c - new Vector2(0, 5), c + new Vector2(0, 5), new Color(Ink, 0.5f));
        DrawRect(new Rect2(Vector2.Zero, Size), Frame, false, 1);
        DrawString(ThemeDB.FallbackFont, new Vector2(8, Size.Y - 6), "WASD mover · roda ou +/− zoom · C centralizar · X ou clique: marcador · M fechar", HorizontalAlignment.Left, -1, 8, Dim);
    }
}
