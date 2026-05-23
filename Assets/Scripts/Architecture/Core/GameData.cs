using QFramework;
using UnityEngine;

/// <summary>
/// <see cref="IGameDataService"/> 的窄静态桥接，与 <see cref="GameResource"/> 平级使用。
/// Library / 工厂类应优先调用本桥接读取数据库与 Json 配置，避免散落的 <c>GameResource.Load&lt;DatabaseAsset&gt;</c> 各自缓存。
/// </summary>
public static class GameData
{
    private static IGameDataService Service
    {
        get
        {
            CultivationApp.EnsureInitialized();
            return CultivationApp.Interface.GetUtility<IGameDataService>();
        }
    }

    public static T LoadAsset<T>(string resourcePath) where T : ScriptableObject
    {
        return Service.LoadAsset<T>(resourcePath);
    }

    public static T LoadJson<T>(string resourcePath) where T : class, new()
    {
        return Service.LoadJson<T>(resourcePath);
    }

    public static bool IsCached(string resourcePath)
    {
        return Service.IsCached(resourcePath);
    }

    public static void ClearCache()
    {
        Service.ClearCache();
    }

    public static void Reload()
    {
        Service.Reload();
    }
}
