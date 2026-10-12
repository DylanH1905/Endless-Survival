using System;
using ESS.Objects.Player;
using UnityEditor;
using UnityEngine;

namespace ESS.UI.GamePlay
{
	public class PlayerCombatController : MonoBehaviour
    {
        private Component activeBasicAttack;
        private readonly System.Collections.Generic.List<Component>
            activeSkills = new();

        public void ApplyCombat(CharacterData character)
        {
            if (character == null || character.combat == null)
            {
                Debug.LogWarning("Character combat is not configured!");
                return;
            }

            ClearCombat();

            activeBasicAttack = AddCombatComponent(
                character.combat.basicAttack
            );

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
            if (activeBasicAttack != null)
                Destroy(activeBasicAttack);

            foreach (Component skill in activeSkills)
            {
                if (skill != null)
                    Destroy(skill);
            }

            activeBasicAttack = null;
            activeSkills.Clear();
        }
	}
}