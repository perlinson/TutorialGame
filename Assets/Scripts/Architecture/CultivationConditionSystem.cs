using UnityEngine;
using QFramework;

public sealed class CultivationConditionSystem : AbstractSystem
{
    protected override void OnInit()
    {
    }

    public bool AreEventConditionsMet(EventCondition[] conditions, CombatTurnContext context, TaskContextSnapshot taskContext)
    {
        if (conditions == null || conditions.Length == 0)
        {
            return true;
        }

        for (var i = 0; i < conditions.Length; i++)
        {
            var condition = conditions[i];
            if (condition == null)
            {
                continue;
            }

            if (!IsEventConditionMet(condition, context, taskContext))
            {
                return false;
            }
        }

        return true;
    }

    public string BuildRequirementText(EventCondition[] conditions, CombatTurnContext context, TaskContextSnapshot taskContext, string overrideText)
    {
        if (AreEventConditionsMet(conditions, context, taskContext))
        {
            return overrideText ?? string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(overrideText))
        {
            return overrideText;
        }

        if (conditions == null)
        {
            return "当前条件不足。";
        }

        for (var i = 0; i < conditions.Length; i++)
        {
            var condition = conditions[i];
            if (condition == null || IsEventConditionMet(condition, context, taskContext))
            {
                continue;
            }

            return BuildMissingConditionText(condition);
        }

        return "当前条件不足。";
    }

    private bool IsEventConditionMet(EventCondition condition, CombatTurnContext context, TaskContextSnapshot taskContext)
    {
        if (condition == null)
        {
            return true;
        }

        switch (condition.Type)
        {
            case EventConditionType.RealmTierAtLeast:
                return context != null && context.SaveData != null && context.SaveData.realmTier >= condition.IntValue;
            case EventConditionType.HasItem:
                return GetCombinedItemCount(context, condition.StringValue) >= Mathf.Max(1, condition.IntValue);
            case EventConditionType.ActiveTaskIs:
                return taskContext != null && taskContext.ActiveTaskId == condition.StringValue;
            case EventConditionType.HasTaskTag:
                return taskContext != null && Contains(taskContext.ActiveTaskTags, condition.StringValue);
            case EventConditionType.HasTaskFlag:
                return taskContext != null && Contains(taskContext.TaskStateFlags, condition.StringValue);
            case EventConditionType.HasTriggeredEvent:
                return taskContext != null && Contains(taskContext.TriggeredEventIds, condition.StringValue);
            case EventConditionType.HasChosenOption:
                return taskContext != null && Contains(taskContext.ChosenOptionIds, condition.StringValue);
            case EventConditionType.TorchlightAtLeast:
                return context != null && context.Torchlight >= condition.IntValue;
            case EventConditionType.SuppliesAtLeast:
                return context != null && context.Supplies >= condition.IntValue;
            default:
                return true;
        }
    }

    private static string BuildMissingConditionText(EventCondition condition)
    {
        switch (condition.Type)
        {
            case EventConditionType.HasItem:
                return "需要携带 " + InventoryLibrary.GetDisplayName(condition.StringValue) + " x" + Mathf.Max(1, condition.IntValue) + "。";
            case EventConditionType.ActiveTaskIs:
                return "需要当前委托与此线索相关。";
            case EventConditionType.HasTaskTag:
                return "需要当前委托具备对应线索标签。";
            case EventConditionType.HasTaskFlag:
                return "需要先取得前置线索。";
            case EventConditionType.HasTriggeredEvent:
                return "需要先触发对应的委托事件。";
            case EventConditionType.HasChosenOption:
                return "需要先走通前一段任务路线。";
            case EventConditionType.TorchlightAtLeast:
                return "火光至少需要 " + condition.IntValue + "。";
            case EventConditionType.SuppliesAtLeast:
                return "补给至少需要 " + condition.IntValue + "。";
            case EventConditionType.RealmTierAtLeast:
                return "至少达到 " + WorldRegionLibrary.GetRealmName(condition.IntValue) + "。";
            default:
                return "当前条件不足。";
        }
    }

    private static int GetCombinedItemCount(CombatTurnContext context, string itemId)
    {
        if (context == null || context.SaveData == null || string.IsNullOrWhiteSpace(itemId))
        {
            return 0;
        }

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
}
