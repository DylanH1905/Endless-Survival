using UnityEngine;
using ESS.Objects;

namespace ESS.Objects.Player
{
    public class IPlayer : ObjectCharacter
    {
        private void Awake()
        {
            InitializeStats();
        }

        public void InitializeStats()
        {
            maxHealth = 2000;
            attackDamage = 20;
            speed = 4f;

            health = maxHealth;
            IsDead = false;
        }

    }
}