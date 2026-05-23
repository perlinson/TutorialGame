/// <summary>
/// 阴阳五行 + 变体元素体系。
/// - 五行：金 / 木 / 水 / 火 / 土。
/// - 阴阳：阴 / 阳。
/// - 变体：雷 / 冰 / 毒。
/// - <see cref="None"/> 表示无元素属性（纯物理 / 真元类技能）。
/// 数值表（克制倍率）由 Json 配置 <see cref="ElementMatchupTable"/> 提供，运行时通过
/// <see cref="IGameDataService"/> 加载。
/// </summary>
public enum CombatElement
{
    None = 0,
    // 五行
    Metal = 1,
    Wood = 2,
    Water = 3,
    Fire = 4,
    Earth = 5,
    // 阴阳
    Yin = 6,
    Yang = 7,
    // 变体
    Thunder = 8,
    Ice = 9,
    Poison = 10,
}

public static class CombatElementExtensions
{
    public const int Count = 11;

    public static string GetDisplayName(this CombatElement element)
    {
        switch (element)
        {
            case CombatElement.Metal: return "金";
            case CombatElement.Wood: return "木";
            case CombatElement.Water: return "水";
            case CombatElement.Fire: return "火";
            case CombatElement.Earth: return "土";
            case CombatElement.Yin: return "阴";
            case CombatElement.Yang: return "阳";
            case CombatElement.Thunder: return "雷";
            case CombatElement.Ice: return "冰";
            case CombatElement.Poison: return "毒";
            default: return "无";
        }
    }
}
