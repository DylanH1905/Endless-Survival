using UnityEngine;

namespace ESS.Objects.Player.Gunner.Skills
{
	public class R : GunnerSkill
	{
		[SerializeField, Min(0.1f)] private float projectileSpeed = 8f;
		[SerializeField, Min(0.1f)] private float maximumRange = 12f;
		[SerializeField, Min(0.1f)] private float impactRadius = 0.4f;
		[SerializeField, Min(0.1f)] private float explosionRadius = 2.5f;
		[SerializeField, Min(1f)] private float damageMultiplier = 4f;
		[SerializeField, Min(0f)] private float knockbackDistance = 3.5f;
		[SerializeField, Min(0.01f)] private float knockbackDuration = 0.25f;
		public override string SkillName => "Siege Bomb";

		public override void Execute()
		{
			if (!TryBeginCast()) return;
			GameObject rocketObject = new GameObject("GunnerSiegeBomb");
			rocketObject.transform.position = Owner.transform.position;
			rocketObject.AddComponent<GunnerRocketProjectile>().Initialize(GetAimDirection(),
				Mathf.Max(1, Mathf.RoundToInt(Owner.attackDamage * damageMultiplier)),
				projectileSpeed, maximumRange, impactRadius, explosionRadius,
				knockbackDistance, knockbackDuration, Visuals);
		}
	}
}