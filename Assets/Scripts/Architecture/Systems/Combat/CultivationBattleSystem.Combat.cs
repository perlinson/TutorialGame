using System.Collections.Generic;
using UnityEngine;

public sealed partial class CultivationBattleSystem
{
    public CombatTurnResult ResolveDirectAttackTurn(CombatTurnContext context, ExpeditionEnemyState target, int damage, string missSummary)
    {
        if (!IsCombatContextValid(context))
        {
            return BuildFailedTurn("当前无法继续执行战斗动作。", context);
        }

        if (target == null || !target.IsAlive)
        {
            return BuildOngoingTurn(context, string.IsNullOrWhiteSpace(missSummary) ? "你挥出近身法器，但没能逼到敌方身前。" : missSummary, null);
        }

        var dealt = DealDamage(target, damage);
        return ResolveHeroTurn(context, "你近身斩向 " + target.Name + "，造成 " + dealt + " 点伤害。");
    }

    public CombatTurnResult ResolveSkillTurn(CombatTurnContext context, int skillIndex)
    {
        if (!IsCombatContextValid(context) || skillIndex < 0 || skillIndex >= context.Hero.Skills.Count)
        {
            return BuildFailedTurn("当前无法继续施展术法。", context);
        }

        var skill = context.Hero.Skills[skillIndex];
        string heroActionSummary;
        switch (skill.Id)
        {
            case "alchemist_fireburst":
                heroActionSummary = PerformAlchemistFireburst(context, skill);
                break;
            case "alchemist_restore":
                heroActionSummary = PerformAlchemistRestore(context, skill);
                break;
            case "alchemist_poison":
                heroActionSummary = PerformAlchemistPoison(context, skill);
                break;
            case "alchemist_barrier":
                heroActionSummary = PerformAlchemistBarrier(context, skill);
                break;
            case "wanderer_bind":
                heroActionSummary = PerformWandererBind(context, skill);
                break;
            case "wanderer_drain":
                heroActionSummary = PerformWandererDrain(context, skill);
                break;
            case "wanderer_mist":
                heroActionSummary = PerformWandererMist(context, skill);
                break;
            case "wanderer_counter":
                heroActionSummary = PerformWandererCounter(context, skill);
                break;
            case "sword_cleave":
                heroActionSummary = PerformSwordCleave(context, skill);
                break;
            case "sword_break":
                heroActionSummary = PerformSwordBreak(context, skill);
                break;
            case "sword_calm":
                heroActionSummary = PerformSwordCalm(context, skill);
                break;
            default:
                heroActionSummary = PerformSwordStrike(context, skill);
                break;
        }

        return ResolveHeroTurn(context, heroActionSummary);
    }

    public CombatTurnResult ResolveTalismanTurn(CombatTurnContext context)
    {
        if (!IsCombatContextValid(context) || context.Hero.TalismanCharges <= 0)
        {
            return BuildFailedTurn("当前无法催动符箓。", context);
        }

        context.Hero.TalismanCharges--;
        string summary;
        switch (context.Hero.ArchetypeId)
        {
            case "alchemist":
                summary = "清秽镇煞符爆开，所有敌人被压制。";
                for (var i = 0; i < context.Enemies.Count; i++)
                {
                    if (!context.Enemies[i].IsAlive)
                    {
                        continue;
                    }

                    DealDamage(context.Enemies[i], context.Hero.Loadout.TalismanPowerBonus + context.Hero.AttackBonus);
                    ApplyExpose(context.Enemies[i], 1);
                }

                ApplyStress(context, -6, null);
                break;
            case "wanderer":
                summary = "缚灵摄气符精准贴上前列敌人，直接令其失衡。";
                var target = GetFirstAliveEnemy(context);
                if (target != null)
                {
                    ApplyStun(target, 1);
                    ApplyExpose(target, 1 + context.Hero.Loadout.TalismanPowerBonus / 2);
                }

                context.Torchlight = Mathf.Min(100, context.Torchlight + 4);
                break;
            default:
                summary = "护体剑符亮起，在周身凝出一层锋锐护体。";
                context.Hero.GuardValue = Mathf.Max(context.Hero.GuardValue, 4 + context.Hero.DefenseBonus + context.Hero.Loadout.TalismanPowerBonus);
                context.Torchlight = Mathf.Min(100, context.Torchlight + 6);
                break;
        }

        return ResolveHeroTurn(context, summary);
    }

    public CombatTurnResult ResolveMedicineTurn(CombatTurnContext context)
    {
        if (!IsCombatContextValid(context) || context.Hero.MedicineCharges <= 0)
        {
            return BuildFailedTurn("当前无法服用丹药。", context);
        }

        context.Hero.MedicineCharges--;
        string summary;
        switch (context.Hero.ArchetypeId)
        {
            case "alchemist":
                HealHero(context, 7 + context.Hero.Loadout.MedicinePowerBonus);
                ApplyStress(context, -10, null);
                summary = "服下回春护脉丹，气血和心境都明显回稳。";
                break;
            case "wanderer":
                HealHero(context, 4 + context.Hero.Loadout.MedicinePowerBonus);
                ApplyStress(context, -14, null);
                context.Torchlight = Mathf.Min(100, context.Torchlight + 8);
                summary = "凝神散入口即化，让神识和脚步重新变得轻稳。";
                break;
            default:
                HealHero(context, 6 + context.Hero.Loadout.MedicinePowerBonus);
                summary = "小还丹迅速化开，受损经络被短暂稳住。";
                break;
        }

        return ResolveHeroTurn(context, summary);
    }

    private CombatTurnResult ResolveHeroTurn(CombatTurnContext context, string heroActionSummary)
    {
        RemoveExpiredEnemyStates(context.Enemies);
        if (GetAliveEnemyCount(context.Enemies) == 0)
        {
            return CompleteCombat(context, heroActionSummary);
        }

        var runtimeNotes = new List<string>();
        var enemyTurnSummary = ResolveEnemyTurn(context, runtimeNotes);
        if (context.Hero.CurrentHealth <= 0)
        {
            return BuildFailedTurn("远征队在 " + context.Region.DisplayName + " 深处彻底溃散。", context);
        }

        RemoveExpiredEnemyStates(context.Enemies);
        if (GetAliveEnemyCount(context.Enemies) == 0)
        {
            return CompleteCombat(context, CombineTurnSummary(heroActionSummary, enemyTurnSummary, runtimeNotes));
        }

        context.CombatRound++;
        return BuildOngoingTurn(context, CombineTurnSummary(heroActionSummary, enemyTurnSummary, runtimeNotes), null);
    }

    private CombatTurnResult CompleteCombat(CombatTurnContext context, string prefix)
    {
        context.Room.Resolved = true;
        var taskUpdates = new List<string>();
        for (var i = 0; i < context.CurrentEncounterSnapshot.Count; i++)
        {
            var factionSnapshot = factionSystem.RecordDefeat(context.SaveData, context.CurrentEncounterSnapshot[i].Faction, context.Region.Id, 1);
            AppendFactionUpdate(taskUpdates, factionSnapshot);
            var progress = taskSystem.RecordProgress(context.SaveData, new TaskProgressSignal
            {
                Type = TaskProgressSignalType.DefeatFaction,
                FactionValue = context.CurrentEncounterSnapshot[i].Faction,
                Count = 1
            });
            AppendTaskUpdate(taskUpdates, progress);
        }

        var loot = rewardSystem.BuildEncounterLoot(context);
        rewardSystem.MergeLoot(context.PendingItemRewards, loot);
        for (var lootIndex = 0; lootIndex < loot.Count; lootIndex++)
        {
            var stack = loot[lootIndex];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            var progress = taskSystem.RecordProgress(context.SaveData, new TaskProgressSignal
            {
                Type = TaskProgressSignalType.ObtainTaskEvidence,
                StringValue = stack.itemId,
                Count = stack.quantity
            });
            AppendTaskUpdate(taskUpdates, progress);
        }
        context.CurrentEncounterSnapshot.Clear();

        var qiReward = context.Room.Kind == ExpeditionRoomKind.Boss
            ? 5 + context.Region.DangerRank
            : context.Room.Kind == ExpeditionRoomKind.Elite ? 4 + context.Region.DangerRank : 2 + context.Region.RequiredRealmTier;
        var crystalReward = context.Room.Kind == ExpeditionRoomKind.Boss ? 3 + context.Region.RequiredRealmTier : context.Room.Kind == ExpeditionRoomKind.Elite ? 2 : 1;
        context.PendingQiGain += qiReward;
        context.PendingCrystalGain += crystalReward;

        var logMessage = prefix + "\n战斗结束，获得修为 +" + qiReward + " / 灵石 +" + crystalReward + "。";
        if (loot.Count > 0)
        {
            logMessage += "\n战利品：" + InventoryLibrary.DescribeLoot(loot) + "。";
        }

        if (taskUpdates.Count > 0)
        {
            logMessage += "\n" + string.Join("\n", taskUpdates);
        }

        var hint = context.CurrentRoomIndex >= 0 && context.Room != null && context.Room.Kind == ExpeditionRoomKind.Boss
            ? "核心险地已肃清，收拢战果后便可返程。"
            : "此室已经稳住，可以决定是继续前进还是先修整。";

        var result = BuildOngoingTurn(context, logMessage, hint);
        result.CombatCleared = true;
        return result;
    }

    private static void AppendTaskUpdate(List<string> taskUpdates, TaskProgressResult progress)
    {
        if (taskUpdates == null || progress == null || string.IsNullOrWhiteSpace(progress.Message))
        {
            return;
        }

        if (!taskUpdates.Contains(progress.Message))
        {
            taskUpdates.Add(progress.Message);
        }
    }

    private static void AppendFactionUpdate(List<string> taskUpdates, FactionReputationSnapshot snapshot)
    {
        if (taskUpdates == null || snapshot == null || snapshot.PressureLevel <= 0)
        {
            return;
        }

        var message = snapshot.DisplayName + " 势力戒备：" + snapshot.AttitudeLabel + "。";
        if (!taskUpdates.Contains(message))
        {
            taskUpdates.Add(message);
        }
    }

    private string ResolveEnemyTurn(CombatTurnContext context, List<string> runtimeNotes)
    {
        var summary = "敌方回合：";
        var alive = GetAliveEnemies(context.Enemies);
        for (var i = 0; i < alive.Count; i++)
        {
            var enemy = alive[i];
            if (enemy.StunnedTurns > 0)
            {
                enemy.StunnedTurns--;
                summary += "\n" + enemy.Name + " 被控在原地，没能出手。";
                continue;
            }

            if (context.Hero.CounterDamage > 0)
            {
                var counter = DealDamage(enemy, context.Hero.CounterDamage);
                summary += "\n" + enemy.Name + " 刚一逼近，就被反制震退，额外承受 " + counter + " 点伤害。";
                context.Hero.CounterDamage = 0;
                if (!enemy.IsAlive)
                {
                    continue;
                }
            }

            summary += "\n" + ResolveEnemyAction(context, enemy, i, runtimeNotes);
            if (context.Hero.CurrentHealth <= 0)
            {
                return summary;
            }
        }

        summary = ApplyEnemyEndOfRoundEffects(context, summary);
        DecayEnemyStatuses(context.Enemies);
        return summary;
    }

    private string ResolveEnemyAction(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex, List<string> runtimeNotes)
    {
        var decision = enemyAiSystem != null
            ? enemyAiSystem.Evaluate(context, enemy, enemyIndex)
            : null;
        if (decision == null)
        {
            return ResolvePhysicalAttack(context, enemy, enemy.TechniqueName, runtimeNotes);
        }

        return ApplyEnemyDecision(context, enemy, decision, runtimeNotes);
    }

    private string ApplyEnemyDecision(CombatTurnContext context, ExpeditionEnemyState enemy, EnemyAiDecision decision, List<string> runtimeNotes)
    {
        if (decision == null)
        {
            return ResolvePhysicalAttack(context, enemy, enemy.TechniqueName, runtimeNotes);
        }

        if (decision.TorchlightDelta != 0)
        {
            context.Torchlight = Mathf.Clamp(context.Torchlight + decision.TorchlightDelta, 8, 100);
        }

        if (decision.SupplyDelta < 0)
        {
            context.Supplies = Mathf.Max(0, context.Supplies + decision.SupplyDelta);
        }

        if (decision.ActionType == EnemyAiActionType.PrepareArmor)
        {
            enemy.Armor = Mathf.Max(0, enemy.Armor + decision.ArmorGain);
            var armorSummary = decision.ArmorGain > 0 ? "，护体 +" + decision.ArmorGain : string.Empty;
            return enemy.Name + " " + decision.ActionLabel + armorSummary + "。";
        }

        if (decision.ActionType == EnemyAiActionType.Stress || decision.ActionType == EnemyAiActionType.StressAndDim)
        {
            var stress = Mathf.Max(1, enemy.StressDamage + decision.StressBonus - context.Hero.StressResistBonus / 4);
            ApplyStress(context, stress, runtimeNotes);
            return enemy.Name + " " + decision.ActionLabel + "，心境 +" + stress + "。";
        }

        var damage = Mathf.Max(1, enemy.Damage + decision.DamageBonus + (context.Torchlight <= 30 ? 1 : 0) - context.Hero.DefenseBonus);
        damage = ConsumeGuard(context, damage);
        ReceiveDamage(context, damage, runtimeNotes);
        var summary = enemy.Name + " " + decision.ActionLabel + "，气血 -" + damage + "。";
        if (decision.SupplyDelta < 0)
        {
            summary += " 补给 -" + Mathf.Abs(decision.SupplyDelta) + "。";
        }

        return summary;
    }

    private static string ResolvePhysicalAttack(CombatTurnContext context, ExpeditionEnemyState enemy, string actionLabel, List<string> runtimeNotes)
    {
        var damage = Mathf.Max(1, enemy.Damage + (context.Torchlight <= 30 ? 1 : 0) - context.Hero.DefenseBonus);
        damage = ConsumeGuard(context, damage);
        ReceiveDamage(context, damage, runtimeNotes);
        return enemy.Name + " " + actionLabel + "，气血 -" + damage + "。";
    }

    private static string ApplyEnemyEndOfRoundEffects(CombatTurnContext context, string currentSummary)
    {
        for (var i = 0; i < context.Enemies.Count; i++)
        {
            var enemy = context.Enemies[i];
            if (!enemy.IsAlive || enemy.PoisonStacks <= 0)
            {
                continue;
            }

            var poisonDamage = enemy.PoisonStacks;
            enemy.CurrentHealth = Mathf.Max(0, enemy.CurrentHealth - poisonDamage);
            enemy.PoisonStacks = Mathf.Max(0, enemy.PoisonStacks - 1);
            currentSummary += "\n丹火毒焰灼烧 " + enemy.Name + "，造成 " + poisonDamage + " 点持续伤害。";
        }

        return currentSummary;
    }

    private static string PerformSwordStrike(CombatTurnContext context, ExpeditionSkillDefinition skill)
    {
        var target = GetFirstAliveEnemy(context);
        if (target == null)
        {
            return skill.Name + "落空。";
        }

        var dealt = DealDamage(target, 4 + context.Hero.AttackBonus + TorchAttackBonus(context));
        return skill.Name + "命中 " + target.Name + "，造成 " + dealt + " 点伤害。";
    }

    private static string PerformSwordCleave(CombatTurnContext context, ExpeditionSkillDefinition skill)
    {
        var summary = string.Empty;
        var targets = GetFirstAliveEnemies(context, 2);
        for (var i = 0; i < targets.Count; i++)
        {
            var dealt = DealDamage(targets[i], 3 + context.Hero.AttackBonus);
            summary += (i == 0 ? string.Empty : "\n") + skill.Name + "扫中 " + targets[i].Name + "，造成 " + dealt + " 点伤害。";
        }

        context.Hero.GuardValue = Mathf.Max(context.Hero.GuardValue, 1 + context.Hero.DefenseBonus);
        return string.IsNullOrEmpty(summary) ? skill.Name + "没有找到目标。" : summary + "\n身法回锋让你暂时更难被击破。";
    }

    private static string PerformSwordBreak(CombatTurnContext context, ExpeditionSkillDefinition skill)
    {
        var target = GetPriorityEnemy(context);
        if (target == null)
        {
            return skill.Name + "落空。";
        }

        target.ExposedTurns = 2;
        var dealt = DealDamage(target, 5 + context.Hero.AttackBonus + 2);
        return skill.Name + "撕开了 " + target.Name + " 的护体，造成 " + dealt + " 点伤害，并让其两回合破绽大开。";
    }

    private string PerformSwordCalm(CombatTurnContext context, ExpeditionSkillDefinition skill)
    {
        HealHero(context, 3);
        ApplyStress(context, -16, null);
        context.Torchlight = Mathf.Min(100, context.Torchlight + 4);
        return skill.Name + "使剑心归一，气血恢复 3，心境明显回稳。";
    }

    private static string PerformAlchemistFireburst(CombatTurnContext context, ExpeditionSkillDefinition skill)
    {
        var summary = string.Empty;
        var targets = GetFirstAliveEnemies(context, 2);
        for (var i = 0; i < targets.Count; i++)
        {
            var dealt = DealDamage(targets[i], 3 + context.Hero.AttackBonus);
            var poisonStacks = ApplyPoison(targets[i], 1);
            summary += (i == 0 ? string.Empty : "\n") + skill.Name + "炸向 " + targets[i].Name + "，造成 " + dealt + " 点伤害" + (poisonStacks > 0 ? "并附着丹火。" : "，但对方体质克毒。");
        }

        context.Torchlight = Mathf.Min(100, context.Torchlight + 5);
        return string.IsNullOrEmpty(summary) ? skill.Name + "未能命中目标。" : summary;
    }

    private string PerformAlchemistRestore(CombatTurnContext context, ExpeditionSkillDefinition skill)
    {
        HealHero(context, 5 + context.Hero.DefenseBonus);
        ApplyStress(context, -12, null);
        return skill.Name + "化成药雾，恢复气血并安抚队伍心境。";
    }

    private static string PerformAlchemistPoison(CombatTurnContext context, ExpeditionSkillDefinition skill)
    {
        var target = GetPriorityEnemy(context);
        if (target == null)
        {
            return skill.Name + "未能锁定目标。";
        }

        var dealt = DealDamage(target, 2 + context.Hero.AttackBonus);
        var poisonStacks = ApplyPoison(target, 3);
        return skill.Name + "侵入 " + target.Name + " 体内，造成 " + dealt + " 点伤害" + (poisonStacks > 0 ? "并叠加 " + poisonStacks + " 层毒焰。" : "，但其体质几乎不受毒焰影响。");
    }

    private static string PerformAlchemistBarrier(CombatTurnContext context, ExpeditionSkillDefinition skill)
    {
        context.Hero.GuardValue = Mathf.Max(context.Hero.GuardValue, 4 + context.Hero.DefenseBonus);
        context.Torchlight = Mathf.Min(100, context.Torchlight + 8);
        return skill.Name + "在周身凝起丹火屏障，下轮会大幅减伤。";
    }

    private static string PerformWandererBind(CombatTurnContext context, ExpeditionSkillDefinition skill)
    {
        var target = GetFirstAliveEnemy(context);
        if (target == null)
        {
            return skill.Name + "落空。";
        }

        var dealt = DealDamage(target, 2 + context.Hero.AttackBonus);
        var stunned = ApplyStun(target, 1);
        return skill.Name + "缠住 " + target.Name + "，造成 " + dealt + " 点伤害" + (stunned ? "并令其失衡一回合。" : "，但对方硬生生顶住了符力。");
    }

    private static string PerformWandererDrain(CombatTurnContext context, ExpeditionSkillDefinition skill)
    {
        var target = GetPriorityEnemy(context);
        if (target == null)
        {
            return skill.Name + "未能吸住对方气机。";
        }

        var dealt = DealDamage(target, 3 + context.Hero.AttackBonus);
        HealHero(context, 2);
        context.Torchlight = Mathf.Min(100, context.Torchlight + 3);
        return skill.Name + "抽走 " + target.Name + " 的灵息，造成 " + dealt + " 点伤害并恢复自身。";
    }

    private string PerformWandererMist(CombatTurnContext context, ExpeditionSkillDefinition skill)
    {
        context.Hero.GuardValue = Mathf.Max(context.Hero.GuardValue, 3 + context.Hero.DefenseBonus);
        ApplyStress(context, -10, null);
        return skill.Name + "借符雾藏形，暂时压住心境并降低承伤。";
    }

    private static string PerformWandererCounter(CombatTurnContext context, ExpeditionSkillDefinition skill)
    {
        context.Hero.CounterDamage = 3 + context.Hero.AttackBonus;
        context.Hero.GuardValue = Mathf.Max(context.Hero.GuardValue, 2 + context.Hero.DefenseBonus);
        return skill.Name + "让你暂时不争先手，等待敌方露出空门后反制。";
    }
}
