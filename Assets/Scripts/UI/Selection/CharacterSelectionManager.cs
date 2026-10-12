
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ESS;
using ESS.Objects.Player;
using ESS.Manager.Audio;

namespace ESS.UI.Selection
{
	public class CharacterSelectionManager : MonoBehaviour
	{
		[Header("Card UI")]
		[SerializeField] private Sprite cardBackground;
		[SerializeField] private Sprite cardDetailsBackground;

		[Header("Character Data")]
		[SerializeField] private List<CharacterData> characters = new();

		[Header("UI")]
		[SerializeField] private Button playButton;
		[SerializeField] private string gameplaySceneName = "Gameplay";

		private readonly List<Button> cards = new();
		private readonly List<CharacterData> cardData = new();

		private Transform cardContainer;
		private TextMeshProUGUI nameText;
		private TextMeshProUGUI descriptionText;
		private TextMeshProUGUI healthText;
		private TextMeshProUGUI damageText;
		private TextMeshProUGUI speedText;
		private TextMeshProUGUI rangeText;

		private CharacterData selectedCharacter;

		private void Start()
		{
			// Fix Canvas scale nếu Scene đang lưu scale = 0.
			Canvas canvas = GetComponentInParent<Canvas>();
			if (canvas == null)
				canvas = FindFirstObjectByType<Canvas>();

			if (canvas != null)
				canvas.transform.localScale = Vector3.one;

			BuildUI();

			if (playButton != null)
			{
				playButton.onClick.RemoveAllListeners();
				playButton.onClick.AddListener(PlayGame);
			}

			if (characters.Count > 0)
			{
				foreach (CharacterData character in characters)
				{
					if (character != null)
					{
						SelectCharacter(character);
						break;
					}
				}
			}
		}

		private void BuildUI()
		{
			RectTransform root = transform as RectTransform;

			// SelectionManager phải là con của Canvas.
			if (root == null)
			{
				Debug.LogError("SelectionManager must be a UI object.");
				return;
			}

			root.anchorMin = Vector2.zero;
			root.anchorMax = Vector2.one;
			root.offsetMin = Vector2.zero;
			root.offsetMax = Vector2.zero;
			root.localScale = Vector3.one;

			GameObject listPanel = CreatePanel(
				"CharacterList", root,
				new Vector2(0.03f, 0.22f),
				new Vector2(0.28f, 0.95f),
				new Color(0.10f, 0.12f, 0.18f, 0.0f)
			);

			GameObject detailPanel = CreatePanel(
				"CharacterDetails", root,
				new Vector2(0.45f, 0.10f),
				new Vector2(0.95f, 0.95f),
				new Color(1.0f, 1.0f, 1.0f, 1.0f)
			);

			detailPanel.GetComponent<Image>().sprite = cardDetailsBackground;

			cardContainer = listPanel.transform;

			GridLayoutGroup grid = listPanel.AddComponent<GridLayoutGroup>();

			grid.cellSize = new Vector2(450, 150);
			grid.spacing = new Vector2(16, 16);
			grid.padding = new RectOffset(16, 16, 16, 16);
			grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
			grid.startAxis = GridLayoutGroup.Axis.Vertical;
			grid.constraintCount = 1;
			grid.childAlignment = TextAnchor.MiddleLeft;

			nameText = CreateText(
				"CharacterName", detailPanel.transform,
				new Vector2(0.08f, 0.71f),
				new Vector2(0.92f, 0.86f),
				72, FontStyles.Bold
			);
			nameText.alignment = TextAlignmentOptions.Center;

			descriptionText = CreateText(
				"Description", detailPanel.transform,
				new Vector2(0.08f, 0.58f),
				new Vector2(0.92f, 0.75f),
				36, FontStyles.Normal
			);

			healthText = CreateText("Health", detailPanel.transform,
				new Vector2(0.08f, 0.46f),
				new Vector2(0.92f, 0.55f), 36, FontStyles.Normal);

			damageText = CreateText("Damage", detailPanel.transform,
				new Vector2(0.08f, 0.36f),
				new Vector2(0.92f, 0.45f), 36, FontStyles.Normal);

			speedText = CreateText("AttackSpeed", detailPanel.transform,
				new Vector2(0.08f, 0.26f),
				new Vector2(0.92f, 0.35f), 36, FontStyles.Normal);

			rangeText = CreateText("AttackRange", detailPanel.transform,
				new Vector2(0.08f, 0.16f),
				new Vector2(0.92f, 0.25f), 36, FontStyles.Normal);

			foreach (CharacterData character in characters)
			{
				if (character == null)
					continue;

				CreateCard(character);
			}
		}

		private GameObject CreatePanel(
			string objectName,
			Transform parent,
			Vector2 anchorMin,
			Vector2 anchorMax,
			Color color)
		{
			GameObject panel = new GameObject(
				objectName, typeof(RectTransform), typeof(Image));

			panel.transform.SetParent(parent, false);

			RectTransform rect = panel.GetComponent<RectTransform>();

			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;

			panel.GetComponent<Image>().color = color;

			return panel;
		}

		private TextMeshProUGUI CreateText(
			string objectName,
			Transform parent,
			Vector2 anchorMin,
			Vector2 anchorMax,
			float fontSize,
			FontStyles style)
		{
			GameObject obj = new GameObject(
				objectName,
				typeof(RectTransform),
				typeof(CanvasRenderer),
				typeof(TextMeshProUGUI));

			obj.transform.SetParent(parent, false);

			RectTransform rect = obj.GetComponent<RectTransform>();
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;

			TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
			text.fontSize = fontSize;
			text.fontStyle = style;
			text.color = Color.white;
			text.alignment = TextAlignmentOptions.MidlineLeft;
			// text.textWrappingMode = TextWrappingModes.Normal;

			return text;
		}

		private void CreateCard(CharacterData character)
		{
			GameObject card = new GameObject(
				character.characterName,
				typeof(RectTransform),
				typeof(Image),
				typeof(Button),
				typeof(LayoutElement),
				typeof(SFXTrigger)
			);

			card.transform.SetParent(cardContainer, false);

			// Card size
			LayoutElement cardLayout = card.GetComponent<LayoutElement>();
			cardLayout.preferredWidth = 420;
			cardLayout.preferredHeight = 125;
			cardLayout.minWidth = 300;
			cardLayout.minHeight = 90;

			// Background
			Image background = card.GetComponent<Image>();
			background.sprite = cardBackground;
			background.type = Image.Type.Simple;
			background.preserveAspect = false;
			background.color = Color.white;

			// Portrait
			GameObject portrait = new GameObject(
				"Portrait",
				typeof(RectTransform),
				typeof(Image)
			);

			portrait.transform.SetParent(card.transform, false);

			RectTransform portraitRect =
				portrait.GetComponent<RectTransform>();

			portraitRect.anchorMin = new Vector2(0.055f, 0.23f);
			portraitRect.anchorMax = new Vector2(0.285f, 0.87f);
			portraitRect.offsetMin = Vector2.zero;
			portraitRect.offsetMax = Vector2.zero;

			Image portraitImage = portrait.GetComponent<Image>();
			portraitImage.sprite = character.portrait;
			portraitImage.preserveAspect = true;
			portraitImage.color = Color.white;
			portraitImage.raycastTarget = false;

			// Name
			GameObject label = new GameObject(
				"Name",
				typeof(RectTransform),
				typeof(TextMeshProUGUI)
			);

			label.transform.SetParent(card.transform, false);

			RectTransform labelRect =
				label.GetComponent<RectTransform>();

			labelRect.anchorMin = new Vector2(0.34f, 0.4f);
			labelRect.anchorMax = new Vector2(0.88f, 0.75f);
			labelRect.offsetMin = Vector2.zero;
			labelRect.offsetMax = Vector2.zero;

			TextMeshProUGUI labelText =
				label.GetComponent<TextMeshProUGUI>();

			labelText.text = character.characterName;
			labelText.fontSize = 36;
			labelText.color = Color.white;
			labelText.alignment = TextAlignmentOptions.Center;
			labelText.enableWordWrapping = false;
			labelText.overflowMode = TextOverflowModes.Ellipsis;
			labelText.raycastTarget = false;

			// Click Card
			Button button = card.GetComponent<Button>();
			button.targetGraphic = background;
			button.transition = Selectable.Transition.None;

			button.onClick.AddListener(
				() => SelectCharacter(character)
			);

			cards.Add(button);
			cardData.Add(character);
		}

		private void SelectCharacter(CharacterData character)
		{
			selectedCharacter = character;

			for (int i = 0; i < cards.Count; i++)
			{
				bool selected = cardData[i] == character;

				cards[i].GetComponent<Image>().color = selected
					? new Color(0.85f, 0.58f, 0.18f)
					: new Color(0.22f, 0.25f, 0.34f);
			}

			nameText.text = character.characterName;
			descriptionText.text = character.description;
			healthText.text = $"Health: {character.maxHealth:0}";
			damageText.text = $"Damage: {character.damage:0.##}";
			speedText.text = $"Attack Speed: {character.attackSpeed:0.##}";
			rangeText.text = $"Attack Range: {character.attackRange:0.##}";
		}

		private void PlayGame()
		{
			if (selectedCharacter == null)
			{
				Debug.LogWarning("Please select a character first.");
				return;
			}

			SelectedCharacter.Current = selectedCharacter;
			SceneManager.LoadScene(gameplaySceneName);
		}
	}
}