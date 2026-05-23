using System;
using System.Collections.Generic;
using UnityEngine;

public static class WorldRegionLibrary
{
    public const string DefaultStartingRegionId = "green_stone_gate";

    private static readonly string[] FallbackRealmNames =
    {
        "练气初期",
        "练气中期",
        "练气后期",
        "筑基初期",
        "筑基中期",
        "结丹前夕"
    };

    private static List<WorldRegionDefinition> regions;
    private static string[] realmNames;
    private static string startingRegionId;

    public static string StartingRegionId
    {
        get
        {
            EnsureLoaded();
            return startingRegionId;
        }
    }

    public static IReadOnlyList<WorldRegionDefinition> GetRegions()
    {
        EnsureLoaded();
        return regions;
    }

    public static WorldRegionDefinition GetStartingRegion()
    {
        EnsureLoaded();
        return regions.Count > 0 ? regions[0] : null;
    }

    public static bool TryGetRegion(string regionId, out WorldRegionDefinition region)
    {
        EnsureLoaded();
        for (var i = 0; i < regions.Count; i++)
        {
            if (regions[i].Id == regionId)
            {
                region = regions[i];
                return true;
            }
        }

        region = null;
        return false;
    }

    public static string GetRegionDisplayName(string regionId)
    {
        WorldRegionDefinition region;
        return TryGetRegion(regionId, out region) ? region.DisplayName : "未知地域";
    }

    public static string GetRealmName(int realmTier)
    {
        EnsureLoaded();
        var names = realmNames != null && realmNames.Length > 0 ? realmNames : FallbackRealmNames;
        var clampedTier = Mathf.Clamp(realmTier, 0, names.Length - 1);
        return names[clampedTier];
    }

    public static int GetQiRequiredForNextRealm(int realmTier)
    {
        EnsureLoaded();
        var names = realmNames != null && realmNames.Length > 0 ? realmNames : FallbackRealmNames;
        if (realmTier >= names.Length - 1)
        {
            return 0;
        }

        return 6 + realmTier * 3;
    }

    public static bool CanTravel(MainMenuSaveData saveData, WorldRegionDefinition region, out string reason)
    {
        if (saveData == null)
        {
            reason = "当前没有有效存档。";
            return false;
        }

        saveData.EnsureDefaults();

        if (!saveData.IsRegionUnlocked(region.Id))
        {
            reason = "尚未探明前往 " + region.DisplayName + " 的路径。";
            return false;
        }

        if (saveData.realmTier < region.RequiredRealmTier)
        {
            reason = "至少达到 " + GetRealmName(region.RequiredRealmTier) + " 后方可深入此地。";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public static void ApplyTrialRewards(MainMenuSaveData saveData, WorldRegionDefinition region, int qiGain, int crystalGain, CultivationRealmSystem realmSystem, out int breakthroughs, out string unlockedRegions)
    {
        saveData.EnsureDefaults();
        saveData.spiritCrystals += Mathf.Max(0, crystalGain);
        saveData.MarkRegionCleared(region.Id);
        saveData.UnlockRegion(region.Id);

        var newRegions = new List<string>();
        for (var i = 0; i < region.UnlockRegionIds.Length; i++)
        {
            var nextRegionId = region.UnlockRegionIds[i];
            if (!saveData.IsRegionUnlocked(nextRegionId))
            {
                saveData.UnlockRegion(nextRegionId);
                newRegions.Add(GetRegionDisplayName(nextRegionId));
            }
        }

        // 使用 RealmSystem.GainQi 处理修为获取和突破
        var gainResult = realmSystem != null
            ? realmSystem.GainQi(saveData, qiGain, autoBreakthrough: true)
            : new RealmGainResult(qiGain, 0, saveData.realmTier, saveData.realmTier);
        breakthroughs = gainResult.BreakthroughCount;

        saveData.currentRegionId = region.UnlockRegionIds.Length > 0 ? region.UnlockRegionIds[0] : region.Id;
        saveData.location = region.DisplayName;
        saveData.lastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        unlockedRegions = newRegions.Count > 0 ? string.Join("、", newRegions.ToArray()) : string.Empty;
    }

    public static int GetVitalityUpgradeCost(MainMenuSaveData saveData)
    {
        saveData.EnsureDefaults();
        return CultivationLoadoutLibrary.GetProtectiveRelicUpgradeCost(saveData);
    }

    public static int GetAttackUpgradeCost(MainMenuSaveData saveData)
    {
        saveData.EnsureDefaults();
        return CultivationLoadoutLibrary.GetMainArtifactUpgradeCost(saveData);
    }

    private static int GetMaxRealmTier()
    {
        EnsureLoaded();
        var names = realmNames != null && realmNames.Length > 0 ? realmNames : FallbackRealmNames;
        return names.Length - 1;
    }

    private static void EnsureLoaded()
    {
        if (regions != null)
        {
            return;
        }

        regions = new List<WorldRegionDefinition>();
        var database = GameData.LoadAsset<WorldRegionDatabaseAsset>("Data/WorldRegionDatabase");
        if (database != null)
        {
            if (database.regions != null && database.regions.Length > 0)
            {
                regions.AddRange(database.regions);
            }

            realmNames = database.realmNames != null && database.realmNames.Length > 0 ? database.realmNames : FallbackRealmNames;
            startingRegionId = string.IsNullOrWhiteSpace(database.startingRegionId) ? DefaultStartingRegionId : database.startingRegionId;
        }
        else
        {
            realmNames = FallbackRealmNames;
            startingRegionId = DefaultStartingRegionId;
        }

        if (regions.Count == 0)
        {
            regions.AddRange(BuildFallbackRegions());
        }

        ApplyGeneratedArtwork(regions);
    }

    private static void ApplyGeneratedArtwork(List<WorldRegionDefinition> loadedRegions)
    {
        if (loadedRegions == null)
        {
            return;
        }

        for (var i = 0; i < loadedRegions.Count; i++)
        {
            var region = loadedRegions[i];
            if (region == null || string.IsNullOrWhiteSpace(region.Id))
            {
                continue;
            }

            if (region.IllustrationImage == null)
            {
                region.IllustrationImage = GeneratedArtLibrary.GetWorldRegionIllustration(region.Id);
            }

            if (region.MapIconImage == null)
            {
                region.MapIconImage = region.IllustrationImage != null
                    ? region.IllustrationImage
                    : GeneratedArtLibrary.GetWorldRegionIllustration(region.Id);
            }
        }
    }

    private static IEnumerable<WorldRegionDefinition> BuildFallbackRegions()
    {
        return new[]
        {
            new WorldRegionDefinition
            {
                Id = "green_stone_gate",
                DisplayName = "青石山门",
                Subtitle = "偏安一隅的启程之地",
                Description = "旧宗残脉仍在此处维持山门。灵气稀薄，却胜在安稳，适合刚踏上仙途的修士打磨根基。",
                RequiredRealmTier = 0,
                DangerRank = 1,
                ClearQiReward = 2,
                ClearCrystalReward = 1,
                EnemyCount = 3,
                EliteEnemyCount = 0,
                SpiritNodeCount = 4,
                HerbCount = 2,
                RelicCount = 1,
                LayoutSeed = 1,
                MapPosition = new Vector2(150f, 430f),
                ArenaSize = new Vector2(22f, 13f),
                PlayerSpawn = new Vector2(-6f, -3f),
                BackdropColor = new Color(0.08f, 0.11f, 0.14f, 1f),
                GroundColor = new Color(0.16f, 0.18f, 0.16f, 1f),
                InnerGroundColor = new Color(0.21f, 0.23f, 0.19f, 1f),
                AccentColor = new Color(0.55f, 0.46f, 0.24f, 1f),
                UnlockRegionIds = new[] { "misty_forest", "crimson_valley" }
            },
            new WorldRegionDefinition
            {
                Id = "misty_forest",
                DisplayName = "雾泽林",
                Subtitle = "瘴雾与异草并生",
                Description = "林中水泽相连，雾气遮断神识。这里藏着大量灵草与散落遗物，适合积攒资源。",
                RequiredRealmTier = 0,
                DangerRank = 2,
                ClearQiReward = 3,
                ClearCrystalReward = 1,
                EnemyCount = 4,
                EliteEnemyCount = 1,
                SpiritNodeCount = 5,
                HerbCount = 3,
                RelicCount = 1,
                LayoutSeed = 2,
                MapPosition = new Vector2(420f, 278f),
                ArenaSize = new Vector2(24f, 14f),
                PlayerSpawn = new Vector2(-7.2f, -2.5f),
                BackdropColor = new Color(0.06f, 0.1f, 0.09f, 1f),
                GroundColor = new Color(0.13f, 0.19f, 0.14f, 1f),
                InnerGroundColor = new Color(0.18f, 0.26f, 0.18f, 1f),
                AccentColor = new Color(0.36f, 0.58f, 0.34f, 1f),
                UnlockRegionIds = new[] { "deep_springs" }
            },
            new WorldRegionDefinition
            {
                Id = "crimson_valley",
                DisplayName = "赤霞谷",
                Subtitle = "火脉翻涌的险峡",
                Description = "谷中有残缺地火脉，灵气雄浑却暴躁。若能压住躁火，修为精进极快。",
                RequiredRealmTier = 1,
                DangerRank = 3,
                ClearQiReward = 4,
                ClearCrystalReward = 2,
                EnemyCount = 5,
                EliteEnemyCount = 1,
                SpiritNodeCount = 5,
                HerbCount = 2,
                RelicCount = 1,
                LayoutSeed = 3,
                MapPosition = new Vector2(710f, 470f),
                ArenaSize = new Vector2(25f, 14f),
                PlayerSpawn = new Vector2(-7.5f, -2.8f),
                BackdropColor = new Color(0.14f, 0.08f, 0.07f, 1f),
                GroundColor = new Color(0.24f, 0.15f, 0.12f, 1f),
                InnerGroundColor = new Color(0.31f, 0.18f, 0.13f, 1f),
                AccentColor = new Color(0.74f, 0.38f, 0.19f, 1f),
                UnlockRegionIds = new[] { "northern_pass" }
            },
            new WorldRegionDefinition
            {
                Id = "deep_springs",
                DisplayName = "玄泉洞天",
                Subtitle = "水脉回旋的静修地",
                Description = "古洞深处留有前代修士的导灵阵，适合在此凝练体魄，稳步冲击更高境界。",
                RequiredRealmTier = 1,
                DangerRank = 3,
                ClearQiReward = 4,
                ClearCrystalReward = 2,
                EnemyCount = 5,
                EliteEnemyCount = 1,
                SpiritNodeCount = 6,
                HerbCount = 3,
                RelicCount = 1,
                LayoutSeed = 4,
                MapPosition = new Vector2(850f, 178f),
                ArenaSize = new Vector2(24f, 15f),
                PlayerSpawn = new Vector2(-6.8f, -3.2f),
                BackdropColor = new Color(0.05f, 0.08f, 0.13f, 1f),
                GroundColor = new Color(0.12f, 0.18f, 0.23f, 1f),
                InnerGroundColor = new Color(0.17f, 0.24f, 0.31f, 1f),
                AccentColor = new Color(0.34f, 0.61f, 0.72f, 1f),
                UnlockRegionIds = new[] { "northern_pass" }
            },
            new WorldRegionDefinition
            {
                Id = "northern_pass",
                DisplayName = "北冥古道",
                Subtitle = "通往外域的破碎山道",
                Description = "古道尽头是更辽阔的州域。风沙中残留的大型阵痕，意味着更强敌手也已接近。",
                RequiredRealmTier = 2,
                DangerRank = 4,
                ClearQiReward = 5,
                ClearCrystalReward = 3,
                EnemyCount = 6,
                EliteEnemyCount = 2,
                SpiritNodeCount = 6,
                HerbCount = 3,
                RelicCount = 2,
                LayoutSeed = 5,
                MapPosition = new Vector2(1130f, 350f),
                ArenaSize = new Vector2(27f, 16f),
                PlayerSpawn = new Vector2(-8.2f, -3.4f),
                BackdropColor = new Color(0.09f, 0.09f, 0.11f, 1f),
                GroundColor = new Color(0.19f, 0.19f, 0.2f, 1f),
                InnerGroundColor = new Color(0.26f, 0.24f, 0.22f, 1f),
                AccentColor = new Color(0.63f, 0.52f, 0.33f, 1f),
                UnlockRegionIds = new[] { "celestial_ruins" }
            },
            new WorldRegionDefinition
            {
                Id = "celestial_ruins",
                DisplayName = "天外灵墟",
                Subtitle = "大域边缘的残天遗迹",
                Description = "这里已不再是小地方的格局。破碎遗迹连接着更高层次的修真世界，也是后续章节的扩展入口。",
                RequiredRealmTier = 3,
                DangerRank = 5,
                ClearQiReward = 6,
                ClearCrystalReward = 4,
                EnemyCount = 7,
                EliteEnemyCount = 2,
                SpiritNodeCount = 7,
                HerbCount = 4,
                RelicCount = 2,
                LayoutSeed = 6,
                MapPosition = new Vector2(1380f, 220f),
                ArenaSize = new Vector2(28f, 17f),
                PlayerSpawn = new Vector2(-8.6f, -3.6f),
                BackdropColor = new Color(0.08f, 0.07f, 0.13f, 1f),
                GroundColor = new Color(0.16f, 0.14f, 0.22f, 1f),
                InnerGroundColor = new Color(0.25f, 0.2f, 0.31f, 1f),
                AccentColor = new Color(0.64f, 0.45f, 0.75f, 1f),
                UnlockRegionIds = Array.Empty<string>()
            }
        };
    }
}
