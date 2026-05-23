using QFramework;
using UnityEngine;

/// <summary>
/// 游戏内日历 / 时间流速服务。
/// - 日历部分（<see cref="EnsureDefaults"/>/<see cref="Advance"/>/<see cref="Format"/>/<see cref="GetTimeLabel"/>）按存档驱动，
///   维持与 <see cref="CultivationGameTime"/> 静态助手的等价语义，便于现有调用方逐步迁移。
/// - 实时控制部分（<see cref="TimeScale"/>/<see cref="Paused"/>）是新增能力，预留给"修炼快进 / 暂停弹窗 / 战斗节奏"使用。
///   服务自身不会自动写 <c>UnityEngine.Time.timeScale</c>，由调用方在合适的层（FSM / Controller）决定是否同步到引擎层。
/// </summary>
public interface IGameTimeService : IUtility
{
    int SegmentsPerDay { get; }
    IReadonlyBindableProperty<float> TimeScale { get; }
    IReadonlyBindableProperty<bool> Paused { get; }

    void EnsureDefaults(MainMenuSaveData saveData);
    void Advance(MainMenuSaveData saveData, int segments);
    string Format(MainMenuSaveData saveData);
    string GetTimeLabel(int timeIndex);

    void SetTimeScale(float scale);
    void Pause();
    void Resume();
    void TogglePause();
}

public sealed class GameTimeService : IGameTimeService
{
    private const float MinTimeScale = 0.1f;
    private const float MaxTimeScale = 8f;

    private readonly BindableProperty<float> timeScale = new BindableProperty<float>(1f);
    private readonly BindableProperty<bool> paused = new BindableProperty<bool>(false);

    public int SegmentsPerDay => 12;
    public IReadonlyBindableProperty<float> TimeScale => timeScale;
    public IReadonlyBindableProperty<bool> Paused => paused;

    public void EnsureDefaults(MainMenuSaveData saveData)
    {
        CultivationGameTime.EnsureDefaults(saveData);
    }

    public void Advance(MainMenuSaveData saveData, int segments)
    {
        CultivationGameTime.Advance(saveData, segments);
    }

    public string Format(MainMenuSaveData saveData)
    {
        return CultivationGameTime.Format(saveData);
    }

    public string GetTimeLabel(int timeIndex)
    {
        return CultivationGameTime.GetTimeLabel(timeIndex);
    }

    public void SetTimeScale(float scale)
    {
        timeScale.Value = Mathf.Clamp(scale, MinTimeScale, MaxTimeScale);
    }

    public void Pause()
    {
        paused.Value = true;
    }

    public void Resume()
    {
        paused.Value = false;
    }

    public void TogglePause()
    {
        paused.Value = !paused.Value;
    }
}
