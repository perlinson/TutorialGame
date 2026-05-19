using UnityEngine;
using UnityEngine.UI;

public sealed partial class WorldMapController
{
    public void Initialize(string gameScene, string menuScene)
    {
        gameplaySceneName = gameScene;
        mainSceneName = menuScene;

        var snapshot = CultivationApp.BootstrapCurrentArchive();
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
        var taskBoardMessage = CultivationApp.ResolveTaskBoard(currentSlotIndex, saveData);
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
        RefreshAll();
        RefreshResponsiveLayout(true);
        ResetUiStateMachines(
            saveData.isInSectResidence && saveData.isSectDisciple ? WorldMapPrimaryState.SectResidence : WorldMapPrimaryState.Map,
            WorldMapModalState.None);

        SetHint(string.IsNullOrEmpty(taskBoardMessage) ? "山海图已展开，选择一处地域继续历练。" : taskBoardMessage);
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
        CultivationApp.SetMusicDuck(ModalMusicDuckReason, false);
        primaryPanels.HideAll();
        modalPanels.HideAll();
    }

    private void Update()
    {
        RefreshResponsiveLayout(false);
        if (HasActiveModalState())
        {
            modalStateMachine.Update();
            return;
        }

        primaryStateMachine.Update();
    }

    private void BindButtons()
    {
        CultivationAudio.BindButton(travelButton, TravelToSelectedRegion, CultivationButtonSound.Confirm);

        CultivationAudio.BindButton(bagButton, OpenInventory);

        CultivationAudio.BindButton(workshopButton, OpenWorkshop);

        if (sectButton != null)
        {
            CultivationAudio.BindButton(sectButton, () => OpenSectResidence(true));
        }

        if (sectResidenceButton != null)
        {
            CultivationAudio.BindButton(sectResidenceButton, () => OpenSectResidence(true));
        }

        CultivationAudio.BindButton(vitalityUpgradeButton, UpgradeVitality, CultivationButtonSound.Confirm);

        CultivationAudio.BindButton(attackUpgradeButton, UpgradeAttack, CultivationButtonSound.Confirm);

        CultivationAudio.BindButton(returnButton, ReturnToMain, CultivationButtonSound.Cancel);

        CultivationAudio.BindButton(closeInventoryButton, CloseInventory, CultivationButtonSound.Cancel);

        CultivationAudio.BindButton(closeWorkshopButton, CloseWorkshop, CultivationButtonSound.Cancel);

        if (closeSectButton != null)
        {
            CultivationAudio.BindButton(closeSectButton, CloseSect, CultivationButtonSound.Cancel);
        }

        CultivationAudio.BindButton(craftQiButton, () => CraftRecipe("pill_cauldron_upgrade"), CultivationButtonSound.Confirm);

        CultivationAudio.BindButton(craftBagButton, () => CraftRecipe("talisman_case_upgrade"), CultivationButtonSound.Confirm);

        CultivationAudio.BindButton(craftVitalityButton, () => CraftRecipe("peiyuan_powder"), CultivationButtonSound.Confirm);

        CultivationAudio.BindButton(craftAttackButton, () => CraftRecipe("nawu_pouch"), CultivationButtonSound.Confirm);
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
                    CultivationAudio.BindButton(view.button, () => SelectRegion(capturedIndex));
                    break;
                }
            }
        }

        BindButtons();
        ResolveLayoutReferences();
        ConfigureUiPanels();
        isInitialized = true;
    }

    private void SelectRegion(int index)
    {
        selectedRegionIndex = Mathf.Clamp(index, 0, regions.Count - 1);
        EnsureValidSelectedRegionSelection();
        RefreshAll();
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
        if (Application.CanStreamedLevelBeLoaded(mainSceneName))
        {
            SceneFlow.RequestScene(mainSceneName);
        }
    }

    private void ConfigureUiPanels()
    {
        primaryPanels.Configure(null, mapScreen, sectPanel);
        modalPanels.Configure(modalBlocker, inventoryPanel, workshopPanel);
        primaryPanels.HideAll();
        modalPanels.HideAll();

        if (modalBlocker != null)
        {
            var button = modalBlocker.GetComponent<Button>();
            if (button != null)
            {
                CultivationAudio.BindButton(button, CloseActiveModal, CultivationButtonSound.Cancel);
            }
        }
    }

    private void ShowMapScreen()
    {
        primaryPanels.Show(mapScreen);
        modalPanels.HideAll();
    }

    private void ShowSectScreen()
    {
        primaryPanels.Show(sectPanel);
        modalPanels.HideAll();
    }

    private void CloseActiveModal()
    {
        ChangeModalState(WorldMapModalState.None);
    }

    private void ResolveLayoutReferences()
    {
        rootRect = transform as RectTransform;
        mapContentRootRect = FindRect(mapScreen, "MapContentRoot");
        inventoryWindowRect = inventoryPanel != null ? inventoryPanel.GetComponent<RectTransform>() : null;
        workshopWindowRect = workshopPanel != null ? workshopPanel.GetComponent<RectTransform>() : null;
        sectWindowRect = FindRect(sectPanel, "SectWindow");
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
        ScaleWindowToFit(inventoryWindowRect, InventoryWindowDesignSize, rect.width, rect.height, 0.92f, 0.9f);
        ScaleWindowToFit(workshopWindowRect, WorkshopWindowDesignSize, rect.width, rect.height, 0.92f, 0.9f);
        ScaleWindowToFit(sectWindowRect, SectWindowDesignSize, rect.width, rect.height, 0.96f, 0.94f);
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
}
