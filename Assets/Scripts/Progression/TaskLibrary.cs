using UnityEngine;

public static class TaskLibrary
{
    private static TaskDefinition[] definitions;

    public static TaskDefinition[] GetDefinitions()
    {
        if (definitions != null)
        {
            return definitions;
        }

        var database = Resources.Load<TaskDatabaseAsset>("Data/TaskDatabase");
        if (database != null && database.tasks != null && database.tasks.Length > 0)
        {
            definitions = database.tasks;
            return definitions;
        }

        definitions = BuildFallbackDefinitions();
        return definitions;
    }

    public static bool TryGetTaskDefinition(string taskId, out TaskDefinition definition)
    {
        definition = null;
        if (string.IsNullOrWhiteSpace(taskId))
        {
            return false;
        }

        var loadedDefinitions = GetDefinitions();
        for (var i = 0; i < loadedDefinitions.Length; i++)
        {
            if (loadedDefinitions[i] != null && loadedDefinitions[i].Id == taskId)
            {
                definition = loadedDefinitions[i];
                return true;
            }
        }

        return false;
    }

    public static bool TryGetActiveTask(MainMenuSaveData saveData, out TaskDefinition definition, out SaveTaskState state)
    {
        saveData.EnsureDefaults();
        definition = null;
        state = null;
        if (string.IsNullOrWhiteSpace(saveData.activeTaskId))
        {
            return false;
        }

        if (!TryGetTaskDefinition(saveData.activeTaskId, out definition))
        {
            return false;
        }

        state = saveData.GetOrCreateTaskState(definition.Id);
        return true;
    }

    public static string BuildActiveTaskSummary(MainMenuSaveData saveData)
    {
        TaskDefinition definition;
        SaveTaskState state;
        if (!TryGetActiveTask(saveData, out definition, out state))
        {
            return "委托：暂无新任务。\n继续推进境界或开辟新地域后，会有新的山门事务。";
        }

        var progress = GetProgressValue(saveData, definition, state);
        var completed = progress >= definition.RequiredCount;
        return "委托：" + definition.Title + "\n" +
               "发布人：" + definition.Issuer + "\n" +
               definition.Description + "\n" +
               "进度：" + progress + " / " + definition.RequiredCount + (completed ? "  已可结算" : string.Empty) + "\n" +
               "奖励：修为 +" + definition.RewardQi + " / 灵石 +" + definition.RewardCrystals +
               (definition.RewardBagCapacity > 0 ? " / 储物袋 +" + definition.RewardBagCapacity + " 格" : string.Empty);
    }

    public static int GetProgressValue(MainMenuSaveData saveData, TaskDefinition definition, SaveTaskState state)
    {
        saveData.EnsureDefaults();
        if (definition == null)
        {
            return 0;
        }

        switch (definition.ObjectiveType)
        {
            case TaskObjectiveType.CollectItem:
                return saveData.GetItemCount(definition.TargetItemId);
            case TaskObjectiveType.ObtainTaskEvidence:
                return state != null ? state.progress : 0;
            default:
                return state != null ? state.progress : 0;
        }
    }

    private static TaskDefinition[] BuildFallbackDefinitions()
    {
        return new[]
        {
            new TaskDefinition
            {
                Id = "task_bandit_route",
                Title = "清剿山道流寇",
                Issuer = "青石山门执事",
                Description = "最近山门外的流寇又开始截取灵石与药材，先拿几枚路引回来。",
                ObjectiveType = TaskObjectiveType.ObtainTaskEvidence,
                TargetEvidenceItemId = "bandit_route_token",
                RequiredCount = 2,
                UnlockRealmTier = 0,
                UnlockRegionId = "green_stone_gate",
                TaskTags = new[] { "bandit", "evidence", "trail" },
                LinkedRegionIds = new[] { "green_stone_gate" },
                HasLinkedFaction = true,
                LinkedFaction = ExpeditionEnemyFaction.Bandit,
                EventRules = new[]
                {
                    new TaskEventInjectionRule { RuleType = TaskEventInjectionRuleType.InjectEventId, Value = "event_bandit_hidden_scout" },
                    new TaskEventInjectionRule { RuleType = TaskEventInjectionRuleType.SuppressEventTag, Value = "generic_scout" }
                },
                RewardQi = 2,
                RewardCrystals = 2,
                RewardBagCapacity = 2,
                RewardItems = new[] { new SaveItemStack("green_spirit_sand", 2) }
            },
            new TaskDefinition
            {
                Id = "task_mist_herbs",
                Title = "采回雾隐芝",
                Issuer = "丹霞旧宗药师",
                Description = "雾泽林中生着一味稀缺灵芝，采够数量便可换取丹道资源。",
                ObjectiveType = TaskObjectiveType.CollectItem,
                TargetItemId = "mist_mushroom",
                RequiredCount = 2,
                UnlockRealmTier = 0,
                UnlockRegionId = "misty_forest",
                TaskTags = new[] { "herb", "gathering" },
                LinkedRegionIds = new[] { "misty_forest" },
                RewardQi = 3,
                RewardCrystals = 2,
                RewardItems = new[] { new SaveItemStack("mind_cleansing_incense", 2) }
            },
            new TaskDefinition
            {
                Id = "task_valley_cultists",
                Title = "焚尽夺灵邪修",
                Issuer = "赤霞谷巡山使",
                Description = "火脉旁的邪修靠夺灵邪诀盘踞已久，需要带回足够多的手札作证。",
                ObjectiveType = TaskObjectiveType.ObtainTaskEvidence,
                TargetEvidenceItemId = "evil_cult_notes",
                RequiredCount = 2,
                UnlockRealmTier = 1,
                UnlockRegionId = "crimson_valley",
                TaskTags = new[] { "cultivator", "evil_cult", "evidence" },
                LinkedRegionIds = new[] { "crimson_valley" },
                HasLinkedFaction = true,
                LinkedFaction = ExpeditionEnemyFaction.Cultivator,
                EventRules = new[]
                {
                    new TaskEventInjectionRule { RuleType = TaskEventInjectionRuleType.InjectEventId, Value = "event_cult_blood_altar" },
                    new TaskEventInjectionRule { RuleType = TaskEventInjectionRuleType.SuppressEventTag, Value = "generic_shrine" }
                },
                RewardQi = 4,
                RewardCrystals = 3,
                RewardItems = new[] { new SaveItemStack("crimson_ore", 2) }
            },
            new TaskDefinition
            {
                Id = "task_springs_array",
                Title = "搜集古阵残片",
                Issuer = "玄泉守阵人",
                Description = "洞天深处的古阵需要修补，带回足量阵片便可换得后续通行。",
                ObjectiveType = TaskObjectiveType.CollectItem,
                TargetItemId = "array_shard",
                RequiredCount = 3,
                UnlockRealmTier = 1,
                UnlockRegionId = "deep_springs",
                TaskTags = new[] { "array", "ruins" },
                LinkedRegionIds = new[] { "deep_springs" },
                RewardQi = 4,
                RewardCrystals = 3,
                RewardItems = new[] { new SaveItemStack("spring_jade_dew", 1) }
            },
            new TaskDefinition
            {
                Id = "task_pass_clear",
                Title = "打通北冥古道",
                Issuer = "外域商旅",
                Description = "只有清掉古道上的大患，后续更大的商路和州域情报才会向你开放。",
                ObjectiveType = TaskObjectiveType.ClearRegion,
                TargetRegionId = "northern_pass",
                RequiredCount = 1,
                UnlockRealmTier = 2,
                UnlockRegionId = "northern_pass",
                TaskTags = new[] { "frontier", "clear_region" },
                LinkedRegionIds = new[] { "northern_pass" },
                RewardQi = 5,
                RewardCrystals = 4,
                RewardBagCapacity = 2,
                RewardItems = new[] { new SaveItemStack("ancient_pass_order", 1) }
            },
            new TaskDefinition
            {
                Id = "task_heart_echoes",
                Title = "平息心印残梦",
                Issuer = "静心古观长老",
                Description = "深处残梦反复侵心，需要找出心印源头并带回足够多的梦痕线索。",
                ObjectiveType = TaskObjectiveType.ResolveEventTag,
                TargetEventTag = "heartdream_clue",
                RequiredCount = 2,
                UnlockRealmTier = 2,
                UnlockRegionId = "deep_springs",
                TaskTags = new[] { "heart_demon", "dream", "clue" },
                LinkedRegionIds = new[] { "deep_springs" },
                HasLinkedFaction = true,
                LinkedFaction = ExpeditionEnemyFaction.HeartDemon,
                EventRules = new[]
                {
                    new TaskEventInjectionRule { RuleType = TaskEventInjectionRuleType.InjectEventId, Value = "event_heartdream_echo" },
                    new TaskEventInjectionRule { RuleType = TaskEventInjectionRuleType.SuppressEventTag, Value = "generic_trap" }
                },
                RewardQi = 5,
                RewardCrystals = 4,
                RewardItems = new[] { new SaveItemStack("heart_mark_fragment", 2) }
            }
        };
    }
}
