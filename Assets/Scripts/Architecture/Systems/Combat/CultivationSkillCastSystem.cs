using System.Collections.Generic;
using QFramework;

/// <summary>
/// S2 SkillCastSystem：技能释放管线。
/// 流程：取技能定义 → 校验消耗 / 冷却 → 调 <see cref="CultivationDamageSystem"/> 算伤害
///       → 写回 HP / Mana → 调 <see cref="CultivationBuffSystem"/> 上 Buff → 写回冷却。
/// 不直接处理"回合切换 / 行动序列"，只负责"一次施放"的原子结算。
/// 调用方：未来由 <see cref="CultivationBattleSystem"/> 或回合控制器编排，每个回合点一次。
/// </summary>
public sealed class CultivationSkillCastSystem : AbstractSystem
{
    private const string SkillDatabasePath = "Data/SkillDatabase";

    private SkillDatabaseAsset cachedDatabase;
    private CultivationCombatStatsModel statsModel;
    private CultivationSkillModel skillModel;
    private CultivationDamageSystem damageSystem;
    private CultivationBuffSystem buffSystem;

    protected override void OnInit()
    {
        statsModel = this.GetModel<CultivationCombatStatsModel>();
        skillModel = this.GetModel<CultivationSkillModel>();
        damageSystem = this.GetSystem<CultivationDamageSystem>();
        buffSystem = this.GetSystem<CultivationBuffSystem>();
    }

    public SkillDefinition GetDefinition(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
        {
            return null;
        }

        var db = GetDatabase();
        if (db == null || db.skills == null)
        {
            return null;
        }

        for (var i = 0; i < db.skills.Length; i++)
        {
            if (db.skills[i] != null && db.skills[i].id == skillId)
            {
                return db.skills[i];
            }
        }

        return null;
    }

    /// <summary>
    /// 释放一次技能。<paramref name="targetIds"/> 应由调用方根据 <see cref="SkillDefinition.targetKind"/> 提前选好。
    /// 命中、伤害、Buff、冷却都在这里完成；回合推进、动画播放交给上层 BattleSystem。
    /// </summary>
    public SkillCastResult Cast(string casterId, string skillId, IReadOnlyList<string> targetIds, int slotIndex = -1, IGameRandomSource random = null)
    {
        var skill = GetDefinition(skillId);
        if (skill == null)
        {
            return SkillCastResult.Fail("未知技能：" + (skillId ?? string.Empty));
        }

        if (string.IsNullOrWhiteSpace(casterId))
        {
            return SkillCastResult.Fail("无效施法者");
        }

        if (!statsModel.TryGetSnapshot(casterId, out var caster))
        {
            return SkillCastResult.Fail("施法者属性未注册");
        }

        if (slotIndex >= 0 && skillModel.GetCooldown(slotIndex) > 0)
        {
            return SkillCastResult.Fail(skill.displayName + " 仍在冷却。");
        }

        if (skill.manaCost > 0 && caster.CurrentMana < skill.manaCost)
        {
            return SkillCastResult.Fail("法力不足：" + skill.displayName);
        }

        if (skill.manaCost > 0)
        {
            caster.CurrentMana -= skill.manaCost;
            statsModel.SetSnapshot(caster);
        }

        var hits = new List<SkillCastHit>();
        if (targetIds != null && IsHostileTarget(skill.targetKind))
        {
            for (var i = 0; i < targetIds.Count; i++)
            {
                var targetId = targetIds[i];
                if (string.IsNullOrWhiteSpace(targetId))
                {
                    continue;
                }

                if (!statsModel.TryGetSnapshot(targetId, out var defender))
                {
                    continue;
                }

                var resolve = damageSystem.Resolve(in caster, in defender, skill, random);
                if (resolve.IsHit && resolve.Damage > 0)
                {
                    defender.CurrentHp = System.Math.Max(0, defender.CurrentHp - resolve.Damage);
                    statsModel.SetSnapshot(defender);

                    if (skill.appliedBuffIds != null)
                    {
                        for (var b = 0; b < skill.appliedBuffIds.Length; b++)
                        {
                            buffSystem.Apply(targetId, skill.appliedBuffIds[b], casterId);
                        }
                    }
                }

                hits.Add(new SkillCastHit(targetId, resolve));
            }
        }
        else if (skill.targetKind == SkillTargetKind.Self || skill.targetKind == SkillTargetKind.SingleAlly || skill.targetKind == SkillTargetKind.AllAllies)
        {
            // 友方/自身向：只挂自身 Buff，未来扩展治疗类
            // （治疗类可走 selfBuffIds 含 hot，或通过专门的 HealSystem，这里先保留入口）
            if (skill.appliedBuffIds != null)
            {
                var primaryTarget = skill.targetKind == SkillTargetKind.Self ? casterId : (targetIds != null && targetIds.Count > 0 ? targetIds[0] : casterId);
                for (var b = 0; b < skill.appliedBuffIds.Length; b++)
                {
                    buffSystem.Apply(primaryTarget, skill.appliedBuffIds[b], casterId);
                }
            }
        }

        if (skill.selfBuffIds != null)
        {
            for (var b = 0; b < skill.selfBuffIds.Length; b++)
            {
                buffSystem.Apply(casterId, skill.selfBuffIds[b], casterId);
            }
        }

        if (slotIndex >= 0 && skill.cooldownTurns > 0)
        {
            skillModel.SetCooldown(slotIndex, skill.cooldownTurns);
        }

        return SkillCastResult.Ok(skill, hits);
    }

    public void Reload()
    {
        cachedDatabase = null;
    }

    private SkillDatabaseAsset GetDatabase()
    {
        if (cachedDatabase != null)
        {
            return cachedDatabase;
        }

        cachedDatabase = GameData.LoadAsset<SkillDatabaseAsset>(SkillDatabasePath);
        return cachedDatabase;
    }

    private static bool IsHostileTarget(SkillTargetKind kind)
    {
        return kind == SkillTargetKind.SingleEnemy || kind == SkillTargetKind.AllEnemies;
    }
}

public readonly struct SkillCastHit
{
    public readonly string TargetId;
    public readonly DamageResolveResult Resolve;

    public SkillCastHit(string targetId, DamageResolveResult resolve)
    {
        TargetId = targetId ?? string.Empty;
        Resolve = resolve;
    }
}

public readonly struct SkillCastResult
{
    public readonly bool Success;
    public readonly string Message;
    public readonly SkillDefinition Skill;
    public readonly IReadOnlyList<SkillCastHit> Hits;

    private SkillCastResult(bool success, string message, SkillDefinition skill, IReadOnlyList<SkillCastHit> hits)
    {
        Success = success;
        Message = message ?? string.Empty;
        Skill = skill;
        Hits = hits ?? System.Array.Empty<SkillCastHit>();
    }

    public static SkillCastResult Ok(SkillDefinition skill, IReadOnlyList<SkillCastHit> hits)
    {
        return new SkillCastResult(true, string.Empty, skill, hits);
    }

    public static SkillCastResult Fail(string reason)
    {
        return new SkillCastResult(false, reason, null, null);
    }
}
