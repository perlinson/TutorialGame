using QFramework;

/// <summary>
/// S6 SchoolSystem：流派加成应用、流派技能解锁。
/// </summary>
public sealed class CultivationSchoolSystem : AbstractSystem
{
    private CultivationSchoolModel schoolModel;
    private CultivationBranchModel branchModel;
    private CultivationAttributeModel attributeModel;

    protected override void OnInit()
    {
        schoolModel = this.GetModel<CultivationSchoolModel>();
        branchModel = this.GetModel<CultivationBranchModel>();
        attributeModel = this.GetModel<CultivationAttributeModel>();
    }

    /// <summary>
    /// 获取流派对基础属性的加成（百分比）
    /// </summary>
    public int GetSchoolAttributeBonus(BaseAttributeType attributeType)
    {
        var school = schoolModel.SelectedSchool.Value;
        if (school == SchoolType.None)
        {
            return 0;
        }

        return school switch
        {
            SchoolType.Mage => GetMageAttributeBonus(attributeType),
            SchoolType.Body => GetBodyAttributeBonus(attributeType),
            SchoolType.Sword => GetSwordAttributeBonus(attributeType),
            SchoolType.Music => GetMusicAttributeBonus(attributeType),
            SchoolType.Artifact => GetArtifactAttributeBonus(attributeType),
            SchoolType.Talisman => GetTalismanAttributeBonus(attributeType),
            SchoolType.Formation => GetFormationAttributeBonus(attributeType),
            SchoolType.Pill => GetPillAttributeBonus(attributeType),
            SchoolType.Ghost => GetGhostAttributeBonus(attributeType),
            _ => 0
        };
    }

    /// <summary>
    /// 获取流派对分支的加成（额外等级）
    /// </summary>
    public int GetSchoolBranchBonus(BranchType branchType)
    {
        var school = schoolModel.SelectedSchool.Value;
        if (school == SchoolType.None)
        {
            return 0;
        }

        var schoolLevel = schoolModel.SchoolLevel.Value;
        var bonusPerLevel = 2; // 每级流派等级+2分支等级

        return school switch
        {
            SchoolType.Mage => IsMageBranch(branchType) ? schoolLevel * bonusPerLevel : 0,
            SchoolType.Body => IsBodyBranch(branchType) ? schoolLevel * bonusPerLevel : 0,
            SchoolType.Sword => IsSwordBranch(branchType) ? schoolLevel * bonusPerLevel : 0,
            SchoolType.Music => IsMusicBranch(branchType) ? schoolLevel * bonusPerLevel : 0,
            SchoolType.Artifact => IsArtifactBranch(branchType) ? schoolLevel * bonusPerLevel : 0,
            SchoolType.Talisman => IsTalismanBranch(branchType) ? schoolLevel * bonusPerLevel : 0,
            SchoolType.Formation => IsFormationBranch(branchType) ? schoolLevel * bonusPerLevel : 0,
            SchoolType.Pill => IsPillBranch(branchType) ? schoolLevel * bonusPerLevel : 0,
            SchoolType.Ghost => IsGhostBranch(branchType) ? schoolLevel * bonusPerLevel : 0,
            _ => 0
        };
    }

    /// <summary>
    /// 应用流派加成到分支模型
    /// </summary>
    public void ApplySchoolBonuses()
    {
        var school = schoolModel.SelectedSchool.Value;
        if (school == SchoolType.None)
        {
            return;
        }

        // 清除所有功法侧重加成
        foreach (BranchType type in System.Enum.GetValues(typeof(BranchType)))
        {
            branchModel.SetArtifactBonus(type, 0);
        }

        // 应用流派分支加成
        foreach (BranchType type in System.Enum.GetValues(typeof(BranchType)))
        {
            var bonus = GetSchoolBranchBonus(type);
            if (bonus > 0)
            {
                branchModel.SetArtifactBonus(type, bonus);
            }
        }
    }

    /// <summary>
    /// 检查玩家是否满足流派选择条件
    /// </summary>
    public bool CanSelectSchool(SchoolType schoolType)
    {
        if (schoolType == SchoolType.None)
        {
            return true;
        }

        // 检查基础属性是否达到流派最低要求
        return schoolType switch
        {
            SchoolType.Mage => attributeModel.SpiritSense.Value >= 30 && attributeModel.Insight.Value >= 20,
            SchoolType.Body => attributeModel.RootBone.Value >= 30 && attributeModel.VitalEnergy.Value >= 20,
            SchoolType.Sword => attributeModel.RootBone.Value >= 30 && attributeModel.Willpower.Value >= 20,
            SchoolType.Music => attributeModel.SpiritSense.Value >= 30 && attributeModel.SoulPower.Value >= 20,
            SchoolType.Artifact => attributeModel.VitalEnergy.Value >= 30 && attributeModel.Dexterity.Value >= 20,
            SchoolType.Talisman => attributeModel.SoulPower.Value >= 30 && attributeModel.Dexterity.Value >= 20,
            SchoolType.Formation => attributeModel.SpiritSense.Value >= 30 && attributeModel.Insight.Value >= 20,
            SchoolType.Pill => attributeModel.SpiritSense.Value >= 30 && attributeModel.VitalEnergy.Value >= 20,
            SchoolType.Ghost => attributeModel.SoulPower.Value >= 30 && attributeModel.Willpower.Value >= 20,
            _ => true
        };
    }

    #region 流派属性加成

    private static int GetMageAttributeBonus(BaseAttributeType type)
    {
        return type switch
        {
            BaseAttributeType.DivineSense => 20,
            BaseAttributeType.Comprehension => 15,
            BaseAttributeType.SpiritRoot => 25,
            BaseAttributeType.Willpower => 10,
            BaseAttributeType.Fortune => 5,
            _ => 0
        };
    }

    private static int GetBodyAttributeBonus(BaseAttributeType type)
    {
        return type switch
        {
            BaseAttributeType.Constitution => 25,
            BaseAttributeType.VitalEnergy => 20,
            BaseAttributeType.Willpower => 15,
            BaseAttributeType.Fortune => 5,
            _ => 0
        };
    }

    private static int GetSwordAttributeBonus(BaseAttributeType type)
    {
        return type switch
        {
            BaseAttributeType.Constitution => 20,
            BaseAttributeType.Willpower => 25,
            BaseAttributeType.DivineSense => 15,
            BaseAttributeType.Comprehension => 10,
            BaseAttributeType.SpiritRoot => 5,
            _ => 0
        };
    }

    private static int GetMusicAttributeBonus(BaseAttributeType type)
    {
        return type switch
        {
            BaseAttributeType.DivineSense => 25,
            BaseAttributeType.SoulPower => 20,
            BaseAttributeType.Charm => 15,
            BaseAttributeType.Comprehension => 10,
            BaseAttributeType.Willpower => 5,
            _ => 0
        };
    }

    private static int GetArtifactAttributeBonus(BaseAttributeType type)
    {
        return type switch
        {
            BaseAttributeType.VitalEnergy => 25,
            BaseAttributeType.Dexterity => 20,
            BaseAttributeType.Constitution => 15,
            BaseAttributeType.DivineSense => 10,
            BaseAttributeType.Fortune => 5,
            _ => 0
        };
    }

    private static int GetTalismanAttributeBonus(BaseAttributeType type)
    {
        return type switch
        {
            BaseAttributeType.SoulPower => 20,
            BaseAttributeType.Dexterity => 25,
            BaseAttributeType.Comprehension => 15,
            BaseAttributeType.DivineSense => 10,
            BaseAttributeType.Fortune => 5,
            _ => 0
        };
    }

    private static int GetFormationAttributeBonus(BaseAttributeType type)
    {
        return type switch
        {
            BaseAttributeType.DivineSense => 25,
            BaseAttributeType.Comprehension => 20,
            BaseAttributeType.SoulPower => 15,
            BaseAttributeType.Dexterity => 10,
            BaseAttributeType.Willpower => 5,
            _ => 0
        };
    }

    private static int GetPillAttributeBonus(BaseAttributeType type)
    {
        return type switch
        {
            BaseAttributeType.DivineSense => 20,
            BaseAttributeType.VitalEnergy => 15,
            BaseAttributeType.Dexterity => 15,
            BaseAttributeType.SpiritRoot => 20,
            BaseAttributeType.Fortune => 10,
            _ => 0
        };
    }

    private static int GetGhostAttributeBonus(BaseAttributeType type)
    {
        return type switch
        {
            BaseAttributeType.SoulPower => 25,
            BaseAttributeType.DivineSense => 20,
            BaseAttributeType.Willpower => 20,
            BaseAttributeType.Comprehension => 10,
            BaseAttributeType.Fortune => 10,
            _ => 0
        };
    }

    #endregion

    #region 流派分支判断

    private static bool IsMageBranch(BranchType type)
    {
        return type == BranchType.DivineSense_Strength ||
               type == BranchType.DivineSense_Control ||
               type == BranchType.DivineSense_Attack ||
               type == BranchType.DivineSense_Defense;
    }

    private static bool IsBodyBranch(BranchType type)
    {
        return type == BranchType.Constitution_Physique ||
               type == BranchType.Constitution_Recovery ||
               type == BranchType.Constitution_Resistance ||
               type == BranchType.Constitution_Tempering;
    }

    private static bool IsSwordBranch(BranchType type)
    {
        return type == BranchType.Constitution_Physique ||
               type == BranchType.Constitution_Tempering ||
               type == BranchType.Willpower_Steadfast ||
               type == BranchType.Willpower_Sacrifice;
    }

    private static bool IsMusicBranch(BranchType type)
    {
        return type == BranchType.DivineSense_Strength ||
               type == BranchType.SoulPower_Strength ||
               type == BranchType.SoulPower_Resonance ||
               type == BranchType.Charm_Charm;
    }

    private static bool IsArtifactBranch(BranchType type)
    {
        return type == BranchType.VitalEnergy_Storage ||
               type == BranchType.VitalEnergy_Purity ||
               type == BranchType.Dexterity_Precision ||
               type == BranchType.Dexterity_Speed;
    }

    private static bool IsTalismanBranch(BranchType type)
    {
        return type == BranchType.SoulPower_Enchant ||
               type == BranchType.Dexterity_Precision ||
               type == BranchType.Dexterity_Innovation ||
               type == BranchType.DivineSense_Control;
    }

    private static bool IsFormationBranch(BranchType type)
    {
        return type == BranchType.DivineSense_Strength ||
               type == BranchType.Comprehension_Insight ||
               type == BranchType.Comprehension_Integration ||
               type == BranchType.Dexterity_Flexibility;
    }

    private static bool IsPillBranch(BranchType type)
    {
        return type == BranchType.DivineSense_Control ||
               type == BranchType.VitalEnergy_Purity ||
               type == BranchType.Dexterity_Precision ||
               type == BranchType.VitalEnergy_Storage;
    }

    private static bool IsGhostBranch(BranchType type)
    {
        return type == BranchType.SoulPower_Strength ||
               type == BranchType.SoulPower_Toughness ||
               type == BranchType.Willpower_Steadfast ||
               type == BranchType.DivineSense_Attack;
    }

    #endregion
}
