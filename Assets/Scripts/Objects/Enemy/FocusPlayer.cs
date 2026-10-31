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
        [SerializeField] private float powerProjectileSpeed = 8f;
        [SerializeField] private float wanderInterval = 1.5f;
        [SerializeField] private float hiddenDetectionRange = 1.5f;

        private IEnemy enemy;
        private SpriteRenderer spriteRenderer;
        private Transform bodyTransform;
        private Vector3 bodyBaseScale;
        private Transform powerTransform;
        private float attackTimer;
        private Vector2 wanderDirection;
        private float wanderTimer;

        public event System.Action AttackStarted;

        private void Awake()
        {
            enemy = GetComponent<IEnemy>();
            Transform body = transform.Find("Body");
            if (body != null)
            {
                bodyTransform = body;
                bodyBaseScale = body.localScale;
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

            FindPlayer();

            if (ignoreOtherEnemies)
            {
                IgnoreEnemyCollisions();
            }
        }

        private void Update()
        {
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

            if (enemy != null && enemy.IsStunned)
            {
                attackTimer -= Time.deltaTime;
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            bool canSeePlayer = !playerCharacter.IsHidden || distanceToPlayer <= hiddenDetectionRange;
            if (!canSeePlayer)
            {
                Wander();
                attackTimer -= Time.deltaTime;
                return;
            }

            Vector2 toPlayer = player.transform.position - transform.position;
            Vector2 direction = toPlayer.normalized;
            float distance = distanceToPlayer;

            FaceDirection(toPlayer.x);

            if (distance > attackRange)
            {
                float enemySpeed = enemy != null ? enemy.MovementSpeed : 0f;
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

        private void Wander()
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                wanderDirection = Random.insideUnitCircle.normalized;
                wanderTimer = Mathf.Max(wanderInterval, 0.1f);
            }

            float enemySpeed = enemy != null ? enemy.MovementSpeed : 0f;
            transform.position = (Vector2)transform.position + wanderDirection * enemySpeed * Time.deltaTime;

            FaceDirection(wanderDirection.x);
        }

        private void FaceDirection(float horizontalDirection)
        {
            if (Mathf.Abs(horizontalDirection) <= Mathf.Epsilon)
            {
                return;
            }

            bool shouldFlip = horizontalDirection < 0f ? spriteFacesRight : !spriteFacesRight;
            if (bodyTransform != null)
            {
                Vector3 scale = bodyBaseScale;
                scale.x *= shouldFlip ? -1f : 1f;
                bodyTransform.localScale = scale;

                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = false;
                }
            }
            else if (spriteRenderer != null)
            {
                spriteRenderer.flipX = shouldFlip;
            }
        }

        private void Attack(IPlayer playerCharacter)
        {
            if (attackTimer > 0f || enemy == null)
            {
                return;
            }

            attackTimer = Mathf.Max(attackCooldown, 0.1f);
            AttackStarted?.Invoke();

            if (powerTransform != null)
            {
                SpawnPowerProjectile(playerCharacter);
                return;
            }

            playerCharacter.TakeDamage(enemy.attackDamage);
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
