using UnityEngine;

/// <summary>
/// U2 IGameInputService：基于 Unity New Input System 的输入服务接口。
/// 提供统一的输入访问，支持键盘、鼠标、触摸等输入设备。
/// </summary>
public interface IGameInputService
{
    /// <summary>是否启用输入处理。</summary>
    bool Enabled { get; set; }

    /// <summary>当前鼠标位置（屏幕坐标）。</summary>
    Vector2 MousePosition { get; }

    /// <summary>当前帧是否按下指定键。</summary>
    /// <param name="key">键盘键位。</param>
    bool IsKeyDown(KeyCode key);

    /// <summary>当前帧是否释放指定键。</summary>
    /// <param name="key">键盘键位。</param>
    bool IsKeyUp(KeyCode key);

    /// <summary>指定键是否持续按下。</summary>
    /// <param name="key">键盘键位。</param>
    bool IsKeyHeld(KeyCode key);

    /// <summary>当前帧是否按下鼠标左键。</summary>
    bool IsMouseLeftDown { get; }

    /// <summary>当前帧是否释放鼠标左键。</summary>
    bool IsMouseLeftUp { get; }

    /// <summary>鼠标左键是否持续按下。</summary>
    bool IsMouseLeftHeld { get; }

    /// <summary>当前帧是否按下鼠标右键。</summary>
    bool IsMouseRightDown { get; }

    /// <summary>当前帧是否释放鼠标右键。</summary>
    bool IsMouseRightUp { get; }

    /// <summary>鼠标右键是否持续按下。</summary>
    bool IsMouseRightHeld { get; }

    /// <summary>鼠标滚轮增量。</summary>
    float MouseScrollDelta { get; }

    /// <summary>重置输入状态（用于场景切换或暂停恢复）。</summary>
    void Reset();
}
