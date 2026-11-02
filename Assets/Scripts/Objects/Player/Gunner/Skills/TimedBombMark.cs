using ESS.Objects.Enemy;
using UnityEngine;

namespace ESS.Objects.Player.Gunner.Skills
{
	public class TimedBombMark : MonoBehaviour
	{
		private int damage;
		private int hitsToDetonate;
		private int basicAttackHits;
		private float remainingTime;
		private float explosionRadius;
		private GameObject visualObject;
		private GameObject impactEffectPrefab;
		private SkillVisualConfig visuals;
		private bool initialized;

		public void Initialize(int bombDamage, float fuseDuration, int hitThreshold, float radius,
			SkillVisualConfig skillVisuals)
		{
			damage = Mathf.Max(bombDamage, 1);
			remainingTime = Mathf.Max(fuseDuration, 0.1f);
			hitsToDetonate = Mathf.Max(hitThreshold, 1);
			explosionRadius = Mathf.Max(radius, 0.1f);
			visuals = skillVisuals;
			impactEffectPrefab = visuals != null ? visuals.impactEffectPrefab : null;
			basicAttackHits = 0;
			if (visualObject == null) CreateVisual();
			UpdateVisualScale();
			initialized = true;
		}

		public void RegisterBasicAttackHit()
		{
			if (!initialized || GetComponent<IEnemy>() == null) return;
			basicAttackHits++;
			UpdateVisualScale();
			if (basicAttackHits >= hitsToDetonate) Detonate();
		}

		private void Update()
		{
			if (!initialized) return;
			IEnemy enemy = GetComponent<IEnemy>();
			if (enemy == null || enemy.IsDead) { Destroy(this); return; }
			remainingTime -= Time.deltaTime;
			if (remainingTime <= 0f) Detonate();
		}

		private void Detonate()
		{
			if (!initialized) return;
			initialized = false;
			Vector3 center = transform.position;
			float radiusSquared = explosionRadius * explosionRadius;
			foreach (IEnemy enemy in FindObjectsOfType<IEnemy>())
			{
				if (enemy == null || enemy.IsDead
					|| ((Vector2)enemy.transform.position - (Vector2)center).sqrMagnitude > radiusSquared)
					continue;
				enemy.TakeDamage(damage);
			}
			if (impactEffectPrefab != null) Instantiate(impactEffectPrefab, center, Quaternion.identity);
			if (visualObject != null) Destroy(visualObject);
			Destroy(this);
		}

		private void CreateVisual()
		{
			if (visuals != null && visuals.projectilePrefab != null)
			{
				visualObject = Instantiate(visuals.projectilePrefab, transform);
				visualObject.transform.localPosition = Vector3.up * 0.65f;
				return;
			}
			visualObject = new GameObject("TimedBombVisual");
			visualObject.transform.SetParent(transform, false);
			visualObject.transform.localPosition = Vector3.up * 0.65f;
			SpriteRenderer sprite = visualObject.AddComponent<SpriteRenderer>();
			if (visuals != null && visuals.projectileSprite != null)
			{
				sprite.sprite = visuals.projectileSprite;
				sprite.color = visuals.projectileTint;
				sprite.sortingOrder = 25;
			}
			else
			{
				Destroy(sprite);
				ProximityMine.DrawRing(visualObject, 0.35f, new Color(1f, 0.2f, 0.1f, 1f));
			}
		}

		private void UpdateVisualScale()
		{
			if (visualObject == null) return;
			float baseScale = visuals != null ? visuals.projectileScale : 1f;
			visualObject.transform.localScale = Vector3.one * baseScale * (1f + basicAttackHits * 0.18f);
		}
	}
}