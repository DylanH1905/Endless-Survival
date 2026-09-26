using ESS.Objects.Player;
using UnityEngine;

namespace ESS.Objects.Enemy
{
    public class FocusPlayer : MonoBehaviour
    {
        [SerializeField] private IPlayer player;
        [SerializeField] private float attackRange = 0.8f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private bool ignoreOtherEnemies = true;
        [SerializeField] private bool spriteFacesRight;
        [SerializeField] private Sprite biteSprite;
        [SerializeField] private float biteDuration = 0.2f;
        [SerializeField] private float powerProjectileSpeed = 8f;

        private IEnemy enemy;
        private SpriteRenderer spriteRenderer;
        private Transform powerTransform;
        private BossWingFlap bossWingFlap;
        private Sprite defaultSprite;
        private float attackTimer;
        private float biteTimer;

        private void Awake()
        {
            enemy = GetComponent<IEnemy>();
            bossWingFlap = GetComponent<BossWingFlap>();
            Transform body = transform.Find("Body");
            if (body != null)
            {
                spriteRenderer = body.GetComponent<SpriteRenderer>();
                powerTransform = body.Find("Power");
            }

            HidePowerTemplate();

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

            IPlayer playerCharacter = player;
            if (playerCharacter == null || playerCharacter.IsDead)
            {
                return;
            }

            Vector2 toPlayer = player.transform.position - transform.position;
            Vector2 direction = toPlayer.normalized;
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (spriteRenderer != null && Mathf.Abs(toPlayer.x) > Mathf.Epsilon)
            {
                spriteRenderer.flipX = toPlayer.x < 0f ? spriteFacesRight : !spriteFacesRight;
            }

            if (distance > attackRange)
            {
                float enemySpeed = enemy != null ? enemy.speed : 0f;
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    player.transform.position,
                    enemySpeed * Time.deltaTime);
            }
            else
            {
                Attack(playerCharacter);
            }

            attackTimer -= Time.deltaTime;
        }

        private void Attack(IPlayer playerCharacter)
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
                powerTransform = body != null ? body.Find("Power") : transform.Find("Power");
                HidePowerTemplate();
            }
        }

        private void HidePowerTemplate()
        {
            if (powerTransform != null)
            {
                powerTransform.gameObject.SetActive(false);
            }
        }

        private void SpawnPowerProjectile(IPlayer playerCharacter)
        {
            GameObject projectileObject = Instantiate(
                powerTransform.gameObject,
                powerTransform.position,
                powerTransform.rotation);
            projectileObject.transform.SetParent(null, true);
            projectileObject.transform.localScale = powerTransform.lossyScale;

            BossPowerProjectile projectile = projectileObject.AddComponent<BossPowerProjectile>();
            Vector3 aimVector = playerCharacter.transform.position - powerTransform.position;
            projectile.Initialize(playerCharacter, enemy.attackDamage, powerProjectileSpeed, aimVector);
            projectileObject.SetActive(true);
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
            IPlayer playerCharacter = FindObjectOfType<IPlayer>();
            if (playerCharacter != null)
            {
                player = playerCharacter;
            }
        }

        private void IgnoreEnemyCollisions()
        {
            Collider2D[] ownColliders = GetComponentsInChildren<Collider2D>();

            foreach (IEnemy otherEnemy in FindObjectsOfType<IEnemy>())
            {
                if (otherEnemy == enemy)
                {
                    continue;
                }

                Collider2D[] otherColliders = ((MonoBehaviour)otherEnemy).GetComponentsInChildren<Collider2D>();
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
}