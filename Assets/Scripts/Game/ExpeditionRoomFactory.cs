using System;
using System.Collections.Generic;
using UnityEngine;

public static class ExpeditionRoomFactory
{
    private static RoomEventTableAsset cachedRoomEventTable;

    public static List<ExpeditionRoomState> Build(WorldRegionDefinition region, MainMenuSaveData saveData, System.Random random)
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

    private static ExpeditionRoomKind PickIntermediateRoomKind(WorldRegionDefinition region, MainMenuSaveData saveData, System.Random random, int index)
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

        switch (room.Kind)
        {
            case ExpeditionRoomKind.Battle:
                room.Title = "阴影甬道";
                room.Description = "甬道里残留着新近踩踏痕迹，若继续前探，多半要与伏兵碰面。";
                break;
            case ExpeditionRoomKind.Treasure:
                room.Title = "散落行囊";
                room.Description = "破损行囊和散碎残卷躺在角落，像是在引诱后人俯身搜查。";
                break;
            case ExpeditionRoomKind.Herb:
                room.Title = "灵草湿地";
                room.Description = "潮气从裂缝翻涌而上，这种地方常能长出稳神与疗伤类灵草。";
                break;
            case ExpeditionRoomKind.Shrine:
                room.Title = "残阵祭台";
                room.Description = "半毁的祭台仍在微弱运转，也许能借其阵势稳定真元。";
                break;
            default:
                room.Title = "隐伏险机";
                room.Description = "地表纹理异常断裂，脚下很可能埋着旧阵或瘴陷。";
                break;
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

        room.Title = "山门外缘";
        room.Description = "远征队刚踏出山门旧界，需要先重新校正地脉与撤退路径。";
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

        room.Title = "险隘关口";
        room.Description = "狭窄地形迫使队伍正面接战，这里通常盘踞着更难缠的敌手。";
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

        room.Title = "核心险地";
        room.Description = "灵压在前方凝成实质，真正的镇守者就潜伏在尽头。";
    }

    private static RoomEventTableAsset GetRoomEventTable()
    {
        if (cachedRoomEventTable == null)
        {
            cachedRoomEventTable = CultivationApp.LoadResource<RoomEventTableAsset>("Data/RoomEventTable");
        }

        return cachedRoomEventTable;
    }

    private static void ApplyVisualCopy(ExpeditionRoomState room)
    {
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
