using UnityEngine;

public sealed class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 7.5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        var desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
    }
}
