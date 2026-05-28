using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CultivationSaveData
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
    public SpiritCrystalWallet wallet;

    public bool HasLegacyCrystals => !wallet.IsEmpty;
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
    public int worldSeed;
    public GeneratedNpcData[] generatedNpcs;
    public NpcRelationEdgeData[] npcRelations;
    public GeneratedLocationState[] generatedLocations;
    public WorldIncidentData[] activeWorldIncidents;
    public string[] worldDayLogs;
    public int settlementBuildCount;
    public string lastSettlementAction;

    // 玩家综合属性（含社交、声望、宗门贡献等）
    public PlayerAttributes attributes = new();

    // 修仙基础属性（M2 AttributeModel 持久化字段）
    public int rootBone;        // 根骨：影响气血上限与抗性
    public int insight;         // 悟性：影响修炼速度与功法学习
    public int spiritSense;     // 神识：影响命中、暴击与法术抗性
    public int vitalityStat;    // 气血上限基底（区别于战斗运行时血量）
    public int manaStat;        // 法力上限基底
    public int charm;           // 魅力：影响 NPC / 道侣 / 师承
    public int soulPower;       // 魂力：灵魂强度、附魔能力
    public int vitalEnergy;     // 精元：生命力、炼器火候
    public int willpower;       // 意志：心魔抵抗、突破成功率
    public int dexterity;       // 机巧：手工精细度、绘制能力
    public int spiritRoot;      // 灵根：五行亲和、灵气吸收（0=凡体,1=伪灵根,2=真灵根,3=天灵根,4=变异灵根）

    // 境界与突破（M1 RealmModel 持久化字段）
    public bool atBottleneck;   // 当前是否陷入瓶颈
    public int breakthroughCount; // 历次突破成功次数
    public int heartDemonMark;  // 心魔印记层数（>0 时有惩罚）

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

        MigrateLegacyCrystals();

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

        attributes ??= new PlayerAttributes();
        attributes.social ??= new SocialAttributes();

        if (rootBone < 0) rootBone = 0;
        if (insight < 0) insight = 0;
        if (spiritSense < 0) spiritSense = 0;
        if (vitalityStat < 0) vitalityStat = 0;
        if (manaStat < 0) manaStat = 0;
        if (charm < 0) charm = 0;
        if (soulPower < 0) soulPower = 0;
        if (vitalEnergy < 0) vitalEnergy = 0;
        if (willpower < 0) willpower = 0;
        if (dexterity < 0) dexterity = 0;
        if (spiritRoot < 0) spiritRoot = 0;
        if (spiritRoot > 4) spiritRoot = 4;
        if (breakthroughCount < 0) breakthroughCount = 0;
        if (heartDemonMark < 0) heartDemonMark = 0;

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
        worldDayLogs = NormalizeStrings(worldDayLogs);

        if (generatedNpcs == null)
        {
            generatedNpcs = Array.Empty<GeneratedNpcData>();
        }
        else
        {
            for (var i = 0; i < generatedNpcs.Length; i++)
            {
                if (generatedNpcs[i] == null)
                {
                    generatedNpcs[i] = new GeneratedNpcData();
                }

                generatedNpcs[i].EnsureDefaults();
            }
        }

        if (npcRelations == null)
        {
            npcRelations = Array.Empty<NpcRelationEdgeData>();
        }
        else
        {
            for (var i = 0; i < npcRelations.Length; i++)
            {
                if (npcRelations[i] == null)
                {
                    npcRelations[i] = new NpcRelationEdgeData();
                }

                npcRelations[i].EnsureDefaults();
            }
        }

        if (generatedLocations == null)
        {
            generatedLocations = Array.Empty<GeneratedLocationState>();
        }
        else
        {
            for (var i = 0; i < generatedLocations.Length; i++)
            {
                if (generatedLocations[i] == null)
                {
                    generatedLocations[i] = new GeneratedLocationState();
                }

                generatedLocations[i].EnsureDefaults();
            }
        }

        if (activeWorldIncidents == null)
        {
            activeWorldIncidents = Array.Empty<WorldIncidentData>();
        }
        else
        {
            for (var i = 0; i < activeWorldIncidents.Length; i++)
            {
                if (activeWorldIncidents[i] == null)
                {
                    activeWorldIncidents[i] = new WorldIncidentData();
                }

                activeWorldIncidents[i].EnsureDefaults();
            }
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

    private void MigrateLegacyCrystals()
    {
        wallet.Normalize();
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

    public GeneratedNpcData FindGeneratedNpc(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId))
        {
            return null;
        }

        EnsureDefaults();
        for (var i = 0; i < generatedNpcs.Length; i++)
        {
            var npc = generatedNpcs[i];
            if (npc != null && npc.npcId == npcId)
            {
                npc.EnsureDefaults();
                return npc;
            }
        }

        return null;
    }

    public GeneratedLocationState FindGeneratedLocation(string locationId)
    {
        if (string.IsNullOrWhiteSpace(locationId))
        {
            return null;
        }

        EnsureDefaults();
        for (var i = 0; i < generatedLocations.Length; i++)
        {
            var state = generatedLocations[i];
            if (state != null && state.locationId == locationId)
            {
                state.EnsureDefaults();
                return state;
            }
        }

        return null;
    }

    public WorldIncidentData FindWorldIncident(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
        {
            return null;
        }

        EnsureDefaults();
        for (var i = 0; i < activeWorldIncidents.Length; i++)
        {
            var incident = activeWorldIncidents[i];
            if (incident != null && incident.incidentId == incidentId)
            {
                incident.EnsureDefaults();
                return incident;
            }
        }

        return null;
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
