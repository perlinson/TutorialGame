using UnityEngine;

public sealed class SpiritEnemy : MonoBehaviour
{
    [SerializeField] private float baseMoveSpeed = 2.3f;
    [SerializeField] private float baseChaseRange = 7f;
    [SerializeField] private float contactDamageInterval = 1f;
    [SerializeField] private int baseMaxHealth = 2;
    [SerializeField] private int baseContactDamage = 1;

    private GameController controller;
    private PlayerCultivator player;
    private SpriteRenderer spriteRenderer;
    private float moveSpeed;
    private float chaseRange;
    private int contactDamage;
    private int currentHealth;
    private float nextContactDamageTime;

    public void Configure(GameController gameController, PlayerCultivator playerController, float moveSpeedMultiplier, int bonusHealth, int bonusDamage)
    {
        controller = gameController;
        player = playerController;
        spriteRenderer = GetComponent<SpriteRenderer>();
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
            spriteRenderer.flipX = delta.x < 0f;
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
            nextContactDamageTime = Time.time + contactDamageInterval;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        PlayHitFeedback();
        if (currentHealth > 0)
        {
            return;
        }

        controller.OnEnemyDefeated(this);
        Destroy(gameObject);
    }

    public void PlayHitFeedback()
    {
        transform.localScale = new Vector3(1.15f, 0.85f, 1f);
    }

    private void LateUpdate()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 1f - Mathf.Exp(-12f * Time.deltaTime));
    }
}
