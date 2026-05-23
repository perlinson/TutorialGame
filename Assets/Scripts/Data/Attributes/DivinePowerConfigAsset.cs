using System;
using UnityEngine;

/// <summary>
/// 神通配置数据（ScriptableObject）
/// 用于定义神通的触发条件、效果、分支前置等
/// </summary>
[CreateAssetMenu(fileName = "DivinePowerConfig", menuName = "Cultivation/Divine Power Config")]
public sealed class DivinePowerConfigAsset : ScriptableObject
{
    [Header("神通基本信息")]
    public string id;
    public string displayName;
    [TextArea(2, 4)] public string description;
    public DivinePowerType type;
    public Sprite icon;

    [Header("触发条件")]
    public TriggerCondition triggerCondition;

    [Header("分支前置条件")]
    public BranchPrerequisite[] branchPrerequisites;

    [Header("神通效果")]
    public DivinePowerEffect effect;

    [Serializable]
    public enum TriggerConditionType
    {
        AttributeThreshold,    // 属性阈值检测
        BehaviorAccumulation,  // 行为累计触发
        StoryEvent,           // 剧情触发
        RandomInsight         // 随机顿悟
    }

    [Serializable]
    public sealed class TriggerCondition
    {
        public TriggerConditionType conditionType;
        public BaseAttributeType attributeType; // 属性阈值类型
        public int attributeThreshold;          // 属性阈值
        public string behaviorId;               // 行为ID
        public int behaviorCount;               // 行为累计次数
        public string storyFlag;                // 剧情标志
        public float randomChance;              // 随机顿悟概率（0-1）
    }

    [Serializable]
    public sealed class BranchPrerequisite
    {
        public BranchType branchType;
        public int requiredLevel;
    }

    [Serializable]
    public sealed class DivinePowerEffect
    {
        [Header("属性加成")]
        public int healthBonus;
        public int attackBonus;
        public int defenseBonus;
        public int speedBonus;
        public int hitChanceBonus;
        public int critRateBonus;

        [Header("抗性加成")]
        public int poisonResistBonus;
        public int stunResistBonus;
        public int allResistBonus;

        [Header("特殊效果")]
        public bool isPassive;           // 是否为被动效果
        public string specialEffectId;   // 特殊效果ID
        [TextArea(2, 3)] public string specialEffectDescription;
    }
}
