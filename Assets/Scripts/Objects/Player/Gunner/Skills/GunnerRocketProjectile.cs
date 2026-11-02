using ESS.Objects.Enemy;
using UnityEngine;

namespace ESS.Objects.Player.Gunner.Skills
{
	public class GunnerRocketProjectile : MonoBehaviour
	{
		private Vector2 direction;
		private float speed, remainingRange, impactRadius, explosionRadius, knockbackDistance, knockbackDuration;
		private int damage;
		private GameObject impactEffectPrefab;
		private bool initialized;

		public void Initialize(Vector2 aimDirection, int rocketDamage, float projectileSpeed, float range,
			float hitRadius, float blastRadius, float pushDistance, float pushDuration, SkillVisualConfig visuals)
		{
			direction = aimDirection.sqrMagnitude > Mathf.Epsilon ? aimDirection.normalized : Vector2.right;
			damage = Mathf.Max(rocketDamage, 1);
			speed = Mathf.Max(projectileSpeed, 0.1f);
			remainingRange = Mathf.Max(range, 0.1f);
			impactRadius = Mathf.Max(hitRadius, 0.1f);
			explosionRadius = Mathf.Max(blastRadius, 0.1f);
			knockbackDistance = Mathf.Max(pushDistance, 0f);
			knockbackDuration = Mathf.Max(pushDuration, 0.01f);
			impactEffectPrefab = visuals != null ? visuals.impactEffectPrefab : null;
			transform.up = direction;
			CreateVisual(visuals);
			initialized = true;
		}

		private void Update()
		{
			if (!initialized) return;
			float step = Mathf.Min(speed * Time.deltaTime, remainingRange);
			Vector2 from = transform.position, to = from + direction * step;
			DestroyBossPowersAlong(from, to);
			foreach (IEnemy enemy in FindObjectsOfType<IEnemy>())
			{
				if (enemy == null || enemy.IsDead || DistanceToSegment(enemy.transform.position, from, to) > impactRadius)
					continue;
				transform.position = to;
				Explode(enemy.transform.position);
				return;
			}
			transform.position = to;
			remainingRange -= step;
			if (remainingRange <= 0f) Explode(transform.position);
		}

		private void DestroyBossPowersAlong(Vector2 from, Vector2 to)
		{
			foreach (BossPowerProjectile power in FindObjectsOfType<BossPowerProjectile>())
				if (power != null && DistanceToSegment(power.transform.position, from, to) <= impactRadius)
					Destroy(power.gameObject);
		}

		private void Explode(Vector2 center)
		{
			initialized = false;
			float radiusSquared = explosionRadius * explosionRadius;
			foreach (IEnemy enemy in FindObjectsOfType<IEnemy>())
			{
				if (enemy == null || enemy.IsDead) continue;
				Vector2 offset = (Vector2)enemy.transform.position - center;
				if (offset.sqrMagnitude > radiusSquared) continue;
				enemy.TakeDamage(damage);
				enemy.ApplyKnockback(offset.sqrMagnitude > Mathf.Epsilon ? offset.normalized : direction,
					knockbackDistance, knockbackDuration);
			}
			foreach (BossPowerProjectile power in FindObjectsOfType<BossPowerProjectile>())
				if (power != null && ((Vector2)power.transform.position - center).sqrMagnitude <= radiusSquared)
					Destroy(power.gameObject);
			if (impactEffectPrefab != null) Instantiate(impactEffectPrefab, center, Quaternion.identity);
			Destroy(gameObject);
		}

		private void CreateVisual(SkillVisualConfig visuals)
		{
			if (visuals != null && visuals.projectilePrefab != null)
			{
				GameObject visual = Instantiate(visuals.projectilePrefab, transform);
				visual.transform.localScale = Vector3.one * visuals.projectileScale * 1.5f;
			}
			else if (visuals != null && visuals.projectileSprite != null)
			{
				GameObject visual = new GameObject("RocketVisual");
				visual.transform.SetParent(transform, false);
				visual.transform.localScale = Vector3.one * visuals.projectileScale * 1.5f;
				visual.transform.localRotation = Quaternion.Euler(0f, 0f, visuals.projectileRotationOffset);
				SpriteRenderer sprite = visual.AddComponent<SpriteRenderer>();
				sprite.sprite = visuals.projectileSprite;
				sprite.color = visuals.projectileTint;
				sprite.sortingOrder = 30;
			}
		}

		private static float DistanceToSegment(Vector2 point, Vector2 from, Vector2 to)
		{
			Vector2 segment = to - from;
			float lengthSquared = segment.sqrMagnitude;
			float progress = lengthSquared > Mathf.Epsilon
				? Mathf.Clamp01(Vector2.Dot(point - from, segment) / lengthSquared) : 0f;
			return Vector2.Distance(point, from + segment * progress);
		}
	}
}