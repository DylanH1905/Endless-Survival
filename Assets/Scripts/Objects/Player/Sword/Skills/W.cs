using UnityEngine;

namespace ESS.Objects.Player.Sword.Skills
{
	public class W : SwordSkill
	{
		[Header("Shield Skill")]
		[SerializeField, Min(0.1f)] private float duration = 4f;
		[SerializeField, Min(1)] private int baseShieldHealth = 100;
		public override string SkillName => "Aegis Shield";

		public override void Execute()
		{
			if (!TryBeginCast()) return;
			Owner.GrantShield(baseShieldHealth * Mathf.Max(Level, 1), duration);
		}

	}
}
