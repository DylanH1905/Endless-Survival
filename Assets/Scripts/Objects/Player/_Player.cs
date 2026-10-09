using UnityEngine;
using ESS.Objects;

namespace ESS.Objects.Player
{
    public class IPlayer : ObjectCharacter
    {
        [SerializeField] private float colliderRadius = 0.4f;

        private void Awake()
        {
            InitializeStats();
            EnsureCollider();
        }

        public void InitializeStats()
        {
            maxHealth = 2000;
            attackDamage = 20;
            speed = 4f;

            health = maxHealth;
            IsDead = false;
        }

        // Environment pickups/obstacles (Shoes, Bush, Terrain, ...) rely on OnTrigger/OnCollision callbacks,
        // which never fire without a Collider2D on the Player.
        private void EnsureCollider()
        {
            if (GetComponent<Collider2D>() != null)
            {
                return;
            }

            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = colliderRadius;
        }

    }
}