using UnityEngine;

public sealed class SpiritEnemy : MonoBehaviour
{
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int HitTrigger = Animator.StringToHash("Hit");

    [SerializeField] private float baseMoveSpeed = 2.3f;
    [SerializeField] private float baseChaseRange = 7f;
    [SerializeField] private float contactDamageInterval = 1f;
    [SerializeField] private int baseMaxHealth = 2;
    [SerializeField] private int baseContactDamage = 1;

    private GameController controller;
    private PlayerCultivator player;
    private Transform visualPivot;
    private Transform visualRoot;
    private Animator visualAnimator;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer portraitRenderer;
    private SpriteRenderer coreRenderer;
    private SpriteRenderer auraRenderer;
    private Color baseColor = Color.white;
    private Vector3 restScale = Vector3.one;
    private Vector3 restLocalPosition = Vector3.zero;
    private float moveSpeed;
    private float chaseRange;
    private int contactDamage;
    private int currentHealth;
    private float nextContactDamageTime;

    private void Awake()
    {
        ResolveVisualReferences();
        CaptureVisualRestState();
        baseColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
    }

    public void Configure(GameController gameController, PlayerCultivator playerController, float moveSpeedMultiplier, int bonusHealth, int bonusDamage)
    {
        controller = gameController;
        player = playerController;
        ResolveVisualReferences();
        baseColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        CaptureVisualRestState();
        moveSpeed = baseMoveSpeed * moveSpeedMultiplier;
        chaseRange = baseChaseRange + bonusHealth * 0.25f;
        contactDamage = baseContactDamage + bonusDamage;
        currentHealth = baseMaxHealth + bonusHealth;
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        var delta = player.transform.position - transform.position;
        if (delta.sqrMagnitude > chaseRange * chaseRange)
        {
            return;
        }

        transform.position += delta.normalized * (moveSpeed * Time.deltaTime);
        if (spriteRenderer != null && Mathf.Abs(delta.x) > 0.05f)
        {
            SetFacingLeft(delta.x < 0f);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextContactDamageTime)
        {
            return;
        }

        var targetPlayer = other.GetComponent<PlayerCultivator>();
        if (targetPlayer == null)
        {
            return;
        }

        if (targetPlayer.ReceiveDamage(contactDamage))
        {
            PlayAttackFeedback(targetPlayer.transform.position);
            if (controller != null)
            {
                controller.SpawnAttackEffect(transform.position, targetPlayer.transform.position, new Color(1f, 0.62f, 0.52f, 0.92f), false);
            }

            nextContactDamageTime = Time.time + contactDamageInterval;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (controller != null)
        {
            controller.SpawnImpactEffect(transform.position + new Vector3(0f, 0.24f, 0f), new Color(1f, 0.88f, 0.48f, 0.92f), true);
            controller.SpawnCombatText(transform.position + new Vector3(0f, 0.96f, 0f), "-" + Mathf.Max(1, amount), new Color(1f, 0.87f, 0.42f, 1f), true);
        }

        PlayHitFeedback();
        if (currentHealth > 0)
        {
            return;
        }

        if (controller != null)
        {
            controller.OnEnemyDefeated(this);
        }

        Destroy(gameObject);
    }

    public void PlayHitFeedback()
    {
        if (visualAnimator != null)
        {
            TriggerAnimation(HitTrigger);
        }
        else if (visualRoot != null)
        {
            visualRoot.localScale = Vector3.Scale(restScale, new Vector3(1.15f, 0.85f, 1f));
            visualRoot.localPosition = restLocalPosition + new Vector3(-0.12f, 0f, 0f);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(baseColor, Color.white, 0.32f);
        }
    }

    public void PlayAttackFeedback(Vector3 targetPosition)
    {
        var direction = targetPosition - transform.position;
        if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.05f)
        {
            SetFacingLeft(direction.x < 0f);
            spriteRenderer.color = Color.Lerp(baseColor, new Color(1f, 0.88f, 0.72f, 1f), 0.22f);
        }

        if (visualAnimator != null)
        {
            TriggerAnimation(AttackTrigger);
            return;
        }

        if (visualRoot != null)
        {
            visualRoot.localScale = Vector3.Scale(restScale, new Vector3(1.22f, 0.82f, 1f));
            visualRoot.localPosition = restLocalPosition + new Vector3(0.14f, 0f, 0f);
        }
    }

    public void ApplyPresentation(Sprite portrait, Color bodyColor, bool elite)
    {
        ResolveVisualReferences();

        if (portraitRenderer != null && portrait != null)
        {
            portraitRenderer.sprite = portrait;
        }

        if (spriteRenderer != null)
        {
            baseColor = elite ? Color.Lerp(bodyColor, Color.white, 0.24f) : bodyColor;
            spriteRenderer.color = baseColor;
        }

        if (coreRenderer != null)
        {
            coreRenderer.color = Color.Lerp(bodyColor, Color.white, elite ? 0.34f : 0.18f);
        }

        if (auraRenderer != null)
        {
            auraRenderer.color = elite
                ? new Color(1f, 0.86f, 0.48f, 0.9f)
                : new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0.6f);
            auraRenderer.gameObject.SetActive(elite);
        }

        if (visualRoot != null)
        {
            visualRoot.localScale = elite ? new Vector3(1.16f, 1.26f, 1f) : new Vector3(0.96f, 1.06f, 1f);
            CaptureVisualRestState();
        }
    }

    private void LateUpdate()
    {
        if (visualAnimator == null && visualRoot != null)
        {
            visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, restScale, 1f - Mathf.Exp(-12f * Time.deltaTime));
            visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, restLocalPosition, 1f - Mathf.Exp(-12f * Time.deltaTime));
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, baseColor, 1f - Mathf.Exp(-10f * Time.deltaTime));
        }
    }

    private void ResolveVisualReferences()
    {
        if (visualRoot == null)
        {
            var visualTransform = transform.Find("VisualPivot/VisualBody");
            if (visualTransform != null)
            {
                visualRoot = visualTransform;
            }
        }

        if (visualPivot == null && visualRoot != null)
        {
            visualPivot = visualRoot.parent;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = FindRendererByName("Backplate");
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        if (portraitRenderer == null)
        {
            portraitRenderer = FindRendererByName("Portrait");
        }

        if (coreRenderer == null)
        {
            coreRenderer = FindRendererByName("Core");
        }

        if (auraRenderer == null)
        {
            auraRenderer = FindRendererByName("EliteAura");
        }

        if (visualAnimator == null)
        {
            visualAnimator = visualRoot != null ? visualRoot.GetComponent<Animator>() : GetComponentInChildren<Animator>();
        }
    }

    private void CaptureVisualRestState()
    {
        if (visualRoot == null)
        {
            restScale = Vector3.one;
            restLocalPosition = Vector3.zero;
            return;
        }

        restScale = visualRoot.localScale;
        restLocalPosition = visualRoot.localPosition;
    }

    private void SetFacingLeft(bool facingLeft)
    {
        if (visualPivot == null)
        {
            return;
        }

        visualPivot.localScale = new Vector3(facingLeft ? -1f : 1f, 1f, 1f);
    }

    private void TriggerAnimation(int triggerHash)
    {
        if (visualAnimator == null)
        {
            return;
        }

        visualAnimator.ResetTrigger(AttackTrigger);
        visualAnimator.ResetTrigger(HitTrigger);
        visualAnimator.SetTrigger(triggerHash);
    }

    private SpriteRenderer FindRendererByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var renderers = GetComponentsInChildren<SpriteRenderer>(true);
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (renderer != null && renderer.name == name)
            {
                return renderer;
            }
        }

        return null;
    }
}
