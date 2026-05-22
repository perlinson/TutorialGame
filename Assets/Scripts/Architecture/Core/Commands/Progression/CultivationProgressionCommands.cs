using QFramework;

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
