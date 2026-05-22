using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class GamePresentationBuilder
{
    private static HeroArchetypeDatabaseAsset cachedHeroArchetypeDatabase;

    public static GameHubSnapshot BuildGameHubSnapshot(MainMenuSaveData saveData, GameHubContext context, string worldTimeText)
    {
        if (saveData == null)
        {
            return new GameHubSnapshot
            {
                WorldTimeText = string.IsNullOrWhiteSpace(worldTimeText) ? "太初历 · 未定时" : worldTimeText,
                HeroName = "无名修士",
                RealmText = "未建立存档",
                LocationText = "当前位置：未知",
                HealthText = "气血 0 / 0",
                SpiritText = "灵力 0 / 0",
                ResourceText = "灵石 0 · 主法器 +0 · 护身 +0",
                CurrentHealth = 0,
                MaxHealth = 0,
                CurrentSpirit = 0,
                MaxSpirit = 0,
                Portrait = null,
                ShowSectButton = false,
                MapSelected = context == GameHubContext.WorldMap,
                SettlementSelected = context == GameHubContext.Settlement,
                SectSelected = context == GameHubContext.SectResidence
            };
        }

        saveData.EnsureDefaults();
        var currentHealth = ComputeMaxHealth(saveData);
        var maxHealth = currentHealth;
        var nextQi = WorldRegionLibrary.GetQiRequiredForNextRealm(saveData.realmTier);
        var baseSpiritCapacity = 18 + saveData.realmTier * 8 + saveData.talismanCaseLevel * 3 + saveData.pillCauldronLevel * 2;
        var maxSpirit = Mathf.Max(1, nextQi > 0 ? nextQi : Mathf.Max(baseSpiritCapacity, saveData.qi));
        var currentSpirit = Mathf.Clamp(saveData.qi, 0, maxSpirit);

        return new GameHubSnapshot
        {
            WorldTimeText = string.IsNullOrWhiteSpace(worldTimeText) ? CultivationGameTime.Format(saveData) : worldTimeText,
            HeroName = string.IsNullOrWhiteSpace(saveData.heroName) ? "无名修士" : saveData.heroName,
            RealmText = saveData.realm + " · " + saveData.archetypeName,
            LocationText = "当前位置：" + saveData.location,
            HealthText = "气血 " + currentHealth + " / " + maxHealth,
            SpiritText = "灵力 " + currentSpirit + " / " + maxSpirit,
            ResourceText = "灵石 " + saveData.spiritCrystals +
                           " · 主法器 +" + saveData.mainArtifactLevel +
                           " · 护身 +" + saveData.protectiveRelicLevel,
            CurrentHealth = currentHealth,
            MaxHealth = maxHealth,
            CurrentSpirit = currentSpirit,
            MaxSpirit = maxSpirit,
            Portrait = ResolvePortrait(saveData),
            ShowSectButton = saveData.isSectDisciple,
            MapSelected = context == GameHubContext.WorldMap,
            SettlementSelected = context == GameHubContext.Settlement,
            SectSelected = context == GameHubContext.SectResidence
        };
    }

    public static PlayerCompendiumSnapshot BuildPlayerCompendiumSnapshot(MainMenuSaveData saveData, PlayerCompendiumMainTab mainTab, string sectionId)
    {
        if (saveData == null)
        {
            return new PlayerCompendiumSnapshot
            {
                PanelTitle = "修士总览",
                PanelSubtitle = "当前没有可用存档",
                SummaryText = "请先返回主菜单建立角色存档。",
                ContentTitle = "暂无数据",
                ContentBody = "当前没有可用于展示的人物、物品、天赋与修仙技艺数据。",
                ResolvedSectionId = string.Empty,
                Preview = new WorldMapPreviewSnapshot
                {
                    Label = "总览占位图",
                    PlaceholderColor = new Color(0.18f, 0.16f, 0.13f, 1f)
                },
                Sections = new PlayerCompendiumSectionSnapshot[0]
            };
        }

        saveData.EnsureDefaults();
        switch (mainTab)
        {
            case PlayerCompendiumMainTab.Items:
                return BuildItemsCompendium(saveData, sectionId);
            case PlayerCompendiumMainTab.Talents:
                return BuildTalentsCompendium(saveData, sectionId);
            case PlayerCompendiumMainTab.Arts:
                return BuildArtsCompendium(saveData, sectionId);
            default:
                return BuildCharacterCompendium(saveData, sectionId);
        }
    }

    public static Sprite ResolvePortrait(MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            return null;
        }

        var record = GetHeroArchetypeRecord(saveData.archetypeId);
        if (record != null && record.portraitImage != null)
        {
            return record.portraitImage;
        }

        return GeneratedArtLibrary.GetHeroPortrait(saveData.archetypeId);
    }

    public static int ComputeMaxHealth(MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            return 0;
        }

        var record = GetHeroArchetypeRecord(saveData.archetypeId);
        var baseHealthBonus = record != null ? record.healthBonus : 0;
        return Mathf.Max(
            1,
            16 +
            saveData.realmTier * 2 +
            baseHealthBonus +
            saveData.protectiveRelicLevel * 3 +
            saveData.pillCauldronLevel);
    }

    private static PlayerCompendiumSnapshot BuildCharacterCompendium(MainMenuSaveData saveData, string sectionId)
    {
        var sections = new[]
        {
            CreateCompendiumSection("overview", "人物概览"),
            CreateCompendiumSection("growth", "成长状态"),
            CreateCompendiumSection("loadout", "装备法器")
        };

        var resolved = ResolveSectionId(sectionId, sections, "overview");
        var contentTitle = "人物概览";
        var contentBody = string.Empty;

        switch (resolved)
        {
            case "growth":
                contentTitle = "成长状态";
                contentBody = BuildCharacterGrowthBody(saveData);
                break;
            case "loadout":
                contentTitle = "装备法器";
                contentBody = BuildCharacterLoadoutBody(saveData);
                break;
            default:
                contentBody = BuildCharacterOverviewBody(saveData);
                break;
        }

        return new PlayerCompendiumSnapshot
        {
            PanelTitle = saveData.heroName + " · 修士总览",
            PanelSubtitle = saveData.realm + " / " + saveData.archetypeName,
            SummaryText = "当前位置：" + saveData.location + "\n灵石：" + saveData.spiritCrystals + "    修为：" + saveData.qi +
                          "    储物袋：" + saveData.GetUsedBagSlots() + " / " + saveData.bagCapacity,
            ContentTitle = contentTitle,
            ContentBody = contentBody,
            ResolvedSectionId = resolved,
            Preview = new WorldMapPreviewSnapshot
            {
                Sprite = ResolvePortrait(saveData),
                Label = saveData.archetypeName + "立绘",
                PlaceholderColor = new Color(0.24f, 0.19f, 0.14f, 1f)
            },
            Sections = sections
        };
    }

    private static PlayerCompendiumSnapshot BuildItemsCompendium(MainMenuSaveData saveData, string sectionId)
    {
        var sections = new[]
        {
            CreateCompendiumSection("all", "全部收纳"),
            CreateCompendiumSection("resources", "修炼资源"),
            CreateCompendiumSection("materials", "材料杂项"),
            CreateCompendiumSection("tokens", "凭证残卷")
        };

        var resolved = ResolveSectionId(sectionId, sections, "all");
        var contentTitle = "全部收纳";
        switch (resolved)
        {
            case "resources":
                contentTitle = "修炼资源";
                break;
            case "materials":
                contentTitle = "材料杂项";
                break;
            case "tokens":
                contentTitle = "凭证残卷";
                break;
        }

        return new PlayerCompendiumSnapshot
        {
            PanelTitle = "物品总览",
            PanelSubtitle = "储物袋 / 材料 / 凭证 / 战利品",
            SummaryText = "当前已使用 " + saveData.GetUsedBagSlots() + " / " + saveData.bagCapacity + " 格。\n" +
                          "不同子页按用途归类，便于后续做出售、炼制与任务交付。",
            ContentTitle = contentTitle,
            ContentBody = BuildItemsBody(saveData, resolved),
            ResolvedSectionId = resolved,
            Preview = BuildCompendiumInventoryPreview(saveData),
            Sections = sections
        };
    }

    private static PlayerCompendiumSnapshot BuildTalentsCompendium(MainMenuSaveData saveData, string sectionId)
    {
        var record = GetHeroArchetypeRecord(saveData.archetypeId);
        var sections = new[]
        {
            CreateCompendiumSection("trait", "天赋核心"),
            CreateCompendiumSection("background", "出身来历"),
            CreateCompendiumSection("identity", "身份路径")
        };

        var resolved = ResolveSectionId(sectionId, sections, "trait");
        var contentTitle = "天赋核心";
        var contentBody = string.Empty;

        switch (resolved)
        {
            case "background":
                contentTitle = "出身来历";
                contentBody = BuildTalentBackgroundBody(saveData, record);
                break;
            case "identity":
                contentTitle = "身份路径";
                contentBody = BuildTalentIdentityBody(saveData);
                break;
            default:
                contentBody = BuildTalentTraitBody(saveData, record);
                break;
        }

        return new PlayerCompendiumSnapshot
        {
            PanelTitle = "天赋总览",
            PanelSubtitle = "出身特性 / 养成倾向 / 身份走向",
            SummaryText = "当前角色的玩法重心由出身、法器偏向和门派身份共同决定。\n" +
                          "这一页先汇总描述层信息，后续再接真正的被动树与境界天赋树。",
            ContentTitle = contentTitle,
            ContentBody = contentBody,
            ResolvedSectionId = resolved,
            Preview = new WorldMapPreviewSnapshot
            {
                Sprite = record != null ? record.bannerImage : ResolvePortrait(saveData),
                Label = "天赋卷轴",
                PlaceholderColor = new Color(0.2f, 0.18f, 0.24f, 1f)
            },
            Sections = sections
        };
    }

    private static PlayerCompendiumSnapshot BuildArtsCompendium(MainMenuSaveData saveData, string sectionId)
    {
        var sections = new[]
        {
            CreateCompendiumSection("main-law", "主修功法"),
            CreateCompendiumSection("combat", "战斗术法"),
            CreateCompendiumSection("alchemy", "丹道"),
            CreateCompendiumSection("talisman", "符道")
        };

        var resolved = ResolveSectionId(sectionId, sections, "main-law");
        var contentTitle = "主修功法";
        var contentBody = string.Empty;

        switch (resolved)
        {
            case "combat":
                contentTitle = "战斗术法";
                contentBody = BuildArtsCombatTreeBody(saveData);
                break;
            case "alchemy":
                contentTitle = "丹道";
                contentBody = BuildArtsAlchemyBody(saveData);
                break;
            case "talisman":
                contentTitle = "符道";
                contentBody = BuildArtsTalismanBody(saveData);
                break;
            default:
                contentBody = BuildArtsMainLawBody(saveData);
                break;
        }

        return new PlayerCompendiumSnapshot
        {
            PanelTitle = "修仙技艺总览",
            PanelSubtitle = "功法路径 / 战斗术法 / 丹道 / 符道",
            SummaryText = "这一页已经不再只是说明文案，而是四类成长入口的总枢纽。\n" +
                          "先用占位树和阶段节点承载数据，后续再继续接成真正的面板化成长树。",
            ContentTitle = contentTitle,
            ContentBody = contentBody,
            ResolvedSectionId = resolved,
            VisualTitle = BuildArtsVisualTitle(resolved),
            Preview = new WorldMapPreviewSnapshot
            {
                Sprite = ResolvePortrait(saveData),
                Label = "术法卷轴",
                PlaceholderColor = new Color(0.16f, 0.19f, 0.25f, 1f)
            },
            Sections = sections,
            VisualNodes = BuildArtsVisualNodes(saveData, resolved)
        };
    }

    private static string BuildCharacterOverviewBody(MainMenuSaveData saveData)
    {
        var builder = new StringBuilder();
        builder.Append("姓名：").Append(saveData.heroName).Append('\n');
        builder.Append("身份：").Append(saveData.archetypeName).Append(" / ").Append(saveData.realm).Append('\n');
        builder.Append("出身：").Append(saveData.origin).Append('\n');
        builder.Append("专长：").Append(saveData.specialty).Append('\n');
        builder.Append("当前所在地：").Append(saveData.location).Append('\n');
        builder.Append("所属势力：").Append(saveData.isSectDisciple ? saveData.sectName : "散修").Append('\n');
        builder.Append("当前时序：").Append(CultivationGameTime.Format(saveData)).Append('\n');
        builder.Append('\n').Append(string.IsNullOrWhiteSpace(saveData.description) ? "暂无额外人物描述。" : saveData.description);
        return builder.ToString();
    }

    private static string BuildCharacterGrowthBody(MainMenuSaveData saveData)
    {
        var nextQi = WorldRegionLibrary.GetQiRequiredForNextRealm(saveData.realmTier);
        var hp = ComputeMaxHealth(saveData);
        var spiritCap = Mathf.Max(1, nextQi > 0 ? nextQi : saveData.qi);
        var builder = new StringBuilder();
        builder.Append("境界：").Append(saveData.realm).Append("（第 ").Append(saveData.realmTier + 1).Append(" 阶）\n");
        builder.Append("气血：").Append(hp).Append(" / ").Append(hp).Append('\n');
        builder.Append("灵力：").Append(saveData.qi).Append(" / ").Append(spiritCap).Append('\n');
        builder.Append("灵石：").Append(saveData.spiritCrystals).Append('\n');
        builder.Append("洞府建设：").Append(saveData.settlementBuildCount).Append(" 次\n");
        builder.Append("最近整备：").Append(string.IsNullOrWhiteSpace(saveData.lastSettlementAction) ? "暂无" : saveData.lastSettlementAction).Append('\n');
        builder.Append("世界时间：").Append(CultivationGameTime.Format(saveData)).Append('\n');
        builder.Append('\n').Append(CultivationLoadoutLibrary.BuildCompactProgressSummary(saveData)).Append('\n');
        builder.Append(CultivationLoadoutLibrary.BuildGrowthEffectSummary(saveData));
        return builder.ToString();
    }

    private static string BuildCharacterLoadoutBody(MainMenuSaveData saveData)
    {
        return CultivationLoadoutLibrary.BuildEquipmentOverview(saveData) +
               "\n\n" +
               CultivationLoadoutLibrary.BuildGrowthEffectSummary(saveData) +
               "\n\n储物袋上限：" + saveData.bagCapacity + " 格";
    }

    private static string BuildItemsBody(MainMenuSaveData saveData, string sectionId)
    {
        var lines = new List<string>();
        lines.Add("储物袋：" + saveData.GetUsedBagSlots() + " / " + saveData.bagCapacity + " 格");
        lines.Add(string.Empty);

        var any = false;
        for (var i = 0; i < saveData.storageItems.Length; i++)
        {
            var stack = saveData.storageItems[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            var definition = InventoryLibrary.GetDefinition(stack.itemId);
            if (!MatchesCompendiumItemSection(sectionId, definition))
            {
                continue;
            }

            any = true;
            if (definition == null)
            {
                lines.Add(stack.itemId + " x" + stack.quantity);
                lines.Add(string.Empty);
                continue;
            }

            lines.Add(definition.DisplayName + " x" + stack.quantity + "  [" + definition.Category + " / " + definition.Rarity + "]");
            lines.Add(definition.Description);
            lines.Add("估值：约 " + definition.CrystalValue + " 灵石");
            lines.Add(string.Empty);
        }

        if (!any)
        {
            lines.Add("当前分类下暂无物品。");
            lines.Add("继续历练、完成委托或在坊市炼制后，这里会逐步丰富。");
        }

        return string.Join("\n", lines.ToArray()).TrimEnd();
    }

    private static string BuildTalentTraitBody(MainMenuSaveData saveData, HeroArchetypeRecord record)
    {
        var builder = new StringBuilder();
        builder.Append("核心天赋：").Append(record != null ? record.trait : saveData.specialty).Append('\n');
        builder.Append("推荐方向：").Append(record != null ? record.recommendation : saveData.description).Append('\n');
        builder.Append('\n');
        builder.Append("这一页当前先承载描述型天赋。后续可继续扩展为：\n");
        builder.Append("1. 先天气运\n2. 境界节点天赋\n3. 宗门路线分支\n4. 功法专精");
        return builder.ToString();
    }

    private static string BuildTalentBackgroundBody(MainMenuSaveData saveData, HeroArchetypeRecord record)
    {
        var builder = new StringBuilder();
        builder.Append("出身：").Append(saveData.origin).Append('\n');
        builder.Append("专长：").Append(saveData.specialty).Append('\n');
        builder.Append("定位：").Append(saveData.archetypeName).Append('\n');
        builder.Append('\n');
        if (record != null)
        {
            builder.Append(string.IsNullOrWhiteSpace(record.description) ? "暂无额外背景描述。" : record.description).Append('\n');
            builder.Append('\n').Append("推荐：").Append(record.recommendation);
        }
        else
        {
            builder.Append(string.IsNullOrWhiteSpace(saveData.description) ? "暂无额外背景描述。" : saveData.description);
        }

        return builder.ToString();
    }

    private static string BuildTalentIdentityBody(MainMenuSaveData saveData)
    {
        var builder = new StringBuilder();
        builder.Append("当前身份：").Append(saveData.isSectDisciple ? saveData.sectName + "门人" : "散修").Append('\n');
        builder.Append("宗门驻地状态：").Append(saveData.isInSectResidence ? "已在驻地内" : "游历中").Append('\n');
        builder.Append("当前地域：").Append(WorldRegionLibrary.GetRegionDisplayName(saveData.currentRegionId)).Append('\n');
        builder.Append('\n');
        builder.Append(saveData.isSectDisciple
            ? "你可以同时走宗门殿堂线、洞府整备线和山外历练线，属于偏稳定成长路线。"
            : "你当前属于散修路线，成长更自由，但缺少宗门驻地的固定支持。");
        return builder.ToString();
    }

    private static string BuildArtsMainLawBody(MainMenuSaveData saveData)
    {
        var nextQi = WorldRegionLibrary.GetQiRequiredForNextRealm(saveData.realmTier);
        var currentStage = saveData.realmTier + 1;
        var builder = new StringBuilder();
        builder.Append("主修路线：").Append(saveData.archetypeName).Append("本命功法\n");
        builder.Append("当前境界：").Append(saveData.realm).Append("（第 ").Append(currentStage).Append(" 层）\n");
        builder.Append("灵力进度：").Append(saveData.qi).Append(nextQi > 0 ? " / " + nextQi : " / 圆满").Append('\n');
        builder.Append("功体倾向：").Append(saveData.specialty).Append('\n');
        builder.Append('\n');
        builder.Append("已激活节点：\n");
        builder.Append("1. 周天运转：基础吐纳效率稳定，可支撑当前历练循环。\n");
        builder.Append("2. 本命法器共鸣：主法器 +").Append(saveData.mainArtifactLevel).Append("，提升攻伐倍率与招式契合。\n");
        builder.Append("3. 护体经络温养：护身法器 +").Append(saveData.protectiveRelicLevel).Append("，增强容错与续航。\n");
        builder.Append('\n');
        builder.Append("下一阶段入口：\n");
        builder.Append(nextQi > 0
            ? "当修为达到下一境界阈值后，可解锁新的功法节点与额外术法槽位。"
            : "当前已到本阶段圆满，适合接突破、功法分支和神通解锁。");
        return builder.ToString();
    }

    private static string BuildArtsVisualTitle(string sectionId)
    {
        switch (sectionId)
        {
            case "combat":
                return "战斗术法节点";
            case "alchemy":
                return "丹道传承节点";
            case "talisman":
                return "符道传承节点";
            default:
                return "主修功法周天图";
        }
    }

    private static PlayerCompendiumVisualNodeSnapshot[] BuildArtsVisualNodes(MainMenuSaveData saveData, string sectionId)
    {
        switch (sectionId)
        {
            case "combat":
                return BuildArtsCombatNodes(saveData);
            case "alchemy":
                return BuildArtsAlchemyNodes(saveData);
            case "talisman":
                return BuildArtsTalismanNodes(saveData);
            default:
                return BuildArtsMainLawNodes(saveData);
        }
    }

    private static string BuildArtsCombatTreeBody(MainMenuSaveData saveData)
    {
        var region = ResolveCompendiumRegion(saveData);
        var hero = ExpeditionBuildFactory.CreateHero(saveData, region);
        var builder = new StringBuilder();
        builder.Append("当前战斗术法栏位：").Append(Mathf.Max(3, hero.Skills.Count)).Append(" 格\n");
        builder.Append("推荐分层：起手 / 核心输出 / 恢复 / 控制 / 终结\n\n");
        for (var i = 0; i < hero.Skills.Count; i++)
        {
            builder.Append(i + 1).Append(". 【已装配】").Append(hero.Skills[i].Name).Append('\n');
            builder.Append(hero.Skills[i].Description).Append("\n\n");
        }

        if (hero.Skills.Count == 0)
        {
            builder.Append("当前没有可展示的战斗术法。");
        }
        else
        {
            builder.Append("后续可继续把这一页接成真正的术法树：左侧流派页签，右侧节点图，底部装配栏。");
        }

        return builder.ToString().TrimEnd();
    }

    private static PlayerCompendiumVisualNodeSnapshot[] BuildArtsMainLawNodes(MainMenuSaveData saveData)
    {
        var nextQi = WorldRegionLibrary.GetQiRequiredForNextRealm(saveData.realmTier);
        return new[]
        {
            CreateCompendiumNode("cycle", "周天运转", saveData.realm, "基础吐纳已稳定成环，维持日常修炼与历练恢复。", "已稳固", true, true),
            CreateCompendiumNode("artifact", "法器共鸣", "主法器 +" + saveData.mainArtifactLevel, "本命法器与功法共振，强化攻击判定与招式契合。", "已激活", true, false),
            CreateCompendiumNode("guard", "护体经络", "护身器 +" + saveData.protectiveRelicLevel, "温养护体脉络，提供更高的生存与抗压余地。", "已激活", true, false),
            CreateCompendiumNode("breakthrough", "破境关隘", nextQi > 0 ? "修为 " + saveData.qi + " / " + nextQi : "当前阶段圆满", nextQi > 0 ? "继续积累修为后可突破下一层，解锁新的主修分支。" : "可进入下一轮突破、分支选择与神通解锁。", nextQi > 0 ? "待突破" : "可突破", nextQi <= 0, false)
        };
    }

    private static PlayerCompendiumVisualNodeSnapshot[] BuildArtsCombatNodes(MainMenuSaveData saveData)
    {
        var region = ResolveCompendiumRegion(saveData);
        var hero = ExpeditionBuildFactory.CreateHero(saveData, region);
        var nodes = new PlayerCompendiumVisualNodeSnapshot[Mathf.Max(4, hero.Skills.Count)];
        for (var i = 0; i < nodes.Length; i++)
        {
            if (i < hero.Skills.Count)
            {
                var skill = hero.Skills[i];
                nodes[i] = CreateCompendiumNode(
                    "combat-" + i,
                    skill.Name,
                    "已装配术法",
                    skill.Description,
                    i == 0 ? "起手位" : i == nodes.Length - 1 ? "终结位" : "可出战",
                    true,
                    i == 0);
            }
            else
            {
                nodes[i] = CreateCompendiumNode(
                    "combat-empty-" + i,
                    "预留槽位",
                    "后续术法位",
                    "突破境界或接入正式功法树后，可在这里装配新术法。",
                    "未解锁",
                    false,
                    false);
            }
        }

        return nodes;
    }

    private static PlayerCompendiumVisualNodeSnapshot[] BuildArtsAlchemyNodes(MainMenuSaveData saveData)
    {
        return new[]
        {
            CreateCompendiumNode("alchemy-fire", "引火稳炉", "丹炉 +" + saveData.pillCauldronLevel, "维持火候与炉压稳定，决定基础炼丹成功率。", "已解锁", true, true),
            CreateCompendiumNode("alchemy-mix", "药性归一", "丹炉 +2", "提高灵药融合效率，减少废丹并解锁更高品阶配方。", saveData.pillCauldronLevel >= 2 ? "已解锁" : "待解锁", saveData.pillCauldronLevel >= 2, false),
            CreateCompendiumNode("alchemy-fragrance", "成丹留香", "丹炉 +4", "成丹后保留余香药性，进一步抬高续航与回复上限。", saveData.pillCauldronLevel >= 4 ? "已解锁" : "待解锁", saveData.pillCauldronLevel >= 4, false)
        };
    }

    private static PlayerCompendiumVisualNodeSnapshot[] BuildArtsTalismanNodes(MainMenuSaveData saveData)
    {
        return new[]
        {
            CreateCompendiumNode("talisman-ink", "朱砂起笔", "符匣 +" + saveData.talismanCaseLevel, "建立最基础的符胆与灵纹起笔法。", "已解锁", true, true),
            CreateCompendiumNode("talisman-link", "灵纹并联", "符匣 +2", "让多道灵纹协同生效，强化实战挂载与辅助效果。", saveData.talismanCaseLevel >= 2 ? "已解锁" : "待解锁", saveData.talismanCaseLevel >= 2, false),
            CreateCompendiumNode("talisman-edge", "符胆藏锋", "符匣 +4", "在符内预埋攻伐锋意，提升瞬时爆发与终结能力。", saveData.talismanCaseLevel >= 4 ? "已解锁" : "待解锁", saveData.talismanCaseLevel >= 4, false)
        };
    }

    private static string BuildArtsAlchemyBody(MainMenuSaveData saveData)
    {
        var builder = new StringBuilder();
        builder.Append("丹炉：").Append(CultivationLoadoutLibrary.GetPillCauldronName(saveData.archetypeId, saveData.pillCauldronLevel))
            .Append("  +").Append(saveData.pillCauldronLevel).Append('\n');
        builder.Append("炼丹熟练：").Append(saveData.pillCauldronLevel * 20).Append(" / 100\n");
        builder.Append("当前收益：丹药次数 +").Append(saveData.pillCauldronLevel).Append("，疗伤效果提升。\n");
        builder.Append('\n');
        builder.Append("节点预览：\n");
        builder.Append("1. 引火稳炉：已解锁\n");
        builder.Append("2. 药性归一：").Append(saveData.pillCauldronLevel >= 2 ? "已解锁" : "待解锁（丹炉 +2）").Append('\n');
        builder.Append("3. 成丹留香：").Append(saveData.pillCauldronLevel >= 4 ? "已解锁" : "待解锁（丹炉 +4）").Append('\n');
        builder.Append('\n');
        builder.Append("后续这里可以直接接炼丹配方树、药材图鉴和成丹品质分支。");
        return builder.ToString();
    }

    private static string BuildArtsTalismanBody(MainMenuSaveData saveData)
    {
        var builder = new StringBuilder();
        builder.Append("符匣：").Append(CultivationLoadoutLibrary.GetTalismanCaseName(saveData.archetypeId, saveData.talismanCaseLevel))
            .Append("  +").Append(saveData.talismanCaseLevel).Append('\n');
        builder.Append("制符熟练：").Append(saveData.talismanCaseLevel * 20).Append(" / 100\n");
        builder.Append("当前收益：符箓次数 +").Append(saveData.talismanCaseLevel)
            .Append("，稳心 +").Append(saveData.talismanCaseLevel * 2).Append('\n');
        builder.Append('\n');
        builder.Append("节点预览：\n");
        builder.Append("1. 朱砂起笔：已解锁\n");
        builder.Append("2. 灵纹并联：").Append(saveData.talismanCaseLevel >= 2 ? "已解锁" : "待解锁（符匣 +2）").Append('\n');
        builder.Append("3. 符胆藏锋：").Append(saveData.talismanCaseLevel >= 4 ? "已解锁" : "待解锁（符匣 +4）").Append('\n');
        builder.Append('\n');
        builder.Append("后续这里可以继续接符箓配方、战斗挂载位和一次性秘符系统。");
        return builder.ToString();
    }

    private static WorldMapPreviewSnapshot BuildCompendiumInventoryPreview(MainMenuSaveData saveData)
    {
        for (var i = 0; i < saveData.storageItems.Length; i++)
        {
            var stack = saveData.storageItems[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            var definition = InventoryLibrary.GetDefinition(stack.itemId);
            if (definition == null)
            {
                continue;
            }

            return new WorldMapPreviewSnapshot
            {
                Sprite = definition.ArtworkImage,
                Label = definition.DisplayName,
                PlaceholderColor = new Color(0.21f, 0.19f, 0.15f, 1f)
            };
        }

        return new WorldMapPreviewSnapshot
        {
            Label = "储物袋总览",
            PlaceholderColor = new Color(0.21f, 0.19f, 0.15f, 1f)
        };
    }

    private static WorldRegionDefinition ResolveCompendiumRegion(MainMenuSaveData saveData)
    {
        WorldRegionDefinition region;
        if (saveData != null && WorldRegionLibrary.TryGetRegion(saveData.currentRegionId, out region))
        {
            return region;
        }

        return WorldRegionLibrary.GetStartingRegion();
    }

    private static bool MatchesCompendiumItemSection(string sectionId, InventoryItemDefinition definition)
    {
        if (definition == null || string.IsNullOrWhiteSpace(sectionId) || sectionId == "all")
        {
            return true;
        }

        var category = definition.Category ?? string.Empty;
        switch (sectionId)
        {
            case "resources":
                return category.Contains("修炼") || category.Contains("天材") || category.Contains("地宝");
            case "materials":
                return category.Contains("材料") || category.Contains("炼器") || category.Contains("炼丹") ||
                       category.Contains("辅材") || category.Contains("妖兽") || category.Contains("尸傀") || category.Contains("遗迹");
            case "tokens":
                return category.Contains("凭证") || category.Contains("残卷") || category.Contains("传承");
            default:
                return true;
        }
    }

    private static PlayerCompendiumSectionSnapshot CreateCompendiumSection(string id, string label)
    {
        return new PlayerCompendiumSectionSnapshot
        {
            Id = id,
            Label = label
        };
    }

    private static PlayerCompendiumVisualNodeSnapshot CreateCompendiumNode(
        string id,
        string title,
        string subtitle,
        string description,
        string stateText,
        bool isUnlocked,
        bool isFocused)
    {
        return new PlayerCompendiumVisualNodeSnapshot
        {
            Id = id,
            Title = title,
            Subtitle = subtitle,
            Description = description,
            StateText = stateText,
            IsUnlocked = isUnlocked,
            IsFocused = isFocused
        };
    }

    private static string ResolveSectionId(string requestedSectionId, PlayerCompendiumSectionSnapshot[] sections, string fallbackId)
    {
        if (sections == null || sections.Length == 0)
        {
            return string.Empty;
        }

        for (var i = 0; i < sections.Length; i++)
        {
            if (sections[i] != null && sections[i].Id == requestedSectionId)
            {
                return requestedSectionId;
            }
        }

        return string.IsNullOrWhiteSpace(fallbackId) ? sections[0].Id : fallbackId;
    }

    private static HeroArchetypeRecord GetHeroArchetypeRecord(string archetypeId)
    {
        if (string.IsNullOrWhiteSpace(archetypeId))
        {
            return null;
        }

        if (cachedHeroArchetypeDatabase == null)
        {
            cachedHeroArchetypeDatabase = GameResource.Load<HeroArchetypeDatabaseAsset>("Data/HeroArchetypeDatabase");
        }

        if (cachedHeroArchetypeDatabase == null || cachedHeroArchetypeDatabase.archetypes == null)
        {
            return null;
        }

        for (var i = 0; i < cachedHeroArchetypeDatabase.archetypes.Length; i++)
        {
            var record = cachedHeroArchetypeDatabase.archetypes[i];
            if (record != null && record.id == archetypeId)
            {
                return record;
            }
        }

        return null;
    }
}
