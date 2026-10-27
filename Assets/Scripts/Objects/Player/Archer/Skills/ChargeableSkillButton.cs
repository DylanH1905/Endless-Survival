using UnityEngine;
using UnityEngine.EventSystems;

namespace ESS.Objects.Player.Archer.Skills
{
	public class ChargeableSkillButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		private IChargeableSkill skill;
		private bool isPointerHeld;

		public void Bind(IChargeableSkill chargeableSkill)
		{
			skill = chargeableSkill;
			isPointerHeld = false;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (skill == null || !((ISkill)skill).CanExecute) return;
			skill.BeginCharge();
			isPointerHeld = skill.IsCharging;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (!isPointerHeld || skill == null) return;
			isPointerHeld = false;
			skill.ReleaseCharge();
		}
	}
}
