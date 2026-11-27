using Godot;

namespace SpiritOfMurin.Game;

/// <summary>The kinds of place the map and the minimap mark with a picture (pedido 14.2).</summary>
public enum MapIcon { Campfire, Mat, Burrow, Boar, Coins, Herb, Marker, Clue, Player }

/// <summary>
/// The little pictures of the map and the minimap: one per kind of place, each with its own shape (not only a
/// colour), a dark outline and the name used by the legend. To change how one looks, edit its case in
/// <see cref="Draw"/> (pixel offsets from the centre, before <c>scale</c>); to change its name, <see cref="Name"/>.
/// The map (M) draws the legend with these same pictures, so the legend can never disagree with the map.
/// </summary>
public static class MapIcons
{
    /// <summary>Colours of the pictures.</summary>
    public static readonly Color Ink = new("#2a2418"), Flame = new("#e8a13a"), FlameCore = new("#ffe08a"), MatC = new("#6fd8d2"), MatLine = new("#2f8a86"),
        Earth = new("#8a6a44"), Hole = new("#2a1e12"), BoarC = new("#7a3a32"), Tusk = new("#f3ecd8"), Gold = new("#e6b422"), GoldDeep = new("#9a7414"),
        Leaf = new("#4c9a4a"), LeafHi = new("#9be07a"), SealC = new("#b33a2a");

    /// <summary>The legend's name of each picture.</summary>
    public static string Name(MapIcon i) => i switch
    {
        MapIcon.Campfire => "fogueira",
        MapIcon.Mat => "esteira (meditar)",
        MapIcon.Burrow => "toca de coelho",
        MapIcon.Boar => "javali",
        MapIcon.Coins => "moedas",
        MapIcon.Herb => "erva",
        MapIcon.Marker => "seu marcador",
        MapIcon.Clue => "onde achou uma pista",
        MapIcon.Player => "você",
        _ => "",
    };

    /// <summary>Draws a picture centred on <paramref name="c"/> (screen px). <paramref name="scale"/> 1 is the minimap size (about 7 px).</summary>
    public static void Draw(CanvasItem g, Vector2 c, MapIcon i, float scale = 1)
    {
        c = c.Round();
        void R(float x, float y, float w, float h, Color col) => g.DrawRect(new Rect2(c + new Vector2(x, y) * scale, new Vector2(w, h) * scale), col);
        void O(float x, float y, float w, float h) => R(x - 1, y - 1, w + 2, h + 2, Ink); // outline behind a part
        switch (i)
        {
            case MapIcon.Campfire: // a flame on two logs
                O(-3, 1, 6, 2); O(-2, -3, 4, 4); O(-1, -4, 2, 1);
                R(-3, 1, 6, 2, Earth); R(-2, -3, 4, 4, Flame); R(-1, -4, 2, 1, Flame); R(-1, -1, 2, 2, FlameCore);
                break;
            case MapIcon.Mat: // a striped mat
                O(-3, -2, 6, 4); R(-3, -2, 6, 4, MatC); R(-3, -1, 6, 1, MatLine); R(-3, 1, 6, 1, MatLine);
                break;
            case MapIcon.Burrow: // a dark hole with an earth rim
                O(-3, -1, 6, 3); R(-3, -1, 6, 3, Earth); R(-2, 0, 4, 2, Hole);
                break;
            case MapIcon.Boar: // a body with a white tusk
                O(-3, -2, 6, 4); R(-3, -2, 6, 4, BoarC); R(2, 0, 2, 1, Tusk); R(-1, -3, 2, 1, BoarC);
                break;
            case MapIcon.Coins: // a gold coin
                O(-2, -2, 4, 4); R(-2, -2, 4, 4, Gold); R(-1, -1, 2, 2, GoldDeep);
                break;
            case MapIcon.Herb: // two leaves on a stem
                O(-2, -2, 2, 2); O(0, -3, 2, 2); R(-2, -2, 2, 2, Leaf); R(0, -3, 2, 2, LeafHi); R(0, -1, 1, 3, Leaf);
                break;
            case MapIcon.Marker: // the red ✕
                g.DrawLine(c + new Vector2(-3, -3) * scale, c + new Vector2(3, 3) * scale, SealC, 2 * scale);
                g.DrawLine(c + new Vector2(-3, 3) * scale, c + new Vector2(3, -3) * scale, SealC, 2 * scale);
                break;
            case MapIcon.Clue: // the seal
                O(-2, -2, 4, 4); R(-2, -2, 4, 4, SealC); R(-1, -1, 2, 2, new Color("#efe4c8"));
                break;
            case MapIcon.Player: // the jade square
                O(-2, -2, 4, 4); R(-2, -2, 4, 4, new Color("#6fd8d2"));
                break;
        }
    }
}
