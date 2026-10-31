using ESS.Objects;
using ESS.Objects.Enemy;
using ESS.Objects.Player;
using UnityEngine;

namespace ESS
{

    public class EnemySpawner : SystemMonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject minionObject;
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
        [SerializeField] private float maxBossSpawnDistance = 4f;
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
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject == null) return;
                player = playerObject.transform;
            }

            IPlayer playerComponent = player.GetComponent<IPlayer>();

            if ((enemyPrefab == null && (minionObject == null || minionSprite == null)) || targetCamera == null || EnemyCount() >= maxEnemies || playerComponent == null || playerComponent.IsDead)
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
            if ((enemyPrefab == null && (minionObject == null || minionSprite == null)) || targetCamera == null || EnemyCount() >= maxEnemies)
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
                BiteSpriteAnimation biteAnimation = focusPlayer.GetComponent<BiteSpriteAnimation>();
                if (biteAnimation == null)
                {
                    biteAnimation = focusPlayer.gameObject.AddComponent<BiteSpriteAnimation>();
                }

                biteAnimation.SetSprites(minionSprite, minionBiteSprite);
            }
        }

        public GameObject SpawnBossPrefab(GameObject bossPrefab)
        {
            if (bossPrefab == null || targetCamera == null)
            {
                return null;
            }

            Vector2 spawnPosition = GetBossSpawnPosition();
            float zPosition = player != null ? player.position.z : 0f;
            return Instantiate(
                bossPrefab,
                new Vector3(spawnPosition.x, spawnPosition.y, zPosition),
                Quaternion.identity
            );
        }

        private Vector2 GetBossSpawnPosition()
        {
            float margin = Mathf.Clamp(screenMargin, 0f, 0.45f);
            float viewportX;

            if (player != null)
            {
                Vector3 playerViewportPosition = targetCamera.WorldToViewportPoint(player.position);
                viewportX = Mathf.Clamp(playerViewportPosition.x, margin, 1f - margin);
            }
            else
            {
                viewportX = Random.value < 0.5f ? margin : 1f - margin;
            }

            float cameraDistance = Mathf.Abs(targetCamera.transform.position.z);
            Vector3 worldPosition = targetCamera.ViewportToWorldPoint(
                new Vector3(viewportX, 1f - margin, cameraDistance));
            Vector2 spawnPosition = new Vector2(worldPosition.x, worldPosition.y);

            if (player != null)
            {
                Vector2 playerPosition = new Vector2(player.position.x, player.position.y);
                Vector2 offset = spawnPosition - playerPosition;
                float maxDistance = Mathf.Max(maxBossSpawnDistance, 0.1f);
                if (offset.magnitude > maxDistance)
                {
                    spawnPosition = playerPosition + offset.normalized * maxDistance;
                }
            }

            return spawnPosition;
        }

        private GameObject CreateMinion(Vector3 position)
        {
            GameObject newMinionObject = Instantiate(minionObject, position, Quaternion.identity);
            newMinionObject.transform.localScale = minionScale;

            SpriteRenderer spriteRenderer = newMinionObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = minionSprite;
            newMinionObject.AddComponent<CircleCollider2D>();
            newMinionObject.AddComponent<Minion>();
            newMinionObject.AddComponent<FocusPlayer>();

            Transform canvasTransform = newMinionObject.transform.Find("Canvas");
            if (canvasTransform == null)
            {
                canvasTransform = newMinionObject.transform.Find("MinionHealthBar");
                if (canvasTransform != null)
                {
                    canvasTransform.name = "Canvas";
                }
                else
                {
                    GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
                    canvasTransform = canvasObject.transform;
                    canvasTransform.SetParent(newMinionObject.transform, false);
                }
            }

            canvasTransform.gameObject.SetActive(true);
            Canvas healthBarCanvas = canvasTransform.GetComponent<Canvas>();
            if (healthBarCanvas == null)
            {
                healthBarCanvas = canvasTransform.gameObject.AddComponent<Canvas>();
            }

            healthBarCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            healthBarCanvas.worldCamera = targetCamera;

            HealthBarUI canvasHealthBar = canvasTransform.GetComponent<HealthBarUI>();
            if (canvasHealthBar != null)
            {
                canvasHealthBar.enabled = false;
                Destroy(canvasHealthBar);
            }

            Transform healthBarUITransform = canvasTransform.Find("HealthBarUI");
            if (healthBarUITransform == null)
            {
                GameObject healthBarUIObject = new GameObject("HealthBarUI", typeof(RectTransform));
                healthBarUITransform = healthBarUIObject.transform;
                healthBarUITransform.SetParent(canvasTransform, false);
            }

            HealthBarUI healthBar = healthBarUITransform.GetComponent<HealthBarUI>();
            if (healthBar == null)
            {
                healthBar = healthBarUITransform.gameObject.AddComponent<HealthBarUI>();
            }

            healthBar.SetTarget(newMinionObject.GetComponent<Minion>());

            return newMinionObject;
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