using System;
using UnityEngine;

[Serializable]
public sealed class BarterListing
{
    public string listingId;
    public string traderId;
    public string offeredItemId;
    public int offeredQuantity;
    public string requestedItemId;
    public int requestedQuantity;
    public long createTime;
    public bool isAccepted;
    public string acceptedBy;
}
