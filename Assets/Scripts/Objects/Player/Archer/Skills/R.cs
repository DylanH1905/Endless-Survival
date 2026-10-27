using UnityEngine;

namespace ESS.Objects.Player.Archer.Skills
{
	public class R : ArcherSkill
	{
		[SerializeField, Min(0.1f)] private float range = 10f;
		[SerializeField, Min(0.1f)] private float projectileSpeed = 9f;
		[SerializeField, Min(0.1f)] private float stunDuration = 1.5f;
		[SerializeField, Min(0f)] private float damageMultiplier = 2.5f;
		public override string SkillName => "Glacier Arrow";

		public override void Execute()
		{
			if (!TryBeginCast()) return;
			int damage = Mathf.Max(1, Mathf.RoundToInt(Owner.attackDamage * damageMultiplier));
			FireArrow(GetAimDirection(), damage, range, projectileSpeed, 0.48f, true,
				new Color(0.55f, 0.9f, 1f), stunDuration: stunDuration);
		}
	}
}
