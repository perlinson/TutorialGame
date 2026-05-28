using QFramework;
using System;
using System.Collections.Generic;

public sealed class CultivationContributionTradeSystem : AbstractSystem
{
    private CultivationTradeSystem tradeSystem;
    private List<ContributionShopListing> contributionShopListings;
    private Dictionary<string, int> dailyPurchaseCounts;

    protected override void OnInit()
    {
        tradeSystem = this.GetSystem<CultivationTradeSystem>();
        contributionShopListings = new List<ContributionShopListing>();
        dailyPurchaseCounts = new Dictionary<string, int>();
        InitializeContributionShop();
    }

    /// <summary>
    /// 初始化宗门贡献度商店
    /// </summary>
    private void InitializeContributionShop()
    {
        contributionShopListings.Add(new ContributionShopListing
        {
            itemId = "sect_token",
            quantity = 1,
            contributionCost = 100,
            dailyLimit = 5,
            realmTierRequirement = 1,
            isLimited = true
        });

        contributionShopListings.Add(new ContributionShopListing
        {
            itemId = "sect_manual",
            quantity = 1,
            contributionCost = 500,
            dailyLimit = 1,
            realmTierRequirement = 3,
            isLimited = true
        });

        contributionShopListings.Add(new ContributionShopListing
        {
            itemId = "sect_essence",
            quantity = 1,
            contributionCost = 1000,
            dailyLimit = 0,
            realmTierRequirement = 6,
            isLimited = false
        });
    }

    /// <summary>
    /// 用宗门贡献度购买物品
    /// </summary>
    public bool BuyWithContribution(CultivationSaveData saveData, string itemId, out string summary)
    {
        summary = string.Empty;
        if (saveData == null || string.IsNullOrEmpty(itemId))
            return false;

        var listing = contributionShopListings.Find(l => l.itemId == itemId);
        if (listing == null)
        {
            summary = "商品不存在。";
            return false;
        }

        saveData.EnsureDefaults();
        if (saveData.realmTier < listing.realmTierRequirement)
        {
            summary = $"需要达到 {listing.realmTierRequirement} 阶境界才能购买。";
            return false;
        }

        if (saveData.attributes.social.sectContribution < listing.contributionCost)
        {
            summary = "宗门贡献度不足。";
            return false;
        }

        if (listing.isLimited)
        {
            var key = saveData.heroName + "_" + itemId;
            if (dailyPurchaseCounts.ContainsKey(key) && dailyPurchaseCounts[key] >= listing.dailyLimit)
            {
                summary = "今日购买次数已达上限。";
                return false;
            }
        }

        saveData.attributes.social.sectContribution -= listing.contributionCost;
        saveData.TryAddItem(listing.itemId, listing.quantity);

        if (listing.isLimited)
        {
            var key = saveData.heroName + "_" + itemId;
            if (!dailyPurchaseCounts.ContainsKey(key))
                dailyPurchaseCounts[key] = 0;
            dailyPurchaseCounts[key]++;
        }

        summary = $"购买成功：{listing.itemId} x{listing.quantity}，消耗 {listing.contributionCost} 宗门贡献度。";
        return true;
    }

    /// <summary>
    /// 获取宗门贡献度商店列表
    /// </summary>
    public ContributionShopListing[] GetContributionShopListings(CultivationSaveData saveData)
    {
        if (saveData == null)
            return new ContributionShopListing[0];

        return contributionShopListings.FindAll(l => saveData.realmTier >= l.realmTierRequirement).ToArray();
    }

    /// <summary>
    /// 获取商品剩余购买次数
    /// </summary>
    public int GetRemainingPurchases(string heroName, string itemId)
    {
        var listing = contributionShopListings.Find(l => l.itemId == itemId);
        if (listing == null || !listing.isLimited)
            return -1;

        var key = heroName + "_" + itemId;
        if (!dailyPurchaseCounts.ContainsKey(key))
            return listing.dailyLimit;

        return listing.dailyLimit - dailyPurchaseCounts[key];
    }

    /// <summary>
    /// 重置每日购买次数
    /// </summary>
    public void ResetDailyPurchases()
    {
        dailyPurchaseCounts.Clear();
    }
}
