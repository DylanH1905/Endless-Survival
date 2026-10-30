using System.Collections;
using UnityEngine;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player.Sword.Skills
{
	public class R : SwordSkill
	{
		[SerializeField, Min(0.1f)] private float range = 2.5f;
		[SerializeField, Range(1f, 180f)] private float coneAngle = 100f;
		[SerializeField, Min(1f)] private float damageMultiplier = 4f;
		[SerializeField, Min(0.1f)] private float effectTravelDistance = 2f;
		[SerializeField, Min(0.05f)] private float effectDuration = 0.3f;
		public override string SkillName => "Heavy Cleave";
		protected override bool UseProjectileSpriteAsCastEffect => false;

		public override void Execute()
		{
			if (!TryBeginCast()) return;
			Vector2 origin = Owner.transform.position;
			IEnemy target = FindClosestEnemy();
			Vector2 direction = target != null
				? ((Vector2)target.transform.position - origin).normalized
				: (Owner.GetComponent<Movement>()?.CurrentMoveDirection ?? Vector2.right);
			if (direction.sqrMagnitude < 0.01f)
			{
				direction = Vector2.right;
			}
			direction.Normalize();
			StartCoroutine(AnimateCleave(direction));

			float damage = Owner.attackDamage * damageMultiplier * (1f + 0.2f * (Level - 1));
			foreach (IEnemy enemy in FindObjectsOfType<IEnemy>())
			{
				if (enemy == null || enemy.IsDead)
				{
					continue;
				}
				Vector2 toEnemy = (Vector2)enemy.transform.position - origin;
				if (toEnemy.magnitude <= range && (toEnemy.sqrMagnitude < 0.001f || Vector2.Angle(direction, toEnemy) <= coneAngle * 0.5f))
				{
					enemy.TakeDamage(Mathf.RoundToInt(damage));
				}
			}
		}

		private IEnumerator AnimateCleave(Vector2 direction)
		{
			if (Visuals == null || (Visuals.projectilePrefab == null && Visuals.projectileSprite == null))
				yield break;

			GameObject effect = new GameObject("HeavyCleaveEffect");
			effect.transform.position = Owner.transform.position;
			effect.transform.rotation = Quaternion.Euler(
				0f,
				0f,
				Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + Visuals.projectileRotationOffset);

			if (Visuals.projectilePrefab != null)
			{
				GameObject prefabVisual = Instantiate(Visuals.projectilePrefab, effect.transform);
				prefabVisual.transform.localPosition = Vector3.zero;
				prefabVisual.transform.localRotation = Quaternion.Euler(0f, 0f, Visuals.projectileRotationOffset);
			}
			else
			{
				GameObject spriteObject = new GameObject("CleaveSprite");
				spriteObject.transform.SetParent(effect.transform, false);
				spriteObject.transform.localRotation = Quaternion.Euler(0f, 0f, Visuals.projectileRotationOffset);
				SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
				spriteRenderer.sprite = Visuals.projectileSprite;
				spriteRenderer.color = Visuals.projectileTint;
				spriteRenderer.sortingOrder = 100;
			}

			float duration = Mathf.Max(effectDuration, 0.05f);
			float scale = Visuals.projectileScale;
			Vector3 start = effect.transform.position;
			Vector3 end = start + (Vector3)(direction * effectTravelDistance);
			float elapsed = 0f;
			while (elapsed < duration)
			{
				if (effect == null) yield break;
				elapsed += Time.deltaTime;
				float progress = Mathf.Clamp01(elapsed / duration);
				effect.transform.position = Vector3.Lerp(start, end, progress);
				effect.transform.localScale = Vector3.one * Mathf.Lerp(scale * 0.25f, scale, progress);
				yield return null;
			}

			if (effect != null) Destroy(effect);
		}

		private IEnemy FindClosestEnemy()
		{
			IEnemy nearest = null; float best = float.PositiveInfinity;
			foreach (IEnemy enemy in FindObjectsOfType<IEnemy>())
			{
				if (enemy == null || enemy.IsDead) continue;
				float distance = (enemy.transform.position - Owner.transform.position).sqrMagnitude;
				if (distance < best) { nearest = enemy; best = distance; }
			}
			return nearest;
		}
	}
}
