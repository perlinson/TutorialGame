using System.Collections.Generic;
using UnityEngine;
using QFramework;

public sealed class CultivationExpeditionSystem : AbstractSystem
{
    private CultivationExpeditionModel expeditionModel;
    private CultivationMindStateSystem mindStateSystem;

    protected override void OnInit()
    {
        expeditionModel = this.GetModel<CultivationExpeditionModel>();
        mindStateSystem = this.GetSystem<CultivationMindStateSystem>();
    }

    public void Sync(CombatTurnContext context)
    {
        expeditionModel.Apply(context);
    }

    public void Clear()
    {
        expeditionModel.Clear();
        MainMenuSaveStore.ClearExpeditionRuntime();
    }

    public ExpeditionTraversalResult EnterRoom(ExpeditionTraversalContext context)
    {
        if (context == null || context.Region == null || context.Hero == null || context.Room == null || context.RoomCount <= 0)
        {
            return new ExpeditionTraversalResult
            {
                Phase = ExpeditionFlowPhase.Failed,
                FailureReason = "当前无法进入该房间。"
            };
        }

        var result = new ExpeditionTraversalResult
        {
            RoomIndex = Mathf.Clamp(context.RoomIndex, 0, context.RoomCount - 1),
            Torchlight = context.Torchlight
        };

        context.Room.Visited = true;
        context.Hero.GuardValue = 0;
        context.Hero.CounterDamage = 0;

        if (result.RoomIndex > 0)
        {
            result.Torchlight = Mathf.Max(8, result.Torchlight - (7 + context.Region.DangerRank * 2));
            ApplyStress(context, result, result.Torchlight <= 30 ? 6 : 2);
            if (result.ExpeditionFailed)
            {
                result.Phase = ExpeditionFlowPhase.Failed;
                return result;
            }
        }

        if (RoomKindStrategyRegistry.IsCombatRoom(context.Room.Kind))
        {
            result.Phase = ExpeditionFlowPhase.CombatPlayerTurn;
            result.StartCombat = true;
            result.LogMessage = PrependPrimary(
                result.LogMessage,
                context.Room.Kind == ExpeditionRoomKind.Boss
                    ? "前方核心灵压暴涨，凶煞、邪修与残阵气息纠缠在一起。"
                    : "黑暗中有气机锁定了远征队，只能当场开战。");
            result.HintMessage = "战斗重点不是无脑输出，而是看门派技能和随身法器如何稳住节奏。";
            return result;
        }

        result.Phase = ExpeditionFlowPhase.RoomDecision;
        result.LogMessage = PrependPrimary(result.LogMessage, "进入 " + context.Room.Title + "。远征队需要先判断这里值不值得停留搜查。");
        result.HintMessage = "非战斗房间更考验资源分配。火光、补给与心境都要留余地。";
        return result;
    }

    public ExpeditionAdvanceResult Advance(ExpeditionAdvanceContext context)
    {
        var result = new ExpeditionAdvanceResult();
        if (context == null || context.RoomCount <= 0)
        {
            return result;
        }

        switch (context.Phase)
        {
            case ExpeditionFlowPhase.RoomDecision:
                result.ShouldSearchCurrentRoom = true;
                break;
            case ExpeditionFlowPhase.AfterRoom:
                if (context.CurrentRoomIndex >= context.RoomCount - 1)
                {
                    result.ShouldCompleteExpedition = true;
                }
                else
                {
                    result.ShouldEnterNextRoom = true;
                    result.NextRoomIndex = context.CurrentRoomIndex + 1;
                }

                break;
            case ExpeditionFlowPhase.Completed:
            case ExpeditionFlowPhase.Retreated:
            case ExpeditionFlowPhase.Failed:
                result.ShouldReturnToWorldMap = true;
                break;
        }

        return result;
    }

    public ExpeditionLootCollectionResult CollectRoomLoot(WorldRegionDefinition region, ExpeditionRoomState room, List<SaveItemStack> pendingItemRewards)
    {
        var loot = ExpeditionLootFactory.BuildRoomLoot(region, room);
        MergeLoot(pendingItemRewards, loot);
        return new ExpeditionLootCollectionResult
        {
            LootSummary = loot != null && loot.Count > 0 ? InventoryLibrary.DescribeLoot(loot) : string.Empty
        };
    }

    private static void MergeLoot(List<SaveItemStack> target, List<SaveItemStack> incoming)
    {
        if (target == null || incoming == null || incoming.Count == 0)
        {
            return;
        }

        for (var i = 0; i < incoming.Count; i++)
        {
            var stack = incoming[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            var merged = false;
            for (var existingIndex = 0; existingIndex < target.Count; existingIndex++)
            {
                if (target[existingIndex] != null && target[existingIndex].itemId == stack.itemId)
                {
                    target[existingIndex].quantity += stack.quantity;
                    merged = true;
                    break;
                }
            }

            if (!merged)
            {
                target.Add(new SaveItemStack(stack.itemId, stack.quantity));
            }
        }
    }

    private void ApplyStress(ExpeditionTraversalContext context, ExpeditionTraversalResult result, int amount)
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

    private static string PrependPrimary(string existing, string primary)
    {
        if (string.IsNullOrWhiteSpace(existing))
        {
            return primary;
        }

        return primary + "\n" + existing;
    }
}
