using System.Text;
using UnityEngine;

public static class WorkshopLibrary
{
    private static WorkshopRecipeDefinition[] recipes;

    public static WorkshopRecipeDefinition[] GetRecipes()
    {
        if (recipes != null)
        {
            return recipes;
        }

        var database = GameData.LoadAsset<WorkshopRecipeDatabaseAsset>("Data/WorkshopRecipeDatabase");
        if (database != null && database.recipes != null && database.recipes.Length > 0)
        {
            recipes = database.recipes;
            return recipes;
        }

        recipes = BuildFallbackRecipes();
        return recipes;
    }

    public static string BuildWorkshopSummary(MainMenuSaveData saveData)
    {
        saveData.EnsureDefaults();
        var loadedRecipes = GetRecipes();
        var builder = new StringBuilder();
        builder.Append("洞府整备可把历练所得真正转成长期成长。\n");
        builder.Append(CultivationLoadoutLibrary.BuildEquipmentOverview(saveData)).Append('\n');
        builder.Append(CultivationLoadoutLibrary.BuildGrowthEffectSummary(saveData)).Append("\n\n");
        for (var i = 0; i < loadedRecipes.Length; i++)
        {
            var recipe = loadedRecipes[i];
            if (i > 0)
            {
                builder.Append("\n\n");
            }

            builder.Append(i + 1).Append(". ").Append(recipe.Title).Append(" / ").Append(recipe.Discipline).Append('\n');
            builder.Append(recipe.Description).Append('\n');
            builder.Append("材料：").Append(FormatCost(recipe)).Append('\n');
            builder.Append("效果：").Append(FormatReward(recipe)).Append('\n');
            builder.Append(CanCraft(saveData, recipe) ? "当前可炼制" : "材料不足");
        }

        return builder.ToString();
    }

    public static string BuildRecipeButtonLabel(MainMenuSaveData saveData, string recipeId)
    {
        var recipe = GetRecipe(recipeId);
        if (recipe == null)
        {
            return "未知丹方";
        }

        return recipe.Title + (CanCraft(saveData, recipe) ? " · 可炼" : " · 缺材");
    }

    public static bool Craft(MainMenuSaveData saveData, string recipeId, CultivationRealmSystem realmSystem, out string summary)
    {
        saveData.EnsureDefaults();
        var recipe = GetRecipe(recipeId);
        if (recipe == null)
        {
            summary = "洞府中没有这门可用的丹方或祭炼法。";
            return false;
        }

        if (!CanCraft(saveData, recipe))
        {
            summary = recipe.Title + " 所需材料不足。";
            return false;
        }

        if (recipe.CostItems != null)
        {
            for (var i = 0; i < recipe.CostItems.Length; i++)
            {
                saveData.RemoveItem(recipe.CostItems[i].itemId, recipe.CostItems[i].quantity);
            }
        }

        saveData.spiritCrystals += recipe.RewardCrystals;
        saveData.mainArtifactLevel += recipe.RewardMainArtifactLevel + recipe.RewardAttackLevel;
        saveData.protectiveRelicLevel += recipe.RewardProtectiveRelicLevel + recipe.RewardVitalityLevel;
        saveData.pillCauldronLevel += recipe.RewardPillCauldronLevel;
        saveData.talismanCaseLevel += recipe.RewardTalismanCaseLevel;
        saveData.bagCapacity += recipe.RewardBagCapacity;
        saveData.attackLevel = saveData.mainArtifactLevel;
        saveData.vitalityLevel = saveData.protectiveRelicLevel;

        // 使用 RealmSystem.GainQi 处理修为获取和突破
        var gainResult = realmSystem != null
            ? realmSystem.GainQi(saveData, recipe.RewardQi, autoBreakthrough: true)
            : new RealmGainResult(recipe.RewardQi, 0, saveData.realmTier, saveData.realmTier);

        summary = "洞府整备完成：" + recipe.Title + "，" + FormatReward(recipe);
        if (gainResult.HasBreakthrough)
        {
            summary += "，并借此突破境界 +" + gainResult.BreakthroughCount;
        }

        summary += "。";
        return true;
    }

    private static bool CanCraft(MainMenuSaveData saveData, WorkshopRecipeDefinition recipe)
    {
        if (recipe.CostItems == null)
        {
            return true;
        }

        for (var i = 0; i < recipe.CostItems.Length; i++)
        {
            if (saveData.GetItemCount(recipe.CostItems[i].itemId) < recipe.CostItems[i].quantity)
            {
                return false;
            }
        }

        return true;
    }

    private static WorkshopRecipeDefinition GetRecipe(string recipeId)
    {
        var loadedRecipes = GetRecipes();
        for (var i = 0; i < loadedRecipes.Length; i++)
        {
            if (loadedRecipes[i].Id == recipeId)
            {
                return loadedRecipes[i];
            }
        }

        return null;
    }

    private static WorkshopRecipeDefinition[] BuildFallbackRecipes()
    {
        return new[]
        {
            new WorkshopRecipeDefinition
            {
                Id = "pill_cauldron_upgrade",
                Title = "丹炉养火",
                Discipline = "炼丹",
                Description = "以灵草、玉露和香灰养住炉火，让出行可携带的丹药更多，药性也更足。",
                CostItems = new[]
                {
                    new SaveItemStack("mist_mushroom", 1),
                    new SaveItemStack("spring_jade_dew", 1),
                    new SaveItemStack("mind_cleansing_incense", 2)
                },
                RewardPillCauldronLevel = 1
            },
            new WorkshopRecipeDefinition
            {
                Id = "talisman_case_upgrade",
                Title = "符匣拓纹",
                Discipline = "符阵",
                Description = "重刻阵纹、稳固符胆后，能多收几道符箓，也更能镇住心神。",
                CostItems = new[]
                {
                    new SaveItemStack("blood_talisman_page", 1),
                    new SaveItemStack("array_shard", 1),
                    new SaveItemStack("heart_mark_fragment", 1)
                },
                RewardTalismanCaseLevel = 1
            },
            new WorkshopRecipeDefinition
            {
                Id = "peiyuan_powder",
                Title = "培元散",
                Discipline = "炼丹",
                Description = "把妖丹碎片和灵砂磨成药散，用来稳固练气期根基。",
                CostItems = new[]
                {
                    new SaveItemStack("green_spirit_sand", 2),
                    new SaveItemStack("beast_core_shard", 2)
                },
                RewardQi = 3
            },
            new WorkshopRecipeDefinition
            {
                Id = "nawu_pouch",
                Title = "纳物符袋",
                Discipline = "杂艺",
                Description = "以路引纹、阵片和灵骨缝合新的纳物夹层，扩充储物袋空间。",
                CostItems = new[]
                {
                    new SaveItemStack("bandit_route_token", 1),
                    new SaveItemStack("array_shard", 1),
                    new SaveItemStack("beast_bone", 2)
                },
                RewardBagCapacity = 1
            }
        };
    }

    private static string FormatCost(WorkshopRecipeDefinition recipe)
    {
        if (recipe.CostItems == null || recipe.CostItems.Length == 0)
        {
            return "无";
        }

        var builder = new StringBuilder();
        for (var i = 0; i < recipe.CostItems.Length; i++)
        {
            if (i > 0)
            {
                builder.Append("、");
            }

            builder.Append(InventoryLibrary.GetDisplayName(recipe.CostItems[i].itemId))
                .Append(" x")
                .Append(recipe.CostItems[i].quantity);
        }

        return builder.ToString();
    }

    private static string FormatReward(WorkshopRecipeDefinition recipe)
    {
        var builder = new StringBuilder();
        var wrote = false;
        var mainArtifactReward = recipe.RewardMainArtifactLevel + recipe.RewardAttackLevel;
        var protectiveRelicReward = recipe.RewardProtectiveRelicLevel + recipe.RewardVitalityLevel;
        AppendReward(builder, recipe.RewardQi, "修为 +", ref wrote);
        AppendReward(builder, recipe.RewardCrystals, "灵石 +", ref wrote);
        AppendReward(builder, mainArtifactReward, "主法器 +", ref wrote);
        AppendReward(builder, protectiveRelicReward, "护身法器 +", ref wrote);
        AppendReward(builder, recipe.RewardPillCauldronLevel, "丹炉 +", ref wrote);
        AppendReward(builder, recipe.RewardTalismanCaseLevel, "符匣 +", ref wrote);
        AppendReward(builder, recipe.RewardBagCapacity, "储物袋 +", ref wrote, " 格");
        return wrote ? builder.ToString() : "无";
    }

    private static void AppendReward(StringBuilder builder, int value, string label, ref bool wrote, string suffix = "")
    {
        if (value <= 0)
        {
            return;
        }

        if (wrote)
        {
            builder.Append(" / ");
        }

        builder.Append(label).Append(value).Append(suffix);
        wrote = true;
    }
}
