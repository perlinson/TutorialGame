/// <summary>
/// <see cref="IGameRandomService"/> 的窄静态桥接。新代码请优先用此入口，避免散落的 <see cref="UnityEngine.Random"/> 调用。
/// </summary>
public static class GameRandom
{
    private static IGameRandomService Service
    {
        get
        {
            CultivationApp.EnsureInitialized();
            return CultivationApp.Interface.GetUtility<IGameRandomService>();
        }
    }

    public static int CurrentSeed => Service.CurrentSeed;
    public static bool IsSeeded => Service.IsSeeded;

    public static void Reseed(int seed) => Service.Reseed(seed);
    public static void ClearSeed() => Service.ClearSeed();

    public static int Range(int minInclusive, int maxExclusive) => Service.Range(minInclusive, maxExclusive);
    public static float Range01() => Service.Range01();

    /// <summary>把当前随机服务作为 <see cref="IGameRandomSource"/> 抛出，便于注入到 DamageSystem 等。</summary>
    public static IGameRandomSource AsSource()
    {
        return Service;
    }
}
