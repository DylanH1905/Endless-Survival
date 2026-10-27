using ESS.Objects.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace ESS.Objects.Player
{
	public class Movement : MonoBehaviour
	{
		[Header("Movement")]
		[SerializeField] private Rigidbody2D rb;
		[SerializeField] private IPlayer player;
		[SerializeField] private float moveSpeed = 5f;

		[Header("Visuals")]
		[SerializeField] private SpriteRenderer headRenderer;
		[SerializeField] private SpriteRenderer weaponRenderer;
		[SerializeField] private Transform bowTransform;
		[SerializeField] private Transform arrowTransform;
		private Transform aimPivot;
		private Transform headPivot;
		private Transform weaponPivot;
        private Transform powerTransform;

		[Header("Dash")]
		[SerializeField] private float dashSpeed = 12f;
		[SerializeField] private float dashLength = 0.5f;
		[SerializeField] private float dashCooldown = 1f;
		[SerializeField] private bool isDashLocked;
		[SerializeField] private Button dashButton;

		[SerializeField] private SkillHandler skillHandler;

		private Vector2 movement;
		private float activeMoveSpeed;
		private float dashCounter;
		private float dashCooldownCounter;
		private bool dashRequested;
		private bool headFacesLeftByDefault;
		private Vector3 weaponBaseLocalPosition;
		private bool weaponFlipXByDefault;
		private Quaternion weaponPivotBaseLocalRotation;
		private Quaternion arrowRestLocalRotation;
		private bool flipHeadTowardTarget = true;
		private CharacterWeaponAimMode weaponAimMode;
		private float weaponAngleOffset;
		public Transform CurrentAimTarget { get; private set; }

		private void Awake()
		{
			if (rb == null)
			{
				rb = GetComponent<Rigidbody2D>();
			}

			if (player == null)
			{
				player = GetComponent<IPlayer>();
			}

			if (skillHandler == null)
			{
				skillHandler = GetComponent<SkillHandler>();
			}

			if (headRenderer == null)
			{
				Transform head = transform.Find("Head");
				if (head != null)
				{
					headRenderer = head.GetComponent<SpriteRenderer>();
				}
			}

			headPivot = transform.Find("Head");
			if (headPivot == null && headRenderer != null)
			{
				headPivot = headRenderer.transform;
			}

            powerTransform = transform.Find("Power");

			foreach (AimTarget aimTarget in GetComponentsInChildren<ESS.Objects.AimTarget>(true))
			{
				aimTarget.enabled = false;
			}

			if (bowTransform == null)
			{
				bowTransform = transform.Find("Bow");
			}

			if (arrowTransform == null)
			{
				arrowTransform = transform.Find("Arrow");
			}

			headFacesLeftByDefault = headRenderer != null && headRenderer.flipX;

			if (weaponRenderer == null)
			{
				foreach (SpriteRenderer renderer
						 in GetComponentsInChildren<SpriteRenderer>(true))
				{
					if (renderer.transform.name == "Weapon")
					{
						weaponRenderer = renderer;
						break;
					}
				}
			}

			if (weaponRenderer != null)
			{
				weaponPivot = weaponRenderer.transform.parent;
				weaponBaseLocalPosition = weaponRenderer.transform.localPosition;
				weaponFlipXByDefault = weaponRenderer.flipX;

				if (weaponPivot != null && weaponPivot.name == "WeaponPivot")
				{
					weaponPivotBaseLocalRotation = weaponPivot.localRotation;
				}
			}

			if (bowTransform != null || arrowTransform != null)
			{
				GameObject pivotObject = new GameObject("AimPivot");
				aimPivot = pivotObject.transform;
				aimPivot.SetParent(transform, false);
				aimPivot.position = headPivot != null ? headPivot.position : transform.position;
				aimPivot.rotation = transform.rotation;

				if (bowTransform != null)
				{
					bowTransform.SetParent(aimPivot, true);
				}

				if (arrowTransform != null)
				{
					arrowTransform.SetParent(aimPivot, true);
					arrowRestLocalRotation = arrowTransform.localRotation;
				}
			}

			activeMoveSpeed = player != null && player.speed > 0f ? player.speed : moveSpeed;
		}

		public void OnClickDash()
		{
			if (!isDashLocked)
			{
				dashRequested = true;
				if (dashButton != null)
				{
					dashButton.interactable = false;
				}
			}
		}

		public void ApplyVisualConfig(CharacterVisualConfig config)
		{
			if (config == null)
			{
				flipHeadTowardTarget = true;
				weaponAimMode = CharacterWeaponAimMode.None;
				weaponAngleOffset = 0f;
				return;
			}

			flipHeadTowardTarget = config.flipHeadTowardTarget;
			weaponAimMode = config.weaponAimMode;
			weaponAngleOffset = config.weaponAngleOffset;

			if (weaponAimMode != CharacterWeaponAimMode.None
				&& weaponPivot != null && weaponPivot.name == "WeaponPivot"
				&& headPivot != null)
			{
				weaponPivot.position = headPivot.position;
			}

            if (weaponAimMode == CharacterWeaponAimMode.RotateAroundHead
                && weaponPivot != null && powerTransform != null
                && powerTransform.parent != weaponPivot)
            {
                powerTransform.SetParent(weaponPivot, true);
            }
		}

		public void OnClickMultiple()
		{
			if (skillHandler != null)
			{
				skillHandler.OnClickMultiple();
			}
		}

		public void OnClickGrenade()
		{
			if (skillHandler != null)
			{
				skillHandler.OnClickGrenade();
			}
		}

		public void onClickDash()
		{
			OnClickDash();
		}

		public void onClickMutiple()
		{
			OnClickMultiple();
		}

		public void onClickGen()
		{
			OnClickGrenade();
		}

		private void Update()
		{
			Debug.Log($"Keyboard: {Input.anyKey}");
			Move();
			RotateToClosestEnemy();
			RotateHeadToMovement();
			UpdateDash();
		}

		private void Move()
		{
			movement.x = 0f;
			movement.y = 0f;

			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			{
				movement.x -= 1f;
			}

			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			{
				movement.x += 1f;
			}

			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
			{
				movement.y += 1f;
			}

			if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
			{
				movement.y -= 1f;
			}

			movement.Normalize();

			if (rb != null)
			{
				rb.velocity = movement * activeMoveSpeed;
			}
		}

		private void RotateHeadToMovement()
		{
			bool hasFacingDirection = false;
			float directionX = 0f;

			if (CurrentAimTarget != null)
			{
				float facingOriginX = headPivot != null
					? headPivot.position.x
					: transform.position.x;
				directionX = CurrentAimTarget.position.x - facingOriginX;
				hasFacingDirection = true;
			}
			else if (movement.x != 0f)
			{
				directionX = movement.x;
				hasFacingDirection = true;
			}

			if (hasFacingDirection && Mathf.Abs(directionX) > Mathf.Epsilon)
			{
				bool facesLeft = directionX < 0f;
				if (flipHeadTowardTarget && headRenderer != null)
				{
					headRenderer.flipX = headFacesLeftByDefault ^ facesLeft;
				}

				if (weaponAimMode == CharacterWeaponAimMode.MirrorAroundHead
					&& weaponRenderer != null && weaponPivot != null
					&& weaponPivot.name == "WeaponPivot")
				{
					Vector3 weaponPosition = weaponBaseLocalPosition;
					weaponPosition.x *= facesLeft ? -1f : 1f;
					weaponRenderer.transform.localPosition = weaponPosition;
					weaponRenderer.flipX = weaponFlipXByDefault ^ facesLeft;
				}
			}

			if (weaponAimMode == CharacterWeaponAimMode.RotateAroundHead
				&& weaponPivot != null && weaponPivot.name == "WeaponPivot")
			{
				if (CurrentAimTarget != null)
				{
					Vector2 aimOrigin = headPivot != null
						? (Vector2)headPivot.position
						: (Vector2)weaponPivot.position;
					Vector2 aimDirection = (Vector2)CurrentAimTarget.position - aimOrigin;
					if (aimDirection.sqrMagnitude > Mathf.Epsilon)
					{
						float angle = Mathf.Atan2(aimDirection.y, aimDirection.x)
							* Mathf.Rad2Deg + weaponAngleOffset;
						weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);
					}
				}
				else
				{
					weaponPivot.localRotation = weaponPivotBaseLocalRotation;
				}
			}
		}

		private void UpdateDash()
		{
			if (dashRequested && dashCooldownCounter <= 0f && dashCounter <= 0f)
			{
				activeMoveSpeed = dashSpeed;
				dashCounter = dashLength;
			}

			dashRequested = false;

			if (dashCounter > 0f)
			{
				dashCounter -= Time.deltaTime;
				if (dashCounter <= 0f)
				{
					activeMoveSpeed = player != null && player.speed > 0f ? player.speed : moveSpeed;
					dashCooldownCounter = dashCooldown;
				}
			}

			if (dashCooldownCounter > 0f)
			{
				dashCooldownCounter -= Time.deltaTime;
			}
		}

		private void RotateToClosestEnemy()
		{
			Vector3 aimOrigin = headRenderer != null ? headRenderer.bounds.center : transform.position;
			IEnemy closestEnemy = null;
			float closestDistance = Mathf.Infinity;

			foreach (IEnemy enemy in FindObjectsOfType<IEnemy>())
			{
				if (enemy == null || enemy.IsDead)
				{
					continue;
				}

				float distance = (enemy.transform.position - aimOrigin).sqrMagnitude;
				if (distance > 0f && distance < closestDistance)
				{
					closestDistance = distance;
					closestEnemy = enemy;
				}
			}

			CurrentAimTarget = closestEnemy != null ? closestEnemy.transform : null;

			if (closestEnemy == null)
			{
				if (aimPivot != null)
				{
					aimPivot.rotation = Quaternion.Slerp(
						aimPivot.rotation,
						transform.rotation,
						Time.deltaTime * 10f);
				}

				if (arrowTransform != null)
				{
					arrowTransform.localRotation = arrowRestLocalRotation;
				}

				return;
			}

			Vector3 direction = closestEnemy.transform.position - aimOrigin;
			float offset = weaponAimMode == CharacterWeaponAimMode.RotateAroundHead
				? weaponAngleOffset
				: -90f;
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + offset;
			Quaternion aimRotation = Quaternion.AngleAxis(angle, Vector3.forward);
			if (aimPivot != null)
			{
				aimPivot.rotation = aimRotation;
			}

			if (arrowTransform != null)
			{
				arrowTransform.localRotation = Quaternion.identity;
			}
		}

		public void AddSpeed()
		{
			moveSpeed += 1f;
			if (dashCounter <= 0f)
			{
				activeMoveSpeed += 1f;
			}
		}

	}
}
