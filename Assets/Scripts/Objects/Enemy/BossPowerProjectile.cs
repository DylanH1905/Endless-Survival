using UnityEngine;
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
            if (distance <= hitDistance)
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            aimDirection = aimVector / distance;
            float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, aimAngle + spriteRotationOffset);

            float travelDistance = speed * Time.deltaTime;
            if (distance <= travelDistance + hitDistance)
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            transform.position += aimDirection * travelDistance;
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