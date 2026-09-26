using UnityEngine;

public class BasicAttack : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private Color projectileColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private GameObject projectilePrefab;

    private float attackTimer;

    private void Awake()
    {
        if (player == null)
        {
            player = GetComponent<Player>();
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