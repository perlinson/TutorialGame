using QFramework;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed class WorldMapWorkshopPanel : CultivationUIPanel
{
    private static readonly Vector2 WindowDesignSize = new Vector2(920f, 680f);

    public Button blockerButton;
    public Button closeButton;
    public Button craftQiButton;
    public Button craftBagButton;
    public Button craftVitalityButton;
    public Button craftAttackButton;
    public Text summaryText;
    public Image previewImage;
    public Text previewLabelText;
    public RectTransform windowRect;

    private WorldMapController owner;
    private RectTransform rootRect;
    private bool buttonsBound;
    private int lastLayoutWidth = -1;
    private int lastLayoutHeight = -1;

    protected override void OnOpen(IUIData uiData = null)
    {
        owner = (uiData as WorldMapWorkshopPanelData)?.Owner;
        rootRect = transform as RectTransform;
        EnsureBindings();
        RefreshFromOwner();
        RefreshResponsiveLayout(true);
        UiPanelOrderUtility.BringToFront(this, 230);
    }

    protected override void OnClose()
    {
    }

    private void Update()
    {
        RefreshResponsiveLayout(false);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    public void RefreshFromOwner()
    {
        if (owner == null)
        {
            return;
        }

        var snapshot = owner.BuildWorkshopSnapshot();
        if (summaryText != null)
        {
            summaryText.text = snapshot.SummaryText;
        }

        if (snapshot.Preview != null)
        {
            GameSpriteLibrary.BindSpriteOrPlaceholder(
                previewImage,
                previewLabelText,
                snapshot.Preview.Sprite,
                snapshot.Preview.Label,
                snapshot.Preview.PlaceholderColor);
        }

        ApplyRecipeSnapshot(craftQiButton, GetRecipe(snapshot, "pill_cauldron_upgrade"));
        ApplyRecipeSnapshot(craftBagButton, GetRecipe(snapshot, "talisman_case_upgrade"));
        ApplyRecipeSnapshot(craftVitalityButton, GetRecipe(snapshot, "peiyuan_powder"));
        ApplyRecipeSnapshot(craftAttackButton, GetRecipe(snapshot, "nawu_pouch"));
    }

    private void EnsureBindings()
    {
        if (buttonsBound)
        {
            return;
        }

        BindButton(blockerButton, ClosePanel, CultivationButtonSound.Cancel);
        BindButton(closeButton, ClosePanel, CultivationButtonSound.Cancel);
        BindButton(craftQiButton, () => owner?.CraftRecipe("pill_cauldron_upgrade"), CultivationButtonSound.Confirm);
        BindButton(craftBagButton, () => owner?.CraftRecipe("talisman_case_upgrade"), CultivationButtonSound.Confirm);
        BindButton(craftVitalityButton, () => owner?.CraftRecipe("peiyuan_powder"), CultivationButtonSound.Confirm);
        BindButton(craftAttackButton, () => owner?.CraftRecipe("nawu_pouch"), CultivationButtonSound.Confirm);
        buttonsBound = true;
    }

    private void ClosePanel()
    {
        owner?.CloseWorkshop();
    }

    private void ApplyRecipeSnapshot(Button button, WorldMapWorkshopRecipeSnapshot recipe)
    {
        if (button == null || recipe == null)
        {
            return;
        }

        var label = button.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            label.text = recipe.ButtonLabel;
        }

        button.interactable = recipe.IsInteractable;
        CultivationTooltipBinder.Bind(button, recipe.TooltipTitle, recipe.TooltipBody);
    }

    private static WorldMapWorkshopRecipeSnapshot GetRecipe(WorldMapWorkshopSnapshot snapshot, string recipeId)
    {
        var recipes = snapshot != null ? snapshot.Recipes : null;
        if (recipes == null)
        {
            return null;
        }

        for (var i = 0; i < recipes.Length; i++)
        {
            if (recipes[i] != null && recipes[i].RecipeId == recipeId)
            {
                return recipes[i];
            }
        }

        return null;
    }

    private void RefreshResponsiveLayout(bool force)
    {
        if (rootRect == null || windowRect == null)
        {
            return;
        }

        var rect = rootRect.rect;
        if (rect.width < 1f || rect.height < 1f)
        {
            return;
        }

        var width = Mathf.RoundToInt(rect.width);
        var height = Mathf.RoundToInt(rect.height);
        if (!force && width == lastLayoutWidth && height == lastLayoutHeight)
        {
            return;
        }

        lastLayoutWidth = width;
        lastLayoutHeight = height;
        windowRect.sizeDelta = WindowDesignSize;
        var scale = Mathf.Min(1f, rect.width * 0.92f / WindowDesignSize.x, rect.height * 0.9f / WindowDesignSize.y);
        windowRect.localScale = new Vector3(scale, scale, 1f);
    }

}
