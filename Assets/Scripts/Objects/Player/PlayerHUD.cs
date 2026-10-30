using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ESS.Objects.Player.Archer.Skills;

namespace ESS.Objects.Player
{
	/// <summary>Connects the editor-built HUD objects to the active player and skills.</summary>
	public class PlayerHUD : SystemMonoBehaviour
	{
		[Header("Optional references (auto-found by child name when empty)")]
		[SerializeField] private IPlayer player;
		[SerializeField] private Image healthFillImage;
		[SerializeField] private Text healthText;
		[SerializeField] private Image expFillImage;
		[SerializeField] private Text expText;
		[SerializeField] private Text levelText;
		[SerializeField] private Text pointText;
		[SerializeField] private Text speedText;
		[SerializeField] private Text attackText;
		[SerializeField] private Text attackSpeedText;
		[SerializeField] private Button[] skillButtonSlots = new Button[4];
		[SerializeField] private Color healthFillColor = new Color(0.85f, 0.12f, 0.12f, 1f);
		[SerializeField] private Color expFillColor = new Color(0.25f, 0.55f, 0.95f, 1f);

		private IBasicAttack basicAttack;
		private ISkill[] characterSkills;
		private readonly List<SkillButtonView> skillButtons = new List<SkillButtonView>();
		private static readonly string[] skillKeys = { "Q", "W", "E", "R" };
		private static Sprite cooldownCircleSprite;
		private static Sprite fillSprite;

		private void Awake()
		{
			Canvas canvas = GetComponentInParent<Canvas>();
			if (canvas != null && canvas.transform.localScale.sqrMagnitude < 0.0001f)
				canvas.transform.localScale = Vector3.one;
			ResolveHudReferences();
			FindPlayer();
			RefreshSkillButtons();
			RefreshStats();
		}

		private void Update()
		{
			if (player == null) FindPlayer();
			if (player != null && basicAttack == null) basicAttack = player.GetComponent<IBasicAttack>();
			RefreshSkillButtons();
			RefreshSkillButtonStates();
			RefreshStats();
		}

		private void FindPlayer()
		{
			if (player == null) player = FindObjectOfType<IPlayer>();
			if (player != null) basicAttack = player.GetComponent<IBasicAttack>();
		}

		public void ApplyCharacter(IPlayer selectedPlayer, ISkill[] selectedSkills)
		{
			player = selectedPlayer;
			characterSkills = selectedSkills ?? System.Array.Empty<ISkill>();
			basicAttack = player != null ? player.GetComponent<IBasicAttack>() : null;
			ResolveHudReferences();
			RefreshSkillButtons();
			RefreshSkillButtonStates();
			RefreshStats();
		}

		private void ResolveHudReferences()
		{
			healthFillImage = healthFillImage != null ? healthFillImage : FindComponent<Image>("StatsPanel/HealthBar/Fill");
			healthText = healthText != null ? healthText : FindComponent<Text>("HealthText");
			expFillImage = expFillImage != null ? expFillImage : FindComponent<Image>("StatsPanel/ExperienceBar/Fill");
			expText = expText != null ? expText : FindComponent<Text>("ExperienceText");
			levelText = levelText != null ? levelText : FindComponent<Text>("LevelText");
			pointText = pointText != null ? pointText : FindComponent<Text>("PointText");
			speedText = speedText != null ? speedText : FindComponent<Text>("SpeedText");
			attackText = attackText != null ? attackText : FindComponent<Text>("AttackText");
			attackSpeedText = attackSpeedText != null ? attackSpeedText : FindComponent<Text>("AttackSpeedText");
			ConfigureFillImage(healthFillImage, healthFillColor);
			ConfigureFillImage(expFillImage, expFillColor);
			ConfigureBackground(FindComponent<Image>("StatsPanel"), new Color(0.04f, 0.05f, 0.08f, 0.82f));
			ConfigureBackground(FindComponent<Image>("StatsPanel/HealthBar/Background"), new Color(0.08f, 0.08f, 0.08f, 0.95f));
			ConfigureBackground(FindComponent<Image>("StatsPanel/ExperienceBar/Background"), new Color(0.08f, 0.08f, 0.08f, 0.95f));
			ConfigureText(healthText, TextAnchor.MiddleCenter, 15);
			ConfigureText(expText, TextAnchor.MiddleCenter, 12);
			ConfigureText(levelText, TextAnchor.MiddleCenter, 16);
			ConfigureText(pointText, TextAnchor.MiddleCenter, 14);
			ConfigureText(speedText, TextAnchor.MiddleCenter, 14);
			ConfigureText(attackText, TextAnchor.MiddleCenter, 14);
			ConfigureText(attackSpeedText, TextAnchor.MiddleCenter, 14);

			if (skillButtonSlots == null || skillButtonSlots.Length != 4)
				skillButtonSlots = new Button[4];
			for (int i = 0; i < skillButtonSlots.Length; i++)
			{
				if (skillButtonSlots[i] == null)
					skillButtonSlots[i] = FindComponent<Button>($"SkillArc/SkillButton{i + 1}");
			}
		}

		private static void ConfigureFillImage(Image image, Color color)
		{
			if (image == null) return;
			if (fillSprite == null)
			{
				Texture2D texture = Texture2D.whiteTexture;
				fillSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
			}
			if (image.sprite == null) image.sprite = fillSprite;
			image.type = Image.Type.Filled;
			image.fillMethod = Image.FillMethod.Horizontal;
			image.fillOrigin = (int)Image.OriginHorizontal.Left;
			image.color = color;
			image.raycastTarget = false;
		}

		private static void ConfigureBackground(Image image, Color color)
		{
			if (image == null) return;
			image.color = color;
			image.raycastTarget = false;
		}

		private static void ConfigureText(Text text, TextAnchor alignment, int size)
		{
			if (text == null) return;
			text.color = Color.white;
			text.alignment = alignment;
			text.fontSize = size;
			text.raycastTarget = false;
		}

		private T FindComponent<T>(string relativePath) where T : Component
		{
			Transform root = transform;
			bool hasPath = relativePath.IndexOf('/') >= 0;
			int slash = relativePath.IndexOf('/');
			while (slash >= 0)
			{
				string segment = relativePath.Substring(0, slash);
				root = FindDirectChild(root, segment);
				if (root == null) return null;
				relativePath = relativePath.Substring(slash + 1);
				slash = relativePath.IndexOf('/');
			}

			Transform target = FindDirectChild(root, relativePath);
			if (target != null) return target.GetComponent<T>();
			if (hasPath) return null;

			foreach (T component in GetComponentsInChildren<T>(true))
				if (component.name == relativePath) return component;
			return null;
		}

		private static Transform FindDirectChild(Transform parent, string childName)
		{
			if (parent == null) return null;
			for (int i = 0; i < parent.childCount; i++)
			{
				if (parent.GetChild(i).name == childName) return parent.GetChild(i);
			}
			return null;
		}

		private void RefreshSkillButtons()
		{
			if (player == null) return;
			ISkill[] skills = characterSkills ?? player.GetComponents<ISkill>();
			int expectedCount = Mathf.Min(skills.Length, skillButtonSlots.Length);
			bool unchanged = expectedCount == skillButtons.Count;
			for (int i = 0; unchanged && i < skills.Length; i++)
				if (i < expectedCount) unchanged = skillButtons[i].Skill == skills[i];
			if (unchanged) return;

			skillButtons.Clear();
			foreach (Button slot in skillButtonSlots)
			{
				if (slot == null) continue;
				slot.onClick.RemoveAllListeners();
				slot.interactable = false;
			}
			for (int i = 0; i < skills.Length && i < skillButtonSlots.Length; i++)
			{
				ISkill skill = skills[i];
				Button button = skillButtonSlots[i];
				if (skill == null || button == null) continue;

				button.onClick.RemoveAllListeners();
				ChargeableSkillButton chargeButton = button.GetComponent<ChargeableSkillButton>();
				if (skill is IChargeableSkill chargeableSkill)
				{
					if (chargeButton == null) chargeButton = button.gameObject.AddComponent<ChargeableSkillButton>();
					chargeButton.Bind(chargeableSkill);
				}
				else
				{
					if (chargeButton != null) chargeButton.Bind(null);
					button.onClick.AddListener(skill.Execute);
				}
				Image buttonImage = button.GetComponent<Image>();
				Image icon = FindInChildren<Image>(button.transform, "Icon");
				Image overlay = FindInChildren<Image>(button.transform, "CooldownOverlay");
				Text cooldownText = FindInChildren<Text>(button.transform, "CooldownText");
				if (overlay == null) overlay = CreateCooldownOverlay(button.transform);
				if (cooldownText == null) cooldownText = CreateCooldownText(button.transform);
				if (overlay.sprite == null) overlay.sprite = GetCooldownCircleSprite();
				Sprite skillSprite = skill.CharacterIcon != null
					? skill.CharacterIcon
					: skill.SkillIcon != null ? skill.SkillIcon.sprite : null;
				if (icon != null && skillSprite != null) icon.sprite = skillSprite;
				else if (buttonImage != null && skillSprite != null) buttonImage.sprite = skillSprite;
				foreach (Graphic graphic in button.GetComponentsInChildren<Graphic>(true))
					if (graphic != buttonImage) graphic.raycastTarget = false;
				overlay.type = Image.Type.Filled;
				overlay.fillMethod = Image.FillMethod.Radial360;
				overlay.fillOrigin = (int)Image.Origin360.Top;
				overlay.raycastTarget = false;
				skillButtons.Add(new SkillButtonView(skill, button, overlay, cooldownText, i));
			}
		}

		private static T FindInChildren<T>(Transform parent, string childName) where T : Component
		{
			foreach (T component in parent.GetComponentsInChildren<T>(true))
				if (component.transform != parent && component.name == childName) return component;
			return null;
		}

		private static Image CreateCooldownOverlay(Transform parent)
		{
			GameObject overlayObject = new GameObject("CooldownOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			RectTransform rect = overlayObject.GetComponent<RectTransform>();
			rect.SetParent(parent, false);
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;

			Image overlay = overlayObject.GetComponent<Image>();
			overlay.sprite = GetCooldownCircleSprite();
			overlay.type = Image.Type.Filled;
			overlay.fillMethod = Image.FillMethod.Radial360;
			overlay.fillOrigin = (int)Image.Origin360.Top;
			overlay.color = new Color(0f, 0f, 0f, 0.72f);
			overlay.raycastTarget = false;
			return overlay;
		}

		private static Text CreateCooldownText(Transform parent)
		{
			GameObject textObject = new GameObject("CooldownText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
			RectTransform rect = textObject.GetComponent<RectTransform>();
			rect.SetParent(parent, false);
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;

			Text text = textObject.GetComponent<Text>();
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 20;
			text.fontStyle = FontStyle.Bold;
			text.alignment = TextAnchor.MiddleCenter;
			text.color = Color.white;
			text.raycastTarget = false;
			return text;
		}

		private static Sprite GetCooldownCircleSprite()
		{
			if (cooldownCircleSprite != null) return cooldownCircleSprite;

			const int size = 64;
			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
			texture.name = "CooldownCircle";
			texture.filterMode = FilterMode.Bilinear;
			Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
			float radius = size * 0.5f - 1f;
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float alpha = Mathf.Clamp01(radius + 0.5f - Vector2.Distance(new Vector2(x, y), center));
					texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
				}
			}
			texture.Apply();
			cooldownCircleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
			return cooldownCircleSprite;
		}

		private void RefreshSkillButtonStates()
		{
			foreach (SkillButtonView view in skillButtons)
			{
				if (view.Skill == null || view.Button == null) continue;
				view.Button.interactable = view.Skill.CanExecute;
				if (!view.Skill.IsUnlocked)
				{
					view.CooldownOverlay.enabled = true;
					view.CooldownOverlay.fillAmount = 1f;
					view.CooldownOverlay.color = new Color(0f, 0f, 0f, 0.82f);
					view.CooldownText.text = $"Lv. {view.Skill.RequiredPlayerLevel}";
					continue;
				}
				float cooldown = Mathf.Max(view.Skill.Cooldown, 0.01f);
				float remaining = Mathf.Max(view.Skill.CooldownRemaining, 0f);
				if (view.Skill is IChargeableSkill chargeable && chargeable.IsCharging)
				{
					view.CooldownOverlay.enabled = true;
					view.CooldownOverlay.fillAmount = chargeable.ChargeProgress;
					view.CooldownOverlay.color = new Color(0.2f, 0.75f, 1f, 0.72f);
					view.CooldownText.text = $"{Mathf.RoundToInt(chargeable.ChargeProgress * 100f)}%";
				}
				else
				{
					view.CooldownOverlay.enabled = remaining > 0f;
					view.CooldownOverlay.color = new Color(0f, 0f, 0f, 0.7f);
					view.CooldownOverlay.fillAmount = Mathf.Clamp01(remaining / cooldown);
					view.CooldownText.text = remaining > 0f
						? Mathf.CeilToInt(remaining).ToString()
						: skillKeys[view.Index];
				}
			}
		}

		private sealed class SkillButtonView
		{
			public readonly ISkill Skill;
			public readonly Button Button;
			public readonly Image CooldownOverlay;
			public readonly Text CooldownText;
			public readonly int Index;

			public SkillButtonView(ISkill skill, Button button, Image overlay, Text cooldownText, int index)
			{
				Skill = skill;
				Button = button;
				CooldownOverlay = overlay;
				CooldownText = cooldownText;
				Index = index;
			}
		}

		private void RefreshStats()
		{
			if (player == null) return;
			if (healthFillImage != null)
				healthFillImage.fillAmount = player.maxHealth > 0 ? Mathf.Clamp01((float)player.health / player.maxHealth) : 0f;
			if (healthText != null) healthText.text = $"{Mathf.Max(player.health, 0)} / {player.maxHealth}";
			if (expFillImage != null)
				expFillImage.fillAmount = player.experienceToNextLevel > 0 ? Mathf.Clamp01((float)player.experience / player.experienceToNextLevel) : 0f;
			if (expText != null) expText.text = $"{player.experience} / {player.experienceToNextLevel} XP";
			if (levelText != null) levelText.text = player.level.ToString();
			if (pointText != null) pointText.text = $"Point: {player.score}";
			if (speedText != null) speedText.text = $"Speed: {player.speed:0.0}";
			if (attackText != null) attackText.text = $"ATK: {player.attackDamage}";
			if (attackSpeedText != null)
				attackSpeedText.text = $"AS: {(basicAttack != null ? basicAttack.AttacksPerSecond : 0f):0.0}/s";
		}
	}
}
