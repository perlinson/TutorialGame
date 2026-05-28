using System;
using UnityEngine;

[Serializable]
public sealed class MarketListing
{
    public string listingId;
    public string sellerId;
    public string itemId;
    public int quantity;
    public SpiritCrystalGrade grade;
    public int price;
    public long createTime;
    public bool isSold;
    public string soldToId;

    public bool IsExpired => System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() - createTime > 7 * 24 * 3600; // 7天过期
}
