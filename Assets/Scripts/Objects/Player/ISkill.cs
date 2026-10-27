using UnityEngine.UI;

namespace ESS.Objects.Player
{
	public abstract class ISkill : SystemMonoBehaviour
	{
		public abstract string SkillName { get; }
		public abstract float Cooldown { get; }
		public abstract float CooldownRemaining { get; }
		public abstract float ManaCost { get; }
		public abstract bool IsUnlocked { get; }
		public abstract bool CanExecute { get; }
		public abstract Image SkillIcon { get; }
		public abstract int Level { get; set; }

		public abstract void LevelUp();

		public abstract void Execute();
	}
}
