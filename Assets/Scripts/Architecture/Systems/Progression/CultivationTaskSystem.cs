using QFramework;

public sealed class CultivationTaskSystem : AbstractSystem
{
    private CultivationRewardSystem rewardSystem;

    protected override void OnInit()
    {
        rewardSystem = this.GetSystem<CultivationRewardSystem>();
    }

    public string ResolveTaskBoard(MainMenuSaveData saveData)
    {
        saveData.EnsureDefaults();

        string reason;
        if (CanClaimActiveTask(saveData, out reason))
        {
            var claimSummary = ClaimActiveTask(saveData);
            EnsureActiveTask(saveData);
            return claimSummary;
        }

        if (string.IsNullOrWhiteSpace(saveData.activeTaskId))
        {
            if (EnsureActiveTask(saveData))
            {
                return "山门委托已更新。";
            }

            return "当前没有新的委托可接。";
        }

        return string.Empty;
    }

    public string BuildActiveTaskSummary(MainMenuSaveData saveData)
    {
        return TaskLibrary.BuildActiveTaskSummary(saveData);
    }

    public TaskContextSnapshot GetActiveTaskContext(MainMenuSaveData saveData)
    {
        saveData.EnsureDefaults();
        TaskDefinition definition;
        SaveTaskState state;
        if (!TaskLibrary.TryGetActiveTask(saveData, out definition, out state))
        {
            return new TaskContextSnapshot
            {
                ActiveTaskId = string.Empty,
                ActiveTaskTitle = "暂无委托",
                ActiveTaskSummary = "委托：暂无新任务。\n继续推进境界或开辟新地域后，会有新的山门事务。",
                ActiveTaskTags = new string[0],
                TaskLinkedRegionIds = new string[0],
                TaskStateFlags = new string[0],
                TriggeredEventIds = new string[0],
                ChosenOptionIds = new string[0],
                InjectEventIds = new string[0],
                SuppressEventTags = new string[0]
            };
        }

        var injectEventIds = new System.Collections.Generic.List<string>();
        var suppressEventTags = new System.Collections.Generic.List<string>();
        if (definition.EventRules != null)
        {
            for (var i = 0; i < definition.EventRules.Length; i++)
            {
                var rule = definition.EventRules[i];
                if (rule == null || string.IsNullOrWhiteSpace(rule.Value))
                {
                    continue;
                }

                if (rule.RuleType == TaskEventInjectionRuleType.InjectEventId)
                {
                    injectEventIds.Add(rule.Value);
                }
                else
                {
                    suppressEventTags.Add(rule.Value);
                }
            }
        }

        var snapshot = new TaskContextSnapshot
        {
            ActiveTaskId = definition.Id,
            ActiveTaskTitle = definition.Title,
            ActiveTaskSummary = TaskLibrary.BuildActiveTaskSummary(saveData),
            ActiveTaskTags = definition.TaskTags ?? new string[0],
            TaskLinkedRegionIds = definition.LinkedRegionIds ?? new string[0],
            HasLinkedFaction = definition.HasLinkedFaction,
            TaskLinkedFaction = definition.LinkedFaction,
            TaskStateFlags = state != null && state.progressFlags != null ? state.progressFlags : new string[0],
            TriggeredEventIds = state != null && state.triggeredEventIds != null ? state.triggeredEventIds : new string[0],
            ChosenOptionIds = state != null && state.chosenOptionIds != null ? state.chosenOptionIds : new string[0],
            InjectEventIds = injectEventIds.ToArray(),
            SuppressEventTags = suppressEventTags.ToArray(),
            CanClaim = TaskLibrary.GetProgressValue(saveData, definition, state) >= definition.RequiredCount,
            IllustrationImage = definition.IllustrationImage
        };
        return snapshot;
    }

    public TaskProgressResult RecordProgress(MainMenuSaveData saveData, TaskProgressSignal signal)
    {
        saveData.EnsureDefaults();
        var result = new TaskProgressResult();
        if (signal == null)
        {
            return result;
        }

        TaskDefinition definition;
        SaveTaskState state;
        if (!TaskLibrary.TryGetActiveTask(saveData, out definition, out state) || definition == null || state == null)
        {
            return result;
        }

        var progressed = false;
        var progressDelta = 0;
        switch (signal.Type)
        {
            case TaskProgressSignalType.DefeatFaction:
                if (definition.ObjectiveType == TaskObjectiveType.DefeatFaction && definition.TargetFaction == signal.FactionValue)
                {
                    progressDelta = signal.Count <= 0 ? 1 : signal.Count;
                    progressed = true;
                }

                break;
            case TaskProgressSignalType.ClearRegion:
                if (definition.ObjectiveType == TaskObjectiveType.ClearRegion && definition.TargetRegionId == signal.StringValue)
                {
                    progressDelta = definition.RequiredCount;
                    progressed = true;
                }

                break;
            case TaskProgressSignalType.ObtainItem:
            case TaskProgressSignalType.ObtainTaskEvidence:
                if (definition.ObjectiveType == TaskObjectiveType.ObtainTaskEvidence && definition.TargetEvidenceItemId == signal.StringValue)
                {
                    progressDelta = signal.Count <= 0 ? 1 : signal.Count;
                    progressed = true;
                }

                break;
            case TaskProgressSignalType.ResolveEventTag:
                if (definition.ObjectiveType == TaskObjectiveType.ResolveEventTag && definition.TargetEventTag == signal.StringValue)
                {
                    progressDelta = signal.Count <= 0 ? 1 : signal.Count;
                    progressed = true;
                }

                break;
            case TaskProgressSignalType.ChooseEventOption:
                if (definition.ObjectiveType == TaskObjectiveType.ChooseEventOption && definition.TargetOptionId == signal.StringValue)
                {
                    progressDelta = signal.Count <= 0 ? 1 : signal.Count;
                    progressed = true;
                }

                break;
            case TaskProgressSignalType.AddProgressToActiveTask:
                progressDelta = signal.Count <= 0 ? 1 : signal.Count;
                progressed = true;
                break;
        }

        if (!progressed)
        {
            return result;
        }

        state.progress += progressDelta;
        var currentProgress = TaskLibrary.GetProgressValue(saveData, definition, state);
        if (currentProgress >= definition.RequiredCount)
        {
            state.completed = true;
            result.CompletedNow = true;
        }

        result.ProgressChanged = true;
        result.CurrentProgress = currentProgress;
        result.RequiredCount = definition.RequiredCount;
        result.Message = result.CompletedNow
            ? "委托进度已达成，可回山门结算：" + definition.Title + "。"
            : "委托推进：" + definition.Title + "（" + currentProgress + " / " + definition.RequiredCount + "）。";
        return result;
    }

    public void AddTaskFlag(MainMenuSaveData saveData, string taskId, string flagId)
    {
        saveData.EnsureDefaults();
        var state = saveData.GetOrCreateTaskState(taskId);
        if (state != null)
        {
            state.AddFlag(flagId);
        }
    }

    public void MarkTriggeredEvent(MainMenuSaveData saveData, string taskId, string eventId)
    {
        saveData.EnsureDefaults();
        var state = saveData.GetOrCreateTaskState(taskId);
        if (state != null)
        {
            state.MarkTriggeredEvent(eventId);
        }
    }

    public void MarkChosenOption(MainMenuSaveData saveData, string taskId, string optionId)
    {
        saveData.EnsureDefaults();
        var state = saveData.GetOrCreateTaskState(taskId);
        if (state != null)
        {
            state.MarkChosenOption(optionId);
        }
    }

    public bool CanClaimActiveTask(MainMenuSaveData saveData, out string reason)
    {
        TaskDefinition definition;
        SaveTaskState state;
        if (!TaskLibrary.TryGetActiveTask(saveData, out definition, out state))
        {
            reason = "当前没有可结算的委托。";
            return false;
        }

        var progress = TaskLibrary.GetProgressValue(saveData, definition, state);
        if (progress < definition.RequiredCount)
        {
            reason = "当前委托还没达成条件。";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public string ClaimActiveTask(MainMenuSaveData saveData)
    {
        saveData.EnsureDefaults();
        TaskDefinition definition;
        SaveTaskState state;
        if (!TaskLibrary.TryGetActiveTask(saveData, out definition, out state))
        {
            return "当前没有可领取的委托奖励。";
        }

        if (definition.ObjectiveType == TaskObjectiveType.CollectItem)
        {
            saveData.RemoveItem(definition.TargetItemId, definition.RequiredCount);
        }
        else if (definition.ObjectiveType == TaskObjectiveType.ObtainTaskEvidence)
        {
            saveData.RemoveItem(definition.TargetEvidenceItemId, definition.RequiredCount);
        }

        saveData.qi += definition.RewardQi;
        saveData.spiritCrystals += definition.RewardCrystals;
        if (definition.RewardBagCapacity > 0)
        {
            saveData.bagCapacity += definition.RewardBagCapacity;
        }

        var pendingRewards = new System.Collections.Generic.List<SaveItemStack>();
        if (definition.RewardItems != null)
        {
            for (var i = 0; i < definition.RewardItems.Length; i++)
            {
                if (definition.RewardItems[i] == null)
                {
                    continue;
                }

                rewardSystem.MergeLoot(pendingRewards, new System.Collections.Generic.List<SaveItemStack>
                {
                    new SaveItemStack(definition.RewardItems[i].itemId, definition.RewardItems[i].quantity)
                });
            }
        }

        var bankResult = rewardSystem.BankPendingLoot(saveData, pendingRewards);
        state.completed = true;
        state.rewardClaimed = true;
        saveData.activeTaskId = string.Empty;
        var message = "委托已结算：" + definition.Title + "。";
        if (!string.IsNullOrWhiteSpace(bankResult.BankedSummary))
        {
            message += "\n获得物资：" + bankResult.BankedSummary + "。";
        }

        if (!string.IsNullOrWhiteSpace(bankResult.OverflowSummary))
        {
            saveData.spiritCrystals += bankResult.OverflowCrystalGain;
            message += "\n储物袋已满，" + bankResult.OverflowSummary + " 已折成灵石 +" + bankResult.OverflowCrystalGain + "。";
        }

        return message;
    }

    private static bool EnsureActiveTask(MainMenuSaveData saveData)
    {
        saveData.EnsureDefaults();
        if (!string.IsNullOrWhiteSpace(saveData.activeTaskId))
        {
            var activeState = saveData.FindTaskState(saveData.activeTaskId);
            if (activeState != null && !activeState.rewardClaimed)
            {
                return false;
            }
        }

        var definitions = TaskLibrary.GetDefinitions();
        for (var i = 0; i < definitions.Length; i++)
        {
            var definition = definitions[i];
            if (definition == null || !IsTaskUnlocked(saveData, definition))
            {
                continue;
            }

            var state = saveData.FindTaskState(definition.Id);
            if (state != null && state.rewardClaimed)
            {
                continue;
            }

            saveData.activeTaskId = definition.Id;
            saveData.GetOrCreateTaskState(definition.Id);
            return true;
        }

        saveData.activeTaskId = string.Empty;
        return false;
    }

    private static bool IsTaskUnlocked(MainMenuSaveData saveData, TaskDefinition definition)
    {
        return saveData.realmTier >= definition.UnlockRealmTier &&
               (string.IsNullOrWhiteSpace(definition.UnlockRegionId) || saveData.IsRegionUnlocked(definition.UnlockRegionId));
    }
}
