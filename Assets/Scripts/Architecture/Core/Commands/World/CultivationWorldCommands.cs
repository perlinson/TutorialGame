using QFramework;

public sealed class BuildSettlementSummaryCommand : AbstractCommand<string>
{
    private readonly CultivationSaveData saveData;

    public BuildSettlementSummaryCommand(CultivationSaveData saveData)
    {
        this.saveData = saveData;
    }

    protected override string OnExecute()
    {
        return this.GetSystem<CultivationSettlementSystem>().BuildSettlementSummary(saveData);
    }
}

public sealed class BuildSectOverviewCommand : AbstractCommand<string>
{
    private readonly CultivationSaveData saveData;

    public BuildSectOverviewCommand(CultivationSaveData saveData)
    {
        this.saveData = saveData;
    }

    protected override string OnExecute()
    {
        return this.GetSystem<CultivationSectSystem>().BuildSectOverview(saveData);
    }
}

public sealed class GetSectHallSnapshotsCommand : AbstractCommand<SectHallSnapshot[]>
{
    private readonly CultivationSaveData saveData;

    public GetSectHallSnapshotsCommand(CultivationSaveData saveData)
    {
        this.saveData = saveData;
    }

    protected override SectHallSnapshot[] OnExecute()
    {
        return this.GetSystem<CultivationSectSystem>().GetHallSnapshots(saveData);
    }
}

public sealed class ExecuteSectActionCommand : AbstractCommand<SectActionResult>
{
    private readonly int slotIndex;
    private readonly CultivationSaveData saveData;
    private readonly string actionId;

    public ExecuteSectActionCommand(int slotIndex, CultivationSaveData saveData, string actionId)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
        this.actionId = actionId;
    }

    protected override SectActionResult OnExecute()
    {
        return this.GetSystem<CultivationSectSystem>().ExecuteAction(slotIndex, saveData, actionId);
    }
}

public sealed class TravelToRegionCommand : AbstractCommand<WorldMapActionResult>
{
    private readonly int slotIndex;
    private readonly CultivationSaveData saveData;
    private readonly WorldRegionDefinition region;

    public TravelToRegionCommand(int slotIndex, CultivationSaveData saveData, WorldRegionDefinition region)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
        this.region = region;
    }

    protected override WorldMapActionResult OnExecute()
    {
        return this.GetSystem<CultivationWorldMapSystem>().TravelToRegion(slotIndex, saveData, region);
    }
}

public sealed class BuildWorldMapRegionSnapshotCommand : AbstractCommand<WorldMapRegionSnapshot>
{
    private readonly CultivationSaveData saveData;
    private readonly string regionId;
    private readonly string fallbackRegionId;

    public BuildWorldMapRegionSnapshotCommand(CultivationSaveData saveData, string regionId, string fallbackRegionId)
    {
        this.saveData = saveData;
        this.regionId = regionId ?? string.Empty;
        this.fallbackRegionId = fallbackRegionId ?? string.Empty;
    }

    protected override WorldMapRegionSnapshot OnExecute()
    {
        return this.GetSystem<CultivationWorldMapSnapshotSystem>().BuildRegionSnapshot(saveData, regionId, fallbackRegionId);
    }
}

public sealed class BuildWorldMapInventorySnapshotCommand : AbstractCommand<WorldMapInventorySnapshot>
{
    private readonly CultivationSaveData saveData;

    public BuildWorldMapInventorySnapshotCommand(CultivationSaveData saveData)
    {
        this.saveData = saveData;
    }

    protected override WorldMapInventorySnapshot OnExecute()
    {
        return this.GetSystem<CultivationWorldMapSnapshotSystem>().BuildInventorySnapshot(saveData);
    }
}

public sealed class BuildWorldMapWorkshopSnapshotCommand : AbstractCommand<WorldMapWorkshopSnapshot>
{
    private readonly CultivationSaveData saveData;

    public BuildWorldMapWorkshopSnapshotCommand(CultivationSaveData saveData)
    {
        this.saveData = saveData;
    }

    protected override WorldMapWorkshopSnapshot OnExecute()
    {
        return this.GetSystem<CultivationWorldMapSnapshotSystem>().BuildWorkshopSnapshot(saveData);
    }
}

public sealed class BuildWorldMapSettlementSnapshotCommand : AbstractCommand<WorldMapSettlementSnapshot>
{
    private readonly CultivationSaveData saveData;

    public BuildWorldMapSettlementSnapshotCommand(CultivationSaveData saveData)
    {
        this.saveData = saveData;
    }

    protected override WorldMapSettlementSnapshot OnExecute()
    {
        return this.GetSystem<CultivationWorldMapSnapshotSystem>().BuildSettlementSnapshot(saveData);
    }
}

public sealed class BuildWorldMapSectResidenceSnapshotCommand : AbstractCommand<WorldMapSectResidenceSnapshot>
{
    private readonly CultivationSaveData saveData;
    private readonly int selectedSectHallIndex;

    public BuildWorldMapSectResidenceSnapshotCommand(CultivationSaveData saveData, int selectedSectHallIndex)
    {
        this.saveData = saveData;
        this.selectedSectHallIndex = selectedSectHallIndex;
    }

    protected override WorldMapSectResidenceSnapshot OnExecute()
    {
        return this.GetSystem<CultivationWorldMapSnapshotSystem>().BuildSectResidenceSnapshot(saveData, selectedSectHallIndex);
    }
}

public sealed class UpgradeProtectiveRelicCommand : AbstractCommand<WorldMapActionResult>
{
    private readonly int slotIndex;
    private readonly CultivationSaveData saveData;

    public UpgradeProtectiveRelicCommand(int slotIndex, CultivationSaveData saveData)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
    }

    protected override WorldMapActionResult OnExecute()
    {
        return this.GetSystem<CultivationWorldMapSystem>().UpgradeProtectiveRelic(slotIndex, saveData);
    }
}

public sealed class UpgradeMainArtifactCommand : AbstractCommand<WorldMapActionResult>
{
    private readonly int slotIndex;
    private readonly CultivationSaveData saveData;

    public UpgradeMainArtifactCommand(int slotIndex, CultivationSaveData saveData)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
    }

    protected override WorldMapActionResult OnExecute()
    {
        return this.GetSystem<CultivationWorldMapSystem>().UpgradeMainArtifact(slotIndex, saveData);
    }
}

public sealed class CraftWorldMapRecipeCommand : AbstractCommand<WorldMapActionResult>
{
    private readonly int slotIndex;
    private readonly CultivationSaveData saveData;
    private readonly string recipeId;

    public CraftWorldMapRecipeCommand(int slotIndex, CultivationSaveData saveData, string recipeId)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
        this.recipeId = recipeId;
    }

    protected override WorldMapActionResult OnExecute()
    {
        return this.GetSystem<CultivationWorldMapSystem>().CraftRecipe(slotIndex, saveData, recipeId);
    }
}

public sealed class BuildNpcDialogueSnapshotCommand : AbstractCommand<WorldMapNpcDialogueSnapshot>
{
    private readonly CultivationSaveData saveData;
    private readonly NpcSceneType sceneType;
    private readonly string regionId;
    private readonly string sectHallId;
    private readonly string locationId;
    private readonly string selectedNpcId;

    public BuildNpcDialogueSnapshotCommand(CultivationSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, string locationId, string selectedNpcId)
    {
        this.saveData = saveData;
        this.sceneType = sceneType;
        this.regionId = regionId ?? string.Empty;
        this.sectHallId = sectHallId ?? string.Empty;
        this.locationId = locationId ?? string.Empty;
        this.selectedNpcId = selectedNpcId ?? string.Empty;
    }

    protected override WorldMapNpcDialogueSnapshot OnExecute()
    {
        return this.GetSystem<CultivationNpcSystem>().BuildDialogueSnapshot(saveData, sceneType, regionId, sectHallId, locationId, selectedNpcId);
    }
}

public sealed class ExecuteNpcDialogueChoiceCommand : AbstractCommand<NpcInteractionResult>
{
    private readonly int slotIndex;
    private readonly CultivationSaveData saveData;
    private readonly NpcSceneType sceneType;
    private readonly string regionId;
    private readonly string sectHallId;
    private readonly string locationId;
    private readonly string npcId;
    private readonly string choiceId;

    public ExecuteNpcDialogueChoiceCommand(int slotIndex, CultivationSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, string locationId, string npcId, string choiceId)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
        this.sceneType = sceneType;
        this.regionId = regionId ?? string.Empty;
        this.sectHallId = sectHallId ?? string.Empty;
        this.locationId = locationId ?? string.Empty;
        this.npcId = npcId ?? string.Empty;
        this.choiceId = choiceId ?? string.Empty;
    }

    protected override NpcInteractionResult OnExecute()
    {
        return this.GetSystem<CultivationNpcSystem>().ExecuteChoice(slotIndex, saveData, sceneType, regionId, sectHallId, locationId, npcId, choiceId);
    }
}

public sealed class StartEventConversationCommand : AbstractCommand<bool>
{
    private readonly string conversationTitle;
    private readonly CultivationSaveData saveData;
    private readonly System.Action onEnd;

    public StartEventConversationCommand(string conversationTitle, CultivationSaveData saveData, System.Action onEnd = null)
    {
        this.conversationTitle = conversationTitle ?? string.Empty;
        this.saveData = saveData;
        this.onEnd = onEnd;
    }

    protected override bool OnExecute()
    {
        return this.GetSystem<CultivationDialogueSystem>().TryStartEventConversation(conversationTitle, saveData, onEnd);
    }
}
