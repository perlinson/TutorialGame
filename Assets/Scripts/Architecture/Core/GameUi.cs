using QFramework;

public static class GameUi
{
    private static IGameUiService Service
    {
        get
        {
            CultivationApp.EnsureInitialized();
            return CultivationApp.Interface.GetUtility<IGameUiService>();
        }
    }

    public static UIPanel OpenPanel(GameUiPanelId panelId, IUIData uiData = null)
    {
        return Service.OpenPanel(panelId, uiData);
    }

    public static void ShowPanel(GameUiPanelId panelId)
    {
        Service.ShowPanel(panelId);
    }

    public static void HidePanel(GameUiPanelId panelId)
    {
        Service.HidePanel(panelId);
    }

    public static void ClosePanel(GameUiPanelId panelId)
    {
        Service.ClosePanel(panelId);
    }
}
