using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerCompendiumNodeView : MonoBehaviour
{
    private static readonly Color LockedBackgroundColor = new Color(0.14f, 0.15f, 0.16f, 0.88f);
    private static readonly Color UnlockedBackgroundColor = new Color(0.19f, 0.17f, 0.12f, 0.94f);
    private static readonly Color FocusedBackgroundColor = new Color(0.34f, 0.27f, 0.12f, 0.98f);
    private static readonly Color LockedAccentColor = new Color(0.28f, 0.29f, 0.31f, 0.82f);
    private static readonly Color UnlockedAccentColor = new Color(0.64f, 0.54f, 0.28f, 0.94f);
    private static readonly Color FocusedAccentColor = new Color(0.9f, 0.74f, 0.34f, 0.98f);
    private static readonly Color LockedTextColor = new Color(0.68f, 0.69f, 0.72f, 0.9f);
    private static readonly Color UnlockedTextColor = new Color(0.92f, 0.88f, 0.78f, 0.98f);

    public Image backgroundImage;
    public Image accentImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;
    public TextMeshProUGUI stateText;
    public TextMeshProUGUI descriptionText;

    public void Bind(PlayerCompendiumVisualNodeSnapshot snapshot)
    {
        if (snapshot == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (titleText != null)
        {
            titleText.text = snapshot.Title;
            titleText.color = snapshot.IsUnlocked ? UnlockedTextColor : LockedTextColor;
        }

        if (subtitleText != null)
        {
            subtitleText.text = snapshot.Subtitle;
            subtitleText.color = snapshot.IsUnlocked ? new Color(0.78f, 0.72f, 0.58f, 0.94f) : LockedTextColor;
        }

        if (stateText != null)
        {
            stateText.text = snapshot.StateText;
            stateText.color = snapshot.IsUnlocked ? new Color(0.98f, 0.9f, 0.64f, 1f) : new Color(0.76f, 0.78f, 0.82f, 0.92f);
        }

        if (descriptionText != null)
        {
            descriptionText.text = snapshot.Description;
            descriptionText.color = snapshot.IsUnlocked ? new Color(0.86f, 0.82f, 0.74f, 0.96f) : LockedTextColor;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = snapshot.IsFocused
                ? FocusedBackgroundColor
                : snapshot.IsUnlocked
                    ? UnlockedBackgroundColor
                    : LockedBackgroundColor;
        }

        if (accentImage != null)
        {
            accentImage.color = snapshot.IsFocused
                ? FocusedAccentColor
                : snapshot.IsUnlocked
                    ? UnlockedAccentColor
                    : LockedAccentColor;
        }
    }
}
