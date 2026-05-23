using QFramework;
using UnityEngine;

public sealed class CultivationApp : Architecture<CultivationApp>
{
    protected override void Init()
    {
        var resourceService = new GameResourceService();
        var dataService = new GameDataService();
        var timeService = new GameTimeService();
        var randomService = new GameRandomService();
        var settingsService = new GameSettingsService();
        var audioService = new GameAudioService();
        var logService = new GameLogService();
        var uiService = new GameUiService();

        RegisterUtility<IGameResourceService>(resourceService);
        RegisterUtility<IGameDataService>(dataService);
        RegisterUtility<IGameTimeService>(timeService);
        RegisterUtility<IGameRandomService>(randomService);
        RegisterUtility<IGameSettingsService>(settingsService);
        RegisterUtility<IGameAudioService>(audioService);
        RegisterUtility<IGameLogService>(logService);
        RegisterUtility<IGameUiService>(uiService);

        RegisterModel(new CultivationArchiveModel());
        RegisterModel(new CultivationInventoryModel());
        RegisterModel(new CultivationPlayerModel());
        RegisterModel(new CultivationRealmModel());
        RegisterModel(new CultivationAttributeModel());
        RegisterModel(new CultivationBranchModel());
        RegisterModel(new CultivationSchoolModel());
        RegisterModel(new CultivationDivinePowerModel());
        RegisterModel(new CultivationStatusModel());
        RegisterModel(new CultivationSocialModel());
        RegisterModel(new CultivationSkillModel());
        RegisterModel(new CultivationCombatStatsModel());
        RegisterModel(new CultivationGameModel());
        RegisterModel(new CultivationTaskBoardModel());
        RegisterModel(new CultivationWorldMapModel());
        RegisterModel(new CultivationExpeditionModel());
        RegisterSystem<ISoundSystem>(new SoundSystem());
        RegisterSystem(new CultivationSaveSystem());
        RegisterSystem(new CultivationRealmSystem());
        RegisterSystem(new CultivationConditionSystem());
        RegisterSystem(new CultivationStorySystem());
        RegisterSystem(new CultivationMindStateSystem());
        RegisterSystem(new CultivationFactionSystem());
        RegisterSystem(new CultivationRewardSystem());
        RegisterSystem(new CultivationTaskSystem());
        RegisterSystem(new CultivationSettlementSystem());
        RegisterSystem(new CultivationSectSystem());
        RegisterSystem(new CultivationNpcSystem());
        RegisterSystem(new CultivationEncounterDirectorSystem());
        RegisterSystem(new CultivationEnemyAiSystem());
        RegisterSystem(new CultivationWorldMapSystem());
        RegisterSystem(new CultivationDamageSystem());
        RegisterSystem(new CultivationBuffSystem());
        RegisterSystem(new CultivationSkillCastSystem());
        RegisterSystem(new CultivationBattleSystem());
        RegisterSystem(new CultivationBranchSystem());
        RegisterSystem(new CultivationSchoolSystem());
        RegisterSystem(new CultivationDivinePowerSystem());
        RegisterSystem(new CultivationStatusSystem());
        RegisterSystem(new CultivationExpeditionSystem());
        RegisterSystem(new CultivationExpeditionEventSystem());

        // 初始化战斗快照构建器（集成流派和分支系统）
        CombatSnapshotBuilder.Initialize();
    }

    public static void EnsureInitialized()
    {
        InitArchitecture();
    }

    public static T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        EnsureInitialized();
        return Interface.GetUtility<IGameResourceService>().Load<T>(path);
    }

    public static string GetResourceBackendName()
    {
        EnsureInitialized();
        return Interface.GetUtility<IGameResourceService>().BackendName;
    }

    public static GameObject InstantiatePrefab(string path, Transform parent = null)
    {
        EnsureInitialized();
        return Interface.GetUtility<IGameResourceService>().InstantiatePrefab(path, parent);
    }

    public static float GetMusicVolume()
    {
        EnsureInitialized();
        return Interface.GetUtility<IGameSettingsService>().MusicVolume.Value;
    }

    public static float GetSfxVolume()
    {
        EnsureInitialized();
        return Interface.GetUtility<IGameSettingsService>().SfxVolume.Value;
    }

    public static float GetVoiceVolume()
    {
        EnsureInitialized();
        return Interface.GetUtility<IGameSettingsService>().VoiceVolume.Value;
    }

    public static bool IsFullscreen()
    {
        EnsureInitialized();
        return Interface.GetUtility<IGameSettingsService>().Fullscreen.Value;
    }

    public static void ApplyUserSettings()
    {
        EnsureInitialized();
        Interface.GetUtility<IGameSettingsService>().ApplyRuntimeSettings();
    }

    public static void SetMusicVolume(float value)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameSettingsService>().SetMusicVolume(value);
    }

    public static void SetSfxVolume(float value)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameSettingsService>().SetSfxVolume(value);
    }

    public static void SetVoiceVolume(float value)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameSettingsService>().SetVoiceVolume(value);
    }

    public static void SetFullscreen(bool fullscreen)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameSettingsService>().SetFullscreen(fullscreen);
    }

    public static void ResetUserSettings()
    {
        EnsureInitialized();
        Interface.GetUtility<IGameSettingsService>().Reset();
    }

    public static void InitializeAudio()
    {
        EnsureInitialized();
        Interface.GetUtility<IGameAudioService>().Initialize();
    }

    public static void PlayMusic(string resourcePath, bool loop = true, float volumeScale = 1f)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameAudioService>().PlayMusic(resourcePath, loop, volumeScale);
    }

    public static void PlayMusic(AudioClip clip, bool loop = true, float volumeScale = 1f)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameAudioService>().PlayMusic(clip, loop, volumeScale);
    }

    public static void StopMusic()
    {
        EnsureInitialized();
        Interface.GetUtility<IGameAudioService>().StopMusic();
    }

    public static void PlayVoice(string resourcePath, bool loop = false, float volumeScale = 1f, bool duckMusic = true)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameAudioService>().PlayVoice(resourcePath, loop, volumeScale, duckMusic);
    }

    public static void PlayVoice(AudioClip clip, bool loop = false, float volumeScale = 1f, bool duckMusic = true)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameAudioService>().PlayVoice(clip, loop, volumeScale, duckMusic);
    }

    public static void StopVoice()
    {
        EnsureInitialized();
        Interface.GetUtility<IGameAudioService>().StopVoice();
    }

    public static void PlaySfx(string resourcePath, float volumeScale = 1f)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameAudioService>().PlaySfx(resourcePath, volumeScale);
    }

    public static void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameAudioService>().PlaySfx(clip, volumeScale);
    }

    public static void SetMusicDuck(string reason, bool enabled, float duckDb = 8f)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameAudioService>().SetMusicDuck(reason, enabled, duckDb);
    }

    public static void LogInfo(string message)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameLogService>().Info(message);
    }

    public static void LogWarning(string message)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameLogService>().Warning(message);
    }

    public static void LogError(string message)
    {
        EnsureInitialized();
        Interface.GetUtility<IGameLogService>().Error(message);
    }

    public static void InitializeUi()
    {
        EnsureInitialized();
        Interface.GetUtility<IGameUiService>().Initialize();
    }

    public static UIPanel OpenUiPanel(GameUiPanelId panelId, IUIData uiData = null)
    {
        EnsureInitialized();
        return Interface.SendCommand(new OpenGameUiPanelCommand(panelId, uiData));
    }

    public static MainMenuController OpenMainMenuPanel(MainMenuConfig config)
    {
        return OpenUiPanel(GameUiPanelId.MainMenu, new MainMenuPanelData(config)) as MainMenuController;
    }

    public static MainMenuSettingsPanel OpenMainMenuSettingsPanel(MainMenuController owner)
    {
        return OpenUiPanel(GameUiPanelId.MainMenuSettings, new MainMenuSettingsPanelData(owner)) as MainMenuSettingsPanel;
    }

    public static MainMenuLoadPanel OpenMainMenuLoadPanel(MainMenuController owner)
    {
        return OpenUiPanel(GameUiPanelId.MainMenuLoad, new MainMenuLoadPanelData(owner)) as MainMenuLoadPanel;
    }

    public static MainMenuCharacterCreatePanel OpenMainMenuCharacterCreatePanel(MainMenuController owner)
    {
        return OpenUiPanel(GameUiPanelId.MainMenuCharacterCreate, new MainMenuCharacterCreatePanelData(owner)) as MainMenuCharacterCreatePanel;
    }

    public static WorldMapController OpenWorldMapPanel(string gameplaySceneName, string mainSceneName)
    {
        return OpenUiPanel(GameUiPanelId.WorldMap, new WorldMapPanelData(gameplaySceneName, mainSceneName)) as WorldMapController;
    }

    public static ExpeditionView OpenExpeditionPanel()
    {
        return OpenUiPanel(GameUiPanelId.Expedition) as ExpeditionView;
    }

    public static CultivationMessagePopupPanel ShowMessagePopup(string message, string title = "", float duration = 2.4f, CultivationMessagePopupStyle style = CultivationMessagePopupStyle.Info)
    {
        EnsureInitialized();
        return Interface.GetUtility<IGameUiService>().ShowMessagePopup(message, title, duration, style);
    }

    public static CultivationMessagePopupPanel ShowInfoMessage(string message, float duration = 2.2f)
    {
        return ShowMessagePopup(message, "提示", duration, CultivationMessagePopupStyle.Info);
    }

    public static CultivationMessagePopupPanel ShowWarningMessage(string message, float duration = 2.8f)
    {
        return ShowMessagePopup(message, "注意", duration, CultivationMessagePopupStyle.Warning);
    }

    public static CultivationMessagePopupPanel ShowErrorMessage(string message, float duration = 3.2f)
    {
        return ShowMessagePopup(message, "异常", duration, CultivationMessagePopupStyle.Error);
    }

    public static CultivationMessagePopupPanel ShowSuccessMessage(string message, float duration = 2.4f)
    {
        return ShowMessagePopup(message, "完成", duration, CultivationMessagePopupStyle.Success);
    }

    public static void CloseUiPanel(GameUiPanelId panelId)
    {
        EnsureInitialized();
        Interface.SendCommand(new CloseGameUiPanelCommand(panelId));
    }

    public static void CloseAllGameUiPanels()
    {
        EnsureInitialized();
        Interface.SendCommand(new CloseAllGameUiPanelsCommand());
    }

    public static void DestroyUiPanel(GameUiPanelId panelId)
    {
        EnsureInitialized();
        Interface.SendCommand(new DestroyGameUiPanelCommand(panelId));
    }

    public static void DestroyAllGameUiPanels()
    {
        EnsureInitialized();
        Interface.SendCommand(new DestroyAllGameUiPanelsCommand());
    }

    public static void HideUiPanel(GameUiPanelId panelId)
    {
        EnsureInitialized();
        Interface.SendCommand(new HideGameUiPanelCommand(panelId));
    }

    public static void ShowUiPanel(GameUiPanelId panelId)
    {
        EnsureInitialized();
        Interface.SendCommand(new ShowGameUiPanelCommand(panelId));
    }

    public static void SetGameHubState(bool visible, GameHubContext context)
    {
        EnsureInitialized();
        Interface.SendCommand(new SetGameHubStateCommand(visible, context));
    }

    public static void SetPlayerCompendiumVisible(bool visible)
    {
        EnsureInitialized();
        Interface.SendCommand(new SetPlayerCompendiumVisibilityCommand(visible));
    }

    public static void SetPlayerCompendiumSelection(PlayerCompendiumMainTab mainTab, string sectionId = "")
    {
        EnsureInitialized();
        Interface.SendCommand(new SetPlayerCompendiumSelectionCommand(mainTab, sectionId));
    }

    public static CultivationArchiveSnapshot BootstrapCurrentArchive()
    {
        EnsureInitialized();
        return Interface.SendCommand(new BootstrapCurrentArchiveCommand());
    }

    public static void SaveArchive(int slotIndex, MainMenuSaveData saveData)
    {
        EnsureInitialized();
        Interface.SendCommand(new SaveArchiveCommand(slotIndex, saveData));
    }

    public static CultivationPlayerModel GetPlayerModel()
    {
        EnsureInitialized();
        return Interface.GetModel<CultivationPlayerModel>();
    }

    public static CultivationGameModel GetGameModel()
    {
        EnsureInitialized();
        return Interface.GetModel<CultivationGameModel>();
    }

    public static void DeleteArchive(int slotIndex)
    {
        EnsureInitialized();
        Interface.SendCommand(new DeleteArchiveCommand(slotIndex));
    }

    public static string ResolveTaskBoard(int slotIndex, MainMenuSaveData saveData)
    {
        EnsureInitialized();
        return Interface.SendCommand(new ResolveTaskBoardCommand(slotIndex, saveData));
    }

    public static TaskContextSnapshot GetActiveTaskContext(MainMenuSaveData saveData)
    {
        EnsureInitialized();
        return Interface.SendCommand(new GetActiveTaskContextCommand(saveData));
    }

    public static string ClaimActiveTask(int slotIndex, MainMenuSaveData saveData)
    {
        EnsureInitialized();
        return Interface.SendCommand(new ClaimActiveTaskCommand(slotIndex, saveData));
    }

    public static TaskProgressResult RecordTaskProgress(int slotIndex, MainMenuSaveData saveData, TaskProgressSignal signal)
    {
        EnsureInitialized();
        return Interface.SendCommand(new RecordTaskProgressCommand(slotIndex, saveData, signal));
    }

    public static TaskProgressResult RecordTaskProgress(MainMenuSaveData saveData, TaskProgressSignal signal)
    {
        EnsureInitialized();
        return Interface.SendCommand(new RecordTaskProgressDirectCommand(saveData, signal));
    }

    public static FactionReputationSnapshot RecordFactionDefeat(MainMenuSaveData saveData, ExpeditionEnemyFaction faction, string regionId, int count)
    {
        EnsureInitialized();
        return Interface.SendCommand(new RecordFactionDefeatCommand(saveData, faction, regionId, count));
    }

    public static FactionReputationSnapshot GetFactionSnapshot(MainMenuSaveData saveData, ExpeditionEnemyFaction faction)
    {
        EnsureInitialized();
        return Interface.SendCommand(new GetFactionSnapshotCommand(saveData, faction));
    }

    public static StorySignalResult RecordStorySignal(MainMenuSaveData saveData, StorySignal signal)
    {
        EnsureInitialized();
        return Interface.SendCommand(new RecordStorySignalCommand(saveData, signal));
    }

    public static string BuildStorySummary(MainMenuSaveData saveData)
    {
        EnsureInitialized();
        return Interface.SendCommand(new BuildStorySummaryCommand(saveData));
    }

    public static string BuildSettlementSummary(MainMenuSaveData saveData)
    {
        EnsureInitialized();
        return Interface.SendCommand(new BuildSettlementSummaryCommand(saveData));
    }

    public static string BuildSectOverview(MainMenuSaveData saveData)
    {
        EnsureInitialized();
        return Interface.SendCommand(new BuildSectOverviewCommand(saveData));
    }

    public static SectHallSnapshot[] GetSectHallSnapshots(MainMenuSaveData saveData)
    {
        EnsureInitialized();
        return Interface.SendCommand(new GetSectHallSnapshotsCommand(saveData));
    }

    public static SectActionResult ExecuteSectAction(int slotIndex, MainMenuSaveData saveData, string actionId)
    {
        EnsureInitialized();
        return Interface.SendCommand(new ExecuteSectActionCommand(slotIndex, saveData, actionId));
    }

    public static MindStateResult ApplyMindStress(CombatTurnContext context, int amount)
    {
        EnsureInitialized();
        return Interface.SendCommand(new ApplyCombatMindStressCommand(context, amount));
    }

    public static MindStateResult ApplyTraversalMindStress(ExpeditionTraversalContext context, int amount)
    {
        EnsureInitialized();
        return Interface.SendCommand(new ApplyTraversalMindStressCommand(context, amount));
    }

    public static void SyncArchiveState(int slotIndex, MainMenuSaveData saveData)
    {
        EnsureInitialized();
        Interface.SendCommand(new SyncArchiveStateCommand(slotIndex, saveData));
    }

    public static WorldMapActionResult TravelToRegion(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region)
    {
        EnsureInitialized();
        return Interface.SendCommand(new TravelToRegionCommand(slotIndex, saveData, region));
    }

    public static WorldMapActionResult UpgradeProtectiveRelic(int slotIndex, MainMenuSaveData saveData)
    {
        EnsureInitialized();
        return Interface.SendCommand(new UpgradeProtectiveRelicCommand(slotIndex, saveData));
    }

    public static WorldMapActionResult UpgradeMainArtifact(int slotIndex, MainMenuSaveData saveData)
    {
        EnsureInitialized();
        return Interface.SendCommand(new UpgradeMainArtifactCommand(slotIndex, saveData));
    }

    public static WorldMapActionResult CraftRecipe(int slotIndex, MainMenuSaveData saveData, string recipeId)
    {
        EnsureInitialized();
        return Interface.SendCommand(new CraftWorldMapRecipeCommand(slotIndex, saveData, recipeId));
    }

    public static ExpeditionResolutionResult CompleteExpedition(
        int slotIndex,
        MainMenuSaveData saveData,
        WorldRegionDefinition region,
        ExpeditionHeroState hero,
        int torchlight,
        int pendingQiGain,
        int pendingCrystalGain,
        System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        EnsureInitialized();
        return Interface.SendCommand(new CompleteExpeditionRunCommand(slotIndex, saveData, region, hero, torchlight, pendingQiGain, pendingCrystalGain, pendingItemRewards));
    }

    public static ExpeditionResolutionResult RetreatExpedition(
        int slotIndex,
        MainMenuSaveData saveData,
        WorldRegionDefinition region,
        int pendingQiGain,
        int pendingCrystalGain,
        System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        EnsureInitialized();
        return Interface.SendCommand(new RetreatExpeditionRunCommand(slotIndex, saveData, region, pendingQiGain, pendingCrystalGain, pendingItemRewards));
    }

    public static ExpeditionResolutionResult FailExpedition(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region, string reason, System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        EnsureInitialized();
        return Interface.SendCommand(new FailExpeditionRunCommand(slotIndex, saveData, region, reason, pendingItemRewards));
    }

    public static System.Collections.Generic.List<ExpeditionRoomState> BuildExpeditionRooms(WorldRegionDefinition region, MainMenuSaveData saveData, System.Random random)
    {
        EnsureInitialized();
        return Interface.SendCommand(new BuildExpeditionRoomsCommand(region, saveData, random));
    }

    public static System.Collections.Generic.List<ExpeditionEnemyState> BuildEncounterEnemies(WorldRegionDefinition region, ExpeditionRoomState room, MainMenuSaveData saveData, System.Random random)
    {
        EnsureInitialized();
        return Interface.SendCommand(new BuildEncounterEnemiesCommand(region, room, saveData, random));
    }

    public static System.Collections.Generic.List<SaveItemStack> BuildEncounterLoot(CombatTurnContext context)
    {
        EnsureInitialized();
        return Interface.SendCommand(new BuildEncounterLootCommand(context));
    }

    public static System.Collections.Generic.List<SaveItemStack> BuildClearLoot(WorldRegionDefinition region, MainMenuSaveData saveData)
    {
        EnsureInitialized();
        return Interface.SendCommand(new BuildClearLootCommand(region, saveData));
    }

    public static RewardBankResult BankPendingLoot(MainMenuSaveData saveData, System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        EnsureInitialized();
        return Interface.SendCommand(new BankPendingLootCommand(saveData, pendingItemRewards));
    }

    public static void MergePendingLoot(System.Collections.Generic.List<SaveItemStack> target, System.Collections.Generic.List<SaveItemStack> incoming)
    {
        EnsureInitialized();
        Interface.SendCommand(new MergePendingLootCommand(target, incoming));
    }

    public static CombatTurnResult ResolveDirectAttackTurn(CombatTurnContext context, ExpeditionEnemyState target, int damage, string missSummary)
    {
        EnsureInitialized();
        return Interface.SendCommand(new ResolveDirectAttackTurnCommand(context, target, damage, missSummary));
    }

    public static CombatTurnResult ResolveSkillTurn(CombatTurnContext context, int skillIndex)
    {
        EnsureInitialized();
        return Interface.SendCommand(new ResolveSkillTurnCommand(context, skillIndex));
    }

    public static CombatTurnResult ResolveTalismanTurn(CombatTurnContext context)
    {
        EnsureInitialized();
        return Interface.SendCommand(new ResolveTalismanTurnCommand(context));
    }

    public static CombatTurnResult ResolveMedicineTurn(CombatTurnContext context)
    {
        EnsureInitialized();
        return Interface.SendCommand(new ResolveMedicineTurnCommand(context));
    }

    public static ExpeditionRoomActionResult ResolveRoomEvent(CombatTurnContext context)
    {
        EnsureInitialized();
        return Interface.SendCommand(new ResolveRoomEventCommand(context));
    }

    public static ExpeditionEventCardResult OpenRoomEvent(CombatTurnContext context)
    {
        EnsureInitialized();
        return Interface.SendCommand(new OpenRoomEventCommand(context));
    }

    public static ExpeditionEventOptionResult ResolveEventOption(CombatTurnContext context, string eventId, string optionId)
    {
        EnsureInitialized();
        return Interface.SendCommand(new ResolveEventOptionCommand(context, eventId, optionId));
    }

    public static ExpeditionTraversalResult EnterRoom(ExpeditionTraversalContext context)
    {
        EnsureInitialized();
        return Interface.SendCommand(new EnterExpeditionRoomCommand(context));
    }

    public static ExpeditionAdvanceResult AdvanceExpedition(ExpeditionAdvanceContext context)
    {
        EnsureInitialized();
        return Interface.SendCommand(new AdvanceExpeditionCommand(context));
    }

    public static ExpeditionLootCollectionResult CollectRoomLoot(WorldRegionDefinition region, ExpeditionRoomState room, System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        EnsureInitialized();
        return Interface.SendCommand(new CollectRoomLootCommand(region, room, pendingItemRewards));
    }

    public static ExpeditionSupportActionResult UseTorchSupply(CombatTurnContext context)
    {
        EnsureInitialized();
        return Interface.SendCommand(new UseTorchSupplyCommand(context));
    }

    public static ExpeditionSupportActionResult CampAndRecover(CombatTurnContext context)
    {
        EnsureInitialized();
        return Interface.SendCommand(new CampAndRecoverCommand(context));
    }

    public static ExpeditionSupportActionResult RecenterMind(CombatTurnContext context)
    {
        EnsureInitialized();
        return Interface.SendCommand(new RecenterMindCommand(context));
    }

    public static ExpeditionSupportActionResult SkipRoom(CombatTurnContext context)
    {
        EnsureInitialized();
        return Interface.SendCommand(new SkipRoomCommand(context));
    }

    public static void SyncExpeditionRuntime(CombatTurnContext context)
    {
        EnsureInitialized();
        Interface.SendCommand(new SyncExpeditionRuntimeCommand(context));
    }

    public static void ClearExpeditionRuntime()
    {
        EnsureInitialized();
        Interface.SendCommand(new ClearExpeditionRuntimeCommand());
    }
}
