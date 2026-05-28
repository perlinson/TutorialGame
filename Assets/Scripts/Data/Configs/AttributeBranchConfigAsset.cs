using UnityEngine;

[CreateAssetMenu(fileName = "AttributeBranchConfig", menuName = "Cultivation/Attribute Branch Config")]
public sealed class AttributeBranchConfigAsset : ScriptableObject
{
    [Header("分支信息")]
    public BranchType branchType;
    public string displayName;
    public BaseAttributeType parentAttribute;

    [Header("成长曲线")]
    [Tooltip("每级所需经验基数")]
    public int baseExperiencePerLevel = 100;
    [Tooltip("每级经验增长系数（1.0=线性，>1.0=加速增长）")]
    public float experienceGrowthFactor = 1.2f;
    [Tooltip("分支等级上限")]
    public int maxLevel = 200;

    [Header("效果描述")]
    [TextArea(2, 4)] public string effectPerLevel;
}
