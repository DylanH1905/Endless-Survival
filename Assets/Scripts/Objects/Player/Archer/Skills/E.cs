using UnityEngine;

namespace ESS.Objects.Player.Archer.Skills
{
	public class E : ArcherSkill, IChargeableSkill
	{
		[SerializeField, Min(0.1f)] private float maxChargeDuration = 3f;
		[SerializeField, Min(0.1f)] private float minRange = 4f;
		[SerializeField, Min(0.1f)] private float maxRange = 15f;
		[SerializeField, Min(0.1f)] private float projectileSpeed = 13f;
		[SerializeField, Min(0f)] private float damageMultiplier = 2f;
		private float chargeTime;

		public override string SkillName => "Piercing Draw";
		public bool IsCharging { get; private set; }
		public float ChargeProgress => Mathf.Clamp01(chargeTime / maxChargeDuration);

		public void BeginCharge()
		{
			if (!CanExecute || IsCharging) return;
			chargeTime = 0f;
			IsCharging = true;
		}

		public void ReleaseCharge()
		{
			if (!IsCharging) return;
			IsCharging = false;
			if (!TryBeginCast()) return;

			float charge = ChargeProgress;
			float range = Mathf.Lerp(minRange, maxRange, charge);
			int damage = Mathf.Max(1, Mathf.RoundToInt(Owner.attackDamage * damageMultiplier * (0.5f + charge * 0.5f)));
			FireArrow(GetAimDirection(), damage, range, projectileSpeed, 0.24f, true,
				new Color(1f, 0.75f, 0.25f));
			chargeTime = 0f;
		}

		public override void Execute()
		{
			BeginCharge();
			ReleaseCharge();
		}

		protected override void Update()
		{
			base.Update();
			if (!IsCharging) return;
			chargeTime = Mathf.Min(maxChargeDuration, chargeTime + Time.deltaTime);
			if (chargeTime >= maxChargeDuration) ReleaseCharge();
		}
	}
}
