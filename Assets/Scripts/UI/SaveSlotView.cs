using UnityEngine;
using UnityEngine.UI;

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

        background.color = selected
            ? new Color(0.39f, 0.25f, 0.15f, 0.9f)
            : new Color(0.13f, 0.14f, 0.16f, 0.82f);
        accent.color = occupied
            ? new Color(0.76f, 0.59f, 0.29f, 0.95f)
            : new Color(0.3f, 0.3f, 0.3f, 0.58f);
        titleText.color = selected
            ? new Color(0.95f, 0.9f, 0.8f, 1f)
            : new Color(0.84f, 0.82f, 0.76f, 0.96f);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
    }

    public int SlotIndex => slotIndex;
}
