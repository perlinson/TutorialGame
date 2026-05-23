using QFramework;

public sealed class CultivationSettlementSystem : AbstractSystem
{
    private CultivationSaveSystem saveSystem;
    private CultivationRealmSystem realmSystem;

    protected override void OnInit()
    {
        saveSystem = this.GetSystem<CultivationSaveSystem>();
        realmSystem = this.GetSystem<CultivationRealmSystem>();
    }

    public WorldMapActionResult UpgradeProtectiveRelic(int slotIndex, MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            return new WorldMapActionResult(false, "当前没有可用存档。");
        }

        var cost = WorldRegionLibrary.GetVitalityUpgradeCost(saveData);
        if (saveData.spiritCrystals < cost)
        {
            return new WorldMapActionResult(false, "灵石不足，无法继续温养护身法器。");
        }

        saveData.spiritCrystals -= cost;
        saveData.protectiveRelicLevel++;
        saveData.vitalityLevel = saveData.protectiveRelicLevel;
        TouchSettlement(saveData);
        saveSystem.SaveArchive(slotIndex, saveData);
        return new WorldMapActionResult(true, "护身法器已温养，出行时的气血和护体都会更稳。");
    }

    public WorldMapActionResult UpgradeMainArtifact(int slotIndex, MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            return new WorldMapActionResult(false, "当前没有可用存档。");
        }

        var cost = WorldRegionLibrary.GetAttackUpgradeCost(saveData);
        if (saveData.spiritCrystals < cost)
        {
            return new WorldMapActionResult(false, "灵石不足，无法继续祭炼主法器。");
        }

        saveData.spiritCrystals -= cost;
        saveData.mainArtifactLevel++;
        saveData.attackLevel = saveData.mainArtifactLevel;
        TouchSettlement(saveData);
        saveSystem.SaveArchive(slotIndex, saveData);
        return new WorldMapActionResult(true, "主法器已祭炼，下一次出手会更凌厉。");
    }

    public WorldMapActionResult CraftRecipe(int slotIndex, MainMenuSaveData saveData, string recipeId)
    {
        if (saveData == null)
        {
            return new WorldMapActionResult(false, "当前没有可用存档。");
        }

        string summary;
        if (!WorkshopLibrary.Craft(saveData, recipeId, realmSystem, out summary))
        {
            return new WorldMapActionResult(false, summary);
        }

        TouchSettlement(saveData);
        saveSystem.SaveArchive(slotIndex, saveData);
        return new WorldMapActionResult(true, summary);
    }

    public string BuildSettlementSummary(MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            return "洞府未展开。";
        }

        saveData.EnsureDefaults();
        return WorkshopLibrary.BuildWorkshopSummary(saveData) +
               "\n\n洞府建设：" + saveData.settlementBuildCount + " 次" +
               "\n最近整备：" + (string.IsNullOrWhiteSpace(saveData.lastSettlementAction) ? "暂无" : saveData.lastSettlementAction);
    }

    private static void TouchSettlement(MainMenuSaveData saveData)
    {
        CultivationGameTime.Advance(saveData, 1);
        saveData.settlementBuildCount++;
        saveData.lastSettlementAction = CultivationGameTime.Format(saveData);
        saveData.lastPlayed = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    }
}
