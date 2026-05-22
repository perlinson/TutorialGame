using QFramework;

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
