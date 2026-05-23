using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// S7 DivinePowerSystem：神通领悟判定、神通效果应用。
/// </summary>
public sealed class CultivationDivinePowerSystem : AbstractSystem
{
    private CultivationDivinePowerModel divinePowerModel;
    private CultivationBranchModel branchModel;
    private CultivationAttributeModel attributeModel;
    private CultivationRealmModel realmModel;

    protected override void OnInit()
    {
        divinePowerModel = this.GetModel<CultivationDivinePowerModel>();
        branchModel = this.GetModel<CultivationBranchModel>();
        attributeModel = this.GetModel<CultivationAttributeModel>();
        realmModel = this.GetModel<CultivationRealmModel>();
    }

    /// <summary>
    /// 检查是否满足神通领悟条件
    /// </summary>
    public bool CanLearnPower(DivinePowerConfigAsset config)
    {
        if (config == null || divinePowerModel.HasLearned(config.id))
        {
            return false;
        }

        // 检查分支前置条件
        if (!CheckBranchPrerequisites(config.branchPrerequisites))
        {
            return false;
        }

        // 检查触发条件
        return CheckTriggerCondition(config.triggerCondition);
    }

    /// <summary>
    /// 尝试领悟神通
    /// </summary>
    public bool TryLearnPower(DivinePowerConfigAsset config)
    {
        if (!CanLearnPower(config))
        {
            return false;
        }

        return divinePowerModel.LearnPower(config.id, config.displayName, config.type);
    }

    /// <summary>
    /// 检查分支前置条件
    /// </summary>
    private bool CheckBranchPrerequisites(DivinePowerConfigAsset.BranchPrerequisite[] prerequisites)
    {
        if (prerequisites == null || prerequisites.Length == 0)
        {
            return true;
        }

        foreach (var prereq in prerequisites)
        {
            var branchLevel = branchModel.GetBranchLevel(prereq.branchType);
            if (branchLevel < prereq.requiredLevel)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 检查触发条件
    /// </summary>
    private bool CheckTriggerCondition(DivinePowerConfigAsset.TriggerCondition condition)
    {
        if (condition == null)
        {
            return true;
        }

        return condition.conditionType switch
        {
            DivinePowerConfigAsset.TriggerConditionType.AttributeThreshold => CheckAttributeThreshold(condition),
            DivinePowerConfigAsset.TriggerConditionType.BehaviorAccumulation => CheckBehaviorAccumulation(condition),
            DivinePowerConfigAsset.TriggerConditionType.StoryEvent => CheckStoryEvent(condition),
            DivinePowerConfigAsset.TriggerConditionType.RandomInsight => CheckRandomInsight(condition),
            _ => true
        };
    }

    /// <summary>
    /// 检查属性阈值
    /// </summary>
    private bool CheckAttributeThreshold(DivinePowerConfigAsset.TriggerCondition condition)
    {
        var attributeValue = GetAttributeValue(condition.attributeType);
        return attributeValue >= condition.attributeThreshold;
    }

    /// <summary>
    /// 检查行为累计（简化版，实际需要持久化行为计数）
    /// </summary>
    private bool CheckBehaviorAccumulation(DivinePowerConfigAsset.TriggerCondition condition)
    {
        // TODO: 实现行为累计系统
        return false;
    }

    /// <summary>
    /// 检查剧情事件
    /// </summary>
    private bool CheckStoryEvent(DivinePowerConfigAsset.TriggerCondition condition)
    {
        // TODO: 实现剧情标志系统
        return false;
    }

    /// <summary>
    /// 检查随机顿悟
    /// </summary>
    private bool CheckRandomInsight(DivinePowerConfigAsset.TriggerCondition condition)
    {
        var random = Random.value;
        return random <= condition.randomChance;
    }

    /// <summary>
    /// 获取属性值
    /// </summary>
    private int GetAttributeValue(BaseAttributeType type)
    {
        return type switch
        {
            BaseAttributeType.DivineSense => attributeModel.SpiritSense.Value,
            BaseAttributeType.Constitution => attributeModel.RootBone.Value,
            BaseAttributeType.Comprehension => attributeModel.Insight.Value,
            BaseAttributeType.Charm => attributeModel.Charm.Value,
            BaseAttributeType.SoulPower => attributeModel.SoulPower.Value,
            BaseAttributeType.VitalEnergy => attributeModel.VitalEnergy.Value,
            BaseAttributeType.Willpower => attributeModel.Willpower.Value,
            BaseAttributeType.Dexterity => attributeModel.Dexterity.Value,
            BaseAttributeType.SpiritRoot => attributeModel.SpiritRoot.Value,
            BaseAttributeType.Fortune => 50, // 福缘默认值
            _ => 0
        };
    }

    /// <summary>
    /// 应用神通效果到战斗属性（简化版）
    /// </summary>
    public CombatStatsSnapshot ApplyPowerEffects(CombatStatsSnapshot baseSnapshot, DivinePowerConfigAsset config)
    {
        if (config == null || config.effect == null)
        {
            return baseSnapshot;
        }

        var effect = config.effect;
        var snapshot = baseSnapshot;

        // 应用属性加成
        snapshot.MaxHp += effect.healthBonus;
        snapshot.PhysicalAttack += effect.attackBonus;
        snapshot.PhysicalDefense += effect.defenseBonus;
        snapshot.Speed += effect.speedBonus;
        snapshot.HitChance += effect.hitChanceBonus;
        snapshot.CritRate += effect.critRateBonus;

        // 应用抗性加成
        if (effect.allResistBonus > 0)
        {
            for (var i = 0; i < snapshot.ElementResistPercents.Length; i++)
            {
                snapshot.ElementResistPercents[i] += effect.allResistBonus;
            }
        }

        return snapshot;
    }

    /// <summary>
    /// 获取所有可领悟的神通（需要配置数据支持）
    /// </summary>
    public List<DivinePowerConfigAsset> GetAvailablePowers()
    {
        // TODO: 从配置数据加载所有神通配置
        return new List<DivinePowerConfigAsset>();
    }

    /// <summary>
    /// 检查是否有新神通可领悟
    /// </summary>
    public bool HasNewPowerAvailable()
    {
        var availablePowers = GetAvailablePowers();
        foreach (var power in availablePowers)
        {
            if (CanLearnPower(power))
            {
                return true;
            }
        }
        return false;
    }
}
