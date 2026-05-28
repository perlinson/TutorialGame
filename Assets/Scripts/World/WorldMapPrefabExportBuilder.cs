#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Text = TMPro.TextMeshProUGUI;

public static class WorldMapPrefabExportBuilder
{
    private const string BackdropArtPath = "Assets/GameArt/Backgrounds/WorldMap/bg_worldmap_ink_scroll.png";
    private const string DetailPanelArtPath = "Assets/GameArt/UI/Panels/panel_worldmap_detail_ink.png";
    private const string RegionNodeArtPath = "Assets/GameArt/UI/Buttons/ui_node_region_ink.png";
    private const string PrimaryButtonArtPath = "Assets/GameArt/UI/Buttons/ui_btn_primary_gold.png";
    private const string SecondaryButtonArtPath = "Assets/GameArt/UI/Buttons/ui_btn_secondary_jade.png";
    private const string GameHubPanelPrefabPath = "Assets/Resources/UI/Game/GameHubPanel.prefab";
    private const string WorldMapRegionPanelPrefabPath = "Assets/Resources/UI/WorldMap/WorldMapRegionPanel.prefab";
    private static readonly Color AccentGold = new Color(0.68f, 0.57f, 0.3f, 0.92f);
    private static readonly Color PaperDark = new Color(0.08f, 0.1f, 0.12f, 0.84f);
    private static readonly Color PaperMid = new Color(0.09f, 0.1f, 0.11f, 0.94f);

    public static WorldMapController BuildPrefabExportController()
    {
        var regions = WorldRegionLibrary.GetRegions();

        var root = CreateUiObject("WorldMapRoot", null);
        root.layer = 5;
        Stretch(root.GetComponent<RectTransform>());

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        root.AddComponent<GraphicRaycaster>();
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var controller = root.AddComponent<WorldMapController>();

        var backdrop = CreateImage("Backdrop", root.transform, new Color(0.05f, 0.07f, 0.09f, 1f));
        Stretch(backdrop);
        ApplyOptionalSprite(backdrop.GetComponent<Image>(), BackdropArtPath, true);
        var mainLayer = CreateUiObject("MainLayer", root.transform).GetComponent<RectTransform>();
        Stretch(mainLayer);

        var mapScreen = CreateUiObject("MapScreen", mainLayer).GetComponent<RectTransform>();
        Stretch(mapScreen);

        var mapContentRoot = CreateUiObject("MapContentRoot", mapScreen).GetComponent<RectTransform>();
        mapContentRoot.anchorMin = new Vector2(0.5f, 0.5f);
        mapContentRoot.anchorMax = new Vector2(0.5f, 0.5f);
        mapContentRoot.pivot = new Vector2(0.5f, 0.5f);
        mapContentRoot.anchoredPosition = Vector2.zero;
        mapContentRoot.sizeDelta = new Vector2(1920f, 1080f);

        var titlePanel = CreatePanel("TitlePanel", mapContentRoot, new Vector2(70f, -50f), new Vector2(1780f, 146f), PaperDark);
        SetAnchors(titlePanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(titlePanel, AccentGold);
        CreateAccentStrip(titlePanel, 10f);
        var titleText = CreateText("Title", titlePanel, new Vector2(28f, -18f), new Vector2(460f, 42f), 36, FontStyle.Bold, TextAnchor.MiddleLeft);
        var heroSummary = CreateText("HeroSummary", titlePanel, new Vector2(28f, -62f), new Vector2(720f, 56f), 22, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(heroSummary);
        var resourceSummary = CreateText("ResourceSummary", titlePanel, new Vector2(860f, -26f), new Vector2(820f, 80f), 20, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(resourceSummary);
        var bagSummary = CreateText("BagSummary", titlePanel, new Vector2(860f, -108f), new Vector2(820f, 54f), 18, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(bagSummary);
        var bagButton = CreateButton("BagButton", titlePanel, new Vector2(1530f, -26f), new Vector2(110f, 42f), "储物袋", 18);
        var workshopButton = CreateButton("WorkshopButton", titlePanel, new Vector2(1658f, -26f), new Vector2(110f, 42f), "洞府整备", 18);

        var mapPanel = CreatePanel("MapPanel", mapContentRoot, new Vector2(70f, -230f), new Vector2(1240f, 780f), new Color(0.08f, 0.09f, 0.1f, 0.18f));
        SetAnchors(mapPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(mapPanel, new Color(0.56f, 0.47f, 0.28f, 0.44f));
        var mapField = CreateUiObject("MapField", mapPanel.transform).GetComponent<RectTransform>();
        SetAnchors(mapField, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        mapField.anchoredPosition = new Vector2(24f, -24f);
        mapField.sizeDelta = new Vector2(1192f, 732f);
        for (var i = 0; i < regions.Count; i++)
        {
            var region = regions[i];
            for (var unlockIndex = 0; unlockIndex < region.UnlockRegionIds.Length; unlockIndex++)
            {
                WorldRegionDefinition targetRegion;
                if (!WorldRegionLibrary.TryGetRegion(region.UnlockRegionIds[unlockIndex], out targetRegion))
                {
                    continue;
                }

                CreatePath(mapField, region.MapPosition, targetRegion.MapPosition, new Color(0.44f, 0.39f, 0.25f, 0.55f));
            }
        }

        var sectResidenceButton = CreateButton("SectResidenceButton", mapField, new Vector2(36f, -28f), new Vector2(180f, 76f), "青玄山门\n门派驻地", 18);

        for (var i = 0; i < regions.Count; i++)
        {
            CreateRegionNode(mapField, regions[i]);
        }

        var detailPanel = CreatePanel("DetailPanel", mapContentRoot, new Vector2(-70f, -230f), new Vector2(470f, 780f), PaperDark);
        SetAnchors(detailPanel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
        AddOutline(detailPanel, AccentGold);
        ApplyOptionalSprite(detailPanel.GetComponent<Image>(), DetailPanelArtPath);
        CreateAccentStrip(detailPanel, 10f);
        var regionTitle = CreateText("RegionTitle", detailPanel, new Vector2(30f, -24f), new Vector2(408f, 52f), 28, FontStyle.Bold, TextAnchor.UpperLeft);
        var regionPreview = CreateImage("RegionPreview", detailPanel, new Color(0.24f, 0.21f, 0.17f, 1f));
        SetAnchors(regionPreview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        regionPreview.anchoredPosition = new Vector2(30f, -92f);
        regionPreview.sizeDelta = new Vector2(410f, 144f);
        regionPreview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var regionPreviewLabel = CreateText("RegionPreviewLabel", detailPanel, new Vector2(30f, -150f), new Vector2(410f, 28f), 20, FontStyle.Bold, TextAnchor.MiddleCenter);
        var regionBody = CreateText("RegionBody", detailPanel, new Vector2(30f, -246f), new Vector2(410f, 146f), 19, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(regionBody);
        var regionStatus = CreateText("RegionStatus", detailPanel, new Vector2(30f, -402f), new Vector2(410f, 48f), 19, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(regionStatus);
        var taskPreview = CreateImage("TaskPreview", detailPanel, new Color(0.19f, 0.17f, 0.13f, 1f));
        SetAnchors(taskPreview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        taskPreview.anchoredPosition = new Vector2(30f, -470f);
        taskPreview.sizeDelta = new Vector2(410f, 84f);
        taskPreview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var taskPreviewLabel = CreateText("TaskPreviewLabel", detailPanel, new Vector2(30f, -500f), new Vector2(410f, 24f), 18, FontStyle.Bold, TextAnchor.MiddleCenter);
        var taskSummary = CreateText("TaskSummary", detailPanel, new Vector2(30f, -564f), new Vector2(410f, 88f), 17, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(taskSummary);

        var travelButton = CreateButton("TravelButton", detailPanel, new Vector2(30f, -680f), new Vector2(188f, 50f), "前往历练");
        var returnButton = CreateButton("ReturnButton", detailPanel, new Vector2(252f, -680f), new Vector2(188f, 50f), "返回主界面");
        var vitalityButton = CreateButton("VitalityButton", detailPanel, new Vector2(30f, -740f), new Vector2(188f, 40f), "温养护身器", 18);
        var attackButton = CreateButton("AttackButton", detailPanel, new Vector2(252f, -740f), new Vector2(188f, 40f), "祭炼主法器", 18);

        var hintPanel = CreatePanel("HintPanel", mapContentRoot, new Vector2(70f, 34f), new Vector2(1780f, 42f), new Color(0.07f, 0.08f, 0.09f, 0.64f));
        SetAnchors(hintPanel, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        CreateAccentStrip(hintPanel, 8f);
        var hintText = CreateText("Hint", hintPanel, new Vector2(18f, 0f), new Vector2(1720f, 44f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
        hintText.text = "山海录 / 正在整合地域情报";

        controller.titleText = titleText;
        controller.heroSummaryText = heroSummary;
        controller.resourceSummaryText = resourceSummary;
        controller.bagSummaryText = bagSummary;
        controller.regionTitleText = regionTitle;
        controller.regionBodyText = regionBody;
        controller.regionStatusText = regionStatus;
        controller.taskSummaryText = taskSummary;
        controller.taskPreviewImage = taskPreview.GetComponent<Image>();
        controller.taskPreviewLabelText = taskPreviewLabel;
        controller.regionPreviewImage = regionPreview.GetComponent<Image>();
        controller.regionPreviewLabelText = regionPreviewLabel;
        controller.hintText = hintText;
        controller.travelButton = travelButton;
        controller.bagButton = bagButton;
        controller.workshopButton = workshopButton;
        controller.sectResidenceButton = sectResidenceButton;
        controller.vitalityUpgradeButton = vitalityButton;
        controller.attackUpgradeButton = attackButton;
        controller.returnButton = returnButton;
        controller.mapScreen = mapScreen.gameObject;
        return controller;
    }

    public static GameHubPanel BuildHudPanelExport()
    {
        var existingPanel = ClonePrefabAsset<GameHubPanel>(GameHubPanelPrefabPath, "GameHubPanel");
        if (existingPanel != null)
        {
            return existingPanel;
        }

        var root = CreateUiObject("GameHubPanel", null);
        root.layer = 5;
        Stretch(root.GetComponent<RectTransform>());

        var panel = root.AddComponent<GameHubPanel>();
        panel.hudView = CreateWorldMapHud("TopHud", root.transform, new Vector2(36f, -28f));
        return panel;
    }

    public static WorldMapInventoryPanel BuildInventoryPanelExport()
    {
        var root = CreateUiObject("WorldMapInventoryPanel", null);
        root.layer = 5;
        Stretch(root.GetComponent<RectTransform>());

        var panel = root.AddComponent<WorldMapInventoryPanel>();

        var blocker = CreatePanel("Blocker", root.transform, Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.04f, 0.72f));
        Stretch(blocker);
        var blockerButton = blocker.gameObject.AddComponent<Button>();
        blockerButton.transition = Selectable.Transition.None;

        var window = CreatePanel("Window", root.transform, Vector2.zero, new Vector2(820f, 660f), PaperMid);
        SetAnchors(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        AddOutline(window, AccentGold);
        ApplyOptionalSprite(window.GetComponent<Image>(), DetailPanelArtPath);
        CreateAccentStrip(window, 10f);
        CreateText("InventoryTitle", window, new Vector2(28f, -22f), new Vector2(240f, 34f), 30, FontStyle.Bold, TextAnchor.MiddleLeft).text = "储物袋";
        var preview = CreateImage("InventoryPreview", window, new Color(0.22f, 0.18f, 0.14f, 1f));
        SetAnchors(preview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        preview.anchoredPosition = new Vector2(28f, -72f);
        preview.sizeDelta = new Vector2(764f, 136f);
        preview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var previewLabel = CreateText("InventoryPreviewLabel", window, new Vector2(28f, -126f), new Vector2(764f, 30f), 20, FontStyle.Bold, TextAnchor.MiddleCenter);
        var detail = CreateText("InventoryDetail", window, new Vector2(28f, -226f), new Vector2(764f, 346f), 20, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(detail);
        var closeButton = CreateButton("CloseInventoryButton", window, new Vector2(590f, -586f), new Vector2(190f, 46f), "收起储物袋", 18);

        panel.blockerButton = blockerButton;
        panel.closeButton = closeButton;
        panel.inventoryDetailText = detail;
        panel.previewImage = preview.GetComponent<Image>();
        panel.previewLabelText = previewLabel;
        panel.windowRect = window;
        return panel;
    }

    public static PlayerCompendiumPanel BuildCompendiumPanelExport()
    {
        var root = CreateUiObject("PlayerCompendiumPanel", null);
        root.layer = 5;
        Stretch(root.GetComponent<RectTransform>());

        var panel = root.AddComponent<PlayerCompendiumPanel>();

        var blocker = CreatePanel("Blocker", root.transform, Vector2.zero, Vector2.zero, new Color(0.02f, 0.05f, 0.06f, 0.86f));
        Stretch(blocker);

        var atmosphere = CreateImage("Atmosphere", root.transform, new Color(0.16f, 0.26f, 0.22f, 0.22f));
        Stretch(atmosphere);
        atmosphere.offsetMin = new Vector2(-120f, -80f);
        atmosphere.offsetMax = new Vector2(120f, 80f);

        var window = CreatePanel("Window", root.transform, Vector2.zero, new Vector2(1920f, 1080f), new Color(0.08f, 0.1f, 0.09f, 0.9f));
        SetAnchors(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        AddOutline(window, AccentGold);
        ApplyOptionalSprite(window.GetComponent<Image>(), DetailPanelArtPath);
        CreateAccentStrip(window, 12f);

        var panelTitle = CreateText("CompendiumTitle", window, new Vector2(36f, -24f), new Vector2(360f, 40f), 36, FontStyle.Bold, TextAnchor.MiddleLeft);
        panelTitle.text = "修士志";
        panelTitle.color = new Color(0.94f, 0.9f, 0.78f, 0.98f);
        var panelSubtitle = CreateText("CompendiumSubtitle", window, new Vector2(36f, -68f), new Vector2(420f, 24f), 18, FontStyle.Normal, TextAnchor.MiddleLeft);
        panelSubtitle.text = "观命火 / 察道途 / 整行藏";
        panelSubtitle.color = new Color(0.74f, 0.8f, 0.72f, 0.94f);
        var closeButton = CreateButton("CompendiumCloseButton", window, new Vector2(1656f, -26f), new Vector2(160f, 42f), "收起卷册", 18);

        var mainTabsPanel = CreatePanel("MainTabsPanel", window, new Vector2(36f, -114f), new Vector2(1780f, 78f), new Color(0.16f, 0.12f, 0.08f, 0.92f));
        SetAnchors(mainTabsPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(mainTabsPanel, new Color(0.42f, 0.56f, 0.48f, 0.72f));

        var mainTabButtons = new Button[4];
        for (var i = 0; i < mainTabButtons.Length; i++)
        {
            mainTabButtons[i] = CreateButton("MainTabButton" + i, mainTabsPanel, new Vector2(18f + i * 436f, -16f), new Vector2(408f, 46f), "主标签", 18);
        }

        var summaryPanel = CreatePanel("SummaryPanel", window, new Vector2(36f, -194f), new Vector2(560f, 848f), new Color(0.08f, 0.11f, 0.1f, 0.88f));
        SetAnchors(summaryPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(summaryPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("SummaryPanelTitle", summaryPanel, new Vector2(20f, -18f), new Vector2(220f, 26f), 24, FontStyle.Bold, TextAnchor.MiddleLeft).text = "命火与法相";
        var realmBadge = CreateText("RealmBadge", summaryPanel, new Vector2(302f, -20f), new Vector2(236f, 24f), 18, FontStyle.Bold, TextAnchor.MiddleRight);
        realmBadge.color = new Color(0.76f, 0.84f, 0.75f, 0.96f);

        var auraBack = CreateImage("AuraBack", summaryPanel, new Color(0.78f, 0.64f, 0.22f, 0.08f));
        SetAnchors(auraBack, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        auraBack.anchoredPosition = new Vector2(110f, -110f);
        auraBack.sizeDelta = new Vector2(340f, 340f);
        var auraFront = CreateImage("AuraFront", summaryPanel, new Color(0.4f, 0.67f, 0.56f, 0.06f));
        SetAnchors(auraFront, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        auraFront.anchoredPosition = new Vector2(136f, -136f);
        auraFront.sizeDelta = new Vector2(288f, 288f);

        var preview = CreateImage("CompendiumPreview", summaryPanel, new Color(0.18f, 0.2f, 0.16f, 1f));
        SetAnchors(preview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        preview.anchoredPosition = new Vector2(104f, -108f);
        preview.sizeDelta = new Vector2(352f, 436f);
        var previewImage = preview.GetComponent<Image>();
        previewImage.sprite = GameSpriteLibrary.WhiteSquareSprite;
        previewImage.preserveAspect = true;
        var previewLabel = CreateText("CompendiumPreviewLabel", summaryPanel, new Vector2(94f, -566f), new Vector2(372f, 28f), 18, FontStyle.Bold, TextAnchor.MiddleCenter);
        previewLabel.color = new Color(0.86f, 0.82f, 0.72f, 0.96f);

        var vitalityMeterRoot = CreatePanel("VitalityMeterRoot", summaryPanel, new Vector2(22f, -142f), new Vector2(62f, 196f), new Color(0.16f, 0.11f, 0.08f, 0.86f));
        SetAnchors(vitalityMeterRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(vitalityMeterRoot, new Color(0.66f, 0.48f, 0.22f, 0.72f));
        var vitalityMeterFill = CreateImage("Fill", vitalityMeterRoot, new Color(0.88f, 0.56f, 0.24f, 0.98f));
        Stretch(vitalityMeterFill);
        vitalityMeterFill.offsetMin = new Vector2(8f, 8f);
        vitalityMeterFill.offsetMax = new Vector2(-8f, -8f);
        var vitalityFillImage = vitalityMeterFill.GetComponent<Image>();
        vitalityFillImage.sprite = GameSpriteLibrary.WhiteSquareSprite;
        vitalityFillImage.type = Image.Type.Filled;
        vitalityFillImage.fillMethod = Image.FillMethod.Vertical;
        vitalityFillImage.fillOrigin = (int) Image.OriginVertical.Bottom;
        vitalityFillImage.fillAmount = 1f;
        var vitalityMeterText = CreateText("VitalityMeterText", summaryPanel, new Vector2(18f, -350f), new Vector2(72f, 62f), 14, FontStyle.Bold, TextAnchor.UpperLeft);
        vitalityMeterText.color = new Color(0.93f, 0.87f, 0.78f, 0.98f);
        EnableWrapping(vitalityMeterText);

        var qiMeterRoot = CreatePanel("QiMeterRoot", summaryPanel, new Vector2(22f, -392f), new Vector2(62f, 196f), new Color(0.09f, 0.14f, 0.16f, 0.86f));
        SetAnchors(qiMeterRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(qiMeterRoot, new Color(0.38f, 0.66f, 0.64f, 0.72f));
        var qiMeterFill = CreateImage("Fill", qiMeterRoot, new Color(0.3f, 0.82f, 0.88f, 0.96f));
        Stretch(qiMeterFill);
        qiMeterFill.offsetMin = new Vector2(8f, 8f);
        qiMeterFill.offsetMax = new Vector2(-8f, -8f);
        var qiFillImage = qiMeterFill.GetComponent<Image>();
        qiFillImage.sprite = GameSpriteLibrary.WhiteSquareSprite;
        qiFillImage.type = Image.Type.Filled;
        qiFillImage.fillMethod = Image.FillMethod.Vertical;
        qiFillImage.fillOrigin = (int) Image.OriginVertical.Bottom;
        qiFillImage.fillAmount = 0.42f;
        var qiMeterText = CreateText("QiMeterText", summaryPanel, new Vector2(18f, -600f), new Vector2(72f, 62f), 14, FontStyle.Bold, TextAnchor.UpperLeft);
        qiMeterText.color = new Color(0.82f, 0.92f, 0.9f, 0.98f);
        EnableWrapping(qiMeterText);

        var summaryText = CreateScrollTextArea("CompendiumSummaryScroll", summaryPanel, new Vector2(18f, -632f), new Vector2(524f, 186f), 17);

        var sectionPanel = CreatePanel("SectionPanel", window, new Vector2(622f, -194f), new Vector2(238f, 848f), new Color(0.1f, 0.11f, 0.1f, 0.88f));
        SetAnchors(sectionPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(sectionPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("SectionPanelTitle", sectionPanel, new Vector2(18f, -18f), new Vector2(180f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "分页索引";

        var sectionTabButtons = new Button[4];
        for (var i = 0; i < sectionTabButtons.Length; i++)
        {
            sectionTabButtons[i] = CreateButton("SectionTabButton" + i, sectionPanel, new Vector2(18f, -72f - i * 64f), new Vector2(202f, 48f), "纵向标签", 17);
        }

        var contentPanel = CreatePanel("ContentPanel", window, new Vector2(886f, -194f), new Vector2(930f, 848f), new Color(0.08f, 0.11f, 0.1f, 0.9f));
        SetAnchors(contentPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(contentPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        var contentTitle = CreateText("ContentTitle", contentPanel, new Vector2(24f, -18f), new Vector2(520f, 28f), 26, FontStyle.Bold, TextAnchor.MiddleLeft);
        contentTitle.text = "条目内容";

        var characterOverviewRoot = CreateUiObject("CharacterOverviewRoot", contentPanel).GetComponent<RectTransform>();
        SetAnchors(characterOverviewRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        characterOverviewRoot.anchoredPosition = new Vector2(0f, 0f);
        characterOverviewRoot.sizeDelta = new Vector2(930f, 848f);

        var overviewNarrativePanel = CreatePanel("OverviewNarrativePanel", characterOverviewRoot, new Vector2(20f, -64f), new Vector2(890f, 176f), new Color(0.12f, 0.13f, 0.11f, 0.78f));
        SetAnchors(overviewNarrativePanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(overviewNarrativePanel, new Color(0.4f, 0.34f, 0.22f, 0.72f));
        CreateText("OverviewNarrativeTitle", overviewNarrativePanel, new Vector2(20f, -18f), new Vector2(220f, 24f), 20, FontStyle.Bold, TextAnchor.MiddleLeft).text = "道途札记";
        var characterNarrative = CreateText("OverviewNarrativeText", overviewNarrativePanel, new Vector2(20f, -56f), new Vector2(850f, 96f), 17, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(characterNarrative);

        var overviewStatsPanel = CreatePanel("OverviewStatsPanel", characterOverviewRoot, new Vector2(20f, -262f), new Vector2(438f, 212f), new Color(0.11f, 0.14f, 0.13f, 0.8f));
        SetAnchors(overviewStatsPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(overviewStatsPanel, new Color(0.36f, 0.6f, 0.54f, 0.68f));
        CreateText("OverviewStatsTitle", overviewStatsPanel, new Vector2(20f, -18f), new Vector2(220f, 24f), 20, FontStyle.Bold, TextAnchor.MiddleLeft).text = "基础属性";
        var characterPrimaryStats = CreateText("PrimaryStatsText", overviewStatsPanel, new Vector2(20f, -58f), new Vector2(188f, 128f), 18, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(characterPrimaryStats);
        var characterSecondaryStats = CreateText("SecondaryStatsText", overviewStatsPanel, new Vector2(226f, -58f), new Vector2(188f, 128f), 18, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(characterSecondaryStats);

        var overviewGrowthPanel = CreatePanel("OverviewGrowthPanel", characterOverviewRoot, new Vector2(474f, -262f), new Vector2(436f, 132f), new Color(0.14f, 0.12f, 0.1f, 0.8f));
        SetAnchors(overviewGrowthPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(overviewGrowthPanel, new Color(0.54f, 0.44f, 0.24f, 0.68f));
        CreateText("OverviewGrowthTitle", overviewGrowthPanel, new Vector2(18f, -16f), new Vector2(220f, 24f), 19, FontStyle.Bold, TextAnchor.MiddleLeft).text = "成长状态";
        var characterGrowthStatus = CreateText("GrowthStatusText", overviewGrowthPanel, new Vector2(18f, -48f), new Vector2(400f, 72f), 17, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(characterGrowthStatus);

        var overviewSectPanel = CreatePanel("OverviewSectPanel", characterOverviewRoot, new Vector2(474f, -406f), new Vector2(436f, 132f), new Color(0.1f, 0.13f, 0.14f, 0.8f));
        SetAnchors(overviewSectPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(overviewSectPanel, new Color(0.32f, 0.58f, 0.62f, 0.68f));
        CreateText("OverviewSectTitle", overviewSectPanel, new Vector2(18f, -16f), new Vector2(220f, 24f), 19, FontStyle.Bold, TextAnchor.MiddleLeft).text = "宗门与行藏";
        var characterSectStatus = CreateText("SectStatusText", overviewSectPanel, new Vector2(18f, -48f), new Vector2(400f, 72f), 17, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(characterSectStatus);

        var overviewSealPanel = CreatePanel("OverviewSealPanel", characterOverviewRoot, new Vector2(20f, -554f), new Vector2(890f, 250f), new Color(0.11f, 0.11f, 0.1f, 0.82f));
        SetAnchors(overviewSealPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(overviewSealPanel, new Color(0.44f, 0.37f, 0.22f, 0.72f));
        CreateText("OverviewSealTitle", overviewSealPanel, new Vector2(20f, -18f), new Vector2(220f, 24f), 20, FontStyle.Bold, TextAnchor.MiddleLeft).text = "修行根基";

        var sealViews = new PlayerCompendiumSealView[5];
        for (var i = 0; i < sealViews.Length; i++)
        {
            var sealRoot = CreatePanel("Seal" + i, overviewSealPanel, new Vector2(18f + i * 174f, -64f), new Vector2(162f, 160f), new Color(0.22f, 0.18f, 0.12f, 0.94f));
            SetAnchors(sealRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            AddOutline(sealRoot, new Color(0.54f, 0.44f, 0.22f, 0.7f));
            var sealShort = CreateText("ShortLabel", sealRoot, new Vector2(16f, -16f), new Vector2(40f, 32f), 24, FontStyle.Bold, TextAnchor.MiddleLeft);
            sealShort.color = new Color(0.94f, 0.88f, 0.72f, 0.98f);
            var sealTitle = CreateText("Title", sealRoot, new Vector2(16f, -54f), new Vector2(128f, 22f), 16, FontStyle.Bold, TextAnchor.MiddleLeft);
            sealTitle.color = new Color(0.78f, 0.84f, 0.74f, 0.94f);
            var sealValue = CreateText("Value", sealRoot, new Vector2(16f, -94f), new Vector2(128f, 34f), 20, FontStyle.Bold, TextAnchor.MiddleLeft);
            sealValue.color = new Color(0.92f, 0.88f, 0.8f, 0.98f);
            sealViews[i] = new PlayerCompendiumSealView
            {
                backgroundImage = sealRoot.GetComponent<Image>(),
                shortLabelText = sealShort,
                titleText = sealTitle,
                valueText = sealValue
            };
        }

        var visualPanel = CreatePanel("VisualPanel", contentPanel, new Vector2(20f, -64f), new Vector2(890f, 298f), new Color(0.11f, 0.13f, 0.11f, 0.78f));
        SetAnchors(visualPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(visualPanel, new Color(0.4f, 0.34f, 0.22f, 0.72f));
        var visualTitle = CreateText("VisualTitle", visualPanel, new Vector2(18f, -16f), new Vector2(280f, 24f), 20, FontStyle.Bold, TextAnchor.MiddleLeft);
        visualTitle.text = "技艺节点";

        var visualNodeViews = new PlayerCompendiumNodeView[4];
        var nodeWidth = 212f;
        var nodeHeight = 198f;
        var nodeGap = 12f;
        for (var i = 0; i < visualNodeViews.Length; i++)
        {
            var nodeRoot = CreatePanel("VisualNode" + i, visualPanel, new Vector2(18f + i * (nodeWidth + nodeGap), -56f), new Vector2(nodeWidth, nodeHeight), new Color(0.18f, 0.16f, 0.12f, 0.94f));
            SetAnchors(nodeRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            AddOutline(nodeRoot, new Color(0.56f, 0.46f, 0.24f, 0.82f));
            var nodeView = nodeRoot.gameObject.AddComponent<PlayerCompendiumNodeView>();
            nodeView.backgroundImage = nodeRoot.GetComponent<Image>();
            var accent = CreateImage("Accent", nodeRoot, AccentGold);
            SetAnchors(accent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
            accent.anchoredPosition = new Vector2(0f, -2f);
            accent.sizeDelta = new Vector2(0f, 8f);
            nodeView.accentImage = accent.GetComponent<Image>();
            nodeView.titleText = CreateText("Title", nodeRoot, new Vector2(12f, -18f), new Vector2(nodeWidth - 24f, 24f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
            nodeView.subtitleText = CreateText("Subtitle", nodeRoot, new Vector2(12f, -48f), new Vector2(nodeWidth - 24f, 22f), 13, FontStyle.Normal, TextAnchor.MiddleLeft);
            nodeView.stateText = CreateText("State", nodeRoot, new Vector2(12f, -76f), new Vector2(nodeWidth - 24f, 22f), 14, FontStyle.Bold, TextAnchor.MiddleLeft);
            nodeView.descriptionText = CreateText("Description", nodeRoot, new Vector2(12f, -102f), new Vector2(nodeWidth - 24f, 82f), 13, FontStyle.Normal, TextAnchor.UpperLeft);
            EnableWrapping(nodeView.descriptionText);
            visualNodeViews[i] = nodeView;

            if (i < visualNodeViews.Length - 1)
            {
                var edge = CreateImage("Connector" + i, visualPanel, new Color(0.5f, 0.44f, 0.26f, 0.62f));
                SetAnchors(edge, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
                edge.anchoredPosition = new Vector2(18f + nodeWidth + i * (nodeWidth + nodeGap), -154f);
                edge.sizeDelta = new Vector2(nodeGap, 4f);
            }
        }

        var contentBody = CreateScrollTextArea("ContentBodyScroll", contentPanel, new Vector2(20f, -384f), new Vector2(890f, 430f), 18);

        panel.closeButton = closeButton;
        panel.mainTabButtons = mainTabButtons;
        panel.sectionTabButtons = sectionTabButtons;
        panel.panelTitleText = panelTitle;
        panel.panelSubtitleText = panelSubtitle;
        panel.summaryText = summaryText;
        panel.contentTitleText = contentTitle;
        panel.contentBodyText = contentBody;
        panel.contentBodyRoot = contentBody.rectTransform.parent.parent.parent.gameObject;
        panel.visualNodeRoot = visualPanel.gameObject;
        panel.visualTitleText = visualTitle;
        panel.visualNodeViews = visualNodeViews;
        panel.previewImage = previewImage;
        panel.previewLabelText = previewLabel;
        panel.realmBadgeText = realmBadge;
        panel.vitalityMeterFillImage = vitalityFillImage;
        panel.vitalityMeterText = vitalityMeterText;
        panel.qiMeterFillImage = qiFillImage;
        panel.qiMeterText = qiMeterText;
        panel.characterOverviewRoot = characterOverviewRoot.gameObject;
        panel.characterNarrativeText = characterNarrative;
        panel.characterPrimaryStatsText = characterPrimaryStats;
        panel.characterSecondaryStatsText = characterSecondaryStats;
        panel.characterGrowthStatusText = characterGrowthStatus;
        panel.characterSectStatusText = characterSectStatus;
        panel.characterSealViews = sealViews;
        panel.windowRect = window;
        return panel;
    }

    public static WorldMapWorkshopPanel BuildWorkshopPanelExport()
    {
        var root = CreateUiObject("WorldMapWorkshopPanel", null);
        root.layer = 5;
        Stretch(root.GetComponent<RectTransform>());

        var panel = root.AddComponent<WorldMapWorkshopPanel>();

        var blocker = CreatePanel("Blocker", root.transform, Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.04f, 0.72f));
        Stretch(blocker);
        var blockerButton = blocker.gameObject.AddComponent<Button>();
        blockerButton.transition = Selectable.Transition.None;

        var window = CreatePanel("Window", root.transform, Vector2.zero, new Vector2(920f, 680f), PaperMid);
        SetAnchors(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        AddOutline(window, AccentGold);
        ApplyOptionalSprite(window.GetComponent<Image>(), DetailPanelArtPath);
        CreateAccentStrip(window, 10f);
        CreateText("WorkshopTitle", window, new Vector2(28f, -22f), new Vector2(300f, 34f), 30, FontStyle.Bold, TextAnchor.MiddleLeft).text = "洞府整备";
        var preview = CreateImage("WorkshopPreview", window, new Color(0.18f, 0.2f, 0.16f, 1f));
        SetAnchors(preview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        preview.anchoredPosition = new Vector2(28f, -72f);
        preview.sizeDelta = new Vector2(864f, 148f);
        preview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var previewLabel = CreateText("WorkshopPreviewLabel", window, new Vector2(28f, -132f), new Vector2(864f, 30f), 20, FontStyle.Bold, TextAnchor.MiddleCenter);
        var summary = CreateText("WorkshopSummary", window, new Vector2(28f, -246f), new Vector2(864f, 246f), 19, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(summary);
        var craftQiButton = CreateButton("CraftQiButton", window, new Vector2(28f, -536f), new Vector2(410f, 48f), "丹炉养火", 18);
        var craftBagButton = CreateButton("CraftBagButton", window, new Vector2(482f, -536f), new Vector2(410f, 48f), "符匣拓纹", 18);
        var craftVitalityButton = CreateButton("CraftVitalityButton", window, new Vector2(28f, -594f), new Vector2(410f, 48f), "培元散", 18);
        var craftAttackButton = CreateButton("CraftAttackButton", window, new Vector2(482f, -594f), new Vector2(410f, 48f), "纳物符袋", 18);
        var closeButton = CreateButton("CloseWorkshopButton", window, new Vector2(702f, -28f), new Vector2(190f, 40f), "收起整备", 18);

        panel.blockerButton = blockerButton;
        panel.closeButton = closeButton;
        panel.craftQiButton = craftQiButton;
        panel.craftBagButton = craftBagButton;
        panel.craftVitalityButton = craftVitalityButton;
        panel.craftAttackButton = craftAttackButton;
        panel.summaryText = summary;
        panel.previewImage = preview.GetComponent<Image>();
        panel.previewLabelText = previewLabel;
        panel.windowRect = window;
        return panel;
    }

    public static WorldMapSectResidencePanel BuildSectResidencePanelExport()
    {
        var root = CreateUiObject("WorldMapSectResidencePanel", null);
        root.layer = 5;
        Stretch(root.GetComponent<RectTransform>());

        var panel = root.AddComponent<WorldMapSectResidencePanel>();

        var atmosphere = CreateImage("SectAtmosphere", root.transform, new Color(0.18f, 0.14f, 0.1f, 0.22f));
        Stretch(atmosphere);
        atmosphere.offsetMin = new Vector2(-80f, -40f);
        atmosphere.offsetMax = new Vector2(80f, 40f);

        var window = CreatePanel("Window", root.transform, Vector2.zero, new Vector2(1920f, 1080f), PaperMid);
        SetAnchors(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        AddOutline(window, AccentGold);
        ApplyOptionalSprite(window.GetComponent<Image>(), DetailPanelArtPath);
        CreateAccentStrip(window, 10f);
        var panelTitle = CreateText("SectPanelTitle", window, new Vector2(360f, -26f), new Vector2(420f, 40f), 32, FontStyle.Bold, TextAnchor.MiddleLeft);
        panelTitle.text = "青玄山门";
        var panelSubtitle = CreateText("SectCaption", window, new Vector2(360f, -68f), new Vector2(640f, 24f), 18, FontStyle.Normal, TextAnchor.MiddleLeft);
        panelSubtitle.text = "门派驻地 / 殿堂事务 / 洞府整备";
        var dialogueButton = CreateButton("OpenNpcDialogueButton", window, new Vector2(1338f, -28f), new Vector2(190f, 42f), "同门人物", 18);
        var closeButton = CreateButton("CloseSectButton", window, new Vector2(1554f, -28f), new Vector2(190f, 42f), "离开门派", 18);

        var hallColumn = CreatePanel("HallColumn", window, new Vector2(36f, -268f), new Vector2(292f, 728f), new Color(0.09f, 0.1f, 0.11f, 0.86f));
        SetAnchors(hallColumn, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(hallColumn, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("HallColumnTitle", hallColumn, new Vector2(20f, -18f), new Vector2(220f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "殿堂总览";
        var hallButton0 = CreateButton("SectHallButton0", hallColumn, new Vector2(20f, -70f), new Vector2(240f, 54f), "勤功殿", 18);
        var hallButton1 = CreateButton("SectHallButton1", hallColumn, new Vector2(20f, -134f), new Vector2(240f, 54f), "炼器殿", 18);
        var hallButton2 = CreateButton("SectHallButton2", hallColumn, new Vector2(20f, -198f), new Vector2(240f, 54f), "丹鼎殿", 18);
        var hallButton3 = CreateButton("SectHallButton3", hallColumn, new Vector2(20f, -262f), new Vector2(240f, 54f), "符阵殿", 18);
        var hallButton4 = CreateButton("SectHallButton4", hallColumn, new Vector2(20f, -326f), new Vector2(240f, 54f), "藏经阁", 18);
        var hallButton5 = CreateButton("SectHallButton5", hallColumn, new Vector2(20f, -390f), new Vector2(240f, 54f), "庶务堂", 18);
        var hallButton6 = CreateButton("SectHallButton6", hallColumn, new Vector2(20f, -454f), new Vector2(240f, 54f), "我的洞府", 18);

        var preview = CreateImage("SectPreview", window, new Color(0.24f, 0.18f, 0.12f, 1f));
        SetAnchors(preview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        preview.anchoredPosition = new Vector2(360f, -116f);
        preview.sizeDelta = new Vector2(1524f, 236f);
        preview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var previewLabel = CreateText("SectPreviewLabel", window, new Vector2(360f, -214f), new Vector2(1524f, 30f), 20, FontStyle.Bold, TextAnchor.MiddleCenter);
        var hallTitle = CreateText("SectTitle", window, new Vector2(360f, -392f), new Vector2(1024f, 36f), 30, FontStyle.Bold, TextAnchor.MiddleLeft);
        var description = CreateText("SectDescription", window, new Vector2(360f, -440f), new Vector2(1024f, 90f), 18, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(description);
        var detailPanel = CreatePanel("SectDetailPanel", window, new Vector2(360f, -548f), new Vector2(1024f, 396f), new Color(0.09f, 0.1f, 0.11f, 0.88f));
        SetAnchors(detailPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(detailPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("SectDetailTitle", detailPanel, new Vector2(20f, -18f), new Vector2(220f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "殿堂纪要";
        var status = CreateScrollTextArea("SectStatusScroll", detailPanel, new Vector2(18f, -56f), new Vector2(988f, 320f), 18);

        var actionPanel = CreatePanel("SectActionPanel", window, new Vector2(1420f, -392f), new Vector2(464f, 552f), new Color(0.09f, 0.1f, 0.11f, 0.88f));
        SetAnchors(actionPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(actionPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("SectActionTitle", actionPanel, new Vector2(20f, -18f), new Vector2(220f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "当前可办事务";
        var actionButton0 = CreateButton("SectActionButton0", actionPanel, new Vector2(20f, -74f), new Vector2(416f, 56f), "殿堂事务", 18);
        var actionButton1 = CreateButton("SectActionButton1", actionPanel, new Vector2(20f, -144f), new Vector2(416f, 56f), "殿堂事务", 18);
        var actionButton2 = CreateButton("SectActionButton2", actionPanel, new Vector2(20f, -214f), new Vector2(416f, 56f), "殿堂事务", 18);
        var actionButton3 = CreateButton("SectActionButton3", actionPanel, new Vector2(20f, -284f), new Vector2(416f, 56f), "殿堂事务", 18);
        CreateText("SectLocationTitle", actionPanel, new Vector2(20f, -358f), new Vector2(220f, 24f), 20, FontStyle.Bold, TextAnchor.MiddleLeft).text = "殿堂支点";
        var locationButton0 = CreateButton("SectLocationButton0", actionPanel, new Vector2(20f, -396f), new Vector2(416f, 58f), "别院 / 偏室", 17);
        var locationButton1 = CreateButton("SectLocationButton1", actionPanel, new Vector2(20f, -466f), new Vector2(416f, 58f), "别院 / 偏室", 17);
        var locationButton2 = CreateButton("SectLocationButton2", actionPanel, new Vector2(20f, -536f), new Vector2(416f, 58f), "别院 / 偏室", 17);
        actionPanel.sizeDelta = new Vector2(464f, 620f);

        panel.closeButton = closeButton;
        panel.dialogueButton = dialogueButton;
        panel.hallButtons = new[] { hallButton0, hallButton1, hallButton2, hallButton3, hallButton4, hallButton5, hallButton6 };
        panel.actionButtons = new[] { actionButton0, actionButton1, actionButton2, actionButton3 };
        panel.panelTitleText = panelTitle;
        panel.panelSubtitleText = panelSubtitle;
        panel.hallTitleText = hallTitle;
        panel.descriptionText = description;
        panel.statusText = status;
        panel.previewImage = preview.GetComponent<Image>();
        panel.previewLabelText = previewLabel;
        panel.locationButtons = new[] { locationButton0, locationButton1, locationButton2 };
        panel.windowRect = window;
        return panel;
    }

    public static WorldMapNpcDialoguePanel BuildNpcDialoguePanelExport()
    {
        var root = CreateUiObject("WorldMapNpcDialoguePanel", null);
        root.layer = 5;
        Stretch(root.GetComponent<RectTransform>());

        var panel = root.AddComponent<WorldMapNpcDialoguePanel>();

        var blocker = CreatePanel("Blocker", root.transform, Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.04f, 0.82f));
        Stretch(blocker);
        var blockerButton = blocker.gameObject.AddComponent<Button>();
        blockerButton.transition = Selectable.Transition.None;

        var window = CreatePanel("Window", root.transform, Vector2.zero, new Vector2(1920f, 1080f), PaperMid);
        SetAnchors(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        AddOutline(window, AccentGold);
        ApplyOptionalSprite(window.GetComponent<Image>(), DetailPanelArtPath);
        CreateAccentStrip(window, 10f);

        var panelTitle = CreateText("NpcPanelTitle", window, new Vector2(360f, -26f), new Vector2(460f, 40f), 32, FontStyle.Bold, TextAnchor.MiddleLeft);
        panelTitle.text = "人物与对话";
        var panelSubtitle = CreateText("NpcPanelSubtitle", window, new Vector2(360f, -68f), new Vector2(680f, 24f), 18, FontStyle.Normal, TextAnchor.MiddleLeft);
        panelSubtitle.text = "人物 / 委托 / 剧情 / 线索";
        var closeButton = CreateButton("CloseNpcDialogueButton", window, new Vector2(1604f, -28f), new Vector2(200f, 42f), "收起话题", 18);

        var rosterPanel = CreatePanel("NpcRosterPanel", window, new Vector2(36f, -126f), new Vector2(292f, 838f), new Color(0.09f, 0.1f, 0.11f, 0.88f));
        SetAnchors(rosterPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(rosterPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("NpcRosterTitle", rosterPanel, new Vector2(20f, -18f), new Vector2(220f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "当前人物";
        var entryButtons = new Button[6];
        for (var i = 0; i < entryButtons.Length; i++)
        {
            entryButtons[i] = CreateButton("NpcEntryButton" + i, rosterPanel, new Vector2(20f, -72f - i * 108f), new Vector2(252f, 92f), "人物", 18);
        }
        CreateText("NpcIncidentTitle", rosterPanel, new Vector2(20f, -714f), new Vector2(220f, 22f), 18, FontStyle.Bold, TextAnchor.MiddleLeft).text = "当前风闻";
        var incidentButtons = new Button[2];
        for (var i = 0; i < incidentButtons.Length; i++)
        {
            incidentButtons[i] = CreateButton("NpcIncidentButton" + i, rosterPanel, new Vector2(20f, -744f - i * 48f), new Vector2(252f, 40f), "风闻入口", 15);
        }

        var preview = CreateImage("NpcPreview", window, new Color(0.2f, 0.18f, 0.13f, 1f));
        SetAnchors(preview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        preview.anchoredPosition = new Vector2(360f, -126f);
        preview.sizeDelta = new Vector2(520f, 236f);
        preview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var previewLabel = CreateText("NpcPreviewLabel", window, new Vector2(360f, -226f), new Vector2(520f, 30f), 22, FontStyle.Bold, TextAnchor.MiddleCenter);

        var infoPanel = CreatePanel("NpcInfoPanel", window, new Vector2(910f, -126f), new Vector2(974f, 236f), new Color(0.09f, 0.1f, 0.11f, 0.88f));
        SetAnchors(infoPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(infoPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        var npcTitle = CreateText("NpcTitle", infoPanel, new Vector2(20f, -16f), new Vector2(540f, 32f), 28, FontStyle.Bold, TextAnchor.MiddleLeft);
        var npcSubtitle = CreateText("NpcSubtitle", infoPanel, new Vector2(20f, -52f), new Vector2(540f, 24f), 18, FontStyle.Normal, TextAnchor.MiddleLeft);
        var npcStatus = CreateText("NpcStatus", infoPanel, new Vector2(20f, -88f), new Vector2(934f, 112f), 18, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(npcStatus);

        var descriptionPanel = CreatePanel("NpcDescriptionPanel", window, new Vector2(360f, -392f), new Vector2(844f, 380f), new Color(0.09f, 0.1f, 0.11f, 0.88f));
        SetAnchors(descriptionPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(descriptionPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("NpcDescriptionTitle", descriptionPanel, new Vector2(20f, -18f), new Vector2(240f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "交谈记录";
        var npcDescription = CreateScrollTextArea("NpcDescriptionScroll", descriptionPanel, new Vector2(20f, -58f), new Vector2(804f, 296f), 18);

        var choicePanel = CreatePanel("NpcChoicePanel", window, new Vector2(1236f, -392f), new Vector2(648f, 380f), new Color(0.09f, 0.1f, 0.11f, 0.88f));
        SetAnchors(choicePanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(choicePanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("NpcChoiceTitle", choicePanel, new Vector2(20f, -18f), new Vector2(220f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "当前可谈";
        var choiceButtons = new Button[4];
        for (var i = 0; i < choiceButtons.Length; i++)
        {
            choiceButtons[i] = CreateButton("NpcChoiceButton" + i, choicePanel, new Vector2(20f, -72f - i * 74f), new Vector2(608f, 56f), "对话选项", 18);
        }

        var storyPanel = CreatePanel("NpcStoryPanel", window, new Vector2(360f, -800f), new Vector2(752f, 232f), new Color(0.09f, 0.1f, 0.11f, 0.88f));
        SetAnchors(storyPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(storyPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("NpcStoryTitle", storyPanel, new Vector2(20f, -18f), new Vector2(220f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "剧情回响";
        var storySummary = CreateScrollTextArea("NpcStorySummaryScroll", storyPanel, new Vector2(20f, -58f), new Vector2(712f, 148f), 17);

        var taskPanel = CreatePanel("NpcTaskPanel", window, new Vector2(1140f, -800f), new Vector2(744f, 232f), new Color(0.09f, 0.1f, 0.11f, 0.88f));
        SetAnchors(taskPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(taskPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("NpcTaskTitle", taskPanel, new Vector2(20f, -18f), new Vector2(220f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "当前委托";
        var taskSummary = CreateScrollTextArea("NpcTaskSummaryScroll", taskPanel, new Vector2(20f, -58f), new Vector2(704f, 148f), 17);

        panel.blockerButton = blockerButton;
        panel.closeButton = closeButton;
        panel.entryButtons = entryButtons;
        panel.incidentButtons = incidentButtons;
        panel.choiceButtons = choiceButtons;
        panel.panelTitleText = panelTitle;
        panel.panelSubtitleText = panelSubtitle;
        panel.storySummaryText = storySummary;
        panel.taskSummaryText = taskSummary;
        panel.npcTitleText = npcTitle;
        panel.npcSubtitleText = npcSubtitle;
        panel.npcDescriptionText = npcDescription;
        panel.npcStatusText = npcStatus;
        panel.previewImage = preview.GetComponent<Image>();
        panel.previewLabelText = previewLabel;
        panel.windowRect = window;
        return panel;
    }

    public static WorldMapSettlementPanel BuildSettlementPanelExport()
    {
        var root = CreateUiObject("WorldMapSettlementPanel", null);
        root.layer = 5;
        Stretch(root.GetComponent<RectTransform>());

        var panel = root.AddComponent<WorldMapSettlementPanel>();

        var atmosphere = CreateImage("SettlementAtmosphere", root.transform, new Color(0.18f, 0.16f, 0.1f, 0.22f));
        Stretch(atmosphere);
        atmosphere.offsetMin = new Vector2(-80f, -40f);
        atmosphere.offsetMax = new Vector2(80f, 40f);

        var window = CreatePanel("Window", root.transform, Vector2.zero, new Vector2(1920f, 1080f), PaperMid);
        SetAnchors(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        AddOutline(window, AccentGold);
        ApplyOptionalSprite(window.GetComponent<Image>(), DetailPanelArtPath);
        CreateAccentStrip(window, 10f);
        var panelTitle = CreateText("SettlementPanelTitle", window, new Vector2(360f, -26f), new Vector2(420f, 40f), 32, FontStyle.Bold, TextAnchor.MiddleLeft);
        panelTitle.text = "山门外坊市";
        var panelSubtitle = CreateText("SettlementCaption", window, new Vector2(360f, -68f), new Vector2(640f, 24f), 18, FontStyle.Normal, TextAnchor.MiddleLeft);
        panelSubtitle.text = "洞府整备 / 储物 / 炼制 / 法器养成";
        var dialogueButton = CreateButton("OpenSettlementNpcButton", window, new Vector2(1364f, -28f), new Vector2(210f, 42f), "坊市人物", 18);
        var closeButton = CreateButton("CloseSettlementButton", window, new Vector2(1600f, -28f), new Vector2(220f, 42f), "离开整备区域", 18);

        var preview = CreateImage("SettlementPreview", window, new Color(0.2f, 0.18f, 0.13f, 1f));
        SetAnchors(preview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        preview.anchoredPosition = new Vector2(360f, -126f);
        preview.sizeDelta = new Vector2(700f, 272f);
        preview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var previewLabel = CreateText("SettlementPreviewLabel", window, new Vector2(360f, -240f), new Vector2(700f, 30f), 20, FontStyle.Bold, TextAnchor.MiddleCenter);

        var statusPanel = CreatePanel("SettlementStatusPanel", window, new Vector2(1100f, -126f), new Vector2(784f, 272f), new Color(0.09f, 0.1f, 0.11f, 0.88f));
        SetAnchors(statusPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(statusPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("SettlementStatusTitle", statusPanel, new Vector2(20f, -18f), new Vector2(220f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "整备概览";
        var statusText = CreateText("SettlementStatus", statusPanel, new Vector2(20f, -58f), new Vector2(744f, 72f), 18, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(statusText);
        var actionHintText = CreateText("SettlementHint", statusPanel, new Vector2(20f, -144f), new Vector2(744f, 48f), 18, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(actionHintText);

        var actionPanel = CreatePanel("SettlementActionPanel", window, new Vector2(1100f, -430f), new Vector2(784f, 514f), new Color(0.09f, 0.1f, 0.11f, 0.88f));
        SetAnchors(actionPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(actionPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("SettlementActionTitle", actionPanel, new Vector2(20f, -18f), new Vector2(220f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "可执行操作";
        var inventoryButton = CreateButton("SettlementInventoryButton", actionPanel, new Vector2(20f, -72f), new Vector2(744f, 52f), "查看储物袋", 18);
        var workshopButton = CreateButton("SettlementWorkshopButton", actionPanel, new Vector2(20f, -136f), new Vector2(744f, 52f), "打开炼制台", 18);
        var vitalityButton = CreateButton("SettlementVitalityButton", actionPanel, new Vector2(20f, -220f), new Vector2(744f, 52f), "温养护身法器", 18);
        var attackButton = CreateButton("SettlementAttackButton", actionPanel, new Vector2(20f, -284f), new Vector2(744f, 52f), "祭炼主法器", 18);
        CreateText("SettlementLocationTitle", actionPanel, new Vector2(20f, -356f), new Vector2(220f, 24f), 20, FontStyle.Bold, TextAnchor.MiddleLeft).text = "坊市分区";
        var locationButton0 = CreateButton("SettlementLocationButton0", actionPanel, new Vector2(20f, -394f), new Vector2(232f, 92f), "临时地点", 17);
        var locationButton1 = CreateButton("SettlementLocationButton1", actionPanel, new Vector2(266f, -394f), new Vector2(232f, 92f), "临时地点", 17);
        var locationButton2 = CreateButton("SettlementLocationButton2", actionPanel, new Vector2(512f, -394f), new Vector2(232f, 92f), "临时地点", 17);

        var summaryPanel = CreatePanel("SettlementSummaryPanel", window, new Vector2(36f, -430f), new Vector2(1024f, 514f), new Color(0.09f, 0.1f, 0.11f, 0.88f));
        SetAnchors(summaryPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(summaryPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("SettlementSummaryTitle", summaryPanel, new Vector2(20f, -18f), new Vector2(240f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "整备纪要";
        var summaryText = CreateScrollTextArea("SettlementSummaryScroll", summaryPanel, new Vector2(18f, -56f), new Vector2(988f, 432f), 18);

        panel.closeButton = closeButton;
        panel.inventoryButton = inventoryButton;
        panel.workshopButton = workshopButton;
        panel.vitalityButton = vitalityButton;
        panel.attackButton = attackButton;
        panel.dialogueButton = dialogueButton;
        panel.panelTitleText = panelTitle;
        panel.panelSubtitleText = panelSubtitle;
        panel.summaryText = summaryText;
        panel.statusText = statusText;
        panel.actionHintText = actionHintText;
        panel.inventoryButtonLabel = inventoryButton.GetComponentInChildren<Text>(true);
        panel.workshopButtonLabel = workshopButton.GetComponentInChildren<Text>(true);
        panel.vitalityButtonLabel = vitalityButton.GetComponentInChildren<Text>(true);
        panel.attackButtonLabel = attackButton.GetComponentInChildren<Text>(true);
        panel.previewImage = preview.GetComponent<Image>();
        panel.previewLabelText = previewLabel;
        panel.locationButtons = new[] { locationButton0, locationButton1, locationButton2 };
        panel.windowRect = window;
        return panel;
    }

    public static WorldMapRegionPanel BuildRegionPanelExport()
    {
        var existingPanel = ClonePrefabAsset<WorldMapRegionPanel>(WorldMapRegionPanelPrefabPath, "WorldMapRegionPanel");
        if (existingPanel != null)
        {
            return existingPanel;
        }

        var root = CreateUiObject("WorldMapRegionPanel", null);
        root.layer = 5;
        Stretch(root.GetComponent<RectTransform>());

        var panel = root.AddComponent<WorldMapRegionPanel>();

        var atmosphere = CreateImage("RegionAtmosphere", root.transform, new Color(0.16f, 0.13f, 0.09f, 0.2f));
        Stretch(atmosphere);
        atmosphere.offsetMin = new Vector2(-80f, -40f);
        atmosphere.offsetMax = new Vector2(80f, 40f);

        var window = CreatePanel("Window", root.transform, Vector2.zero, new Vector2(1920f, 1080f), PaperMid);
        SetAnchors(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        AddOutline(window, AccentGold);
        ApplyOptionalSprite(window.GetComponent<Image>(), DetailPanelArtPath);
        CreateAccentStrip(window, 10f);
        var panelTitle = CreateText("RegionPanelTitle", window, new Vector2(360f, -26f), new Vector2(520f, 40f), 34, FontStyle.Bold, TextAnchor.MiddleLeft);
        panelTitle.text = "青石山门";
        var panelSubtitle = CreateText("RegionPanelSubtitle", window, new Vector2(360f, -70f), new Vector2(640f, 26f), 18, FontStyle.Normal, TextAnchor.MiddleLeft);
        panelSubtitle.text = "偏安一隅的启程之地 / 练气初期";
        var closeButton = CreateButton("RegionCloseButton", window, new Vector2(1668f, -28f), new Vector2(180f, 42f), "返回山海图", 18);

        var preview = CreateImage("RegionPreview", window, new Color(0.24f, 0.18f, 0.12f, 1f));
        SetAnchors(preview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        preview.anchoredPosition = new Vector2(360f, -126f);
        preview.sizeDelta = new Vector2(836f, 320f);
        preview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var previewLabel = CreateText("RegionPreviewLabel", window, new Vector2(360f, -270f), new Vector2(836f, 30f), 20, FontStyle.Bold, TextAnchor.MiddleCenter);

        var statusPanel = CreatePanel("StatusPanel", window, new Vector2(1240f, -126f), new Vector2(644f, 320f), new Color(0.09f, 0.1f, 0.11f, 0.88f));
        SetAnchors(statusPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(statusPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("StatusPanelTitle", statusPanel, new Vector2(20f, -18f), new Vector2(240f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "地域概览";
        var statusText = CreateText("RegionStatus", statusPanel, new Vector2(20f, -58f), new Vector2(604f, 78f), 19, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(statusText);
        var taskSummary = CreateText("RegionTaskSummary", statusPanel, new Vector2(20f, -154f), new Vector2(604f, 120f), 18, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(taskSummary);
        var travelButton = CreateButton("RegionTravelButton", statusPanel, new Vector2(20f, -258f), new Vector2(280f, 44f), "进入历练", 18);
        var dialogueInlineButton = CreateButton("RegionDialogueInlineButton", statusPanel, new Vector2(320f, -258f), new Vector2(304f, 44f), "人物与线索", 18);
        var vitalityButton = CreateButton("RegionVitalityButton", statusPanel, new Vector2(20f, -312f), new Vector2(604f, 44f), "护身法器", 17);
        var attackButton = CreateButton("RegionAttackButton", statusPanel, new Vector2(20f, -366f), new Vector2(604f, 44f), "主法器", 17);
        CreateText("RegionLocationTitle", statusPanel, new Vector2(20f, -420f), new Vector2(220f, 24f), 20, FontStyle.Bold, TextAnchor.MiddleLeft).text = "前沿驻点";
        var locationButton0 = CreateButton("RegionLocationButton0", statusPanel, new Vector2(20f, -456f), new Vector2(286f, 84f), "临时驻点", 16);
        var locationButton1 = CreateButton("RegionLocationButton1", statusPanel, new Vector2(318f, -456f), new Vector2(286f, 84f), "临时驻点", 16);
        var locationButton2 = CreateButton("RegionLocationButton2", statusPanel, new Vector2(20f, -550f), new Vector2(286f, 84f), "临时驻点", 16);
        var locationButton3 = CreateButton("RegionLocationButton3", statusPanel, new Vector2(318f, -550f), new Vector2(286f, 84f), "临时驻点", 16);
        statusPanel.sizeDelta = new Vector2(644f, 640f);

        var descriptionPanel = CreatePanel("DescriptionPanel", window, new Vector2(36f, -478f), new Vector2(1848f, 566f), new Color(0.09f, 0.1f, 0.11f, 0.88f));
        SetAnchors(descriptionPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(descriptionPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("DescriptionPanelTitle", descriptionPanel, new Vector2(20f, -18f), new Vector2(240f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "地域详情";
        var descriptionText = CreateScrollTextArea("RegionDescriptionScroll", descriptionPanel, new Vector2(20f, -58f), new Vector2(1808f, 480f), 18);

        panel.closeButton = closeButton;
        panel.travelButton = travelButton;
        panel.vitalityButton = vitalityButton;
        panel.attackButton = attackButton;
        panel.dialogueButton = dialogueInlineButton;
        panel.panelTitleText = panelTitle;
        panel.panelSubtitleText = panelSubtitle;
        panel.descriptionText = descriptionText;
        panel.statusText = statusText;
        panel.taskSummaryText = taskSummary;
        panel.travelButtonLabel = travelButton.GetComponentInChildren<Text>(true);
        panel.vitalityButtonLabel = vitalityButton.GetComponentInChildren<Text>(true);
        panel.attackButtonLabel = attackButton.GetComponentInChildren<Text>(true);
        panel.previewImage = preview.GetComponent<Image>();
        panel.previewLabelText = previewLabel;
        panel.locationButtons = new[] { locationButton0, locationButton1, locationButton2, locationButton3 };
        panel.windowRect = window;
        return panel;
    }

    private static T ClonePrefabAsset<T>(string assetPath, string expectedName) where T : Component
    {
        var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefabAsset == null)
        {
            return null;
        }

        var instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
        if (instance == null)
        {
            return null;
        }

        instance.name = expectedName;
        var component = instance.GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        Object.DestroyImmediate(instance);
        return null;
    }

    private static GameHubView CreateWorldMapHud(string name, Transform parent, Vector2 anchoredPosition)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(GameHubView));
        root.layer = 5;
        root.transform.SetParent(parent, false);

        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchoredPosition = anchoredPosition;
        rootRect.sizeDelta = new Vector2(304f, 216f);
        SetAnchors(rootRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        var rootImage = root.GetComponent<Image>();
        rootImage.color = PaperDark;
        ApplyOptionalSprite(rootImage, DetailPanelArtPath);
        AddOutline(rootRect, new Color(0.56f, 0.47f, 0.28f, 0.9f));
        CreateAccentStrip(rootRect, 8f);

        var view = root.GetComponent<GameHubView>();

        var portraitFrame = CreatePanel("PortraitFrame", root.transform, new Vector2(18f, -18f), new Vector2(96f, 96f), new Color(0.17f, 0.14f, 0.11f, 0.96f));
        AddOutline(portraitFrame, new Color(0.56f, 0.47f, 0.28f, 0.82f));
        var portrait = CreateImage("Portrait", portraitFrame, new Color(0.22f, 0.18f, 0.14f, 1f));
        Stretch(portrait);
        portrait.offsetMin = new Vector2(4f, 4f);
        portrait.offsetMax = new Vector2(-4f, -4f);
        portrait.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var portraitLabel = CreateText("PortraitLabel", portraitFrame, new Vector2(0f, 0f), new Vector2(88f, 88f), 16, FontStyle.Bold, TextAnchor.MiddleCenter);
        SetAnchors(portraitLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        portraitLabel.rectTransform.anchoredPosition = Vector2.zero;

        var worldTime = CreateText("WorldTime", root.transform, new Vector2(18f, -12f), new Vector2(268f, 20f), 15, FontStyle.Bold, TextAnchor.MiddleCenter);
        var heroName = CreateText("HeroName", root.transform, new Vector2(128f, -38f), new Vector2(158f, 26f), 24, FontStyle.Bold, TextAnchor.MiddleLeft);
        var realm = CreateText("Realm", root.transform, new Vector2(128f, -68f), new Vector2(158f, 20f), 16, FontStyle.Normal, TextAnchor.MiddleLeft);
        var location = CreateText("Location", root.transform, new Vector2(18f, -124f), new Vector2(268f, 20f), 15, FontStyle.Normal, TextAnchor.MiddleLeft);

        var healthText = CreateText("HealthText", root.transform, new Vector2(18f, -150f), new Vector2(268f, 18f), 14, FontStyle.Bold, TextAnchor.MiddleLeft);
        var healthBar = CreatePanel("HealthBar", root.transform, new Vector2(18f, -170f), new Vector2(268f, 12f), new Color(0.12f, 0.08f, 0.08f, 0.92f));
        AddOutline(healthBar, new Color(0.48f, 0.2f, 0.18f, 0.64f));
        var healthFill = CreateImage("Fill", healthBar, new Color(0.74f, 0.28f, 0.22f, 0.96f));
        Stretch(healthFill);
        healthFill.offsetMin = new Vector2(1f, 1f);
        healthFill.offsetMax = new Vector2(-1f, -1f);
        var healthFillImage = healthFill.GetComponent<Image>();
        healthFillImage.sprite = GameSpriteLibrary.WhiteSquareSprite;
        healthFillImage.type = Image.Type.Filled;
        healthFillImage.fillMethod = Image.FillMethod.Horizontal;
        healthFillImage.fillOrigin = 0;

        var spiritText = CreateText("SpiritText", root.transform, new Vector2(18f, -188f), new Vector2(268f, 18f), 14, FontStyle.Bold, TextAnchor.MiddleLeft);
        var spiritBar = CreatePanel("SpiritBar", root.transform, new Vector2(18f, -208f), new Vector2(268f, 12f), new Color(0.08f, 0.1f, 0.12f, 0.92f));
        AddOutline(spiritBar, new Color(0.16f, 0.44f, 0.56f, 0.64f));
        var spiritFill = CreateImage("Fill", spiritBar, new Color(0.2f, 0.62f, 0.84f, 0.96f));
        Stretch(spiritFill);
        spiritFill.offsetMin = new Vector2(1f, 1f);
        spiritFill.offsetMax = new Vector2(-1f, -1f);
        var spiritFillImage = spiritFill.GetComponent<Image>();
        spiritFillImage.sprite = GameSpriteLibrary.WhiteSquareSprite;
        spiritFillImage.type = Image.Type.Filled;
        spiritFillImage.fillMethod = Image.FillMethod.Horizontal;
        spiritFillImage.fillOrigin = 0;

        var resourceText = CreateText("ResourceText", root.transform, new Vector2(18f, -100f), new Vector2(268f, 20f), 15, FontStyle.Bold, TextAnchor.MiddleLeft);

        var buttonWidth = 64f;
        var buttonY = -240f;
        var mapButton = CreateHudNavButton("HudMapButton", root.transform, new Vector2(18f, buttonY), new Vector2(buttonWidth, 42f), "地图");
        var inventoryButton = CreateHudNavButton("HudInventoryButton", root.transform, new Vector2(90f, buttonY), new Vector2(buttonWidth, 42f), "总览");
        var settlementButton = CreateHudNavButton("HudSettlementButton", root.transform, new Vector2(162f, buttonY), new Vector2(buttonWidth, 42f), "坊市");
        var sectButton = CreateHudNavButton("HudSectButton", root.transform, new Vector2(234f, buttonY), new Vector2(buttonWidth, 42f), "山门");

        rootRect.sizeDelta = new Vector2(304f, 292f);

        view.worldTimeText = worldTime;
        view.portraitImage = portrait.GetComponent<Image>();
        view.portraitLabelText = portraitLabel;
        view.heroNameText = heroName;
        view.realmText = realm;
        view.locationText = location;
        view.healthText = healthText;
        view.spiritText = spiritText;
        view.resourceText = resourceText;
        view.healthFillImage = healthFillImage;
        view.spiritFillImage = spiritFillImage;
        view.mapButton = mapButton;
        view.inventoryButton = inventoryButton;
        view.settlementButton = settlementButton;
        view.sectButton = sectButton;
        view.mapButtonImage = mapButton.GetComponent<Image>();
        view.inventoryButtonImage = inventoryButton.GetComponent<Image>();
        view.settlementButtonImage = settlementButton.GetComponent<Image>();
        view.sectButtonImage = sectButton.GetComponent<Image>();
        view.mapButtonLabel = mapButton.GetComponentInChildren<TextMeshProUGUI>(true);
        view.inventoryButtonLabel = inventoryButton.GetComponentInChildren<TextMeshProUGUI>(true);
        view.settlementButtonLabel = settlementButton.GetComponentInChildren<TextMeshProUGUI>(true);
        view.sectButtonLabel = sectButton.GetComponentInChildren<TextMeshProUGUI>(true);
        return view;
    }

    private static Button CreateHudNavButton(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, string label)
    {
        var button = CreateButton(name, parent, anchoredPosition, size, label, 14);
        var image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.2f, 0.18f, 0.14f, 0.94f);
        }

        return button;
    }

    private static void CreateRegionNode(RectTransform parent, WorldRegionDefinition region)
    {
        var node = new GameObject(region.Id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(WorldRegionNodeView));
        node.layer = 5;
        node.transform.SetParent(parent, false);

        var rect = node.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(154f, 74f);
        rect.anchoredPosition = new Vector2(region.MapPosition.x, -region.MapPosition.y);
        SetAnchors(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        var background = node.GetComponent<Image>();
        background.color = new Color(0.23f, 0.24f, 0.21f, 0.92f);
        var button = node.GetComponent<Button>();

        var border = CreateImage("Border", node.transform, new Color(0.72f, 0.61f, 0.32f, 0.9f));
        Stretch(border);
        border.offsetMin = new Vector2(0f, 0f);
        border.offsetMax = new Vector2(0f, 0f);

        var inner = CreateImage("Inner", node.transform, new Color(0.11f, 0.13f, 0.15f, 0.96f));
        Stretch(inner);
        inner.offsetMin = new Vector2(2f, 2f);
        inner.offsetMax = new Vector2(-2f, -2f);

        var title = CreateText("Title", node.transform, new Vector2(14f, -12f), new Vector2(120f, 24f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
        var subtitle = CreateText("Subtitle", node.transform, new Vector2(14f, -40f), new Vector2(120f, 20f), 15, FontStyle.Normal, TextAnchor.MiddleLeft);

        var view = node.GetComponent<WorldRegionNodeView>();
        view.regionId = region.Id;
        view.background = inner.GetComponent<Image>();
        view.border = border.GetComponent<Image>();
        view.titleText = title;
        view.subtitleText = subtitle;
        view.button = button;
        ApplyOptionalSprite(view.background, RegionNodeArtPath);
    }

    private static void CreatePath(RectTransform parent, Vector2 start, Vector2 end, Color color)
    {
        var path = new GameObject("Path", typeof(RectTransform), typeof(Image));
        path.layer = 5;
        path.transform.SetParent(parent, false);

        var rect = path.GetComponent<RectTransform>();
        var uiStart = new Vector2(start.x + 72f, -start.y - 34f);
        var uiEnd = new Vector2(end.x + 72f, -end.y - 34f);
        var delta = uiEnd - uiStart;
        rect.sizeDelta = new Vector2(delta.magnitude, 6f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.anchoredPosition = uiStart;
        rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        path.GetComponent<Image>().color = color;
    }

    private static void CreateAccentStrip(RectTransform parent, float width)
    {
        var accent = CreateImage("Accent", parent, AccentGold);
        SetAnchors(accent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        accent.anchoredPosition = Vector2.zero;
        accent.sizeDelta = new Vector2(width, 0f);
    }

    private static Button CreateButton(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, string label, int fontSize = 20)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIButtonSoundBinder));
        root.layer = 5;
        root.transform.SetParent(parent, false);
        var rect = root.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        SetAnchors(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        var image = root.GetComponent<Image>();
        image.color = new Color(0.23f, 0.19f, 0.12f, 0.96f);
        ApplyOptionalSprite(image, ResolveButtonArtPath(name));

        var text = CreateText("Label", root.transform, Vector2.zero, size, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter);
        text.text = label;
        return root.GetComponent<Button>();
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        var root = new GameObject(name, typeof(RectTransform));
        if (parent != null)
        {
            root.transform.SetParent(parent, false);
        }

        return root;
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

    private static RectTransform CreateImage(string name, Transform parent, Color color)
    {
        var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.layer = 5;
        imageObject.transform.SetParent(parent, false);
        var rect = imageObject.GetComponent<RectTransform>();
        imageObject.GetComponent<Image>().color = color;
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
        SetAnchors(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        var text = textObject.GetComponent<Text>();
        if (TMP_Settings.defaultFontAsset != null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }

        text.fontSize = fontSize;
        text.fontStyle = ConvertFontStyle(fontStyle);
        text.color = new Color(0.88f, 0.84f, 0.76f, 0.96f);
        text.alignment = ConvertAlignment(alignment);
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static Text CreateScrollTextArea(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, int fontSize)
    {
        var root = CreateUiObject(name, parent).GetComponent<RectTransform>();
        root.anchoredPosition = anchoredPosition;
        root.sizeDelta = size;
        SetAnchors(root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        var scrollRect = root.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 24f;

        var viewport = CreateUiObject("Viewport", root).GetComponent<RectTransform>();
        Stretch(viewport);
        viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        viewport.gameObject.AddComponent<RectMask2D>();

        var content = CreateUiObject("Content", viewport).GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 0f);
        var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(0, 12, 0, 12);
        contentLayout.childAlignment = TextAnchor.UpperLeft;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandHeight = false;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var text = CreateText("Text", content, new Vector2(0f, 0f), new Vector2(size.x - 28f, 0f), fontSize, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(text);
        var textRect = text.rectTransform;
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(-12f, 0f);
        var layoutElement = text.gameObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 0f;
        text.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport;
        scrollRect.content = content;
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

    private static string ResolveButtonArtPath(string buttonName)
    {
        switch (buttonName)
        {
            case "TravelButton":
            case "RegionTravelButton":
            case "RegionVitalityButton":
            case "RegionAttackButton":
            case "SettlementInventoryButton":
            case "SettlementWorkshopButton":
            case "SettlementVitalityButton":
            case "SettlementAttackButton":
            case "VitalityButton":
            case "AttackButton":
            case "CraftQiButton":
            case "CraftBagButton":
            case "CraftVitalityButton":
            case "CraftAttackButton":
            case "SectActionButton0":
            case "SectActionButton1":
            case "SectActionButton2":
            case "SectActionButton3":
                return PrimaryButtonArtPath;
            default:
                return SecondaryButtonArtPath;
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

    private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
    }
}
#endif
