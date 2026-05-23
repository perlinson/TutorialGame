using QFramework;

/// <summary>
/// M2 AttributeModel：修仙类持久属性（10个基础属性）。
/// 区别于 <see cref="CultivationCombatStatsModel"/>（战斗运行时属性）。
/// 写入字段时请通过 <see cref="MainMenuSaveData"/>，再调用 <see cref="Apply"/> 同步到 Bindable。
/// </summary>
public sealed class CultivationAttributeModel : AbstractModel
{
    // 原有属性
    public readonly BindableProperty<int> RootBone = new BindableProperty<int>(0);
    public readonly BindableProperty<int> Insight = new BindableProperty<int>(0);
    public readonly BindableProperty<int> SpiritSense = new BindableProperty<int>(0);
    public readonly BindableProperty<int> VitalityStat = new BindableProperty<int>(0);
    public readonly BindableProperty<int> ManaStat = new BindableProperty<int>(0);
    public readonly BindableProperty<int> Charm = new BindableProperty<int>(0);

    // 新增属性
    public readonly BindableProperty<int> SoulPower = new BindableProperty<int>(0);
    public readonly BindableProperty<int> VitalEnergy = new BindableProperty<int>(0);
    public readonly BindableProperty<int> Willpower = new BindableProperty<int>(0);
    public readonly BindableProperty<int> Dexterity = new BindableProperty<int>(0);
    public readonly BindableProperty<int> SpiritRoot = new BindableProperty<int>(0);

    public MainMenuSaveData CurrentSaveData { get; private set; }

    protected override void OnInit()
    {
    }

    public void Apply(MainMenuSaveData saveData)
    {
        CurrentSaveData = saveData;
        if (saveData == null)
        {
            RootBone.Value = 0;
            Insight.Value = 0;
            SpiritSense.Value = 0;
            VitalityStat.Value = 0;
            ManaStat.Value = 0;
            Charm.Value = 0;
            SoulPower.Value = 0;
            VitalEnergy.Value = 0;
            Willpower.Value = 0;
            Dexterity.Value = 0;
            SpiritRoot.Value = 0;
            return;
        }

        saveData.EnsureDefaults();
        RootBone.Value = saveData.rootBone;
        Insight.Value = saveData.insight;
        SpiritSense.Value = saveData.spiritSense;
        VitalityStat.Value = saveData.vitalityStat;
        ManaStat.Value = saveData.manaStat;
        Charm.Value = saveData.charm;
        SoulPower.Value = saveData.soulPower;
        VitalEnergy.Value = saveData.vitalEnergy;
        Willpower.Value = saveData.willpower;
        Dexterity.Value = saveData.dexterity;
        SpiritRoot.Value = saveData.spiritRoot;
    }

    public void Clear()
    {
        Apply(null);
    }
}
