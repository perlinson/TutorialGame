using QFramework;

public enum GameUiPanelId
{
    MainMenu,
    MainMenuSettings,
    MainMenuLoad,
    MainMenuCharacterCreate,
    WorldMap,
    Expedition
}

public sealed class MainMenuPanelData : UIPanelData
{
    public MainMenuPanelData(MainMenuConfig config)
    {
        Config = config;
    }

    public MainMenuConfig Config { get; }
}

public sealed class MainMenuSettingsPanelData : UIPanelData
{
    public MainMenuSettingsPanelData(MainMenuController owner)
    {
        Owner = owner;
    }

    public MainMenuController Owner { get; }
}

public sealed class MainMenuLoadPanelData : UIPanelData
{
    public MainMenuLoadPanelData(MainMenuController owner)
    {
        Owner = owner;
    }

    public MainMenuController Owner { get; }
}

public sealed class MainMenuCharacterCreatePanelData : UIPanelData
{
    public MainMenuCharacterCreatePanelData(MainMenuController owner)
    {
        Owner = owner;
    }

    public MainMenuController Owner { get; }
}

public sealed class WorldMapPanelData : UIPanelData
{
    public WorldMapPanelData(string gameplaySceneName, string mainSceneName)
    {
        GameplaySceneName = gameplaySceneName;
        MainSceneName = mainSceneName;
    }

    public string GameplaySceneName { get; }
    public string MainSceneName { get; }
}

public sealed class MainMenuSettingsSnapshot
{
    public float MusicVolume;
    public float SfxVolume;
    public float VoiceVolume;
    public bool IsFullscreen;
}

public sealed class MainMenuSlotSnapshot
{
    public int SlotIndex;
    public string Title;
    public string Detail;
    public string Footer;
    public bool Selected;
    public bool Occupied;
}

public sealed class MainMenuLoadSnapshot
{
    public MainMenuSlotSnapshot[] Slots;
    public string DetailTitle;
    public string DetailBody;
    public string ActionText;
    public bool CanLoad;
    public bool CanDelete;
}

public sealed class MainMenuCharacterSnapshot
{
    public MainMenuSlotSnapshot[] Slots;
    public MainMenuArchetype[] Archetypes;
    public int SelectedArchetypeIndex;
    public string SummaryTitle;
    public string SummaryBody;
    public string HeroName;
}
