using System.Collections.Generic;
using UnityEngine;

public sealed class ExpeditionEventEffectExecutionContext
{
    public CombatTurnContext CombatContext;
    public TaskContextSnapshot TaskContext;
    public ExpeditionEventOptionResult Result;
    public CultivationTaskSystem TaskSystem;
    public CultivationRewardSystem RewardSystem;
    public CultivationMindStateSystem MindStateSystem;
}

public interface IExpeditionEventEffectHandler
{
    EventEffectType EffectType { get; }
    void Apply(EventEffect effect, ExpeditionEventEffectExecutionContext context);
}

public sealed class ExpeditionEventEffectExecutor
{
    private readonly Dictionary<EventEffectType, IExpeditionEventEffectHandler> handlers =
        new Dictionary<EventEffectType, IExpeditionEventEffectHandler>();

    public ExpeditionEventEffectExecutor(params IExpeditionEventEffectHandler[] effectHandlers)
    {
        if (effectHandlers == null)
        {
            return;
        }

        for (var i = 0; i < effectHandlers.Length; i++)
        {
            var handler = effectHandlers[i];
            if (handler == null)
            {
                continue;
            }

            handlers[handler.EffectType] = handler;
        }
    }

    public static ExpeditionEventEffectExecutor CreateDefault()
    {
        return new ExpeditionEventEffectExecutor(
            new GainPendingQiHandler(),
            new GainPendingCrystalsHandler(),
            new ModifyTorchlightHandler(),
            new ModifySuppliesHandler(),
            new HealHeroHandler(),
            new ModifyStressHandler(),
            new ReceiveDamageHandler(),
            new AddPendingItemHandler(),
            new AddTaskFlagHandler(),
            new AddTaskProgressHandler());
    }

    public void Apply(EventEffect[] effects, ExpeditionEventEffectExecutionContext context)
    {
        if (effects == null || context == null || context.CombatContext == null || context.Result == null)
        {
            return;
        }

        for (var i = 0; i < effects.Length; i++)
        {
            var effect = effects[i];
            if (effect == null)
            {
                continue;
            }

            IExpeditionEventEffectHandler handler;
            if (!handlers.TryGetValue(effect.Type, out handler))
            {
                continue;
            }

            handler.Apply(effect, context);
            if (context.Result.ExpeditionFailed)
            {
                return;
            }
        }
    }

    private sealed class GainPendingQiHandler : IExpeditionEventEffectHandler
    {
        public EventEffectType EffectType => EventEffectType.GainPendingQi;

        public void Apply(EventEffect effect, ExpeditionEventEffectExecutionContext context)
        {
            context.Result.PendingQiGain += effect.IntValue;
        }
    }

    private sealed class GainPendingCrystalsHandler : IExpeditionEventEffectHandler
    {
        public EventEffectType EffectType => EventEffectType.GainPendingCrystals;

        public void Apply(EventEffect effect, ExpeditionEventEffectExecutionContext context)
        {
            context.Result.PendingCrystalGain += effect.IntValue;
        }
    }

    private sealed class ModifyTorchlightHandler : IExpeditionEventEffectHandler
    {
        public EventEffectType EffectType => EventEffectType.ModifyTorchlight;

        public void Apply(EventEffect effect, ExpeditionEventEffectExecutionContext context)
        {
            context.Result.Torchlight = Mathf.Clamp(context.Result.Torchlight + effect.IntValue, 0, 100);
        }
    }

    private sealed class ModifySuppliesHandler : IExpeditionEventEffectHandler
    {
        public EventEffectType EffectType => EventEffectType.ModifySupplies;

        public void Apply(EventEffect effect, ExpeditionEventEffectExecutionContext context)
        {
            context.Result.Supplies = Mathf.Max(0, context.Result.Supplies + effect.IntValue);
        }
    }

    private sealed class HealHeroHandler : IExpeditionEventEffectHandler
    {
        public EventEffectType EffectType => EventEffectType.HealHero;

        public void Apply(EventEffect effect, ExpeditionEventEffectExecutionContext context)
        {
            var hero = context.CombatContext.Hero;
            if (hero == null)
            {
                return;
            }

            hero.CurrentHealth = Mathf.Min(hero.MaxHealth, hero.CurrentHealth + Mathf.Max(0, effect.IntValue));
        }
    }

    private sealed class ModifyStressHandler : IExpeditionEventEffectHandler
    {
        public EventEffectType EffectType => EventEffectType.ModifyStress;

        public void Apply(EventEffect effect, ExpeditionEventEffectExecutionContext context)
        {
            if (context.MindStateSystem == null)
            {
                return;
            }

            var mindResult = context.MindStateSystem.ApplyStress(context.CombatContext, effect.IntValue);
            if (mindResult.ExpeditionFailed)
            {
                context.Result.ExpeditionFailed = true;
                context.Result.FailureReason = mindResult.FailureReason;
                return;
            }

            if (!string.IsNullOrWhiteSpace(mindResult.Message) && mindResult.BreakdownTriggered)
            {
                context.Result.LogMessage = string.IsNullOrWhiteSpace(context.Result.LogMessage)
                    ? mindResult.Message
                    : context.Result.LogMessage + "\n" + mindResult.Message;
            }
        }
    }

    private sealed class ReceiveDamageHandler : IExpeditionEventEffectHandler
    {
        public EventEffectType EffectType => EventEffectType.ReceiveDamage;

        public void Apply(EventEffect effect, ExpeditionEventEffectExecutionContext context)
        {
            var combatContext = context.CombatContext;
            if (combatContext.Hero == null)
            {
                return;
            }

            combatContext.Hero.CurrentHealth = Mathf.Max(0, combatContext.Hero.CurrentHealth - Mathf.Max(0, effect.IntValue));
            if (combatContext.Hero.CurrentHealth > 0)
            {
                return;
            }

            context.Result.ExpeditionFailed = true;
            context.Result.FailureReason = combatContext.Region == null
                ? "远征队彻底溃散。"
                : "远征队在 " + combatContext.Region.DisplayName + " 深处彻底溃散。";
        }
    }

    private sealed class AddPendingItemHandler : IExpeditionEventEffectHandler
    {
        public EventEffectType EffectType => EventEffectType.AddPendingItem;

        public void Apply(EventEffect effect, ExpeditionEventEffectExecutionContext context)
        {
            if (context.RewardSystem == null)
            {
                return;
            }

            context.RewardSystem.AddPendingItem(
                context.CombatContext.PendingItemRewards,
                effect.StringValue,
                Mathf.Max(1, effect.IntValue));
        }
    }

    private sealed class AddTaskFlagHandler : IExpeditionEventEffectHandler
    {
        public EventEffectType EffectType => EventEffectType.AddTaskFlag;

        public void Apply(EventEffect effect, ExpeditionEventEffectExecutionContext context)
        {
            if (context.TaskSystem == null || context.TaskContext == null || string.IsNullOrWhiteSpace(context.TaskContext.ActiveTaskId))
            {
                return;
            }

            context.TaskSystem.AddTaskFlag(context.CombatContext.SaveData, context.TaskContext.ActiveTaskId, effect.StringValue);
        }
    }

    private sealed class AddTaskProgressHandler : IExpeditionEventEffectHandler
    {
        public EventEffectType EffectType => EventEffectType.AddTaskProgress;

        public void Apply(EventEffect effect, ExpeditionEventEffectExecutionContext context)
        {
            if (context.TaskSystem == null)
            {
                return;
            }

            context.TaskSystem.RecordProgress(context.CombatContext.SaveData, new TaskProgressSignal
            {
                Type = TaskProgressSignalType.AddProgressToActiveTask,
                Count = Mathf.Max(1, effect.IntValue)
            });
        }
    }
}
