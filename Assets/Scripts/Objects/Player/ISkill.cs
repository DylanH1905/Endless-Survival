using UnityEngine;
using UnityEngine.UI;

namespace ESS.Objects.Player
{
	public abstract class ISkill : SystemMonoBehaviour
	{
		private Sprite characterIcon;
		private AudioClip characterSoundEffect;

		public abstract string SkillName { get; }
		public abstract float Cooldown { get; }
		public abstract float CooldownRemaining { get; }
		public abstract void ConfigureCooldown(float cooldown);
		public abstract float ManaCost { get; }
		public abstract bool IsUnlocked { get; }
		public abstract bool CanExecute { get; }
		public abstract Image SkillIcon { get; }
		public Sprite CharacterIcon => characterIcon;
		public abstract int Level { get; set; }

		public void ConfigureCharacterPresentation(Sprite icon, AudioClip soundEffect)
		{
			characterIcon = icon;
			characterSoundEffect = soundEffect;
		}

		protected void PlayCharacterSkillSound()
		{
			if (characterSoundEffect != null)
				ESS.Manager.Audio.AudioManager.Instance?.PlayClip(characterSoundEffect);
		}

		public abstract void LevelUp();

		public abstract void Execute();
	}
}
