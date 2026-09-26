using UnityEngine;

public class EnemySpawner : SystemMonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform player;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxEnemies = 20;
    [SerializeField] private float screenMargin = 0.08f;
    [SerializeField] private float minimumDistanceFromPlayer = 2f;
    [SerializeField] private bool spawnOnStart = true;

    private float spawnTimer;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        spawnTimer = spawnOnStart ? 0f : spawnInterval;
    }

    private void Update()
    {
        if (enemyPrefab == null || targetCamera == null || EnemyCount() >= maxEnemies || player == null || player.GetComponent<Player>().IsDead)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = Mathf.Max(spawnInterval, 0.1f);
        }
    }

    public void SpawnEnemy()
    {
        if (enemyPrefab == null || targetCamera == null || EnemyCount() >= maxEnemies)
        {
            return;
        }

        Vector2 spawnPosition = GetSpawnPosition();
        float zPosition = player != null ? player.position.z : 0f;
        Instantiate(enemyPrefab, new Vector3(spawnPosition.x, spawnPosition.y, zPosition), Quaternion.identity);
    }

    private Vector2 GetSpawnPosition()
    {
        float margin = Mathf.Clamp(screenMargin, 0f, 0.45f);
        float cameraDistance = Mathf.Abs(targetCamera.transform.position.z);
        Vector2 spawnPosition = Vector2.zero;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 viewportPosition = new Vector3(
                Random.Range(margin, 1f - margin),
                Random.Range(margin, 1f - margin),
                cameraDistance);

            Vector3 worldPosition = targetCamera.ViewportToWorldPoint(viewportPosition);
            spawnPosition = new Vector2(worldPosition.x, worldPosition.y);

            if (player == null || Vector2.Distance(spawnPosition, new Vector2(player.position.x, player.position.y)) >= minimumDistanceFromPlayer)
            {
                return spawnPosition;
            }
        }

        Vector3 centerPosition = targetCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, cameraDistance));
        spawnPosition = new Vector2(centerPosition.x, centerPosition.y);
        return spawnPosition;
    }

    private int EnemyCount()
    {
        return FindObjectsOfType<Enemy>().Length;
    }
}