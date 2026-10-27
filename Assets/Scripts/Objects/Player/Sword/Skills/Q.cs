using System.Collections.Generic;
using UnityEngine;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player.Sword.Skills
{
	public class Q : SwordSkill
	{
		[SerializeField, Min(1)] private int targetCount = 4;
		[SerializeField, Min(0.1f)] private float firstTargetRange = 5f;
		[SerializeField, Min(0.1f)] private float chainRange = 3f;
		[SerializeField, Min(0f)] private float damageMultiplier = 1.5f;
		public override string SkillName => "Lightning Chain";

		public override void Execute()
		{
			if (!TryBeginCast()) return;
			List<IEnemy> candidates = new List<IEnemy>(FindObjectsOfType<IEnemy>());
			Vector2 origin = Owner.transform.position;
			IEnemy next = FindNearest(candidates, origin, firstTargetRange);
			if (next == null) return;

			for (int hit = 0; hit < targetCount && next != null; hit++)
			{
				next.TakeDamage(Mathf.RoundToInt(Owner.attackDamage * damageMultiplier * (1f + 0.15f * (Level - 1))));
				origin = next.transform.position;
				candidates.Remove(next);
				next = FindNearest(candidates, origin, chainRange);
			}
		}

		private static IEnemy FindNearest(List<IEnemy> enemies, Vector2 origin, float range)
		{
			IEnemy nearest = null;
			float bestDistance = range * range;
			foreach (IEnemy enemy in enemies)
			{
				if (enemy == null || enemy.IsDead) continue;
				float distance = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
				if (distance <= bestDistance) { nearest = enemy; bestDistance = distance; }
			}
			return nearest;
		}
	}
}
