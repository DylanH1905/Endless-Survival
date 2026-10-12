using System.Collections;
using UnityEngine;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player.Sword
{
	public class BasicAttack : IBasicAttack
	{
		[Header("Sword References")]
		[SerializeField] private Transform weaponPivot;
		[SerializeField] private GameObject attackEffect;

		[Header("Slash Settings")]
		[SerializeField, Range(1f, 180f)]
		private float slashAngle = 60f;

		[SerializeField, Min(0.01f)]
		private float slashDuration = 0.2f;

		[SerializeField, Min(0f)]
		private float hitDelay = 0.08f;

		[SerializeField, Min(0f)]
		private float hitRadius = 1.5f;

		[SerializeField, Range(0f, 360f)]
		private float hitAngle = 120f;

		private Quaternion weaponRestRotation;
		private Coroutine slashRoutine;
		private bool hasWeaponRestRotation;

		protected override void Awake()
		{
			base.Awake();

			if (weaponPivot != null)
			{
				weaponRestRotation = weaponPivot.localRotation;
				hasWeaponRestRotation = true;
			}

			if (attackEffect != null)
			{
				attackEffect.SetActive(false);
			}
		}

		protected override void Attack(IEnemy enemy)
		{
			if (enemy == null || enemy.IsDead)
			{
				return;
			}

			// Không để hai nhát chém chạy đồng thời.
			if (slashRoutine != null)
			{
				StopCoroutine(slashRoutine);
				slashRoutine = null;
			}

			ResetSlash();

			slashRoutine = StartCoroutine(SlashRoutine(enemy));
		}

		private IEnumerator SlashRoutine(IEnemy initialTarget)
		{
			if (weaponPivot == null)
			{
				Error("BasicAttack: WeaponPivot chưa được gán trong Inspector.");
				slashRoutine = null;
				yield break;
			}

			Vector2 direction = GetAttackDirection(initialTarget);

			float worldAngle = Mathf.Atan2(
				direction.y,
				direction.x
			) * Mathf.Rad2Deg;

			float parentAngle = weaponPivot.parent != null
				? weaponPivot.parent.eulerAngles.z
				: 0f;

			float baseLocalAngle = worldAngle - parentAngle;

			float halfAngle = slashAngle * 0.5f;
			float duration = Mathf.Max(slashDuration, 0.01f);
			float elapsed = 0f;
			bool hasHit = false;

			if (attackEffect != null)
			{
				attackEffect.SetActive(true);
			}

			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;

				float progress = Mathf.Clamp01(elapsed / duration);
				float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

				float currentAngle = Mathf.Lerp(
					-halfAngle,
					halfAngle,
					easedProgress
				);

				float localAngle = baseLocalAngle + currentAngle;

				weaponPivot.localRotation = Quaternion.Euler(
					0f,
					0f,
					localAngle
				);

				if (!hasHit && elapsed >= hitDelay)
				{
					DealSlashDamage(direction);
					hasHit = true;
				}

				yield return null;
			}

			ResetSlash();
			slashRoutine = null;
		}

		private Vector2 GetAttackDirection(IEnemy target)
		{
			if (target != null && !target.IsDead)
			{
				Vector2 direction =
					target.transform.position - transform.position;

				if (direction.sqrMagnitude > Mathf.Epsilon)
				{
					return direction.normalized;
				}
			}

			// Fallback nếu mục tiêu không còn hợp lệ.
			return transform.right.normalized;
		}

		private void DealSlashDamage(Vector2 direction)
		{
			IEnemy[] enemies = FindObjectsOfType<IEnemy>();

			foreach (IEnemy enemy in enemies)
			{
				if (enemy == null || enemy.IsDead)
				{
					continue;
				}

				Vector2 toEnemy =
					enemy.transform.position - transform.position;

				if (toEnemy.sqrMagnitude > hitRadius * hitRadius)
				{
					continue;
				}

				if (toEnemy.sqrMagnitude <= Mathf.Epsilon)
				{
					enemy.TakeDamage(player.attackDamage);
					continue;
				}

				float angle = Vector2.Angle(direction, toEnemy);

				if (angle <= hitAngle * 0.5f)
				{
					enemy.TakeDamage(player.attackDamage);
				}
			}
		}

		private void ResetSlash()
		{
			if (attackEffect != null)
			{
				attackEffect.SetActive(false);
			}

			if (weaponPivot != null && hasWeaponRestRotation)
			{
				weaponPivot.localRotation = weaponRestRotation;
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			if (slashRoutine != null)
			{
				StopCoroutine(slashRoutine);
				slashRoutine = null;
			}

			ResetSlash();
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, hitRadius);

			Vector2 direction = transform.right;

			if (weaponPivot != null)
			{
				direction = weaponPivot.right;
			}

			float halfAngle = hitAngle * 0.5f;
			Vector3 origin = transform.position;

			Vector3 left = Quaternion.Euler(
				0f, 0f, -halfAngle
			) * direction;

			Vector3 right = Quaternion.Euler(
				0f, 0f, halfAngle
			) * direction;

			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(origin, left * hitRadius);
			Gizmos.DrawRay(origin, right * hitRadius);
		}
	}
}