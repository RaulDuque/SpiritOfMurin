using System.Collections.Generic;
using System.Linq;
using Godot;
using SpiritOfMurin.Plains;
using static SpiritOfMurin.Tests.Unit.Plains.PlainsFixtures;

namespace SpiritOfMurin.Tests.Unit.Plains;

/// <summary>Seeded generation: determinism, visiting order, chunk borders, the guaranteed start.</summary>
public static class PlainsGeneratorTests
{
    private static string Signature(ChunkData c) =>
        string.Join(",", c.Tiles.Select(t => (int)t)) + "|" + string.Join(";", c.Features.Select(f => $"{f.Id}:{f.Position.X:0.###},{f.Position.Y:0.###}:{f.Variation}"));

    private static readonly Vector2I[] Block = Enumerable.Range(-2, 5).SelectMany(y => Enumerable.Range(-2, 5).Select(x => new Vector2I(x, y))).ToArray();

    /// <summary>Same seed, chunks visited in different orders (and by different generator instances): identical chunks.</summary>
    [UnitTest]
    public static void SameSeed_DifferentVisitOrders_ProduceIdenticalChunks()
    {
        foreach (int seed in new[] { SeedA, SeedB })
        {
            // Arrange: three orders over the same 5×5 block — row by row, reversed, and a spiral walk that comes back
            var rowOrder = Block.ToList();
            var reversed = Block.Reverse().ToList();
            var spiral = new List<Vector2I> { new(0, 0), new(1, 0), new(1, 1), new(0, 1), new(-1, 1), new(-1, 0), new(-1, -1), new(0, -1), new(1, -1), new(2, -1), new(0, 0) };
            spiral.AddRange(Block.OrderBy(v => (v.X * 7 + v.Y * 13) % 5));
            // Act
            var a = new Dictionary<Vector2I, string>(); var genA = Gen(seed);
            foreach (var k in rowOrder) a[k] = Signature(genA.GenerateChunk(k));
            var b = new Dictionary<Vector2I, string>(); var genB = Gen(seed);
            foreach (var k in reversed) b[k] = Signature(genB.GenerateChunk(k));
            var genC = Gen(seed);
            var c = new Dictionary<Vector2I, string>();
            foreach (var k in spiral)
            {
                string s = Signature(genC.GenerateChunk(k));
                if (c.TryGetValue(k, out var before)) Assert.Equal(before, s, $"seed {seed}: chunk {k} generated twice differs");
                c[k] = s;
            }
            // Assert
            foreach (var k in Block)
            {
                Assert.Equal(a[k], b[k], $"seed {seed}: chunk {k} row order vs reversed");
                Assert.Equal(a[k], c[k], $"seed {seed}: chunk {k} row order vs spiral");
            }
        }
    }

    /// <summary>Two seeds give different worlds.</summary>
    [UnitTest]
    public static void DifferentSeeds_ProduceDifferentWorlds()
    {
        // Arrange
        var a = Gen(SeedA); var b = Gen(SeedB);
        // Act
        int differentChunks = Block.Count(k => Signature(a.GenerateChunk(k)) != Signature(b.GenerateChunk(k)));
        // Assert
        Assert.True(differentChunks >= Block.Length - 2, $"only {differentChunks}/{Block.Length} chunks differ");
        Assert.True(a.BushAnchor != b.BushAnchor || a.MudAnchor != b.MudAnchor, "anchors identical");
    }

    /// <summary>Chunk edges: each tile in a chunk equals the world function, so neighbours meet; the creek runs on across borders.</summary>
    [UnitTest]
    public static void ChunkBorders_MatchWorldTilesAndCreekContinues()
    {
        foreach (int seed in new[] { SeedA, SeedB })
        {
            // Arrange
            var g = Gen(seed); int n = g.Settings.ChunkTiles;
            var chunks = Block.ToDictionary(k => k, g.GenerateChunk);
            // Act / Assert: every edge tile equals TileAt, and the tile across the border (in the neighbour chunk) too
            foreach (var (k, c) in chunks)
                for (int i = 0; i < n; i++)
                    foreach (var (lx, ly) in new[] { (0, i), (n - 1, i), (i, 0), (i, n - 1) })
                    {
                        int tx = k.X * n + lx, ty = k.Y * n + ly;
                        Assert.Equal(g.TileAt(tx, ty), c[lx, ly], $"seed {seed} chunk {k} tile ({tx},{ty})");
                    }
            // the creek: every row of the block has water within one tile of the row above (no gap at chunk rows)
            for (int ty = -2 * n; ty < 3 * n - 1; ty++)
            {
                var row = Enumerable.Range(-2 * n, 5 * n).Where(tx => g.TileAt(tx, ty) is TileKind.Water or TileKind.Ford).ToList();
                var next = Enumerable.Range(-2 * n, 5 * n).Where(tx => g.TileAt(tx, ty + 1) is TileKind.Water or TileKind.Ford).ToList();
                Assert.True(row.Count > 0 && next.Count > 0, $"seed {seed}: creek missing at row {ty}");
                Assert.True(row.Any(x => next.Any(y => System.Math.Abs(x - y) <= 1)), $"seed {seed}: creek breaks between rows {ty} and {ty + 1}");
            }
        }
    }

    /// <summary>Each object belongs to exactly one chunk: ids are unique across a block and lie in their owning chunk.</summary>
    [UnitTest]
    public static void Features_HaveUniqueIdsAndOneOwningChunk()
    {
        foreach (int seed in new[] { SeedA, SeedB })
        {
            // Arrange
            var g = Gen(seed);
            // Act
            var all = Block.SelectMany(k => g.GenerateChunk(k).Features.Select(f => (k, f))).ToList();
            // Assert
            Assert.Equal(all.Count, all.Select(x => x.f.Id).Distinct().Count(), $"seed {seed}: duplicate feature ids");
            foreach (var (k, f) in all) Assert.Equal(k, g.ChunkOfTile(f.Tile.X, f.Tile.Y), $"seed {seed}: {f.Id} owner");
        }
    }

    private static bool Blocks(FeatureKind k) => k is FeatureKind.Pine or FeatureKind.Boulder or FeatureKind.Bush;

    /// <summary>The start: open clearing, first materials on the bank, herb by a stone, burrows and a boar with roots, all reachable on foot.</summary>
    [UnitTest]
    public static void Start_IsWalkableWithFirstMaterialsAndAllHabitatsReachable()
    {
        for (int seed = 1; seed <= 60; seed++)
        {
            // Arrange
            var g = Gen(seed == 1 ? SeedA : seed == 2 ? SeedB : seed * 7919);
            int R = 26;
            var blocked = new HashSet<Vector2I>();
            var feats = new List<Feature>();
            for (int ty = -R; ty <= R; ty++)
                for (int tx = -R; tx <= R; tx++)
                {
                    if (!PlainsGenerator.IsWalkableGround(g.TileAt(tx, ty))) blocked.Add(new Vector2I(tx, ty));
                    foreach (var f in g.FeaturesAt(tx, ty)) { feats.Add(f); if (Blocks(f.Kind)) blocked.Add(f.Tile); }
                }
            // Act: flood fill from the start over walkable tiles
            var seen = new HashSet<Vector2I> { Vector2I.Zero };
            var q = new Queue<Vector2I>(); q.Enqueue(Vector2I.Zero);
            while (q.Count > 0)
            {
                var p = q.Dequeue();
                foreach (var d in new[] { Vector2I.Left, Vector2I.Right, Vector2I.Up, Vector2I.Down })
                {
                    var nb = p + d;
                    if (System.Math.Abs(nb.X) > R || System.Math.Abs(nb.Y) > R || blocked.Contains(nb) || !seen.Add(nb)) continue;
                    q.Enqueue(nb);
                }
            }
            bool Reach(Feature f) => seen.Contains(f.Tile) || new[] { Vector2I.Left, Vector2I.Right, Vector2I.Up, Vector2I.Down }.Any(d => seen.Contains(f.Tile + d));
            // Assert
            for (int ty = -4; ty <= 4; ty++) for (int tx = -4; tx <= 4; tx++)
                if (new Vector2(tx + 0.5f, ty + 0.5f).Length() < g.Settings.ClearingRadius)
                    Assert.True(!blocked.Contains(new Vector2I(tx, ty)), $"seed {g.Seed}: clearing blocked at ({tx},{ty})");
            foreach (var (kind, max) in new[] { (FeatureKind.Flint, 10f), (FeatureKind.Branch, 10f), (FeatureKind.Herb, 10f), (FeatureKind.Burrow, 22f), (FeatureKind.Boar, 22f), (FeatureKind.Root, 24f) })
                Assert.True(feats.Any(f => f.Kind == kind && Reach(f) && f.Tile.Length() <= max), $"seed {g.Seed}: no reachable {kind} within {max} tiles");
            Assert.True(feats.Any(f => f.Kind == FeatureKind.Flint && g.HabitatAt(f.Tile.X, f.Tile.Y) == Habitat.Bank), $"seed {g.Seed}: no flint on the bank");
            Assert.True(feats.Any(f => f.Kind == FeatureKind.Herb && feats.Any(b => b.Kind == FeatureKind.Boulder && b.Position.DistanceTo(f.Position) < 24)), $"seed {g.Seed}: no herb by a stone");
            Assert.True(feats.Any(f => f.Kind == FeatureKind.Root && feats.Any(r => r.Kind == FeatureKind.Rooted && r.Position.DistanceTo(f.Position) < 12)), $"seed {g.Seed}: no root on rooted earth");
            Assert.True(feats.Any(f => f.Kind == FeatureKind.Burrow && g.HabitatAt(f.Tile.X, f.Tile.Y) == Habitat.Bushes), $"seed {g.Seed}: no burrow in the bush field");
            Assert.True(feats.Any(f => f.Kind == FeatureKind.Boar && g.HabitatAt(f.Tile.X, f.Tile.Y) == Habitat.Mud), $"seed {g.Seed}: no boar in the mud");
        }
    }

    /// <summary>A stamp claims tiles before the random rules (the hook for handmade towns and tombs).</summary>
    [UnitTest]
    public static void Stamp_ClaimsTileGroundAndObjects()
    {
        // Arrange
        var g = Gen(SeedA);
        var t = new Vector2I(-40, 40);
        // Act
        g.AddStamp(t, TileKind.Bank, new[] { (FeatureKind.Boulder, new Vector2(16, 20)) });
        // Assert
        Assert.Equal(TileKind.Bank, g.TileAt(t.X, t.Y), "stamped ground");
        var f = g.FeaturesAt(t.X, t.Y).ToList();
        Assert.True(f.Count == 1 && f[0].Kind == FeatureKind.Boulder, "stamped objects");
    }
}
