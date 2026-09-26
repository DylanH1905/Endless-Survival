using UnityEngine;

public class FocusPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private bool ignoreOtherEnemies = true;
    [SerializeField] private bool spriteFacesRight = true;
    [SerializeField] private Sprite biteSprite;
    [SerializeField] private float biteDuration = 0.2f;

    private Enemy enemy;
    private SpriteRenderer spriteRenderer;
    private BossWingFlap bossWingFlap;
    private Sprite defaultSprite;
    private float attackTimer;
    private float biteTimer;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        bossWingFlap = GetComponent<BossWingFlap>();
        if (spriteRenderer != null)
        {
            defaultSprite = spriteRenderer.sprite;
        }

        FindPlayer();

        if (ignoreOtherEnemies)
        {
            IgnoreEnemyCollisions();
        }
    }

    private void Update()
    {
        UpdateBiteAnimation();

        if (player == null)
        {
            FindPlayer();
            return;
        }

        Player playerCharacter = player.GetComponent<Player>();
        if (playerCharacter == null || playerCharacter.IsDead)
        {
            return;
        }

        Vector2 toPlayer = player.position - transform.position;
        Vector2 direction = toPlayer.normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        if (spriteRenderer != null && Mathf.Abs(toPlayer.x) > Mathf.Epsilon)
        {
            spriteRenderer.flipX = toPlayer.x < 0f ? spriteFacesRight : !spriteFacesRight;
        }

        if (distance > attackRange)
        {
            float enemySpeed = enemy != null ? enemy.speed : 0f;
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                enemySpeed * Time.deltaTime);
        }
        else
        {
            Attack(playerCharacter);
        }

        attackTimer -= Time.deltaTime;
    }

    private void Attack(Player playerCharacter)
    {
        if (attackTimer > 0f || enemy == null)
        {
            return;
        }

        playerCharacter.TakeDamage(enemy.attackDamage);
        attackTimer = Mathf.Max(attackCooldown, 0.1f);
        if (bossWingFlap != null)
        {
            bossWingFlap.PlayBite();
        }

        if (spriteRenderer != null && biteSprite != null)
        {
            spriteRenderer.sprite = biteSprite;
            biteTimer = biteDuration;
        }
    }

    public void SetSprites(Sprite idleSprite, Sprite attackSprite)
    {
        biteSprite = attackSprite;
        if (idleSprite != null)
        {
            defaultSprite = idleSprite;
            if (spriteRenderer != null && biteTimer <= 0f)
            {
                spriteRenderer.sprite = idleSprite;
            }
        }
    }

    private void UpdateBiteAnimation()
    {
        if (biteTimer <= 0f)
        {
            return;
        }

        biteTimer -= Time.deltaTime;
        if (biteTimer <= 0f && spriteRenderer != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }

    private void FindPlayer()
    {
        Player playerCharacter = FindObjectOfType<Player>();
        if (playerCharacter != null)
        {
            player = playerCharacter.transform;
        }
    }

    private void IgnoreEnemyCollisions()
    {
        Collider2D[] ownColliders = GetComponentsInChildren<Collider2D>();

        foreach (Enemy otherEnemy in FindObjectsOfType<Enemy>())
        {
            if (otherEnemy == enemy)
            {
                continue;
            }

            Collider2D[] otherColliders = otherEnemy.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D ownCollider in ownColliders)
            {
                foreach (Collider2D otherCollider in otherColliders)
                {
                    Physics2D.IgnoreCollision(ownCollider, otherCollider, true);
                }
            }
        }
    }
}