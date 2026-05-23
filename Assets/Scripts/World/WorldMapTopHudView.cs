using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameHubView : MonoBehaviour
{
    [Header("Portrait")]
    public Image portraitImage;
    public TextMeshProUGUI portraitLabelText;

    [Header("Texts")]
    public TextMeshProUGUI worldTimeText;
    public TextMeshProUGUI heroNameText;
    public TextMeshProUGUI realmText;
    public TextMeshProUGUI locationText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI spiritText;
    public TextMeshProUGUI resourceText;

    [Header("Bars")]
    public Image healthFillImage;
    public Image spiritFillImage;

    [Header("Navigation")]
    public Button mapButton;
    public Button inventoryButton;
    public Button settlementButton;
    public Button sectButton;
    public Image mapButtonImage;
    public Image inventoryButtonImage;
    public Image settlementButtonImage;
    public Image sectButtonImage;
    public TextMeshProUGUI mapButtonLabel;
    public TextMeshProUGUI inventoryButtonLabel;
    public TextMeshProUGUI settlementButtonLabel;
    public TextMeshProUGUI sectButtonLabel;

    private static readonly Color NormalButtonColor = new Color(0.18f, 0.16f, 0.12f, 0.92f);
    private static readonly Color SelectedButtonColor = new Color(0.62f, 0.50f, 0.22f, 0.98f);
    private static readonly Color DisabledButtonColor = new Color(0.13f, 0.12f, 0.10f, 0.55f);
    private static readonly Color PortraitPlaceholderColor = new Color(0.22f, 0.18f, 0.14f, 1f);

    private static readonly Color NormalTextColor = new Color(0.82f, 0.78f, 0.68f, 1f);
    private static readonly Color SelectedTextColor = new Color(1f, 0.95f, 0.78f, 1f);
    private static readonly Color DisabledTextColor = new Color(0.45f, 0.42f, 0.38f, 1f);

    public void Apply(GameHubSnapshot snapshot, bool canNavigateSettlement, bool canNavigateSect)
    {
        if (snapshot == null)
        {
            return;
        }

        if (worldTimeText != null)
        {
            worldTimeText.text = snapshot.WorldTimeText;
        }

        if (heroNameText != null)
        {
            heroNameText.text = snapshot.HeroName;
        }

        if (realmText != null)
        {
            realmText.text = snapshot.RealmText;
        }

        if (locationText != null)
        {
            locationText.text = snapshot.LocationText;
        }

        if (healthText != null)
        {
            healthText.text = snapshot.HealthText;
        }

        if (spiritText != null)
        {
            spiritText.text = snapshot.SpiritText;
        }

        if (resourceText != null)
        {
            resourceText.text = snapshot.ResourceText;
        }

        GameSpriteLibrary.BindSpriteOrPlaceholder(
            portraitImage,
            portraitLabelText,
            snapshot.Portrait,
            "头像",
            PortraitPlaceholderColor,
            false);

        ApplyBarFill(healthFillImage, snapshot.CurrentHealth, snapshot.MaxHealth);
        ApplyBarFill(spiritFillImage, snapshot.CurrentSpirit, snapshot.MaxSpirit);

        if (mapButton != null)
        {
            mapButton.interactable = !snapshot.MapSelected;
        }

        if (inventoryButton != null)
        {
            inventoryButton.interactable = true;
        }

        if (settlementButton != null)
        {
            settlementButton.interactable = canNavigateSettlement && !snapshot.SettlementSelected;
        }

        if (sectButton != null)
        {
            sectButton.gameObject.SetActive(snapshot.ShowSectButton);
            sectButton.interactable = canNavigateSect && snapshot.ShowSectButton && !snapshot.SectSelected;
        }

        ApplyButtonVisual(mapButton, mapButtonImage, mapButtonLabel, "◈", snapshot.MapSelected);
        ApplyButtonVisual(inventoryButton, inventoryButtonImage, inventoryButtonLabel, "◆", false);
        ApplyButtonVisual(settlementButton, settlementButtonImage, settlementButtonLabel, "◇", snapshot.SettlementSelected);
        ApplyButtonVisual(sectButton, sectButtonImage, sectButtonLabel, "◎", snapshot.SectSelected);
    }

    private static void ApplyBarFill(Image fillImage, int currentValue, int maxValue)
    {
        if (fillImage == null)
        {
            return;
        }

        fillImage.fillAmount = maxValue <= 0 ? 0f : Mathf.Clamp01(currentValue / (float) maxValue);
    }

    private static void ApplyButtonVisual(Button button, Image image, TextMeshProUGUI label, string icon, bool selected)
    {
        if (image == null) return;

        var interactable = button != null && button.interactable;
        image.color = !interactable && !selected
            ? DisabledButtonColor
            : selected
                ? SelectedButtonColor
                : NormalButtonColor;

        if (label != null)
        {
            label.text = icon;
            label.color = !interactable && !selected
                ? DisabledTextColor
                : selected
                    ? SelectedTextColor
                    : NormalTextColor;
            label.fontSize = 22;
        }
    }
}
