using UnityEngine;
using ESS;
using ESS.Objects.Player;
using UnityEngine.EventSystems;

namespace ESS.UI.GamePlay
{

	public class Joystick : SystemMonoBehaviour,
		IPointerDownHandler,
		IDragHandler,
		IPointerUpHandler
	{
		[SerializeField] private RectTransform background;
		[SerializeField] private RectTransform handle;

		public Vector2 Direction { get; private set; }

		private float radius;

		private void Awake()
		{
			radius = background.sizeDelta.x / 2f;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			OnDrag(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				background,
				eventData.position,
				eventData.pressEventCamera,
				out Vector2 localPoint
			);

			Direction = Vector2.ClampMagnitude(
				localPoint / radius, 1f
			);

			handle.anchoredPosition = Direction * radius;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			Direction = Vector2.zero;
			handle.anchoredPosition = Vector2.zero;
		}
	}
}