public sealed partial class WorldMapController
{
    public PlayerCompendiumSnapshot BuildCompendiumSnapshot(PlayerCompendiumMainTab mainTab, string sectionId)
    {
        var playerModel = PlayerModel;
        return GamePresentationBuilder.BuildPlayerCompendiumSnapshot(
            playerModel != null ? playerModel.CurrentSaveData : null,
            mainTab,
            sectionId);
    }

    public void OpenCompendium()
    {
        CloseWorkshop();
        SetPlayerCompendiumSelection(PlayerCompendiumMainTab.Character, string.Empty);
        SetPlayerCompendiumVisible(true);
        if (OpenGameUiPanel(GameUiPanelId.PlayerCompendium, new PlayerCompendiumPanelData()) == null)
        {
            SetHint("修士总览面板 prefab 缺失，请先重新生成 UI Prefabs。");
            ShowErrorMessage("修士总览面板 prefab 缺失，请先重新生成 UI Prefabs。");
            return;
        }

        SetHint("已展开修士总览，可在人物、物品、天赋与修仙技艺之间切换。");
    }
}
