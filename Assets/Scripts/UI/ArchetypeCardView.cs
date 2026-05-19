using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

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

        CultivationAudio.BindButton(button, onClick);
    }

    public void SetSelected(bool selected)
    {
        GeneratedUiSkinLibrary.ApplyArchetypeCardPanelSkin(background);
        background.color = selected
            ? new Color(1f, 0.97f, 0.9f, 1f)
            : new Color(0.9f, 0.88f, 0.82f, 0.94f);
        accent.color = selected
            ? new Color(0.76f, 0.59f, 0.29f, 0.95f)
            : new Color(0.38f, 0.42f, 0.35f, 0.84f);
        titleText.color = selected
            ? new Color(0.25f, 0.17f, 0.08f, 1f)
            : new Color(0.2f, 0.18f, 0.14f, 0.96f);
        originText.color = new Color(0.34f, 0.3f, 0.22f, 0.96f);
        specialtyText.color = selected
            ? new Color(0.22f, 0.34f, 0.24f, 0.96f)
            : new Color(0.24f, 0.32f, 0.26f, 0.9f);
        descriptionText.color = new Color(0.26f, 0.23f, 0.18f, 0.92f);
        traitText.color = new Color(0.3f, 0.25f, 0.16f, 0.92f);
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
