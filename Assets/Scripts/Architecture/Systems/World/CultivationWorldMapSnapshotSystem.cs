using QFramework;
using UnityEngine;

public sealed class CultivationWorldMapSnapshotSystem : AbstractSystem
{
    private static readonly string[] WorkshopRecipeIds =
    {
        "pill_cauldron_upgrade",
        "talisman_case_upgrade",
        "peiyuan_powder",
        "nawu_pouch"
    };

    private CultivationTaskSystem taskSystem;
    private CultivationSettlementSystem settlementSystem;
    private CultivationSectSystem sectSystem;
    private CultivationCurrencySystem currencySystem;
    private CultivationTradeSystem tradeSystem;
    private CultivationWorldGenerationSystem worldGenerationSystem;
    private CultivationWorldIncidentSystem worldIncidentSystem;

    protected override void OnInit()
    {
        taskSystem = this.GetSystem<CultivationTaskSystem>();
        settlementSystem = this.GetSystem<CultivationSettlementSystem>();
        sectSystem = this.GetSystem<CultivationSectSystem>();
        currencySystem = this.GetSystem<CultivationCurrencySystem>();
        tradeSystem = this.GetSystem<CultivationTradeSystem>();
        worldGenerationSystem = this.GetSystem<CultivationWorldGenerationSystem>();
        worldIncidentSystem = this.GetSystem<CultivationWorldIncidentSystem>();
    }

    private void EnsureDependencies()
    {
        taskSystem ??= this.GetSystem<CultivationTaskSystem>();
        settlementSystem ??= this.GetSystem<CultivationSettlementSystem>();
        sectSystem ??= this.GetSystem<CultivationSectSystem>();
        currencySystem ??= this.GetSystem<CultivationCurrencySystem>();
        tradeSystem ??= this.GetSystem<CultivationTradeSystem>();
        worldGenerationSystem ??= this.GetSystem<CultivationWorldGenerationSystem>();
        worldIncidentSystem ??= this.GetSystem<CultivationWorldIncidentSystem>();
    }

    public WorldMapRegionSnapshot BuildRegionSnapshot(CultivationSaveData saveData, string regionId, string fallbackRegionId)
    {
        EnsureDependencies();

        if (saveData == null)
        {
            return BuildMissingRegionSnapshot();
        }

        saveData.EnsureDefaults();

        var region = ResolveRegion(regionId, fallbackRegionId);
        if (region == null)
        {
            return BuildMissingRegionSnapshot();
        }

        var unlocked = saveData.IsRegionUnlocked(region.Id);
        var cleared = saveData.IsRegionCleared(region.Id);
        string reason;
        var canTravel = WorldRegionLibrary.CanTravel(saveData, region, out reason);
        var taskContext = taskSystem.GetActiveTaskContext(saveData);
        var vitalityCost = WorldRegionLibrary.GetVitalityUpgradeCost(saveData);
        var attackCost = WorldRegionLibrary.GetAttackUpgradeCost(saveData);
        var locationDigest = worldGenerationSystem != null ? worldGenerationSystem.BuildLocationDigest(saveData, region.Id, NpcSceneType.Region) : string.Empty;
        var incidents = worldIncidentSystem != null ? worldIncidentSystem.GetIncidentsForParent(saveData, region.Id, NpcSceneType.Region) : new WorldIncidentData[0];
        var locationEntries = BuildLocationEntries(saveData, region.Id, NpcSceneType.Region);

        var description = region.Description + "\n\n" +
                          "危险阶：第 " + region.DangerRank + " 等\n" +
                          "基础奖赏：修为 +" + region.ClearQiReward + " / " + CultivationCurrencySystem.GradeName(CultivationCurrencySystem.RealmToGrade(region.RequiredRealmTier)) + " +" + region.ClearCrystalReward + "\n" +
                          "遭遇配置：心障 " + (region.EnemyCount + region.EliteEnemyCount) +
                          " · 灵气 " + region.SpiritNodeCount +
                          " · 灵草 " + region.HerbCount +
                          " · 遗物 " + region.RelicCount + "\n\n" +
                          "当前养成：\n" + CultivationLoadoutLibrary.BuildEquipmentOverview(saveData);
        if (!string.IsNullOrWhiteSpace(locationDigest))
        {
            description += "\n\n" + locationDigest;
        }

        var status = !unlocked
            ? "状态：路引未明，需要先完成前置地界。"
            : cleared && canTravel
                ? "状态：已肃清，可再次进入刷取资源。"
                : canTravel
                    ? "状态：可前往历练。"
                    : "状态：" + reason;
        if (incidents.Length > 0)
        {
            status += "\n当前风闻：" + incidents[0].displayTitle;
        }

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
            DialogueButtonLabel = "进入前沿据点",
            DialogueButtonTooltip = "进入 " + region.DisplayName + " 的前沿据点，查看斥候、线人和地界线索。" +
                                    (string.IsNullOrWhiteSpace(locationDigest) ? string.Empty : "\n\n" + locationDigest),
            VitalityButtonLabel = CultivationLoadoutLibrary.GetProtectiveRelicName(saveData.archetypeId, saveData.protectiveRelicLevel) + " +" +
                                  saveData.protectiveRelicLevel + "  升阶-" + vitalityCost,
            AttackButtonLabel = CultivationLoadoutLibrary.GetMainArtifactName(saveData.archetypeId, saveData.mainArtifactLevel) + " +" +
                                saveData.mainArtifactLevel + "  升阶-" + attackCost,
            CanTravel = canTravel,
            CanUpgradeVitality = currencySystem.CanAfford(saveData, vitalityCost),
            CanUpgradeAttack = currencySystem.CanAfford(saveData, attackCost),
            LocationEntries = locationEntries,
            Preview = new WorldMapPreviewSnapshot
            {
                Sprite = region.IllustrationImage,
                Label = region.DisplayName + "占位图",
                PlaceholderColor = new Color(region.AccentColor.r * 0.7f, region.AccentColor.g * 0.7f, region.AccentColor.b * 0.7f, 1f)
            }
        };
    }

    public WorldMapInventorySnapshot BuildInventorySnapshot(CultivationSaveData saveData)
    {
        EnsureDependencies();

        if (saveData == null)
        {
            return new WorldMapInventorySnapshot
            {
                DetailText = "当前没有可用存档。",
                Preview = new WorldMapPreviewSnapshot
                {
                    Label = "储物袋占位图",
                    PlaceholderColor = new Color(0.22f, 0.18f, 0.14f, 1f)
                }
            };
        }

        saveData.EnsureDefaults();
        return new WorldMapInventorySnapshot
        {
            DetailText = InventoryLibrary.BuildDetailedBagSummary(saveData),
            Preview = BuildInventoryPreviewSnapshot(saveData)
        };
    }

    public WorldMapWorkshopSnapshot BuildWorkshopSnapshot(CultivationSaveData saveData)
    {
        EnsureDependencies();

        if (saveData != null)
        {
            saveData.EnsureDefaults();
        }

        var recipes = new WorldMapWorkshopRecipeSnapshot[WorkshopRecipeIds.Length];
        for (var i = 0; i < WorkshopRecipeIds.Length; i++)
        {
            recipes[i] = BuildWorkshopRecipeSnapshot(saveData, WorkshopRecipeIds[i]);
        }

        return new WorldMapWorkshopSnapshot
        {
            SummaryText = settlementSystem.BuildSettlementSummary(saveData),
            Preview = BuildWorkshopPreviewSnapshot(),
            Recipes = recipes
        };
    }

    public WorldMapSettlementSnapshot BuildSettlementSnapshot(CultivationSaveData saveData)
    {
        EnsureDependencies();

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
                DialogueButtonLabel = "进入坊市",
                DialogueButtonTooltip = "进入当地坊市，查看行商、药师与风闻人物。",
                VitalityButtonLabel = "温养护身法器",
                AttackButtonLabel = "祭炼主法器",
                Preview = new WorldMapPreviewSnapshot
                {
                    Label = "整备区域占位图",
                    PlaceholderColor = new Color(0.18f, 0.16f, 0.12f, 1f)
                }
            };
        }

        saveData.EnsureDefaults();
        var vitalityCost = WorldRegionLibrary.GetVitalityUpgradeCost(saveData);
        var attackCost = WorldRegionLibrary.GetAttackUpgradeCost(saveData);
        var locationDigest = worldGenerationSystem != null ? worldGenerationSystem.BuildLocationDigest(saveData, saveData.currentRegionId, NpcSceneType.Settlement) : string.Empty;
        var incidents = worldIncidentSystem != null ? worldIncidentSystem.GetIncidentsForParent(saveData, saveData.currentRegionId, NpcSceneType.Settlement) : new WorldIncidentData[0];
        var locationEntries = BuildLocationEntries(saveData, saveData.currentRegionId, NpcSceneType.Settlement);

        return new WorldMapSettlementSnapshot
        {
            PanelTitle = saveData.isSectDisciple ? "山门外坊市" : "行脚坊市",
            PanelSubtitle = saveData.isSectDisciple
                ? "洞府整备 / 储物 / 炼制 / 法器养成"
                : "路边坊市 / 储物 / 炼制 / 行前整备",
            SummaryText = settlementSystem.BuildSettlementSummary(saveData) + "\n\n" +
                          CultivationLoadoutLibrary.BuildEquipmentOverview(saveData) +
                          (string.IsNullOrWhiteSpace(locationDigest) ? string.Empty : "\n\n" + locationDigest),
            StatusText = "灵石：" + currencySystem.GetDisplayString(saveData) +
                         "    储物袋：" + saveData.GetUsedBagSlots() + " / " + saveData.bagCapacity +
                         "\n当前所在：" + saveData.location +
                         (incidents.Length > 0 ? "\n当前风闻：" + incidents[0].displayTitle : string.Empty),
            ActionHintText = "可先查看修士总览与整备状态；若要接触人物与风闻，需要再进入坊市内部。" +
                             (string.IsNullOrWhiteSpace(locationDigest) ? string.Empty : "\n" + locationDigest),
            InventoryButtonLabel = "修士总览",
            WorkshopButtonLabel = "打开炼制台",
            DialogueButtonLabel = saveData.isSectDisciple ? "进入山门坊市" : "进入行脚坊市",
            DialogueButtonTooltip = (saveData.isSectDisciple
                ? "进入山门外坊市，接触行商、药师与周边消息。"
                : "进入行脚坊市，接触路途中可遇见的行商、药师与风闻人物。")
                + (string.IsNullOrWhiteSpace(locationDigest) ? string.Empty : "\n\n" + locationDigest),
            VitalityButtonLabel = "温养护身法器 · " + vitalityCost + " " + CultivationCurrencySystem.GradeName(currencySystem.GetPlayerGrade(saveData)),
            AttackButtonLabel = "祭炼主法器 · " + attackCost + " " + CultivationCurrencySystem.GradeName(currencySystem.GetPlayerGrade(saveData)),
            CanUpgradeVitality = currencySystem.CanAfford(saveData, vitalityCost),
            CanUpgradeAttack = currencySystem.CanAfford(saveData, attackCost),
            LocationEntries = locationEntries,
            Preview = BuildWorkshopPreviewSnapshot()
        };
    }

    public WorldMapSectResidenceSnapshot BuildSectResidenceSnapshot(CultivationSaveData saveData, int selectedSectHallIndex)
    {
        EnsureDependencies();

        var panelTitle = saveData != null && !string.IsNullOrWhiteSpace(saveData.sectName) ? saveData.sectName : "宗门驻地";
        if (saveData != null)
        {
            saveData.EnsureDefaults();
        }

        var hallSnapshots = sectSystem.GetHallSnapshots(saveData) ?? new SectHallSnapshot[0];
        var hallButtons = new WorldMapSectHallButtonSnapshot[hallSnapshots.Length];
        var resolvedSelectedHallIndex = hallSnapshots.Length > 0
            ? Mathf.Clamp(selectedSectHallIndex, 0, hallSnapshots.Length - 1)
            : -1;

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
                IsSelected = i == resolvedSelectedHallIndex
            };
        }

        if (hallSnapshots.Length == 0)
        {
            return new WorldMapSectResidenceSnapshot
            {
                PanelTitle = panelTitle,
                PanelSubtitle = "门派驻地 / 殿堂事务 / 洞府整备",
                HallTitle = "宗门",
                Description = "宗门尚未接引。",
                Status = string.Empty,
                DialogueButtonLabel = "进入当前殿堂",
                DialogueButtonTooltip = "先选定一个殿堂，再进入其中查看同门与执事人物。",
                ResolvedSelectedHallIndex = resolvedSelectedHallIndex,
                SelectedHallId = string.Empty,
                Preview = BuildSectPlaceholderPreview(),
                HallButtons = hallButtons,
                ActionButtons = new WorldMapSectActionButtonSnapshot[0]
            };
        }

        var current = hallSnapshots[resolvedSelectedHallIndex];
        if (current == null || current.Definition == null)
        {
            return new WorldMapSectResidenceSnapshot
            {
                PanelTitle = panelTitle,
                PanelSubtitle = "门派驻地 / 殿堂事务 / 洞府整备",
                HallTitle = "宗门",
                Description = "宗门数据异常。",
                Status = string.Empty,
                DialogueButtonLabel = "进入当前殿堂",
                DialogueButtonTooltip = "先选定一个殿堂，再进入其中查看同门与执事人物。",
                ResolvedSelectedHallIndex = resolvedSelectedHallIndex,
                SelectedHallId = string.Empty,
                Preview = BuildSectPlaceholderPreview(),
                HallButtons = hallButtons,
                ActionButtons = new WorldMapSectActionButtonSnapshot[0]
            };
        }

        var definition = current.Definition;
        var actions = current.Actions ?? new SectActionSnapshot[0];
        var locationDigest = worldGenerationSystem != null ? worldGenerationSystem.BuildLocationDigest(saveData, definition.Id, NpcSceneType.SectResidence) : string.Empty;
        var incidents = worldIncidentSystem != null ? worldIncidentSystem.GetIncidentsForParent(saveData, definition.Id, NpcSceneType.SectResidence) : new WorldIncidentData[0];
        var locationEntries = BuildLocationEntries(saveData, definition.Id, NpcSceneType.SectResidence);
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
            PanelTitle = panelTitle,
            PanelSubtitle = "门派驻地 / 殿堂事务 / 洞府整备",
            HallTitle = definition.DisplayName + " · " + definition.Subtitle,
            Description = definition.Description + "\n\n" + sectSystem.BuildSectOverview(saveData) +
                          (string.IsNullOrWhiteSpace(locationDigest) ? string.Empty : "\n\n" + locationDigest),
            Status = current.StatusSummary + (incidents.Length > 0 ? "\n当前风闻：" + incidents[0].displayTitle : string.Empty),
            DialogueButtonLabel = "进入" + definition.DisplayName,
            DialogueButtonTooltip = "进入" + definition.DisplayName + "，查看其中可对接的执事、前辈与同门人物。" +
                                    (string.IsNullOrWhiteSpace(locationDigest) ? string.Empty : "\n\n" + locationDigest),
            ResolvedSelectedHallIndex = resolvedSelectedHallIndex,
            SelectedHallId = definition.Id,
            Preview = new WorldMapPreviewSnapshot
            {
                Sprite = definition.IllustrationImage,
                Label = definition.DisplayName + "占位图",
                PlaceholderColor = definition.PlaceholderColor
            },
            HallButtons = hallButtons,
            ActionButtons = actionButtons,
            LocationEntries = locationEntries
        };
    }

    private WorldMapLocationEntrySnapshot[] BuildLocationEntries(CultivationSaveData saveData, string parentLocationId, NpcSceneType sceneType)
    {
        EnsureDependencies();
        if (saveData == null || worldGenerationSystem == null || string.IsNullOrWhiteSpace(parentLocationId))
        {
            return new WorldMapLocationEntrySnapshot[0];
        }

        var locations = worldGenerationSystem.GetVisibleLocations(saveData, parentLocationId, sceneType);
        if (locations == null || locations.Length == 0)
        {
            return new WorldMapLocationEntrySnapshot[0];
        }

        var snapshots = new WorldMapLocationEntrySnapshot[locations.Length];
        for (var i = 0; i < locations.Length; i++)
        {
            var location = locations[i];
            if (location == null)
            {
                snapshots[i] = new WorldMapLocationEntrySnapshot
                {
                    LocationId = string.Empty,
                    DisplayName = string.Empty,
                    Subtitle = string.Empty,
                    StatusText = string.Empty,
                    ButtonLabel = string.Empty,
                    TooltipTitle = string.Empty,
                    TooltipBody = string.Empty,
                    IsVisible = false,
                    IsInteractable = false
                };
                continue;
            }

            location.EnsureDefaults();
            var residentCount = location.residentNpcIds != null ? location.residentNpcIds.Length : 0;
            var locationIncidents = worldIncidentSystem != null
                ? GetLocationIncidents(saveData, location.locationId)
                : new WorldIncidentData[0];
            var status = BuildLocationStatusText(location, residentCount, locationIncidents);
            var builder = new System.Text.StringBuilder();
            builder.Append(string.IsNullOrWhiteSpace(location.description) ? "暂无额外介绍。" : location.description);
            builder.Append("\n\n驻留修士：").Append(residentCount);
            if (!string.IsNullOrWhiteSpace(status))
            {
                builder.Append("\n状态：").Append(status);
            }

            if (locationIncidents.Length > 0)
            {
                builder.Append("\n风闻：");
                for (var incidentIndex = 0; incidentIndex < locationIncidents.Length; incidentIndex++)
                {
                    if (incidentIndex > 0)
                    {
                        builder.Append(" / ");
                    }

                    builder.Append(locationIncidents[incidentIndex].displayTitle);
                }
            }

            snapshots[i] = new WorldMapLocationEntrySnapshot
            {
                LocationId = location.locationId,
                DisplayName = location.displayName,
                Subtitle = location.subtitle,
                StatusText = status,
                ButtonLabel = string.IsNullOrWhiteSpace(location.subtitle)
                    ? location.displayName
                    : location.displayName + "\n<size=16>" + location.subtitle + "</size>",
                TooltipTitle = location.displayName,
                TooltipBody = builder.ToString(),
                IsVisible = true,
                IsInteractable = true,
                IsTemporary = location.isTemporary
            };
        }

        return snapshots;
    }

    private WorldIncidentData[] GetLocationIncidents(CultivationSaveData saveData, string locationId)
    {
        if (saveData == null || saveData.activeWorldIncidents == null || string.IsNullOrWhiteSpace(locationId))
        {
            return new WorldIncidentData[0];
        }

        var list = new System.Collections.Generic.List<WorldIncidentData>();
        for (var i = 0; i < saveData.activeWorldIncidents.Length; i++)
        {
            var incident = saveData.activeWorldIncidents[i];
            if (incident == null || incident.status != WorldIncidentStatus.Active || incident.locationId != locationId)
            {
                continue;
            }

            list.Add(incident);
        }

        return list.ToArray();
    }

    private static string BuildLocationStatusText(GeneratedLocationState location, int residentCount, WorldIncidentData[] incidents)
    {
        var status = string.IsNullOrWhiteSpace(location.subtitle) ? string.Empty : location.subtitle;
        status += residentCount > 0
            ? (string.IsNullOrWhiteSpace(status) ? string.Empty : " / ") + "驻留 " + residentCount + " 人"
            : string.Empty;
        status += incidents != null && incidents.Length > 0
            ? (string.IsNullOrWhiteSpace(status) ? string.Empty : " / ") + "有风闻"
            : string.Empty;
        status += location.isTemporary
            ? (string.IsNullOrWhiteSpace(status) ? string.Empty : " / ") + "临时入口"
            : string.Empty;
        return status;
    }

    private static WorldRegionDefinition ResolveRegion(string regionId, string fallbackRegionId)
    {
        WorldRegionDefinition region;
        if (!string.IsNullOrWhiteSpace(regionId) && WorldRegionLibrary.TryGetRegion(regionId, out region))
        {
            return region;
        }

        if (!string.IsNullOrWhiteSpace(fallbackRegionId) && WorldRegionLibrary.TryGetRegion(fallbackRegionId, out region))
        {
            return region;
        }

        return null;
    }

    private static WorldMapRegionSnapshot BuildMissingRegionSnapshot()
    {
        return new WorldMapRegionSnapshot
        {
            PanelTitle = "历练地点",
            PanelSubtitle = "地域情报缺失",
            Description = "当前没有可展示的地域数据。",
            Status = "状态：不可进入。",
            TaskSummary = "委托：暂无。",
            TravelButtonLabel = "不可前往",
            DialogueButtonLabel = "进入前沿据点",
            DialogueButtonTooltip = "查看该地域的斥候、线人与当前线索。",
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

    private WorldMapWorkshopRecipeSnapshot BuildWorkshopRecipeSnapshot(CultivationSaveData saveData, string recipeId)
    {
        var recipes = WorkshopLibrary.GetRecipes();
        if (recipes == null)
        {
            return BuildMissingRecipeSnapshot(recipeId);
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
                IsInteractable = tradeSystem.CanCraft(saveData, recipe),
                TooltipTitle = recipe.Title,
                TooltipBody = recipe.Description + "\n\n" + WorkshopLibrary.BuildRecipeButtonLabel(saveData, recipeId)
            };
        }

        return BuildMissingRecipeSnapshot(recipeId);
    }

    private WorldMapWorkshopRecipeSnapshot BuildMissingRecipeSnapshot(string recipeId)
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

    private static WorldMapPreviewSnapshot BuildInventoryPreviewSnapshot(CultivationSaveData saveData)
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

    private static WorldMapPreviewSnapshot BuildWorkshopPreviewSnapshot()
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

    private static WorldMapPreviewSnapshot BuildSectPlaceholderPreview()
    {
        return new WorldMapPreviewSnapshot
        {
            Sprite = null,
            Label = "宗门占位图",
            PlaceholderColor = new Color(0.24f, 0.18f, 0.12f, 1f)
        };
    }
}
