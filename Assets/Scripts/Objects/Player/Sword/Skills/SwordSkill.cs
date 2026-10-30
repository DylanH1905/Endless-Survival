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
		protected SkillVisualConfig Visuals => visuals;
		protected virtual bool UseProjectileSpriteAsCastEffect => true;
		public override float Cooldown => cooldown;
		public override float CooldownRemaining => cooldownRemaining;
		public override void ConfigureCooldown(float value)
		{
			cooldown = Mathf.Max(0.1f, value);
			cooldownRemaining = Mathf.Min(cooldownRemaining, cooldown);
		}
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
			else if (UseProjectileSpriteAsCastEffect
				&& visuals != null
				&& visuals.projectileSprite != null)
			{
				GameObject castEffect = new GameObject($"{SkillName}CastEffect");
				castEffect.transform.position = Owner.transform.position;
				castEffect.transform.localScale = Vector3.one * visuals.projectileScale;
				SpriteRenderer spriteRenderer = castEffect.AddComponent<SpriteRenderer>();
				spriteRenderer.sprite = visuals.projectileSprite;
				spriteRenderer.color = visuals.projectileTint;
				spriteRenderer.sortingOrder = 100;
				Destroy(castEffect, 0.3f);
			}
			return true;
		}

		protected virtual void Update()
		{
			cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
		}

		public override void LevelUp() => Level++;
	}
}
