using ESS.Objects.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace ESS.Objects.Player.Archer.Skills
{
	public abstract class ArcherSkill : ISkill
	{
		[SerializeField, Min(0.1f)] private float cooldown = 10f;
		[SerializeField, Min(0f)] private float manaCost;
		[SerializeField] private bool isUnlocked = true;
		private int requiredPlayerLevel = 1;
		[SerializeField] private Image skillIcon;

		private float cooldownRemaining;
		private SkillVisualConfig visuals;
		protected IPlayer Owner => GetComponent<IPlayer>() ?? GetComponentInParent<IPlayer>();
		public override float Cooldown => cooldown;
		public override float CooldownRemaining => cooldownRemaining;
		public override void ConfigureCooldown(float value)
		{
			cooldown = Mathf.Max(0.1f, value);
			cooldownRemaining = Mathf.Min(cooldownRemaining, cooldown);
		}
		public override float ManaCost => manaCost;
		public override bool IsUnlocked => isUnlocked && Owner != null && Owner.level >= requiredPlayerLevel;
		public override int RequiredPlayerLevel => requiredPlayerLevel;
		public override void ConfigureRequiredPlayerLevel(int level) => requiredPlayerLevel = Mathf.Max(1, level);
		public override bool CanExecute => IsUnlocked && cooldownRemaining <= 0f && Owner != null && !Owner.IsDead;
		public override Image SkillIcon => skillIcon;
		public override int Level { get; set; } = 1;

		public void ConfigureVisuals(SkillVisualConfig config)
		{
			visuals = config;
		}

		protected bool TryBeginCast()
		{
			if (!CanExecute) return false;
			cooldownRemaining = cooldown;
			PlayCharacterSkillSound();
			if (visuals != null && visuals.castEffectPrefab != null)
				Instantiate(visuals.castEffectPrefab, Owner.transform.position, Quaternion.identity);
			return true;
		}

		protected virtual void Update()
		{
			cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
		}

		public override void LevelUp() => Level++;

		protected IEnemy FindClosestEnemy(float maxRange = Mathf.Infinity)
		{
			IEnemy closest = null;
			float bestDistance = maxRange * maxRange;
			foreach (IEnemy enemy in FindObjectsOfType<IEnemy>())
			{
				if (enemy == null || enemy.IsDead) continue;
				float distance = ((Vector2)enemy.transform.position - (Vector2)Owner.transform.position).sqrMagnitude;
				if (distance <= bestDistance)
				{
					bestDistance = distance;
					closest = enemy;
				}
			}
			return closest;
		}

		protected Vector2 GetAimDirection()
		{
			IEnemy target = FindClosestEnemy();
			if (target != null)
			{
				Vector2 towardTarget = (Vector2)target.transform.position - (Vector2)Owner.transform.position;
				if (towardTarget.sqrMagnitude > Mathf.Epsilon) return towardTarget.normalized;
			}

			Movement movement = Owner.GetComponent<Movement>();
			Vector2 moveDirection = movement != null ? movement.CurrentMoveDirection : Vector2.zero;
			return moveDirection.sqrMagnitude > 0.01f ? moveDirection.normalized : Vector2.right;
		}

		protected void FireArrow(Vector2 direction, int damage, float range, float projectileSpeed,
			float collisionRadius, bool piercing, Color color,
			float slowDuration = 0f, float slowMultiplier = 1f, float stunDuration = 0f,
			SkillVisualConfig skillVisuals = null, bool destroysBossProjectiles = false)
		{
			GameObject arrowObject = new GameObject("ArcherSkillArrow");
			arrowObject.transform.position = Owner.transform.position;
			StraightArrowProjectile projectile = arrowObject.AddComponent<StraightArrowProjectile>();
			projectile.Initialize(direction, damage, range, projectileSpeed, collisionRadius,
				piercing, color, slowDuration, slowMultiplier, stunDuration,
				skillVisuals ?? visuals, destroysBossProjectiles);
		}
	}
}
