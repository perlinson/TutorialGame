using QFramework;

public sealed partial class WorldMapController
{
    public GameHubContext CurrentHudContext
    {
        get { return GameModel.CurrentHubContext.Value; }
    }

    public GameHubSnapshot BuildHudSnapshot(GameHubContext context)
    {
        var playerModel = PlayerModel;
        var gameModel = GameModel;
        return GamePresentationBuilder.BuildGameHubSnapshot(
            playerModel != null ? playerModel.CurrentSaveData : null,
            context,
            gameModel != null ? gameModel.WorldTimeText.Value : string.Empty);
    }

    public void SetHudContext(GameHubContext context)
    {
        SetHubState(true, context);
    }

    public void EnsureHudPanel()
    {
        var hudPanel = UIKit.GetPanel<GameHubPanel>();
        if (hudPanel != null)
        {
            hudPanel.BindOwner(this);
            hudPanel.RefreshFromOwner();
            ShowGameUiPanel(GameUiPanelId.GameHub);
            return;
        }

        if (OpenGameUiPanel(GameUiPanelId.GameHub, new GameHubPanelData(this)) == null)
        {
            ShowErrorMessage("GameHub prefab 缺失，请先重新生成 UI Prefabs。");
        }
    }

    public void RefreshHudPanel()
    {
        var hudPanel = UIKit.GetPanel<GameHubPanel>();
        if (hudPanel != null)
        {
            hudPanel.RefreshFromOwner();
        }
    }
}
