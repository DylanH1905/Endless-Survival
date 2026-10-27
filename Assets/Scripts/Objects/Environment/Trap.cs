using ESS.Objects.Player;
using UnityEngine;

namespace ESS.Objects.Environment
{

    [RequireComponent(typeof(Collider2D))]
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

        private void Awake()
        {
            InitializeTrap();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            IPlayer player = other.GetComponent<IPlayer>();
            if (player == null)
            {
                return;
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