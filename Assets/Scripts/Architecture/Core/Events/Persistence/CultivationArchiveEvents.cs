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
