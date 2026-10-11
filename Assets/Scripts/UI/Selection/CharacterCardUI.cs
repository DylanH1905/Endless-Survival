using System;
using TMPro;
using ESS.Objects.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ESS.UI.Selection
{
	public class CharacterCardUI : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private Button button;
		[SerializeField] private Image portraitImage;
		[SerializeField] private TMP_Text nameText;
		[SerializeField] private GameObject highlight;

		private CharacterData characterData;
		private Action<CharacterData> onSelected;

		public CharacterData Character => characterData;

		public void Setup(
			CharacterData data,
			Action<CharacterData> selectionCallback
		)
		{
			characterData = data;
			onSelected = selectionCallback;

			portraitImage.sprite = data.portrait;
			nameText.text = data.characterName;

			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(HandleClick);

			SetSelected(false);
		}

		private void HandleClick()
		{
			onSelected?.Invoke(characterData);
		}

		public void SetSelected(bool selected)
		{
			highlight.SetActive(selected);
		}
	}
}