using System.Collections.Generic;
using UnityEngine;
using QFramework;

public sealed class CultivationRewardSystem : AbstractSystem
{
    private CultivationFactionSystem factionSystem;

    protected override void OnInit()
    {
        factionSystem = this.GetSystem<CultivationFactionSystem>();
    }

    public List<SaveItemStack> BuildEncounterLoot(CombatTurnContext context)
    {
        if (context == null || context.Region == null || context.Room == null || context.CurrentEncounterSnapshot == null)
        {
            return new List<SaveItemStack>();
        }

        var loot = ExpeditionLootFactory.BuildEncounterLoot(context.Region, context.Room, context.CurrentEncounterSnapshot);
        ApplyFactionPressureLoot(context.SaveData, context.Region, context.CurrentEncounterSnapshot, loot);
        return loot;
    }

    public List<SaveItemStack> BuildRoomLoot(WorldRegionDefinition region, ExpeditionRoomState room, MainMenuSaveData saveData)
    {
        if (region == null || room == null)
        {
            return new List<SaveItemStack>();
        }

        var loot = ExpeditionLootFactory.BuildRoomLoot(region, room);
        if (saveData != null && room.Kind == ExpeditionRoomKind.Treasure)
        {
            var pressure = GetHighestPressure(saveData);
            if (pressure >= 3)
            {
                MergeLoot(loot, new List<SaveItemStack> { new SaveItemStack(InventoryLibrary.GetRegionalRareItemId(region.Id), 1) });
            }
        }

        return loot;
    }

    public List<SaveItemStack> BuildClearLoot(WorldRegionDefinition region, MainMenuSaveData saveData)
    {
        if (region == null)
        {
            return new List<SaveItemStack>();
        }

        var loot = ExpeditionLootFactory.BuildClearLoot(region);
        if (saveData != null && saveData.IsRegionCleared(region.Id))
        {
            return loot;
        }

        return loot;
    }

    public void AddPendingItem(List<SaveItemStack> pendingItemRewards, string itemId, int quantity)
    {
        if (pendingItemRewards == null || string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
        {
            return;
        }

        MergeLoot(pendingItemRewards, new List<SaveItemStack> { new SaveItemStack(itemId, quantity) });
    }

    public void MergeLoot(List<SaveItemStack> target, List<SaveItemStack> incoming)
    {
        if (target == null || incoming == null || incoming.Count == 0)
        {
            return;
        }

        for (var i = 0; i < incoming.Count; i++)
        {
            var stack = incoming[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            var merged = false;
            for (var existingIndex = 0; existingIndex < target.Count; existingIndex++)
            {
                if (target[existingIndex] != null && target[existingIndex].itemId == stack.itemId)
                {
                    target[existingIndex].quantity += stack.quantity;
                    merged = true;
                    break;
                }
            }

            if (!merged)
            {
                target.Add(new SaveItemStack(stack.itemId, stack.quantity));
            }
        }
    }

    public RewardBankResult BankPendingLoot(MainMenuSaveData saveData, List<SaveItemStack> pendingItemRewards)
    {
        var result = new RewardBankResult();
        if (saveData == null || pendingItemRewards == null || pendingItemRewards.Count == 0)
        {
            return result;
        }

        saveData.EnsureDefaults();
        var banked = new List<SaveItemStack>();
        var overflow = new List<SaveItemStack>();
        for (var i = 0; i < pendingItemRewards.Count; i++)
        {
            var stack = pendingItemRewards[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            if (saveData.TryAddItem(stack.itemId, stack.quantity))
            {
                banked.Add(new SaveItemStack(stack.itemId, stack.quantity));
            }
            else
            {
                overflow.Add(new SaveItemStack(stack.itemId, stack.quantity));
                result.OverflowCrystalGain += InventoryLibrary.GetCrystalValue(stack.itemId) * stack.quantity;
            }
        }

        pendingItemRewards.Clear();
        result.BankedSummary = banked.Count > 0 ? InventoryLibrary.DescribeLoot(banked) : string.Empty;
        result.OverflowSummary = overflow.Count > 0 ? InventoryLibrary.DescribeLoot(overflow) : string.Empty;
        return result;
    }

    private void ApplyFactionPressureLoot(MainMenuSaveData saveData, WorldRegionDefinition region, List<ExpeditionEnemyState> encounter, List<SaveItemStack> loot)
    {
        if (saveData == null || region == null || encounter == null || loot == null)
        {
            return;
        }

        for (var i = 0; i < encounter.Count; i++)
        {
            var enemy = encounter[i];
            if (enemy == null || !enemy.IsElite)
            {
                continue;
            }

            var pressure = factionSystem.GetFactionPressure(saveData, enemy.Faction);
            if (pressure >= 3)
            {
                AddPendingItem(loot, InventoryLibrary.GetRegionalRareItemId(region.Id), 1);
            }
        }
    }

    private int GetHighestPressure(MainMenuSaveData saveData)
    {
        var highest = 0;
        for (var i = 0; i < 5; i++)
        {
            highest = Mathf.Max(highest, factionSystem.GetFactionPressure(saveData, (ExpeditionEnemyFaction)i));
        }

        return highest;
    }
}
