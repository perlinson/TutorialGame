using QFramework;
using System;
using System.Collections.Generic;

public sealed class CultivationMarketSystem : AbstractSystem
{
    private CultivationCurrencySystem currencySystem;
    private CultivationTradeSystem tradeSystem;
    private List<MarketListing> marketListings;

    protected override void OnInit()
    {
        currencySystem = this.GetSystem<CultivationCurrencySystem>();
        tradeSystem = this.GetSystem<CultivationTradeSystem>();
        marketListings = new List<MarketListing>();
    }

    /// <summary>
    /// 创建市场挂单
    /// </summary>
    public string CreateMarketListing(CultivationSaveData saveData, string itemId, int quantity, SpiritCrystalGrade grade, int price)
    {
        if (saveData == null || string.IsNullOrEmpty(itemId) || quantity <= 0 || price <= 0)
            return null;

        saveData.EnsureDefaults();
        if (saveData.GetItemCount(itemId) < quantity)
            return null;

        saveData.RemoveItem(itemId, quantity);

        var listing = new MarketListing
        {
            listingId = Guid.NewGuid().ToString(),
            sellerId = saveData.heroName,
            itemId = itemId,
            quantity = quantity,
            grade = grade,
            price = price,
            createTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            isSold = false
        };

        marketListings.Add(listing);
        return listing.listingId;
    }

    /// <summary>
    /// 购买市场物品
    /// </summary>
    public bool BuyFromMarket(CultivationSaveData saveData, string listingId, out string summary)
    {
        summary = string.Empty;
        if (saveData == null || string.IsNullOrEmpty(listingId))
            return false;

        var listing = marketListings.Find(l => l.listingId == listingId);
        if (listing == null || listing.isSold || listing.IsExpired)
        {
            summary = "商品不存在或已下架。";
            return false;
        }

        saveData.EnsureDefaults();
        if (!currencySystem.CanAfford(saveData, listing.price))
        {
            summary = $"{CultivationCurrencySystem.GradeName(listing.grade)}不足。";
            return false;
        }

        if (!saveData.wallet.Spend(listing.grade, listing.price))
        {
            summary = "支付失败。";
            return false;
        }

        saveData.TryAddItem(listing.itemId, listing.quantity);
        listing.isSold = true;
        listing.soldToId = saveData.heroName;

        summary = $"购买成功：{listing.itemId} x{listing.quantity}，花费 {listing.price} {CultivationCurrencySystem.GradeName(listing.grade)}。";
        
        // 在实际游戏中，这里需要将灵石转给卖家
        marketListings.Remove(listing);
        return true;
    }

    /// <summary>
    /// 取消市场挂单
    /// </summary>
    public bool CancelMarketListing(CultivationSaveData saveData, string listingId, out string summary)
    {
        summary = string.Empty;
        if (saveData == null || string.IsNullOrEmpty(listingId))
            return false;

        var listing = marketListings.Find(l => l.listingId == listingId);
        if (listing == null || listing.isSold)
        {
            summary = "挂单不存在或已售出。";
            return false;
        }

        if (listing.sellerId != saveData.heroName)
        {
            summary = "只能取消自己的挂单。";
            return false;
        }

        saveData.TryAddItem(listing.itemId, listing.quantity);
        marketListings.Remove(listing);
        summary = "挂单已取消，物品已退回。";
        return true;
    }

    /// <summary>
    /// 获取市场列表
    /// </summary>
    public MarketListing[] GetMarketListings()
    {
        return marketListings.FindAll(l => !l.isSold && !l.IsExpired).ToArray();
    }

    /// <summary>
    /// 获取我的市场挂单
    /// </summary>
    public MarketListing[] GetMyListings(string sellerId)
    {
        return marketListings.FindAll(l => l.sellerId == sellerId && !l.isSold).ToArray();
    }

    /// <summary>
    /// 清理过期挂单
    /// </summary>
    public void CleanupExpiredListings()
    {
        var expired = marketListings.FindAll(l => l.IsExpired && !l.isSold);
        foreach (var listing in expired)
        {
            marketListings.Remove(listing);
        }
    }
}
