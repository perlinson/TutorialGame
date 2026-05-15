using System;
using UnityEngine;

[Serializable]
public enum TaskObjectiveType
{
    DefeatFaction,
    CollectItem,
    ClearRegion,
    ResolveEventTag,
    ChooseEventOption,
    ObtainTaskEvidence
}

[Serializable]
public enum TaskEventInjectionRuleType
{
    InjectEventId,
    SuppressEventTag
}

[Serializable]
public sealed class TaskEventInjectionRule
{
    public TaskEventInjectionRuleType RuleType;
    public string Value;
}

[Serializable]
public sealed class TaskDefinition
{
    public string Id;
    public string Title;
    public string Issuer;
    public Sprite IllustrationImage;
    public string Description;
    public TaskObjectiveType ObjectiveType;
    public ExpeditionEnemyFaction TargetFaction;
    public bool HasLinkedFaction;
    public ExpeditionEnemyFaction LinkedFaction;
    public string TargetItemId;
    public string TargetRegionId;
    public string TargetEventTag;
    public string TargetOptionId;
    public string TargetEvidenceItemId;
    public int RequiredCount;
    public int UnlockRealmTier;
    public string UnlockRegionId;
    public string[] TaskTags;
    public string[] LinkedRegionIds;
    public TaskEventInjectionRule[] EventRules;
    public int RewardQi;
    public int RewardCrystals;
    public int RewardBagCapacity;
    public SaveItemStack[] RewardItems;
}
