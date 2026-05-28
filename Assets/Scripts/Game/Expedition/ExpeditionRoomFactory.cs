using System;
using System.Collections.Generic;
using UnityEngine;

public static class ExpeditionRoomFactory
{
    private static RoomEventTableAsset cachedRoomEventTable;

    public static List<ExpeditionRoomState> Build(WorldRegionDefinition region, CultivationSaveData saveData, System.Random random)
    {
        var rooms = new List<ExpeditionRoomState>();
        var roomCount = Mathf.Clamp(4 + region.DangerRank, 5, 8);

        for (var i = 0; i < roomCount; i++)
        {
            var room = new ExpeditionRoomState
            {
                Index = i,
                Seed = random.Next(1000, 9999)
            };

            if (i == 0)
            {
                room.Kind = ExpeditionRoomKind.Scout;
                ApplyStartScoutCopy(room);
            }
            else if (i == roomCount - 1)
            {
                room.Kind = ExpeditionRoomKind.Boss;
                ApplyBossCopy(room);
            }
            else if (i == roomCount - 2 && region.DangerRank >= 3)
            {
                room.Kind = ExpeditionRoomKind.Elite;
                ApplyEliteCopy(room);
            }
            else
            {
                room.Kind = PickIntermediateRoomKind(region, saveData, random, i);
                ApplyRoomCopy(room);
            }

            if (room.IllustrationImage == null)
            {
                room.IllustrationImage = region.IllustrationImage;
            }

            rooms.Add(room);
        }

        return rooms;
    }

    public static ExpeditionRoomState Restore(PersistentExpeditionRoomSnapshot snapshot, WorldRegionDefinition region)
    {
        if (snapshot == null)
        {
            return null;
        }

        snapshot.EnsureDefaults();
        var room = new ExpeditionRoomState
        {
            Index = snapshot.index,
            Kind = snapshot.kind,
            Title = snapshot.title,
            Description = snapshot.description,
            Seed = snapshot.seed,
            Visited = snapshot.visited,
            Resolved = snapshot.resolved
        };

        var visualTemplate = new ExpeditionRoomState
        {
            Index = snapshot.index,
            Kind = snapshot.kind
        };
        ApplyVisualCopy(visualTemplate);
        room.IllustrationImage = visualTemplate.IllustrationImage != null
            ? visualTemplate.IllustrationImage
            : region != null ? region.IllustrationImage : null;

        if (string.IsNullOrWhiteSpace(room.Title))
        {
            room.Title = visualTemplate.Title;
        }

        if (string.IsNullOrWhiteSpace(room.Description))
        {
            room.Description = visualTemplate.Description;
        }

        return room;
    }

    private static ExpeditionRoomKind PickIntermediateRoomKind(WorldRegionDefinition region, CultivationSaveData saveData, System.Random random, int index)
    {
        saveData.EnsureDefaults();
        var roll = (region.LayoutSeed + index * 3 + saveData.mainArtifactLevel + random.Next(0, 10)) % 10;
        if (roll <= 2)
        {
            return ExpeditionRoomKind.Battle;
        }

        if (roll <= 4)
        {
            return ExpeditionRoomKind.Treasure;
        }

        if (roll <= 6)
        {
            return ExpeditionRoomKind.Herb;
        }

        if (roll <= 7)
        {
            return ExpeditionRoomKind.Shrine;
        }

        return ExpeditionRoomKind.Trap;
    }

    public static void ApplyRoomCopy(ExpeditionRoomState room)
    {
        var table = GetRoomEventTable();
        if (table != null && table.roomCopies != null)
        {
            for (var i = 0; i < table.roomCopies.Length; i++)
            {
                var record = table.roomCopies[i];
                if (record == null || record.roomKind != (int)room.Kind)
                {
                    continue;
                }

                room.Title = record.title;
                room.IllustrationImage = record.illustrationImage;
                room.Description = record.description;
                return;
            }
        }

        var strategy = RoomKindStrategyRegistry.Get(room.Kind);
        if (strategy != null)
        {
            room.Title = strategy.DefaultTitle;
            room.Description = strategy.DefaultDescription;
        }
    }

    private static void ApplyStartScoutCopy(ExpeditionRoomState room)
    {
        var table = GetRoomEventTable();
        if (table != null && !string.IsNullOrWhiteSpace(table.startScoutTitle))
        {
            room.Title = table.startScoutTitle;
            room.IllustrationImage = table.startScoutImage;
            room.Description = table.startScoutDescription;
            return;
        }

        var strategy = RoomKindStrategyRegistry.Get(ExpeditionRoomKind.Scout);
        if (strategy != null)
        {
            room.Title = strategy.DefaultTitle;
            room.Description = strategy.DefaultDescription;
        }
    }

    public static void ApplyEliteCopy(ExpeditionRoomState room)
    {
        var table = GetRoomEventTable();
        if (table != null && !string.IsNullOrWhiteSpace(table.eliteTitle))
        {
            room.Title = table.eliteTitle;
            room.IllustrationImage = table.eliteImage;
            room.Description = table.eliteDescription;
            return;
        }

        var strategy = RoomKindStrategyRegistry.Get(ExpeditionRoomKind.Elite);
        if (strategy != null)
        {
            room.Title = strategy.DefaultTitle;
            room.Description = strategy.DefaultDescription;
        }
    }

    private static void ApplyBossCopy(ExpeditionRoomState room)
    {
        var table = GetRoomEventTable();
        if (table != null && !string.IsNullOrWhiteSpace(table.bossTitle))
        {
            room.Title = table.bossTitle;
            room.IllustrationImage = table.bossImage;
            room.Description = table.bossDescription;
            return;
        }

        var strategy = RoomKindStrategyRegistry.Get(ExpeditionRoomKind.Boss);
        if (strategy != null)
        {
            room.Title = strategy.DefaultTitle;
            room.Description = strategy.DefaultDescription;
        }
    }

    private static RoomEventTableAsset GetRoomEventTable()
    {
        if (cachedRoomEventTable == null)
        {
            cachedRoomEventTable = GameResource.Load<RoomEventTableAsset>("Data/RoomEventTable");
        }

        return cachedRoomEventTable;
    }

    private static void ApplyVisualCopy(ExpeditionRoomState room)
    {
        if (RoomKindStrategyRegistry.IsCombatRoom(room.Kind))
        {
            ApplyRoomCopy(room);
            return;
        }

        switch (room.Kind)
        {
            case ExpeditionRoomKind.Scout:
                ApplyStartScoutCopy(room);
                break;
            case ExpeditionRoomKind.Elite:
                ApplyEliteCopy(room);
                break;
            case ExpeditionRoomKind.Boss:
                ApplyBossCopy(room);
                break;
            default:
                ApplyRoomCopy(room);
                break;
        }
    }
}
