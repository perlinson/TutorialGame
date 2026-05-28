using QFramework;

public sealed class CultivationTradeSystem : AbstractSystem
{
    private CultivationCurrencySystem currencySystem;
    private CultivationRealmSystem realmSystem;

    protected override void OnInit()
    {
        currencySystem = this.GetSystem<CultivationCurrencySystem>();
        realmSystem = this.GetSystem<CultivationRealmSystem>();
    }

    /// <summary>
    /// 用灵石购买物品
    /// </summary>
    public bool BuyItem(CultivationSaveData saveData, string itemId, int quantity, SpiritCrystalGrade grade, int cost)
    {
        if (saveData == null || string.IsNullOrEmpty(itemId) || quantity <= 0 || cost <= 0)
            return false;

        saveData.EnsureDefaults();

        if (!currencySystem.CanAfford(saveData, cost))
            return false;

        if (!saveData.wallet.Spend(grade, cost))
            return false;

        saveData.TryAddItem(itemId, quantity);
        return true;
    }

    /// <summary>
    /// 出售物品换取灵石
    /// </summary>
    public bool SellItem(CultivationSaveData saveData, string itemId, int quantity, SpiritCrystalGrade grade, int price)
    {
        if (saveData == null || string.IsNullOrEmpty(itemId) || quantity <= 0 || price <= 0)
            return false;

        saveData.EnsureDefaults();

        if (saveData.GetItemCount(itemId) < quantity)
            return false;

        saveData.RemoveItem(itemId, quantity);
        saveData.wallet.Add(grade, price);
        return true;
    }

    /// <summary>
    /// 配方制作：材料→物品
    /// </summary>
    public bool CraftRecipe(CultivationSaveData saveData, string recipeId, out string summary)
    {
        summary = string.Empty;
        if (saveData == null || string.IsNullOrEmpty(recipeId))
        {
            summary = "无效的配方。";
            return false;
        }

        saveData.EnsureDefaults();
        var recipe = WorkshopLibrary.GetRecipe(recipeId);
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

        var grade = CultivationCurrencySystem.RealmToGrade(saveData.realmTier);
        saveData.wallet.Add(grade, recipe.RewardCrystals);
        saveData.mainArtifactLevel += recipe.RewardMainArtifactLevel + recipe.RewardAttackLevel;
        saveData.protectiveRelicLevel += recipe.RewardProtectiveRelicLevel + recipe.RewardVitalityLevel;
        saveData.pillCauldronLevel += recipe.RewardPillCauldronLevel;
        saveData.talismanCaseLevel += recipe.RewardTalismanCaseLevel;
        saveData.bagCapacity += recipe.RewardBagCapacity;
        saveData.attackLevel = saveData.mainArtifactLevel;
        saveData.vitalityLevel = saveData.protectiveRelicLevel;

        var gainResult = realmSystem != null
            ? realmSystem.GainQi(saveData, recipe.RewardQi, autoBreakthrough: true)
            : new RealmGainResult(recipe.RewardQi, 0, saveData.realmTier, saveData.realmTier);

        summary = "洞府整备完成：" + recipe.Title + "，" + WorkshopLibrary.FormatReward(recipe);
        if (gainResult.HasBreakthrough)
        {
            summary += "，并借此突破境界 +" + gainResult.BreakthroughCount;
        }

        summary += "。";
        return true;
    }

    /// <summary>
    /// 灵石兑换：低级→高级（100:1）
    /// </summary>
    public bool ExchangeCrystals(CultivationSaveData saveData, SpiritCrystalGrade fromGrade, SpiritCrystalGrade toGrade, int amount)
    {
        if (saveData == null || amount <= 0)
            return false;

        saveData.EnsureDefaults();

        // 只允许向上兑换
        if (fromGrade >= toGrade)
            return false;

        // 检查是否有足够的低级灵石
        var requiredAmount = amount * 100;
        if (!saveData.wallet.CanAfford(fromGrade, requiredAmount))
            return false;

        saveData.wallet.Spend(fromGrade, requiredAmount);
        saveData.wallet.Add(toGrade, amount);
        return true;
    }

    /// <summary>
    /// 检查配方是否可制作
    /// </summary>
    public bool CanCraft(CultivationSaveData saveData, string recipeId)
    {
        if (saveData == null || string.IsNullOrEmpty(recipeId))
            return false;

        var recipe = WorkshopLibrary.GetRecipe(recipeId);
        return CanCraft(saveData, recipe);
    }

    /// <summary>
    /// 检查配方是否可制作
    /// </summary>
    public bool CanCraft(CultivationSaveData saveData, WorkshopRecipeDefinition recipe)
    {
        if (saveData == null || recipe == null)
            return false;

        if (recipe.CostItems == null)
            return true;

        for (var i = 0; i < recipe.CostItems.Length; i++)
        {
            if (saveData.GetItemCount(recipe.CostItems[i].itemId) < recipe.CostItems[i].quantity)
                return false;
        }

        return true;
    }
}
