using System.Text;

public static class CultivationLoadoutLibrary
{
    public static int GetMainArtifactUpgradeCost(MainMenuSaveData saveData)
    {
        saveData.EnsureDefaults();
        return 3 + saveData.mainArtifactLevel * 2;
    }

    public static int GetProtectiveRelicUpgradeCost(MainMenuSaveData saveData)
    {
        saveData.EnsureDefaults();
        return 3 + saveData.protectiveRelicLevel * 2;
    }

    public static string GetMainArtifactName(string archetypeId, int level)
    {
        return ApplyTierLabel(
            archetypeId == "alchemist" ? "离火丹轮" :
            archetypeId == "wanderer" ? "玄纹符伞" :
            "青魄飞剑",
            level);
    }

    public static string GetProtectiveRelicName(string archetypeId, int level)
    {
        return ApplyTierLabel(
            archetypeId == "alchemist" ? "镇火寒玉佩" :
            archetypeId == "wanderer" ? "游云避劫戒" :
            "护心温玉",
            level);
    }

    public static string GetPillCauldronName(string archetypeId, int level)
    {
        return ApplyTierLabel(
            archetypeId == "alchemist" ? "朱砂丹火炉" :
            archetypeId == "wanderer" ? "归息药炉" :
            "养脉丹炉",
            level);
    }

    public static string GetTalismanCaseName(string archetypeId, int level)
    {
        return ApplyTierLabel(
            archetypeId == "alchemist" ? "丹纹符匣" :
            archetypeId == "wanderer" ? "引煞符匣" :
            "庚金剑符匣",
            level);
    }

    public static string BuildCompactProgressSummary(MainMenuSaveData saveData)
    {
        saveData.EnsureDefaults();
        return "主法器 +" + saveData.mainArtifactLevel +
               " / 护身 +" + saveData.protectiveRelicLevel +
               " / 丹炉 +" + saveData.pillCauldronLevel +
               " / 符匣 +" + saveData.talismanCaseLevel;
    }

    public static string BuildEquipmentOverview(MainMenuSaveData saveData)
    {
        saveData.EnsureDefaults();
        var builder = new StringBuilder();
        builder.Append("主法器：").Append(GetMainArtifactName(saveData.archetypeId, saveData.mainArtifactLevel)).Append("  +" ).Append(saveData.mainArtifactLevel).Append('\n');
        builder.Append("护身法器：").Append(GetProtectiveRelicName(saveData.archetypeId, saveData.protectiveRelicLevel)).Append("  +").Append(saveData.protectiveRelicLevel).Append('\n');
        builder.Append("丹炉：").Append(GetPillCauldronName(saveData.archetypeId, saveData.pillCauldronLevel)).Append("  +").Append(saveData.pillCauldronLevel).Append('\n');
        builder.Append("符匣：").Append(GetTalismanCaseName(saveData.archetypeId, saveData.talismanCaseLevel)).Append("  +").Append(saveData.talismanCaseLevel);
        return builder.ToString();
    }

    public static string BuildGrowthEffectSummary(MainMenuSaveData saveData)
    {
        saveData.EnsureDefaults();
        return "攻伐加成 +" + saveData.mainArtifactLevel * 2 +
               "    护体加成 +" + (saveData.protectiveRelicLevel * 3) +
               "  HP\n丹药次数 +" + saveData.pillCauldronLevel +
               "    符箓次数 +" + saveData.talismanCaseLevel +
               "    稳心 +" + saveData.talismanCaseLevel * 2;
    }

    private static string ApplyTierLabel(string baseName, int level)
    {
        if (level <= 0)
        {
            return baseName + "·初炼";
        }

        return baseName + "·" + GetTierLabel(level);
    }

    private static string GetTierLabel(int level)
    {
        switch (level)
        {
            case 0:
                return "初炼";
            case 1:
                return "凝光";
            case 2:
                return "化纹";
            case 3:
                return "通灵";
            case 4:
                return "归真";
            default:
                return "圆满";
        }
    }
}
