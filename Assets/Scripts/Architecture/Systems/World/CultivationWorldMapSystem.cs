using QFramework;

public sealed class CultivationWorldMapSystem : AbstractSystem
{
    private CultivationSaveSystem saveSystem;
    private CultivationSettlementSystem settlementSystem;

    protected override void OnInit()
    {
        saveSystem = this.GetSystem<CultivationSaveSystem>();
        settlementSystem = this.GetSystem<CultivationSettlementSystem>();
    }

    public WorldMapActionResult TravelToRegion(int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region)
    {
        if (saveData == null || region == null)
        {
            return new WorldMapActionResult(false, "当前没有可用的地图数据。");
        }

        string reason;
        if (!WorldRegionLibrary.CanTravel(saveData, region, out reason))
        {
            return new WorldMapActionResult(false, reason);
        }

        saveData.currentRegionId = region.Id;
        saveData.location = region.DisplayName;
        CultivationGameTime.Advance(saveData, 1);
        saveData.lastPlayed = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        saveSystem.SaveArchive(slotIndex, saveData);
        return new WorldMapActionResult(true, "已整备路引，准备前往 " + region.DisplayName + "。");
    }

    public WorldMapActionResult UpgradeProtectiveRelic(int slotIndex, MainMenuSaveData saveData)
    {
        return settlementSystem.UpgradeProtectiveRelic(slotIndex, saveData);
    }

    public WorldMapActionResult UpgradeMainArtifact(int slotIndex, MainMenuSaveData saveData)
    {
        return settlementSystem.UpgradeMainArtifact(slotIndex, saveData);
    }

    public WorldMapActionResult CraftRecipe(int slotIndex, MainMenuSaveData saveData, string recipeId)
    {
        return settlementSystem.CraftRecipe(slotIndex, saveData, recipeId);
    }
}
