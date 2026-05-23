using UnityEngine;

/// <summary>
/// GameInput：静态桥接，封装 IGameInputService，方便统一调用输入服务。
/// </summary>
public static class GameInput
{
    private static IGameInputService service;

    internal static void SetService(IGameInputService inputService)
    {
        service = inputService;
    }

    public static bool Enabled
    {
        get => service?.Enabled ?? false;
        set
        {
            if (service != null)
            {
                service.Enabled = value;
            }
        }
    }

    public static Vector2 MousePosition => service?.MousePosition ?? Vector2.zero;

    public static bool IsKeyDown(KeyCode key) => service?.IsKeyDown(key) ?? false;

    public static bool IsKeyUp(KeyCode key) => service?.IsKeyUp(key) ?? false;

    public static bool IsKeyHeld(KeyCode key) => service?.IsKeyHeld(key) ?? false;

    public static bool IsMouseLeftDown => service?.IsMouseLeftDown ?? false;

    public static bool IsMouseLeftUp => service?.IsMouseLeftUp ?? false;

    public static bool IsMouseLeftHeld => service?.IsMouseLeftHeld ?? false;

    public static bool IsMouseRightDown => service?.IsMouseRightDown ?? false;

    public static bool IsMouseRightUp => service?.IsMouseRightUp ?? false;

    public static bool IsMouseRightHeld => service?.IsMouseRightHeld ?? false;

    public static float MouseScrollDelta => service?.MouseScrollDelta ?? 0f;

    public static void Reset() => service?.Reset();
}
