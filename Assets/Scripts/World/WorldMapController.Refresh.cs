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
        return new WorldMapInventorySnapshot
        {
            DetailText = InventoryLibrary.BuildDetailedBagSummary(saveData),
            Preview = BuildInventoryPreviewSnapshot()
        };
    }

    public WorldMapRegionSnapshot BuildRegionSnapshot(string regionId)
    {
        WorldRegionDefinition region = null;
        if (!string.IsNullOrWhiteSpace(regionId))
        {
            for (var i = 0; i < regions.Count; i++)
            {
                if (regions[i] != null && regions[i].Id == regionId)
                {
                    region = regions[i];
                    break;
                }
            }
        }

        if (region == null && EnsureValidSelectedRegionSelection())
        {
            region = regions[selectedRegionIndex];
        }

        if (region == null)
        {
            return new WorldMapRegionSnapshot
            {
                PanelTitle = "历练地点",
                PanelSubtitle = "地域情报缺失",
                Description = "当前没有可展示的地域数据。",
                Status = "状态：不可进入。",
                TaskSummary = "委托：暂无。",
                TravelButtonLabel = "不可前往",
                VitalityButtonLabel = "温养护身器",
                AttackButtonLabel = "祭炼主法器",
                CanTravel = false,
                CanUpgradeVitality = false,
                CanUpgradeAttack = false,
                Preview = new WorldMapPreviewSnapshot
                {
                    Label = "地域占位图",
                    PlaceholderColor = new Color(0.22f, 0.18f, 0.14f, 1f)
                }
            };
        }

        var unlocked = saveData.IsRegionUnlocked(region.Id);
        var cleared = saveData.IsRegionCleared(region.Id);
        string reason;
        var canTravel = WorldRegionLibrary.CanTravel(saveData, region, out reason);
        var taskContext = GetActiveTaskContext(saveData);
        var vitalityCost = WorldRegionLibrary.GetVitalityUpgradeCost(saveData);
        var attackCost = WorldRegionLibrary.GetAttackUpgradeCost(saveData);

        var description = region.Description + "\n\n" +
                          "危险阶：第 " + region.DangerRank + " 等\n" +
                          "基础奖赏：修为 +" + region.ClearQiReward + " / 灵石 +" + region.ClearCrystalReward + "\n" +
                          "遭遇配置：心障 " + (region.EnemyCount + region.EliteEnemyCount) +
                          " · 灵气 " + region.SpiritNodeCount +
                          " · 灵草 " + region.HerbCount +
                          " · 遗物 " + region.RelicCount + "\n\n" +
                          "当前养成：\n" + CultivationLoadoutLibrary.BuildEquipmentOverview(saveData);

        var status = !unlocked
            ? "状态：路引未明，需要先完成前置地界。"
            : cleared && canTravel
                ? "状态：已肃清，可再次进入刷取资源。"
                : canTravel
                    ? "状态：可前往历练。"
                    : "状态：" + reason;

        return new WorldMapRegionSnapshot
        {
            PanelTitle = region.DisplayName,
            PanelSubtitle = region.Subtitle + " / " + WorldRegionLibrary.GetRealmName(region.RequiredRealmTier),
            Description = description,
            Status = status,
            TaskSummary = taskContext != null
                ? taskContext.ActiveTaskSummary
                : "委托：暂无新任务，可先前往此地探路。",
            TravelButtonLabel = canTravel ? "进入历练" : unlocked ? "暂不可前往" : "尚未探明",
            VitalityButtonLabel = CultivationLoadoutLibrary.GetProtectiveRelicName(saveData.archetypeId, saveData.protectiveRelicLevel) + " +" +
                                  saveData.protectiveRelicLevel + "  升阶-" + vitalityCost,
            AttackButtonLabel = CultivationLoadoutLibrary.GetMainArtifactName(saveData.archetypeId, saveData.mainArtifactLevel) + " +" +
                                saveData.mainArtifactLevel + "  升阶-" + attackCost,
            CanTravel = canTravel,
            CanUpgradeVitality = saveData.spiritCrystals >= vitalityCost,
            CanUpgradeAttack = saveData.spiritCrystals >= attackCost,
            Preview = new WorldMapPreviewSnapshot
            {
                Sprite = region.IllustrationImage,
                Label = region.DisplayName + "占位图",
                PlaceholderColor = new Color(region.AccentColor.r * 0.7f, region.AccentColor.g * 0.7f, region.AccentColor.b * 0.7f, 1f)
            }
        };
    }

    public WorldMapWorkshopSnapshot BuildWorkshopSnapshot()
    {
        return new WorldMapWorkshopSnapshot
        {
            SummaryText = BuildSettlementSummary(saveData),
            Preview = BuildWorkshopPreviewSnapshot(),
            Recipes = new[]
            {
                BuildWorkshopRecipeSnapshot("pill_cauldron_upgrade"),
                BuildWorkshopRecipeSnapshot("talisman_case_upgrade"),
                BuildWorkshopRecipeSnapshot("peiyuan_powder"),
                BuildWorkshopRecipeSnapshot("nawu_pouch")
            }
        };
    }

    public WorldMapSettlementSnapshot BuildSettlementSnapshot()
    {
        if (saveData == null)
        {
            return new WorldMapSettlementSnapshot
            {
                PanelTitle = "整备区域",
                PanelSubtitle = "数据缺失",
                SummaryText = "当前没有可用存档。",
                StatusText = "状态：不可整备。",
                ActionHintText = "请先返回主菜单建立存档。",
                InventoryButtonLabel = "修士总览",
                WorkshopButtonLabel = "打开炼制台",
                VitalityButtonLabel = "温养护身法器",
                AttackButtonLabel = "祭炼主法器",
                Preview = new WorldMapPreviewSnapshot
                {
                    Label = "整备区域占位图",
                    PlaceholderColor = new Color(0.18f, 0.16f, 0.12f, 1f)
                }
            };
        }

        var vitalityCost = WorldRegionLibrary.GetVitalityUpgradeCost(saveData);
        var attackCost = WorldRegionLibrary.GetAttackUpgradeCost(saveData);

        return new WorldMapSettlementSnapshot
        {
            PanelTitle = saveData != null && saveData.isSectDisciple ? "山门外坊市" : "行脚坊市",
            PanelSubtitle = saveData != null && saveData.isSectDisciple
                ? "洞府整备 / 储物 / 炼制 / 法器养成"
                : "路边坊市 / 储物 / 炼制 / 行前整备",
            SummaryText = BuildSettlementSummary(saveData) + "\n\n" +
                          CultivationLoadoutLibrary.BuildEquipmentOverview(saveData),
            StatusText = "灵石：" + saveData.spiritCrystals +
                         "    储物袋：" + saveData.GetUsedBagSlots() + " / " + saveData.bagCapacity +
                         "\n当前所在：" + saveData.location,
            ActionHintText = "可先查看修士总览，再处理炼制、主法器祭炼与护身法器温养。",
            InventoryButtonLabel = "修士总览",
            WorkshopButtonLabel = "打开炼制台",
            VitalityButtonLabel = "温养护身法器 · " + vitalityCost + " 灵石",
            AttackButtonLabel = "祭炼主法器 · " + attackCost + " 灵石",
            CanUpgradeVitality = saveData.spiritCrystals >= vitalityCost,
            CanUpgradeAttack = saveData.spiritCrystals >= attackCost,
            Preview = BuildWorkshopPreviewSnapshot()
        };
    }

    public WorldMapSectResidenceSnapshot BuildSectResidenceSnapshot()
    {
        var hallSnapshots = GetSectHallSnapshots(saveData) ?? new SectHallSnapshot[0];
        sectHallSnapshots = hallSnapshots;

        var hallButtons = new WorldMapSectHallButtonSnapshot[hallSnapshots.Length];
        for (var i = 0; i < hallSnapshots.Length; i++)
        {
            var hall = hallSnapshots[i];
            if (hall == null || hall.Definition == null)
            {
                hallButtons[i] = new WorldMapSectHallButtonSnapshot { DisplayName = string.Empty, IsSelected = false };
                continue;
            }

            hallButtons[i] = new WorldMapSectHallButtonSnapshot
            {
                DisplayName = hall.Definition.DisplayName,
                IsSelected = i == Mathf.Clamp(selectedSectHallIndex, 0, Mathf.Max(0, hallSnapshots.Length - 1))
            };
        }

        if (hallSnapshots.Length == 0)
        {
            return new WorldMapSectResidenceSnapshot
            {
                PanelTitle = string.IsNullOrWhiteSpace(saveData.sectName) ? "宗门驻地" : saveData.sectName,
                PanelSubtitle = "门派驻地 / 殿堂事务 / 洞府整备",
                HallTitle = "宗门",
                Description = "宗门尚未接引。",
                Status = string.Empty,
                Preview = new WorldMapPreviewSnapshot
                {
                    Sprite = null,
                    Label = "宗门占位图",
                    PlaceholderColor = new Color(0.24f, 0.18f, 0.12f, 1f)
                },
                HallButtons = hallButtons,
                ActionButtons = new WorldMapSectActionButtonSnapshot[0]
            };
        }

        selectedSectHallIndex = Mathf.Clamp(selectedSectHallIndex, 0, hallSnapshots.Length - 1);
        var current = hallSnapshots[selectedSectHallIndex];
        if (current == null || current.Definition == null)
        {
            return new WorldMapSectResidenceSnapshot
            {
                PanelTitle = string.IsNullOrWhiteSpace(saveData.sectName) ? "宗门驻地" : saveData.sectName,
                PanelSubtitle = "门派驻地 / 殿堂事务 / 洞府整备",
                HallTitle = "宗门",
                Description = "宗门数据异常。",
                Status = string.Empty,
                Preview = new WorldMapPreviewSnapshot
                {
                    Sprite = null,
                    Label = "宗门占位图",
                    PlaceholderColor = new Color(0.24f, 0.18f, 0.12f, 1f)
                },
                HallButtons = hallButtons,
                ActionButtons = new WorldMapSectActionButtonSnapshot[0]
            };
        }

        var definition = current.Definition;
        var actions = current.Actions ?? new SectActionSnapshot[0];
        var actionButtons = new WorldMapSectActionButtonSnapshot[actions.Length];
        for (var i = 0; i < actions.Length; i++)
        {
            var action = actions[i];
            actionButtons[i] = new WorldMapSectActionButtonSnapshot
            {
                ActionId = action.Definition != null ? action.Definition.Id : string.Empty,
                ButtonLabel = action.ButtonLabel,
                IsVisible = action.Definition != null,
                IsInteractable = action.IsAvailable,
                TooltipTitle = action.Definition != null ? action.Definition.Title : string.Empty,
                TooltipBody = action.Definition != null
                    ? action.Definition.Description + (action.IsAvailable || string.IsNullOrWhiteSpace(action.UnavailableReason) ? string.Empty : "\n\n" + action.UnavailableReason)
                    : string.Empty
            };
        }

        return new WorldMapSectResidenceSnapshot
        {
            PanelTitle = string.IsNullOrWhiteSpace(saveData.sectName) ? "宗门驻地" : saveData.sectName,
            PanelSubtitle = "门派驻地 / 殿堂事务 / 洞府整备",
            HallTitle = definition.DisplayName + " · " + definition.Subtitle,
            Description = definition.Description + "\n\n" + BuildSectOverview(saveData),
            Status = current.StatusSummary,
            Preview = new WorldMapPreviewSnapshot
            {
                Sprite = definition.IllustrationImage,
                Label = definition.DisplayName + "占位图",
                PlaceholderColor = definition.PlaceholderColor
            },
            HallButtons = hallButtons,
            ActionButtons = actionButtons
        };
    }

    private WorldMapWorkshopRecipeSnapshot BuildWorkshopRecipeSnapshot(string recipeId)
    {
        WorkshopRecipeDefinition[] recipes = WorkshopLibrary.GetRecipes();
        if (recipes == null)
        {
            return new WorldMapWorkshopRecipeSnapshot
            {
                RecipeId = recipeId,
                ButtonLabel = recipeId,
                IsInteractable = false,
                TooltipTitle = recipeId,
                TooltipBody = "未找到对应配方。"
            };
        }

        for (var i = 0; i < recipes.Length; i++)
        {
            var recipe = recipes[i];
            if (recipe == null || recipe.Id != recipeId)
            {
                continue;
            }

            return new WorldMapWorkshopRecipeSnapshot
            {
                RecipeId = recipeId,
                ButtonLabel = WorkshopLibrary.BuildRecipeButtonLabel(saveData, recipeId),
                IsInteractable = CanCraft(recipeId),
                TooltipTitle = recipe.Title,
                TooltipBody = recipe.Description + "\n\n" + WorkshopLibrary.BuildRecipeButtonLabel(saveData, recipeId)
            };
        }

        return new WorldMapWorkshopRecipeSnapshot
        {
            RecipeId = recipeId,
            ButtonLabel = recipeId,
            IsInteractable = false,
            TooltipTitle = recipeId,
            TooltipBody = "未找到对应配方。"
        };
    }

    private WorldMapPreviewSnapshot BuildInventoryPreviewSnapshot()
    {
        string itemId = null;
        var items = saveData != null ? saveData.storageItems : null;
        for (var i = 0; items != null && i < items.Length; i++)
        {
            var stack = items[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            itemId = stack.itemId;
            break;
        }

        return new WorldMapPreviewSnapshot
        {
            Sprite = string.IsNullOrWhiteSpace(itemId) ? null : InventoryLibrary.GetArtwork(itemId),
            Label = string.IsNullOrWhiteSpace(itemId) ? "储物袋占位图" : InventoryLibrary.GetDisplayName(itemId),
            PlaceholderColor = new Color(0.22f, 0.18f, 0.14f, 1f)
        };
    }

    private WorldMapPreviewSnapshot BuildWorkshopPreviewSnapshot()
    {
        var recipes = WorkshopLibrary.GetRecipes();
        var recipe = recipes != null && recipes.Length > 0 ? recipes[0] : null;
        return new WorldMapPreviewSnapshot
        {
            Sprite = recipe != null ? recipe.IllustrationImage : null,
            Label = recipe != null ? recipe.Title : "洞府整备占位图",
            PlaceholderColor = new Color(0.18f, 0.2f, 0.16f, 1f)
        };
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
