using QFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class CultivationControllerExtensions
{
    public static void BindButton(this IController controller, Button button, UnityAction action, CultivationButtonSound sound = CultivationButtonSound.Click)
    {
        CultivationUiAudio.BindButton(button, action, controller.GetSystem<ISoundSystem>(), sound);
    }

    public static void PlayButtonSound(this IController controller, CultivationButtonSound sound = CultivationButtonSound.Click)
    {
        CultivationUiAudio.PlayButtonSound(controller.GetSystem<ISoundSystem>(), sound);
    }

    public static T LoadResource<T>(this IController controller, string path) where T : Object
    {
        return controller.GetUtility<IGameResourceService>().Load<T>(path);
    }

    public static GameObject InstantiatePrefab(this IController controller, string path, Transform parent = null)
    {
        return controller.GetUtility<IGameResourceService>().InstantiatePrefab(path, parent);
    }

    public static UIPanel OpenGameUiPanel(this IController controller, GameUiPanelId panelId, IUIData uiData = null)
    {
        return controller.SendCommand(new OpenGameUiPanelCommand(panelId, uiData));
    }

    public static MainMenuController OpenMainMenuPanel(this IController controller, MainMenuConfig config)
    {
        return controller.SendCommand(new OpenMainMenuPanelCommand(config));
    }

    public static WorldMapController OpenWorldMapPanel(this IController controller, string gameplaySceneName, string mainSceneName)
    {
        return controller.SendCommand(new OpenWorldMapPanelCommand(gameplaySceneName, mainSceneName));
    }

    public static void CloseGameUiPanel(this IController controller, GameUiPanelId panelId)
    {
        controller.SendCommand(new CloseGameUiPanelCommand(panelId));
    }

    public static void CloseAllGameUiPanels(this IController controller)
    {
        controller.SendCommand(new CloseAllGameUiPanelsCommand());
    }

    public static void HideGameUiPanel(this IController controller, GameUiPanelId panelId)
    {
        controller.SendCommand(new HideGameUiPanelCommand(panelId));
    }

    public static void ShowGameUiPanel(this IController controller, GameUiPanelId panelId)
    {
        controller.SendCommand(new ShowGameUiPanelCommand(panelId));
    }

    public static void DestroyGameUiPanel(this IController controller, GameUiPanelId panelId)
    {
        controller.SendCommand(new DestroyGameUiPanelCommand(panelId));
    }

    public static void SetHubState(this IController controller, bool visible, GameHubContext context)
    {
        controller.SendCommand(new SetGameHubStateCommand(visible, context));
    }

    public static void SetPlayerCompendiumVisible(this IController controller, bool visible)
    {
        controller.SendCommand(new SetPlayerCompendiumVisibilityCommand(visible));
    }

    public static void SetPlayerCompendiumSelection(this IController controller, PlayerCompendiumMainTab mainTab, string sectionId = "")
    {
        controller.SendCommand(new SetPlayerCompendiumSelectionCommand(mainTab, sectionId));
    }

    public static void SaveArchive(this IController controller, int slotIndex, CultivationSaveData saveData)
    {
        controller.SendCommand(new SaveArchiveCommand(slotIndex, saveData));
    }

    public static void SyncArchiveState(this IController controller, int slotIndex, CultivationSaveData saveData)
    {
        controller.SendCommand(new SyncArchiveStateCommand(slotIndex, saveData));
    }

    public static void DeleteArchive(this IController controller, int slotIndex)
    {
        controller.SendCommand(new DeleteArchiveCommand(slotIndex));
    }

    public static CultivationArchiveSnapshot BootstrapCurrentArchive(this IController controller)
    {
        return controller.SendCommand(new BootstrapCurrentArchiveCommand());
    }

    public static string ResolveTaskBoard(this IController controller, int slotIndex, CultivationSaveData saveData)
    {
        return controller.SendCommand(new ResolveTaskBoardCommand(slotIndex, saveData));
    }

    public static TaskContextSnapshot GetActiveTaskContext(this IController controller, CultivationSaveData saveData)
    {
        return controller.SendCommand(new GetActiveTaskContextCommand(saveData));
    }

    public static WorldMapRegionSnapshot BuildWorldMapRegionSnapshot(this IController controller, CultivationSaveData saveData, string regionId, string fallbackRegionId)
    {
        return controller.SendCommand(new BuildWorldMapRegionSnapshotCommand(saveData, regionId, fallbackRegionId));
    }

    public static WorldMapInventorySnapshot BuildWorldMapInventorySnapshot(this IController controller, CultivationSaveData saveData)
    {
        return controller.SendCommand(new BuildWorldMapInventorySnapshotCommand(saveData));
    }

    public static WorldMapWorkshopSnapshot BuildWorldMapWorkshopSnapshot(this IController controller, CultivationSaveData saveData)
    {
        return controller.SendCommand(new BuildWorldMapWorkshopSnapshotCommand(saveData));
    }

    public static WorldMapSettlementSnapshot BuildWorldMapSettlementSnapshot(this IController controller, CultivationSaveData saveData)
    {
        return controller.SendCommand(new BuildWorldMapSettlementSnapshotCommand(saveData));
    }

    public static WorldMapSectResidenceSnapshot BuildWorldMapSectResidenceSnapshot(this IController controller, CultivationSaveData saveData, int selectedSectHallIndex)
    {
        return controller.SendCommand(new BuildWorldMapSectResidenceSnapshotCommand(saveData, selectedSectHallIndex));
    }

    public static WorldMapNpcDialogueSnapshot BuildNpcDialogueSnapshot(this IController controller, CultivationSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, string locationId, string selectedNpcId)
    {
        return controller.SendCommand(new BuildNpcDialogueSnapshotCommand(saveData, sceneType, regionId, sectHallId, locationId, selectedNpcId));
    }

    public static NpcInteractionResult ExecuteNpcDialogueChoice(this IController controller, int slotIndex, CultivationSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, string locationId, string npcId, string choiceId)
    {
        return controller.SendCommand(new ExecuteNpcDialogueChoiceCommand(slotIndex, saveData, sceneType, regionId, sectHallId, locationId, npcId, choiceId));
    }

    public static bool StartEventConversation(this IController controller, string conversationTitle, CultivationSaveData saveData, System.Action onEnd = null)
    {
        return controller.SendCommand(new StartEventConversationCommand(conversationTitle, saveData, onEnd));
    }

    public static ExpeditionView OpenExpeditionPanel(this IController controller)
    {
        return controller.SendCommand(new OpenExpeditionPanelCommand());
    }

    public static System.Collections.Generic.List<ExpeditionRoomState> BuildExpeditionRooms(this IController controller, WorldRegionDefinition region, CultivationSaveData saveData, System.Random random)
    {
        return controller.SendCommand(new BuildExpeditionRoomsCommand(region, saveData, random));
    }

    public static System.Collections.Generic.List<ExpeditionEnemyState> BuildEncounterEnemies(this IController controller, WorldRegionDefinition region, ExpeditionRoomState room, CultivationSaveData saveData, System.Random random)
    {
        return controller.SendCommand(new BuildEncounterEnemiesCommand(region, room, saveData, random));
    }

    public static CombatTurnResult ResolveDirectAttackTurn(this IController controller, CombatTurnContext context, ExpeditionEnemyState target, int damage, string missSummary)
    {
        return controller.SendCommand(new ResolveDirectAttackTurnCommand(context, target, damage, missSummary));
    }

    public static CombatTurnResult ResolveSkillTurn(this IController controller, CombatTurnContext context, int skillIndex)
    {
        return controller.SendCommand(new ResolveSkillTurnCommand(context, skillIndex));
    }

    public static CombatTurnResult ResolveTalismanTurn(this IController controller, CombatTurnContext context)
    {
        return controller.SendCommand(new ResolveTalismanTurnCommand(context));
    }

    public static CombatTurnResult ResolveMedicineTurn(this IController controller, CombatTurnContext context)
    {
        return controller.SendCommand(new ResolveMedicineTurnCommand(context));
    }

    public static ExpeditionSupportActionResult UseTorchSupply(this IController controller, CombatTurnContext context)
    {
        return controller.SendCommand(new UseTorchSupplyCommand(context));
    }

    public static ExpeditionSupportActionResult CampAndRecover(this IController controller, CombatTurnContext context)
    {
        return controller.SendCommand(new CampAndRecoverCommand(context));
    }

    public static ExpeditionSupportActionResult RecenterMind(this IController controller, CombatTurnContext context)
    {
        return controller.SendCommand(new RecenterMindCommand(context));
    }

    public static ExpeditionSupportActionResult SkipRoom(this IController controller, CombatTurnContext context)
    {
        return controller.SendCommand(new SkipRoomCommand(context));
    }

    public static EnemyIntentPreview[] PreviewEnemyIntents(this IController controller, CombatTurnContext context)
    {
        return controller.SendCommand(new PreviewEnemyIntentsCommand(context));
    }

    public static ExpeditionEventCardResult OpenRoomEvent(this IController controller, CombatTurnContext context)
    {
        return controller.SendCommand(new OpenRoomEventCommand(context));
    }

    public static ExpeditionEventOptionResult ResolveEventOption(this IController controller, CombatTurnContext context, string eventId, string optionId)
    {
        return controller.SendCommand(new ResolveEventOptionCommand(context, eventId, optionId));
    }

    public static ExpeditionTraversalResult EnterExpeditionRoom(this IController controller, ExpeditionTraversalContext context)
    {
        return controller.SendCommand(new EnterExpeditionRoomCommand(context));
    }

    public static ExpeditionAdvanceResult AdvanceExpedition(this IController controller, ExpeditionAdvanceContext context)
    {
        return controller.SendCommand(new AdvanceExpeditionCommand(context));
    }

    public static ExpeditionResolutionResult CompleteExpedition(
        this IController controller,
        int slotIndex,
        CultivationSaveData saveData,
        WorldRegionDefinition region,
        ExpeditionHeroState hero,
        int torchlight,
        int pendingQiGain,
        int pendingCrystalGain,
        System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        return controller.SendCommand(new CompleteExpeditionRunCommand(
            slotIndex,
            saveData,
            region,
            hero,
            torchlight,
            pendingQiGain,
            pendingCrystalGain,
            pendingItemRewards));
    }

    public static ExpeditionResolutionResult RetreatExpedition(
        this IController controller,
        int slotIndex,
        CultivationSaveData saveData,
        WorldRegionDefinition region,
        int pendingQiGain,
        int pendingCrystalGain,
        System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        return controller.SendCommand(new RetreatExpeditionRunCommand(
            slotIndex,
            saveData,
            region,
            pendingQiGain,
            pendingCrystalGain,
            pendingItemRewards));
    }

    public static ExpeditionResolutionResult FailExpedition(
        this IController controller,
        int slotIndex,
        CultivationSaveData saveData,
        WorldRegionDefinition region,
        string reason,
        System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        return controller.SendCommand(new FailExpeditionRunCommand(slotIndex, saveData, region, reason, pendingItemRewards));
    }

    public static MindStateResult ApplyCombatMindStress(this IController controller, CombatTurnContext context, int amount)
    {
        return controller.SendCommand(new ApplyCombatMindStressCommand(context, amount));
    }

    public static void SyncExpeditionRuntime(this IController controller, CombatTurnContext context)
    {
        controller.SendCommand(new SyncExpeditionRuntimeCommand(context));
    }

    public static void ClearExpeditionRuntime(this IController controller)
    {
        controller.SendCommand(new ClearExpeditionRuntimeCommand());
    }

    public static WorldMapActionResult TravelToRegion(this IController controller, int slotIndex, CultivationSaveData saveData, WorldRegionDefinition region)
    {
        return controller.SendCommand(new TravelToRegionCommand(slotIndex, saveData, region));
    }

    public static WorldMapActionResult UpgradeProtectiveRelic(this IController controller, int slotIndex, CultivationSaveData saveData)
    {
        return controller.SendCommand(new UpgradeProtectiveRelicCommand(slotIndex, saveData));
    }

    public static WorldMapActionResult UpgradeMainArtifact(this IController controller, int slotIndex, CultivationSaveData saveData)
    {
        return controller.SendCommand(new UpgradeMainArtifactCommand(slotIndex, saveData));
    }

    public static WorldMapActionResult CraftWorldMapRecipe(this IController controller, int slotIndex, CultivationSaveData saveData, string recipeId)
    {
        return controller.SendCommand(new CraftWorldMapRecipeCommand(slotIndex, saveData, recipeId));
    }

    public static string BuildSettlementSummary(this IController controller, CultivationSaveData saveData)
    {
        return controller.SendCommand(new BuildSettlementSummaryCommand(saveData));
    }

    public static SectHallSnapshot[] GetSectHallSnapshots(this IController controller, CultivationSaveData saveData)
    {
        return controller.SendCommand(new GetSectHallSnapshotsCommand(saveData));
    }

    public static string BuildSectOverview(this IController controller, CultivationSaveData saveData)
    {
        return controller.SendCommand(new BuildSectOverviewCommand(saveData));
    }

    public static SectActionResult ExecuteSectAction(this IController controller, int slotIndex, CultivationSaveData saveData, string actionId)
    {
        return controller.SendCommand(new ExecuteSectActionCommand(slotIndex, saveData, actionId));
    }

    public static float GetMusicVolume(this IController controller)
    {
        return controller.GetUtility<IGameSettingsService>().MusicVolume.Value;
    }

    public static float GetSfxVolume(this IController controller)
    {
        return controller.GetUtility<IGameSettingsService>().SfxVolume.Value;
    }

    public static float GetVoiceVolume(this IController controller)
    {
        return controller.GetUtility<IGameSettingsService>().VoiceVolume.Value;
    }

    public static bool IsFullscreen(this IController controller)
    {
        return controller.GetUtility<IGameSettingsService>().Fullscreen.Value;
    }

    public static void ApplyUserSettings(this IController controller)
    {
        controller.GetUtility<IGameSettingsService>().ApplyRuntimeSettings();
    }

    public static void SetMusicVolume(this IController controller, float value)
    {
        controller.GetUtility<IGameSettingsService>().SetMusicVolume(value);
    }

    public static void SetSfxVolume(this IController controller, float value)
    {
        controller.GetUtility<IGameSettingsService>().SetSfxVolume(value);
    }

    public static void SetVoiceVolume(this IController controller, float value)
    {
        controller.GetUtility<IGameSettingsService>().SetVoiceVolume(value);
    }

    public static void SetFullscreen(this IController controller, bool fullscreen)
    {
        controller.GetUtility<IGameSettingsService>().SetFullscreen(fullscreen);
    }

    public static void ResetUserSettings(this IController controller)
    {
        controller.GetUtility<IGameSettingsService>().Reset();
    }

    public static void SetMusicDuck(this IController controller, string reason, bool enabled, float duckDb = 8f)
    {
        controller.GetUtility<IGameAudioService>().SetMusicDuck(reason, enabled, duckDb);
    }

    public static void PlayMainMenuMusic(this IController controller)
    {
        controller.GetSystem<ISoundSystem>().PlayMainMenuMusic();
    }

    public static void PlayWorldMapMusic(this IController controller)
    {
        controller.GetSystem<ISoundSystem>().PlayWorldMapMusic();
    }

    public static void PlayExpeditionMusic(this IController controller, WorldRegionDefinition region)
    {
        controller.GetSystem<ISoundSystem>().PlayExpeditionMusic(region);
    }

    public static void PlaySound(this IController controller, SoundType type)
    {
        controller.GetSystem<ISoundSystem>().PlaySound(type);
    }

    public static void LogError(this IController controller, string message)
    {
        controller.GetUtility<IGameLogService>().Error(message);
    }

    public static CultivationMessagePopupPanel ShowMessagePopup(
        this IController controller,
        string message,
        string title = "",
        float duration = 2.4f,
        CultivationMessagePopupStyle style = CultivationMessagePopupStyle.Info)
    {
        return controller.GetUtility<IGameUiService>().ShowMessagePopup(message, title, duration, style);
    }

    public static CultivationMessagePopupPanel ShowInfoMessage(this IController controller, string message, float duration = 2.2f)
    {
        return controller.ShowMessagePopup(message, "提示", duration, CultivationMessagePopupStyle.Info);
    }

    public static CultivationMessagePopupPanel ShowWarningMessage(this IController controller, string message, float duration = 2.8f)
    {
        return controller.ShowMessagePopup(message, "注意", duration, CultivationMessagePopupStyle.Warning);
    }

    public static CultivationMessagePopupPanel ShowErrorMessage(this IController controller, string message, float duration = 3.2f)
    {
        return controller.ShowMessagePopup(message, "异常", duration, CultivationMessagePopupStyle.Error);
    }

    public static CultivationMessagePopupPanel ShowSuccessMessage(this IController controller, string message, float duration = 2.4f)
    {
        return controller.ShowMessagePopup(message, "完成", duration, CultivationMessagePopupStyle.Success);
    }
}
