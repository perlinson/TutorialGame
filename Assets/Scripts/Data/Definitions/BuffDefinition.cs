using System;
using UnityEngine;

[Serializable]
public sealed class BuffDefinition
{
    public string id;
    public string displayName;
    [TextArea(1, 3)] public string description;

    public BuffKind kind = BuffKind.Buff;
    public BuffStackingRule stackingRule = BuffStackingRule.RefreshDuration;
    public int defaultDurationTurns = 3;
    public int maxStacks = 1;

    [Tooltip("每回合 tick 时对目标造成的伤害（>0 表示伤害，<0 表示治疗）。")]
    public int tickDamage = 0;

    [Tooltip("元素属性，用于 tick 伤害与抗性结算。")]
    public CombatElement element = CombatElement.None;

    [Tooltip("攻击力修正（百分比，正负皆可）。")]
    public int attackModifierPercent = 0;
    [Tooltip("防御力修正（百分比）。")]
    public int defenseModifierPercent = 0;
    [Tooltip("速度修正（百分比）。")]
    public int speedModifierPercent = 0;
    [Tooltip("命中率修正（绝对值，正负皆可）。")]
    public int hitChanceModifier = 0;
    [Tooltip("受到伤害修正（百分比，正数表示增伤，负数表示减伤）。")]
    public int incomingDamageModifierPercent = 0;

    public Sprite icon;
}

public enum BuffKind
{
    Buff = 0,        // 增益
    Debuff = 1,      // 减益
    Dot = 2,         // 持续伤害
    Hot = 3,         // 持续治疗
    Control = 4,     // 控制类（冰封 / 锁灵 / 眩晕）
}

public enum BuffStackingRule
{
    Independent = 0,      // 各自独立结算，不合并
    RefreshDuration = 1,  // 重复施加：刷新持续时间，层数不变
    StackUpToMax = 2,     // 重复施加：层数 +1（不超过 maxStacks），刷新持续时间
    Replace = 3,          // 重复施加：替换为新的
}
