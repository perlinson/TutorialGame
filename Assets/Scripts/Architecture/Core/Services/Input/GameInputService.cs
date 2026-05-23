using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// GameInputService：基于 Unity New Input System 的输入服务实现。
/// 兼容旧 KeyCode API，平滑过渡到新输入系统。
/// </summary>
public sealed class GameInputService : IGameInputService
{
    private Mouse mouse;
    private Keyboard keyboard;
    private bool enabled = true;

    public GameInputService()
    {
        mouse = Mouse.current;
        keyboard = Keyboard.current;
    }

    public bool Enabled
    {
        get => enabled;
        set => enabled = value;
    }

    public Vector2 MousePosition
    {
        get
        {
            if (!enabled || mouse == null)
            {
                return Vector2.zero;
            }

            return mouse.position.ReadValue();
        }
    }

    public bool IsKeyDown(KeyCode key)
    {
        if (!enabled || keyboard == null)
        {
            return false;
        }

        var keyToCheck = MapKeyCodeToKey(key);
        return keyToCheck != null && keyboard[keyToCheck.Value].wasPressedThisFrame;
    }

    public bool IsKeyUp(KeyCode key)
    {
        if (!enabled || keyboard == null)
        {
            return false;
        }

        var keyToCheck = MapKeyCodeToKey(key);
        return keyToCheck != null && keyboard[keyToCheck.Value].wasReleasedThisFrame;
    }

    public bool IsKeyHeld(KeyCode key)
    {
        if (!enabled || keyboard == null)
        {
            return false;
        }

        var keyToCheck = MapKeyCodeToKey(key);
        return keyToCheck != null && keyboard[keyToCheck.Value].isPressed;
    }

    public bool IsMouseLeftDown
    {
        get
        {
            if (!enabled || mouse == null)
            {
                return false;
            }

            return mouse.leftButton.wasPressedThisFrame;
        }
    }

    public bool IsMouseLeftUp
    {
        get
        {
            if (!enabled || mouse == null)
            {
                return false;
            }

            return mouse.leftButton.wasReleasedThisFrame;
        }
    }

    public bool IsMouseLeftHeld
    {
        get
        {
            if (!enabled || mouse == null)
            {
                return false;
            }

            return mouse.leftButton.isPressed;
        }
    }

    public bool IsMouseRightDown
    {
        get
        {
            if (!enabled || mouse == null)
            {
                return false;
            }

            return mouse.rightButton.wasPressedThisFrame;
        }
    }

    public bool IsMouseRightUp
    {
        get
        {
            if (!enabled || mouse == null)
            {
                return false;
            }

            return mouse.rightButton.wasReleasedThisFrame;
        }
    }

    public bool IsMouseRightHeld
    {
        get
        {
            if (!enabled || mouse == null)
            {
                return false;
            }

            return mouse.rightButton.isPressed;
        }
    }

    public float MouseScrollDelta
    {
        get
        {
            if (!enabled || mouse == null)
            {
                return 0f;
            }

            return mouse.scroll.y.ReadValue();
        }
    }

    public void Reset()
    {
        // New Input System 不需要显式重置状态
        // 此方法保留用于未来可能的扩展需求
    }

    private static Key? MapKeyCodeToKey(KeyCode keyCode)
    {
        return keyCode switch
        {
            KeyCode.A => Key.A,
            KeyCode.B => Key.B,
            KeyCode.C => Key.C,
            KeyCode.D => Key.D,
            KeyCode.E => Key.E,
            KeyCode.F => Key.F,
            KeyCode.G => Key.G,
            KeyCode.H => Key.H,
            KeyCode.I => Key.I,
            KeyCode.J => Key.J,
            KeyCode.K => Key.K,
            KeyCode.L => Key.L,
            KeyCode.M => Key.M,
            KeyCode.N => Key.N,
            KeyCode.O => Key.O,
            KeyCode.P => Key.P,
            KeyCode.Q => Key.Q,
            KeyCode.R => Key.R,
            KeyCode.S => Key.S,
            KeyCode.T => Key.T,
            KeyCode.U => Key.U,
            KeyCode.V => Key.V,
            KeyCode.W => Key.W,
            KeyCode.X => Key.X,
            KeyCode.Y => Key.Y,
            KeyCode.Z => Key.Z,
            KeyCode.Alpha0 => Key.Digit0,
            KeyCode.Alpha1 => Key.Digit1,
            KeyCode.Alpha2 => Key.Digit2,
            KeyCode.Alpha3 => Key.Digit3,
            KeyCode.Alpha4 => Key.Digit4,
            KeyCode.Alpha5 => Key.Digit5,
            KeyCode.Alpha6 => Key.Digit6,
            KeyCode.Alpha7 => Key.Digit7,
            KeyCode.Alpha8 => Key.Digit8,
            KeyCode.Alpha9 => Key.Digit9,
            KeyCode.Space => Key.Space,
            KeyCode.Return => Key.Enter,
            KeyCode.Escape => Key.Escape,
            KeyCode.Tab => Key.Tab,
            KeyCode.Backspace => Key.Backspace,
            KeyCode.Delete => Key.Delete,
            KeyCode.Insert => Key.Insert,
            KeyCode.Home => Key.Home,
            KeyCode.End => Key.End,
            KeyCode.PageUp => Key.PageUp,
            KeyCode.PageDown => Key.PageDown,
            KeyCode.UpArrow => Key.UpArrow,
            KeyCode.DownArrow => Key.DownArrow,
            KeyCode.LeftArrow => Key.LeftArrow,
            KeyCode.RightArrow => Key.RightArrow,
            KeyCode.LeftShift => Key.LeftShift,
            KeyCode.RightShift => Key.RightShift,
            KeyCode.LeftControl => Key.LeftCtrl,
            KeyCode.RightControl => Key.RightCtrl,
            KeyCode.LeftAlt => Key.LeftAlt,
            KeyCode.RightAlt => Key.RightAlt,
            _ => null
        };
    }
}
