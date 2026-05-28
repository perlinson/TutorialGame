using UnityEngine;

public enum SectHallType
{
    DutyHall,
    RefiningHall,
    AlchemyHall,
    TalismanHall,
    ScriptureHall,
    StewardHall,
    CaveResidence
}

public enum SectActionKind
{
    ResolveTaskBoard,
    ClaimActiveTask,
    UpgradeMainArtifact,
    UpgradeProtectiveRelic,
    CraftRecipe,
    ShowSummary,
    Placeholder
}

public sealed class SectHallDefinition
{
    public string Id;
    public string DisplayName;
    public string Subtitle;
    public string Description;
    public SectHallType HallType;
    public Sprite IllustrationImage;
    public Color PlaceholderColor;
    public string[] ActionIds;
}

public sealed class SectActionDefinition
{
    public string Id;
    public string HallId;
    public string Title;
    public string Description;
    public SectActionKind Kind;
    public string LinkedRecipeId;
}

public sealed class SectHallSnapshot
{
    public SectHallDefinition Definition;
    public string StatusSummary;
    public bool IsUnlocked;
    public string LockedReason;
    public SectActionSnapshot[] Actions;
}

public sealed class SectActionSnapshot
{
    public SectActionDefinition Definition;
    public string ButtonLabel;
    public bool IsAvailable;
    public string UnavailableReason;
}

public sealed class SectActionResult
{
    public SectActionResult(bool succeeded, string message, string hallId, string actionId)
    {
        Succeeded = succeeded;
        Message = message;
        HallId = hallId;
        ActionId = actionId;
    }

    public bool Succeeded { get; }
    public string Message { get; }
    public string HallId { get; }
    public string ActionId { get; }
}
