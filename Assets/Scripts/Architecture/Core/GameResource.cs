using QFramework;
using UnityEngine;

public static class GameResource
{
    private static IGameResourceService Service
    {
        get
        {
            CultivationApp.EnsureInitialized();
            return CultivationApp.Interface.GetUtility<IGameResourceService>();
        }
    }

    public static string BackendName
    {
        get { return Service.BackendName; }
    }

    public static T Load<T>(string path) where T : Object
    {
        return Service.Load<T>(path);
    }

    public static GameObject InstantiatePrefab(string path, Transform parent = null)
    {
        return Service.InstantiatePrefab(path, parent);
    }

    public static void ClearCache()
    {
        Service.ClearCache();
    }
}
