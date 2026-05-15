using System;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryDatabase", menuName = "TutorialGame/Data/Inventory Database")]
public sealed class InventoryDatabaseAsset : ScriptableObject
{
    public Sprite coverImage;
    public InventoryItemRecord[] items;
}

[Serializable]
public sealed class InventoryItemRecord
{
    public string id;
    public string displayName;
    public Sprite iconImage;
    public string category;
    public string rarity;
    [TextArea(2, 5)] public string description;
    public int crystalValue = 1;
}
