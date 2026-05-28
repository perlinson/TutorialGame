using QFramework;
using System;
using System.Collections.Generic;

public sealed class CultivationBlackMarketSystem : AbstractSystem
{
    private CultivationCurrencySystem currencySystem;
    private CultivationTradeSystem tradeSystem;
    private List<BlackMarketListing> blackMarketListings;

    protected override void OnInit()
    {
        currencySystem = this.GetSystem<CultivationCurrencySystem>();
        tradeSystem = this.GetSystem<CultivationTradeSystem>();
        blackMarketListings = new List<BlackMarketListing>();
        InitializeBlackMarket();
    }

    /// <summary>
    /// 初始化黑市商品
    /// </summary>
    private void InitializeBlackMarket()
    {
        // 示例黑市商品
        blackMarketListings.Add(new BlackMarketListing
        {
            listingId = Guid.NewGuid().ToString(),
            itemId = "mystic_essence",
            quantity = 1,
            grade = SpiritCrystalGrade.Supreme,
            basePrice = 500,
            priceMultiplier = 1.0f,
            refreshTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600,
            isAvailable = true
        });

        blackMarketListings.Add(new BlackMarketListing
        {
            listingId = Guid.NewGuid().ToString(),
            itemId = "forbidden_scroll",
            quantity = 1,
            grade = SpiritCrystalGrade.High,
            basePrice = 200,
            priceMultiplier = 1.2f,
            refreshTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 7200,
            isAvailable = true
        });
    }

    /// <summary>
    /// 刷新黑市商品
    /// </summary>
    public void RefreshBlackMarket()
    {
        foreach (var listing in blackMarketListings)
        {
            if (listing.NeedsRefresh)
            {
                listing.priceMultiplier = 0.8f + UnityEngine.Random.value * 0.4f; // 0.8-1.2倍波动
                listing.refreshTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600 + (int)(UnityEngine.Random.value * 3600);
                listing.isAvailable = true;
            }
        }
    }

    /// <summary>
    /// 从黑市购买
    /// </summary>
    public bool BuyFromBlackMarket(CultivationSaveData saveData, string listingId, out string summary)
    {
        summary = string.Empty;
        if (saveData == null || string.IsNullOrEmpty(listingId))
            return false;

        var listing = blackMarketListings.Find(l => l.listingId == listingId);
        if (listing == null || !listing.isAvailable)
        {
            summary = "商品不存在或已售罄。";
            return false;
        }

        saveData.EnsureDefaults();
        var currentPrice = listing.CurrentPrice;
        if (!currencySystem.CanAfford(saveData, currentPrice))
        {
            summary = $"{CultivationCurrencySystem.GradeName(listing.grade)}不足。";
            return false;
        }

        if (!saveData.wallet.Spend(listing.grade, currentPrice))
        {
            summary = "支付失败。";
            return false;
        }

        saveData.TryAddItem(listing.itemId, listing.quantity);
        listing.isAvailable = false;
        listing.refreshTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 86400; // 24小时后刷新

        summary = $"黑市购买成功：{listing.itemId} x{listing.quantity}，花费 {currentPrice} {CultivationCurrencySystem.GradeName(listing.grade)}。";
        return true;
    }

    /// <summary>
    /// 获取黑市商品列表
    /// </summary>
    public BlackMarketListing[] GetBlackMarketListings()
    {
        RefreshBlackMarket();
        return blackMarketListings.FindAll(l => l.isAvailable).ToArray();
    }

    /// <summary>
    /// 获取商品当前价格
    /// </summary>
    public int GetItemPrice(string listingId)
    {
        var listing = blackMarketListings.Find(l => l.listingId == listingId);
        return listing?.CurrentPrice ?? 0;
    }
}
