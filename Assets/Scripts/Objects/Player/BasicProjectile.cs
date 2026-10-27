using UnityEngine;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player
{
	public class BasicProjectile : MonoBehaviour
	{
		private IEnemy target;
		private int damage;
		private float speed;
		private float trailLength;
		private LineRenderer lineRenderer;
		private SpriteRenderer spriteRenderer;
		private Vector3 aimDirection;
		private float spriteRotationOffset;

		public void Initialize(IEnemy newTarget, int newDamage, float newSpeed, Color color)
		{
			target = newTarget;
			damage = newDamage;
			speed = newSpeed;

			if (spriteRenderer == null)
			{
				SetupLineRenderer(color);
			}
			else
			{
				Vector3 aimVector = target.transform.position - transform.position;
				aimDirection = aimVector.sqrMagnitude > Mathf.Epsilon ? aimVector.normalized : Vector3.up;
				float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
				spriteRotationOffset = transform.eulerAngles.z - aimAngle;
				RotateArrowAlong(aimDirection);
			}
		}

		private void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			if (spriteRenderer == null)
			{
				SetupLineRenderer(Color.yellow);
			}
		}

		private void Update()
		{
			if (target == null || target.IsDead)
			{
				Destroy(gameObject);
				return;
			}

			Vector3 targetPosition = target.transform.position;
			Vector3 aimVector = targetPosition - transform.position;
			float distance = aimVector.magnitude;
			if (distance <= 0.05f)
			{
				target.TakeDamage(damage);
				Destroy(gameObject);
				return;
			}

			aimDirection = aimVector / distance;
			float travelDistance = speed * Time.deltaTime;
			Vector3 nextPosition = transform.position + aimDirection * Mathf.Min(travelDistance, distance);

			if (spriteRenderer != null)
			{
				RotateArrowAlong(aimDirection);
			}
			else if (lineRenderer != null)
			{
				lineRenderer.SetPosition(0, nextPosition);
				lineRenderer.SetPosition(1, nextPosition - aimDirection * trailLength);
			}

			transform.position = nextPosition;
			if (distance <= travelDistance + 0.05f)
			{
				target.TakeDamage(damage);
				Destroy(gameObject);
			}
		}

		private void RotateArrowAlong(Vector3 direction)
		{
			float directionAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler(0f, 0f, directionAngle + spriteRotationOffset);
		}

		private void SetupLineRenderer(Color color)
		{
			if (lineRenderer == null)
			{
				lineRenderer = GetComponent<LineRenderer>();
				if (lineRenderer == null)
				{
					lineRenderer = gameObject.AddComponent<LineRenderer>();
				}
			}

			lineRenderer.positionCount = 2;
			lineRenderer.useWorldSpace = true;
			lineRenderer.startWidth = 0.08f;
			lineRenderer.endWidth = 0.02f;
			lineRenderer.startColor = color;
			lineRenderer.endColor = new Color(color.r, color.g, color.b, 0f);
			lineRenderer.numCapVertices = 4;
			lineRenderer.sortingLayerName = "Default";
			lineRenderer.sortingOrder = 10;

			if (lineRenderer.material == null)
			{
				Shader shader = Shader.Find("Sprites/Default");
				if (shader != null)
				{
					lineRenderer.material = new Material(shader);
				}
			}

			trailLength = 0.35f;
			lineRenderer.SetPosition(0, transform.position);
			lineRenderer.SetPosition(1, transform.position);
		}
	}
}