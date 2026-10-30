using UnityEngine;

namespace ESS.Objects.Player.Sword.Skills
{
	public class W : SwordSkill
	{
		[Header("Shield Skill")]
		[SerializeField, Min(0.1f)] private float duration = 4f;
		[SerializeField, Min(1)] private int baseShieldHealth = 100;
		public override string SkillName => "Aegis Shield";
		protected override bool UseProjectileSpriteAsCastEffect => false;

		public override void Execute()
		{
			if (!TryBeginCast()) return;
			float collisionRadius = CreateShieldVisual();
			Owner.GrantShield(baseShieldHealth * Mathf.Max(Level, 1), duration, collisionRadius);
		}

		private float CreateShieldVisual()
		{
			if (Visuals == null || Visuals.projectileSprite == null)
			{
				Collider2D playerCollider = Owner.GetComponent<Collider2D>();
				return playerCollider != null
					? Mathf.Max(playerCollider.bounds.extents.x, playerCollider.bounds.extents.y)
					: 0f;
			}

			GameObject shieldVisual = new GameObject("SwordShieldVisual");
			shieldVisual.transform.SetParent(Owner.transform, false);
			Vector3 parentScale = Owner.transform.lossyScale;
			float scale = Visuals.projectileScale;
			shieldVisual.transform.localScale = new Vector3(
				scale / Mathf.Max(Mathf.Abs(parentScale.x), 0.0001f),
				scale / Mathf.Max(Mathf.Abs(parentScale.y), 0.0001f),
				scale / Mathf.Max(Mathf.Abs(parentScale.z), 0.0001f));

			SpriteRenderer shieldRenderer = shieldVisual.AddComponent<SpriteRenderer>();
			shieldRenderer.sprite = Visuals.projectileSprite;
			shieldRenderer.color = Visuals.projectileTint;

			int sortingOrder = 0;
			foreach (SpriteRenderer characterRenderer in Owner.GetComponentsInChildren<SpriteRenderer>(true))
			{
				if (characterRenderer == shieldRenderer) continue;
				shieldRenderer.sortingLayerID = characterRenderer.sortingLayerID;
				sortingOrder = Mathf.Max(sortingOrder, characterRenderer.sortingOrder);
			}
			shieldRenderer.sortingOrder = sortingOrder + 1;

			Destroy(shieldVisual, duration);
			return Mathf.Max(shieldRenderer.bounds.extents.x, shieldRenderer.bounds.extents.y);
		}

	}
}
