public sealed class CultivationArchiveSnapshot
{
    public CultivationArchiveSnapshot(int slotIndex, CultivationSaveData saveData)
    {
        SlotIndex = slotIndex;
        SaveData = saveData;
    }

    public int SlotIndex { get; }
    public CultivationSaveData SaveData { get; }
}
