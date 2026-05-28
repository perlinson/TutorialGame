using UnityEngine;

public static class ExpeditionBuildFactory
{
    private static HeroArchetypeDatabaseAsset cachedDatabase;

    public static ExpeditionHeroState CreateHero(CultivationSaveData saveData, WorldRegionDefinition region)
    {
        var record = GetArchetypeRecord(saveData.archetypeId);
        var loadout = BuildLoadout(saveData.archetypeId);
        loadout.MainArtifact = CultivationLoadoutLibrary.GetMainArtifactName(saveData.archetypeId, saveData.mainArtifactLevel);
        loadout.ProtectiveRelic = CultivationLoadoutLibrary.GetProtectiveRelicName(saveData.archetypeId, saveData.protectiveRelicLevel);
        loadout.PillCauldron = CultivationLoadoutLibrary.GetPillCauldronName(saveData.archetypeId, saveData.pillCauldronLevel);
        loadout.TalismanCase = CultivationLoadoutLibrary.GetTalismanCaseName(saveData.archetypeId, saveData.talismanCaseLevel);
        loadout.MainArtifactLevel = saveData.mainArtifactLevel;
        loadout.ProtectiveRelicLevel = saveData.protectiveRelicLevel;
        loadout.PillCauldronLevel = saveData.pillCauldronLevel;
        loadout.TalismanCaseLevel = saveData.talismanCaseLevel;
        loadout.AttackBonus += saveData.mainArtifactLevel * 2;
        loadout.HealthBonus += saveData.protectiveRelicLevel * 3 + saveData.pillCauldronLevel;
        loadout.StressResistBonus += saveData.talismanCaseLevel * 2;
        loadout.TalismanCharges += saveData.talismanCaseLevel;
        loadout.MedicineCharges += saveData.pillCauldronLevel;
        loadout.MedicinePowerBonus = saveData.pillCauldronLevel * 2;
        loadout.TalismanPowerBonus = 1 + saveData.talismanCaseLevel;

        var hero = new ExpeditionHeroState
        {
            HeroName = saveData.heroName,
            ArchetypeId = saveData.archetypeId,
            ArchetypeName = saveData.archetypeName,
            PortraitImage = record != null && record.portraitImage != null ? record.portraitImage : GeneratedArtLibrary.GetHeroPortrait(saveData.archetypeId),
            MaxHealth = 16 + region.RequiredRealmTier * 2 + loadout.HealthBonus,
            AttackBonus = loadout.AttackBonus,
            DefenseBonus = Mathf.Max(0, saveData.protectiveRelicLevel / 2),
            StressResistBonus = loadout.StressResistBonus,
            TalismanCharges = loadout.TalismanCharges,
            MedicineCharges = loadout.MedicineCharges,
            Loadout = loadout
        };
        hero.CurrentHealth = hero.MaxHealth;
        hero.Stress = Mathf.Clamp(region.DangerRank * 6 - loadout.StressResistBonus, 4, 24);
        BuildSkills(hero);
        return hero;
    }

    public static string DescribeSkills(ExpeditionHeroState hero)
    {
        var summary = string.Empty;
        for (var i = 0; i < hero.Skills.Count; i++)
        {
            if (i > 0)
            {
                summary += "\n";
            }

            summary += (i + 1) + ". " + hero.Skills[i].Name + "：" + hero.Skills[i].Description;
        }

        summary += "\n5. " + hero.Loadout.TalismanName + "：法器携带的应急符箓。";
        summary += "\n6. " + hero.Loadout.MedicineName + "：远征丹药，用于救急。";
        return summary;
    }

    private static ExpeditionEquipmentLoadout BuildLoadout(string archetypeId)
    {
        var record = GetArchetypeRecord(archetypeId);
        if (record != null)
        {
            return new ExpeditionEquipmentLoadout
            {
                MainArtifact = record.mainArtifact,
                ProtectiveRelic = record.protectiveRelic,
                TalismanName = record.talismanName,
                MedicineName = record.medicineName,
                HealthBonus = record.healthBonus,
                AttackBonus = record.attackBonus,
                StressResistBonus = record.stressResistBonus,
                StartingTorchBonus = record.startingTorchBonus,
                StartingSupplyBonus = record.startingSupplyBonus,
                TalismanCharges = record.talismanCharges,
                MedicineCharges = record.medicineCharges
            };
        }

        return BuildFallbackLoadout(archetypeId);
    }

    private static void BuildSkills(ExpeditionHeroState hero)
    {
        hero.Skills.Clear();

        var record = GetArchetypeRecord(hero.ArchetypeId);
        if (record != null && record.skills != null && record.skills.Length > 0)
        {
            for (var i = 0; i < record.skills.Length; i++)
            {
                if (record.skills[i] == null)
                {
                    continue;
                }

                var iconImage = record.skills[i].iconImage != null
                    ? record.skills[i].iconImage
                    : GeneratedArtLibrary.GetSkillIcon(record.skills[i].id);
                hero.Skills.Add(new ExpeditionSkillDefinition(record.skills[i].id, record.skills[i].name, record.skills[i].description, iconImage));
            }

            return;
        }

        BuildFallbackSkills(hero);
    }

    private static HeroArchetypeRecord GetArchetypeRecord(string archetypeId)
    {
        var database = GetDatabase();
        if (database == null || database.archetypes == null)
        {
            return null;
        }

        for (var i = 0; i < database.archetypes.Length; i++)
        {
            var record = database.archetypes[i];
            if (record != null && record.id == archetypeId)
            {
                return record;
            }
        }

        return null;
    }

    private static HeroArchetypeDatabaseAsset GetDatabase()
    {
        if (cachedDatabase == null)
        {
            cachedDatabase = GameData.LoadAsset<HeroArchetypeDatabaseAsset>("Data/HeroArchetypeDatabase");
        }

        return cachedDatabase;
    }

    private static ExpeditionEquipmentLoadout BuildFallbackLoadout(string archetypeId)
    {
        switch (archetypeId)
        {
            case "alchemist":
                return new ExpeditionEquipmentLoadout
                {
                    MainArtifact = "离火丹轮",
                    ProtectiveRelic = "镇火寒玉佩",
                    PillCauldron = "朱砂丹火炉",
                    TalismanCase = "丹纹符匣",
                    TalismanName = "清秽镇煞符",
                    MedicineName = "回春护脉丹",
                    HealthBonus = 2,
                    AttackBonus = 1,
                    StressResistBonus = 4,
                    StartingTorchBonus = 6,
                    StartingSupplyBonus = 1,
                    TalismanCharges = 2,
                    MedicineCharges = 2
                };
            case "wanderer":
                return new ExpeditionEquipmentLoadout
                {
                    MainArtifact = "玄纹符伞",
                    ProtectiveRelic = "游云避劫戒",
                    PillCauldron = "归息药炉",
                    TalismanCase = "引煞符匣",
                    TalismanName = "缚灵摄气符",
                    MedicineName = "凝神散",
                    HealthBonus = 1,
                    AttackBonus = 1,
                    StressResistBonus = 5,
                    StartingTorchBonus = 10,
                    StartingSupplyBonus = 1,
                    TalismanCharges = 3,
                    MedicineCharges = 2
                };
            default:
                return new ExpeditionEquipmentLoadout
                {
                    MainArtifact = "青魄飞剑",
                    ProtectiveRelic = "护心温玉",
                    PillCauldron = "养脉丹炉",
                    TalismanCase = "庚金剑符匣",
                    TalismanName = "护体剑符",
                    MedicineName = "小还丹",
                    HealthBonus = 0,
                    AttackBonus = 2,
                    StressResistBonus = 2,
                    StartingTorchBonus = 4,
                    StartingSupplyBonus = 0,
                    TalismanCharges = 2,
                    MedicineCharges = 2
                };
        }
    }

    private static void BuildFallbackSkills(ExpeditionHeroState hero)
    {
        switch (hero.ArchetypeId)
        {
            case "alchemist":
                hero.Skills.Add(new ExpeditionSkillDefinition("alchemist_fireburst", "离火丹爆", "丹火爆开，压制前两名敌手并灼烧心神。", GeneratedArtLibrary.GetSkillIcon("alchemist_fireburst")));
                hero.Skills.Add(new ExpeditionSkillDefinition("alchemist_restore", "回春丹雾", "以药雾回稳气血，同时安抚心境。", GeneratedArtLibrary.GetSkillIcon("alchemist_restore")));
                hero.Skills.Add(new ExpeditionSkillDefinition("alchemist_poison", "蚀骨毒焰", "给单体叠加毒焰，适合拖死精英。", GeneratedArtLibrary.GetSkillIcon("alchemist_poison")));
                hero.Skills.Add(new ExpeditionSkillDefinition("alchemist_barrier", "炉火护身", "以丹火成罩，降低下轮承伤并提灯。", GeneratedArtLibrary.GetSkillIcon("alchemist_barrier")));
                break;
            case "wanderer":
                hero.Skills.Add(new ExpeditionSkillDefinition("wanderer_bind", "缚灵符", "封锁前列敌人，使其短暂失衡。", GeneratedArtLibrary.GetSkillIcon("wanderer_bind")));
                hero.Skills.Add(new ExpeditionSkillDefinition("wanderer_drain", "摄气诀", "抽取敌方灵息，转为自身回复。", GeneratedArtLibrary.GetSkillIcon("wanderer_drain")));
                hero.Skills.Add(new ExpeditionSkillDefinition("wanderer_mist", "迷踪换影", "藏身符雾，降低承伤并稳住心境。", GeneratedArtLibrary.GetSkillIcon("wanderer_mist")));
                hero.Skills.Add(new ExpeditionSkillDefinition("wanderer_counter", "借势反击", "以退为进，等待敌人露出破绽。", GeneratedArtLibrary.GetSkillIcon("wanderer_counter")));
                break;
            default:
                hero.Skills.Add(new ExpeditionSkillDefinition("sword_strike", "御剑斩", "稳定斩杀前列敌人，是最可靠的正面术。", GeneratedArtLibrary.GetSkillIcon("sword_strike")));
                hero.Skills.Add(new ExpeditionSkillDefinition("sword_cleave", "踏云回锋", "步法回旋，连续扫击前两名敌手。", GeneratedArtLibrary.GetSkillIcon("sword_cleave")));
                hero.Skills.Add(new ExpeditionSkillDefinition("sword_break", "庚金裂罡", "专破护体与甲胄，适合打开精英缺口。", GeneratedArtLibrary.GetSkillIcon("sword_break")));
                hero.Skills.Add(new ExpeditionSkillDefinition("sword_calm", "心剑澄明", "收剑照心，恢复少量气血并压低心境。", GeneratedArtLibrary.GetSkillIcon("sword_calm")));
                break;
        }
    }
}
