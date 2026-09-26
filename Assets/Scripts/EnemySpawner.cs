using ESS.Objects.Enemy;
using ESS.Objects.Player;
using UnityEngine;

namespace ESS
{

    public class EnemySpawner : SystemMonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Sprite minionSprite;
        [SerializeField] private Sprite minionBiteSprite;
        [SerializeField] private Vector3 minionScale = new Vector3(0.15f, 0.15f, 1f);
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
            if ((enemyPrefab == null && minionSprite == null) || targetCamera == null || EnemyCount() >= maxEnemies || player == null || player.GetComponent<IPlayer>().IsDead)
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
            if ((enemyPrefab == null && minionSprite == null) || targetCamera == null || EnemyCount() >= maxEnemies)
            {
                return;
            }

            Vector2 spawnPosition = GetSpawnPosition();
            float zPosition = player != null ? player.position.z : 0f;
            Vector3 worldPosition = new Vector3(spawnPosition.x, spawnPosition.y, zPosition);
            GameObject spawnedEnemy = enemyPrefab != null
                ? Instantiate(enemyPrefab, worldPosition, Quaternion.identity)
                : CreateMinion(worldPosition);

            if (minionSprite != null)
            {
                SpriteRenderer spriteRenderer = spawnedEnemy.GetComponentInChildren<SpriteRenderer>(true);
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = minionSprite;
                }
            }

            FocusPlayer focusPlayer = spawnedEnemy.GetComponentInChildren<FocusPlayer>(true);
            if (focusPlayer != null)
            {
                focusPlayer.SetSprites(minionSprite, minionBiteSprite);
            }
        }

        private GameObject CreateMinion(Vector3 position)
        {
            GameObject minionObject = new GameObject("Minion");
            minionObject.transform.position = position;
            minionObject.transform.localScale = minionScale;

            SpriteRenderer spriteRenderer = minionObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = minionSprite;
            minionObject.AddComponent<CircleCollider2D>();
            minionObject.AddComponent<Minion>();
            minionObject.AddComponent<FocusPlayer>();

            return minionObject;
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
            return FindObjectsOfType<IEnemy>().Length;
        }
    }
}