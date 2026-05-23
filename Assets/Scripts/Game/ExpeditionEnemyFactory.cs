using System.Collections.Generic;
using UnityEngine;

public static class ExpeditionEnemyFactory
{
    private static EnemyArchetypeDatabaseAsset cachedEnemyDatabase;
    private static RegionEncounterDatabaseAsset cachedEncounterDatabase;

    public static List<ExpeditionEnemyState> BuildEnemies(WorldRegionDefinition region, ExpeditionRoomState room, System.Random random)
    {
        return BuildEnemies(region, room, random, null, 0);
    }

    public static List<ExpeditionEnemyState> BuildEnemies(
        WorldRegionDefinition region,
        ExpeditionRoomState room,
        System.Random random,
        ExpeditionEnemyFaction? preferredFaction,
        int pressureLevel)
    {
        var enemies = new List<ExpeditionEnemyState>();
        var enemyCount = room.Kind == ExpeditionRoomKind.Boss ? 3 : room.Kind == ExpeditionRoomKind.Elite ? 2 : 1 + region.DangerRank / 2;
        if (pressureLevel >= 4 && room.Kind == ExpeditionRoomKind.Battle)
        {
            enemyCount++;
        }

        var factions = GetRegionFactions(region.Id);

        for (var i = 0; i < enemyCount; i++)
        {
            var isElite = room.Kind == ExpeditionRoomKind.Boss || room.Kind == ExpeditionRoomKind.Elite || (i == enemyCount - 1 && region.DangerRank >= 4);
            var faction = preferredFaction.HasValue && i == 0 ? preferredFaction.Value : factions[(room.Seed + i * 5 + (isElite ? 1 : 0)) % factions.Length];
            enemies.Add(CreateEnemy(region, room, faction, isElite, i));
        }

        return enemies;
    }

    public static ExpeditionEnemyState Restore(PersistentExpeditionEnemySnapshot snapshot)
    {
        if (snapshot == null)
        {
            return null;
        }

        snapshot.EnsureDefaults();
        var portrait = ResolvePortrait(snapshot);
        var enemy = new ExpeditionEnemyState(
            snapshot.faction,
            snapshot.name,
            snapshot.techniqueName,
            portrait,
            snapshot.maxHealth,
            snapshot.damage,
            snapshot.stressDamage,
            snapshot.isElite,
            snapshot.armor,
            snapshot.poisonResistance,
            snapshot.stunResistance,
            snapshot.position);
        enemy.CurrentHealth = snapshot.currentHealth;
        enemy.PoisonStacks = snapshot.poisonStacks;
        enemy.ExposedTurns = snapshot.exposedTurns;
        enemy.StunnedTurns = snapshot.stunnedTurns;
        enemy.Armor = snapshot.armor;
        return enemy;
    }

    private static ExpeditionEnemyState CreateEnemy(WorldRegionDefinition region, ExpeditionRoomState room, ExpeditionEnemyFaction faction, bool isElite, int index)
    {
        var maxHealth = 7 + region.DangerRank * 2 + (isElite ? 5 : 0) + index;
        var damage = 2 + region.RequiredRealmTier + (isElite ? 2 : 0);
        var stressDamage = 4 + region.DangerRank + (isElite ? 2 : 0);
        var armor = isElite ? 2 : 1;
        var poisonResistance = 0;
        var stunResistance = 0;
        string name;
        string techniqueName;

        var archetype = GetEnemyArchetype(faction, isElite, room.Kind, room.Seed + index * 11);
        if (archetype != null)
        {
            name = archetype.displayName;
            techniqueName = archetype.techniqueName;
            maxHealth += archetype.healthOffset;
            damage += archetype.damageOffset;
            stressDamage += archetype.stressOffset;
            armor += archetype.armorOffset;
            poisonResistance = Mathf.Max(0, archetype.poisonResistance);
            stunResistance = Mathf.Max(0, archetype.stunResistance);
        }
        else
        {
            ApplyFallbackEnemyData(room.Kind, faction, index, isElite, ref maxHealth, ref damage, ref stressDamage, ref armor, ref poisonResistance, ref stunResistance, out name, out techniqueName);
        }

        maxHealth = Mathf.Max(1, maxHealth);
        damage = Mathf.Max(1, damage);
        stressDamage = Mathf.Max(1, stressDamage);
        armor = Mathf.Max(0, armor);

        var portrait = archetype != null ? archetype.portraitImage : null;
        if (portrait == null)
        {
            portrait = GeneratedArtLibrary.GetEnemyPortrait(faction, isElite);
        }

        return new ExpeditionEnemyState(
            faction,
            name,
            techniqueName,
            portrait,
            maxHealth,
            damage,
            stressDamage,
            isElite,
            armor,
            poisonResistance,
            stunResistance,
            index);
    }

    private static EnemyArchetypeRecord GetEnemyArchetype(ExpeditionEnemyFaction faction, bool isElite, ExpeditionRoomKind roomKind, int seed)
    {
        var database = GetEnemyDatabase();
        if (database == null || database.archetypes == null || database.archetypes.Length == 0)
        {
            return null;
        }

        var candidates = new List<EnemyArchetypeRecord>();
        for (var i = 0; i < database.archetypes.Length; i++)
        {
            var archetype = database.archetypes[i];
            if (archetype == null || archetype.faction != (int)faction)
            {
                continue;
            }

            if (isElite && !archetype.eliteOnly && HasEliteCandidate(database, faction, roomKind))
            {
                continue;
            }

            if (!isElite && archetype.eliteOnly)
            {
                continue;
            }

            if (!MatchesRoomKind(archetype, roomKind))
            {
                continue;
            }

            candidates.Add(archetype);
        }

        if (candidates.Count == 0)
        {
            for (var i = 0; i < database.archetypes.Length; i++)
            {
                var archetype = database.archetypes[i];
                if (archetype == null || archetype.faction != (int)faction)
                {
                    continue;
                }

                if (!isElite && archetype.eliteOnly)
                {
                    continue;
                }

                candidates.Add(archetype);
            }
        }

        return candidates.Count > 0 ? candidates[Mathf.Abs(seed) % candidates.Count] : null;
    }

    private static Sprite ResolvePortrait(PersistentExpeditionEnemySnapshot snapshot)
    {
        var database = GetEnemyDatabase();
        if (database == null || database.archetypes == null)
        {
            return GeneratedArtLibrary.GetEnemyPortrait(snapshot.faction, snapshot.isElite);
        }

        for (var i = 0; i < database.archetypes.Length; i++)
        {
            var archetype = database.archetypes[i];
            if (archetype == null
                || archetype.faction != (int)snapshot.faction
                || archetype.displayName != snapshot.name
                || archetype.techniqueName != snapshot.techniqueName)
            {
                continue;
            }

            return archetype.portraitImage;
        }

        return GeneratedArtLibrary.GetEnemyPortrait(snapshot.faction, snapshot.isElite);
    }

    private static bool HasEliteCandidate(EnemyArchetypeDatabaseAsset database, ExpeditionEnemyFaction faction, ExpeditionRoomKind roomKind)
    {
        if (database == null || database.archetypes == null)
        {
            return false;
        }

        for (var i = 0; i < database.archetypes.Length; i++)
        {
            var archetype = database.archetypes[i];
            if (archetype != null && archetype.faction == (int)faction && archetype.eliteOnly && MatchesRoomKind(archetype, roomKind))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesRoomKind(EnemyArchetypeRecord archetype, ExpeditionRoomKind roomKind)
    {
        if (archetype.allowedRoomKinds == null || archetype.allowedRoomKinds.Length == 0)
        {
            return true;
        }

        for (var i = 0; i < archetype.allowedRoomKinds.Length; i++)
        {
            if (archetype.allowedRoomKinds[i] == (int)roomKind)
            {
                return true;
            }
        }

        return false;
    }

    private static EnemyArchetypeDatabaseAsset GetEnemyDatabase()
    {
        if (cachedEnemyDatabase == null)
        {
            cachedEnemyDatabase = GameData.LoadAsset<EnemyArchetypeDatabaseAsset>("Data/EnemyArchetypeDatabase");
        }

        return cachedEnemyDatabase;
    }

    private static RegionEncounterDatabaseAsset GetEncounterDatabase()
    {
        if (cachedEncounterDatabase == null)
        {
            cachedEncounterDatabase = GameData.LoadAsset<RegionEncounterDatabaseAsset>("Data/RegionEncounterDatabase");
        }

        return cachedEncounterDatabase;
    }

    private static ExpeditionEnemyFaction[] GetRegionFactions(string regionId)
    {
        var database = GetEncounterDatabase();
        if (database != null && database.profiles != null)
        {
            for (var i = 0; i < database.profiles.Length; i++)
            {
                var profile = database.profiles[i];
                if (profile == null || profile.regionId != regionId || profile.factions == null || profile.factions.Length == 0)
                {
                    continue;
                }

                var factions = new ExpeditionEnemyFaction[profile.factions.Length];
                for (var factionIndex = 0; factionIndex < profile.factions.Length; factionIndex++)
                {
                    factions[factionIndex] = (ExpeditionEnemyFaction)profile.factions[factionIndex];
                }

                return factions;
            }
        }

        return GetRegionFactionsFallback(regionId);
    }

    private static ExpeditionEnemyFaction[] GetRegionFactionsFallback(string regionId)
    {
        switch (regionId)
        {
            case "misty_forest":
                return new[] { ExpeditionEnemyFaction.Beast, ExpeditionEnemyFaction.HeartDemon, ExpeditionEnemyFaction.Bandit };
            case "crimson_valley":
                return new[] { ExpeditionEnemyFaction.Cultivator, ExpeditionEnemyFaction.Beast, ExpeditionEnemyFaction.HeartDemon };
            case "deep_springs":
                return new[] { ExpeditionEnemyFaction.CorpsePuppet, ExpeditionEnemyFaction.HeartDemon, ExpeditionEnemyFaction.Cultivator };
            case "northern_pass":
                return new[] { ExpeditionEnemyFaction.Bandit, ExpeditionEnemyFaction.Cultivator, ExpeditionEnemyFaction.CorpsePuppet, ExpeditionEnemyFaction.Beast };
            case "celestial_ruins":
                return new[] { ExpeditionEnemyFaction.CorpsePuppet, ExpeditionEnemyFaction.HeartDemon, ExpeditionEnemyFaction.Cultivator, ExpeditionEnemyFaction.Beast };
            default:
                return new[] { ExpeditionEnemyFaction.Bandit, ExpeditionEnemyFaction.HeartDemon, ExpeditionEnemyFaction.Cultivator };
        }
    }

    private static void ApplyFallbackEnemyData(
        ExpeditionRoomKind roomKind,
        ExpeditionEnemyFaction faction,
        int index,
        bool isElite,
        ref int maxHealth,
        ref int damage,
        ref int stressDamage,
        ref int armor,
        ref int poisonResistance,
        ref int stunResistance,
        out string name,
        out string techniqueName)
    {
        var strategy = FactionStrategyRegistry.Get(faction);
        if (strategy != null)
        {
            strategy.ApplyFallbackEnemyData(index, isElite, roomKind,
                ref maxHealth, ref damage, ref stressDamage,
                ref armor, ref poisonResistance, ref stunResistance,
                out name, out techniqueName);
            return;
        }

        name = "未知敌人";
        techniqueName = "普通攻击";
    }
}
