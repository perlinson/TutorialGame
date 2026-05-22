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
