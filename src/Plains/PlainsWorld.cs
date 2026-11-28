using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SpiritOfMurin.Characters;
using SpiritOfMurin.Creatures;
using SpiritOfMurin.Progression;
using SpiritOfMurin.World;

namespace SpiritOfMurin.Plains;

/// <summary>
/// Streams the Planície around a focus (the player): builds the chunks within
/// <see cref="PlainsGenSettings.LoadRadius"/> and frees those beyond <see cref="PlainsGenSettings.UnloadRadius"/>.
/// A built chunk is a ground drawing, a water collider and the nodes of its objects, which go into the
/// y-sorted <see cref="Objects"/> layer. Objects already collected and animals already killed (see
/// <see cref="WorldState"/>) are not rebuilt; a killed animal comes back as its carcass.
/// </summary>
public partial class PlainsWorld : Node2D
{
    /// <summary>The generator (set before the node enters the tree, or via <see cref="Setup"/>).</summary>
    public PlainsGenerator Gen { get; private set; } = null!;

    /// <summary>What changed this session.</summary>
    public WorldState State { get; private set; } = new();

    /// <summary>The y-sorted layer where objects, animals and the player live.</summary>
    public Node2D Objects { get; set; } = null!;

    /// <summary>The player animals react to.</summary>
    public Player? Player { get; set; }
    /// <summary>The ambience numbers (tall grass per chunk).</summary>
    public AmbientSettings Ambient { get; set; } = new();

    /// <summary>Rabbit tuning.</summary>
    public RabbitTuning RabbitTuning { get; set; } = new();

    /// <summary>Boar tuning (level-1 of the kind's range; each animal gets a copy scaled to its level).</summary>
    public BoarTuning BoarTuning { get; set; } = new();

    /// <summary>The elements (trees, rocks, bamboo, bushes) the generator's features become; null = the old decor only.</summary>
    public SpiritOfMurin.Elements.ElementCatalog? Elements { get; set; }

    /// <summary>Raised for every workable element created (so the game can hook its events).</summary>
    public event Action<SpiritOfMurin.Elements.WorldElement>? ElementSpawned;

    /// <summary>Progression data (creature levels); null keeps every animal at its base tuning.</summary>
    public ProgressionData? Progression { get; set; }

    /// <summary>Raised when an animal spawned by a chunk dies.</summary>
    public event Action<IHittable, Vector2>? CreatureDied;

    /// <summary>Raised for every creature node created (so the game can hook its events).</summary>
    public event Action<Node>? CreatureSpawned;

    private sealed class Chunk
    {
        public ChunkData Data = null!;
        public readonly List<Node> Nodes = new();
    }

    private readonly Dictionary<Vector2I, Chunk> _chunks = new();
    private Node2D _groundLayer = null!;

    /// <summary>Chunks currently built.</summary>
    public IEnumerable<Vector2I> LoadedChunks => _chunks.Keys;

    /// <summary>Times a chunk was built (for checks: re-entering builds it again).</summary>
    public int ChunkBuilds { get; private set; }

    /// <summary>Prepares the world for a seed. Call once, before the first <see cref="UpdateAround"/>.</summary>
    public void Setup(PlainsGenerator gen, WorldState state)
    {
        Gen = gen; State = state;
        _groundLayer = new Node2D { Name = "GroundLayer", ZIndex = -10 };
        AddChild(_groundLayer);
    }

    /// <summary>Builds and frees chunks around a world position. Returns true when something changed.</summary>
    public bool UpdateAround(Vector2 focus)
    {
        var s = Gen.Settings;
        Vector2I c = Gen.ChunkOfPixel(focus);
        bool changed = false;
        for (int dy = -s.LoadRadius; dy <= s.LoadRadius; dy++)
            for (int dx = -s.LoadRadius; dx <= s.LoadRadius; dx++)
            {
                var k = new Vector2I(c.X + dx, c.Y + dy);
                if (!_chunks.ContainsKey(k)) { Build(k); changed = true; }
            }
        var drop = new List<Vector2I>();
        foreach (var k in _chunks.Keys)
            if (Math.Max(Math.Abs(k.X - c.X), Math.Abs(k.Y - c.Y)) > s.UnloadRadius) drop.Add(k);
        foreach (var k in drop) { Free(k); changed = true; }
        return changed;
    }

    /// <summary>The data of a built chunk, or null.</summary>
    public ChunkData? ChunkAt(Vector2I k) => _chunks.TryGetValue(k, out var ch) ? ch.Data : null;

    private void Free(Vector2I k)
    {
        var ch = _chunks[k];
        foreach (var n in ch.Nodes) if (IsInstanceValid(n)) n.QueueFree();
        _chunks.Remove(k);
    }

    private void Build(Vector2I k)
    {
        ChunkBuilds++;
        var data = Gen.GenerateChunk(k);
        var ch = new Chunk { Data = data };
        _chunks[k] = ch;
        int px = Gen.Settings.ChunkTiles * Gen.Settings.TileSize;
        var ground = new ChunkGround { Data = data, Gen = Gen, Position = new Vector2(k.X * px, k.Y * px), Name = $"Ground_{k.X}_{k.Y}" };
        _groundLayer.AddChild(ground);
        ch.Nodes.Add(ground);
        var grass = new GrassSway { Data = data, Gen = Gen, Settings = Ambient, Player = Player, Position = ground.Position, Name = $"Grass_{k.X}_{k.Y}" };
        _groundLayer.AddChild(grass);
        ch.Nodes.Add(grass);
        var water = BuildWater(data, k);
        if (water != null) { AddChild(water); ch.Nodes.Add(water); }
        foreach (var f in data.Features)
        {
            Node? n = MakeNode(f);
            if (n == null) continue;
            Objects.AddChild(n);
            ch.Nodes.Add(n);
        }
    }

    /// <summary>
    /// The physics layer of deep water (pedido 14.3): the animals collide with it (their mask has it), the character
    /// does not (walking on it or swimming, see <see cref="WaterRules"/>).
    /// </summary>
    public const uint WaterLayer = 16;

    // deep water blocks the animals; merges each row's run of water tiles into one rectangle
    private StaticBody2D? BuildWater(ChunkData data, Vector2I k)
    {
        int n = data.Size, ts = Gen.Settings.TileSize;
        StaticBody2D? body = null;
        for (int ly = 0; ly < n; ly++)
        {
            int start = -1;
            for (int lx = 0; lx <= n; lx++)
            {
                bool w = lx < n && data[lx, ly] == TileKind.Water;
                if (w && start < 0) start = lx;
                if (!w && start >= 0)
                {
                    body ??= new StaticBody2D { Name = $"Water_{k.X}_{k.Y}", CollisionLayer = WaterLayer, CollisionMask = 0 };
                    float x0 = (k.X * n + start) * ts, y0 = (k.Y * n + ly) * ts, wpx = (lx - start) * ts;
                    body.AddChild(new CollisionShape2D { Position = new Vector2(x0 + wpx / 2, y0 + ts / 2f), Shape = new RectangleShape2D { Size = new Vector2(wpx, ts) } });
                    start = -1;
                }
            }
        }
        return body;
    }

    // pedido 14, Entrega 5: which element a tree, rock, bamboo or bush feature becomes (data in assets/data/elements).
    // The clearing's guaranteed ones stay as they were (decor), so the waking place never changes.
    private Node MakeElement(Feature f)
    {
        string family = f.Kind switch { FeatureKind.Pine => "arvore", FeatureKind.Boulder => "rocha", FeatureKind.Bamboo => "bambu", _ => "arbusto" };
        if (Elements == null || (Gen.IsForced(f.Tile) && f.Kind != FeatureKind.Bush))
        {
            var kind = f.Kind switch { FeatureKind.Pine => Obstacle.Kind.Pine, FeatureKind.Boulder => Obstacle.Kind.Boulder, FeatureKind.Bamboo => Obstacle.Kind.Bamboo, _ => Obstacle.Kind.Bush };
            return new Obstacle { Type = kind, Variation = f.Variation, Position = f.Position, Name = Safe(f.Id) };
        }
        string habitat = Gen.HabitatAt(f.Tile.X, f.Tile.Y).ToString().ToLowerInvariant();
        var def = Elements.Pick(family, habitat, Gen.RollAt(f.Tile, 50)) ?? Elements.Family(family).FirstOrDefault();
        if (def == null) return new Obstacle { Type = Obstacle.Kind.Pine, Variation = f.Variation, Position = f.Position, Name = Safe(f.Id) };
        var e = new SpiritOfMurin.Elements.WorldElement
        {
            Def = def, PlaceId = f.Id, Variation = f.Variation, Work = State.WorkOf(f.Id), Position = f.Position, Name = Safe(f.Id),
            Movers = def.ContactRadius > 0 ? MoversNear : null,
        };
        if (def.Workable) ElementSpawned?.Invoke(e);
        return e;
    }

    // the bodies that bend the bushes: the swordsman and the animals
    private IEnumerable<CharacterBody2D> MoversNear()
    {
        if (Player != null) yield return Player;
        foreach (var n in GetTree().GetNodesInGroup("creatures")) if (n is CharacterBody2D b) yield return b;
    }

    /// <summary>Item id of a collectable feature, or null.</summary>
    public static string? ItemOf(FeatureKind k) => k switch
    {
        FeatureKind.Flint => "silex",
        FeatureKind.Branch => "galho_rijo",
        FeatureKind.Herb => "grama_sangue",
        FeatureKind.Root => "raiz_amarga",
        FeatureKind.CoinPouch => "moedas",
        FeatureKind.Bundle => "trouxa_viajante",
        _ => null,
    };

    /// <summary>Parts a carcass yields, by animal kind, each taken once: hide (Esfolar), meat (Preparo de Caça) and, for the boar, tusks.</summary>
    public static List<string> PartsOf(bool boar) => boar ? new() { "hide", "meat", "tusks" } : new() { "hide", "meat" };

    /// <summary>The level of the animal with that id (fixed for the seed).</summary>
    public int LevelOf(string kind, string creatureId) => Progression == null ? (kind == "boar" ? 3 : 1) : HarvestRules.CreatureLevel(Progression, Gen.Seed, kind, creatureId);

    private Node? MakeNode(Feature f)
    {
        switch (f.Kind)
        {
            case FeatureKind.Flint or FeatureKind.Branch or FeatureKind.Herb or FeatureKind.Root or FeatureKind.CoinPouch or FeatureKind.Bundle:
                if (State.IsCollected(f.Id)) return null;
                return new Pickup { FeatureId = f.Id, ItemId = ItemOf(f.Kind)!, Kind = f.Kind, Variation = f.Variation, Position = f.Position, Name = f.Id.Replace('@', '_').Replace(',', '_') };
            case FeatureKind.Pine or FeatureKind.Boulder or FeatureKind.Bush or FeatureKind.Bamboo:
                return MakeElement(f);
            case FeatureKind.Track:
            {
                var parts = f.Link.Split('|');
                int idx = int.Parse(f.Id[(f.Id.IndexOf('#') + 1)..]);
                return new TrackPrint { FeatureId = f.Id, TrailId = parts[0], CreatureId = parts.Length > 1 ? parts[1] : "", Index = idx, Dir = f.Dir, Position = f.Position, ZIndex = -5, Name = Safe(f.Id).Replace('#', '_') };
            }
            case FeatureKind.Campfire:
                return new Campfire { Position = f.Position, Name = "Campfire" };
            case FeatureKind.MeditationMat:
                return new MeditationMat { Position = f.Position, Name = "MeditationMat", ZIndex = -4 };
            case FeatureKind.Burrow:
                return Creature($"rabbit@{f.Tile.X},{f.Tile.Y}", false, f);
            case FeatureKind.Boar:
                return Creature($"boar@{f.Tile.X},{f.Tile.Y}", true, f);
            default:
                return null; // rooted earth is a ground mark
        }
    }

    private static string Safe(string id) => id.Replace('@', '_').Replace(',', '_');

    private Node? Creature(string id, bool boar, Feature f)
    {
        if (State.CarcassAt(id) is { } at)
        {
            var parts = PartsOf(boar).FindAll(p => !State.IsPartTaken(id, p));
            if (parts.Count == 0) return null;
            return new Carcass { CreatureId = id, IsBoar = boar, Parts = parts, Position = at, Name = Safe(id) + "_carcass" };
        }
        int level = LevelOf(boar ? "boar" : "rabbit", id);
        CreatureDef? def = null;
        Progression?.Creatures.TryGetValue(boar ? "boar" : "rabbit", out def);
        Node2D n = boar
            ? new Boar { CreatureId = id, Level = level, Tuning = def == null ? BoarTuning : CreatureLevels.Boar(BoarTuning, def, level), Target = Player, HomeAt = f.Position, Variation = f.Variation, Position = f.Position, Name = Safe(id) }
            : new Rabbit { CreatureId = id, Level = level, Tuning = def == null ? RabbitTuning : CreatureLevels.Rabbit(RabbitTuning, def, level), Target = Player, BurrowAt = f.Position, Variation = f.Variation, Position = f.Position + new Vector2(14, 6), Name = Safe(id) };
        ((IHittable)n).Died += OnDied;
        CreatureSpawned?.Invoke(n);
        return n;
    }

    private void OnDied(IHittable who, Vector2 at)
    {
        State.MarkDead(who.CreatureId, at);
        var node = (Node2D)who;
        bool boar = who is Boar;
        var carcass = new Carcass { CreatureId = who.CreatureId, IsBoar = boar, Parts = PartsOf(boar), Position = at, Name = Safe(who.CreatureId) + "_carcass" };
        // the carcass belongs to the same chunk as the animal did
        foreach (var ch in _chunks.Values)
            if (ch.Nodes.Remove(node)) { ch.Nodes.Add(carcass); break; }
        Objects.CallDeferred(Node.MethodName.AddChild, carcass);
        node.QueueFree();
        CreatureDied?.Invoke(who, at);
    }

    /// <summary>Removes a node from its chunk's bookkeeping and frees it (a pickup taken, an emptied carcass).</summary>
    public void Remove(Node n)
    {
        foreach (var ch in _chunks.Values) if (ch.Nodes.Remove(n)) break;
        n.QueueFree();
    }
}
