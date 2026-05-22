using QFramework;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed class WorldMapSectResidencePanel : CultivationUIPanel
{
    private static readonly Vector2 WindowDesignSize = new Vector2(1920f, 1080f);

    public Button closeButton;
    public Button dialogueButton;
    public Button[] hallButtons;
    public Button[] actionButtons;
    public Text panelTitleText;
    public Text panelSubtitleText;
    public Text hallTitleText;
    public Text descriptionText;
    public Text statusText;
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
        owner = (uiData as WorldMapSectResidencePanelData)?.Owner;
        rootRect = transform as RectTransform;
        EnsureBindings();
        RefreshFromOwner();
        RefreshResponsiveLayout(true);
        UiPanelOrderUtility.BringToFront(this, 210);
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

        var snapshot = owner.BuildSectResidenceSnapshot();
        if (panelTitleText != null)
        {
            panelTitleText.text = snapshot.PanelTitle;
        }

        if (panelSubtitleText != null)
        {
            panelSubtitleText.text = snapshot.PanelSubtitle;
        }

        if (hallTitleText != null)
        {
            hallTitleText.text = snapshot.HallTitle;
        }

        if (descriptionText != null)
        {
            descriptionText.text = snapshot.Description;
        }

        if (statusText != null)
        {
            statusText.text = snapshot.Status;
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

        ApplyHallButtons(snapshot.HallButtons);
        ApplyActionButtons(snapshot.ActionButtons);
    }

    private void EnsureBindings()
    {
        if (buttonsBound)
        {
            return;
        }

        BindButton(closeButton, ClosePanel, CultivationButtonSound.Cancel);
        BindButton(dialogueButton, () => owner?.OpenSectDialogue(), CultivationButtonSound.Confirm);
        CultivationTooltipBinder.Bind(dialogueButton, "同门人物", "按当前殿堂查看可对接的执事、前辈与同门。");
        buttonsBound = true;
    }

    private void ApplyHallButtons(WorldMapSectHallButtonSnapshot[] snapshots)
    {
        if (hallButtons == null)
        {
            return;
        }

        for (var i = 0; i < hallButtons.Length; i++)
        {
            var button = hallButtons[i];
            if (button == null)
            {
                continue;
            }

            var hasSnapshot = snapshots != null && i < snapshots.Length && snapshots[i] != null;
            button.gameObject.SetActive(hasSnapshot);
            if (!hasSnapshot)
            {
                continue;
            }

            var snapshot = snapshots[i];
            var label = button.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = snapshot.IsSelected ? "【" + snapshot.DisplayName + "】" : snapshot.DisplayName;
            }

            var capturedIndex = i;
            BindButton(button, () => owner?.SelectSectHall(capturedIndex));
        }
    }

    private void ApplyActionButtons(WorldMapSectActionButtonSnapshot[] snapshots)
    {
        if (actionButtons == null)
        {
            return;
        }

        for (var i = 0; i < actionButtons.Length; i++)
        {
            var button = actionButtons[i];
            if (button == null)
            {
                continue;
            }

            var hasSnapshot = snapshots != null && i < snapshots.Length && snapshots[i] != null && snapshots[i].IsVisible;
            button.gameObject.SetActive(hasSnapshot);
            if (!hasSnapshot)
            {
                continue;
            }

            var snapshot = snapshots[i];
            var label = button.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = snapshot.ButtonLabel;
            }

            button.interactable = snapshot.IsInteractable;
            CultivationTooltipBinder.Bind(button, snapshot.TooltipTitle, snapshot.TooltipBody);
            var capturedActionId = snapshot.ActionId;
            BindButton(button, () => owner?.ExecuteSectAction(capturedActionId), CultivationButtonSound.Confirm);
        }
    }

    private void ClosePanel()
    {
        owner?.CloseSect();
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
        var scale = Mathf.Min(1f, rect.width / WindowDesignSize.x, rect.height / WindowDesignSize.y);
        windowRect.localScale = new Vector3(scale, scale, 1f);
    }

}
