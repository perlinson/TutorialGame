using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class MainMenuSaveData
{
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
    public SaveFactionState[] factionStates;
    public SaveAfflictionState[] afflictions;
    public string[] storyFlags;
    public string[] storyLog;
    public int settlementBuildCount;
    public string lastSettlementAction;

    public void EnsureDefaults()
    {
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

        if (string.IsNullOrWhiteSpace(currentRegionId))
        {
            currentRegionId = WorldRegionLibrary.StartingRegionId;
        }

        if (unlockedRegionIds == null || unlockedRegionIds.Length == 0)
        {
            unlockedRegionIds = new[] { currentRegionId };
        }
        else if (!ContainsRegion(unlockedRegionIds, currentRegionId))
        {
            unlockedRegionIds = AddRegion(unlockedRegionIds, currentRegionId);
        }

        if (clearedRegionIds == null)
        {
            clearedRegionIds = Array.Empty<string>();
        }

        if (storageItems == null)
        {
            storageItems = Array.Empty<SaveItemStack>();
        }

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

        if (storyFlags == null)
        {
            storyFlags = Array.Empty<string>();
        }

        if (storyLog == null)
        {
            storyLog = Array.Empty<string>();
        }

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

        realm = WorldRegionLibrary.GetRealmName(realmTier);

        if (string.IsNullOrWhiteSpace(location))
        {
            location = WorldRegionLibrary.GetRegionDisplayName(currentRegionId);
        }

        if (string.IsNullOrWhiteSpace(lastPlayed))
        {
            lastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }
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
}
