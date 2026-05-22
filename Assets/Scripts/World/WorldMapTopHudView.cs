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

    private static readonly Color NormalButtonColor = new Color(0.2f, 0.18f, 0.14f, 0.94f);
    private static readonly Color SelectedButtonColor = new Color(0.64f, 0.52f, 0.24f, 0.98f);
    private static readonly Color DisabledButtonColor = new Color(0.15f, 0.14f, 0.12f, 0.58f);
    private static readonly Color PortraitPlaceholderColor = new Color(0.22f, 0.18f, 0.14f, 1f);

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

        ApplyButtonVisual(mapButtonImage, snapshot.MapSelected, mapButton != null && mapButton.interactable);
        ApplyButtonVisual(inventoryButtonImage, false, inventoryButton != null && inventoryButton.interactable);
        ApplyButtonVisual(settlementButtonImage, snapshot.SettlementSelected, settlementButton != null && settlementButton.interactable);
        ApplyButtonVisual(sectButtonImage, snapshot.SectSelected, sectButton != null && sectButton.interactable);
    }

    private static void ApplyBarFill(Image fillImage, int currentValue, int maxValue)
    {
        if (fillImage == null)
        {
            return;
        }

        fillImage.fillAmount = maxValue <= 0 ? 0f : Mathf.Clamp01(currentValue / (float) maxValue);
    }

    private static void ApplyButtonVisual(Image image, bool selected, bool interactable)
    {
        if (image == null)
        {
            return;
        }

        image.color = !interactable && !selected
            ? DisabledButtonColor
            : selected
                ? SelectedButtonColor
                : NormalButtonColor;
    }
}
