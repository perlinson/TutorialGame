using QFramework;

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
