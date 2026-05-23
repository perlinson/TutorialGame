#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 一键生成战斗示例数据：技能库 / Buff 库 / 元素克制表。
/// 入口：菜单 "Cultivation/Combat/Generate Example Data"。
/// 已存在的资产不会被覆盖（避免覆盖手工调整），可手动删后再跑。
/// </summary>
public static class CombatDataExampleGenerator
{
    private const string DataFolder = "Assets/Resources/Data";
    private const string ConfigFolder = "Assets/Resources/Config";
    private const string SkillDatabasePath = DataFolder + "/SkillDatabase.asset";
    private const string BuffDatabasePath = DataFolder + "/BuffDatabase.asset";
    private const string ElementMatchupJsonPath = ConfigFolder + "/ElementMatchupTable.json";

    [MenuItem("Cultivation/Combat/Generate Example Data")]
    public static void Generate()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder(DataFolder);
        EnsureFolder(ConfigFolder);

        GenerateSkillDatabase();
        GenerateBuffDatabase();
        GenerateElementMatchupJson();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CombatDataExampleGenerator] Done. Skill / Buff databases & ElementMatchupTable ready.");
    }

    private static void GenerateSkillDatabase()
    {
        var asset = AssetDatabase.LoadAssetAtPath<SkillDatabaseAsset>(SkillDatabasePath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<SkillDatabaseAsset>();
            AssetDatabase.CreateAsset(asset, SkillDatabasePath);
        }

        if (asset.skills != null && asset.skills.Length > 0)
        {
            return;
        }

        asset.skills = new[]
        {
            new SkillDefinition
            {
                id = "skill_metal_slash",
                displayName = "金锋斩",
                description = "御金气化作锋刃，对单一目标造成金属性伤害。",
                element = CombatElement.Metal,
                category = SkillCategory.Physical,
                targetKind = SkillTargetKind.SingleEnemy,
                basePowerPercent = 130,
                manaCost = 6,
                cooldownTurns = 1,
                hitChance = 95,
                critRate = 10,
                critMultiplierPercent = 160,
            },
            new SkillDefinition
            {
                id = "skill_fire_burst",
                displayName = "焚灵爆",
                description = "凝聚火元素掀起爆炸，伤害全体敌人并附带灼烧。",
                element = CombatElement.Fire,
                category = SkillCategory.Spell,
                targetKind = SkillTargetKind.AllEnemies,
                basePowerPercent = 90,
                manaCost = 14,
                cooldownTurns = 3,
                hitChance = 90,
                critRate = 5,
                critMultiplierPercent = 150,
                appliedBuffIds = new[] { "buff_burning" },
            },
            new SkillDefinition
            {
                id = "skill_water_mend",
                displayName = "玄水回元",
                description = "汇水脉之力为自身回复气血，并提升下一回合的法术防御。",
                element = CombatElement.Water,
                category = SkillCategory.Spell,
                targetKind = SkillTargetKind.Self,
                basePowerPercent = 0,
                manaCost = 8,
                cooldownTurns = 2,
                hitChance = 100,
                selfBuffIds = new[] { "buff_water_guard" },
            },
            new SkillDefinition
            {
                id = "skill_thunder_pierce",
                displayName = "雷罡穿心",
                description = "凝雷成枪，无视部分防御，命中后概率使目标麻痹。",
                element = CombatElement.Thunder,
                category = SkillCategory.True,
                targetKind = SkillTargetKind.SingleEnemy,
                basePowerPercent = 110,
                manaCost = 12,
                cooldownTurns = 4,
                hitChance = 92,
                critRate = 15,
                critMultiplierPercent = 175,
                appliedBuffIds = new[] { "buff_paralysis" },
            },
        };

        EditorUtility.SetDirty(asset);
    }

    private static void GenerateBuffDatabase()
    {
        var asset = AssetDatabase.LoadAssetAtPath<BuffDatabaseAsset>(BuffDatabasePath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<BuffDatabaseAsset>();
            AssetDatabase.CreateAsset(asset, BuffDatabasePath);
        }

        if (asset.buffs != null && asset.buffs.Length > 0)
        {
            return;
        }

        asset.buffs = new[]
        {
            new BuffDefinition
            {
                id = "buff_burning",
                displayName = "灼烧",
                description = "持续受到火属性灼烧伤害。",
                kind = BuffKind.Dot,
                element = CombatElement.Fire,
                stackingRule = BuffStackingRule.StackUpToMax,
                defaultDurationTurns = 3,
                maxStacks = 3,
                tickDamage = 6,
            },
            new BuffDefinition
            {
                id = "buff_water_guard",
                displayName = "玄水护体",
                description = "下一回合大幅减伤。",
                kind = BuffKind.Buff,
                element = CombatElement.Water,
                stackingRule = BuffStackingRule.RefreshDuration,
                defaultDurationTurns = 2,
                maxStacks = 1,
                incomingDamageModifierPercent = -35,
                defenseModifierPercent = 25,
            },
            new BuffDefinition
            {
                id = "buff_paralysis",
                displayName = "麻痹",
                description = "雷电缠身，速度大幅下降，命中下降。",
                kind = BuffKind.Control,
                element = CombatElement.Thunder,
                stackingRule = BuffStackingRule.RefreshDuration,
                defaultDurationTurns = 2,
                maxStacks = 1,
                speedModifierPercent = -40,
                hitChanceModifier = -20,
            },
        };

        EditorUtility.SetDirty(asset);
    }

    private static void GenerateElementMatchupJson()
    {
        if (File.Exists(ElementMatchupJsonPath))
        {
            return;
        }

        var table = new ElementMatchupTable
        {
            entries = new[]
            {
                MakeEntry(CombatElement.Metal, CombatElement.Wood, 200),
                MakeEntry(CombatElement.Wood, CombatElement.Earth, 200),
                MakeEntry(CombatElement.Earth, CombatElement.Water, 200),
                MakeEntry(CombatElement.Water, CombatElement.Fire, 200),
                MakeEntry(CombatElement.Fire, CombatElement.Metal, 200),
                MakeEntry(CombatElement.Yin, CombatElement.Yang, 175),
                MakeEntry(CombatElement.Yang, CombatElement.Yin, 175),
                MakeEntry(CombatElement.Thunder, CombatElement.Water, 200),
                MakeEntry(CombatElement.Ice, CombatElement.Wood, 175),
                MakeEntry(CombatElement.Fire, CombatElement.Ice, 200),
                MakeEntry(CombatElement.Poison, CombatElement.Wood, 175),
                MakeEntry(CombatElement.Poison, CombatElement.Metal, 50),
            }
        };

        var json = JsonUtility.ToJson(table, true);
        File.WriteAllText(ElementMatchupJsonPath, json);
    }

    private static ElementMatchupEntry MakeEntry(CombatElement attacker, CombatElement defender, int multiplierPercent)
    {
        return new ElementMatchupEntry
        {
            attacker = attacker,
            defender = defender,
            multiplierPercent = multiplierPercent,
        };
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        var parent = Path.GetDirectoryName(path);
        var leaf = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent.Replace('\\', '/'));
        }

        AssetDatabase.CreateFolder(parent, leaf);
    }
}
#endif
