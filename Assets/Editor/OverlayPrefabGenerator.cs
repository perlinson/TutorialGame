#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

public static class OverlayPrefabGenerator
{
    private const string RootFolder = "Assets/Resources/UI/Overlay";
    private const string TooltipPrefabPath = RootFolder + "/CultivationTooltipPanel.prefab";
    private const string MessagePopupPrefabPath = RootFolder + "/CultivationMessagePopupPanel.prefab";
    private const string TooltipPanelArtPath = "Assets/GameArt/UI/Panels/ui_tooltip_small_ink.png";

    public static void RegeneratePrefabs()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/UI");
        EnsureFolder(RootFolder);

        CreateTooltipPrefab(true);
        CreateMessagePopupPrefab(true);
    }

    private static void CreateTooltipPrefab(bool overwrite)
    {
        var root = CreateUiObject("CultivationTooltipPanel", null);
        Stretch(root.GetComponent<RectTransform>());

        var controller = root.AddComponent<CultivationTooltipPanel>();

        var window = CreateImage("Window", root.transform, Color.white);
        var windowImage = window.GetComponent<Image>();
        ApplyOptionalSprite(windowImage, TooltipPanelArtPath, true, true);
        windowImage.raycastTarget = false;
        window.sizeDelta = new Vector2(360f, 148f);

        var canvasGroup = window.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        var title = CreateText("Title", window, new Vector2(24f, -20f), new Vector2(312f, 28f), 24, FontStyle.Bold, TextAnchor.UpperLeft);
        title.color = new Color(0.22f, 0.16f, 0.09f, 1f);
        title.enableWordWrapping = false;

        var body = CreateText("Body", window, new Vector2(24f, -58f), new Vector2(312f, 60f), 20, FontStyle.Normal, TextAnchor.UpperLeft);
        body.color = new Color(0.19f, 0.16f, 0.12f, 1f);
        EnableWrapping(body);

        controller.panelRect = window;
        controller.titleRect = title.rectTransform;
        controller.bodyRect = body.rectTransform;
        controller.canvasGroup = canvasGroup;
        controller.backgroundImage = windowImage;
        controller.titleText = title;
        controller.bodyText = body;

        SaveAsPrefab(root, TooltipPrefabPath, overwrite);
    }

    private static void CreateMessagePopupPrefab(bool overwrite)
    {
        var root = CreateUiObject("CultivationMessagePopupPanel", null);
        Stretch(root.GetComponent<RectTransform>());

        var controller = root.AddComponent<CultivationMessagePopupPanel>();

        var window = CreateImage("Window", root.transform, Color.white);
        var windowImage = window.GetComponent<Image>();
        ApplyOptionalSprite(windowImage, TooltipPanelArtPath, true, true);
        windowImage.raycastTarget = false;
        SetAnchors(window, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        window.anchoredPosition = new Vector2(0f, -112f);
        window.sizeDelta = new Vector2(620f, 164f);

        var accent = CreateImage("Accent", window.transform, new Color(0.71f, 0.56f, 0.23f, 1f));
        SetAnchors(accent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        accent.anchoredPosition = Vector2.zero;
        accent.sizeDelta = new Vector2(12f, 0f);

        var canvasGroup = root.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        var title = CreateText("Title", window, new Vector2(28f, -18f), new Vector2(564f, 30f), 26, FontStyle.Bold, TextAnchor.UpperLeft);
        title.color = new Color(0.23f, 0.15f, 0.08f, 1f);
        title.enableWordWrapping = false;
        title.gameObject.SetActive(false);

        var body = CreateText("Body", window, new Vector2(28f, -58f), new Vector2(564f, 64f), 22, FontStyle.Normal, TextAnchor.UpperLeft);
        body.color = new Color(0.2f, 0.16f, 0.12f, 1f);
        EnableWrapping(body);

        controller.windowRect = window;
        controller.canvasGroup = canvasGroup;
        controller.titleText = title;
        controller.bodyText = body;
        controller.backgroundImage = windowImage;
        controller.accentImage = accent.GetComponent<Image>();

        SaveAsPrefab(root, MessagePopupPrefabPath, overwrite);
    }

    private static RectTransform CreateImage(string name, Transform parent, Color color)
    {
        var gameObject = CreateUiObject(name, parent);
        var image = gameObject.AddComponent<Image>();
        image.color = color;
        return gameObject.GetComponent<RectTransform>();
    }

    private static Text CreateText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, int fontSize, FontStyle fontStyle, TextAnchor alignment)
    {
        var gameObject = CreateUiObject(name, parent);
        var rect = gameObject.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        SetAnchors(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        var text = gameObject.AddComponent<Text>();
        if (TMP_Settings.defaultFontAsset != null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }

        text.fontSize = fontSize;
        text.fontStyle = ConvertFontStyle(fontStyle);
        text.alignment = ConvertAlignment(alignment);
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static void EnableWrapping(Text text)
    {
        if (text == null)
        {
            return;
        }

        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
    }

    private static void ApplyOptionalSprite(Image image, string assetPath, bool useSlicedType = false, bool preserveAspect = false)
    {
        if (image == null || string.IsNullOrWhiteSpace(assetPath))
        {
            return;
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null)
        {
            return;
        }

        image.sprite = sprite;
        image.overrideSprite = sprite;
        image.color = Color.white;
        image.type = useSlicedType ? Image.Type.Sliced : Image.Type.Simple;
        image.preserveAspect = preserveAspect;
    }

    private static FontStyles ConvertFontStyle(FontStyle style)
    {
        switch (style)
        {
            case FontStyle.Bold:
                return FontStyles.Bold;
            case FontStyle.Italic:
                return FontStyles.Italic;
            case FontStyle.BoldAndItalic:
                return FontStyles.Bold | FontStyles.Italic;
            default:
                return FontStyles.Normal;
        }
    }

    private static TextAlignmentOptions ConvertAlignment(TextAnchor alignment)
    {
        switch (alignment)
        {
            case TextAnchor.UpperCenter:
                return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight:
                return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft:
                return TextAlignmentOptions.MidlineLeft;
            case TextAnchor.MiddleCenter:
                return TextAlignmentOptions.Center;
            case TextAnchor.MiddleRight:
                return TextAlignmentOptions.MidlineRight;
            case TextAnchor.LowerLeft:
                return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter:
                return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight:
                return TextAlignmentOptions.BottomRight;
            default:
                return TextAlignmentOptions.TopLeft;
        }
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        var gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.layer = 5;
        if (parent != null)
        {
            gameObject.transform.SetParent(parent, false);
        }

        var rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        return gameObject;
    }

    private static GameObject SaveAsPrefab(GameObject temporaryObject, string path, bool overwrite)
    {
        EnsureFolder(System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/"));

        GameObject prefab;
        if (overwrite || AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            prefab = PrefabUtility.SaveAsPrefabAsset(temporaryObject, path);
        }
        else
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        Object.DestroyImmediate(temporaryObject);
        return prefab;
    }

    private static void EnsureFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        var parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        var folderName = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folderName);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }

    private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
    }
}
#endif
