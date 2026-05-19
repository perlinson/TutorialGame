using System.Collections.Generic;
using NUnit.Framework;

public sealed class CultivationConditionSystemEditModeTests
{
    [Test]
    public void AreEventConditionsMet_CountsPendingRewardsForHasItemCondition()
    {
        var system = new CultivationConditionSystem();
        var saveData = new MainMenuSaveData
        {
            heroName = "测试修士",
            archetypeId = "sword",
            archetypeName = "流云剑修",
            storageItems = new[] { new SaveItemStack("green_spirit_sand", 1) }
        };
        saveData.EnsureDefaults();

        var context = new CombatTurnContext
        {
            SaveData = saveData,
            PendingItemRewards = new List<SaveItemStack> { new SaveItemStack("green_spirit_sand", 1) }
        };
        var conditions = new[]
        {
            new EventCondition
            {
                Type = EventConditionType.HasItem,
                StringValue = "green_spirit_sand",
                IntValue = 2
            }
        };

        var result = system.AreEventConditionsMet(conditions, context, null);

        Assert.That(result, Is.True);
    }

    [Test]
    public void BuildRequirementText_ReturnsRealmRequirementWhenRealmTierTooLow()
    {
        var system = new CultivationConditionSystem();
        var saveData = new MainMenuSaveData
        {
            heroName = "测试修士",
            archetypeId = "sword",
            archetypeName = "流云剑修",
            realmTier = 0
        };
        saveData.EnsureDefaults();

        var context = new CombatTurnContext
        {
            SaveData = saveData
        };
        var conditions = new[]
        {
            new EventCondition
            {
                Type = EventConditionType.RealmTierAtLeast,
                IntValue = 2
            }
        };

        var result = system.BuildRequirementText(conditions, context, null, null);

        Assert.That(result, Is.EqualTo("至少达到 " + WorldRegionLibrary.GetRealmName(2) + "。"));
    }

    [Test]
    public void BuildRequirementText_PrefersOverrideTextWhenConditionsUnmet()
    {
        var system = new CultivationConditionSystem();
        var context = new CombatTurnContext
        {
            Torchlight = 12
        };
        var conditions = new[]
        {
            new EventCondition
            {
                Type = EventConditionType.TorchlightAtLeast,
                IntValue = 20
            }
        };

        var result = system.BuildRequirementText(conditions, context, null, "需要更亮的火光。");

        Assert.That(result, Is.EqualTo("需要更亮的火光。"));
    }
}
