using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ESS.Objects.Enemy
{
    public class IEnemy : ObjectCharacter
    {
        public static event System.Action<IEnemy> OnDeath;

        public void Start()
        {
            InitializeStats();
        }

        public virtual void InitializeStats()
        {
            maxHealth = 150;
            attackDamage = 15;
            speed = 4f;

            health = maxHealth;
            IsDead = false;
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);

            if (IsDead)
            {
                OnDeath?.Invoke(this);
                Destroy(gameObject);
            }
        }
    }
}