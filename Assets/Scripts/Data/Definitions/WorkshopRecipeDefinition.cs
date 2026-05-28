using System;
using UnityEngine;

[Serializable]
public sealed class WorkshopRecipeDefinition
{
    public string Id;
    public string Title;
    public string Discipline;
    public Sprite IllustrationImage;
    public string Description;
    public SaveItemStack[] CostItems;
    public int RewardQi;
    public int RewardCrystals;
    public int RewardAttackLevel;
    public int RewardVitalityLevel;
    public int RewardMainArtifactLevel;
    public int RewardProtectiveRelicLevel;
    public int RewardPillCauldronLevel;
    public int RewardTalismanCaseLevel;
    public int RewardBagCapacity;
}
