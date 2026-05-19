using UnityEngine;

public sealed class CombatHitStop : MonoBehaviour
{
    [SerializeField] private float minimumScale = 0.03f;

    private float timer;
    private float activeScale = 1f;
    private float baseFixedDeltaTime;

    private void Awake()
    {
        baseFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
        if (timer <= 0f)
        {
            return;
        }

        timer -= Time.unscaledDeltaTime;
        if (timer > 0f)
        {
            return;
        }

        RestoreTimeScaleIfAllowed();
    }

    public void Trigger(float duration, float scale)
    {
        if (duration <= 0f)
        {
            return;
        }

        var gameFlow = AppRoot.GetGameFlowManager();
        if (gameFlow != null && gameFlow.IsPaused)
        {
            return;
        }

        timer = Mathf.Max(timer, duration);
        activeScale = Mathf.Clamp(scale, minimumScale, 1f);
        Time.timeScale = activeScale;
        Time.fixedDeltaTime = baseFixedDeltaTime * activeScale;
    }

    private void RestoreTimeScaleIfAllowed()
    {
        timer = 0f;
        activeScale = 1f;

        var gameFlow = AppRoot.GetGameFlowManager();
        if (gameFlow != null && gameFlow.IsPaused)
        {
            return;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = baseFixedDeltaTime;
    }
}
