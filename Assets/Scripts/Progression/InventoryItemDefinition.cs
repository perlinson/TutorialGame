using UnityEngine;

public sealed class InventoryItemDefinition
{
    public InventoryItemDefinition(string id, string displayName, string category, string rarity, string description, Sprite artworkImage = null, int crystalValue = 1)
    {
        Id = id;
        DisplayName = displayName;
        Category = category;
        Rarity = rarity;
        Description = description;
        ArtworkImage = artworkImage;
        CrystalValue = crystalValue;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string Category { get; }
    public string Rarity { get; }
    public string Description { get; }
    public Sprite ArtworkImage { get; }
    public int CrystalValue { get; }
}
