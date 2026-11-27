using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Game;

/// <summary>
/// A line of the map's notebook: a mission or a clue the campaign gave. <paramref name="At"/> is where it
/// was found (a seal on the map); <paramref name="Toward"/> is where it leads — the map draws a short red
/// brush stroke in that direction, never the place itself (the prototype's rule).
/// </summary>
public readonly record struct MapNote(string Title, string Text, bool Done, Vector2? At, Vector2? Toward, bool Clue);

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
    /// <summary>A short legend of the ground colours (the places have pictures: see <see cref="LegendIcons"/>).</summary>
    string MapLegend { get; }
    /// <summary>The pictures this region's map uses, in legend order (pedido 14.2).</summary>
    IReadOnlyList<MapIcon> LegendIcons => System.Array.Empty<MapIcon>();
    /// <summary>The map's title.</summary>
    string MapTitle { get; }
    /// <summary>"Onde estou?": the place the player stands in, in words.</summary>
    string MapWhere { get; }
    /// <summary>Missions and clues, oldest first (the last clue is "the last clue I found").</summary>
    IReadOnlyList<MapNote> MapNotes { get; }
}

/// <summary>
/// The world map, in the prototype's look: a parchment sheet, ground drawn in ink tones, a seal where each
/// mission or clue was found with a red stroke toward where it leads, and the player as a blinking jade
/// square with a tick toward the facing. The full map (M or the pause menu) has a notebook column that
/// answers "Onde estou?" and "Qual foi a última pista?" and lists the missions. Pan with WASD, zoom with the
/// wheel or + / −, C recentres, X (or a left click) puts or removes a marker. With <see cref="Compact"/> it
/// is the corner minimap: the same signs, centred on the player, no text.
/// </summary>
public partial class MapView : Control
{
    /// <summary>The region shown.</summary>
    public IMapSource? Source { get; set; }
    /// <summary>World point at the centre of the map area.</summary>
    public Vector2 Center { get; set; }
    /// <summary>Map pixels per world pixel.</summary>
    public float Zoom { get; set; } = 0.1f;
    /// <summary>Smallest zoom.</summary>
    public float MinZoom { get; set; } = 0.02f;
    /// <summary>Largest zoom.</summary>
    public float MaxZoom { get; set; } = 0.5f;
    /// <summary>The corner minimap: no notebook, no help line, follows the player.</summary>
    public bool Compact { get; set; }

    /// <summary>Parchment and ink (the prototype's map colours).</summary>
    public static readonly Color Paper = new("#d8c7a0"), PaperEdge = new("#c8b48a"), PaperShade = new(0.47f, 0.35f, 0.2f, 0.15f), Ink = new("#2a2118"), Ink2 = new("#5a4a36"),
        Seal = new("#a8382a"), Jade = new("#3fae9e"), JadeHi = new("#a6f2e2"), Roller = new("#2a1d12"), RollerHi = new("#4a3420");

    private float _t;
    private Rect2 _mapRect;

    /// <summary>The map area inside the panel (the notebook takes the rest).</summary>
    public Rect2 MapRect => _mapRect;

    /// <summary>World → panel coordinates.</summary>
    public Vector2 ToScreen(Vector2 world) => _mapRect.GetCenter() + (world - Center) * Zoom;

    /// <summary>Panel → world coordinates.</summary>
    public Vector2 ToWorld(Vector2 screen) => Center + (screen - _mapRect.GetCenter()) / Zoom;

    private bool Inside(Vector2 p, float pad = 0) => p.X >= _mapRect.Position.X - pad && p.Y >= _mapRect.Position.Y - pad && p.X <= _mapRect.End.X + pad && p.Y <= _mapRect.End.Y + pad;

    /// <summary>A filled rectangle in world units (clipped to the map area).</summary>
    public void WorldRect(Vector2 topLeft, Vector2 size, Color c)
    {
        Vector2 a = ToScreen(topLeft), b = a + size * Zoom;
        a = a.Clamp(_mapRect.Position, _mapRect.End); b = b.Clamp(_mapRect.Position, _mapRect.End);
        if (b.X - a.X <= 0 || b.Y - a.Y <= 0) return;
        DrawRect(new Rect2(a, new Vector2(Mathf.Max(1, b.X - a.X), Mathf.Max(1, b.Y - a.Y))), c);
    }

    /// <summary>A texture covering a world rectangle (one texel per tile), clipped to the map area.</summary>
    public void WorldTexture(Vector2 topLeft, Vector2 size, Texture2D tex)
    {
        Vector2 a = ToScreen(topLeft), b = a + size * Zoom;
        var dst = new Rect2(a, b - a);
        var clip = dst.Intersection(_mapRect);
        if (clip.Size.X <= 0 || clip.Size.Y <= 0) return;
        Vector2 ts = tex.GetSize();
        var src = new Rect2((clip.Position - a) / (b - a) * ts, clip.Size / (b - a) * ts);
        DrawTextureRectRegion(tex, clip, src);
    }

    /// <summary>A small label at a world point (full map only).</summary>
    public void WorldLabel(Vector2 world, string text, Color c)
    {
        if (Compact) return;
        Vector2 p = ToScreen(world);
        if (!Inside(p)) return;
        var f = ThemeDB.FallbackFont;
        float w = f.GetStringSize(text, HorizontalAlignment.Left, -1, 8).X;
        Vector2 at = new(Mathf.Round(p.X - w / 2), Mathf.Round(p.Y - 4));
        DrawString(f, at + new Vector2(1, 1), text, HorizontalAlignment.Left, -1, 8, new Color("#e0d2ae"));
        DrawString(f, at, text, HorizontalAlignment.Left, -1, 8, Ink);
    }

    /// <summary>A dot (place) at a world point.</summary>
    public void WorldDot(Vector2 world, Color c, float r = 2)
    {
        Vector2 p = ToScreen(world);
        if (!Inside(p, -1)) return;
        if (Compact) r = Mathf.Max(1, r - 1);
        DrawRect(new Rect2(p - new Vector2(r + 1, r + 1), new Vector2(r * 2 + 2, r * 2 + 2)), Ink);
        DrawRect(new Rect2(p - new Vector2(r, r), new Vector2(r * 2, r * 2)), c);
    }

    /// <summary>A place's picture at a world point (its size follows the map: small on the minimap, larger on the full map).</summary>
    public void WorldIcon(Vector2 world, MapIcon icon)
    {
        Vector2 p = ToScreen(world);
        if (!Inside(p)) return;
        MapIcons.Draw(this, p, icon, Compact ? 1 : 2);
    }

    /// <summary>Zooms by a factor, keeping the centre.</summary>
    public void ZoomBy(float k) => Zoom = Mathf.Clamp(Zoom * k, MinZoom, MaxZoom);

    /// <summary>Puts a marker at a world point, or removes one within a few map pixels of it.</summary>
    public bool ToggleMarker(Vector2 world)
    {
        if (Source == null) return false;
        var list = Source.MapMarkers;
        int near = list.FindIndex(m => (m - world).Length() * Zoom < 18);
        if (near >= 0) { list.RemoveAt(near); return false; }
        list.Add(world);
        return true;
    }

    /// <summary>The newest clue (null when none yet).</summary>
    public static MapNote? LastClue(IMapSource s)
    {
        for (int i = s.MapNotes.Count - 1; i >= 0; i--) if (s.MapNotes[i].Clue) return s.MapNotes[i];
        return null;
    }

    private float _redraw;

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!Visible) return;
        _t += (float)delta;
        if (Compact)
        {
            if (Source != null) Center = Source.MapPlayer;
            _redraw -= (float)delta;
            if (_redraw > 0) return;
            _redraw = 0.1f; // the minimap needs no more than ten frames a second
        }
        QueueRedraw();
    }

    /// <inheritdoc />
    public override void _GuiInput(InputEvent e)
    {
        if (Compact) return;
        if (e is InputEventMouseButton mb && mb.Pressed)
        {
            if (mb.ButtonIndex == MouseButton.WheelUp) ZoomBy(1.25f);
            else if (mb.ButtonIndex == MouseButton.WheelDown) ZoomBy(0.8f);
            else if (mb.ButtonIndex == MouseButton.Left && _mapRect.HasPoint(mb.Position)) ToggleMarker(ToWorld(mb.Position));
            AcceptEvent();
        }
    }

    /// <summary>The notebook column's width for the current size (0 when the notebook goes under the map).</summary>
    private float NotebookWidth => Size.X >= 460 ? Mathf.Clamp(Size.X * 0.34f, 170, 260) : 0;

    /// <inheritdoc />
    public override void _Draw()
    {
        if (Compact) { DrawCompact(); return; }
        // the scroll: rollers at the top and bottom, parchment, a soft frame and specks
        DrawRect(new Rect2(-6, -8, Size.X + 12, 8), Roller); DrawRect(new Rect2(-6, Size.Y, Size.X + 12, 8), Roller);
        DrawRect(new Rect2(-6, -7, Size.X + 12, 2), RollerHi); DrawRect(new Rect2(-6, Size.Y + 1, Size.X + 12, 2), RollerHi);
        DrawRect(new Rect2(Vector2.Zero, Size), Paper);
        DrawRect(new Rect2(0, 0, Size.X, 3), PaperEdge); DrawRect(new Rect2(0, Size.Y - 3, Size.X, 3), PaperEdge);
        DrawRect(new Rect2(0, 0, 3, Size.Y), PaperEdge); DrawRect(new Rect2(Size.X - 3, 0, 3, Size.Y), PaperEdge);
        for (int i = 0; i < 60; i++) { uint h = (uint)(i * 2654435761u); DrawRect(new Rect2(h % (uint)Mathf.Max(1, Size.X), (h >> 8) % (uint)Mathf.Max(1, Size.Y), 3 + h % 6, 1), PaperShade); }

        float nb = NotebookWidth;
        var font = ThemeDB.FallbackFont;
        string title = Source?.MapTitle ?? "Mapa";
        float mapW = nb > 0 ? Size.X - nb - 26 : Size.X - 24;
        float mapH = nb > 0 ? Size.Y - 44 : Mathf.Round((Size.Y - 44) * 0.6f);
        _mapRect = new Rect2(12, 24, mapW, mapH);
        DrawString(font, new Vector2(12, 16), title, HorizontalAlignment.Left, mapW, 11, Ink);
        DrawRect(new Rect2(_mapRect.Position - Vector2.One * 2, _mapRect.Size + Vector2.One * 4), PaperShade);
        if (Source != null)
        {
            Source.DrawMap(this);
            DrawNotesOnMap(Source);
            DrawMarkers(Source);
            DrawPlayer(Source, 3);
            // the cross at the centre (where X puts a marker)
            Vector2 c = _mapRect.GetCenter();
            DrawLine(c - new Vector2(5, 0), c + new Vector2(5, 0), new Color(Ink, 0.45f)); DrawLine(c - new Vector2(0, 5), c + new Vector2(0, 5), new Color(Ink, 0.45f));
            if (nb > 0) DrawNotebook(Source, new Rect2(Size.X - nb - 8, 10, nb, Size.Y - 34));
            else DrawNotebook(Source, new Rect2(12, _mapRect.End.Y + 6, Size.X - 24, Size.Y - _mapRect.End.Y - 30));
        }
        DrawRect(_mapRect, new Color(Ink2, 0.6f), false, 1);
        DrawString(font, new Vector2(12, Size.Y - 8), Bindings.F("[walk] mover · roda ou +/− zoom · C centralizar · X ou clique: marcador · [map] fechar · O traço vermelho aponta a direção, não o lugar."),
            HorizontalAlignment.Left, Size.X - 24, 8, Ink2);
    }

    private void DrawCompact()
    {
        // a small ink-washed parchment in the corner, centred on the player
        _mapRect = new Rect2(2, 2, Size.X - 4, Size.Y - 4);
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("#16140f", 0.85f));
        DrawRect(_mapRect, new Color(Paper, 0.92f));
        if (Source != null)
        {
            Source.DrawMap(this);
            DrawNotesOnMap(Source);
            DrawMarkers(Source);
            DrawPlayer(Source, 2);
        }
        DrawRect(_mapRect, new Color(Ink2, 0.8f), false, 1);
    }

    // a seal where each note was found, and a fading red brush stroke toward where it leads
    private void DrawNotesOnMap(IMapSource s)
    {
        foreach (var n in s.MapNotes)
        {
            if (n.At is not { } at) continue;
            Vector2 q = ToScreen(at);
            if (!Inside(q, 2)) continue;
            if (n.Toward is { } to && !n.Done)
            {
                float a = (to - at).Angle(), L = Compact ? 14 : 26;
                for (int i = 4; i < L; i++)
                {
                    var col = new Color(Seal, 0.9f * (1 - i / L));
                    Vector2 p = q + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * i;
                    if (Inside(p)) DrawRect(new Rect2(p.Round() - Vector2.One, new Vector2(i < 10 ? 3 : 2, 2)), col);
                }
            }
            float r = Compact ? 2 : 3;
            DrawRect(new Rect2(q.Round() - Vector2.One * r, Vector2.One * (r * 2 + 1)), n.Done ? Ink2 : Seal);
            DrawRect(new Rect2(q.Round() - Vector2.One * (r - 2), Vector2.One * (r * 2 - 3)), Paper);
        }
    }

    private void DrawMarkers(IMapSource s)
    {
        foreach (var m in s.MapMarkers)
        {
            Vector2 q = ToScreen(m);
            if (!Inside(q, 2)) continue;
            float k = Compact ? 2 : 4;
            DrawLine(q - new Vector2(k, k), q + new Vector2(k, k), Seal, 2); DrawLine(q + new Vector2(-k, k), q + new Vector2(k, -k), Seal, 2);
        }
    }

    // the player: an ink square with a blinking jade centre (the prototype) and a tick toward the facing
    private void DrawPlayer(IMapSource s, float r)
    {
        Vector2 p = ToScreen(s.MapPlayer).Round();
        p = p.Clamp(_mapRect.Position + Vector2.One * 3, _mapRect.End - Vector2.One * 3);
        Vector2 f = s.MapFacing.LengthSquared() > 0 ? s.MapFacing.Normalized() : Vector2.Down;
        bool blink = Mathf.FloorToInt(_t * 3) % 2 == 0;
        for (int i = 0; i < (Compact ? 6 : 9); i++) DrawRect(new Rect2((p + f * (r + 1 + i)).Round() - Vector2.One, new Vector2(2, 2)), i < 3 ? Ink : new Color(Ink, 0.7f - i * 0.06f));
        DrawRect(new Rect2(p - Vector2.One * r, Vector2.One * (r * 2 + 1)), Ink);
        DrawRect(new Rect2(p - Vector2.One * (r - 1), Vector2.One * (r * 2 - 1)), blink ? Jade : JadeHi);
    }

    // "Onde estou?", "Última pista" and the missions, in ink, as the prototype's right column
    private void DrawNotebook(IMapSource s, Rect2 box)
    {
        var font = ThemeDB.FallbackFont;
        float x = box.Position.X, y = box.Position.Y + 6, w = box.Size.X;
        void Head(string t) { DrawString(font, new Vector2(x, y + 9), t, HorizontalAlignment.Left, w, 10, Ink); y += 14; }
        void Body(string t, Color c, int size = 8)
        {
            var lines = Wrap(t, w - 10, size);
            foreach (var l in lines) { if (y > box.End.Y - 4) return; DrawString(font, new Vector2(x + 10, y + 8), l, HorizontalAlignment.Left, w - 10, size, c); y += size + 3; }
        }
        Head("Onde estou?");
        Body(s.MapWhere, Ink);
        y += 4;
        Head("Última pista");
        var last = LastClue(s);
        Body(last is { } lc ? lc.Text : "Nenhuma ainda. Converse, examine, explore.", last is null ? Ink2 : Ink);
        y += 4;
        var quests = new List<MapNote>();
        foreach (var n in s.MapNotes) if (!n.Clue) quests.Add(n);
        if (quests.Count > 0)
        {
            Head("Missões");
            for (int i = quests.Count - 1; i >= 0 && y < box.End.Y - 10; i--)
            {
                var q = quests[i];
                DrawRect(new Rect2(x, y + 2, 6, 6), q.Done ? Ink2 : Seal);
                DrawString(font, new Vector2(x + 10, y + 9), q.Title, HorizontalAlignment.Left, w - 10, 9, q.Done ? Ink2 : Ink);
                if (q.Done) DrawRect(new Rect2(x + 10, y + 5, Mathf.Min(w - 10, font.GetStringSize(q.Title, HorizontalAlignment.Left, -1, 9).X), 1), Ink2);
                y += 12;
                if (!q.Done) Body(q.Text, Ink2);
                y += 3;
            }
        }
        // the legend: the same pictures the map draws, with their names, two per line
        var icons = s.LegendIcons;
        if (icons.Count > 0 && y < box.End.Y - 30)
        {
            y += 4; Head("Legenda");
            float colW = w / 2;
            for (int i = 0; i < icons.Count && y < box.End.Y - 22; i++)
            {
                float cx = x + (i % 2) * colW;
                MapIcons.Draw(this, new Vector2(cx + 6, y + 5), icons[i], 1.5f);
                DrawString(font, new Vector2(cx + 14, y + 8), MapIcons.Name(icons[i]), HorizontalAlignment.Left, colW - 16, 8, Ink);
                if (i % 2 == 1 || i == icons.Count - 1) y += 11;
            }
        }
        var legend = s.MapLegend;
        if (legend.Length > 0 && y < box.End.Y - 20) { int lines = Wrap(legend, w - 10, 7).Count; y = Mathf.Max(y + 4, box.End.Y - 10 * lines - 2); Body(legend, Ink2, 7); }
    }

    /// <summary>Splits a text into lines that fit a width at a font size.</summary>
    public static List<string> Wrap(string text, float width, int size)
    {
        var font = ThemeDB.FallbackFont;
        var outLines = new List<string>();
        foreach (var para in text.Split('\n'))
        {
            string line = "";
            foreach (var word in para.Split(' '))
            {
                string t = line.Length == 0 ? word : line + " " + word;
                if (font.GetStringSize(t, HorizontalAlignment.Left, -1, size).X > width && line.Length > 0) { outLines.Add(line); line = word; }
                else line = t;
            }
            outLines.Add(line);
        }
        return outLines;
    }
}
