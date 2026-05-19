using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed partial class WorldMapController
{
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
        var regionCount = regions != null ? regions.Count : 0;
        titleText.text = "九州山海图";
        heroSummaryText.text = saveData.heroName + " / " + saveData.archetypeName + "\n" +
                               saveData.realm + " · " + saveData.sectName + " · 当前驻足 " + saveData.location + "\n" +
                               "已肃清地界：" + saveData.clearedRegionIds.Length + " / " + regionCount;

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
        var hasSelectedRegion = EnsureValidSelectedRegionSelection();
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
            view.Bind(region, hasSelectedRegion && regions[selectedRegionIndex].Id == region.Id, unlocked, accessible, cleared, () => SelectRegion(regions.IndexOf(region)));
        }
    }

    private void RefreshDetail()
    {
        if (!EnsureValidSelectedRegionSelection())
        {
            if (regionTitleText != null)
            {
                regionTitleText.text = "暂无地域数据";
            }

            if (regionBodyText != null)
            {
                regionBodyText.text = "当前没有可用的大地图地域定义。请检查 `WorldRegionDatabase` 或 fallback 数据是否可用。";
            }

            if (regionStatusText != null)
            {
                regionStatusText.text = "状态：地域数据未加载。";
            }

            GameSpriteLibrary.BindSpriteOrPlaceholder(
                regionPreviewImage,
                regionPreviewLabelText,
                null,
                "地域占位图",
                new Color(0.18f, 0.18f, 0.2f, 1f));
            return;
        }

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
        if (!EnsureValidSelectedRegionSelection())
        {
            if (travelButton != null)
            {
                travelButton.interactable = false;
            }

            if (vitalityUpgradeButton != null)
            {
                vitalityUpgradeButton.interactable = saveData.spiritCrystals >= WorldRegionLibrary.GetVitalityUpgradeCost(saveData);
            }

            if (attackUpgradeButton != null)
            {
                attackUpgradeButton.interactable = saveData.spiritCrystals >= WorldRegionLibrary.GetAttackUpgradeCost(saveData);
            }

            return;
        }

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

            CultivationAudio.BindButton(button, () => SelectSectHall(capturedIndex));
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
            CultivationAudio.BindButton(button, () => ExecuteSectAction(capturedActionId), CultivationButtonSound.Confirm);
        }
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
