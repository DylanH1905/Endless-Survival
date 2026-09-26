using System.Collections;
using UnityEngine;

public class BasicAttack : MonoBehaviour
{
    [SerializeField] private Player player;
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
    private Transform arrowTransform;
    private Coroutine bowDrawRoutine;
    private Vector3 arrowRestLocalPosition;
    private bool hasArrowRestPosition;

    private void Awake()
    {
        if (player == null)
        {
            player = GetComponent<Player>();
        }

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

        Enemy closestEnemy = FindClosestEnemy();
        if (closestEnemy == null)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, closestEnemy.transform.position);
        if (distance <= attackRange)
        {
            LaunchProjectile(closestEnemy);
            attackTimer = Mathf.Max(attackCooldown, 0.1f);
        }
    }

    private void LaunchProjectile(Enemy target)
    {
        PlayBowDraw();

        GameObject projectileObject = projectilePrefab != null
            ? Instantiate(projectilePrefab, transform.position, Quaternion.identity)
            : new GameObject("BasicProjectile");

        if (projectilePrefab == null)
        {
            projectileObject.transform.position = transform.position;
        }

        BasicProjectile projectile = projectileObject.GetComponent<BasicProjectile>();
        if (projectile == null)
        {
            projectile = projectileObject.AddComponent<BasicProjectile>();
        }

        projectile.Initialize(target, player.attackDamage, projectileSpeed, projectileColor);
    }

    private void PlayBowDraw()
    {
        if (bowRenderer == null || drawingBowSprite == null)
        {
            return;
        }

        if (bowDrawRoutine != null)
        {
            StopCoroutine(bowDrawRoutine);
            bowRenderer.sprite = defaultBowSprite;
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

        bowDrawRoutine = StartCoroutine(ShowDrawingBow());
    }

    private IEnumerator ShowDrawingBow()
    {
        float duration = Mathf.Max(bowDrawDuration, 0.01f);
        bowRenderer.sprite = drawingBowSprite;

        if (arrowTransform != null)
        {
            Vector3 pulledBackPosition = arrowRestLocalPosition - Vector3.up * arrowPullbackDistance;
            float elapsed = 0f;
            const float pullbackPortion = 0.65f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                if (progress < pullbackPortion)
                {
                    float pullbackProgress = Mathf.SmoothStep(0f, 1f, progress / pullbackPortion);
                    arrowTransform.localPosition = Vector3.Lerp(
                        arrowRestLocalPosition,
                        pulledBackPosition,
                        pullbackProgress);
                }
                else
                {
                    float releaseProgress = Mathf.SmoothStep(
                        0f,
                        1f,
                        (progress - pullbackPortion) / (1f - pullbackPortion));
                    arrowTransform.localPosition = Vector3.Lerp(
                        pulledBackPosition,
                        arrowRestLocalPosition,
                        releaseProgress);
                }

                yield return null;
            }

            arrowTransform.localPosition = arrowRestLocalPosition;
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }

        bowRenderer.sprite = defaultBowSprite;
        bowDrawRoutine = null;
    }

    private Enemy FindClosestEnemy()
    {
        Enemy closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
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