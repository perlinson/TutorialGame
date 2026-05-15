using QFramework;

public sealed class BootstrapCurrentArchiveCommand : AbstractCommand<CultivationArchiveSnapshot>
{
    protected override CultivationArchiveSnapshot OnExecute()
    {
        return this.GetSystem<CultivationSaveSystem>().BootstrapCurrentArchive();
    }
}

public sealed class SaveArchiveCommand : AbstractCommand
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;

    public SaveArchiveCommand(int slotIndex, MainMenuSaveData saveData)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
    }

    protected override void OnExecute()
    {
        this.GetSystem<CultivationSaveSystem>().SaveArchive(slotIndex, saveData);
    }
}

public sealed class DeleteArchiveCommand : AbstractCommand
{
    private readonly int slotIndex;

    public DeleteArchiveCommand(int slotIndex)
    {
        this.slotIndex = slotIndex;
    }

    protected override void OnExecute()
    {
        this.GetSystem<CultivationSaveSystem>().DeleteArchive(slotIndex);
    }
}

public sealed class ResolveTaskBoardCommand : AbstractCommand<string>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;

    public ResolveTaskBoardCommand(int slotIndex, MainMenuSaveData saveData)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
    }

    protected override string OnExecute()
    {
        return this.GetSystem<CultivationSaveSystem>().ResolveTaskBoard(slotIndex, saveData);
    }
}

public sealed class GetActiveTaskContextCommand : AbstractCommand<TaskContextSnapshot>
{
    private readonly MainMenuSaveData saveData;

    public GetActiveTaskContextCommand(MainMenuSaveData saveData)
    {
        this.saveData = saveData;
    }

    protected override TaskContextSnapshot OnExecute()
    {
        return this.GetSystem<CultivationSaveSystem>().GetActiveTaskContext(saveData);
    }
}

public sealed class ClaimActiveTaskCommand : AbstractCommand<string>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;

    public ClaimActiveTaskCommand(int slotIndex, MainMenuSaveData saveData)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
    }

    protected override string OnExecute()
    {
        return this.GetSystem<CultivationSaveSystem>().ClaimActiveTask(slotIndex, saveData);
    }
}

public sealed class RecordTaskProgressCommand : AbstractCommand<TaskProgressResult>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;
    private readonly TaskProgressSignal signal;

    public RecordTaskProgressCommand(int slotIndex, MainMenuSaveData saveData, TaskProgressSignal signal)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
        this.signal = signal;
    }

    protected override TaskProgressResult OnExecute()
    {
        return this.GetSystem<CultivationSaveSystem>().RecordTaskProgress(slotIndex, saveData, signal);
    }
}

public sealed class RecordTaskProgressDirectCommand : AbstractCommand<TaskProgressResult>
{
    private readonly MainMenuSaveData saveData;
    private readonly TaskProgressSignal signal;

    public RecordTaskProgressDirectCommand(MainMenuSaveData saveData, TaskProgressSignal signal)
    {
        this.saveData = saveData;
        this.signal = signal;
    }

    protected override TaskProgressResult OnExecute()
    {
        return this.GetSystem<CultivationTaskSystem>().RecordProgress(saveData, signal);
    }
}

public sealed class RecordFactionDefeatCommand : AbstractCommand<FactionReputationSnapshot>
{
    private readonly MainMenuSaveData saveData;
    private readonly ExpeditionEnemyFaction faction;
    private readonly string regionId;
    private readonly int count;

    public RecordFactionDefeatCommand(MainMenuSaveData saveData, ExpeditionEnemyFaction faction, string regionId, int count)
    {
        this.saveData = saveData;
        this.faction = faction;
        this.regionId = regionId;
        this.count = count;
    }

    protected override FactionReputationSnapshot OnExecute()
    {
        return this.GetSystem<CultivationFactionSystem>().RecordDefeat(saveData, faction, regionId, count);
    }
}

public sealed class GetFactionSnapshotCommand : AbstractCommand<FactionReputationSnapshot>
{
    private readonly MainMenuSaveData saveData;
    private readonly ExpeditionEnemyFaction faction;

    public GetFactionSnapshotCommand(MainMenuSaveData saveData, ExpeditionEnemyFaction faction)
    {
        this.saveData = saveData;
        this.faction = faction;
    }

    protected override FactionReputationSnapshot OnExecute()
    {
        return this.GetSystem<CultivationFactionSystem>().GetSnapshot(saveData, faction);
    }
}

public sealed class RecordStorySignalCommand : AbstractCommand<StorySignalResult>
{
    private readonly MainMenuSaveData saveData;
    private readonly StorySignal signal;

    public RecordStorySignalCommand(MainMenuSaveData saveData, StorySignal signal)
    {
        this.saveData = saveData;
        this.signal = signal;
    }

    protected override StorySignalResult OnExecute()
    {
        return this.GetSystem<CultivationStorySystem>().RecordSignal(saveData, signal);
    }
}

public sealed class BuildSettlementSummaryCommand : AbstractCommand<string>
{
    private readonly MainMenuSaveData saveData;

    public BuildSettlementSummaryCommand(MainMenuSaveData saveData)
    {
        this.saveData = saveData;
    }

    protected override string OnExecute()
    {
        return this.GetSystem<CultivationSettlementSystem>().BuildSettlementSummary(saveData);
    }
}

public sealed class BuildStorySummaryCommand : AbstractCommand<string>
{
    private readonly MainMenuSaveData saveData;

    public BuildStorySummaryCommand(MainMenuSaveData saveData)
    {
        this.saveData = saveData;
    }

    protected override string OnExecute()
    {
        return this.GetSystem<CultivationStorySystem>().BuildStorySummary(saveData);
    }
}

public sealed class BuildSectOverviewCommand : AbstractCommand<string>
{
    private readonly MainMenuSaveData saveData;

    public BuildSectOverviewCommand(MainMenuSaveData saveData)
    {
        this.saveData = saveData;
    }

    protected override string OnExecute()
    {
        return this.GetSystem<CultivationSectSystem>().BuildSectOverview(saveData);
    }
}

public sealed class GetSectHallSnapshotsCommand : AbstractCommand<SectHallSnapshot[]>
{
    private readonly MainMenuSaveData saveData;

    public GetSectHallSnapshotsCommand(MainMenuSaveData saveData)
    {
        this.saveData = saveData;
    }

    protected override SectHallSnapshot[] OnExecute()
    {
        return this.GetSystem<CultivationSectSystem>().GetHallSnapshots(saveData);
    }
}

public sealed class ExecuteSectActionCommand : AbstractCommand<SectActionResult>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;
    private readonly string actionId;

    public ExecuteSectActionCommand(int slotIndex, MainMenuSaveData saveData, string actionId)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
        this.actionId = actionId;
    }

    protected override SectActionResult OnExecute()
    {
        return this.GetSystem<CultivationSectSystem>().ExecuteAction(slotIndex, saveData, actionId);
    }
}

public sealed class ApplyCombatMindStressCommand : AbstractCommand<MindStateResult>
{
    private readonly CombatTurnContext context;
    private readonly int amount;

    public ApplyCombatMindStressCommand(CombatTurnContext context, int amount)
    {
        this.context = context;
        this.amount = amount;
    }

    protected override MindStateResult OnExecute()
    {
        return this.GetSystem<CultivationMindStateSystem>().ApplyStress(context, amount);
    }
}

public sealed class ApplyTraversalMindStressCommand : AbstractCommand<MindStateResult>
{
    private readonly ExpeditionTraversalContext context;
    private readonly int amount;

    public ApplyTraversalMindStressCommand(ExpeditionTraversalContext context, int amount)
    {
        this.context = context;
        this.amount = amount;
    }

    protected override MindStateResult OnExecute()
    {
        return this.GetSystem<CultivationMindStateSystem>().ApplyStress(context, amount);
    }
}

public sealed class SyncArchiveStateCommand : AbstractCommand
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;

    public SyncArchiveStateCommand(int slotIndex, MainMenuSaveData saveData)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
    }

    protected override void OnExecute()
    {
        this.GetSystem<CultivationSaveSystem>().SyncArchiveState(slotIndex, saveData);
    }
}

public sealed class TravelToRegionCommand : AbstractCommand<WorldMapActionResult>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;
    private readonly WorldRegionDefinition region;

    public TravelToRegionCommand(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
        this.region = region;
    }

    protected override WorldMapActionResult OnExecute()
    {
        return this.GetSystem<CultivationWorldMapSystem>().TravelToRegion(slotIndex, saveData, region);
    }
}

public sealed class UpgradeProtectiveRelicCommand : AbstractCommand<WorldMapActionResult>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;

    public UpgradeProtectiveRelicCommand(int slotIndex, MainMenuSaveData saveData)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
    }

    protected override WorldMapActionResult OnExecute()
    {
        return this.GetSystem<CultivationWorldMapSystem>().UpgradeProtectiveRelic(slotIndex, saveData);
    }
}

public sealed class UpgradeMainArtifactCommand : AbstractCommand<WorldMapActionResult>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;

    public UpgradeMainArtifactCommand(int slotIndex, MainMenuSaveData saveData)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
    }

    protected override WorldMapActionResult OnExecute()
    {
        return this.GetSystem<CultivationWorldMapSystem>().UpgradeMainArtifact(slotIndex, saveData);
    }
}

public sealed class CraftWorldMapRecipeCommand : AbstractCommand<WorldMapActionResult>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;
    private readonly string recipeId;

    public CraftWorldMapRecipeCommand(int slotIndex, MainMenuSaveData saveData, string recipeId)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
        this.recipeId = recipeId;
    }

    protected override WorldMapActionResult OnExecute()
    {
        return this.GetSystem<CultivationWorldMapSystem>().CraftRecipe(slotIndex, saveData, recipeId);
    }
}

public sealed class CompleteExpeditionRunCommand : AbstractCommand<ExpeditionResolutionResult>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;
    private readonly WorldRegionDefinition region;
    private readonly ExpeditionHeroState hero;
    private readonly int torchlight;
    private readonly int pendingQiGain;
    private readonly int pendingCrystalGain;
    private readonly System.Collections.Generic.List<SaveItemStack> pendingItemRewards;

    public CompleteExpeditionRunCommand(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region, ExpeditionHeroState hero, int torchlight, int pendingQiGain, int pendingCrystalGain, System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
        this.region = region;
        this.hero = hero;
        this.torchlight = torchlight;
        this.pendingQiGain = pendingQiGain;
        this.pendingCrystalGain = pendingCrystalGain;
        this.pendingItemRewards = pendingItemRewards;
    }

    protected override ExpeditionResolutionResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().CompleteExpedition(slotIndex, saveData, region, hero, torchlight, pendingQiGain, pendingCrystalGain, pendingItemRewards);
    }
}

public sealed class RetreatExpeditionRunCommand : AbstractCommand<ExpeditionResolutionResult>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;
    private readonly WorldRegionDefinition region;
    private readonly int pendingQiGain;
    private readonly int pendingCrystalGain;
    private readonly System.Collections.Generic.List<SaveItemStack> pendingItemRewards;

    public RetreatExpeditionRunCommand(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region, int pendingQiGain, int pendingCrystalGain, System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
        this.region = region;
        this.pendingQiGain = pendingQiGain;
        this.pendingCrystalGain = pendingCrystalGain;
        this.pendingItemRewards = pendingItemRewards;
    }

    protected override ExpeditionResolutionResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().RetreatExpedition(slotIndex, saveData, region, pendingQiGain, pendingCrystalGain, pendingItemRewards);
    }
}

public sealed class FailExpeditionRunCommand : AbstractCommand<ExpeditionResolutionResult>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;
    private readonly WorldRegionDefinition region;
    private readonly string reason;
    private readonly System.Collections.Generic.List<SaveItemStack> pendingItemRewards;

    public FailExpeditionRunCommand(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region, string reason, System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
        this.region = region;
        this.reason = reason;
        this.pendingItemRewards = pendingItemRewards;
    }

    protected override ExpeditionResolutionResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().FailExpedition(slotIndex, saveData, region, reason, pendingItemRewards);
    }
}

public sealed class BuildExpeditionRoomsCommand : AbstractCommand<System.Collections.Generic.List<ExpeditionRoomState>>
{
    private readonly WorldRegionDefinition region;
    private readonly MainMenuSaveData saveData;
    private readonly System.Random random;

    public BuildExpeditionRoomsCommand(WorldRegionDefinition region, MainMenuSaveData saveData, System.Random random)
    {
        this.region = region;
        this.saveData = saveData;
        this.random = random;
    }

    protected override System.Collections.Generic.List<ExpeditionRoomState> OnExecute()
    {
        return this.GetSystem<CultivationEncounterDirectorSystem>().BuildRooms(region, saveData, random);
    }
}

public sealed class BuildEncounterEnemiesCommand : AbstractCommand<System.Collections.Generic.List<ExpeditionEnemyState>>
{
    private readonly WorldRegionDefinition region;
    private readonly ExpeditionRoomState room;
    private readonly MainMenuSaveData saveData;
    private readonly System.Random random;

    public BuildEncounterEnemiesCommand(WorldRegionDefinition region, ExpeditionRoomState room, MainMenuSaveData saveData, System.Random random)
    {
        this.region = region;
        this.room = room;
        this.saveData = saveData;
        this.random = random;
    }

    protected override System.Collections.Generic.List<ExpeditionEnemyState> OnExecute()
    {
        return this.GetSystem<CultivationEncounterDirectorSystem>().BuildEnemies(region, room, saveData, random);
    }
}

public sealed class BuildEncounterLootCommand : AbstractCommand<System.Collections.Generic.List<SaveItemStack>>
{
    private readonly CombatTurnContext context;

    public BuildEncounterLootCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override System.Collections.Generic.List<SaveItemStack> OnExecute()
    {
        return this.GetSystem<CultivationRewardSystem>().BuildEncounterLoot(context);
    }
}

public sealed class BuildClearLootCommand : AbstractCommand<System.Collections.Generic.List<SaveItemStack>>
{
    private readonly WorldRegionDefinition region;
    private readonly MainMenuSaveData saveData;

    public BuildClearLootCommand(WorldRegionDefinition region, MainMenuSaveData saveData)
    {
        this.region = region;
        this.saveData = saveData;
    }

    protected override System.Collections.Generic.List<SaveItemStack> OnExecute()
    {
        return this.GetSystem<CultivationRewardSystem>().BuildClearLoot(region, saveData);
    }
}

public sealed class BankPendingLootCommand : AbstractCommand<RewardBankResult>
{
    private readonly MainMenuSaveData saveData;
    private readonly System.Collections.Generic.List<SaveItemStack> pendingItemRewards;

    public BankPendingLootCommand(MainMenuSaveData saveData, System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        this.saveData = saveData;
        this.pendingItemRewards = pendingItemRewards;
    }

    protected override RewardBankResult OnExecute()
    {
        return this.GetSystem<CultivationRewardSystem>().BankPendingLoot(saveData, pendingItemRewards);
    }
}

public sealed class MergePendingLootCommand : AbstractCommand
{
    private readonly System.Collections.Generic.List<SaveItemStack> target;
    private readonly System.Collections.Generic.List<SaveItemStack> incoming;

    public MergePendingLootCommand(System.Collections.Generic.List<SaveItemStack> target, System.Collections.Generic.List<SaveItemStack> incoming)
    {
        this.target = target;
        this.incoming = incoming;
    }

    protected override void OnExecute()
    {
        this.GetSystem<CultivationRewardSystem>().MergeLoot(target, incoming);
    }
}

public sealed class ResolveDirectAttackTurnCommand : AbstractCommand<CombatTurnResult>
{
    private readonly CombatTurnContext context;
    private readonly ExpeditionEnemyState target;
    private readonly int damage;
    private readonly string missSummary;

    public ResolveDirectAttackTurnCommand(CombatTurnContext context, ExpeditionEnemyState target, int damage, string missSummary)
    {
        this.context = context;
        this.target = target;
        this.damage = damage;
        this.missSummary = missSummary;
    }

    protected override CombatTurnResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().ResolveDirectAttackTurn(context, target, damage, missSummary);
    }
}

public sealed class ResolveSkillTurnCommand : AbstractCommand<CombatTurnResult>
{
    private readonly CombatTurnContext context;
    private readonly int skillIndex;

    public ResolveSkillTurnCommand(CombatTurnContext context, int skillIndex)
    {
        this.context = context;
        this.skillIndex = skillIndex;
    }

    protected override CombatTurnResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().ResolveSkillTurn(context, skillIndex);
    }
}

public sealed class ResolveTalismanTurnCommand : AbstractCommand<CombatTurnResult>
{
    private readonly CombatTurnContext context;

    public ResolveTalismanTurnCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override CombatTurnResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().ResolveTalismanTurn(context);
    }
}

public sealed class ResolveMedicineTurnCommand : AbstractCommand<CombatTurnResult>
{
    private readonly CombatTurnContext context;

    public ResolveMedicineTurnCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override CombatTurnResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().ResolveMedicineTurn(context);
    }
}

public sealed class ResolveRoomEventCommand : AbstractCommand<ExpeditionRoomActionResult>
{
    private readonly CombatTurnContext context;

    public ResolveRoomEventCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override ExpeditionRoomActionResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().ResolveRoomEvent(context);
    }
}

public sealed class OpenRoomEventCommand : AbstractCommand<ExpeditionEventCardResult>
{
    private readonly CombatTurnContext context;

    public OpenRoomEventCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override ExpeditionEventCardResult OnExecute()
    {
        return this.GetSystem<CultivationExpeditionEventSystem>().OpenRoomEvent(context);
    }
}

public sealed class ResolveEventOptionCommand : AbstractCommand<ExpeditionEventOptionResult>
{
    private readonly CombatTurnContext context;
    private readonly string eventId;
    private readonly string optionId;

    public ResolveEventOptionCommand(CombatTurnContext context, string eventId, string optionId)
    {
        this.context = context;
        this.eventId = eventId;
        this.optionId = optionId;
    }

    protected override ExpeditionEventOptionResult OnExecute()
    {
        return this.GetSystem<CultivationExpeditionEventSystem>().ResolveEventOption(context, eventId, optionId);
    }
}

public sealed class EnterExpeditionRoomCommand : AbstractCommand<ExpeditionTraversalResult>
{
    private readonly ExpeditionTraversalContext context;

    public EnterExpeditionRoomCommand(ExpeditionTraversalContext context)
    {
        this.context = context;
    }

    protected override ExpeditionTraversalResult OnExecute()
    {
        return this.GetSystem<CultivationExpeditionSystem>().EnterRoom(context);
    }
}

public sealed class AdvanceExpeditionCommand : AbstractCommand<ExpeditionAdvanceResult>
{
    private readonly ExpeditionAdvanceContext context;

    public AdvanceExpeditionCommand(ExpeditionAdvanceContext context)
    {
        this.context = context;
    }

    protected override ExpeditionAdvanceResult OnExecute()
    {
        return this.GetSystem<CultivationExpeditionSystem>().Advance(context);
    }
}

public sealed class CollectRoomLootCommand : AbstractCommand<ExpeditionLootCollectionResult>
{
    private readonly WorldRegionDefinition region;
    private readonly ExpeditionRoomState room;
    private readonly System.Collections.Generic.List<SaveItemStack> pendingItemRewards;

    public CollectRoomLootCommand(WorldRegionDefinition region, ExpeditionRoomState room, System.Collections.Generic.List<SaveItemStack> pendingItemRewards)
    {
        this.region = region;
        this.room = room;
        this.pendingItemRewards = pendingItemRewards;
    }

    protected override ExpeditionLootCollectionResult OnExecute()
    {
        var loot = this.GetSystem<CultivationRewardSystem>().BuildRoomLoot(region, room, null);
        this.GetSystem<CultivationRewardSystem>().MergeLoot(pendingItemRewards, loot);
        return new ExpeditionLootCollectionResult
        {
            LootSummary = loot != null && loot.Count > 0 ? InventoryLibrary.DescribeLoot(loot) : string.Empty
        };
    }
}

public sealed class UseTorchSupplyCommand : AbstractCommand<ExpeditionSupportActionResult>
{
    private readonly CombatTurnContext context;

    public UseTorchSupplyCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override ExpeditionSupportActionResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().UseTorchSupply(context);
    }
}

public sealed class CampAndRecoverCommand : AbstractCommand<ExpeditionSupportActionResult>
{
    private readonly CombatTurnContext context;

    public CampAndRecoverCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override ExpeditionSupportActionResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().CampAndRecover(context);
    }
}

public sealed class RecenterMindCommand : AbstractCommand<ExpeditionSupportActionResult>
{
    private readonly CombatTurnContext context;

    public RecenterMindCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override ExpeditionSupportActionResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().RecenterMind(context);
    }
}

public sealed class SkipRoomCommand : AbstractCommand<ExpeditionSupportActionResult>
{
    private readonly CombatTurnContext context;

    public SkipRoomCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override ExpeditionSupportActionResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().SkipRoom(context);
    }
}

public sealed class SyncExpeditionRuntimeCommand : AbstractCommand
{
    private readonly CombatTurnContext context;

    public SyncExpeditionRuntimeCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override void OnExecute()
    {
        this.GetSystem<CultivationExpeditionSystem>().Sync(context);
    }
}

public sealed class ClearExpeditionRuntimeCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        this.GetSystem<CultivationExpeditionSystem>().Clear();
    }
}
