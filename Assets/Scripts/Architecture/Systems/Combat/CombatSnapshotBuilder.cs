using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// CombatSnapshotBuilder：将远征战斗上下文（CombatTurnContext）转换为战斗属性快照（CombatStatsSnapshot）。
/// 用于桥接旧战斗系统与新战斗核心系统（DamageSystem/BuffSystem/SkillCastSystem）。
/// 集成流派加成和分支系统。
/// </summary>
public static class CombatSnapshotBuilder
{
    private static CultivationBranchSystem branchSystem;
    private static CultivationSchoolSystem schoolSystem;

    /// <summary>
    /// 初始化系统引用（在 CultivationApp.Init 后调用）
    /// </summary>
    public static void Initialize()
    {
        var app = CultivationApp.Interface;
        if (app != null)
        {
            branchSystem = app.GetSystem<CultivationBranchSystem>();
            schoolSystem = app.GetSystem<CultivationSchoolSystem>();
        }
    }

    /// <summary>
    /// 从英雄状态构建玩家战斗属性快照。
    /// </summary>
    public static CombatStatsSnapshot BuildHeroSnapshot(ExpeditionHeroState hero)
    {
        if (hero == null)
        {
            return default;
        }

        // 获取流派加成
        var schoolBonus = schoolSystem != null ? schoolSystem.GetSchoolAttributeBonus(BaseAttributeType.Constitution) : 0;
        var divineSenseBonus = schoolSystem != null ? schoolSystem.GetSchoolAttributeBonus(BaseAttributeType.DivineSense) : 0;

        // 获取分支加成
        var physiqueBonus = branchSystem != null ? branchSystem.MapBranchToCombatStat(BranchType.Constitution_Physique) : 0;
        var strengthBonus = branchSystem != null ? branchSystem.MapBranchToCombatStat(BranchType.DivineSense_Strength) : 0;

        return new CombatStatsSnapshot
        {
            CombatantId = "hero",
            MaxHp = hero.MaxHealth + physiqueBonus + (schoolBonus * 2),
            CurrentHp = hero.CurrentHealth,
            PhysicalAttack = hero.AttackBonus + 10 + physiqueBonus / 5, // 基础攻击力 10 + 加成 + 分支加成
            PhysicalDefense = hero.DefenseBonus + 5 + physiqueBonus / 10, // 基础防御力 5 + 加成 + 分支加成
            SpellAttack = hero.AttackBonus + 8 + strengthBonus / 5,     // 基础法攻 8 + 加成 + 分支加成
            SpellDefense = hero.DefenseBonus + 4 + strengthBonus / 10,   // 基础法防 4 + 加成 + 分支加成
            Speed = 10 + (branchSystem != null ? branchSystem.MapBranchToCombatStat(BranchType.Comprehension_Deduction) : 0),
            HitChance = 85 + hero.StressResistBonus / 4 + strengthBonus / 10,
            CritRate = 5 + strengthBonus / 20,
            CritMultiplierPercent = 150,
            IncomingDamageModifierPercent = 0,
            Element = CombatElement.None,
            ElementResistPercents = new int[11] // 对应 CombatElement 枚举长度
        };
    }

    /// <summary>
    /// 从敌人状态构建敌人战斗属性快照。
    /// </summary>
    public static CombatStatsSnapshot BuildEnemySnapshot(ExpeditionEnemyState enemy)
    {
        if (enemy == null)
        {
            return default;
        }

        return new CombatStatsSnapshot
        {
            CombatantId = "enemy",
            MaxHp = enemy.MaxHealth,
            CurrentHp = enemy.CurrentHealth,
            PhysicalAttack = enemy.Damage,
            PhysicalDefense = enemy.Armor,
            SpellAttack = enemy.Damage,
            SpellDefense = enemy.Armor,
            Speed = 8,
            HitChance = 80,
            CritRate = 0,
            CritMultiplierPercent = 120,
            IncomingDamageModifierPercent = enemy.ExposedTurns > 0 ? 50 : 0, // 破绽时增伤 50%
            Element = CombatElement.None,
            ElementResistPercents = new int[11]
        };
    }

    /// <summary>
    /// 从战斗上下文构建所有参与者的属性快照字典。
    /// Key = "hero" 或 "enemy_{index}"
    /// </summary>
    public static Dictionary<string, CombatStatsSnapshot> BuildAllSnapshots(CombatTurnContext context)
    {
        var snapshots = new Dictionary<string, CombatStatsSnapshot>();

        if (context?.Hero != null)
        {
            snapshots["hero"] = BuildHeroSnapshot(context.Hero);
        }

        if (context?.Enemies != null)
        {
            for (var i = 0; i < context.Enemies.Count; i++)
            {
                var enemy = context.Enemies[i];
                if (enemy != null && enemy.IsAlive)
                {
                    snapshots[$"enemy_{i}"] = BuildEnemySnapshot(enemy);
                }
            }
        }

        return snapshots;
    }

    /// <summary>
    /// 将伤害结算结果应用到敌人状态。
    /// </summary>
    public static void ApplyDamageToEnemy(ExpeditionEnemyState enemy, DamageResolveResult result)
    {
        if (enemy == null || !result.IsHit)
        {
            return;
        }

        var finalDamage = Mathf.Max(1, result.Damage - enemy.GetEffectiveArmor());
        enemy.CurrentHealth = Mathf.Max(0, enemy.CurrentHealth - finalDamage);
    }

    /// <summary>
    /// 将伤害结算结果应用到英雄状态。
    /// </summary>
    public static void ApplyDamageToHero(ExpeditionHeroState hero, DamageResolveResult result, int guardValue)
    {
        if (hero == null || !result.IsHit)
        {
            return;
        }

        var damage = result.Damage;
        if (guardValue > 0)
        {
            damage = Mathf.Max(1, damage - guardValue);
        }

        hero.CurrentHealth = Mathf.Max(0, hero.CurrentHealth - damage);
    }
}
