using System.Collections.Generic;
using QFramework;

public sealed class CompleteExpeditionRunCommand : AbstractCommand<ExpeditionResolutionResult>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;
    private readonly WorldRegionDefinition region;
    private readonly ExpeditionHeroState hero;
    private readonly int torchlight;
    private readonly int pendingQiGain;
    private readonly int pendingCrystalGain;
    private readonly List<SaveItemStack> pendingItemRewards;

    public CompleteExpeditionRunCommand(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region, ExpeditionHeroState hero, int torchlight, int pendingQiGain, int pendingCrystalGain, List<SaveItemStack> pendingItemRewards)
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
    private readonly List<SaveItemStack> pendingItemRewards;

    public RetreatExpeditionRunCommand(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region, int pendingQiGain, int pendingCrystalGain, List<SaveItemStack> pendingItemRewards)
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
    private readonly List<SaveItemStack> pendingItemRewards;

    public FailExpeditionRunCommand(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region, string reason, List<SaveItemStack> pendingItemRewards)
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

public sealed class BuildExpeditionRoomsCommand : AbstractCommand<List<ExpeditionRoomState>>
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

    protected override List<ExpeditionRoomState> OnExecute()
    {
        return this.GetSystem<CultivationEncounterDirectorSystem>().BuildRooms(region, saveData, random);
    }
}

public sealed class BuildEncounterEnemiesCommand : AbstractCommand<List<ExpeditionEnemyState>>
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

    protected override List<ExpeditionEnemyState> OnExecute()
    {
        return this.GetSystem<CultivationEncounterDirectorSystem>().BuildEnemies(region, room, saveData, random);
    }
}

public sealed class BuildEncounterLootCommand : AbstractCommand<List<SaveItemStack>>
{
    private readonly CombatTurnContext context;

    public BuildEncounterLootCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override List<SaveItemStack> OnExecute()
    {
        return this.GetSystem<CultivationRewardSystem>().BuildEncounterLoot(context);
    }
}

public sealed class BuildClearLootCommand : AbstractCommand<List<SaveItemStack>>
{
    private readonly WorldRegionDefinition region;
    private readonly MainMenuSaveData saveData;

    public BuildClearLootCommand(WorldRegionDefinition region, MainMenuSaveData saveData)
    {
        this.region = region;
        this.saveData = saveData;
    }

    protected override List<SaveItemStack> OnExecute()
    {
        return this.GetSystem<CultivationRewardSystem>().BuildClearLoot(region, saveData);
    }
}

public sealed class BankPendingLootCommand : AbstractCommand<RewardBankResult>
{
    private readonly MainMenuSaveData saveData;
    private readonly List<SaveItemStack> pendingItemRewards;

    public BankPendingLootCommand(MainMenuSaveData saveData, List<SaveItemStack> pendingItemRewards)
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
    private readonly List<SaveItemStack> target;
    private readonly List<SaveItemStack> incoming;

    public MergePendingLootCommand(List<SaveItemStack> target, List<SaveItemStack> incoming)
    {
        this.target = target;
        this.incoming = incoming;
    }

    protected override void OnExecute()
    {
        this.GetSystem<CultivationRewardSystem>().MergeLoot(target, incoming);
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
    private readonly List<SaveItemStack> pendingItemRewards;

    public CollectRoomLootCommand(WorldRegionDefinition region, ExpeditionRoomState room, List<SaveItemStack> pendingItemRewards)
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
