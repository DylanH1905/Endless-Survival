
using System;
using System.Collections.Generic;
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

		[Header("Character Data")]
		[SerializeField] private List<CharacterData> characters = new();

		[Header("UI")]
		[SerializeField] private Button playButton;
		[SerializeField] private string gameplaySceneName = "Gameplay";
		[SerializeField] private Text nameText;
		[SerializeField] private Text descriptionText;
		[SerializeField] private Text healthText;
		[SerializeField] private Text damageText;
		[SerializeField] private Text speedText;
		[SerializeField] private Text rangeText;
		[SerializeField] private Text skillCountdownText;
		[SerializeField] private SkillDetailView[] skillDetails =
		{
			new SkillDetailView(), new SkillDetailView(),
			new SkillDetailView(), new SkillDetailView()
		};

		private readonly List<Button> cards = new();
		private readonly List<CharacterData> cardData = new();

		private Transform cardContainer;

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

			RectTransform portraitRect = portrait.GetComponent<RectTransform>();

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
				typeof(Text)
			);

			label.transform.SetParent(card.transform, false);

			RectTransform labelRect = label.GetComponent<RectTransform>();

			labelRect.anchorMin = new Vector2(0.34f, 0.4f);
			labelRect.anchorMax = new Vector2(0.88f, 0.75f);
			labelRect.offsetMin = Vector2.zero;
			labelRect.offsetMax = Vector2.zero;

			Text labelText = label.GetComponent<Text>();

			labelText.text = character.characterName;
			labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			labelText.fontSize = 36;
			labelText.color = Color.white;
			labelText.alignment = TextAnchor.MiddleCenter;
			labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
			labelText.verticalOverflow = VerticalWrapMode.Truncate;
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

			if (nameText != null) nameText.text = character.characterName;
			if (descriptionText != null) descriptionText.text = character.description;
			if (healthText != null) healthText.text = $"{character.maxHealth:0}";
			if (damageText != null) damageText.text = $"{character.damage:0.##}";
			if (speedText != null) speedText.text = $"{character.attackSpeed:0.##}";
			if (rangeText != null) rangeText.text = $"{character.attackRange:0.##}";

			CharacterSkillConfig[] configs = character.combat?.skillConfigs;
			for (int index = 0; index < skillDetails.Length; index++)
			{
				CharacterSkillConfig config = configs != null && index < configs.Length
					? configs[index]
					: null;
				skillDetails[index].Set(config);
			}

			if (skillCountdownText != null)
			{
				List<string> cooldownLines = new();
				if (configs != null)
				{
					foreach (CharacterSkillConfig config in configs)
					{
						if (config == null) continue;
						string skillName = string.IsNullOrEmpty(config.skillName)
							? config.skill?.name
							: config.skillName;
						cooldownLines.Add($"{skillName}: {config.cooldown:0.##}s");
					}
				}
				skillCountdownText.text = string.Join("\n", cooldownLines);
			}
		}

		[Serializable]
		private class SkillDetailView
		{
			[SerializeField] private Transform group;
			[SerializeField] private Image icon;
			[SerializeField] private Text nameText;
			[SerializeField] private Text descriptionText;
			[SerializeField] private Text countdownText;

			public void Set(CharacterSkillConfig config)
			{
				if (group != null) group.gameObject.SetActive(config != null);
				if (config == null) return;

				if (icon != null)
				{
					icon.sprite = config.icon;
					icon.enabled = config.icon != null;
				}
				if (nameText != null)
					nameText.text = string.IsNullOrEmpty(config.skillName) ? config.skill?.name : config.skillName;
				if (descriptionText != null)
					descriptionText.text = config.description;
				if (countdownText != null)
					countdownText.text = $"{config.cooldown:0.##}s";
			}
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