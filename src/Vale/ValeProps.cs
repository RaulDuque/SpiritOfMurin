using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Combat;

namespace SpiritOfMurin.Vale;

/// <summary>The prologue's items (the prototype's ITEMS): name, description, use.</summary>
public static class ValeItems
{
    /// <summary>Name and description of an item.</summary>
    public static (string Name, string Desc) Of(string id) => id switch
    {
        "balsamo" => ("Bálsamo de Ervas", "Recupera 4 de Vida. Use no inventário ou com [heal]."),
        "pedra" => ("Pedra de Ki", "Uma pedra de jade cheia de ki. Devolve todo o Ki."),
        "erva" => ("Erva Espiritual", "Uma erva que guarda ki. Fica guardada para a fornalha."),
        "prata" => ("Taéis de Prata", "A moeda das seitas. Ainda não há onde gastar."),
        "faixa" => ("Faixa da Seita", "A faixa dos discípulos da Guarda Fechada. Cada uma, um duelo vencido."),
        "pingente" => ("Pingente de Jade", "Do Espadachim Sênior. A prova da sua vitória."),
        "espada" => ("Espada do Espadachim", "A sua lâmina, desde antes do Vale."),
        _ => (id, ""),
    };

    /// <summary>Draws the item's icon at (0, 0) (the prototype's drawItemIcon).</summary>
    public static void Icon(CanvasItem ci, PixelPainter p, string id, Vector2 at)
    {
        void R(float x, float y, float w, float h, string c) => p.Rect(at.X + x, at.Y + y, w, h, new Color(c));
        switch (id)
        {
            case "prata": R(-5, 0, 10, 3, "#c9ccd0"); R(-6, -2, 3, 3, "#e3e6ea"); R(3, -2, 3, 3, "#e3e6ea"); R(-2, -3, 4, 3, "#f4f6f7"); R(-5, 2, 10, 1, "#8e9196"); break;
            case "balsamo": R(-4, -2, 8, 7, "#8a5a3a"); R(-3, -4, 6, 2, "#a87048"); R(-2, -6, 4, 2, "#3f8f5a"); R(-3, 0, 6, 2, "#7fd08a"); R(2, -1, 1, 5, "#6a4128"); break;
            case "pedra": R(-1, -6, 2, 1, "#8fe8d8"); R(-3, -5, 6, 3, "#4fc0b0"); R(-4, -2, 8, 3, "#3aa89a"); R(-3, 1, 6, 2, "#2f8a80"); R(-1, 3, 2, 1, "#2f8a80"); R(-2, -4, 2, 2, "#d8fff6"); break;
            case "erva": R(0, -4, 1, 10, "#4f8a3a"); R(-4, -2, 4, 2, "#7fbf5a"); R(1, -4, 4, 2, "#7fbf5a"); R(-3, 1, 3, 2, "#6aa84a"); R(1, 2, 3, 2, "#6aa84a"); R(-1, -6, 3, 2, "#eef4d6"); break;
            case "faixa": R(-6, -2, 12, 4, "#a28f58"); R(-6, -2, 12, 1, "#c8b37a"); R(3, 2, 2, 5, "#a28f58"); R(0, 2, 2, 4, "#8f7c48"); break;
            case "pingente": R(0, -7, 1, 3, "#c79e4e"); R(-3, -4, 6, 1, "#5fc9a0"); R(-4, -3, 1, 6, "#5fc9a0"); R(3, -3, 1, 6, "#3fa880"); R(-3, 3, 6, 1, "#3fa880"); R(-1, 4, 2, 4, "#8f2d2d"); break;
            default: R(-3, -3, 6, 6, "#a39e8c"); break;
        }
        p.Flush(ci, FighterArt.Outline);
    }
}

/// <summary>A breakable pot (index.html drawPot). A cut or a ki wave breaks it; it drops silver or a herb.</summary>
public partial class ValePot : StaticBody2D
{
    /// <summary>Index in <see cref="ValeMap.Pots"/> (saved when broken).</summary>
    public int Index { get; set; }
    private readonly PixelPainter _p = new();

    /// <inheritdoc />
    public override void _Ready() => AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(12, 5) }, Position = new Vector2(0, -2) });

    /// <inheritdoc />
    public override void _Draw()
    {
        DrawRect(new Rect2(-7, -2, 14, 4), new Color(8 / 255f, 10 / 255f, 8 / 255f, 0.35f));
        void R(float x, float y, float w, float h, string c) => _p.Rect(x, y, w, h, new Color(c));
        R(-5, -12, 10, 11, "#9a5b36"); R(-6, -10, 12, 7, "#9a5b36"); R(-3, -15, 6, 3, "#b8774a"); R(-4, -16, 8, 1, "#6e3f24");
        R(2, -11, 3, 9, "#6e3f24"); R(-6, -8, 12, 1, "#d9b48a"); R(-4, -11, 1, 3, "#c98d5e");
        _p.Flush(this, FighterArt.Outline);
    }
}

/// <summary>A spirit herb, picked by walking over it (index.html drawHerb).</summary>
public partial class ValeHerb : Node2D
{
    /// <summary>Index in <see cref="ValeMap.Herbs"/> (saved when picked).</summary>
    public int Index { get; set; }
    private float _t;
    private readonly PixelPainter _p = new();

    /// <inheritdoc />
    public override void _Process(double delta) { _t += (float)delta; QueueRedraw(); }

    /// <inheritdoc />
    public override void _Draw()
    {
        int sw = Mathf.RoundToInt(Mathf.Sin(_t * 2 + Position.X) * 1);
        void R(float x, float y, float w, float h, string c) => _p.Rect(x, y, w, h, new Color(c));
        R(-1, -7, 2, 7, "#3f7a34"); R(-6 + sw, -6, 5, 2, "#6aa84a"); R(1 + sw, -8, 5, 2, "#7fbf5a"); R(-4 + sw, -3, 3, 2, "#5a9a42");
        R(-1 + sw, -11, 3, 3, "#f2f7e0"); R(4 + sw, -10, 2, 2, "#f2f7e0");
        _p.Flush(this, FighterArt.Outline);
    }
}

/// <summary>The forgotten chest beyond the gap (index.html drawChest).</summary>
public partial class ValeChest : StaticBody2D
{
    /// <summary>True once opened.</summary>
    public bool Open { get; set; }
    private readonly PixelPainter _p = new();

    /// <inheritdoc />
    public override void _Ready() => AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(20, 6) }, Position = new Vector2(0, -3) });

    /// <inheritdoc />
    public override void _Draw()
    {
        DrawRect(new Rect2(-11, -2, 22, 4), new Color(8 / 255f, 10 / 255f, 8 / 255f, 0.35f));
        void R(float x, float y, float w, float h, string c) => _p.Rect(x, y, w, h, new Color(c));
        R(-10, -11, 20, 10, "#6d4a2c"); R(-10, -11, 20, 2, "#e2a640"); R(-10, -3, 20, 1, "#4a3120");
        if (Open) { R(-10, -19, 20, 6, "#5a3c24"); R(-10, -19, 20, 1, "#e2a640"); R(-8, -12, 16, 2, "#1c140c"); }
        else { R(-1, -9, 3, 4, "#ffd27a"); }
        _p.Flush(this, FighterArt.Outline);
    }
}

/// <summary>The fallen disciple in the bamboo valley; the torn page of the manual glows until taken.</summary>
public partial class ValeCorpse : Node2D
{
    /// <summary>True while the half manual is still on him.</summary>
    public bool WithPage { get; set; } = true;
    private float _t;
    private readonly PixelPainter _p = new();

    /// <inheritdoc />
    public override void _Process(double delta) { _t += (float)delta; QueueRedraw(); }

    /// <inheritdoc />
    public override void _Draw() => FighterArt.DrawDead(this, _p, FighterArt.Disciple, false, WithPage, _t);
}

/// <summary>An item thrown out by a defeat or a broken pot; it bounces, settles and is picked by walking over it.</summary>
public partial class ValeDrop : Node2D
{
    /// <summary>Item id.</summary>
    public string Item { get; set; } = "";
    /// <summary>Quantity.</summary>
    public int N { get; set; } = 1;
    /// <summary>Ground velocity.</summary>
    public Vector2 V { get; set; }
    /// <summary>Height of the bounce.</summary>
    public float Z { get; set; } = 2;
    /// <summary>Vertical speed of the bounce.</summary>
    public float Vz { get; set; } = 120;
    /// <summary>Seconds since it was thrown.</summary>
    public float Age { get; private set; }
    private readonly PixelPainter _p = new();

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        Age += dt;
        if (Z > 0 || Vz > 0)
        {
            Vz -= 420 * dt; Z = Mathf.Max(0, Z + Vz * dt); if (Z == 0) Vz = Vz < -60 ? -Vz * 0.35f : 0;
            Position += V * dt; V *= 0.96f;
        }
        QueueRedraw();
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        float bob = Z > 0 ? Z : Mathf.Sin(Age * 4 + Position.X) * 1.5f;
        DrawRect(new Rect2(-4, -1, 8, 2), new Color(8 / 255f, 10 / 255f, 8 / 255f, 0.35f));
        DrawRect(new Rect2(-7, -14 - bob, 14, 12), new Color(226 / 255f, 166 / 255f, 64 / 255f, 0.22f));
        ValeItems.Icon(this, _p, Item, new Vector2(0, -8 - Mathf.Round(bob)));
    }
}
