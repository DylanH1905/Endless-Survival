using UnityEngine;

namespace ESS.Objects.Player.Gunner.Skills
{
	public class Q : GunnerSkill
	{
		[SerializeField, Min(1f)] private float attackSpeedMultiplier = 1.75f;
		[SerializeField, Min(0.1f)] private float duration = 5f;
		public override string SkillName => "Rapid Fire";

		public override void Execute()
		{
			if (!TryBeginCast()) return;
			BasicAttack attack = Owner.GetComponentInChildren<BasicAttack>();
			if (attack != null) attack.ApplyAttackSpeedBoost(attackSpeedMultiplier, duration);
		}
	}
}