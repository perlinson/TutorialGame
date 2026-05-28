using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public sealed class FloatingCombatText : MonoBehaviour
{
    private const string ShadowName = "Shadow";

    private TextMesh textMesh;
    private MeshRenderer meshRenderer;
    private TextMesh shadowTextMesh;
    private MeshRenderer shadowMeshRenderer;
    private Color baseColor;
    private Vector3 velocity;
    private float lifetime = 0.8f;
    private float elapsed;
    private float configuredCharacterSize;

    public void Configure(string message, Color color, Vector3 initialVelocity, float duration, int sortingOrder, float characterSize)
    {
        EnsureMeshes();

        lifetime = Mathf.Max(0.2f, duration);
        elapsed = 0f;
        velocity = initialVelocity;
        baseColor = color;
        configuredCharacterSize = characterSize;

        textMesh.text = message;
        textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textMesh.fontSize = 48;
        textMesh.characterSize = characterSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;

        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = sortingOrder;
        }

        if (shadowTextMesh != null)
        {
            shadowTextMesh.text = message;
            shadowTextMesh.font = textMesh.font;
            shadowTextMesh.fontSize = textMesh.fontSize;
            shadowTextMesh.characterSize = characterSize;
            shadowTextMesh.anchor = textMesh.anchor;
            shadowTextMesh.alignment = textMesh.alignment;
            shadowTextMesh.color = new Color(0f, 0f, 0f, 0.6f);
        }

        if (shadowMeshRenderer != null)
        {
            shadowMeshRenderer.sortingOrder = sortingOrder - 1;
        }

        RefreshShadowOffset();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        velocity += Vector3.up * (0.55f * Time.deltaTime);

        var lifeT = Mathf.Clamp01(elapsed / lifetime);
        var alpha = 1f - lifeT;
        transform.localScale = Vector3.one * Mathf.Lerp(1f, 1.16f, lifeT);

        if (textMesh != null)
        {
            textMesh.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }

        if (shadowTextMesh != null)
        {
            shadowTextMesh.color = new Color(0f, 0f, 0f, alpha * 0.62f);
        }

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void EnsureMeshes()
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMesh>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (shadowTextMesh != null)
        {
            return;
        }

        var shadowTransform = transform.Find(ShadowName);
        if (shadowTransform == null)
        {
            var shadowRoot = new GameObject(ShadowName, typeof(TextMesh), typeof(MeshRenderer));
            shadowTransform = shadowRoot.transform;
            shadowTransform.SetParent(transform, false);
        }

        shadowTextMesh = shadowTransform.GetComponent<TextMesh>();
        shadowMeshRenderer = shadowTransform.GetComponent<MeshRenderer>();
    }

    private void RefreshShadowOffset()
    {
        if (shadowTextMesh == null)
        {
            return;
        }

        shadowTextMesh.transform.localPosition = new Vector3(configuredCharacterSize * 0.18f, -configuredCharacterSize * 0.18f, 0.01f);
        shadowTextMesh.transform.localRotation = Quaternion.identity;
        shadowTextMesh.transform.localScale = Vector3.one;
    }
}
