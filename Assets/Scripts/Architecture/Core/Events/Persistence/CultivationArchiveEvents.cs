public sealed class CultivationArchiveChangedEvent
{
    public CultivationArchiveChangedEvent(int slotIndex, CultivationSaveData saveData)
    {
        SlotIndex = slotIndex;
        SaveData = saveData;
    }

    public int SlotIndex { get; }
    public CultivationSaveData SaveData { get; }
}
