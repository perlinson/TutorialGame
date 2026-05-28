using UnityEngine;

public enum ExpeditionRoomKind
{
    Scout,
    Battle,
    Elite,
    Treasure,
    Herb,
    Shrine,
    Trap,
    Boss
}

public sealed class ExpeditionRoomState
{
    public int Index;
    public ExpeditionRoomKind Kind;
    public string Title;
    public string Description;
    public Sprite IllustrationImage;
    public int Seed;
    public bool Visited;
    public bool Resolved;

    public string Symbol
    {
        get
        {
            var strategy = RoomKindStrategyRegistry.Get(Kind);
            return strategy != null ? strategy.Symbol : "?";
        }
    }
}
