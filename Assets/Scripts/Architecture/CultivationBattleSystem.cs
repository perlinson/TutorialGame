using System.Collections.Generic;
using UnityEngine;
using QFramework;

public sealed class CultivationBattleSystem : AbstractSystem
{
    private CultivationSaveSystem saveSystem;

    protected override void OnInit()
    {
        saveSystem = this.GetSystem<CultivationSaveSystem>();
    }

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

    public ExpeditionRoomActionResult ResolveRoomEvent(CombatTurnContext context)
    {
        if (context == null || context.Region == null || context.Room == null || context.Hero == null)
        {
            return new ExpeditionRoomActionResult
            {
                FailureReason = "当前无法结算该房间事件。"
            };
        }

        var result = new ExpeditionRoomActionResult
        {
            Torchlight = context.Torchlight,
            Supplies = context.Supplies,
            PendingQiGain = context.PendingQiGain,
            PendingCrystalGain = context.PendingCrystalGain
        };

        switch (context.Room.Kind)
        {
            case ExpeditionRoomKind.Scout:
                result.PendingQiGain += 1 + context.Region.RequiredRealmTier;
                result.Torchlight = Mathf.Min(100, result.Torchlight + 8);
                result.LogMessage = "你重新测定了地脉走向，修为 +" + (1 + context.Region.RequiredRealmTier) + "，火光也稳定下来。";
                break;
            case ExpeditionRoomKind.Treasure:
            {
                var randomSource = new System.Random(context.Room.Seed + context.CurrentRoomIndex * 31 + context.PendingQiGain * 7 + context.PendingCrystalGain * 13);
                var crystals = 2 + context.Region.DangerRank + randomSource.Next(0, 2);
                result.PendingCrystalGain += crystals;
                ApplyStress(context, result, result.Torchlight < 35 ? 7 : 3);
                result.LogMessage = AppendPrimary(result.LogMessage, "前人行囊中翻出灵石 +" + crystals + "，但搜刮过程中也让队伍心境紧绷。");
                break;
            }
            case ExpeditionRoomKind.Herb:
            {
                var healAmount = 3 + context.Hero.DefenseBonus;
                var qiGain = 1 + context.Region.RequiredRealmTier;
                HealHero(context, healAmount);
                result.PendingQiGain += qiGain;
                ApplyStress(context, result, -10);
                result.LogMessage = AppendPrimary(result.LogMessage, "采下温养灵草，气血恢复 " + healAmount + "，修为 +" + qiGain + "。");
                break;
            }
            case ExpeditionRoomKind.Shrine:
            {
                var qiGain = 2 + context.Region.RequiredRealmTier;
                result.PendingQiGain += qiGain;
                result.Torchlight = Mathf.Min(100, result.Torchlight + 12);
                ApplyStress(context, result, -14);
                result.LogMessage = AppendPrimary(result.LogMessage, "借残阵祭台调息，修为 +" + qiGain + "，火光与心境都暂时稳住。");
                break;
            }
            default:
            {
                var damage = 2 + context.Region.DangerRank;
                ReceiveDamage(context, damage, null);
                ApplyStress(context, result, 10);
                result.PendingCrystalGain += 1;
                result.LogMessage = AppendPrimary(result.LogMessage, "旧阵瘴陷突然爆开，气血 -" + damage + "，心境受扰，但也捞回些散碎灵石。");
                if (context.Hero.CurrentHealth <= 0)
                {
                    result.ExpeditionFailed = true;
                    result.FailureReason = "远征队在 " + context.Region.DisplayName + " 深处彻底溃散。";
                }

                break;
            }
        }

        return result;
    }

    public ExpeditionSupportActionResult UseTorchSupply(CombatTurnContext context)
    {
        if (!IsExpeditionContextValid(context) || context.Supplies <= 0)
        {
            return BuildFailedSupportAction("当前无法继续整备灵灯。", context);
        }

        context.Supplies--;
        context.Torchlight = Mathf.Min(100, context.Torchlight + 24);
        ApplyStress(context, -5, null);
        return BuildSupportAction(context, "你补充了灵灯油和符火，视野重新稳定。");
    }

    public ExpeditionSupportActionResult CampAndRecover(CombatTurnContext context)
    {
        if (!IsExpeditionContextValid(context) || context.Supplies <= 0)
        {
            return BuildFailedSupportAction("当前无法进行扎营休整。", context);
        }

        context.Supplies--;
        HealHero(context, 4 + context.Hero.DefenseBonus);
        ApplyStress(context, -12, null);
        return BuildSupportAction(context, "短暂扎营后，气息与心神都得到了一轮稳固。");
    }

    public ExpeditionSupportActionResult RecenterMind(CombatTurnContext context)
    {
        if (!IsExpeditionContextValid(context))
        {
            return BuildFailedSupportAction("当前无法继续整理行囊。", context);
        }

        ApplyStress(context, -6, null);
        context.Torchlight = Mathf.Max(8, context.Torchlight - 4);
        return BuildSupportAction(context, "你重新清点法器、符箓与丹囊，心境稍稳，但也消耗了些时间与火光。");
    }

    public ExpeditionSupportActionResult SkipRoom(CombatTurnContext context)
    {
        if (!IsExpeditionContextValid(context) || context.Room == null)
        {
            return BuildFailedSupportAction("当前无法跳过该房间。", context);
        }

        context.Room.Resolved = true;
        var result = BuildSupportAction(context, "你放弃了继续搜查 " + context.Room.Title + "，把状态留给更深处的风险。");
        result.RoomResolved = true;
        return result;
    }

    public ExpeditionResolutionResult CompleteExpedition(
        int slotIndex,
        MainMenuSaveData saveData,
        WorldRegionDefinition region,
        ExpeditionHeroState hero,
        int torchlight,
        int pendingQiGain,
        int pendingCrystalGain,
        List<SaveItemStack> pendingItemRewards)
    {
        if (saveData == null || region == null || hero == null)
        {
            return new ExpeditionResolutionResult("远征结算失败，缺少必要数据。", "当前无法完成远征结算。");
        }

        var clearLoot = CultivationApp.BuildClearLoot(region, saveData);
        CultivationApp.MergePendingLoot(pendingItemRewards, clearLoot);
        var clearProgress = CultivationApp.RecordTaskProgress(saveData, new TaskProgressSignal
        {
            Type = TaskProgressSignalType.ClearRegion,
            StringValue = region.Id,
            Count = 1
        });

        var qiBonus = Mathf.Max(0, (70 - hero.Stress) / 12);
        var crystalBonus = torchlight >= 40 ? 1 : 0;
        var bankResult = CultivationApp.BankPendingLoot(saveData, pendingItemRewards);
        var bankSummary = bankResult.BankedSummary;
        var overflowSummary = bankResult.OverflowSummary;
        var overflowCrystalGain = bankResult.OverflowCrystalGain;
        var totalQi = pendingQiGain + qiBonus;
        var totalCrystals = pendingCrystalGain + crystalBonus + overflowCrystalGain;

        WorldRegionLibrary.ApplyTrialRewards(
            saveData,
            region,
            totalQi + region.ClearQiReward,
            totalCrystals + region.ClearCrystalReward,
            out var breakthroughs,
            out var unlockedRegions);
        saveSystem.SaveArchive(slotIndex, saveData);

        var logMessage = "远征成功：修为 +" + (totalQi + region.ClearQiReward) + " / 灵石 +" + (totalCrystals + region.ClearCrystalReward);
        if (!string.IsNullOrEmpty(bankSummary))
        {
            logMessage += "\n带回物资：" + bankSummary + "。";
        }

        if (!string.IsNullOrEmpty(overflowSummary))
        {
            logMessage += "\n储物袋已满，" + overflowSummary + " 已折成灵石 +" + overflowCrystalGain + "。";
        }

        if (breakthroughs > 0)
        {
            logMessage += "\n突破境界 +" + breakthroughs + "。";
        }

        if (!string.IsNullOrEmpty(unlockedRegions))
        {
            logMessage += "\n新开地界：" + unlockedRegions + "。";
        }

        if (clearProgress != null && !string.IsNullOrWhiteSpace(clearProgress.Message))
        {
            logMessage += "\n" + clearProgress.Message;
        }

        return new ExpeditionResolutionResult(logMessage, "此地远征已完成，可以回山海图整备下一次出行。");
    }

    public ExpeditionResolutionResult RetreatExpedition(
        int slotIndex,
        MainMenuSaveData saveData,
        WorldRegionDefinition region,
        int pendingQiGain,
        int pendingCrystalGain,
        List<SaveItemStack> pendingItemRewards)
    {
        if (saveData == null || region == null)
        {
            return new ExpeditionResolutionResult("撤离结算失败，缺少必要数据。", "当前无法完成撤离结算。");
        }

        var qiGain = Mathf.FloorToInt(pendingQiGain * 0.6f);
        var crystalGain = Mathf.CeilToInt(pendingCrystalGain * 0.7f);
        var bankResult = CultivationApp.BankPendingLoot(saveData, pendingItemRewards);
        var bankSummary = bankResult.BankedSummary;
        var overflowSummary = bankResult.OverflowSummary;
        var overflowCrystalGain = bankResult.OverflowCrystalGain;

        ApplyPartialRewards(saveData, region, qiGain, crystalGain + overflowCrystalGain);
        saveSystem.SaveArchive(slotIndex, saveData);

        var logMessage = "你选择提前撤离，保住了部分收获：修为 +" + qiGain + " / 灵石 +" + (crystalGain + overflowCrystalGain) + "。";
        if (!string.IsNullOrEmpty(bankSummary))
        {
            logMessage += "\n带回物资：" + bankSummary + "。";
        }

        if (!string.IsNullOrEmpty(overflowSummary))
        {
            logMessage += "\n储物袋已满，" + overflowSummary + " 已折成灵石 +" + overflowCrystalGain + "。";
        }

        return new ExpeditionResolutionResult(logMessage, "主动撤离不会清理地域，但能保存本次搜刮到的部分资源。");
    }

    public ExpeditionResolutionResult FailExpedition(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region, string reason, List<SaveItemStack> pendingItemRewards)
    {
        if (pendingItemRewards != null)
        {
            pendingItemRewards.Clear();
        }

        if (saveData != null && region != null)
        {
            saveData.location = region.DisplayName;
            saveData.lastPlayed = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            saveSystem.SaveArchive(slotIndex, saveData);
        }

        return new ExpeditionResolutionResult(reason + "\n这次远征没有带回完整战果。", "失败后未结算的深入收益会直接损失。");
    }

    private static bool IsExpeditionContextValid(CombatTurnContext context)
    {
        return context != null && context.Region != null && context.Hero != null;
    }

    private static ExpeditionSupportActionResult BuildSupportAction(CombatTurnContext context, string logMessage)
    {
        return new ExpeditionSupportActionResult
        {
            LogMessage = logMessage,
            Torchlight = context != null ? context.Torchlight : 0,
            Supplies = context != null ? context.Supplies : 0
        };
    }

    private static ExpeditionSupportActionResult BuildFailedSupportAction(string failureReason, CombatTurnContext context)
    {
        var result = BuildSupportAction(context, failureReason);
        result.FailureReason = failureReason;
        if (context != null && context.Hero != null && context.Hero.CurrentHealth <= 0 && context.Region != null)
        {
            result.ExpeditionFailed = true;
            result.FailureReason = "远征队在 " + context.Region.DisplayName + " 深处彻底溃散。";
        }

        return result;
    }

    private static void ApplyPartialRewards(MainMenuSaveData saveData, WorldRegionDefinition region, int qiGain, int crystalGain)
    {
        saveData.qi += Mathf.Max(0, qiGain);
        saveData.spiritCrystals += Mathf.Max(0, crystalGain);
        saveData.currentRegionId = region.Id;
        saveData.location = region.DisplayName;
        saveData.lastPlayed = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        while (true)
        {
            var qiRequired = WorldRegionLibrary.GetQiRequiredForNextRealm(saveData.realmTier);
            if (qiRequired <= 0 || saveData.qi < qiRequired)
            {
                break;
            }

            saveData.qi -= qiRequired;
            saveData.realmTier++;
        }

        saveData.realm = WorldRegionLibrary.GetRealmName(saveData.realmTier);
    }

    private static CombatTurnResult ResolveHeroTurn(CombatTurnContext context, string heroActionSummary)
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

    private static CombatTurnResult CompleteCombat(CombatTurnContext context, string prefix)
    {
        context.Room.Resolved = true;
        var taskUpdates = new List<string>();
        for (var i = 0; i < context.CurrentEncounterSnapshot.Count; i++)
        {
            var factionSnapshot = CultivationApp.RecordFactionDefeat(context.SaveData, context.CurrentEncounterSnapshot[i].Faction, context.Region.Id, 1);
            AppendFactionUpdate(taskUpdates, factionSnapshot);
            var progress = CultivationApp.RecordTaskProgress(context.SaveData, new TaskProgressSignal
            {
                Type = TaskProgressSignalType.DefeatFaction,
                FactionValue = context.CurrentEncounterSnapshot[i].Faction,
                Count = 1
            });
            AppendTaskUpdate(taskUpdates, progress);
        }

        var loot = CultivationApp.BuildEncounterLoot(context);
        CultivationApp.MergePendingLoot(context.PendingItemRewards, loot);
        for (var lootIndex = 0; lootIndex < loot.Count; lootIndex++)
        {
            var stack = loot[lootIndex];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            var progress = CultivationApp.RecordTaskProgress(context.SaveData, new TaskProgressSignal
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

    private static string ResolveEnemyTurn(CombatTurnContext context, List<string> runtimeNotes)
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

    private static string ResolveEnemyAction(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex, List<string> runtimeNotes)
    {
        switch (enemy.Faction)
        {
            case ExpeditionEnemyFaction.Bandit:
                return ResolveBanditAction(context, enemy, enemyIndex, runtimeNotes);
            case ExpeditionEnemyFaction.Cultivator:
                return ResolveCultivatorAction(context, enemy, enemyIndex, runtimeNotes);
            case ExpeditionEnemyFaction.Beast:
                return ResolveBeastAction(context, enemy, enemyIndex, runtimeNotes);
            case ExpeditionEnemyFaction.HeartDemon:
                return ResolveHeartDemonAction(context, enemy, enemyIndex, runtimeNotes);
            default:
                return ResolveCorpseAction(context, enemy, enemyIndex, runtimeNotes);
        }
    }

    private static string ResolveBanditAction(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex, List<string> runtimeNotes)
    {
        if (context.Torchlight > 18 && (context.CombatRound + enemyIndex) % 2 == 0)
        {
            context.Torchlight = Mathf.Max(8, context.Torchlight - 6);
            if (context.Supplies > 0 && (context.CombatRound + context.CurrentRoomIndex + enemyIndex) % 3 == 0)
            {
                context.Supplies--;
                return enemy.Name + " 施展" + enemy.TechniqueName + "，压低火光并顺手抢走一份补给。";
            }

            return enemy.Name + " 施展" + enemy.TechniqueName + "，灵灯被遮得一暗。";
        }

        return ResolvePhysicalAttack(context, enemy, "挥刃逼近", runtimeNotes);
    }

    private static string ResolveCultivatorAction(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex, List<string> runtimeNotes)
    {
        var useStress = ((context.CombatRound + enemyIndex + enemy.CurrentHealth) % 3 == 0) || context.Torchlight <= 28;
        if (useStress)
        {
            var stress = Mathf.Max(1, enemy.StressDamage - context.Hero.StressResistBonus / 3 + 1);
            ApplyStress(context, stress, runtimeNotes);
            return enemy.Name + " 催动" + enemy.TechniqueName + "，心境 +" + stress + "。";
        }

        context.Torchlight = Mathf.Max(8, context.Torchlight - 2);
        return ResolvePhysicalAttack(context, enemy, "邪诀穿身", runtimeNotes);
    }

    private static string ResolveBeastAction(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex, List<string> runtimeNotes)
    {
        if (context.Torchlight <= 24 && (context.CombatRound + enemyIndex) % 2 == 1)
        {
            var stress = Mathf.Max(1, enemy.StressDamage - 2);
            ApplyStress(context, stress, runtimeNotes);
            return enemy.Name + " 发出低吼，借黑暗逼得心境 +" + stress + "。";
        }

        return ResolvePhysicalAttack(context, enemy, enemy.TechniqueName, runtimeNotes);
    }

    private static string ResolveHeartDemonAction(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex, List<string> runtimeNotes)
    {
        if ((context.CombatRound + enemyIndex) % 3 != 1 || context.Torchlight <= 36)
        {
            var stress = Mathf.Max(2, enemy.StressDamage - context.Hero.StressResistBonus / 4 + (context.Torchlight <= 24 ? 2 : 0));
            ApplyStress(context, stress, runtimeNotes);
            return enemy.Name + " 借" + enemy.TechniqueName + "侵心，心境 +" + stress + "。";
        }

        context.Torchlight = Mathf.Max(8, context.Torchlight - 4);
        return ResolvePhysicalAttack(context, enemy, "幻身扑杀", runtimeNotes);
    }

    private static string ResolveCorpseAction(CombatTurnContext context, ExpeditionEnemyState enemy, int enemyIndex, List<string> runtimeNotes)
    {
        if ((context.CombatRound + enemyIndex) % 3 == 0)
        {
            context.Torchlight = Mathf.Max(8, context.Torchlight - 3);
            return ResolvePhysicalAttack(context, enemy, enemy.TechniqueName + "并裹挟阴气", runtimeNotes);
        }

        return ResolvePhysicalAttack(context, enemy, enemy.TechniqueName, runtimeNotes);
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

    private static string PerformSwordCalm(CombatTurnContext context, ExpeditionSkillDefinition skill)
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

    private static string PerformAlchemistRestore(CombatTurnContext context, ExpeditionSkillDefinition skill)
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

    private static string PerformWandererMist(CombatTurnContext context, ExpeditionSkillDefinition skill)
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

    private static void ApplyStress(CombatTurnContext context, int amount, List<string> runtimeNotes)
    {
        var mindResult = CultivationApp.ApplyMindStress(context, amount);
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

    private static void ApplyStress(CombatTurnContext context, ExpeditionRoomActionResult result, int amount)
    {
        var mindResult = CultivationApp.ApplyMindStress(context, amount);
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
