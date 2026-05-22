using QFramework;

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
