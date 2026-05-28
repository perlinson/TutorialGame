using System.Collections.Generic;
using UnityEngine;

public interface IExpeditionArenaContext
{
    Vector2 ArenaMinBounds { get; }
    Vector2 ArenaMaxBounds { get; }
    Color AccentColor { get; }
    Color InnerGroundColor { get; }
    void SpawnSpiritNode(Vector2 position, int qiAmount);
    void SpawnHerb(Vector2 position, int healAmount, int qiAmount);
    void SpawnRelic(Vector2 position, int crystalAmount);
    void CreateDecor(string name, Vector2 position, Vector2 size, Color color, int sortingOrder);
}

public sealed class ScoutRoomStrategy : IRoomKindStrategy
{
    public ExpeditionRoomKind Kind => ExpeditionRoomKind.Scout;
    public string DefaultTitle => "山门外缘";
    public string DefaultDescription => "远征队刚踏出山门旧界，需要先重新校正地脉与撤退路径。";
    public string Symbol => "途";
    public bool IsCombatRoom => false;

    public int DefaultEnemyCount(WorldRegionDefinition region) => 0;

    public void SpawnEventActors(GameObject arenaRoot, ExpeditionRoomState room, WorldRegionDefinition region, System.Random random, GameController controller)
    {
        var context = new ArenaContextAdapter(region, controller);
        var nodeCount = Mathf.Clamp(1 + region.DangerRank / 2, 1, 3);
        var qiAmount = Mathf.Max(1, 1 + region.RequiredRealmTier / 2);
        for (var i = 0; i < nodeCount; i++)
        {
            context.SpawnSpiritNode(SamplePoint(context, random, -0.1f, 0.26f), qiAmount);
        }
    }

    public void ApplyFallbackLoot(List<SaveItemStack> loot, string regionId)
    {
        AddLoot(loot, "green_spirit_sand", 1);
    }

    private static Vector2 SamplePoint(ArenaContextAdapter context, System.Random random, float xBias, float yBias)
    {
        var xMin = context.ArenaMinBounds.x + 1.8f;
        var xMax = context.ArenaMaxBounds.x - 1.2f;
        var yMin = context.ArenaMinBounds.y + 1.2f;
        var yMax = context.ArenaMaxBounds.y - 0.8f;
        var x = Mathf.Lerp(xMin, xMax, Mathf.Clamp01((float)random.NextDouble() + xBias));
        var y = Mathf.Lerp(yMin, yMax, Mathf.Clamp01((float)random.NextDouble() + yBias));
        return new Vector2(x, y);
    }

    private static void AddLoot(List<SaveItemStack> loot, string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return;
        for (var i = 0; i < loot.Count; i++)
        {
            if (loot[i].itemId == itemId) { loot[i].quantity += quantity; return; }
        }
        loot.Add(new SaveItemStack(itemId, quantity));
    }
}

public sealed class BattleRoomStrategy : IRoomKindStrategy
{
    public ExpeditionRoomKind Kind => ExpeditionRoomKind.Battle;
    public string DefaultTitle => "阴影甬道";
    public string DefaultDescription => "甬道里残留着新近踩踏痕迹，若继续前探，多半要与伏兵碰面。";
    public string Symbol => "战";
    public bool IsCombatRoom => true;

    public int DefaultEnemyCount(WorldRegionDefinition region) => 1 + region.DangerRank / 2;

    public void SpawnEventActors(GameObject arenaRoot, ExpeditionRoomState room, WorldRegionDefinition region, System.Random random, GameController controller) { }

    public void ApplyFallbackLoot(List<SaveItemStack> loot, string regionId) { }
}

public sealed class EliteRoomStrategy : IRoomKindStrategy
{
    public ExpeditionRoomKind Kind => ExpeditionRoomKind.Elite;
    public string DefaultTitle => "险隘关口";
    public string DefaultDescription => "狭窄地形迫使队伍正面接战，这里通常盘踞着更难缠的敌手。";
    public string Symbol => "险";
    public bool IsCombatRoom => true;

    public int DefaultEnemyCount(WorldRegionDefinition region) => 2;

    public void SpawnEventActors(GameObject arenaRoot, ExpeditionRoomState room, WorldRegionDefinition region, System.Random random, GameController controller) { }

    public void ApplyFallbackLoot(List<SaveItemStack> loot, string regionId) { }
}

public sealed class TreasureRoomStrategy : IRoomKindStrategy
{
    public ExpeditionRoomKind Kind => ExpeditionRoomKind.Treasure;
    public string DefaultTitle => "散落行囊";
    public string DefaultDescription => "破损行囊和散碎残卷躺在角落，像是在引诱后人俯身搜查。";
    public string Symbol => "宝";
    public bool IsCombatRoom => false;

    public int DefaultEnemyCount(WorldRegionDefinition region) => 0;

    public void SpawnEventActors(GameObject arenaRoot, ExpeditionRoomState room, WorldRegionDefinition region, System.Random random, GameController controller)
    {
        var context = new ArenaContextAdapter(region, controller);
        var relicCount = 1 + region.DangerRank / 3;
        var crystalAmount = 1 + region.RequiredRealmTier;
        for (var i = 0; i < relicCount; i++)
        {
            context.SpawnRelic(SamplePoint(context, random, 0.02f, 0.34f), crystalAmount);
        }
        context.CreateDecor("TreasurePile", new Vector2(2.4f, -0.8f), new Vector2(1.1f, 0.8f), new Color(0.52f, 0.4f, 0.18f, 0.72f), -6);
    }

    public void ApplyFallbackLoot(List<SaveItemStack> loot, string regionId)
    {
        AddLoot(loot, "array_shard", 1);
        AddLoot(loot, InventoryLibrary.GetRegionalRareItemId(regionId), 1);
    }

    private static Vector2 SamplePoint(ArenaContextAdapter context, System.Random random, float xBias, float yBias)
    {
        var xMin = context.ArenaMinBounds.x + 1.8f;
        var xMax = context.ArenaMaxBounds.x - 1.2f;
        var yMin = context.ArenaMinBounds.y + 1.2f;
        var yMax = context.ArenaMaxBounds.y - 0.8f;
        var x = Mathf.Lerp(xMin, xMax, Mathf.Clamp01((float)random.NextDouble() + xBias));
        var y = Mathf.Lerp(yMin, yMax, Mathf.Clamp01((float)random.NextDouble() + yBias));
        return new Vector2(x, y);
    }

    private static void AddLoot(List<SaveItemStack> loot, string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return;
        for (var i = 0; i < loot.Count; i++)
        {
            if (loot[i].itemId == itemId) { loot[i].quantity += quantity; return; }
        }
        loot.Add(new SaveItemStack(itemId, quantity));
    }
}

public sealed class HerbRoomStrategy : IRoomKindStrategy
{
    public ExpeditionRoomKind Kind => ExpeditionRoomKind.Herb;
    public string DefaultTitle => "灵草湿地";
    public string DefaultDescription => "潮气从裂缝翻涌而上，这种地方常能长出稳神与疗伤类灵草。";
    public string Symbol => "药";
    public bool IsCombatRoom => false;

    public int DefaultEnemyCount(WorldRegionDefinition region) => 0;

    public void SpawnEventActors(GameObject arenaRoot, ExpeditionRoomState room, WorldRegionDefinition region, System.Random random, GameController controller)
    {
        var context = new ArenaContextAdapter(region, controller);
        var herbCount = Mathf.Clamp(1 + region.HerbCount / 3, 1, 3);
        var healAmount = 1 + region.RequiredRealmTier;
        var qiAmount = 1 + region.RequiredRealmTier;
        for (var i = 0; i < herbCount; i++)
        {
            context.SpawnHerb(SamplePoint(context, random, -0.16f, 0.18f), healAmount, qiAmount);
        }
    }

    public void ApplyFallbackLoot(List<SaveItemStack> loot, string regionId)
    {
        AddLoot(loot, ExpeditionLootFactory.GetRegionalHerbFallback(regionId), 1);
    }

    private static Vector2 SamplePoint(ArenaContextAdapter context, System.Random random, float xBias, float yBias)
    {
        var xMin = context.ArenaMinBounds.x + 1.8f;
        var xMax = context.ArenaMaxBounds.x - 1.2f;
        var yMin = context.ArenaMinBounds.y + 1.2f;
        var yMax = context.ArenaMaxBounds.y - 0.8f;
        var x = Mathf.Lerp(xMin, xMax, Mathf.Clamp01((float)random.NextDouble() + xBias));
        var y = Mathf.Lerp(yMin, yMax, Mathf.Clamp01((float)random.NextDouble() + yBias));
        return new Vector2(x, y);
    }

    private static void AddLoot(List<SaveItemStack> loot, string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return;
        for (var i = 0; i < loot.Count; i++)
        {
            if (loot[i].itemId == itemId) { loot[i].quantity += quantity; return; }
        }
        loot.Add(new SaveItemStack(itemId, quantity));
    }
}

public sealed class ShrineRoomStrategy : IRoomKindStrategy
{
    public ExpeditionRoomKind Kind => ExpeditionRoomKind.Shrine;
    public string DefaultTitle => "残阵祭台";
    public string DefaultDescription => "半毁的祭台仍在微弱运转，也许能借其阵势稳定真元。";
    public string Symbol => "祭";
    public bool IsCombatRoom => false;

    public int DefaultEnemyCount(WorldRegionDefinition region) => 0;

    public void SpawnEventActors(GameObject arenaRoot, ExpeditionRoomState room, WorldRegionDefinition region, System.Random random, GameController controller)
    {
        var context = new ArenaContextAdapter(region, controller);
        var qiAmount = 2 + region.RequiredRealmTier / 2;
        context.SpawnSpiritNode(SamplePoint(context, random, -0.1f, 0.26f), qiAmount);
        context.CreateDecor("ShrineCore", new Vector2(0f, 1.4f), new Vector2(1.2f, 1.6f), new Color(region.AccentColor.r, region.AccentColor.g, region.AccentColor.b, 0.4f), -6);
    }

    public void ApplyFallbackLoot(List<SaveItemStack> loot, string regionId)
    {
        AddLoot(loot, "mind_cleansing_incense", 1);
    }

    private static Vector2 SamplePoint(ArenaContextAdapter context, System.Random random, float xBias, float yBias)
    {
        var xMin = context.ArenaMinBounds.x + 1.8f;
        var xMax = context.ArenaMaxBounds.x - 1.2f;
        var yMin = context.ArenaMinBounds.y + 1.2f;
        var yMax = context.ArenaMaxBounds.y - 0.8f;
        var x = Mathf.Lerp(xMin, xMax, Mathf.Clamp01((float)random.NextDouble() + xBias));
        var y = Mathf.Lerp(yMin, yMax, Mathf.Clamp01((float)random.NextDouble() + yBias));
        return new Vector2(x, y);
    }

    private static void AddLoot(List<SaveItemStack> loot, string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return;
        for (var i = 0; i < loot.Count; i++)
        {
            if (loot[i].itemId == itemId) { loot[i].quantity += quantity; return; }
        }
        loot.Add(new SaveItemStack(itemId, quantity));
    }
}

public sealed class TrapRoomStrategy : IRoomKindStrategy
{
    public ExpeditionRoomKind Kind => ExpeditionRoomKind.Trap;
    public string DefaultTitle => "隐伏险机";
    public string DefaultDescription => "地表纹理异常断裂，脚下很可能埋着旧阵或瘴陷。";
    public string Symbol => "陷";
    public bool IsCombatRoom => false;

    public int DefaultEnemyCount(WorldRegionDefinition region) => 0;

    public void SpawnEventActors(GameObject arenaRoot, ExpeditionRoomState room, WorldRegionDefinition region, System.Random random, GameController controller)
    {
        var context = new ArenaContextAdapter(region, controller);
        context.CreateDecor("TrapShardA", new Vector2(-1.8f, -0.8f), new Vector2(0.45f, 1.35f), new Color(0.58f, 0.22f, 0.18f, 0.76f), -6);
        context.CreateDecor("TrapShardB", new Vector2(1.5f, 0.2f), new Vector2(0.36f, 1f), new Color(0.58f, 0.22f, 0.18f, 0.76f), -6);
    }

    public void ApplyFallbackLoot(List<SaveItemStack> loot, string regionId)
    {
        AddLoot(loot, "array_shard", 1);
    }

    private static void AddLoot(List<SaveItemStack> loot, string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return;
        for (var i = 0; i < loot.Count; i++)
        {
            if (loot[i].itemId == itemId) { loot[i].quantity += quantity; return; }
        }
        loot.Add(new SaveItemStack(itemId, quantity));
    }
}

public sealed class BossRoomStrategy : IRoomKindStrategy
{
    public ExpeditionRoomKind Kind => ExpeditionRoomKind.Boss;
    public string DefaultTitle => "核心险地";
    public string DefaultDescription => "灵压在前方凝成实质，真正的镇守者就潜伏在尽头。";
    public string Symbol => "王";
    public bool IsCombatRoom => true;

    public int DefaultEnemyCount(WorldRegionDefinition region) => 3;

    public void SpawnEventActors(GameObject arenaRoot, ExpeditionRoomState room, WorldRegionDefinition region, System.Random random, GameController controller) { }

    public void ApplyFallbackLoot(List<SaveItemStack> loot, string regionId) { }
}
