using UnityEngine;
public sealed class MainMenuBootstrap : MonoBehaviour
{
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
        AppRoot.EnsureCreated();
        SceneFlow.SyncActiveSceneState(SceneFlow.MainMenuSceneName);
        CultivationAudio.PlayMainMenuMusic();
        ConfigureCamera();
        EnsureMenuInstance();
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
        CultivationApp.OpenMainMenuPanel(new MainMenuConfig(gameplaySceneName, gameTitle, gameSubtitle, gameDescription));
    }
}
