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
}