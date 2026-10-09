using System.Collections;
using UnityEngine;

namespace ESS.Objects
{
    public enum EnvironmentObjectType
    {
		/// <summary>
		/// Environment object types that can exist in the game world.
		/// </summary>
        Terrain,

		/// <summary>
		/// Player can hide on bushes. Enemy cannot see through if not nearby.
		/// </summary>
        Bush,

		/// <summary>
		/// Player can move the stone slowly.
		/// </summary>
        Stone,
		
		/// <summary>
		/// Impassable obstacle for both Player and Enemy.
		/// </summary>
        Wall,

		/// <summary>
		/// Player can swim or be slowed down in water.
		/// </summary>
        Water,

		/// <summary>
		/// Wind can push the player or affect movement.
		/// </summary>
        Wind,

		/// <summary>
		/// Provides a speed boost or special movement ability.
		/// </summary>
        Shoes,

		/// <summary>
		/// Restores health or provides temporary buffs.
		/// </summary>
        Food,

		/// <summary>
		/// Triggers a harmful effect when the player interacts with it.
		/// </summary>
        Trap
    }

    public enum EnvironmentEffectType
    {
		/// <summary>
		/// Environment effect types that can be applied to characters.
		/// </summary>
        None,

		/// <summary>
		/// Instantly restores the character's health.
		/// </summary>
        Heal,

		/// <summary>
		/// Increases the character's movement speed temporarily.
		/// </summary>
        SpeedBuff,

		/// <summary>
		/// Inflicts damage to the character.
		/// </summary>
        Damaged,

		/// <summary>
		/// Temporarily increases the character's attack damage.
		/// </summary>
        ScaleDamage,

		/// <summary>
		/// Stuns the character, preventing movement and actions.
		/// </summary>
        Stun,

		/// <summary>
		/// Reduces the character's movement speed temporarily.
		/// </summary>
        Slow,

		/// <summary>
		/// Blocks the character's movement.
		/// </summary>
        Obstruction,
		
		/// <summary>
		/// Blocks the character's movement.
		/// </summary>
        Freeze,
    }

    // Pairs an EnvironmentObjectType with the UI icon used to represent and render it (e.g. in EnvironmentSpawner or a minimap legend).
    [System.Serializable]
    public class EnvironmentObjectHandle
    {
        public EnvironmentObjectType type;
        public Sprite icon;
        [Tooltip("Uniform render/collider scale applied when this object is spawned")]
        public float size = 0.3f;
    }

    public class EnvironmentCharacter : SystemMonoBehaviour
    {
        [Tooltip("What kind of environment object this is (Bush/Terrain/Stone/Wall/...)")]
        public EnvironmentObjectType objectType;

        [Tooltip("Static object (e.g. terrain/wall) that blocks the Player's straight movement")]
        public bool blocksMovement;

        [Tooltip("Whether touching this object applies an effect (heal/buff/trap) to a Character")]
        public bool effectObject;

        public ObjectEffect effect;
    }


    public class ObjectEffect
    {
        public bool isActive;

        public float duration;

        public float intensity;

        public EnvironmentEffectType effectType;

        public ObjectCharacter target;

        // delta applied by a temporary effect, kept so it can be reverted exactly
        private float appliedAmount;

        public void ApplyEffect()
        {
            if (target == null || !isActive)
            {
                return;
            }

            switch (effectType)
            {
                case EnvironmentEffectType.Heal:
                    target.Heal((int)intensity);
                    break;
                case EnvironmentEffectType.SpeedBuff:
                    appliedAmount = intensity;
                    target.speed += appliedAmount;
                    break;
                case EnvironmentEffectType.Damaged:
                    target.TakeDamage((int)intensity);
                    break;
                case EnvironmentEffectType.ScaleDamage:
                    appliedAmount = Mathf.RoundToInt(target.attackDamage * intensity) - target.attackDamage;
                    target.attackDamage += (int)appliedAmount;
                    break;
                case EnvironmentEffectType.Stun:
                    target.Stun(duration);
                    break;
                case EnvironmentEffectType.Slow:
                    appliedAmount = intensity;
                    target.speed -= appliedAmount;
                    break;
                case EnvironmentEffectType.Obstruction:
                    // Implement obstruction effect logic here
                    break;
                
            }

            // SpeedBuff/ScaleDamage are temporary mutations; Heal/Damage are instant and permanent
            bool isTemporary = effectType == EnvironmentEffectType.SpeedBuff || effectType == EnvironmentEffectType.ScaleDamage;
            if (isTemporary && duration > 0f)
            {
                target.StartCoroutine(RevertAfterDuration());
            }
        }

        private IEnumerator RevertAfterDuration()
        {
            yield return new WaitForSeconds(duration);

            if (target == null)
            {
                yield break;
            }

            switch (effectType)
            {
                case EnvironmentEffectType.SpeedBuff:
                    target.speed -= appliedAmount;
                    break;
                case EnvironmentEffectType.ScaleDamage:
                    target.attackDamage -= (int)appliedAmount;
                    break;
            }

            isActive = false;
        }
    }
}