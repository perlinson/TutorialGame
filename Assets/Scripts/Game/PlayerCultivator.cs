using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerCultivator : MonoBehaviour
{
    [SerializeField] private float baseMoveSpeed = 5.2f;
    [SerializeField] private float baseAttackRange = 1.35f;
    [SerializeField] private float baseAttackCooldown = 0.45f;
    [SerializeField] private int baseAttackDamage = 1;
    [SerializeField] private int baseMaxHealth = 6;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private GameController controller;
    private Vector2 moveInput;
    private Vector2 minBounds;
    private Vector2 maxBounds;
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

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        moveSpeed = baseMoveSpeed;
        attackRange = baseAttackRange;
        attackCooldown = baseAttackCooldown;
        attackDamage = baseAttackDamage;
        maxHealth = baseMaxHealth;
        currentHealth = maxHealth;
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

        if (controller != null)
        {
            controller.OnPlayerHealthChanged(currentHealth, maxHealth);
        }
    }

    private void Update()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (moveInput.x != 0f && spriteRenderer != null)
        {
            spriteRenderer.flipX = moveInput.x < 0f;
        }

        if ((Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            if (controller != null)
            {
                controller.HandlePlayerAttack(transform.position, attackRange, attackDamage);
            }

            transform.localScale = new Vector3(0.88f, 1.12f, 1f);
        }

        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 1f - Mathf.Exp(-14f * Time.deltaTime));
    }

    private void FixedUpdate()
    {
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
            controller.OnPlayerHealthChanged(currentHealth, maxHealth);
        }

        return true;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
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
}
