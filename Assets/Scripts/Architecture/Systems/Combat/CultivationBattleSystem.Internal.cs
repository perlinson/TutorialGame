using System.Collections.Generic;
using UnityEngine;

public sealed partial class CultivationBattleSystem
{
    private static ExpeditionEnemyState GetFirstAliveEnemy(CombatTurnContext context)
    {
        var alive = GetAliveEnemies(context.Enemies);
        return alive.Count > 0 ? alive[0] : null;
    }

    private static ExpeditionEnemyState GetPriorityEnemy(CombatTurnContext context)
    {
        var alive = GetAliveEnemies(context.Enemies);
        if (alive.Count == 0)
        {
            return null;
        }

        for (var i = 0; i < alive.Count; i++)
        {
            if (alive[i].IsElite)
            {
                return alive[i];
            }
        }

        return alive[0];
    }

    private static List<ExpeditionEnemyState> GetFirstAliveEnemies(CombatTurnContext context, int count)
    {
        var alive = GetAliveEnemies(context.Enemies);
        if (alive.Count <= count)
        {
            return alive;
        }

        return alive.GetRange(0, count);
    }

    private static List<ExpeditionEnemyState> GetAliveEnemies(List<ExpeditionEnemyState> enemies)
    {
        var alive = new List<ExpeditionEnemyState>();
        for (var i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsAlive)
            {
                alive.Add(enemies[i]);
            }
        }

        return alive;
    }

    private static int GetAliveEnemyCount(List<ExpeditionEnemyState> enemies)
    {
        var count = 0;
        for (var i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsAlive)
            {
                count++;
            }
        }

        return count;
    }

    private static int DealDamage(ExpeditionEnemyState enemy, int rawDamage)
    {
        if (enemy == null)
        {
            return 0;
        }

        var dealt = Mathf.Max(1, rawDamage - enemy.GetEffectiveArmor());
        enemy.CurrentHealth = Mathf.Max(0, enemy.CurrentHealth - dealt);
        return dealt;
    }

    private static int ApplyPoison(ExpeditionEnemyState enemy, int stacks)
    {
        if (enemy == null || stacks <= 0)
        {
            return 0;
        }

        var actualStacks = Mathf.Max(0, stacks - enemy.PoisonResistance);
        enemy.PoisonStacks += actualStacks;
        return actualStacks;
    }

    private static bool ApplyStun(ExpeditionEnemyState enemy, int turns)
    {
        if (enemy == null || turns <= 0)
        {
            return false;
        }

        var actualTurns = Mathf.Max(0, turns - enemy.StunResistance);
        if (actualTurns <= 0)
        {
            return false;
        }

        enemy.StunnedTurns = Mathf.Max(enemy.StunnedTurns, actualTurns);
        return true;
    }

    private static int ApplyExpose(ExpeditionEnemyState enemy, int turns)
    {
        if (enemy == null || turns <= 0)
        {
            return 0;
        }

        enemy.ExposedTurns = Mathf.Max(enemy.ExposedTurns, turns);
        return enemy.ExposedTurns;
    }

    private static void RemoveExpiredEnemyStates(List<ExpeditionEnemyState> enemies)
    {
        enemies.RemoveAll(enemy => enemy == null || !enemy.IsAlive);
    }

    private static void DecayEnemyStatuses(List<ExpeditionEnemyState> enemies)
    {
        for (var i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].ExposedTurns > 0)
            {
                enemies[i].ExposedTurns--;
            }
        }
    }

    private static int ConsumeGuard(CombatTurnContext context, int damage)
    {
        if (context.Hero.GuardValue <= 0)
        {
            return damage;
        }

        var reduced = Mathf.Max(1, damage - context.Hero.GuardValue);
        context.Hero.GuardValue = 0;
        return reduced;
    }

    private static int TorchAttackBonus(CombatTurnContext context)
    {
        return context.Torchlight >= 65 ? 1 : 0;
    }

    private static void HealHero(CombatTurnContext context, int amount)
    {
        context.Hero.CurrentHealth = Mathf.Min(context.Hero.MaxHealth, context.Hero.CurrentHealth + Mathf.Max(0, amount));
    }

    private static void ReceiveDamage(CombatTurnContext context, int amount, List<string> runtimeNotes)
    {
        context.Hero.CurrentHealth = Mathf.Max(0, context.Hero.CurrentHealth - Mathf.Max(0, amount));
        if (context.Hero.CurrentHealth <= 0)
        {
            AppendRuntimeNote(runtimeNotes, "远征队在 " + context.Region.DisplayName + " 深处彻底溃散。");
        }
    }

    private void ApplyStress(CombatTurnContext context, int amount, List<string> runtimeNotes)
    {
        var mindResult = mindStateSystem.ApplyStress(context, amount);
        if (mindResult.ExpeditionFailed)
        {
            AppendRuntimeNote(runtimeNotes, mindResult.FailureReason);
            return;
        }

        if (!string.IsNullOrWhiteSpace(mindResult.Message) && mindResult.BreakdownTriggered)
        {
            AppendRuntimeNote(runtimeNotes, mindResult.Message);
        }
    }

    private void ApplyStress(CombatTurnContext context, ExpeditionRoomActionResult result, int amount)
    {
        var mindResult = mindStateSystem.ApplyStress(context, amount);
        if (mindResult.ExpeditionFailed)
        {
            result.ExpeditionFailed = true;
            result.FailureReason = mindResult.FailureReason;
            return;
        }

        if (!string.IsNullOrWhiteSpace(mindResult.Message) && mindResult.BreakdownTriggered)
        {
            result.LogMessage = string.IsNullOrWhiteSpace(result.LogMessage)
                ? mindResult.Message
                : result.LogMessage + "\n" + mindResult.Message;
        }
    }

    private static string AppendPrimary(string existing, string primary)
    {
        if (string.IsNullOrWhiteSpace(existing))
        {
            return primary;
        }

        return primary + "\n" + existing;
    }

    private static void AppendRuntimeNote(List<string> runtimeNotes, string note)
    {
        if (runtimeNotes == null || string.IsNullOrWhiteSpace(note))
        {
            return;
        }

        runtimeNotes.Add(note);
    }

    private static string CombineTurnSummary(string heroActionSummary, string enemyTurnSummary, List<string> runtimeNotes)
    {
        var summary = string.IsNullOrWhiteSpace(enemyTurnSummary) ? heroActionSummary : heroActionSummary + "\n" + enemyTurnSummary;
        if (runtimeNotes == null || runtimeNotes.Count == 0)
        {
            return summary;
        }

        for (var i = 0; i < runtimeNotes.Count; i++)
        {
            summary += "\n" + runtimeNotes[i];
        }

        return summary;
    }

    private static bool IsCombatContextValid(CombatTurnContext context)
    {
        return context != null && context.Region != null && context.Hero != null && context.Enemies != null && context.Room != null;
    }

    private static CombatTurnResult BuildOngoingTurn(CombatTurnContext context, string logMessage, string hintMessage)
    {
        return new CombatTurnResult
        {
            CombatRound = context != null ? context.CombatRound : 0,
            Torchlight = context != null ? context.Torchlight : 0,
            Supplies = context != null ? context.Supplies : 0,
            PendingQiGain = context != null ? context.PendingQiGain : 0,
            PendingCrystalGain = context != null ? context.PendingCrystalGain : 0,
            LogMessage = logMessage,
            HintMessage = hintMessage
        };
    }

    private static CombatTurnResult BuildFailedTurn(string failureReason, CombatTurnContext context)
    {
        var result = BuildOngoingTurn(context, string.Empty, string.Empty);
        result.ExpeditionFailed = true;
        result.FailureReason = failureReason;
        return result;
    }
}
