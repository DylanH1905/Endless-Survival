using System;
using UnityEngine;

namespace ESS.Objects.Player
{
	[CreateAssetMenu(
		fileName = "CharacterData",
		menuName = "ESS/Characters/Character Data"
	)]
	public class CharacterData : ScriptableObject
	{
		[Header("Basic Information")]
		public string characterId;
		public string characterName;
		[TextArea(2, 4)]
		public string description;
		public Sprite portrait;

		[Header("Attributes")]
		public float maxHealth = 100f;
		public float damage = 10f;
		public float attackSpeed = 1f;
		public float attackRange = 2f;

		[Header("Combat Setup")]
		public CharacterCombatConfig combat;

		[Header("Visual Setup")]
		public CharacterVisualConfig visuals = new CharacterVisualConfig();

		[Header("Gameplay")]
		public Sprite head;
		public Sprite power = null;
		public Sprite weapon = null;

		[Header("Audio")]
		public AudioClip baseAttack;
		public AudioClip FistSkill = null;
		public AudioClip SecondSkill = null;
		public AudioClip ThirdSkill = null;
	}

	public enum CharacterWeaponAimMode
	{
		None,
		MirrorAroundHead,
		RotateAroundHead
	}

	[Serializable]
	public class CharacterVisualConfig
	{
		[Tooltip("Flip the head sprite toward the current target.")]
		public bool flipHeadTowardTarget = true;
		[Tooltip("Choose whether the weapon mirrors or rotates around the Head pivot.")]
		public CharacterWeaponAimMode weaponAimMode;
		[Tooltip("Angle correction for weapon sprites whose forward axis is not right.")]
		[Range(-180f, 180f)] public float weaponAngleOffset;
		[Tooltip("Optional weapon sprite used while charging an attack, such as a drawn bow.")]
		public Sprite weaponAttackSprite;
		[Min(0.01f)] public float weaponDrawDuration = 0.15f;
		[Min(0f)] public float arrowPullbackDistance = 0.3f;
	}
}
