using System;
using UnityEngine;

[Serializable]
public enum ExpeditionEventCardType
{
    Generic,
    TaskInjected,
    TaskExclusive
}

[Serializable]
public enum EventConditionType
{
    RealmTierAtLeast,
    HasItem,
    ActiveTaskIs,
    HasTaskTag,
    HasTaskFlag,
    HasTriggeredEvent,
    HasChosenOption,
    TorchlightAtLeast,
    SuppliesAtLeast
}

[Serializable]
public enum EventEffectType
{
    GainPendingQi,
    GainPendingCrystals,
    ModifyTorchlight,
    ModifySupplies,
    HealHero,
    ModifyStress,
    ReceiveDamage,
    AddPendingItem,
    AddTaskFlag,
    AddTaskProgress
}

[Serializable]
public enum TaskProgressSignalType
{
    DefeatFaction,
    ClearRegion,
    ObtainItem,
    ResolveEventTag,
    ChooseEventOption,
    ObtainTaskEvidence,
    AddProgressToActiveTask
}

[Serializable]
public sealed class EventCondition
{
    public EventConditionType Type;
    public int IntValue;
    public string StringValue;
}

[Serializable]
public sealed class EventEffect
{
    public EventEffectType Type;
    public int IntValue;
    public string StringValue;
    public string SecondaryStringValue;
}

[Serializable]
public sealed class TaskProgressSignal
{
    public TaskProgressSignalType Type;
    public int Count = 1;
    public string StringValue;
    public ExpeditionEnemyFaction FactionValue;
}

[Serializable]
public sealed class ExpeditionEventOptionDefinition
{
    public string Id;
    public string Label;
    public string RequirementText;
    public string BadgeText;
    public string ResultText;
    public bool IsTaskOption;
    public EventCondition[] Conditions;
    public EventEffect[] Effects;
    public TaskProgressSignal[] TaskProgressSignals;
}

[Serializable]
public sealed class ExpeditionEventDefinition
{
    public string Id;
    public string Title;
    public Sprite IllustrationImage;
    public string Body;
    public string BadgeText;
    public string[] Tags;
    public ExpeditionEventCardType CardType;
    public ExpeditionRoomKind[] EligibleRoomKinds;
    public string[] EligibleRegionIds;
    public string RequiredTaskId;
    public int Priority;
    public int Weight = 1;
    public EventCondition[] Conditions;
    public ExpeditionEventOptionDefinition[] Options;

    public int GetSelectionPriority()
    {
        if (Priority != 0)
        {
            return Priority;
        }

        switch (CardType)
        {
            case ExpeditionEventCardType.TaskExclusive:
                return 200;
            case ExpeditionEventCardType.TaskInjected:
                return 100;
            default:
                return 0;
        }
    }
}
