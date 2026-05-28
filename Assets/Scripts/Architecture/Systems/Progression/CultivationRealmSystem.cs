using System.Text;
using QFramework;
using UnityEngine;

/// <summary>
/// S1 RealmSystem：修炼 / 突破 / 瓶颈 / 心魔 业务下沉。
/// 负责对 <see cref="CultivationSaveData"/> 上的 realmTier / qi / atBottleneck / breakthroughCount / heartDemonMark
/// 进行加减并同步到 <see cref="CultivationRealmModel"/>。不直接负责存盘，由调用方在合适的时机
/// （Command / SaveSystem）触发 <see cref="ISaveSystem"/>.SaveArchive。
/// </summary>
public sealed class CultivationRealmSystem : AbstractSystem
{
    private const int HeartDemonMarkCap = 10;
    private CultivationBranchModel branchModel;

    protected override void OnInit()
    {
        branchModel = this.GetModel<CultivationBranchModel>();
    }

    /// <summary>当前境界突破到下一境所需修为，0 表示已封顶。</summary>
    public int GetQiRequiredForNextRealm(int realmTier)
    {
        return WorldRegionLibrary.GetQiRequiredForNextRealm(realmTier);
    }

    public string GetRealmName(int realmTier)
    {
        return WorldRegionLibrary.GetRealmName(realmTier);
    }

    /// <summary>
    /// 增加修为，自动尝试连环突破，返回实际突破次数。
    /// 当 <paramref name="autoBreakthrough"/> 为 false 时仅累加修为，不触发突破。
    /// </summary>
    public RealmGainResult GainQi(CultivationSaveData saveData, int amount, bool autoBreakthrough = true)
    {
        if (saveData == null || amount <= 0)
        {
            return RealmGainResult.None;
        }

        saveData.EnsureDefaults();
        saveData.qi += amount;

        var breakthroughs = 0;
        var startTier = saveData.realmTier;

        if (autoBreakthrough && !saveData.atBottleneck)
        {
            while (true)
            {
                var requiredQi = GetQiRequiredForNextRealm(saveData.realmTier);
                if (requiredQi <= 0 || saveData.qi < requiredQi)
                {
                    break;
                }

                saveData.qi -= requiredQi;
                saveData.realmTier++;
                saveData.breakthroughCount++;
                breakthroughs++;
            }
        }

        if (breakthroughs > 0)
        {
            saveData.realm = GetRealmName(saveData.realmTier);
            // 境界突破时更新分支基数
            if (branchModel != null)
            {
                branchModel.UpdateRealmBase(saveData.realmTier);
            }
        }

        SyncModel(saveData);
        return new RealmGainResult(amount, breakthroughs, startTier, saveData.realmTier);
    }

    /// <summary>显式尝试一次突破（用于"打坐 / 闭关 / 服丹"等主动行为）。</summary>
    public RealmBreakthroughResult TryBreakthrough(CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            return new RealmBreakthroughResult(false, 0, 0, "当前没有有效存档。");
        }

        saveData.EnsureDefaults();

        if (saveData.atBottleneck)
        {
            return new RealmBreakthroughResult(false, saveData.realmTier, saveData.realmTier, "正陷于瓶颈，需先静心化解。");
        }

        var requiredQi = GetQiRequiredForNextRealm(saveData.realmTier);
        if (requiredQi <= 0)
        {
            return new RealmBreakthroughResult(false, saveData.realmTier, saveData.realmTier, "境界已至当前体系顶端。");
        }

        if (saveData.qi < requiredQi)
        {
            return new RealmBreakthroughResult(false, saveData.realmTier, saveData.realmTier, "修为不足以冲击 " + GetRealmName(saveData.realmTier + 1) + "，尚差 " + (requiredQi - saveData.qi) + " 缕灵气。");
        }

        var startTier = saveData.realmTier;
        saveData.qi -= requiredQi;
        saveData.realmTier++;
        saveData.breakthroughCount++;
        saveData.realm = GetRealmName(saveData.realmTier);

        // 境界突破时更新分支基数
        if (branchModel != null)
        {
            branchModel.UpdateRealmBase(saveData.realmTier);
        }

        SyncModel(saveData);

        return new RealmBreakthroughResult(true, startTier, saveData.realmTier, "成功突破至 " + saveData.realm + "。");
    }

    /// <summary>进入瓶颈状态，后续修为获取不会触发自动突破。</summary>
    public void EnterBottleneck(CultivationSaveData saveData, string reason = null)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.EnsureDefaults();
        saveData.atBottleneck = true;
        SyncModel(saveData);
    }

    public void ExitBottleneck(CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.EnsureDefaults();
        saveData.atBottleneck = false;
        SyncModel(saveData);
    }

    public void AddHeartDemonMark(CultivationSaveData saveData, int delta)
    {
        if (saveData == null || delta == 0)
        {
            return;
        }

        saveData.EnsureDefaults();
        saveData.heartDemonMark = Mathf.Clamp(saveData.heartDemonMark + delta, 0, HeartDemonMarkCap);
        SyncModel(saveData);
    }

    public void ClearHeartDemonMark(CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.EnsureDefaults();
        saveData.heartDemonMark = 0;
        SyncModel(saveData);
    }

    public string BuildSummary(CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            return string.Empty;
        }

        saveData.EnsureDefaults();
        var builder = new StringBuilder();
        builder.Append("境界：").Append(string.IsNullOrWhiteSpace(saveData.realm) ? GetRealmName(saveData.realmTier) : saveData.realm).Append('\n');
        var requiredQi = GetQiRequiredForNextRealm(saveData.realmTier);
        if (requiredQi > 0)
        {
            builder.Append("修为：").Append(saveData.qi).Append(" / ").Append(requiredQi).Append('\n');
        }
        else
        {
            builder.Append("修为：").Append(saveData.qi).Append("（已至当前体系顶端）\n");
        }

        if (saveData.atBottleneck)
        {
            builder.Append("当前陷于瓶颈，需先化解。\n");
        }

        if (saveData.heartDemonMark > 0)
        {
            builder.Append("心魔印记：").Append(saveData.heartDemonMark).Append(" 层\n");
        }

        builder.Append("历次突破：").Append(saveData.breakthroughCount).Append(" 次");
        return builder.ToString();
    }

    private void SyncModel(CultivationSaveData saveData)
    {
        var model = this.GetModel<CultivationRealmModel>();
        if (model != null)
        {
            model.Apply(saveData);
        }

        var attribute = this.GetModel<CultivationAttributeModel>();
        if (attribute != null)
        {
            attribute.Apply(saveData);
        }
    }
}

public readonly struct RealmGainResult
{
    public readonly int QiGained;
    public readonly int BreakthroughCount;
    public readonly int StartTier;
    public readonly int EndTier;

    public RealmGainResult(int qiGained, int breakthroughCount, int startTier, int endTier)
    {
        QiGained = qiGained;
        BreakthroughCount = breakthroughCount;
        StartTier = startTier;
        EndTier = endTier;
    }

    public bool HasBreakthrough => BreakthroughCount > 0;

    public static RealmGainResult None => new RealmGainResult(0, 0, 0, 0);
}

public readonly struct RealmBreakthroughResult
{
    public readonly bool Success;
    public readonly int StartTier;
    public readonly int EndTier;
    public readonly string Message;

    public RealmBreakthroughResult(bool success, int startTier, int endTier, string message)
    {
        Success = success;
        StartTier = startTier;
        EndTier = endTier;
        Message = message ?? string.Empty;
    }
}
