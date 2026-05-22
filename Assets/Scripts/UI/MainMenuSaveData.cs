using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class MainMenuSaveData
{
    private static readonly string[] FallbackRealmNames =
    {
        "练气初期",
        "练气中期",
        "练气后期",
        "筑基初期",
        "筑基中期",
        "结丹前夕"
    };

    private const string FallbackStartingRegionId = "green_stone_gate";
    private const string FallbackStartingRegionName = "青石山门";

    public string heroName;
    public string archetypeId;
    public string archetypeName;
    public string origin;
    public string specialty;
    public string description;
    public string realm;
    public string location;
    public string lastPlayed;
    public string sectId;
    public string sectName;
    public bool isSectDisciple;
    public bool isInSectResidence;
    public int worldDay;
    public int worldTimeIndex;
    public int realmTier;
    public int qi;
    public string currentRegionId;
    public string[] unlockedRegionIds;
    public string[] clearedRegionIds;
    public int spiritCrystals;
    public int attackLevel;
    public int vitalityLevel;
    public int mainArtifactLevel;
    public int protectiveRelicLevel;
    public int pillCauldronLevel;
    public int talismanCaseLevel;
    public int bagCapacity;
    public SaveItemStack[] storageItems;
    public string activeTaskId;
    public SaveTaskState[] taskStates;
    public SaveNpcState[] npcStates;
    public SaveFactionState[] factionStates;
    public SaveAfflictionState[] afflictions;
    public string[] storyFlags;
    public string[] storyLog;
    public int settlementBuildCount;
    public string lastSettlementAction;

    public void EnsureDefaults()
    {
        heroName = heroName ?? string.Empty;
        archetypeId = archetypeId ?? string.Empty;
        archetypeName = archetypeName ?? string.Empty;
        origin = origin ?? string.Empty;
        specialty = specialty ?? string.Empty;
        description = description ?? string.Empty;
        activeTaskId = activeTaskId ?? string.Empty;

        if (realmTier < 0)
        {
            realmTier = 0;
        }

        if (qi < 0)
        {
            qi = 0;
        }

        if (spiritCrystals < 0)
        {
            spiritCrystals = 0;
        }

        if (attackLevel < 0)
        {
            attackLevel = 0;
        }

        if (vitalityLevel < 0)
        {
            vitalityLevel = 0;
        }

        if (mainArtifactLevel < 0)
        {
            mainArtifactLevel = 0;
        }

        if (protectiveRelicLevel < 0)
        {
            protectiveRelicLevel = 0;
        }

        if (pillCauldronLevel < 0)
        {
            pillCauldronLevel = 0;
        }

        if (talismanCaseLevel < 0)
        {
            talismanCaseLevel = 0;
        }

        if (mainArtifactLevel == 0 && attackLevel > 0)
        {
            mainArtifactLevel = attackLevel;
        }

        if (protectiveRelicLevel == 0 && vitalityLevel > 0)
        {
            protectiveRelicLevel = vitalityLevel;
        }

        attackLevel = mainArtifactLevel;
        vitalityLevel = protectiveRelicLevel;

        if (bagCapacity <= 0)
        {
            bagCapacity = 12;
        }

        var regionWasMissing = string.IsNullOrWhiteSpace(currentRegionId);
        if (regionWasMissing)
        {
            currentRegionId = FallbackStartingRegionId;
        }

        unlockedRegionIds = NormalizeRegions(unlockedRegionIds);
        if (unlockedRegionIds.Length == 0)
        {
            unlockedRegionIds = new[] { currentRegionId };
        }
        else if (!ContainsRegion(unlockedRegionIds, currentRegionId))
        {
            unlockedRegionIds = AddRegion(unlockedRegionIds, currentRegionId);
        }

        clearedRegionIds = NormalizeRegions(clearedRegionIds);
        storageItems = NormalizeStorageItems(storageItems);

        if (taskStates == null)
        {
            taskStates = Array.Empty<SaveTaskState>();
        }
        else
        {
            for (var i = 0; i < taskStates.Length; i++)
            {
                if (taskStates[i] == null)
                {
                    taskStates[i] = new SaveTaskState();
                }

                taskStates[i].EnsureDefaults();
            }
        }

        if (npcStates == null)
        {
            npcStates = Array.Empty<SaveNpcState>();
        }
        else
        {
            for (var i = 0; i < npcStates.Length; i++)
            {
                if (npcStates[i] == null)
                {
                    npcStates[i] = new SaveNpcState();
                }

                npcStates[i].EnsureDefaults();
            }
        }

        if (factionStates == null)
        {
            factionStates = Array.Empty<SaveFactionState>();
        }
        else
        {
            for (var i = 0; i < factionStates.Length; i++)
            {
                if (factionStates[i] == null)
                {
                    factionStates[i] = new SaveFactionState();
                }

                factionStates[i].EnsureDefaults();
            }
        }

        if (afflictions == null)
        {
            afflictions = Array.Empty<SaveAfflictionState>();
        }

        storyFlags = NormalizeStrings(storyFlags);
        storyLog = NormalizeStrings(storyLog);

        if (settlementBuildCount < 0)
        {
            settlementBuildCount = 0;
        }

        if (lastSettlementAction == null)
        {
            lastSettlementAction = string.Empty;
        }

        if (string.IsNullOrWhiteSpace(sectId))
        {
            sectId = archetypeId == "wanderer" ? "rogue" : "qingxuan_sect";
        }

        if (string.IsNullOrWhiteSpace(sectName))
        {
            sectName = sectId == "rogue" ? "散修" : "青玄山门";
        }

        isSectDisciple = sectId != "rogue";
        if (!isSectDisciple)
        {
            isInSectResidence = false;
        }

        realm = ResolveFallbackRealmName(realmTier);

        if (regionWasMissing || string.IsNullOrWhiteSpace(location))
        {
            location = ResolveFallbackRegionName(currentRegionId);
        }

        if (string.IsNullOrWhiteSpace(lastPlayed))
        {
            lastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        CultivationGameTime.EnsureDefaults(this);
    }

    public bool IsRegionUnlocked(string regionId)
    {
        return ContainsRegion(unlockedRegionIds, regionId);
    }

    public bool IsRegionCleared(string regionId)
    {
        return ContainsRegion(clearedRegionIds, regionId);
    }

    public void UnlockRegion(string regionId)
    {
        unlockedRegionIds = AddRegion(unlockedRegionIds, regionId);
    }

    public void MarkRegionCleared(string regionId)
    {
        clearedRegionIds = AddRegion(clearedRegionIds, regionId);
    }

    public int GetUsedBagSlots()
    {
        var used = 0;
        for (var i = 0; i < storageItems.Length; i++)
        {
            if (storageItems[i] != null && !string.IsNullOrWhiteSpace(storageItems[i].itemId) && storageItems[i].quantity > 0)
            {
                used++;
            }
        }

        return used;
    }

    public int GetItemCount(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return 0;
        }

        for (var i = 0; i < storageItems.Length; i++)
        {
            var stack = storageItems[i];
            if (stack != null && stack.itemId == itemId)
            {
                return Mathf.Max(0, stack.quantity);
            }
        }

        return 0;
    }

    public bool TryAddItem(string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
        {
            return false;
        }

        for (var i = 0; i < storageItems.Length; i++)
        {
            var stack = storageItems[i];
            if (stack != null && stack.itemId == itemId)
            {
                stack.quantity += quantity;
                return true;
            }
        }

        if (GetUsedBagSlots() >= bagCapacity)
        {
            return false;
        }

        var merged = new List<SaveItemStack>(storageItems);
        merged.Add(new SaveItemStack(itemId, quantity));
        storageItems = merged.ToArray();
        return true;
    }

    public int RemoveItem(string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
        {
            return 0;
        }

        for (var i = 0; i < storageItems.Length; i++)
        {
            var stack = storageItems[i];
            if (stack == null || stack.itemId != itemId)
            {
                continue;
            }

            var removed = Mathf.Min(quantity, stack.quantity);
            stack.quantity -= removed;
            if (stack.quantity <= 0)
            {
                var merged = new List<SaveItemStack>(storageItems);
                merged.RemoveAt(i);
                storageItems = merged.ToArray();
            }

            return removed;
        }

        return 0;
    }

    public SaveTaskState GetOrCreateTaskState(string taskId)
    {
        if (string.IsNullOrWhiteSpace(taskId))
        {
            return null;
        }

        EnsureDefaults();
        for (var i = 0; i < taskStates.Length; i++)
        {
            if (taskStates[i] != null && taskStates[i].taskId == taskId)
            {
                taskStates[i].EnsureDefaults();
                return taskStates[i];
            }
        }

        var state = new SaveTaskState(taskId);
        var merged = new List<SaveTaskState>(taskStates) { state };
        taskStates = merged.ToArray();
        return state;
    }

    public SaveTaskState FindTaskState(string taskId)
    {
        if (string.IsNullOrWhiteSpace(taskId))
        {
            return null;
        }

        EnsureDefaults();
        for (var i = 0; i < taskStates.Length; i++)
        {
            if (taskStates[i] != null && taskStates[i].taskId == taskId)
            {
                taskStates[i].EnsureDefaults();
                return taskStates[i];
            }
        }

        return null;
    }

    public SaveNpcState GetOrCreateNpcState(string npcId)
    {
        EnsureDefaults();
        for (var i = 0; i < npcStates.Length; i++)
        {
            var state = npcStates[i];
            if (state != null && state.npcId == npcId)
            {
                return state;
            }
        }

        var created = new SaveNpcState(npcId);
        var merged = new List<SaveNpcState>(npcStates) { created };
        npcStates = merged.ToArray();
        return created;
    }

    public SaveFactionState GetOrCreateFactionState(ExpeditionEnemyFaction faction)
    {
        EnsureDefaults();
        for (var i = 0; i < factionStates.Length; i++)
        {
            if (factionStates[i] != null && factionStates[i].faction == faction)
            {
                factionStates[i].EnsureDefaults();
                return factionStates[i];
            }
        }

        var state = new SaveFactionState(faction);
        var merged = new List<SaveFactionState>(factionStates) { state };
        factionStates = merged.ToArray();
        return state;
    }

    private static bool ContainsRegion(string[] regions, string regionId)
    {
        if (regions == null || string.IsNullOrWhiteSpace(regionId))
        {
            return false;
        }

        for (var i = 0; i < regions.Length; i++)
        {
            if (regions[i] == regionId)
            {
                return true;
            }
        }

        return false;
    }

    private static string[] AddRegion(string[] regions, string regionId)
    {
        if (string.IsNullOrWhiteSpace(regionId))
        {
            return regions ?? Array.Empty<string>();
        }

        var merged = new List<string>(regions ?? Array.Empty<string>());
        if (!merged.Contains(regionId))
        {
            merged.Add(regionId);
        }

        return merged.ToArray();
    }

    private static string[] NormalizeRegions(string[] regions)
    {
        return NormalizeStrings(regions);
    }

    private static string[] NormalizeStrings(string[] values)
    {
        if (values == null || values.Length == 0)
        {
            return Array.Empty<string>();
        }

        var normalized = new List<string>(values.Length);
        for (var i = 0; i < values.Length; i++)
        {
            var value = values[i];
            if (string.IsNullOrWhiteSpace(value) || normalized.Contains(value))
            {
                continue;
            }

            normalized.Add(value);
        }

        return normalized.ToArray();
    }

    private static SaveItemStack[] NormalizeStorageItems(SaveItemStack[] items)
    {
        if (items == null || items.Length == 0)
        {
            return Array.Empty<SaveItemStack>();
        }

        var normalized = new List<SaveItemStack>(items.Length);
        for (var i = 0; i < items.Length; i++)
        {
            var stack = items[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            normalized.Add(new SaveItemStack(stack.itemId, Mathf.Max(0, stack.quantity)));
        }

        return normalized.ToArray();
    }

    private static string ResolveFallbackRealmName(int tier)
    {
        var clampedTier = Mathf.Clamp(tier, 0, FallbackRealmNames.Length - 1);
        return FallbackRealmNames[clampedTier];
    }

    private static string ResolveFallbackRegionName(string regionId)
    {
        return regionId == FallbackStartingRegionId ? FallbackStartingRegionName : "未知地域";
    }
}
