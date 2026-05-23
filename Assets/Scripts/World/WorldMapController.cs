using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed partial class WorldMapController : CultivationUIPanel, IGameHubNavigator
{
    private static readonly Vector2 MapContentDesignSize = new Vector2(1920f, 1080f);
    private static readonly Vector2 CompactActionButtonSize = new Vector2(72f, 72f);
    private static readonly Vector2 ScrollableMapSize = new Vector2(1920f, 2160f); // 地图高度翻倍，支持滚动

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
    public RectTransform mapScrollContent; // 可滚动的地图内容
    public Image characterIcon; // 人物图标

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

    // 地图滚动相关
    private Vector2 mapScrollPosition;
    private Vector2 targetMapScrollPosition;
    private float scrollSpeed = 800f;
    private bool isDraggingMap;
    private Vector2 lastMousePosition;
    private Vector2 characterTargetPosition;
    private float characterMoveSpeed = 600f;
    private bool isCharacterMoving;
}
