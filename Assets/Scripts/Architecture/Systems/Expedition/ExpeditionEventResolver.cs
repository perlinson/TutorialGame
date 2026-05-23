using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ExpeditionEventRuntimeContext
{
    public CombatTurnContext CombatContext;
    public TaskContextSnapshot TaskContext;
}

public sealed class ExpeditionEventResolver
{
    public ExpeditionEventDefinition Resolve(
        ExpeditionEventDefinition[] definitions,
        ExpeditionEventRuntimeContext runtimeContext,
        Func<ExpeditionEventDefinition, ExpeditionEventRuntimeContext, bool> eligibilityPredicate)
    {
        return Resolve(definitions, runtimeContext, eligibilityPredicate, BuildStableSeed(runtimeContext));
    }

    public ExpeditionEventDefinition Resolve(
        ExpeditionEventDefinition[] definitions,
        ExpeditionEventRuntimeContext runtimeContext,
        Func<ExpeditionEventDefinition, ExpeditionEventRuntimeContext, bool> eligibilityPredicate,
        int seed)
    {
        if (definitions == null || definitions.Length == 0 || eligibilityPredicate == null)
        {
            return null;
        }

        var candidates = CollectCandidates(definitions, runtimeContext, eligibilityPredicate);
        if (candidates.Count == 0)
        {
            return null;
        }

        var highestPriority = int.MinValue;
        for (var i = 0; i < candidates.Count; i++)
        {
            highestPriority = Mathf.Max(highestPriority, candidates[i].GetSelectionPriority());
        }

        var priorityCandidates = new List<ExpeditionEventDefinition>();
        var totalWeight = 0;
        for (var i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            if (candidate.GetSelectionPriority() != highestPriority)
            {
                continue;
            }

            priorityCandidates.Add(candidate);
            totalWeight += Mathf.Max(1, candidate.Weight);
        }

        if (priorityCandidates.Count == 0)
        {
            return null;
        }

        if (priorityCandidates.Count == 1)
        {
            return priorityCandidates[0];
        }

        var randomSource = new System.Random(seed);
        var roll = randomSource.Next(0, Mathf.Max(1, totalWeight));
        var accumulated = 0;
        for (var i = 0; i < priorityCandidates.Count; i++)
        {
            accumulated += Mathf.Max(1, priorityCandidates[i].Weight);
            if (roll < accumulated)
            {
                return priorityCandidates[i];
            }
        }

        return priorityCandidates[0];
    }

    public List<ExpeditionEventDefinition> CollectCandidates(
        ExpeditionEventDefinition[] definitions,
        ExpeditionEventRuntimeContext runtimeContext,
        Func<ExpeditionEventDefinition, ExpeditionEventRuntimeContext, bool> eligibilityPredicate)
    {
        var candidates = new List<ExpeditionEventDefinition>();
        if (definitions == null || eligibilityPredicate == null)
        {
            return candidates;
        }

        for (var i = 0; i < definitions.Length; i++)
        {
            var definition = definitions[i];
            if (definition == null || !eligibilityPredicate(definition, runtimeContext))
            {
                continue;
            }

            candidates.Add(definition);
        }

        return candidates;
    }

    private static int BuildStableSeed(ExpeditionEventRuntimeContext runtimeContext)
    {
        if (runtimeContext == null || runtimeContext.CombatContext == null || runtimeContext.CombatContext.Room == null)
        {
            return 0;
        }

        var combatContext = runtimeContext.CombatContext;
        var taskContext = runtimeContext.TaskContext;
        return combatContext.Room.Seed * 31
               + combatContext.CurrentRoomIndex * 17
               + SafeHash(taskContext != null ? taskContext.ActiveTaskId : string.Empty)
               + combatContext.PendingQiGain * 7
               + combatContext.PendingCrystalGain * 13;
    }

    private static int SafeHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        unchecked
        {
            var hash = 17;
            for (var i = 0; i < value.Length; i++)
            {
                hash = hash * 31 + value[i];
            }

            return hash;
        }
    }
}
