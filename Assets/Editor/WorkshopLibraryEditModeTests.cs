using NUnit.Framework;
using QFramework;

public sealed class WorkshopLibraryEditModeTests
{
    [Test]
    public void BuildRecipeButtonLabel_ShowsCraftableWhenMaterialsAreEnough()
    {
        var saveData = new MainMenuSaveData
        {
            heroName = "测试修士",
            archetypeId = "sword",
            archetypeName = "流云剑修",
            storageItems = new[]
            {
                new SaveItemStack("green_spirit_sand", 2),
                new SaveItemStack("beast_core_shard", 2)
            }
        };
        saveData.EnsureDefaults();

        var label = WorkshopLibrary.BuildRecipeButtonLabel(saveData, "peiyuan_powder");

        Assert.That(label, Does.Contain("可炼"));
    }

    [Test]
    public void Craft_ConsumesMaterialsAndAppliesRecipeReward()
    {
        var saveData = new MainMenuSaveData
        {
            heroName = "测试修士",
            archetypeId = "sword",
            archetypeName = "流云剑修",
            storageItems = new[]
            {
                new SaveItemStack("mist_mushroom", 1),
                new SaveItemStack("spring_jade_dew", 1),
                new SaveItemStack("mind_cleansing_incense", 2)
            }
        };
        saveData.EnsureDefaults();

        var realmSystem = new CultivationRealmSystem();
        var architecture = new TestArchitecture();
        architecture.RegisterSystem(realmSystem);
        architecture.InternalInit();

        var crafted = WorkshopLibrary.Craft(saveData, "pill_cauldron_upgrade", realmSystem, out var summary);

        Assert.That(crafted, Is.True);
        Assert.That(saveData.pillCauldronLevel, Is.EqualTo(1));
        Assert.That(saveData.GetItemCount("mist_mushroom"), Is.EqualTo(0));
        Assert.That(saveData.GetItemCount("spring_jade_dew"), Is.EqualTo(0));
        Assert.That(saveData.GetItemCount("mind_cleansing_incense"), Is.EqualTo(0));
        Assert.That(summary, Does.Contain("丹炉养火"));
    }

    [Test]
    public void Craft_BreaksThroughRealmWhenQiCrossesThreshold()
    {
        var requiredQi = WorldRegionLibrary.GetQiRequiredForNextRealm(0);
        var saveData = new MainMenuSaveData
        {
            heroName = "测试修士",
            archetypeId = "sword",
            archetypeName = "流云剑修",
            realmTier = 0,
            qi = requiredQi - 3,
            storageItems = new[]
            {
                new SaveItemStack("green_spirit_sand", 2),
                new SaveItemStack("beast_core_shard", 2)
            }
        };
        saveData.EnsureDefaults();

        var realmSystem = new CultivationRealmSystem();
        var architecture = new TestArchitecture();
        architecture.RegisterSystem(realmSystem);
        architecture.InternalInit();

        var crafted = WorkshopLibrary.Craft(saveData, "peiyuan_powder", realmSystem, out _);

        Assert.That(crafted, Is.True);
        Assert.That(saveData.realmTier, Is.EqualTo(1));
        Assert.That(saveData.qi, Is.EqualTo(0));
        Assert.That(saveData.realm, Is.EqualTo(WorldRegionLibrary.GetRealmName(1)));
    }

    private class TestArchitecture : Architecture<TestArchitecture>
    {
        public void InternalInit()
        {
            Init();
        }

        protected override void Init()
        {
        }
    }
}
