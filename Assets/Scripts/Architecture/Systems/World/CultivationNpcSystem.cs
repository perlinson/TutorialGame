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
}

public sealed class CultivationNpcSystem : AbstractSystem
{
    private CultivationSaveSystem saveSystem;
    private CultivationTaskSystem taskSystem;
    private CultivationStorySystem storySystem;

    private static readonly NpcDefinition[] BaseDefinitions = BuildBaseDefinitions();

    protected override void OnInit()
    {
        saveSystem = this.GetSystem<CultivationSaveSystem>();
        taskSystem = this.GetSystem<CultivationTaskSystem>();
        storySystem = this.GetSystem<CultivationStorySystem>();
    }

    public WorldMapNpcDialogueSnapshot BuildDialogueSnapshot(
        MainMenuSaveData saveData,
        NpcSceneType sceneType,
        string regionId,
        string sectHallId,
        string selectedNpcId)
    {
        saveData = EnsureSaveData(saveData);
        var definitions = BuildNpcRoster(saveData, sceneType, regionId, sectHallId);
        var storySummary = storySystem.BuildStorySummary(saveData);
        var taskContext = taskSystem.GetActiveTaskContext(saveData);
        var resolvedDefinition = ResolveSelectedDefinition(definitions, selectedNpcId);
        var entries = BuildEntrySnapshots(saveData, definitions, resolvedDefinition);

        if (resolvedDefinition == null)
        {
            return new WorldMapNpcDialogueSnapshot
            {
                PanelTitle = BuildPanelTitle(sceneType),
                PanelSubtitle = BuildPanelSubtitle(sceneType, regionId, sectHallId),
                StorySummary = storySummary,
                TaskSummary = taskContext != null ? taskContext.ActiveTaskSummary : "委托：暂无。",
                NpcTitle = "暂无可交谈对象",
                NpcSubtitle = "人物 / 对话 / 线索",
                NpcDescription = "当前场景没有可交谈的 NPC。",
                NpcStatus = "状态：无可用对话。",
                Preview = new WorldMapPreviewSnapshot
                {
                    Label = "人物卷宗",
                    PlaceholderColor = new Color(0.18f, 0.15f, 0.12f, 1f)
                },
                Entries = entries,
                Choices = new WorldMapNpcChoiceSnapshot[0],
                SelectedNpcId = string.Empty
            };
        }

        var npcState = saveData.GetOrCreateNpcState(resolvedDefinition.Id);
        var status = BuildNpcStatus(saveData, resolvedDefinition, npcState, taskContext, regionId);
        return new WorldMapNpcDialogueSnapshot
        {
            PanelTitle = BuildPanelTitle(sceneType),
            PanelSubtitle = BuildPanelSubtitle(sceneType, regionId, sectHallId),
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
            Choices = BuildChoiceSnapshots(saveData, resolvedDefinition, npcState, taskContext, regionId),
            SelectedNpcId = resolvedDefinition.Id
        };
    }

    public NpcInteractionResult ExecuteChoice(
        int slotIndex,
        MainMenuSaveData saveData,
        NpcSceneType sceneType,
        string regionId,
        string sectHallId,
        string npcId,
        string choiceId)
    {
        saveData = EnsureSaveData(saveData);
        var definitions = BuildNpcRoster(saveData, sceneType, regionId, sectHallId);
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

        var messages = new List<string>();
        var result = ApplyChoiceEffect(saveData, definition, npcState, choice, taskContext, regionId, messages);
        CultivationGameTime.Advance(saveData, 1);
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

    private static MainMenuSaveData EnsureSaveData(MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            saveData = new MainMenuSaveData();
        }

        saveData.EnsureDefaults();
        return saveData;
    }

    private List<NpcDefinition> BuildNpcRoster(MainMenuSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId)
    {
        var list = new List<NpcDefinition>();
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

        if (sceneType == NpcSceneType.Region)
        {
            AppendRegionDefinitions(saveData, regionId, list);
        }

        return list;
    }

    private void AppendRegionDefinitions(MainMenuSaveData saveData, string regionId, List<NpcDefinition> list)
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

    private WorldMapNpcEntrySnapshot[] BuildEntrySnapshots(MainMenuSaveData saveData, List<NpcDefinition> definitions, NpcDefinition selected)
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
        MainMenuSaveData saveData,
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

    private bool IsChoiceAvailable(
        MainMenuSaveData saveData,
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
            default:
                return true;
        }
    }

    private string ApplyChoiceEffect(
        MainMenuSaveData saveData,
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
                saveData.spiritCrystals += 1;
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
                saveData.spiritCrystals += 1;
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
            default:
                npcState.affinity += 1;
                return "对话结束。";
        }
    }

    private void AppendConditionalTaskProgress(MainMenuSaveData saveData, TaskContextSnapshot taskContext, string regionId, List<string> messages)
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

    private void AppendTaskProgress(MainMenuSaveData saveData, int count, List<string> messages)
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

    private void AppendStory(MainMenuSaveData saveData, string storyId, string nodeId, string resultText, List<string> messages)
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

    private static void TryGrantItem(MainMenuSaveData saveData, string itemId, int quantity, List<string> messages)
    {
        if (!saveData.TryAddItem(itemId, quantity))
        {
            messages.Add("储物袋已满，未能收下 " + InventoryLibrary.GetDisplayName(itemId) + "。");
            return;
        }

        messages.Add("获得：" + InventoryLibrary.GetDisplayName(itemId) + " x" + quantity + "。");
    }

    private static string BuildPanelTitle(NpcSceneType sceneType)
    {
        switch (sceneType)
        {
            case NpcSceneType.SectResidence:
                return "门派人物与对话";
            case NpcSceneType.Region:
                return "地域人物与线索";
            default:
                return "坊市人物与对话";
        }
    }

    private static string BuildPanelSubtitle(NpcSceneType sceneType, string regionId, string sectHallId)
    {
        switch (sceneType)
        {
            case NpcSceneType.SectResidence:
                return string.IsNullOrWhiteSpace(sectHallId) ? "同门 / 执事 / 指点" : "当前殿堂 / 同门 / 事务对接";
            case NpcSceneType.Region:
                return WorldRegionLibrary.GetRegionDisplayName(regionId) + " / 线索 / 接头";
            default:
                return "坊市 / 行商 / 药师 / 风闻";
        }
    }

    private static string BuildNpcDescription(MainMenuSaveData saveData, NpcDefinition definition, TaskContextSnapshot taskContext, string regionId)
    {
        if (definition == null)
        {
            return string.Empty;
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

    private static string BuildNpcStatus(MainMenuSaveData saveData, NpcDefinition definition, SaveNpcState state, TaskContextSnapshot taskContext, string regionId)
    {
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
                    new NpcChoiceDefinition { Id = "sect_mentor_guidance", Label = "请教修行关窍", Description = "按当前境界领取一次点拨与修为增益。" }
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
