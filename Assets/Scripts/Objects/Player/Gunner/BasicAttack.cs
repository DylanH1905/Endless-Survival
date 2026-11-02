
using System.Collections;
using UnityEngine;
using ESS.Objects.Enemy;
using ESS.Objects.Player;
using ESS.Objects.Player.Projectiles;

namespace ESS.Objects.Player.Gunner
{
	public class BasicAttack : IBasicAttack
	{
		[Header("Projectile")]
		[SerializeField] private float projectileSpeed = 12f;
		[SerializeField, Min(0.1f)] private float projectileScaleMultiplier = 3f;
		[SerializeField]
		private Color projectileColor =
			new Color(1f, 0.85f, 0.2f, 1f);
		[SerializeField] private GameObject projectilePrefab;
		[SerializeField, Min(0f)] private float bulletFadeInDuration = 0.12f;

		[Header("Bullet Animation")]
		[SerializeField] private Sprite weaponAttackSprite;
		[SerializeField, Min(0f)] private float weaponAttackDelay = 0.08f;
		[SerializeField] private float weaponAttackDuration = 0.15f;

		private SpriteRenderer bulletRenderer;
		private Sprite defaultBulletSprite;
		private Transform arrowTransform;
		private Coroutine bulletDrawRoutine;
		private float defaultAttackCooldown;
		private Coroutine attackSpeedBoostRoutine;

		protected override void Awake()
		{
			base.Awake();
			defaultAttackCooldown = attackCooldown;

			foreach (SpriteRenderer spriteRenderer
					 in GetComponentsInChildren<SpriteRenderer>(true))
			{
				if (spriteRenderer.transform.name == "Weapon")
				{
					bulletRenderer = spriteRenderer;
					defaultBulletSprite = bulletRenderer.sprite;
				}

				if (spriteRenderer.transform.name == "Power")
				{
					arrowTransform = spriteRenderer.transform;
				}
			}

		}

		protected override void Attack(IEnemy enemy)
		{
			if (enemy == null || enemy.IsDead)
			{
				return;
			}

			if (bulletDrawRoutine != null)
			{
				StopCoroutine(bulletDrawRoutine);
				ResetBullet();
			}

			bulletDrawRoutine = StartCoroutine(DrawAndFire(enemy));
		}

		public void ConfigureWeaponAttackAnimation(
			Sprite baseWeaponSprite,
			Sprite attackSprite,
			float duration)
		{
			defaultBulletSprite = baseWeaponSprite;
			weaponAttackSprite = attackSprite;
			weaponAttackDuration = Mathf.Max(duration, 0.01f);
		}

		public void SetChargeAnimation(bool isCharging)
		{
			if (bulletRenderer != null)
				bulletRenderer.sprite = isCharging && weaponAttackSprite != null
					? weaponAttackSprite
					: defaultBulletSprite;
		}

		public override void ConfigureAttacksPerSecond(float attacksPerSecond)
		{
			base.ConfigureAttacksPerSecond(attacksPerSecond);
			defaultAttackCooldown = attackCooldown;
		}

		protected override void OnAttackSuppressed()
		{
			if (bulletDrawRoutine != null)
			{
				StopCoroutine(bulletDrawRoutine);
				bulletDrawRoutine = null;
			}
			ResetBullet();
		}

		public void ApplyAttackSpeedBoost(float multiplier, float duration)
		{
			if (attackSpeedBoostRoutine != null)
				StopCoroutine(attackSpeedBoostRoutine);

			attackCooldown = Mathf.Max(0.1f, defaultAttackCooldown / Mathf.Max(multiplier, 1f));
			attackSpeedBoostRoutine = StartCoroutine(ResetAttackSpeedBoost(duration));
		}

		private IEnumerator ResetAttackSpeedBoost(float duration)
		{
			yield return new WaitForSeconds(Mathf.Max(duration, 0.1f));
			attackCooldown = defaultAttackCooldown;
			attackSpeedBoostRoutine = null;
		}

		private IEnumerator DrawAndFire(IEnemy enemy)
		{
			if (enemy != null && !enemy.IsDead)
				SpawnProjectile(enemy);

			if (weaponAttackDelay > 0f)
				yield return new WaitForSeconds(weaponAttackDelay);

			if (bulletRenderer != null && weaponAttackSprite != null)
				bulletRenderer.sprite = weaponAttackSprite;

			yield return new WaitForSeconds(weaponAttackDuration);

			ResetBullet();
			bulletDrawRoutine = null;
		}

		private void SpawnProjectile(IEnemy enemy)
		{
			GameObject projectileSource = arrowTransform != null
				? arrowTransform.gameObject
				: projectilePrefab;

			Vector3 spawnPosition = player != null
				? player.transform.position
				: transform.position;

			Quaternion spawnRotation = arrowTransform != null
				? arrowTransform.rotation
				: Quaternion.identity;

			GameObject projectileObject = projectileSource != null
				? Instantiate(
					projectileSource,
					spawnPosition,
					spawnRotation)
				: new GameObject("HomingProjectile");

			projectileObject.name = "ArrowProjectile";
			projectileObject.transform.SetParent(null, true);

			if (projectileSource == null)
			{
				projectileObject.transform.position = spawnPosition;
			}
			else if (arrowTransform != null)
			{
				projectileObject.transform.localScale =
					arrowTransform.lossyScale * projectileScaleMultiplier;

				SpriteRenderer sourceRenderer =
					arrowTransform.GetComponent<SpriteRenderer>();
				SpriteRenderer projectileRenderer =
					projectileObject.GetComponent<SpriteRenderer>();

				if (sourceRenderer != null && projectileRenderer != null)
				{
					projectileRenderer.sprite = sourceRenderer.sprite;
					projectileRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
					projectileRenderer.sortingOrder = Mathf.Max(
						sourceRenderer.sortingOrder + 1,
						100);
				}
			}

			foreach (ESS.Objects.AimTarget aimTarget
					 in projectileObject.GetComponentsInChildren<ESS.Objects.AimTarget>(true))
			{
				aimTarget.enabled = false;
			}

			projectileObject.SetActive(true);

			HomingProjectile projectile =
				projectileObject.GetComponent<HomingProjectile>();

			if (projectile == null)
			{
				projectile =
					projectileObject.AddComponent<HomingProjectile>();
			}

			projectile.Initialize(
				enemy,
				player.attackDamage,
				projectileSpeed,
				projectileColor,
				Vector3.zero,
				bulletFadeInDuration,
				true);
		}

		private void ResetBullet()
		{
			if (bulletRenderer != null)
			{
				bulletRenderer.sprite = defaultBulletSprite;
			}

		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (attackSpeedBoostRoutine != null)
			{
				StopCoroutine(attackSpeedBoostRoutine);
				attackSpeedBoostRoutine = null;
			}
			attackCooldown = defaultAttackCooldown;

			if (bulletDrawRoutine != null)
			{
				StopCoroutine(bulletDrawRoutine);
				bulletDrawRoutine = null;
			}

			ResetBullet();
		}
	}
}
