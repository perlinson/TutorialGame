using QFramework;

/// <summary>
/// M1 RealmModel：境界 / 修为 / 瓶颈 / 突破 / 心魔 状态。
/// 持久化字段位于 <see cref="MainMenuSaveData"/>，本 Model 只在运行时承载 BindableProperty 表现层订阅。
/// 所有"突破 / 上修为 / 进入或解除瓶颈"业务请通过 <see cref="CultivationRealmSystem"/> 操作。
/// </summary>
public sealed class CultivationRealmModel : AbstractModel
{
    public readonly BindableProperty<int> Tier = new BindableProperty<int>(0);
    public readonly BindableProperty<int> Qi = new BindableProperty<int>(0);
    public readonly BindableProperty<int> QiRequiredForNext = new BindableProperty<int>(0);
    public readonly BindableProperty<string> RealmName = new BindableProperty<string>(string.Empty);
    public readonly BindableProperty<bool> AtBottleneck = new BindableProperty<bool>(false);
    public readonly BindableProperty<int> BreakthroughCount = new BindableProperty<int>(0);
    public readonly BindableProperty<int> HeartDemonMark = new BindableProperty<int>(0);

    public MainMenuSaveData CurrentSaveData { get; private set; }

    protected override void OnInit()
    {
    }

    public void Apply(MainMenuSaveData saveData)
    {
        CurrentSaveData = saveData;
        if (saveData == null)
        {
            Tier.Value = 0;
            Qi.Value = 0;
            QiRequiredForNext.Value = 0;
            RealmName.Value = string.Empty;
            AtBottleneck.Value = false;
            BreakthroughCount.Value = 0;
            HeartDemonMark.Value = 0;
            return;
        }

        saveData.EnsureDefaults();
        Tier.Value = saveData.realmTier;
        Qi.Value = saveData.qi;
        QiRequiredForNext.Value = WorldRegionLibrary.GetQiRequiredForNextRealm(saveData.realmTier);
        RealmName.Value = string.IsNullOrWhiteSpace(saveData.realm)
            ? WorldRegionLibrary.GetRealmName(saveData.realmTier)
            : saveData.realm;
        AtBottleneck.Value = saveData.atBottleneck;
        BreakthroughCount.Value = saveData.breakthroughCount;
        HeartDemonMark.Value = saveData.heartDemonMark;
    }

    public void Clear()
    {
        Apply(null);
    }
}
