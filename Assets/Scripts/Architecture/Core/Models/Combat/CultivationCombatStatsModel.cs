using System.Collections.Generic;
using QFramework;

/// <summary>
/// M4 CombatStatsModel：战斗运行时属性的中央表。
/// - 玩家主角的属性以 BindableProperty 暴露，供 UI 直接订阅。
/// - 其它战斗参与者（敌方 / 召唤物 / 灵宠）以字典存 <see cref="CombatStatsSnapshot"/>。
/// 战斗结束应调用 <see cref="ResetForBattle"/> 释放敌方快照。
/// 真正的"基于 SaveData + Buff 重新计算"放在 <see cref="CultivationDamageSystem"/>
/// 或专门的 StatsCalculator，本 Model 只负责存与发布。
/// </summary>
public sealed class CultivationCombatStatsModel : AbstractModel
{
    public const string PlayerCombatantId = "player";

    private readonly Dictionary<string, CombatStatsSnapshot> snapshots = new Dictionary<string, CombatStatsSnapshot>();

    public readonly BindableProperty<int> PlayerCurrentHp = new BindableProperty<int>(0);
    public readonly BindableProperty<int> PlayerMaxHp = new BindableProperty<int>(0);
    public readonly BindableProperty<int> PlayerCurrentMana = new BindableProperty<int>(0);
    public readonly BindableProperty<int> PlayerMaxMana = new BindableProperty<int>(0);
    public readonly BindableProperty<int> PlayerSpeed = new BindableProperty<int>(0);

    public readonly EasyEvent SnapshotsChanged = new EasyEvent();

    protected override void OnInit()
    {
    }

    public bool TryGetSnapshot(string combatantId, out CombatStatsSnapshot snapshot)
    {
        if (string.IsNullOrWhiteSpace(combatantId))
        {
            snapshot = default;
            return false;
        }

        return snapshots.TryGetValue(combatantId, out snapshot);
    }

    public CombatStatsSnapshot GetOrCreate(string combatantId)
    {
        if (string.IsNullOrWhiteSpace(combatantId))
        {
            return CombatStatsSnapshot.CreateEmpty(string.Empty);
        }

        if (!snapshots.TryGetValue(combatantId, out var snapshot))
        {
            snapshot = CombatStatsSnapshot.CreateEmpty(combatantId);
            snapshots[combatantId] = snapshot;
        }

        return snapshot;
    }

    public void SetSnapshot(CombatStatsSnapshot snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot.CombatantId))
        {
            return;
        }

        snapshots[snapshot.CombatantId] = snapshot;

        if (snapshot.CombatantId == PlayerCombatantId)
        {
            PlayerCurrentHp.Value = snapshot.CurrentHp;
            PlayerMaxHp.Value = snapshot.MaxHp;
            PlayerCurrentMana.Value = snapshot.CurrentMana;
            PlayerMaxMana.Value = snapshot.MaxMana;
            PlayerSpeed.Value = snapshot.Speed;
        }

        SnapshotsChanged.Trigger();
    }

    public void RemoveSnapshot(string combatantId)
    {
        if (string.IsNullOrWhiteSpace(combatantId))
        {
            return;
        }

        if (snapshots.Remove(combatantId))
        {
            SnapshotsChanged.Trigger();
        }
    }

    public void ResetForBattle()
    {
        snapshots.Clear();
        PlayerCurrentHp.Value = 0;
        PlayerMaxHp.Value = 0;
        PlayerCurrentMana.Value = 0;
        PlayerMaxMana.Value = 0;
        PlayerSpeed.Value = 0;
        SnapshotsChanged.Trigger();
    }
}
