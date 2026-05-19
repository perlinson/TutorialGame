using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;
using InputField = TMPro.TMP_InputField;

public sealed class MainMenuCharacterCreatePanel : UIPanel
{
    private static readonly Vector2 WindowDesignSize = new Vector2(1540f, 860f);

    public Button blockerButton;
    public Button startNewGameButton;
    public Button closeButton;
    public InputField heroNameInput;

    public Text characterSummaryTitleText;
    public Text characterSummaryBodyText;

    public RectTransform windowRect;
    public Transform characterSlotsParent;
    public Transform archetypeCardsParent;
    public SaveSlotView characterSlotPrefab;
    public ArchetypeCardView archetypeCardPrefab;

    private readonly List<SaveSlotView> characterSlotViews = new List<SaveSlotView>();
    private readonly List<ArchetypeCardView> archetypeViews = new List<ArchetypeCardView>();
    private MainMenuController owner;
    private RectTransform rootRect;
    private int lastLayoutWidth = -1;
    private int lastLayoutHeight = -1;
    private bool buttonsBound;

    protected override void OnOpen(IUIData uiData = null)
    {
        owner = (uiData as MainMenuCharacterCreatePanelData)?.Owner;
        rootRect = transform as RectTransform;
        EnsureButtonsBound();
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
        BindButton(startNewGameButton, StartNewGame, CultivationButtonSound.Confirm);
        BindButton(closeButton, ClosePanel, CultivationButtonSound.Cancel);
        if (heroNameInput != null)
        {
            heroNameInput.onValueChanged.AddListener(OnHeroNameChanged);
        }

        buttonsBound = true;
    }

    private void RefreshFromOwner()
    {
        if (owner == null)
        {
            return;
        }

        var snapshot = owner.BuildCharacterSnapshot();
        EnsureCharacterSlotViews(snapshot.Slots.Length);
        EnsureArchetypeViews(snapshot.Archetypes.Length);

        for (var i = 0; i < characterSlotViews.Count && i < snapshot.Slots.Length; i++)
        {
            var slot = snapshot.Slots[i];
            characterSlotViews[i].Bind(
                slot.SlotIndex,
                slot.Title,
                slot.Detail,
                slot.Footer,
                slot.Selected,
                slot.Occupied,
                () =>
                {
                    owner.SelectCharacterSlot(slot.SlotIndex);
                    RefreshFromOwner();
                });
        }

        for (var i = 0; i < archetypeViews.Count && i < snapshot.Archetypes.Length; i++)
        {
            var index = i;
            archetypeViews[i].Bind(i, snapshot.Archetypes[i], i == snapshot.SelectedArchetypeIndex, () =>
            {
                owner.SelectArchetype(index);
                RefreshFromOwner();
            });
        }

        if (characterSummaryTitleText != null) characterSummaryTitleText.text = snapshot.SummaryTitle;
        if (characterSummaryBodyText != null) characterSummaryBodyText.text = snapshot.SummaryBody;
        if (heroNameInput != null && heroNameInput.text != snapshot.HeroName)
        {
            heroNameInput.SetTextWithoutNotify(snapshot.HeroName);
        }
    }

    private void EnsureCharacterSlotViews(int count)
    {
        if (characterSlotPrefab == null || characterSlotsParent == null)
        {
            return;
        }

        while (characterSlotViews.Count < count)
        {
            characterSlotViews.Add(Object.Instantiate(characterSlotPrefab, characterSlotsParent));
        }
    }

    private void EnsureArchetypeViews(int count)
    {
        if (archetypeCardPrefab == null || archetypeCardsParent == null)
        {
            return;
        }

        while (archetypeViews.Count < count)
        {
            archetypeViews.Add(Object.Instantiate(archetypeCardPrefab, archetypeCardsParent));
        }
    }

    private void OnHeroNameChanged(string value)
    {
        owner?.SetPendingHeroName(value);
    }

    private void StartNewGame()
    {
        owner?.StartNewGame(heroNameInput != null ? heroNameInput.text : string.Empty);
    }

    private void ClosePanel()
    {
        owner?.CloseCharacterPanel();
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
