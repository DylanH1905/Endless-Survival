using System.Collections.Generic;
using UnityEngine;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player.Archer.Skills
{
	public class StraightArrowProjectile : MonoBehaviour
	{
		private readonly HashSet<int> hitEnemies = new HashSet<int>();
		private LineRenderer trail;
		private Vector2 direction;
		private Vector2 previousPosition;
		private float speed;
		private float remainingRange;
		private float collisionRadius;
		private bool piercing;
		private int damage;
		private float slowDuration;
		private float slowMultiplier;
		private float stunDuration;
		private bool didImpact;

		public void Initialize(Vector2 aimDirection, int arrowDamage, float range, float projectileSpeed,
			float radius, bool canPierce, Color color, float slowTime, float slowFactor, float stunTime)
		{
			direction = aimDirection.sqrMagnitude > Mathf.Epsilon ? aimDirection.normalized : Vector2.right;
			damage = Mathf.Max(arrowDamage, 1);
			remainingRange = Mathf.Max(range, 0.1f);
			speed = Mathf.Max(projectileSpeed, 0.1f);
			collisionRadius = Mathf.Max(radius, 0.05f);
			piercing = canPierce;
			slowDuration = slowTime;
			slowMultiplier = slowFactor;
			stunDuration = stunTime;
			previousPosition = transform.position;
			transform.right = direction;

			trail = gameObject.AddComponent<LineRenderer>();
			trail.positionCount = 2;
			trail.useWorldSpace = true;
			trail.startWidth = collisionRadius * 1.2f;
			trail.endWidth = collisionRadius * 0.55f;
			trail.startColor = color;
			trail.endColor = new Color(color.r, color.g, color.b, 0.15f);
			trail.numCapVertices = 4;
			trail.sortingLayerName = "Default";
			trail.sortingOrder = 100;
			Shader shader = Shader.Find("Sprites/Default");
			if (shader != null) trail.material = new Material(shader);
		}

		private void Update()
		{
			if (didImpact) return;
			float step = Mathf.Min(speed * Time.deltaTime, remainingRange);
			Vector2 nextPosition = (Vector2)transform.position + direction * step;
			CheckHits(previousPosition, nextPosition);
			if (didImpact) return;

			transform.position = nextPosition;
			previousPosition = nextPosition;
			remainingRange -= step;
			trail.SetPosition(0, nextPosition);
			trail.SetPosition(1, nextPosition - direction * Mathf.Min(0.5f, remainingRange + step));
			if (remainingRange <= 0f) Destroy(gameObject);
		}

		private void CheckHits(Vector2 from, Vector2 to)
		{
			foreach (IEnemy enemy in FindObjectsOfType<IEnemy>())
			{
				if (enemy == null || enemy.IsDead || hitEnemies.Contains(enemy.GetInstanceID())) continue;
				Vector2 point = enemy.transform.position;
				Vector2 closestPoint = ClosestPointOnSegment(from, to, point);
				if ((point - closestPoint).sqrMagnitude > collisionRadius * collisionRadius) continue;

				hitEnemies.Add(enemy.GetInstanceID());
				enemy.TakeDamage(damage);
				if (slowDuration > 0f) enemy.ApplySlow(slowDuration, slowMultiplier);
				if (stunDuration > 0f) enemy.Stun(stunDuration);
				if (!piercing)
				{
					didImpact = true;
					Destroy(gameObject);
					return;
				}
			}
		}

		private static Vector2 ClosestPointOnSegment(Vector2 from, Vector2 to, Vector2 point)
		{
			Vector2 segment = to - from;
			float lengthSquared = segment.sqrMagnitude;
			if (lengthSquared <= Mathf.Epsilon) return from;
			float t = Mathf.Clamp01(Vector2.Dot(point - from, segment) / lengthSquared);
			return from + segment * t;
		}
	}
}
