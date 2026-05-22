using UnityEngine;

public sealed class WorldMapActionResult
{
    public WorldMapActionResult(bool succeeded, string message)
    {
        Succeeded = succeeded;
        Message = message;
    }

    public bool Succeeded { get; }
    public string Message { get; }
}

public sealed class ExpeditionResolutionResult
{
    public ExpeditionResolutionResult(string logMessage, string hintMessage)
    {
        LogMessage = logMessage;
        HintMessage = hintMessage;
    }

    public string LogMessage { get; }
    public string HintMessage { get; }
}

public sealed class CombatTurnContext
{
    public MainMenuSaveData SaveData;
    public WorldRegionDefinition Region;
    public ExpeditionRoomState Room;
    public ExpeditionHeroState Hero;
    public System.Collections.Generic.List<ExpeditionEnemyState> Enemies;
    public System.Collections.Generic.List<ExpeditionEnemyState> CurrentEncounterSnapshot;
    public System.Collections.Generic.List<SaveItemStack> PendingItemRewards;
    public int CurrentRoomIndex;
    public int CombatRound;
    public int Torchlight;
    public int Supplies;
    public int PendingQiGain;
    public int PendingCrystalGain;
}

public sealed class CombatTurnResult
{
    public string LogMessage;
    public string HintMessage;
    public string FailureReason;
    public int CombatRound;
    public int Torchlight;
    public int Supplies;
    public int PendingQiGain;
    public int PendingCrystalGain;
    public bool CombatCleared;
    public bool ExpeditionFailed;
}

public sealed class ExpeditionRoomActionResult
{
    public string LogMessage;
    public string HintMessage;
    public string FailureReason;
    public int Torchlight;
    public int Supplies;
    public int PendingQiGain;
    public int PendingCrystalGain;
    public bool ExpeditionFailed;
}

public sealed class ExpeditionSupportActionResult
{
    public string LogMessage;
    public string HintMessage;
    public string FailureReason;
    public int Torchlight;
    public int Supplies;
    public bool RoomResolved;
    public bool ExpeditionFailed;
}

public sealed class ExpeditionTraversalContext
{
    public WorldRegionDefinition Region;
    public ExpeditionHeroState Hero;
    public ExpeditionRoomState Room;
    public int RoomIndex;
    public int RoomCount;
    public int Torchlight;
}

public sealed class ExpeditionTraversalResult
{
    public string LogMessage;
    public string HintMessage;
    public string FailureReason;
    public int RoomIndex;
    public int Torchlight;
    public ExpeditionFlowPhase Phase;
    public bool StartCombat;
    public bool ExpeditionFailed;
}

public sealed class ExpeditionAdvanceContext
{
    public ExpeditionFlowPhase Phase;
    public int CurrentRoomIndex;
    public int RoomCount;
}

public sealed class ExpeditionAdvanceResult
{
    public int NextRoomIndex;
    public bool ShouldSearchCurrentRoom;
    public bool ShouldEnterNextRoom;
    public bool ShouldCompleteExpedition;
    public bool ShouldReturnToWorldMap;
}

public sealed class ExpeditionLootCollectionResult
{
    public string LootSummary;
}

public sealed class RewardBankResult
{
    public string BankedSummary;
    public string OverflowSummary;
    public int OverflowCrystalGain;
}

public sealed class FactionReputationSnapshot
{
    public ExpeditionEnemyFaction Faction;
    public string DisplayName;
    public int DefeatedCount;
    public int Hostility;
    public int PressureLevel;
    public string LastRegionId;
    public string AttitudeLabel;
}

public sealed class StorySignal
{
    public string StoryId;
    public string NodeId;
    public string Title;
    public string ResultText;
}

public sealed class StorySignalResult
{
    public bool Recorded;
    public string StoryFlag;
    public string Message;
}

public sealed class MindStateResult
{
    public int PreviousStress;
    public int CurrentStress;
    public int HealthDamage;
    public bool BreakdownTriggered;
    public bool ExpeditionFailed;
    public string FailureReason;
    public string Message;
}

public sealed class TaskContextSnapshot
{
    public string ActiveTaskId;
    public string ActiveTaskTitle;
    public string ActiveTaskSummary;
    public string[] ActiveTaskTags;
    public string[] TaskLinkedRegionIds;
    public bool HasLinkedFaction;
    public ExpeditionEnemyFaction TaskLinkedFaction;
    public string[] TaskStateFlags;
    public string[] TriggeredEventIds;
    public string[] ChosenOptionIds;
    public string[] InjectEventIds;
    public string[] SuppressEventTags;
    public bool CanClaim;
    public Sprite IllustrationImage;
}

public sealed class TaskProgressResult
{
    public bool ProgressChanged;
    public bool CompletedNow;
    public string Message;
    public int CurrentProgress;
    public int RequiredCount;
}

public sealed class NpcInteractionResult
{
    public NpcInteractionResult(bool succeeded, string message, string selectedNpcId)
    {
        Succeeded = succeeded;
        Message = message ?? string.Empty;
        SelectedNpcId = selectedNpcId ?? string.Empty;
    }

    public bool Succeeded { get; }
    public string Message { get; }
    public string SelectedNpcId { get; }
}

public sealed class EnemyIntentPreview
{
    public string RoleLabel;
    public string IntentLabel;
    public string DetailText;
}

public sealed class ExpeditionEventCardResult
{
    public string EventId;
    public string Title;
    public string Body;
    public string BadgeText;
    public string FailureReason;
    public Sprite IllustrationImage;
    public ExpeditionEventOptionPresentation[] Options;
}

public sealed class ExpeditionEventOptionPresentation
{
    public string OptionId;
    public string Label;
    public string RequirementText;
    public string BadgeText;
    public bool IsAvailable;
}

public sealed class ExpeditionEventOptionResult
{
    public string LogMessage;
    public string HintMessage;
    public string ResultTitle;
    public string ResultBody;
    public string ResultBadgeText;
    public string FailureReason;
    public int Torchlight;
    public int Supplies;
    public int PendingQiGain;
    public int PendingCrystalGain;
    public bool RoomResolved;
    public bool ExpeditionFailed;
}
