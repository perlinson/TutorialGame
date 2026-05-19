using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Text = TMPro.TextMeshProUGUI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class GameSceneBootstrap : MonoBehaviour
{
#if UNITY_EDITOR
    private const string PrimaryButtonArtPath = "Assets/GameArt/UI/Buttons/ui_btn_primary_gold.png";
    private const string RoomNodeArtPath = "Assets/GameArt/UI/Buttons/ui_node_room_gold.png";
#endif

    private void Awake()
    {
        AppRoot.EnsureCreated();
        SceneFlow.SyncActiveSceneState(SceneFlow.GameplaySceneName);
        CultivationApp.CloseAllGameUiPanels();

        var snapshot = CultivationApp.BootstrapCurrentArchive();
        if (snapshot == null || snapshot.SaveData == null)
        {
            SceneFlow.RequestScene("Main");
            return;
        }

        var slotIndex = snapshot.SlotIndex;
        var saveData = snapshot.SaveData;

        saveData.EnsureDefaults();
        WorldRegionDefinition region;
        if (!WorldRegionLibrary.TryGetRegion(saveData.currentRegionId, out region))
        {
            region = WorldRegionLibrary.GetStartingRegion();
            saveData.currentRegionId = region.Id;
            CultivationApp.SyncArchiveState(slotIndex, saveData);
        }

        PersistentExpeditionRuntimeSnapshot runtimeSnapshot = null;
        if (MainMenuSaveStore.TryLoadExpeditionRuntime(out var storedSnapshot))
        {
            if (IsCompatibleSnapshot(storedSnapshot, slotIndex, saveData, region))
            {
                runtimeSnapshot = storedSnapshot;
            }
            else
            {
                MainMenuSaveStore.ClearExpeditionRuntime();
            }
        }

        ConfigureCamera(region);
        CultivationAudio.PlayExpeditionMusic(region);

        var controller = GetComponent<GameController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<GameController>();
        }

        if (runtimeSnapshot != null)
        {
            controller.InitializeFromSnapshot(slotIndex, saveData, region, runtimeSnapshot);
        }
        else
        {
            controller.Initialize(slotIndex, saveData, region);
        }

        var view = CultivationApp.OpenExpeditionPanel();
        if (view == null)
        {
            CultivationApp.LogError("Expedition UI prefab is required but missing or invalid: UI/Game/ExpeditionRoot");
            SceneFlow.RequestScene("Main");
            return;
        }

        var arena = GameArenaBuilder.Build(region, saveData);
        controller.AttachArena(arena);
        AttachCameraFollow(controller, arena);
        controller.SetView(view);
    }

    private static bool IsCompatibleSnapshot(PersistentExpeditionRuntimeSnapshot snapshot, int slotIndex, MainMenuSaveData saveData, WorldRegionDefinition region)
    {
        if (snapshot == null || saveData == null || region == null)
        {
            return false;
        }

        snapshot.EnsureDefaults();
        return snapshot.IsUsable()
               && snapshot.slotIndex == slotIndex
               && snapshot.regionId == region.Id
               && snapshot.heroName == saveData.heroName
               && snapshot.archetypeId == saveData.archetypeId
               && snapshot.saveRealmTier == saveData.realmTier;
    }

    private void ConfigureCamera(WorldRegionDefinition region)
    {
        var sceneCamera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
        if (sceneCamera == null)
        {
            return;
        }

        sceneCamera.clearFlags = CameraClearFlags.SolidColor;
        sceneCamera.backgroundColor = Color.Lerp(region.BackdropColor, Color.black, 0.38f);
        sceneCamera.orthographic = true;
        sceneCamera.orthographicSize = 6.4f;
    }

    private void AttachCameraFollow(GameController controller, GameArenaRuntimeBindings arena)
    {
        if (arena == null || arena.Player == null)
        {
            return;
        }

        var sceneCamera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
        if (sceneCamera == null)
        {
            return;
        }

        var follow = sceneCamera.GetComponent<CameraFollow2D>();
        if (follow == null)
        {
            follow = sceneCamera.gameObject.AddComponent<CameraFollow2D>();
        }

        var hitStop = sceneCamera.GetComponent<CombatHitStop>();
        if (hitStop == null)
        {
            hitStop = sceneCamera.gameObject.AddComponent<CombatHitStop>();
        }

        follow.SetTarget(arena.Player.transform);
        if (controller != null)
        {
            controller.AttachCombatPresentation(follow, hitStop);
        }
    }

#if UNITY_EDITOR
    public static ExpeditionView BuildPrefabExportView(WorldRegionDefinition region, int roomCount)
    {
        var canvasRoot = new GameObject("ExpeditionCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasRoot.layer = 5;

        var canvas = canvasRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var root = canvasRoot.GetComponent<RectTransform>();
        Stretch(root);

        var backdrop = CreatePanel("Backdrop", root, Vector2.zero, Vector2.zero, Color.Lerp(region.BackdropColor, Color.black, 0.18f));
        Stretch(backdrop);

        var header = CreatePanel("Header", root, new Vector2(46f, -38f), new Vector2(1828f, 140f), new Color(0.07f, 0.08f, 0.09f, 0.88f));
        SetTopLeft(header);
        AddOutline(header, Color.Lerp(region.AccentColor, new Color(0.83f, 0.72f, 0.38f, 1f), 0.4f));

        var titleText = CreateText("Title", header, new Vector2(24f, -16f), new Vector2(520f, 30f), 28, FontStyle.Bold, TextAnchor.MiddleLeft);
        var heroNameText = CreateText("HeroName", header, new Vector2(24f, -52f), new Vector2(420f, 26f), 20, FontStyle.Bold, TextAnchor.MiddleLeft);
        var heroStatsText = CreateText("HeroStats", header, new Vector2(24f, -82f), new Vector2(820f, 24f), 19, FontStyle.Normal, TextAnchor.MiddleLeft);
        var expeditionStatsText = CreateText("ExpeditionStats", header, new Vector2(878f, -52f), new Vector2(760f, 56f), 20, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(expeditionStatsText);
        var phaseText = CreateText("Phase", header, new Vector2(1646f, -42f), new Vector2(150f, 34f), 24, FontStyle.Bold, TextAnchor.MiddleRight);

        var trackPanel = CreatePanel("TrackPanel", root, new Vector2(46f, -200f), new Vector2(1828f, 110f), new Color(0.08f, 0.09f, 0.1f, 0.78f));
        SetTopLeft(trackPanel);
        AddOutline(trackPanel, new Color(0.45f, 0.38f, 0.24f, 0.88f));

        var trackCaption = CreateText("TrackCaption", trackPanel, new Vector2(20f, -12f), new Vector2(220f, 24f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
        trackCaption.text = "远征路径";

        var nodeParent = new GameObject("Nodes", typeof(RectTransform)).GetComponent<RectTransform>();
        nodeParent.SetParent(trackPanel, false);
        nodeParent.anchoredPosition = new Vector2(22f, -48f);
        nodeParent.sizeDelta = new Vector2(1780f, 48f);
        SetTopLeft(nodeParent);

        var nodes = new ExpeditionRoomNodeView[roomCount];
        var spacing = roomCount > 1 ? 1620f / (roomCount - 1) : 0f;
        for (var i = 0; i < roomCount; i++)
        {
            nodes[i] = CreateRoomNode(nodeParent, new Vector2(56f + spacing * i, 0f));
        }

        var contentLeft = CreatePanel("ContentLeft", root, new Vector2(46f, -338f), new Vector2(980f, 560f), new Color(0.08f, 0.09f, 0.11f, 0.82f));
        SetTopLeft(contentLeft);
        AddOutline(contentLeft, Color.Lerp(region.AccentColor, new Color(0.84f, 0.73f, 0.4f, 1f), 0.32f));

        var roomTitleText = CreateText("RoomTitle", contentLeft, new Vector2(24f, -18f), new Vector2(900f, 30f), 26, FontStyle.Bold, TextAnchor.MiddleLeft);
        var roomPreview = CreatePanel("RoomPreview", contentLeft, new Vector2(24f, -62f), new Vector2(926f, 128f), new Color(0.22f, 0.18f, 0.14f, 1f));
        SetTopLeft(roomPreview);
        roomPreview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var roomPreviewLabel = CreateText("RoomPreviewLabel", roomPreview, new Vector2(0f, 0f), new Vector2(926f, 128f), 22, FontStyle.Bold, TextAnchor.MiddleCenter);
        var roomDescriptionText = CreateText("RoomDescription", contentLeft, new Vector2(24f, -208f), new Vector2(900f, 72f), 20, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(roomDescriptionText);
        var loadoutText = CreateText("LoadoutText", contentLeft, new Vector2(24f, -286f), new Vector2(900f, 62f), 17, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(loadoutText);

        var heroBanner = CreatePanel("HeroBanner", contentLeft, new Vector2(26f, -364f), new Vector2(280f, 180f), Color.Lerp(region.AccentColor, new Color(0.16f, 0.12f, 0.09f, 1f), 0.58f));
        SetTopLeft(heroBanner);
        heroBanner.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var heroPreviewLabel = CreateText("HeroPreviewLabel", heroBanner, new Vector2(0f, 0f), new Vector2(280f, 180f), 24, FontStyle.Bold, TextAnchor.MiddleCenter);

        var enemyBanner = CreatePanel("EnemyBanner", contentLeft, new Vector2(358f, -364f), new Vector2(592f, 180f), new Color(0.14f, 0.11f, 0.12f, 0.92f));
        SetTopLeft(enemyBanner);
        enemyBanner.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var enemyPreviewLabel = CreateText("EnemyPreviewLabel", enemyBanner, new Vector2(0f, 0f), new Vector2(592f, 48f), 24, FontStyle.Bold, TextAnchor.MiddleCenter);
        var enemyStatusText = CreateText("EnemyStatus", enemyBanner, new Vector2(20f, -54f), new Vector2(550f, 118f), 18, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(enemyStatusText);

        var contentRight = CreatePanel("ContentRight", root, new Vector2(1060f, -338f), new Vector2(814f, 560f), new Color(0.09f, 0.1f, 0.11f, 0.84f));
        SetTopLeft(contentRight);
        AddOutline(contentRight, new Color(0.48f, 0.4f, 0.24f, 0.88f));

        CreateText("LogLabel", contentRight, new Vector2(24f, -18f), new Vector2(220f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "远征记录";
        var logText = CreateText("LogText", contentRight, new Vector2(24f, -56f), new Vector2(766f, 228f), 19, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(logText);

        var skillPanel = CreatePanel("SkillPanel", contentRight, new Vector2(24f, -304f), new Vector2(766f, 132f), new Color(0.08f, 0.09f, 0.1f, 0.84f));
        SetTopLeft(skillPanel);
        var skillText = CreateText("SkillText", skillPanel, new Vector2(16f, -12f), new Vector2(734f, 104f), 16, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(skillText);

        var hintPanel = CreatePanel("HintPanel", contentRight, new Vector2(24f, -452f), new Vector2(766f, 66f), new Color(0.08f, 0.09f, 0.09f, 0.86f));
        SetTopLeft(hintPanel);
        var hintText = CreateText("HintText", hintPanel, new Vector2(18f, -10f), new Vector2(730f, 42f), 17, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(hintText);

        var actionPanel = CreatePanel("ActionPanel", root, new Vector2(46f, -928f), new Vector2(1828f, 116f), new Color(0.07f, 0.08f, 0.09f, 0.9f));
        SetTopLeft(actionPanel);
        AddOutline(actionPanel, Color.Lerp(region.AccentColor, new Color(0.84f, 0.72f, 0.4f, 1f), 0.3f));

        var buttons = new Button[6];
        var buttonLabels = new Text[6];
        var buttonIcons = new Image[6];
        var buttonIconLabels = new Text[6];
        for (var i = 0; i < buttons.Length; i++)
        {
            var buttonRect = CreatePanel("ActionButton" + i, actionPanel, new Vector2(20f + i * 302f, -22f), new Vector2(282f, 72f), new Color(0.2f, 0.17f, 0.12f, 0.96f));
            SetTopLeft(buttonRect);
            var buttonImage = buttonRect.GetComponent<Image>();
            var button = buttonRect.gameObject.AddComponent<Button>();
            var iconRect = CreatePanel("Icon", buttonRect, new Vector2(10f, -10f), new Vector2(52f, 52f), new Color(0.28f, 0.22f, 0.15f, 1f));
            SetTopLeft(iconRect);
            iconRect.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
            var iconLabel = CreateText("IconLabel", iconRect, Vector2.zero, new Vector2(52f, 52f), 16, FontStyle.Bold, TextAnchor.MiddleCenter);
            var labelRect = CreateText("Label", buttonRect, new Vector2(76f, 0f), new Vector2(196f, 72f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
            ApplyOptionalSprite(buttonImage, PrimaryButtonArtPath);

            buttons[i] = button;
            buttonIcons[i] = iconRect.GetComponent<Image>();
            buttonIconLabels[i] = iconLabel;
            buttonLabels[i] = labelRect;
            buttonLabels[i].text = "待命";
            button.targetGraphic = buttonImage;
        }

        var eventOverlayBlocker = CreatePanel("EventOverlayBlocker", root, Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.04f, 0.72f));
        Stretch(eventOverlayBlocker);
        eventOverlayBlocker.gameObject.AddComponent<Button>().transition = Selectable.Transition.None;
        eventOverlayBlocker.gameObject.SetActive(false);

        var eventOverlay = CreatePanel("EventOverlay", root, new Vector2(400f, -170f), new Vector2(1120f, 700f), new Color(0.05f, 0.05f, 0.06f, 0.96f));
        SetTopLeft(eventOverlay);
        AddOutline(eventOverlay, Color.Lerp(region.AccentColor, new Color(0.88f, 0.76f, 0.42f, 1f), 0.5f));
        eventOverlay.gameObject.SetActive(false);

        var eventBadge = CreateText("EventBadge", eventOverlay, new Vector2(26f, -18f), new Vector2(180f, 28f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
        var eventTitle = CreateText("EventTitle", eventOverlay, new Vector2(26f, -52f), new Vector2(720f, 34f), 28, FontStyle.Bold, TextAnchor.MiddleLeft);
        var eventPreview = CreatePanel("EventPreview", eventOverlay, new Vector2(26f, -102f), new Vector2(1068f, 180f), new Color(0.2f, 0.17f, 0.13f, 1f));
        SetTopLeft(eventPreview);
        eventPreview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var eventPreviewLabel = CreateText("EventPreviewLabel", eventPreview, Vector2.zero, new Vector2(1068f, 180f), 24, FontStyle.Bold, TextAnchor.MiddleCenter);

        var eventBody = CreateText("EventBody", eventOverlay, new Vector2(26f, -304f), new Vector2(1068f, 120f), 21, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(eventBody);

        var eventResultText = CreateText("EventResultText", eventOverlay, new Vector2(26f, -440f), new Vector2(1068f, 88f), 18, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(eventResultText);
        eventResultText.color = new Color(0.87f, 0.82f, 0.68f, 0.98f);

        var eventOptionButtons = new Button[4];
        var eventOptionLabelTexts = new Text[4];
        var eventOptionRequirementTexts = new Text[4];
        var eventOptionBadgeTexts = new Text[4];
        for (var i = 0; i < eventOptionButtons.Length; i++)
        {
            var optionRect = CreatePanel("EventOption" + i, eventOverlay, new Vector2(26f + (i % 2) * 540f, -548f + (i / 2) * 82f), new Vector2(512f, 64f), new Color(0.18f, 0.15f, 0.11f, 0.98f));
            SetTopLeft(optionRect);
            var optionImage = optionRect.GetComponent<Image>();
            var optionButton = optionRect.gameObject.AddComponent<Button>();
            optionButton.targetGraphic = optionImage;
            ApplyOptionalSprite(optionImage, PrimaryButtonArtPath);
            var optionBadge = CreateText("Badge", optionRect, new Vector2(12f, -8f), new Vector2(140f, 18f), 14, FontStyle.Bold, TextAnchor.MiddleLeft);
            optionBadge.color = new Color(0.87f, 0.76f, 0.45f, 0.98f);
            var optionLabel = CreateText("Label", optionRect, new Vector2(12f, -26f), new Vector2(486f, 20f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
            var optionRequirement = CreateText("Requirement", optionRect, new Vector2(12f, -44f), new Vector2(486f, 16f), 13, FontStyle.Normal, TextAnchor.MiddleLeft);
            optionRequirement.color = new Color(0.77f, 0.66f, 0.56f, 0.96f);
            EnableWrapping(optionRequirement);

            eventOptionButtons[i] = optionButton;
            eventOptionLabelTexts[i] = optionLabel;
            eventOptionRequirementTexts[i] = optionRequirement;
            eventOptionBadgeTexts[i] = optionBadge;
        }

        var confirmRect = CreatePanel("EventConfirm", eventOverlay, new Vector2(792f, -18f), new Vector2(302f, 48f), new Color(0.25f, 0.2f, 0.14f, 0.98f));
        SetTopLeft(confirmRect);
        var confirmImage = confirmRect.GetComponent<Image>();
        var confirmButton = confirmRect.gameObject.AddComponent<Button>();
        confirmButton.targetGraphic = confirmImage;
        ApplyOptionalSprite(confirmImage, PrimaryButtonArtPath);
        confirmRect.gameObject.SetActive(false);
        var confirmLabel = CreateText("EventConfirmLabel", confirmRect, Vector2.zero, new Vector2(302f, 48f), 18, FontStyle.Bold, TextAnchor.MiddleCenter);
        confirmLabel.text = "收拢结果";

        var expeditionView = canvasRoot.AddComponent<ExpeditionView>();
        expeditionView.titleText = titleText;
        expeditionView.heroNameText = heroNameText;
        expeditionView.heroStatsText = heroStatsText;
        expeditionView.expeditionStatsText = expeditionStatsText;
        expeditionView.phaseText = phaseText;
        expeditionView.roomTitleText = roomTitleText;
        expeditionView.roomDescriptionText = roomDescriptionText;
        expeditionView.roomPreviewImage = roomPreview.GetComponent<Image>();
        expeditionView.roomPreviewLabelText = roomPreviewLabel;
        expeditionView.loadoutText = loadoutText;
        expeditionView.heroPreviewImage = heroBanner.GetComponent<Image>();
        expeditionView.heroPreviewLabelText = heroPreviewLabel;
        expeditionView.enemyPreviewImage = enemyBanner.GetComponent<Image>();
        expeditionView.enemyPreviewLabelText = enemyPreviewLabel;
        expeditionView.enemyStatusText = enemyStatusText;
        expeditionView.logText = logText;
        expeditionView.skillText = skillText;
        expeditionView.hintText = hintText;
        expeditionView.actionButtons = buttons;
        expeditionView.actionLabels = buttonLabels;
        expeditionView.actionIconImages = buttonIcons;
        expeditionView.actionIconLabelTexts = buttonIconLabels;
        expeditionView.roomNodes = nodes;
        expeditionView.eventOverlayBlocker = eventOverlayBlocker.gameObject;
        expeditionView.eventOverlayRoot = eventOverlay.gameObject;
        expeditionView.eventBadgeText = eventBadge;
        expeditionView.eventTitleText = eventTitle;
        expeditionView.eventBodyText = eventBody;
        expeditionView.eventPreviewImage = eventPreview.GetComponent<Image>();
        expeditionView.eventPreviewLabelText = eventPreviewLabel;
        expeditionView.eventOptionButtons = eventOptionButtons;
        expeditionView.eventOptionLabelTexts = eventOptionLabelTexts;
        expeditionView.eventOptionRequirementTexts = eventOptionRequirementTexts;
        expeditionView.eventOptionBadgeTexts = eventOptionBadgeTexts;
        expeditionView.eventResultText = eventResultText;
        expeditionView.eventConfirmButton = confirmButton;
        expeditionView.eventConfirmLabelText = confirmLabel;
        return expeditionView;
    }

    private static ExpeditionRoomNodeView CreateRoomNode(RectTransform parent, Vector2 anchoredPosition)
    {
        var root = new GameObject("RoomNode", typeof(RectTransform), typeof(Image), typeof(ExpeditionRoomNodeView));
        root.layer = 5;
        root.transform.SetParent(parent, false);

        var rect = root.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(126f, 44f);
        SetTopLeft(rect);
        root.GetComponent<Image>().color = new Color(0.18f, 0.17f, 0.15f, 0.94f);
        ApplyOptionalSprite(root.GetComponent<Image>(), RoomNodeArtPath);

        var icon = CreateText("Icon", rect, new Vector2(10f, -8f), new Vector2(32f, 28f), 18, FontStyle.Bold, TextAnchor.MiddleCenter);
        var label = CreateText("Label", rect, new Vector2(46f, -10f), new Vector2(70f, 24f), 16, FontStyle.Bold, TextAnchor.MiddleLeft);
        var state = CreateText("State", rect, new Vector2(46f, -24f), new Vector2(70f, 18f), 13, FontStyle.Normal, TextAnchor.MiddleLeft);

        var view = root.GetComponent<ExpeditionRoomNodeView>();
        view.background = root.GetComponent<Image>();
        view.iconText = icon;
        view.labelText = label;
        view.stateText = state;
        return view;
    }

    private static RectTransform CreatePanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.layer = 5;
        panel.transform.SetParent(parent, false);

        var rect = panel.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        panel.GetComponent<Image>().color = color;
        return rect;
    }

    private static Text CreateText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, int fontSize, FontStyle fontStyle, TextAnchor alignment)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.layer = 5;
        textObject.transform.SetParent(parent, false);

        var rect = textObject.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        SetTopLeft(rect);

        var text = textObject.GetComponent<Text>();
        if (TMP_Settings.defaultFontAsset != null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }

        text.fontSize = fontSize;
        text.fontStyle = ConvertFontStyle(fontStyle);
        text.color = new Color(0.9f, 0.86f, 0.78f, 0.98f);
        text.alignment = ConvertAlignment(alignment);
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static void EnableWrapping(Text text)
    {
        if (text == null)
        {
            return;
        }

        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
    }

    private static FontStyles ConvertFontStyle(FontStyle style)
    {
        switch (style)
        {
            case FontStyle.Bold:
                return FontStyles.Bold;
            case FontStyle.Italic:
                return FontStyles.Italic;
            case FontStyle.BoldAndItalic:
                return FontStyles.Bold | FontStyles.Italic;
            default:
                return FontStyles.Normal;
        }
    }

    private static TextAlignmentOptions ConvertAlignment(TextAnchor alignment)
    {
        switch (alignment)
        {
            case TextAnchor.UpperLeft:
                return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter:
                return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight:
                return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft:
                return TextAlignmentOptions.MidlineLeft;
            case TextAnchor.MiddleCenter:
                return TextAlignmentOptions.Center;
            case TextAnchor.MiddleRight:
                return TextAlignmentOptions.MidlineRight;
            case TextAnchor.LowerLeft:
                return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter:
                return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight:
                return TextAlignmentOptions.BottomRight;
            default:
                return TextAlignmentOptions.TopLeft;
        }
    }

    private static void ApplyOptionalSprite(Image image, string assetPath, bool preserveAspect = false)
    {
        if (image == null || string.IsNullOrWhiteSpace(assetPath))
        {
            return;
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null)
        {
            return;
        }

        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = preserveAspect;
    }

    private static void AddOutline(RectTransform rect, Color color)
    {
        CreateEdge("Top", rect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 2f), color);
        CreateEdge("Bottom", rect, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 2f), color);
        CreateEdge("Left", rect, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(2f, 0f), color);
        CreateEdge("Right", rect, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(2f, 0f), color);
    }

    private static void CreateEdge(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Color color)
    {
        var edge = new GameObject(name, typeof(RectTransform), typeof(Image));
        edge.layer = 5;
        edge.transform.SetParent(parent, false);
        var rect = edge.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        edge.GetComponent<Image>().color = color;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }

    private static void SetTopLeft(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
    }
#endif
}
