using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RealmConfig", menuName = "Cultivation/Realm Config")]
public sealed class RealmConfigAsset : ScriptableObject
{
    [Header("境界信息")]
    public CultivationLevel level;
    public string displayName;
    [TextArea(2, 4)] public string description;

    [Header("突破条件")]
    public int requiredCultivation;
    public int requiredBranchTotal;
    public int[] requiredSpecificBranchLevels;

    [Header("必然提升")]
    public int divineSenseBonus;
    public int maxHealthBonus;
    public int maxManaBonus;

    [Header("分支基数")]
    public int allBranchBase;
    public int divineSenseBranchExtra;
    public int constitutionBranchExtra;

    [Header("可选奖励")]
    public int optionalRewardValue;

    [Header("寿元加成")]
    public int lifespanBonus;
}
