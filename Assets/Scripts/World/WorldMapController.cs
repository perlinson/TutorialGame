using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed partial class WorldMapController : CultivationUIPanel, IGameHubNavigator
{
    private static readonly Vector2 MapContentDesignSize = new Vector2(1920f, 1080f);
    private static readonly Vector2 CompactActionButtonSize = new Vector2(72f, 72f);

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
    public Image regionPreviewImage;
    public Text regionPreviewLabelText;
    public Text hintText;

    public Button travelButton;
    public Button bagButton;
    public Button workshopButton;
    public Button sectResidenceButton;
    public Button vitalityUpgradeButton;
    public Button attackUpgradeButton;
    public Button returnButton;

    public GameObject mapScreen;

    private readonly List<WorldRegionDefinition> regions = new List<WorldRegionDefinition>();
    private readonly List<WorldRegionNodeView> nodeViews = new List<WorldRegionNodeView>();

    private RectTransform rootRect;
    private RectTransform titlePanelRect;
    private RectTransform mapPanelRect;
    private RectTransform mapContentRootRect;
    private RectTransform mapFieldRect;
    private RectTransform detailPanelRect;
    private RectTransform hintPanelRect;
    private CanvasGroup detailPanelCanvasGroup;
    private bool isInitialized;
    private bool compactMapLayoutPrepared;
    private int lastLayoutWidth = -1;
    private int lastLayoutHeight = -1;
    private string gameplaySceneName;
    private string mainSceneName;
    private int currentSlotIndex = -1;
    private int selectedRegionIndex;
    private int selectedSectHallIndex;
    private string selectedNpcId;
    private float detailPanelVisibility;
    private float detailPanelTargetVisibility;
    private Vector2 detailPanelShownPosition;
    private Vector2 detailPanelHiddenPosition;
    private MainMenuSaveData saveData;
    private SectHallSnapshot[] sectHallSnapshots = new SectHallSnapshot[0];
}
