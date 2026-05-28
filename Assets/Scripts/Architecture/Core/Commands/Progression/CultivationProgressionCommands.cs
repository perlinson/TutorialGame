using QFramework;

public sealed class BuildStorySummaryCommand : AbstractCommand<string>
{
    private readonly CultivationSaveData saveData;

    public BuildStorySummaryCommand(CultivationSaveData saveData)
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
