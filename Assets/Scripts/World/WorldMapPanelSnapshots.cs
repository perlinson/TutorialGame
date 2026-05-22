using UnityEngine;

public sealed class GameHubSnapshot
{
    public string WorldTimeText;
    public string HeroName;
    public string RealmText;
    public string LocationText;
    public string HealthText;
    public string SpiritText;
    public string ResourceText;
    public int CurrentHealth;
    public int MaxHealth;
    public int CurrentSpirit;
    public int MaxSpirit;
    public Sprite Portrait;
    public bool ShowSectButton;
    public bool MapSelected;
    public bool SettlementSelected;
    public bool SectSelected;
}

public sealed class WorldMapPreviewSnapshot
{
    public Sprite Sprite;
    public string Label;
    public Color PlaceholderColor;
}

public sealed class WorldMapRegionSnapshot
{
    public string PanelTitle;
    public string PanelSubtitle;
    public string Description;
    public string Status;
    public string TaskSummary;
    public string TravelButtonLabel;
    public string VitalityButtonLabel;
    public string AttackButtonLabel;
    public bool CanTravel;
    public bool CanUpgradeVitality;
    public bool CanUpgradeAttack;
    public WorldMapPreviewSnapshot Preview;
}

public sealed class WorldMapSettlementSnapshot
{
    public string PanelTitle;
    public string PanelSubtitle;
    public string SummaryText;
    public string StatusText;
    public string ActionHintText;
    public string InventoryButtonLabel;
    public string WorkshopButtonLabel;
    public string VitalityButtonLabel;
    public string AttackButtonLabel;
    public bool CanUpgradeVitality;
    public bool CanUpgradeAttack;
    public WorldMapPreviewSnapshot Preview;
}

public enum PlayerCompendiumMainTab
{
    Character,
    Items,
    Talents,
    Arts
}

public sealed class PlayerCompendiumSectionSnapshot
{
    public string Id;
    public string Label;
}

public sealed class PlayerCompendiumVisualNodeSnapshot
{
    public string Id;
    public string Title;
    public string Subtitle;
    public string Description;
    public string StateText;
    public bool IsUnlocked;
    public bool IsFocused;
}

public sealed class PlayerCompendiumSnapshot
{
    public string PanelTitle;
    public string PanelSubtitle;
    public string SummaryText;
    public string ContentTitle;
    public string ContentBody;
    public string ResolvedSectionId;
    public string VisualTitle;
    public WorldMapPreviewSnapshot Preview;
    public PlayerCompendiumSectionSnapshot[] Sections;
    public PlayerCompendiumVisualNodeSnapshot[] VisualNodes;
}

public sealed class WorldMapInventorySnapshot
{
    public string DetailText;
    public WorldMapPreviewSnapshot Preview;
}

public sealed class WorldMapWorkshopRecipeSnapshot
{
    public string RecipeId;
    public string ButtonLabel;
    public bool IsInteractable;
    public string TooltipTitle;
    public string TooltipBody;
}

public sealed class WorldMapWorkshopSnapshot
{
    public string SummaryText;
    public WorldMapPreviewSnapshot Preview;
    public WorldMapWorkshopRecipeSnapshot[] Recipes;
}

public sealed class WorldMapSectHallButtonSnapshot
{
    public string DisplayName;
    public bool IsSelected;
}

public sealed class WorldMapSectActionButtonSnapshot
{
    public string ActionId;
    public string ButtonLabel;
    public bool IsVisible;
    public bool IsInteractable;
    public string TooltipTitle;
    public string TooltipBody;
}

public sealed class WorldMapSectResidenceSnapshot
{
    public string PanelTitle;
    public string PanelSubtitle;
    public string HallTitle;
    public string Description;
    public string Status;
    public WorldMapPreviewSnapshot Preview;
    public WorldMapSectHallButtonSnapshot[] HallButtons;
    public WorldMapSectActionButtonSnapshot[] ActionButtons;
}

public sealed class WorldMapNpcEntrySnapshot
{
    public string NpcId;
    public string DisplayName;
    public string RoleLabel;
    public string StatusText;
    public bool IsSelected;
    public bool IsInteractable;
}

public sealed class WorldMapNpcChoiceSnapshot
{
    public string ChoiceId;
    public string ButtonLabel;
    public string Description;
    public bool IsVisible;
    public bool IsInteractable;
    public string TooltipTitle;
    public string TooltipBody;
}

public sealed class WorldMapNpcDialogueSnapshot
{
    public string PanelTitle;
    public string PanelSubtitle;
    public string StorySummary;
    public string TaskSummary;
    public string NpcTitle;
    public string NpcSubtitle;
    public string NpcDescription;
    public string NpcStatus;
    public string SelectedNpcId;
    public WorldMapPreviewSnapshot Preview;
    public WorldMapNpcEntrySnapshot[] Entries;
    public WorldMapNpcChoiceSnapshot[] Choices;
}
