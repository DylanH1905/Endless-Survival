using System;
using ESS.Objects.Player;
using UnityEditor;
using UnityEngine;
using ESS.Objects.Player.Archer; 
using ESS.Objects.Player.Gunner; 
using ESS.Objects.Player.Sword; 

namespace ESS.UI.GamePlay
{
	public class PlayerCombatController : MonoBehaviour
	{
		private Behaviour activeBasicAttack;
		private bool ownsActiveBasicAttack;
		private readonly System.Collections.Generic.List<Component>
			activeSkills = new();

		public ISkill[] GetActiveSkills()
		{
			System.Collections.Generic.List<ISkill> result = new();
			foreach (Component component in activeSkills)
				if (component is ISkill skill) result.Add(skill);
			return result.ToArray();
		}

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
				activeBasicAttack = AddCombatComponent(character.combat.basicAttack) as Behaviour;
				ownsActiveBasicAttack = activeBasicAttack != null;
			}

			if (activeBasicAttack is IBasicAttack basicAttack)
			{
				basicAttack.ConfigureAttackRange(character.attackRange);
				basicAttack.ConfigureAttacksPerSecond(character.attackSpeed);
			}

			if (activeBasicAttack is ESS.Objects.Player.Archer.BasicAttack archerAttack)
			{
				archerAttack.ConfigureBowAnimation(
					character.weapon,
					character.visuals != null ? character.visuals.weaponAttackSprite : null,
					character.visuals != null ? character.visuals.weaponDrawDuration : 0.15f,
					character.visuals != null ? character.visuals.arrowPullbackDistance : 0.3f);
			}

			if (activeBasicAttack is ESS.Objects.Player.Gunner.BasicAttack gunnerAttack)
			{
				gunnerAttack.ConfigureWeaponAttackAnimation(
					character.weapon,
					character.visuals != null ? character.visuals.weaponAttackSprite : null,
					character.visuals != null ? character.visuals.weaponDrawDuration : 0.15f);
			}

			if (activeBasicAttack is ESS.Objects.Player.Sword.BasicAttack swordAttack)
			{
				swordAttack.ConfigureWeaponAttackAnimation(
					character.weapon,
					character.visuals != null ? character.visuals.weaponAttackSprite : null,
					character.visuals != null ? character.visuals.weaponDrawDuration : 0.15f);
			}

			CharacterSkillConfig[] configuredSkills = character.combat.skillConfigs;
			if (configuredSkills != null && configuredSkills.Length > 0)
			{
				foreach (CharacterSkillConfig skillConfig in configuredSkills)
				{
					if (skillConfig == null) continue;
					Component skillComponent = AddCombatComponent(skillConfig.skill);
					if (skillComponent is ISkill skill)
					{
						skill.ConfigureCooldown(skillConfig.cooldown);
						skill.ConfigureRequiredPlayerLevel(skillConfig.minimumPlayerLevel);
						skill.ConfigureCharacterPresentation(skillConfig.icon, skillConfig.soundEffect);
						if (skillComponent is ESS.Objects.Player.Archer.Skills.ArcherSkill archerSkill)
							archerSkill.ConfigureVisuals(skillConfig.visuals);
						if (skillComponent is ESS.Objects.Player.Sword.Skills.SwordSkill swordSkill)
							swordSkill.ConfigureVisuals(skillConfig.visuals);
						activeSkills.Add(skillComponent);
					}
					else if (skillComponent != null)
					{
						Debug.LogError($"Configured skill {skillConfig.skill.name} does not inherit from ISkill.", this);
						Destroy(skillComponent);
					}
				}
				return;
			}

			// Keep existing CharacterData assets working until their skill list is migrated.
			if (character.combat.skills == null) return;
			foreach (MonoScript legacySkill in character.combat.skills)
			{
				Component skill = AddCombatComponent(legacySkill);
				if (skill != null) activeSkills.Add(skill);
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
