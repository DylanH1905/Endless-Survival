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
		[Tooltip("Optional per-skill effects and projectile appearance.")]
		public SkillVisualConfig visuals = new SkillVisualConfig();
	}

	[Serializable]
	public class SkillVisualConfig
	{
		[Header("Cast and Impact")]
		public GameObject castEffectPrefab;
		public GameObject impactEffectPrefab;

		[Header("Projectile")]
		[Tooltip("Use a prefab when the skill needs a custom animated projectile. Otherwise use projectileSprite.")]
		public GameObject projectilePrefab;
		public Sprite projectileSprite;
		public Color projectileTint = Color.white;
		[Min(0.01f)] public float projectileScale = 1f;
		[Tooltip("Local Z rotation for projectile art whose forward direction is not up.")]
		[Range(-180f, 180f)] public float projectileRotationOffset;
		public bool useProjectileTrail = true;
	}
}
