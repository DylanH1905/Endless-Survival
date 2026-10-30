using UnityEngine;
using ESS.Objects;
using ESS.Objects.Player;

namespace ESS.Objects.Enemy
{
    public class BossPowerProjectile : MonoBehaviour
    {
        [SerializeField] private float hitDistance = 0.2f;
        [SerializeField] private float openingDuration = 0.25f;
        [SerializeField, Range(0f, 1f)] private float initialScaleMultiplier = 0.25f;
        [SerializeField, Range(0f, 1f)] private float initialBrightness = 0.2f;

        private IPlayer target;
        private int damage;
        private float speed;
        private Vector3 aimDirection;
        private float spriteRotationOffset;
        private Vector3 fullScale;
        private SpriteRenderer spriteRenderer;
        private Color fullColor;
        private float openingTimer;

        public void Initialize(IPlayer newTarget, int newDamage, float newSpeed, Vector3 aimVector)
        {
            target = newTarget;
            damage = newDamage;
            speed = newSpeed;
            fullScale = transform.localScale;
            openingTimer = Mathf.Max(openingDuration, 0f);
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                fullColor = spriteRenderer.color;
                spriteRenderer.color = Color.Lerp(Color.clear, fullColor, initialBrightness);
            }

            transform.localScale = fullScale * initialScaleMultiplier;
            aimDirection = aimVector.sqrMagnitude > Mathf.Epsilon
                ? aimVector.normalized
                : Vector3.right;

            float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            spriteRotationOffset = transform.eulerAngles.z - aimAngle;
        }

        private void Update()
        {
            if (target == null || target.IsDead)
            {
                Destroy(gameObject);
                return;
            }

            UpdateOpeningEffect();

            Vector3 aimVector = target.transform.position - transform.position;
            float distance = aimVector.magnitude;
            aimDirection = distance > Mathf.Epsilon ? aimVector / distance : aimDirection;
            float travelDistance = speed * Time.deltaTime;
            Vector3 nextPosition = transform.position
                + aimDirection * Mathf.Min(travelDistance, distance);
            if (IntersectsShield(transform.position, nextPosition))
            {
                Destroy(gameObject);
                return;
            }

            if (distance <= hitDistance)
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, aimAngle + spriteRotationOffset);

            if (distance <= travelDistance + hitDistance)
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            transform.position += aimDirection * travelDistance;
        }

        private bool IntersectsShield(Vector3 from, Vector3 to)
        {
            ObjectCharacter character = target as ObjectCharacter;
            if (character == null
                || !character.HasActiveShield
                || character.ShieldCollisionRadius <= 0f)
            {
                return false;
            }

            Vector2 segment = (Vector2)(to - from);
            Vector2 offset = (Vector2)character.transform.position - (Vector2)from;
            float segmentLengthSquared = segment.sqrMagnitude;
            float progress = segmentLengthSquared > Mathf.Epsilon
                ? Mathf.Clamp01(Vector2.Dot(offset, segment) / segmentLengthSquared)
                : 0f;
            Vector2 closestPoint = (Vector2)from + segment * progress;
            float radius = character.ShieldCollisionRadius + hitDistance;
            return ((Vector2)character.transform.position - closestPoint).sqrMagnitude
                <= radius * radius;
        }

        private void UpdateOpeningEffect()
        {
            if (openingTimer <= 0f)
            {
                return;
            }

            openingTimer = Mathf.Max(openingTimer - Time.deltaTime, 0f);
            float progress = openingDuration > 0f ? 1f - openingTimer / openingDuration : 1f;
            progress = Mathf.SmoothStep(0f, 1f, progress);
            transform.localScale = Vector3.Lerp(fullScale * initialScaleMultiplier, fullScale, progress);

            if (spriteRenderer != null)
            {
                Color startingColor = Color.Lerp(Color.clear, fullColor, initialBrightness);
                spriteRenderer.color = Color.Lerp(startingColor, fullColor, progress);
            }
        }
    }
}