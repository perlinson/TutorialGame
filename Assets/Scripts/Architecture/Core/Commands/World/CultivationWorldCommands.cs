using QFramework;

public sealed class BuildSettlementSummaryCommand : AbstractCommand<string>
{
    private readonly MainMenuSaveData saveData;

    public BuildSettlementSummaryCommand(MainMenuSaveData saveData)
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
    private readonly MainMenuSaveData saveData;

    public BuildSectOverviewCommand(MainMenuSaveData saveData)
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
    private readonly MainMenuSaveData saveData;

    public GetSectHallSnapshotsCommand(MainMenuSaveData saveData)
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
    private readonly MainMenuSaveData saveData;
    private readonly string actionId;

    public ExecuteSectActionCommand(int slotIndex, MainMenuSaveData saveData, string actionId)
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
    private readonly MainMenuSaveData saveData;
    private readonly WorldRegionDefinition region;

    public TravelToRegionCommand(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region)
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

public sealed class UpgradeProtectiveRelicCommand : AbstractCommand<WorldMapActionResult>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;

    public UpgradeProtectiveRelicCommand(int slotIndex, MainMenuSaveData saveData)
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
    private readonly MainMenuSaveData saveData;

    public UpgradeMainArtifactCommand(int slotIndex, MainMenuSaveData saveData)
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
    private readonly MainMenuSaveData saveData;
    private readonly string recipeId;

    public CraftWorldMapRecipeCommand(int slotIndex, MainMenuSaveData saveData, string recipeId)
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
    private readonly MainMenuSaveData saveData;
    private readonly NpcSceneType sceneType;
    private readonly string regionId;
    private readonly string sectHallId;
    private readonly string selectedNpcId;

    public BuildNpcDialogueSnapshotCommand(MainMenuSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, string selectedNpcId)
    {
        this.saveData = saveData;
        this.sceneType = sceneType;
        this.regionId = regionId ?? string.Empty;
        this.sectHallId = sectHallId ?? string.Empty;
        this.selectedNpcId = selectedNpcId ?? string.Empty;
    }

    protected override WorldMapNpcDialogueSnapshot OnExecute()
    {
        return this.GetSystem<CultivationNpcSystem>().BuildDialogueSnapshot(saveData, sceneType, regionId, sectHallId, selectedNpcId);
    }
}

public sealed class ExecuteNpcDialogueChoiceCommand : AbstractCommand<NpcInteractionResult>
{
    private readonly int slotIndex;
    private readonly MainMenuSaveData saveData;
    private readonly NpcSceneType sceneType;
    private readonly string regionId;
    private readonly string sectHallId;
    private readonly string npcId;
    private readonly string choiceId;

    public ExecuteNpcDialogueChoiceCommand(int slotIndex, MainMenuSaveData saveData, NpcSceneType sceneType, string regionId, string sectHallId, string npcId, string choiceId)
    {
        this.slotIndex = slotIndex;
        this.saveData = saveData;
        this.sceneType = sceneType;
        this.regionId = regionId ?? string.Empty;
        this.sectHallId = sectHallId ?? string.Empty;
        this.npcId = npcId ?? string.Empty;
        this.choiceId = choiceId ?? string.Empty;
    }

    protected override NpcInteractionResult OnExecute()
    {
        return this.GetSystem<CultivationNpcSystem>().ExecuteChoice(slotIndex, saveData, sceneType, regionId, sectHallId, npcId, choiceId);
    }
}
