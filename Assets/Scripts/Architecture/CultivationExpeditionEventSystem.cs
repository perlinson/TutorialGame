using System.Collections.Generic;
using UnityEngine;
using QFramework;

public sealed class CultivationExpeditionEventSystem : AbstractSystem
{
    private CultivationTaskSystem taskSystem;
    private CultivationConditionSystem conditionSystem;
    private CultivationRewardSystem rewardSystem;
    private ExpeditionEventDefinition[] cachedEvents;

    protected override void OnInit()
    {
        taskSystem = this.GetSystem<CultivationTaskSystem>();
        conditionSystem = this.GetSystem<CultivationConditionSystem>();
        rewardSystem = this.GetSystem<CultivationRewardSystem>();
    }

    public ExpeditionEventCardResult OpenRoomEvent(CombatTurnContext context)
    {
        if (context == null || context.Region == null || context.Room == null || context.Hero == null || context.SaveData == null)
        {
            return new ExpeditionEventCardResult
            {
                FailureReason = "当前无法展开历练事件。"
            };
        }

        var taskContext = taskSystem.GetActiveTaskContext(context.SaveData);
        var definition = PickEventDefinition(context, taskContext);
        if (definition == null)
        {
            return new ExpeditionEventCardResult
            {
                FailureReason = "当前没有可用的历练事件。"
            };
        }

        return BuildCardResult(definition, context, taskContext);
    }

    public ExpeditionEventOptionResult ResolveEventOption(CombatTurnContext context, string eventId, string optionId)
    {
        if (context == null || context.Region == null || context.Room == null || context.Hero == null || context.SaveData == null)
        {
            return new ExpeditionEventOptionResult
            {
                FailureReason = "当前无法结算历练事件。"
            };
        }

        ExpeditionEventDefinition definition;
        if (!TryGetEventDefinition(eventId, out definition))
        {
            return new ExpeditionEventOptionResult
            {
                FailureReason = "未能找到对应的历练事件。"
            };
        }

        var taskContext = taskSystem.GetActiveTaskContext(context.SaveData);
        if (!IsEventEligible(definition, context, taskContext))
        {
            return new ExpeditionEventOptionResult
            {
                FailureReason = "当前条件下无法处理该事件。"
            };
        }

        ExpeditionEventOptionDefinition option = null;
        if (definition.Options != null)
        {
            for (var i = 0; i < definition.Options.Length; i++)
            {
                if (definition.Options[i] != null && definition.Options[i].Id == optionId)
                {
                    option = definition.Options[i];
                    break;
                }
            }
        }

        if (option == null)
        {
            return new ExpeditionEventOptionResult
            {
                FailureReason = "未能找到对应的事件选项。"
            };
        }

        var requirementText = BuildRequirementText(option, context, taskContext);
        if (!AreConditionsMet(option.Conditions, context, taskContext))
        {
            return new ExpeditionEventOptionResult
            {
                FailureReason = string.IsNullOrWhiteSpace(requirementText) ? "当前还无法执行这个选择。" : requirementText
            };
        }

        var result = new ExpeditionEventOptionResult
        {
            Torchlight = context.Torchlight,
            Supplies = context.Supplies,
            PendingQiGain = context.PendingQiGain,
            PendingCrystalGain = context.PendingCrystalGain,
            RoomResolved = true,
            ResultTitle = definition.Title,
            ResultBadgeText = string.IsNullOrWhiteSpace(option.BadgeText) ? definition.BadgeText : option.BadgeText
        };

        ApplyEffects(option.Effects, context, taskContext, result);
        var taskProgress = ApplyTaskSignals(option.TaskProgressSignals, context);

        if (!string.IsNullOrWhiteSpace(taskContext.ActiveTaskId))
        {
            taskSystem.MarkTriggeredEvent(context.SaveData, taskContext.ActiveTaskId, definition.Id);
            taskSystem.MarkChosenOption(context.SaveData, taskContext.ActiveTaskId, option.Id);
        }

        context.Room.Resolved = true;
        result.ResultBody = CombinePrimaryAndNotes(option.ResultText, result.LogMessage);
        if (taskProgress != null && !string.IsNullOrWhiteSpace(taskProgress.Message))
        {
            result.ResultBody = CombinePrimaryAndNotes(result.ResultBody, taskProgress.Message);
        }

        var storyResult = CultivationApp.RecordStorySignal(context.SaveData, new StorySignal
        {
            StoryId = definition.Id,
            NodeId = option.Id,
            Title = definition.Title,
            ResultText = !string.IsNullOrWhiteSpace(option.BadgeText) ? "经历已记录：" + definition.Title + " / " + option.BadgeText + "。" : string.Empty
        });
        if (storyResult != null && !string.IsNullOrWhiteSpace(storyResult.Message))
        {
            result.ResultBody = CombinePrimaryAndNotes(result.ResultBody, storyResult.Message);
        }

        result.LogMessage = result.ResultBody;
        if (result.ExpeditionFailed)
        {
            result.HintMessage = "远征队已无法继续前行。";
        }
        else if (taskProgress != null && taskProgress.CompletedNow)
        {
            result.HintMessage = "主委托已经达成，可以继续深入，也可以回去结算。";
        }
        else if (taskProgress != null && taskProgress.ProgressChanged)
        {
            result.HintMessage = "主委托进度已更新，后续房间的事件池可能继续变化。";
        }
        else if (!string.IsNullOrWhiteSpace(taskContext.ActiveTaskId) && option.IsTaskOption)
        {
            result.HintMessage = "任务路线已经推进，后续房间的遭遇可能发生变化。";
        }
        else
        {
            result.HintMessage = "事件已经处理完毕，可以继续向更深处推进。";
        }

        return result;
    }

    private ExpeditionEventCardResult BuildCardResult(ExpeditionEventDefinition definition, CombatTurnContext context, TaskContextSnapshot taskContext)
    {
        var options = definition.Options != null ? new ExpeditionEventOptionPresentation[definition.Options.Length] : new ExpeditionEventOptionPresentation[0];
        for (var i = 0; i < options.Length; i++)
        {
            var option = definition.Options[i];
            options[i] = new ExpeditionEventOptionPresentation
            {
                OptionId = option.Id,
                Label = option.Label,
                RequirementText = BuildRequirementText(option, context, taskContext),
                BadgeText = option.BadgeText,
                IsAvailable = AreConditionsMet(option.Conditions, context, taskContext)
            };
        }

        return new ExpeditionEventCardResult
        {
            EventId = definition.Id,
            Title = definition.Title,
            Body = definition.Body,
            BadgeText = definition.BadgeText,
            IllustrationImage = definition.IllustrationImage != null ? definition.IllustrationImage : context.Room.IllustrationImage,
            Options = options
        };
    }

    private ExpeditionEventDefinition PickEventDefinition(CombatTurnContext context, TaskContextSnapshot taskContext)
    {
        var definitions = GetDefinitions();
        var candidates = new List<ExpeditionEventDefinition>();
        var totalWeight = 0;
        for (var i = 0; i < definitions.Length; i++)
        {
            var definition = definitions[i];
            if (!IsEventEligible(definition, context, taskContext))
            {
                continue;
            }

            candidates.Add(definition);
            totalWeight += Mathf.Max(1, definition.Weight);
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        var seed = context.Room.Seed * 31 + context.CurrentRoomIndex * 17 + SafeHash(taskContext.ActiveTaskId) + context.PendingQiGain * 7 + context.PendingCrystalGain * 13;
        var randomSource = new System.Random(seed);
        var roll = randomSource.Next(0, Mathf.Max(1, totalWeight));
        var accumulated = 0;
        for (var i = 0; i < candidates.Count; i++)
        {
            accumulated += Mathf.Max(1, candidates[i].Weight);
            if (roll < accumulated)
            {
                return candidates[i];
            }
        }

        return candidates[0];
    }

    private bool IsEventEligible(ExpeditionEventDefinition definition, CombatTurnContext context, TaskContextSnapshot taskContext)
    {
        if (definition == null || context == null || context.Room == null || context.Region == null)
        {
            return false;
        }

        if (!MatchesRoomKind(definition.EligibleRoomKinds, context.Room.Kind) || !MatchesRegion(definition.EligibleRegionIds, context.Region.Id))
        {
            return false;
        }

        if (!AreConditionsMet(definition.Conditions, context, taskContext))
        {
            return false;
        }

        switch (definition.CardType)
        {
            case ExpeditionEventCardType.Generic:
                if (HasSuppressedTag(definition, taskContext))
                {
                    return false;
                }

                break;
            case ExpeditionEventCardType.TaskInjected:
                if (taskContext == null || string.IsNullOrWhiteSpace(taskContext.ActiveTaskId) || !Contains(taskContext.InjectEventIds, definition.Id))
                {
                    return false;
                }

                break;
            case ExpeditionEventCardType.TaskExclusive:
                if (taskContext == null || string.IsNullOrWhiteSpace(taskContext.ActiveTaskId) || definition.RequiredTaskId != taskContext.ActiveTaskId)
                {
                    return false;
                }

                break;
        }

        if (!string.IsNullOrWhiteSpace(definition.RequiredTaskId) && (taskContext == null || taskContext.ActiveTaskId != definition.RequiredTaskId))
        {
            return false;
        }

        return true;
    }

    private bool AreConditionsMet(EventCondition[] conditions, CombatTurnContext context, TaskContextSnapshot taskContext)
    {
        return conditionSystem.AreEventConditionsMet(conditions, context, taskContext);
    }

    private string BuildRequirementText(ExpeditionEventOptionDefinition option, CombatTurnContext context, TaskContextSnapshot taskContext)
    {
        if (option == null)
        {
            return string.Empty;
        }

        return conditionSystem.BuildRequirementText(option.Conditions, context, taskContext, option.RequirementText);
    }

    private void ApplyEffects(EventEffect[] effects, CombatTurnContext context, TaskContextSnapshot taskContext, ExpeditionEventOptionResult result)
    {
        if (effects == null)
        {
            return;
        }

        for (var i = 0; i < effects.Length; i++)
        {
            var effect = effects[i];
            if (effect == null)
            {
                continue;
            }

            switch (effect.Type)
            {
                case EventEffectType.GainPendingQi:
                    result.PendingQiGain += effect.IntValue;
                    break;
                case EventEffectType.GainPendingCrystals:
                    result.PendingCrystalGain += effect.IntValue;
                    break;
                case EventEffectType.ModifyTorchlight:
                    result.Torchlight = Mathf.Clamp(result.Torchlight + effect.IntValue, 0, 100);
                    break;
                case EventEffectType.ModifySupplies:
                    result.Supplies = Mathf.Max(0, result.Supplies + effect.IntValue);
                    break;
                case EventEffectType.HealHero:
                    context.Hero.CurrentHealth = Mathf.Min(context.Hero.MaxHealth, context.Hero.CurrentHealth + Mathf.Max(0, effect.IntValue));
                    break;
                case EventEffectType.ModifyStress:
                    ApplyStress(context, result, effect.IntValue);
                    break;
                case EventEffectType.ReceiveDamage:
                    ReceiveDamage(context, result, effect.IntValue);
                    break;
                case EventEffectType.AddPendingItem:
                    rewardSystem.AddPendingItem(context.PendingItemRewards, effect.StringValue, Mathf.Max(1, effect.IntValue));
                    break;
                case EventEffectType.AddTaskFlag:
                    if (taskContext != null && !string.IsNullOrWhiteSpace(taskContext.ActiveTaskId))
                    {
                        taskSystem.AddTaskFlag(context.SaveData, taskContext.ActiveTaskId, effect.StringValue);
                    }

                    break;
                case EventEffectType.AddTaskProgress:
                    taskSystem.RecordProgress(context.SaveData, new TaskProgressSignal
                    {
                        Type = TaskProgressSignalType.AddProgressToActiveTask,
                        Count = Mathf.Max(1, effect.IntValue)
                    });
                    break;
            }

            if (context.Hero.CurrentHealth <= 0)
            {
                result.ExpeditionFailed = true;
                result.FailureReason = "远征队在 " + context.Region.DisplayName + " 深处彻底溃散。";
                return;
            }
        }
    }

    private TaskProgressResult ApplyTaskSignals(TaskProgressSignal[] signals, CombatTurnContext context)
    {
        if (signals == null)
        {
            return null;
        }

        TaskProgressResult aggregate = null;
        for (var i = 0; i < signals.Length; i++)
        {
            var signal = signals[i];
            if (signal == null)
            {
                continue;
            }

            var runtimeSignal = new TaskProgressSignal
            {
                Type = signal.Type,
                Count = signal.Count,
                StringValue = signal.StringValue,
                FactionValue = signal.FactionValue
            };

            if (runtimeSignal.Type == TaskProgressSignalType.ClearRegion && string.IsNullOrWhiteSpace(runtimeSignal.StringValue))
            {
                runtimeSignal.StringValue = context.Region.Id;
            }

            var progress = taskSystem.RecordProgress(context.SaveData, runtimeSignal);
            if (progress != null && progress.ProgressChanged)
            {
                aggregate = progress;
            }
        }

        return aggregate;
    }

    private ExpeditionEventDefinition[] GetDefinitions()
    {
        if (cachedEvents != null)
        {
            return cachedEvents;
        }

        var database = CultivationApp.LoadResource<ExpeditionEventDatabaseAsset>("Data/ExpeditionEventDatabase");
        if (database != null && database.events != null && database.events.Length > 0)
        {
            cachedEvents = database.events;
            return cachedEvents;
        }

        cachedEvents = BuildFallbackEvents();
        return cachedEvents;
    }

    private bool TryGetEventDefinition(string eventId, out ExpeditionEventDefinition definition)
    {
        var definitions = GetDefinitions();
        for (var i = 0; i < definitions.Length; i++)
        {
            if (definitions[i] != null && definitions[i].Id == eventId)
            {
                definition = definitions[i];
                return true;
            }
        }

        definition = null;
        return false;
    }

    private static ExpeditionEventDefinition[] BuildFallbackEvents()
    {
        return new[]
        {
            new ExpeditionEventDefinition
            {
                Id = "event_generic_scout_map",
                Title = "测脉旧图",
                Body = "石壁裂缝里卡着一卷旧图，似乎是前人标注的退路与灵脉节点。",
                BadgeText = "普通历练",
                Tags = new[] { "generic_scout" },
                CardType = ExpeditionEventCardType.Generic,
                EligibleRoomKinds = new[] { ExpeditionRoomKind.Scout },
                Weight = 3,
                Options = new[]
                {
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "scout_study_map",
                        Label = "细查地脉",
                        ResultText = "你借着残图重新校正地脉，心神更稳，也为后续推进积累了些修为。",
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.GainPendingQi, IntValue = 2 },
                            new EventEffect { Type = EventEffectType.ModifyTorchlight, IntValue = 6 }
                        }
                    },
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "scout_follow_marks",
                        Label = "沿着旧标搜刮",
                        ResultText = "你顺着旧标翻检了几处缝隙，找回些散碎灵石，但也消耗了不少心力。",
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.GainPendingCrystals, IntValue = 2 },
                            new EventEffect { Type = EventEffectType.ModifyStress, IntValue = 4 }
                        }
                    }
                }
            },
            new ExpeditionEventDefinition
            {
                Id = "event_generic_treasure_pack",
                Title = "散落行囊",
                Body = "一只破损行囊被半埋在碎石间，像是仓促撤离时遗落的补给。",
                BadgeText = "普通历练",
                Tags = new[] { "generic_treasure" },
                CardType = ExpeditionEventCardType.Generic,
                EligibleRoomKinds = new[] { ExpeditionRoomKind.Treasure },
                Weight = 3,
                Options = new[]
                {
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "treasure_rummage",
                        Label = "翻检行囊",
                        ResultText = "你翻出几块还能用的灵石，行囊里残留的血迹却让心境有些发紧。",
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.GainPendingCrystals, IntValue = 2 },
                            new EventEffect { Type = EventEffectType.ModifyStress, IntValue = 4 }
                        }
                    },
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "treasure_hidden_slot",
                        Label = "拆开暗格",
                        ResultText = "暗格里藏着一小袋灵材，你花了些时间才把它完整拆下。",
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.AddPendingItem, StringValue = "green_spirit_sand", IntValue = 2 },
                            new EventEffect { Type = EventEffectType.ModifyTorchlight, IntValue = -4 }
                        }
                    }
                }
            },
            new ExpeditionEventDefinition
            {
                Id = "event_generic_herb_patch",
                Title = "灵草湿地",
                Body = "潮湿的裂地边生着几株药性温和的灵草，枝叶间还残留着薄雾灵气。",
                BadgeText = "普通历练",
                Tags = new[] { "generic_herb" },
                CardType = ExpeditionEventCardType.Generic,
                EligibleRoomKinds = new[] { ExpeditionRoomKind.Herb },
                Weight = 3,
                Options = new[]
                {
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "herb_pick",
                        Label = "采下灵草",
                        ResultText = "你迅速采下可用的灵草，一部分立刻入药，另一部分收入行囊。",
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.HealHero, IntValue = 4 },
                            new EventEffect { Type = EventEffectType.GainPendingQi, IntValue = 1 },
                            new EventEffect { Type = EventEffectType.AddPendingItem, StringValue = "mist_mushroom", IntValue = 1 }
                        }
                    },
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "herb_uproot",
                        Label = "连根移走",
                        ResultText = "你连根取走了更完整的药材，但粗暴的采法也让心神有些浮动。",
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.AddPendingItem, StringValue = "mind_cleansing_incense", IntValue = 1 },
                            new EventEffect { Type = EventEffectType.ModifyStress, IntValue = 2 }
                        }
                    }
                }
            },
            new ExpeditionEventDefinition
            {
                Id = "event_generic_shrine_rest",
                Title = "残阵祭台",
                Body = "祭台的残阵仍有微弱回响，只是稍一碰触，便能感到其中杂乱的旧念。",
                BadgeText = "普通历练",
                Tags = new[] { "generic_shrine" },
                CardType = ExpeditionEventCardType.Generic,
                EligibleRoomKinds = new[] { ExpeditionRoomKind.Shrine },
                Weight = 3,
                Options = new[]
                {
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "shrine_meditate",
                        Label = "借阵调息",
                        ResultText = "你顺着残阵余势调息了片刻，火光更稳，心境也沉了下来。",
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.GainPendingQi, IntValue = 2 },
                            new EventEffect { Type = EventEffectType.ModifyTorchlight, IntValue = 10 },
                            new EventEffect { Type = EventEffectType.ModifyStress, IntValue = -10 }
                        }
                    },
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "shrine_pry_stone",
                        Label = "抠走阵石",
                        ResultText = "你强行撬下几块阵石，换来些灵石和残片，但杂念随之翻涌。",
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.GainPendingCrystals, IntValue = 2 },
                            new EventEffect { Type = EventEffectType.AddPendingItem, StringValue = "array_shard", IntValue = 1 },
                            new EventEffect { Type = EventEffectType.ModifyStress, IntValue = 6 }
                        }
                    }
                }
            },
            new ExpeditionEventDefinition
            {
                Id = "event_generic_trap_miasma",
                Title = "隐伏瘴陷",
                Body = "脚下阵纹忽明忽暗，旧瘴与碎石交织成一道难看的险口。",
                BadgeText = "普通历练",
                Tags = new[] { "generic_trap" },
                CardType = ExpeditionEventCardType.Generic,
                EligibleRoomKinds = new[] { ExpeditionRoomKind.Trap },
                Weight = 3,
                Options = new[]
                {
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "trap_force_cross",
                        Label = "强行跨过",
                        ResultText = "你硬顶着瘴气冲了过去，虽然伤了些气血，却也顺手捞回几块散碎灵石。",
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.ReceiveDamage, IntValue = 3 },
                            new EventEffect { Type = EventEffectType.GainPendingCrystals, IntValue = 1 }
                        }
                    },
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "trap_dismantle",
                        Label = "拆阵取材",
                        RequirementText = "至少需要 1 份补给。",
                        ResultText = "你耗掉一份补给稳定住阵脚，拆下可用残片后才慢慢退开。",
                        Conditions = new[]
                        {
                            new EventCondition { Type = EventConditionType.SuppliesAtLeast, IntValue = 1 }
                        },
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.ModifySupplies, IntValue = -1 },
                            new EventEffect { Type = EventEffectType.AddPendingItem, StringValue = "array_shard", IntValue = 1 },
                            new EventEffect { Type = EventEffectType.ModifyStress, IntValue = 4 }
                        }
                    }
                }
            },
            new ExpeditionEventDefinition
            {
                Id = "event_bandit_hidden_scout",
                Title = "山贼暗哨",
                Body = "乱石后藏着一处新近启用的暗哨，脚印一路延向更深的山道。",
                BadgeText = "委托线索",
                Tags = new[] { "bandit_task" },
                CardType = ExpeditionEventCardType.TaskInjected,
                RequiredTaskId = "task_bandit_route",
                EligibleRoomKinds = new[] { ExpeditionRoomKind.Scout, ExpeditionRoomKind.Treasure },
                Weight = 5,
                Options = new[]
                {
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "bandit_loot_watchpost",
                        Label = "搜刮暗哨包袱",
                        ResultText = "你从暗哨遗留的包袱里翻出些散碎灵石，但更深的线索也因此断了。",
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.GainPendingCrystals, IntValue = 2 }
                        }
                    },
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "bandit_follow_trail",
                        Label = "顺着脚印追查匪寨",
                        BadgeText = "任务选项",
                        ResultText = "你顺着暗号和脚印摸清了流寇接头路线，还顺手拿到一枚可用的路引。",
                        IsTaskOption = true,
                        Conditions = new[]
                        {
                            new EventCondition { Type = EventConditionType.ActiveTaskIs, StringValue = "task_bandit_route" }
                        },
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.AddTaskFlag, StringValue = "bandit_trail_found" },
                            new EventEffect { Type = EventEffectType.AddPendingItem, StringValue = "bandit_route_token", IntValue = 1 },
                            new EventEffect { Type = EventEffectType.GainPendingQi, IntValue = 1 }
                        },
                        TaskProgressSignals = new[]
                        {
                            new TaskProgressSignal { Type = TaskProgressSignalType.ObtainTaskEvidence, StringValue = "bandit_route_token", Count = 1 }
                        }
                    },
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "bandit_fake_contact",
                        Label = "凭路引混入接头点",
                        RequirementText = "需要携带匪寨路引。",
                        BadgeText = "证物可用",
                        ResultText = "你借着路引和口令逼近接头点，截下了第二枚路引，也打乱了山贼的传信线。",
                        IsTaskOption = true,
                        Conditions = new[]
                        {
                            new EventCondition { Type = EventConditionType.ActiveTaskIs, StringValue = "task_bandit_route" },
                            new EventCondition { Type = EventConditionType.HasTaskFlag, StringValue = "bandit_trail_found" },
                            new EventCondition { Type = EventConditionType.HasItem, StringValue = "bandit_route_token", IntValue = 1 }
                        },
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.AddTaskFlag, StringValue = "bandit_contact_broken" },
                            new EventEffect { Type = EventEffectType.AddPendingItem, StringValue = "bandit_route_token", IntValue = 1 },
                            new EventEffect { Type = EventEffectType.GainPendingCrystals, IntValue = 3 }
                        },
                        TaskProgressSignals = new[]
                        {
                            new TaskProgressSignal { Type = TaskProgressSignalType.ObtainTaskEvidence, StringValue = "bandit_route_token", Count = 1 },
                            new TaskProgressSignal { Type = TaskProgressSignalType.ChooseEventOption, StringValue = "bandit_fake_contact", Count = 1 }
                        }
                    }
                }
            },
            new ExpeditionEventDefinition
            {
                Id = "event_bandit_messenger",
                Title = "截获信使",
                Body = "一名替山道流寇传讯的信使正从岔道掠过，腰间挂着熟悉的匪寨路引。",
                BadgeText = "委托线索",
                Tags = new[] { "bandit_task", "bandit_messenger" },
                CardType = ExpeditionEventCardType.TaskExclusive,
                RequiredTaskId = "task_bandit_route",
                EligibleRoomKinds = new[] { ExpeditionRoomKind.Treasure, ExpeditionRoomKind.Trap },
                Weight = 6,
                Conditions = new[]
                {
                    new EventCondition { Type = EventConditionType.HasItem, StringValue = "bandit_route_token", IntValue = 1 },
                    new EventCondition { Type = EventConditionType.HasChosenOption, StringValue = "bandit_fake_contact" }
                },
                Options = new[]
                {
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "bandit_cut_messenger",
                        Label = "截下传讯路引",
                        BadgeText = "任务选项",
                        ResultText = "你截住了信使，路引和暗记一并落入手中，山道流寇的藏身路线终于明朗。",
                        IsTaskOption = true,
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.AddPendingItem, StringValue = "bandit_route_token", IntValue = 1 },
                            new EventEffect { Type = EventEffectType.GainPendingQi, IntValue = 1 }
                        },
                        TaskProgressSignals = new[]
                        {
                            new TaskProgressSignal { Type = TaskProgressSignalType.ObtainTaskEvidence, StringValue = "bandit_route_token", Count = 1 }
                        }
                    }
                }
            },
            new ExpeditionEventDefinition
            {
                Id = "event_cult_blood_altar",
                Title = "血祭残坛",
                Body = "祭坛边的血痕还未彻底干透，四周阵纹像在往同一处地脉节点汇流。",
                BadgeText = "委托线索",
                Tags = new[] { "cult_task" },
                CardType = ExpeditionEventCardType.TaskInjected,
                RequiredTaskId = "task_valley_cultists",
                EligibleRoomKinds = new[] { ExpeditionRoomKind.Shrine, ExpeditionRoomKind.Trap },
                Weight = 5,
                Options = new[]
                {
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "cult_break_altar",
                        Label = "掰下祭坛灵石",
                        ResultText = "你拆下几块还算完整的祭石，却也让血煞余念顺势翻涌上来。",
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.GainPendingCrystals, IntValue = 2 },
                            new EventEffect { Type = EventEffectType.ModifyStress, IntValue = 6 }
                        }
                    },
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "cult_trace_pattern",
                        Label = "搜查邪修布阵痕迹",
                        BadgeText = "任务选项",
                        ResultText = "你在阵脚中找到了一页夺灵手札，顺带摸清了邪修转移阵纹的方向。",
                        IsTaskOption = true,
                        Conditions = new[]
                        {
                            new EventCondition { Type = EventConditionType.ActiveTaskIs, StringValue = "task_valley_cultists" }
                        },
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.AddTaskFlag, StringValue = "cult_trace_found" },
                            new EventEffect { Type = EventEffectType.AddPendingItem, StringValue = "evil_cult_notes", IntValue = 1 },
                            new EventEffect { Type = EventEffectType.GainPendingQi, IntValue = 1 }
                        },
                        TaskProgressSignals = new[]
                        {
                            new TaskProgressSignal { Type = TaskProgressSignalType.ObtainTaskEvidence, StringValue = "evil_cult_notes", Count = 1 }
                        }
                    },
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "cult_reverse_array",
                        Label = "比对手札逆追阵纹",
                        RequirementText = "需要携带夺灵手札。",
                        BadgeText = "证物可用",
                        ResultText = "你把手札上的阵纹与祭台残痕一一比对，顺利反推出下一处转移节点。",
                        IsTaskOption = true,
                        Conditions = new[]
                        {
                            new EventCondition { Type = EventConditionType.ActiveTaskIs, StringValue = "task_valley_cultists" },
                            new EventCondition { Type = EventConditionType.HasTaskFlag, StringValue = "cult_trace_found" },
                            new EventCondition { Type = EventConditionType.HasItem, StringValue = "evil_cult_notes", IntValue = 1 }
                        },
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.AddTaskFlag, StringValue = "cult_array_broken" },
                            new EventEffect { Type = EventEffectType.AddPendingItem, StringValue = "evil_cult_notes", IntValue = 1 },
                            new EventEffect { Type = EventEffectType.GainPendingCrystals, IntValue = 2 }
                        },
                        TaskProgressSignals = new[]
                        {
                            new TaskProgressSignal { Type = TaskProgressSignalType.ObtainTaskEvidence, StringValue = "evil_cult_notes", Count = 1 }
                        }
                    }
                }
            },
            new ExpeditionEventDefinition
            {
                Id = "event_heartdream_echo",
                Title = "心印残梦",
                Body = "碎裂的梦痕像潮水一样贴上神识，仿佛有人在黑暗中反复低语同一句残愿。",
                BadgeText = "委托线索",
                Tags = new[] { "heartdream_task" },
                CardType = ExpeditionEventCardType.TaskInjected,
                RequiredTaskId = "task_heart_echoes",
                EligibleRoomKinds = new[] { ExpeditionRoomKind.Shrine, ExpeditionRoomKind.Trap },
                Weight = 5,
                Options = new[]
                {
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "heartdream_endure",
                        Label = "强压幻梦",
                        ResultText = "你硬撑着压下幻梦，只留下些零碎梦痕，神识却被撕扯得更紧了。",
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.ModifyStress, IntValue = 8 },
                            new EventEffect { Type = EventEffectType.AddPendingItem, StringValue = "heart_mark_fragment", IntValue = 1 }
                        }
                    },
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "heartdream_trace",
                        Label = "循着残响追索源头",
                        BadgeText = "任务选项",
                        ResultText = "你没有急着斩断残梦，而是顺着回响摸清了第一处心印源头。",
                        IsTaskOption = true,
                        Conditions = new[]
                        {
                            new EventCondition { Type = EventConditionType.ActiveTaskIs, StringValue = "task_heart_echoes" }
                        },
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.AddTaskFlag, StringValue = "heartdream_first_echo" },
                            new EventEffect { Type = EventEffectType.ModifyStress, IntValue = 2 }
                        },
                        TaskProgressSignals = new[]
                        {
                            new TaskProgressSignal { Type = TaskProgressSignalType.ResolveEventTag, StringValue = "heartdream_clue", Count = 1 }
                        }
                    },
                    new ExpeditionEventOptionDefinition
                    {
                        Id = "heartdream_bind_mark",
                        Label = "借梦痕反封心印",
                        RequirementText = "需要携带心印残片。",
                        BadgeText = "证物可用",
                        ResultText = "你借着残片中残留的梦痕反向封住了心印回路，第二处线索也随之浮现。",
                        IsTaskOption = true,
                        Conditions = new[]
                        {
                            new EventCondition { Type = EventConditionType.ActiveTaskIs, StringValue = "task_heart_echoes" },
                            new EventCondition { Type = EventConditionType.HasTaskFlag, StringValue = "heartdream_first_echo" },
                            new EventCondition { Type = EventConditionType.HasItem, StringValue = "heart_mark_fragment", IntValue = 1 }
                        },
                        Effects = new[]
                        {
                            new EventEffect { Type = EventEffectType.GainPendingQi, IntValue = 2 },
                            new EventEffect { Type = EventEffectType.ModifyStress, IntValue = -6 }
                        },
                        TaskProgressSignals = new[]
                        {
                            new TaskProgressSignal { Type = TaskProgressSignalType.ResolveEventTag, StringValue = "heartdream_clue", Count = 1 }
                        }
                    }
                }
            }
        };
    }

    private static void ApplyStress(CombatTurnContext context, ExpeditionEventOptionResult result, int amount)
    {
        var mindResult = CultivationApp.ApplyMindStress(context, amount);
        if (mindResult.ExpeditionFailed)
        {
            result.ExpeditionFailed = true;
            result.FailureReason = mindResult.FailureReason;
            return;
        }

        if (!string.IsNullOrWhiteSpace(mindResult.Message) && mindResult.BreakdownTriggered)
        {
            result.LogMessage = string.IsNullOrWhiteSpace(result.LogMessage)
                ? mindResult.Message
                : result.LogMessage + "\n" + mindResult.Message;
        }
    }

    private static void ReceiveDamage(CombatTurnContext context, ExpeditionEventOptionResult result, int amount)
    {
        context.Hero.CurrentHealth = Mathf.Max(0, context.Hero.CurrentHealth - Mathf.Max(0, amount));
        if (context.Hero.CurrentHealth <= 0)
        {
            result.ExpeditionFailed = true;
            result.FailureReason = "远征队在 " + context.Region.DisplayName + " 深处彻底溃散。";
        }
    }

    private static void AddPendingItem(List<SaveItemStack> pendingItemRewards, string itemId, int quantity)
    {
        if (pendingItemRewards == null || string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
        {
            return;
        }

        for (var i = 0; i < pendingItemRewards.Count; i++)
        {
            if (pendingItemRewards[i] != null && pendingItemRewards[i].itemId == itemId)
            {
                pendingItemRewards[i].quantity += quantity;
                return;
            }
        }

        pendingItemRewards.Add(new SaveItemStack(itemId, quantity));
    }

    private static int GetCombinedItemCount(CombatTurnContext context, string itemId)
    {
        var count = context.SaveData.GetItemCount(itemId);
        if (context.PendingItemRewards == null)
        {
            return count;
        }

        for (var i = 0; i < context.PendingItemRewards.Count; i++)
        {
            var stack = context.PendingItemRewards[i];
            if (stack != null && stack.itemId == itemId)
            {
                count += Mathf.Max(0, stack.quantity);
            }
        }

        return count;
    }

    private static bool MatchesRoomKind(ExpeditionRoomKind[] eligibleKinds, ExpeditionRoomKind roomKind)
    {
        if (eligibleKinds == null || eligibleKinds.Length == 0)
        {
            return true;
        }

        for (var i = 0; i < eligibleKinds.Length; i++)
        {
            if (eligibleKinds[i] == roomKind)
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesRegion(string[] eligibleRegionIds, string regionId)
    {
        if (eligibleRegionIds == null || eligibleRegionIds.Length == 0)
        {
            return true;
        }

        return Contains(eligibleRegionIds, regionId);
    }

    private static bool HasSuppressedTag(ExpeditionEventDefinition definition, TaskContextSnapshot taskContext)
    {
        if (definition == null || definition.Tags == null || taskContext == null || taskContext.SuppressEventTags == null)
        {
            return false;
        }

        for (var i = 0; i < definition.Tags.Length; i++)
        {
            if (Contains(taskContext.SuppressEventTags, definition.Tags[i]))
            {
                return true;
            }
        }

        return false;
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

    private static int SafeHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        unchecked
        {
            var hash = 17;
            for (var i = 0; i < value.Length; i++)
            {
                hash = hash * 31 + value[i];
            }

            return hash;
        }
    }

    private static string CombinePrimaryAndNotes(string primary, string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return primary;
        }

        if (string.IsNullOrWhiteSpace(primary))
        {
            return notes;
        }

        return primary + "\n" + notes;
    }
}
