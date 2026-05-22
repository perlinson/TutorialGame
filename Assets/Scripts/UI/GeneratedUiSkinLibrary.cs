using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public enum GeneratedUiButtonSkin
{
    Primary,
    Secondary,
    Danger
}

public static class GeneratedUiSkinLibrary
{
    private const string PrimaryButtonPath = "UI/Buttons/ui_btn_primary_gold";
    private const string SecondaryButtonPath = "UI/Buttons/ui_btn_secondary_jade";
    private const string DangerButtonPath = "UI/Buttons/ui_btn_danger_red";
    private const string RegionNodePath = "UI/Buttons/ui_node_region_ink";
    private const string RoomNodePath = "UI/Buttons/ui_node_room_gold";
    private const string SaveSlotPanelPath = "UI/Panels/panel_save_slot_ink";
    private const string ArchetypeCardPanelPath = "UI/Panels/panel_archetype_card_ink";
    private const string TooltipSmallPanelPath = "UI/Panels/ui_tooltip_small_ink";
    private const string IdleTabPath = "UI/Buttons/ui_tab_idle_ink";
    private const string SelectedTabPath = "UI/Buttons/ui_tab_selected_gold";

    private static Sprite primaryButtonSprite;
    private static Sprite secondaryButtonSprite;
    private static Sprite dangerButtonSprite;
    private static Sprite regionNodeSprite;
    private static Sprite roomNodeSprite;
    private static Sprite saveSlotPanelSprite;
    private static Sprite archetypeCardPanelSprite;
    private static Sprite tooltipSmallPanelSprite;
    private static Sprite idleTabSprite;
    private static Sprite selectedTabSprite;

    public static void ApplyButtonSkin(Button button, GeneratedUiButtonSkin skin)
    {
        if (button == null)
        {
            return;
        }

        var graphic = button.image;
        if (graphic != null)
        {
            var sprite = ResolveButtonSprite(skin);
            if (sprite != null)
            {
                graphic.sprite = sprite;
                graphic.overrideSprite = sprite;
                graphic.color = Color.white;
                graphic.type = Image.Type.Simple;
                graphic.preserveAspect = false;
            }
        }

        button.transition = Selectable.Transition.ColorTint;
        button.colors = BuildColorBlock(skin);

        var label = button.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            label.color = ResolveLabelColor(skin);
            EnsureShadow(label);
        }
    }

    public static void ApplyRegionNodeSkin(Image image)
    {
        ApplySprite(image, ref regionNodeSprite, RegionNodePath);
    }

    public static void ApplyRoomNodeSkin(Image image)
    {
        ApplySprite(image, ref roomNodeSprite, RoomNodePath);
    }

    public static void ApplySaveSlotPanelSkin(Image image)
    {
        ApplySprite(image, ref saveSlotPanelSprite, SaveSlotPanelPath);
    }

    public static void ApplyArchetypeCardPanelSkin(Image image)
    {
        ApplySprite(image, ref archetypeCardPanelSprite, ArchetypeCardPanelPath);
    }

    public static void ApplySmallTooltipPanelSkin(Image image)
    {
        ApplySprite(image, ref tooltipSmallPanelSprite, TooltipSmallPanelPath);
        if (image != null && image.sprite != null)
        {
            image.type = image.sprite.border.sqrMagnitude > 0.01f ? Image.Type.Sliced : Image.Type.Simple;
            image.preserveAspect = false;
        }
    }

    public static void ApplyTabSkin(Image image, bool selected)
    {
        if (selected)
        {
            ApplySprite(image, ref selectedTabSprite, SelectedTabPath);
            return;
        }

        ApplySprite(image, ref idleTabSprite, IdleTabPath);
    }

    private static void ApplySprite(Image image, ref Sprite cache, string relativePath)
    {
        if (image == null)
        {
            return;
        }

        if (cache == null)
        {
            cache = GeneratedArtLibrary.GetRuntimeArtSprite(relativePath);
        }

        if (cache == null)
        {
            return;
        }

        image.sprite = cache;
        image.overrideSprite = cache;
        image.color = Color.white;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
    }

    private static Sprite ResolveButtonSprite(GeneratedUiButtonSkin skin)
    {
        switch (skin)
        {
            case GeneratedUiButtonSkin.Primary:
                if (primaryButtonSprite == null)
                {
                    primaryButtonSprite = GeneratedArtLibrary.GetRuntimeArtSprite(PrimaryButtonPath);
                }

                return primaryButtonSprite;
            case GeneratedUiButtonSkin.Danger:
                if (dangerButtonSprite == null)
                {
                    dangerButtonSprite = GeneratedArtLibrary.GetRuntimeArtSprite(DangerButtonPath);
                }

                return dangerButtonSprite;
            default:
                if (secondaryButtonSprite == null)
                {
                    secondaryButtonSprite = GeneratedArtLibrary.GetRuntimeArtSprite(SecondaryButtonPath);
                }

                return secondaryButtonSprite;
        }
    }

    private static ColorBlock BuildColorBlock(GeneratedUiButtonSkin skin)
    {
        var block = ColorBlock.defaultColorBlock;
        switch (skin)
        {
            case GeneratedUiButtonSkin.Primary:
                block.normalColor = new Color(0.96f, 0.94f, 0.9f, 1f);
                block.highlightedColor = Color.white;
                block.pressedColor = new Color(0.84f, 0.8f, 0.72f, 1f);
                block.selectedColor = new Color(1f, 0.97f, 0.86f, 1f);
                block.disabledColor = new Color(0.42f, 0.42f, 0.42f, 0.72f);
                break;
            case GeneratedUiButtonSkin.Danger:
                block.normalColor = new Color(0.95f, 0.9f, 0.9f, 1f);
                block.highlightedColor = new Color(1f, 0.96f, 0.96f, 1f);
                block.pressedColor = new Color(0.82f, 0.74f, 0.74f, 1f);
                block.selectedColor = new Color(1f, 0.92f, 0.92f, 1f);
                block.disabledColor = new Color(0.42f, 0.42f, 0.42f, 0.72f);
                break;
            default:
                block.normalColor = new Color(0.94f, 0.96f, 0.94f, 1f);
                block.highlightedColor = Color.white;
                block.pressedColor = new Color(0.8f, 0.84f, 0.8f, 1f);
                block.selectedColor = new Color(0.96f, 1f, 0.96f, 1f);
                block.disabledColor = new Color(0.42f, 0.42f, 0.42f, 0.72f);
                break;
        }

        block.colorMultiplier = 1f;
        block.fadeDuration = 0.08f;
        return block;
    }

    private static Color ResolveLabelColor(GeneratedUiButtonSkin skin)
    {
        switch (skin)
        {
            case GeneratedUiButtonSkin.Danger:
                return new Color(1f, 0.95f, 0.92f, 1f);
            default:
                return new Color(0.98f, 0.94f, 0.86f, 1f);
        }
    }

    private static void EnsureShadow(Text label)
    {
        var shadow = label.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = label.gameObject.AddComponent<Shadow>();
        }

        shadow.effectColor = new Color(0f, 0f, 0f, 0.42f);
        shadow.effectDistance = new Vector2(1.6f, -1.6f);
        shadow.useGraphicAlpha = true;
    }
}
