using System.Collections.Generic;
using ESS.Objects;
using ESS.Objects.Environment;
using ESS.Objects.Player;
using UnityEngine;

namespace ESS
{
    public class EnvironmentSpawner : SystemMonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnvironmentObjectHandle[] environmentObjects;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform player;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 1f;
        [SerializeField] private int maxObjects = 40;
        [SerializeField] private float areaSizeMultiplier = 2f;
        [SerializeField] private float minimumDistanceFromPlayer = 2f;
        [SerializeField] private float minimumDistanceBetweenObjects = 1.5f;

        private readonly struct SpawnedObject
        {
            public readonly Transform transform;
            public readonly EnvironmentObjectType type;

            public SpawnedObject(Transform transform, EnvironmentObjectType type)
            {
                this.transform = transform;
                this.type = type;
            }
        }

        private readonly List<SpawnedObject> spawnedObjects = new List<SpawnedObject>();
        private float spawnTimer;
        private Rect spawnArea;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                Warning("No camera found - assign Target Camera or tag a camera 'MainCamera'.");
            }

            if (environmentObjects == null || environmentObjects.Length == 0)
            {
                Warning("No environment objects configured - nothing will spawn.");
            }

            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    player = playerObject.transform;
                }
            }

            if (targetCamera != null)
            {
                ComputeSpawnArea();
            }
        }


        private void ComputeSpawnArea()
        {
            Vector2 center = player != null ? (Vector2)player.position : (Vector2)targetCamera.transform.position;
            float halfHeight = targetCamera.orthographic
                ? targetCamera.orthographicSize
                : Mathf.Abs(targetCamera.transform.position.z) * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * targetCamera.aspect;

            float areaWidth = halfWidth * 2f * areaSizeMultiplier;
            float areaHeight = halfHeight * 2f * areaSizeMultiplier;

            spawnArea = new Rect(center.x - areaWidth * 0.5f, center.y - areaHeight * 0.5f, areaWidth, areaHeight);
        }

        private void Update()
        {
            if (targetCamera == null || environmentObjects == null || environmentObjects.Length == 0 || player.GetComponent<IPlayer>().IsDead == true)
            {
                return;
            }

            ComputeSpawnArea();

            spawnedObjects.RemoveAll(existing => existing.transform == null);

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnRandomObject();
                spawnTimer = Mathf.Max(spawnInterval, 0.1f);
            }
        }

        private void SpawnRandomObject()
        {
            if (spawnedObjects.Count >= maxObjects)
            {
                DespawnFarthestObject();
            }

            EnvironmentObjectHandle handle = SelectSpawnableHandle();
            if (handle == null)
            {
                return;
            }

            if (handle.icon == null)
            {
                Warning($"EnvironmentObjectHandle for {handle.type} has no icon assigned.");
                return;
            }

            Vector2? spawnPosition = GetSpawnPosition(handle);
            if (spawnPosition == null)
            {
                Warning("Could not find a valid spawn position");
                return;
            }

            float zPosition = player != null ? player.position.z : 0f;
            Vector3 worldPosition = new Vector3(spawnPosition.Value.x, spawnPosition.Value.y, zPosition);
            GameObject spawned = CreateEnvironmentObject(handle, worldPosition);
            spawned.transform.localScale = Vector3.one * handle.size;
            spawnedObjects.Add(new SpawnedObject(spawned.transform, handle.type));

            Info($"Spawned {handle.type} at {worldPosition}");
        }

        // Picks a random handle whose per-type maxCount (if any) hasn't been reached yet.
        private EnvironmentObjectHandle SelectSpawnableHandle()
        {
            int startIndex = Random.Range(0, environmentObjects.Length);
            for (int i = 0; i < environmentObjects.Length; i++)
            {
                EnvironmentObjectHandle candidate = environmentObjects[(startIndex + i) % environmentObjects.Length];
                if (candidate.maxCount <= 0 || CountOfType(candidate.type) < candidate.maxCount)
                {
                    return candidate;
                }
            }

            return null;
        }

        private int CountOfType(EnvironmentObjectType type)
        {
            int count = 0;
            foreach (SpawnedObject existing in spawnedObjects)
            {
                if (existing.transform != null && existing.type == type)
                {
                    count++;
                }
            }

            return count;
        }

        private GameObject CreateEnvironmentObject(EnvironmentObjectHandle handle, Vector3 position)
        {
            GameObject spawned = new GameObject(handle.type.ToString());
            spawned.transform.position = position;

            SpriteRenderer spriteRenderer = spawned.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = handle.icon;
            // "Environment" sorting layer renders behind the Default layer used by Player/Enemy
            spriteRenderer.sortingLayerName = "Environment";

            BoxCollider2D collider = spawned.AddComponent<BoxCollider2D>();
            collider.size = handle.icon.bounds.size;
            collider.isTrigger = handle.type == EnvironmentObjectType.Bush || handle.type == EnvironmentObjectType.Shoes;

            EnvironmentCharacter environmentCharacter;
            if (handle.type == EnvironmentObjectType.Bush)
            {
                environmentCharacter = spawned.AddComponent<Bush>();
            }
            else if (handle.type == EnvironmentObjectType.Shoes)
            {
                environmentCharacter = spawned.AddComponent<Shoes>();
            }
            else
            {
                environmentCharacter = spawned.AddComponent<EnvironmentCharacter>();
            }

            environmentCharacter.objectType = handle.type;
            if (handle.type != EnvironmentObjectType.Bush && handle.type != EnvironmentObjectType.Shoes)
            {
                environmentCharacter.blocksMovement = handle.type == EnvironmentObjectType.Terrain
                    || handle.type == EnvironmentObjectType.Stone
                    || handle.type == EnvironmentObjectType.Wall;
            }

            return spawned;
        }

        private Vector2? GetSpawnPosition(EnvironmentObjectHandle handle)
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector2 spawnPosition = new Vector2(
                    Random.Range(spawnArea.xMin, spawnArea.xMax),
                    Random.Range(spawnArea.yMin, spawnArea.yMax));

                if (player != null && Vector2.Distance(spawnPosition, player.position) < minimumDistanceFromPlayer)
                {
                    continue;
                }

                if (IsTooCloseToExisting(spawnPosition, handle.type, handle.minDistanceToSameType))
                {
                    continue;
                }

                return spawnPosition;
            }

            return null;
        }

        // Objects of the same type must respect minDistanceToSameType; objects of different types only
        // need to respect the general minimumDistanceBetweenObjects.
        private bool IsTooCloseToExisting(Vector2 position, EnvironmentObjectType type, float minDistanceToSameType)
        {
            foreach (SpawnedObject existing in spawnedObjects)
            {
                if (existing.transform == null)
                {
                    continue;
                }

                float requiredDistance = existing.type == type ? minDistanceToSameType : minimumDistanceBetweenObjects;
                if (Vector2.Distance(position, existing.transform.position) < requiredDistance)
                {
                    return true;
                }
            }

            return false;
        }

        private void DespawnFarthestObject()
        {
            if (spawnedObjects.Count == 0 || player == null)
            {
                return;
            }

            int farthestIndex = -1;
            float farthestDistance = -1f;
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                Transform existing = spawnedObjects[i].transform;
                if (existing == null)
                {
                    continue;
                }

                float distance = Vector2.Distance(existing.position, player.position);
                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthestIndex = i;
                }
            }

            if (farthestIndex >= 0)
            {
                Transform farthest = spawnedObjects[farthestIndex].transform;
                spawnedObjects.RemoveAt(farthestIndex);
                Destroy(farthest.gameObject);
            }
        }
    }
}
