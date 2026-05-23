using System;

/// <summary>
/// 战斗运行时属性快照。攻击方与防御方各传入一份，由 <see cref="CultivationDamageSystem"/> 结算。
/// 设计为 struct + 数组，避免在战斗回合内频繁分配。
/// </summary>
[Serializable]
public struct CombatStatsSnapshot
{
    public string CombatantId;
    public CombatElement Element;

    public int CurrentHp;
    public int MaxHp;
    public int CurrentMana;
    public int MaxMana;

    public int PhysicalAttack;
    public int PhysicalDefense;
    public int SpellAttack;
    public int SpellDefense;
    public int Speed;
    public int HitChance;     // 0~100
    public int CritRate;      // 0~100
    public int CritMultiplierPercent; // 150 = 1.5x
    public int IncomingDamageModifierPercent; // 增/减伤百分比

    /// <summary>各元素抗性百分比（0~100，正数减伤，负数增伤）。索引对应 <see cref="CombatElement"/> 数值。</summary>
    public int[] ElementResistPercents;

    public int GetElementResist(CombatElement element)
    {
        if (ElementResistPercents == null || (int)element < 0 || (int)element >= ElementResistPercents.Length)
        {
            return 0;
        }

        return ElementResistPercents[(int)element];
    }

    public static CombatStatsSnapshot CreateEmpty(string combatantId)
    {
        return new CombatStatsSnapshot
        {
            CombatantId = combatantId ?? string.Empty,
            Element = CombatElement.None,
            ElementResistPercents = new int[CombatElementExtensions.Count],
            HitChance = 100,
            CritMultiplierPercent = 150,
        };
    }
}
