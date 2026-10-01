using System.Collections;
using UnityEngine;

namespace ESS.Objects
{
    public enum EnvironmentObjectType
    {
        Terrain,
        Bush,
        Stone,
        Wall,
        Water,
        Wild,
        Shoes,
        Food,
    }

    public enum EnvironmentEffectType
    {
        None,
        Heal,
        SpeedBuff,
        Damaged,
        ScaleDamage,
        Stun,
        Slow,
        Obstruction // Movement blocking effect,
    }

    // Pairs an EnvironmentObjectType with the prefab/UI icon used to represent it (e.g. in EnvironmentSpawner or a minimap legend).
    [System.Serializable]
    public class EnvironmentObjectHandle
    {
        public EnvironmentObjectType type;
        public GameObject prefab;
        public Sprite icon;
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