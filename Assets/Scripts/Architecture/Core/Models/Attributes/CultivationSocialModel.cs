using System.Collections.Generic;
using QFramework;

/// <summary>
/// M16 SocialModel：缘分属性（声望、功德、杀气、道侣数量、宗门贡献）。
/// </summary>
public sealed class CultivationSocialModel : AbstractModel
{
    // 缘分属性
    public readonly BindableProperty<int> Reputation = new BindableProperty<int>(0);           // 声望
    public readonly BindableProperty<int> Merit = new BindableProperty<int>(0);                // 功德
    public readonly BindableProperty<int> KillingIntent = new BindableProperty<int>(0);         // 杀气
    public readonly BindableProperty<int> DaoCompanionCount = new BindableProperty<int>(0);    // 道侣数量
    public readonly BindableProperty<int> SectContribution = new BindableProperty<int>(0);      // 宗门贡献

    // 道侣列表
    private readonly List<string> daoCompanions = new List<string>();

    /// <summary>
    /// 获取道侣列表
    /// </summary>
    public List<string> GetDaoCompanions()
    {
        return new List<string>(daoCompanions);
    }

    /// <summary>
    /// 添加道侣
    /// </summary>
    public bool AddDaoCompanion(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId) || daoCompanions.Contains(npcId))
        {
            return false;
        }

        daoCompanions.Add(npcId);
        DaoCompanionCount.Value = daoCompanions.Count;
        return true;
    }

    /// <summary>
    /// 移除道侣
    /// </summary>
    public bool RemoveDaoCompanion(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId) || !daoCompanions.Contains(npcId))
        {
            return false;
        }

        daoCompanions.Remove(npcId);
        DaoCompanionCount.Value = daoCompanions.Count;
        return true;
    }

    /// <summary>
    /// 增加声望
    /// </summary>
    public void AddReputation(int amount)
    {
        Reputation.Value += amount;
    }

    /// <summary>
    /// 增加功德
    /// </summary>
    public void AddMerit(int amount)
    {
        Merit.Value += amount;
    }

    /// <summary>
    /// 增加杀气
    /// </summary>
    public void AddKillingIntent(int amount)
    {
        KillingIntent.Value += amount;
    }

    /// <summary>
    /// 增加宗门贡献
    /// </summary>
    public void AddSectContribution(int amount)
    {
        SectContribution.Value += amount;
    }

    /// <summary>
    /// 消耗宗门贡献
    /// </summary>
    public bool ConsumeSectContribution(int amount)
    {
        if (SectContribution.Value < amount)
        {
            return false;
        }

        SectContribution.Value -= amount;
        return true;
    }

    /// <summary>
    /// 获取天劫难度修正（功德影响）
    /// </summary>
    public float GetTribulationDifficultyModifier()
    {
        // 功德越高，天劫难度越低
        // 杀气越高，天劫难度越高
        var modifier = 1.0f;
        modifier -= Merit.Value * 0.001f; // 每100功德降低10%难度
        modifier += KillingIntent.Value * 0.002f; // 每50杀气增加10%难度
        return UnityEngine.Mathf.Clamp(modifier, 0.5f, 2.0f);
    }

    /// <summary>
    /// 获取正邪判定
    /// </summary>
    public string GetAlignment()
    {
        var alignment = Merit.Value - KillingIntent.Value;

        if (alignment > 100)
        {
            return "正道";
        }
        else if (alignment < -100)
        {
            return "邪道";
        }
        else
        {
            return "中立";
        }
    }

    /// <summary>
    /// 获取奇遇触发概率修正（声望和福缘影响）
    /// </summary>
    public float GetEncounterChanceModifier()
    {
        // 声望越高，高级奇遇概率越高
        return 1.0f + Reputation.Value * 0.0005f;
    }

    /// <summary>
    /// 重置所有社交属性（新游戏时调用）
    /// </summary>
    public void Reset()
    {
        Reputation.Value = 0;
        Merit.Value = 0;
        KillingIntent.Value = 0;
        DaoCompanionCount.Value = 0;
        SectContribution.Value = 0;
        daoCompanions.Clear();
    }

    protected override void OnInit()
    {
    }
}
