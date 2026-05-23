using QFramework;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class WorldMapController
{
    public void Initialize(string gameScene, string menuScene)
    {
        gameplaySceneName = gameScene;
        mainSceneName = menuScene;

        var snapshot = BootstrapCurrentArchive();
        if (snapshot == null || snapshot.SaveData == null)
        {
            if (Application.CanStreamedLevelBeLoaded(mainSceneName))
            {
                SceneFlow.RequestScene(mainSceneName);
            }

            return;
        }

        currentSlotIndex = snapshot.SlotIndex;
        saveData = snapshot.SaveData;
        saveData.EnsureDefaults();
        var taskBoardMessage = ResolveTaskBoard(currentSlotIndex, saveData);
        regions.Clear();
        regions.AddRange(WorldRegionLibrary.GetRegions());

        EnsureViewInitialized();

        selectedRegionIndex = 0;
        for (var i = 0; i < regions.Count; i++)
        {
            if (regions[i].Id == saveData.currentRegionId)
            {
                selectedRegionIndex = i;
                break;
            }
        }

        EnsureValidSelectedRegionSelection();
        SetHudContext(GameHubContext.WorldMap);
        EnsureHudPanel();
        EnsureNavigationInitialized();
        RefreshAll();
        RefreshResponsiveLayout(true);

        SetHint(string.IsNullOrEmpty(taskBoardMessage) ? "山海图已展开，选择一处地域继续历练。" : taskBoardMessage);

        if (saveData.isInSectResidence && saveData.isSectDisciple)
        {
            OpenSectResidence(false);
        }
        else
        {
            CloseGameUiPanel(GameUiPanelId.WorldMapRegion);
            CloseGameUiPanel(GameUiPanelId.WorldMapSectResidence);
        }
    }

    protected override void OnOpen(QFramework.IUIData uiData = null)
    {
        var panelData = uiData as WorldMapPanelData;
        var gameplayScene = panelData != null ? panelData.GameplaySceneName : SceneFlow.GameplaySceneName;
        var menuScene = panelData != null ? panelData.MainSceneName : SceneFlow.MainMenuSceneName;
        Initialize(gameplayScene, menuScene);
    }

    protected override void OnClose()
    {
        SetMusicDuck(ModalMusicDuckReason, false);
        SetHubState(false, GameHubContext.WorldMap);
        SetPlayerCompendiumVisible(false);
        CloseGameUiPanel(GameUiPanelId.GameHub);
        CloseGameUiPanel(GameUiPanelId.WorldMapRegion);
        CloseGameUiPanel(GameUiPanelId.WorldMapSectResidence);
        CloseGameUiPanel(GameUiPanelId.PlayerCompendium);
    }

    private void Update()
    {
        RefreshResponsiveLayout(false);
        UpdateDetailPanelTransition(Time.unscaledDeltaTime);
        UpdateNavigation(Time.unscaledDeltaTime);
        if (Input.GetKeyDown(KeyCode.Escape) && !HasBlockingPanelOpen())
        {
            ReturnToMain();
        }
    }

    private void BindButtons()
    {
        BindButton(travelButton, TravelToSelectedRegion, CultivationButtonSound.Confirm);
        CultivationTooltipBinder.Bind(travelButton, "前往历练", "进入当前选中地域，开始本次历练流程。");

        BindButton(bagButton, OpenInventory);
        CultivationTooltipBinder.Bind(bagButton, "储物袋", "查看当前存档携带的材料、药材、战利品和任务道具。");

        BindButton(workshopButton, OpenSettlement);
        CultivationTooltipBinder.Bind(workshopButton, "坊市整备", "打开城镇/洞府整备页面，处理储物、炼制与法器养成。");

        if (sectResidenceButton != null)
        {
            BindButton(sectResidenceButton, () => OpenSectResidence(true));
            CultivationTooltipBinder.Bind(sectResidenceButton, "门派驻地", "返回山门驻地，处理宗门与洞府事务。");
        }

        BindButton(vitalityUpgradeButton, UpgradeVitality, CultivationButtonSound.Confirm);
        CultivationTooltipBinder.Bind(vitalityUpgradeButton, "温养护身法器", "消耗灵石强化护身法器，提升生存能力与护体强度。");

        BindButton(attackUpgradeButton, UpgradeAttack, CultivationButtonSound.Confirm);
        CultivationTooltipBinder.Bind(attackUpgradeButton, "祭炼主法器", "消耗灵石强化主法器，提升战斗中的攻伐能力。");

        BindButton(returnButton, ReturnToMain, CultivationButtonSound.Cancel);
        CultivationTooltipBinder.Bind(returnButton, "返回主界面", "离开大地图并回到主菜单。");
    }

    private void EnsureViewInitialized()
    {
        if (isInitialized)
        {
            return;
        }

        nodeViews.Clear();
        nodeViews.AddRange(GetComponentsInChildren<WorldRegionNodeView>(true));

        for (var i = 0; i < nodeViews.Count; i++)
        {
            var view = nodeViews[i];
            if (view == null)
            {
                continue;
            }

            for (var regionIndex = 0; regionIndex < regions.Count; regionIndex++)
            {
                if (regions[regionIndex].Id == view.RegionId)
                {
                    var capturedIndex = regionIndex;
                    BindButton(view.button, () => BeginTravelToRegion(capturedIndex));
                    break;
                }
            }
        }

        BindButtons();
        ResolveLayoutReferences();
        isInitialized = true;
    }

    private bool HasRegions()
    {
        return regions != null && regions.Count > 0;
    }

    private bool EnsureValidSelectedRegionSelection()
    {
        if (!HasRegions())
        {
            selectedRegionIndex = 0;
            return false;
        }

        selectedRegionIndex = Mathf.Clamp(selectedRegionIndex, 0, regions.Count - 1);
        return true;
    }

    private void ReturnToMain()
    {
        CloseFloatingPanels();
        if (Application.CanStreamedLevelBeLoaded(mainSceneName))
        {
            SceneFlow.RequestScene(mainSceneName);
        }
    }

    private void ResolveLayoutReferences()
    {
        rootRect = transform as RectTransform;
        titlePanelRect = FindRect(mapScreen, "MapContentRoot/TitlePanel");
        mapPanelRect = FindRect(mapScreen, "MapContentRoot/MapPanel");
        mapContentRootRect = FindRect(mapScreen, "MapContentRoot");
        mapFieldRect = FindRect(mapScreen, "MapContentRoot/MapPanel/MapField");
        detailPanelRect = FindRect(mapScreen, "MapContentRoot/DetailPanel");
        hintPanelRect = FindRect(mapScreen, "MapContentRoot/HintPanel");
        PrepareCompactMapLayout();
        PrepareDetailPanelTransition();
    }

    private void RefreshResponsiveLayout(bool force)
    {
        if (rootRect == null)
        {
            rootRect = transform as RectTransform;
            if (rootRect == null)
            {
                return;
            }
        }

        var rect = rootRect.rect;
        if (rect.width < 1f || rect.height < 1f)
        {
            return;
        }

        var layoutWidth = Mathf.RoundToInt(rect.width);
        var layoutHeight = Mathf.RoundToInt(rect.height);
        if (!force && layoutWidth == lastLayoutWidth && layoutHeight == lastLayoutHeight)
        {
            return;
        }

        lastLayoutWidth = layoutWidth;
        lastLayoutHeight = layoutHeight;

        ScaleWindowToFit(mapContentRootRect, MapContentDesignSize, rect.width, rect.height, 0.96f, 0.96f);
        RefreshCompactMapLayout();
        RefreshDetailPanelTransitionLayout();
    }

    private static RectTransform FindRect(GameObject parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        var child = parent.transform.Find(childName);
        return child != null ? child as RectTransform : null;
    }

    private static void ScaleWindowToFit(RectTransform rect, Vector2 designSize, float width, float height, float widthPaddingRatio, float heightPaddingRatio)
    {
        if (rect == null)
        {
            return;
        }

        rect.sizeDelta = designSize;
        var scale = Mathf.Min(1f, width * widthPaddingRatio / designSize.x, height * heightPaddingRatio / designSize.y);
        rect.localScale = new Vector3(scale, scale, 1f);
    }

    private void PrepareCompactMapLayout()
    {
        if (compactMapLayoutPrepared || mapContentRootRect == null)
        {
            return;
        }

        compactMapLayoutPrepared = true;

        HideGraphicAndDisableRaycast(titlePanelRect);
        HideGraphicAndDisableRaycast(mapPanelRect);
        HideGraphicAndDisableRaycast(hintPanelRect);

        HideDecorativeChild(titlePanelRect, "Top");
        HideDecorativeChild(titlePanelRect, "Bottom");
        HideDecorativeChild(titlePanelRect, "Left");
        HideDecorativeChild(titlePanelRect, "Right");
        HideDecorativeChild(mapPanelRect, "Top");
        HideDecorativeChild(mapPanelRect, "Bottom");
        HideDecorativeChild(mapPanelRect, "Left");
        HideDecorativeChild(mapPanelRect, "Right");
        HideDecorativeChild(mapPanelRect, "MapCaption");

        SetObjectVisible(titleText, false);
        SetObjectVisible(heroSummaryText, false);
        SetObjectVisible(resourceSummaryText, false);
        SetObjectVisible(bagSummaryText, false);
        SetObjectVisible(hintText, false);
        SetObjectVisible(regionPreviewImage, false);
        SetObjectVisible(regionPreviewLabelText, false);
        SetObjectVisible(taskPreviewImage, false);
        SetObjectVisible(taskPreviewLabelText, false);
        SetObjectVisible(taskSummaryText, false);
        SetObjectVisible(bagButton, false);
        SetObjectVisible(workshopButton, false);

        ConfigureCompactActionButton(returnButton, new Vector2(-66f, -44f), "返");

        if (sectResidenceButton != null)
        {
            SetObjectVisible(sectResidenceButton, false);
        }

        ConfigureCompactDetailPanel();
    }

    private void RefreshCompactMapLayout()
    {
        if (navigationInitialized)
        {
            // 觅长生风格大地图布局由 EnsureNavigationInitialized 负责，不再覆盖。
            return;
        }

        if (mapPanelRect != null)
        {
            mapPanelRect.anchorMin = new Vector2(0f, 1f);
            mapPanelRect.anchorMax = new Vector2(0f, 1f);
            mapPanelRect.pivot = new Vector2(0f, 1f);
            mapPanelRect.anchoredPosition = new Vector2(70f, -180f);
            mapPanelRect.sizeDelta = new Vector2(1180f, 730f);
        }
    }

    private void PrepareDetailPanelTransition()
    {
        if (detailPanelRect == null)
        {
            return;
        }

        detailPanelCanvasGroup = detailPanelRect.GetComponent<CanvasGroup>();
        if (detailPanelCanvasGroup == null)
        {
            detailPanelCanvasGroup = detailPanelRect.gameObject.AddComponent<CanvasGroup>();
        }

        detailPanelShownPosition = detailPanelRect.anchoredPosition;
        detailPanelHiddenPosition = detailPanelShownPosition + new Vector2(84f, 0f);
        detailPanelVisibility = 0f;
        detailPanelTargetVisibility = 0f;
        ApplyDetailPanelTransition();
    }

    private void ConfigureCompactDetailPanel()
    {
        if (detailPanelRect == null)
        {
            return;
        }

        detailPanelRect.sizeDelta = new Vector2(500f, 392f);

        SetRect(regionTitleText, new Vector2(24f, -22f), new Vector2(452f, 34f));
        SetRect(regionBodyText, new Vector2(24f, -74f), new Vector2(452f, 148f));
        SetRect(regionStatusText, new Vector2(24f, -232f), new Vector2(452f, 56f));

        SetRect(travelButton, new Vector2(24f, -310f), new Vector2(452f, 54f));
        SetRect(vitalityUpgradeButton, new Vector2(24f, -372f), new Vector2(216f, 40f));
        SetRect(attackUpgradeButton, new Vector2(260f, -372f), new Vector2(216f, 40f));
    }

    private void RefreshDetailPanelTransitionLayout()
    {
        if (detailPanelRect == null)
        {
            return;
        }

        detailPanelRect.anchorMin = new Vector2(1f, 0.5f);
        detailPanelRect.anchorMax = new Vector2(1f, 0.5f);
        detailPanelRect.pivot = new Vector2(1f, 0.5f);
        detailPanelShownPosition = new Vector2(-70f, 0f);
        detailPanelHiddenPosition = detailPanelShownPosition + new Vector2(84f, 0f);
        ApplyDetailPanelTransition();
    }

    private void RestartDetailPanelTransition()
    {
        detailPanelVisibility = 0f;
        detailPanelTargetVisibility = 1f;
        ApplyDetailPanelTransition();
    }

    private void SetDetailPanelVisible(bool visible)
    {
        detailPanelTargetVisibility = visible ? 1f : 0f;
        if (!visible)
        {
            detailPanelVisibility = 0f;
            ApplyDetailPanelTransition();
        }
    }

    private void UpdateDetailPanelTransition(float deltaTime)
    {
        if (detailPanelRect == null)
        {
            return;
        }

        if (Mathf.Approximately(detailPanelVisibility, detailPanelTargetVisibility))
        {
            return;
        }

        detailPanelVisibility = Mathf.MoveTowards(detailPanelVisibility, detailPanelTargetVisibility, deltaTime * 4.6f);
        ApplyDetailPanelTransition();
    }

    private void ApplyDetailPanelTransition()
    {
        if (detailPanelRect == null || detailPanelCanvasGroup == null)
        {
            return;
        }

        var eased = 1f - Mathf.Pow(1f - detailPanelVisibility, 3f);
        detailPanelRect.anchoredPosition = Vector2.Lerp(detailPanelHiddenPosition, detailPanelShownPosition, eased);
        detailPanelCanvasGroup.alpha = eased;
        detailPanelCanvasGroup.blocksRaycasts = eased > 0.99f;
        detailPanelCanvasGroup.interactable = eased > 0.99f;
    }

    private void ConfigureCompactActionButton(Button button, Vector2 anchoredPosition, string label)
    {
        if (button == null || mapContentRootRect == null)
        {
            return;
        }

        button.transform.SetParent(mapContentRootRect, false);
        var rect = button.transform as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = CompactActionButtonSize;
        }

        SetButtonLabel(button, label, 26f);
    }

    private void ConfigureTopLeftActionButton(Button button, Vector2 anchoredPosition, string label)
    {
        if (button == null || mapContentRootRect == null)
        {
            return;
        }

        var rect = button.transform as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = CompactActionButtonSize;
        }

        SetButtonLabel(button, label, 26f);
    }

    private static void SetButtonLabel(Button button, string label, float size)
    {
        if (button == null)
        {
            return;
        }

        var text = button.GetComponentInChildren<TMPro.TMP_Text>(true);
        if (text == null)
        {
            return;
        }

        text.text = label;
        text.fontSize = size;
    }

    private static void SetRect(Component component, Vector2 anchoredPosition, Vector2 size)
    {
        if (component == null)
        {
            return;
        }

        var rect = component.transform as RectTransform;
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void HideGraphicAndDisableRaycast(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        var image = rect.GetComponent<Image>();
        if (image != null)
        {
            var color = image.color;
            color.a = 0f;
            image.color = color;
            image.raycastTarget = false;
        }
    }

    private static void HideDecorativeChild(RectTransform parent, string childName)
    {
        if (parent == null)
        {
            return;
        }

        var child = parent.Find(childName);
        if (child != null)
        {
            child.gameObject.SetActive(false);
        }
    }

    private static void SetObjectVisible(Object target, bool visible)
    {
        switch (target)
        {
            case Component component when component != null:
                component.gameObject.SetActive(visible);
                break;
            case GameObject gameObject when gameObject != null:
                gameObject.SetActive(visible);
                break;
        }
    }

    private static bool HasBlockingPanelOpen()
    {
        return UIKit.GetPanel<WorldMapRegionPanel>() != null ||
               UIKit.GetPanel<WorldMapSettlementPanel>() != null ||
               UIKit.GetPanel<PlayerCompendiumPanel>() != null ||
               UIKit.GetPanel<WorldMapInventoryPanel>() != null ||
               UIKit.GetPanel<WorldMapWorkshopPanel>() != null ||
               UIKit.GetPanel<WorldMapSectResidencePanel>() != null;
    }
}
