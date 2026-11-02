using ESS.Objects.Enemy;
using ESS.UI.GamePlay;
using UnityEngine;
using UnityEngine.UI;

namespace ESS.Objects.Player
{
	public class Movement : MonoBehaviour
	{
		[Header("Movement")]
		[SerializeField] private Rigidbody2D rb;
		[SerializeField] private IPlayer player;
		[SerializeField] private Joystick joystick;
		[SerializeField] private float moveSpeed = 5f;

		[Header("Visuals")]
		[SerializeField] private SpriteRenderer headRenderer;
		private Transform headPivot;

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
		private Vector3 headBaseLocalScale;
		private bool flipHeadTowardTarget = true;
		public Transform CurrentAimTarget { get; private set; }
		public Vector2 CurrentMoveDirection => movement;

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

			if (joystick == null)
			{
				joystick = FindObjectOfType<Joystick>();
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
			headBaseLocalScale = headPivot != null ? headPivot.localScale : Vector3.one;
			foreach (AimTarget aimTarget in GetComponentsInChildren<ESS.Objects.AimTarget>(true))
			{
				aimTarget.enabled = false;
			}

			headFacesLeftByDefault = (headRenderer != null && headRenderer.flipX)
				^ (headPivot != null && headPivot.lossyScale.x < 0f);

			activeMoveSpeed = player != null && player.MovementSpeed > 0f ? player.MovementSpeed : moveSpeed;
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
			flipHeadTowardTarget = config == null || config.flipHeadTowardTarget;
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
			HandleSkillInput(KeyCode.Q, "Q");
			HandleSkillInput(KeyCode.W, "W");
			HandleSkillInput(KeyCode.E, "E");
			HandleSkillInput(KeyCode.R, "R");
			if (dashCounter <= 0f && player != null)
				activeMoveSpeed = player.MovementSpeed;

			Move();
			UpdateClosestEnemyTarget();
			RotateHeadToMovement();
			UpdateDash();
		}

		private void HandleSkillInput(KeyCode key, string skillName)
		{
			if (!Input.GetKeyDown(key) && !Input.GetKeyUp(key)) return;
			foreach (ISkill skill in GetComponentsInChildren<ISkill>(true))
			{
				if (skill != null && skill.GetType().Name == skillName)
				{
					if (skill is ESS.Objects.Player.Archer.Skills.IChargeableSkill chargeable)
					{
						if (Input.GetKeyDown(key)) chargeable.BeginCharge();
						if (Input.GetKeyUp(key)) chargeable.ReleaseCharge();
					}
					else if (Input.GetKeyDown(key))
					{
						skill.Execute();
					}
					return;
				}
			}
		}

		private void Move()
		{
			movement = joystick != null ? joystick.Direction : Vector2.zero;

			if (Input.GetKey(KeyCode.LeftArrow))
			{
				movement.x -= 1f;
			}

			if (Input.GetKey(KeyCode.RightArrow))
			{
				movement.x += 1f;
			}

			if (Input.GetKey(KeyCode.UpArrow))
			{
				movement.y += 1f;
			}

			if (Input.GetKey(KeyCode.DownArrow))
			{
				movement.y -= 1f;
			}

			movement = Vector2.ClampMagnitude(movement, 1f);

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
				if (flipHeadTowardTarget && headPivot != null)
				{
					Vector3 headScale = headBaseLocalScale;
					headScale.x *= headFacesLeftByDefault != facesLeft ? -1f : 1f;
					headPivot.localScale = headScale;
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
			activeMoveSpeed = player != null && player.MovementSpeed > 0f ? player.MovementSpeed : moveSpeed;
					dashCooldownCounter = dashCooldown;
				}
			}

			if (dashCooldownCounter > 0f)
			{
				dashCooldownCounter -= Time.deltaTime;
			}
		}

		private void UpdateClosestEnemyTarget()
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
