using System;
using UnityEngine;

[Serializable]
public sealed class AuctionListing
{
    public string listingId;
    public string sellerId;
    public string itemId;
    public int quantity;
    public SpiritCrystalGrade grade;
    public int startingPrice;
    public int currentBid;
    public string currentBidderId;
    public long endTime;
    public bool isSold;
    public string soldToId;
    public int finalPrice;

    public bool IsExpired => System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= endTime;
    public bool HasBid => !string.IsNullOrEmpty(currentBidderId);
    public int MinimumBid => currentBid > 0 ? currentBid + (currentBid / 10) : startingPrice;
}
