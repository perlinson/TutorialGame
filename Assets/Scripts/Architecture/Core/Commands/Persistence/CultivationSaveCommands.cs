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
