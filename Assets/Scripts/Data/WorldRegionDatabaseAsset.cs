using UnityEngine;

[CreateAssetMenu(fileName = "WorldRegionDatabase", menuName = "TutorialGame/Data/World Region Database")]
public sealed class WorldRegionDatabaseAsset : ScriptableObject
{
    public Sprite coverImage;
    public string startingRegionId;
    public string[] realmNames;
    public WorldRegionDefinition[] regions;
}
