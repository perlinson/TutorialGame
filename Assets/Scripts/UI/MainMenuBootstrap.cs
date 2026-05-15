using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class MainMenuBootstrap : MonoBehaviour
{
    private const string MenuPrefabPath = "UI/MainMenu/MainMenuRoot";

    [Header("Scene Routing")]
    [SerializeField] private string gameplaySceneName = string.Empty;

    [Header("Copy")]
    [SerializeField] private string gameTitle = "山海问道";
    [SerializeField] private string gameSubtitle = "单机修真 / 2D 角色冒险";
    [SerializeField]
    [TextArea(2, 4)]
    private string gameDescription = "以卷轴、墨色、金铜点缀为主的主界面原型。先把新游戏、继续游戏、加载存档和角色选择流程搭通，后续再接战斗、地图与正式存档系统。";

    private void Awake()
    {
        CultivationApp.EnsureInitialized();
        EnsureEventSystem();
        ConfigureCamera();
        EnsureMenuInstance();
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystem.layer = 5;
    }

    private void ConfigureCamera()
    {
        var sceneCamera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
        if (sceneCamera == null)
        {
            return;
        }

        sceneCamera.clearFlags = CameraClearFlags.SolidColor;
        sceneCamera.backgroundColor = new Color(0.055f, 0.07f, 0.09f, 1f);
        sceneCamera.orthographic = true;
    }

    private void EnsureMenuInstance()
    {
        var existing = FindObjectOfType<MainMenuController>();
        if (existing != null)
        {
            if (!IsUsable(existing, out var existingReason))
            {
                Debug.LogWarning("Discarded stale main menu instance: " + existingReason);
                Destroy(existing.gameObject);
            }
            else
            {
                existing.Initialize(new MainMenuConfig(gameplaySceneName, gameTitle, gameSubtitle, gameDescription));
                return;
            }
        }

        var prefab = Resources.Load<GameObject>(MenuPrefabPath);
        if (prefab != null)
        {
            var instance = Instantiate(prefab);
            instance.name = prefab.name;

            var controller = instance.GetComponent<MainMenuController>();
            if (controller != null && IsUsable(controller, out _))
            {
                controller.Initialize(new MainMenuConfig(gameplaySceneName, gameTitle, gameSubtitle, gameDescription));
                return;
            }

            var reason = controller == null ? "missing MainMenuController" : "serialized references are incomplete";
            Debug.LogWarning("Main menu prefab is stale, falling back to runtime UI: " + reason);
            Destroy(instance);
        }

        var fallbackController = MainMenuRuntimeBuilder.Create();
        fallbackController.Initialize(new MainMenuConfig(gameplaySceneName, gameTitle, gameSubtitle, gameDescription));
    }

    private static bool IsUsable(MainMenuController controller, out string reason)
    {
        if (controller == null)
        {
            reason = "controller is null";
            return false;
        }

        if (!HasReference(controller.titleText, "titleText", out reason) ||
            !HasReference(controller.subtitleText, "subtitleText", out reason) ||
            !HasReference(controller.descriptionText, "descriptionText", out reason) ||
            !HasReference(controller.statusText, "statusText", out reason) ||
            !HasReference(controller.infoFlavorText, "infoFlavorText", out reason) ||
            !HasReference(controller.recentSaveText, "recentSaveText", out reason) ||
            !HasReference(controller.volumeValueText, "volumeValueText", out reason) ||
            !HasReference(controller.fullscreenValueText, "fullscreenValueText", out reason) ||
            !HasReference(controller.loadDetailTitleText, "loadDetailTitleText", out reason) ||
            !HasReference(controller.loadDetailBodyText, "loadDetailBodyText", out reason) ||
            !HasReference(controller.loadActionText, "loadActionText", out reason) ||
            !HasReference(controller.characterSummaryTitleText, "characterSummaryTitleText", out reason) ||
            !HasReference(controller.characterSummaryBodyText, "characterSummaryBodyText", out reason) ||
            !HasReference(controller.newGameButton, "newGameButton", out reason) ||
            !HasReference(controller.continueButton, "continueButton", out reason) ||
            !HasReference(controller.loadButton, "loadButton", out reason) ||
            !HasReference(controller.settingsButton, "settingsButton", out reason) ||
            !HasReference(controller.quitButton, "quitButton", out reason) ||
            !HasReference(controller.volumeDownButton, "volumeDownButton", out reason) ||
            !HasReference(controller.volumeUpButton, "volumeUpButton", out reason) ||
            !HasReference(controller.fullscreenToggleButton, "fullscreenToggleButton", out reason) ||
            !HasReference(controller.resetSettingsButton, "resetSettingsButton", out reason) ||
            !HasReference(controller.closeSettingsButton, "closeSettingsButton", out reason) ||
            !HasReference(controller.loadSelectedButton, "loadSelectedButton", out reason) ||
            !HasReference(controller.deleteSelectedButton, "deleteSelectedButton", out reason) ||
            !HasReference(controller.closeLoadPanelButton, "closeLoadPanelButton", out reason) ||
            !HasReference(controller.startNewGameButton, "startNewGameButton", out reason) ||
            !HasReference(controller.closeCharacterPanelButton, "closeCharacterPanelButton", out reason) ||
            !HasReference(controller.heroNameInput, "heroNameInput", out reason) ||
            !HasReference(controller.settingsPanel, "settingsPanel", out reason) ||
            !HasReference(controller.loadPanel, "loadPanel", out reason) ||
            !HasReference(controller.characterPanel, "characterPanel", out reason) ||
            !HasReference(controller.loadSlotsParent, "loadSlotsParent", out reason) ||
            !HasReference(controller.characterSlotsParent, "characterSlotsParent", out reason) ||
            !HasReference(controller.archetypeCardsParent, "archetypeCardsParent", out reason) ||
            !HasReference(controller.loadSlotPrefab, "loadSlotPrefab", out reason) ||
            !HasReference(controller.characterSlotPrefab, "characterSlotPrefab", out reason) ||
            !HasReference(controller.archetypeCardPrefab, "archetypeCardPrefab", out reason))
        {
            return false;
        }

        var archetypeCard = controller.archetypeCardPrefab;
        if (archetypeCard.GetComponent<RectMask2D>() == null)
        {
            reason = "archetypeCardPrefab is missing RectMask2D";
            return false;
        }

        if (archetypeCard.portraitImage == null || archetypeCard.portraitImage.GetComponent<AspectRatioFitter>() == null)
        {
            reason = "archetypeCardPrefab portrait is missing AspectRatioFitter";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool HasReference(Object target, string fieldName, out string reason)
    {
        if (target != null)
        {
            reason = string.Empty;
            return true;
        }

        reason = fieldName + " is missing";
        return false;
    }
}
