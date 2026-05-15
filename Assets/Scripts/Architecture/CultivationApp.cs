using QFramework;

public sealed class CultivationApp : Architecture<CultivationApp>
{
    protected override void Init()
    {
        RegisterModel(new CultivationArchiveModel());
        RegisterModel(new CultivationInventoryModel());
        RegisterModel(new CultivationTaskBoardModel());
        RegisterModel(new CultivationWorldMapModel());
        RegisterModel(new CultivationExpeditionModel());
        RegisterSystem(new CultivationSaveSystem());
        RegisterSystem(new CultivationConditionSystem());
        RegisterSystem(new CultivationStorySystem());
        RegisterSystem(new CultivationMindStateSystem());
        RegisterSystem(new CultivationFactionSystem());
        RegisterSystem(new CultivationRewardSystem());
        RegisterSystem(new CultivationTaskSystem());
        RegisterSystem(new CultivationSettlementSystem());
        RegisterSystem(new CultivationSectSystem());
        RegisterSystem(new CultivationEncounterDirectorSystem());
        RegisterSystem(new CultivationWorldMapSystem());
        RegisterSystem(new CultivationBattleSystem());
        RegisterSystem(new CultivationExpeditionSystem());
        RegisterSystem(new CultivationExpeditionEventSystem());
    }

    public static void EnsureInitialized()
    {
        InitArchitecture();
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
