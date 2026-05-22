using QFramework;

public sealed class CultivationPlayerModel : AbstractModel
{
    public readonly BindableProperty<string> HeroName = new BindableProperty<string>(string.Empty);
    public readonly BindableProperty<string> ArchetypeName = new BindableProperty<string>(string.Empty);
    public readonly BindableProperty<string> RealmName = new BindableProperty<string>(string.Empty);
    public readonly BindableProperty<string> LocationName = new BindableProperty<string>(string.Empty);
    public readonly BindableProperty<int> Qi = new BindableProperty<int>(0);
    public readonly BindableProperty<int> SpiritCrystals = new BindableProperty<int>(0);
    public readonly BindableProperty<int> MainArtifactLevel = new BindableProperty<int>(0);
    public readonly BindableProperty<int> ProtectiveRelicLevel = new BindableProperty<int>(0);
    public readonly BindableProperty<int> BagUsedSlots = new BindableProperty<int>(0);
    public readonly BindableProperty<int> BagCapacity = new BindableProperty<int>(0);

    public MainMenuSaveData CurrentSaveData { get; private set; }

    protected override void OnInit()
    {
    }

    public void Apply(MainMenuSaveData saveData)
    {
        CurrentSaveData = saveData;
        HeroName.Value = saveData != null ? saveData.heroName : string.Empty;
        ArchetypeName.Value = saveData != null ? saveData.archetypeName : string.Empty;
        RealmName.Value = saveData != null ? saveData.realm : string.Empty;
        LocationName.Value = saveData != null ? saveData.location : string.Empty;
        Qi.Value = saveData != null ? saveData.qi : 0;
        SpiritCrystals.Value = saveData != null ? saveData.spiritCrystals : 0;
        MainArtifactLevel.Value = saveData != null ? saveData.mainArtifactLevel : 0;
        ProtectiveRelicLevel.Value = saveData != null ? saveData.protectiveRelicLevel : 0;
        BagUsedSlots.Value = saveData != null ? saveData.GetUsedBagSlots() : 0;
        BagCapacity.Value = saveData != null ? saveData.bagCapacity : 0;
    }

    public void Clear()
    {
        Apply(null);
    }
}
