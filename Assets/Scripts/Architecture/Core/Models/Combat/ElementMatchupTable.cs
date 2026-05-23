using System;
using UnityEngine;

/// <summary>
/// 元素克制倍率表。Json 载体，从 <c>Resources/Config/ElementMatchupTable.json</c> 加载。
/// 表项 = "攻击元素 vs 防御元素 -> 倍率(百分比)"。
/// 默认 100（普通），200=克制（双倍），50=被克制（半伤），300=极克。
/// 缺省值由代码兜底（见 <see cref="GetMultiplier"/>），保证就算没 Json 也能跑。
/// </summary>
[Serializable]
public sealed class ElementMatchupTable
{
    public ElementMatchupEntry[] entries = Array.Empty<ElementMatchupEntry>();

    public int GetMultiplier(CombatElement attacker, CombatElement defender)
    {
        if (attacker == CombatElement.None || defender == CombatElement.None)
        {
            return 100;
        }

        if (entries != null)
        {
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry != null && entry.attacker == attacker && entry.defender == defender)
                {
                    return entry.multiplierPercent;
                }
            }
        }

        return DefaultMatchup(attacker, defender);
    }

    /// <summary>
    /// 五行相生克 + 阴阳互克 + 雷克水 / 冰克木 / 火克冰 等代码兜底。
    /// 真正的平衡数值请走 Json 表覆盖。
    /// </summary>
    public static int DefaultMatchup(CombatElement attacker, CombatElement defender)
    {
        // 五行相克：金克木、木克土、土克水、水克火、火克金
        if (Beats(attacker, defender, CombatElement.Metal, CombatElement.Wood)) return 200;
        if (Beats(attacker, defender, CombatElement.Wood, CombatElement.Earth)) return 200;
        if (Beats(attacker, defender, CombatElement.Earth, CombatElement.Water)) return 200;
        if (Beats(attacker, defender, CombatElement.Water, CombatElement.Fire)) return 200;
        if (Beats(attacker, defender, CombatElement.Fire, CombatElement.Metal)) return 200;
        // 反之被克
        if (Beats(defender, attacker, CombatElement.Metal, CombatElement.Wood)) return 50;
        if (Beats(defender, attacker, CombatElement.Wood, CombatElement.Earth)) return 50;
        if (Beats(defender, attacker, CombatElement.Earth, CombatElement.Water)) return 50;
        if (Beats(defender, attacker, CombatElement.Water, CombatElement.Fire)) return 50;
        if (Beats(defender, attacker, CombatElement.Fire, CombatElement.Metal)) return 50;

        // 阴阳互克
        if (attacker == CombatElement.Yin && defender == CombatElement.Yang) return 175;
        if (attacker == CombatElement.Yang && defender == CombatElement.Yin) return 175;

        // 变体规则
        if (attacker == CombatElement.Thunder && defender == CombatElement.Water) return 200;
        if (attacker == CombatElement.Ice && defender == CombatElement.Wood) return 175;
        if (attacker == CombatElement.Fire && defender == CombatElement.Ice) return 200;
        if (attacker == CombatElement.Poison && defender == CombatElement.Wood) return 175;
        if (attacker == CombatElement.Poison && defender == CombatElement.Metal) return 50;

        return 100;
    }

    private static bool Beats(CombatElement a, CombatElement b, CombatElement strong, CombatElement weak)
    {
        return a == strong && b == weak;
    }
}

[Serializable]
public sealed class ElementMatchupEntry
{
    public CombatElement attacker;
    public CombatElement defender;
    [Tooltip("百分比倍率，100 = 普通，200 = 双倍，50 = 半伤。")]
    public int multiplierPercent = 100;
}
