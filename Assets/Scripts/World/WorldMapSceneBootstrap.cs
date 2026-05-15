using UnityEngine;
using UnityEngine.EventSystems;

public sealed class WorldMapSceneBootstrap : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "Main";
    [SerializeField] private string gameplaySceneName = "Game";

    private void Awake()
    {
        CultivationApp.EnsureInitialized();
        EnsureEventSystem();
        ConfigureCamera();
        EnsureMapInstance();
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
        sceneCamera.backgroundColor = new Color(0.05f, 0.07f, 0.09f, 1f);
        sceneCamera.orthographic = true;
    }

    private void EnsureMapInstance()
    {
        var existing = FindObjectOfType<WorldMapController>();
        if (existing != null && existing.sectResidenceButton == null)
        {
            Destroy(existing.gameObject);
            existing = null;
        }

        if (existing == null)
        {
            existing = WorldMapRuntimeBuilder.Create();
        }

        existing.Initialize(gameplaySceneName, mainSceneName);
    }
}
