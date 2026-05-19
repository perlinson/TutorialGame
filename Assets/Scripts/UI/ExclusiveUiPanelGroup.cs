using System.Collections.Generic;
using UnityEngine;

public sealed class ExclusiveUiPanelGroup
{
    private readonly List<GameObject> panels = new List<GameObject>();

    private GameObject activePanel;
    private GameObject blocker;

    public GameObject ActivePanel => activePanel;
    public bool HasActivePanel => activePanel != null;

    public void Configure(GameObject blockerObject, params GameObject[] registeredPanels)
    {
        panels.Clear();
        blocker = blockerObject;

        if (registeredPanels != null)
        {
            for (var i = 0; i < registeredPanels.Length; i++)
            {
                var panel = registeredPanels[i];
                if (panel != null && !panels.Contains(panel))
                {
                    panels.Add(panel);
                }
            }
        }

        if (activePanel != null && !panels.Contains(activePanel))
        {
            activePanel = null;
        }

        ApplyVisibility();
    }

    public void Show(GameObject panel)
    {
        if (panel == null || !panels.Contains(panel))
        {
            HideAll();
            return;
        }

        activePanel = panel;
        ApplyVisibility();
    }

    public void Hide(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        if (activePanel == panel)
        {
            HideAll();
            return;
        }

        if (panels.Contains(panel))
        {
            panel.SetActive(false);
        }
    }

    public void HideAll()
    {
        activePanel = null;
        ApplyVisibility();
    }

    public bool IsActive(GameObject panel)
    {
        return panel != null && activePanel == panel && panel.activeSelf;
    }

    private void ApplyVisibility()
    {
        for (var i = 0; i < panels.Count; i++)
        {
            var panel = panels[i];
            if (panel != null)
            {
                panel.SetActive(panel == activePanel);
            }
        }

        if (blocker != null)
        {
            blocker.SetActive(activePanel != null);
        }
    }
}
