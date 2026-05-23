using System.Collections.Generic;
using QFramework;

/// <summary>
/// S4 BuffSystem：状态层运行时管理。
/// - 以 combatantId 为 key 存活动 Buff 列表。
/// - 应用规则由 BuffDefinition.stackingRule 决定。
/// - <see cref="Tick"/> 在每个回合开始/结束时由 BattleSystem 调用，递减 duration、结算 DoT。
/// 持久化 Buff（中长期 affliction）应放在 SaveData，本系统只管战斗内 / 短期状态。
/// </summary>
public sealed class CultivationBuffSystem : AbstractSystem
{
    private const string BuffDatabasePath = "Data/BuffDatabase";
    private readonly Dictionary<string, List<ActiveBuff>> active = new Dictionary<string, List<ActiveBuff>>();
    private BuffDatabaseAsset cachedDatabase;

    protected override void OnInit()
    {
    }

    public BuffDefinition GetDefinition(string buffId)
    {
        if (string.IsNullOrWhiteSpace(buffId))
        {
            return null;
        }

        var db = GetDatabase();
        if (db == null || db.buffs == null)
        {
            return null;
        }

        for (var i = 0; i < db.buffs.Length; i++)
        {
            if (db.buffs[i] != null && db.buffs[i].id == buffId)
            {
                return db.buffs[i];
            }
        }

        return null;
    }

    public bool Apply(string targetId, string buffId, string sourceId = null, int durationOverride = 0)
    {
        if (string.IsNullOrWhiteSpace(targetId))
        {
            return false;
        }

        var def = GetDefinition(buffId);
        if (def == null)
        {
            return false;
        }

        if (!active.TryGetValue(targetId, out var list))
        {
            list = new List<ActiveBuff>();
            active[targetId] = list;
        }

        var duration = durationOverride > 0 ? durationOverride : def.defaultDurationTurns;

        switch (def.stackingRule)
        {
            case BuffStackingRule.Independent:
                list.Add(new ActiveBuff(buffId, duration, 1, sourceId));
                return true;
            case BuffStackingRule.RefreshDuration:
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].BuffId == buffId)
                    {
                        list[i] = new ActiveBuff(buffId, duration, list[i].Stacks, sourceId);
                        return true;
                    }
                }

                list.Add(new ActiveBuff(buffId, duration, 1, sourceId));
                return true;
            case BuffStackingRule.StackUpToMax:
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].BuffId == buffId)
                    {
                        var nextStacks = list[i].Stacks + 1;
                        if (def.maxStacks > 0 && nextStacks > def.maxStacks)
                        {
                            nextStacks = def.maxStacks;
                        }

                        list[i] = new ActiveBuff(buffId, duration, nextStacks, sourceId);
                        return true;
                    }
                }

                list.Add(new ActiveBuff(buffId, duration, 1, sourceId));
                return true;
            case BuffStackingRule.Replace:
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].BuffId == buffId)
                    {
                        list.RemoveAt(i);
                    }
                }

                list.Add(new ActiveBuff(buffId, duration, 1, sourceId));
                return true;
            default:
                list.Add(new ActiveBuff(buffId, duration, 1, sourceId));
                return true;
        }
    }

    public bool Remove(string targetId, string buffId)
    {
        if (string.IsNullOrWhiteSpace(targetId) || !active.TryGetValue(targetId, out var list))
        {
            return false;
        }

        for (var i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].BuffId == buffId)
            {
                list.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public IReadOnlyList<ActiveBuff> GetActive(string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId) || !active.TryGetValue(targetId, out var list))
        {
            return System.Array.Empty<ActiveBuff>();
        }

        return list;
    }

    /// <summary>对单个目标 tick 一回合，返回本回合的 DoT/HoT 总量（>0 为伤害，<0 为治疗）。</summary>
    public int Tick(string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId) || !active.TryGetValue(targetId, out var list))
        {
            return 0;
        }

        var deltaHp = 0;
        for (var i = list.Count - 1; i >= 0; i--)
        {
            var entry = list[i];
            var def = GetDefinition(entry.BuffId);
            if (def == null)
            {
                list.RemoveAt(i);
                continue;
            }

            if (def.tickDamage != 0)
            {
                deltaHp += def.tickDamage * (entry.Stacks <= 0 ? 1 : entry.Stacks);
            }

            var nextDuration = entry.RemainingTurns - 1;
            if (nextDuration <= 0)
            {
                list.RemoveAt(i);
            }
            else
            {
                list[i] = new ActiveBuff(entry.BuffId, nextDuration, entry.Stacks, entry.SourceId);
            }
        }

        return deltaHp;
    }

    public void ClearTarget(string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId))
        {
            return;
        }

        active.Remove(targetId);
    }

    public void ClearAll()
    {
        active.Clear();
    }

    public void Reload()
    {
        cachedDatabase = null;
    }

    private BuffDatabaseAsset GetDatabase()
    {
        if (cachedDatabase != null)
        {
            return cachedDatabase;
        }

        cachedDatabase = GameData.LoadAsset<BuffDatabaseAsset>(BuffDatabasePath);
        return cachedDatabase;
    }
}

public readonly struct ActiveBuff
{
    public readonly string BuffId;
    public readonly int RemainingTurns;
    public readonly int Stacks;
    public readonly string SourceId;

    public ActiveBuff(string buffId, int remainingTurns, int stacks, string sourceId)
    {
        BuffId = buffId ?? string.Empty;
        RemainingTurns = remainingTurns < 0 ? 0 : remainingTurns;
        Stacks = stacks <= 0 ? 1 : stacks;
        SourceId = sourceId ?? string.Empty;
    }
}
