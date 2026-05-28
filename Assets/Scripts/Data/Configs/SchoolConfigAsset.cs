using System;
using UnityEngine;

/// <summary>
/// 流派配置数据（ScriptableObject）
/// 用于定义各流派的属性权重、特色技能、解锁条件等
/// </summary>
[CreateAssetMenu(fileName = "SchoolConfig", menuName = "Cultivation/School Config")]
public sealed class SchoolConfigAsset : ScriptableObject
{
    [Header("流派基本信息")]
    public SchoolType schoolType;
    public string displayName;
    [TextArea(2, 4)] public string description;

    [Header("属性权重（1-5星）")]
    public int divineSenseWeight;   // 神识权重
    public int constitutionWeight;  // 根骨权重
    public int comprehensionWeight;  // 悟性权重
    public int fortuneWeight;        // 福缘权重
    public int charmWeight;          // 魅力权重
    public int soulPowerWeight;      // 魂力权重
    public int vitalEnergyWeight;    // 精元权重
    public int willpowerWeight;      // 意志权重
    public int dexterityWeight;      // 机巧权重
    public int spiritRootWeight;     // 灵根权重

    [Header("属性加成（百分比）")]
    public int divineSenseBonus;
    public int constitutionBonus;
    public int comprehensionBonus;
    public int fortuneBonus;
    public int charmBonus;
    public int soulPowerBonus;
    public int vitalEnergyBonus;
    public int willpowerBonus;
    public int dexterityBonus;
    public int spiritRootBonus;

    [Header("分支加成（流派等级×每级加成）")]
    public int branchBonusPerLevel = 2;
    public BranchType[] favoredBranches; // 侧重分支列表

    [Header("选择条件")]
    public int minDivineSense;
    public int minConstitution;
    public int minComprehension;
    public int minSoulPower;
    public int minVitalEnergy;
    public int minWillpower;
    public int minDexterity;

    [Header("特色技能")]
    public string[] signatureSkillIds; // 特色技能ID列表

    [Header("解锁等级奖励")]
    public LevelReward[] levelRewards;

    [Serializable]
    public sealed class LevelReward
    {
        public int level;
        public string rewardDescription;
        public int attributeBonus; // 属性加成值
        public BaseAttributeType attributeType; // 加成属性类型
    }
}
