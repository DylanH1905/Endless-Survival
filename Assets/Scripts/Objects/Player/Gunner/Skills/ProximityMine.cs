using ESS.Objects.Enemy;
using UnityEngine;

namespace ESS.Objects.Player.Gunner.Skills
{
	public class ProximityMine : MonoBehaviour
	{
		private int damage;
		private float triggerRadius;
		private float slowDuration;
		private float slowMultiplier;
		private float armingDelay;
		private float lifetime;
		private float age;
		private bool initialized;
		private GameObject impactEffectPrefab;

		public void Initialize(int mineDamage, float radius, float slowTime, float slowFactor,
			float armDelay, float mineLifetime, SkillVisualConfig visuals)
		{
			damage = Mathf.Max(mineDamage, 1);
			triggerRadius = Mathf.Max(radius, 0.1f);
			slowDuration = Mathf.Max(slowTime, 0f);
			slowMultiplier = Mathf.Clamp01(slowFactor);
			armingDelay = Mathf.Max(armDelay, 0f);
			lifetime = Mathf.Max(mineLifetime, 0.1f);
			impactEffectPrefab = visuals != null ? visuals.impactEffectPrefab : null;
			CreateVisual(visuals);
			initialized = true;
		}

		private void Update()
		{
			if (!initialized) return;
			age += Time.deltaTime;
			if (age >= lifetime) { Destroy(gameObject); return; }
			if (age < armingDelay) return;

			foreach (IEnemy enemy in FindObjectsOfType<IEnemy>())
			{
				if (enemy == null || enemy.IsDead
					|| ((Vector2)enemy.transform.position - (Vector2)transform.position).sqrMagnitude > triggerRadius * triggerRadius)
					continue;
				enemy.TakeDamage(damage);
				enemy.ApplySlow(slowDuration, slowMultiplier);
				if (impactEffectPrefab != null) Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
				Destroy(gameObject);
				return;
			}
		}

		private void CreateVisual(SkillVisualConfig visuals)
		{
			if (visuals != null && visuals.projectilePrefab != null)
			{
				GameObject visual = Instantiate(visuals.projectilePrefab, transform);
				visual.transform.localScale = Vector3.one * visuals.projectileScale;
				return;
			}
			GameObject visualObject = new GameObject("MineVisual");
			visualObject.transform.SetParent(transform, false);
			SpriteRenderer sprite = visualObject.AddComponent<SpriteRenderer>();
			if (visuals != null && visuals.projectileSprite != null)
			{
				sprite.sprite = visuals.projectileSprite;
				sprite.color = visuals.projectileTint;
				sprite.sortingOrder = 20;
				visualObject.transform.localScale = Vector3.one * visuals.projectileScale;
			}
			else
			{
				Destroy(sprite);
				DrawRing(visualObject, triggerRadius, new Color(1f, 0.55f, 0.1f, 0.9f));
			}
		}

		public static void DrawRing(GameObject target, float radius, Color color)
		{
			LineRenderer ring = target.AddComponent<LineRenderer>();
			ring.useWorldSpace = false;
			ring.loop = true;
			ring.positionCount = 32;
			ring.startWidth = ring.endWidth = 0.07f;
			ring.startColor = ring.endColor = color;
			ring.sortingOrder = 20;
			Shader shader = Shader.Find("Sprites/Default");
			if (shader != null) ring.material = new Material(shader);
			for (int index = 0; index < ring.positionCount; index++)
			{
				float angle = index * Mathf.PI * 2f / ring.positionCount;
				ring.SetPosition(index, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
			}
		}
	}
}