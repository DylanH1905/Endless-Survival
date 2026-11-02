using ESS.Objects.Enemy;
using UnityEngine;

namespace ESS.Objects.Player.Gunner.Skills
{
	public class E : GunnerSkill
	{
		[SerializeField, Min(0.1f)] private float targetRange = 10f;
		[SerializeField, Min(0.1f)] private float fuseDuration = 5f;
		[SerializeField, Min(1)] private int hitsToDetonate = 5;
		[SerializeField, Min(1f)] private float damageMultiplier = 3f;
		[SerializeField, Min(0.1f)] private float explosionRadius = 2.5f;
		public override string SkillName => "Timed Bomb";

		public override void Execute()
		{
			IEnemy target = FindClosestEnemy(targetRange);
			if (target == null || !TryBeginCast()) return;
			TimedBombMark bomb = target.GetComponent<TimedBombMark>();
			if (bomb == null) bomb = target.gameObject.AddComponent<TimedBombMark>();
			bomb.Initialize(Mathf.Max(1, Mathf.RoundToInt(Owner.attackDamage * damageMultiplier)),
				fuseDuration, hitsToDetonate, explosionRadius, Visuals);
		}
	}
}