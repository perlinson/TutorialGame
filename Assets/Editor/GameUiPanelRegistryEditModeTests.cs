using NUnit.Framework;
using QFramework;
using UnityEditor;
using UnityEngine;

public sealed class GameUiPanelRegistryEditModeTests
{
    [Test]
    public void GameHub_IsRegisteredAsConstLevel()
    {
        var definition = GameUiPanelRegistry.Get(GameUiPanelId.GameHub);

        Assert.That(definition.Level, Is.EqualTo(UILevel.Const));
        Assert.That(definition.ExclusiveWithinLevel, Is.False);
    }

    [Test]
    public void PlayerCompendium_IsRegisteredAsPopupLevel()
    {
        var definition = GameUiPanelRegistry.Get(GameUiPanelId.PlayerCompendium);

        Assert.That(definition.Level, Is.EqualTo(UILevel.PopUI));
    }

    [Test]
    public void EveryRegisteredPanel_HasMatchingPrefabAssetAndComponent()
    {
        var panelIds = (GameUiPanelId[])System.Enum.GetValues(typeof(GameUiPanelId));
        for (var i = 0; i < panelIds.Length; i++)
        {
            var panelId = panelIds[i];
            var definition = GameUiPanelRegistry.Get(panelId);
            var assetPath = "Assets/Resources/" + definition.ResourcePath + ".prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            Assert.That(prefab, Is.Not.Null, panelId + " prefab missing at " + assetPath);
            Assert.That(prefab.GetComponent(definition.PanelType), Is.Not.Null, panelId + " prefab is missing component " + definition.PanelType.Name);
        }
    }

    [Test]
    public void WorldMapNpcDialogue_IsRegisteredAsPopupLevel()
    {
        var definition = GameUiPanelRegistry.Get(GameUiPanelId.WorldMapNpcDialogue);

        Assert.That(definition.Level, Is.EqualTo(UILevel.PopUI));
        Assert.That(definition.ExclusiveWithinLevel, Is.True);
    }
}
