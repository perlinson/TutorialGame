using UnityEngine;
using QFramework;

/// <summary>
/// S3 DamageSystem：单步伤害结算。
/// 公式（v1，简洁可调）：
///   raw = max(0, atk * skill.basePower% / 100 - def) + skill.flatBonusDamage
///   raw *= elementMul / 100
///   raw *= incomingDamageModifier (target)
///   if crit: raw *= critMul / 100
///   final = max(1, round(raw)) （除非 raw <= 0 时为 0）
///   忽略防御 = 真元类（<see cref="SkillCategory.True"/>）
/// 命中判定使用 hitChance + skill.hitChance 简单点击。
/// </summary>
public sealed class CultivationDamageSystem : AbstractSystem
{
    private const string ElementMatchupConfigPath = "Config/ElementMatchupTable";

    private ElementMatchupTable matchupTable;

    protected override void OnInit()
    {
    }

    public DamageResolveResult Resolve(
        in CombatStatsSnapshot attacker,
        in CombatStatsSnapshot defender,
        SkillDefinition skill,
        IGameRandomSource random = null)
    {
        if (skill == null)
        {
            return DamageResolveResult.Miss("无效技能");
        }

        // 命中判定
        var hitChance = Mathf.Clamp(attacker.HitChance + skill.hitChance, 0, 100);
        if (hitChance < 100)
        {
            var hitRoll = NextRoll(random, 0, 100);
            if (hitRoll >= hitChance)
            {
                return DamageResolveResult.Miss("攻击未命中");
            }
        }

        // 攻防选择
        int atk;
        int def;
        switch (skill.category)
        {
            case SkillCategory.Physical:
                atk = attacker.PhysicalAttack;
                def = defender.PhysicalDefense;
                break;
            case SkillCategory.Spell:
                atk = attacker.SpellAttack;
                def = defender.SpellDefense;
                break;
            case SkillCategory.True:
            default:
                atk = Mathf.Max(attacker.PhysicalAttack, attacker.SpellAttack);
                def = 0;
                break;
        }

        var basePower = skill.basePowerPercent <= 0 ? 100 : skill.basePowerPercent;
        var raw = Mathf.Max(0, (atk * basePower / 100) - def) + Mathf.Max(0, skill.flatBonusDamage);

        // 元素克制
        var elementMul = GetMatchupTable().GetMultiplier(skill.element, defender.Element);
        if (elementMul != 100)
        {
            raw = raw * elementMul / 100;
        }

        // 元素抗性
        var resist = defender.GetElementResist(skill.element);
        if (resist != 0)
        {
            raw = raw * Mathf.Clamp(100 - resist, 0, 200) / 100;
        }

        // 入射伤害修正
        if (defender.IncomingDamageModifierPercent != 0)
        {
            raw = raw * Mathf.Clamp(100 + defender.IncomingDamageModifierPercent, 0, 500) / 100;
        }

        // 暴击
        var critRate = Mathf.Clamp(attacker.CritRate + skill.critRate, 0, 100);
        var isCrit = false;
        if (critRate > 0)
        {
            var critRoll = NextRoll(random, 0, 100);
            if (critRoll < critRate)
            {
                var critMul = skill.critMultiplierPercent > 100 ? skill.critMultiplierPercent : Mathf.Max(150, attacker.CritMultiplierPercent);
                raw = raw * critMul / 100;
                isCrit = true;
            }
        }

        var final = raw <= 0 ? 0 : Mathf.Max(1, raw);
        return DamageResolveResult.Hit(final, isCrit, skill.element, elementMul);
    }

    public void Reload()
    {
        matchupTable = null;
    }

    private ElementMatchupTable GetMatchupTable()
    {
        if (matchupTable != null)
        {
            return matchupTable;
        }

        matchupTable = GameData.LoadJson<ElementMatchupTable>(ElementMatchupConfigPath);
        if (matchupTable == null)
        {
            matchupTable = new ElementMatchupTable();
        }

        return matchupTable;
    }

    private static int NextRoll(IGameRandomSource random, int minInclusive, int maxExclusive)
    {
        if (random != null)
        {
            return random.Range(minInclusive, maxExclusive);
        }

        return Random.Range(minInclusive, maxExclusive);
    }
}

public readonly struct DamageResolveResult
{
    public readonly bool IsHit;
    public readonly int Damage;
    public readonly bool IsCrit;
    public readonly CombatElement Element;
    public readonly int ElementMultiplierPercent;
    public readonly string MissReason;

    private DamageResolveResult(bool hit, int damage, bool isCrit, CombatElement element, int elementMul, string missReason)
    {
        IsHit = hit;
        Damage = damage;
        IsCrit = isCrit;
        Element = element;
        ElementMultiplierPercent = elementMul;
        MissReason = missReason ?? string.Empty;
    }

    public static DamageResolveResult Miss(string reason) => new DamageResolveResult(false, 0, false, CombatElement.None, 100, reason);
    public static DamageResolveResult Hit(int damage, bool isCrit, CombatElement element, int elementMul) => new DamageResolveResult(true, damage, isCrit, element, elementMul, string.Empty);
}

/// <summary>抽象随机源占位，给 S3 / S4 / S2 共用。U4 IGameRandomService 落地后再统一替换。</summary>
public interface IGameRandomSource
{
    int Range(int minInclusive, int maxExclusive);
    float Range01();
}
