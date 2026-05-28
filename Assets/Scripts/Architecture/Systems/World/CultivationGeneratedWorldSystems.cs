using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using QFramework;
using UnityEngine;

internal sealed class NpcArchetypeTemplate
{
    public NpcRoleType RoleType;
    public NpcSceneType SceneType;
    public string Title;
    public string FactionId;
    public string FactionName;
    public string[] PersonalityPool;
    public string[] FortunePool;
    public string[] SocialStyles;
    public string[] GrowthStyles;
    public string[] ConversationTemplates;
    public string[] PreferredAnchors;
}

internal sealed class NamePoolTemplate
{
    public string[] Surnames;
    public string[] GivenNames;
    public string[] StyleNames;
}

internal sealed class DynamicLocationTemplate
{
    public string Id;
    public string ParentLocationId;
    public string DisplayName;
    public string Subtitle;
    public string Description;
    public NpcSceneType SceneType;
    public bool DefaultVisible;
    public bool IsTemporary;
    public string SourceTaskId;
    public string SourceStoryFlagId;
}

internal sealed class WorldEventTemplate
{
    public string Id;
    public string DisplayTitle;
    public string Description;
    public NpcSceneType SceneType;
    public NpcRoleType RequiredRoleType;
    public string RequiredTaskId;
    public string PreferredLocationId;
    public string ConversationTitle;
}

public sealed class CultivationWorldGenerationSystem : AbstractSystem
{
    private static readonly NamePoolTemplate NamePool = BuildNamePool();
    private static readonly NpcArchetypeTemplate[] NpcArchetypes = BuildNpcArchetypes();
    private static readonly DynamicLocationTemplate[] DynamicLocationTemplates = BuildDynamicLocations();
    private static readonly string[] SectHallIds =
    {
        "duty_hall",
        "refining_hall",
        "alchemy_hall",
        "talisman_hall",
        "scripture_hall",
        "steward_hall",
        "cave_residence"
    };

    private CultivationTaskSystem taskSystem;

    protected override void OnInit()
    {
        taskSystem = this.GetSystem<CultivationTaskSystem>();
    }

    public void EnsureWorldGenerated(CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.EnsureDefaults();
        if (saveData.worldSeed <= 0)
        {
            saveData.worldSeed = BuildFallbackSeed(saveData);
        }

        var hasGeneratedNpcs = saveData.generatedNpcs != null && saveData.generatedNpcs.Length > 0;
        var hasGeneratedLocations = saveData.generatedLocations != null && saveData.generatedLocations.Length > 0;
        var hasRelations = saveData.npcRelations != null && saveData.npcRelations.Length > 0;
        if (!hasGeneratedNpcs || !hasGeneratedLocations || !hasRelations)
        {
            GenerateNewWorld(saveData, saveData.worldSeed);
            return;
        }

        RefreshLocationResidents(saveData);
    }

    public void GenerateNewWorld(CultivationSaveData saveData, int seed)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.EnsureDefaults();
        saveData.worldSeed = seed <= 0 ? BuildFallbackSeed(saveData) : seed;

        var random = new System.Random(saveData.worldSeed);
        saveData.generatedLocations = BuildGeneratedLocations(saveData);
        saveData.generatedNpcs = BuildGeneratedNpcs(saveData, random);
        saveData.npcRelations = BuildRelations(saveData, random);
        saveData.activeWorldIncidents = Array.Empty<WorldIncidentData>();
        saveData.worldDayLogs = AppendUnique(saveData.worldDayLogs, "太初历第" + saveData.worldDay + "日：此界修士谱系已重新织定。");
        RefreshLocationResidents(saveData);

        var incidentSystem = this.GetSystem<CultivationWorldIncidentSystem>();
        incidentSystem?.RebuildIncidents(saveData);
    }

    public GeneratedLocationState[] GetVisibleLocations(CultivationSaveData saveData, string parentLocationId)
    {
        return GetVisibleLocations(saveData, parentLocationId, null);
    }

    public GeneratedLocationState[] GetVisibleLocations(CultivationSaveData saveData, string parentLocationId, NpcSceneType? sceneType)
    {
        EnsureWorldGenerated(saveData);
        if (saveData == null || string.IsNullOrWhiteSpace(parentLocationId))
        {
            return Array.Empty<GeneratedLocationState>();
        }

        var taskContext = taskSystem != null ? taskSystem.GetActiveTaskContext(saveData) : null;
        var list = new List<GeneratedLocationState>();
        for (var i = 0; i < saveData.generatedLocations.Length; i++)
        {
            var state = saveData.generatedLocations[i];
            if (state == null)
            {
                continue;
            }

            state.EnsureDefaults();
            if (state.parentLocationId != parentLocationId)
            {
                continue;
            }

            if (sceneType.HasValue && state.sceneType != sceneType.Value)
            {
                continue;
            }

            if (!IsLocationVisible(saveData, state, taskContext))
            {
                continue;
            }

            list.Add(state);
        }

        return list.ToArray();
    }

    public GeneratedNpcData[] GetNpcsAtLocation(CultivationSaveData saveData, string locationId)
    {
        EnsureWorldGenerated(saveData);
        if (saveData == null || string.IsNullOrWhiteSpace(locationId))
        {
            return Array.Empty<GeneratedNpcData>();
        }

        var list = new List<GeneratedNpcData>();
        for (var i = 0; i < saveData.generatedNpcs.Length; i++)
        {
            var npc = saveData.generatedNpcs[i];
            if (npc == null)
            {
                continue;
            }

            npc.EnsureDefaults();
            if (!npc.isAlive || npc.currentLocationId != locationId)
            {
                continue;
            }

            list.Add(npc);
        }

        return list.ToArray();
    }

    public GeneratedNpcData[] GetNpcsForScene(CultivationSaveData saveData, NpcSceneType sceneType, string anchorId)
    {
        EnsureWorldGenerated(saveData);
        if (saveData == null || string.IsNullOrWhiteSpace(anchorId))
        {
            return Array.Empty<GeneratedNpcData>();
        }

        var list = new List<GeneratedNpcData>();
        for (var i = 0; i < saveData.generatedNpcs.Length; i++)
        {
            var npc = saveData.generatedNpcs[i];
            if (npc == null)
            {
                continue;
            }

            npc.EnsureDefaults();
            if (!npc.isAlive || npc.sceneType != sceneType)
            {
                continue;
            }

            var resolvedAnchor = ResolveSceneAnchor(saveData, npc.currentLocationId);
            if (resolvedAnchor != anchorId)
            {
                continue;
            }

            list.Add(npc);
        }

        return list.ToArray();
    }

    public GeneratedLocationState ResolveLocationState(CultivationSaveData saveData, string locationId)
    {
        EnsureWorldGenerated(saveData);
        return saveData != null ? saveData.FindGeneratedLocation(locationId) : null;
    }

    public string ResolveLocationName(CultivationSaveData saveData, string locationId)
    {
        if (string.IsNullOrWhiteSpace(locationId))
        {
            return string.Empty;
        }

        var state = ResolveLocationState(saveData, locationId);
        if (state != null && !string.IsNullOrWhiteSpace(state.displayName))
        {
            return state.displayName;
        }

        SectHallDefinition hallDefinition;
        var sectSystem = this.GetSystem<CultivationSectSystem>();
        if (sectSystem != null && sectSystem.TryGetHallDefinition(locationId, out hallDefinition) && hallDefinition != null)
        {
            return hallDefinition.DisplayName;
        }

        return WorldRegionLibrary.GetRegionDisplayName(locationId);
    }

    public string BuildLocationDigest(CultivationSaveData saveData, string parentLocationId, NpcSceneType sceneType)
    {
        var locations = GetVisibleLocations(saveData, parentLocationId, sceneType);
        if (locations.Length == 0)
        {
            return string.Empty;
        }

        var builder = new System.Text.StringBuilder();
        builder.Append("可接触支点：");
        for (var i = 0; i < locations.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(" / ");
            }

            builder.Append(locations[i].displayName);
            if (!string.IsNullOrWhiteSpace(locations[i].subtitle))
            {
                builder.Append("（").Append(locations[i].subtitle).Append("）");
            }
        }

        return builder.ToString();
    }

    public string ResolveSceneAnchor(CultivationSaveData saveData, string locationId)
    {
        if (string.IsNullOrWhiteSpace(locationId))
        {
            return string.Empty;
        }

        var locationState = saveData != null ? saveData.FindGeneratedLocation(locationId) : null;
        if (locationState != null && !string.IsNullOrWhiteSpace(locationState.parentLocationId))
        {
            return locationState.parentLocationId;
        }

        return locationId;
    }

    public void RefreshLocationResidents(CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.EnsureDefaults();
        for (var i = 0; i < saveData.generatedLocations.Length; i++)
        {
            var location = saveData.generatedLocations[i];
            if (location == null)
            {
                continue;
            }

            var residents = new List<string>();
            for (var j = 0; j < saveData.generatedNpcs.Length; j++)
            {
                var npc = saveData.generatedNpcs[j];
                if (npc == null)
                {
                    continue;
                }

                npc.EnsureDefaults();
                if (npc.isAlive && npc.currentLocationId == location.locationId)
                {
                    residents.Add(npc.npcId);
                }
            }

            location.residentNpcIds = residents.ToArray();
        }
    }

    private GeneratedLocationState[] BuildGeneratedLocations(CultivationSaveData saveData)
    {
        var list = new List<GeneratedLocationState>(DynamicLocationTemplates.Length);
        for (var i = 0; i < DynamicLocationTemplates.Length; i++)
        {
            var template = DynamicLocationTemplates[i];
            list.Add(new GeneratedLocationState
            {
                locationId = template.Id,
                parentLocationId = template.ParentLocationId,
                displayName = template.DisplayName,
                subtitle = template.Subtitle,
                description = template.Description,
                isUnlocked = template.DefaultVisible,
                isTemporary = template.IsTemporary,
                sourceTaskId = template.SourceTaskId,
                sourceStoryFlagId = template.SourceStoryFlagId,
                residentNpcIds = Array.Empty<string>(),
                sceneType = template.SceneType
            });
        }

        return list.ToArray();
    }

    private GeneratedNpcData[] BuildGeneratedNpcs(CultivationSaveData saveData, System.Random random)
    {
        var list = new List<GeneratedNpcData>();
        var targetCounts = new Dictionary<NpcRoleType, int>
        {
            { NpcRoleType.Merchant, 3 },
            { NpcRoleType.Healer, 2 },
            { NpcRoleType.Scout, 3 },
            { NpcRoleType.Hermit, 2 },
            { NpcRoleType.Mentor, saveData.isSectDisciple ? 2 : 1 },
            { NpcRoleType.Rival, saveData.isSectDisciple ? 2 : 1 },
            { NpcRoleType.Steward, saveData.isSectDisciple ? 1 : 0 }
        };

        foreach (var pair in targetCounts)
        {
            if (pair.Value <= 0)
            {
                continue;
            }

            var template = FindArchetype(pair.Key);
            for (var i = 0; i < pair.Value; i++)
            {
                list.Add(BuildGeneratedNpc(saveData, random, template, i, list.Count));
            }
        }

        return list.ToArray();
    }

    private GeneratedNpcData BuildGeneratedNpc(CultivationSaveData saveData, System.Random random, NpcArchetypeTemplate template, int localIndex, int globalIndex)
    {
        var surname = Pick(random, NamePool.Surnames);
        var givenName = Pick(random, NamePool.GivenNames);
        var styleName = Pick(random, NamePool.StyleNames);
        var currentLocationId = PickSpawnLocation(saveData, random, template);
        var currentAnchor = ResolveSceneAnchor(saveData, currentLocationId);
        var factionName = string.IsNullOrWhiteSpace(template.FactionName)
            ? ResolveLocationName(saveData, currentAnchor)
            : template.FactionName;

        return new GeneratedNpcData
        {
            npcId = "generated_" + template.RoleType.ToString().ToLowerInvariant() + "_" + globalIndex,
            displayName = surname + givenName,
            title = template.Title + "·" + styleName,
            gender = random.NextDouble() >= 0.5 ? GeneratedNpcGender.Male : GeneratedNpcGender.Female,
            ageBand = (GeneratedNpcAgeBand)random.Next(0, 3),
            realmTier = Mathf.Clamp(saveData.realmTier + random.Next(-1, 2), 0, 5),
            spiritRootGrade = random.Next(0, 5),
            personalityTags = PickDistinct(random, template.PersonalityPool, 2),
            fortuneTags = PickDistinct(random, template.FortunePool, 2),
            homeRegionId = currentAnchor,
            currentLocationId = currentLocationId,
            factionId = string.IsNullOrWhiteSpace(template.FactionId) ? "wild" : template.FactionId,
            factionName = factionName,
            socialStyle = Pick(random, template.SocialStyles),
            growthStyle = Pick(random, template.GrowthStyles),
            isAlive = true,
            roleType = template.RoleType,
            sceneType = template.SceneType,
            conversationTemplateTitle = Pick(random, template.ConversationTemplates)
        };
    }

    private NpcRelationEdgeData[] BuildRelations(CultivationSaveData saveData, System.Random random)
    {
        if (saveData.generatedNpcs == null || saveData.generatedNpcs.Length == 0)
        {
            return Array.Empty<NpcRelationEdgeData>();
        }

        var list = new List<NpcRelationEdgeData>();
        for (var i = 0; i < saveData.generatedNpcs.Length; i++)
        {
            var source = saveData.generatedNpcs[i];
            if (source == null)
            {
                continue;
            }

            var target = saveData.generatedNpcs[random.Next(0, saveData.generatedNpcs.Length)];
            if (target == null || target.npcId == source.npcId)
            {
                continue;
            }

            var relationshipType = (GeneratedRelationshipType)random.Next(0, 8);
            list.Add(new NpcRelationEdgeData
            {
                sourceNpcId = source.npcId,
                targetNpcId = target.npcId,
                relationshipType = relationshipType,
                affinity = random.Next(-1, 7),
                hostility = random.Next(0, 5),
                recentIncidentId = string.Empty
            });
        }

        return list.ToArray();
    }

    private bool IsLocationVisible(CultivationSaveData saveData, GeneratedLocationState state, TaskContextSnapshot taskContext)
    {
        if (state == null)
        {
            return false;
        }

        if (state.isUnlocked)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(state.sourceStoryFlagId) && Contains(saveData.storyFlags, state.sourceStoryFlagId))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(state.sourceTaskId) && saveData.activeTaskId == state.sourceTaskId)
        {
            return true;
        }

        return taskContext != null && Contains(taskContext.InjectLocationIds, state.locationId);
    }

    private string PickSpawnLocation(CultivationSaveData saveData, System.Random random, NpcArchetypeTemplate template)
    {
        var preferred = template.PreferredAnchors;
        if (preferred != null && preferred.Length > 0)
        {
            var index = random.Next(0, preferred.Length);
            return preferred[index];
        }

        switch (template.SceneType)
        {
            case NpcSceneType.SectResidence:
                return SectHallIds[random.Next(0, SectHallIds.Length)];
            case NpcSceneType.Region:
            {
                var regions = WorldRegionLibrary.GetRegions();
                return regions[random.Next(0, regions.Count)].Id;
            }
            default:
            {
                var visible = new List<GeneratedLocationState>();
                for (var i = 0; i < saveData.generatedLocations.Length; i++)
                {
                    var location = saveData.generatedLocations[i];
                    if (location != null && location.sceneType == NpcSceneType.Settlement && location.isUnlocked)
                    {
                        visible.Add(location);
                    }
                }

                if (visible.Count > 0 && random.NextDouble() > 0.35d)
                {
                    return visible[random.Next(0, visible.Count)].locationId;
                }

                var regions = WorldRegionLibrary.GetRegions();
                return regions[random.Next(0, regions.Count)].Id;
            }
        }
    }

    private static int BuildFallbackSeed(CultivationSaveData saveData)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + (saveData.heroName ?? string.Empty).GetHashCode();
            hash = hash * 31 + (saveData.archetypeId ?? string.Empty).GetHashCode();
            hash = hash * 31 + (saveData.currentRegionId ?? string.Empty).GetHashCode();
            hash = hash * 31 + DateTime.UtcNow.Day.GetHashCode();
            return Mathf.Abs(hash);
        }
    }

    private static bool Contains(string[] values, string target)
    {
        if (values == null || string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (values[i] == target)
            {
                return true;
            }
        }

        return false;
    }

    private static string[] PickDistinct(System.Random random, string[] source, int count)
    {
        if (source == null || source.Length == 0 || count <= 0)
        {
            return Array.Empty<string>();
        }

        var pool = new List<string>(source);
        var picked = new List<string>(Mathf.Min(count, source.Length));
        while (pool.Count > 0 && picked.Count < count)
        {
            var index = random.Next(0, pool.Count);
            picked.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return picked.ToArray();
    }

    private static string Pick(System.Random random, string[] values)
    {
        if (values == null || values.Length == 0)
        {
            return string.Empty;
        }

        return values[random.Next(0, values.Length)];
    }

    private static string[] AppendUnique(string[] values, string entry)
    {
        var list = new List<string>(values ?? Array.Empty<string>());
        if (!string.IsNullOrWhiteSpace(entry) && !list.Contains(entry))
        {
            list.Add(entry);
        }

        return list.ToArray();
    }

    private static NpcArchetypeTemplate FindArchetype(NpcRoleType roleType)
    {
        for (var i = 0; i < NpcArchetypes.Length; i++)
        {
            if (NpcArchetypes[i].RoleType == roleType)
            {
                return NpcArchetypes[i];
            }
        }

        return NpcArchetypes[0];
    }

    private static NamePoolTemplate BuildNamePool()
    {
        return new NamePoolTemplate
        {
            Surnames = new[] { "顾", "沈", "林", "苏", "裴", "叶", "周", "温", "柳", "祁", "闻", "谢" },
            GivenNames = new[] { "长风", "知微", "清玄", "惊鸿", "晏宁", "星阑", "行舟", "照影", "听潮", "怀玉", "灵素", "归岚" },
            StyleNames = new[] { "守拙", "观澜", "抱朴", "问山", "听雪", "临渊", "忘机", "归云" }
        };
    }

    private static NpcArchetypeTemplate[] BuildNpcArchetypes()
    {
        return new[]
        {
            new NpcArchetypeTemplate
            {
                RoleType = NpcRoleType.Merchant,
                SceneType = NpcSceneType.Settlement,
                Title = "行商",
                FactionId = "market_union",
                FactionName = "四方行会",
                PersonalityPool = new[] { "消息灵通", "谨慎", "爱周转", "肯让利", "嘴甜", "眼毒" },
                FortunePool = new[] { "带财", "逢险有路", "路熟", "识货" },
                SocialStyles = new[] { "圆滑", "爽利", "善谈" },
                GrowthStyles = new[] { "扩商路", "囤奇货", "结善缘" },
                ConversationTemplates = Array.Empty<string>()
            },
            new NpcArchetypeTemplate
            {
                RoleType = NpcRoleType.Healer,
                SceneType = NpcSceneType.Settlement,
                Title = "药师",
                FactionId = "herbal_lodge",
                FactionName = "药炉旧脉",
                PersonalityPool = new[] { "采药", "心细", "耐心", "医理纯熟", "善观气色" },
                FortunePool = new[] { "药缘", "心灯稳", "草木亲和", "避毒" },
                SocialStyles = new[] { "轻声", "稳重", "温和" },
                GrowthStyles = new[] { "修药理", "炼新方", "收弟子" },
                ConversationTemplates = Array.Empty<string>()
            },
            new NpcArchetypeTemplate
            {
                RoleType = NpcRoleType.Scout,
                SceneType = NpcSceneType.Region,
                Title = "斥候",
                FactionId = "frontier_watch",
                FactionName = "前路巡探",
                PersonalityPool = new[] { "巡哨", "识地势", "警觉", "沉默", "熟山径" },
                FortunePool = new[] { "夜眼", "避凶", "脚程快", "藏身稳" },
                SocialStyles = new[] { "寡言", "直接", "谨慎" },
                GrowthStyles = new[] { "记地脉", "追踪异动", "护路引" },
                ConversationTemplates = Array.Empty<string>()
            },
            new NpcArchetypeTemplate
            {
                RoleType = NpcRoleType.Hermit,
                SceneType = NpcSceneType.Region,
                Title = "隐修",
                FactionId = "wild_hermit",
                FactionName = "山泽散脉",
                PersonalityPool = new[] { "独行", "阵理", "观天象", "偏执", "见闻广" },
                FortunePool = new[] { "洞察", "静心", "机缘深", "借势" },
                SocialStyles = new[] { "冷淡", "克制", "偶尔热心" },
                GrowthStyles = new[] { "守洞府", "悟阵纹", "访旧迹" },
                ConversationTemplates = Array.Empty<string>()
            },
            new NpcArchetypeTemplate
            {
                RoleType = NpcRoleType.Mentor,
                SceneType = NpcSceneType.SectResidence,
                Title = "前辈",
                FactionId = "qingxuan_sect",
                FactionName = "青玄山门",
                PersonalityPool = new[] { "讲规矩", "善点拨", "护短", "阅历深", "看人准" },
                FortunePool = new[] { "授法", "清心", "逢关有悟", "长线布局" },
                SocialStyles = new[] { "淡然", "沉稳", "不苟言笑" },
                GrowthStyles = new[] { "育弟子", "守殿堂", "收集古卷" },
                ConversationTemplates = new[] { "清岚长老" }
            },
            new NpcArchetypeTemplate
            {
                RoleType = NpcRoleType.Rival,
                SceneType = NpcSceneType.SectResidence,
                Title = "同门",
                FactionId = "qingxuan_sect",
                FactionName = "青玄山门",
                PersonalityPool = new[] { "好胜", "嘴硬", "重情义", "爱切磋", "不服输" },
                FortunePool = new[] { "火盛", "战意高", "运锋", "直觉强" },
                SocialStyles = new[] { "冲劲足", "坦直", "爱逞强" },
                GrowthStyles = new[] { "练杀招", "争名次", "冲境界" },
                ConversationTemplates = Array.Empty<string>()
            },
            new NpcArchetypeTemplate
            {
                RoleType = NpcRoleType.Steward,
                SceneType = NpcSceneType.SectResidence,
                Title = "执事",
                FactionId = "qingxuan_sect",
                FactionName = "青玄山门",
                PersonalityPool = new[] { "案牍清楚", "算账快", "知轻重", "通人情", "讲章法" },
                FortunePool = new[] { "稳局", "财运平", "人脉广", "记性好" },
                SocialStyles = new[] { "周全", "简练", "老成" },
                GrowthStyles = new[] { "管宗务", "养线人", "扩名册" },
                ConversationTemplates = Array.Empty<string>()
            }
        };
    }

    private static DynamicLocationTemplate[] BuildDynamicLocations()
    {
        return new[]
        {
            new DynamicLocationTemplate
            {
                Id = "green_stone_gate_black_market",
                ParentLocationId = "green_stone_gate",
                DisplayName = "山门暗市",
                Subtitle = "夜半开张",
                Description = "平日藏在杂铺后的小巷暗市，只有任务逼近时才会重新热闹起来。",
                SceneType = NpcSceneType.Settlement,
                DefaultVisible = false,
                IsTemporary = true,
                SourceTaskId = "task_bandit_route"
            },
            new DynamicLocationTemplate
            {
                Id = "misty_forest_herb_camp",
                ParentLocationId = "misty_forest",
                DisplayName = "雾泽采药营",
                Subtitle = "药篓与篝火",
                Description = "采药人与巡路修士暂歇的木棚营地，常在急缺灵草时重新聚人。",
                SceneType = NpcSceneType.Region,
                DefaultVisible = false,
                IsTemporary = true,
                SourceTaskId = "task_mist_herbs"
            },
            new DynamicLocationTemplate
            {
                Id = "crimson_valley_watch_post",
                ParentLocationId = "crimson_valley",
                DisplayName = "谷口哨台",
                Subtitle = "余烬未熄",
                Description = "谷外巡山修士搭建的哨台，会在邪修异动加剧时再次集结。",
                SceneType = NpcSceneType.Region,
                DefaultVisible = false,
                IsTemporary = true,
                SourceTaskId = "task_valley_cultists"
            },
            new DynamicLocationTemplate
            {
                Id = "deep_springs_array_rest",
                ParentLocationId = "deep_springs",
                DisplayName = "古阵歇脚台",
                Subtitle = "泉鸣回响",
                Description = "洞天旧阵旁留下的临时歇脚台，专供收集阵片的人短暂停驻。",
                SceneType = NpcSceneType.Region,
                DefaultVisible = false,
                IsTemporary = true,
                SourceTaskId = "task_springs_array"
            },
            new DynamicLocationTemplate
            {
                Id = "cave_residence_guest_court",
                ParentLocationId = "cave_residence",
                DisplayName = "会客别院",
                Subtitle = "宗门来客",
                Description = "山门内招待同道与外客的偏院，常驻些会说话又会听风向的人。",
                SceneType = NpcSceneType.SectResidence,
                DefaultVisible = true,
                IsTemporary = false
            },
            new DynamicLocationTemplate
            {
                Id = "scripture_hall_annex",
                ParentLocationId = "scripture_hall",
                DisplayName = "经阁偏室",
                Subtitle = "旧卷与灯火",
                Description = "藏经阁旁的偏室，堆着不常示人的副卷和往年批注。",
                SceneType = NpcSceneType.SectResidence,
                DefaultVisible = true,
                IsTemporary = false
            }
        };
    }
}

public sealed class CultivationWorldSimulationSystem : AbstractSystem
{
    private CultivationWorldGenerationSystem generationSystem;
    private CultivationWorldIncidentSystem incidentSystem;

    protected override void OnInit()
    {
        generationSystem = this.GetSystem<CultivationWorldGenerationSystem>();
        incidentSystem = this.GetSystem<CultivationWorldIncidentSystem>();
    }

    private void EnsureDependencies()
    {
        generationSystem ??= this.GetSystem<CultivationWorldGenerationSystem>();
        incidentSystem ??= this.GetSystem<CultivationWorldIncidentSystem>();
    }

    public void AdvanceWorldDay(CultivationSaveData saveData, int days)
    {
        if (saveData == null || days <= 0)
        {
            return;
        }

        EnsureDependencies();
        generationSystem.EnsureWorldGenerated(saveData);
        var startDay = saveData.worldDay - days + 1;
        for (var i = 0; i < days; i++)
        {
            AdvanceSingleDay(saveData, startDay + i);
        }

        generationSystem.RefreshLocationResidents(saveData);
        incidentSystem?.RebuildIncidents(saveData);
    }

    private void AdvanceSingleDay(CultivationSaveData saveData, int simulationDay)
    {
        var random = new System.Random(saveData.worldSeed ^ (simulationDay * 397));
        for (var i = 0; i < saveData.generatedNpcs.Length; i++)
        {
            var npc = saveData.generatedNpcs[i];
            if (npc == null || !npc.isAlive)
            {
                continue;
            }

            npc.EnsureDefaults();
            if (random.NextDouble() < 0.38d)
            {
                npc.currentLocationId = PickNextLocation(saveData, npc, random);
            }

            if (random.NextDouble() < 0.14d && npc.realmTier < 5)
            {
                npc.realmTier += 1;
            }
        }

        for (var i = 0; i < saveData.npcRelations.Length; i++)
        {
            var edge = saveData.npcRelations[i];
            if (edge == null)
            {
                continue;
            }

            edge.EnsureDefaults();
            edge.affinity = Mathf.Clamp(edge.affinity + random.Next(-1, 2), -3, 12);
            edge.hostility = Mathf.Clamp(edge.hostility + random.Next(-1, 2), 0, 10);
        }

        saveData.worldDayLogs = AppendUnique(saveData.worldDayLogs,
            "太初历第" + simulationDay + "日：山海间的修士们又换了几处落脚地。");
    }

    private string PickNextLocation(CultivationSaveData saveData, GeneratedNpcData npc, System.Random random)
    {
        var anchor = generationSystem.ResolveSceneAnchor(saveData, npc.currentLocationId);
        var visibleLocations = generationSystem.GetVisibleLocations(saveData, anchor, npc.sceneType);
        if (visibleLocations.Length > 0 && random.NextDouble() > 0.4d)
        {
            return visibleLocations[random.Next(0, visibleLocations.Length)].locationId;
        }

        switch (npc.sceneType)
        {
            case NpcSceneType.SectResidence:
            {
                var halls = new[] { "duty_hall", "refining_hall", "alchemy_hall", "talisman_hall", "scripture_hall", "steward_hall", "cave_residence" };
                return halls[random.Next(0, halls.Length)];
            }
            case NpcSceneType.Region:
            {
                var regions = WorldRegionLibrary.GetRegions();
                return regions[random.Next(0, regions.Count)].Id;
            }
            default:
            {
                var regions = WorldRegionLibrary.GetRegions();
                var regionId = regions[random.Next(0, regions.Count)].Id;
                var settlementLocations = generationSystem.GetVisibleLocations(saveData, regionId, NpcSceneType.Settlement);
                return settlementLocations.Length > 0 && random.NextDouble() > 0.5d
                    ? settlementLocations[random.Next(0, settlementLocations.Length)].locationId
                    : regionId;
            }
        }
    }

    private static string[] AppendUnique(string[] values, string entry)
    {
        var list = new List<string>(values ?? Array.Empty<string>());
        if (!string.IsNullOrWhiteSpace(entry) && !list.Contains(entry))
        {
            list.Add(entry);
        }

        return list.ToArray();
    }
}

public sealed class CultivationWorldIncidentSystem : AbstractSystem
{
    private static readonly WorldEventTemplate[] Templates = BuildTemplates();

    private CultivationTaskSystem taskSystem;
    private CultivationWorldGenerationSystem generationSystem;

    protected override void OnInit()
    {
        taskSystem = this.GetSystem<CultivationTaskSystem>();
        generationSystem = this.GetSystem<CultivationWorldGenerationSystem>();
    }

    public void EnsureIncidents(CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        generationSystem.EnsureWorldGenerated(saveData);
        if (saveData.activeWorldIncidents == null || saveData.activeWorldIncidents.Length == 0)
        {
            RebuildIncidents(saveData);
            return;
        }

        for (var i = 0; i < saveData.activeWorldIncidents.Length; i++)
        {
            var incident = saveData.activeWorldIncidents[i];
            if (incident == null)
            {
                continue;
            }

            incident.EnsureDefaults();
            if (incident.status == WorldIncidentStatus.Active && incident.expireDay > 0 && saveData.worldDay > incident.expireDay)
            {
                incident.status = WorldIncidentStatus.Expired;
            }
        }
    }

    public void RebuildIncidents(CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        generationSystem.EnsureWorldGenerated(saveData);
        var taskContext = taskSystem != null ? taskSystem.GetActiveTaskContext(saveData) : null;
        var incidents = new List<WorldIncidentData>();
        for (var i = 0; i < Templates.Length; i++)
        {
            var template = Templates[i];
            if (!TemplateMatches(saveData, taskContext, template))
            {
                continue;
            }

            if (TryInstantiateIncident(saveData, taskContext, template, out var incident))
            {
                incidents.Add(incident);
            }
        }

        saveData.activeWorldIncidents = incidents.ToArray();
    }

    public bool TryCreateIncident(CultivationSaveData saveData, string regionId, out WorldIncidentData incident)
    {
        incident = null;
        EnsureIncidents(saveData);
        if (saveData == null)
        {
            return false;
        }

        for (var i = 0; i < saveData.activeWorldIncidents.Length; i++)
        {
            var candidate = saveData.activeWorldIncidents[i];
            if (candidate == null || candidate.status != WorldIncidentStatus.Active)
            {
                continue;
            }

            var location = saveData.FindGeneratedLocation(candidate.locationId);
            var parent = location != null ? location.parentLocationId : generationSystem.ResolveSceneAnchor(saveData, candidate.locationId);
            if (parent == regionId)
            {
                incident = candidate;
                return true;
            }
        }

        return false;
    }

    public WorldIncidentData[] GetIncidentsForParent(CultivationSaveData saveData, string parentLocationId, NpcSceneType sceneType)
    {
        EnsureIncidents(saveData);
        if (saveData == null || string.IsNullOrWhiteSpace(parentLocationId))
        {
            return Array.Empty<WorldIncidentData>();
        }

        var list = new List<WorldIncidentData>();
        for (var i = 0; i < saveData.activeWorldIncidents.Length; i++)
        {
            var incident = saveData.activeWorldIncidents[i];
            if (incident == null || incident.status != WorldIncidentStatus.Active)
            {
                continue;
            }

            var location = saveData.FindGeneratedLocation(incident.locationId);
            var resolvedParent = location != null ? location.parentLocationId : generationSystem.ResolveSceneAnchor(saveData, incident.locationId);
            var resolvedScene = location != null ? location.sceneType : sceneType;
            if (resolvedParent == parentLocationId && resolvedScene == sceneType)
            {
                list.Add(incident);
            }
        }

        return list.ToArray();
    }

    private bool TryInstantiateIncident(CultivationSaveData saveData, TaskContextSnapshot taskContext, WorldEventTemplate template, out WorldIncidentData incident)
    {
        incident = null;
        var participants = FindParticipants(saveData, template, taskContext);
        if (participants.Length == 0)
        {
            return false;
        }

        var primaryNpc = saveData.FindGeneratedNpc(participants[0]);
        var locationId = !string.IsNullOrWhiteSpace(template.PreferredLocationId)
            ? template.PreferredLocationId
            : primaryNpc != null ? primaryNpc.currentLocationId : string.Empty;
        if (string.IsNullOrWhiteSpace(locationId))
        {
            return false;
        }

        incident = new WorldIncidentData
        {
            incidentId = template.Id + ":" + saveData.worldDay,
            templateId = template.Id,
            displayTitle = template.DisplayTitle,
            description = template.Description,
            locationId = locationId,
            conversationTitle = template.ConversationTitle,
            sourceTaskId = template.RequiredTaskId ?? string.Empty,
            participantNpcIds = participants,
            startDay = saveData.worldDay,
            expireDay = saveData.worldDay + 2,
            status = WorldIncidentStatus.Active
        };

        for (var i = 0; i < saveData.npcRelations.Length; i++)
        {
            var edge = saveData.npcRelations[i];
            if (edge != null && (edge.sourceNpcId == participants[0] || edge.targetNpcId == participants[0]))
            {
                edge.recentIncidentId = incident.incidentId;
            }
        }

        return true;
    }

    private string[] FindParticipants(CultivationSaveData saveData, WorldEventTemplate template, TaskContextSnapshot taskContext)
    {
        var requiredTags = ResolveRequiredNpcTags(taskContext, template);
        if (!string.IsNullOrWhiteSpace(template.PreferredLocationId))
        {
            var residents = generationSystem.GetNpcsAtLocation(saveData, template.PreferredLocationId);
            var matches = CollectMatches(residents, template.RequiredRoleType, requiredTags);
            if (matches.Length > 0)
            {
                return matches;
            }
        }

        var fallback = new List<string>();
        for (var i = 0; i < saveData.generatedNpcs.Length; i++)
        {
            var npc = saveData.generatedNpcs[i];
            if (npc == null || !npc.isAlive || npc.sceneType != template.SceneType || npc.roleType != template.RequiredRoleType)
            {
                continue;
            }

            if (!NpcMatchesRequiredTags(npc, requiredTags))
            {
                continue;
            }

            fallback.Add(npc.npcId);
            if (fallback.Count >= 2)
            {
                break;
            }
        }

        return fallback.ToArray();
    }

    private static string[] CollectMatches(GeneratedNpcData[] npcs, NpcRoleType roleType, string[] requiredTags)
    {
        var list = new List<string>();
        for (var i = 0; i < npcs.Length; i++)
        {
            var npc = npcs[i];
            if (npc == null || npc.roleType != roleType || !NpcMatchesRequiredTags(npc, requiredTags))
            {
                continue;
            }

            list.Add(npc.npcId);
            if (list.Count >= 2)
            {
                break;
            }
        }

        return list.ToArray();
    }

    private static string[] ResolveRequiredNpcTags(TaskContextSnapshot taskContext, WorldEventTemplate template)
    {
        if (taskContext == null || taskContext.RequiredNpcTags == null || taskContext.RequiredNpcTags.Length == 0 || template == null)
        {
            return Array.Empty<string>();
        }

        if (!string.IsNullOrWhiteSpace(template.RequiredTaskId) && taskContext.ActiveTaskId == template.RequiredTaskId)
        {
            return taskContext.RequiredNpcTags;
        }

        return Contains(taskContext.InjectIncidentTemplateIds, template.Id)
            ? taskContext.RequiredNpcTags
            : Array.Empty<string>();
    }

    private static bool NpcMatchesRequiredTags(GeneratedNpcData npc, string[] requiredTags)
    {
        if (npc == null)
        {
            return false;
        }

        if (requiredTags == null || requiredTags.Length == 0)
        {
            return true;
        }

        for (var i = 0; i < requiredTags.Length; i++)
        {
            var tag = requiredTags[i];
            if (string.IsNullOrWhiteSpace(tag))
            {
                continue;
            }

            if (Contains(npc.personalityTags, tag) ||
                Contains(npc.fortuneTags, tag) ||
                npc.title.Contains(tag) ||
                npc.socialStyle.Contains(tag) ||
                npc.growthStyle.Contains(tag))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TemplateMatches(CultivationSaveData saveData, TaskContextSnapshot taskContext, WorldEventTemplate template)
    {
        if (template == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(template.RequiredTaskId) && saveData.activeTaskId != template.RequiredTaskId)
        {
            if (taskContext == null || !Contains(taskContext.InjectIncidentTemplateIds, template.Id))
            {
                return false;
            }
        }

        return true;
    }

    private static bool Contains(string[] values, string target)
    {
        if (values == null || string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (values[i] == target)
            {
                return true;
            }
        }

        return false;
    }

    private static WorldEventTemplate[] BuildTemplates()
    {
        return new[]
        {
            new WorldEventTemplate
            {
                Id = "incident_black_market_tipoff",
                DisplayTitle = "暗市线报",
                Description = "有人在暗巷里兜售和流寇路引有关的残缺消息。",
                SceneType = NpcSceneType.Settlement,
                RequiredRoleType = NpcRoleType.Merchant,
                RequiredTaskId = "task_bandit_route",
                PreferredLocationId = "green_stone_gate_black_market",
                ConversationTitle = string.Empty
            },
            new WorldEventTemplate
            {
                Id = "incident_herb_seekers",
                DisplayTitle = "采药人求援",
                Description = "营地缺一位认得灵芝脉络的人，几只药篓旁留着未熄的火。",
                SceneType = NpcSceneType.Region,
                RequiredRoleType = NpcRoleType.Hermit,
                RequiredTaskId = "task_mist_herbs",
                PreferredLocationId = "misty_forest_herb_camp",
                ConversationTitle = string.Empty
            },
            new WorldEventTemplate
            {
                Id = "incident_valley_watchfire",
                DisplayTitle = "谷口烽火",
                Description = "哨台上有人记录邪修的来回路线，正等人补完最后一道证词。",
                SceneType = NpcSceneType.Region,
                RequiredRoleType = NpcRoleType.Scout,
                RequiredTaskId = "task_valley_cultists",
                PreferredLocationId = "crimson_valley_watch_post",
                ConversationTitle = string.Empty
            },
            new WorldEventTemplate
            {
                Id = "incident_array_resonance",
                DisplayTitle = "古阵回鸣",
                Description = "洞天泉眼附近的歇脚台传来阵纹回响，像是等谁补上缺失的那一块。",
                SceneType = NpcSceneType.Region,
                RequiredRoleType = NpcRoleType.Hermit,
                RequiredTaskId = "task_springs_array",
                PreferredLocationId = "deep_springs_array_rest",
                ConversationTitle = string.Empty
            },
            new WorldEventTemplate
            {
                Id = "incident_guest_court_gossip",
                DisplayTitle = "别院风闻",
                Description = "会客别院里刚传开一则关于宗门人情的新消息。",
                SceneType = NpcSceneType.SectResidence,
                RequiredRoleType = NpcRoleType.Steward,
                PreferredLocationId = "cave_residence_guest_court",
                ConversationTitle = string.Empty
            }
        };
    }
}

public sealed class CultivationDialogueBindingSystem : AbstractSystem
{
    private CultivationWorldGenerationSystem generationSystem;

    protected override void OnInit()
    {
        generationSystem = this.GetSystem<CultivationWorldGenerationSystem>();
    }

    private void EnsureDependencies()
    {
        generationSystem ??= this.GetSystem<CultivationWorldGenerationSystem>();
    }

    public void BindNpcConversationContext(CultivationSaveData saveData, GeneratedNpcData npc, GeneratedLocationState location, WorldIncidentData incident)
    {
        if (saveData == null)
        {
            return;
        }

        EnsureDependencies();
        generationSystem.EnsureWorldGenerated(saveData);
        DialogueLua.SetVariable("RuntimeNpcId", npc != null ? npc.npcId : string.Empty);
        DialogueLua.SetVariable("RuntimeNpcName", npc != null ? npc.displayName : string.Empty);
        DialogueLua.SetVariable("RuntimeNpcTitle", npc != null ? npc.title : string.Empty);
        DialogueLua.SetVariable("RuntimeNpcRole", npc != null ? npc.roleType.ToString() : string.Empty);
        DialogueLua.SetVariable("RuntimeNpcFaction", npc != null ? npc.factionName : string.Empty);
        DialogueLua.SetVariable("RuntimeLocationId", location != null ? location.locationId : string.Empty);
        DialogueLua.SetVariable("RuntimeLocationName", location != null ? location.displayName : string.Empty);
        DialogueLua.SetVariable("RuntimeLocationSubtitle", location != null ? location.subtitle : string.Empty);
        DialogueLua.SetVariable("RuntimeIncidentId", incident != null ? incident.incidentId : string.Empty);
        DialogueLua.SetVariable("RuntimeIncidentTitle", incident != null ? incident.displayTitle : string.Empty);
        DialogueLua.SetVariable("RuntimeHeroName", saveData.heroName);
        DialogueLua.SetVariable("RuntimeSectName", saveData.sectName);
        DialogueLua.SetVariable("RuntimeWorldDay", saveData.worldDay);
    }

    public void ClearRuntimeConversationContext()
    {
        DialogueLua.SetVariable("RuntimeNpcId", string.Empty);
        DialogueLua.SetVariable("RuntimeNpcName", string.Empty);
        DialogueLua.SetVariable("RuntimeNpcTitle", string.Empty);
        DialogueLua.SetVariable("RuntimeNpcRole", string.Empty);
        DialogueLua.SetVariable("RuntimeNpcFaction", string.Empty);
        DialogueLua.SetVariable("RuntimeLocationId", string.Empty);
        DialogueLua.SetVariable("RuntimeLocationName", string.Empty);
        DialogueLua.SetVariable("RuntimeLocationSubtitle", string.Empty);
        DialogueLua.SetVariable("RuntimeIncidentId", string.Empty);
        DialogueLua.SetVariable("RuntimeIncidentTitle", string.Empty);
        DialogueLua.SetVariable("RuntimeHeroName", string.Empty);
        DialogueLua.SetVariable("RuntimeSectName", string.Empty);
        DialogueLua.SetVariable("RuntimeWorldDay", 0);
    }
}
