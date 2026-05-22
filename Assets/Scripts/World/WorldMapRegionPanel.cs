using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class WorldMapRegionPanel : CultivationUIPanel
{
    private static readonly Vector2 WindowDesignSize = new Vector2(1920f, 1080f);

    public Button closeButton;
    public Button travelButton;
    public Button vitalityButton;
    public Button attackButton;
    public Button dialogueButton;
    public TextMeshProUGUI panelTitleText;
    public TextMeshProUGUI panelSubtitleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI taskSummaryText;
    public TextMeshProUGUI vitalityButtonLabel;
    public TextMeshProUGUI attackButtonLabel;
    public TextMeshProUGUI travelButtonLabel;
    public Image previewImage;
    public TextMeshProUGUI previewLabelText;
    public RectTransform windowRect;

    private WorldMapController owner;
    private string regionId;
    private RectTransform rootRect;
    private bool buttonsBound;
    private int lastLayoutWidth = -1;
    private int lastLayoutHeight = -1;

    protected override void OnOpen(IUIData uiData = null)
    {
        var panelData = uiData as WorldMapRegionPanelData;
        owner = panelData != null ? panelData.Owner : null;
        regionId = panelData != null ? panelData.RegionId : string.Empty;
        rootRect = transform as RectTransform;
        EnsureBindings();
        RefreshFromOwner();
        RefreshResponsiveLayout(true);
        UiPanelOrderUtility.BringToFront(this, 205);
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

        var snapshot = owner.BuildRegionSnapshot(regionId);

        if (panelTitleText != null)
        {
            panelTitleText.text = snapshot.PanelTitle;
        }

        if (panelSubtitleText != null)
        {
            panelSubtitleText.text = snapshot.PanelSubtitle;
        }

        if (descriptionText != null)
        {
            descriptionText.text = snapshot.Description;
        }

        if (statusText != null)
        {
            statusText.text = snapshot.Status;
        }

        if (taskSummaryText != null)
        {
            taskSummaryText.text = snapshot.TaskSummary;
        }

        if (travelButton != null)
        {
            travelButton.interactable = snapshot.CanTravel;
        }

        if (vitalityButton != null)
        {
            vitalityButton.interactable = snapshot.CanUpgradeVitality;
        }

        if (attackButton != null)
        {
            attackButton.interactable = snapshot.CanUpgradeAttack;
        }

        if (travelButtonLabel != null)
        {
            travelButtonLabel.text = snapshot.TravelButtonLabel;
        }

        if (vitalityButtonLabel != null)
        {
            vitalityButtonLabel.text = snapshot.VitalityButtonLabel;
        }

        if (attackButtonLabel != null)
        {
            attackButtonLabel.text = snapshot.AttackButtonLabel;
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
    }

    private void EnsureBindings()
    {
        if (buttonsBound)
        {
            return;
        }

        BindButton(closeButton, ClosePanel, CultivationButtonSound.Cancel);
        BindButton(travelButton, () => owner?.TravelToSelectedRegion(), CultivationButtonSound.Confirm);
        BindButton(vitalityButton, () => owner?.UpgradeVitality(), CultivationButtonSound.Confirm);
        BindButton(attackButton, () => owner?.UpgradeAttack(), CultivationButtonSound.Confirm);
        BindButton(dialogueButton, () => owner?.OpenRegionDialogue(regionId), CultivationButtonSound.Confirm);
        CultivationTooltipBinder.Bind(closeButton, "返回山海图", "关闭当前地点页面，回到大地图。");
        CultivationTooltipBinder.Bind(travelButton, "进入历练", "直接进入当前地点对应的历练流程。");
        CultivationTooltipBinder.Bind(vitalityButton, "温养护身法器", "消耗灵石强化护身法器，提升生存能力。");
        CultivationTooltipBinder.Bind(attackButton, "祭炼主法器", "消耗灵石强化主法器，提升攻伐能力。");
        CultivationTooltipBinder.Bind(dialogueButton, "人物与线索", "查看本地斥候、线人和当前地域的对话线索。");
        buttonsBound = true;
    }

    private void ClosePanel()
    {
        owner?.CloseRegionPage();
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
