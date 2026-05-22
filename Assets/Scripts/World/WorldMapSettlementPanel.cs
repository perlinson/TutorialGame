using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class WorldMapSettlementPanel : CultivationUIPanel
{
    private static readonly Vector2 WindowDesignSize = new Vector2(1920f, 1080f);

    public Button closeButton;
    public Button inventoryButton;
    public Button workshopButton;
    public Button vitalityButton;
    public Button attackButton;
    public Button dialogueButton;
    public TextMeshProUGUI panelTitleText;
    public TextMeshProUGUI panelSubtitleText;
    public TextMeshProUGUI summaryText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI actionHintText;
    public TextMeshProUGUI inventoryButtonLabel;
    public TextMeshProUGUI workshopButtonLabel;
    public TextMeshProUGUI vitalityButtonLabel;
    public TextMeshProUGUI attackButtonLabel;
    public Image previewImage;
    public TextMeshProUGUI previewLabelText;
    public RectTransform windowRect;

    private WorldMapController owner;
    private RectTransform rootRect;
    private bool buttonsBound;
    private int lastLayoutWidth = -1;
    private int lastLayoutHeight = -1;

    protected override void OnOpen(IUIData uiData = null)
    {
        owner = (uiData as WorldMapSettlementPanelData)?.Owner;
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

        var snapshot = owner.BuildSettlementSnapshot();
        if (panelTitleText != null)
        {
            panelTitleText.text = snapshot.PanelTitle;
        }

        if (panelSubtitleText != null)
        {
            panelSubtitleText.text = snapshot.PanelSubtitle;
        }

        if (summaryText != null)
        {
            summaryText.text = snapshot.SummaryText;
        }

        if (statusText != null)
        {
            statusText.text = snapshot.StatusText;
        }

        if (actionHintText != null)
        {
            actionHintText.text = snapshot.ActionHintText;
        }

        if (inventoryButtonLabel != null)
        {
            inventoryButtonLabel.text = snapshot.InventoryButtonLabel;
        }

        if (workshopButtonLabel != null)
        {
            workshopButtonLabel.text = snapshot.WorkshopButtonLabel;
        }

        if (vitalityButtonLabel != null)
        {
            vitalityButtonLabel.text = snapshot.VitalityButtonLabel;
        }

        if (attackButtonLabel != null)
        {
            attackButtonLabel.text = snapshot.AttackButtonLabel;
        }

        if (vitalityButton != null)
        {
            vitalityButton.interactable = snapshot.CanUpgradeVitality;
        }

        if (attackButton != null)
        {
            attackButton.interactable = snapshot.CanUpgradeAttack;
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
        BindButton(inventoryButton, () => owner?.OpenInventory());
        BindButton(workshopButton, () => owner?.OpenWorkshopWorkbench());
        BindButton(vitalityButton, () => owner?.UpgradeVitality(), CultivationButtonSound.Confirm);
        BindButton(attackButton, () => owner?.UpgradeAttack(), CultivationButtonSound.Confirm);
        BindButton(dialogueButton, () => owner?.OpenSettlementDialogue(), CultivationButtonSound.Confirm);
        CultivationTooltipBinder.Bind(closeButton, "返回山海图", "关闭当前城镇/整备页面，回到大地图。");
        CultivationTooltipBinder.Bind(inventoryButton, "修士总览", "打开人物、物品、天赋与修仙技艺的综合页面。");
        CultivationTooltipBinder.Bind(workshopButton, "炼制台", "打开配方炼制与整备弹窗。");
        CultivationTooltipBinder.Bind(vitalityButton, "温养护身法器", "消耗灵石提升护身能力。");
        CultivationTooltipBinder.Bind(attackButton, "祭炼主法器", "消耗灵石提升攻伐能力。");
        CultivationTooltipBinder.Bind(dialogueButton, "坊市人物", "查看行商、药师与地域风声相关的对话入口。");
        buttonsBound = true;
    }

    private void ClosePanel()
    {
        owner?.CloseSettlement();
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
