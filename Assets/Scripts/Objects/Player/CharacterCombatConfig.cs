using System;
using UnityEditor;
using UnityEngine;

namespace ESS.Objects.Player
{
	[Serializable]
	public class CharacterCombatConfig
	{
		[Header("Combat Scripts")]
		public MonoScript basicAttack;
		[Tooltip("Legacy script list. New characters should use Skill Configs below.")]
		[HideInInspector] public MonoScript[] skills;
		public CharacterSkillConfig[] skillConfigs;
	}

	[Serializable]
	public class CharacterSkillConfig
	{
		[Tooltip("Concrete MonoBehaviour implementing ISkill.")]
		public MonoScript skill;
		public Sprite icon;
		public AudioClip soundEffect;
	}
}
