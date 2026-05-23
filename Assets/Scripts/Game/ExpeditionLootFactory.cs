using System.Collections.Generic;
using UnityEngine;

public static class ExpeditionLootFactory
{
    private static LootTableAsset cachedTable;

    public static List<SaveItemStack> BuildEncounterLoot(WorldRegionDefinition region, ExpeditionRoomState room, List<ExpeditionEnemyState> encounter)
    {
        var loot = new List<SaveItemStack>();
        var table = GetTable();
        for (var i = 0; i < encounter.Count; i++)
        {
            var enemy = encounter[i];
            if (enemy == null)
            {
                continue;
            }

            if (TryApplyFactionLoot(table, loot, enemy))
            {
                continue;
            }

            ApplyFallbackFactionLoot(loot, enemy);
        }

        if (room.Kind == ExpeditionRoomKind.Boss)
        {
            AddLoot(loot, GetRegionalRareItemId(table, region.Id), 1);
        }

        return loot;
    }

    public static List<SaveItemStack> BuildRoomLoot(WorldRegionDefinition region, ExpeditionRoomState room)
    {
        var loot = new List<SaveItemStack>();
        var table = GetTable();
        if (!TryApplyRoomLoot(table, loot, room.Kind))
        {
            ApplyFallbackRoomLoot(loot, region.Id, room.Kind);
            return loot;
        }

        if (room.Kind == ExpeditionRoomKind.Treasure)
        {
            AddLoot(loot, GetRegionalRareItemId(table, region.Id), 1);
        }
        else if (room.Kind == ExpeditionRoomKind.Herb)
        {
            AddLoot(loot, GetRegionalHerb(table, region.Id), 1);
        }

        return loot;
    }

    public static List<SaveItemStack> BuildClearLoot(WorldRegionDefinition region)
    {
        var loot = new List<SaveItemStack>();
        var table = GetTable();
        AddLoot(loot, GetRegionalRareItemId(table, region.Id), 1);
        if (!TryApplyClearLoot(table, loot, region.Id))
        {
            ApplyFallbackClearLoot(loot, region.Id);
        }

        return loot;
    }

    private static LootTableAsset GetTable()
    {
        if (cachedTable == null)
        {
            cachedTable = GameResource.Load<LootTableAsset>("Data/LootTable");
        }

        return cachedTable;
    }

    private static bool TryApplyFactionLoot(LootTableAsset table, List<SaveItemStack> loot, ExpeditionEnemyState enemy)
    {
        if (table == null || table.factionLoots == null)
        {
            return false;
        }

        for (var i = 0; i < table.factionLoots.Length; i++)
        {
            var record = table.factionLoots[i];
            if (record == null || record.faction != (int)enemy.Faction)
            {
                continue;
            }

            AddDrops(loot, record.drops, enemy.IsElite);
            return true;
        }

        return false;
    }

    private static bool TryApplyRoomLoot(LootTableAsset table, List<SaveItemStack> loot, ExpeditionRoomKind roomKind)
    {
        if (table == null || table.roomLoots == null)
        {
            return false;
        }

        for (var i = 0; i < table.roomLoots.Length; i++)
        {
            var record = table.roomLoots[i];
            if (record == null || record.roomKind != (int)roomKind)
            {
                continue;
            }

            AddDrops(loot, record.drops, false);
            return true;
        }

        return false;
    }

    private static bool TryApplyClearLoot(LootTableAsset table, List<SaveItemStack> loot, string regionId)
    {
        var record = FindRegionRecord(table, regionId);
        if (record == null || record.clearDrops == null)
        {
            return false;
        }

        AddDrops(loot, record.clearDrops, false);
        return true;
    }

    private static RegionLootRecord FindRegionRecord(LootTableAsset table, string regionId)
    {
        if (table == null || table.regionLoots == null || string.IsNullOrWhiteSpace(regionId))
        {
            return null;
        }

        for (var i = 0; i < table.regionLoots.Length; i++)
        {
            var record = table.regionLoots[i];
            if (record != null && record.regionId == regionId)
            {
                return record;
            }
        }

        return null;
    }

    private static string GetRegionalRareItemId(LootTableAsset table, string regionId)
    {
        var record = FindRegionRecord(table, regionId);
        return record != null && !string.IsNullOrWhiteSpace(record.rareItemId)
            ? record.rareItemId
            : InventoryLibrary.GetRegionalRareItemId(regionId);
    }

    private static string GetRegionalHerb(LootTableAsset table, string regionId)
    {
        var record = FindRegionRecord(table, regionId);
        if (record != null && !string.IsNullOrWhiteSpace(record.herbItemId))
        {
            return record.herbItemId;
        }

        return GetRegionalHerbFallback(regionId);
    }

    private static void AddDrops(List<SaveItemStack> loot, LootDropRecord[] drops, bool useElite)
    {
        if (drops == null)
        {
            return;
        }

        for (var i = 0; i < drops.Length; i++)
        {
            if (drops[i] == null)
            {
                continue;
            }

            AddLoot(loot, drops[i].itemId, useElite ? drops[i].eliteQuantity : drops[i].normalQuantity);
        }
    }

    private static void ApplyFallbackFactionLoot(List<SaveItemStack> loot, ExpeditionEnemyState enemy)
    {
        var strategy = FactionStrategyRegistry.Get(enemy.Faction);
        if (strategy != null)
        {
            strategy.ApplyFallbackLoot(loot, enemy.IsElite);
        }
    }

    private static void ApplyFallbackRoomLoot(List<SaveItemStack> loot, string regionId, ExpeditionRoomKind roomKind)
    {
        var strategy = RoomKindStrategyRegistry.Get(roomKind);
        if (strategy != null)
        {
            strategy.ApplyFallbackLoot(loot, regionId);
        }
    }

    private static void ApplyFallbackClearLoot(List<SaveItemStack> loot, string regionId)
    {
        if (regionId == "celestial_ruins")
        {
            AddLoot(loot, "void_script", 1);
        }
        else if (regionId == "northern_pass")
        {
            AddLoot(loot, "ancient_pass_order", 1);
        }
    }

    private static void AddLoot(List<SaveItemStack> loot, string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
        {
            return;
        }

        for (var i = 0; i < loot.Count; i++)
        {
            if (loot[i].itemId == itemId)
            {
                loot[i].quantity += quantity;
                return;
            }
        }

        loot.Add(new SaveItemStack(itemId, quantity));
    }

    public static string GetRegionalHerbFallback(string regionId)
    {
        switch (regionId)
        {
            case "misty_forest":
                return "mist_mushroom";
            case "crimson_valley":
                return "flame_jujube";
            case "deep_springs":
                return "cold_marrow_algae";
            case "northern_pass":
                return "north_iron";
            case "celestial_ruins":
                return "starfall_crystal";
            default:
                return "green_spirit_sand";
        }
    }
}
