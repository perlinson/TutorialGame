using NUnit.Framework;

public sealed class GamePresentationBuilderEditModeTests
{
    [Test]
    public void BuildPlayerCompendiumSnapshot_ArtsTab_CreatesVisualNodes()
    {
        var saveData = new MainMenuSaveData
        {
            heroName = "沈墨",
            archetypeId = "sword",
            archetypeName = "流云剑修",
            specialty = "御剑攻伐",
            realm = "练气中期",
            realmTier = 1,
            qi = 48,
            spiritCrystals = 23,
            mainArtifactLevel = 2,
            protectiveRelicLevel = 1,
            pillCauldronLevel = 2,
            talismanCaseLevel = 1,
            currentRegionId = "green_stone_gate"
        };
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
        var saveData = new MainMenuSaveData
        {
            heroName = "沈墨",
            archetypeId = "sword",
            archetypeName = "流云剑修"
        };
        saveData.EnsureDefaults();

        var snapshot = GamePresentationBuilder.BuildPlayerCompendiumSnapshot(saveData, PlayerCompendiumMainTab.Character, "overview");

        Assert.That(snapshot, Is.Not.Null);
        Assert.That(snapshot.VisualNodes, Is.Null.Or.Empty);
    }
}
