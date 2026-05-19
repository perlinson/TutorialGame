using QFramework;
using UnityEngine;

public interface IGameResourceService : IUtility
{
    string BackendName { get; }
    T Load<T>(string path) where T : Object;
    GameObject InstantiatePrefab(string path, Transform parent = null);
    void ClearCache();
}

public interface IGameSettingsService : IUtility
{
    IReadonlyBindableProperty<float> MusicVolume { get; }
    IReadonlyBindableProperty<float> SfxVolume { get; }
    IReadonlyBindableProperty<float> VoiceVolume { get; }
    IReadonlyBindableProperty<bool> Fullscreen { get; }
    void RefreshFromStorage();
    void SetMusicVolume(float value);
    void SetSfxVolume(float value);
    void SetVoiceVolume(float value);
    void SetFullscreen(bool fullscreen);
    void Reset();
    void ApplyRuntimeSettings();
}

public interface IGameAudioService : IUtility
{
    void Initialize();
    void PlayMusic(AudioClip clip, bool loop = true, float volumeScale = 1f);
    void PlayMusic(string resourcePath, bool loop = true, float volumeScale = 1f);
    void StopMusic();
    void PlayVoice(AudioClip clip, bool loop = false, float volumeScale = 1f, bool duckMusic = true);
    void PlayVoice(string resourcePath, bool loop = false, float volumeScale = 1f, bool duckMusic = true);
    void StopVoice();
    void PlaySfx(AudioClip clip, float volumeScale = 1f);
    void PlaySfx(string resourcePath, float volumeScale = 1f);
    void SetMusicDuck(string reason, bool enabled, float duckDb = 8f);
}

public interface IGameLogService : IUtility
{
    void Info(string message);
    void Warning(string message);
    void Error(string message);
}

public interface IGameUiService : IUtility
{
    void Initialize();
    MainMenuController OpenMainMenu(MainMenuConfig config);
    MainMenuSettingsPanel OpenMainMenuSettings(MainMenuController owner);
    MainMenuLoadPanel OpenMainMenuLoad(MainMenuController owner);
    MainMenuCharacterCreatePanel OpenMainMenuCharacterCreate(MainMenuController owner);
    WorldMapController OpenWorldMap(string gameplaySceneName, string mainSceneName);
    ExpeditionView OpenExpedition();
    void ClosePanel(GameUiPanelId panelId);
    void CloseAllPanels();
    void HidePanel(GameUiPanelId panelId);
    void ShowPanel(GameUiPanelId panelId);
}

public sealed class GameResourceService : IGameResourceService
{
    private ResLoader loader;
    private bool useResKitBackend;

    public GameResourceService()
    {
        if (!Application.isPlaying)
        {
            useResKitBackend = false;
            loader = null;
            return;
        }

        try
        {
            ResKit.Init();
            loader = ResLoader.Allocate();
            useResKitBackend = loader != null;
        }
        catch (System.Exception ex)
        {
            useResKitBackend = false;
            loader = null;
            Debug.LogWarning("GameResourceService fell back to Unity Resources because ResKit initialization failed: " + ex.Message);
        }
    }

    public string BackendName => useResKitBackend ? "ResKit" : "Resources";

    public T Load<T>(string path) where T : Object
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (useResKitBackend && loader != null)
        {
            try
            {
                return loader.LoadSync<T>(NormalizePath(path));
            }
            catch (System.Exception ex)
            {
                useResKitBackend = false;
                loader = null;
                Debug.LogWarning("GameResourceService fell back to Unity Resources because ResKit load failed for `" + path + "`: " + ex.Message);
            }
        }

        return Resources.Load<T>(NormalizeResourcesPath(path));
    }

    public GameObject InstantiatePrefab(string path, Transform parent = null)
    {
        var prefab = Load<GameObject>(path);
        if (prefab == null)
        {
            return null;
        }

        return parent != null ? Object.Instantiate(prefab, parent, false) : Object.Instantiate(prefab);
    }

    public void ClearCache()
    {
        if (!useResKitBackend || loader == null)
        {
            return;
        }

        loader.ReleaseAllRes();
        loader.UnloadAllInstantiateRes(true);
        loader.Recycle2Cache();
        loader = ResLoader.Allocate();
    }

    private static string NormalizePath(string path)
    {
        return path.Contains("://") ? path : "resources://" + path;
    }

    private static string NormalizeResourcesPath(string path)
    {
        const string resourcesPrefix = "resources://";
        return path.StartsWith(resourcesPrefix) ? path.Substring(resourcesPrefix.Length) : path;
    }
}

public sealed class GameSettingsService : IGameSettingsService
{
    private readonly BindableProperty<float> musicVolume = new BindableProperty<float>(0.8f);
    private readonly BindableProperty<float> sfxVolume = new BindableProperty<float>(0.8f);
    private readonly BindableProperty<float> voiceVolume = new BindableProperty<float>(0.8f);
    private readonly BindableProperty<bool> fullscreen = new BindableProperty<bool>(true);

    public IReadonlyBindableProperty<float> MusicVolume => musicVolume;
    public IReadonlyBindableProperty<float> SfxVolume => sfxVolume;
    public IReadonlyBindableProperty<float> VoiceVolume => voiceVolume;
    public IReadonlyBindableProperty<bool> Fullscreen => fullscreen;

    public GameSettingsService()
    {
        RefreshFromStorage();
    }

    public void RefreshFromStorage()
    {
        musicVolume.SetValueWithoutEvent(Mathf.Clamp01(MainMenuSaveStore.LoadMusicVolume()));
        sfxVolume.SetValueWithoutEvent(Mathf.Clamp01(MainMenuSaveStore.LoadSfxVolume()));
        voiceVolume.SetValueWithoutEvent(Mathf.Clamp01(MainMenuSaveStore.LoadVoiceVolume()));
        fullscreen.SetValueWithoutEvent(MainMenuSaveStore.LoadFullscreen());
        ApplyRuntimeSettings();
    }

    public void SetMusicVolume(float value)
    {
        var normalized = Mathf.Clamp01(value);
        MainMenuSaveStore.SaveMusicVolume(normalized);
        musicVolume.Value = normalized;
        ApplyRuntimeSettings();
    }

    public void SetSfxVolume(float value)
    {
        var normalized = Mathf.Clamp01(value);
        MainMenuSaveStore.SaveSfxVolume(normalized);
        sfxVolume.Value = normalized;
        ApplyRuntimeSettings();
    }

    public void SetVoiceVolume(float value)
    {
        var normalized = Mathf.Clamp01(value);
        MainMenuSaveStore.SaveVoiceVolume(normalized);
        voiceVolume.Value = normalized;
        ApplyRuntimeSettings();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        MainMenuSaveStore.SaveFullscreen(isFullscreen);
        fullscreen.Value = isFullscreen;
        ApplyRuntimeSettings();
    }

    public void Reset()
    {
        SetMusicVolume(0.8f);
        SetSfxVolume(0.8f);
        SetVoiceVolume(0.8f);
        SetFullscreen(true);
    }

    public void ApplyRuntimeSettings()
    {
        var normalizedMusicVolume = Mathf.Clamp01(musicVolume.Value);
        var normalizedSfxVolume = Mathf.Clamp01(sfxVolume.Value);
        var normalizedVoiceVolume = Mathf.Clamp01(voiceVolume.Value);

        AudioKit.Settings.MusicVolume.Value = normalizedMusicVolume;
        AudioKit.Settings.SoundVolume.Value = normalizedSfxVolume;
        AudioKit.Settings.VoiceVolume.Value = normalizedVoiceVolume;
        AudioKit.Settings.IsMusicOn.Value = normalizedMusicVolume > 0.001f;
        AudioKit.Settings.IsSoundOn.Value = normalizedSfxVolume > 0.001f;
        AudioKit.Settings.IsVoiceOn.Value = normalizedVoiceVolume > 0.001f;
        CultivationAudioMixerRouter.ApplyUserVolumes(normalizedMusicVolume, normalizedSfxVolume, normalizedVoiceVolume);
        AudioListener.volume = 1f;
        Screen.fullScreen = fullscreen.Value;
    }
}

public sealed class GameAudioService : IGameAudioService
{
    private const string VoiceDuckReason = "VoicePlayback";

    private bool isInitialized;

    public void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        AudioManager.Instance.Init();
        AudioManager.Instance.CheckAudioListener();
        isInitialized = true;
    }

    public void PlayMusic(AudioClip clip, bool loop = true, float volumeScale = 1f)
    {
        if (clip == null)
        {
            return;
        }

        Initialize();
        AudioKit.PlayMusic(clip, loop, volume: Mathf.Clamp01(volumeScale));
    }

    public void PlayMusic(string resourcePath, bool loop = true, float volumeScale = 1f)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return;
        }

        Initialize();
        AudioKit.PlayMusic(NormalizePath(resourcePath), loop, volume: Mathf.Clamp01(volumeScale));
    }

    public void StopMusic()
    {
        Initialize();
        AudioKit.StopMusic();
    }

    public void PlayVoice(AudioClip clip, bool loop = false, float volumeScale = 1f, bool duckMusic = true)
    {
        if (clip == null)
        {
            return;
        }

        Initialize();
        CultivationAudioMixerRouter.SetMusicDuck(VoiceDuckReason, duckMusic);
        AudioKit.PlayVoice(
            clip,
            loop,
            volumeScale: Mathf.Clamp01(volumeScale),
            onEndedCallback: () => CultivationAudioMixerRouter.SetMusicDuck(VoiceDuckReason, false));
    }

    public void PlayVoice(string resourcePath, bool loop = false, float volumeScale = 1f, bool duckMusic = true)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return;
        }

        Initialize();
        CultivationAudioMixerRouter.SetMusicDuck(VoiceDuckReason, duckMusic);
        AudioKit.PlayVoice(
            NormalizePath(resourcePath),
            loop,
            onEndedCallback: () => CultivationAudioMixerRouter.SetMusicDuck(VoiceDuckReason, false));

        if (AudioKit.VoicePlayer != null)
        {
            AudioKit.VoicePlayer.VolumeScale(Mathf.Clamp01(volumeScale));
        }
    }

    public void StopVoice()
    {
        Initialize();
        CultivationAudioMixerRouter.SetMusicDuck(VoiceDuckReason, false);
        AudioKit.StopVoice();
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null)
        {
            return;
        }

        Initialize();
        AudioKit.PlaySound(clip, volume: Mathf.Clamp01(volumeScale));
    }

    public void PlaySfx(string resourcePath, float volumeScale = 1f)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return;
        }

        Initialize();
        AudioKit.PlaySound(NormalizePath(resourcePath), volume: Mathf.Clamp01(volumeScale));
    }

    public void SetMusicDuck(string reason, bool enabled, float duckDb = 8f)
    {
        Initialize();
        CultivationAudioMixerRouter.SetMusicDuck(reason, enabled, duckDb);
    }

    private static string NormalizePath(string path)
    {
        return path.Contains("://") ? path : "resources://" + path;
    }
}

public sealed class GameLogService : IGameLogService
{
    public void Info(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            Debug.Log(message);
        }
    }

    public void Warning(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            Debug.LogWarning(message);
        }
    }

    public void Error(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            Debug.LogError(message);
        }
    }
}

public sealed class GameUiService : IGameUiService
{
    private const string MainMenuPanelPath = "UI/MainMenu/MainMenuRoot";
    private const string MainMenuSettingsPanelPath = "UI/MainMenu/MainMenuSettingsPanel";
    private const string MainMenuLoadPanelPath = "UI/MainMenu/MainMenuLoadPanel";
    private const string MainMenuCharacterCreatePanelPath = "UI/MainMenu/MainMenuCharacterCreatePanel";
    private const string WorldMapPanelPath = "UI/WorldMap/WorldMapRoot";
    private const string ExpeditionPanelPath = "UI/Game/ExpeditionRoot";

    private bool isInitialized;

    public void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        UIKit.Config.PanelLoaderPool = new DefaultPanelLoaderPool();

        var root = UIKit.Root;
        if (root != null)
        {
            root.ScreenSpaceOverlayRenderMode();
            root.SetResolution(1920, 1080, 0.5f);
            Object.DontDestroyOnLoad(root.gameObject);
        }

        isInitialized = true;
    }

    public MainMenuController OpenMainMenu(MainMenuConfig config)
    {
        Initialize();
        ClosePanel(GameUiPanelId.Expedition);
        ClosePanel(GameUiPanelId.WorldMap);
        ClosePanel(GameUiPanelId.MainMenuSettings);
        ClosePanel(GameUiPanelId.MainMenuLoad);
        ClosePanel(GameUiPanelId.MainMenuCharacterCreate);
        return UIKit.OpenPanel<MainMenuController>(
            PanelOpenType.Single,
            UILevel.CanvasPanel,
            new MainMenuPanelData(config),
            prefabName: MainMenuPanelPath);
    }

    public MainMenuSettingsPanel OpenMainMenuSettings(MainMenuController owner)
    {
        Initialize();
        ClosePanel(GameUiPanelId.MainMenuLoad);
        ClosePanel(GameUiPanelId.MainMenuCharacterCreate);
        return UIKit.OpenPanel<MainMenuSettingsPanel>(
            PanelOpenType.Single,
            UILevel.PopUI,
            new MainMenuSettingsPanelData(owner),
            prefabName: MainMenuSettingsPanelPath);
    }

    public MainMenuLoadPanel OpenMainMenuLoad(MainMenuController owner)
    {
        Initialize();
        ClosePanel(GameUiPanelId.MainMenuSettings);
        ClosePanel(GameUiPanelId.MainMenuCharacterCreate);
        return UIKit.OpenPanel<MainMenuLoadPanel>(
            PanelOpenType.Single,
            UILevel.PopUI,
            new MainMenuLoadPanelData(owner),
            prefabName: MainMenuLoadPanelPath);
    }

    public MainMenuCharacterCreatePanel OpenMainMenuCharacterCreate(MainMenuController owner)
    {
        Initialize();
        ClosePanel(GameUiPanelId.MainMenuSettings);
        ClosePanel(GameUiPanelId.MainMenuLoad);
        return UIKit.OpenPanel<MainMenuCharacterCreatePanel>(
            PanelOpenType.Single,
            UILevel.PopUI,
            new MainMenuCharacterCreatePanelData(owner),
            prefabName: MainMenuCharacterCreatePanelPath);
    }

    public WorldMapController OpenWorldMap(string gameplaySceneName, string mainSceneName)
    {
        Initialize();
        ClosePanel(GameUiPanelId.Expedition);
        ClosePanel(GameUiPanelId.MainMenuSettings);
        ClosePanel(GameUiPanelId.MainMenuLoad);
        ClosePanel(GameUiPanelId.MainMenuCharacterCreate);
        ClosePanel(GameUiPanelId.MainMenu);
        return UIKit.OpenPanel<WorldMapController>(
            PanelOpenType.Single,
            UILevel.CanvasPanel,
            new WorldMapPanelData(gameplaySceneName, mainSceneName),
            prefabName: WorldMapPanelPath);
    }

    public ExpeditionView OpenExpedition()
    {
        Initialize();
        ClosePanel(GameUiPanelId.MainMenuSettings);
        ClosePanel(GameUiPanelId.MainMenuLoad);
        ClosePanel(GameUiPanelId.MainMenuCharacterCreate);
        ClosePanel(GameUiPanelId.MainMenu);
        ClosePanel(GameUiPanelId.WorldMap);
        return UIKit.OpenPanel<ExpeditionView>(
            PanelOpenType.Single,
            UILevel.CanvasPanel,
            prefabName: ExpeditionPanelPath);
    }

    public void ClosePanel(GameUiPanelId panelId)
    {
        switch (panelId)
        {
            case GameUiPanelId.MainMenu:
                UIKit.ClosePanel<MainMenuController>();
                break;
            case GameUiPanelId.MainMenuSettings:
                UIKit.ClosePanel<MainMenuSettingsPanel>();
                break;
            case GameUiPanelId.MainMenuLoad:
                UIKit.ClosePanel<MainMenuLoadPanel>();
                break;
            case GameUiPanelId.MainMenuCharacterCreate:
                UIKit.ClosePanel<MainMenuCharacterCreatePanel>();
                break;
            case GameUiPanelId.WorldMap:
                UIKit.ClosePanel<WorldMapController>();
                break;
            case GameUiPanelId.Expedition:
                UIKit.ClosePanel<ExpeditionView>();
                break;
        }
    }

    public void CloseAllPanels()
    {
        ClosePanel(GameUiPanelId.MainMenu);
        ClosePanel(GameUiPanelId.MainMenuSettings);
        ClosePanel(GameUiPanelId.MainMenuLoad);
        ClosePanel(GameUiPanelId.MainMenuCharacterCreate);
        ClosePanel(GameUiPanelId.WorldMap);
        ClosePanel(GameUiPanelId.Expedition);
    }

    public void HidePanel(GameUiPanelId panelId)
    {
        switch (panelId)
        {
            case GameUiPanelId.MainMenu:
                UIKit.HidePanel<MainMenuController>();
                break;
            case GameUiPanelId.MainMenuSettings:
                UIKit.HidePanel<MainMenuSettingsPanel>();
                break;
            case GameUiPanelId.MainMenuLoad:
                UIKit.HidePanel<MainMenuLoadPanel>();
                break;
            case GameUiPanelId.MainMenuCharacterCreate:
                UIKit.HidePanel<MainMenuCharacterCreatePanel>();
                break;
            case GameUiPanelId.WorldMap:
                UIKit.HidePanel<WorldMapController>();
                break;
            case GameUiPanelId.Expedition:
                UIKit.HidePanel<ExpeditionView>();
                break;
        }
    }

    public void ShowPanel(GameUiPanelId panelId)
    {
        switch (panelId)
        {
            case GameUiPanelId.MainMenu:
                UIKit.ShowPanel<MainMenuController>();
                break;
            case GameUiPanelId.MainMenuSettings:
                UIKit.ShowPanel<MainMenuSettingsPanel>();
                break;
            case GameUiPanelId.MainMenuLoad:
                UIKit.ShowPanel<MainMenuLoadPanel>();
                break;
            case GameUiPanelId.MainMenuCharacterCreate:
                UIKit.ShowPanel<MainMenuCharacterCreatePanel>();
                break;
            case GameUiPanelId.WorldMap:
                UIKit.ShowPanel<WorldMapController>();
                break;
            case GameUiPanelId.Expedition:
                UIKit.ShowPanel<ExpeditionView>();
                break;
        }
    }
}
