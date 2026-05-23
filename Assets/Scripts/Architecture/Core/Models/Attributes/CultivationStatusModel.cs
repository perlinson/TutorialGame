using QFramework;

/// <summary>
/// M15 StatusModel：状态属性（饱食度、精力、心情、伤势、煞气）。
/// 影响修炼效率和战斗属性。
/// </summary>
public sealed class CultivationStatusModel : AbstractModel
{
    // 状态属性（范围 0-100）
    public readonly BindableProperty<int> Hunger = new BindableProperty<int>(100);      // 饱食度
    public readonly BindableProperty<int> Energy = new BindableProperty<int>(100);      // 精力
    public readonly BindableProperty<int> Mood = new BindableProperty<int>(100);        // 心情
    public readonly BindableProperty<int> Injury = new BindableProperty<int>(0);        // 伤势
    public readonly BindableProperty<int> KillingIntent = new BindableProperty<int>(0);  // 煞气

    /// <summary>
    /// 获取修炼效率修正（0.3-1.5）
    /// </summary>
    public float GetCultivationEfficiency()
    {
        var efficiency = 1.0f;

        if (Hunger.Value < 30) efficiency *= 0.7f;
        if (Energy.Value < 30) efficiency *= 0.6f;
        if (Mood.Value < 30) efficiency *= 0.8f;
        if (Injury.Value > 50) efficiency *= 0.5f;

        return UnityEngine.Mathf.Clamp(efficiency, 0.3f, 1.5f);
    }

    /// <summary>
    /// 获取突破成功率修正（0.5-1.0）
    /// </summary>
    public float GetBreakthroughSuccessRate()
    {
        var rate = 1.0f;

        if (Mood.Value < 30) rate *= 0.7f;
        if (Injury.Value > 30) rate *= 0.8f;
        if (KillingIntent.Value > 50) rate *= 0.6f;

        return UnityEngine.Mathf.Clamp(rate, 0.5f, 1.0f);
    }

    /// <summary>
    /// 获取战斗属性修正
    /// </summary>
    public CombatStatusModifier GetCombatModifier()
    {
        return new CombatStatusModifier
        {
            AttackPercent = -Injury.Value / 2,           // 伤势降低攻击
            DefensePercent = -Injury.Value / 3,          // 伤势降低防御
            SpeedPercent = -Injury.Value / 4,           // 伤势降低速度
            HitChancePercent = -Injury.Value / 5,       // 伤势降低命中
            CritRatePercent = -Injury.Value / 10        // 伤势降低暴击
        };
    }

    /// <summary>
    /// 增加饱食度
    /// </summary>
    public void AddHunger(int amount)
    {
        Hunger.Value = UnityEngine.Mathf.Clamp(Hunger.Value + amount, 0, 100);
    }

    /// <summary>
    /// 增加精力
    /// </summary>
    public void AddEnergy(int amount)
    {
        Energy.Value = UnityEngine.Mathf.Clamp(Energy.Value + amount, 0, 100);
    }

    /// <summary>
    /// 增加心情
    /// </summary>
    public void AddMood(int amount)
    {
        Mood.Value = UnityEngine.Mathf.Clamp(Mood.Value + amount, 0, 100);
    }

    /// <summary>
    /// 增加伤势
    /// </summary>
    public void AddInjury(int amount)
    {
        Injury.Value = UnityEngine.Mathf.Clamp(Injury.Value + amount, 0, 100);
    }

    /// <summary>
    /// 增加煞气
    /// </summary>
    public void AddKillingIntent(int amount)
    {
        KillingIntent.Value = UnityEngine.Mathf.Clamp(KillingIntent.Value + amount, 0, 100);
    }

    /// <summary>
    /// 重置所有状态（新游戏时调用）
    /// </summary>
    public void Reset()
    {
        Hunger.Value = 100;
        Energy.Value = 100;
        Mood.Value = 100;
        Injury.Value = 0;
        KillingIntent.Value = 0;
    }

    protected override void OnInit()
    {
    }
}

/// <summary>
/// 战斗状态修正
/// </summary>
public struct CombatStatusModifier
{
    public int AttackPercent;
    public int DefensePercent;
    public int SpeedPercent;
    public int HitChancePercent;
    public int CritRatePercent;
}
