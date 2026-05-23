using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 属性数据生成器：为 SchoolConfigAsset 和 DivinePowerConfigAsset 生成示例实例
/// </summary>
public class AttributeDataGenerator
{
    [MenuItem("Tools/Cultivation/Generate School Configs")]
    public static void GenerateSchoolConfigs()
    {
        var folderPath = "Assets/Resources/Data/Attributes";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 法修
        CreateSchoolConfig(folderPath, "School_Mage", SchoolType.Mage, "法修", "以法术攻击为主，依赖神识和灵根",
            divineSense: 3, comprehension: 2, spiritRoot: 3, willpower: 1, fortune: 1,
            divineSenseBonus: 20, comprehensionBonus: 15, spiritRootBonus: 25, willpowerBonus: 10, fortuneBonus: 5,
            favoredBranches: new[] { BranchType.DivineSense_Strength, BranchType.DivineSense_Control, BranchType.DivineSense_Attack, BranchType.DivineSense_Defense },
            minDivineSense: 30, minComprehension: 20);

        // 体修
        CreateSchoolConfig(folderPath, "School_Body", SchoolType.Body, "体修", "以肉身力量为主，依赖根骨和精元",
            constitution: 3, vitalEnergy: 2, willpower: 2, fortune: 1,
            constitutionBonus: 25, vitalEnergyBonus: 20, willpowerBonus: 15, fortuneBonus: 5,
            favoredBranches: new[] { BranchType.Constitution_Physique, BranchType.Constitution_Recovery, BranchType.Constitution_Resistance, BranchType.Constitution_Tempering },
            minConstitution: 30, minVitalEnergy: 20);

        // 剑修
        CreateSchoolConfig(folderPath, "School_Sword", SchoolType.Sword, "剑修", "以剑术为主，依赖根骨和意志",
            constitution: 3, willpower: 3, divineSense: 2, comprehension: 1, spiritRoot: 1,
            constitutionBonus: 20, willpowerBonus: 25, divineSenseBonus: 15, comprehensionBonus: 10, spiritRootBonus: 5,
            favoredBranches: new[] { BranchType.Constitution_Physique, BranchType.Constitution_Tempering, BranchType.Willpower_Steadfast, BranchType.Willpower_Sacrifice },
            minConstitution: 30, minWillpower: 20);

        // 音修
        CreateSchoolConfig(folderPath, "School_Music", SchoolType.Music, "音修", "以音波攻击为主，依赖神识和魂力",
            divineSense: 3, soulPower: 3, charm: 2, comprehension: 1, willpower: 1,
            divineSenseBonus: 25, soulPowerBonus: 20, charmBonus: 15, comprehensionBonus: 10, willpowerBonus: 5,
            favoredBranches: new[] { BranchType.DivineSense_Strength, BranchType.SoulPower_Strength, BranchType.SoulPower_Resonance, BranchType.Charm_Charm },
            minDivineSense: 30, minSoulPower: 20);

        // 器修
        CreateSchoolConfig(folderPath, "School_Artifact", SchoolType.Artifact, "器修", "以法宝炼制为主，依赖精元和机巧",
            vitalEnergy: 3, dexterity: 3, constitution: 2, divineSense: 1, fortune: 1,
            vitalEnergyBonus: 25, dexterityBonus: 20, constitutionBonus: 15, divineSenseBonus: 10, fortuneBonus: 5,
            favoredBranches: new[] { BranchType.VitalEnergy_Storage, BranchType.VitalEnergy_Purity, BranchType.Dexterity_Precision, BranchType.Dexterity_Speed },
            minVitalEnergy: 30, minDexterity: 20);

        // 符修
        CreateSchoolConfig(folderPath, "School_Talisman", SchoolType.Talisman, "符修", "以符箓绘制为主，依赖魂力和机巧",
            soulPower: 2, dexterity: 3, comprehension: 2, divineSense: 1, fortune: 1,
            soulPowerBonus: 20, dexterityBonus: 25, comprehensionBonus: 15, divineSenseBonus: 10, fortuneBonus: 5,
            favoredBranches: new[] { BranchType.SoulPower_Enchant, BranchType.Dexterity_Precision, BranchType.Dexterity_Innovation, BranchType.DivineSense_Control },
            minSoulPower: 30, minDexterity: 20);

        // 阵修
        CreateSchoolConfig(folderPath, "School_Formation", SchoolType.Formation, "阵修", "以阵法布设为主，依赖神识和悟性",
            divineSense: 3, comprehension: 3, soulPower: 2, dexterity: 1, willpower: 1,
            divineSenseBonus: 25, comprehensionBonus: 20, soulPowerBonus: 15, dexterityBonus: 10, willpowerBonus: 5,
            favoredBranches: new[] { BranchType.DivineSense_Strength, BranchType.Comprehension_Insight, BranchType.Comprehension_Integration, BranchType.Dexterity_Flexibility },
            minDivineSense: 30, minComprehension: 20);

        // 丹修
        CreateSchoolConfig(folderPath, "School_Pill", SchoolType.Pill, "丹修", "以炼丹为主，依赖神识和精元",
            divineSense: 3, vitalEnergy: 2, dexterity: 2, spiritRoot: 2, fortune: 1,
            divineSenseBonus: 20, vitalEnergyBonus: 15, dexterityBonus: 15, spiritRootBonus: 20, fortuneBonus: 10,
            favoredBranches: new[] { BranchType.DivineSense_Control, BranchType.VitalEnergy_Purity, BranchType.Dexterity_Precision, BranchType.VitalEnergy_Storage },
            minDivineSense: 30, minVitalEnergy: 20);

        // 鬼修
        CreateSchoolConfig(folderPath, "School_Ghost", SchoolType.Ghost, "鬼修", "以灵魂力量为主，依赖魂力和意志",
            soulPower: 3, divineSense: 2, willpower: 3, comprehension: 1, fortune: 1,
            soulPowerBonus: 25, divineSenseBonus: 20, willpowerBonus: 20, comprehensionBonus: 10, fortuneBonus: 10,
            favoredBranches: new[] { BranchType.SoulPower_Strength, BranchType.SoulPower_Toughness, BranchType.Willpower_Steadfast, BranchType.DivineSense_Attack },
            minSoulPower: 30, minWillpower: 20);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("School Configs generated successfully!");
    }

    private static void CreateSchoolConfig(string folderPath, string fileName, SchoolType schoolType, string displayName, string description,
        int divineSense = 0, int constitution = 0, int comprehension = 0, int fortune = 0, int charm = 0,
        int soulPower = 0, int vitalEnergy = 0, int willpower = 0, int dexterity = 0, int spiritRoot = 0,
        int divineSenseBonus = 0, int constitutionBonus = 0, int comprehensionBonus = 0, int fortuneBonus = 0, int charmBonus = 0,
        int soulPowerBonus = 0, int vitalEnergyBonus = 0, int willpowerBonus = 0, int dexterityBonus = 0, int spiritRootBonus = 0,
        BranchType[] favoredBranches = null,
        int minDivineSense = 0, int minConstitution = 0, int minComprehension = 0, int minSoulPower = 0, int minVitalEnergy = 0, int minWillpower = 0, int minDexterity = 0)
    {
        var config = ScriptableObject.CreateInstance<SchoolConfigAsset>();
        config.schoolType = schoolType;
        config.displayName = displayName;
        config.description = description;

        config.divineSenseWeight = divineSense;
        config.constitutionWeight = constitution;
        config.comprehensionWeight = comprehension;
        config.fortuneWeight = fortune;
        config.charmWeight = charm;
        config.soulPowerWeight = soulPower;
        config.vitalEnergyWeight = vitalEnergy;
        config.willpowerWeight = willpower;
        config.dexterityWeight = dexterity;
        config.spiritRootWeight = spiritRoot;

        config.divineSenseBonus = divineSenseBonus;
        config.constitutionBonus = constitutionBonus;
        config.comprehensionBonus = comprehensionBonus;
        config.fortuneBonus = fortuneBonus;
        config.charmBonus = charmBonus;
        config.soulPowerBonus = soulPowerBonus;
        config.vitalEnergyBonus = vitalEnergyBonus;
        config.willpowerBonus = willpowerBonus;
        config.dexterityBonus = dexterityBonus;
        config.spiritRootBonus = spiritRootBonus;

        config.branchBonusPerLevel = 2;
        config.favoredBranches = favoredBranches ?? new BranchType[0];

        config.minDivineSense = minDivineSense;
        config.minConstitution = minConstitution;
        config.minComprehension = minComprehension;
        config.minSoulPower = minSoulPower;
        config.minVitalEnergy = minVitalEnergy;
        config.minWillpower = minWillpower;
        config.minDexterity = minDexterity;

        var assetPath = Path.Combine(folderPath, fileName + ".asset");
        AssetDatabase.CreateAsset(config, assetPath);
    }

    [MenuItem("Tools/Cultivation/Generate Divine Power Configs")]
    public static void GenerateDivinePowerConfigs()
    {
        var folderPath = "Assets/Resources/Data/Attributes";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 小神通示例
        CreateDivinePowerConfig(folderPath, "DivinePower_IronBone", "铁骨", "提升防御力", DivinePowerType.Minor,
            triggerType: DivinePowerConfigAsset.TriggerConditionType.AttributeThreshold,
            attributeType: BaseAttributeType.Constitution, attributeThreshold: 50,
            defenseBonus: 20, allResistBonus: 10);

        CreateDivinePowerConfig(folderPath, "DivinePower_LightBody", "轻身", "提升速度", DivinePowerType.Minor,
            triggerType: DivinePowerConfigAsset.TriggerConditionType.AttributeThreshold,
            attributeType: BaseAttributeType.Constitution, attributeThreshold: 50,
            speedBonus: 10);

        CreateDivinePowerConfig(folderPath, "DivinePower_BrightEye", "明目", "提升暴击", DivinePowerType.Minor,
            triggerType: DivinePowerConfigAsset.TriggerConditionType.AttributeThreshold,
            attributeType: BaseAttributeType.DivineSense, attributeThreshold: 50,
            critRateBonus: 5);

        // 大神通示例
        CreateDivinePowerConfig(folderPath, "DivinePower_GoldenBody", "金刚不坏", "大幅提升防御和抗性", DivinePowerType.Passive,
            triggerType: DivinePowerConfigAsset.TriggerConditionType.AttributeThreshold,
            attributeType: BaseAttributeType.Constitution, attributeThreshold: 100,
            defenseBonus: 100, allResistBonus: 20, isPassive: true,
            prerequisites: new[] { new BranchPrereq(BranchType.Constitution_Physique, 50) });

        CreateDivinePowerConfig(folderPath, "DivinePower_FiveElementsEscape", "五行遁术", "元素遁术", DivinePowerType.Major,
            triggerType: DivinePowerConfigAsset.TriggerConditionType.AttributeThreshold,
            attributeType: BaseAttributeType.DivineSense, attributeThreshold: 100,
            speedBonus: 50, hitChanceBonus: 20,
            prerequisites: new[] { new BranchPrereq(BranchType.DivineSense_Strength, 60) });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Divine Power Configs generated successfully!");
    }

    private static void CreateDivinePowerConfig(string folderPath, string fileName, string displayName, string description, DivinePowerType type,
        DivinePowerConfigAsset.TriggerConditionType triggerType = DivinePowerConfigAsset.TriggerConditionType.AttributeThreshold,
        BaseAttributeType attributeType = BaseAttributeType.Constitution, int attributeThreshold = 50,
        int healthBonus = 0, int attackBonus = 0, int defenseBonus = 0, int speedBonus = 0,
        int hitChanceBonus = 0, int critRateBonus = 0, int poisonResistBonus = 0, int stunResistBonus = 0, int allResistBonus = 0,
        bool isPassive = false, BranchPrereq[] prerequisites = null)
    {
        var config = ScriptableObject.CreateInstance<DivinePowerConfigAsset>();
        config.id = fileName;
        config.displayName = displayName;
        config.description = description;
        config.type = type;

        config.triggerCondition = new DivinePowerConfigAsset.TriggerCondition
        {
            conditionType = triggerType,
            attributeType = attributeType,
            attributeThreshold = attributeThreshold
        };

        if (prerequisites != null && prerequisites.Length > 0)
        {
            config.branchPrerequisites = new DivinePowerConfigAsset.BranchPrerequisite[prerequisites.Length];
            for (int i = 0; i < prerequisites.Length; i++)
            {
                config.branchPrerequisites[i] = new DivinePowerConfigAsset.BranchPrerequisite
                {
                    branchType = prerequisites[i].branchType,
                    requiredLevel = prerequisites[i].requiredLevel
                };
            }
        }

        config.effect = new DivinePowerConfigAsset.DivinePowerEffect
        {
            healthBonus = healthBonus,
            attackBonus = attackBonus,
            defenseBonus = defenseBonus,
            speedBonus = speedBonus,
            hitChanceBonus = hitChanceBonus,
            critRateBonus = critRateBonus,
            poisonResistBonus = poisonResistBonus,
            stunResistBonus = stunResistBonus,
            allResistBonus = allResistBonus,
            isPassive = isPassive
        };

        var assetPath = Path.Combine(folderPath, fileName + ".asset");
        AssetDatabase.CreateAsset(config, assetPath);
    }

    private struct BranchPrereq
    {
        public BranchType branchType;
        public int requiredLevel;

        public BranchPrereq(BranchType branchType, int requiredLevel)
        {
            this.branchType = branchType;
            this.requiredLevel = requiredLevel;
        }
    }
}
