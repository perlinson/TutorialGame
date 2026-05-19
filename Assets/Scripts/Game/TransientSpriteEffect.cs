using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class TransientSpriteEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color baseColor = Color.white;
    private Vector3 startScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;
    private Vector3 velocity;
    private float spinSpeed;
    private float lifetime = 0.2f;
    private float elapsed;

    public void Configure(
        Sprite sprite,
        Color color,
        Vector3 initialScale,
        Vector3 endScale,
        float duration,
        int sortingOrder,
        float angleDegrees,
        Vector3 drift,
        float spinDegreesPerSecond)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        baseColor = color;
        startScale = initialScale;
        targetScale = endScale;
        lifetime = Mathf.Max(0.05f, duration);
        elapsed = 0f;
        velocity = drift;
        spinSpeed = spinDegreesPerSecond;

        transform.localScale = initialScale;
        transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);

        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        var lifeT = Mathf.Clamp01(elapsed / lifetime);
        transform.position += velocity * Time.deltaTime;
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        transform.localScale = Vector3.Lerp(startScale, targetScale, lifeT);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - lifeT);
        }

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
