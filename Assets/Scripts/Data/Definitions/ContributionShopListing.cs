using System;
using UnityEngine;

[Serializable]
public sealed class ContributionShopListing
{
    public string itemId;
    public int quantity;
    public int contributionCost;
    public int dailyLimit;
    public int realmTierRequirement;
    public bool isLimited;
}
