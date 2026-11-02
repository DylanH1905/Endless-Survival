using UnityEngine;

namespace ESS.Objects.Player.Gunner.Skills
{
	public class W : GunnerSkill
	{
		[SerializeField, Min(0.1f)] private float damageMultiplier = 2f;
		[SerializeField, Min(0.1f)] private float triggerRadius = 1.1f;
		[SerializeField, Min(0.1f)] private float slowDuration = 2.5f;
		[SerializeField, Range(0.1f, 1f)] private float slowMultiplier = 0.5f;
		[SerializeField, Min(0f)] private float armingDelay = 0.3f;
		[SerializeField, Min(0.1f)] private float mineLifetime = 25f;
		public override string SkillName => "Mine Trap";

		public override void Execute()
		{
			if (!TryBeginCast()) return;
			GameObject mineObject = new GameObject("GunnerMine");
			mineObject.transform.position = Owner.transform.position;
			mineObject.AddComponent<ProximityMine>().Initialize(
				Mathf.Max(1, Mathf.RoundToInt(Owner.attackDamage * damageMultiplier)),
				triggerRadius, slowDuration, slowMultiplier, armingDelay, mineLifetime, Visuals);
		}
	}
}