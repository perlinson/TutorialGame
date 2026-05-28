using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class WorldMapNpcDialoguePanel : CultivationUIPanel
{
    private static readonly Vector2 WindowDesignSize = new Vector2(1920f, 1080f);

    public Button blockerButton;
    public Button closeButton;
    public Button[] entryButtons;
    public Button[] incidentButtons;
    public Button[] choiceButtons;
    public TextMeshProUGUI panelTitleText;
    public TextMeshProUGUI panelSubtitleText;
    public TextMeshProUGUI storySummaryText;
    public TextMeshProUGUI taskSummaryText;
    public TextMeshProUGUI npcTitleText;
    public TextMeshProUGUI npcSubtitleText;
    public TextMeshProUGUI npcDescriptionText;
    public TextMeshProUGUI npcStatusText;
    public Image previewImage;
    public TextMeshProUGUI previewLabelText;
    public RectTransform windowRect;

    private WorldMapController owner;
    private NpcSceneType sceneType;
    private string regionId;
    private string sectHallId;
    private string locationId;
    private RectTransform rootRect;
    private int lastLayoutWidth = -1;
    private int lastLayoutHeight = -1;

    protected override void OnOpen(IUIData uiData = null)
    {
        var panelData = uiData as WorldMapNpcDialoguePanelData;
        owner = panelData != null ? panelData.Owner : null;
        sceneType = panelData != null ? panelData.SceneType : NpcSceneType.Settlement;
        regionId = panelData != null ? panelData.RegionId : string.Empty;
        sectHallId = panelData != null ? panelData.SectHallId : string.Empty;
        locationId = panelData != null ? panelData.LocationId : string.Empty;
        rootRect = transform as RectTransform;
        EnsureBindings();
        RefreshFromOwner();
        RefreshResponsiveLayout(true);
        UiPanelOrderUtility.BringToFront(this, 240);
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

        var snapshot = owner.BuildNpcDialogueSnapshot(sceneType, regionId, sectHallId, locationId);
        if (snapshot == null)
        {
            return;
        }

        if (panelTitleText != null)
        {
            panelTitleText.text = snapshot.PanelTitle;
        }

        if (panelSubtitleText != null)
        {
            panelSubtitleText.text = snapshot.PanelSubtitle;
        }

        if (storySummaryText != null)
        {
            storySummaryText.text = snapshot.StorySummary;
        }

        if (taskSummaryText != null)
        {
            taskSummaryText.text = snapshot.TaskSummary;
        }

        if (npcTitleText != null)
        {
            npcTitleText.text = snapshot.NpcTitle;
        }

        if (npcSubtitleText != null)
        {
            npcSubtitleText.text = snapshot.NpcSubtitle;
        }

        if (npcDescriptionText != null)
        {
            npcDescriptionText.text = snapshot.NpcDescription;
        }

        if (npcStatusText != null)
        {
            npcStatusText.text = snapshot.NpcStatus;
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

        ApplyEntryButtons(snapshot.Entries);
        ApplyIncidentButtons(snapshot.Incidents);
        ApplyChoiceButtons(snapshot.SelectedNpcId, snapshot.Choices);
    }

    private void EnsureBindings()
    {
        BindButton(blockerButton, ClosePanel, CultivationButtonSound.Cancel);
        BindButton(closeButton, ClosePanel, CultivationButtonSound.Cancel);
    }

    private void ApplyEntryButtons(WorldMapNpcEntrySnapshot[] entries)
    {
        if (entryButtons == null)
        {
            return;
        }

        for (var i = 0; i < entryButtons.Length; i++)
        {
            var button = entryButtons[i];
            if (button == null)
            {
                continue;
            }

            var hasEntry = entries != null && i < entries.Length && entries[i] != null;
            button.gameObject.SetActive(hasEntry);
            if (!hasEntry)
            {
                continue;
            }

            var entry = entries[i];
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = (entry.IsSelected ? "【" : string.Empty) +
                             entry.DisplayName +
                             (entry.IsSelected ? "】" : string.Empty) +
                             "\n<size=16>" + entry.RoleLabel + " · " + entry.StatusText + "</size>";
            }

            button.interactable = entry.IsInteractable;
            var npcId = entry.NpcId;
            CultivationTooltipBinder.Bind(button, entry.DisplayName, entry.RoleLabel + "\n" + entry.StatusText);
            BindButton(button, () => owner?.SelectNpc(sceneType, regionId, sectHallId, locationId, npcId));
        }
    }

    private void ApplyIncidentButtons(WorldMapIncidentEntrySnapshot[] incidents)
    {
        if (incidentButtons == null)
        {
            return;
        }

        for (var i = 0; i < incidentButtons.Length; i++)
        {
            var button = incidentButtons[i];
            if (button == null)
            {
                continue;
            }

            var hasIncident = incidents != null && i < incidents.Length && incidents[i] != null && incidents[i].IsVisible;
            button.gameObject.SetActive(hasIncident);
            if (!hasIncident)
            {
                continue;
            }

            var incident = incidents[i];
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = incident.ButtonLabel;
            }

            button.interactable = incident.IsInteractable;
            CultivationTooltipBinder.Bind(button, incident.TooltipTitle, incident.TooltipBody);
            var incidentId = incident.IncidentId;
            BindButton(button, () => owner?.OpenIncidentEntry(sceneType, regionId, sectHallId, locationId, incidentId), CultivationButtonSound.Confirm);
        }
    }

    private void ApplyChoiceButtons(string selectedNpcId, WorldMapNpcChoiceSnapshot[] choices)
    {
        if (choiceButtons == null)
        {
            return;
        }

        for (var i = 0; i < choiceButtons.Length; i++)
        {
            var button = choiceButtons[i];
            if (button == null)
            {
                continue;
            }

            var hasChoice = choices != null && i < choices.Length && choices[i] != null && choices[i].IsVisible;
            button.gameObject.SetActive(hasChoice);
            if (!hasChoice)
            {
                continue;
            }

            var choice = choices[i];
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = choice.ButtonLabel;
            }

            button.interactable = choice.IsInteractable;
            CultivationTooltipBinder.Bind(button, choice.TooltipTitle, choice.TooltipBody);
            var choiceId = choice.ChoiceId;
            BindButton(button, () => owner?.ExecuteNpcDialogueChoice(sceneType, regionId, sectHallId, locationId, selectedNpcId, choiceId), CultivationButtonSound.Confirm);
        }
    }

    private void ClosePanel()
    {
        owner?.CloseNpcDialogue();
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
