using UnityEngine;

[CreateAssetMenu(fileName = "WorldRegionDatabase", menuName = "Cultivation/Data/World Region Database")]
public sealed class WorldRegionDatabaseAsset : ScriptableObject
{
    public Sprite coverImage;
    public string startingRegionId;
    public string[] realmNames;
    public WorldRegionDefinition[] regions;
}
