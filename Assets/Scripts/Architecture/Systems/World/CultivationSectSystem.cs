using System.Collections.Generic;
using System.Text;
using QFramework;
using UnityEngine;

public sealed class CultivationSectSystem : AbstractSystem
{
    private CultivationSaveSystem saveSystem;
    private CultivationSettlementSystem settlementSystem;

    private static readonly SectHallDefinition[] HallDefinitions = BuildHallDefinitions();
    private static readonly SectActionDefinition[] ActionDefinitions = BuildActionDefinitions();

    protected override void OnInit()
    {
        saveSystem = this.GetSystem<CultivationSaveSystem>();
        settlementSystem = this.GetSystem<CultivationSettlementSystem>();
    }

    public string BuildSectOverview(MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            return "宗门尚未接引。";
        }

        saveData.EnsureDefaults();
        var builder = new StringBuilder();
        builder.Append("宗门：").Append(saveData.sectName).Append('\n');
        builder.Append(saveData.heroName).Append(" · ").Append(saveData.realm).Append(" · ").Append(saveData.archetypeName).Append('\n');
        builder.Append("灵石：").Append(saveData.spiritCrystals)
            .Append("    储物袋：").Append(saveData.GetUsedBagSlots()).Append(" / ").Append(saveData.bagCapacity).Append('\n');
        builder.Append(CultivationLoadoutLibrary.BuildCompactProgressSummary(saveData)).Append('\n');
        builder.Append("主事殿堂：勤功、炼器、丹鼎、符阵、藏经、庶务、洞府");
        return builder.ToString();
    }

    public SectHallSnapshot[] GetHallSnapshots(MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            return new SectHallSnapshot[0];
        }

        saveData.EnsureDefaults();
        var snapshots = new SectHallSnapshot[HallDefinitions.Length];
        for (var i = 0; i < HallDefinitions.Length; i++)
        {
            var hall = HallDefinitions[i];
            snapshots[i] = new SectHallSnapshot
            {
                Definition = hall,
                IsUnlocked = true,
                LockedReason = string.Empty,
                StatusSummary = BuildHallStatus(saveData, hall),
                Actions = BuildActionSnapshots(saveData, hall)
            };
        }

        return snapshots;
    }

    public SectActionResult ExecuteAction(int slotIndex, MainMenuSaveData saveData, string actionId)
    {
        if (saveData == null)
        {
            return new SectActionResult(false, "当前没有可用存档。", string.Empty, actionId);
        }

        saveData.EnsureDefaults();
        var action = FindAction(actionId);
        if (action == null)
        {
            return new SectActionResult(false, "宗门中没有这项事务。", string.Empty, actionId);
        }

        switch (action.Kind)
        {
            case SectActionKind.ResolveTaskBoard:
            {
                var message = saveSystem.ResolveTaskBoard(slotIndex, saveData);
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "勤功殿案卷已查阅，当前主委托仍在进行。";
                }

                CultivationGameTime.Advance(saveData, 1);
                saveSystem.SaveArchive(slotIndex, saveData);
                return new SectActionResult(true, message, action.HallId, action.Id);
            }
            case SectActionKind.ClaimActiveTask:
            {
                var taskSystem = this.GetSystem<CultivationTaskSystem>();
                string reason;
                if (!taskSystem.CanClaimActiveTask(saveData, out reason))
                {
                    return new SectActionResult(false, reason, action.HallId, action.Id);
                }

                var resultMessage = saveSystem.ClaimActiveTask(slotIndex, saveData);
                CultivationGameTime.Advance(saveData, 1);
                saveSystem.SaveArchive(slotIndex, saveData);
                return new SectActionResult(true, resultMessage, action.HallId, action.Id);
            }
            case SectActionKind.UpgradeMainArtifact:
                return ConvertResult(settlementSystem.UpgradeMainArtifact(slotIndex, saveData), action);
            case SectActionKind.UpgradeProtectiveRelic:
                return ConvertResult(settlementSystem.UpgradeProtectiveRelic(slotIndex, saveData), action);
            case SectActionKind.CraftRecipe:
                return ConvertResult(settlementSystem.CraftRecipe(slotIndex, saveData, action.LinkedRecipeId), action);
            case SectActionKind.ShowSummary:
                CultivationGameTime.Advance(saveData, 1);
                saveSystem.SaveArchive(slotIndex, saveData);
                return new SectActionResult(true, BuildHallStatus(saveData, FindHall(action.HallId)), action.HallId, action.Id);
            case SectActionKind.Placeholder:
                CultivationGameTime.Advance(saveData, 1);
                saveSystem.SaveArchive(slotIndex, saveData);
                return new SectActionResult(true, "藏经阁已开放阅览。功法、术法、身法与神通树会在下一轮接入，现在先保留入口。", action.HallId, action.Id);
            default:
                return new SectActionResult(false, "这项宗门事务尚未开放。", action.HallId, action.Id);
        }
    }

    private static SectActionResult ConvertResult(WorldMapActionResult result, SectActionDefinition action)
    {
        if (result == null)
        {
            return new SectActionResult(false, "宗门事务没有返回结果。", action.HallId, action.Id);
        }

        return new SectActionResult(result.Succeeded, result.Message, action.HallId, action.Id);
    }

    private static SectActionSnapshot[] BuildActionSnapshots(MainMenuSaveData saveData, SectHallDefinition hall)
    {
        var actions = new List<SectActionSnapshot>();
        if (hall.ActionIds == null)
        {
            return actions.ToArray();
        }

        for (var i = 0; i < hall.ActionIds.Length; i++)
        {
            var action = FindAction(hall.ActionIds[i]);
            if (action == null)
            {
                continue;
            }

            string reason;
            actions.Add(new SectActionSnapshot
            {
                Definition = action,
                ButtonLabel = BuildActionButtonLabel(saveData, action, out reason),
                IsAvailable = string.IsNullOrWhiteSpace(reason),
                UnavailableReason = reason
            });
        }

        return actions.ToArray();
    }

    private static string BuildActionButtonLabel(MainMenuSaveData saveData, SectActionDefinition action, out string unavailableReason)
    {
        unavailableReason = string.Empty;
        switch (action.Kind)
        {
            case SectActionKind.ClaimActiveTask:
            {
                TaskDefinition definition;
                SaveTaskState state;
                if (!TaskLibrary.TryGetActiveTask(saveData, out definition, out state))
                {
                    unavailableReason = "当前没有可结算的委托。";
                    return action.Title + " · 无委托";
                }

                var progress = TaskLibrary.GetProgressValue(saveData, definition, state);
                if (progress < definition.RequiredCount)
                {
                    unavailableReason = "当前委托还没达成条件。";
                    return action.Title + " · 未达成";
                }

                return action.Title + " · 可结算";
            }
            case SectActionKind.UpgradeMainArtifact:
            {
                var cost = WorldRegionLibrary.GetAttackUpgradeCost(saveData);
                if (saveData.spiritCrystals < cost)
                {
                    unavailableReason = "灵石不足。";
                }

                return action.Title + " · " + cost + " 灵石";
            }
            case SectActionKind.UpgradeProtectiveRelic:
            {
                var cost = WorldRegionLibrary.GetVitalityUpgradeCost(saveData);
                if (saveData.spiritCrystals < cost)
                {
                    unavailableReason = "灵石不足。";
                }

                return action.Title + " · " + cost + " 灵石";
            }
            case SectActionKind.CraftRecipe:
                if (!CanCraft(saveData, action.LinkedRecipeId))
                {
                    unavailableReason = "材料不足。";
                }

                return WorkshopLibrary.BuildRecipeButtonLabel(saveData, action.LinkedRecipeId);
            default:
                return action.Title;
        }
    }

    private static bool CanCraft(MainMenuSaveData saveData, string recipeId)
    {
        var recipes = WorkshopLibrary.GetRecipes();
        for (var i = 0; i < recipes.Length; i++)
        {
            var recipe = recipes[i];
            if (recipe == null || recipe.Id != recipeId)
            {
                continue;
            }

            if (recipe.CostItems == null)
            {
                return true;
            }

            for (var itemIndex = 0; itemIndex < recipe.CostItems.Length; itemIndex++)
            {
                var cost = recipe.CostItems[itemIndex];
                if (cost == null || saveData.GetItemCount(cost.itemId) < cost.quantity)
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    private static string BuildHallStatus(MainMenuSaveData saveData, SectHallDefinition hall)
    {
        if (hall == null)
        {
            return string.Empty;
        }

        switch (hall.HallType)
        {
            case SectHallType.DutyHall:
                return TaskLibrary.BuildActiveTaskSummary(saveData);
            case SectHallType.RefiningHall:
                return CultivationLoadoutLibrary.BuildEquipmentOverview(saveData) + "\n\n" +
                       "祭炼主法器耗费：" + WorldRegionLibrary.GetAttackUpgradeCost(saveData) + " 灵石\n" +
                       "温养护身法器耗费：" + WorldRegionLibrary.GetVitalityUpgradeCost(saveData) + " 灵石";
            case SectHallType.AlchemyHall:
                return "丹炉：" + CultivationLoadoutLibrary.GetPillCauldronName(saveData.archetypeId, saveData.pillCauldronLevel) +
                       "  +" + saveData.pillCauldronLevel + "\n" +
                       "丹药次数加成 +" + saveData.pillCauldronLevel + "\n\n" +
                       DescribeRecipe(saveData, "pill_cauldron_upgrade") + "\n" +
                       DescribeRecipe(saveData, "peiyuan_powder");
            case SectHallType.TalismanHall:
                return "符匣：" + CultivationLoadoutLibrary.GetTalismanCaseName(saveData.archetypeId, saveData.talismanCaseLevel) +
                       "  +" + saveData.talismanCaseLevel + "\n" +
                       "符箓次数 +" + saveData.talismanCaseLevel + "    稳心 +" + saveData.talismanCaseLevel * 2 + "\n\n" +
                       DescribeRecipe(saveData, "talisman_case_upgrade");
            case SectHallType.ScriptureHall:
                return "当前术法仍由出身决定，战斗内会自动使用现有技艺。\n\n" +
                       "预留方向：功法主修、术法招式、身法遁术、神通秘卷。";
            case SectHallType.StewardHall:
                return InventoryLibrary.BuildBagSummary(saveData, 6) + "\n\n" +
                       settlementSummaryLine(saveData) + "\n" +
                       DescribeRecipe(saveData, "nawu_pouch");
            case SectHallType.CaveResidence:
                return settlementSystemSummary(saveData);
            default:
                return hall.Description;
        }
    }

    private static string settlementSystemSummary(MainMenuSaveData saveData)
    {
        return "你的洞府位于山门后坡，当前仍以整备、储物和闭关静修为主。\n\n" +
               WorkshopLibrary.BuildWorkshopSummary(saveData) +
               "\n\n洞府建设：" + saveData.settlementBuildCount + " 次\n最近整备：" +
               (string.IsNullOrWhiteSpace(saveData.lastSettlementAction) ? "暂无" : saveData.lastSettlementAction);
    }

    private static string settlementSummaryLine(MainMenuSaveData saveData)
    {
        return "洞府建设：" + saveData.settlementBuildCount + " 次    最近整备：" +
               (string.IsNullOrWhiteSpace(saveData.lastSettlementAction) ? "暂无" : saveData.lastSettlementAction);
    }

    private static string DescribeRecipe(MainMenuSaveData saveData, string recipeId)
    {
        var recipes = WorkshopLibrary.GetRecipes();
        for (var i = 0; i < recipes.Length; i++)
        {
            var recipe = recipes[i];
            if (recipe == null || recipe.Id != recipeId)
            {
                continue;
            }

            return recipe.Title + "：" + recipe.Description + "\n状态：" + WorkshopLibrary.BuildRecipeButtonLabel(saveData, recipeId);
        }

        return "未知配方：" + recipeId;
    }

    private static SectHallDefinition FindHall(string hallId)
    {
        for (var i = 0; i < HallDefinitions.Length; i++)
        {
            if (HallDefinitions[i].Id == hallId)
            {
                return HallDefinitions[i];
            }
        }

        return null;
    }

    private static SectActionDefinition FindAction(string actionId)
    {
        for (var i = 0; i < ActionDefinitions.Length; i++)
        {
            if (ActionDefinitions[i].Id == actionId)
            {
                return ActionDefinitions[i];
            }
        }

        return null;
    }

    private static SectHallDefinition[] BuildHallDefinitions()
    {
        return new[]
        {
            new SectHallDefinition
            {
                Id = "duty_hall",
                DisplayName = "勤功殿",
                Subtitle = "委托 / 悬赏 / 结算",
                Description = "宗门案牍、山外悬赏与历练委托都由此处派发，完成后也在这里核功发赏。",
                HallType = SectHallType.DutyHall,
                PlaceholderColor = new Color(0.24f, 0.18f, 0.12f, 1f),
                ActionIds = new[] { "duty_refresh", "duty_claim" }
            },
            new SectHallDefinition
            {
                Id = "refining_hall",
                DisplayName = "炼器殿",
                Subtitle = "法器 / 护身 / 祭炼",
                Description = "主法器和护身法器在此温养祭炼，是战斗攻伐和护体气血的长期来源。",
                HallType = SectHallType.RefiningHall,
                PlaceholderColor = new Color(0.28f, 0.15f, 0.1f, 1f),
                ActionIds = new[] { "refine_main_artifact", "refine_protective_relic" }
            },
            new SectHallDefinition
            {
                Id = "alchemy_hall",
                DisplayName = "丹鼎殿",
                Subtitle = "丹炉 / 丹药 / 修炼资源",
                Description = "负责丹炉养火、药材炮制和修炼丹药，把灵草妖丹转化为稳定修为。",
                HallType = SectHallType.AlchemyHall,
                PlaceholderColor = new Color(0.17f, 0.24f, 0.14f, 1f),
                ActionIds = new[] { "alchemy_cauldron_upgrade", "alchemy_peiyuan_powder" }
            },
            new SectHallDefinition
            {
                Id = "talisman_hall",
                DisplayName = "符阵殿",
                Subtitle = "符匣 / 阵纹 / 历练辅助",
                Description = "符箓、阵片和镇心纹路都归此殿统辖，影响历练中的符法余裕与心神稳定。",
                HallType = SectHallType.TalismanHall,
                PlaceholderColor = new Color(0.15f, 0.2f, 0.27f, 1f),
                ActionIds = new[] { "talisman_case_upgrade" }
            },
            new SectHallDefinition
            {
                Id = "scripture_hall",
                DisplayName = "藏经阁",
                Subtitle = "功法 / 术法 / 神通",
                Description = "收录主修功法、术法招式、身法遁术与神通秘卷。v1 先保留入口，不改动现有战斗技能树。",
                HallType = SectHallType.ScriptureHall,
                PlaceholderColor = new Color(0.18f, 0.16f, 0.25f, 1f),
                ActionIds = new[] { "scripture_preview" }
            },
            new SectHallDefinition
            {
                Id = "steward_hall",
                DisplayName = "庶务堂",
                Subtitle = "储物 / 洞府 / 后勤",
                Description = "宗门后勤与洞府整备的归口，可扩充储物袋、登记杂项资源和查看建设近况。",
                HallType = SectHallType.StewardHall,
                PlaceholderColor = new Color(0.2f, 0.19f, 0.14f, 1f),
                ActionIds = new[] { "steward_nawu_pouch", "steward_summary" }
            },
            new SectHallDefinition
            {
                Id = "cave_residence",
                DisplayName = "我的洞府",
                Subtitle = "静修 / 整备 / 私库",
                Description = "你在山门内的独立洞府。闭关修整、查看私库和后续洞府建筑都会集中在这里。",
                HallType = SectHallType.CaveResidence,
                PlaceholderColor = new Color(0.13f, 0.18f, 0.16f, 1f),
                ActionIds = new[] { "cave_summary", "steward_nawu_pouch" }
            }
        };
    }

    private static SectActionDefinition[] BuildActionDefinitions()
    {
        return new[]
        {
            new SectActionDefinition { Id = "duty_refresh", HallId = "duty_hall", Title = "查阅委托", Description = "刷新或接取当前可用的单主委托。", Kind = SectActionKind.ResolveTaskBoard },
            new SectActionDefinition { Id = "duty_claim", HallId = "duty_hall", Title = "结算委托", Description = "若当前主委托已达成，则领取勤功殿奖励。", Kind = SectActionKind.ClaimActiveTask },
            new SectActionDefinition { Id = "refine_main_artifact", HallId = "refining_hall", Title = "祭炼主法器", Description = "消耗灵石提升主法器，强化攻伐能力。", Kind = SectActionKind.UpgradeMainArtifact },
            new SectActionDefinition { Id = "refine_protective_relic", HallId = "refining_hall", Title = "温养护身法器", Description = "消耗灵石提升护身法器，强化气血和护体。", Kind = SectActionKind.UpgradeProtectiveRelic },
            new SectActionDefinition { Id = "alchemy_cauldron_upgrade", HallId = "alchemy_hall", Title = "丹炉养火", Description = "消耗药材提升丹炉。", Kind = SectActionKind.CraftRecipe, LinkedRecipeId = "pill_cauldron_upgrade" },
            new SectActionDefinition { Id = "alchemy_peiyuan_powder", HallId = "alchemy_hall", Title = "炼制培元散", Description = "消耗妖丹碎片和灵砂换取修为。", Kind = SectActionKind.CraftRecipe, LinkedRecipeId = "peiyuan_powder" },
            new SectActionDefinition { Id = "talisman_case_upgrade", HallId = "talisman_hall", Title = "符匣拓纹", Description = "消耗符页和阵片提升符匣。", Kind = SectActionKind.CraftRecipe, LinkedRecipeId = "talisman_case_upgrade" },
            new SectActionDefinition { Id = "scripture_preview", HallId = "scripture_hall", Title = "阅览经阁", Description = "查看后续功法系统入口。", Kind = SectActionKind.Placeholder },
            new SectActionDefinition { Id = "steward_nawu_pouch", HallId = "steward_hall", Title = "缝制纳物符袋", Description = "消耗路引纹、阵片和灵骨扩充储物袋。", Kind = SectActionKind.CraftRecipe, LinkedRecipeId = "nawu_pouch" },
            new SectActionDefinition { Id = "steward_summary", HallId = "steward_hall", Title = "查看庶务账册", Description = "查看洞府建设与储物状态。", Kind = SectActionKind.ShowSummary },
            new SectActionDefinition { Id = "cave_summary", HallId = "cave_residence", Title = "查看洞府", Description = "查看自己的洞府整备状态。", Kind = SectActionKind.ShowSummary }
        };
    }
}
