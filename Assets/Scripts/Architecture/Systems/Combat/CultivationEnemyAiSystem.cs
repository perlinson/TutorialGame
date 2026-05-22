using System.Collections.Generic;
using QFramework;
using UnityEngine;

internal enum EnemyAiActionType
{
    Attack,
    Stress,
    AttackAndDim,
    StressAndDim,
    StealSupply,
    PrepareArmor
}

internal sealed class EnemyAiDecision
{
    public string RoleLabel;
    public string IntentLabel;
    public string ActionLabel;
    public EnemyAiActionType ActionType;
    public int DamageBonus;
    public int StressBonus;
    public int TorchlightDelta;
    public int SupplyDelta;
    public int ArmorGain;
}

public sealed class CultivationEnemyAiSystem : AbstractSystem
{
    protected override void OnInit()
    {
    }

    public EnemyIntentPreview[] PreviewIntents(CombatTurnContext context)
    {
        if (context == null || context.Enemies == null || context.Enemies.Count == 0)
        {
            return new EnemyIntentPreview[0];
        }

        var previews = new EnemyIntentPreview[context.Enemies.Count];
        for (var i = 0; i < context.Enemies.Count; i++)
        {
            var enemy = context.Enemies[i];
            if (enemy == null || !enemy.IsAlive)
            {
                previews[i] = null;
                continue;
            }

            var decision = Evaluate(context, enemy, i);
            previews[i] = new EnemyIntentPreview
            {
                RoleLabel = decision.RoleLabel,
                IntentLabel = decision.IntentLabel,
                DetailText = decision.ActionLabel
            };
        }

        return previews;
    }

    internal EnemyAiDecision Evaluate(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex)
    {
        switch (ResolveRoleId(enemy))
        {
            case "bandit_saboteur":
                return EvaluateBanditSaboteur(context, enemy, enemyIndex);
            case "bandit_captain":
                return EvaluateBanditCaptain(context, enemy, enemyIndex);
            case "cultivator_hexer":
                return EvaluateCultivatorHexer(context, enemy, enemyIndex);
            case "cultivator_ritualist":
                return EvaluateCultivatorRitualist(context, enemy, enemyIndex);
            case "beast_stalker":
                return EvaluateBeastStalker(context, enemy, enemyIndex);
            case "beast_brute":
                return EvaluateBeastBrute(context, enemy, enemyIndex);
            case "heart_whisperer":
                return EvaluateHeartWhisperer(context, enemy, enemyIndex);
            case "heart_breaker":
                return EvaluateHeartBreaker(context, enemy, enemyIndex);
            default:
                return EvaluateCorpseWarden(context, enemy, enemyIndex);
        }
    }

    private static EnemyAiDecision EvaluateBanditSaboteur(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex)
    {
        if (context.Supplies > 0 && (context.CombatRound + context.CurrentRoomIndex + enemyIndex) % 3 == 0)
        {
            return new EnemyAiDecision
            {
                RoleLabel = "破灯贼",
                IntentLabel = "劫补给",
                ActionLabel = "掷灰夺灯并顺手搜走补给",
                ActionType = EnemyAiActionType.StealSupply,
                TorchlightDelta = -5,
                SupplyDelta = -1
            };
        }

        if (context.Torchlight > 28)
        {
            return new EnemyAiDecision
            {
                RoleLabel = "破灯贼",
                IntentLabel = "压火光",
                ActionLabel = "挥刃逼近并遮压火光",
                ActionType = EnemyAiActionType.AttackAndDim,
                DamageBonus = -1,
                TorchlightDelta = -4
            };
        }

        return new EnemyAiDecision
        {
            RoleLabel = "破灯贼",
            IntentLabel = "近身突袭",
            ActionLabel = "借乱近身抢攻",
            ActionType = EnemyAiActionType.Attack
        };
    }

    private static EnemyAiDecision EvaluateBanditCaptain(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex)
    {
        if (context.Hero.GuardValue > 0 && (context.CombatRound + enemyIndex) % 2 == 1)
        {
            return new EnemyAiDecision
            {
                RoleLabel = "匪首",
                IntentLabel = "重击破架",
                ActionLabel = "趁护体未稳重斩压身",
                ActionType = EnemyAiActionType.Attack,
                DamageBonus = 2
            };
        }

        return new EnemyAiDecision
        {
            RoleLabel = "匪首",
            IntentLabel = "威逼压灯",
            ActionLabel = "挥刃逼近并压低火光",
            ActionType = EnemyAiActionType.AttackAndDim,
            DamageBonus = 1,
            TorchlightDelta = -3
        };
    }

    private static EnemyAiDecision EvaluateCultivatorHexer(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex)
    {
        if (context.Hero.Stress >= 50 || context.Torchlight <= 32 || (context.CombatRound + enemyIndex) % 2 == 0)
        {
            return new EnemyAiDecision
            {
                RoleLabel = "侵神邪修",
                IntentLabel = "侵心咒压",
                ActionLabel = "催动邪诀直接侵心",
                ActionType = EnemyAiActionType.Stress,
                StressBonus = 2
            };
        }

        if (enemy.Armor <= 0 && enemy.IsElite)
        {
            return new EnemyAiDecision
            {
                RoleLabel = "侵神邪修",
                IntentLabel = "凝血护体",
                ActionLabel = "以血符护住经脉",
                ActionType = EnemyAiActionType.PrepareArmor,
                ArmorGain = 2,
                TorchlightDelta = -1
            };
        }

        return new EnemyAiDecision
        {
            RoleLabel = "侵神邪修",
            IntentLabel = "邪火穿身",
            ActionLabel = "裹挟阴火突进",
            ActionType = EnemyAiActionType.AttackAndDim,
            TorchlightDelta = -2
        };
    }

    private static EnemyAiDecision EvaluateCultivatorRitualist(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex)
    {
        if (enemy.Armor <= 1 && (context.CombatRound + enemyIndex) % 2 == 0)
        {
            return new EnemyAiDecision
            {
                RoleLabel = "祭使",
                IntentLabel = "立咒结障",
                ActionLabel = "先结出一层护体邪障",
                ActionType = EnemyAiActionType.PrepareArmor,
                ArmorGain = 2
            };
        }

        return new EnemyAiDecision
        {
            RoleLabel = "祭使",
            IntentLabel = "扰神施压",
            ActionLabel = "借祭纹拉高心境压力",
            ActionType = EnemyAiActionType.StressAndDim,
            StressBonus = 1,
            TorchlightDelta = -2
        };
    }

    private static EnemyAiDecision EvaluateBeastStalker(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex)
    {
        if (context.Torchlight <= 26)
        {
            return new EnemyAiDecision
            {
                RoleLabel = "伏猎妖兽",
                IntentLabel = "借暗扑杀",
                ActionLabel = "贴着暗处猛扑",
                ActionType = EnemyAiActionType.Attack,
                DamageBonus = 2
            };
        }

        if ((context.CombatRound + enemyIndex) % 2 == 0)
        {
            return new EnemyAiDecision
            {
                RoleLabel = "伏猎妖兽",
                IntentLabel = "低吼逼心",
                ActionLabel = "以低吼和阴影逼压心神",
                ActionType = EnemyAiActionType.Stress,
                StressBonus = 1
            };
        }

        return new EnemyAiDecision
        {
            RoleLabel = "伏猎妖兽",
            IntentLabel = "撕咬追击",
            ActionLabel = "贴身连扑",
            ActionType = EnemyAiActionType.Attack,
            DamageBonus = 1
        };
    }

    private static EnemyAiDecision EvaluateBeastBrute(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex)
    {
        return new EnemyAiDecision
        {
            RoleLabel = "蛮横妖兽",
            IntentLabel = enemy.CurrentHealth <= enemy.MaxHealth / 2 ? "暴怒重击" : "正面冲撞",
            ActionLabel = enemy.CurrentHealth <= enemy.MaxHealth / 2 ? "负伤后愈发凶猛地硬冲" : "凭蛮力正面撞来",
            ActionType = EnemyAiActionType.Attack,
            DamageBonus = enemy.CurrentHealth <= enemy.MaxHealth / 2 ? 2 : 1
        };
    }

    private static EnemyAiDecision EvaluateHeartWhisperer(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex)
    {
        return new EnemyAiDecision
        {
            RoleLabel = "惑心残影",
            IntentLabel = context.Hero.Stress >= 60 ? "诱发崩念" : "低语侵神",
            ActionLabel = context.Hero.Stress >= 60 ? "挑动你心底最不稳的那根弦" : "借幻念反复叠压心神",
            ActionType = EnemyAiActionType.StressAndDim,
            StressBonus = context.Hero.Stress >= 60 ? 3 : 1,
            TorchlightDelta = context.Torchlight > 20 ? -2 : 0
        };
    }

    private static EnemyAiDecision EvaluateHeartBreaker(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex)
    {
        if ((context.CombatRound + enemyIndex) % 3 == 1 && context.Torchlight > 34)
        {
            return new EnemyAiDecision
            {
                RoleLabel = "魇念魔影",
                IntentLabel = "幻身扑杀",
                ActionLabel = "借仍算明亮的视野制造错觉扑杀",
                ActionType = EnemyAiActionType.AttackAndDim,
                DamageBonus = 1,
                TorchlightDelta = -4
            };
        }

        return new EnemyAiDecision
        {
            RoleLabel = "魇念魔影",
            IntentLabel = "侵心拔念",
            ActionLabel = "反复勾动执念",
            ActionType = EnemyAiActionType.Stress,
            StressBonus = 2
        };
    }

    private static EnemyAiDecision EvaluateCorpseWarden(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex)
    {
        if (enemy.Armor <= 1 && (context.CombatRound + enemyIndex) % 2 == 1)
        {
            return new EnemyAiDecision
            {
                RoleLabel = "尸傀守卫",
                IntentLabel = "阴骨稳架",
                ActionLabel = "先把阴骨重新架稳",
                ActionType = EnemyAiActionType.PrepareArmor,
                ArmorGain = 1
            };
        }

        return new EnemyAiDecision
        {
            RoleLabel = "尸傀守卫",
            IntentLabel = "裹煞压进",
            ActionLabel = "裹着阴煞向前硬顶",
            ActionType = EnemyAiActionType.AttackAndDim,
            DamageBonus = 1,
            TorchlightDelta = -2
        };
    }

    private static string ResolveRoleId(ExpeditionEnemyState enemy)
    {
        if (enemy == null)
        {
            return "corpse_warden";
        }

        switch (enemy.Faction)
        {
            case ExpeditionEnemyFaction.Bandit:
                return enemy.IsElite || ContainsAny(enemy.Name, "头目", "悍匪") ? "bandit_captain" : "bandit_saboteur";
            case ExpeditionEnemyFaction.Cultivator:
                return enemy.IsElite || ContainsAny(enemy.Name, "祭使", "魔焰") ? "cultivator_ritualist" : "cultivator_hexer";
            case ExpeditionEnemyFaction.Beast:
                return ContainsAny(enemy.Name, "狼", "山魈") ? "beast_stalker" : "beast_brute";
            case ExpeditionEnemyFaction.HeartDemon:
                return enemy.IsElite || ContainsAny(enemy.Name, "魔主", "幻身") ? "heart_breaker" : "heart_whisperer";
            default:
                return "corpse_warden";
        }
    }

    private static bool ContainsAny(string value, params string[] needles)
    {
        if (string.IsNullOrWhiteSpace(value) || needles == null)
        {
            return false;
        }

        for (var i = 0; i < needles.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(needles[i]) && value.Contains(needles[i]))
            {
                return true;
            }
        }

        return false;
    }
}
