using System;
using QFramework;

public enum GameUiPanelId
{
    MainMenu,
    MainMenuSettings,
    MainMenuLoad,
    MainMenuCharacterCreate,
    MessagePopup,
    Tooltip,
    WorldMap,
    GameHub,
    WorldMapRegion,
    WorldMapSettlement,
    PlayerCompendium,
    WorldMapInventory,
    WorldMapWorkshop,
    WorldMapSectResidence,
    WorldMapNpcDialogue,
    Expedition
}

public interface IGameHubNavigator
{
    void OpenWorldMapHome();
    void OpenSettlement();
    void OpenSectResidence(bool persistState);
}

public readonly struct GameUiPanelDefinition
{
    public GameUiPanelDefinition(Type panelType, string resourcePath, UILevel level, bool exclusiveWithinLevel = true)
    {
        PanelType = panelType;
        ResourcePath = resourcePath;
        Level = level;
        ExclusiveWithinLevel = exclusiveWithinLevel;
    }

    public Type PanelType { get; }
    public string ResourcePath { get; }
    public UILevel Level { get; }
    public bool ExclusiveWithinLevel { get; }
}

public static class GameUiPanelRegistry
{
    public static GameUiPanelDefinition Get(GameUiPanelId panelId)
    {
        switch (panelId)
        {
            case GameUiPanelId.MainMenu:
                return new GameUiPanelDefinition(typeof(MainMenuController), "UI/MainMenu/MainMenuRoot", UILevel.Bg);
            case GameUiPanelId.MainMenuSettings:
                return new GameUiPanelDefinition(typeof(MainMenuSettingsPanel), "UI/MainMenu/MainMenuSettingsPanel", UILevel.PopUI);
            case GameUiPanelId.MainMenuLoad:
                return new GameUiPanelDefinition(typeof(MainMenuLoadPanel), "UI/MainMenu/MainMenuLoadPanel", UILevel.PopUI);
            case GameUiPanelId.MainMenuCharacterCreate:
                return new GameUiPanelDefinition(typeof(MainMenuCharacterCreatePanel), "UI/MainMenu/MainMenuCharacterCreatePanel", UILevel.PopUI);
            case GameUiPanelId.MessagePopup:
                return new GameUiPanelDefinition(typeof(CultivationMessagePopupPanel), "UI/Overlay/CultivationMessagePopupPanel", UILevel.PopUI, false);
            case GameUiPanelId.Tooltip:
                return new GameUiPanelDefinition(typeof(CultivationTooltipPanel), "UI/Overlay/CultivationTooltipPanel", UILevel.AlwayTop, false);
            case GameUiPanelId.WorldMap:
                return new GameUiPanelDefinition(typeof(WorldMapController), "UI/WorldMap/WorldMapRoot", UILevel.Bg);
            case GameUiPanelId.GameHub:
                return new GameUiPanelDefinition(typeof(GameHubPanel), "UI/Game/GameHubPanel", UILevel.Const, false);
            case GameUiPanelId.WorldMapRegion:
                return new GameUiPanelDefinition(typeof(WorldMapRegionPanel), "UI/WorldMap/WorldMapRegionPanel", UILevel.Bg);
            case GameUiPanelId.WorldMapSettlement:
                return new GameUiPanelDefinition(typeof(WorldMapSettlementPanel), "UI/WorldMap/WorldMapSettlementPanel", UILevel.Bg);
            case GameUiPanelId.PlayerCompendium:
                return new GameUiPanelDefinition(typeof(PlayerCompendiumPanel), "UI/Game/PlayerCompendiumPanel", UILevel.PopUI);
            case GameUiPanelId.WorldMapInventory:
                return new GameUiPanelDefinition(typeof(WorldMapInventoryPanel), "UI/WorldMap/WorldMapInventoryPanel", UILevel.PopUI);
            case GameUiPanelId.WorldMapWorkshop:
                return new GameUiPanelDefinition(typeof(WorldMapWorkshopPanel), "UI/WorldMap/WorldMapWorkshopPanel", UILevel.PopUI);
            case GameUiPanelId.WorldMapSectResidence:
                return new GameUiPanelDefinition(typeof(WorldMapSectResidencePanel), "UI/WorldMap/WorldMapSectResidencePanel", UILevel.Bg);
            case GameUiPanelId.WorldMapNpcDialogue:
                return new GameUiPanelDefinition(typeof(WorldMapNpcDialoguePanel), "UI/WorldMap/WorldMapNpcDialoguePanel", UILevel.PopUI);
            case GameUiPanelId.Expedition:
                return new GameUiPanelDefinition(typeof(ExpeditionView), "UI/Game/ExpeditionRoot", UILevel.Bg);
            default:
                throw new ArgumentOutOfRangeException(nameof(panelId), panelId, null);
        }
    }
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

public sealed class GameHubPanelData : UIPanelData
{
    public GameHubPanelData(IGameHubNavigator navigator)
    {
        Navigator = navigator;
    }

    public IGameHubNavigator Navigator { get; }
}

public sealed class WorldMapRegionPanelData : UIPanelData
{
    public WorldMapRegionPanelData(WorldMapController owner, string regionId)
    {
        Owner = owner;
        RegionId = regionId ?? string.Empty;
    }

    public WorldMapController Owner { get; }
    public string RegionId { get; }
}

public sealed class WorldMapSettlementPanelData : UIPanelData
{
    public WorldMapSettlementPanelData(WorldMapController owner)
    {
        Owner = owner;
    }

    public WorldMapController Owner { get; }
}

public sealed class PlayerCompendiumPanelData : UIPanelData
{
}

public sealed class CultivationMessagePopupPanelData : UIPanelData
{
    public CultivationMessagePopupPanelData(string title, string message, float duration, CultivationMessagePopupStyle style = CultivationMessagePopupStyle.Info)
    {
        Title = title ?? string.Empty;
        Message = message ?? string.Empty;
        Duration = duration;
        Style = style;
    }

    public string Title { get; }
    public string Message { get; }
    public float Duration { get; }
    public CultivationMessagePopupStyle Style { get; }
}

public enum CultivationMessagePopupStyle
{
    Neutral,
    Info,
    Warning,
    Error,
    Success
}

public sealed class CultivationTooltipPanelData : UIPanelData
{
}

public sealed class WorldMapInventoryPanelData : UIPanelData
{
    public WorldMapInventoryPanelData(WorldMapController owner)
    {
        Owner = owner;
    }

    public WorldMapController Owner { get; }
}

public sealed class WorldMapWorkshopPanelData : UIPanelData
{
    public WorldMapWorkshopPanelData(WorldMapController owner)
    {
        Owner = owner;
    }

    public WorldMapController Owner { get; }
}

public sealed class WorldMapSectResidencePanelData : UIPanelData
{
    public WorldMapSectResidencePanelData(WorldMapController owner)
    {
        Owner = owner;
    }

    public WorldMapController Owner { get; }
}

public sealed class WorldMapNpcDialoguePanelData : UIPanelData
{
    public WorldMapNpcDialoguePanelData(WorldMapController owner, NpcSceneType sceneType, string regionId, string sectHallId)
    {
        Owner = owner;
        SceneType = sceneType;
        RegionId = regionId ?? string.Empty;
        SectHallId = sectHallId ?? string.Empty;
    }

    public WorldMapController Owner { get; }
    public NpcSceneType SceneType { get; }
    public string RegionId { get; }
    public string SectHallId { get; }
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
