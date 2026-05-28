using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public sealed class AppRoot : MonoBehaviour
{
    private static AppRoot instance;

    private bool rootInitialized;
    private bool isInitialized;
    private GlobalAudioManager audioManager;
    private GlobalGameFlowManager gameFlowManager;
    private GlobalUiManager uiManager;
    private GlobalSaveManager saveManager;

    public static AppRoot EnsureCreated()
    {
        if (instance != null)
        {
            return instance;
        }

        instance = FindObjectOfType<AppRoot>();
        if (instance == null)
        {
            var root = new GameObject("AppRoot");
            root.AddComponent<GlobalAudioManager>();
            root.AddComponent<GlobalGameFlowManager>();
            root.AddComponent<GlobalUiManager>();
            root.AddComponent<GlobalSaveManager>();
            instance = root.AddComponent<AppRoot>();
        }

        instance.EnsureManagerComponents();
        instance.Initialize();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Initialize();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            RuntimeShutdownTracker.MarkShuttingDown();
            instance = null;
            CleanupPersistentFrameworkRoots();
        }
    }

    private void Initialize()
    {
        if (!rootInitialized)
        {
            DontDestroyOnLoad(gameObject);
            CultivationApp.EnsureInitialized();
            GameLog.Info("Resource backend: " + GameResource.BackendName);
            rootInitialized = true;
        }

        EnsureManagerComponents();
        audioManager = GetComponent<GlobalAudioManager>();
        gameFlowManager = GetComponent<GlobalGameFlowManager>();
        uiManager = GetComponent<GlobalUiManager>();
        saveManager = GetComponent<GlobalSaveManager>();

        if (audioManager != null)
        {
            audioManager.Initialize();
        }

        if (gameFlowManager != null)
        {
            gameFlowManager.Initialize();
        }

        if (uiManager != null)
        {
            uiManager.Initialize();
        }

        if (saveManager != null)
        {
            saveManager.Initialize();
        }

        isInitialized = audioManager != null
                        && gameFlowManager != null
                        && uiManager != null
                        && saveManager != null;
    }

    private void EnsureManagerComponents()
    {
        if (GetComponent<GlobalAudioManager>() == null)
        {
            gameObject.AddComponent<GlobalAudioManager>();
        }

        if (GetComponent<GlobalGameFlowManager>() == null)
        {
            gameObject.AddComponent<GlobalGameFlowManager>();
        }

        if (GetComponent<GlobalUiManager>() == null)
        {
            gameObject.AddComponent<GlobalUiManager>();
        }

        if (GetComponent<GlobalSaveManager>() == null)
        {
            gameObject.AddComponent<GlobalSaveManager>();
        }
    }

    public static GlobalGameFlowManager GetGameFlowManager()
    {
        if (instance == null)
        {
            return null;
        }

        if (instance.gameFlowManager == null)
        {
            instance.gameFlowManager = instance.GetComponent<GlobalGameFlowManager>();
        }

        return instance.gameFlowManager;
    }

    private static void CleanupPersistentFrameworkRoots()
    {
        DestroyPersistentRoot("UIRoot");
        DestroyPersistentRoot("QFramework");
        DestroyPersistentRoot("UnRegisterCurrentSceneUnloadedTrigger");
    }

    private static void DestroyPersistentRoot(string objectName)
    {
        var objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (var i = 0; i < objects.Length; i++)
        {
            var candidate = objects[i];
            if (candidate == null || candidate.name != objectName || candidate.transform.parent != null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(candidate);
            }
            else
            {
                DestroyImmediate(candidate);
            }
        }
    }
}

public static class SceneFlow
{
    public const string BootSceneName = "Boot";
    public const string MainMenuSceneName = "Main";
    public const string WorldMapSceneName = "WorldMap";
    public const string GameplaySceneName = "Game";

    private static string pendingSceneName = string.Empty;
    private static string runtimeTargetSceneName = string.Empty;

    public static void RequestScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        pendingSceneName = sceneName;
        if (SceneManager.GetActiveScene().name == BootSceneName)
        {
            var gameFlowManager = AppRoot.GetGameFlowManager();
            if (gameFlowManager != null)
            {
                gameFlowManager.RouteToScene(ConsumePendingSceneNameOrDefault(sceneName));
                return;
            }

            LoadPendingOrDefault(sceneName);
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(BootSceneName))
        {
            SceneManager.LoadScene(BootSceneName);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public static void BeginBootFlow(string defaultSceneName, float splashDuration)
    {
        var gameFlowManager = AppRoot.GetGameFlowManager();
        if (gameFlowManager != null)
        {
            gameFlowManager.BeginBootFlow(defaultSceneName, splashDuration);
            return;
        }

        LoadPendingOrDefault(defaultSceneName);
    }

    public static void SyncActiveSceneState(string sceneName)
    {
        var gameFlowManager = AppRoot.GetGameFlowManager();
        if (gameFlowManager != null)
        {
            gameFlowManager.SyncToScene(sceneName);
        }
    }

    public static void LoadPendingOrDefault(string defaultSceneName)
    {
        var targetSceneName = ConsumePendingSceneNameOrDefault(defaultSceneName);

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            GameLog.Error("SceneFlow missing target scene.");
            return;
        }

        LoadSceneDirect(targetSceneName);
    }

    internal static string ConsumePendingSceneNameOrDefault(string defaultSceneName)
    {
        var targetSceneName = string.IsNullOrWhiteSpace(pendingSceneName) ? defaultSceneName : pendingSceneName;
        pendingSceneName = string.Empty;
        return targetSceneName;
    }

    internal static void SetRuntimeTargetScene(string sceneName)
    {
        runtimeTargetSceneName = sceneName ?? string.Empty;
    }

    internal static string ConsumeRuntimeTargetSceneOrDefault(string defaultSceneName)
    {
        var targetSceneName = string.IsNullOrWhiteSpace(runtimeTargetSceneName) ? defaultSceneName : runtimeTargetSceneName;
        runtimeTargetSceneName = string.Empty;
        return targetSceneName;
    }

    internal static GlobalGameFlowState? ResolveFlowState(string sceneName)
    {
        switch (sceneName)
        {
            case MainMenuSceneName:
                return GlobalGameFlowState.MainMenu;
            case WorldMapSceneName:
                return GlobalGameFlowState.WorldMap;
            case GameplaySceneName:
                return GlobalGameFlowState.Gameplay;
            case BootSceneName:
                return GlobalGameFlowState.Splash;
            default:
                return null;
        }
    }

    internal static void LoadSceneDirect(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            GameLog.Error("SceneFlow missing target scene.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            GameLog.Error("Scene is not in Build Settings: " + sceneName);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}

public sealed class GlobalAudioManager : CultivationController
{
    private bool isInitialized;
    private IGameSettingsService settingsService;

    public void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        settingsService = this.GetUtility<IGameSettingsService>();
        _ = SoundSystem;
        settingsService.ApplyRuntimeSettings();
        SceneManager.sceneLoaded += HandleSceneLoaded;
        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (isInitialized)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (settingsService == null)
        {
            settingsService = this.GetUtility<IGameSettingsService>();
        }

        settingsService.ApplyRuntimeSettings();
    }
}

public sealed class GlobalUiManager : CultivationController
{
    private EventSystem eventSystem;
    private bool isInitialized;
    private IGameUiService uiService;
    private IGameLogService logService;

    public void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        EnsureUiRoot();
        EnsureEventSystem();
        SceneManager.sceneLoaded += HandleSceneLoaded;
        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (isInitialized)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureUiRoot();
        EnsureEventSystem();
        RemoveDuplicateEventSystems();
    }

    private void EnsureUiRoot()
    {
        if (uiService == null)
        {
            uiService = this.GetUtility<IGameUiService>();
        }

        uiService.Initialize();

        var uiRoot = UIKit.Root;
        if (uiRoot != null)
        {
            Object.DontDestroyOnLoad(uiRoot.gameObject);
        }
    }

    private void EnsureEventSystem()
    {
        if (eventSystem == null)
        {
            eventSystem = FindObjectOfType<EventSystem>();
        }

        if (eventSystem == null)
        {
            if (logService == null)
            {
                logService = this.GetUtility<IGameLogService>();
            }

            logService.Error("GlobalUiManager could not find EventSystem. Boot scene must contain a static EventSystem.");
            return;
        }

        DontDestroyOnLoad(eventSystem.transform.root.gameObject);
    }

    private void RemoveDuplicateEventSystems()
    {
        var allEventSystems = FindObjectsOfType<EventSystem>();
        for (var i = 0; i < allEventSystems.Length; i++)
        {
            if (allEventSystems[i] != null && allEventSystems[i] != eventSystem)
            {
                Destroy(allEventSystems[i].gameObject);
            }
        }
    }
}

public sealed class GlobalSaveManager : MonoBehaviour
{
    private bool isInitialized;

    public void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        isInitialized = true;
    }

    public bool TryLoadCurrentArchive(out int slotIndex, out CultivationSaveData saveData)
    {
        return CultivationLocalSaveStore.TryGetCurrentSave(out slotIndex, out saveData);
    }

    public void SaveCurrentArchive(int slotIndex, CultivationSaveData saveData)
    {
        CultivationLocalSaveStore.SaveCurrent(slotIndex, saveData);
    }

    public void SaveExpeditionRuntime(PersistentExpeditionRuntimeSnapshot snapshot)
    {
        CultivationLocalSaveStore.SaveExpeditionRuntime(snapshot);
    }

    public void ClearExpeditionRuntime()
    {
        CultivationLocalSaveStore.ClearExpeditionRuntime();
    }
}
