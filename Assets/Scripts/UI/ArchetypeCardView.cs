using UnityEngine;
using UnityEngine.UI;

public sealed class ArchetypeCardView : MonoBehaviour
{
    public Image background;
    public Image accent;
    public Image portraitImage;
    public Text titleText;
    public Text originText;
    public Text specialtyText;
    public Text descriptionText;
    public Text traitText;
    public Text portraitLabelText;
    public Button button;

    private AspectRatioFitter portraitAspectFitter;

    private int index;
    private MainMenuArchetype boundData;

    public void Bind(int newIndex, MainMenuArchetype data, bool selected, UnityEngine.Events.UnityAction onClick)
    {
        index = newIndex;
        boundData = data;

        titleText.text = data.name;
        originText.text = data.origin;
        specialtyText.text = data.specialty;
        descriptionText.text = data.description;
        traitText.text = data.trait + "\n\n" + data.recommendation;
        GameSpriteLibrary.BindSpriteOrPlaceholder(
            portraitImage,
            portraitLabelText,
            data.portraitSprite,
            data.name,
            new Color(0.26f, 0.22f, 0.18f, 1f),
            false);
        RefreshPortraitLayout();

        SetSelected(selected);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
    }

    public void SetSelected(bool selected)
    {
        background.color = selected
            ? new Color(0.36f, 0.25f, 0.16f, 0.84f)
            : new Color(0.13f, 0.14f, 0.16f, 0.78f);
        accent.color = selected
            ? new Color(0.76f, 0.59f, 0.29f, 0.95f)
            : new Color(0.34f, 0.31f, 0.26f, 0.85f);
        titleText.color = selected
            ? new Color(0.95f, 0.9f, 0.8f, 1f)
            : new Color(0.84f, 0.82f, 0.76f, 0.96f);
    }

    public int Index => index;
    public MainMenuArchetype Data => boundData;

    private void RefreshPortraitLayout()
    {
        if (portraitImage == null)
        {
            return;
        }

        if (portraitAspectFitter == null)
        {
            portraitAspectFitter = portraitImage.GetComponent<AspectRatioFitter>();
        }

        if (portraitAspectFitter == null)
        {
            return;
        }

        var sprite = portraitImage.sprite;
        if (sprite == null || sprite == GameSpriteLibrary.WhiteSquareSprite)
        {
            portraitAspectFitter.aspectMode = AspectRatioFitter.AspectMode.None;
            return;
        }

        portraitAspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        portraitAspectFitter.aspectRatio = sprite.rect.width / sprite.rect.height;
    }
}
