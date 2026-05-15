using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyArchetypeDatabase", menuName = "TutorialGame/Data/Enemy Archetype Database")]
public sealed class EnemyArchetypeDatabaseAsset : ScriptableObject
{
    public Sprite coverImage;
    public EnemyArchetypeRecord[] archetypes;
}

[Serializable]
public sealed class EnemyArchetypeRecord
{
    public string id;
    public int faction;
    public bool eliteOnly;
    public int[] allowedRoomKinds;
    public string displayName;
    public Sprite portraitImage;
    public string techniqueName;
    public int healthOffset;
    public int damageOffset;
    public int stressOffset;
    public int armorOffset;
    public int poisonResistance;
    public int stunResistance;
}
