using QFramework;

public sealed class CultivationArchiveModel : AbstractModel
{
    public readonly BindableProperty<int> CurrentSlotIndex = new BindableProperty<int>(-1);
    public readonly BindableProperty<string> HeroName = new BindableProperty<string>(string.Empty);
    public readonly BindableProperty<string> ArchetypeName = new BindableProperty<string>(string.Empty);

    public CultivationSaveData CurrentSaveData { get; private set; }

    protected override void OnInit()
    {
    }

    public void Apply(int slotIndex, CultivationSaveData saveData)
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
