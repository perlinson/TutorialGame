using System.Collections.Generic;
using UnityEngine;
using QFramework;

public sealed class CultivationMindStateSystem : AbstractSystem
{
    protected override void OnInit()
    {
    }

    public MindStateResult ApplyStress(CombatTurnContext context, int amount)
    {
        if (context == null || context.Hero == null)
        {
            return new MindStateResult();
        }

        return ApplyStress(context.SaveData, context.Hero, context.Region, amount);
    }

    public MindStateResult ApplyStress(ExpeditionTraversalContext context, int amount)
    {
        if (context == null || context.Hero == null)
        {
            return new MindStateResult();
        }

        return ApplyStress(null, context.Hero, context.Region, amount);
    }

    public MindStateResult ApplyStress(MainMenuSaveData saveData, ExpeditionHeroState hero, WorldRegionDefinition region, int amount)
    {
        var result = new MindStateResult();
        if (hero == null)
        {
            return result;
        }

        result.PreviousStress = hero.Stress;
        hero.Stress = Mathf.Clamp(hero.Stress + amount, 0, 120);
        result.CurrentStress = hero.Stress;
        if (hero.Stress < 100)
        {
            result.Message = amount < 0 ? "心境稍稳。" : string.Empty;
            return result;
        }

        hero.Stress = 72;
        result.CurrentStress = hero.Stress;
        result.BreakdownTriggered = true;
        result.HealthDamage = 3 + (region != null ? region.RequiredRealmTier : 0);
        hero.CurrentHealth = Mathf.Max(0, hero.CurrentHealth - result.HealthDamage);
        if (hero.CurrentHealth <= 0)
        {
            result.ExpeditionFailed = true;
            result.FailureReason = region != null ? "远征队在 " + region.DisplayName + " 深处彻底溃散。" : "远征队彻底溃散。";
        }
        else
        {
            result.Message = "心境几乎崩裂，队伍强行稳住神识，但额外损失了气血。";
            AddAffliction(saveData, "mind_crack", 1);
        }

        return result;
    }

    public void AddAffliction(MainMenuSaveData saveData, string afflictionId, int stacks)
    {
        if (saveData == null || string.IsNullOrWhiteSpace(afflictionId) || stacks <= 0)
        {
            return;
        }

        saveData.EnsureDefaults();
        var afflictions = new List<SaveAfflictionState>(saveData.afflictions ?? System.Array.Empty<SaveAfflictionState>());
        for (var i = 0; i < afflictions.Count; i++)
        {
            if (afflictions[i] != null && afflictions[i].afflictionId == afflictionId)
            {
                afflictions[i].stacks += stacks;
                saveData.afflictions = afflictions.ToArray();
                return;
            }
        }

        afflictions.Add(new SaveAfflictionState(afflictionId, stacks));
        saveData.afflictions = afflictions.ToArray();
    }
}
