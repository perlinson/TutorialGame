using System;

[Serializable]
public sealed class SaveFactionState
{
    public ExpeditionEnemyFaction faction;
    public int defeatedCount;
    public int hostility;
    public string lastRegionId;

    public SaveFactionState()
    {
    }

    public SaveFactionState(ExpeditionEnemyFaction faction)
    {
        this.faction = faction;
        defeatedCount = 0;
        hostility = 0;
        lastRegionId = string.Empty;
    }

    public void EnsureDefaults()
    {
        if (lastRegionId == null)
        {
            lastRegionId = string.Empty;
        }
    }
}
