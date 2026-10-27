using System;
using ESS.Objects.Player;
using UnityEditor;
using UnityEngine;

namespace ESS.UI.GamePlay
{
	public class PlayerCombatController : MonoBehaviour
    {
        private Behaviour activeBasicAttack;
        private bool ownsActiveBasicAttack;
        private readonly System.Collections.Generic.List<Component>
            activeSkills = new();

        public void ApplyCombat(CharacterData character)
        {
            if (character == null || character.combat == null)
            {
                Debug.LogWarning("Character combat is not configured!");
                return;
            }

            Component previousOwnedBasicAttack = ownsActiveBasicAttack
                ? activeBasicAttack
                : null;
            ClearCombat();

            Type basicAttackType = character.combat.basicAttack != null
                ? character.combat.basicAttack.GetClass()
                : null;

            Component configuredBasicAttack = null;
            foreach (MonoBehaviour component
                     in GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == previousOwnedBasicAttack
                    || !(component is IPlayerAttack))
                {
                    continue;
                }

                bool matchesSelectedAttack = basicAttackType != null
                    && basicAttackType.IsInstanceOfType(component)
                    && configuredBasicAttack == null;

                component.enabled = matchesSelectedAttack;
                if (matchesSelectedAttack)
                {
                    configuredBasicAttack = component;
                }
            }

            if (configuredBasicAttack != null)
            {
                activeBasicAttack = configuredBasicAttack as Behaviour;
            }
            else
            {
                activeBasicAttack = AddCombatComponent(
                    character.combat.basicAttack) as Behaviour;
                ownsActiveBasicAttack = activeBasicAttack != null;
            }

            if (activeBasicAttack is ESS.Objects.Player.Archer.BasicAttack archerAttack
                && character.visuals != null)
            {
                archerAttack.ConfigureBowAnimation(
                    character.visuals.weaponAttackSprite,
                    character.visuals.weaponDrawDuration,
                    character.visuals.arrowPullbackDistance);
            }

            if (character.combat.skills == null)
                return;

            foreach (MonoScript skillScript in character.combat.skills)
            {
                Component skill = AddCombatComponent(skillScript);

                if (skill != null)
                    activeSkills.Add(skill);
            }
        }

        private Component AddCombatComponent(MonoScript script)
        {
            if (script == null)
                return null;

            Type type = script.GetClass();

            if (type == null ||
                !typeof(MonoBehaviour).IsAssignableFrom(type) ||
                type.IsAbstract)
            {
                Debug.LogError(
                    $"Invalid combat script: {script.name}",
                    this
                );

                return null;
            }

            return gameObject.AddComponent(type);
        }

        private void ClearCombat()
        {
            if (activeBasicAttack != null && ownsActiveBasicAttack)
            {
                activeBasicAttack.enabled = false;
                Destroy(activeBasicAttack);
            }

            foreach (Component skill in activeSkills)
            {
                if (skill != null)
                    Destroy(skill);
            }

            activeBasicAttack = null;
            ownsActiveBasicAttack = false;
            activeSkills.Clear();
        }
	}
}
