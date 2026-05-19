using System.Collections.Generic;
using UnityEngine;

public sealed partial class CultivationBattleSystem
{
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
}
