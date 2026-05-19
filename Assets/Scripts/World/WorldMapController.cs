using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed partial class WorldMapController : UIPanel
{
    private static readonly Vector2 MapContentDesignSize = new Vector2(1920f, 1080f);
    private static readonly Vector2 InventoryWindowDesignSize = new Vector2(820f, 660f);
    private static readonly Vector2 WorkshopWindowDesignSize = new Vector2(920f, 680f);
    private static readonly Vector2 SectWindowDesignSize = new Vector2(1780f, 960f);

    public Text titleText;
    public Text heroSummaryText;
    public Text resourceSummaryText;
    public Text bagSummaryText;
    public Text regionTitleText;
    public Text regionBodyText;
    public Text regionStatusText;
    public Text taskSummaryText;
    public Image taskPreviewImage;
    public Text taskPreviewLabelText;
    public Text inventoryDetailText;
    public Text workshopSummaryText;
    public Text sectTitleText;
    public Text sectDescriptionText;
    public Text sectStatusText;
    public Image regionPreviewImage;
    public Text regionPreviewLabelText;
    public Image inventoryPreviewImage;
    public Text inventoryPreviewLabelText;
    public Image workshopPreviewImage;
    public Text workshopPreviewLabelText;
    public Image sectPreviewImage;
    public Text sectPreviewLabelText;
    public Text hintText;

    public Button travelButton;
    public Button bagButton;
    public Button workshopButton;
    public Button sectButton;
    public Button sectResidenceButton;
    public Button vitalityUpgradeButton;
    public Button attackUpgradeButton;
    public Button returnButton;
    public Button closeInventoryButton;
    public Button closeWorkshopButton;
    public Button closeSectButton;
    public Button craftQiButton;
    public Button craftBagButton;
    public Button craftVitalityButton;
    public Button craftAttackButton;
    public Button[] sectHallButtons;
    public Button[] sectActionButtons;

    public GameObject mapScreen;
    public GameObject inventoryPanel;
    public GameObject workshopPanel;
    public GameObject sectPanel;
    public GameObject modalBlocker;

    private readonly List<WorldRegionDefinition> regions = new List<WorldRegionDefinition>();
    private readonly List<WorldRegionNodeView> nodeViews = new List<WorldRegionNodeView>();
    private readonly ExclusiveUiPanelGroup primaryPanels = new ExclusiveUiPanelGroup();
    private readonly ExclusiveUiPanelGroup modalPanels = new ExclusiveUiPanelGroup();

    private RectTransform rootRect;
    private RectTransform mapContentRootRect;
    private RectTransform inventoryWindowRect;
    private RectTransform workshopWindowRect;
    private RectTransform sectWindowRect;
    private bool isInitialized;
    private int lastLayoutWidth = -1;
    private int lastLayoutHeight = -1;
    private string gameplaySceneName;
    private string mainSceneName;
    private int currentSlotIndex = -1;
    private int selectedRegionIndex;
    private int selectedSectHallIndex;
    private MainMenuSaveData saveData;
    private SectHallSnapshot[] sectHallSnapshots = new SectHallSnapshot[0];
}
