using UnityEngine;
using UnityEngine.UI;

public static class MainMenuRuntimeBuilder
{
    public static MainMenuController Create()
    {
        var root = CreateUiObject("MainMenuRoot", null);
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

        var controller = root.AddComponent<MainMenuController>();

        var backdrop = CreateImage("Backdrop", root.transform, new Color(0.055f, 0.07f, 0.09f, 1f));
        Stretch(backdrop);

        var mist = CreateImage("MistBand", root.transform, new Color(0.2f, 0.24f, 0.21f, 0.34f));
        Stretch(mist);
        mist.offsetMin = new Vector2(0f, 240f);
        mist.offsetMax = new Vector2(0f, -240f);

        var header = CreatePanel("Header", root.transform, new Vector2(110f, -88f), new Vector2(920f, 316f), new Color(0.08f, 0.09f, 0.1f, 0.82f));
        SetAnchors(header, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        AddOutline(header, new Color(0.64f, 0.51f, 0.27f, 0.88f));
        var title = CreateText("Title", header, new Vector2(34f, -72f), new Vector2(680f, 66f), 72, FontStyle.Bold, TextAnchor.UpperLeft);
        var subtitle = CreateText("Subtitle", header, new Vector2(42f, -160f), new Vector2(420f, 30f), 26, FontStyle.Bold, TextAnchor.MiddleLeft);
        var description = CreateText("Description", header, new Vector2(42f, -206f), new Vector2(780f, 70f), 22, FontStyle.Normal, TextAnchor.UpperLeft);
        description.horizontalOverflow = HorizontalWrapMode.Wrap;
        description.verticalOverflow = VerticalWrapMode.Overflow;

        var menu = CreateUiObject("MenuPanel", root.transform).GetComponent<RectTransform>();
        SetAnchors(menu, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        menu.anchoredPosition = new Vector2(110f, 122f);
        menu.sizeDelta = new Vector2(450f, 420f);
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
        AddOutline(info, new Color(0.47f, 0.39f, 0.24f, 0.88f));
        var infoFlavor = CreateText("InfoFlavor", info, new Vector2(42f, -120f), new Vector2(620f, 110f), 22, FontStyle.Normal, TextAnchor.UpperLeft);
        infoFlavor.horizontalOverflow = HorizontalWrapMode.Wrap;
        infoFlavor.verticalOverflow = VerticalWrapMode.Overflow;
        var recentSave = CreateText("RecentSave", info, new Vector2(42f, -310f), new Vector2(620f, 120f), 22, FontStyle.Normal, TextAnchor.UpperLeft);
        recentSave.horizontalOverflow = HorizontalWrapMode.Wrap;
        recentSave.verticalOverflow = VerticalWrapMode.Overflow;

        var footer = CreatePanel("FooterPanel", root.transform, new Vector2(110f, 46f), new Vector2(820f, 48f), new Color(0.07f, 0.08f, 0.09f, 0.82f));
        SetAnchors(footer, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        var status = CreateText("Status", footer, new Vector2(18f, 0f), new Vector2(780f, 48f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);

        var settingsPanel = CreateOverlay("SettingsPanel", root.transform, new Color(0.02f, 0.02f, 0.03f, 0.68f));
        var settingsWindow = CreatePanel("Window", settingsPanel.transform, Vector2.zero, new Vector2(760f, 430f), new Color(0.1f, 0.12f, 0.14f, 0.95f));
        SetAnchors(settingsWindow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        AddOutline(settingsWindow, new Color(0.64f, 0.51f, 0.27f, 0.88f));
        CreateText("SettingsTitle", settingsWindow, new Vector2(40f, -34f), new Vector2(260f, 36f), 34, FontStyle.Bold, TextAnchor.MiddleLeft).text = "洞府设置";
        CreateText("VolumeLabel", settingsWindow, new Vector2(40f, -126f), new Vector2(180f, 28f), 24, FontStyle.Bold, TextAnchor.MiddleLeft).text = "主音量";
        var volumeDown = CreateInlineButton("-", settingsWindow, new Vector2(300f, -114f), new Vector2(64f, 44f));
        var volumeValue = CreateText("VolumeValue", settingsWindow, new Vector2(374f, -114f), new Vector2(96f, 44f), 22, FontStyle.Bold, TextAnchor.MiddleCenter);
        var volumeUp = CreateInlineButton("+", settingsWindow, new Vector2(480f, -114f), new Vector2(64f, 44f));
        CreateText("FullscreenLabel", settingsWindow, new Vector2(40f, -202f), new Vector2(180f, 28f), 24, FontStyle.Bold, TextAnchor.MiddleLeft).text = "显示模式";
        var fullscreenToggle = CreateInlineButton("切换", settingsWindow, new Vector2(300f, -190f), new Vector2(244f, 44f));
        var fullscreenValue = CreateText("FullscreenValue", settingsWindow, new Vector2(554f, -190f), new Vector2(120f, 44f), 22, FontStyle.Bold, TextAnchor.MiddleLeft);
        var resetSettings = CreateInlineButton("恢复默认", settingsWindow, new Vector2(40f, -292f), new Vector2(220f, 52f));
        var closeSettings = CreateInlineButton("关闭", settingsWindow, new Vector2(494f, -330f), new Vector2(180f, 52f));

        var loadPanel = CreateOverlay("LoadPanel", root.transform, new Color(0.02f, 0.02f, 0.03f, 0.74f));
        var loadWindow = CreatePanel("Window", loadPanel.transform, Vector2.zero, new Vector2(1380f, 760f), new Color(0.1f, 0.12f, 0.14f, 0.95f));
        SetAnchors(loadWindow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        AddOutline(loadWindow, new Color(0.64f, 0.51f, 0.27f, 0.88f));
        CreateText("LoadTitle", loadWindow, new Vector2(42f, -34f), new Vector2(260f, 36f), 36, FontStyle.Bold, TextAnchor.MiddleLeft).text = "加载存档";
        var loadSlotsParent = CreateUiObject("Slots", loadWindow.transform).GetComponent<RectTransform>();
        SetAnchors(loadSlotsParent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        loadSlotsParent.anchoredPosition = new Vector2(42f, -146f);
        loadSlotsParent.sizeDelta = new Vector2(520f, 520f);
        var loadSlotsLayout = loadSlotsParent.gameObject.AddComponent<VerticalLayoutGroup>();
        loadSlotsLayout.spacing = 16f;
        loadSlotsLayout.childControlWidth = true;
        loadSlotsLayout.childForceExpandWidth = true;
        loadSlotsLayout.childControlHeight = false;
        loadSlotsLayout.childForceExpandHeight = false;
        loadSlotsParent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var loadDetailTitle = CreateText("DetailTitle", loadWindow, new Vector2(640f, -146f), new Vector2(520f, 34f), 28, FontStyle.Bold, TextAnchor.MiddleLeft);
        var loadDetailBody = CreateText("DetailBody", loadWindow, new Vector2(640f, -204f), new Vector2(600f, 250f), 22, FontStyle.Normal, TextAnchor.UpperLeft);
        loadDetailBody.horizontalOverflow = HorizontalWrapMode.Wrap;
        loadDetailBody.verticalOverflow = VerticalWrapMode.Overflow;
        var loadAction = CreateText("Action", loadWindow, new Vector2(640f, -488f), new Vector2(600f, 30f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
        var loadSelected = CreateInlineButton("载入此档", loadWindow, new Vector2(640f, -536f), new Vector2(204f, 56f));
        var deleteSelected = CreateInlineButton("删除此档", loadWindow, new Vector2(860f, -536f), new Vector2(204f, 56f));
        var closeLoad = CreateInlineButton("关闭", loadWindow, new Vector2(1080f, -536f), new Vector2(160f, 56f));

        var characterPanel = CreateOverlay("CharacterPanel", root.transform, new Color(0.02f, 0.02f, 0.03f, 0.78f));
        var characterWindow = CreatePanel("Window", characterPanel.transform, Vector2.zero, new Vector2(1540f, 860f), new Color(0.1f, 0.12f, 0.14f, 0.95f));
        SetAnchors(characterWindow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        AddOutline(characterWindow, new Color(0.64f, 0.51f, 0.27f, 0.88f));
        CreateText("CharacterTitle", characterWindow, new Vector2(42f, -34f), new Vector2(360f, 38f), 38, FontStyle.Bold, TextAnchor.MiddleLeft).text = "择道入世";
        var archetypeParent = CreateUiObject("Archetypes", characterWindow.transform).GetComponent<RectTransform>();
        SetAnchors(archetypeParent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        archetypeParent.anchoredPosition = new Vector2(42f, -150f);
        archetypeParent.sizeDelta = new Vector2(1020f, 620f);
        var archetypeLayout = archetypeParent.gameObject.AddComponent<HorizontalLayoutGroup>();
        archetypeLayout.spacing = 18f;
        archetypeLayout.childControlWidth = false;
        archetypeLayout.childControlHeight = false;
        archetypeLayout.childForceExpandWidth = false;
        archetypeLayout.childForceExpandHeight = false;
        var characterSummaryTitle = CreateText("SummaryTitle", characterWindow, new Vector2(1120f, -170f), new Vector2(300f, 34f), 30, FontStyle.Bold, TextAnchor.MiddleLeft);
        var characterSummaryBody = CreateText("SummaryBody", characterWindow, new Vector2(1120f, -226f), new Vector2(340f, 220f), 20, FontStyle.Normal, TextAnchor.UpperLeft);
        characterSummaryBody.horizontalOverflow = HorizontalWrapMode.Wrap;
        characterSummaryBody.verticalOverflow = VerticalWrapMode.Overflow;
        CreateText("HeroNameLabel", characterWindow, new Vector2(1120f, -480f), new Vector2(140f, 24f), 18, FontStyle.Bold, TextAnchor.MiddleLeft).text = "修士名号";
        var heroNameInput = CreateInputField(characterWindow.transform, new Vector2(1120f, -514f), new Vector2(320f, 48f));
        var characterSlotsParent = CreateUiObject("CharacterSlots", characterWindow.transform).GetComponent<RectTransform>();
        SetAnchors(characterSlotsParent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        characterSlotsParent.anchoredPosition = new Vector2(1120f, -594f);
        characterSlotsParent.sizeDelta = new Vector2(340f, 80f);
        var characterSlotsLayout = characterSlotsParent.gameObject.AddComponent<HorizontalLayoutGroup>();
        characterSlotsLayout.spacing = 12f;
        characterSlotsLayout.childControlWidth = false;
        characterSlotsLayout.childControlHeight = false;
        characterSlotsLayout.childForceExpandWidth = false;
        characterSlotsLayout.childForceExpandHeight = false;
        var startNew = CreateInlineButton("踏入仙途", characterWindow, new Vector2(1120f, -710f), new Vector2(150f, 54f));
        var closeCharacter = CreateInlineButton("返回", characterWindow, new Vector2(1290f, -710f), new Vector2(150f, 54f));

        controller.titleText = title;
        controller.subtitleText = subtitle;
        controller.descriptionText = description;
        controller.statusText = status;
        controller.infoFlavorText = infoFlavor;
        controller.recentSaveText = recentSave;
        controller.volumeValueText = volumeValue;
        controller.fullscreenValueText = fullscreenValue;
        controller.loadDetailTitleText = loadDetailTitle;
        controller.loadDetailBodyText = loadDetailBody;
        controller.loadActionText = loadAction;
        controller.characterSummaryTitleText = characterSummaryTitle;
        controller.characterSummaryBodyText = characterSummaryBody;
        controller.newGameButton = newGameButton;
        controller.continueButton = continueButton;
        controller.loadButton = loadButton;
        controller.settingsButton = settingsButton;
        controller.quitButton = quitButton;
        controller.volumeDownButton = volumeDown;
        controller.volumeUpButton = volumeUp;
        controller.fullscreenToggleButton = fullscreenToggle;
        controller.resetSettingsButton = resetSettings;
        controller.closeSettingsButton = closeSettings;
        controller.loadSelectedButton = loadSelected;
        controller.deleteSelectedButton = deleteSelected;
        controller.closeLoadPanelButton = closeLoad;
        controller.startNewGameButton = startNew;
        controller.closeCharacterPanelButton = closeCharacter;
        controller.heroNameInput = heroNameInput;
        controller.settingsPanel = settingsPanel.gameObject;
        controller.loadPanel = loadPanel.gameObject;
        controller.characterPanel = characterPanel.gameObject;
        controller.loadSlotsParent = loadSlotsParent;
        controller.characterSlotsParent = characterSlotsParent;
        controller.archetypeCardsParent = archetypeParent;
        controller.loadSlotPrefab = CreateLoadSlotTemplate();
        controller.characterSlotPrefab = CreateCharacterSlotTemplate();
        controller.archetypeCardPrefab = CreateArchetypeCardTemplate();

        settingsPanel.gameObject.SetActive(false);
        loadPanel.gameObject.SetActive(false);
        characterPanel.gameObject.SetActive(false);

        return controller;
    }

    private static SaveSlotView CreateLoadSlotTemplate()
    {
        var root = CreateUiObject("LoadSlotTemplate", null);
        root.SetActive(false);
        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 118f);
        var image = root.AddComponent<Image>();
        image.color = new Color(0.13f, 0.14f, 0.16f, 0.82f);
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
        return view;
    }

    private static SaveSlotView CreateCharacterSlotTemplate()
    {
        var root = CreateUiObject("CharacterSlotTemplate", null);
        root.SetActive(false);
        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(106f, 70f);
        var image = root.AddComponent<Image>();
        image.color = new Color(0.16f, 0.15f, 0.14f, 0.82f);
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
        return view;
    }

    private static ArchetypeCardView CreateArchetypeCardTemplate()
    {
        var root = CreateUiObject("ArchetypeCardTemplate", null);
        root.SetActive(false);
        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300f, 608f);
        var image = root.AddComponent<Image>();
        image.color = new Color(0.13f, 0.14f, 0.16f, 0.78f);
        root.AddComponent<RectMask2D>();
        var button = root.AddComponent<Button>();
        var accent = CreateImage("Accent", root.transform, new Color(0.34f, 0.31f, 0.26f, 0.85f));
        SetAnchors(accent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        accent.sizeDelta = new Vector2(8f, 0f);
        var portrait = CreateImage("Portrait", root.transform, new Color(0.24f, 0.2f, 0.17f, 1f));
        var portraitRect = portrait.GetComponent<RectTransform>();
        Stretch(portraitRect);
        var portraitImage = portrait.GetComponent<Image>();
        portraitImage.sprite = GameSpriteLibrary.WhiteSquareSprite;
        portraitImage.preserveAspect = true;
        portrait.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.None;
        var portraitShade = CreateImage("PortraitShade", root.transform, new Color(0.04f, 0.05f, 0.06f, 0.34f));
        Stretch(portraitShade);
        var textBacking = CreateImage("TextBacking", root.transform, new Color(0.05f, 0.05f, 0.06f, 0.56f));
        var textBackingRect = textBacking.GetComponent<RectTransform>();
        textBackingRect.anchorMin = new Vector2(0f, 0f);
        textBackingRect.anchorMax = new Vector2(1f, 0f);
        textBackingRect.pivot = new Vector2(0.5f, 0f);
        textBackingRect.anchoredPosition = Vector2.zero;
        textBackingRect.sizeDelta = new Vector2(0f, 446f);
        var portraitLabel = CreateText("PortraitLabel", root.transform, new Vector2(22f, -24f), new Vector2(240f, 28f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
        var title = CreateText("Title", root.transform, new Vector2(22f, -28f), new Vector2(220f, 40f), 30, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.rectTransform.anchoredPosition = new Vector2(22f, -146f);
        var origin = CreateText("Origin", root.transform, new Vector2(22f, -192f), new Vector2(240f, 22f), 16, FontStyle.Bold, TextAnchor.MiddleLeft);
        var specialty = CreateText("Specialty", root.transform, new Vector2(22f, -220f), new Vector2(240f, 22f), 16, FontStyle.Bold, TextAnchor.MiddleLeft);
        var description = CreateText("Description", root.transform, new Vector2(22f, -264f), new Vector2(240f, 100f), 20, FontStyle.Normal, TextAnchor.UpperLeft);
        description.horizontalOverflow = HorizontalWrapMode.Wrap;
        description.verticalOverflow = VerticalWrapMode.Overflow;
        var trait = CreateText("Trait", root.transform, new Vector2(22f, -410f), new Vector2(240f, 168f), 18, FontStyle.Normal, TextAnchor.UpperLeft);
        trait.horizontalOverflow = HorizontalWrapMode.Wrap;
        trait.verticalOverflow = VerticalWrapMode.Overflow;
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
        return view;
    }

    private static Button CreateMenuButton(string label, Transform parent)
    {
        var root = CreateUiObject(label + "Button", parent);
        var image = root.AddComponent<Image>();
        image.color = new Color(0.11f, 0.12f, 0.13f, 0.92f);
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

    private static RectTransform CreateOverlay(string name, Transform parent, Color color)
    {
        var overlay = CreateUiObject(name, parent);
        var rect = overlay.GetComponent<RectTransform>();
        Stretch(rect);
        overlay.AddComponent<Image>().color = color;
        return rect;
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
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = new Color(0.88f, 0.84f, 0.76f, 0.96f);
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
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

    private static void AddOutline(RectTransform rect, Color color)
    {
        CreateEdge("Top", rect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 2f), color);
        CreateEdge("Bottom", rect, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 2f), color);
        CreateEdge("Left", rect, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(2f, 0f), color);
        CreateEdge("Right", rect, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(2f, 0f), color);
    }

    private static void CreateEdge(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Color color)
    {
        var edge = CreateImage(name, parent, color);
        edge.anchorMin = anchorMin;
        edge.anchorMax = anchorMax;
        edge.pivot = pivot;
        edge.sizeDelta = size;
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
