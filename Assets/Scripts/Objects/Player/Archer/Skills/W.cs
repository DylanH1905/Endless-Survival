using UnityEngine;

namespace ESS.Objects.Player.Archer.Skills
{
	public class W : ArcherSkill
	{
		[SerializeField, Min(0.1f)] private float range = 8f;
		[SerializeField, Min(0.1f)] private float projectileSpeed = 11f;
		[SerializeField, Min(0.05f)] private float slowDuration = 2.5f;
		[SerializeField, Range(0.1f, 1f)] private float slowedSpeedMultiplier = 0.6f;
		[SerializeField, Range(1f, 45f)] private float spreadAngle = 14f;
		[SerializeField, Min(0f)] private float damageMultiplier = 0.8f;
		public override string SkillName => "Frost Volley";

		public override void Execute()
		{
			if (!TryBeginCast()) return;
			Vector2 aim = GetAimDirection();
			int damage = Mathf.Max(1, Mathf.RoundToInt(Owner.attackDamage * damageMultiplier));
			for (int index = -1; index <= 1; index++)
			{
				Vector2 direction = Quaternion.Euler(0f, 0f, index * spreadAngle) * aim;
				FireArrow(direction, damage, range, projectileSpeed, 0.18f, false,
					new Color(0.35f, 0.85f, 1f), slowDuration, slowedSpeedMultiplier);
			}
		}
	}
}
