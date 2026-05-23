using QFramework;

/// <summary>
/// S8 StatusSystem：状态属性衰减、状态对修炼效率影响。
/// </summary>
public sealed class CultivationStatusSystem : AbstractSystem
{
    private CultivationStatusModel statusModel;

    protected override void OnInit()
    {
        statusModel = this.GetModel<CultivationStatusModel>();
    }

    /// <summary>
    /// 每日状态衰减（在每日结算时调用）
    /// </summary>
    public void TickDailyDecay()
    {
        // 饱食度每日衰减
        statusModel.AddHunger(-15);

        // 精力每日衰减
        statusModel.AddEnergy(-20);

        // 心情自然衰减（缓慢）
        statusModel.AddMood(-5);

        // 伤势自然恢复（如果有）
        if (statusModel.Injury.Value > 0)
        {
            statusModel.AddInjury(-2);
        }

        // 煞气自然衰减（缓慢）
        if (statusModel.KillingIntent.Value > 0)
        {
            statusModel.AddKillingIntent(-1);
        }
    }

    /// <summary>
    /// 修炼时状态消耗（在修炼行为时调用）
    /// </summary>
    /// <param name="hours">修炼时长（小时）</param>
    public void ConsumeForCultivation(int hours)
    {
        // 修炼消耗饱食度
        statusModel.AddHunger(-hours * 2);

        // 修炼消耗精力
        statusModel.AddEnergy(-hours * 3);

        // 修炼可能降低心情（如果状态不佳）
        if (statusModel.Hunger.Value < 30 || statusModel.Energy.Value < 30)
        {
            statusModel.AddMood(-hours);
        }
    }

    /// <summary>
    /// 战斗时状态消耗（在战斗结束时调用）
    /// </summary>
    /// <param name="intensity">战斗强度（1-10）</param>
    public void ConsumeForCombat(int intensity)
    {
        // 战斗消耗精力
        statusModel.AddEnergy(-intensity * 5);

        // 战斗可能增加伤势
        statusModel.AddInjury(intensity * 3);

        // 战斗可能增加煞气
        statusModel.AddKillingIntent(intensity * 2);
    }

    /// <summary>
    /// 进食恢复
    /// </summary>
    public void Eat(int foodQuality)
    {
        // 食物品质影响恢复量
        var hungerRestore = foodQuality * 20;
        var energyRestore = foodQuality * 10;
        var moodRestore = foodQuality * 5;

        statusModel.AddHunger(hungerRestore);
        statusModel.AddEnergy(energyRestore);
        statusModel.AddMood(moodRestore);
    }

    /// <summary>
    /// 休息恢复
    /// </summary>
    /// <param name="hours">休息时长（小时）</param>
    public void Rest(int hours)
    {
        // 休息恢复精力
        statusModel.AddEnergy(hours * 8);

        // 休息恢复心情
        statusModel.AddMood(hours * 3);

        // 休息恢复伤势
        if (statusModel.Injury.Value > 0)
        {
            statusModel.AddInjury(-hours);
        }

        // 休息消耗饱食度
        statusModel.AddHunger(-hours);
    }

    /// <summary>
    /// 娱乐活动（提升心情）
    /// </summary>
    /// <param name="activityType">活动类型</param>
    public void Entertainment(string activityType)
    {
        var moodBonus = activityType switch
        {
            "饮酒" => 20,
            "游历" => 15,
            "双修" => 25,
            "品茶" => 10,
            _ => 10
        };

        statusModel.AddMood(moodBonus);
    }

    /// <summary>
    /// 疗伤
    /// </summary>
    /// <param name="medicineQuality">丹药品质</param>
    public void HealInjury(int medicineQuality)
    {
        var injuryRestore = medicineQuality * 15;
        statusModel.AddInjury(-injuryRestore);
    }

    /// <summary>
    /// 获取状态警告信息
    /// </summary>
    public string GetStatusWarnings()
    {
        var warnings = new System.Text.StringBuilder();

        if (statusModel.Hunger.Value < 30)
        {
            warnings.AppendLine("饱食度过低，修炼效率降低！");
        }

        if (statusModel.Energy.Value < 30)
        {
            warnings.AppendLine("精力不足，修炼效率大幅降低！");
        }

        if (statusModel.Mood.Value < 30)
        {
            warnings.AppendLine("心情不佳，影响修炼和突破！");
        }

        if (statusModel.Injury.Value > 50)
        {
            warnings.AppendLine("伤势严重，急需疗伤！");
        }

        if (statusModel.KillingIntent.Value > 50)
        {
            warnings.AppendLine("煞气过重，易生心魔！");
        }

        return warnings.ToString();
    }

    /// <summary>
    /// 检查是否可以进行修炼
    /// </summary>
    public bool CanCultivate()
    {
        return statusModel.Hunger.Value > 10 && statusModel.Energy.Value > 10;
    }

    /// <summary>
    /// 检查是否可以进行战斗
    /// </summary>
    public bool CanFight()
    {
        return statusModel.Energy.Value > 20 && statusModel.Injury.Value < 80;
    }
}
