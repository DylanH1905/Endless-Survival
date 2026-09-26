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
    }
}
