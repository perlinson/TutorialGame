using QFramework;

public sealed class GameHubPanel : CultivationUIPanel
{
    public GameHubView hudView;

    private IGameHubNavigator navigator;
    private bool isBoundToModels;
    private bool buttonsBound;

    protected override void OnOpen(IUIData uiData = null)
    {
        navigator = (uiData as GameHubPanelData)?.Navigator;
        if (hudView == null)
        {
            hudView = GetComponentInChildren<GameHubView>(true);
        }

        EnsureButtonBindings();
        EnsureModelBindings();
        RefreshFromModels();
        UiPanelOrderUtility.BringToFront(this, 320);
    }

    protected override void OnClose()
    {
    }

    public void RefreshFromOwner()
    {
        RefreshFromModels();
    }

    public void BindOwner(IGameHubNavigator hubNavigator)
    {
        navigator = hubNavigator;
    }

    private void EnsureButtonBindings()
    {
        if (buttonsBound || hudView == null)
        {
            return;
        }

        BindButton(hudView.mapButton, OnMapClicked);
        BindButton(hudView.inventoryButton, OnInventoryClicked);
        BindButton(hudView.settlementButton, OnSettlementClicked);
        BindButton(hudView.sectButton, OnSectClicked);

        CultivationTooltipBinder.Bind(hudView.mapButton, "山海图", "回到大地图主界面。");
        CultivationTooltipBinder.Bind(hudView.inventoryButton, "修士总览", "打开人物、物品、天赋与修仙技艺的综合页面。");
        CultivationTooltipBinder.Bind(hudView.settlementButton, "坊市整备", "打开城镇整备与炼制相关界面。");
        CultivationTooltipBinder.Bind(hudView.sectButton, "门派驻地", "前往山门驻地处理宗门事务。");
        buttonsBound = true;
    }

    private void EnsureModelBindings()
    {
        if (isBoundToModels)
        {
            return;
        }

        var playerModel = PlayerModel;
        var gameModel = GameModel;

        if (playerModel != null)
        {
            playerModel.HeroName.RegisterWithInitValue(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            playerModel.RealmName.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            playerModel.LocationName.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            playerModel.Qi.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            playerModel.SpiritCrystals.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            playerModel.MainArtifactLevel.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            playerModel.ProtectiveRelicLevel.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        if (gameModel != null)
        {
            gameModel.WorldTimeText.RegisterWithInitValue(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            gameModel.CurrentHubContext.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            gameModel.IsHubVisible.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        this.RegisterEvent<GameHubStateChangedEvent>(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
        isBoundToModels = true;
    }

    private void RefreshFromModels()
    {
        if (hudView == null)
        {
            return;
        }

        var playerModel = PlayerModel;
        var gameModel = GameModel;
        var snapshot = GamePresentationBuilder.BuildGameHubSnapshot(
            playerModel != null ? playerModel.CurrentSaveData : null,
            gameModel != null ? gameModel.CurrentHubContext.Value : GameHubContext.WorldMap,
            gameModel != null ? gameModel.WorldTimeText.Value : string.Empty);

        hudView.Apply(snapshot, navigator != null, navigator != null);
    }

    private void OnMapClicked()
    {
        if (navigator == null || GameModel.CurrentHubContext.Value == GameHubContext.WorldMap)
        {
            return;
        }

        navigator.OpenWorldMapHome();
    }

    private void OnInventoryClicked()
    {
        SetPlayerCompendiumSelection(PlayerCompendiumMainTab.Character, string.Empty);
        SetPlayerCompendiumVisible(true);
        if (OpenGameUiPanel(GameUiPanelId.PlayerCompendium, new PlayerCompendiumPanelData()) == null)
        {
            ShowErrorMessage("修士总览面板 prefab 缺失，请先重新生成 UI Prefabs。");
        }
    }

    private void OnSettlementClicked()
    {
        if (navigator == null || GameModel.CurrentHubContext.Value == GameHubContext.Settlement)
        {
            return;
        }

        navigator.OpenSettlement();
    }

    private void OnSectClicked()
    {
        if (navigator == null || GameModel.CurrentHubContext.Value == GameHubContext.SectResidence)
        {
            return;
        }

        navigator.OpenSectResidence(true);
    }
}
