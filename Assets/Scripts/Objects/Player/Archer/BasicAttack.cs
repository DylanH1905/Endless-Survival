
using System.Collections;
using UnityEngine;
using ESS.Objects.Enemy;
using ESS.Objects.Player;
using ESS.Objects.Player.Projectiles;

namespace ESS.Objects.Player.Archer
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

		[Header("Bow Animation")]
		[SerializeField] private Sprite drawingBowSprite;
		[SerializeField] private float bowDrawDuration = 0.15f;
		[SerializeField] private float arrowPullbackDistance = 0.3f;

		private SpriteRenderer bowRenderer;
		private Sprite defaultBowSprite;
		private Transform headTransform;
		private Transform arrowTransform;
		private Coroutine bowDrawRoutine;
		private Vector3 arrowRestLocalPosition;
		private bool hasArrowRestPosition;
		private float defaultAttackCooldown;
		private Coroutine attackSpeedBoostRoutine;

		protected override void Awake()
		{
			base.Awake();
			defaultAttackCooldown = attackCooldown;

			headTransform = transform.Find("Head");

			foreach (SpriteRenderer spriteRenderer
					 in GetComponentsInChildren<SpriteRenderer>(true))
			{
				if (spriteRenderer.transform.name == "Weapon")
				{
					bowRenderer = spriteRenderer;
					defaultBowSprite = bowRenderer.sprite;
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

			if (bowDrawRoutine != null)
			{
				StopCoroutine(bowDrawRoutine);
				ResetBow();
			}

			if (arrowTransform != null && !hasArrowRestPosition)
			{
				arrowRestLocalPosition = arrowTransform.localPosition;
				hasArrowRestPosition = true;
			}

			bowDrawRoutine = StartCoroutine(DrawAndFire(enemy));
		}

		public void ConfigureBowAnimation(
			Sprite attackSprite,
			float drawDuration,
			float pullbackDistance)
		{
			drawingBowSprite = attackSprite;
			bowDrawDuration = Mathf.Max(drawDuration, 0.01f);
			arrowPullbackDistance = Mathf.Max(pullbackDistance, 0f);
		}

		public void SetChargeAnimation(bool isCharging)
		{
			if (bowRenderer != null)
				bowRenderer.sprite = isCharging && drawingBowSprite != null
					? drawingBowSprite
					: defaultBowSprite;
		}

		public override void ConfigureAttacksPerSecond(float attacksPerSecond)
		{
			base.ConfigureAttacksPerSecond(attacksPerSecond);
			defaultAttackCooldown = attackCooldown;
		}

		protected override void OnAttackSuppressed()
		{
			if (bowDrawRoutine != null)
			{
				StopCoroutine(bowDrawRoutine);
				bowDrawRoutine = null;
			}
			ResetBow();
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
			Info("DrawAndFire coroutine started for bow");

			float duration = Mathf.Max(bowDrawDuration, 0.01f);
			float pullbackDuration = duration * 0.6f;

			Vector3 pulledBackLocalPosition = arrowRestLocalPosition;

			if (arrowTransform != null)
			{
				Transform aimTransform = arrowTransform.parent;

				Vector3 pullDirection = aimTransform != null
					? aimTransform.right
					: headTransform != null
						? arrowTransform.position - headTransform.position
						: arrowTransform.up;

				if (pullDirection.sqrMagnitude <= Mathf.Epsilon)
				{
					pullDirection = aimTransform != null
						? aimTransform.up
						: arrowTransform.up;
				}

				pullDirection.Normalize();

				Vector3 pullbackOffset = aimTransform != null
					? aimTransform.InverseTransformVector(
						pullDirection * arrowPullbackDistance)
					: pullDirection * arrowPullbackDistance;

				pulledBackLocalPosition -= pullbackOffset;
			}

			// Đổi sprite cung sang trạng thái kéo dây.
			if (bowRenderer != null && drawingBowSprite != null)
			{
				bowRenderer.sprite = drawingBowSprite;
				Info("Drawing bow");
			}

			// Kéo mũi tên lùi về phía sau.
			float elapsed = 0f;

			while (elapsed < pullbackDuration)
			{
				elapsed += Time.deltaTime;

				float progress = Mathf.SmoothStep(
					0f,
					1f,
					Mathf.Clamp01(elapsed / pullbackDuration));

				if (arrowTransform != null)
				{
					arrowTransform.localPosition = Vector3.Lerp(
						arrowRestLocalPosition,
						pulledBackLocalPosition,
						progress);
				}

				yield return null;
			}

			if (arrowTransform != null)
			{
				arrowTransform.localPosition = pulledBackLocalPosition;
			}

			// Thả dây và bắn tên.
			if (enemy != null && !enemy.IsDead)
			{
				SpawnProjectile(enemy);
			}

			// Đưa mũi tên về vị trí ban đầu.
			if (arrowTransform != null)
			{
				elapsed = 0f;
				float releaseDuration = duration - pullbackDuration;

				while (elapsed < releaseDuration)
				{
					elapsed += Time.deltaTime;

					float progress = Mathf.SmoothStep(
						0f,
						1f,
						Mathf.Clamp01(elapsed / releaseDuration));

					arrowTransform.localPosition = Vector3.Lerp(
						pulledBackLocalPosition,
						arrowRestLocalPosition,
						progress);

					yield return null;
				}

				arrowTransform.localPosition = arrowRestLocalPosition;
			}
			else
			{
				yield return new WaitForSeconds(
					duration - pullbackDuration);
			}

			ResetBow();
			bowDrawRoutine = null;
		}

		private void SpawnProjectile(IEnemy enemy)
		{
			GameObject projectileSource = arrowTransform != null
				? arrowTransform.gameObject
				: projectilePrefab;

			Vector3 spawnPosition = arrowTransform != null
				? arrowTransform.position
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
				projectileColor);
		}

		private void ResetBow()
		{
			if (bowRenderer != null)
			{
				bowRenderer.sprite = defaultBowSprite;
			}

			if (arrowTransform != null && hasArrowRestPosition)
			{
				arrowTransform.localPosition = arrowRestLocalPosition;
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

			if (bowDrawRoutine != null)
			{
				StopCoroutine(bowDrawRoutine);
				bowDrawRoutine = null;
			}

			ResetBow();
		}
	}
}
