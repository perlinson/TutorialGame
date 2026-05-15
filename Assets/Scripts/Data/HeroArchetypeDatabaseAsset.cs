using System;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroArchetypeDatabase", menuName = "TutorialGame/Data/Hero Archetype Database")]
public sealed class HeroArchetypeDatabaseAsset : ScriptableObject
{
    public Sprite coverImage;
    public HeroArchetypeRecord[] archetypes;
}

[Serializable]
public sealed class HeroArchetypeRecord
{
    public string id;
    public string displayName;
    public string origin;
    public string specialty;
    [TextArea(2, 4)] public string description;
    [TextArea(2, 4)] public string trait;
    [TextArea(2, 4)] public string recommendation;
    public string defaultHeroName;
    public Sprite portraitImage;
    public Sprite bannerImage;
    public string mainArtifact;
    public string supportArtifact;
    public string protectiveRelic;
    public string talismanName;
    public string medicineName;
    public int healthBonus;
    public int attackBonus;
    public int stressResistBonus;
    public int startingTorchBonus;
    public int startingSupplyBonus;
    public int talismanCharges;
    public int medicineCharges;
    public HeroSkillRecord[] skills;
}

[Serializable]
public sealed class HeroSkillRecord
{
    public string id;
    public string name;
    public Sprite iconImage;
    [TextArea(2, 4)] public string description;
}
