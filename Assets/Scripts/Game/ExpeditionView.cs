using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class ExpeditionView : MonoBehaviour
{
    public Text titleText;
    public Text heroNameText;
    public Text heroStatsText;
    public Text expeditionStatsText;
    public Text phaseText;
    public Text roomTitleText;
    public Text roomDescriptionText;
    public Image roomPreviewImage;
    public Text roomPreviewLabelText;
    public Text loadoutText;
    public Image heroPreviewImage;
    public Text heroPreviewLabelText;
    public Image enemyPreviewImage;
    public Text enemyPreviewLabelText;
    public Text enemyStatusText;
    public Text logText;
    public Text skillText;
    public Text hintText;
    public Button[] actionButtons;
    public Text[] actionLabels;
    public Image[] actionIconImages;
    public Text[] actionIconLabelTexts;
    public ExpeditionRoomNodeView[] roomNodes;
    public GameObject eventOverlayRoot;
    public Text eventBadgeText;
    public Text eventTitleText;
    public Text eventBodyText;
    public Image eventPreviewImage;
    public Text eventPreviewLabelText;
    public Button[] eventOptionButtons;
    public Text[] eventOptionLabelTexts;
    public Text[] eventOptionRequirementTexts;
    public Text[] eventOptionBadgeTexts;
    public Text eventResultText;
    public Button eventConfirmButton;
    public Text eventConfirmLabelText;

    public bool IsEventOverlayVisible => eventOverlayRoot != null && eventOverlayRoot.activeSelf;

    public void SetHeader(string title, string heroName, string heroStats, string expeditionStats, string phase)
    {
        titleText.text = title;
        heroNameText.text = heroName;
        heroStatsText.text = heroStats;
        expeditionStatsText.text = expeditionStats;
        phaseText.text = phase;
    }

    public void SetRoomContent(string roomTitle, string description, string loadoutSummary, string enemySummary, string logSummary, string skillSummary, string hint)
    {
        roomTitleText.text = roomTitle;
        roomDescriptionText.text = description;
        loadoutText.text = loadoutSummary;
        enemyStatusText.text = enemySummary;
        logText.text = logSummary;
        skillText.text = skillSummary;
        hintText.text = "远征提示 / " + hint;
    }

    public void SetVisuals(Sprite roomSprite, string roomLabel, Sprite heroSprite, string heroLabel, Sprite enemySprite, string enemyLabel)
    {
        GameSpriteLibrary.BindSpriteOrPlaceholder(roomPreviewImage, roomPreviewLabelText, roomSprite, roomLabel, new Color(0.23f, 0.2f, 0.16f, 1f));
        GameSpriteLibrary.BindSpriteOrPlaceholder(heroPreviewImage, heroPreviewLabelText, heroSprite, heroLabel, new Color(0.22f, 0.18f, 0.14f, 1f));
        GameSpriteLibrary.BindSpriteOrPlaceholder(enemyPreviewImage, enemyPreviewLabelText, enemySprite, enemyLabel, new Color(0.22f, 0.14f, 0.14f, 1f));
    }

    public void SetTrack(System.Collections.Generic.IReadOnlyList<ExpeditionRoomState> rooms, int currentRoomIndex)
    {
        if (roomNodes == null)
        {
            return;
        }

        for (var i = 0; i < roomNodes.Length && i < rooms.Count; i++)
        {
            roomNodes[i].Bind(rooms[i], i == currentRoomIndex);
        }
    }

    public void ClearActions()
    {
        if (actionButtons == null)
        {
            return;
        }

        for (var i = 0; i < actionButtons.Length; i++)
        {
            SetAction(i, "待命", false, null);
        }
    }

    public void SetAction(int index, string label, bool enabled, UnityAction action, Sprite iconSprite = null, string iconCaption = null)
    {
        if (actionButtons == null || actionLabels == null || index < 0 || index >= actionButtons.Length || index >= actionLabels.Length)
        {
            return;
        }

        actionLabels[index].text = label;
        actionButtons[index].interactable = enabled;
        actionButtons[index].onClick.RemoveAllListeners();
        if (action != null)
        {
            actionButtons[index].onClick.AddListener(action);
        }

        if (actionIconImages != null && index < actionIconImages.Length)
        {
            var iconLabel = actionIconLabelTexts != null && index < actionIconLabelTexts.Length ? actionIconLabelTexts[index] : null;
            GameSpriteLibrary.BindSpriteOrPlaceholder(
                actionIconImages[index],
                iconLabel,
                iconSprite,
                BuildIconCaption(label, iconCaption),
                new Color(0.28f, 0.22f, 0.15f, 1f),
                false);
        }
    }

    public void HideEventOverlay()
    {
        if (eventOverlayRoot != null)
        {
            eventOverlayRoot.SetActive(false);
        }
    }

    public void ShowEventCard(ExpeditionEventCardResult card, System.Action<string> onOptionSelected)
    {
        if (card == null || eventOverlayRoot == null)
        {
            return;
        }

        eventOverlayRoot.SetActive(true);
        if (eventBadgeText != null)
        {
            eventBadgeText.text = string.IsNullOrWhiteSpace(card.BadgeText) ? "历练事件" : card.BadgeText;
        }

        if (eventTitleText != null)
        {
            eventTitleText.text = card.Title;
        }

        if (eventBodyText != null)
        {
            eventBodyText.text = card.Body;
        }

        GameSpriteLibrary.BindSpriteOrPlaceholder(eventPreviewImage, eventPreviewLabelText, card.IllustrationImage, card.Title, new Color(0.23f, 0.2f, 0.16f, 1f));
        if (eventResultText != null)
        {
            eventResultText.text = string.Empty;
        }

        if (eventConfirmButton != null)
        {
            eventConfirmButton.gameObject.SetActive(false);
            eventConfirmButton.onClick.RemoveAllListeners();
        }

        for (var i = 0; i < eventOptionButtons.Length; i++)
        {
            var button = eventOptionButtons[i];
            if (button == null)
            {
                continue;
            }

            var hasOption = card.Options != null && i < card.Options.Length && card.Options[i] != null;
            button.gameObject.SetActive(hasOption);
            button.onClick.RemoveAllListeners();
            if (!hasOption)
            {
                continue;
            }

            var option = card.Options[i];
            if (eventOptionLabelTexts != null && i < eventOptionLabelTexts.Length && eventOptionLabelTexts[i] != null)
            {
                eventOptionLabelTexts[i].text = option.Label;
            }

            if (eventOptionRequirementTexts != null && i < eventOptionRequirementTexts.Length && eventOptionRequirementTexts[i] != null)
            {
                eventOptionRequirementTexts[i].text = option.IsAvailable ? string.Empty : option.RequirementText;
            }

            if (eventOptionBadgeTexts != null && i < eventOptionBadgeTexts.Length && eventOptionBadgeTexts[i] != null)
            {
                eventOptionBadgeTexts[i].text = option.BadgeText ?? string.Empty;
            }

            button.interactable = option.IsAvailable;
            var optionId = option.OptionId;
            if (option.IsAvailable && onOptionSelected != null)
            {
                button.onClick.AddListener(() => onOptionSelected(optionId));
            }
        }
    }

    public void ShowEventResult(ExpeditionEventCardResult card, ExpeditionEventOptionResult result, UnityAction onConfirm)
    {
        if (card == null || result == null || eventOverlayRoot == null)
        {
            return;
        }

        eventOverlayRoot.SetActive(true);
        if (eventBadgeText != null)
        {
            eventBadgeText.text = string.IsNullOrWhiteSpace(result.ResultBadgeText) ? (card.BadgeText ?? "历练事件") : result.ResultBadgeText;
        }

        if (eventTitleText != null)
        {
            eventTitleText.text = string.IsNullOrWhiteSpace(result.ResultTitle) ? card.Title : result.ResultTitle;
        }

        if (eventBodyText != null)
        {
            eventBodyText.text = card.Body;
        }

        if (eventResultText != null)
        {
            eventResultText.text = result.ResultBody;
        }

        for (var i = 0; i < eventOptionButtons.Length; i++)
        {
            if (eventOptionButtons[i] != null)
            {
                eventOptionButtons[i].gameObject.SetActive(false);
            }
        }

        if (eventConfirmButton != null)
        {
            eventConfirmButton.gameObject.SetActive(true);
            eventConfirmButton.onClick.RemoveAllListeners();
            if (onConfirm != null)
            {
                eventConfirmButton.onClick.AddListener(onConfirm);
            }
        }

        if (eventConfirmLabelText != null)
        {
            eventConfirmLabelText.text = "收拢结果";
        }
    }

    private static string BuildIconCaption(string label, string overrideCaption)
    {
        if (!string.IsNullOrWhiteSpace(overrideCaption))
        {
            return overrideCaption;
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            return "图";
        }

        return label.Length <= 2 ? label : label.Substring(0, 2);
    }
}
