using UnityEngine;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player
{
	public abstract class IBasicAttack : IPlayerAttack
	{
		[Header("Basic Attack")]
		[SerializeField] protected float attackRange = 3f;
		[SerializeField] protected float attackCooldown = 0.5f;

		private float attackTimer;

		public float AttacksPerSecond =>
			attackCooldown > 0f ? 1f / attackCooldown : 0f;

		protected virtual void Update()
		{
			if (!CanAttack)
			{
				target = null;
				return;
			}

			attackTimer -= Time.deltaTime;

			target = FindClosestEnemyInRange();

			if (target == null || attackTimer > 0f)
			{
				return;
			}

			Attack(target);

			attackTimer = Mathf.Max(attackCooldown, 0.1f);
		}

		protected virtual IEnemy FindClosestEnemyInRange()
		{
			IEnemy closestEnemy = null;
			float closestDistance = attackRange * attackRange;

			foreach (IEnemy enemy in FindObjectsOfType<IEnemy>())
			{
				if (enemy == null || enemy.IsDead)
				{
					continue;
				}

				float distance =
					(enemy.transform.position - transform.position).sqrMagnitude;

				if (distance <= closestDistance)
				{
					closestDistance = distance;
					closestEnemy = enemy;
				}
			}

			return closestEnemy;
		}

		protected abstract void Attack(IEnemy enemy);
	}
}