using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactConfig", menuName = "Cultivation/Artifact Config")]
public sealed class ArtifactConfigAsset : ScriptableObject
{
    [Header("功法信息")]
    public string id;
    public string displayName;
    [TextArea(2, 4)] public string description;
    public Sprite icon;

    [Header("属性加成")]
    public int divineSenseBonus;
    public int constitutionBonus;
    public int comprehensionBonus;
    public int fortuneBonus;
    public int charmBonus;
    public int soulPowerBonus;
    public int vitalEnergyBonus;
    public int willpowerBonus;
    public int dexterityBonus;

    [Header("分支侧重")]
    public BranchFocus[] branchFocuses;

    [Header("修炼效率")]
    [Tooltip("修炼该功法时，特定分支的经验获取倍率")]
    public float trainingEfficiencyMultiplier = 1.0f;

    [Header("流派适配")]
    public SchoolType[] compatibleSchools;

    [Serializable]
    public sealed class BranchFocus
    {
        public BranchType branchType;
        public int levelBonus;
    }
}
