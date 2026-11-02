using ESS.Objects.Player;
using UnityEngine;

namespace ESS.Objects.Environment
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class Food : EnvironmentCharacter
    {
        [SerializeField, Range(0f, 1f)] private float healFraction = 0.3f;

        private bool consumed;

        private void Awake()
        {
            objectType = EnvironmentObjectType.Food;
            blocksMovement = false;
            effectObject = true;
            effect = new ObjectEffect();

            Rigidbody2D body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.useFullKinematicContacts = true;
        }

        public void ConfigureHealFraction(float fraction)
        {
            healFraction = Mathf.Clamp01(fraction);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (consumed)
            {
                return;
            }

            ObjectCharacter character = other.GetComponentInParent<ObjectCharacter>();
            if (character == null || character.IsDead || character.health >= character.maxHealth)
            {
                return;
            }

            int previousHealth = character.health;
            effect.target = character;
            effect.effectType = EnvironmentEffectType.Heal;
            effect.intensity = Mathf.Max(1, Mathf.RoundToInt(character.maxHealth * healFraction));
            effect.isActive = true;
            effect.ApplyEffect();

            if (character.health > previousHealth)
            {
                consumed = true;
                Destroy(gameObject);
            }
        }
    }
}