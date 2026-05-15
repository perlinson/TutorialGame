using System.Collections.Generic;
using UnityEngine;

public sealed class ExpeditionUiSnapshot
{
    public string HeaderTitle;
    public string HeaderSubtitle;
    public string HeaderStatus;
    public string HeaderResources;
    public string PhaseName;
    public string RoomTitle;
    public string RoomDescription;
    public string LoadoutSummary;
    public string EnemySummary;
    public string LogMessage;
    public string SkillSummary;
    public string HintMessage;
    public Sprite RoomIllustration;
    public string RoomIllustrationTitle;
    public Sprite HeroPortrait;
    public string HeroPortraitTitle;
    public Sprite EnemyPortrait;
    public string EnemyPortraitTitle;
}

public static class ExpeditionUiComposer
{
    public static ExpeditionUiSnapshot Build(
        MainMenuSaveData saveData,
        WorldRegionDefinition region,
        ExpeditionHeroState hero,
        List<ExpeditionRoomState> rooms,
        int currentRoomIndex,
        List<ExpeditionEnemyState> enemies,
        int torchlight,
        int supplies,
        int pendingQiGain,
        int pendingCrystalGain,
        List<SaveItemStack> pendingItemRewards,
        ExpeditionFlowPhase phase,
        string logMessage,
        string hintMessage)
    {
        var snapshot = new ExpeditionUiSnapshot();
        if (saveData == null || region == null || hero == null || rooms == null || rooms.Count == 0)
        {
            return snapshot;
        }

        var room = rooms[Mathf.Clamp(currentRoomIndex, 0, rooms.Count - 1)];
        var primaryEnemy = GetPrimaryEnemy(enemies);

        snapshot.HeaderTitle = region.DisplayName + " · " + region.Subtitle;
        snapshot.HeaderSubtitle = hero.HeroName + " / " + hero.ArchetypeName;
        snapshot.HeaderStatus = "境界 " + WorldRegionLibrary.GetRealmName(saveData.realmTier) +
                                "    气血 " + hero.CurrentHealth + " / " + hero.MaxHealth +
                                "    心境 " + hero.Stress + " / 100";
        snapshot.HeaderResources = "火光 " + torchlight +
                                   "    补给 " + supplies +
                                   "    符 " + hero.TalismanCharges +
                                   "    丹 " + hero.MedicineCharges +
                                   "    修为 +" + pendingQiGain +
                                   "    灵石 +" + pendingCrystalGain +
                                   "    行囊 " + saveData.GetUsedBagSlots() + "/" + saveData.bagCapacity +
                                   "    待收 " + (pendingItemRewards != null ? pendingItemRewards.Count : 0) + " 种";
        snapshot.PhaseName = GetPhaseName(phase);
        snapshot.RoomTitle = "第 " + (currentRoomIndex + 1) + " 室 / " + room.Title;
        snapshot.RoomDescription = room.Description;
        snapshot.LoadoutSummary = hero.Loadout.ToSummary();
        snapshot.EnemySummary = BuildEnemySummary(enemies, phase);
        snapshot.LogMessage = logMessage;
        snapshot.SkillSummary = ExpeditionBuildFactory.DescribeSkills(hero);
        snapshot.HintMessage = hintMessage;
        snapshot.RoomIllustration = room.IllustrationImage != null ? room.IllustrationImage : region.IllustrationImage;
        snapshot.RoomIllustrationTitle = room.Title;
        snapshot.HeroPortrait = hero.PortraitImage;
        snapshot.HeroPortraitTitle = hero.HeroName;
        snapshot.EnemyPortrait = primaryEnemy != null ? primaryEnemy.PortraitImage : null;
        snapshot.EnemyPortraitTitle = primaryEnemy != null ? primaryEnemy.Name : "敌阵占位";
        return snapshot;
    }

    private static string BuildEnemySummary(List<ExpeditionEnemyState> enemies, ExpeditionFlowPhase phase)
    {
        if (enemies == null || enemies.Count == 0 || phase == ExpeditionFlowPhase.RoomDecision || phase == ExpeditionFlowPhase.AfterRoom || phase == ExpeditionFlowPhase.Completed || phase == ExpeditionFlowPhase.Retreated || phase == ExpeditionFlowPhase.Failed)
        {
            return "本室暂无需要正面交锋的目标。";
        }

        var summary = string.Empty;
        var index = 1;
        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy == null || !enemy.IsAlive)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(summary))
            {
                summary += "\n";
            }

            summary += index + ". " + enemy.Name +
                       " [" + enemy.FactionLabel + "]" +
                       "  HP " + enemy.CurrentHealth + " / " + enemy.MaxHealth +
                       "  护体 " + enemy.GetEffectiveArmor() +
                       "  招式 " + enemy.TechniqueName;
            if (enemy.PoisonStacks > 0)
            {
                summary += "  丹火" + enemy.PoisonStacks;
            }

            if (enemy.StunnedTurns > 0)
            {
                summary += "  失衡";
            }

            if (enemy.ExposedTurns > 0)
            {
                summary += "  破绽";
            }

            index++;
        }

        return string.IsNullOrEmpty(summary) ? "本室暂无需要正面交锋的目标。" : summary;
    }

    private static ExpeditionEnemyState GetPrimaryEnemy(List<ExpeditionEnemyState> enemies)
    {
        if (enemies == null)
        {
            return null;
        }

        for (var i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null && enemies[i].IsAlive)
            {
                return enemies[i];
            }
        }

        return null;
    }

    private static string GetPhaseName(ExpeditionFlowPhase phase)
    {
        switch (phase)
        {
            case ExpeditionFlowPhase.RoomDecision:
                return "房间搜查";
            case ExpeditionFlowPhase.CombatPlayerTurn:
                return "术法交锋";
            case ExpeditionFlowPhase.AfterRoom:
                return "房间结算";
            case ExpeditionFlowPhase.Completed:
                return "远征完成";
            case ExpeditionFlowPhase.Retreated:
                return "主动撤离";
            default:
                return "远征失败";
        }
    }
}
