using Godot;
using SpiritOfMurin.Plains;

namespace SpiritOfMurin.Elements;

/// <summary>
/// A print's drawing (T07, element standard): two marks pointing the way the animal went — hoof (boar: two deep
/// slots and two lighter ones behind) or paw (rabbit: two dots and a small toe). A fresh print (left while the animal
/// walked, not generated with the world) fades out over its life. An examined trail shines.
/// <b>Where to change it:</b> colours below; the shapes in <see cref="Draw"/>. How long a fresh print lasts and how far
/// apart they are: <c>assets/data/plains/plains_fauna.json</c> (<c>track_seconds</c>, <c>track_spacing_*</c>).
/// </summary>
public static class PegadaArt
{
    /// <summary>Colours: the mark, its lighter half, the shine of an examined trail.</summary>
    public static readonly Color Mark = new("#2a2219"), MarkLight = new("#5e4a34"), Shine = new("#d4fbf6");

    private static void R(CanvasItem c, Vector2 p, float w, float h, Color col) => c.DrawRect(new Rect2(Mathf.Round(p.X), Mathf.Round(p.Y), w, h), col);

    /// <summary>Draws the print <paramref name="t"/> on itself.</summary>
    public static void Draw(TrackPrint t, float time)
    {
        float fade = t.Opacity;
        if (fade <= 0) return;
        Vector2 f = t.Dir * 3f, s = new Vector2(-t.Dir.Y, t.Dir.X) * 2f;
        bool glow = t.Glow != 0;
        if (glow) R(t, new Vector2(-6, -5), 12, 10, new Color(Shine, (0.35f + 0.25f * Mathf.Sin(time * 6)) * 0.5f));
        Color c = glow ? Shine : new Color(Mark, fade), light = glow ? Shine : new Color(MarkLight, fade);
        if (t.Hoof)
        {
            R(t, -s + f - Vector2.One, 2, 3, c); R(t, s + f - Vector2.One, 2, 3, c);
            R(t, -s - f - Vector2.One, 2, 2, light); R(t, s - f - Vector2.One, 2, 2, light);
        }
        else
        {
            R(t, -s - Vector2.One, 2, 2, c); R(t, s + f - Vector2.One, 2, 2, c); R(t, f * 2, 1, 1, light);
        }
    }
}
