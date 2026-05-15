using QFramework;

public sealed class CultivationArchiveSnapshot
{
    public CultivationArchiveSnapshot(int slotIndex, MainMenuSaveData saveData)
    {
        SlotIndex = slotIndex;
        SaveData = saveData;
    }

    public int SlotIndex { get; }
    public MainMenuSaveData SaveData { get; }
}

public sealed class CultivationArchiveChangedEvent
{
    public CultivationArchiveChangedEvent(int slotIndex, MainMenuSaveData saveData)
    {
        SlotIndex = slotIndex;
        SaveData = saveData;
    }

    public int SlotIndex { get; }
    public MainMenuSaveData SaveData { get; }
}

public sealed class CultivationArchiveModel : AbstractModel
{
    public readonly BindableProperty<int> CurrentSlotIndex = new BindableProperty<int>(-1);
    public readonly BindableProperty<string> HeroName = new BindableProperty<string>(string.Empty);
    public readonly BindableProperty<string> ArchetypeName = new BindableProperty<string>(string.Empty);

    public MainMenuSaveData CurrentSaveData { get; private set; }

    protected override void OnInit()
    {
    }

    public void Apply(int slotIndex, MainMenuSaveData saveData)
    {
        CurrentSlotIndex.Value = slotIndex;
        CurrentSaveData = saveData;
        HeroName.Value = saveData != null ? saveData.heroName : string.Empty;
        ArchetypeName.Value = saveData != null ? saveData.archetypeName : string.Empty;
    }

    public void Clear()
    {
        Apply(-1, null);
    }
}

public sealed class CultivationInventoryModel : AbstractModel
{
    public readonly BindableProperty<int> BagUsedSlots = new BindableProperty<int>(0);
    public readonly BindableProperty<int> BagCapacity = new BindableProperty<int>(0);
    public readonly BindableProperty<int> SpiritCrystals = new BindableProperty<int>(0);

    protected override void OnInit()
    {
    }

    public void Apply(MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            BagUsedSlots.Value = 0;
            BagCapacity.Value = 0;
            SpiritCrystals.Value = 0;
            return;
        }

        BagUsedSlots.Value = saveData.GetUsedBagSlots();
        BagCapacity.Value = saveData.bagCapacity;
        SpiritCrystals.Value = saveData.spiritCrystals;
    }
}

public sealed class CultivationTaskBoardModel : AbstractModel
{
    public readonly BindableProperty<string> ActiveTaskId = new BindableProperty<string>(string.Empty);
    public readonly BindableProperty<string> ActiveTaskSummary = new BindableProperty<string>(string.Empty);
    public readonly BindableProperty<string> LastBoardMessage = new BindableProperty<string>(string.Empty);

    protected override void OnInit()
    {
    }

    public void Apply(MainMenuSaveData saveData, string boardMessage = null)
    {
        if (saveData == null)
        {
            ActiveTaskId.Value = string.Empty;
            ActiveTaskSummary.Value = "委托：暂无新任务。";
            LastBoardMessage.Value = boardMessage ?? string.Empty;
            return;
        }

        ActiveTaskId.Value = saveData.activeTaskId ?? string.Empty;
        ActiveTaskSummary.Value = TaskLibrary.BuildActiveTaskSummary(saveData);
        if (boardMessage != null)
        {
            LastBoardMessage.Value = boardMessage;
        }
    }
}

public sealed class CultivationWorldMapModel : AbstractModel
{
    public readonly BindableProperty<int> RealmTier = new BindableProperty<int>(0);
    public readonly BindableProperty<string> RealmName = new BindableProperty<string>(string.Empty);
    public readonly BindableProperty<string> CurrentRegionId = new BindableProperty<string>(string.Empty);
    public readonly BindableProperty<string> CurrentRegionName = new BindableProperty<string>(string.Empty);

    protected override void OnInit()
    {
    }

    public void Apply(MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            RealmTier.Value = 0;
            RealmName.Value = string.Empty;
            CurrentRegionId.Value = string.Empty;
            CurrentRegionName.Value = string.Empty;
            return;
        }

        RealmTier.Value = saveData.realmTier;
        RealmName.Value = saveData.realm;
        CurrentRegionId.Value = saveData.currentRegionId;
        CurrentRegionName.Value = WorldRegionLibrary.GetRegionDisplayName(saveData.currentRegionId);
    }
}

public sealed class CultivationExpeditionModel : AbstractModel
{
    public readonly BindableProperty<int> CurrentRoomIndex = new BindableProperty<int>(0);
    public readonly BindableProperty<int> Torchlight = new BindableProperty<int>(0);
    public readonly BindableProperty<int> Supplies = new BindableProperty<int>(0);
    public readonly BindableProperty<int> PendingQiGain = new BindableProperty<int>(0);
    public readonly BindableProperty<int> PendingCrystalGain = new BindableProperty<int>(0);
    public readonly BindableProperty<int> CombatRound = new BindableProperty<int>(0);

    protected override void OnInit()
    {
    }

    public void Apply(CombatTurnContext context)
    {
        if (context == null)
        {
            Clear();
            return;
        }

        CurrentRoomIndex.Value = context.CurrentRoomIndex;
        Torchlight.Value = context.Torchlight;
        Supplies.Value = context.Supplies;
        PendingQiGain.Value = context.PendingQiGain;
        PendingCrystalGain.Value = context.PendingCrystalGain;
        CombatRound.Value = context.CombatRound;
    }

    public void Clear()
    {
        CurrentRoomIndex.Value = 0;
        Torchlight.Value = 0;
        Supplies.Value = 0;
        PendingQiGain.Value = 0;
        PendingCrystalGain.Value = 0;
        CombatRound.Value = 0;
    }
}
