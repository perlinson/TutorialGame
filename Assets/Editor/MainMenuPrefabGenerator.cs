#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using InputField = TMPro.TMP_InputField;
using Text = TMPro.TextMeshProUGUI;

public static class MainMenuPrefabGenerator
{
    private const string RootFolder = "Assets/Resources/UI/MainMenu";
    private const string MainMenuPrefabPath = RootFolder + "/MainMenuRoot.prefab";
    private const string MainMenuSettingsPanelPrefabPath = RootFolder + "/MainMenuSettingsPanel.prefab";
    private const string MainMenuLoadPanelPrefabPath = RootFolder + "/MainMenuLoadPanel.prefab";
    private const string MainMenuCharacterCreatePanelPrefabPath = RootFolder + "/MainMenuCharacterCreatePanel.prefab";
    private const string LoadSlotPrefabPath = RootFolder + "/LoadSlotItem.prefab";
    private const string CharacterSlotPrefabPath = RootFolder + "/CharacterSlotItem.prefab";
    private const string ArchetypeCardPrefabPath = RootFolder + "/ArchetypeCard.prefab";
    private const string MainMenuBackdropArtPath = "Assets/GameArt/Backgrounds/MainMenu/bg_mainmenu_mountain_gate.png";
    private const string PrimaryButtonArtPath = "Assets/GameArt/UI/Buttons/ui_btn_primary_gold.png";
    private const string SecondaryButtonArtPath = "Assets/GameArt/UI/Buttons/ui_btn_secondary_jade.png";
    private const string DangerButtonArtPath = "Assets/GameArt/UI/Buttons/ui_btn_danger_red.png";
    private const string SaveSlotPanelArtPath = "Assets/GameArt/UI/Panels/panel_save_slot_ink.png";
    private const string ArchetypeCardPanelArtPath = "Assets/GameArt/UI/Panels/panel_archetype_card_ink.png";

    [MenuItem("Tools/Cultivation/Regenerate Main Menu Prefabs")]
    public static void RegeneratePrefabs()
    {
        EnsurePrefabAssets(true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Main menu prefabs regenerated.");
    }

    private static void EnsurePrefabAssets(bool overwrite)
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/UI");
        EnsureFolder(RootFolder);

        var loadSlotPrefab = CreateLoadSlotPrefab(overwrite);
        var characterSlotPrefab = CreateCharacterSlotPrefab(overwrite);
        var archetypeCardPrefab = CreateArchetypeCardPrefab(overwrite);

        CreateMainMenuRootPrefab(overwrite);
        CreateSettingsPanelPrefab(overwrite);
        CreateLoadPanelPrefab(loadSlotPrefab, overwrite);
        CreateCharacterCreatePanelPrefab(characterSlotPrefab, archetypeCardPrefab, overwrite);
    }

    private static SaveSlotView CreateLoadSlotPrefab(bool overwrite)
    {
        if (!overwrite)
        {
            var existing = AssetDatabase.LoadAssetAtPath<SaveSlotView>(LoadSlotPrefabPath);
            if (existing != null)
            {
                return existing;
            }
        }

        var root = CreateUiObject("LoadSlotItem", null);
        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 118f);

        var image = root.AddComponent<Image>();
        image.color = new Color(0.13f, 0.14f, 0.16f, 0.82f);
        ApplyOptionalSprite(image, SaveSlotPanelArtPath);
        var button = root.AddComponent<Button>();
        var layout = root.AddComponent<LayoutElement>();
        layout.preferredHeight = 118f;
        layout.minHeight = 118f;

        var accent = CreateImage("Accent", root.transform, new Color(0.76f, 0.59f, 0.29f, 0.95f));
        SetAnchors(accent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        accent.sizeDelta = new Vector2(8f, 0f);

        var title = CreateText("Title", root.transform, new Vector2(24f, -18f), new Vector2(280f, 28f), 22, FontStyle.Bold, TextAnchor.MiddleLeft);
        var detail = CreateText("Detail", root.transform, new Vector2(24f, -50f), new Vector2(360f, 28f), 22, FontStyle.Normal, TextAnchor.MiddleLeft);
        var footer = CreateText("Footer", root.transform, new Vector2(24f, -82f), new Vector2(360f, 22f), 18, FontStyle.Normal, TextAnchor.MiddleLeft);

        var view = root.AddComponent<SaveSlotView>();
        view.background = image;
        view.accent = accent.GetComponent<Image>();
        view.titleText = title;
        view.detailText = detail;
        view.footerText = footer;
        view.button = button;

        return SaveAsPrefab(root, LoadSlotPrefabPath, overwrite).GetComponent<SaveSlotView>();
    }

    private static SaveSlotView CreateCharacterSlotPrefab(bool overwrite)
    {
        if (!overwrite)
        {
            var existing = AssetDatabase.LoadAssetAtPath<SaveSlotView>(CharacterSlotPrefabPath);
            if (existing != null)
            {
                return existing;
            }
        }

        var root = CreateUiObject("CharacterSlotItem", null);
        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(106f, 70f);

        var image = root.AddComponent<Image>();
        image.color = new Color(0.16f, 0.15f, 0.14f, 0.82f);
        ApplyOptionalSprite(image, SaveSlotPanelArtPath);
        var button = root.AddComponent<Button>();

        var accent = CreateImage("Accent", root.transform, new Color(0.76f, 0.59f, 0.29f, 0.95f));
        SetAnchors(accent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        accent.sizeDelta = new Vector2(6f, 0f);

        var title = CreateText("Title", root.transform, new Vector2(14f, -14f), new Vector2(74f, 18f), 16, FontStyle.Bold, TextAnchor.MiddleLeft);
        var detail = CreateText("Detail", root.transform, new Vector2(14f, -34f), new Vector2(74f, 16f), 14, FontStyle.Bold, TextAnchor.MiddleLeft);
        var footer = CreateText("Footer", root.transform, new Vector2(14f, -52f), new Vector2(74f, 14f), 12, FontStyle.Normal, TextAnchor.MiddleLeft);

        var view = root.AddComponent<SaveSlotView>();
        view.background = image;
        view.accent = accent.GetComponent<Image>();
        view.titleText = title;
        view.detailText = detail;
        view.footerText = footer;
        view.button = button;

        return SaveAsPrefab(root, CharacterSlotPrefabPath, overwrite).GetComponent<SaveSlotView>();
    }

    private static ArchetypeCardView CreateArchetypeCardPrefab(bool overwrite)
    {
        if (!overwrite)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ArchetypeCardView>(ArchetypeCardPrefabPath);
            if (existing != null)
            {
                return existing;
            }
        }

        var root = CreateUiObject("ArchetypeCard", null);
        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300f, 608f);

        var image = root.AddComponent<Image>();
        image.color = new Color(0.13f, 0.14f, 0.16f, 0.78f);
        ApplyOptionalSprite(image, ArchetypeCardPanelArtPath);
        root.AddComponent<RectMask2D>();
        var button = root.AddComponent<Button>();

        var accent = CreateImage("Accent", root.transform, new Color(0.34f, 0.31f, 0.26f, 0.85f));
        SetAnchors(accent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        accent.sizeDelta = new Vector2(8f, 0f);

        var portrait = CreateImage("Portrait", root.transform, new Color(0.24f, 0.2f, 0.17f, 1f));
        Stretch(portrait);
        var portraitImage = portrait.GetComponent<Image>();
        portraitImage.sprite = GameSpriteLibrary.WhiteSquareSprite;
        portraitImage.preserveAspect = true;
        portrait.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.None;
        var portraitShade = CreateImage("PortraitShade", root.transform, new Color(0.04f, 0.05f, 0.06f, 0.34f));
        Stretch(portraitShade);
        var textBacking = CreateImage("TextBacking", root.transform, new Color(0.05f, 0.05f, 0.06f, 0.56f));
        textBacking.anchorMin = new Vector2(0f, 0f);
        textBacking.anchorMax = new Vector2(1f, 0f);
        textBacking.pivot = new Vector2(0.5f, 0f);
        textBacking.anchoredPosition = Vector2.zero;
        textBacking.sizeDelta = new Vector2(0f, 446f);
        var portraitLabel = CreateText("PortraitLabel", root.transform, new Vector2(22f, -24f), new Vector2(240f, 28f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
        var title = CreateText("Title", root.transform, new Vector2(22f, -146f), new Vector2(220f, 40f), 30, FontStyle.Bold, TextAnchor.MiddleLeft);
        var origin = CreateText("Origin", root.transform, new Vector2(22f, -192f), new Vector2(240f, 22f), 16, FontStyle.Bold, TextAnchor.MiddleLeft);
        var specialty = CreateText("Specialty", root.transform, new Vector2(22f, -220f), new Vector2(240f, 22f), 16, FontStyle.Bold, TextAnchor.MiddleLeft);
        var description = CreateText("Description", root.transform, new Vector2(22f, -264f), new Vector2(240f, 100f), 20, FontStyle.Normal, TextAnchor.UpperLeft);
        var trait = CreateText("Trait", root.transform, new Vector2(22f, -410f), new Vector2(240f, 168f), 18, FontStyle.Normal, TextAnchor.UpperLeft);

        EnableWrapping(description);
        EnableWrapping(trait);

        var view = root.AddComponent<ArchetypeCardView>();
        view.background = image;
        view.accent = accent.GetComponent<Image>();
        view.portraitImage = portrait.GetComponent<Image>();
        view.titleText = title;
        view.originText = origin;
        view.specialtyText = specialty;
        view.descriptionText = description;
        view.traitText = trait;
        view.portraitLabelText = portraitLabel;
        view.button = button;

        return SaveAsPrefab(root, ArchetypeCardPrefabPath, overwrite).GetComponent<ArchetypeCardView>();
    }

    private static void CreateMainMenuRootPrefab(bool overwrite)
    {
        if (!overwrite)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuPrefabPath);
            if (existing != null)
            {
                return;
            }
        }

        var root = CreateUiObject("MainMenuRoot", null);
        root.layer = 5;
        Stretch(root.GetComponent<RectTransform>());
        root.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        root.AddComponent<GraphicRaycaster>();
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var controller = root.AddComponent<MainMenuController>();

        var backdrop = CreateImage("Backdrop", root.transform, new Color(0.055f, 0.07f, 0.09f, 1f));
        Stretch(backdrop);
        ApplyOptionalSprite(backdrop.GetComponent<Image>(), MainMenuBackdropArtPath, true);

        var header = CreatePanel("Header", root.transform, new Vector2(110f, -88f), new Vector2(920f, 316f), new Color(0.08f, 0.09f, 0.1f, 0.82f));
        SetAnchors(header, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        var title = CreateText("Title", header, new Vector2(34f, -72f), new Vector2(680f, 66f), 72, FontStyle.Bold, TextAnchor.UpperLeft);
        var subtitle = CreateText("Subtitle", header, new Vector2(42f, -160f), new Vector2(420f, 30f), 26, FontStyle.Bold, TextAnchor.MiddleLeft);
        var description = CreateText("Description", header, new Vector2(42f, -206f), new Vector2(780f, 70f), 22, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(description);

        var menu = CreatePanel("MenuPanel", root.transform, new Vector2(110f, 122f), new Vector2(450f, 420f), new Color(0f, 0f, 0f, 0f));
        SetAnchors(menu, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        var menuLayout = menu.gameObject.AddComponent<VerticalLayoutGroup>();
        menuLayout.spacing = 16f;
        menuLayout.childControlWidth = true;
        menuLayout.childForceExpandWidth = true;
        menuLayout.childControlHeight = false;
        menuLayout.childForceExpandHeight = false;

        var newGameButton = CreateMenuButton("新游戏", menu);
        var continueButton = CreateMenuButton("继续游戏", menu);
        var loadButton = CreateMenuButton("加载存档", menu);
        var settingsButton = CreateMenuButton("设置", menu);
        var quitButton = CreateMenuButton("离开游戏", menu);

        var info = CreatePanel("InfoPanel", root.transform, new Vector2(-116f, -102f), new Vector2(760f, 836f), new Color(0.08f, 0.09f, 0.1f, 0.76f));
        SetAnchors(info, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
        var infoFlavor = CreateText("InfoFlavor", info, new Vector2(42f, -120f), new Vector2(620f, 110f), 22, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(infoFlavor);
        var recentSave = CreateText("RecentSave", info, new Vector2(42f, -310f), new Vector2(620f, 120f), 22, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(recentSave);

        var footer = CreatePanel("FooterPanel", root.transform, new Vector2(110f, 46f), new Vector2(820f, 48f), new Color(0.07f, 0.08f, 0.09f, 0.82f));
        SetAnchors(footer, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        var status = CreateText("Status", footer, new Vector2(18f, 0f), new Vector2(780f, 48f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);

        controller.titleText = title;
        controller.subtitleText = subtitle;
        controller.descriptionText = description;
        controller.statusText = status;
        controller.infoFlavorText = infoFlavor;
        controller.recentSaveText = recentSave;
        controller.newGameButton = newGameButton;
        controller.continueButton = continueButton;
        controller.loadButton = loadButton;
        controller.settingsButton = settingsButton;
        controller.quitButton = quitButton;

        SaveAsPrefab(root, MainMenuPrefabPath, overwrite);
    }

    private static void CreateSettingsPanelPrefab(bool overwrite)
    {
        if (!overwrite)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuSettingsPanelPrefabPath);
            if (existing != null)
            {
                return;
            }
        }

        var root = CreateOverlayRoot("MainMenuSettingsPanel");
        var controller = root.AddComponent<MainMenuSettingsPanel>();
        var blocker = root.GetComponent<Button>();

        var window = CreatePanel("Window", root.transform, Vector2.zero, new Vector2(1380f, 760f), new Color(0.1f, 0.12f, 0.14f, 0.95f));
        SetAnchors(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

        CreateText("Title", window, new Vector2(40f, -34f), new Vector2(260f, 36f), 34, FontStyle.Bold, TextAnchor.MiddleLeft).text = "洞府设置";
        CreateText("MusicVolumeLabel", window, new Vector2(40f, -116f), new Vector2(180f, 28f), 24, FontStyle.Bold, TextAnchor.MiddleLeft).text = "背景音乐";
        var musicVolumeDown = CreateInlineButton("-", window, new Vector2(300f, -104f), new Vector2(64f, 44f));
        var musicVolumeValue = CreateText("MusicVolumeValue", window, new Vector2(374f, -104f), new Vector2(96f, 44f), 22, FontStyle.Bold, TextAnchor.MiddleCenter);
        var musicVolumeUp = CreateInlineButton("+", window, new Vector2(480f, -104f), new Vector2(64f, 44f));

        CreateText("SfxVolumeLabel", window, new Vector2(40f, -178f), new Vector2(180f, 28f), 24, FontStyle.Bold, TextAnchor.MiddleLeft).text = "操作音效";
        var sfxVolumeDown = CreateInlineButton("-", window, new Vector2(300f, -166f), new Vector2(64f, 44f));
        var sfxVolumeValue = CreateText("SfxVolumeValue", window, new Vector2(374f, -166f), new Vector2(96f, 44f), 22, FontStyle.Bold, TextAnchor.MiddleCenter);
        var sfxVolumeUp = CreateInlineButton("+", window, new Vector2(480f, -166f), new Vector2(64f, 44f));

        CreateText("VoiceVolumeLabel", window, new Vector2(40f, -240f), new Vector2(180f, 28f), 24, FontStyle.Bold, TextAnchor.MiddleLeft).text = "角色语音";
        var voiceVolumeDown = CreateInlineButton("-", window, new Vector2(300f, -228f), new Vector2(64f, 44f));
        var voiceVolumeValue = CreateText("VoiceVolumeValue", window, new Vector2(374f, -228f), new Vector2(96f, 44f), 22, FontStyle.Bold, TextAnchor.MiddleCenter);
        var voiceVolumeUp = CreateInlineButton("+", window, new Vector2(480f, -228f), new Vector2(64f, 44f));

        CreateText("FullscreenLabel", window, new Vector2(40f, -316f), new Vector2(180f, 28f), 24, FontStyle.Bold, TextAnchor.MiddleLeft).text = "显示模式";
        var fullscreenToggle = CreateInlineButton("切换", window, new Vector2(300f, -304f), new Vector2(244f, 44f));
        var fullscreenValue = CreateText("FullscreenValue", window, new Vector2(554f, -304f), new Vector2(120f, 44f), 22, FontStyle.Bold, TextAnchor.MiddleLeft);
        var resetSettings = CreateInlineButton("恢复默认", window, new Vector2(40f, -408f), new Vector2(220f, 52f));
        var closeSettings = CreateInlineButton("关闭", window, new Vector2(494f, -446f), new Vector2(180f, 52f));

        controller.blockerButton = blocker;
        controller.musicVolumeDownButton = musicVolumeDown;
        controller.musicVolumeUpButton = musicVolumeUp;
        controller.sfxVolumeDownButton = sfxVolumeDown;
        controller.sfxVolumeUpButton = sfxVolumeUp;
        controller.voiceVolumeDownButton = voiceVolumeDown;
        controller.voiceVolumeUpButton = voiceVolumeUp;
        controller.fullscreenToggleButton = fullscreenToggle;
        controller.resetSettingsButton = resetSettings;
        controller.closeButton = closeSettings;
        controller.musicVolumeValueText = musicVolumeValue;
        controller.sfxVolumeValueText = sfxVolumeValue;
        controller.voiceVolumeValueText = voiceVolumeValue;
        controller.fullscreenValueText = fullscreenValue;
        controller.windowRect = window;

        SaveAsPrefab(root, MainMenuSettingsPanelPrefabPath, overwrite);
    }

    private static void CreateLoadPanelPrefab(SaveSlotView loadSlotPrefab, bool overwrite)
    {
        if (!overwrite)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuLoadPanelPrefabPath);
            if (existing != null)
            {
                return;
            }
        }

        var root = CreateOverlayRoot("MainMenuLoadPanel");
        var controller = root.AddComponent<MainMenuLoadPanel>();
        var blocker = root.GetComponent<Button>();

        var window = CreatePanel("Window", root.transform, Vector2.zero, new Vector2(1540f, 860f), new Color(0.1f, 0.12f, 0.14f, 0.95f));
        SetAnchors(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        CreateText("Title", window, new Vector2(42f, -34f), new Vector2(260f, 36f), 36, FontStyle.Bold, TextAnchor.MiddleLeft).text = "加载存档";

        var loadSlotsParent = CreatePanel("Slots", window.transform, new Vector2(42f, -146f), new Vector2(520f, 520f), new Color(0f, 0f, 0f, 0f));
        SetAnchors(loadSlotsParent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        var loadSlotsLayout = loadSlotsParent.gameObject.AddComponent<VerticalLayoutGroup>();
        loadSlotsLayout.spacing = 16f;
        loadSlotsLayout.childControlWidth = true;
        loadSlotsLayout.childForceExpandWidth = true;
        loadSlotsLayout.childControlHeight = false;
        loadSlotsLayout.childForceExpandHeight = false;
        loadSlotsParent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var loadDetailTitle = CreateText("DetailTitle", window, new Vector2(640f, -146f), new Vector2(520f, 34f), 28, FontStyle.Bold, TextAnchor.MiddleLeft);
        var loadDetailBody = CreateText("DetailBody", window, new Vector2(640f, -204f), new Vector2(600f, 250f), 22, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(loadDetailBody);
        var loadAction = CreateText("Action", window, new Vector2(640f, -488f), new Vector2(600f, 30f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
        var loadSelected = CreateInlineButton("载入此档", window, new Vector2(640f, -536f), new Vector2(204f, 56f));
        var deleteSelected = CreateInlineButton("删除此档", window, new Vector2(860f, -536f), new Vector2(204f, 56f));
        var closeLoad = CreateInlineButton("关闭", window, new Vector2(1080f, -536f), new Vector2(160f, 56f));

        controller.blockerButton = blocker;
        controller.loadSelectedButton = loadSelected;
        controller.deleteSelectedButton = deleteSelected;
        controller.closeButton = closeLoad;
        controller.loadDetailTitleText = loadDetailTitle;
        controller.loadDetailBodyText = loadDetailBody;
        controller.loadActionText = loadAction;
        controller.windowRect = window;
        controller.loadSlotsParent = loadSlotsParent;
        controller.loadSlotPrefab = loadSlotPrefab;

        SaveAsPrefab(root, MainMenuLoadPanelPrefabPath, overwrite);
    }

    private static void CreateCharacterCreatePanelPrefab(SaveSlotView characterSlotPrefab, ArchetypeCardView archetypeCardPrefab, bool overwrite)
    {
        if (!overwrite)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuCharacterCreatePanelPrefabPath);
            if (existing != null)
            {
                return;
            }
        }

        var root = CreateOverlayRoot("MainMenuCharacterCreatePanel");
        var controller = root.AddComponent<MainMenuCharacterCreatePanel>();
        var blocker = root.GetComponent<Button>();

        var window = CreatePanel("Window", root.transform, Vector2.zero, new Vector2(1540f, 860f), new Color(0.1f, 0.12f, 0.14f, 0.95f));
        SetAnchors(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        CreateText("Title", window, new Vector2(42f, -34f), new Vector2(360f, 38f), 38, FontStyle.Bold, TextAnchor.MiddleLeft).text = "择道入世";

        var archetypeParent = CreatePanel("Archetypes", window.transform, new Vector2(42f, -150f), new Vector2(1020f, 620f), new Color(0f, 0f, 0f, 0f));
        SetAnchors(archetypeParent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        var archetypeLayout = archetypeParent.gameObject.AddComponent<HorizontalLayoutGroup>();
        archetypeLayout.spacing = 18f;
        archetypeLayout.childControlWidth = false;
        archetypeLayout.childControlHeight = false;
        archetypeLayout.childForceExpandWidth = false;
        archetypeLayout.childForceExpandHeight = false;

        var characterSummaryTitle = CreateText("SummaryTitle", window, new Vector2(1120f, -170f), new Vector2(300f, 34f), 30, FontStyle.Bold, TextAnchor.MiddleLeft);
        var characterSummaryBody = CreateText("SummaryBody", window, new Vector2(1120f, -226f), new Vector2(340f, 220f), 20, FontStyle.Normal, TextAnchor.UpperLeft);
        EnableWrapping(characterSummaryBody);
        CreateText("HeroNameLabel", window, new Vector2(1120f, -480f), new Vector2(140f, 24f), 18, FontStyle.Bold, TextAnchor.MiddleLeft).text = "修士名号";
        var heroNameInput = CreateInputField(window.transform, new Vector2(1120f, -514f), new Vector2(320f, 48f));

        var characterSlotsParent = CreatePanel("CharacterSlots", window.transform, new Vector2(1120f, -594f), new Vector2(340f, 80f), new Color(0f, 0f, 0f, 0f));
        SetAnchors(characterSlotsParent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        var characterSlotsLayout = characterSlotsParent.gameObject.AddComponent<HorizontalLayoutGroup>();
        characterSlotsLayout.spacing = 12f;
        characterSlotsLayout.childControlWidth = false;
        characterSlotsLayout.childControlHeight = false;
        characterSlotsLayout.childForceExpandWidth = false;
        characterSlotsLayout.childForceExpandHeight = false;

        var startNew = CreateInlineButton("踏入仙途", window, new Vector2(1120f, -710f), new Vector2(150f, 54f));
        var closeCharacter = CreateInlineButton("返回", window, new Vector2(1290f, -710f), new Vector2(150f, 54f));

        controller.blockerButton = blocker;
        controller.startNewGameButton = startNew;
        controller.closeButton = closeCharacter;
        controller.heroNameInput = heroNameInput;
        controller.characterSummaryTitleText = characterSummaryTitle;
        controller.characterSummaryBodyText = characterSummaryBody;
        controller.windowRect = window;
        controller.characterSlotsParent = characterSlotsParent;
        controller.archetypeCardsParent = archetypeParent;
        controller.characterSlotPrefab = characterSlotPrefab;
        controller.archetypeCardPrefab = archetypeCardPrefab;

        SaveAsPrefab(root, MainMenuCharacterCreatePanelPrefabPath, overwrite);
    }

    private static Button CreateMenuButton(string label, Transform parent)
    {
        var root = CreateUiObject(label + "Button", parent);
        var image = root.AddComponent<Image>();
        image.color = new Color(0.11f, 0.12f, 0.13f, 0.92f);
        ApplyOptionalSprite(image, ResolveMenuButtonArtPath(label));
        var button = root.AddComponent<Button>();
        var layout = root.AddComponent<LayoutElement>();
        layout.preferredHeight = 76f;
        layout.minHeight = 76f;

        var accent = CreateImage("Accent", root.transform, new Color(0.76f, 0.59f, 0.29f, 0.95f));
        SetAnchors(accent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        accent.sizeDelta = new Vector2(8f, 0f);

        var text = CreateText("Label", root.transform, Vector2.zero, Vector2.zero, 28, FontStyle.Bold, TextAnchor.MiddleCenter);
        text.text = label;
        Stretch(text.rectTransform);
        text.rectTransform.offsetMin = new Vector2(24f, 0f);
        text.rectTransform.offsetMax = new Vector2(-24f, 0f);
        return button;
    }

    private static Button CreateInlineButton(string label, RectTransform parent, Vector2 anchoredPosition, Vector2 size)
    {
        var root = CreateUiObject(label + "Button", parent);
        var rect = root.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        SetAnchors(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        var image = root.AddComponent<Image>();
        image.color = new Color(0.17f, 0.13f, 0.11f, 0.95f);
        ApplyOptionalSprite(image, ResolveInlineButtonArtPath(label));
        var button = root.AddComponent<Button>();

        var accent = CreateImage("Accent", root.transform, new Color(0.76f, 0.59f, 0.29f, 0.95f));
        SetAnchors(accent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        accent.sizeDelta = new Vector2(5f, 0f);

        var text = CreateText("Label", root.transform, Vector2.zero, Vector2.zero, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
        text.text = label;
        Stretch(text.rectTransform);
        text.rectTransform.offsetMin = new Vector2(14f, 0f);
        text.rectTransform.offsetMax = new Vector2(-14f, 0f);
        return button;
    }

    private static InputField CreateInputField(Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        var root = CreateUiObject("HeroNameInput", parent);
        var rect = root.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        SetAnchors(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        var image = root.AddComponent<Image>();
        image.color = new Color(0.13f, 0.13f, 0.12f, 0.94f);
        var input = root.AddComponent<InputField>();
        input.lineType = InputField.LineType.SingleLine;
        input.characterLimit = 12;

        var accent = CreateImage("Accent", root.transform, new Color(0.76f, 0.59f, 0.29f, 0.95f));
        SetAnchors(accent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        accent.sizeDelta = new Vector2(5f, 0f);

        var text = CreateText("Text", root.transform, Vector2.zero, Vector2.zero, 22, FontStyle.Bold, TextAnchor.MiddleLeft);
        text.text = string.Empty;
        Stretch(text.rectTransform);
        text.rectTransform.offsetMin = new Vector2(16f, 0f);
        text.rectTransform.offsetMax = new Vector2(-16f, 0f);

        var placeholder = CreateText("Placeholder", root.transform, Vector2.zero, Vector2.zero, 20, FontStyle.Normal, TextAnchor.MiddleLeft);
        placeholder.text = "请输入角色名";
        placeholder.color = new Color(0.65f, 0.63f, 0.58f, 0.8f);
        Stretch(placeholder.rectTransform);
        placeholder.rectTransform.offsetMin = new Vector2(16f, 0f);
        placeholder.rectTransform.offsetMax = new Vector2(-16f, 0f);

        input.textComponent = text;
        input.placeholder = placeholder;
        return input;
    }

    private static RectTransform CreatePanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        var panel = CreateUiObject(name, parent);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        panel.AddComponent<Image>().color = color;
        return rect;
    }

    private static GameObject CreateOverlayRoot(string name)
    {
        var root = CreateUiObject(name, null);
        root.layer = 5;
        Stretch(root.GetComponent<RectTransform>());
        var image = root.AddComponent<Image>();
        image.color = new Color(0.02f, 0.02f, 0.03f, 0.74f);
        var button = root.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        return root;
    }

    private static RectTransform CreateImage(string name, Transform parent, Color color)
    {
        var imageObject = CreateUiObject(name, parent);
        imageObject.AddComponent<Image>().color = color;
        return imageObject.GetComponent<RectTransform>();
    }

    private static Text CreateText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, int fontSize, FontStyle fontStyle, TextAnchor alignment)
    {
        var textObject = CreateUiObject(name, parent);
        var rect = textObject.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        SetAnchors(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        var text = textObject.AddComponent<Text>();
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

    private static string ResolveMenuButtonArtPath(string label)
    {
        switch (label)
        {
            case "离开游戏":
                return DangerButtonArtPath;
            case "加载存档":
            case "设置":
                return SecondaryButtonArtPath;
            default:
                return PrimaryButtonArtPath;
        }
    }

    private static string ResolveInlineButtonArtPath(string label)
    {
        switch (label)
        {
            case "删除此档":
                return DangerButtonArtPath;
            case "关闭":
            case "返回":
            case "切换":
            case "+":
            case "-":
                return SecondaryButtonArtPath;
            default:
                return PrimaryButtonArtPath;
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

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        if (parent != null)
        {
            go.transform.SetParent(parent, false);
        }

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);

        return go;
    }

    private static GameObject SaveAsPrefab(GameObject temporaryObject, string path, bool overwrite)
    {
        EnsureFolder(System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/"));

        GameObject prefab;
        if (overwrite || AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            prefab = PrefabUtility.SaveAsPrefabAsset(temporaryObject, path);
        }
        else
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        Object.DestroyImmediate(temporaryObject);
        return prefab;
    }

    private static void EnsureFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        var parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        var folderName = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folderName);
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
