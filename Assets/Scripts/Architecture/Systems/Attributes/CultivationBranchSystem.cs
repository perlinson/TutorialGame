using QFramework;
using UnityEngine;

/// <summary>
/// S5 BranchSystem：属性分支修炼系统。
/// 负责分支修炼、分支等级计算、分支与战斗属性映射。
/// </summary>
public sealed class CultivationBranchSystem : AbstractSystem
{
    private CultivationBranchModel branchModel;
    private CultivationRealmModel realmModel;
    private CultivationAttributeModel attributeModel;

    protected override void OnInit()
    {
        branchModel = this.GetModel<CultivationBranchModel>();
        realmModel = this.GetModel<CultivationRealmModel>();
        attributeModel = this.GetModel<CultivationAttributeModel>();
    }

    /// <summary>
    /// 修炼指定分支
    /// </summary>
    /// <param name="branchType">分支类型</param>
    /// <param name="timeSpent">消耗时间（天）</param>
    /// <param name="hasArtifactFocus">是否有功法侧重</param>
    /// <returns>实际获得的修炼投入值</returns>
    public int TrainBranch(BranchType branchType, int timeSpent, bool hasArtifactFocus = false)
    {
        if (timeSpent <= 0)
        {
            return 0;
        }

        var efficiency = CalculateTrainingEfficiency(branchType, hasArtifactFocus);
        var investment = Mathf.RoundToInt(timeSpent * efficiency);

        branchModel.AddTrainingInvestment(branchType, investment);
        return investment;
    }

    /// <summary>
    /// 计算修炼效率
    /// </summary>
    public float CalculateTrainingEfficiency(BranchType branchType, bool hasArtifactFocus)
    {
        var efficiency = 1.0f;

        // 境界加成
        efficiency *= GetRealmTrainingBonus(realmModel.Tier.Value);

        // 功法侧重加成
        if (hasArtifactFocus)
        {
            efficiency *= 1.5f;
        }

        // 基础属性加成（天赋影响效率）
        efficiency *= GetBaseAttributeEfficiency(branchType);

        return efficiency;
    }

    /// <summary>
    /// 获取境界修炼加成
    /// </summary>
    private static float GetRealmTrainingBonus(int realmTier)
    {
        return realmTier switch
        {
            0 => 1.0f,  // 炼气
            1 => 1.2f,  // 筑基
            2 => 1.5f,  // 金丹
            3 => 2.0f,  // 元婴
            4 => 3.0f,  // 化神
            _ => 1.0f
        };
    }

    /// <summary>
    /// 获取基础属性修炼效率加成
    /// </summary>
    private float GetBaseAttributeEfficiency(BranchType branchType)
    {
        var baseAttrType = GetBaseAttributeForBranch(branchType);
        var baseAttrValue = GetBaseAttributeValue(baseAttrType);

        if (baseAttrValue < 50)
        {
            return 0.7f; // 天赋低，修炼慢
        }
        else if (baseAttrValue > 80)
        {
            return 1.3f; // 天赋高，修炼快
        }

        return 1.0f;
    }

    /// <summary>
    /// 获取分支对应的基础属性
    /// </summary>
    private static BaseAttributeType GetBaseAttributeForBranch(BranchType branchType)
    {
        return branchType switch
        {
            BranchType.DivineSense_Strength => BaseAttributeType.DivineSense,
            BranchType.DivineSense_Control => BaseAttributeType.DivineSense,
            BranchType.DivineSense_Attack => BaseAttributeType.DivineSense,
            BranchType.DivineSense_Defense => BaseAttributeType.DivineSense,
            BranchType.Constitution_Physique => BaseAttributeType.Constitution,
            BranchType.Constitution_Recovery => BaseAttributeType.Constitution,
            BranchType.Constitution_Resistance => BaseAttributeType.Constitution,
            BranchType.Constitution_Tempering => BaseAttributeType.Constitution,
            BranchType.Comprehension_Learning => BaseAttributeType.Comprehension,
            BranchType.Comprehension_Insight => BaseAttributeType.Comprehension,
            BranchType.Comprehension_Integration => BaseAttributeType.Comprehension,
            BranchType.Comprehension_Deduction => BaseAttributeType.Comprehension,
            BranchType.Charm_Favor => BaseAttributeType.Charm,
            BranchType.Charm_Persuasion => BaseAttributeType.Charm,
            BranchType.Charm_Deterrence => BaseAttributeType.Charm,
            BranchType.Charm_Charm => BaseAttributeType.Charm,
            BranchType.SoulPower_Strength => BaseAttributeType.SoulPower,
            BranchType.SoulPower_Toughness => BaseAttributeType.SoulPower,
            BranchType.SoulPower_Enchant => BaseAttributeType.SoulPower,
            BranchType.SoulPower_Resonance => BaseAttributeType.SoulPower,
            BranchType.VitalEnergy_Storage => BaseAttributeType.VitalEnergy,
            BranchType.VitalEnergy_Purity => BaseAttributeType.VitalEnergy,
            BranchType.VitalEnergy_Recovery => BaseAttributeType.VitalEnergy,
            BranchType.VitalEnergy_Conversion => BaseAttributeType.VitalEnergy,
            BranchType.Willpower_Steadfast => BaseAttributeType.Willpower,
            BranchType.Willpower_Focus => BaseAttributeType.Willpower,
            BranchType.Willpower_Deterrence => BaseAttributeType.Willpower,
            BranchType.Willpower_Sacrifice => BaseAttributeType.Willpower,
            BranchType.Dexterity_Precision => BaseAttributeType.Dexterity,
            BranchType.Dexterity_Speed => BaseAttributeType.Dexterity,
            BranchType.Dexterity_Innovation => BaseAttributeType.Dexterity,
            BranchType.Dexterity_Flexibility => BaseAttributeType.Dexterity,
            _ => BaseAttributeType.Comprehension
        };
    }

    /// <summary>
    /// 获取基础属性值
    /// </summary>
    private int GetBaseAttributeValue(BaseAttributeType type)
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
            BaseAttributeType.Fortune => 50, // 福缘默认值（暂无实现）
            _ => 50
        };
    }

    /// <summary>
    /// 分支等级映射到战斗属性（简化版，后续可扩展）
    /// </summary>
    public int MapBranchToCombatStat(BranchType branchType)
    {
        var level = branchModel.GetBranchLevel(branchType);

        return branchType switch
        {
            // 神识分支 → 暴击/命中
            BranchType.DivineSense_Strength => level / 2,      // 探测 → 命中
            BranchType.DivineSense_Attack => level / 3,       // 攻击 → 暴击

            // 根骨分支 → 生命/防御
            BranchType.Constitution_Physique => level * 5,    // 体质 → 生命
            BranchType.Constitution_Resistance => level / 2,  // 抗性 → 防御

            // 悟性分支 → 速度/闪避
            BranchType.Comprehension_Deduction => level / 3,  // 推演 → 闪避

            // 魂力分支 → 法力/法术防御
            BranchType.SoulPower_Strength => level * 3,       // 强度 → 法力
            BranchType.SoulPower_Toughness => level / 2,      // 韧性 → 法防

            // 精元分支 → 生命恢复
            BranchType.VitalEnergy_Recovery => level / 2,     // 恢复 → 回血

            // 意志分支 → 抗性
            BranchType.Willpower_Steadfast => level / 2,      // 坚定 → 全抗

            // 机巧分支 → 命中/暴击
            BranchType.Dexterity_Precision => level / 3,     // 精度 → 命中

            _ => 0
        };
    }
}
