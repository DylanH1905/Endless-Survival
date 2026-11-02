using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ESS.Objects
{
    public class ObjectCharacter : SystemMonoBehaviour
    {
        public int health;
        public int maxHealth;
        public int attackDamage;
        public float speed;
        public bool IsDead { get; protected set; }
        public bool IsHidden { get; set; }

        [SerializeField] private Color freezeOverlayColor = new Color(0.45f, 0.82f, 1f, 0.5f);
        [SerializeField, Min(0f)] private float freezePulseSpeed = 5f;
        [SerializeField, Range(0f, 0.1f)] private float freezeScalePulse = 0.035f;

        private int shieldHealth;
        private Coroutine shieldRoutine;
        public bool HasActiveShield => shieldHealth > 0;
        public float ShieldCollisionRadius { get; private set; }
        private float slowMultiplier = 1f;
        private float slowRemaining;
        private float stunRemaining;
        private Coroutine knockbackRoutine;
        private readonly List<SpriteRenderer> freezeSources = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> freezeOverlays = new List<SpriteRenderer>();
        private bool freezeOverlayCreated;

        public float MovementSpeed => stunRemaining > 0f ? 0f : speed * slowMultiplier;
        public bool IsStunned => stunRemaining > 0f;

        protected virtual void Update()
        {
            if (slowRemaining > 0f)
            {
                slowRemaining -= Time.deltaTime;
                if (slowRemaining <= 0f)
                {
                    slowRemaining = 0f;
                    slowMultiplier = 1f;
                }
            }

            if (stunRemaining > 0f)
                stunRemaining = Mathf.Max(0f, stunRemaining - Time.deltaTime);

            UpdateFreezeOverlay();
        }

        public virtual void TakeDamage(int damage)
        {
            if (IsDead || damage <= 0)
            {
                Info($"{name} is already dead or received non-positive damage.");
                return;
            }

            int absorbedDamage = Mathf.Min(shieldHealth, damage);
            shieldHealth -= absorbedDamage;
            int remainingDamage = damage - absorbedDamage;
            health = Mathf.Max(health - remainingDamage, 0);
            Info($"{name} took {damage} damage ({absorbedDamage} absorbed), remaining health: {health}");
            if (health == 0)
            {
                IsDead = true;
                Info($"{name} has died.");
            }
        }

        public void GrantShield(int amount, float duration, float collisionRadius = 0f)
        {
            if (IsDead || amount <= 0 || duration <= 0f)
            {
                return;
            }

            shieldHealth = amount;
            ShieldCollisionRadius = Mathf.Max(collisionRadius, 0f);
            if (shieldRoutine != null)
            {
                StopCoroutine(shieldRoutine);
            }

            shieldRoutine = StartCoroutine(ShieldDuration(duration));
        }

        private IEnumerator ShieldDuration(float duration)
        {
            yield return new WaitForSeconds(duration);
            shieldHealth = 0;
            ShieldCollisionRadius = 0f;
            shieldRoutine = null;
        }

        public virtual void Heal(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            health = Mathf.Min(health + amount, maxHealth);
            Info($"{name} healed {amount}, current health: {health}");
        }

        public virtual void Stun(float duration)
        {
            if (!IsDead && duration > 0f)
            {
                stunRemaining = Mathf.Max(stunRemaining, duration);
                EnsureFreezeOverlay();
            }
        }

        public virtual void ApplySlow(float duration, float speedMultiplier)
        {
            if (IsDead || duration <= 0f)
                return;

            slowMultiplier = Mathf.Min(slowMultiplier, Mathf.Clamp01(speedMultiplier));
            slowRemaining = Mathf.Max(slowRemaining, duration);
        }

        public void ApplyKnockback(Vector2 direction, float distance, float duration)
        {
            if (IsDead || direction.sqrMagnitude <= Mathf.Epsilon || distance <= 0f || duration <= 0f)
                return;

            if (knockbackRoutine != null)
                StopCoroutine(knockbackRoutine);
            knockbackRoutine = StartCoroutine(Knockback(direction.normalized, distance, duration));
        }

        private IEnumerator Knockback(Vector2 direction, float distance, float duration)
        {
            float elapsed = 0f;
            float previousProgress = 0f;
            while (elapsed < duration)
            {
                elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
                float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.position += (Vector3)(direction * (distance * (progress - previousProgress)));
                previousProgress = progress;
                yield return null;
            }
            knockbackRoutine = null;
        }

        private void EnsureFreezeOverlay()
        {
            if (freezeOverlayCreated)
            {
                return;
            }

            freezeOverlayCreated = true;
            foreach (SpriteRenderer source in GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (source == null || source.sprite == null || !source.enabled
                    || !source.gameObject.activeInHierarchy
                    || source.transform.name == "Weapon"
                    || source.transform.name == "Power")
                {
                    continue;
                }

                GameObject overlayObject = new GameObject("FreezeOverlay");
                overlayObject.transform.SetParent(source.transform, false);
                SpriteRenderer overlay = overlayObject.AddComponent<SpriteRenderer>();
                overlay.sprite = source.sprite;
                overlay.sortingLayerID = source.sortingLayerID;
                overlay.sortingOrder = source.sortingOrder + 1;
                overlay.maskInteraction = source.maskInteraction;
                freezeSources.Add(source);
                freezeOverlays.Add(overlay);
            }
        }

        private void UpdateFreezeOverlay()
        {
            if (!freezeOverlayCreated)
            {
                return;
            }

            float pulse = (Mathf.Sin(Time.time * freezePulseSpeed) + 1f) * 0.5f;
            Color overlayColor = freezeOverlayColor;
            overlayColor.a *= 0.8f + pulse * 0.2f;
            float scale = 1f + freezeScalePulse * pulse;

            for (int index = 0; index < freezeOverlays.Count; index++)
            {
                SpriteRenderer source = freezeSources[index];
                SpriteRenderer overlay = freezeOverlays[index];
                if (source == null || overlay == null)
                {
                    continue;
                }

                overlay.enabled = IsStunned && source.enabled && source.gameObject.activeInHierarchy;
                overlay.sprite = source.sprite;
                overlay.flipX = source.flipX;
                overlay.flipY = source.flipY;
                overlay.sortingLayerID = source.sortingLayerID;
                overlay.sortingOrder = source.sortingOrder + 1;
                overlay.color = overlayColor;
                overlay.transform.localScale = Vector3.one * scale;
            }
        }
    }
}
