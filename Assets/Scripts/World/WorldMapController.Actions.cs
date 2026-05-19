using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed partial class WorldMapController
{
    private void TravelToSelectedRegion()
    {
        if (!EnsureValidSelectedRegionSelection())
        {
            SetHint("当前没有可前往的地域数据。");
            return;
        }

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

        SceneFlow.RequestScene(gameplaySceneName);
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

    private void OpenInventory()
    {
        ChangePrimaryState(WorldMapPrimaryState.Map);
        ChangeModalState(WorldMapModalState.Inventory);
        SetHint("储物袋已展开，可在此查看历练带回的灵材与凭证。");
    }

    private void CloseInventory()
    {
        ChangeModalState(WorldMapModalState.None);
    }

    private void OpenWorkshop()
    {
        ChangePrimaryState(WorldMapPrimaryState.Map);
        ChangeModalState(WorldMapModalState.Workshop);
        SetHint("洞府整备已展开。主法器与护身法器可直接耗灵石精修，丹炉和符匣则依赖材料拓展。");
    }

    private void CloseWorkshop()
    {
        ChangeModalState(WorldMapModalState.None);
    }

    private void OpenSectResidence(bool persistState)
    {
        if (saveData == null || !saveData.isSectDisciple)
        {
            SetHint("散修无固定山门驻地，只能在大地图中游历寻机缘。");
            return;
        }

        if (persistState)
        {
            saveData.isInSectResidence = true;
            saveData.location = saveData.sectName;
            CultivationApp.SaveArchive(currentSlotIndex, saveData);
            RefreshAll();
        }

        ChangePrimaryState(WorldMapPrimaryState.SectResidence);
        SetHint("已回到" + saveData.sectName + "。自己的洞府和各殿堂都在山门内。");
    }

    private void CloseSect()
    {
        if (saveData != null)
        {
            saveData.isInSectResidence = false;
            saveData.location = WorldRegionLibrary.GetRegionDisplayName(saveData.currentRegionId);
            CultivationApp.SaveArchive(currentSlotIndex, saveData);
            RefreshAll();
        }

        ChangePrimaryState(WorldMapPrimaryState.Map);
        SetHint("已离开门派，回到山海大地图。");
    }

    private void CraftRecipe(string recipeId)
    {
        var result = CultivationApp.CraftRecipe(currentSlotIndex, saveData, recipeId);
        RefreshAll();
        RefreshPanels();
        SetHint(result.Message);
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
}
