public struct ExpeditionPhaseChangedEvent
{
    public ExpeditionFlowPhase PreviousPhase;
    public ExpeditionFlowPhase NewPhase;
}

public struct ExpeditionHeroHealthChangedEvent
{
    public int CurrentHealth;
    public int MaxHealth;
}

public struct ExpeditionTorchlightChangedEvent
{
    public int PreviousValue;
    public int NewValue;
}

public struct ExpeditionSuppliesChangedEvent
{
    public int PreviousValue;
    public int NewValue;
}

public struct ExpeditionCombatRoundChangedEvent
{
    public int Round;
}

public struct ExpeditionResourcesChangedEvent
{
    public int PendingQiGain;
    public int PendingCrystalGain;
}

public struct ExpeditionViewRefreshRequestedEvent
{
}

public struct ExpeditionEventOpenedEvent
{
    public string EventId;
    public string ActiveTaskId;
    public ExpeditionRoomKind RoomKind;
}

public struct ExpeditionEventResolvedEvent
{
    public string EventId;
    public string OptionId;
    public string ActiveTaskId;
    public ExpeditionRoomKind RoomKind;
    public bool RoomResolved;
    public bool ExpeditionFailed;
}
