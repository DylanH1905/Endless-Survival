using System.Collections;
using UnityEngine;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player.Sword.Skills
{
	public class E : SwordSkill
	{
		[SerializeField, Min(0.1f)] private float dashDistance = 2.5f;
		[SerializeField, Min(0.01f)] private float dashDuration = 0.15f;
		private bool isDashing;
		public override string SkillName => "Quickstep";

		public override void Execute()
		{
			if (isDashing || !TryBeginCast()) return;
			Vector2 direction = Owner.GetComponent<Movement>()?.CurrentMoveDirection ?? Vector2.zero;
			if (direction.sqrMagnitude < 0.01f)
			{
				IEnemy target = FindClosestEnemy();
				direction = target != null ? (Vector2)(target.transform.position - Owner.transform.position) : Vector2.right;
			}
			StartCoroutine(Dash(direction.normalized));
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

		private IEnumerator Dash(Vector2 direction)
		{
			isDashing = true;
			Rigidbody2D body = Owner.GetComponent<Rigidbody2D>();
			Vector2 start = Owner.transform.position;
			Vector2 end = start + direction * dashDistance;
			float elapsed = 0f;
			while (elapsed < dashDuration && Owner != null)
			{
				elapsed += Time.deltaTime;
				Vector2 position = Vector2.Lerp(start, end, Mathf.Clamp01(elapsed / dashDuration));
				if (body != null) body.MovePosition(position); else Owner.transform.position = position;
				yield return new WaitForFixedUpdate();
			}
			isDashing = false;
		}
	}
}
