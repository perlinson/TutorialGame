using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RegionEncounterDatabase", menuName = "TutorialGame/Data/Region Encounter Database")]
public sealed class RegionEncounterDatabaseAsset : ScriptableObject
{
    public Sprite coverImage;
    public RegionEncounterProfile[] profiles;
}

[Serializable]
public sealed class RegionEncounterProfile
{
    public string regionId;
    public Sprite illustrationImage;
    public int[] factions;
}
