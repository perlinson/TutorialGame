using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class WorldMapController : MonoBehaviour
{
    public Text titleText;
    public Text heroSummaryText;
    public Text resourceSummaryText;
    public Text bagSummaryText;
    public Text regionTitleText;
    public Text regionBodyText;
    public Text regionStatusText;
    public Text taskSummaryText;
    public Image taskPreviewImage;
    public Text taskPreviewLabelText;
    public Text inventoryDetailText;
    public Text workshopSummaryText;
    public Text sectTitleText;
    public Text sectDescriptionText;
    public Text sectStatusText;
    public Image regionPreviewImage;
    public Text regionPreviewLabelText;
    public Image inventoryPreviewImage;
    public Text inventoryPreviewLabelText;
    public Image workshopPreviewImage;
    public Text workshopPreviewLabelText;
    public Image sectPreviewImage;
    public Text sectPreviewLabelText;
    public Text hintText;

    public Button travelButton;
    public Button bagButton;
    public Button workshopButton;
    public Button sectButton;
    public Button sectResidenceButton;
    public Button vitalityUpgradeButton;
    public Button attackUpgradeButton;
    public Button returnButton;
    public Button closeInventoryButton;
    public Button closeWorkshopButton;
    public Button closeSectButton;
    public Button craftQiButton;
    public Button craftBagButton;
    public Button craftVitalityButton;
    public Button craftAttackButton;
    public Button[] sectHallButtons;
    public Button[] sectActionButtons;

    public GameObject inventoryPanel;
    public GameObject workshopPanel;
    public GameObject sectPanel;

    private readonly List<WorldRegionDefinition> regions = new List<WorldRegionDefinition>();
    private readonly List<WorldRegionNodeView> nodeViews = new List<WorldRegionNodeView>();

    private string gameplaySceneName;
    private string mainSceneName;
    private int currentSlotIndex = -1;
    private int selectedRegionIndex;
    private int selectedSectHallIndex;
    private MainMenuSaveData saveData;
    private SectHallSnapshot[] sectHallSnapshots = new SectHallSnapshot[0];

    public void Initialize(string gameScene, string menuScene)
    {
        gameplaySceneName = gameScene;
        mainSceneName = menuScene;

        var snapshot = CultivationApp.BootstrapCurrentArchive();
        if (snapshot == null || snapshot.SaveData == null)
        {
            if (Application.CanStreamedLevelBeLoaded(mainSceneName))
            {
                SceneManager.LoadScene(mainSceneName);
            }

            return;
        }

        currentSlotIndex = snapshot.SlotIndex;
        saveData = snapshot.SaveData;
        saveData.EnsureDefaults();
        var taskBoardMessage = CultivationApp.ResolveTaskBoard(currentSlotIndex, saveData);
        regions.Clear();
        regions.AddRange(WorldRegionLibrary.GetRegions());

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
                    view.button.onClick.RemoveAllListeners();
                    view.button.onClick.AddListener(() => SelectRegion(capturedIndex));
                    break;
                }
            }
        }

        BindButtons();

        selectedRegionIndex = 0;
        for (var i = 0; i < regions.Count; i++)
        {
            if (regions[i].Id == saveData.currentRegionId)
            {
                selectedRegionIndex = i;
                break;
            }
        }

        RefreshAll();
        if (saveData.isInSectResidence && saveData.isSectDisciple)
        {
            OpenSectResidence(false);
        }

        SetHint(string.IsNullOrEmpty(taskBoardMessage) ? "山海图已展开，选择一处地域继续历练。" : taskBoardMessage);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventoryPanel != null && inventoryPanel.activeSelf)
            {
                CloseInventory();
                return;
            }

            if (workshopPanel != null && workshopPanel.activeSelf)
            {
                CloseWorkshop();
                return;
            }

            if (sectPanel != null && sectPanel.activeSelf)
            {
                CloseSect();
                return;
            }

            ReturnToMain();
        }
    }

    private void BindButtons()
    {
        travelButton.onClick.RemoveAllListeners();
        travelButton.onClick.AddListener(TravelToSelectedRegion);

        bagButton.onClick.RemoveAllListeners();
        bagButton.onClick.AddListener(OpenInventory);

        workshopButton.onClick.RemoveAllListeners();
        workshopButton.onClick.AddListener(OpenWorkshop);

        if (sectButton != null)
        {
            sectButton.onClick.RemoveAllListeners();
            sectButton.onClick.AddListener(() => OpenSectResidence(true));
        }

        if (sectResidenceButton != null)
        {
            sectResidenceButton.onClick.RemoveAllListeners();
            sectResidenceButton.onClick.AddListener(() => OpenSectResidence(true));
        }

        vitalityUpgradeButton.onClick.RemoveAllListeners();
        vitalityUpgradeButton.onClick.AddListener(UpgradeVitality);

        attackUpgradeButton.onClick.RemoveAllListeners();
        attackUpgradeButton.onClick.AddListener(UpgradeAttack);

        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(ReturnToMain);

        closeInventoryButton.onClick.RemoveAllListeners();
        closeInventoryButton.onClick.AddListener(CloseInventory);

        closeWorkshopButton.onClick.RemoveAllListeners();
        closeWorkshopButton.onClick.AddListener(CloseWorkshop);

        if (closeSectButton != null)
        {
            closeSectButton.onClick.RemoveAllListeners();
            closeSectButton.onClick.AddListener(CloseSect);
        }

        craftQiButton.onClick.RemoveAllListeners();
        craftQiButton.onClick.AddListener(() => CraftRecipe("pill_cauldron_upgrade"));

        craftBagButton.onClick.RemoveAllListeners();
        craftBagButton.onClick.AddListener(() => CraftRecipe("talisman_case_upgrade"));

        craftVitalityButton.onClick.RemoveAllListeners();
        craftVitalityButton.onClick.AddListener(() => CraftRecipe("peiyuan_powder"));

        craftAttackButton.onClick.RemoveAllListeners();
        craftAttackButton.onClick.AddListener(() => CraftRecipe("nawu_pouch"));
    }

    private void SelectRegion(int index)
    {
        selectedRegionIndex = Mathf.Clamp(index, 0, regions.Count - 1);
        RefreshAll();
    }

    private void TravelToSelectedRegion()
    {
        var region = regions[selectedRegionIndex];
        var result = CultivationApp.TravelToRegion(currentSlotIndex, saveData, region);
        if (!result.Succeeded)
        {
            SetHint(result.Message);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            SetHint("场景未加入 Build Settings: " + gameplaySceneName);
            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    private void UpgradeVitality()
    {
        var result = CultivationApp.UpgradeProtectiveRelic(currentSlotIndex, saveData);
        RefreshAll();
        SetHint(result.Message);
    }

    private void UpgradeAttack()
    {
        var result = CultivationApp.UpgradeMainArtifact(currentSlotIndex, saveData);
        RefreshAll();
        SetHint(result.Message);
    }

    private void ReturnToMain()
    {
        if (Application.CanStreamedLevelBeLoaded(mainSceneName))
        {
            SceneManager.LoadScene(mainSceneName);
        }
    }

    private void OpenInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        if (workshopPanel != null)
        {
            workshopPanel.SetActive(false);
        }

        if (sectPanel != null)
        {
            sectPanel.SetActive(false);
        }

        RefreshPanels();
        SetHint("储物袋已展开，可在此查看历练带回的灵材与凭证。");
    }

    private void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    private void OpenWorkshop()
    {
        if (workshopPanel != null)
        {
            workshopPanel.SetActive(true);
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (sectPanel != null)
        {
            sectPanel.SetActive(false);
        }

        RefreshPanels();
        SetHint("洞府整备已展开。主法器与护身法器可直接耗灵石精修，丹炉和符匣则依赖材料拓展。");
    }

    private void CloseWorkshop()
    {
        if (workshopPanel != null)
        {
            workshopPanel.SetActive(false);
        }
    }

    private void OpenSectResidence(bool persistState)
    {
        if (saveData == null || !saveData.isSectDisciple)
        {
            SetHint("散修无固定山门驻地，只能在大地图中游历寻机缘。");
            return;
        }

        if (sectPanel != null)
        {
            sectPanel.SetActive(true);
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (workshopPanel != null)
        {
            workshopPanel.SetActive(false);
        }

        RefreshPanels();
        if (persistState)
        {
            saveData.isInSectResidence = true;
            saveData.location = saveData.sectName;
            CultivationApp.SaveArchive(currentSlotIndex, saveData);
        }

        SetHint("已回到" + saveData.sectName + "。自己的洞府和各殿堂都在山门内。");
    }

    private void CloseSect()
    {
        if (sectPanel != null)
        {
            sectPanel.SetActive(false);
        }

        if (saveData != null)
        {
            saveData.isInSectResidence = false;
            saveData.location = WorldRegionLibrary.GetRegionDisplayName(saveData.currentRegionId);
            CultivationApp.SaveArchive(currentSlotIndex, saveData);
            RefreshAll();
        }

        SetHint("已离开门派，回到山海大地图。");
    }

    private void CraftRecipe(string recipeId)
    {
        var result = CultivationApp.CraftRecipe(currentSlotIndex, saveData, recipeId);
        RefreshAll();
        RefreshPanels();
        SetHint(result.Message);
    }

    private void RefreshAll()
    {
        RefreshHeader();
        RefreshNodes();
        RefreshDetail();
        RefreshButtons();
        RefreshPanels();
    }

    private void RefreshHeader()
    {
        titleText.text = "九州山海图";
        heroSummaryText.text = saveData.heroName + " / " + saveData.archetypeName + "\n" +
                               saveData.realm + " · " + saveData.sectName + " · 当前驻足 " + saveData.location + "\n" +
                               "已肃清地界：" + saveData.clearedRegionIds.Length + " / " + regions.Count;

        var nextQi = WorldRegionLibrary.GetQiRequiredForNextRealm(saveData.realmTier);
        resourceSummaryText.text = "灵石：" + saveData.spiritCrystals +
                                   "    修为：" + saveData.qi + (nextQi > 0 ? " / " + nextQi : " / 圆满") + "\n" +
                                   CultivationLoadoutLibrary.BuildCompactProgressSummary(saveData) + "\n" +
                                   "护身温养耗费 " + WorldRegionLibrary.GetVitalityUpgradeCost(saveData) +
                                   " 灵石    主器祭炼耗费 " + WorldRegionLibrary.GetAttackUpgradeCost(saveData) + " 灵石";
        if (bagSummaryText != null)
        {
            bagSummaryText.text = InventoryLibrary.BuildBagSummary(saveData, 4);
        }
    }

    private void RefreshNodes()
    {
        for (var i = 0; i < nodeViews.Count; i++)
        {
            var view = nodeViews[i];
            if (view == null)
            {
                continue;
            }

            WorldRegionDefinition region = null;
            for (var regionIndex = 0; regionIndex < regions.Count; regionIndex++)
            {
                if (regions[regionIndex].Id == view.RegionId)
                {
                    region = regions[regionIndex];
                    break;
                }
            }

            if (region == null)
            {
                continue;
            }

            var unlocked = saveData.IsRegionUnlocked(region.Id);
            var cleared = saveData.IsRegionCleared(region.Id);
            var accessible = unlocked && saveData.realmTier >= region.RequiredRealmTier;
            view.Bind(region, regions[selectedRegionIndex].Id == region.Id, unlocked, accessible, cleared, () => SelectRegion(regions.IndexOf(region)));
        }
    }

    private void RefreshDetail()
    {
        var region = regions[selectedRegionIndex];
        var unlocked = saveData.IsRegionUnlocked(region.Id);
        var cleared = saveData.IsRegionCleared(region.Id);
        string reason;
        var accessible = WorldRegionLibrary.CanTravel(saveData, region, out reason);

        regionTitleText.text = region.DisplayName + " · " + region.Subtitle;
        GameSpriteLibrary.BindSpriteOrPlaceholder(
            regionPreviewImage,
            regionPreviewLabelText,
            region.IllustrationImage,
            region.DisplayName,
            new Color(region.AccentColor.r * 0.7f, region.AccentColor.g * 0.7f, region.AccentColor.b * 0.7f, 1f));
        regionBodyText.text = region.Description + "\n\n" +
                              "危险阶：第 " + region.DangerRank + " 等\n" +
                              "历练内容：心障 " + (region.EnemyCount + region.EliteEnemyCount) +
                              " / 灵气 " + region.SpiritNodeCount +
                              " / 灵草 " + region.HerbCount +
                              " / 遗物 " + region.RelicCount + "\n" +
                              "基础奖赏：修为 +" + region.ClearQiReward + " / 灵石 +" + region.ClearCrystalReward + "\n\n" +
                              CultivationLoadoutLibrary.BuildEquipmentOverview(saveData);

        if (!unlocked)
        {
            regionStatusText.text = "状态：路引未明，需要先完成前置地界。";
        }
        else if (cleared && accessible)
        {
            regionStatusText.text = "状态：已肃清，可再次进入刷取资源。";
        }
        else if (accessible)
        {
            regionStatusText.text = "状态：可前往历练。";
        }
        else
        {
            regionStatusText.text = "状态：" + reason;
        }

        var taskContext = CultivationApp.GetActiveTaskContext(saveData);
        if (taskSummaryText != null)
        {
            taskSummaryText.text = taskContext != null ? taskContext.ActiveTaskSummary : "委托：暂无新任务。";
        }

        BindTaskPreview(taskContext);
    }

    private void RefreshButtons()
    {
        var region = regions[selectedRegionIndex];
        string reason;
        travelButton.interactable = WorldRegionLibrary.CanTravel(saveData, region, out reason);
        vitalityUpgradeButton.interactable = saveData.spiritCrystals >= WorldRegionLibrary.GetVitalityUpgradeCost(saveData);
        attackUpgradeButton.interactable = saveData.spiritCrystals >= WorldRegionLibrary.GetAttackUpgradeCost(saveData);
        if (sectResidenceButton != null)
        {
            sectResidenceButton.interactable = saveData.isSectDisciple;
            var label = sectResidenceButton.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = saveData.isSectDisciple ? saveData.sectName + "\n门派驻地" : "散修无门派";
            }
        }

        craftQiButton.interactable = CanCraft("pill_cauldron_upgrade");
        craftBagButton.interactable = CanCraft("talisman_case_upgrade");
        craftVitalityButton.interactable = CanCraft("peiyuan_powder");
        craftAttackButton.interactable = CanCraft("nawu_pouch");
    }

    private void RefreshPanels()
    {
        if (inventoryDetailText != null)
        {
            inventoryDetailText.text = InventoryLibrary.BuildDetailedBagSummary(saveData);
        }

        BindInventoryPreview();

        if (workshopSummaryText != null)
        {
            workshopSummaryText.text = CultivationApp.BuildSettlementSummary(saveData);
        }

        BindWorkshopPreview();
        RefreshSectPanel();

        if (craftQiButton != null)
        {
            UpdateRecipeButton(craftQiButton, "pill_cauldron_upgrade");
        }

        if (craftBagButton != null)
        {
            UpdateRecipeButton(craftBagButton, "talisman_case_upgrade");
        }

        if (craftVitalityButton != null)
        {
            UpdateRecipeButton(craftVitalityButton, "peiyuan_powder");
        }

        if (craftAttackButton != null)
        {
            UpdateRecipeButton(craftAttackButton, "nawu_pouch");
        }
    }

    private void RefreshSectPanel()
    {
        if (sectPanel == null)
        {
            return;
        }

        sectHallSnapshots = CultivationApp.GetSectHallSnapshots(saveData);
        if (sectHallSnapshots == null)
        {
            sectHallSnapshots = new SectHallSnapshot[0];
        }

        if (sectHallSnapshots.Length == 0)
        {
            if (sectTitleText != null)
            {
                sectTitleText.text = "宗门";
            }

            if (sectDescriptionText != null)
            {
                sectDescriptionText.text = "宗门尚未接引。";
            }

            if (sectStatusText != null)
            {
                sectStatusText.text = string.Empty;
            }

            return;
        }

        selectedSectHallIndex = Mathf.Clamp(selectedSectHallIndex, 0, sectHallSnapshots.Length - 1);
        if (sectHallButtons == null)
        {
            sectHallButtons = new Button[0];
        }

        if (sectActionButtons == null)
        {
            sectActionButtons = new Button[0];
        }

        for (var i = 0; i < sectHallButtons.Length; i++)
        {
            var button = sectHallButtons[i];
            if (button == null)
            {
                continue;
            }

            var hasHall = i < sectHallSnapshots.Length && sectHallSnapshots[i] != null && sectHallSnapshots[i].Definition != null;
            button.gameObject.SetActive(hasHall);
            if (!hasHall)
            {
                continue;
            }

            var capturedIndex = i;
            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = (i == selectedSectHallIndex ? "【" : string.Empty) +
                             sectHallSnapshots[i].Definition.DisplayName +
                             (i == selectedSectHallIndex ? "】" : string.Empty);
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectSectHall(capturedIndex));
        }

        var current = sectHallSnapshots[selectedSectHallIndex];
        var definition = current.Definition;
        if (sectTitleText != null)
        {
            sectTitleText.text = definition.DisplayName + " · " + definition.Subtitle;
        }

        if (sectDescriptionText != null)
        {
            sectDescriptionText.text = definition.Description + "\n\n" + CultivationApp.BuildSectOverview(saveData);
        }

        if (sectStatusText != null)
        {
            sectStatusText.text = current.StatusSummary;
        }

        GameSpriteLibrary.BindSpriteOrPlaceholder(
            sectPreviewImage,
            sectPreviewLabelText,
            definition.IllustrationImage,
            definition.DisplayName + "占位图",
            definition.PlaceholderColor);

        var actions = current.Actions ?? new SectActionSnapshot[0];
        for (var i = 0; i < sectActionButtons.Length; i++)
        {
            var button = sectActionButtons[i];
            if (button == null)
            {
                continue;
            }

            var hasAction = i < actions.Length && actions[i] != null && actions[i].Definition != null;
            button.gameObject.SetActive(hasAction);
            if (!hasAction)
            {
                continue;
            }

            var action = actions[i];
            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = action.ButtonLabel;
            }

            var capturedActionId = action.Definition.Id;
            button.interactable = action.IsAvailable;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ExecuteSectAction(capturedActionId));
        }
    }

    private void SelectSectHall(int index)
    {
        selectedSectHallIndex = index;
        RefreshSectPanel();
    }

    private void ExecuteSectAction(string actionId)
    {
        var result = CultivationApp.ExecuteSectAction(currentSlotIndex, saveData, actionId);
        RefreshAll();
        RefreshPanels();
        SetHint(result != null ? result.Message : "宗门事务没有返回结果。");
    }

    private void UpdateRecipeButton(Button button, string recipeId)
    {
        var label = button != null ? button.GetComponentInChildren<Text>() : null;
        if (label != null)
        {
            label.text = WorkshopLibrary.BuildRecipeButtonLabel(saveData, recipeId);
        }
    }

    private bool CanCraft(string recipeId)
    {
        WorkshopRecipeDefinition[] recipes = WorkshopLibrary.GetRecipes();
        for (var i = 0; i < recipes.Length; i++)
        {
            if (recipes[i].Id != recipeId)
            {
                continue;
            }

            if (recipes[i].CostItems == null)
            {
                return true;
            }

            for (var itemIndex = 0; itemIndex < recipes[i].CostItems.Length; itemIndex++)
            {
                if (saveData.GetItemCount(recipes[i].CostItems[itemIndex].itemId) < recipes[i].CostItems[itemIndex].quantity)
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    private void SetHint(string message)
    {
        hintText.text = "山海录 / " + message;
    }

    private void BindInventoryPreview()
    {
        if (inventoryPreviewImage == null && inventoryPreviewLabelText == null)
        {
            return;
        }

        string itemId = null;
        for (var i = 0; i < saveData.storageItems.Length; i++)
        {
            var stack = saveData.storageItems[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            itemId = stack.itemId;
            break;
        }

        var label = string.IsNullOrWhiteSpace(itemId) ? "储物袋占位图" : InventoryLibrary.GetDisplayName(itemId);
        GameSpriteLibrary.BindSpriteOrPlaceholder(
            inventoryPreviewImage,
            inventoryPreviewLabelText,
            string.IsNullOrWhiteSpace(itemId) ? null : InventoryLibrary.GetArtwork(itemId),
            label,
            new Color(0.22f, 0.18f, 0.14f, 1f));
    }

    private void BindTaskPreview(TaskContextSnapshot taskContext)
    {
        if (taskPreviewImage == null && taskPreviewLabelText == null)
        {
            return;
        }

        GameSpriteLibrary.BindSpriteOrPlaceholder(
            taskPreviewImage,
            taskPreviewLabelText,
            taskContext != null ? taskContext.IllustrationImage : null,
            taskContext != null && !string.IsNullOrWhiteSpace(taskContext.ActiveTaskTitle) ? taskContext.ActiveTaskTitle : "当前委托占位图",
            new Color(0.19f, 0.17f, 0.13f, 1f));
    }

    private void BindWorkshopPreview()
    {
        if (workshopPreviewImage == null && workshopPreviewLabelText == null)
        {
            return;
        }

        var recipes = WorkshopLibrary.GetRecipes();
        var recipe = recipes != null && recipes.Length > 0 ? recipes[0] : null;
        GameSpriteLibrary.BindSpriteOrPlaceholder(
            workshopPreviewImage,
            workshopPreviewLabelText,
            recipe != null ? recipe.IllustrationImage : null,
            recipe != null ? recipe.Title : "洞府整备占位图",
            new Color(0.18f, 0.2f, 0.16f, 1f));
    }
}
