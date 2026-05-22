using QFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

internal static class CultivationUiAudio
{
    public static void BindButton(Button button, UnityAction action, CultivationButtonSound sound = CultivationButtonSound.Click)
    {
        BindButton(button, action, GameSound.Instance, sound);
    }

    public static void BindButton(Button button, UnityAction action, ISoundSystem soundSystem, CultivationButtonSound sound = CultivationButtonSound.Click)
    {
        if (button == null)
        {
            return;
        }

        var soundBinder = button.GetComponent<UIButtonSoundBinder>();
        button.onClick.RemoveAllListeners();
        if (action == null)
        {
            soundBinder?.RemoveBinding();
            return;
        }

        if (soundBinder != null)
        {
            soundBinder.Configure(button, sound);
            button.onClick.AddListener(action);
            return;
        }

        button.onClick.AddListener(() =>
        {
            PlayButtonSound(soundSystem, sound);
            action();
        });
    }

    public static void PlayButtonSound(ISoundSystem soundSystem, CultivationButtonSound sound)
    {
        if (soundSystem == null || sound == CultivationButtonSound.None)
        {
            return;
        }

        soundSystem.PlaySound(MapButtonSound(sound));
    }

    public static void PlayButtonSound(CultivationButtonSound sound)
    {
        PlayButtonSound(GameSound.Instance, sound);
    }

    private static SoundType MapButtonSound(CultivationButtonSound sound)
    {
        switch (sound)
        {
            case CultivationButtonSound.Confirm:
                return SoundType.Button_Confirm;
            case CultivationButtonSound.Cancel:
                return SoundType.Close;
            case CultivationButtonSound.Click:
            default:
                return SoundType.Button_Low;
        }
    }
}

public abstract class CultivationController : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture()
    {
        return CultivationApp.Interface;
    }

    protected CultivationPlayerModel PlayerModel
    {
        get { return this.GetModel<CultivationPlayerModel>(); }
    }

    protected CultivationGameModel GameModel
    {
        get { return this.GetModel<CultivationGameModel>(); }
    }

    protected ISoundSystem SoundSystem
    {
        get { return this.GetSystem<ISoundSystem>(); }
    }

    protected void BindButton(Button button, UnityAction action, CultivationButtonSound sound = CultivationButtonSound.Click)
    {
        CultivationUiAudio.BindButton(button, action, SoundSystem, sound);
    }

    protected void PlayButtonSound(CultivationButtonSound sound = CultivationButtonSound.Click)
    {
        CultivationUiAudio.PlayButtonSound(SoundSystem, sound);
    }

    protected T LoadResource<T>(string path) where T : Object
    {
        return this.GetUtility<IGameResourceService>().Load<T>(path);
    }

    protected GameObject InstantiatePrefab(string path, Transform parent = null)
    {
        return this.GetUtility<IGameResourceService>().InstantiatePrefab(path, parent);
    }

    protected UIPanel OpenGameUiPanel(GameUiPanelId panelId, IUIData uiData = null)
    {
        return this.SendCommand(new OpenGameUiPanelCommand(panelId, uiData));
    }

    protected MainMenuController OpenMainMenuPanel(MainMenuConfig config)
    {
        return this.SendCommand(new OpenMainMenuPanelCommand(config));
    }

    protected WorldMapController OpenWorldMapPanel(string gameplaySceneName, string mainSceneName)
    {
        return this.SendCommand(new OpenWorldMapPanelCommand(gameplaySceneName, mainSceneName));
    }

    protected void CloseGameUiPanel(GameUiPanelId panelId)
    {
        this.SendCommand(new CloseGameUiPanelCommand(panelId));
    }

    protected void CloseAllGameUiPanels()
    {
        this.SendCommand(new CloseAllGameUiPanelsCommand());
    }

    protected void HideGameUiPanel(GameUiPanelId panelId)
    {
        this.SendCommand(new HideGameUiPanelCommand(panelId));
    }

    protected void ShowGameUiPanel(GameUiPanelId panelId)
    {
        this.SendCommand(new ShowGameUiPanelCommand(panelId));
    }

    protected void DestroyGameUiPanel(GameUiPanelId panelId)
    {
        this.SendCommand(new DestroyGameUiPanelCommand(panelId));
    }

    protected void SetHubState(bool visible, GameHubContext context)
    {
        this.SendCommand(new SetGameHubStateCommand(visible, context));
    }

    protected void SetPlayerCompendiumVisible(bool visible)
    {
        this.SendCommand(new SetPlayerCompendiumVisibilityCommand(visible));
    }

    protected void SetPlayerCompendiumSelection(PlayerCompendiumMainTab mainTab, string sectionId = "")
    {
        this.SendCommand(new SetPlayerCompendiumSelectionCommand(mainTab, sectionId));
    }

    protected void SaveArchive(int slotIndex, MainMenuSaveData saveData)
    {
        this.SendCommand(new SaveArchiveCommand(slotIndex, saveData));
    }

    protected void SyncArchiveState(int slotIndex, MainMenuSaveData saveData)
    {
        this.SendCommand(new SyncArchiveStateCommand(slotIndex, saveData));
    }

    protected void DeleteArchive(int slotIndex)
    {
        this.SendCommand(new DeleteArchiveCommand(slotIndex));
    }

    protected CultivationArchiveSnapshot BootstrapCurrentArchive()
    {
        return this.SendCommand(new BootstrapCurrentArchiveCommand());
    }

    protected string ResolveTaskBoard(int slotIndex, MainMenuSaveData saveData)
    {
        return this.SendCommand(new ResolveTaskBoardCommand(slotIndex, saveData));
    }

    protected TaskContextSnapshot GetActiveTaskContext(MainMenuSaveData saveData)
    {
        return this.SendCommand(new GetActiveTaskContextCommand(saveData));
    }

    protected WorldMapNpcDialogueSnapshot BuildNpcDialogueSnapshot(MainMenuSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, string selectedNpcId)
    {
        return this.SendCommand(new BuildNpcDialogueSnapshotCommand(saveData, sceneType, regionId, sectHallId, selectedNpcId));
    }

    protected NpcInteractionResult ExecuteNpcDialogueChoice(int slotIndex, MainMenuSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, string npcId, string choiceId)
    {
        return this.SendCommand(new ExecuteNpcDialogueChoiceCommand(slotIndex, saveData, sceneType, regionId, sectHallId, npcId, choiceId));
    }

    protected ExpeditionView OpenExpeditionPanel()
    {
        return this.SendCommand(new OpenExpeditionPanelCommand());
    }

    protected System.Collections.Generic.List<ExpeditionRoomState> BuildExpeditionRooms(WorldRegionDefinition region, MainMenuSaveData saveData, System.Random random)
    {
        return this.SendCommand(new BuildExpeditionRoomsCommand(region, saveData, random));
    }

    protected System.Collections.Generic.List<ExpeditionEnemyState> BuildEncounterEnemies(WorldRegionDefinition region, ExpeditionRoomState room, MainMenuSaveData saveData, System.Random random)
    {
        return this.SendCommand(new BuildEncounterEnemiesCommand(region, room, saveData, random));
    }

    protected CombatTurnResult ResolveDirectAttackTurn(CombatTurnContext context, ExpeditionEnemyState target, int damage, string missSummary)
    {
        return this.SendCommand(new ResolveDirectAttackTurnCommand(context, target, damage, missSummary));
    }

    protected CombatTurnResult ResolveSkillTurn(CombatTurnContext context, int skillIndex)
    {
        return this.SendCommand(new ResolveSkillTurnCommand(context, skillIndex));
    }

    protected CombatTurnResult ResolveTalismanTurn(CombatTurnContext context)
    {
        return this.SendCommand(new ResolveTalismanTurnCommand(context));
    }

    protected CombatTurnResult ResolveMedicineTurn(CombatTurnContext context)
    {
        return this.SendCommand(new ResolveMedicineTurnCommand(context));
    }

    protected ExpeditionSupportActionResult UseTorchSupply(CombatTurnContext context)
    {
        return this.SendCommand(new UseTorchSupplyCommand(context));
    }

    protected ExpeditionSupportActionResult CampAndRecover(CombatTurnContext context)
    {
        return this.SendCommand(new CampAndRecoverCommand(context));
    }

    protected ExpeditionSupportActionResult RecenterMind(CombatTurnContext context)
    {
        return this.SendCommand(new RecenterMindCommand(context));
    }

    protected ExpeditionSupportActionResult SkipRoom(CombatTurnContext context)
    {
        return this.SendCommand(new SkipRoomCommand(context));
    }

    protected EnemyIntentPreview[] PreviewEnemyIntents(CombatTurnContext context)
    {
        return this.SendCommand(new PreviewEnemyIntentsCommand(context));
    }

    protected ExpeditionEventCardResult OpenRoomEvent(CombatTurnContext context)
    {
        return this.SendCommand(new OpenRoomEventCommand(context));
    }

    protected ExpeditionEventOptionResult ResolveEventOption(CombatTurnContext context, string eventId, string optionId)
    {
        return this.SendCommand(new ResolveEventOptionCommand(context, eventId, optionId));
    }

    protected ExpeditionTraversalResult EnterExpeditionRoom(ExpeditionTraversalContext context)
    {
        return this.SendCommand(new EnterExpeditionRoomCommand(context));
    }

    protected ExpeditionAdvanceResult AdvanceExpedition(ExpeditionAdvanceContext context)
    {
        return this.SendCommand(new AdvanceExpeditionCommand(context));
    }

    protected ExpeditionResolutionResult CompleteExpedition(
        int slotIndex,
        MainMenuSaveData saveData,
        WorldRegionDefinition region,
        ExpeditionHeroState hero,
        int torchlight,
        int pendingQiGain,
        int pendingCrystalGain,
        System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        return this.SendCommand(new CompleteExpeditionRunCommand(
            slotIndex,
            saveData,
            region,
            hero,
            torchlight,
            pendingQiGain,
            pendingCrystalGain,
            pendingItemRewards));
    }

    protected ExpeditionResolutionResult RetreatExpedition(
        int slotIndex,
        MainMenuSaveData saveData,
        WorldRegionDefinition region,
        int pendingQiGain,
        int pendingCrystalGain,
        System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        return this.SendCommand(new RetreatExpeditionRunCommand(
            slotIndex,
            saveData,
            region,
            pendingQiGain,
            pendingCrystalGain,
            pendingItemRewards));
    }

    protected ExpeditionResolutionResult FailExpedition(
        int slotIndex,
        MainMenuSaveData saveData,
        WorldRegionDefinition region,
        string reason,
        System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        return this.SendCommand(new FailExpeditionRunCommand(slotIndex, saveData, region, reason, pendingItemRewards));
    }

    protected MindStateResult ApplyCombatMindStress(CombatTurnContext context, int amount)
    {
        return this.SendCommand(new ApplyCombatMindStressCommand(context, amount));
    }

    protected void SyncExpeditionRuntime(CombatTurnContext context)
    {
        this.SendCommand(new SyncExpeditionRuntimeCommand(context));
    }

    protected void ClearExpeditionRuntime()
    {
        this.SendCommand(new ClearExpeditionRuntimeCommand());
    }

    protected WorldMapActionResult TravelToRegion(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region)
    {
        return this.SendCommand(new TravelToRegionCommand(slotIndex, saveData, region));
    }

    protected WorldMapActionResult UpgradeProtectiveRelic(int slotIndex, MainMenuSaveData saveData)
    {
        return this.SendCommand(new UpgradeProtectiveRelicCommand(slotIndex, saveData));
    }

    protected WorldMapActionResult UpgradeMainArtifact(int slotIndex, MainMenuSaveData saveData)
    {
        return this.SendCommand(new UpgradeMainArtifactCommand(slotIndex, saveData));
    }

    protected WorldMapActionResult CraftWorldMapRecipe(int slotIndex, MainMenuSaveData saveData, string recipeId)
    {
        return this.SendCommand(new CraftWorldMapRecipeCommand(slotIndex, saveData, recipeId));
    }

    protected string BuildSettlementSummary(MainMenuSaveData saveData)
    {
        return this.SendCommand(new BuildSettlementSummaryCommand(saveData));
    }

    protected SectHallSnapshot[] GetSectHallSnapshots(MainMenuSaveData saveData)
    {
        return this.SendCommand(new GetSectHallSnapshotsCommand(saveData));
    }

    protected string BuildSectOverview(MainMenuSaveData saveData)
    {
        return this.SendCommand(new BuildSectOverviewCommand(saveData));
    }

    protected SectActionResult ExecuteSectAction(int slotIndex, MainMenuSaveData saveData, string actionId)
    {
        return this.SendCommand(new ExecuteSectActionCommand(slotIndex, saveData, actionId));
    }

    protected float GetMusicVolume()
    {
        return this.GetUtility<IGameSettingsService>().MusicVolume.Value;
    }

    protected float GetSfxVolume()
    {
        return this.GetUtility<IGameSettingsService>().SfxVolume.Value;
    }

    protected float GetVoiceVolume()
    {
        return this.GetUtility<IGameSettingsService>().VoiceVolume.Value;
    }

    protected bool IsFullscreen()
    {
        return this.GetUtility<IGameSettingsService>().Fullscreen.Value;
    }

    protected void ApplyUserSettings()
    {
        this.GetUtility<IGameSettingsService>().ApplyRuntimeSettings();
    }

    protected void SetMusicVolume(float value)
    {
        this.GetUtility<IGameSettingsService>().SetMusicVolume(value);
    }

    protected void SetSfxVolume(float value)
    {
        this.GetUtility<IGameSettingsService>().SetSfxVolume(value);
    }

    protected void SetVoiceVolume(float value)
    {
        this.GetUtility<IGameSettingsService>().SetVoiceVolume(value);
    }

    protected void SetFullscreen(bool fullscreen)
    {
        this.GetUtility<IGameSettingsService>().SetFullscreen(fullscreen);
    }

    protected void ResetUserSettings()
    {
        this.GetUtility<IGameSettingsService>().Reset();
    }

    protected void SetMusicDuck(string reason, bool enabled, float duckDb = 8f)
    {
        this.GetUtility<IGameAudioService>().SetMusicDuck(reason, enabled, duckDb);
    }

    protected void PlayMainMenuMusic()
    {
        SoundSystem.PlayMainMenuMusic();
    }

    protected void PlayWorldMapMusic()
    {
        SoundSystem.PlayWorldMapMusic();
    }

    protected void PlayExpeditionMusic(WorldRegionDefinition region)
    {
        SoundSystem.PlayExpeditionMusic(region);
    }

    protected void PlaySound(SoundType type)
    {
        SoundSystem.PlaySound(type);
    }

    protected void LogError(string message)
    {
        this.GetUtility<IGameLogService>().Error(message);
    }

    protected CultivationMessagePopupPanel ShowMessagePopup(
        string message,
        string title = "",
        float duration = 2.4f,
        CultivationMessagePopupStyle style = CultivationMessagePopupStyle.Info)
    {
        return this.GetUtility<IGameUiService>().ShowMessagePopup(message, title, duration, style);
    }

    protected CultivationMessagePopupPanel ShowInfoMessage(string message, float duration = 2.2f)
    {
        return ShowMessagePopup(message, "提示", duration, CultivationMessagePopupStyle.Info);
    }

    protected CultivationMessagePopupPanel ShowWarningMessage(string message, float duration = 2.8f)
    {
        return ShowMessagePopup(message, "注意", duration, CultivationMessagePopupStyle.Warning);
    }

    protected CultivationMessagePopupPanel ShowErrorMessage(string message, float duration = 3.2f)
    {
        return ShowMessagePopup(message, "异常", duration, CultivationMessagePopupStyle.Error);
    }

    protected CultivationMessagePopupPanel ShowSuccessMessage(string message, float duration = 2.4f)
    {
        return ShowMessagePopup(message, "完成", duration, CultivationMessagePopupStyle.Success);
    }
}

public abstract class CultivationUIPanel : UIPanel, IController
{
    public IArchitecture GetArchitecture()
    {
        return CultivationApp.Interface;
    }

    protected CultivationPlayerModel PlayerModel
    {
        get { return this.GetModel<CultivationPlayerModel>(); }
    }

    protected CultivationGameModel GameModel
    {
        get { return this.GetModel<CultivationGameModel>(); }
    }

    protected ISoundSystem SoundSystem
    {
        get { return this.GetSystem<ISoundSystem>(); }
    }

    protected void BindButton(Button button, UnityAction action, CultivationButtonSound sound = CultivationButtonSound.Click)
    {
        CultivationUiAudio.BindButton(button, action, SoundSystem, sound);
    }

    protected void PlayButtonSound(CultivationButtonSound sound = CultivationButtonSound.Click)
    {
        CultivationUiAudio.PlayButtonSound(SoundSystem, sound);
    }

    protected T LoadResource<T>(string path) where T : Object
    {
        return this.GetUtility<IGameResourceService>().Load<T>(path);
    }

    protected GameObject InstantiatePrefab(string path, Transform parent = null)
    {
        return this.GetUtility<IGameResourceService>().InstantiatePrefab(path, parent);
    }

    protected UIPanel OpenGameUiPanel(GameUiPanelId panelId, IUIData uiData = null)
    {
        return this.SendCommand(new OpenGameUiPanelCommand(panelId, uiData));
    }

    protected MainMenuController OpenMainMenuPanel(MainMenuConfig config)
    {
        return this.SendCommand(new OpenMainMenuPanelCommand(config));
    }

    protected WorldMapController OpenWorldMapPanel(string gameplaySceneName, string mainSceneName)
    {
        return this.SendCommand(new OpenWorldMapPanelCommand(gameplaySceneName, mainSceneName));
    }

    protected void CloseGameUiPanel(GameUiPanelId panelId)
    {
        this.SendCommand(new CloseGameUiPanelCommand(panelId));
    }

    protected void CloseAllGameUiPanels()
    {
        this.SendCommand(new CloseAllGameUiPanelsCommand());
    }

    protected void HideGameUiPanel(GameUiPanelId panelId)
    {
        this.SendCommand(new HideGameUiPanelCommand(panelId));
    }

    protected void ShowGameUiPanel(GameUiPanelId panelId)
    {
        this.SendCommand(new ShowGameUiPanelCommand(panelId));
    }

    protected void DestroyGameUiPanel(GameUiPanelId panelId)
    {
        this.SendCommand(new DestroyGameUiPanelCommand(panelId));
    }

    protected void SetHubState(bool visible, GameHubContext context)
    {
        this.SendCommand(new SetGameHubStateCommand(visible, context));
    }

    protected void SetPlayerCompendiumVisible(bool visible)
    {
        this.SendCommand(new SetPlayerCompendiumVisibilityCommand(visible));
    }

    protected void SetPlayerCompendiumSelection(PlayerCompendiumMainTab mainTab, string sectionId = "")
    {
        this.SendCommand(new SetPlayerCompendiumSelectionCommand(mainTab, sectionId));
    }

    protected void SaveArchive(int slotIndex, MainMenuSaveData saveData)
    {
        this.SendCommand(new SaveArchiveCommand(slotIndex, saveData));
    }

    protected void SyncArchiveState(int slotIndex, MainMenuSaveData saveData)
    {
        this.SendCommand(new SyncArchiveStateCommand(slotIndex, saveData));
    }

    protected void DeleteArchive(int slotIndex)
    {
        this.SendCommand(new DeleteArchiveCommand(slotIndex));
    }

    protected CultivationArchiveSnapshot BootstrapCurrentArchive()
    {
        return this.SendCommand(new BootstrapCurrentArchiveCommand());
    }

    protected string ResolveTaskBoard(int slotIndex, MainMenuSaveData saveData)
    {
        return this.SendCommand(new ResolveTaskBoardCommand(slotIndex, saveData));
    }

    protected TaskContextSnapshot GetActiveTaskContext(MainMenuSaveData saveData)
    {
        return this.SendCommand(new GetActiveTaskContextCommand(saveData));
    }

    protected WorldMapNpcDialogueSnapshot BuildNpcDialogueSnapshot(MainMenuSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, string selectedNpcId)
    {
        return this.SendCommand(new BuildNpcDialogueSnapshotCommand(saveData, sceneType, regionId, sectHallId, selectedNpcId));
    }

    protected NpcInteractionResult ExecuteNpcDialogueChoice(int slotIndex, MainMenuSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, string npcId, string choiceId)
    {
        return this.SendCommand(new ExecuteNpcDialogueChoiceCommand(slotIndex, saveData, sceneType, regionId, sectHallId, npcId, choiceId));
    }

    protected ExpeditionView OpenExpeditionPanel()
    {
        return this.SendCommand(new OpenExpeditionPanelCommand());
    }

    protected System.Collections.Generic.List<ExpeditionRoomState> BuildExpeditionRooms(WorldRegionDefinition region, MainMenuSaveData saveData, System.Random random)
    {
        return this.SendCommand(new BuildExpeditionRoomsCommand(region, saveData, random));
    }

    protected System.Collections.Generic.List<ExpeditionEnemyState> BuildEncounterEnemies(WorldRegionDefinition region, ExpeditionRoomState room, MainMenuSaveData saveData, System.Random random)
    {
        return this.SendCommand(new BuildEncounterEnemiesCommand(region, room, saveData, random));
    }

    protected CombatTurnResult ResolveDirectAttackTurn(CombatTurnContext context, ExpeditionEnemyState target, int damage, string missSummary)
    {
        return this.SendCommand(new ResolveDirectAttackTurnCommand(context, target, damage, missSummary));
    }

    protected CombatTurnResult ResolveSkillTurn(CombatTurnContext context, int skillIndex)
    {
        return this.SendCommand(new ResolveSkillTurnCommand(context, skillIndex));
    }

    protected CombatTurnResult ResolveTalismanTurn(CombatTurnContext context)
    {
        return this.SendCommand(new ResolveTalismanTurnCommand(context));
    }

    protected CombatTurnResult ResolveMedicineTurn(CombatTurnContext context)
    {
        return this.SendCommand(new ResolveMedicineTurnCommand(context));
    }

    protected ExpeditionSupportActionResult UseTorchSupply(CombatTurnContext context)
    {
        return this.SendCommand(new UseTorchSupplyCommand(context));
    }

    protected ExpeditionSupportActionResult CampAndRecover(CombatTurnContext context)
    {
        return this.SendCommand(new CampAndRecoverCommand(context));
    }

    protected ExpeditionSupportActionResult RecenterMind(CombatTurnContext context)
    {
        return this.SendCommand(new RecenterMindCommand(context));
    }

    protected ExpeditionSupportActionResult SkipRoom(CombatTurnContext context)
    {
        return this.SendCommand(new SkipRoomCommand(context));
    }

    protected ExpeditionEventCardResult OpenRoomEvent(CombatTurnContext context)
    {
        return this.SendCommand(new OpenRoomEventCommand(context));
    }

    protected ExpeditionEventOptionResult ResolveEventOption(CombatTurnContext context, string eventId, string optionId)
    {
        return this.SendCommand(new ResolveEventOptionCommand(context, eventId, optionId));
    }

    protected ExpeditionTraversalResult EnterExpeditionRoom(ExpeditionTraversalContext context)
    {
        return this.SendCommand(new EnterExpeditionRoomCommand(context));
    }

    protected ExpeditionAdvanceResult AdvanceExpedition(ExpeditionAdvanceContext context)
    {
        return this.SendCommand(new AdvanceExpeditionCommand(context));
    }

    protected ExpeditionResolutionResult CompleteExpedition(
        int slotIndex,
        MainMenuSaveData saveData,
        WorldRegionDefinition region,
        ExpeditionHeroState hero,
        int torchlight,
        int pendingQiGain,
        int pendingCrystalGain,
        System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        return this.SendCommand(new CompleteExpeditionRunCommand(
            slotIndex,
            saveData,
            region,
            hero,
            torchlight,
            pendingQiGain,
            pendingCrystalGain,
            pendingItemRewards));
    }

    protected ExpeditionResolutionResult RetreatExpedition(
        int slotIndex,
        MainMenuSaveData saveData,
        WorldRegionDefinition region,
        int pendingQiGain,
        int pendingCrystalGain,
        System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        return this.SendCommand(new RetreatExpeditionRunCommand(
            slotIndex,
            saveData,
            region,
            pendingQiGain,
            pendingCrystalGain,
            pendingItemRewards));
    }

    protected ExpeditionResolutionResult FailExpedition(
        int slotIndex,
        MainMenuSaveData saveData,
        WorldRegionDefinition region,
        string reason,
        System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        return this.SendCommand(new FailExpeditionRunCommand(slotIndex, saveData, region, reason, pendingItemRewards));
    }

    protected MindStateResult ApplyCombatMindStress(CombatTurnContext context, int amount)
    {
        return this.SendCommand(new ApplyCombatMindStressCommand(context, amount));
    }

    protected void SyncExpeditionRuntime(CombatTurnContext context)
    {
        this.SendCommand(new SyncExpeditionRuntimeCommand(context));
    }

    protected void ClearExpeditionRuntime()
    {
        this.SendCommand(new ClearExpeditionRuntimeCommand());
    }

    protected WorldMapActionResult TravelToRegion(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region)
    {
        return this.SendCommand(new TravelToRegionCommand(slotIndex, saveData, region));
    }

    protected WorldMapActionResult UpgradeProtectiveRelic(int slotIndex, MainMenuSaveData saveData)
    {
        return this.SendCommand(new UpgradeProtectiveRelicCommand(slotIndex, saveData));
    }

    protected WorldMapActionResult UpgradeMainArtifact(int slotIndex, MainMenuSaveData saveData)
    {
        return this.SendCommand(new UpgradeMainArtifactCommand(slotIndex, saveData));
    }

    protected WorldMapActionResult CraftWorldMapRecipe(int slotIndex, MainMenuSaveData saveData, string recipeId)
    {
        return this.SendCommand(new CraftWorldMapRecipeCommand(slotIndex, saveData, recipeId));
    }

    protected string BuildSettlementSummary(MainMenuSaveData saveData)
    {
        return this.SendCommand(new BuildSettlementSummaryCommand(saveData));
    }

    protected SectHallSnapshot[] GetSectHallSnapshots(MainMenuSaveData saveData)
    {
        return this.SendCommand(new GetSectHallSnapshotsCommand(saveData));
    }

    protected string BuildSectOverview(MainMenuSaveData saveData)
    {
        return this.SendCommand(new BuildSectOverviewCommand(saveData));
    }

    protected SectActionResult ExecuteSectAction(int slotIndex, MainMenuSaveData saveData, string actionId)
    {
        return this.SendCommand(new ExecuteSectActionCommand(slotIndex, saveData, actionId));
    }

    protected float GetMusicVolume()
    {
        return this.GetUtility<IGameSettingsService>().MusicVolume.Value;
    }

    protected float GetSfxVolume()
    {
        return this.GetUtility<IGameSettingsService>().SfxVolume.Value;
    }

    protected float GetVoiceVolume()
    {
        return this.GetUtility<IGameSettingsService>().VoiceVolume.Value;
    }

    protected bool IsFullscreen()
    {
        return this.GetUtility<IGameSettingsService>().Fullscreen.Value;
    }

    protected void ApplyUserSettings()
    {
        this.GetUtility<IGameSettingsService>().ApplyRuntimeSettings();
    }

    protected void SetMusicVolume(float value)
    {
        this.GetUtility<IGameSettingsService>().SetMusicVolume(value);
    }

    protected void SetSfxVolume(float value)
    {
        this.GetUtility<IGameSettingsService>().SetSfxVolume(value);
    }

    protected void SetVoiceVolume(float value)
    {
        this.GetUtility<IGameSettingsService>().SetVoiceVolume(value);
    }

    protected void SetFullscreen(bool fullscreen)
    {
        this.GetUtility<IGameSettingsService>().SetFullscreen(fullscreen);
    }

    protected void ResetUserSettings()
    {
        this.GetUtility<IGameSettingsService>().Reset();
    }

    protected void SetMusicDuck(string reason, bool enabled, float duckDb = 8f)
    {
        this.GetUtility<IGameAudioService>().SetMusicDuck(reason, enabled, duckDb);
    }

    protected void PlayMainMenuMusic()
    {
        SoundSystem.PlayMainMenuMusic();
    }

    protected void PlayWorldMapMusic()
    {
        SoundSystem.PlayWorldMapMusic();
    }

    protected void PlayExpeditionMusic(WorldRegionDefinition region)
    {
        SoundSystem.PlayExpeditionMusic(region);
    }

    protected void PlaySound(SoundType type)
    {
        SoundSystem.PlaySound(type);
    }

    protected void LogError(string message)
    {
        this.GetUtility<IGameLogService>().Error(message);
    }

    protected CultivationMessagePopupPanel ShowMessagePopup(
        string message,
        string title = "",
        float duration = 2.4f,
        CultivationMessagePopupStyle style = CultivationMessagePopupStyle.Info)
    {
        return this.GetUtility<IGameUiService>().ShowMessagePopup(message, title, duration, style);
    }

    protected CultivationMessagePopupPanel ShowInfoMessage(string message, float duration = 2.2f)
    {
        return ShowMessagePopup(message, "提示", duration, CultivationMessagePopupStyle.Info);
    }

    protected CultivationMessagePopupPanel ShowWarningMessage(string message, float duration = 2.8f)
    {
        return ShowMessagePopup(message, "注意", duration, CultivationMessagePopupStyle.Warning);
    }

    protected CultivationMessagePopupPanel ShowErrorMessage(string message, float duration = 3.2f)
    {
        return ShowMessagePopup(message, "异常", duration, CultivationMessagePopupStyle.Error);
    }

    protected CultivationMessagePopupPanel ShowSuccessMessage(string message, float duration = 2.4f)
    {
        return ShowMessagePopup(message, "完成", duration, CultivationMessagePopupStyle.Success);
    }
}
