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

        public virtual void TakeDamage(int damage)
        {
            if (IsDead || damage <= 0)
            {
                Info($"{name} is already dead or received non-positive damage.");
                return;
            }

            health = Mathf.Max(health - damage, 0);
            Info($"{name} took {damage} damage, remaining health: {health}");
            if (health == 0)
            {
                IsDead = true;
                Info($"{name} has died.");
            }
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
            if (IsDead)
            {
                return;
            }

            // Implement stun logic here (e.g., disable movement for the duration)
            StartCoroutine(StunCoroutine(duration));
        }

        private IEnumerator StunCoroutine(float duration)
        {
            // Disable movement or other actions here
            float originalSpeed = speed;
            speed = 0f;
            yield return new WaitForSeconds(duration);
            // Re-enable movement or other actions here
            speed = originalSpeed;
        }
    }
}
