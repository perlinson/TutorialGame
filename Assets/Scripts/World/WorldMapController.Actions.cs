using UnityEngine;
using QFramework;
public sealed partial class WorldMapController
{
    public void TravelToRegionById(string regionId)
    {
        int regionIndex;
        if (!TryResolveRegionIndex(regionId, out regionIndex))
        {
            SetHint("当前地点数据未同步，无法进入历练。");
            ShowWarningMessage("当前地点数据未同步，无法进入历练。");
            return;
        }

        selectedRegionIndex = regionIndex;
        TravelToSelectedRegion();
    }

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
        if (TryStartRegionIntroConversation(region))
        {
            return;
        }

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

    private bool TryStartRegionIntroConversation(WorldRegionDefinition region)
    {
        if (saveData == null || region == null)
        {
            return false;
        }

        var introFlag = BuildRegionIntroFlag(region.Id);
        if (HasStoryFlag(introFlag))
        {
            return false;
        }

        var conversationTitle = BuildRegionIntroConversationTitle(region.Id);
        var dialogueSystem = this.GetSystem<CultivationDialogueSystem>();
        if (dialogueSystem == null || !dialogueSystem.HasConversation(conversationTitle))
        {
            return false;
        }

        SetHint("初到 " + region.DisplayName + "，一段见闻在心头浮现。");
        return StartEventConversation(conversationTitle, saveData, () =>
        {
            CultivationApp.RecordStorySignal(saveData, new StorySignal
            {
                StoryId = "region_intro",
                NodeId = region.Id,
                Title = "地域初见",
                ResultText = "已记录对 " + region.DisplayName + " 的初见见闻。"
            });
            SaveArchive(currentSlotIndex, saveData);
            SceneFlow.RequestScene(gameplaySceneName);
        });
    }

    private static string BuildRegionIntroConversationTitle(string regionId)
    {
        return "region_intro_" + (regionId ?? string.Empty);
    }

    private static string BuildRegionIntroFlag(string regionId)
    {
        return "region_intro:" + (regionId ?? string.Empty);
    }

    private bool HasStoryFlag(string flagId)
    {
        if (saveData == null || saveData.storyFlags == null || string.IsNullOrWhiteSpace(flagId))
        {
            return false;
        }

        for (var i = 0; i < saveData.storyFlags.Length; i++)
        {
            if (saveData.storyFlags[i] == flagId)
            {
                return true;
            }
        }

        return false;
    }

    public void CloseSettlement()
    {
        selectedSettlementLocationId = string.Empty;
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
        selectedSectLocationId = string.Empty;
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
        selectedRegionLocationId = string.Empty;
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
        selectedSectLocationId = string.Empty;
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

    public void OpenSettlementDialogue(string locationId = null)
    {
        selectedSettlementLocationId = locationId ?? string.Empty;
        OpenNpcDialoguePanel(NpcSceneType.Settlement, string.Empty, string.Empty, locationId);
    }

    public void OpenSectDialogue(string locationId = null)
    {
        selectedSectLocationId = locationId ?? string.Empty;
        OpenNpcDialoguePanel(NpcSceneType.SectResidence, string.Empty, GetSelectedSectHallId(), locationId);
    }

    public void OpenRegionDialogue(string regionId, string locationId = null)
    {
        selectedRegionLocationId = locationId ?? string.Empty;
        OpenNpcDialoguePanel(NpcSceneType.Region, regionId, string.Empty, locationId);
    }

    public void SelectNpc(NpcSceneType sceneType, string regionId, string sectHallId, string locationId, string npcId)
    {
        selectedNpcId = npcId ?? string.Empty;
        SetSelectedNpcForContext(sceneType, regionId, sectHallId, locationId, selectedNpcId);
        RefreshOpenPanels();
    }

    public void CloseNpcDialogue()
    {
        CloseGameUiPanel(GameUiPanelId.WorldMapNpcDialogue);
        RefreshOpenPanels();
    }

    public WorldMapNpcDialogueSnapshot BuildNpcDialogueSnapshot(NpcSceneType sceneType, string regionId, string sectHallId, string locationId)
    {
        var resolvedSelectedNpcId = ResolveSelectedNpcForContext(sceneType, regionId, sectHallId, locationId);
        var snapshot = base.BuildNpcDialogueSnapshot(saveData, sceneType, regionId, sectHallId, locationId, resolvedSelectedNpcId);
        if (snapshot != null)
        {
            selectedNpcId = snapshot.SelectedNpcId ?? string.Empty;
            SetSelectedNpcForContext(sceneType, regionId, sectHallId, locationId, selectedNpcId);
        }

        return snapshot;
    }

    public void ExecuteNpcDialogueChoice(NpcSceneType sceneType, string regionId, string sectHallId, string locationId, string npcId, string choiceId)
    {
        var result = base.ExecuteNpcDialogueChoice(currentSlotIndex, saveData, sceneType, regionId, sectHallId, locationId, npcId, choiceId);
        if (result != null)
        {
            selectedNpcId = result.SelectedNpcId;
            SetSelectedNpcForContext(sceneType, regionId, sectHallId, locationId, selectedNpcId);
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

    public void OpenIncidentEntry(NpcSceneType sceneType, string regionId, string sectHallId, string locationId, string incidentId)
    {
        var incident = FindIncidentById(incidentId);
        if (incident == null || incident.status != WorldIncidentStatus.Active)
        {
            const string unavailableMessage = "这则风闻已经散去，当前无法继续追查。";
            SetHint(unavailableMessage);
            ShowWarningMessage(unavailableMessage);
            RefreshOpenPanels();
            return;
        }

        var primaryNpc = ResolvePrimaryIncidentNpc(incident);
        if (primaryNpc != null)
        {
            selectedNpcId = primaryNpc.npcId;
            SetSelectedNpcForContext(sceneType, regionId, sectHallId, locationId, selectedNpcId);
        }

        var dialogueSystem = this.GetSystem<CultivationDialogueSystem>();
        if (!string.IsNullOrWhiteSpace(incident.conversationTitle) &&
            dialogueSystem != null &&
            dialogueSystem.IsReady &&
            dialogueSystem.HasConversation(incident.conversationTitle))
        {
            var bindingSystem = this.GetSystem<CultivationDialogueBindingSystem>();
            var worldGenerationSystem = this.GetSystem<CultivationWorldGenerationSystem>();
            var runtimeLocation = worldGenerationSystem != null
                ? worldGenerationSystem.ResolveLocationState(saveData, incident.locationId)
                : null;

            bindingSystem?.BindNpcConversationContext(saveData, primaryNpc, runtimeLocation, incident);
            var started = StartEventConversation(incident.conversationTitle, saveData, () =>
            {
                SaveArchive(currentSlotIndex, saveData);
                RefreshAll();
                RefreshOpenPanels();
            });

            if (started)
            {
                SetHint("已切入风闻：" + incident.displayTitle);
                RefreshOpenPanels();
                return;
            }
        }

        if (primaryNpc != null)
        {
            ExecuteNpcDialogueChoice(sceneType, regionId, sectHallId, locationId, primaryNpc.npcId, "generated_incident_followup");
            return;
        }

        RefreshOpenPanels();
        var fallbackMessage = string.IsNullOrWhiteSpace(incident.description)
            ? "这则风闻暂时还没有落到具体人物身上。"
            : incident.description;
        SetHint(fallbackMessage);
        ShowInfoMessage(fallbackMessage);
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

    private void OpenNpcDialoguePanel(NpcSceneType sceneType, string regionId, string sectHallId, string locationId)
    {
        CloseGameUiPanel(GameUiPanelId.WorldMapInventory);
        CloseGameUiPanel(GameUiPanelId.WorldMapWorkshop);
        var panel = OpenGameUiPanel(GameUiPanelId.WorldMapNpcDialogue, new WorldMapNpcDialoguePanelData(this, sceneType, regionId, sectHallId, locationId));
        if (panel == null)
        {
            SetHint("人物对话面板 prefab 缺失，请先重新生成 WorldMap UI Prefabs。");
            ShowErrorMessage("人物对话面板 prefab 缺失，请先重新生成 WorldMap UI Prefabs。");
            return;
        }

        RefreshOpenPanels();
        SetHint(BuildNpcLocaleHint(sceneType, regionId, sectHallId, locationId));
    }

    private string BuildNpcLocaleHint(NpcSceneType sceneType, string regionId, string sectHallId, string locationId)
    {
        var locationName = ResolveRuntimeLocationName(locationId);
        if (!string.IsNullOrWhiteSpace(locationName))
        {
            return "已进入" + locationName + "。";
        }

        switch (sceneType)
        {
            case NpcSceneType.SectResidence:
                var sectSystem = this.GetSystem<CultivationSectSystem>();
                SectHallDefinition hallDefinition;
                if (sectSystem != null && sectSystem.TryGetHallDefinition(sectHallId, out hallDefinition) && hallDefinition != null)
                {
                    return "已进入" + hallDefinition.DisplayName + "。";
                }

                return "已进入宗门殿堂。";
            case NpcSceneType.Region:
                return "已进入" + WorldRegionLibrary.GetRegionDisplayName(regionId) + "的前沿据点。";
            default:
                return saveData != null && saveData.isSectDisciple ? "已进入山门坊市。" : "已进入行脚坊市。";
        }
    }

    private string ResolveRuntimeLocationName(string locationId)
    {
        if (string.IsNullOrWhiteSpace(locationId))
        {
            return string.Empty;
        }

        var worldGenerationSystem = this.GetSystem<CultivationWorldGenerationSystem>();
        return worldGenerationSystem != null ? worldGenerationSystem.ResolveLocationName(saveData, locationId) : string.Empty;
    }

    private bool TryResolveRegionIndex(string regionId, out int regionIndex)
    {
        regionIndex = -1;
        if (!HasRegions() || string.IsNullOrWhiteSpace(regionId))
        {
            return false;
        }

        for (var i = 0; i < regions.Count; i++)
        {
            var region = regions[i];
            if (region == null || region.Id != regionId)
            {
                continue;
            }

            regionIndex = i;
            return true;
        }

        return false;
    }

    private string GetSelectedSectHallId()
    {
        return selectedSectHallId ?? string.Empty;
    }

    private string ResolveSelectedNpcForContext(NpcSceneType sceneType, string regionId, string sectHallId, string locationId)
    {
        var key = BuildNpcContextKey(sceneType, regionId, sectHallId, locationId);
        string resolvedNpcId;
        if (selectedNpcIdsByContext.TryGetValue(key, out resolvedNpcId))
        {
            return resolvedNpcId ?? string.Empty;
        }

        return string.Empty;
    }

    private void SetSelectedNpcForContext(NpcSceneType sceneType, string regionId, string sectHallId, string locationId, string npcId)
    {
        var key = BuildNpcContextKey(sceneType, regionId, sectHallId, locationId);
        if (string.IsNullOrWhiteSpace(npcId))
        {
            selectedNpcIdsByContext.Remove(key);
            return;
        }

        selectedNpcIdsByContext[key] = npcId;
    }

    private string BuildNpcContextKey(NpcSceneType sceneType, string regionId, string sectHallId, string locationId)
    {
        var anchorId = sceneType == NpcSceneType.Region
            ? regionId ?? string.Empty
            : sceneType == NpcSceneType.SectResidence
                ? sectHallId ?? string.Empty
                : saveData != null ? saveData.currentRegionId : string.Empty;
        return sceneType + "|" + anchorId + "|" + (locationId ?? string.Empty);
    }

    private WorldIncidentData FindIncidentById(string incidentId)
    {
        if (saveData == null || saveData.activeWorldIncidents == null || string.IsNullOrWhiteSpace(incidentId))
        {
            return null;
        }

        for (var i = 0; i < saveData.activeWorldIncidents.Length; i++)
        {
            var incident = saveData.activeWorldIncidents[i];
            if (incident != null && incident.incidentId == incidentId)
            {
                return incident;
            }
        }

        return null;
    }

    private GeneratedNpcData ResolvePrimaryIncidentNpc(WorldIncidentData incident)
    {
        if (saveData == null || incident == null || incident.participantNpcIds == null)
        {
            return null;
        }

        for (var i = 0; i < incident.participantNpcIds.Length; i++)
        {
            var npc = saveData.FindGeneratedNpc(incident.participantNpcIds[i]);
            if (npc != null)
            {
                return npc;
            }
        }

        return null;
    }
}
