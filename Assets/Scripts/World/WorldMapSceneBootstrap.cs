using UnityEngine;
public sealed class WorldMapSceneBootstrap : CultivationController
{
    [SerializeField] private string mainSceneName = "Main";
    [SerializeField] private string gameplaySceneName = "Game";

    private void Awake()
    {
        AppRoot.EnsureCreated();
        SceneFlow.SyncActiveSceneState(SceneFlow.WorldMapSceneName);
        PlayWorldMapMusic();
        ConfigureCamera();
        EnsureMapInstance();
    }

    private void ConfigureCamera()
    {
        var sceneCamera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
        if (sceneCamera == null)
        {
            return;
        }

        sceneCamera.clearFlags = CameraClearFlags.SolidColor;
        sceneCamera.backgroundColor = new Color(0.05f, 0.07f, 0.09f, 1f);
        sceneCamera.orthographic = true;
    }

    private void EnsureMapInstance()
    {
        OpenWorldMapPanel(gameplaySceneName, mainSceneName);
    }
}
