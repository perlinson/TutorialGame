using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LootTable", menuName = "Cultivation/Data/Loot Table")]
public sealed class LootTableAsset : ScriptableObject
{
    public Sprite coverImage;
    public FactionLootRecord[] factionLoots;
    public RoomLootRecord[] roomLoots;
    public RegionLootRecord[] regionLoots;
}

[Serializable]
public sealed class LootDropRecord
{
    public string itemId;
    public Sprite iconImage;
    public int normalQuantity;
    public int eliteQuantity;
}

[Serializable]
public sealed class FactionLootRecord
{
    public int faction;
    public Sprite illustrationImage;
    public LootDropRecord[] drops;
}

[Serializable]
public sealed class RoomLootRecord
{
    public int roomKind;
    public Sprite illustrationImage;
    public LootDropRecord[] drops;
}

[Serializable]
public sealed class RegionLootRecord
{
    public string regionId;
    public Sprite illustrationImage;
    public string rareItemId;
    public string herbItemId;
    public LootDropRecord[] clearDrops;
}
