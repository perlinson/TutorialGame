using QFramework;
using System;
using System.Collections.Generic;

public sealed class CultivationAuctionSystem : AbstractSystem
{
    private CultivationCurrencySystem currencySystem;
    private CultivationTradeSystem tradeSystem;
    private List<AuctionListing> auctionListings;

    protected override void OnInit()
    {
        currencySystem = this.GetSystem<CultivationCurrencySystem>();
        tradeSystem = this.GetSystem<CultivationTradeSystem>();
        auctionListings = new List<AuctionListing>();
    }

    /// <summary>
    /// 创建拍卖委托
    /// </summary>
    public string CreateAuction(CultivationSaveData saveData, string itemId, int quantity, SpiritCrystalGrade grade, int startingPrice, int durationHours)
    {
        if (saveData == null || string.IsNullOrEmpty(itemId) || quantity <= 0 || startingPrice <= 0)
            return null;

        saveData.EnsureDefaults();
        if (saveData.GetItemCount(itemId) < quantity)
            return null;

        saveData.RemoveItem(itemId, quantity);

        var listing = new AuctionListing
        {
            listingId = Guid.NewGuid().ToString(),
            sellerId = saveData.heroName,
            itemId = itemId,
            quantity = quantity,
            grade = grade,
            startingPrice = startingPrice,
            currentBid = 0,
            currentBidderId = null,
            endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + durationHours * 3600,
            isSold = false
        };

        auctionListings.Add(listing);
        return listing.listingId;
    }

    /// <summary>
    /// 竞价
    /// </summary>
    public bool PlaceBid(CultivationSaveData saveData, string listingId, int bidAmount)
    {
        if (saveData == null || string.IsNullOrEmpty(listingId) || bidAmount <= 0)
            return false;

        var listing = auctionListings.Find(l => l.listingId == listingId);
        if (listing == null || listing.isSold || listing.IsExpired)
            return false;

        if (bidAmount < listing.MinimumBid)
            return false;

        saveData.EnsureDefaults();
        if (!currencySystem.CanAfford(saveData, bidAmount))
            return false;

        // 退还之前的竞价
        if (listing.HasBid && listing.currentBidderId == saveData.heroName)
        {
            saveData.wallet.Add(listing.grade, listing.currentBid);
        }

        // 扣除新竞价
        if (!saveData.wallet.Spend(listing.grade, bidAmount))
            return false;

        listing.currentBid = bidAmount;
        listing.currentBidderId = saveData.heroName;
        return true;
    }

    /// <summary>
    /// 结束拍卖（到期或手动结束）
    /// </summary>
    public bool EndAuction(CultivationSaveData saveData, string listingId, out string summary)
    {
        summary = string.Empty;
        var listing = auctionListings.Find(l => l.listingId == listingId);
        if (listing == null)
        {
            summary = "拍卖不存在。";
            return false;
        }

        if (listing.isSold)
        {
            summary = "拍卖已结束。";
            return false;
        }

        listing.isSold = true;

        if (listing.HasBid)
        {
            // 拍卖成功，卖家获得灵石
            listing.soldToId = listing.currentBidderId;
            listing.finalPrice = listing.currentBid;
            summary = $"拍卖成功：{listing.itemId} x{listing.quantity} 以 {listing.finalPrice} {CultivationCurrencySystem.GradeName(listing.grade)} 成交。";
            
            // 在实际游戏中，这里需要将灵石转给卖家
            // 由于当前是单机游戏，这里简化处理
        }
        else
        {
            // 拍卖失败，退还物品给卖家
            summary = $"拍卖流拍：{listing.itemId} x{listing.quantity} 无人竞价，物品已退还。";
            // 在实际游戏中，这里需要将物品退还给卖家
        }

        auctionListings.Remove(listing);
        return true;
    }

    /// <summary>
    /// 获取所有拍卖列表
    /// </summary>
    public AuctionListing[] GetAuctionListings()
    {
        return auctionListings.ToArray();
    }

    /// <summary>
    /// 获取我的拍卖列表
    /// </summary>
    public AuctionListing[] GetMyAuctions(string sellerId)
    {
        return auctionListings.FindAll(l => l.sellerId == sellerId).ToArray();
    }

    /// <summary>
    /// 清理过期拍卖
    /// </summary>
    public void CleanupExpiredAuctions()
    {
        var expired = auctionListings.FindAll(l => l.IsExpired && !l.isSold);
        foreach (var listing in expired)
        {
            auctionListings.Remove(listing);
        }
    }
}
