using System.Collections.Generic;
using UnityEngine;
using QFramework;

public sealed class CultivationEncounterDirectorSystem : AbstractSystem
{
    private CultivationTaskSystem taskSystem;
    private CultivationFactionSystem factionSystem;

    protected override void OnInit()
    {
        taskSystem = this.GetSystem<CultivationTaskSystem>();
        factionSystem = this.GetSystem<CultivationFactionSystem>();
    }

    public List<ExpeditionRoomState> BuildRooms(WorldRegionDefinition region, MainMenuSaveData saveData, System.Random random)
    {
        var rooms = ExpeditionRoomFactory.Build(region, saveData, random);
        if (region == null || saveData == null || rooms == null || rooms.Count <= 2)
        {
            return rooms;
        }

        var taskContext = taskSystem.GetActiveTaskContext(saveData);
        if (IsTaskLinkedToRegion(taskContext, region.Id))
        {
            ForceTaskPressureRoom(rooms, region);
        }

        var pressure = GetHighestFactionPressure(saveData);
        if (pressure >= 4)
        {
            ForceEliteWarningRoom(rooms, region);
        }

        return rooms;
    }

    public List<ExpeditionEnemyState> BuildEnemies(WorldRegionDefinition region, ExpeditionRoomState room, MainMenuSaveData saveData, System.Random random)
    {
        var taskContext = saveData != null ? taskSystem.GetActiveTaskContext(saveData) : null;
        var preferredFaction = taskContext != null && taskContext.HasLinkedFaction ? taskContext.TaskLinkedFaction : (ExpeditionEnemyFaction?)null;
        var pressure = preferredFaction.HasValue && saveData != null ? factionSystem.GetFactionPressure(saveData, preferredFaction.Value) : 0;
        return ExpeditionEnemyFactory.BuildEnemies(region, room, random, preferredFaction, pressure);
    }

    private static void ForceTaskPressureRoom(List<ExpeditionRoomState> rooms, WorldRegionDefinition region)
    {
        var targetIndex = Mathf.Clamp(rooms.Count / 2, 1, rooms.Count - 2);
        var room = rooms[targetIndex];
        if (room.Kind == ExpeditionRoomKind.Boss || room.Kind == ExpeditionRoomKind.Elite)
        {
            return;
        }

        room.Kind = ExpeditionRoomKind.Battle;
        ExpeditionRoomFactory.ApplyRoomCopy(room);
        if (room.IllustrationImage == null && region != null)
        {
            room.IllustrationImage = region.IllustrationImage;
        }
    }

    private static void ForceEliteWarningRoom(List<ExpeditionRoomState> rooms, WorldRegionDefinition region)
    {
        var targetIndex = Mathf.Clamp(rooms.Count - 2, 1, rooms.Count - 2);
        var room = rooms[targetIndex];
        if (room.Kind == ExpeditionRoomKind.Boss)
        {
            return;
        }

        room.Kind = ExpeditionRoomKind.Elite;
        ExpeditionRoomFactory.ApplyEliteCopy(room);
        if (room.IllustrationImage == null && region != null)
        {
            room.IllustrationImage = region.IllustrationImage;
        }
    }

    private int GetHighestFactionPressure(MainMenuSaveData saveData)
    {
        var highest = 0;
        for (var i = 0; i < 5; i++)
        {
            highest = Mathf.Max(highest, factionSystem.GetFactionPressure(saveData, (ExpeditionEnemyFaction)i));
        }

        return highest;
    }

    private static bool IsTaskLinkedToRegion(TaskContextSnapshot taskContext, string regionId)
    {
        if (taskContext == null || taskContext.TaskLinkedRegionIds == null || string.IsNullOrWhiteSpace(regionId))
        {
            return false;
        }

        for (var i = 0; i < taskContext.TaskLinkedRegionIds.Length; i++)
        {
            if (taskContext.TaskLinkedRegionIds[i] == regionId)
            {
                return true;
            }
        }

        return false;
    }
}
