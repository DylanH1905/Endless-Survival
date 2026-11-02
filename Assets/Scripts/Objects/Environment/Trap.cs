using ESS.Objects.Player;
using UnityEngine;

namespace ESS.Objects.Environment
{

    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class Trap : EnvironmentCharacter
    {
        private void InitializeTrap()
        {
            objectType = EnvironmentObjectType.Trap;
            blocksMovement = false;
			effectObject = true;
			effect = new ObjectEffect();
        }

        [SerializeField, Min(0.1f)] private float damage = 0.3f;
        private SpriteRenderer spriteRenderer;
        private bool triggered;

        private void Awake()
        {
            InitializeTrap();

            Rigidbody2D body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.useFullKinematicContacts = true;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered)
            {
                return;
            }

            IPlayer player = other.GetComponentInParent<IPlayer>();
            if (player == null)
            {
                return;
            }

            triggered = true;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            effect.target = player;
            effect.effectType = EnvironmentEffectType.Damaged;
            effect.intensity = damage * player.maxHealth;
            effect.isActive = true;
            effect.ApplyEffect();

            // Destroy(gameObject);
        }
    }
}