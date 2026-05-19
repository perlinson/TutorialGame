using UnityEngine;

public sealed class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 7.5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float shakeDamping = 16f;

    private Transform target;
    private Vector3 shakeOffset;
    private float shakeTimer;
    private float shakeAmplitude;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    public void AddImpulse(float amplitude, float duration)
    {
        shakeAmplitude = Mathf.Max(shakeAmplitude, Mathf.Max(0f, amplitude));
        shakeTimer = Mathf.Max(shakeTimer, Mathf.Max(0.02f, duration));
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        UpdateShakeOffset();
        var desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired + shakeOffset, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
    }

    private void UpdateShakeOffset()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.unscaledDeltaTime;
            var jitter = Random.insideUnitCircle * shakeAmplitude;
            shakeOffset = new Vector3(jitter.x, jitter.y, 0f);
            shakeAmplitude = Mathf.Lerp(shakeAmplitude, 0f, 1f - Mathf.Exp(-shakeDamping * Time.unscaledDeltaTime));
            return;
        }

        shakeAmplitude = 0f;
        shakeOffset = Vector3.Lerp(shakeOffset, Vector3.zero, 1f - Mathf.Exp(-shakeDamping * Time.unscaledDeltaTime));
    }
}
