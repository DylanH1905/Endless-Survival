using UnityEngine;
using ESS.Objects.Enemy;
using ESS.Objects.Player;

namespace ESS
{
	[DefaultExecutionOrder(-100)]
	public class GamePlayManager : SystemMonoBehaviour
	{
		[System.Serializable]
		private class BossWave
		{
			[Min(1)] public int unlockLevel = 5;
			[Min(1)] public int bossLevel = 1;
			public Boss[] sceneBosses;
			public GameObject[] bossPrefabs;
			[System.NonSerialized] public bool spawned;
		}

		public static GamePlayManager Instance { get; private set; }

		[SerializeField] private IPlayer player;
		[SerializeField] private EnemySpawner enemySpawner;
		[SerializeField, Min(0f)] private float enemyHealthIncreasePerLevel = 0.15f;
		[SerializeField, Min(0f)] private float enemyDamageIncreasePerLevel = 0.1f;
		[SerializeField] private BossWave[] bossWaves =
		{
			new BossWave { unlockLevel = 5, bossLevel = 1 },
			new BossWave { unlockLevel = 10, bossLevel = 2 },
			new BossWave { unlockLevel = 15, bossLevel = 3 }
		};
		private Boss[] bossTemplates;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void EnsureInstance()
		{
			if (FindObjectOfType<GamePlayManager>() != null)
			{
				return;
			}

			EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
			GameObject managerObject = spawner != null
				? spawner.gameObject
				: new GameObject("GamePlayManager");
			managerObject.AddComponent<GamePlayManager>();
		}

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			RefreshReferences();
			PrepareBossWaves();
		}

		private void OnEnable()
		{
			IPlayer.OnLevelUp += HandlePlayerLevelUp;
		}

		private void OnDisable()
		{
			IPlayer.OnLevelUp -= HandlePlayerLevelUp;
		}

		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}

		private void Start()
		{
			TrySpawnBossWaves();
		}

		private void Update()
		{
			if (player == null)
			{
				RefreshReferences();
			}

			TrySpawnBossWaves();
		}

		public void ApplyEnemyLevelScaling(IEnemy enemy)
		{
			if (enemy == null)
			{
				return;
			}

			RefreshReferences();
			int level = player != null ? player.level : 1;
			int levelsAboveOne = Mathf.Max(0, level - 1);
			enemy.maxHealth = Mathf.Max(1, Mathf.RoundToInt(
				enemy.maxHealth * (1f + levelsAboveOne * enemyHealthIncreasePerLevel)
			));
			enemy.attackDamage = Mathf.Max(1, Mathf.RoundToInt(
				enemy.attackDamage * (1f + levelsAboveOne * enemyDamageIncreasePerLevel)
			));
			enemy.health = enemy.maxHealth;
		}

		private void RefreshReferences()
		{
			if (player == null)
			{
				player = FindObjectOfType<IPlayer>();
			}

			if (enemySpawner == null)
			{
				enemySpawner = FindObjectOfType<EnemySpawner>();
			}
		}

		private void PrepareBossWaves()
		{
			if (bossWaves == null || bossWaves.Length == 0)
			{
				bossWaves = new[]
				{
					new BossWave { unlockLevel = 5, bossLevel = 1 },
					new BossWave { unlockLevel = 10, bossLevel = 2 },
					new BossWave { unlockLevel = 15, bossLevel = 3 }
				};
			}

			bossTemplates = FindObjectsOfType<Boss>(true);
			foreach (Boss boss in bossTemplates)
			{
				if (boss != null)
				{
					boss.gameObject.SetActive(false);
				}
			}
		}

		private void HandlePlayerLevelUp(IPlayer leveledPlayer)
		{
			player = leveledPlayer;
			TrySpawnBossWaves();
		}

		private void TrySpawnBossWaves()
		{
			if (player == null || player.IsDead || bossWaves == null)
			{
				return;
			}

			foreach (BossWave wave in bossWaves)
			{
				if (wave == null || wave.spawned || player.level < wave.unlockLevel)
				{
					continue;
				}

				if (wave.bossPrefabs != null && wave.bossPrefabs.Length > 0)
				{
					foreach (GameObject bossPrefab in wave.bossPrefabs)
					{
						if (bossPrefab != null)
						{
							SpawnBoss(bossPrefab, wave.bossLevel);
						}
					}
				}
				else if (wave.sceneBosses != null && wave.sceneBosses.Length > 0)
				{
					foreach (Boss template in wave.sceneBosses)
					{
						if (template != null)
						{
							SpawnBoss(template.gameObject, wave.bossLevel);
						}
					}
				}
				else if (bossTemplates != null && bossTemplates.Length > 0 && bossTemplates[0] != null)
				{
					SpawnBoss(bossTemplates[0].gameObject, wave.bossLevel);
				}

				wave.spawned = true;
			}
		}

		private void SpawnBoss(GameObject bossSource, int bossLevel)
		{
			GameObject bossObject = enemySpawner != null
				? enemySpawner.SpawnBossPrefab(bossSource)
				: null;
			if (bossObject == null)
			{
				bossObject = Instantiate(
					bossSource,
					player.transform.position,
					Quaternion.identity
				);
			}

			Boss boss = bossObject.GetComponent<Boss>();
			bossObject.SetActive(true);
			if (boss != null)
			{
				boss.ConfigureBossLevel(bossLevel);
			}
		}
	}
}