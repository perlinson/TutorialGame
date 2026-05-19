using System.Collections.Generic;
using UnityEngine;

public static class InventoryLibrary
{
    private static Dictionary<string, InventoryItemDefinition> definitions;

    public static InventoryItemDefinition GetDefinition(string itemId)
    {
        EnsureDefinitions();
        InventoryItemDefinition definition;
        return !string.IsNullOrWhiteSpace(itemId) && definitions.TryGetValue(itemId, out definition) ? definition : null;
    }

    public static string GetDisplayName(string itemId)
    {
        var definition = GetDefinition(itemId);
        return definition != null ? definition.DisplayName : itemId;
    }

    public static Sprite GetArtwork(string itemId)
    {
        var definition = GetDefinition(itemId);
        return definition != null ? definition.ArtworkImage : null;
    }

    public static string BuildBagSummary(MainMenuSaveData saveData, int maxEntries)
    {
        saveData.EnsureDefaults();
        var lines = new List<string>
        {
            "储物袋：" + saveData.GetUsedBagSlots() + " / " + saveData.bagCapacity + " 格"
        };

        var shown = 0;
        for (var i = 0; i < saveData.storageItems.Length && shown < maxEntries; i++)
        {
            var stack = saveData.storageItems[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            lines.Add(GetDisplayName(stack.itemId) + " x" + stack.quantity);
            shown++;
        }

        if (shown == 0)
        {
            lines.Add("袋中暂时空空如也。");
        }

        return string.Join("\n", lines.ToArray());
    }

    public static string BuildDetailedBagSummary(MainMenuSaveData saveData)
    {
        saveData.EnsureDefaults();
        var lines = new List<string>
        {
            "储物袋：" + saveData.GetUsedBagSlots() + " / " + saveData.bagCapacity + " 格",
            string.Empty
        };

        var anyItems = false;
        for (var i = 0; i < saveData.storageItems.Length; i++)
        {
            var stack = saveData.storageItems[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            anyItems = true;
            var definition = GetDefinition(stack.itemId);
            if (definition == null)
            {
                lines.Add(stack.itemId + " x" + stack.quantity);
                continue;
            }

            lines.Add(definition.DisplayName + " x" + stack.quantity + "  [" + definition.Category + " / " + definition.Rarity + "]");
            lines.Add(definition.Description);
            lines.Add(string.Empty);
        }

        if (!anyItems)
        {
            lines.Add("袋中暂时空空如也。");
            lines.Add("继续历练、清剿敌手、搜查药室与残阵后，材料会慢慢充实起来。");
        }

        return string.Join("\n", lines.ToArray()).TrimEnd();
    }

    public static string DescribeLoot(List<SaveItemStack> loot)
    {
        if (loot == null || loot.Count == 0)
        {
            return "无额外物资";
        }

        var lines = new List<string>();
        for (var i = 0; i < loot.Count; i++)
        {
            if (loot[i] == null || loot[i].quantity <= 0)
            {
                continue;
            }

            lines.Add(GetDisplayName(loot[i].itemId) + " x" + loot[i].quantity);
        }

        return lines.Count > 0 ? string.Join("、", lines.ToArray()) : "无额外物资";
    }

    public static int GetCrystalValue(string itemId)
    {
        var definition = GetDefinition(itemId);
        return definition != null ? Mathf.Max(1, definition.CrystalValue) : 1;
    }

    public static string GetRegionalRareItemId(string regionId)
    {
        switch (regionId)
        {
            case "misty_forest":
                return "mist_mushroom";
            case "crimson_valley":
                return "crimson_ore";
            case "deep_springs":
                return "spring_jade_dew";
            case "northern_pass":
                return "north_iron";
            case "celestial_ruins":
                return "starfall_crystal";
            default:
                return "green_spirit_sand";
        }
    }

    private static void EnsureDefinitions()
    {
        if (definitions != null)
        {
            return;
        }

        definitions = new Dictionary<string, InventoryItemDefinition>();
        var database = CultivationApp.LoadResource<InventoryDatabaseAsset>("Data/InventoryDatabase");
        if (database != null && database.items != null)
        {
            for (var i = 0; i < database.items.Length; i++)
            {
                var item = database.items[i];
                if (item == null || string.IsNullOrWhiteSpace(item.id))
                {
                    continue;
                }

                var iconImage = item.iconImage != null ? item.iconImage : GeneratedArtLibrary.GetItemIcon(item.id);
                definitions[item.id] = new InventoryItemDefinition(item.id, item.displayName, item.category, item.rarity, item.description, iconImage, item.crystalValue);
            }
        }

        if (definitions.Count == 0)
        {
            LoadFallbackDefinitions();
        }
    }

    private static void LoadFallbackDefinitions()
    {
        AddFallback("green_spirit_sand", "青石灵砂", "修炼资源", "凡阶", "山门附近最常见的灵砂，可用于稳固练气期根基。", 1);
        AddFallback("bandit_route_token", "匪寨路引", "任务凭证", "凡阶", "从山贼身上搜出的路引，可追查附近流寇据点。", 1);
        AddFallback("mist_mushroom", "雾隐芝", "天材地宝", "灵阶", "生长在瘴雾湿地的灵芝，常用于安神与解瘴。", 2);
        AddFallback("vine_fruit", "青萝果", "修炼资源", "灵阶", "含木水灵机的果实，适合作为丹修辅材。", 2);
        AddFallback("crimson_ore", "赤霞矿髓", "炼器材料", "灵阶", "火脉附近凝出的矿髓，炼火属性法器常要用到。", 2);
        AddFallback("flame_jujube", "火枣", "天材地宝", "灵阶", "温补气血的火属性灵果，兼具修炼与疗伤价值。", 2);
        AddFallback("spring_jade_dew", "玄泉玉露", "天材地宝", "灵阶", "深泉中沉淀出的玉露，常用于稳神和温养经脉。", 2);
        AddFallback("cold_marrow_algae", "寒髓藻", "炼丹辅材", "灵阶", "寒泉边少见的藻类，可平衡火毒丹方。", 2);
        AddFallback("north_iron", "朔风玄铁", "炼器材料", "玄阶", "常见于北地古道的硬铁，用于法器胚料极佳。", 4);
        AddFallback("ancient_pass_order", "古道关令", "任务凭证", "玄阶", "留在北冥古道残关中的令牌，可用来换取更大势力的委托。", 4);
        AddFallback("starfall_crystal", "星陨残晶", "天材地宝", "玄阶", "天外遗迹中才能找到的星辉结晶，价值极高。", 4);
        AddFallback("void_script", "太虚遗简", "传承残卷", "玄阶", "记载残破功法思路的古简，是后续章节的重要材料。", 4);
        AddFallback("beast_core_shard", "妖丹碎片", "妖兽材料", "凡阶", "妖兽体内残留的灵核碎片，可用于炼丹和委托。", 1);
        AddFallback("beast_bone", "灵骨", "妖兽材料", "凡阶", "带微弱灵性的妖骨，是常见的炼器底材。", 1);
        AddFallback("blood_talisman_page", "血符残页", "邪修材料", "灵阶", "残存邪意的符页，研究价值高，但不宜久留。", 2);
        AddFallback("evil_cult_notes", "夺灵手札", "邪修材料", "灵阶", "邪修记下的掠夺心得，可作为任务证据。", 2);
        AddFallback("heart_mark_fragment", "心印残片", "神魂材料", "灵阶", "击溃心魔后留下的神念碎片，可入药也可交付委托。", 2);
        AddFallback("corpse_core", "尸核", "尸傀材料", "灵阶", "尸傀体内的阴煞凝核，许多驱尸邪修会高价收购。", 2);
        AddFallback("yin_bone", "阴骨", "尸傀材料", "凡阶", "阴气浸透的骨片，是尸傀和阴阵常见材料。", 1);
        AddFallback("array_shard", "古阵残片", "遗迹材料", "灵阶", "残阵破碎后留下的阵纹碎片，可用于高阶委托。", 2);
        AddFallback("mind_cleansing_incense", "清心香灰", "修炼资源", "凡阶", "祭台余烬中的香灰，对稳定心境略有帮助。", 1);
    }

    private static void AddFallback(string id, string displayName, string category, string rarity, string description, int crystalValue)
    {
        definitions[id] = new InventoryItemDefinition(id, displayName, category, rarity, description, GeneratedArtLibrary.GetItemIcon(id), crystalValue);
    }
}
