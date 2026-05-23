using System;
using System.Collections.Generic;
using QFramework;

/// <summary>
/// M14 DivinePowerModel：已领悟神通列表。
/// </summary>
public sealed class CultivationDivinePowerModel : AbstractModel
{
    /// <summary>
    /// 单个神通数据
    /// </summary>
    [Serializable]
    public sealed class DivinePowerData
    {
        public string id;
        public string displayName;
        public DivinePowerType type;
        public DateTime learnedTime;
        public int masteryLevel; // 熟练度等级

        public DivinePowerData(string id, string displayName, DivinePowerType type)
        {
            this.id = id;
            this.displayName = displayName;
            this.type = type;
            this.learnedTime = DateTime.Now;
            this.masteryLevel = 1;
        }
    }

    private readonly Dictionary<string, DivinePowerData> learnedPowers = new Dictionary<string, DivinePowerData>();

    /// <summary>
    /// 已领悟神通数量
    /// </summary>
    public int Count => learnedPowers.Count;

    /// <summary>
    /// 是否已领悟指定神通
    /// </summary>
    public bool HasLearned(string powerId)
    {
        return !string.IsNullOrWhiteSpace(powerId) && learnedPowers.ContainsKey(powerId);
    }

    /// <summary>
    /// 获取神通数据
    /// </summary>
    public DivinePowerData GetPower(string powerId)
    {
        if (string.IsNullOrWhiteSpace(powerId) || !learnedPowers.TryGetValue(powerId, out var power))
        {
            return null;
        }
        return power;
    }

    /// <summary>
    /// 领悟神通
    /// </summary>
    public bool LearnPower(string powerId, string displayName, DivinePowerType type)
    {
        if (string.IsNullOrWhiteSpace(powerId) || learnedPowers.ContainsKey(powerId))
        {
            return false;
        }

        learnedPowers[powerId] = new DivinePowerData(powerId, displayName, type);
        return true;
    }

    /// <summary>
    /// 提升神通熟练度
    /// </summary>
    public bool UpgradePower(string powerId)
    {
        if (string.IsNullOrWhiteSpace(powerId) || !learnedPowers.TryGetValue(powerId, out var power))
        {
            return false;
        }

        power.masteryLevel++;
        return true;
    }

    /// <summary>
    /// 获取所有已领悟神通
    /// </summary>
    public List<DivinePowerData> GetAllPowers()
    {
        return new List<DivinePowerData>(learnedPowers.Values);
    }

    /// <summary>
    /// 按类型获取神通列表
    /// </summary>
    public List<DivinePowerData> GetPowersByType(DivinePowerType type)
    {
        var result = new List<DivinePowerData>();
        foreach (var power in learnedPowers.Values)
        {
            if (power.type == type)
            {
                result.Add(power);
            }
        }
        return result;
    }

    /// <summary>
    /// 重置所有神通（新游戏时调用）
    /// </summary>
    public void Reset()
    {
        learnedPowers.Clear();
    }

    protected override void OnInit()
    {
    }
}
