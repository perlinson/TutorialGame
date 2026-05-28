using QFramework;

public sealed class CultivationInventoryModel : AbstractModel
{
    public readonly BindableProperty<int> BagUsedSlots = new BindableProperty<int>(0);
    public readonly BindableProperty<int> BagCapacity = new BindableProperty<int>(0);
    public readonly BindableProperty<string> SpiritCrystals = new BindableProperty<string>(string.Empty);

    protected override void OnInit()
    {
    }

    public void Apply(CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            BagUsedSlots.Value = 0;
            BagCapacity.Value = 0;
            SpiritCrystals.Value = string.Empty;
            return;
        }

        BagUsedSlots.Value = saveData.GetUsedBagSlots();
        BagCapacity.Value = saveData.bagCapacity;
        SpiritCrystals.Value = saveData.wallet.ToDisplayString();
    }
}
