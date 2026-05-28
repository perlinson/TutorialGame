using QFramework;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed partial class WorldMapController
{
    private void RefreshAll()
    {
        RefreshHeader();
        RefreshHudPanel();
        RefreshNodes();
        RefreshDetail();
        RefreshButtons();
        RefreshOpenPanels();
    }

    public void RefreshOpenPanels()
    {
        var hudPanel = UIKit.GetPanel<GameHubPanel>();
        if (hudPanel != null)
        {
            hudPanel.RefreshFromOwner();
        }

        var regionPanel = UIKit.GetPanel<WorldMapRegionPanel>();
        if (regionPanel != null)
        {
            regionPanel.RefreshFromOwner();
        }

        var settlementPanel = UIKit.GetPanel<WorldMapSettlementPanel>();
        if (settlementPanel != null)
        {
            settlementPanel.RefreshFromOwner();
        }

        var compendiumPanel = UIKit.GetPanel<PlayerCompendiumPanel>();
        if (compendiumPanel != null)
        {
            compendiumPanel.RefreshFromOwner();
        }

        var inventoryPanel = UIKit.GetPanel<WorldMapInventoryPanel>();
        if (inventoryPanel != null)
        {
            inventoryPanel.RefreshFromOwner();
        }

        var workshopPanel = UIKit.GetPanel<WorldMapWorkshopPanel>();
        if (workshopPanel != null)
        {
            workshopPanel.RefreshFromOwner();
        }

        var sectPanel = UIKit.GetPanel<WorldMapSectResidencePanel>();
        if (sectPanel != null)
        {
            sectPanel.RefreshFromOwner();
        }

        var npcPanel = UIKit.GetPanel<WorldMapNpcDialoguePanel>();
        if (npcPanel != null)
        {
            npcPanel.RefreshFromOwner();
        }
    }

    private void RefreshHeader()
    {
        var regionCount = regions != null ? regions.Count : 0;
        titleText.text = "九州山海图";
        heroSummaryText.text = saveData.heroName + " / " + saveData.archetypeName;
        var nextQi = WorldRegionLibrary.GetQiRequiredForNextRealm(saveData.realmTier);
        resourceSummaryText.text = "修为 " + saveData.qi + (nextQi > 0 ? " / " + nextQi : " / 圆满");
        if (bagSummaryText != null)
        {
            bagSummaryText.text = "已肃清 " + saveData.clearedRegionIds.Length + " / " + regionCount;
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
            // 觅长生风格：所有城镇都显示，仅按状态区分高亮（未解锁灰显但仍可见）
            view.gameObject.SetActive(true);

            var capturedRegion = region;
            view.Bind(region, hasSelectedRegion && regions[selectedRegionIndex].Id == region.Id, unlocked, accessible, cleared, () => BeginTravelToRegion(regions.IndexOf(capturedRegion)));
        }
    }

    private void RefreshDetail()
    {
        // The map root is now a lightweight entry page only.
        // Region details live in the dedicated WorldMapRegionPanel prefab.
        SetDetailPanelVisible(false);
    }

    private void RefreshButtons()
    {
        if (!EnsureValidSelectedRegionSelection())
        {
            SetDetailPanelVisible(false);
            if (travelButton != null)
            {
                travelButton.interactable = false;
            }

            if (vitalityUpgradeButton != null)
            {
                vitalityUpgradeButton.interactable = saveData.wallet.CanAfford(CultivationCurrencySystem.RealmToGrade(saveData.realmTier), WorldRegionLibrary.GetVitalityUpgradeCost(saveData));
            }

            if (attackUpgradeButton != null)
            {
                attackUpgradeButton.interactable = saveData.wallet.CanAfford(CultivationCurrencySystem.RealmToGrade(saveData.realmTier), WorldRegionLibrary.GetAttackUpgradeCost(saveData));
            }

            return;
        }

        var region = regions[selectedRegionIndex];
        string reason;
        travelButton.interactable = WorldRegionLibrary.CanTravel(saveData, region, out reason);
        vitalityUpgradeButton.interactable = saveData.wallet.CanAfford(CultivationCurrencySystem.RealmToGrade(saveData.realmTier), WorldRegionLibrary.GetVitalityUpgradeCost(saveData));
        attackUpgradeButton.interactable = saveData.wallet.CanAfford(CultivationCurrencySystem.RealmToGrade(saveData.realmTier), WorldRegionLibrary.GetAttackUpgradeCost(saveData));
        if (sectResidenceButton != null)
        {
            sectResidenceButton.gameObject.SetActive(saveData.isSectDisciple);
            sectResidenceButton.interactable = saveData.isSectDisciple;
            var label = sectResidenceButton.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = "门";
            }
        }
    }

    public WorldMapInventorySnapshot BuildInventorySnapshot()
    {
        return BuildWorldMapInventorySnapshot(saveData);
    }

    public WorldMapRegionSnapshot BuildRegionSnapshot(string regionId)
    {
        var fallbackRegionId = string.Empty;
        if (EnsureValidSelectedRegionSelection())
        {
            var selectedRegion = regions[selectedRegionIndex];
            fallbackRegionId = selectedRegion != null ? selectedRegion.Id : string.Empty;
        }

        var snapshot = BuildWorldMapRegionSnapshot(saveData, regionId, fallbackRegionId);
        DecorateLocationEntries(snapshot != null ? snapshot.LocationEntries : null, selectedRegionLocationId);
        if (snapshot != null)
        {
            snapshot.Description = AppendFocusedLocationSummary(snapshot.Description, snapshot.LocationEntries, "当前聚焦驻点");
            snapshot.Status = AppendFocusedLocationStatus(snapshot.Status, snapshot.LocationEntries);
        }

        return snapshot;
    }

    public WorldMapWorkshopSnapshot BuildWorkshopSnapshot()
    {
        return BuildWorldMapWorkshopSnapshot(saveData);
    }

    public WorldMapSettlementSnapshot BuildSettlementSnapshot()
    {
        var snapshot = BuildWorldMapSettlementSnapshot(saveData);
        DecorateLocationEntries(snapshot != null ? snapshot.LocationEntries : null, selectedSettlementLocationId);
        if (snapshot != null)
        {
            snapshot.SummaryText = AppendFocusedLocationSummary(snapshot.SummaryText, snapshot.LocationEntries, "当前聚焦分区");
            snapshot.ActionHintText = AppendFocusedLocationStatus(snapshot.ActionHintText, snapshot.LocationEntries);
        }

        return snapshot;
    }

    public WorldMapSectResidenceSnapshot BuildSectResidenceSnapshot()
    {
        var snapshot = BuildWorldMapSectResidenceSnapshot(saveData, selectedSectHallIndex);
        selectedSectHallIndex = snapshot != null ? snapshot.ResolvedSelectedHallIndex : -1;
        selectedSectHallId = snapshot != null ? snapshot.SelectedHallId ?? string.Empty : string.Empty;
        DecorateLocationEntries(snapshot != null ? snapshot.LocationEntries : null, selectedSectLocationId);
        if (snapshot != null)
        {
            snapshot.Description = AppendFocusedLocationSummary(snapshot.Description, snapshot.LocationEntries, "当前聚焦支点");
            snapshot.Status = AppendFocusedLocationStatus(snapshot.Status, snapshot.LocationEntries);
        }

        return snapshot;
    }

    private static void DecorateLocationEntries(WorldMapLocationEntrySnapshot[] entries, string selectedLocationId)
    {
        if (entries == null)
        {
            return;
        }

        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry == null)
            {
                continue;
            }

            entry.IsSelected = !string.IsNullOrWhiteSpace(selectedLocationId) && entry.LocationId == selectedLocationId;
        }
    }

    private static string AppendFocusedLocationSummary(string baseText, WorldMapLocationEntrySnapshot[] entries, string label)
    {
        var entry = FindSelectedLocationEntry(entries);
        if (entry == null)
        {
            return baseText;
        }

        var builder = new System.Text.StringBuilder(baseText ?? string.Empty);
        if (builder.Length > 0)
        {
            builder.Append("\n\n");
        }

        builder.Append(label).Append("：").Append(entry.DisplayName);
        if (!string.IsNullOrWhiteSpace(entry.StatusText))
        {
            builder.Append("\n").Append(entry.StatusText);
        }

        var body = entry.TooltipBody ?? string.Empty;
        var firstBreak = body.IndexOf("\n\n", System.StringComparison.Ordinal);
        var excerpt = firstBreak >= 0 ? body.Substring(0, firstBreak) : body;
        if (!string.IsNullOrWhiteSpace(excerpt))
        {
            builder.Append("\n").Append(excerpt);
        }

        return builder.ToString();
    }

    private static string AppendFocusedLocationStatus(string baseText, WorldMapLocationEntrySnapshot[] entries)
    {
        var entry = FindSelectedLocationEntry(entries);
        if (entry == null)
        {
            return baseText;
        }

        var builder = new System.Text.StringBuilder(baseText ?? string.Empty);
        if (builder.Length > 0)
        {
            builder.Append("\n");
        }

        builder.Append("当前聚焦：").Append(entry.DisplayName);
        if (!string.IsNullOrWhiteSpace(entry.StatusText))
        {
            builder.Append(" / ").Append(entry.StatusText);
        }

        return builder.ToString();
    }

    private static WorldMapLocationEntrySnapshot FindSelectedLocationEntry(WorldMapLocationEntrySnapshot[] entries)
    {
        if (entries == null)
        {
            return null;
        }

        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry != null && entry.IsSelected)
            {
                return entry;
            }
        }

        return null;
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
}
