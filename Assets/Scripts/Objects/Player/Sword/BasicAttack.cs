using System.Collections;
using UnityEngine;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player.Sword
{
	public class BasicAttack : IBasicAttack
	{
		[Header("Attack Visuals")]
		[SerializeField] private Transform weaponPivot;
		[SerializeField] private SpriteRenderer weaponRenderer;
		[SerializeField] private Sprite weaponAttackSprite;
		[SerializeField, Min(0.01f)] private float weaponAttackDuration = 0.15f;

		[Header("Attack Settings")]
		[SerializeField, Min(0f)]
		private float hitDelay = 0.08f;

		[SerializeField, Min(0f)]
		private float hitRadius = 1.5f;

		[SerializeField, Range(0f, 360f)]
		private float hitAngle = 120f;

		private Sprite defaultWeaponSprite;
		private Coroutine attackRoutine;

		protected override void Awake()
		{
			base.Awake();

			Transform characterRoot = player != null
				? player.transform
				: transform.root;

			if (weaponPivot == null && characterRoot != null)
			{
				weaponPivot = characterRoot.Find("WeaponPivot");
			}

			if (weaponRenderer == null)
			{
				foreach (SpriteRenderer spriteRenderer
						 in GetComponentsInChildren<SpriteRenderer>(true))
				{
					if (spriteRenderer.transform.name == "Weapon")
					{
						weaponRenderer = spriteRenderer;
						break;
					}
				}
			}

			if (weaponRenderer != null)
			{
				defaultWeaponSprite = weaponRenderer.sprite;
			}
		}

		protected override void Attack(IEnemy enemy)
		{
			// The shared attack controller selects targets using attackRange.
			// A sword slash should only animate when a target is actually close
			// enough to be hit by the slash.
			if (!IsNearbyEnemy(enemy))
			{
				return;
			}

			if (attackRoutine != null)
			{
				StopCoroutine(attackRoutine);
				attackRoutine = null;
			}

			ResetWeaponSprite();
			attackRoutine = StartCoroutine(WeaponAttackRoutine(enemy));
		}

		public void ConfigureWeaponAttackAnimation(Sprite attackSprite, float duration)
		{
			weaponAttackSprite = attackSprite;
			weaponAttackDuration = Mathf.Max(duration, 0.01f);
			if (weaponRenderer != null)
			{
				defaultWeaponSprite = weaponRenderer.sprite;
			}
		}

		private bool IsNearbyEnemy(IEnemy enemy)
		{
			if (enemy == null || enemy.IsDead)
			{
				return false;
			}

			float effectiveRange = Mathf.Min(attackRange, hitRadius);
			return ((Vector2)enemy.transform.position - GetAttackOrigin()).sqrMagnitude
				<= effectiveRange * effectiveRange;
		}

		private IEnumerator WeaponAttackRoutine(IEnemy initialTarget)
		{
			Vector2 direction = GetAttackDirection(initialTarget);
			if (weaponRenderer != null && weaponAttackSprite != null)
			{
				weaponRenderer.sprite = weaponAttackSprite;
			}

			float duration = Mathf.Max(weaponAttackDuration, 0.01f);
			float elapsed = 0f;
			bool hasHit = false;

			while (elapsed < duration)
			{
				if (!IsNearbyEnemy(initialTarget))
				{
					ResetWeaponSprite();
					attackRoutine = null;
					yield break;
				}

				elapsed += Time.deltaTime;

				if (!hasHit && elapsed >= hitDelay)
				{
					DealSlashDamage(direction);
					hasHit = true;
				}

				yield return null;
			}

			ResetWeaponSprite();
			attackRoutine = null;
		}

		private Vector2 GetAttackDirection(IEnemy target)
		{
			if (target != null && !target.IsDead)
			{
				Vector2 direction =
					(Vector2)target.transform.position - GetAttackOrigin();

				if (direction.sqrMagnitude > Mathf.Epsilon)
				{
					return direction.normalized;
				}
			}

			return weaponPivot != null
				? (Vector2)weaponPivot.right
				: (Vector2)transform.right;
		}

		private Vector2 GetAttackOrigin()
		{
			return weaponPivot != null
				? (Vector2)weaponPivot.position
				: (Vector2)transform.position;
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
					(Vector2)enemy.transform.position - GetAttackOrigin();

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

		private void ResetWeaponSprite()
		{
			if (weaponRenderer != null)
			{
				weaponRenderer.sprite = defaultWeaponSprite;
			}
		}

		protected override void OnAttackSuppressed()
		{
			if (attackRoutine != null)
			{
				StopCoroutine(attackRoutine);
				attackRoutine = null;
			}

			ResetWeaponSprite();
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			if (attackRoutine != null)
			{
				StopCoroutine(attackRoutine);
				attackRoutine = null;
			}

			ResetWeaponSprite();
		}

		private void OnDrawGizmosSelected()
		{
			Vector3 origin = weaponPivot != null
				? weaponPivot.position
				: transform.position;

			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(origin, hitRadius);

			Vector2 direction = transform.right;

			if (weaponPivot != null)
			{
				direction = weaponPivot.right;
			}

			float halfAngle = hitAngle * 0.5f;

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
