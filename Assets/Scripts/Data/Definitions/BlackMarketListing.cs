using System;
using UnityEngine;

[Serializable]
public sealed class BlackMarketListing
{
    public string listingId;
    public string itemId;
    public int quantity;
    public SpiritCrystalGrade grade;
    public int basePrice;
    public float priceMultiplier; // 价格波动系数
    public long refreshTime;
    public bool isAvailable;

    public int CurrentPrice => (int)(basePrice * priceMultiplier);
    public bool NeedsRefresh => System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= refreshTime;
}
