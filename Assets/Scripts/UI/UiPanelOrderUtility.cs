using UnityEngine;

using UnityEngine.UI;

public static class UiPanelOrderUtility
{
    public static void BringToFront(Component component, int sortingOrder)
    {
        if (component == null)
        {
            return;
        }

        component.transform.SetAsLastSibling();

        var canvas = component.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = component.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = component.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = component.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (component.GetComponent<GraphicRaycaster>() == null)
            {
                component.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
    }
}
