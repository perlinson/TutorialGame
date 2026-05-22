using QFramework;

public sealed partial class MainMenuController
{
    protected override void OnOpen(IUIData uiData = null)
    {
        var panelData = uiData as MainMenuPanelData;
        Initialize(panelData != null ? panelData.Config : BuildFallbackConfig());
    }

    protected override void OnClose()
    {
        SetMusicDuck(ModalMusicDuckReason, false);
        CloseAllPanels();
    }

    private MainMenuConfig BuildFallbackConfig()
    {
        if (!string.IsNullOrWhiteSpace(config.Title)
            || !string.IsNullOrWhiteSpace(config.Subtitle)
            || !string.IsNullOrWhiteSpace(config.Description)
            || !string.IsNullOrWhiteSpace(config.GameplaySceneName))
        {
            return config;
        }

        return new MainMenuConfig(string.Empty, "山海问道", "单机修真 / 2D 角色冒险", "主菜单面板缺少配置数据。");
    }
}
