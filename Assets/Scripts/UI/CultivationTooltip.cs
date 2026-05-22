using QFramework;
using UnityEngine;
using UnityEngine.UI;
public static class CultivationTooltip
{
    public static void Show(string title, string body, Vector2 screenPosition)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(body))
        {
            Hide();
            return;
        }

        var panel = EnsurePanel();
        if (panel == null)
        {
            return;
        }

        panel.Present(title, body, screenPosition);
    }

    public static void UpdatePosition(Vector2 screenPosition)
    {
        var panel = UIKit.GetPanel<CultivationTooltipPanel>();
        if (panel == null)
        {
            return;
        }

        panel.MoveTo(screenPosition);
    }

    public static void Hide()
    {
        var panel = UIKit.GetPanel<CultivationTooltipPanel>();
        if (panel == null)
        {
            return;
        }

        panel.BeginHide();
    }

    private static CultivationTooltipPanel EnsurePanel()
    {
        var panel = UIKit.GetPanel<CultivationTooltipPanel>();
        if (panel != null)
        {
            GameUi.ShowPanel(GameUiPanelId.Tooltip);
            return panel;
        }

        return GameUi.OpenPanel(GameUiPanelId.Tooltip, new CultivationTooltipPanelData()) as CultivationTooltipPanel;
    }
}

public static class CultivationTooltipBinder
{
    public static void Bind(Button button, string title, string body)
    {
        if (button == null)
        {
            return;
        }

        Bind(button.gameObject, title, body);
    }

    public static void Bind(GameObject target, string title, string body)
    {
        if (target == null)
        {
            return;
        }

        var trigger = target.GetComponent<CultivationTooltipTarget>();
        if (trigger == null)
        {
            trigger = target.AddComponent<CultivationTooltipTarget>();
        }

        trigger.Configure(title, body);
    }
}
