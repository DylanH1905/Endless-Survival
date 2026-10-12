
using UnityEngine;
using UnityEngine.EventSystems;

namespace ESS.Manager.Audio
{
    public class SFXTrigger : MonoBehaviour,
        IPointerEnterHandler,
        IPointerClickHandler
    {
		[SerializeField]
		private bool isWood = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
			if (isWood)
			{
				return;
			}

            AudioManager.Instance?.PlayHover();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
			if (isWood)
			{
				AudioManager.Instance?.PlayClickWood();
				return;
			}

            AudioManager.Instance?.PlayClick();
        }
    }
}