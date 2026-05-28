using UnityEngine;
using QFramework;

public sealed class CultivationFactionSystem : AbstractSystem
{
    protected override void OnInit()
    {
    }

    public FactionReputationSnapshot GetSnapshot(CultivationSaveData saveData, ExpeditionEnemyFaction faction)
    {
        if (saveData == null)
        {
            return new FactionReputationSnapshot
            {
                Faction = faction,
                DisplayName = GetFactionDisplayName(faction),
                AttitudeLabel = "未知"
            };
        }

        saveData.EnsureDefaults();
        var state = saveData.GetOrCreateFactionState(faction);
        return new FactionReputationSnapshot
        {
            Faction = faction,
            DisplayName = GetFactionDisplayName(faction),
            DefeatedCount = state.defeatedCount,
            Hostility = state.hostility,
            PressureLevel = GetFactionPressure(saveData, faction),
            LastRegionId = state.lastRegionId,
            AttitudeLabel = BuildAttitudeLabel(state.hostility)
        };
    }

    public int GetFactionPressure(CultivationSaveData saveData, ExpeditionEnemyFaction faction)
    {
        if (saveData == null)
        {
            return 0;
        }

        saveData.EnsureDefaults();
        var state = saveData.GetOrCreateFactionState(faction);
        return Mathf.Clamp(state.hostility / 3 + state.defeatedCount / 5, 0, 5);
    }

    public FactionReputationSnapshot RecordDefeat(CultivationSaveData saveData, ExpeditionEnemyFaction faction, string regionId, int count)
    {
        if (saveData == null)
        {
            return GetSnapshot(null, faction);
        }

        saveData.EnsureDefaults();
        var state = saveData.GetOrCreateFactionState(faction);
        var amount = Mathf.Max(1, count);
        state.defeatedCount += amount;
        state.hostility = Mathf.Clamp(state.hostility + amount, 0, 99);
        state.lastRegionId = regionId ?? string.Empty;
        return GetSnapshot(saveData, faction);
    }

    public string BuildFactionDefeatSummary(FactionReputationSnapshot snapshot)
    {
        if (snapshot == null || snapshot.PressureLevel <= 0)
        {
            return string.Empty;
        }

        return snapshot.DisplayName + " 对你的戒备提升至 " + snapshot.AttitudeLabel + "。";
    }

    public static string GetFactionDisplayName(ExpeditionEnemyFaction faction)
    {
        switch (faction)
        {
            case ExpeditionEnemyFaction.Bandit:
                return "山贼流寇";
            case ExpeditionEnemyFaction.Cultivator:
                return "邪修散修";
            case ExpeditionEnemyFaction.Beast:
                return "妖兽";
            case ExpeditionEnemyFaction.HeartDemon:
                return "心魔";
            case ExpeditionEnemyFaction.CorpsePuppet:
                return "尸傀";
            default:
                return "未知势力";
        }
    }

    private static string BuildAttitudeLabel(int hostility)
    {
        if (hostility >= 18)
        {
            return "追杀";
        }

        if (hostility >= 9)
        {
            return "敌视";
        }

        if (hostility >= 3)
        {
            return "警觉";
        }

        return "平静";
    }
}
