using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class CultivationBattleSystemEditModeTests
{
    [Test]
    public void ResolveDirectAttackTurn_ReturnsFailureForInvalidContext()
    {
        var system = new CultivationBattleSystem();

        var result = system.ResolveDirectAttackTurn(null, null, 3, "miss");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ExpeditionFailed, Is.True);
        Assert.That(result.FailureReason, Is.EqualTo("当前无法继续执行战斗动作。"));
    }

    [Test]
    public void ResolveDirectAttackTurn_UsesMissSummaryWhenTargetIsMissing()
    {
        var system = new CultivationBattleSystem();
        var context = CreateCombatContext();

        var result = system.ResolveDirectAttackTurn(context, null, 3, "你扑了个空。");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ExpeditionFailed, Is.False);
        Assert.That(result.LogMessage, Is.EqualTo("你扑了个空。"));
        Assert.That(result.CombatRound, Is.EqualTo(context.CombatRound));
    }

    [Test]
    public void ResolveRoomEvent_ScoutRoomGrantsQiAndTorchlight()
    {
        var system = new CultivationBattleSystem();
        var context = CreateCombatContext();
        context.Room.Kind = ExpeditionRoomKind.Scout;
        context.Region.RequiredRealmTier = 2;
        context.Torchlight = 30;
        context.PendingQiGain = 1;

        var result = system.ResolveRoomEvent(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ExpeditionFailed, Is.False);
        Assert.That(result.PendingQiGain, Is.EqualTo(4));
        Assert.That(result.Torchlight, Is.EqualTo(38));
        Assert.That(result.LogMessage, Does.Contain("修为 +3"));
    }

    [Test]
    public void SkipRoom_MarksRoomResolved()
    {
        var system = new CultivationBattleSystem();
        var context = CreateCombatContext();
        context.Room.Title = "旧日回廊";

        var result = system.SkipRoom(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.RoomResolved, Is.True);
        Assert.That(context.Room.Resolved, Is.True);
        Assert.That(result.LogMessage, Does.Contain("旧日回廊"));
    }

    private static CombatTurnContext CreateCombatContext()
    {
        return new CombatTurnContext
        {
            SaveData = new CultivationSaveData
            {
                heroName = "测试修士",
                archetypeId = "sword",
                archetypeName = "流云剑修"
            },
            Region = new WorldRegionDefinition
            {
                Id = "test_region",
                DisplayName = "测试地域",
                RequiredRealmTier = 1,
                DangerRank = 1,
                UnlockRegionIds = new string[0]
            },
            Room = new ExpeditionRoomState
            {
                Kind = ExpeditionRoomKind.Battle,
                Title = "测试房间"
            },
            Hero = new ExpeditionHeroState
            {
                HeroName = "测试修士",
                MaxHealth = 10,
                CurrentHealth = 10,
                Loadout = new ExpeditionEquipmentLoadout()
            },
            Enemies = new List<ExpeditionEnemyState>(),
            CurrentEncounterSnapshot = new List<ExpeditionEnemyState>(),
            PendingItemRewards = new List<SaveItemStack>(),
            CombatRound = 2,
            Torchlight = 24,
            Supplies = 2
        };
    }
}
