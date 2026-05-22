using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerCompendiumPanel : CultivationUIPanel
{
    private static readonly Vector2 WindowDesignSize = new Vector2(1920f, 1080f);
    private static readonly string[] MainTabLabels = { "人物", "物品", "天赋", "修仙技艺" };
    private static readonly Color SelectedTabColor = new Color(0.62f, 0.5f, 0.24f, 0.98f);
    private static readonly Color NormalTabColor = new Color(0.2f, 0.18f, 0.14f, 0.96f);

    public Button closeButton;
    public Button[] mainTabButtons;
    public Button[] sectionTabButtons;
    public TextMeshProUGUI panelTitleText;
    public TextMeshProUGUI panelSubtitleText;
    public TextMeshProUGUI summaryText;
    public TextMeshProUGUI contentTitleText;
    public TextMeshProUGUI contentBodyText;
    public GameObject visualNodeRoot;
    public TextMeshProUGUI visualTitleText;
    public PlayerCompendiumNodeView[] visualNodeViews;
    public Image previewImage;
    public TextMeshProUGUI previewLabelText;
    public RectTransform windowRect;

    private RectTransform rootRect;
    private bool closeButtonBound;
    private bool modelBindingsReady;
    private int lastLayoutWidth = -1;
    private int lastLayoutHeight = -1;

    protected override void OnOpen(IUIData uiData = null)
    {
        rootRect = transform as RectTransform;
        EnsureCloseBinding();
        EnsureModelBindings();
        RefreshFromModels();
        RefreshResponsiveLayout(true);
        UiPanelOrderUtility.BringToFront(this, 240);
    }

    protected override void OnClose()
    {
    }

    private void Update()
    {
        RefreshResponsiveLayout(false);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    public void RefreshFromOwner()
    {
        RefreshFromModels();
    }

    private void EnsureModelBindings()
    {
        if (modelBindingsReady)
        {
            return;
        }

        var playerModel = PlayerModel;
        var gameModel = GameModel;

        if (playerModel != null)
        {
            playerModel.HeroName.RegisterWithInitValue(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            playerModel.ArchetypeName.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            playerModel.RealmName.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            playerModel.LocationName.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            playerModel.Qi.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            playerModel.SpiritCrystals.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            playerModel.BagUsedSlots.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            playerModel.BagCapacity.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        if (gameModel != null)
        {
            gameModel.PlayerCompendiumMainTab.RegisterWithInitValue(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            gameModel.PlayerCompendiumSectionId.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
            gameModel.IsPlayerCompendiumVisible.Register(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        this.RegisterEvent<PlayerCompendiumStateChangedEvent>(_ => RefreshFromModels()).UnRegisterWhenGameObjectDestroyed(gameObject);
        modelBindingsReady = true;
    }

    private void RefreshFromModels()
    {
        var playerModel = PlayerModel;
        var gameModel = GameModel;
        var selectedMainTab = gameModel != null ? gameModel.PlayerCompendiumMainTab.Value : PlayerCompendiumMainTab.Character;
        var selectedSectionId = gameModel != null ? gameModel.PlayerCompendiumSectionId.Value : string.Empty;
        var snapshot = GamePresentationBuilder.BuildPlayerCompendiumSnapshot(
            playerModel != null ? playerModel.CurrentSaveData : null,
            selectedMainTab,
            selectedSectionId);
        if (snapshot == null)
        {
            return;
        }

        if (panelTitleText != null)
        {
            panelTitleText.text = snapshot.PanelTitle;
        }

        if (panelSubtitleText != null)
        {
            panelSubtitleText.text = snapshot.PanelSubtitle;
        }

        if (summaryText != null)
        {
            summaryText.text = snapshot.SummaryText;
        }

        if (contentTitleText != null)
        {
            contentTitleText.text = snapshot.ContentTitle;
        }

        if (contentBodyText != null)
        {
            contentBodyText.text = snapshot.ContentBody;
        }

        ApplyVisualNodes(snapshot.VisualTitle, snapshot.VisualNodes);

        if (snapshot.Preview != null)
        {
            GameSpriteLibrary.BindSpriteOrPlaceholder(
                previewImage,
                previewLabelText,
                snapshot.Preview.Sprite,
                snapshot.Preview.Label,
                snapshot.Preview.PlaceholderColor);
        }

        ApplyMainTabs(selectedMainTab);
        ApplySectionTabs(snapshot.Sections, snapshot.ResolvedSectionId);
    }

    private void EnsureCloseBinding()
    {
        if (closeButtonBound)
        {
            return;
        }

        BindButton(closeButton, ClosePanel, CultivationButtonSound.Cancel);
        CultivationTooltipBinder.Bind(closeButton, "关闭总览", "收起当前综合页，回到上一层主界面。");
        closeButtonBound = true;
    }

    private void ApplyMainTabs(PlayerCompendiumMainTab selectedMainTab)
    {
        if (mainTabButtons == null)
        {
            return;
        }

        for (var i = 0; i < mainTabButtons.Length; i++)
        {
            var button = mainTabButtons[i];
            if (button == null)
            {
                continue;
            }

            var index = i;
            button.gameObject.SetActive(i < MainTabLabels.Length);
            if (!button.gameObject.activeSelf)
            {
                continue;
            }

            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = MainTabLabels[i];
            }

            button.interactable = selectedMainTab != (PlayerCompendiumMainTab) index;
            ApplyButtonSelection(button, selectedMainTab == (PlayerCompendiumMainTab) index);
            BindButton(button, () => SelectMainTab((PlayerCompendiumMainTab) index));
            CultivationTooltipBinder.Bind(button, MainTabLabels[i], "切换到" + MainTabLabels[i] + "分类。");
        }
    }

    private void ApplySectionTabs(PlayerCompendiumSectionSnapshot[] sections, string selectedSectionId)
    {
        if (sectionTabButtons == null)
        {
            return;
        }

        for (var i = 0; i < sectionTabButtons.Length; i++)
        {
            var button = sectionTabButtons[i];
            if (button == null)
            {
                continue;
            }

            var visible = sections != null && i < sections.Length && sections[i] != null;
            button.gameObject.SetActive(visible);
            if (!visible)
            {
                continue;
            }

            var section = sections[i];
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = section.Label;
            }

            var isSelected = selectedSectionId == section.Id;
            button.interactable = !isSelected;
            ApplyButtonSelection(button, isSelected);
            var capturedSectionId = section.Id;
            BindButton(button, () => SelectSection(capturedSectionId));
            CultivationTooltipBinder.Bind(button, section.Label, "查看" + section.Label + "内容。");
        }
    }

    private void SelectMainTab(PlayerCompendiumMainTab tab)
    {
        SetPlayerCompendiumSelection(tab, string.Empty);
    }

    private void SelectSection(string sectionId)
    {
        var gameModel = GameModel;
        var currentTab = gameModel != null ? gameModel.PlayerCompendiumMainTab.Value : PlayerCompendiumMainTab.Character;
        SetPlayerCompendiumSelection(currentTab, sectionId);
    }

    private void ClosePanel()
    {
        SetPlayerCompendiumVisible(false);
        CloseGameUiPanel(GameUiPanelId.PlayerCompendium);
    }

    private void ApplyVisualNodes(string visualTitle, PlayerCompendiumVisualNodeSnapshot[] nodes)
    {
        var hasNodes = nodes != null && nodes.Length > 0;

        if (visualNodeRoot != null)
        {
            visualNodeRoot.SetActive(hasNodes);
        }

        if (visualTitleText != null)
        {
            visualTitleText.text = string.IsNullOrWhiteSpace(visualTitle) ? "技艺节点" : visualTitle;
        }

        if (visualNodeViews == null)
        {
            return;
        }

        for (var i = 0; i < visualNodeViews.Length; i++)
        {
            var view = visualNodeViews[i];
            if (view == null)
            {
                continue;
            }

            var visible = hasNodes && i < nodes.Length && nodes[i] != null;
            view.gameObject.SetActive(visible);
            if (visible)
            {
                view.Bind(nodes[i]);
            }
        }
    }

    private void RefreshResponsiveLayout(bool force)
    {
        if (rootRect == null || windowRect == null)
        {
            return;
        }

        var rect = rootRect.rect;
        if (rect.width < 1f || rect.height < 1f)
        {
            return;
        }

        var width = Mathf.RoundToInt(rect.width);
        var height = Mathf.RoundToInt(rect.height);
        if (!force && width == lastLayoutWidth && height == lastLayoutHeight)
        {
            return;
        }

        lastLayoutWidth = width;
        lastLayoutHeight = height;
        windowRect.sizeDelta = WindowDesignSize;
        var scale = Mathf.Min(1f, rect.width / WindowDesignSize.x, rect.height / WindowDesignSize.y);
        windowRect.localScale = new Vector3(scale, scale, 1f);
    }

    private static void ApplyButtonSelection(Button button, bool selected)
    {
        if (button == null)
        {
            return;
        }

        var image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = selected ? SelectedTabColor : NormalTabColor;
        }
    }
}
