using System;
using System.Collections.Generic;
using QFramework;

/// <summary>
/// M12 BranchModel：属性分支系统（40个分支，每个基础属性4个分支）。
/// 分支等级 = 大境界基数 + 功法侧重加成 + 玩家主动修炼投入 + 临时加成
/// </summary>
public sealed class CultivationBranchModel : AbstractModel
{
    /// <summary>
    /// 单个分支的数据结构
    /// </summary>
    [Serializable]
    public sealed class BranchData
    {
        public BranchType Type;
        public int TrainingInvestment; // 玩家主动修炼投入值
        public int RealmBase;          // 大境界基数（从境界实时计算）
        public int ArtifactBonus;      // 功法侧重加成
        public int TemporaryBonus;     // 临时加成（Buff、道具等）

        /// <summary>
        /// 分支最终等级
        /// </summary>
        public int FinalLevel => RealmBase + ArtifactBonus + TrainingInvestment + TemporaryBonus;

        public BranchData(BranchType type)
        {
            Type = type;
            TrainingInvestment = 0;
            RealmBase = 0;
            ArtifactBonus = 0;
            TemporaryBonus = 0;
        }
    }

    /// <summary>
    /// 所有分支数据字典
    /// </summary>
    private readonly Dictionary<BranchType, BranchData> branches = new Dictionary<BranchType, BranchData>();

    /// <summary>
    /// 获取分支数据
    /// </summary>
    public BranchData GetBranch(BranchType type)
    {
        if (!branches.ContainsKey(type))
        {
            branches[type] = new BranchData(type);
        }
        return branches[type];
    }

    /// <summary>
    /// 获取分支最终等级
    /// </summary>
    public int GetBranchLevel(BranchType type)
    {
        return GetBranch(type).FinalLevel;
    }

    /// <summary>
    /// 增加分支修炼投入
    /// </summary>
    public void AddTrainingInvestment(BranchType type, int amount)
    {
        var branch = GetBranch(type);
        branch.TrainingInvestment += amount;
    }

    /// <summary>
    /// 设置功法侧重加成
    /// </summary>
    public void SetArtifactBonus(BranchType type, int bonus)
    {
        var branch = GetBranch(type);
        branch.ArtifactBonus = bonus;
    }

    /// <summary>
    /// 设置临时加成
    /// </summary>
    public void SetTemporaryBonus(BranchType type, int bonus)
    {
        var branch = GetBranch(type);
        branch.TemporaryBonus = bonus;
    }

    /// <summary>
    /// 更新所有分支的大境界基数（境界突破时调用）
    /// </summary>
    public void UpdateRealmBase(int realmTier)
    {
        var baseValue = GetRealmBaseValue(realmTier);
        var divineSenseExtra = GetDivineSenseExtra(realmTier);
        var constitutionExtra = GetConstitutionExtra(realmTier);

        foreach (var kvp in branches)
        {
            var type = kvp.Key;
            var branch = kvp.Value;

            // 全分支基数
            branch.RealmBase = baseValue;

            // 神识分支额外加成
            if (IsDivineSenseBranch(type))
            {
                branch.RealmBase += divineSenseExtra;
            }

            // 根骨分支额外加成
            if (IsConstitutionBranch(type))
            {
                branch.RealmBase += constitutionExtra;
            }
        }
    }

    /// <summary>
    /// 清除所有临时加成（战斗结束或Buff过期时调用）
    /// </summary>
    public void ClearTemporaryBonuses()
    {
        foreach (var branch in branches.Values)
        {
            branch.TemporaryBonus = 0;
        }
    }

    /// <summary>
    /// 重置所有分支（新游戏时调用）
    /// </summary>
    public void ResetAll()
    {
        branches.Clear();
    }

    protected override void OnInit()
    {
        // 初始化所有分支
        foreach (BranchType type in Enum.GetValues(typeof(BranchType)))
        {
            branches[type] = new BranchData(type);
        }
    }

    private static int GetRealmBaseValue(int realmTier)
    {
        return realmTier switch
        {
            0 => 5,   // 炼气
            1 => 15,  // 筑基
            2 => 35,  // 金丹
            3 => 70,  // 元婴
            4 => 120, // 化神
            _ => 5
        };
    }

    private static int GetDivineSenseExtra(int realmTier)
    {
        return realmTier switch
        {
            0 => 0,
            1 => 5,
            2 => 10,
            3 => 15,
            4 => 20,
            _ => 0
        };
    }

    private static int GetConstitutionExtra(int realmTier)
    {
        return realmTier switch
        {
            0 => 0,
            1 => 3,
            2 => 5,
            3 => 8,
            4 => 10,
            _ => 0
        };
    }

    private static bool IsDivineSenseBranch(BranchType type)
    {
        return type == BranchType.DivineSense_Strength ||
               type == BranchType.DivineSense_Control ||
               type == BranchType.DivineSense_Attack ||
               type == BranchType.DivineSense_Defense;
    }

    private static bool IsConstitutionBranch(BranchType type)
    {
        return type == BranchType.Constitution_Physique ||
               type == BranchType.Constitution_Recovery ||
               type == BranchType.Constitution_Resistance ||
               type == BranchType.Constitution_Tempering;
    }
}
