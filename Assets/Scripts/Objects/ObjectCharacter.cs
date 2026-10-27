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

        private int shieldHealth;
        private Coroutine shieldRoutine;
        private float slowMultiplier = 1f;
        private float slowRemaining;
        private float stunRemaining;

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

        public void GrantShield(int amount, float duration)
        {
            if (IsDead || amount <= 0 || duration <= 0f)
            {
                return;
            }

            shieldHealth = amount;
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
                stunRemaining = Mathf.Max(stunRemaining, duration);
        }

        public virtual void ApplySlow(float duration, float speedMultiplier)
        {
            if (IsDead || duration <= 0f)
                return;

            slowMultiplier = Mathf.Min(slowMultiplier, Mathf.Clamp01(speedMultiplier));
            slowRemaining = Mathf.Max(slowRemaining, duration);
        }
    }
}
