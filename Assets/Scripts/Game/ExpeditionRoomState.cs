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
            switch (Kind)
            {
                case ExpeditionRoomKind.Scout:
                    return "途";
                case ExpeditionRoomKind.Battle:
                    return "战";
                case ExpeditionRoomKind.Elite:
                    return "险";
                case ExpeditionRoomKind.Treasure:
                    return "宝";
                case ExpeditionRoomKind.Herb:
                    return "药";
                case ExpeditionRoomKind.Shrine:
                    return "祭";
                case ExpeditionRoomKind.Trap:
                    return "陷";
                default:
                    return "王";
            }
        }
    }
}
