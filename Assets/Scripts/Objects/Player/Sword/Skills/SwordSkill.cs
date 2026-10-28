using UnityEngine;
using UnityEngine.UI;

namespace ESS.Objects.Player.Sword.Skills
{
	public abstract class SwordSkill : ISkill
	{
		[SerializeField, Min(0.1f)] private float cooldown = 8f;
		[SerializeField, Min(0f)] private float manaCost;
		[SerializeField] private bool isUnlocked = true;
		[SerializeField] private Image skillIcon;

		private float cooldownRemaining;
		private SkillVisualConfig visuals;
		public override float Cooldown => cooldown;
		public override float CooldownRemaining => cooldownRemaining;
		public override float ManaCost => manaCost;
		public override bool IsUnlocked => isUnlocked;
		public override bool CanExecute => IsUnlocked && cooldownRemaining <= 0f && Owner != null && !Owner.IsDead;
		public override Image SkillIcon => skillIcon;
		public override int Level { get; set; } = 1;

		protected IPlayer Owner
		{
			get
			{
				IPlayer player = GetComponent<IPlayer>();
				return player != null ? player : GetComponentInParent<IPlayer>();
			}
		}

		public void ConfigureVisuals(SkillVisualConfig config)
		{
			visuals = config;
		}

		protected bool TryBeginCast()
		{
			if (!CanExecute)
			{
				return false;
			}

			cooldownRemaining = cooldown;
			PlayCharacterSkillSound();
			if (visuals != null && visuals.castEffectPrefab != null)
				Instantiate(visuals.castEffectPrefab, Owner.transform.position, Quaternion.identity);
			return true;
		}

		protected virtual void Update()
		{
			cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
		}

		public override void LevelUp() => Level++;
	}
}
