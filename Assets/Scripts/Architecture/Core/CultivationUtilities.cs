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
    UIPanel OpenPanel(GameUiPanelId panelId, IUIData uiData = null);
    MainMenuController OpenMainMenu(MainMenuConfig config);
    MainMenuSettingsPanel OpenMainMenuSettings(MainMenuController owner);
    MainMenuLoadPanel OpenMainMenuLoad(MainMenuController owner);
    MainMenuCharacterCreatePanel OpenMainMenuCharacterCreate(MainMenuController owner);
    WorldMapController OpenWorldMap(string gameplaySceneName, string mainSceneName);
    ExpeditionView OpenExpedition();
    CultivationMessagePopupPanel ShowMessagePopup(string message, string title = "", float duration = 2.4f, CultivationMessagePopupStyle style = CultivationMessagePopupStyle.Info);
    void ClosePanel(GameUiPanelId panelId);
    void CloseAllPanels();
    void DestroyPanel(GameUiPanelId panelId);
    void DestroyAllPanels();
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
    private static readonly GameUiPanelId[] RegisteredPanels = (GameUiPanelId[])System.Enum.GetValues(typeof(GameUiPanelId));
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
        HideTransientPanels();
        CloseWorldMapPanels();
        ClosePanel(GameUiPanelId.Expedition);
        ClosePanel(GameUiPanelId.MainMenuSettings);
        ClosePanel(GameUiPanelId.MainMenuLoad);
        ClosePanel(GameUiPanelId.MainMenuCharacterCreate);
        return OpenPanel(GameUiPanelId.MainMenu, new MainMenuPanelData(config)) as MainMenuController;
    }

    public MainMenuSettingsPanel OpenMainMenuSettings(MainMenuController owner)
    {
        Initialize();
        HideTransientPanels();
        ClosePanel(GameUiPanelId.MainMenuLoad);
        ClosePanel(GameUiPanelId.MainMenuCharacterCreate);
        return OpenPanel(GameUiPanelId.MainMenuSettings, new MainMenuSettingsPanelData(owner)) as MainMenuSettingsPanel;
    }

    public MainMenuLoadPanel OpenMainMenuLoad(MainMenuController owner)
    {
        Initialize();
        HideTransientPanels();
        ClosePanel(GameUiPanelId.MainMenuSettings);
        ClosePanel(GameUiPanelId.MainMenuCharacterCreate);
        return OpenPanel(GameUiPanelId.MainMenuLoad, new MainMenuLoadPanelData(owner)) as MainMenuLoadPanel;
    }

    public MainMenuCharacterCreatePanel OpenMainMenuCharacterCreate(MainMenuController owner)
    {
        Initialize();
        HideTransientPanels();
        ClosePanel(GameUiPanelId.MainMenuSettings);
        ClosePanel(GameUiPanelId.MainMenuLoad);
        return OpenPanel(GameUiPanelId.MainMenuCharacterCreate, new MainMenuCharacterCreatePanelData(owner)) as MainMenuCharacterCreatePanel;
    }

    public WorldMapController OpenWorldMap(string gameplaySceneName, string mainSceneName)
    {
        Initialize();
        HideTransientPanels();
        CloseWorldMapPanels();
        ClosePanel(GameUiPanelId.Expedition);
        ClosePanel(GameUiPanelId.MainMenuSettings);
        ClosePanel(GameUiPanelId.MainMenuLoad);
        ClosePanel(GameUiPanelId.MainMenuCharacterCreate);
        ClosePanel(GameUiPanelId.MainMenu);
        return OpenPanel(GameUiPanelId.WorldMap, new WorldMapPanelData(gameplaySceneName, mainSceneName)) as WorldMapController;
    }

    public ExpeditionView OpenExpedition()
    {
        Initialize();
        HideTransientPanels();
        CloseWorldMapPanels();
        ClosePanel(GameUiPanelId.MainMenuSettings);
        ClosePanel(GameUiPanelId.MainMenuLoad);
        ClosePanel(GameUiPanelId.MainMenuCharacterCreate);
        ClosePanel(GameUiPanelId.MainMenu);
        return OpenPanel(GameUiPanelId.Expedition) as ExpeditionView;
    }

    public CultivationMessagePopupPanel ShowMessagePopup(string message, string title = "", float duration = 2.4f, CultivationMessagePopupStyle style = CultivationMessagePopupStyle.Info)
    {
        Initialize();
        var panel = UIKit.GetPanel<CultivationMessagePopupPanel>();
        if (panel != null)
        {
            UIKit.ShowPanel(GameUiPanelRegistry.Get(GameUiPanelId.MessagePopup).ResourcePath);
            panel.Present(title, message, duration, style);
            return panel;
        }

        return OpenPanel(GameUiPanelId.MessagePopup, new CultivationMessagePopupPanelData(title, message, duration, style)) as CultivationMessagePopupPanel;
    }

    public UIPanel OpenPanel(GameUiPanelId panelId, IUIData uiData = null)
    {
        Initialize();
        ClosePanelsAtSameLevel(panelId);
        switch (panelId)
        {
            case GameUiPanelId.MainMenu:
                return OpenMappedPanel<MainMenuController>(panelId, uiData);
            case GameUiPanelId.MainMenuSettings:
                return OpenMappedPanel<MainMenuSettingsPanel>(panelId, uiData);
            case GameUiPanelId.MainMenuLoad:
                return OpenMappedPanel<MainMenuLoadPanel>(panelId, uiData);
            case GameUiPanelId.MainMenuCharacterCreate:
                return OpenMappedPanel<MainMenuCharacterCreatePanel>(panelId, uiData);
            case GameUiPanelId.MessagePopup:
                return OpenMappedPanel<CultivationMessagePopupPanel>(panelId, uiData);
            case GameUiPanelId.Tooltip:
                return OpenMappedPanel<CultivationTooltipPanel>(panelId, uiData);
            case GameUiPanelId.WorldMap:
                return OpenMappedPanel<WorldMapController>(panelId, uiData);
            case GameUiPanelId.GameHub:
                return OpenMappedPanel<GameHubPanel>(panelId, uiData);
            case GameUiPanelId.WorldMapRegion:
                ClosePanel(GameUiPanelId.WorldMapInventory);
                ClosePanel(GameUiPanelId.WorldMapWorkshop);
                return OpenMappedPanel<WorldMapRegionPanel>(panelId, uiData);
            case GameUiPanelId.WorldMapSettlement:
                ClosePanel(GameUiPanelId.WorldMapInventory);
                ClosePanel(GameUiPanelId.WorldMapWorkshop);
                return OpenMappedPanel<WorldMapSettlementPanel>(panelId, uiData);
            case GameUiPanelId.PlayerCompendium:
                ClosePanel(GameUiPanelId.WorldMapInventory);
                ClosePanel(GameUiPanelId.WorldMapWorkshop);
                return OpenMappedPanel<PlayerCompendiumPanel>(panelId, uiData);
            case GameUiPanelId.WorldMapInventory:
                ClosePanel(GameUiPanelId.WorldMapWorkshop);
                return OpenMappedPanel<WorldMapInventoryPanel>(panelId, uiData);
            case GameUiPanelId.WorldMapWorkshop:
                ClosePanel(GameUiPanelId.WorldMapInventory);
                return OpenMappedPanel<WorldMapWorkshopPanel>(panelId, uiData);
            case GameUiPanelId.WorldMapSectResidence:
                ClosePanel(GameUiPanelId.WorldMapInventory);
                ClosePanel(GameUiPanelId.WorldMapWorkshop);
                return OpenMappedPanel<WorldMapSectResidencePanel>(panelId, uiData);
            case GameUiPanelId.WorldMapNpcDialogue:
                ClosePanel(GameUiPanelId.WorldMapInventory);
                ClosePanel(GameUiPanelId.WorldMapWorkshop);
                return OpenMappedPanel<WorldMapNpcDialoguePanel>(panelId, uiData);
            case GameUiPanelId.Expedition:
                return OpenMappedPanel<ExpeditionView>(panelId, uiData);
            default:
                return null;
        }
    }

    public void ClosePanel(GameUiPanelId panelId)
    {
        if (panelId == GameUiPanelId.WorldMap)
        {
            ClosePanel(GameUiPanelId.GameHub);
            ClosePanel(GameUiPanelId.WorldMapRegion);
            ClosePanel(GameUiPanelId.WorldMapSettlement);
            ClosePanel(GameUiPanelId.PlayerCompendium);
            ClosePanel(GameUiPanelId.WorldMapInventory);
            ClosePanel(GameUiPanelId.WorldMapWorkshop);
            ClosePanel(GameUiPanelId.WorldMapSectResidence);
            ClosePanel(GameUiPanelId.WorldMapNpcDialogue);
        }

        UIKit.HidePanel(GameUiPanelRegistry.Get(panelId).ResourcePath);
    }

    public void CloseAllPanels()
    {
        ClosePanel(GameUiPanelId.MainMenu);
        ClosePanel(GameUiPanelId.MainMenuSettings);
        ClosePanel(GameUiPanelId.MainMenuLoad);
        ClosePanel(GameUiPanelId.MainMenuCharacterCreate);
        ClosePanel(GameUiPanelId.MessagePopup);
        ClosePanel(GameUiPanelId.Tooltip);
        ClosePanel(GameUiPanelId.WorldMap);
        ClosePanel(GameUiPanelId.GameHub);
        ClosePanel(GameUiPanelId.WorldMapRegion);
        ClosePanel(GameUiPanelId.WorldMapSettlement);
        ClosePanel(GameUiPanelId.PlayerCompendium);
        ClosePanel(GameUiPanelId.WorldMapInventory);
        ClosePanel(GameUiPanelId.WorldMapWorkshop);
        ClosePanel(GameUiPanelId.WorldMapSectResidence);
        ClosePanel(GameUiPanelId.WorldMapNpcDialogue);
        ClosePanel(GameUiPanelId.Expedition);
    }

    public void DestroyPanel(GameUiPanelId panelId)
    {
        if (panelId == GameUiPanelId.WorldMap)
        {
            DestroyPanel(GameUiPanelId.GameHub);
            DestroyPanel(GameUiPanelId.WorldMapRegion);
            DestroyPanel(GameUiPanelId.WorldMapSettlement);
            DestroyPanel(GameUiPanelId.PlayerCompendium);
            DestroyPanel(GameUiPanelId.WorldMapInventory);
            DestroyPanel(GameUiPanelId.WorldMapWorkshop);
            DestroyPanel(GameUiPanelId.WorldMapSectResidence);
            DestroyPanel(GameUiPanelId.WorldMapNpcDialogue);
        }

        UIKit.ClosePanel(GameUiPanelRegistry.Get(panelId).ResourcePath);
    }

    public void DestroyAllPanels()
    {
        DestroyPanel(GameUiPanelId.MainMenu);
        DestroyPanel(GameUiPanelId.MainMenuSettings);
        DestroyPanel(GameUiPanelId.MainMenuLoad);
        DestroyPanel(GameUiPanelId.MainMenuCharacterCreate);
        DestroyPanel(GameUiPanelId.MessagePopup);
        DestroyPanel(GameUiPanelId.Tooltip);
        DestroyPanel(GameUiPanelId.WorldMap);
        DestroyPanel(GameUiPanelId.GameHub);
        DestroyPanel(GameUiPanelId.WorldMapRegion);
        DestroyPanel(GameUiPanelId.WorldMapSettlement);
        DestroyPanel(GameUiPanelId.PlayerCompendium);
        DestroyPanel(GameUiPanelId.WorldMapInventory);
        DestroyPanel(GameUiPanelId.WorldMapWorkshop);
        DestroyPanel(GameUiPanelId.WorldMapSectResidence);
        DestroyPanel(GameUiPanelId.WorldMapNpcDialogue);
        DestroyPanel(GameUiPanelId.Expedition);
    }

    public void HidePanel(GameUiPanelId panelId)
    {
        UIKit.HidePanel(GameUiPanelRegistry.Get(panelId).ResourcePath);
    }

    public void ShowPanel(GameUiPanelId panelId)
    {
        UIKit.ShowPanel(GameUiPanelRegistry.Get(panelId).ResourcePath);
    }

    private static T OpenMappedPanel<T>(GameUiPanelId panelId, IUIData uiData = null) where T : UIPanel
    {
        var definition = GameUiPanelRegistry.Get(panelId);
        var prefab = Resources.Load<GameObject>(definition.ResourcePath);
        if (prefab == null)
        {
            Debug.LogError("GameUiService failed to open `" + panelId + "` because prefab was not found at Resources path `" + definition.ResourcePath + "`.");
            return null;
        }

        return UIKit.OpenPanel<T>(PanelOpenType.Single, definition.Level, uiData, prefabName: definition.ResourcePath);
    }

    private void ClosePanelsAtSameLevel(GameUiPanelId panelId)
    {
        var targetDefinition = GameUiPanelRegistry.Get(panelId);
        if (!targetDefinition.ExclusiveWithinLevel)
        {
            return;
        }

        var level = targetDefinition.Level;
        for (var i = 0; i < RegisteredPanels.Length; i++)
        {
            var other = RegisteredPanels[i];
            if (other == panelId)
            {
                continue;
            }

            var otherDefinition = GameUiPanelRegistry.Get(other);
            if (!otherDefinition.ExclusiveWithinLevel)
            {
                continue;
            }

            if (otherDefinition.Level == level)
            {
                UIKit.HidePanel(otherDefinition.ResourcePath);
            }
        }
    }

    private void CloseWorldMapPanels()
    {
        ClosePanel(GameUiPanelId.GameHub);
        ClosePanel(GameUiPanelId.WorldMapRegion);
        ClosePanel(GameUiPanelId.WorldMapSettlement);
        ClosePanel(GameUiPanelId.PlayerCompendium);
        ClosePanel(GameUiPanelId.WorldMapInventory);
        ClosePanel(GameUiPanelId.WorldMapWorkshop);
        ClosePanel(GameUiPanelId.WorldMapSectResidence);
        ClosePanel(GameUiPanelId.WorldMapNpcDialogue);
        ClosePanel(GameUiPanelId.WorldMap);
    }

    private void HideTransientPanels()
    {
        ClosePanel(GameUiPanelId.MessagePopup);
        ClosePanel(GameUiPanelId.Tooltip);
    }
}
