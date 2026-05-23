using System.Collections.Generic;
using UnityEngine;

public interface IRoomKindStrategy
{
    ExpeditionRoomKind Kind { get; }
    string DefaultTitle { get; }
    string DefaultDescription { get; }
    string Symbol { get; }
    int DefaultEnemyCount(WorldRegionDefinition region);
    bool IsCombatRoom { get; }
    void SpawnEventActors(GameObject arenaRoot, ExpeditionRoomState room, WorldRegionDefinition region, System.Random random, GameController controller);
    void ApplyFallbackLoot(List<SaveItemStack> loot, string regionId);
}
