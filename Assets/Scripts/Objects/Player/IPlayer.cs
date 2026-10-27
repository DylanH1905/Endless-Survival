using UnityEngine;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player
{
	public class IPlayer : ObjectCharacter
	{
		public static event System.Action<IPlayer> OnDeath;

		[Header("Collider")]
		[SerializeField] private float colliderRadius = 0.4f;

		[Header("Experience")]
		[SerializeField] private int scorePerKill = 10;
		[SerializeField] private int experiencePerKill = 25;
		[SerializeField] private int baseExperienceToNextLevel = 100;
		[SerializeField] private float experienceToNextLevelGrowth = 1.25f;

		public int score { get; private set; }
		public int level { get; private set; }
		public int experience { get; private set; }
		public int experienceToNextLevel { get; private set; }

		protected virtual void Awake()
		{
			InitializeStats();
			EnsureCollider();
		}

		private void OnEnable()
		{
			IEnemy.OnDeath += HandleEnemyDeath;
		}

		private void OnDisable()
		{
			IEnemy.OnDeath -= HandleEnemyDeath;
		}

		protected virtual void ConfigureBaseStats()
		{
			maxHealth = 2000;
			attackDamage = 20;
			speed = 4f;
		}

		public void InitializeStats()
		{
			ConfigureBaseStats();

			health = maxHealth;
			IsDead = false;
			IsHidden = false;

			score = 0;
			level = 1;
			experience = 0;
			experienceToNextLevel = baseExperienceToNextLevel;
		}

		private void HandleEnemyDeath(IEnemy enemy)
		{
			if (IsDead)
			{
				return;
			}

			score += scorePerKill;
			AddExperience(experiencePerKill);
		}

		public override void TakeDamage(int damage)
		{
			bool wasDead = IsDead;

			base.TakeDamage(damage);

			if (!wasDead && IsDead)
			{
				OnDeath?.Invoke(this);
			}
		}

		public void AddExperience(int amount)
		{
			if (amount <= 0 || IsDead)
			{
				return;
			}

			experience += amount;

			while (experience >= experienceToNextLevel)
			{
				experience -= experienceToNextLevel;
				LevelUp();
			}
		}

		private void LevelUp()
		{
			level++;
			maxHealth += 200;
			attackDamage += 5;
			speed += 0.2f;

			health = maxHealth;

			experienceToNextLevel = Mathf.RoundToInt(
				experienceToNextLevel * experienceToNextLevelGrowth
			);
		}

		private void EnsureCollider()
		{
			if (GetComponent<Collider2D>() != null)
			{
				return;
			}

			CircleCollider2D playerCollider =
				gameObject.AddComponent<CircleCollider2D>();

			playerCollider.radius = colliderRadius;
		}
	}
}
