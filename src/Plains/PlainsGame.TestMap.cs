using System.Linq;
using Godot;
using SpiritOfMurin.Items;
using SpiritOfMurin.Save;

namespace SpiritOfMurin.Plains;

/// <summary>
/// The test map (pedido do criador, 2026-11-28, junto da T18): the Planície opened with <c>-- --mapa-testes</c> (Painel →
/// "Mapa de testes"). A fresh journey every time, in its own file (<see cref="TestMapSave"/>, never the creator's journey),
/// with every item of the catalogue in the bag and a showcase south of the campfire: one of each weapon, robe, hat and tool on
/// the ground, named, to pick up with E. The weapons' playable techniques come known, so their look shows at once.
/// What goes in comes from the catalogue itself (<c>plains_items.json</c> and <c>assets/data/equipamento/</c>): a new piece
/// made with <c>/create</c> appears here with no list to update.
/// </summary>
public partial class PlainsGame
{
    /// <summary>The test map's own journey file.</summary>
    public const string TestMapSave = "user://teste_mapa.json";

    /// <summary>The test map's seed (the creator's reference seed).</summary>
    public const int TestMapSeed = 20261126;

    /// <summary>True when the Planície was opened as the test map.</summary>
    public bool TestMap { get; set; }

    /// <summary>How many of each kind the test map puts in the bag: pieces and tools, things to use, materials.</summary>
    private const int MapPieces = 1, MapUsables = 5, MapMaterials = 10, MapCoins = 100;

    /// <summary>Showcase layout: pieces per row and the spacing (px), from a spot south-west of the campfire.</summary>
    private const int ShowcasePerRow = 5;
    private static readonly Vector2 ShowcaseStep = new(52, 42), ShowcaseFrom = new(-110, 46);

    /// <summary>A name split at the space nearest its middle, so two neighbours in the showcase do not overlap.</summary>
    public static string TwoLines(string name)
    {
        int mid = name.Length / 2, best = -1;
        for (int i = 0; i < name.Length; i++) if (name[i] == ' ' && (best < 0 || System.Math.Abs(i - mid) < System.Math.Abs(best - mid))) best = i;
        return best < 0 ? name : name[..best] + "\n" + name[(best + 1)..];
    }

    // fills the bag and lays the showcase (called once the journey has started)
    private void SetupTestMap()
    {
        foreach (var d in Catalog.Items.Values.OrderBy(d => d.Id, System.StringComparer.Ordinal))
        {
            if (d.Base || d.Id is "trouxa_viajante" or "moedas") continue;
            bool piece = d.Weapon != null || d.Wear != null || d.Tool != null || d.Equip.Length > 0;
            Inventory.Add(d.Id, piece ? MapPieces : d.Usable ? MapUsables : MapMaterials);
        }
        ReceiveCoins(MapCoins);
        foreach (var t in WeaponTechs.Values.Where(t => t.Playable)) KnownWeaponTechs.Add(t.Id);
        var shown = Catalog.Items.Values.Where(d => !d.Base && (d.Weapon != null || d.Wear != null || d.Tool != null)).OrderBy(d => d.Wear != null ? 1 : 0).ThenBy(d => d.Id, System.StringComparer.Ordinal).ToList();
        var camp = World.Objects.GetChildren().OfType<Campfire>().FirstOrDefault();
        Vector2 origin = (camp?.GlobalPosition ?? Player.GlobalPosition) + ShowcaseFrom;
        for (int i = 0; i < shown.Count; i++)
        {
            var d = shown[i];
            var at = origin + new Vector2(i % ShowcasePerRow * ShowcaseStep.X, i / ShowcasePerRow * ShowcaseStep.Y);
            var p = new Pickup { FeatureId = $"mapa#{d.Id}", ItemId = d.Id, Kind = FeatureKind.Bundle, IconItem = d.Id, Position = at, Name = $"Mapa_{d.Id}" };
            var label = new Label { Text = TwoLines(d.Name), Position = new Vector2(-26, -36), Size = new Vector2(52, 18), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, ZIndex = 10 };
            label.AddThemeFontSizeOverride("font_size", 6);
            label.AddThemeColorOverride("font_color", new Color("#ece5d0"));
            label.AddThemeColorOverride("font_outline_color", new Color("#16140f"));
            label.AddThemeConstantOverride("outline_size", 2);
            p.AddChild(label);
            World.Objects.AddChild(p);
        }
        RefreshRows(); RefreshQuickBar(); RefreshHands();
        Hud.Toast(Game.Bindings.F("Mapa de testes: tudo na bolsa ([inventory]) e um mostruário ao sul da fogueira (E pega). Este mapa usa um arquivo próprio: a sua jornada não muda."), important: true);
    }
}
