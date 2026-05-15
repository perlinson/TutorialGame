using System;
using UnityEngine;

[Serializable]
public sealed class WorldRegionDefinition
{
    public string Id;
    public string DisplayName;
    public string Subtitle;
    public Sprite IllustrationImage;
    public Sprite MapIconImage;
    public string Description;
    public int RequiredRealmTier;
    public int DangerRank;
    public int ClearQiReward;
    public int ClearCrystalReward;
    public int EnemyCount;
    public int EliteEnemyCount;
    public int SpiritNodeCount;
    public int HerbCount;
    public int RelicCount;
    public int LayoutSeed;
    public Vector2 MapPosition;
    public Vector2 ArenaSize;
    public Vector2 PlayerSpawn;
    public Color BackdropColor;
    public Color GroundColor;
    public Color InnerGroundColor;
    public Color AccentColor;
    public string[] UnlockRegionIds;
}
