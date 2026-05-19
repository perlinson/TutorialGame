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
        CreateImage("MistLeft", root.transform, new Color(0.13f, 0.17f, 0.18f, 0.28f)).sizeDelta = new Vector2(760f, 1080f);
        CreateImage("MistRight", root.transform, new Color(0.16f, 0.13f, 0.09f, 0.24f)).sizeDelta = new Vector2(900f, 1080f);

        var mainLayer = CreateUiObject("MainLayer", root.transform).GetComponent<RectTransform>();
        Stretch(mainLayer);

        var blockerLayer = CreateUiObject("BlockerLayer", root.transform).GetComponent<RectTransform>();
        Stretch(blockerLayer);

        var modalLayer = CreateUiObject("ModalLayer", root.transform).GetComponent<RectTransform>();
        Stretch(modalLayer);

        var mapScreen = CreateUiObject("MapScreen", mainLayer).GetComponent<RectTransform>();
        Stretch(mapScreen);

        var mapContentRoot = CreateUiObject("MapContentRoot", mapScreen).GetComponent<RectTransform>();
        mapContentRoot.anchorMin = new Vector2(0.5f, 0.5f);
        mapContentRoot.anchorMax = new Vector2(0.5f, 0.5f);
        mapContentRoot.pivot = new Vector2(0.5f, 0.5f);
        mapContentRoot.anchoredPosition = Vector2.zero;
        mapContentRoot.sizeDelta = new Vector2(1920f, 1080f);

        var titlePanel = CreatePanel("TitlePanel", mapContentRoot, new Vector2(70f, -50f), new Vector2(1780f, 170f), new Color(0.08f, 0.1f, 0.12f, 0.84f));
        SetAnchors(titlePanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(titlePanel, new Color(0.68f, 0.57f, 0.3f, 0.9f));
        var titleText = CreateText("Title", titlePanel, new Vector2(28f, -18f), new Vector2(460f, 42f), 36, FontStyle.Bold, TextAnchor.MiddleLeft);
        var heroSummary = CreateText("HeroSummary", titlePanel, new Vector2(28f, -62f), new Vector2(720f, 56f), 22, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(heroSummary);
        var resourceSummary = CreateText("ResourceSummary", titlePanel, new Vector2(860f, -26f), new Vector2(820f, 80f), 20, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(resourceSummary);
        var bagSummary = CreateText("BagSummary", titlePanel, new Vector2(860f, -108f), new Vector2(820f, 54f), 18, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(bagSummary);
        var bagButton = CreateButton("BagButton", titlePanel, new Vector2(1530f, -26f), new Vector2(110f, 42f), "储物袋", 18);
        var workshopButton = CreateButton("WorkshopButton", titlePanel, new Vector2(1658f, -26f), new Vector2(110f, 42f), "洞府整备", 18);

        var mapPanel = CreatePanel("MapPanel", mapContentRoot, new Vector2(70f, -260f), new Vector2(1180f, 730f), new Color(0.07f, 0.09f, 0.1f, 0.74f));
        SetAnchors(mapPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(mapPanel, new Color(0.47f, 0.39f, 0.24f, 0.88f));
        CreateText("MapCaption", mapPanel, new Vector2(24f, -18f), new Vector2(320f, 30f), 24, FontStyle.Bold, TextAnchor.MiddleLeft).text = "外域山海脉络";
        var mapField = CreateUiObject("MapField", mapPanel.transform).GetComponent<RectTransform>();
        SetAnchors(mapField, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        mapField.anchoredPosition = new Vector2(18f, -60f);
        mapField.sizeDelta = new Vector2(1140f, 640f);
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

        var detailPanel = CreatePanel("DetailPanel", mapContentRoot, new Vector2(-70f, -260f), new Vector2(530f, 730f), new Color(0.08f, 0.1f, 0.12f, 0.82f));
        SetAnchors(detailPanel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
        AddOutline(detailPanel, new Color(0.68f, 0.57f, 0.3f, 0.9f));
        ApplyOptionalSprite(detailPanel.GetComponent<Image>(), DetailPanelArtPath);
        var regionTitle = CreateText("RegionTitle", detailPanel, new Vector2(26f, -24f), new Vector2(460f, 52f), 28, FontStyle.Bold, TextAnchor.UpperLeft);
        var regionPreview = CreateImage("RegionPreview", detailPanel, new Color(0.24f, 0.21f, 0.17f, 1f));
        SetAnchors(regionPreview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        regionPreview.anchoredPosition = new Vector2(26f, -92f);
        regionPreview.sizeDelta = new Vector2(470f, 132f);
        regionPreview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var regionPreviewLabel = CreateText("RegionPreviewLabel", detailPanel, new Vector2(26f, -144f), new Vector2(470f, 28f), 20, FontStyle.Bold, TextAnchor.MiddleCenter);
        var regionBody = CreateText("RegionBody", detailPanel, new Vector2(26f, -214f), new Vector2(470f, 164f), 20, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(regionBody);
        var regionStatus = CreateText("RegionStatus", detailPanel, new Vector2(26f, -386f), new Vector2(470f, 48f), 20, FontStyle.Bold, TextAnchor.UpperLeft);
        EnableWrapping(regionStatus);
        var taskPreview = CreateImage("TaskPreview", detailPanel, new Color(0.19f, 0.17f, 0.13f, 1f));
        SetAnchors(taskPreview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        taskPreview.anchoredPosition = new Vector2(26f, -444f);
        taskPreview.sizeDelta = new Vector2(470f, 84f);
        taskPreview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var taskPreviewLabel = CreateText("TaskPreviewLabel", detailPanel, new Vector2(26f, -474f), new Vector2(470f, 24f), 18, FontStyle.Bold, TextAnchor.MiddleCenter);
        var taskSummary = CreateText("TaskSummary", detailPanel, new Vector2(26f, -540f), new Vector2(470f, 76f), 17, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(taskSummary);

        var travelButton = CreateButton("TravelButton", detailPanel, new Vector2(26f, -628f), new Vector2(220f, 50f), "前往历练");
        var vitalityButton = CreateButton("VitalityButton", detailPanel, new Vector2(26f, -688f), new Vector2(220f, 40f), "温养护身器", 18);
        var attackButton = CreateButton("AttackButton", detailPanel, new Vector2(276f, -688f), new Vector2(220f, 40f), "祭炼主法器", 18);
        var returnButton = CreateButton("ReturnButton", detailPanel, new Vector2(276f, -628f), new Vector2(220f, 50f), "返回主界面");

        var hintPanel = CreatePanel("HintPanel", mapContentRoot, new Vector2(70f, 40f), new Vector2(1780f, 44f), new Color(0.07f, 0.08f, 0.09f, 0.82f));
        SetAnchors(hintPanel, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        var hintText = CreateText("Hint", hintPanel, new Vector2(18f, 0f), new Vector2(1720f, 44f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
        hintText.text = "山海录 / 正在整合地域情报";

        var modalBlocker = CreatePanel("ModalBlocker", blockerLayer, Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.04f, 0.72f));
        Stretch(modalBlocker);
        modalBlocker.gameObject.AddComponent<Button>().transition = Selectable.Transition.None;
        modalBlocker.gameObject.SetActive(false);

        var inventoryPanel = CreatePanel("InventoryPanel", modalLayer, Vector2.zero, new Vector2(820f, 660f), new Color(0.07f, 0.08f, 0.09f, 0.96f));
        SetAnchors(inventoryPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        AddOutline(inventoryPanel, new Color(0.68f, 0.57f, 0.3f, 0.9f));
        CreateText("InventoryTitle", inventoryPanel, new Vector2(28f, -22f), new Vector2(240f, 34f), 30, FontStyle.Bold, TextAnchor.MiddleLeft).text = "储物袋";
        var inventoryPreview = CreateImage("InventoryPreview", inventoryPanel, new Color(0.22f, 0.18f, 0.14f, 1f));
        SetAnchors(inventoryPreview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        inventoryPreview.anchoredPosition = new Vector2(28f, -72f);
        inventoryPreview.sizeDelta = new Vector2(764f, 136f);
        inventoryPreview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var inventoryPreviewLabel = CreateText("InventoryPreviewLabel", inventoryPanel, new Vector2(28f, -126f), new Vector2(764f, 30f), 20, FontStyle.Bold, TextAnchor.MiddleCenter);
        var inventoryDetail = CreateText("InventoryDetail", inventoryPanel, new Vector2(28f, -226f), new Vector2(764f, 346f), 20, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(inventoryDetail);
        var closeInventoryButton = CreateButton("CloseInventoryButton", inventoryPanel, new Vector2(590f, -586f), new Vector2(190f, 46f), "收起储物袋", 18);
        inventoryPanel.gameObject.SetActive(false);

        var workshopPanel = CreatePanel("WorkshopPanel", modalLayer, Vector2.zero, new Vector2(920f, 680f), new Color(0.07f, 0.08f, 0.09f, 0.96f));
        SetAnchors(workshopPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        AddOutline(workshopPanel, new Color(0.68f, 0.57f, 0.3f, 0.9f));
        CreateText("WorkshopTitle", workshopPanel, new Vector2(28f, -22f), new Vector2(300f, 34f), 30, FontStyle.Bold, TextAnchor.MiddleLeft).text = "洞府整备";
        var workshopPreview = CreateImage("WorkshopPreview", workshopPanel, new Color(0.18f, 0.2f, 0.16f, 1f));
        SetAnchors(workshopPreview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        workshopPreview.anchoredPosition = new Vector2(28f, -72f);
        workshopPreview.sizeDelta = new Vector2(864f, 148f);
        workshopPreview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var workshopPreviewLabel = CreateText("WorkshopPreviewLabel", workshopPanel, new Vector2(28f, -132f), new Vector2(864f, 30f), 20, FontStyle.Bold, TextAnchor.MiddleCenter);
        var workshopSummary = CreateText("WorkshopSummary", workshopPanel, new Vector2(28f, -246f), new Vector2(864f, 246f), 19, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(workshopSummary);
        var craftQiButton = CreateButton("CraftQiButton", workshopPanel, new Vector2(28f, -536f), new Vector2(410f, 48f), "丹炉养火", 18);
        var craftBagButton = CreateButton("CraftBagButton", workshopPanel, new Vector2(482f, -536f), new Vector2(410f, 48f), "符匣拓纹", 18);
        var craftVitalityButton = CreateButton("CraftVitalityButton", workshopPanel, new Vector2(28f, -594f), new Vector2(410f, 48f), "培元散", 18);
        var craftAttackButton = CreateButton("CraftAttackButton", workshopPanel, new Vector2(482f, -594f), new Vector2(410f, 48f), "纳物符袋", 18);
        var closeWorkshopButton = CreateButton("CloseWorkshopButton", workshopPanel, new Vector2(702f, -28f), new Vector2(190f, 40f), "收起整备", 18);
        workshopPanel.gameObject.SetActive(false);

        var sectPanel = CreatePanel("SectPanel", mainLayer, Vector2.zero, Vector2.zero, new Color(0.03f, 0.04f, 0.05f, 0.985f));
        Stretch(sectPanel);
        var sectAtmosphere = CreateImage("SectAtmosphere", sectPanel, new Color(0.18f, 0.14f, 0.1f, 0.22f));
        Stretch(sectAtmosphere);
        sectAtmosphere.offsetMin = new Vector2(-220f, -120f);
        sectAtmosphere.offsetMax = new Vector2(220f, 120f);
        var sectWindow = CreatePanel("SectWindow", sectPanel, Vector2.zero, new Vector2(1780f, 960f), new Color(0.07f, 0.08f, 0.09f, 0.98f));
        SetAnchors(sectWindow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        AddOutline(sectWindow, new Color(0.68f, 0.57f, 0.3f, 0.9f));
        CreateText("SectPanelTitle", sectWindow, new Vector2(36f, -26f), new Vector2(360f, 40f), 32, FontStyle.Bold, TextAnchor.MiddleLeft).text = "青玄山门";
        var sectCaption = CreateText("SectCaption", sectWindow, new Vector2(36f, -68f), new Vector2(520f, 24f), 18, FontStyle.Normal, TextAnchor.MiddleLeft);
        sectCaption.text = "门派驻地 / 殿堂事务 / 洞府整备";
        var closeSectButton = CreateButton("CloseSectButton", sectWindow, new Vector2(1554f, -28f), new Vector2(190f, 42f), "离开门派", 18);

        var hallColumn = CreatePanel("HallColumn", sectWindow, new Vector2(28f, -120f), new Vector2(280f, 800f), new Color(0.09f, 0.1f, 0.11f, 0.92f));
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

        var sectPreview = CreateImage("SectPreview", sectWindow, new Color(0.24f, 0.18f, 0.12f, 1f));
        SetAnchors(sectPreview, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        sectPreview.anchoredPosition = new Vector2(340f, -110f);
        sectPreview.sizeDelta = new Vector2(1404f, 214f);
        sectPreview.GetComponent<Image>().sprite = GameSpriteLibrary.WhiteSquareSprite;
        var sectPreviewLabel = CreateText("SectPreviewLabel", sectWindow, new Vector2(340f, -198f), new Vector2(1404f, 30f), 20, FontStyle.Bold, TextAnchor.MiddleCenter);

        var sectTitle = CreateText("SectTitle", sectWindow, new Vector2(340f, -360f), new Vector2(920f, 36f), 30, FontStyle.Bold, TextAnchor.MiddleLeft);
        var sectDescription = CreateText("SectDescription", sectWindow, new Vector2(340f, -408f), new Vector2(920f, 90f), 18, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(sectDescription);
        var sectDetailPanel = CreatePanel("SectDetailPanel", sectWindow, new Vector2(340f, -512f), new Vector2(920f, 342f), new Color(0.09f, 0.1f, 0.11f, 0.94f));
        SetAnchors(sectDetailPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(sectDetailPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("SectDetailTitle", sectDetailPanel, new Vector2(20f, -18f), new Vector2(220f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "殿堂纪要";
        var sectStatus = CreateScrollTextArea("SectStatusScroll", sectDetailPanel, new Vector2(18f, -56f), new Vector2(884f, 266f), 18);

        var actionPanel = CreatePanel("SectActionPanel", sectWindow, new Vector2(1288f, -360f), new Vector2(456f, 494f), new Color(0.09f, 0.1f, 0.11f, 0.94f));
        SetAnchors(actionPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(actionPanel, new Color(0.44f, 0.37f, 0.22f, 0.8f));
        CreateText("SectActionTitle", actionPanel, new Vector2(20f, -18f), new Vector2(220f, 26f), 22, FontStyle.Bold, TextAnchor.MiddleLeft).text = "当前可办事务";
        var sectActionButton0 = CreateButton("SectActionButton0", actionPanel, new Vector2(20f, -74f), new Vector2(416f, 56f), "殿堂事务", 18);
        var sectActionButton1 = CreateButton("SectActionButton1", actionPanel, new Vector2(20f, -144f), new Vector2(416f, 56f), "殿堂事务", 18);
        var sectActionButton2 = CreateButton("SectActionButton2", actionPanel, new Vector2(20f, -214f), new Vector2(416f, 56f), "殿堂事务", 18);
        var sectActionButton3 = CreateButton("SectActionButton3", actionPanel, new Vector2(20f, -284f), new Vector2(416f, 56f), "殿堂事务", 18);
        sectPanel.gameObject.SetActive(false);

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
        controller.inventoryDetailText = inventoryDetail;
        controller.workshopSummaryText = workshopSummary;
        controller.regionPreviewImage = regionPreview.GetComponent<Image>();
        controller.regionPreviewLabelText = regionPreviewLabel;
        controller.inventoryPreviewImage = inventoryPreview.GetComponent<Image>();
        controller.inventoryPreviewLabelText = inventoryPreviewLabel;
        controller.workshopPreviewImage = workshopPreview.GetComponent<Image>();
        controller.workshopPreviewLabelText = workshopPreviewLabel;
        controller.hintText = hintText;
        controller.travelButton = travelButton;
        controller.bagButton = bagButton;
        controller.workshopButton = workshopButton;
        controller.sectResidenceButton = sectResidenceButton;
        controller.vitalityUpgradeButton = vitalityButton;
        controller.attackUpgradeButton = attackButton;
        controller.returnButton = returnButton;
        controller.closeInventoryButton = closeInventoryButton;
        controller.closeWorkshopButton = closeWorkshopButton;
        controller.closeSectButton = closeSectButton;
        controller.craftQiButton = craftQiButton;
        controller.craftBagButton = craftBagButton;
        controller.craftVitalityButton = craftVitalityButton;
        controller.craftAttackButton = craftAttackButton;
        controller.sectHallButtons = new[] { hallButton0, hallButton1, hallButton2, hallButton3, hallButton4, hallButton5, hallButton6 };
        controller.sectActionButtons = new[] { sectActionButton0, sectActionButton1, sectActionButton2, sectActionButton3 };
        controller.mapScreen = mapScreen.gameObject;
        controller.inventoryPanel = inventoryPanel.gameObject;
        controller.workshopPanel = workshopPanel.gameObject;
        controller.sectPanel = sectPanel.gameObject;
        controller.modalBlocker = modalBlocker.gameObject;
        controller.sectTitleText = sectTitle;
        controller.sectDescriptionText = sectDescription;
        controller.sectStatusText = sectStatus;
        controller.sectPreviewImage = sectPreview.GetComponent<Image>();
        controller.sectPreviewLabelText = sectPreviewLabel;
        return controller;
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

    private static Button CreateButton(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, string label, int fontSize = 20)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
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
