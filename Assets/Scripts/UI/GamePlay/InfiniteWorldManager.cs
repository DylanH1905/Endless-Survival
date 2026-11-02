
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ESS.UI.GamePlay
{
    public class InfiniteWorldManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Grid worldGrid;
        [SerializeField] private Tilemap chunkPrefab;
        [SerializeField] private TileBase groundTile;
        [SerializeField] private Camera mainCamera;

        [Header("World Settings")]
        [SerializeField] private int renderDistance = 2;

        private readonly Dictionary<Vector2Int, Tilemap> chunks = new();

        private Vector2Int chunkDimensions;
        private Vector2Int currentPlayerChunk;
        private bool initialized;

        private void Start()
        {
            if (!ValidateReferences())
                return;

            chunkDimensions = GetCameraChunkDimensions();
            UpdateChunks();
            initialized = true;
        }

        private void Update()
        {
            if (!initialized)
                return;

            Vector2Int cameraChunkDimensions = GetCameraChunkDimensions();
            if (cameraChunkDimensions != chunkDimensions)
            {
                chunkDimensions = cameraChunkDimensions;

                foreach (Tilemap chunk in chunks.Values)
                {
                    Destroy(chunk.gameObject);
                }

                chunks.Clear();
            }

            Vector2Int playerChunk = GetPlayerChunk();

            // Only update when Player enters another Chunk
            if (playerChunk != currentPlayerChunk || chunks.Count == 0)
            {
                UpdateChunks();
            }
        }

        private bool ValidateReferences()
        {
            if (player == null ||
                worldGrid == null ||
                chunkPrefab == null ||
                groundTile == null)
            {
                Debug.LogError(
                    "InfiniteWorldManager: Missing references!",
                    this
                );

                return false;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null || renderDistance < 0)
            {
                Debug.LogError(
                    "InfiniteWorldManager: Main Camera is missing or render distance is invalid!",
                    this
                );
                return false;
            }

            return true;
        }

        private Vector2Int GetCameraChunkDimensions()
        {
            Vector3 gridOrigin = worldGrid.transform.position;
            float cameraDepth = Vector3.Dot(
                gridOrigin - mainCamera.transform.position,
                mainCamera.transform.forward
            );

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            for (int x = 0; x <= 1; x++)
            {
                for (int y = 0; y <= 1; y++)
                {
                    Vector3 viewportPoint = mainCamera.ViewportToWorldPoint(
                        new Vector3(x, y, cameraDepth)
                    );
                    Vector3Int cell = worldGrid.WorldToCell(viewportPoint);

                    minX = Mathf.Min(minX, cell.x);
                    minY = Mathf.Min(minY, cell.y);
                    maxX = Mathf.Max(maxX, cell.x);
                    maxY = Mathf.Max(maxY, cell.y);
                }
            }

            return new Vector2Int(
                Mathf.Max(1, maxX - minX + 1),
                Mathf.Max(1, maxY - minY + 1)
            );
        }

        private Vector2Int GetPlayerChunk()
        {
            Vector3Int cellPosition =
                worldGrid.WorldToCell(player.position);

            return new Vector2Int(
                Mathf.FloorToInt(
                    cellPosition.x / (float)chunkDimensions.x
                ),
                Mathf.FloorToInt(
                    cellPosition.y / (float)chunkDimensions.y
                )
            );
        }

        private void UpdateChunks()
        {
            currentPlayerChunk = GetPlayerChunk();

            HashSet<Vector2Int> requiredChunks = new();

            // Create surrounding Chunks
            for (int x = -renderDistance;
                 x <= renderDistance; x++)
            {
                for (int y = -renderDistance;
                     y <= renderDistance; y++)
                {
                    Vector2Int coordinate =
                        currentPlayerChunk + new Vector2Int(x, y);

                    requiredChunks.Add(coordinate);

                    if (!chunks.ContainsKey(coordinate))
                    {
                        CreateChunk(coordinate);
                    }
                }
            }

            // Remove Chunks outside render distance
            List<Vector2Int> removeList = new();

            foreach (var chunk in chunks)
            {
                if (!requiredChunks.Contains(chunk.Key))
                {
                    removeList.Add(chunk.Key);
                }
            }

            foreach (Vector2Int coordinate in removeList)
            {
                Destroy(chunks[coordinate].gameObject);
                chunks.Remove(coordinate);
            }
        }

        private void CreateChunk(Vector2Int coordinate)
        {
            Vector3Int cellOrigin = new Vector3Int(
                coordinate.x * chunkDimensions.x,
                coordinate.y * chunkDimensions.y,
                0
            );

            Vector3 worldPosition =
                worldGrid.CellToWorld(cellOrigin);

            Tilemap chunk = Instantiate(
                chunkPrefab,
                worldPosition,
                worldGrid.transform.rotation,
                worldGrid.transform
            );

            chunk.name =
                $"Ground_Chunk_{coordinate.x}_{coordinate.y}";

            TilemapRenderer chunkRenderer =
                chunk.GetComponent<TilemapRenderer>();
            if (chunkRenderer != null)
            {
                chunkRenderer.sortingLayerName = "Environment";
                chunkRenderer.sortingOrder = -1;
            }

            // Fill Chunk with ground tiles
            for (int x = 0; x < chunkDimensions.x; x++)
            {
                for (int y = 0; y < chunkDimensions.y; y++)
                {
                    chunk.SetTile(
                        new Vector3Int(x, y, 0),
                        groundTile
                    );
                }
            }

            chunks.Add(coordinate, chunk);
        }
    }
}
