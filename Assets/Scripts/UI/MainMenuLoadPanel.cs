using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed class MainMenuLoadPanel : UIPanel
{
    private static readonly Vector2 WindowDesignSize = new Vector2(1540f, 860f);

    public Button blockerButton;
    public Button loadSelectedButton;
    public Button deleteSelectedButton;
    public Button closeButton;

    public Text loadDetailTitleText;
    public Text loadDetailBodyText;
    public Text loadActionText;

    public RectTransform windowRect;
    public Transform loadSlotsParent;
    public SaveSlotView loadSlotPrefab;

    private readonly List<SaveSlotView> loadSlotViews = new List<SaveSlotView>();
    private MainMenuController owner;
    private RectTransform rootRect;
    private int lastLayoutWidth = -1;
    private int lastLayoutHeight = -1;
    private bool buttonsBound;

    protected override void OnOpen(IUIData uiData = null)
    {
        owner = (uiData as MainMenuLoadPanelData)?.Owner;
        rootRect = transform as RectTransform;
        EnsureButtonsBound();
        EnsureSlotViews();
        RefreshFromOwner();
        RefreshResponsiveLayout(true);
    }

    protected override void OnClose()
    {
    }

    private void Update()
    {
        RefreshResponsiveLayout(false);
    }

    private void EnsureButtonsBound()
    {
        if (buttonsBound)
        {
            return;
        }

        BindButton(blockerButton, ClosePanel, CultivationButtonSound.Cancel);
        BindButton(loadSelectedButton, () => { owner?.LoadSelectedSave(); RefreshFromOwner(); }, CultivationButtonSound.Confirm);
        BindButton(deleteSelectedButton, () => { owner?.DeleteSelectedSave(); RefreshFromOwner(); }, CultivationButtonSound.Cancel);
        BindButton(closeButton, ClosePanel, CultivationButtonSound.Cancel);
        buttonsBound = true;
    }

    private void EnsureSlotViews()
    {
        if (loadSlotPrefab == null || loadSlotsParent == null)
        {
            return;
        }

        while (loadSlotViews.Count < MainMenuSaveStore.SaveSlotCount)
        {
            loadSlotViews.Add(Object.Instantiate(loadSlotPrefab, loadSlotsParent));
        }
    }

    private void RefreshFromOwner()
    {
        if (owner == null)
        {
            return;
        }

        var snapshot = owner.BuildLoadSnapshot();
        for (var i = 0; i < loadSlotViews.Count && i < snapshot.Slots.Length; i++)
        {
            var slot = snapshot.Slots[i];
            loadSlotViews[i].Bind(
                slot.SlotIndex,
                slot.Title,
                slot.Detail,
                slot.Footer,
                slot.Selected,
                slot.Occupied,
                () =>
                {
                    owner.SelectLoadSlot(slot.SlotIndex);
                    RefreshFromOwner();
                });
        }

        if (loadDetailTitleText != null) loadDetailTitleText.text = snapshot.DetailTitle;
        if (loadDetailBodyText != null) loadDetailBodyText.text = snapshot.DetailBody;
        if (loadActionText != null) loadActionText.text = snapshot.ActionText;
        if (loadSelectedButton != null) loadSelectedButton.interactable = snapshot.CanLoad;
        if (deleteSelectedButton != null) deleteSelectedButton.interactable = snapshot.CanDelete;
    }

    private void ClosePanel()
    {
        owner?.CloseLoadPanel();
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
        var scale = Mathf.Min(1f, rect.width * 0.92f / WindowDesignSize.x, rect.height * 0.9f / WindowDesignSize.y);
        windowRect.localScale = new Vector3(scale, scale, 1f);
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action, CultivationButtonSound sound = CultivationButtonSound.Click)
    {
        CultivationAudio.BindButton(button, action, sound);
    }
}
