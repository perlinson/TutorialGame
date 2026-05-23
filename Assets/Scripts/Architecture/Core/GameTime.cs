using QFramework;

/// <summary>
/// <see cref="IGameTimeService"/> 的窄静态桥接。新代码请优先使用此入口而非 <see cref="CultivationGameTime"/> 静态助手。
/// </summary>
public static class GameTime
{
    private static IGameTimeService Service
    {
        get
        {
            CultivationApp.EnsureInitialized();
            return CultivationApp.Interface.GetUtility<IGameTimeService>();
        }
    }

    public static int SegmentsPerDay => Service.SegmentsPerDay;
    public static IReadonlyBindableProperty<float> TimeScale => Service.TimeScale;
    public static IReadonlyBindableProperty<bool> Paused => Service.Paused;

    public static void EnsureDefaults(MainMenuSaveData saveData) => Service.EnsureDefaults(saveData);
    public static void Advance(MainMenuSaveData saveData, int segments) => Service.Advance(saveData, segments);
    public static string Format(MainMenuSaveData saveData) => Service.Format(saveData);
    public static string GetTimeLabel(int timeIndex) => Service.GetTimeLabel(timeIndex);

    public static void SetTimeScale(float scale) => Service.SetTimeScale(scale);
    public static void Pause() => Service.Pause();
    public static void Resume() => Service.Resume();
    public static void TogglePause() => Service.TogglePause();
}
