using UnityEngine;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player.Sword.Skills
{
	public class R : SwordSkill
	{
		[SerializeField, Min(0.1f)] private float range = 2.5f;
		[SerializeField, Range(1f, 180f)] private float coneAngle = 100f;
		[SerializeField, Min(1f)] private float damageMultiplier = 4f;
		public override string SkillName => "Heavy Cleave";

		public override void Execute()
		{
			if (!TryBeginCast()) return;
			Vector2 origin = Owner.transform.position;
			IEnemy target = FindClosestEnemy();
			Vector2 direction = target != null
				? ((Vector2)target.transform.position - origin).normalized
				: (Owner.GetComponent<Movement>()?.CurrentMoveDirection ?? Vector2.right);
			if (direction.sqrMagnitude < 0.01f) direction = Vector2.right;
			float damage = Owner.attackDamage * damageMultiplier * (1f + 0.2f * (Level - 1));
			foreach (IEnemy enemy in FindObjectsOfType<IEnemy>())
			{
				if (enemy == null || enemy.IsDead) continue;
				Vector2 toEnemy = (Vector2)enemy.transform.position - origin;
				if (toEnemy.magnitude <= range && (toEnemy.sqrMagnitude < 0.001f || Vector2.Angle(direction, toEnemy) <= coneAngle * 0.5f))
					enemy.TakeDamage(Mathf.RoundToInt(damage));
			}
		}

		private IEnemy FindClosestEnemy()
		{
			IEnemy nearest = null; float best = float.PositiveInfinity;
			foreach (IEnemy enemy in FindObjectsOfType<IEnemy>())
			{
				if (enemy == null || enemy.IsDead) continue;
				float distance = (enemy.transform.position - Owner.transform.position).sqrMagnitude;
				if (distance < best) { nearest = enemy; best = distance; }
			}
			return nearest;
		}
	}
}
