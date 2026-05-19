using UnityEngine;
public sealed class BootSceneBootstrap : MonoBehaviour
{
    [SerializeField] private string defaultSceneName = "Main";
    [SerializeField] private float splashDuration = 0.45f;

    private void Awake()
    {
        AppRoot.EnsureCreated();
        ConfigureCamera();
        SceneFlow.BeginBootFlow(defaultSceneName, splashDuration);
    }

    private void ConfigureCamera()
    {
        var sceneCamera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
        if (sceneCamera == null)
        {
            return;
        }

        sceneCamera.clearFlags = CameraClearFlags.SolidColor;
        sceneCamera.backgroundColor = new Color(0.045f, 0.055f, 0.065f, 1f);
        sceneCamera.orthographic = true;
    }
}
