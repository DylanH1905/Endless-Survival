using System.Collections;
using UnityEngine;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player
{

    public class BasicAttack : PlayerAttack
    {
        [SerializeField] private IPlayer player;
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private Color projectileColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Sprite drawingBowSprite;
        [SerializeField] private float bowDrawDuration = 0.15f;
        [SerializeField] private float arrowPullbackDistance = 0.3f;

        private float attackTimer;
        private SpriteRenderer bowRenderer;
        private Sprite defaultBowSprite;
        private Transform headTransform;
        private Transform arrowTransform;
        private Coroutine bowDrawRoutine;
        private Vector3 arrowRestLocalPosition;
        private bool hasArrowRestPosition;

        private void Awake()
        {
            if (player == null)
            {
                player = GetComponent<IPlayer>();
            }

            headTransform = transform.Find("Head");
            foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (spriteRenderer.transform.name == "Bow")
                {
                    bowRenderer = spriteRenderer;
                    defaultBowSprite = bowRenderer.sprite;
                }

                if (spriteRenderer.transform.name == "Arrow")
                {
                    arrowTransform = spriteRenderer.transform;
                }
            }
        }

        private void Update()
        {
            if (player == null || player.IsDead)
            {
                return;
            }

            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f)
            {
                return;
            }

            IEnemy closestEnemy = FindClosestEnemy();
            if (closestEnemy == null)
            {
                target = null;
                return;
            }

            float distance = Vector2.Distance(transform.position, closestEnemy.transform.position);
            if (distance <= attackRange)
            {
                target = closestEnemy;
            }
            else
            {
                target = null;
                LaunchProjectile(closestEnemy);
                attackTimer = Mathf.Max(attackCooldown, 0.1f);
            }
        }

        private void LaunchProjectile(IEnemy target)
        {
            if (bowDrawRoutine != null)
            {
                StopCoroutine(bowDrawRoutine);
                if (bowRenderer != null)
                {
                    bowRenderer.sprite = defaultBowSprite;
                }

                if (arrowTransform != null && hasArrowRestPosition)
                {
                    arrowTransform.localPosition = arrowRestLocalPosition;
                }
            }

            if (arrowTransform != null && !hasArrowRestPosition)
            {
                arrowRestLocalPosition = arrowTransform.localPosition;
                hasArrowRestPosition = true;
            }

            bowDrawRoutine = StartCoroutine(DrawAndFire(target));
        }

        private IEnumerator DrawAndFire(IEnemy target)
        {
            float duration = Mathf.Max(bowDrawDuration, 0.01f);
            float pullbackDuration = duration * 0.6f;
            Vector3 pulledBackLocalPosition = arrowRestLocalPosition;
            if (arrowTransform != null)
            {
                Transform aimTransform = arrowTransform.parent;
                Vector3 pullDirection = headTransform != null
                    ? arrowTransform.position - headTransform.position
                    : aimTransform != null ? aimTransform.up : arrowTransform.up;
                if (pullDirection.sqrMagnitude <= Mathf.Epsilon)
                {
                    pullDirection = aimTransform != null ? aimTransform.up : arrowTransform.up;
                }

                pullDirection.Normalize();
                Vector3 pullbackOffset = aimTransform != null
                    ? aimTransform.InverseTransformVector(pullDirection * arrowPullbackDistance)
                    : pullDirection * arrowPullbackDistance;
                pulledBackLocalPosition -= pullbackOffset;
            }

            if (bowRenderer != null && drawingBowSprite != null)
            {
                bowRenderer.sprite = drawingBowSprite;
            }

            float elapsed = 0f;
            while (elapsed < pullbackDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / pullbackDuration));
                if (arrowTransform != null)
                {
                    arrowTransform.localPosition = Vector3.Lerp(
                        arrowRestLocalPosition,
                        pulledBackLocalPosition,
                        progress);
                }

                yield return null;
            }

            if (arrowTransform != null)
            {
                arrowTransform.localPosition = pulledBackLocalPosition;
            }

            if (target != null && !target.IsDead)
            {
                SpawnProjectile(target);
            }

            if (arrowTransform != null)
            {
                elapsed = 0f;
                float releaseDuration = duration - pullbackDuration;
                while (elapsed < releaseDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / releaseDuration));
                    arrowTransform.localPosition = Vector3.Lerp(
                        pulledBackLocalPosition,
                        arrowRestLocalPosition,
                        progress);
                    yield return null;
                }

                arrowTransform.localPosition = arrowRestLocalPosition;
            }
            else
            {
                yield return new WaitForSeconds(duration - pullbackDuration);
            }

            if (bowRenderer != null)
            {
                bowRenderer.sprite = defaultBowSprite;
            }

            bowDrawRoutine = null;
        }

        private void SpawnProjectile(IEnemy target)
        {
            GameObject projectileSource = arrowTransform != null
                ? arrowTransform.gameObject
                : projectilePrefab;
            Vector3 spawnPosition = arrowTransform != null
                ? arrowTransform.position
                : transform.position;
            Quaternion spawnRotation = arrowTransform != null
                ? arrowTransform.rotation
                : Quaternion.identity;

            GameObject projectileObject = projectileSource != null
                ? Instantiate(projectileSource, spawnPosition, spawnRotation)
                : new GameObject("BasicProjectile");

            if (projectileSource == null)
            {
                projectileObject.transform.position = spawnPosition;
            }
            else if (arrowTransform != null)
            {
                projectileObject.transform.localScale = arrowTransform.lossyScale;
            }

            BasicProjectile projectile = projectileObject.GetComponent<BasicProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<BasicProjectile>();
            }

            projectile.Initialize(target, player.attackDamage, projectileSpeed, projectileColor);
        }

        private IEnemy FindClosestEnemy()
        {
            IEnemy closestEnemy = null;
            float closestDistance = Mathf.Infinity;

            foreach (IEnemy enemy in FindObjectsOfType<IEnemy>())
            {
                if (enemy.IsDead)
                {
                    continue;
                }

                float distance = (enemy.transform.position - transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }
    }
}