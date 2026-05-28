using QFramework;
using System;
using System.Collections.Generic;

public sealed class CultivationBarterSystem : AbstractSystem
{
    private CultivationTradeSystem tradeSystem;
    private List<BarterListing> barterListings;

    protected override void OnInit()
    {
        tradeSystem = this.GetSystem<CultivationTradeSystem>();
        barterListings = new List<BarterListing>();
    }

    /// <summary>
    /// 创建以物易物委托
    /// </summary>
    public string CreateBarterListing(CultivationSaveData saveData, string offeredItemId, int offeredQuantity, string requestedItemId, int requestedQuantity)
    {
        if (saveData == null || string.IsNullOrEmpty(offeredItemId) || string.IsNullOrEmpty(requestedItemId))
            return null;

        if (offeredQuantity <= 0 || requestedQuantity <= 0)
            return null;

        saveData.EnsureDefaults();
        if (saveData.GetItemCount(offeredItemId) < offeredQuantity)
            return null;

        saveData.RemoveItem(offeredItemId, offeredQuantity);

        var listing = new BarterListing
        {
            listingId = Guid.NewGuid().ToString(),
            traderId = saveData.heroName,
            offeredItemId = offeredItemId,
            offeredQuantity = offeredQuantity,
            requestedItemId = requestedItemId,
            requestedQuantity = requestedQuantity,
            createTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            isAccepted = false
        };

        barterListings.Add(listing);
        return listing.listingId;
    }

    /// <summary>
    /// 接受以物易物委托
    /// </summary>
    public bool AcceptBarterListing(CultivationSaveData saveData, string listingId, out string summary)
    {
        summary = string.Empty;
        if (saveData == null || string.IsNullOrEmpty(listingId))
            return false;

        var listing = barterListings.Find(l => l.listingId == listingId);
        if (listing == null || listing.isAccepted)
        {
            summary = "委托不存在或已被接受。";
            return false;
        }

        if (listing.traderId == saveData.heroName)
        {
            summary = "不能接受自己的委托。";
            return false;
        }

        saveData.EnsureDefaults();
        if (saveData.GetItemCount(listing.requestedItemId) < listing.requestedQuantity)
        {
            summary = $"缺少 {listing.requestedItemId} x{listing.requestedQuantity}。";
            return false;
        }

        saveData.RemoveItem(listing.requestedItemId, listing.requestedQuantity);
        saveData.TryAddItem(listing.offeredItemId, listing.offeredQuantity);

        // 将请求的物品给委托者（在多人游戏中需要通过服务器处理）
        // 单机游戏中简化处理，假设委托者会自动获得

        listing.isAccepted = true;
        listing.acceptedBy = saveData.heroName;

        summary = $"交易成功：用 {listing.requestedItemId} x{listing.requestedQuantity} 换取 {listing.offeredItemId} x{listing.offeredQuantity}。";
        barterListings.Remove(listing);
        return true;
    }

    /// <summary>
    /// 取消以物易物委托
    /// </summary>
    public bool CancelBarterListing(CultivationSaveData saveData, string listingId, out string summary)
    {
        summary = string.Empty;
        if (saveData == null || string.IsNullOrEmpty(listingId))
            return false;

        var listing = barterListings.Find(l => l.listingId == listingId);
        if (listing == null || listing.isAccepted)
        {
            summary = "委托不存在或已被接受。";
            return false;
        }

        if (listing.traderId != saveData.heroName)
        {
            summary = "只能取消自己的委托。";
            return false;
        }

        saveData.TryAddItem(listing.offeredItemId, listing.offeredQuantity);
        barterListings.Remove(listing);
        summary = "委托已取消，物品已退回。";
        return true;
    }

    /// <summary>
    /// 获取以物易物列表
    /// </summary>
    public BarterListing[] GetBarterListings()
    {
        return barterListings.FindAll(l => !l.isAccepted).ToArray();
    }

    /// <summary>
    /// 获取我的委托列表
    /// </summary>
    public BarterListing[] GetMyListings(string traderId)
    {
        return barterListings.FindAll(l => l.traderId == traderId && !l.isAccepted).ToArray();
    }

    /// <summary>
    /// 清理过期委托（7天）
    /// </summary>
    public void CleanupExpiredListings()
    {
        var expired = barterListings.FindAll(l => 
            DateTimeOffset.UtcNow.ToUnixTimeSeconds() - l.createTime > 7 * 24 * 3600 && !l.isAccepted);
        
        foreach (var listing in expired)
        {
            barterListings.Remove(listing);
        }
    }
}
