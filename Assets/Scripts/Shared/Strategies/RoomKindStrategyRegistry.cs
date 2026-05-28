using System.Collections.Generic;
using UnityEngine;

public sealed class ArenaContextAdapter : IExpeditionArenaContext
{
    private readonly WorldRegionDefinition region;
    private readonly GameController controller;

    public ArenaContextAdapter(WorldRegionDefinition region, GameController controller)
    {
        this.region = region;
        this.controller = controller;
    }

    public Vector2 ArenaMinBounds => new Vector2(-region.ArenaSize.x * 0.5f + 0.9f, -region.ArenaSize.y * 0.5f + 0.9f);
    public Vector2 ArenaMaxBounds => new Vector2(region.ArenaSize.x * 0.5f - 0.9f, region.ArenaSize.y * 0.5f - 0.9f);
    public Color AccentColor => region.AccentColor;
    public Color InnerGroundColor => region.InnerGroundColor;

    public void SpawnSpiritNode(Vector2 position, int qiAmount)
    {
        controller.SpawnSpiritNodeAt(position, qiAmount);
    }

    public void SpawnHerb(Vector2 position, int healAmount, int qiAmount)
    {
        controller.SpawnHerbAt(position, healAmount, qiAmount);
    }

    public void SpawnRelic(Vector2 position, int crystalAmount)
    {
        controller.SpawnRelicAt(position, crystalAmount);
    }

    public void CreateDecor(string name, Vector2 position, Vector2 size, Color color, int sortingOrder)
    {
        controller.CreateArenaDecor(name, position, size, color, sortingOrder);
    }
}

public static class RoomKindStrategyRegistry
{
    private static readonly Dictionary<ExpeditionRoomKind, IRoomKindStrategy> strategies;
    private static bool initialized;

    static RoomKindStrategyRegistry()
    {
        strategies = new Dictionary<ExpeditionRoomKind, IRoomKindStrategy>();
    }

    public static void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        Register(new ScoutRoomStrategy());
        Register(new BattleRoomStrategy());
        Register(new EliteRoomStrategy());
        Register(new TreasureRoomStrategy());
        Register(new HerbRoomStrategy());
        Register(new ShrineRoomStrategy());
        Register(new TrapRoomStrategy());
        Register(new BossRoomStrategy());
    }

    public static void Register(IRoomKindStrategy strategy)
    {
        if (strategy == null) return;
        strategies[strategy.Kind] = strategy;
    }

    public static IRoomKindStrategy Get(ExpeditionRoomKind kind)
    {
        EnsureInitialized();
        strategies.TryGetValue(kind, out var strategy);
        return strategy;
    }

    public static bool IsCombatRoom(ExpeditionRoomKind kind)
    {
        var strategy = Get(kind);
        return strategy != null && strategy.IsCombatRoom;
    }
}
