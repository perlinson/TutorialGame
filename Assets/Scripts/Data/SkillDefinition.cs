using System;
using UnityEngine;

[Serializable]
public sealed class SkillDefinition
{
    public string id;
    public string displayName;
    [TextArea(1, 4)] public string description;

    public CombatElement element = CombatElement.None;
    public SkillTargetKind targetKind = SkillTargetKind.SingleEnemy;
    public SkillCategory category = SkillCategory.Spell;

    [Tooltip("基础威力系数（百分比）。例：100 表示按攻击力 1.0 倍计算基础伤害。")]
    public int basePowerPercent = 100;

    [Tooltip("法力消耗。")]
    public int manaCost = 0;

    [Tooltip("冷却回合数（>0 表示释放后需等待若干回合）。")]
    public int cooldownTurns = 0;

    [Tooltip("命中率（0~100），<=0 视为必中。")]
    public int hitChance = 100;

    [Tooltip("基础暴击率（0~100），与角色暴击率叠加。")]
    public int critRate = 0;

    [Tooltip("暴击倍率系数（百分比）。150 表示 1.5 倍。<=100 表示不暴击。")]
    public int critMultiplierPercent = 150;

    [Tooltip("固定附加伤害（不受攻击力缩放）。")]
    public int flatBonusDamage = 0;

    [Tooltip("命中后追加的 Buff（按 ID 引用 BuffDatabase）。")]
    public string[] appliedBuffIds = Array.Empty<string>();

    [Tooltip("自身释放后获得的 Buff。")]
    public string[] selfBuffIds = Array.Empty<string>();

    public Sprite icon;
}

public enum SkillTargetKind
{
    SingleEnemy = 0,
    AllEnemies = 1,
    Self = 2,
    SingleAlly = 3,
    AllAllies = 4,
}

public enum SkillCategory
{
    Physical = 0, // 物理：攻防对抗
    Spell = 1,    // 法术：法攻 vs 法防
    True = 2,     // 真元：忽略防御
}
