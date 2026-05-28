using QFramework;

public sealed class OpenGameUiPanelCommand : AbstractCommand<UIPanel>
{
    private readonly GameUiPanelId panelId;
    private readonly IUIData uiData;

    public OpenGameUiPanelCommand(GameUiPanelId panelId, IUIData uiData = null)
    {
        this.panelId = panelId;
        this.uiData = uiData;
    }

    protected override UIPanel OnExecute()
    {
        return this.GetUtility<IGameUiService>().OpenPanel(panelId, uiData);
    }
}

public sealed class OpenMainMenuPanelCommand : AbstractCommand<MainMenuController>
{
    private readonly MainMenuConfig config;

    public OpenMainMenuPanelCommand(MainMenuConfig config)
    {
        this.config = config;
    }

    protected override MainMenuController OnExecute()
    {
        return this.GetUtility<IGameUiService>().OpenPanel(GameUiPanelId.MainMenu, new MainMenuPanelData(config)) as MainMenuController;
    }
}

public sealed class OpenWorldMapPanelCommand : AbstractCommand<WorldMapController>
{
    private readonly string gameplaySceneName;
    private readonly string mainSceneName;

    public OpenWorldMapPanelCommand(string gameplaySceneName, string mainSceneName)
    {
        this.gameplaySceneName = gameplaySceneName;
        this.mainSceneName = mainSceneName;
    }

    protected override WorldMapController OnExecute()
    {
        return this.GetUtility<IGameUiService>().OpenPanel(GameUiPanelId.WorldMap, new WorldMapPanelData(gameplaySceneName, mainSceneName)) as WorldMapController;
    }
}

public sealed class OpenExpeditionPanelCommand : AbstractCommand<ExpeditionView>
{
    protected override ExpeditionView OnExecute()
    {
        return this.GetUtility<IGameUiService>().OpenPanel(GameUiPanelId.Expedition) as ExpeditionView;
    }
}

public sealed class CloseGameUiPanelCommand : AbstractCommand
{
    private readonly GameUiPanelId panelId;

    public CloseGameUiPanelCommand(GameUiPanelId panelId)
    {
        this.panelId = panelId;
    }

    protected override void OnExecute()
    {
        GameUiStateCommandUtility.SyncBeforeHideOrClose(this, panelId);
        this.GetUtility<IGameUiService>().ClosePanel(panelId);
    }
}

public sealed class CloseAllGameUiPanelsCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        GameUiStateCommandUtility.SyncAllHidden(this);
        this.GetUtility<IGameUiService>().CloseAllPanels();
    }
}

public sealed class DestroyGameUiPanelCommand : AbstractCommand
{
    private readonly GameUiPanelId panelId;

    public DestroyGameUiPanelCommand(GameUiPanelId panelId)
    {
        this.panelId = panelId;
    }

    protected override void OnExecute()
    {
        GameUiStateCommandUtility.SyncBeforeHideOrClose(this, panelId);
        this.GetUtility<IGameUiService>().DestroyPanel(panelId);
    }
}

public sealed class HideGameUiPanelCommand : AbstractCommand
{
    private readonly GameUiPanelId panelId;

    public HideGameUiPanelCommand(GameUiPanelId panelId)
    {
        this.panelId = panelId;
    }

    protected override void OnExecute()
    {
        GameUiStateCommandUtility.SyncBeforeHideOrClose(this, panelId);
        this.GetUtility<IGameUiService>().HidePanel(panelId);
    }
}

public sealed class ShowGameUiPanelCommand : AbstractCommand
{
    private readonly GameUiPanelId panelId;

    public ShowGameUiPanelCommand(GameUiPanelId panelId)
    {
        this.panelId = panelId;
    }

    protected override void OnExecute()
    {
        this.GetUtility<IGameUiService>().ShowPanel(panelId);
    }
}

public sealed class SetGameHubStateCommand : AbstractCommand
{
    private readonly bool visible;
    private readonly GameHubContext context;

    public SetGameHubStateCommand(bool visible, GameHubContext context)
    {
        this.visible = visible;
        this.context = context;
    }

    protected override void OnExecute()
    {
        this.GetModel<CultivationGameModel>().SetHubState(visible, context);
    }
}

public sealed class SetPlayerCompendiumVisibilityCommand : AbstractCommand
{
    private readonly bool visible;

    public SetPlayerCompendiumVisibilityCommand(bool visible)
    {
        this.visible = visible;
    }

    protected override void OnExecute()
    {
        this.GetModel<CultivationGameModel>().SetPlayerCompendiumVisible(visible);
    }
}

public sealed class SetPlayerCompendiumSelectionCommand : AbstractCommand
{
    private readonly PlayerCompendiumMainTab mainTab;
    private readonly string sectionId;

    public SetPlayerCompendiumSelectionCommand(PlayerCompendiumMainTab mainTab, string sectionId)
    {
        this.mainTab = mainTab;
        this.sectionId = sectionId ?? string.Empty;
    }

    protected override void OnExecute()
    {
        this.GetModel<CultivationGameModel>().SetPlayerCompendiumSelection(mainTab, sectionId);
    }
}

internal static class GameUiStateCommandUtility
{
    public static void SyncBeforeHideOrClose(AbstractCommand command, GameUiPanelId panelId)
    {
        var gameModel = command.GetModel<CultivationGameModel>();
        if (gameModel == null)
        {
            return;
        }

        if (panelId == GameUiPanelId.WorldMap || panelId == GameUiPanelId.GameHub)
        {
            gameModel.SetHubState(false, GameHubContext.WorldMap);
        }

        if (panelId == GameUiPanelId.WorldMap || panelId == GameUiPanelId.PlayerCompendium)
        {
            gameModel.SetPlayerCompendiumVisible(false);
        }
    }

    public static void SyncAllHidden(AbstractCommand command)
    {
        var gameModel = command.GetModel<CultivationGameModel>();
        if (gameModel == null)
        {
            return;
        }

        gameModel.SetHubState(false, GameHubContext.WorldMap);
        gameModel.SetPlayerCompendiumVisible(false);
    }
}
