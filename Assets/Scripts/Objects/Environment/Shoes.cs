using ESS.Objects.Player;
using UnityEngine;

namespace ESS.Objects.Environment
{
    // One-time pickup that gives the Player a temporary speed multiplier, then destroys itself.
    [RequireComponent(typeof(Collider2D))]
    class Shoes : EnvironmentCharacter
    {
        [SerializeField] private float speedMultiplier = 1.5f;
        [SerializeField] private float buffDuration = 2f;

        private void Awake()
        {
            InitializeShoes();
        }
        private void InitializeShoes()
        {
            objectType = EnvironmentObjectType.Shoes;
            blocksMovement = false;
            effectObject = true;
            effect = new ObjectEffect();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            IPlayer player = other.GetComponent<IPlayer>();
            if (player == null)
            {
                return;
            }

            effect.target = player;
            effect.effectType = EnvironmentEffectType.SpeedBuff;
            effect.duration = buffDuration;
            // intensity is an additive delta, so derive it from the multiplier and the player's current speed
            effect.intensity = player.speed * (speedMultiplier - 1f);
            effect.isActive = true;
            effect.ApplyEffect();

            Destroy(gameObject);
        }
    }
}