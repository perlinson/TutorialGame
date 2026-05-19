using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed class SaveSlotView : MonoBehaviour
{
    public Image background;
    public Image accent;
    public Text titleText;
    public Text detailText;
    public Text footerText;
    public Button button;

    private int slotIndex;

    public void Bind(int newSlotIndex, string title, string detail, string footer, bool selected, bool occupied, UnityEngine.Events.UnityAction onClick)
    {
        slotIndex = newSlotIndex;
        titleText.text = title;
        detailText.text = detail;
        footerText.text = footer;
        GeneratedUiSkinLibrary.ApplySaveSlotPanelSkin(background);

        background.color = selected
            ? new Color(1f, 0.97f, 0.9f, 1f)
            : new Color(0.92f, 0.9f, 0.84f, 0.94f);
        accent.color = occupied
            ? new Color(0.76f, 0.59f, 0.29f, 0.95f)
            : new Color(0.45f, 0.44f, 0.4f, 0.58f);
        titleText.color = selected
            ? new Color(0.25f, 0.17f, 0.08f, 1f)
            : new Color(0.2f, 0.18f, 0.14f, 0.96f);
        detailText.color = selected
            ? new Color(0.32f, 0.24f, 0.14f, 0.96f)
            : new Color(0.28f, 0.25f, 0.2f, 0.9f);
        footerText.color = occupied
            ? new Color(0.44f, 0.33f, 0.16f, 0.96f)
            : new Color(0.36f, 0.34f, 0.32f, 0.86f);

        CultivationAudio.BindButton(button, onClick);
    }

    public int SlotIndex => slotIndex;
}
