using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class RuntimeShutdownTracker
{
    public static bool IsShuttingDown { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnDomainReload()
    {
        IsShuttingDown = false;
        Application.quitting -= HandleApplicationQuitting;
        Application.quitting += HandleApplicationQuitting;
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void RegisterEditorHooks()
    {
        EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingPlayMode:
                IsShuttingDown = true;
                break;
            case PlayModeStateChange.EnteredPlayMode:
            case PlayModeStateChange.EnteredEditMode:
                IsShuttingDown = false;
                break;
        }
    }
#endif

    public static void MarkShuttingDown()
    {
        IsShuttingDown = true;
    }

    private static void HandleApplicationQuitting()
    {
        IsShuttingDown = true;
    }
}
