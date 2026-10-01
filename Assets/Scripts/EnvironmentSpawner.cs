using System.Collections.Generic;
using ESS.Objects;
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
        [SerializeField] private float screenMargin = 0.1f;
        [SerializeField] private float minimumDistanceFromPlayer = 2f;
        [SerializeField] private float minimumDistanceBetweenObjects = 1.5f;

        private readonly List<Transform> spawnedObjects = new List<Transform>();
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
        }

        private void Update()
        {
            if (targetCamera == null || environmentObjects == null || environmentObjects.Length == 0)
            {
                return;
            }

            spawnedObjects.RemoveAll(existing => existing == null);

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

            Vector2? spawnPosition = GetSpawnPosition();
            if (spawnPosition == null)
            {
                return;
            }

            EnvironmentObjectHandle handle = environmentObjects[Random.Range(0, environmentObjects.Length)];
            if (handle.prefab == null)
            {
                return;
            }

            float zPosition = handle.prefab.transform.position.z;
            Vector3 worldPosition = new Vector3(spawnPosition.Value.x, spawnPosition.Value.y, zPosition);
            GameObject spawned = Instantiate(handle.prefab, worldPosition, Quaternion.identity);
            spawnedObjects.Add(spawned.transform);

            EnvironmentCharacter environmentCharacter = spawned.GetComponent<EnvironmentCharacter>();
            if (environmentCharacter != null)
            {
                environmentCharacter.objectType = handle.type;
            }

            Info($"Spawned {handle.type} at {worldPosition}");
        }

        private Vector2? GetSpawnPosition()
        {
            float margin = Mathf.Clamp(screenMargin, 0f, 0.45f);
            float cameraDistance = Mathf.Abs(targetCamera.transform.position.z);

            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector3 viewportPosition = new Vector3(
                    Random.Range(margin, 1f - margin),
                    Random.Range(margin, 1f - margin),
                    cameraDistance);

                Vector3 worldPosition = targetCamera.ViewportToWorldPoint(viewportPosition);
                Vector2 spawnPosition = new Vector2(worldPosition.x, worldPosition.y);

                if (player != null && Vector2.Distance(spawnPosition, player.position) < minimumDistanceFromPlayer)
                {
                    continue;
                }

                if (IsTooCloseToExisting(spawnPosition))
                {
                    continue;
                }

                return spawnPosition;
            }

            return null;
        }

        private bool IsTooCloseToExisting(Vector2 position)
        {
            foreach (Transform existing in spawnedObjects)
            {
                if (existing != null && Vector2.Distance(position, existing.position) < minimumDistanceBetweenObjects)
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

            Transform farthest = null;
            float farthestDistance = -1f;
            foreach (Transform existing in spawnedObjects)
            {
                if (existing == null)
                {
                    continue;
                }

                float distance = Vector2.Distance(existing.position, player.position);
                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthest = existing;
                }
            }

            if (farthest != null)
            {
                spawnedObjects.Remove(farthest);
                Destroy(farthest.gameObject);
            }
        }
    }
}
