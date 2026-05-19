using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerCultivator : MonoBehaviour
{
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int HitTrigger = Animator.StringToHash("Hit");
    private static readonly int HealTrigger = Animator.StringToHash("Heal");

    [SerializeField] private float baseMoveSpeed = 5.2f;
    [SerializeField] private float baseAttackRange = 1.35f;
    [SerializeField] private float baseAttackCooldown = 0.45f;
    [SerializeField] private int baseAttackDamage = 1;
    [SerializeField] private int baseMaxHealth = 6;

    private Rigidbody2D body;
    private Transform visualPivot;
    private Transform visualRoot;
    private Animator visualAnimator;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer portraitRenderer;
    private SpriteRenderer auraRenderer;
    private Color baseColor = Color.white;
    private GameController controller;
    private Vector2 moveInput;
    private Vector2 minBounds;
    private Vector2 maxBounds;
    private Vector3 restScale = Vector3.one;
    private Vector3 restLocalPosition = Vector3.zero;
    private float nextAttackTime;
    private float nextDamageTime;
    private float moveSpeed;
    private float attackRange;
    private float attackCooldown;
    private int attackDamage;
    private int maxHealth;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsFacingLeft => visualPivot != null && visualPivot.localScale.x < 0f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        ResolveVisualReferences();
        baseColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        moveSpeed = baseMoveSpeed;
        attackRange = baseAttackRange;
        attackCooldown = baseAttackCooldown;
        attackDamage = baseAttackDamage;
        maxHealth = baseMaxHealth;
        currentHealth = maxHealth;
        CaptureVisualRestState();
    }

    public void Configure(GameController gameController, Vector2 minimumBounds, Vector2 maximumBounds, int attackBonusLevel, int vitalityBonusLevel, int initialHealth, int configuredMaxHealth)
    {
        controller = gameController;
        minBounds = minimumBounds;
        maxBounds = maximumBounds;
        moveSpeed = baseMoveSpeed + attackBonusLevel * 0.12f;
        attackRange = baseAttackRange + Mathf.Min(0.3f, attackBonusLevel * 0.04f);
        attackCooldown = Mathf.Max(0.24f, baseAttackCooldown - attackBonusLevel * 0.02f);
        attackDamage = baseAttackDamage + attackBonusLevel / 2;
        maxHealth = Mathf.Max(baseMaxHealth + vitalityBonusLevel, configuredMaxHealth);
        currentHealth = Mathf.Clamp(initialHealth, 1, maxHealth);
        ResolveVisualReferences();
        CaptureVisualRestState();

        if (controller != null)
        {
            controller.OnPlayerHealthChanged(currentHealth, maxHealth);
        }
    }

    private void Update()
    {
        if (controller != null && controller.ShouldBlockRealtimeInput())
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (moveInput.x != 0f && spriteRenderer != null)
        {
            SetFacingLeft(moveInput.x < 0f);
        }

        if ((Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            if (controller != null)
            {
                controller.HandlePlayerAttack(transform.position, attackRange, attackDamage);
            }
        }

        if (visualAnimator == null && visualRoot != null)
        {
            visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, restScale, 1f - Mathf.Exp(-14f * Time.deltaTime));
            visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, restLocalPosition, 1f - Mathf.Exp(-14f * Time.deltaTime));
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, baseColor, 1f - Mathf.Exp(-12f * Time.deltaTime));
        }
    }

    private void FixedUpdate()
    {
        if (controller != null && controller.ShouldBlockRealtimeInput())
        {
            moveInput = Vector2.zero;
            return;
        }

        var targetPosition = body.position + moveInput * (moveSpeed * Time.fixedDeltaTime);
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        body.MovePosition(targetPosition);
    }

    public bool ReceiveDamage(int amount)
    {
        if (Time.time < nextDamageTime || currentHealth <= 0)
        {
            return false;
        }

        nextDamageTime = Time.time + 0.7f;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (controller != null)
        {
            controller.SpawnCombatText(transform.position + new Vector3(0f, 1.18f, 0f), "-" + Mathf.Max(1, amount), new Color(1f, 0.46f, 0.46f, 1f), false);
            controller.SpawnImpactEffect(transform.position + new Vector3(0f, 0.3f, 0f), new Color(1f, 0.48f, 0.48f, 0.94f), false);
        }

        PlayDamageFeedback();
        if (controller != null)
        {
            controller.OnPlayerHealthChanged(currentHealth, maxHealth);
        }

        return true;
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        if (controller != null)
        {
            controller.SpawnCombatText(transform.position + new Vector3(0f, 1.18f, 0f), "+" + amount, new Color(0.52f, 1f, 0.62f, 1f), false);
            controller.SpawnHealEffect(transform.position + new Vector3(0f, 0.3f, 0f), false);
        }

        PlayHealFeedback();
        if (controller != null)
        {
            controller.OnPlayerHealthChanged(currentHealth, maxHealth);
        }
    }

    public void SyncHealth(int currentHp, int maxHp)
    {
        maxHealth = Mathf.Max(1, maxHp);
        currentHealth = Mathf.Clamp(currentHp, 0, maxHealth);
        if (controller != null)
        {
            controller.OnPlayerHealthChanged(currentHealth, maxHealth);
        }
    }

    public void PlayAttackFeedback(Vector3 targetPosition)
    {
        var delta = targetPosition - transform.position;
        if (spriteRenderer != null && Mathf.Abs(delta.x) > 0.05f)
        {
            SetFacingLeft(delta.x < 0f);
            spriteRenderer.color = Color.Lerp(baseColor, Color.white, 0.2f);
        }

        if (visualAnimator != null)
        {
            TriggerAnimation(AttackTrigger);
            return;
        }

        if (visualRoot != null)
        {
            visualRoot.localScale = Vector3.Scale(restScale, new Vector3(0.88f, 1.14f, 1f));
            visualRoot.localPosition = restLocalPosition + new Vector3(0.12f, 0f, 0f);
        }
    }

    public void PlayDamageFeedback()
    {
        if (visualAnimator != null)
        {
            TriggerAnimation(HitTrigger);
        }
        else if (visualRoot != null)
        {
            visualRoot.localScale = Vector3.Scale(restScale, new Vector3(1.12f, 0.9f, 1f));
            visualRoot.localPosition = restLocalPosition + new Vector3(-0.1f, 0f, 0f);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.54f, 0.54f, 1f);
        }
    }

    public void PlayHealFeedback()
    {
        if (visualAnimator != null)
        {
            TriggerAnimation(HealTrigger);
        }
        else if (visualRoot != null)
        {
            visualRoot.localScale = Vector3.Scale(restScale, new Vector3(0.96f, 1.18f, 1f));
            visualRoot.localPosition = restLocalPosition + new Vector3(0f, 0.08f, 0f);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.7f, 1f, 0.74f, 1f);
        }
    }

    public void ApplyPresentation(Sprite portrait, Color accentColor)
    {
        ResolveVisualReferences();

        if (portraitRenderer != null && portrait != null)
        {
            portraitRenderer.sprite = portrait;
        }

        if (spriteRenderer != null)
        {
            baseColor = Color.Lerp(accentColor, Color.white, 0.5f);
            spriteRenderer.color = baseColor;
        }

        if (auraRenderer != null)
        {
            auraRenderer.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.88f);
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
            portraitRenderer = FindRendererByName("HeroPortrait");
        }

        if (auraRenderer == null)
        {
            auraRenderer = FindRendererByName("AuraLine");
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
        visualAnimator.ResetTrigger(HealTrigger);
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
