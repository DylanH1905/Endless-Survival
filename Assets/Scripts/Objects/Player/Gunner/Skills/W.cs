using UnityEngine;

namespace ESS.Objects.Player.Gunner.Skills
{
	public class W : GunnerSkill
	{
		private const int MinesPerRing = 5;
		private const int RingCount = 3;
		[SerializeField, Min(0.1f)] private float damageMultiplier = 2f;
		[SerializeField, Min(0.1f)] private float triggerRadius = 1.1f;
		[SerializeField, Min(0.1f)] private float placementRadius = 1.8f;
		[SerializeField, Min(0.1f)] private float slowDuration = 2.5f;
		[SerializeField, Range(0.1f, 1f)] private float slowMultiplier = 0.5f;
		[SerializeField, Min(0f)] private float armingDelay = 0.3f;
		[SerializeField, Min(0.1f)] private float mineLifetime = 25f;
		public override string SkillName => "Mine Trap";

		public override void Execute()
		{
			if (!TryBeginCast()) return;

			Vector3 fieldCenter = Owner.transform.position;
			int mineDamage = Mathf.Max(1, Mathf.RoundToInt(Owner.attackDamage * damageMultiplier));
			for (int index = 0; index < MinesPerRing * RingCount; index++)
			{
				int ring = index / MinesPerRing;
				int positionInRing = index % MinesPerRing;
				float radius = placementRadius * (ring + 1) / RingCount;
				float angle = positionInRing * Mathf.PI * 2f / MinesPerRing
					+ ring * Mathf.PI / MinesPerRing;
				Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f)
					* radius;
				GameObject mineObject = new GameObject("GunnerMine");
				mineObject.transform.position = fieldCenter + offset;
				mineObject.AddComponent<ProximityMine>().Initialize(
					mineDamage, triggerRadius, slowDuration, slowMultiplier,
					armingDelay, mineLifetime, Visuals);
			}
		}
	}
}