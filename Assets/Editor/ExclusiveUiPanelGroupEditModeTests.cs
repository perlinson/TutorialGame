using NUnit.Framework;
using UnityEngine;

public sealed class ExclusiveUiPanelGroupEditModeTests
{
    [Test]
    public void Show_ActivatesOnlyRequestedPanel()
    {
        var blocker = CreatePanel("Blocker");
        var panelA = CreatePanel("PanelA");
        var panelB = CreatePanel("PanelB");
        var group = new ExclusiveUiPanelGroup();

        group.Configure(blocker, panelA, panelB);
        group.Show(panelA);

        Assert.That(group.IsActive(panelA), Is.True);
        Assert.That(panelA.activeSelf, Is.True);
        Assert.That(panelB.activeSelf, Is.False);
        Assert.That(blocker.activeSelf, Is.True);

        DestroyPanels(blocker, panelA, panelB);
    }

    [Test]
    public void Show_SwitchesToNewPanelAndClosesOldOne()
    {
        var blocker = CreatePanel("Blocker");
        var panelA = CreatePanel("PanelA");
        var panelB = CreatePanel("PanelB");
        var group = new ExclusiveUiPanelGroup();

        group.Configure(blocker, panelA, panelB);
        group.Show(panelA);
        group.Show(panelB);

        Assert.That(group.IsActive(panelA), Is.False);
        Assert.That(group.IsActive(panelB), Is.True);
        Assert.That(panelA.activeSelf, Is.False);
        Assert.That(panelB.activeSelf, Is.True);

        DestroyPanels(blocker, panelA, panelB);
    }

    [Test]
    public void HideAll_ClosesPanelsAndDisablesBlocker()
    {
        var blocker = CreatePanel("Blocker");
        var panelA = CreatePanel("PanelA");
        var panelB = CreatePanel("PanelB");
        var group = new ExclusiveUiPanelGroup();

        group.Configure(blocker, panelA, panelB);
        group.Show(panelA);
        group.HideAll();

        Assert.That(group.HasActivePanel, Is.False);
        Assert.That(panelA.activeSelf, Is.False);
        Assert.That(panelB.activeSelf, Is.False);
        Assert.That(blocker.activeSelf, Is.False);

        DestroyPanels(blocker, panelA, panelB);
    }

    [Test]
    public void Hide_InactivePanel_DoesNotAffectCurrentPanel()
    {
        var blocker = CreatePanel("Blocker");
        var panelA = CreatePanel("PanelA");
        var panelB = CreatePanel("PanelB");
        var group = new ExclusiveUiPanelGroup();

        group.Configure(blocker, panelA, panelB);
        group.Show(panelA);
        group.Hide(panelB);

        Assert.That(group.IsActive(panelA), Is.True);
        Assert.That(panelA.activeSelf, Is.True);
        Assert.That(panelB.activeSelf, Is.False);
        Assert.That(blocker.activeSelf, Is.True);

        DestroyPanels(blocker, panelA, panelB);
    }

    private static GameObject CreatePanel(string name)
    {
        var panel = new GameObject(name);
        panel.SetActive(true);
        return panel;
    }

    private static void DestroyPanels(params GameObject[] panels)
    {
        for (var i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                Object.DestroyImmediate(panels[i]);
            }
        }
    }
}
