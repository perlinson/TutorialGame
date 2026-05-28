using System.Collections.Generic;
using System.Text;
using QFramework;
using UnityEngine;

public enum NpcSceneType
{
    Settlement,
    SectResidence,
    Region
}

public enum NpcRoleType
{
    Steward,
    Mentor,
    Rival,
    Merchant,
    Healer,
    Scout,
    Hermit,
    TaskContact
}

internal sealed class NpcChoiceDefinition
{
    public string Id;
    public string Label;
    public string Description;
    public string ConversationTitle;
}

internal sealed class NpcDefinition
{
    public string Id;
    public string DisplayName;
    public string Subtitle;
    public string Description;
    public NpcRoleType RoleType;
    public NpcSceneType SceneType;
    public string SectHallId;
    public string RegionId;
    public Color AccentColor;
    public NpcChoiceDefinition[] Choices;
    public GeneratedNpcData RuntimeNpc;
    public GeneratedLocationState RuntimeLocation;
    public WorldIncidentData RuntimeIncident;
}

public sealed class CultivationNpcSystem : AbstractSystem
{
    private CultivationSaveSystem saveSystem;
    private CultivationTaskSystem taskSystem;
    private CultivationStorySystem storySystem;
    private CultivationCurrencySystem currencySystem;
    private CultivationDialogueSystem dialogueSystem;
    private CultivationSectSystem sectSystem;
    private CultivationWorldGenerationSystem worldGenerationSystem;
    private CultivationWorldIncidentSystem worldIncidentSystem;
    private CultivationDialogueBindingSystem dialogueBindingSystem;

    private static readonly NpcDefinition[] BaseDefinitions = BuildBaseDefinitions();

    protected override void OnInit()
    {
        saveSystem = this.GetSystem<CultivationSaveSystem>();
        taskSystem = this.GetSystem<CultivationTaskSystem>();
        storySystem = this.GetSystem<CultivationStorySystem>();
        currencySystem = this.GetSystem<CultivationCurrencySystem>();
        dialogueSystem = this.GetSystem<CultivationDialogueSystem>();
        sectSystem = this.GetSystem<CultivationSectSystem>();
        worldGenerationSystem = this.GetSystem<CultivationWorldGenerationSystem>();
        worldIncidentSystem = this.GetSystem<CultivationWorldIncidentSystem>();
        dialogueBindingSystem = this.GetSystem<CultivationDialogueBindingSystem>();
    }

    private void EnsureDependencies()
    {
        saveSystem ??= this.GetSystem<CultivationSaveSystem>();
        taskSystem ??= this.GetSystem<CultivationTaskSystem>();
        storySystem ??= this.GetSystem<CultivationStorySystem>();
        currencySystem ??= this.GetSystem<CultivationCurrencySystem>();
        dialogueSystem ??= this.GetSystem<CultivationDialogueSystem>();
        sectSystem ??= this.GetSystem<CultivationSectSystem>();
        worldGenerationSystem ??= this.GetSystem<CultivationWorldGenerationSystem>();
        worldIncidentSystem ??= this.GetSystem<CultivationWorldIncidentSystem>();
        dialogueBindingSystem ??= this.GetSystem<CultivationDialogueBindingSystem>();
    }

    public WorldMapNpcDialogueSnapshot BuildDialogueSnapshot(
        CultivationSaveData saveData,
        NpcSceneType sceneType,
        string regionId,
        string sectHallId,
        string locationId,
        string selectedNpcId)
    {
        EnsureDependencies();
        saveData = EnsureSaveData(saveData);
        var definitions = BuildNpcRoster(saveData, sceneType, regionId, sectHallId, locationId);
        var storySummary = storySystem.BuildStorySummary(saveData);
        var anchorId = ResolveAnchorId(saveData, sceneType, regionId, sectHallId);
        var incidents = ResolveRelevantIncidents(saveData, sceneType, anchorId, locationId);
        if (incidents.Length > 0)
        {
            storySummary += string.IsNullOrWhiteSpace(locationId) ? "\n\n当前风闻：" : "\n\n驻点风闻：";
            for (var i = 0; i < incidents.Length; i++)
            {
                if (i > 0)
                {
                    storySummary += " / ";
                }

                storySummary += incidents[i].displayTitle;
            }
        }
        var taskContext = taskSystem.GetActiveTaskContext(saveData);
        var resolvedDefinition = ResolveSelectedDefinition(definitions, selectedNpcId);
        var entries = BuildEntrySnapshots(saveData, definitions, resolvedDefinition);

        if (resolvedDefinition == null)
        {
            return new WorldMapNpcDialogueSnapshot
            {
                PanelTitle = BuildPanelTitle(saveData, sceneType, regionId, sectHallId, locationId),
                PanelSubtitle = BuildPanelSubtitle(saveData, sceneType, regionId, sectHallId, locationId),
                StorySummary = storySummary,
                TaskSummary = taskContext != null ? taskContext.ActiveTaskSummary : "委托：暂无。",
                NpcTitle = "暂无可交谈对象",
                NpcSubtitle = "人物 / 对话 / 线索",
                NpcDescription = string.IsNullOrWhiteSpace(locationId) ? "当前场景没有可交谈的 NPC。" : "这个驻点当前没有可交谈的 NPC。",
                NpcStatus = "状态：无可用对话。",
                Preview = new WorldMapPreviewSnapshot
                {
                    Label = "人物卷宗",
                    PlaceholderColor = new Color(0.18f, 0.15f, 0.12f, 1f)
                },
                Entries = entries,
                Incidents = BuildIncidentSnapshots(saveData, incidents),
                Choices = new WorldMapNpcChoiceSnapshot[0],
                SelectedNpcId = string.Empty
            };
        }

        var npcState = saveData.GetOrCreateNpcState(resolvedDefinition.Id);
        var status = BuildNpcStatus(saveData, resolvedDefinition, npcState, taskContext, regionId);
        return new WorldMapNpcDialogueSnapshot
        {
            PanelTitle = BuildPanelTitle(saveData, sceneType, regionId, sectHallId, locationId),
            PanelSubtitle = BuildPanelSubtitle(saveData, sceneType, regionId, sectHallId, locationId),
            StorySummary = storySummary,
            TaskSummary = taskContext != null ? taskContext.ActiveTaskSummary : "委托：暂无。",
            NpcTitle = resolvedDefinition.DisplayName,
            NpcSubtitle = resolvedDefinition.Subtitle + " / " + GetRoleLabel(resolvedDefinition.RoleType),
            NpcDescription = BuildNpcDescription(saveData, resolvedDefinition, taskContext, regionId),
            NpcStatus = status,
            Preview = new WorldMapPreviewSnapshot
            {
                Label = resolvedDefinition.DisplayName,
                PlaceholderColor = resolvedDefinition.AccentColor
            },
            Entries = entries,
            Incidents = BuildIncidentSnapshots(saveData, incidents),
            Choices = BuildChoiceSnapshots(saveData, resolvedDefinition, npcState, taskContext, regionId),
            SelectedNpcId = resolvedDefinition.Id
        };
    }

    public NpcInteractionResult ExecuteChoice(
        int slotIndex,
        CultivationSaveData saveData,
        NpcSceneType sceneType,
        string regionId,
        string sectHallId,
        string locationId,
        string npcId,
        string choiceId)
    {
        EnsureDependencies();
        saveData = EnsureSaveData(saveData);
        var definitions = BuildNpcRoster(saveData, sceneType, regionId, sectHallId, locationId);
        var definition = FindDefinition(definitions, npcId);
        if (definition == null)
        {
            return new NpcInteractionResult(false, "当前没有可交谈的对象。", npcId);
        }

        var choice = FindChoice(definition, choiceId);
        if (choice == null)
        {
            return new NpcInteractionResult(false, "对话选项不存在。", definition.Id);
        }

        var npcState = saveData.GetOrCreateNpcState(definition.Id);
        var taskContext = taskSystem.GetActiveTaskContext(saveData);
        string unavailableReason;
        if (!IsChoiceAvailable(saveData, definition, npcState, choice, taskContext, regionId, out unavailableReason))
        {
            return new NpcInteractionResult(false, unavailableReason, definition.Id);
        }

        if (!string.IsNullOrWhiteSpace(choice.ConversationTitle) &&
            dialogueSystem != null &&
            dialogueSystem.IsReady &&
            dialogueSystem.HasConversation(choice.ConversationTitle))
        {
            dialogueBindingSystem?.BindNpcConversationContext(saveData, definition.RuntimeNpc, definition.RuntimeLocation, definition.RuntimeIncident);

            dialogueSystem.StartNpcConversation(choice.ConversationTitle, saveData, () =>
            {
                saveData.EnsureDefaults();
                npcState.interactionCount++;
                npcState.lastInteractionDay = saveData.worldDay;
                npcState.lastChoiceId = choice.Id ?? string.Empty;
                saveSystem.SaveArchive(slotIndex, saveData);
            });

            return new NpcInteractionResult(true, "开始与" + definition.DisplayName + "交谈。", definition.Id);
        }

        var messages = new List<string>();
        var result = ApplyChoiceEffect(saveData, definition, npcState, choice, taskContext, regionId, messages);
        GameTime.Advance(saveData, 1);
        npcState.interactionCount++;
        npcState.lastInteractionDay = saveData.worldDay;
        npcState.lastChoiceId = choice.Id ?? string.Empty;
        saveSystem.SaveArchive(slotIndex, saveData);

        if (messages.Count == 0)
        {
            messages.Add(result);
        }
        else if (!string.IsNullOrWhiteSpace(result))
        {
            messages.Insert(0, result);
        }

        return new NpcInteractionResult(true, string.Join("\n", messages), definition.Id);
    }

    private static CultivationSaveData EnsureSaveData(CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            saveData = new CultivationSaveData();
        }

        saveData.EnsureDefaults();
        return saveData;
    }

    private List<NpcDefinition> BuildNpcRoster(CultivationSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, string locationId)
    {
        var list = new List<NpcDefinition>();
        if (!string.IsNullOrWhiteSpace(locationId))
        {
            AppendGeneratedLocationDefinitions(saveData, sceneType, locationId, list);
            return list;
        }

        AppendGeneratedDefinitions(saveData, sceneType, regionId, sectHallId, list);
        if (list.Count == 0)
        {
            for (var i = 0; i < BaseDefinitions.Length; i++)
            {
                var definition = BaseDefinitions[i];
                if (definition == null || definition.SceneType != sceneType)
                {
                    continue;
                }

                if (sceneType == NpcSceneType.SectResidence && definition.SectHallId != sectHallId)
                {
                    continue;
                }

                list.Add(definition);
            }
        }

        if (sceneType == NpcSceneType.Region)
        {
            AppendRegionDefinitions(saveData, regionId, list);
        }

        return list;
    }

    private void AppendGeneratedLocationDefinitions(CultivationSaveData saveData, NpcSceneType sceneType, string locationId, List<NpcDefinition> list)
    {
        if (saveData == null || list == null || worldGenerationSystem == null || string.IsNullOrWhiteSpace(locationId))
        {
            return;
        }

        worldGenerationSystem.EnsureWorldGenerated(saveData);
        worldIncidentSystem?.EnsureIncidents(saveData);
        var npcs = worldGenerationSystem.GetNpcsAtLocation(saveData, locationId);
        for (var i = 0; i < npcs.Length; i++)
        {
            var npc = npcs[i];
            if (npc == null || npc.sceneType != sceneType)
            {
                continue;
            }

            list.Add(BuildGeneratedDefinition(saveData, npc, FindIncidentForNpc(saveData, npc.npcId)));
        }
    }

    private void AppendGeneratedDefinitions(CultivationSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, List<NpcDefinition> list)
    {
        if (saveData == null || list == null || worldGenerationSystem == null)
        {
            return;
        }

        worldGenerationSystem.EnsureWorldGenerated(saveData);
        worldIncidentSystem?.EnsureIncidents(saveData);
        var anchorId = ResolveAnchorId(saveData, sceneType, regionId, sectHallId);
        if (string.IsNullOrWhiteSpace(anchorId))
        {
            return;
        }

        var npcs = worldGenerationSystem.GetNpcsForScene(saveData, sceneType, anchorId);
        for (var i = 0; i < npcs.Length; i++)
        {
            var npc = npcs[i];
            if (npc == null)
            {
                continue;
            }

            list.Add(BuildGeneratedDefinition(saveData, npc, FindIncidentForNpc(saveData, npc.npcId)));
        }
    }

    private NpcDefinition BuildGeneratedDefinition(CultivationSaveData saveData, GeneratedNpcData npc, WorldIncidentData incident)
    {
        var runtimeLocation = worldGenerationSystem != null ? worldGenerationSystem.ResolveLocationState(saveData, npc.currentLocationId) : null;
        return new NpcDefinition
        {
            Id = npc.npcId,
            DisplayName = npc.displayName,
            Subtitle = string.IsNullOrWhiteSpace(npc.title) ? GetRoleLabel(npc.roleType) : npc.title,
            Description = BuildGeneratedIdentityLine(saveData, npc, runtimeLocation),
            RoleType = npc.roleType,
            SceneType = npc.sceneType,
            SectHallId = npc.sceneType == NpcSceneType.SectResidence ? worldGenerationSystem.ResolveSceneAnchor(saveData, npc.currentLocationId) : string.Empty,
            RegionId = npc.sceneType == NpcSceneType.Region || npc.sceneType == NpcSceneType.Settlement
                ? worldGenerationSystem.ResolveSceneAnchor(saveData, npc.currentLocationId)
                : string.Empty,
            AccentColor = BuildGeneratedAccentColor(npc.roleType),
            Choices = BuildGeneratedChoices(npc, incident),
            RuntimeNpc = npc,
            RuntimeLocation = runtimeLocation,
            RuntimeIncident = incident
        };
    }

    private void AppendRegionDefinitions(CultivationSaveData saveData, string regionId, List<NpcDefinition> list)
    {
        if (list == null || string.IsNullOrWhiteSpace(regionId))
        {
            return;
        }

        var regionName = WorldRegionLibrary.GetRegionDisplayName(regionId);
        list.Add(new NpcDefinition
        {
            Id = "region_scout_" + regionId,
            DisplayName = regionName + "斥候",
            Subtitle = "先行探路 / 地域见闻",
            Description = "熟悉地脉岔路与残阵气息的本地斥候，擅长指出最危险的异动点。",
            RoleType = NpcRoleType.Scout,
            SceneType = NpcSceneType.Region,
            RegionId = regionId,
            AccentColor = new Color(0.16f, 0.22f, 0.18f, 1f),
            Choices = new[]
            {
                new NpcChoiceDefinition { Id = "region_scout_clue", Label = "询问地界异动", Description = "换取当前地域的见闻与推进线索。" },
                new NpcChoiceDefinition { Id = "region_scout_route", Label = "确认安全路线", Description = "了解当前地域更稳妥的推进方向。" }
            }
        });

        var taskContext = taskSystem.GetActiveTaskContext(saveData);
        if (taskContext == null || taskContext.TaskLinkedRegionIds == null)
        {
            return;
        }

        for (var i = 0; i < taskContext.TaskLinkedRegionIds.Length; i++)
        {
            if (taskContext.TaskLinkedRegionIds[i] != regionId)
            {
                continue;
            }

            list.Add(new NpcDefinition
            {
                Id = "task_contact_" + regionId,
                DisplayName = "线人信使",
                Subtitle = "委托接头 / 口供 / 证物",
                Description = "负责和历练者交换线索、确认委托节点的外线接头。",
                RoleType = NpcRoleType.TaskContact,
                SceneType = NpcSceneType.Region,
                RegionId = regionId,
                AccentColor = new Color(0.28f, 0.2f, 0.12f, 1f),
                Choices = new[]
                {
                    new NpcChoiceDefinition { Id = "task_contact_exchange", Label = "交换任务线索", Description = "对接当前主委托，获取一次额外推进。" }
                }
            });
            break;
        }
    }

    private static string BuildGeneratedIdentityLine(CultivationSaveData saveData, GeneratedNpcData npc, GeneratedLocationState location)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(npc.title))
        {
            builder.Append(npc.title);
        }

        if (!string.IsNullOrWhiteSpace(npc.factionName))
        {
            builder.Append(builder.Length > 0 ? " / " : string.Empty).Append(npc.factionName);
        }

        builder.Append("\n境界：").Append(WorldRegionLibrary.GetRealmName(npc.realmTier));
        if (location != null && !string.IsNullOrWhiteSpace(location.displayName))
        {
            builder.Append("\n驻留：").Append(location.displayName);
        }

        if (saveData != null && !string.IsNullOrWhiteSpace(saveData.heroName))
        {
            builder.Append("\n近来常听闻 ").Append(saveData.heroName).Append(" 的名号。");
        }

        return builder.ToString();
    }

    private static Color BuildGeneratedAccentColor(NpcRoleType roleType)
    {
        switch (roleType)
        {
            case NpcRoleType.Merchant:
                return new Color(0.30f, 0.22f, 0.10f, 1f);
            case NpcRoleType.Healer:
                return new Color(0.14f, 0.27f, 0.19f, 1f);
            case NpcRoleType.Scout:
                return new Color(0.15f, 0.21f, 0.19f, 1f);
            case NpcRoleType.Mentor:
                return new Color(0.22f, 0.21f, 0.32f, 1f);
            case NpcRoleType.Rival:
                return new Color(0.31f, 0.15f, 0.15f, 1f);
            case NpcRoleType.Steward:
                return new Color(0.27f, 0.20f, 0.14f, 1f);
            default:
                return new Color(0.22f, 0.22f, 0.20f, 1f);
        }
    }

    private NpcChoiceDefinition[] BuildGeneratedChoices(GeneratedNpcData npc, WorldIncidentData incident)
    {
        var choices = new List<NpcChoiceDefinition>();
        if (incident != null)
        {
            choices.Add(new NpcChoiceDefinition
            {
                Id = "generated_incident_followup",
                Label = incident.displayTitle,
                Description = incident.description,
                ConversationTitle = incident.conversationTitle
            });
        }

        switch (npc.roleType)
        {
            case NpcRoleType.Steward:
                choices.Add(new NpcChoiceDefinition { Id = "sect_steward_hint", Label = "核对委托线索", Description = "对接一次主委托进度，获得更明确的目标提示。" });
                break;
            case NpcRoleType.Mentor:
                choices.Add(new NpcChoiceDefinition
                {
                    Id = "sect_mentor_guidance",
                    Label = "请教修行关窍",
                    Description = "请对方点评最近的修行节点。",
                    ConversationTitle = npc.conversationTemplateTitle
                });
                break;
            case NpcRoleType.Rival:
                choices.Add(new NpcChoiceDefinition { Id = "sect_rival_spar", Label = "约一场切磋", Description = "每日一次，换取少量修为与灵石。" });
                break;
            case NpcRoleType.Merchant:
                choices.Add(new NpcChoiceDefinition { Id = "settlement_merchant_news", Label = "打听外路风声", Description = "获得当前地界的情报，并可能推进关联委托。" });
                choices.Add(new NpcChoiceDefinition { Id = "settlement_merchant_supply", Label = "收一份顺手补给", Description = "每日一次，领取少量灵砂与灵石。" });
                break;
            case NpcRoleType.Healer:
                choices.Add(new NpcChoiceDefinition { Id = "settlement_healer_tonic", Label = "讨一味安神药引", Description = "每日一次，领取清心香灰。" });
                choices.Add(new NpcChoiceDefinition { Id = "settlement_healer_consult", Label = "请她看斗法后患", Description = "补充一条人物经历记录。" });
                break;
            case NpcRoleType.Scout:
                choices.Add(new NpcChoiceDefinition { Id = "region_scout_clue", Label = "询问地界异动", Description = "换取当前地域的见闻与推进线索。" });
                choices.Add(new NpcChoiceDefinition { Id = "region_scout_route", Label = "确认安全路线", Description = "了解当前地域更稳妥的推进方向。" });
                break;
            case NpcRoleType.TaskContact:
                choices.Add(new NpcChoiceDefinition { Id = "task_contact_exchange", Label = "交换任务线索", Description = "对接当前主委托，获取一次额外推进。" });
                break;
            default:
                choices.Add(new NpcChoiceDefinition { Id = "generated_hermit_insight", Label = "听他谈一段山中见闻", Description = "补一段心境与见闻记录。" });
                break;
        }

        return choices.ToArray();
    }

    private WorldIncidentData FindIncidentForNpc(CultivationSaveData saveData, string npcId)
    {
        if (saveData == null || saveData.activeWorldIncidents == null)
        {
            return null;
        }

        for (var i = 0; i < saveData.activeWorldIncidents.Length; i++)
        {
            var incident = saveData.activeWorldIncidents[i];
            if (incident == null || incident.status != WorldIncidentStatus.Active || incident.participantNpcIds == null)
            {
                continue;
            }

            for (var j = 0; j < incident.participantNpcIds.Length; j++)
            {
                if (incident.participantNpcIds[j] == npcId)
                {
                    return incident;
                }
            }
        }

        return null;
    }

    private WorldIncidentData[] ResolveRelevantIncidents(CultivationSaveData saveData, NpcSceneType sceneType, string anchorId, string locationId)
    {
        if (saveData == null)
        {
            return new WorldIncidentData[0];
        }

        if (!string.IsNullOrWhiteSpace(locationId))
        {
            return FindIncidentsForLocation(saveData, locationId);
        }

        if (worldIncidentSystem == null || string.IsNullOrWhiteSpace(anchorId))
        {
            return new WorldIncidentData[0];
        }

        var incidents = worldIncidentSystem.GetIncidentsForParent(saveData, anchorId, sceneType);
        return incidents ?? new WorldIncidentData[0];
    }

    private static WorldIncidentData[] FindIncidentsForLocation(CultivationSaveData saveData, string locationId)
    {
        if (saveData == null || saveData.activeWorldIncidents == null || string.IsNullOrWhiteSpace(locationId))
        {
            return new WorldIncidentData[0];
        }

        var list = new List<WorldIncidentData>();
        for (var i = 0; i < saveData.activeWorldIncidents.Length; i++)
        {
            var incident = saveData.activeWorldIncidents[i];
            if (incident == null || incident.status != WorldIncidentStatus.Active || incident.locationId != locationId)
            {
                continue;
            }

            list.Add(incident);
        }

        return list.ToArray();
    }

    private static string ResolveAnchorId(CultivationSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId)
    {
        switch (sceneType)
        {
            case NpcSceneType.SectResidence:
                return sectHallId;
            case NpcSceneType.Region:
                return regionId;
            default:
                return saveData != null ? saveData.currentRegionId : string.Empty;
        }
    }

    private static NpcDefinition ResolveSelectedDefinition(List<NpcDefinition> definitions, string selectedNpcId)
    {
        if (definitions == null || definitions.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(selectedNpcId))
        {
            for (var i = 0; i < definitions.Count; i++)
            {
                if (definitions[i] != null && definitions[i].Id == selectedNpcId)
                {
                    return definitions[i];
                }
            }
        }

        return definitions[0];
    }

    private static NpcDefinition FindDefinition(List<NpcDefinition> definitions, string npcId)
    {
        if (definitions == null || string.IsNullOrWhiteSpace(npcId))
        {
            return null;
        }

        for (var i = 0; i < definitions.Count; i++)
        {
            if (definitions[i] != null && definitions[i].Id == npcId)
            {
                return definitions[i];
            }
        }

        return null;
    }

    private static NpcChoiceDefinition FindChoice(NpcDefinition definition, string choiceId)
    {
        if (definition == null || definition.Choices == null || string.IsNullOrWhiteSpace(choiceId))
        {
            return null;
        }

        for (var i = 0; i < definition.Choices.Length; i++)
        {
            if (definition.Choices[i] != null && definition.Choices[i].Id == choiceId)
            {
                return definition.Choices[i];
            }
        }

        return null;
    }

    private WorldMapNpcEntrySnapshot[] BuildEntrySnapshots(CultivationSaveData saveData, List<NpcDefinition> definitions, NpcDefinition selected)
    {
        if (definitions == null || definitions.Count == 0)
        {
            return new WorldMapNpcEntrySnapshot[0];
        }

        var snapshots = new WorldMapNpcEntrySnapshot[definitions.Count];
        for (var i = 0; i < definitions.Count; i++)
        {
            var definition = definitions[i];
            var state = saveData.GetOrCreateNpcState(definition.Id);
            snapshots[i] = new WorldMapNpcEntrySnapshot
            {
                NpcId = definition.Id,
                DisplayName = definition.DisplayName,
                RoleLabel = GetRoleLabel(definition.RoleType),
                StatusText = GetAffinityLabel(state.affinity),
                IsSelected = selected != null && definition.Id == selected.Id,
                IsInteractable = true
            };
        }

        return snapshots;
    }

    private WorldMapNpcChoiceSnapshot[] BuildChoiceSnapshots(
        CultivationSaveData saveData,
        NpcDefinition definition,
        SaveNpcState npcState,
        TaskContextSnapshot taskContext,
        string regionId)
    {
        if (definition == null || definition.Choices == null)
        {
            return new WorldMapNpcChoiceSnapshot[0];
        }

        var snapshots = new WorldMapNpcChoiceSnapshot[definition.Choices.Length];
        for (var i = 0; i < definition.Choices.Length; i++)
        {
            var choice = definition.Choices[i];
            string unavailableReason;
            var available = IsChoiceAvailable(saveData, definition, npcState, choice, taskContext, regionId, out unavailableReason);
            snapshots[i] = new WorldMapNpcChoiceSnapshot
            {
                ChoiceId = choice.Id,
                ButtonLabel = choice.Label,
                Description = choice.Description,
                IsVisible = choice != null,
                IsInteractable = available,
                TooltipTitle = choice.Label,
                TooltipBody = string.IsNullOrWhiteSpace(unavailableReason) ? choice.Description : choice.Description + "\n\n" + unavailableReason
            };
        }

        return snapshots;
    }

    private WorldMapIncidentEntrySnapshot[] BuildIncidentSnapshots(CultivationSaveData saveData, WorldIncidentData[] incidents)
    {
        if (incidents == null || incidents.Length == 0)
        {
            return new WorldMapIncidentEntrySnapshot[0];
        }

        var snapshots = new WorldMapIncidentEntrySnapshot[incidents.Length];
        for (var i = 0; i < incidents.Length; i++)
        {
            var incident = incidents[i];
            if (incident == null)
            {
                continue;
            }

            var hasConversation = !string.IsNullOrWhiteSpace(incident.conversationTitle) &&
                                  dialogueSystem != null &&
                                  dialogueSystem.IsReady &&
                                  dialogueSystem.HasConversation(incident.conversationTitle);
            var tooltip = new StringBuilder(incident.description ?? string.Empty);
            var locationName = worldGenerationSystem != null
                ? worldGenerationSystem.ResolveLocationName(saveData, incident.locationId)
                : string.Empty;
            if (!string.IsNullOrWhiteSpace(locationName))
            {
                tooltip.Append("\n\n发生地点：").Append(locationName);
            }

            var participants = BuildIncidentParticipantSummary(saveData, incident);
            if (!string.IsNullOrWhiteSpace(participants))
            {
                tooltip.Append("\n参与人物：").Append(participants);
            }

            if (!string.IsNullOrWhiteSpace(incident.sourceTaskId) && incident.sourceTaskId == saveData.activeTaskId)
            {
                tooltip.Append("\n关联：当前主委托。");
            }

            tooltip.Append(hasConversation ? "\n\n可直接进入事件对话。" : "\n\n将跟进相关人物的风闻线索。");

            snapshots[i] = new WorldMapIncidentEntrySnapshot
            {
                IncidentId = incident.incidentId,
                ButtonLabel = hasConversation ? "事件：" + incident.displayTitle : "追查：" + incident.displayTitle,
                Description = incident.description,
                TooltipTitle = incident.displayTitle,
                TooltipBody = tooltip.ToString(),
                IsVisible = true,
                IsInteractable = true,
                HasConversation = hasConversation
            };
        }

        return snapshots;
    }

    private string BuildIncidentParticipantSummary(CultivationSaveData saveData, WorldIncidentData incident)
    {
        if (saveData == null || incident == null || incident.participantNpcIds == null || incident.participantNpcIds.Length == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        for (var i = 0; i < incident.participantNpcIds.Length; i++)
        {
            var npcId = incident.participantNpcIds[i];
            if (string.IsNullOrWhiteSpace(npcId))
            {
                continue;
            }

            var npc = saveData.FindGeneratedNpc(npcId);
            if (builder.Length > 0)
            {
                builder.Append(" / ");
            }

            builder.Append(npc != null && !string.IsNullOrWhiteSpace(npc.displayName) ? npc.displayName : npcId);
        }

        return builder.ToString();
    }

    private bool IsChoiceAvailable(
        CultivationSaveData saveData,
        NpcDefinition definition,
        SaveNpcState npcState,
        NpcChoiceDefinition choice,
        TaskContextSnapshot taskContext,
        string regionId,
        out string reason)
    {
        reason = string.Empty;
        if (definition == null || npcState == null || choice == null)
        {
            reason = "当前无法进行该对话。";
            return false;
        }

        switch (choice.Id)
        {
            case "sect_steward_hint":
                if (taskContext == null || string.IsNullOrWhiteSpace(taskContext.ActiveTaskId))
                {
                    reason = "当前没有主委托。";
                    return false;
                }

                var stewardFlag = "hint:" + taskContext.ActiveTaskId;
                if (npcState.HasFlag(stewardFlag))
                {
                    reason = "这条委托线索今天已经对接过。";
                    return false;
                }

                return true;
            case "sect_mentor_guidance":
            {
                var flag = "guidance:" + saveData.realmTier;
                if (npcState.HasFlag(flag))
                {
                    reason = "当前境界的点拨已经领过。";
                    return false;
                }

                return true;
            }
            case "sect_rival_spar":
            {
                var flag = "spar:" + saveData.worldDay;
                if (npcState.HasFlag(flag))
                {
                    reason = "今天已经切磋过了。";
                    return false;
                }

                return true;
            }
            case "settlement_merchant_news":
            {
                var flag = "news:" + saveData.currentRegionId;
                if (npcState.HasFlag(flag))
                {
                    reason = "这片地界的消息你已经听过一轮。";
                    return false;
                }

                return true;
            }
            case "settlement_merchant_supply":
            {
                var flag = "supply:" + saveData.worldDay;
                if (npcState.HasFlag(flag))
                {
                    reason = "今天的顺手补给已经拿过。";
                    return false;
                }

                if (saveData.GetUsedBagSlots() >= saveData.bagCapacity)
                {
                    reason = "储物袋已满。";
                    return false;
                }

                return true;
            }
            case "settlement_healer_tonic":
            {
                var tonicFlag = "tonic:" + saveData.worldDay;
                if (npcState.HasFlag(tonicFlag))
                {
                    reason = "今日已经领过安神药引。";
                    return false;
                }

                if (saveData.GetUsedBagSlots() >= saveData.bagCapacity)
                {
                    reason = "储物袋已满。";
                    return false;
                }

                return true;
            }
            case "settlement_healer_consult":
                return true;
            case "region_scout_clue":
            {
                var clueFlag = "region_clue:" + regionId;
                if (npcState.HasFlag(clueFlag))
                {
                    reason = "这个地域的显性线索已经问过。";
                    return false;
                }

                return true;
            }
            case "region_scout_route":
                return true;
            case "task_contact_exchange":
            {
                if (taskContext == null || string.IsNullOrWhiteSpace(taskContext.ActiveTaskId))
                {
                    reason = "当前没有可对接的主委托。";
                    return false;
                }

                var taskFlag = "contact:" + taskContext.ActiveTaskId;
                if (npcState.HasFlag(taskFlag))
                {
                    reason = "这条委托已经完成过线人对接。";
                    return false;
                }

                return true;
            }
            case "generated_incident_followup":
                return definition.RuntimeIncident != null;
            case "generated_hermit_insight":
                return true;
            default:
                return true;
        }
    }

    private string ApplyChoiceEffect(
        CultivationSaveData saveData,
        NpcDefinition definition,
        SaveNpcState npcState,
        NpcChoiceDefinition choice,
        TaskContextSnapshot taskContext,
        string regionId,
        List<string> messages)
    {
        switch (choice.Id)
        {
            case "sect_steward_hint":
                npcState.AddFlag("hint:" + taskContext.ActiveTaskId);
                npcState.affinity += 1;
                AppendTaskProgress(saveData, 1, messages);
                AppendStory(saveData, "sect_steward", taskContext.ActiveTaskId, "勤功殿递来补充案卷。", messages);
                return "执事把委托卷宗翻到最后一页，替你点明了下一步该盯的证物和人证。";
            case "sect_mentor_guidance":
                npcState.AddFlag("guidance:" + saveData.realmTier);
                npcState.affinity += 2;
                saveData.qi += 2 + saveData.realmTier;
                AppendStory(saveData, "sect_mentor", "realm_" + saveData.realmTier, "长老的点拨被记入经历。", messages);
                messages.Add("修为 + " + (2 + saveData.realmTier) + "。");
                return "长老为你拆解了当前境界的运行关窍，让后续吐纳和斗法都更稳了一分。";
            case "sect_rival_spar":
                npcState.AddFlag("spar:" + saveData.worldDay);
                npcState.affinity += 1;
                saveData.qi += 1;
                currencySystem.AddCrystals(saveData, 1);
                AppendStory(saveData, "sect_rival", "spar_day_" + saveData.worldDay, "同门切磋被记入经历。", messages);
                messages.Add("修为 + 1 / 灵石 + 1。");
                return "你和同门在演武坪过了几招，对方嘴上不服，却把一块压注灵石推给了你。";
            case "settlement_merchant_news":
                npcState.AddFlag("news:" + saveData.currentRegionId);
                npcState.affinity += 1;
                AppendConditionalTaskProgress(saveData, taskContext, saveData.currentRegionId, messages);
                AppendStory(saveData, "market_merchant", saveData.currentRegionId, "坊市里流出的风声被记下。", messages);
                return "行商把沿路收来的消息掐头去尾，只留最有用的部分递给了你。";
            case "settlement_merchant_supply":
                npcState.AddFlag("supply:" + saveData.worldDay);
                npcState.affinity += 1;
                TryGrantItem(saveData, "green_spirit_sand", 1, messages);
                messages.Add("灵石 + 1。");
                currencySystem.AddCrystals(saveData, 1);
                return "行商顺手塞给你一包压仓灵砂，还提醒你别把储物袋塞得太死。";
            case "settlement_healer_tonic":
                npcState.AddFlag("tonic:" + saveData.worldDay);
                npcState.affinity += 1;
                TryGrantItem(saveData, "mind_cleansing_incense", 1, messages);
                AppendStory(saveData, "market_healer", "tonic_" + saveData.worldDay, "药引与医嘱被记入经历。", messages);
                return "药师递来一撮清心香灰，叮嘱你下次入险地前先稳住神识。";
            case "settlement_healer_consult":
                npcState.affinity += 1;
                AppendStory(saveData, "market_healer", "consult_" + saveData.worldDay, "关于伤势与心境的交谈被记下。", messages);
                return "药师替你把最近的斗法痕迹捋了一遍，连你自己都忽略的隐患都点了出来。";
            case "region_scout_clue":
                npcState.AddFlag("region_clue:" + regionId);
                npcState.affinity += 1;
                AppendConditionalTaskProgress(saveData, taskContext, regionId, messages);
                AppendStory(saveData, "region_scout", regionId, "地域异动线索被记下。", messages);
                return "斥候把地形与异动一起画给你看，让你知道这片地界真正危险的并不是表面那条路。";
            case "region_scout_route":
                npcState.affinity += 1;
                AppendStory(saveData, "region_scout", "route_" + regionId, "推进路线建议被记下。", messages);
                return "对方替你圈出几段更稳妥的落脚点，也顺便指出哪处最容易遇到精英敌人。";
            case "task_contact_exchange":
                npcState.AddFlag("contact:" + taskContext.ActiveTaskId);
                npcState.affinity += 2;
                AppendTaskProgress(saveData, 1, messages);
                AppendStory(saveData, "task_contact", taskContext.ActiveTaskId, "委托接头完成。", messages);
                return "线人确认了你手上证据的价值，并把委托真正缺的那一块口供补了上来。";
            case "generated_incident_followup":
                npcState.affinity += 1;
                if (definition.RuntimeIncident != null)
                {
                    AppendStory(saveData, "world_incident", definition.RuntimeIncident.incidentId, definition.RuntimeIncident.displayTitle, messages);
                    if (!string.IsNullOrWhiteSpace(definition.RuntimeIncident.sourceTaskId) && definition.RuntimeIncident.sourceTaskId == saveData.activeTaskId)
                    {
                        AppendTaskProgress(saveData, 1, messages);
                    }

                    return definition.RuntimeIncident.description;
                }

                return "这则风闻暂时还没有后续。";
            case "generated_hermit_insight":
                npcState.affinity += 1;
                saveData.qi += 1;
                AppendStory(saveData, "wild_hermit", definition.Id, "山中见闻被记入经历。", messages);
                messages.Add("修为 + 1。");
                return "隐修者指了指山脊与云气交汇的地方，让你自己去体会那缕转瞬即逝的契机。";
            default:
                npcState.affinity += 1;
                return "对话结束。";
        }
    }

    private void AppendConditionalTaskProgress(CultivationSaveData saveData, TaskContextSnapshot taskContext, string regionId, List<string> messages)
    {
        if (taskContext == null || string.IsNullOrWhiteSpace(taskContext.ActiveTaskId))
        {
            return;
        }

        var linked = false;
        if (taskContext.TaskLinkedRegionIds != null)
        {
            for (var i = 0; i < taskContext.TaskLinkedRegionIds.Length; i++)
            {
                if (taskContext.TaskLinkedRegionIds[i] == regionId)
                {
                    linked = true;
                    break;
                }
            }
        }

        if (!linked)
        {
            return;
        }

        AppendTaskProgress(saveData, 1, messages);
    }

    private void AppendTaskProgress(CultivationSaveData saveData, int count, List<string> messages)
    {
        var progress = taskSystem.RecordProgress(saveData, new TaskProgressSignal
        {
            Type = TaskProgressSignalType.AddProgressToActiveTask,
            Count = count
        });

        if (progress != null && !string.IsNullOrWhiteSpace(progress.Message))
        {
            messages.Add(progress.Message);
        }
    }

    private void AppendStory(CultivationSaveData saveData, string storyId, string nodeId, string resultText, List<string> messages)
    {
        var story = storySystem.RecordSignal(saveData, new StorySignal
        {
            StoryId = storyId,
            NodeId = nodeId,
            Title = "人物交谈",
            ResultText = resultText
        });

        if (story != null && !string.IsNullOrWhiteSpace(story.Message))
        {
            messages.Add(story.Message);
        }
    }

    private static void TryGrantItem(CultivationSaveData saveData, string itemId, int quantity, List<string> messages)
    {
        if (!saveData.TryAddItem(itemId, quantity))
        {
            messages.Add("储物袋已满，未能收下 " + InventoryLibrary.GetDisplayName(itemId) + "。");
            return;
        }

        messages.Add("获得：" + InventoryLibrary.GetDisplayName(itemId) + " x" + quantity + "。");
    }

    private string BuildPanelTitle(CultivationSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, string locationId)
    {
        if (!string.IsNullOrWhiteSpace(locationId) && worldGenerationSystem != null)
        {
            var locationName = worldGenerationSystem.ResolveLocationName(saveData, locationId);
            if (!string.IsNullOrWhiteSpace(locationName))
            {
                return locationName;
            }
        }

        switch (sceneType)
        {
            case NpcSceneType.SectResidence:
                SectHallDefinition hallDefinition;
                if (sectSystem != null && sectSystem.TryGetHallDefinition(sectHallId, out hallDefinition) && hallDefinition != null)
                {
                    return hallDefinition.DisplayName;
                }

                return "宗门殿堂";
            case NpcSceneType.Region:
                return WorldRegionLibrary.GetRegionDisplayName(regionId) + "前沿据点";
            default:
                return saveData != null && saveData.isSectDisciple ? "山门坊市" : "行脚坊市";
        }
    }

    private string BuildPanelSubtitle(CultivationSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, string locationId)
    {
        if (!string.IsNullOrWhiteSpace(locationId))
        {
            var locationState = worldGenerationSystem != null ? worldGenerationSystem.ResolveLocationState(saveData, locationId) : null;
            if (locationState != null)
            {
                var suffix = sceneType == NpcSceneType.Region
                    ? " / 前沿驻点 / 人物 / 风闻"
                    : sceneType == NpcSceneType.SectResidence
                        ? " / 殿堂支点 / 同门 / 事务"
                        : " / 坊市分区 / 人物 / 风闻";
                return (string.IsNullOrWhiteSpace(locationState.subtitle) ? locationState.displayName : locationState.subtitle) + suffix;
            }
        }

        switch (sceneType)
        {
            case NpcSceneType.SectResidence:
                SectHallDefinition hallDefinition;
                if (sectSystem != null && sectSystem.TryGetHallDefinition(sectHallId, out hallDefinition) && hallDefinition != null)
                {
                    return hallDefinition.Subtitle + " / 同门往来 / 人情事务";
                }

                return string.IsNullOrWhiteSpace(sectHallId) ? "同门 / 执事 / 指点" : "当前殿堂 / 同门 / 事务对接";
            case NpcSceneType.Region:
                return WorldRegionLibrary.GetRegionDisplayName(regionId) + " / 斥候 / 线索 / 接头";
            default:
                return saveData != null && saveData.isSectDisciple
                    ? "坊市 / 行商 / 药师 / 山门风闻"
                    : "坊市 / 行商 / 药师 / 路途风闻";
        }
    }

    private static string BuildNpcDescription(CultivationSaveData saveData, NpcDefinition definition, TaskContextSnapshot taskContext, string regionId)
    {
        if (definition == null)
        {
            return string.Empty;
        }

        if (definition.RuntimeNpc != null)
        {
            var runtimeNpc = definition.RuntimeNpc;
            var runtimeBuilder = new StringBuilder();
            runtimeBuilder.Append(definition.Description);
            if (runtimeNpc.personalityTags != null && runtimeNpc.personalityTags.Length > 0)
            {
                runtimeBuilder.Append("\n\n性情：").Append(string.Join(" / ", runtimeNpc.personalityTags));
            }

            if (runtimeNpc.fortuneTags != null && runtimeNpc.fortuneTags.Length > 0)
            {
                runtimeBuilder.Append("\n机缘：").Append(string.Join(" / ", runtimeNpc.fortuneTags));
            }

            if (definition.RuntimeLocation != null && !string.IsNullOrWhiteSpace(definition.RuntimeLocation.description))
            {
                runtimeBuilder.Append("\n驻点见闻：").Append(definition.RuntimeLocation.description);
            }

            if (definition.RuntimeIncident != null)
            {
                runtimeBuilder.Append("\n当前风声：").Append(definition.RuntimeIncident.displayTitle);
            }

            return runtimeBuilder.ToString();
        }

        var builder = new StringBuilder();
        builder.Append(definition.Description);
        builder.Append("\n\n当前关系：").Append(GetRoleLabel(definition.RoleType));
        if (!string.IsNullOrWhiteSpace(regionId))
        {
            builder.Append("\n涉及地域：").Append(WorldRegionLibrary.GetRegionDisplayName(regionId));
        }

        if (taskContext != null && !string.IsNullOrWhiteSpace(taskContext.ActiveTaskTitle))
        {
            builder.Append("\n当前委托：").Append(taskContext.ActiveTaskTitle);
        }

        builder.Append("\n修士名号：").Append(saveData.heroName);
        return builder.ToString();
    }

    private static string BuildNpcStatus(CultivationSaveData saveData, NpcDefinition definition, SaveNpcState state, TaskContextSnapshot taskContext, string regionId)
    {
        if (definition != null && definition.RuntimeNpc != null)
        {
            var runtimeBuilder = new StringBuilder();
            runtimeBuilder.Append("状态：").Append(GetAffinityLabel(state != null ? state.affinity : 0));
            runtimeBuilder.Append("    交谈 ").Append(state != null ? state.interactionCount : 0).Append(" 次");
            runtimeBuilder.Append("\n派系：").Append(definition.RuntimeNpc.factionName);
            runtimeBuilder.Append("\n修为：").Append(WorldRegionLibrary.GetRealmName(definition.RuntimeNpc.realmTier));
            if (definition.RuntimeLocation != null)
            {
                runtimeBuilder.Append("\n常驻：").Append(definition.RuntimeLocation.displayName);
            }

            if (definition.RuntimeIncident != null)
            {
                runtimeBuilder.Append("\n在手风闻：").Append(definition.RuntimeIncident.displayTitle);
            }

            return runtimeBuilder.ToString();
        }

        var builder = new StringBuilder();
        builder.Append("状态：").Append(GetAffinityLabel(state != null ? state.affinity : 0));
        builder.Append("    交谈 ").Append(state != null ? state.interactionCount : 0).Append(" 次");
        if (!string.IsNullOrWhiteSpace(regionId))
        {
            builder.Append("\n关注地界：").Append(WorldRegionLibrary.GetRegionDisplayName(regionId));
        }

        if (taskContext != null && !string.IsNullOrWhiteSpace(taskContext.ActiveTaskTitle))
        {
            builder.Append("\n挂钩委托：").Append(taskContext.ActiveTaskTitle);
        }

        if (definition != null && definition.RoleType == NpcRoleType.TaskContact)
        {
            builder.Append("\n该人物可直接补强一次当前主委托进度。");
        }

        return builder.ToString();
    }

    private static string GetRoleLabel(NpcRoleType roleType)
    {
        switch (roleType)
        {
            case NpcRoleType.Steward:
                return "执事";
            case NpcRoleType.Mentor:
                return "前辈";
            case NpcRoleType.Rival:
                return "同门";
            case NpcRoleType.Merchant:
                return "行商";
            case NpcRoleType.Healer:
                return "药师";
            case NpcRoleType.Scout:
                return "斥候";
            case NpcRoleType.TaskContact:
                return "线人";
            default:
                return "隐士";
        }
    }

    private static string GetAffinityLabel(int affinity)
    {
        if (affinity >= 6)
        {
            return "相熟";
        }

        if (affinity >= 3)
        {
            return "可信";
        }

        if (affinity >= 1)
        {
            return "略有来往";
        }

        return "初识";
    }

    private static NpcDefinition[] BuildBaseDefinitions()
    {
        return new[]
        {
            new NpcDefinition
            {
                Id = "sect_steward_he",
                DisplayName = "何执事",
                Subtitle = "勤功殿执笔",
                Description = "负责山门委托、卷宗归档与功绩核验，最擅长从一堆案牍里指出真正有用的那几页。",
                RoleType = NpcRoleType.Steward,
                SceneType = NpcSceneType.SectResidence,
                SectHallId = "duty_hall",
                AccentColor = new Color(0.26f, 0.19f, 0.12f, 1f),
                Choices = new[]
                {
                    new NpcChoiceDefinition { Id = "sect_steward_hint", Label = "核对委托线索", Description = "对接一次主委托进度，获得更明确的目标提示。" }
                }
            },
            new NpcDefinition
            {
                Id = "sect_mentor_qing",
                DisplayName = "清岚长老",
                Subtitle = "藏经阁值守",
                Description = "常年看守经阁，讲话不多，但每次开口都能切到修行中的关键处。",
                RoleType = NpcRoleType.Mentor,
                SceneType = NpcSceneType.SectResidence,
                SectHallId = "scripture_hall",
                AccentColor = new Color(0.18f, 0.18f, 0.28f, 1f),
                Choices = new[]
                {
                    new NpcChoiceDefinition { Id = "sect_mentor_guidance", Label = "请教修行关窍", Description = "若已配置 DSU 对话则优先进入对话树，否则按当前境界领取一次点拨与修为增益。", ConversationTitle = "清岚长老" }
                }
            },
            new NpcDefinition
            {
                Id = "sect_rival_pei",
                DisplayName = "裴惊鸿",
                Subtitle = "演武坪同门",
                Description = "嘴硬心直的同门，最喜欢拿实战和你抬杠，也最愿意用输赢来衡量彼此进步。",
                RoleType = NpcRoleType.Rival,
                SceneType = NpcSceneType.SectResidence,
                SectHallId = "cave_residence",
                AccentColor = new Color(0.3f, 0.15f, 0.12f, 1f),
                Choices = new[]
                {
                    new NpcChoiceDefinition { Id = "sect_rival_spar", Label = "约一场切磋", Description = "每日一次，换取少量修为与灵石。" }
                }
            },
            new NpcDefinition
            {
                Id = "settlement_merchant_qiu",
                DisplayName = "邱行商",
                Subtitle = "外路货商",
                Description = "消息和货一样都爱周转，愿意给熟客一点真正有用的口风。",
                RoleType = NpcRoleType.Merchant,
                SceneType = NpcSceneType.Settlement,
                AccentColor = new Color(0.28f, 0.21f, 0.12f, 1f),
                Choices = new[]
                {
                    new NpcChoiceDefinition { Id = "settlement_merchant_news", Label = "打听外路风声", Description = "获得当前地界的情报，并可能推进关联委托。" },
                    new NpcChoiceDefinition { Id = "settlement_merchant_supply", Label = "收一份顺手补给", Description = "每日一次，领取少量灵砂与灵石。" }
                }
            },
            new NpcDefinition
            {
                Id = "settlement_healer_lan",
                DisplayName = "蓝药娘",
                Subtitle = "药铺坐堂",
                Description = "对草药、妖毒和心神磨损都很敏感，说话轻，但判断很准。",
                RoleType = NpcRoleType.Healer,
                SceneType = NpcSceneType.Settlement,
                AccentColor = new Color(0.16f, 0.24f, 0.18f, 1f),
                Choices = new[]
                {
                    new NpcChoiceDefinition { Id = "settlement_healer_tonic", Label = "讨一味安神药引", Description = "每日一次，领取清心香灰。" },
                    new NpcChoiceDefinition { Id = "settlement_healer_consult", Label = "请她看斗法后患", Description = "补充一条人物经历记录。" }
                }
            }
        };
    }
}
