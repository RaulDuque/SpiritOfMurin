using Godot;

namespace SpiritOfMurin.Game;

/// <summary>
/// The items' pictures, drawn in code as pixel art (the HTML prototype's drawItemIcon style), centred on a
/// point and scaled by whole pixels. Each item has its own silhouette so it can be recognised in the bag:
/// the knife, the herb, the meat, the hide, the flint, the branch, the coins and the prologue's items.
/// Unknown ids get a plain bundle.
/// </summary>
public static class ItemIcons
{
    private static readonly Color Ink = new("#2a2118");

    /// <summary>Draws the icon of <paramref name="id"/> centred at <paramref name="c"/>, each art pixel <paramref name="s"/> screen pixels.</summary>
    public static void Draw(CanvasItem ci, string id, Vector2 c, float s = 2)
    {
        void R(float x, float y, float w, float h, string col) => ci.DrawRect(new Rect2(c.X + x * s, c.Y + y * s, w * s, h * s), new Color(col));
        void O(float x, float y, float w, float h) => ci.DrawRect(new Rect2(c.X + (x - 1) * s, c.Y + (y - 1) * s, (w + 2) * s, (h + 2) * s), Ink);
        string key = id.StartsWith("grama_sangue") ? "grama" : id;
        switch (key)
        {
            case "silex": // a sharp grey flake
                O(-5, -3, 10, 6); R(-5, -1, 10, 3, "#8c8a80"); R(-3, -3, 7, 2, "#b4b2a8"); R(-4, 2, 6, 1, "#6a685f"); R(2, -2, 2, 1, "#dedcd2"); break;
            case "galho_rijo": // a forked brown branch
                O(-6, 0, 12, 2); R(-6, 0, 12, 2, "#7a5a3a"); R(-6, 0, 12, 1, "#9a7a52"); R(2, -3, 2, 3, "#7a5a3a"); R(3, -4, 1, 1, "#5f8f4a"); R(-3, 2, 1, 2, "#6a4a2c"); break;
            case "faca_pedra": // flint blade bound to a wooden grip
                O(-6, -1, 12, 3); R(-1, -1, 7, 3, "#9a988e"); R(0, -1, 6, 1, "#d6d4ca"); R(-6, -1, 5, 3, "#7a5a3a"); R(-2, -2, 1, 5, "#c8b48a"); R(-4, -2, 1, 5, "#c8b48a"); break;
            case "grama": // red-tipped blades of grass
                {
                    string tip = id.EndsWith("q3") ? "#ff6a5a" : id.EndsWith("q2") ? "#d8463a" : id.EndsWith("q0") ? "#8a4a3a" : "#c0382e";
                    O(-4, -5, 8, 11); R(-1, -3, 1, 9, "#4f8a3a"); R(1, -4, 1, 10, "#5f9a44"); R(-3, -1, 1, 7, "#4f8a3a"); R(3, -2, 1, 8, "#5f9a44");
                    R(-1, -5, 1, 2, tip); R(1, -6, 1, 2, tip); R(-3, -3, 1, 2, tip); R(3, -4, 1, 2, tip);
                    if (id.EndsWith("q0")) R(-4, 3, 8, 2, "#6a7a3a");
                    break;
                }
            case "raiz_amarga":
                O(-4, -4, 8, 9); R(-1, -4, 3, 3, "#5f8f4a"); R(-2, -1, 4, 3, "#a87a4a"); R(-1, 2, 2, 2, "#8a6a3a"); R(1, 3, 2, 2, "#8a6a3a"); R(-3, 0, 1, 2, "#8a6a3a"); break;
            case "carne_macia": case "carne_rija": case "carne_macia_assada": case "carne_rija_assada":
                {
                    bool roast = id.EndsWith("assada"), tough = id.Contains("rija");
                    string flesh = roast ? (tough ? "#7a3f22" : "#9a5a2c") : tough ? "#a8383a" : "#d8606a", fat = roast ? "#c8904a" : "#f0c8b8";
                    O(-5, -3, 9, 7); R(-5, -3, 9, 7, flesh); R(-4, -2, 3, 2, fat); R(1, 1, 2, 2, fat); R(4, -1, 3, 2, "#efe4c8"); R(6, -2, 1, 1, "#efe4c8"); R(6, 1, 1, 1, "#efe4c8");
                    if (roast) { R(-5, 3, 9, 1, "#4a2a14"); }
                    break;
                }
            case "pele_rasgada": case "pele_comum": case "pele_fina":
                {
                    string hide = id == "pele_fina" ? "#c8a878" : id == "pele_comum" ? "#a8845a" : "#8f6f4a";
                    O(-6, -4, 12, 9); R(-5, -4, 10, 9, hide); R(-6, -3, 1, 2, hide); R(5, -3, 1, 2, hide); R(-6, 3, 1, 2, hide); R(5, 3, 1, 2, hide);
                    R(-3, -2, 6, 1, "#d8c098"); if (id == "pele_rasgada") { R(1, 0, 1, 4, "#5a3a24"); R(2, 2, 2, 1, "#5a3a24"); }
                    break;
                }
            case "couro_rasgado": case "couro_comum": case "couro_grosso":
                {
                    string lea = id == "couro_grosso" ? "#4a2e1c" : id == "couro_comum" ? "#6a4228" : "#7a5234";
                    O(-6, -4, 12, 8); R(-6, -4, 12, 8, lea); R(-6, -4, 12, 1, "#8a6244"); R(-4, -1, 8, 1, "#3a2414"); if (id == "couro_rasgado") R(2, 0, 1, 4, "#2a1a10");
                    break;
                }
            case "presa_javali": case "presa_espiritual":
                {
                    string tusk = id == "presa_espiritual" ? "#d4fbf6" : "#e6dcc0";
                    O(-5, -4, 10, 9); R(-4, 2, 2, 2, tusk); R(-3, 0, 2, 2, tusk); R(-2, -2, 2, 2, tusk); R(-1, -4, 2, 2, tusk);
                    R(2, 2, 2, 2, tusk); R(3, 0, 2, 2, tusk); R(3, -2, 1, 2, tusk);
                    if (id == "presa_espiritual") { R(-5, -5, 1, 1, "#6fd8d2"); R(4, -4, 1, 1, "#6fd8d2"); R(0, 4, 1, 1, "#6fd8d2"); }
                    break;
                }
            case "tendao": O(-5, -3, 10, 6); R(-5, -1, 10, 2, "#efe4c8"); R(-3, -3, 2, 2, "#d8ccb0"); R(1, 1, 2, 2, "#d8ccb0"); R(-5, 1, 2, 1, "#c8b8a0"); break;
            case "tira_amarracao": O(-6, -2, 12, 4); R(-6, -2, 12, 2, "#a8845a"); R(-6, 0, 12, 2, "#8f6f4a"); R(-2, -2, 1, 4, "#5a3a24"); R(2, -2, 1, 4, "#5a3a24"); break;
            case "bolsa_simples": // a tied pouch
                O(-5, -4, 10, 10); R(-4, -1, 8, 7, "#8f6f4a"); R(-5, 1, 10, 4, "#8f6f4a"); R(-2, -4, 4, 3, "#a8845a"); R(-3, -1, 6, 1, "#5a3a24"); R(-3, 2, 2, 2, "#a8845a"); break;
            case "moedas": // a stack of coins
                O(-5, -3, 10, 8); R(-5, 1, 10, 3, "#c9a14a"); R(-5, 1, 10, 1, "#f0d27a"); R(-4, -2, 8, 3, "#d4ae58"); R(-4, -2, 8, 1, "#f5dc8a"); R(-1, -3, 2, 1, "#fff2c0"); R(-5, 4, 10, 1, "#8a6a2a"); break;
            // the prologue (prototype icons)
            case "espada": O(-7, -1, 14, 3); R(-2, -1, 9, 2, "#eef3f2"); R(-2, 1, 9, 1, "#b8c0c0"); R(-4, -2, 1, 5, "#c79e4e"); R(-7, -1, 3, 3, "#7a5a3a"); break;
            case "prata": R(-5, 0, 10, 3, "#c9ccd0"); R(-6, -2, 3, 3, "#e3e6ea"); R(3, -2, 3, 3, "#e3e6ea"); R(-2, -3, 4, 3, "#f4f6f7"); R(-5, 2, 10, 1, "#8e9196"); break;
            case "balsamo": R(-4, -2, 8, 7, "#8a5a3a"); R(-3, -4, 6, 2, "#a87048"); R(-2, -6, 4, 2, "#3f8f5a"); R(-3, 0, 6, 2, "#7fd08a"); R(2, -1, 1, 5, "#6a4128"); break;
            case "pedra": R(-1, -6, 2, 1, "#8fe8d8"); R(-3, -5, 6, 3, "#4fc0b0"); R(-4, -2, 8, 3, "#3aa89a"); R(-3, 1, 6, 2, "#2f8a80"); R(-1, 3, 2, 1, "#2f8a80"); R(-2, -4, 2, 2, "#d8fff6"); break;
            case "erva": R(0, -4, 1, 10, "#4f8a3a"); R(-4, -2, 4, 2, "#7fbf5a"); R(1, -4, 4, 2, "#7fbf5a"); R(-3, 1, 3, 2, "#6aa84a"); R(1, 2, 3, 2, "#6aa84a"); R(-1, -6, 3, 2, "#eef4d6"); break;
            case "faixa": R(-6, -2, 12, 4, "#a28f58"); R(-6, -2, 12, 1, "#c8b37a"); R(3, 2, 2, 5, "#a28f58"); R(0, 2, 2, 4, "#8f7c48"); break;
            case "pingente": R(0, -7, 1, 3, "#c79e4e"); R(-3, -4, 6, 1, "#5fc9a0"); R(-4, -3, 1, 6, "#5fc9a0"); R(3, -3, 1, 6, "#3fa880"); R(-3, 3, 6, 1, "#3fa880"); R(-1, 4, 2, 4, "#8f2d2d"); break;
            default: O(-4, -4, 8, 8); R(-4, -4, 8, 8, "#a89878"); R(-4, -1, 8, 1, "#5a3a24"); break;
        }
    }
}
