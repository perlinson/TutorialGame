using NUnit.Framework;

public sealed class GamePresentationBuilderEditModeTests
{
    [Test]
    public void BuildPlayerCompendiumSnapshot_ArtsTab_CreatesVisualNodes()
    {
        var saveData = new CultivationSaveData
        {
            heroName = "沈墨",
            archetypeId = "sword",
            archetypeName = "流云剑修",
            specialty = "御剑攻伐",
            realm = "练气中期",
            realmTier = 1,
            qi = 48,
            mainArtifactLevel = 2,
            protectiveRelicLevel = 1,
            pillCauldronLevel = 2,
            talismanCaseLevel = 1,
            currentRegionId = "green_stone_gate"
        };
        saveData.wallet.Add(SpiritCrystalGrade.Low, 23);
        saveData.EnsureDefaults();

        var snapshot = GamePresentationBuilder.BuildPlayerCompendiumSnapshot(saveData, PlayerCompendiumMainTab.Arts, "main-law");

        Assert.That(snapshot, Is.Not.Null);
        Assert.That(snapshot.VisualNodes, Is.Not.Null);
        Assert.That(snapshot.VisualNodes.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(snapshot.VisualTitle, Is.Not.Empty);
        Assert.That(snapshot.VisualNodes[0].Title, Is.Not.Empty);
    }

    [Test]
    public void BuildPlayerCompendiumSnapshot_CharacterTab_DoesNotCreateVisualNodes()
    {
        var saveData = new CultivationSaveData
        {
            heroName = "沈墨",
            archetypeId = "sword",
            archetypeName = "流云剑修",
            origin = "青石城旧户",
            specialty = "御剑攻伐"
        };
        saveData.EnsureDefaults();

        var snapshot = GamePresentationBuilder.BuildPlayerCompendiumSnapshot(saveData, PlayerCompendiumMainTab.Character, "overview");

        Assert.That(snapshot, Is.Not.Null);
        Assert.That(snapshot.VisualNodes, Is.Null.Or.Empty);
        Assert.That(snapshot.CharacterOverview, Is.Not.Null);
        Assert.That(snapshot.CharacterOverview.Seals, Is.Not.Null);
        Assert.That(snapshot.CharacterOverview.Seals.Length, Is.EqualTo(5));
    }
}
