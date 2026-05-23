using UnityEngine;

public sealed partial class WorldMapController
{
    public void TravelToSelectedRegion()
    {
        if (!EnsureValidSelectedRegionSelection())
        {
            SetHint("当前没有可前往的地域数据。");
            ShowWarningMessage("当前没有可前往的地域数据。");
            return;
        }

        var region = regions[selectedRegionIndex];
        var result = TravelToRegion(currentSlotIndex, saveData, region);
        if (!result.Succeeded)
        {
            SetHint(result.Message);
            ShowWarningMessage(result.Message);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            SetHint("场景未加入 Build Settings: " + gameplaySceneName);
            ShowErrorMessage("场景未加入 Build Settings: " + gameplaySceneName);
            return;
        }

        CloseFloatingPanels();
        SceneFlow.RequestScene(gameplaySceneName);
    }

    public void UpgradeVitality()
    {
        var result = UpgradeProtectiveRelic(currentSlotIndex, saveData);
        RefreshAll();
        SetHint(result.Message);
        if (result.Succeeded)
        {
            ShowSuccessMessage(result.Message);
        }
        else
        {
            ShowWarningMessage(result.Message);
        }
    }

    public void UpgradeAttack()
    {
        var result = UpgradeMainArtifact(currentSlotIndex, saveData);
        RefreshAll();
        SetHint(result.Message);
        if (result.Succeeded)
        {
            ShowSuccessMessage(result.Message);
        }
        else
        {
            ShowWarningMessage(result.Message);
        }
    }

    public void OpenInventory()
    {
        CloseGameUiPanel(GameUiPanelId.WorldMapWorkshop);
        OpenCompendium();
    }

    public void OpenWorldMapHome()
    {
        CloseGameUiPanel(GameUiPanelId.WorldMapRegion);
        CloseGameUiPanel(GameUiPanelId.WorldMapSettlement);
        CloseGameUiPanel(GameUiPanelId.WorldMapSectResidence);
        CloseGameUiPanel(GameUiPanelId.WorldMapNpcDialogue);
        CloseInventory();
        CloseWorkshop();
        SetHudContext(GameHubContext.WorldMap);
        EnsureHudPanel();
        ShowGameUiPanel(GameUiPanelId.WorldMap);
        ShowGameUiPanel(GameUiPanelId.GameHub);
        RefreshAll();
        SetHint("已返回山海大地图。");
    }

    public void CloseInventory()
    {
        SetPlayerCompendiumVisible(false);
        CloseGameUiPanel(GameUiPanelId.PlayerCompendium);
        CloseGameUiPanel(GameUiPanelId.WorldMapInventory);
    }

    public void OpenSettlement()
    {
        CloseInventory();
        CloseGameUiPanel(GameUiPanelId.WorldMapRegion);
        CloseGameUiPanel(GameUiPanelId.WorldMapSectResidence);
        CloseGameUiPanel(GameUiPanelId.WorldMapNpcDialogue);
        var panel = OpenGameUiPanel(GameUiPanelId.WorldMapSettlement, new WorldMapSettlementPanelData(this));
        if (panel == null)
        {
            SetHint("城镇整备面板 prefab 缺失，请先重新生成 WorldMap UI Prefabs。");
            ShowErrorMessage("城镇整备面板 prefab 缺失，请先重新生成 WorldMap UI Prefabs。");
            return;
        }

        SetHudContext(GameHubContext.Settlement);
        RefreshHudPanel();
        SetHint("已进入整备区域，可处理储物、炼制与法器养成。");
    }

    public void CloseSettlement()
    {
        CloseGameUiPanel(GameUiPanelId.WorldMapSettlement);
        OpenWorldMapHome();
        SetHint("已离开整备区域，回到山海大地图。");
    }

    public void OpenWorkshopWorkbench()
    {
        CloseGameUiPanel(GameUiPanelId.WorldMapRegion);
        CloseGameUiPanel(GameUiPanelId.PlayerCompendium);
        CloseGameUiPanel(GameUiPanelId.WorldMapInventory);
        CloseGameUiPanel(GameUiPanelId.WorldMapNpcDialogue);
        if (OpenGameUiPanel(GameUiPanelId.WorldMapWorkshop, new WorldMapWorkshopPanelData(this)) == null)
        {
            SetHint("洞府整备面板 prefab 缺失，请先重新生成 WorldMap UI Prefabs。");
            ShowErrorMessage("洞府整备面板 prefab 缺失，请先重新生成 WorldMap UI Prefabs。");
            return;
        }

        SetHint("洞府整备已展开。主法器与护身法器可直接耗灵石精修，丹炉和符匣则依赖材料拓展。");
    }

    public void CloseWorkshop()
    {
        CloseGameUiPanel(GameUiPanelId.WorldMapWorkshop);
    }

    public void OpenSectResidence(bool persistState)
    {
        if (saveData == null || !saveData.isSectDisciple)
        {
            SetHint("散修无固定山门驻地，只能在大地图中游历寻机缘。");
            ShowInfoMessage("散修无固定山门驻地，只能在大地图中游历寻机缘。");
            return;
        }

        if (persistState)
        {
            saveData.isInSectResidence = true;
            saveData.location = saveData.sectName;
            SaveArchive(currentSlotIndex, saveData);
            RefreshAll();
        }

        CloseInventory();
        CloseWorkshop();
        CloseGameUiPanel(GameUiPanelId.WorldMapNpcDialogue);
        CloseGameUiPanel(GameUiPanelId.WorldMapSettlement);
        CloseGameUiPanel(GameUiPanelId.WorldMap);
        CloseRegionPage();
        var panel = OpenGameUiPanel(GameUiPanelId.WorldMapSectResidence, new WorldMapSectResidencePanelData(this));
        if (panel == null)
        {
            SetHint("门派驻地面板 prefab 缺失，请先重新生成 WorldMap UI Prefabs。");
            ShowErrorMessage("门派驻地面板 prefab 缺失，请先重新生成 WorldMap UI Prefabs。");
            return;
        }

        SetHudContext(GameHubContext.SectResidence);
        RefreshHudPanel();
        SetHint("已回到" + saveData.sectName + "。自己的洞府和各殿堂都在山门内。");
    }

    public void CloseSect()
    {
        if (saveData != null)
        {
            saveData.isInSectResidence = false;
            saveData.location = WorldRegionLibrary.GetRegionDisplayName(saveData.currentRegionId);
            SaveArchive(currentSlotIndex, saveData);
            RefreshAll();
        }

        CloseGameUiPanel(GameUiPanelId.WorldMapSectResidence);
        CloseGameUiPanel(GameUiPanelId.WorldMapNpcDialogue);
        OpenWorldMapHome();
        SetHint("已离开门派，回到山海大地图。");
    }

    public void OpenRegionPage(int index)
    {
        if (!HasRegions())
        {
            SetHint("当前没有可展示的地域数据。");
            ShowWarningMessage("当前没有可展示的地域数据。");
            return;
        }

        CloseInventory();
        CloseWorkshop();
        CloseGameUiPanel(GameUiPanelId.WorldMapSettlement);
        CloseGameUiPanel(GameUiPanelId.WorldMapSectResidence);
        CloseGameUiPanel(GameUiPanelId.WorldMapNpcDialogue);

        selectedRegionIndex = Mathf.Clamp(index, 0, regions.Count - 1);
        EnsureValidSelectedRegionSelection();
        RefreshAll();

        var region = regions[selectedRegionIndex];
        var panel = OpenGameUiPanel(GameUiPanelId.WorldMapRegion, new WorldMapRegionPanelData(this, region.Id));
        if (panel == null)
        {
            SetHint("地域详情面板 prefab 缺失，请先重新生成 WorldMap UI Prefabs。");
            ShowErrorMessage("地域详情面板 prefab 缺失，请先重新生成 WorldMap UI Prefabs。");
            return;
        }

        SetHudContext(GameHubContext.Region);
        RefreshHudPanel();
        SetHint("已展开 " + region.DisplayName + " 的全屏情报页。");
    }

    public void CloseRegionPage()
    {
        CloseGameUiPanel(GameUiPanelId.WorldMapRegion);
        CloseGameUiPanel(GameUiPanelId.WorldMapNpcDialogue);
        ShowGameUiPanel(GameUiPanelId.WorldMap);
        SetDetailPanelVisible(false);
        RefreshAll();
    }

    public void CraftRecipe(string recipeId)
    {
        var result = CraftWorldMapRecipe(currentSlotIndex, saveData, recipeId);
        RefreshAll();
        SetHint(result.Message);
        if (result.Succeeded)
        {
            ShowSuccessMessage(result.Message);
        }
        else
        {
            ShowWarningMessage(result.Message);
        }
    }

    public void SelectSectHall(int index)
    {
        selectedSectHallIndex = index;
        RefreshOpenPanels();
    }

    public void ExecuteSectAction(string actionId)
    {
        var result = ExecuteSectAction(currentSlotIndex, saveData, actionId);
        RefreshAll();
        var message = result != null ? result.Message : "宗门事务没有返回结果。";
        SetHint(message);
        if (result == null)
        {
            ShowWarningMessage(message);
            return;
        }

        if (result.Succeeded)
        {
            ShowSuccessMessage(message);
        }
        else
        {
            ShowWarningMessage(message);
        }
    }

    public void OpenSettlementDialogue()
    {
        selectedNpcId = string.Empty;
        OpenNpcDialoguePanel(NpcSceneType.Settlement, string.Empty, string.Empty);
    }

    public void OpenSectDialogue()
    {
        selectedNpcId = string.Empty;
        OpenNpcDialoguePanel(NpcSceneType.SectResidence, string.Empty, GetSelectedSectHallId());
    }

    public void OpenRegionDialogue(string regionId)
    {
        selectedNpcId = string.Empty;
        OpenNpcDialoguePanel(NpcSceneType.Region, regionId, string.Empty);
    }

    public void SelectNpc(string npcId)
    {
        selectedNpcId = npcId ?? string.Empty;
        RefreshOpenPanels();
    }

    public void CloseNpcDialogue()
    {
        CloseGameUiPanel(GameUiPanelId.WorldMapNpcDialogue);
    }

    public WorldMapNpcDialogueSnapshot BuildNpcDialogueSnapshot(NpcSceneType sceneType, string regionId, string sectHallId)
    {
        return base.BuildNpcDialogueSnapshot(saveData, sceneType, regionId, sectHallId, selectedNpcId);
    }

    public void ExecuteNpcDialogueChoice(NpcSceneType sceneType, string regionId, string sectHallId, string npcId, string choiceId)
    {
        var result = base.ExecuteNpcDialogueChoice(currentSlotIndex, saveData, sceneType, regionId, sectHallId, npcId, choiceId);
        if (result != null)
        {
            selectedNpcId = result.SelectedNpcId;
        }

        RefreshAll();
        RefreshOpenPanels();
        var message = result != null ? result.Message : "人物交谈没有返回结果。";
        SetHint(message);
        if (result != null && result.Succeeded)
        {
            ShowSuccessMessage(message);
            return;
        }

        ShowWarningMessage(message);
    }

    private bool CanCraft(string recipeId)
    {
        WorkshopRecipeDefinition[] recipes = WorkshopLibrary.GetRecipes();
        if (recipes == null)
        {
            return false;
        }

        for (var i = 0; i < recipes.Length; i++)
        {
            if (recipes[i] == null || recipes[i].Id != recipeId)
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

    private void CloseFloatingPanels()
    {
        SetHubState(false, GameHubContext.WorldMap);
        SetPlayerCompendiumVisible(false);
        CloseGameUiPanel(GameUiPanelId.GameHub);
        CloseGameUiPanel(GameUiPanelId.PlayerCompendium);
        CloseGameUiPanel(GameUiPanelId.WorldMapRegion);
        CloseGameUiPanel(GameUiPanelId.WorldMapSettlement);
        CloseInventory();
        CloseWorkshop();
        CloseGameUiPanel(GameUiPanelId.WorldMapSectResidence);
        CloseGameUiPanel(GameUiPanelId.WorldMapNpcDialogue);
    }

    private void OpenNpcDialoguePanel(NpcSceneType sceneType, string regionId, string sectHallId)
    {
        CloseGameUiPanel(GameUiPanelId.WorldMapInventory);
        CloseGameUiPanel(GameUiPanelId.WorldMapWorkshop);
        var panel = OpenGameUiPanel(GameUiPanelId.WorldMapNpcDialogue, new WorldMapNpcDialoguePanelData(this, sceneType, regionId, sectHallId));
        if (panel == null)
        {
            SetHint("人物对话面板 prefab 缺失，请先重新生成 WorldMap UI Prefabs。");
            ShowErrorMessage("人物对话面板 prefab 缺失，请先重新生成 WorldMap UI Prefabs。");
            return;
        }

        RefreshOpenPanels();
        SetHint("已打开人物交谈界面。");
    }

    private string GetSelectedSectHallId()
    {
        if (sectHallSnapshots == null || sectHallSnapshots.Length == 0)
        {
            return string.Empty;
        }

        var index = Mathf.Clamp(selectedSectHallIndex, 0, sectHallSnapshots.Length - 1);
        var snapshot = sectHallSnapshots[index];
        return snapshot != null && snapshot.Definition != null ? snapshot.Definition.Id : string.Empty;
    }
}
