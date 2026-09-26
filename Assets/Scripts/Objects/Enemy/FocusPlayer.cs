using UnityEngine;

public class FocusPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private bool ignoreOtherEnemies = true;
    [SerializeField] private bool spriteFacesRight;
    [SerializeField] private Sprite biteSprite;
    [SerializeField] private float biteDuration = 0.2f;
    [SerializeField] private float powerProjectileSpeed = 8f;

    private Enemy enemy;
    private SpriteRenderer spriteRenderer;
    private Transform bodyTransform;
    private Transform powerTransform;
    private SpriteRenderer powerRenderer;
    private BossWingFlap bossWingFlap;
    private Sprite defaultSprite;
    private Vector3 powerRestBodyLocalPosition;
    private float attackTimer;
    private float biteTimer;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        bossWingFlap = GetComponent<BossWingFlap>();
        Transform body = transform.Find("Body");
        bodyTransform = body != null ? body : transform;
        if (body != null)
        {
            spriteRenderer = body.GetComponent<SpriteRenderer>();
            powerTransform = body.Find("Power");
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        if (spriteRenderer != null)
        {
            defaultSprite = spriteRenderer.sprite;
        }

        CachePowerRenderer();

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

        SyncPowerWithBody();

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

        attackTimer = Mathf.Max(attackCooldown, 0.1f);
        if (bossWingFlap != null)
        {
            bossWingFlap.PlayBite();
        }

        if (powerTransform != null)
        {
            SpawnPowerProjectile(playerCharacter);
            return;
        }

        playerCharacter.TakeDamage(enemy.attackDamage);
        if (spriteRenderer != null && biteSprite != null)
        {
            spriteRenderer.sprite = biteSprite;
            biteTimer = biteDuration;
        }
    }

    public void ConfigurePowerAttack(float range, float projectileSpeed)
    {
        attackRange = range;
        powerProjectileSpeed = projectileSpeed;
        if (powerTransform == null)
        {
            Transform body = transform.Find("Body");
            bodyTransform = body != null ? body : transform;
            powerTransform = body != null ? body.Find("Power") : transform.Find("Power");
            CachePowerRenderer();
        }
    }

    private void CachePowerRenderer()
    {
        if (powerTransform == null)
        {
            return;
        }

        powerRenderer = powerTransform.GetComponent<SpriteRenderer>();
        powerRestBodyLocalPosition = bodyTransform.InverseTransformPoint(powerTransform.position);
    }

    private void SyncPowerWithBody()
    {
        if (powerTransform == null || spriteRenderer == null)
        {
            return;
        }

        Vector3 localPosition = powerRestBodyLocalPosition;
        if (spriteRenderer.flipX)
        {
            localPosition.x = -localPosition.x;
        }

        powerTransform.position = bodyTransform.TransformPoint(localPosition);
        if (powerRenderer != null)
        {
            powerRenderer.flipX = spriteRenderer.flipX;
        }
    }

    private void SpawnPowerProjectile(Player playerCharacter)
    {
        SyncPowerWithBody();
        GameObject projectileObject = Instantiate(
            powerTransform.gameObject,
            powerTransform.position,
            powerTransform.rotation);
        projectileObject.transform.SetParent(null, true);
        projectileObject.transform.localScale = powerTransform.lossyScale;

        BossPowerProjectile projectile = projectileObject.AddComponent<BossPowerProjectile>();
        Vector3 aimVector = playerCharacter.transform.position - powerTransform.position;
        projectile.Initialize(playerCharacter, enemy.attackDamage, powerProjectileSpeed, aimVector);
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