using UnityEngine;
using UnityEngine.UI;

namespace ESS.Objects.Player
{
    // Builds a League-of-Legends-style stat bar (Health / Point / Speed / Attack / Attack Speed) entirely at runtime.
    // Attach to the Main Camera (or any scene object); it creates its own Canvas so no prefab wiring is required.
    public class PlayerHUD : SystemMonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private IPlayer player;
        [SerializeField] private IBasicAttack basicAttack;

        [Header("Layout")]
        [SerializeField] private Vector2 panelSize = new Vector2(480f, 108f);
        [SerializeField] private Vector2 bottomOffset = new Vector2(0f, 20f);

        [Header("Colors")]
        [SerializeField] private Color panelBackgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.75f);
        [SerializeField] private Color healthBackgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.95f);
        [SerializeField] private Color healthFillColor = new Color(0.85f, 0.12f, 0.12f, 1f);
        [SerializeField] private Color expBackgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.95f);
        [SerializeField] private Color expFillColor = new Color(0.25f, 0.55f, 0.95f, 1f);
        [SerializeField] private Color pointColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color speedColor = new Color(0.3f, 0.8f, 1f, 1f);
        [SerializeField] private Color attackColor = new Color(1f, 0.5f, 0.2f, 1f);
        [SerializeField] private Color attackSpeedColor = new Color(0.7f, 0.4f, 1f, 1f);

        private static Sprite fillSprite;

        private Font defaultFont;
        private Image healthFillImage;
        private Text healthText;
        private Image expFillImage;
        private Text expText;
        private Text levelText;
        private Text pointText;
        private Text speedText;
        private Text attackText;
        private Text attackSpeedText;

        private void Awake()
        {
            if (player == null)
            {
                player = FindObjectOfType<IPlayer>();
            }

            if (basicAttack == null && player != null)
            {
                basicAttack = player.GetComponent<IBasicAttack>();
            }

            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            RectTransform canvasRect = BuildCanvas();
            BuildHud(canvasRect);
            RefreshStats();
        }

        private void Update()
        {
            RefreshStats();
        }

        private RectTransform BuildCanvas()
        {
            GameObject canvasObject = new GameObject("HUDCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = GetComponent<Camera>() != null ? GetComponent<Camera>() : Camera.main;
            canvas.planeDistance = 1f;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvasObject.GetComponent<RectTransform>();
        }

        private void BuildHud(RectTransform canvasRect)
        {
            GameObject panel = new GameObject("PlayerHUDPanel", typeof(RectTransform), typeof(Image));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.SetParent(canvasRect, false);
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = bottomOffset;
            panelRect.sizeDelta = panelSize;
            panel.GetComponent<Image>().color = panelBackgroundColor;

            BuildHealthBar(panelRect);
            BuildExperienceBar(panelRect);
            BuildLevelBadge(panelRect);

            GameObject statsRow = new GameObject("StatsRow", typeof(RectTransform));
            RectTransform statsRowRect = statsRow.GetComponent<RectTransform>();
            statsRowRect.SetParent(panelRect, false);
            statsRowRect.anchorMin = new Vector2(0f, 0f);
            statsRowRect.anchorMax = new Vector2(1f, 0f);
            statsRowRect.pivot = new Vector2(0.5f, 0f);
            statsRowRect.anchoredPosition = new Vector2(0f, 8f);
            statsRowRect.sizeDelta = new Vector2(-16f, 32f);

            HorizontalLayoutGroup statsLayout = statsRow.AddComponent<HorizontalLayoutGroup>();
            statsLayout.childAlignment = TextAnchor.MiddleCenter;
            statsLayout.spacing = 14f;
            statsLayout.childForceExpandWidth = false;
            statsLayout.childForceExpandHeight = false;

            pointText = CreateStatItem(statsRowRect, "Point", pointColor);
            speedText = CreateStatItem(statsRowRect, "Speed", speedColor);
            attackText = CreateStatItem(statsRowRect, "ATK", attackColor);
            attackSpeedText = CreateStatItem(statsRowRect, "AS", attackSpeedColor);
        }

        private void BuildHealthBar(RectTransform panelRect)
        {
            GameObject healthContainer = new GameObject("HealthBar", typeof(RectTransform));
            RectTransform healthRect = healthContainer.GetComponent<RectTransform>();
            healthRect.SetParent(panelRect, false);
            healthRect.anchorMin = new Vector2(0f, 1f);
            healthRect.anchorMax = new Vector2(1f, 1f);
            healthRect.pivot = new Vector2(0.5f, 1f);
            healthRect.anchoredPosition = new Vector2(0f, -8f);
            healthRect.sizeDelta = new Vector2(-16f, 26f);

            CreateFullStretchImage(healthRect, "Background", healthBackgroundColor);
            healthFillImage = CreateFullStretchImage(healthRect, "Fill", healthFillColor);
            healthFillImage.sprite = GetFillSprite();
            healthFillImage.type = Image.Type.Filled;
            healthFillImage.fillMethod = Image.FillMethod.Horizontal;
            healthFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            healthFillImage.fillAmount = 1f;

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(healthRect, false);
            StretchFull(textRect);

            healthText = textObject.GetComponent<Text>();
            healthText.font = defaultFont;
            healthText.fontSize = 16;
            healthText.alignment = TextAnchor.MiddleCenter;
            healthText.color = Color.white;
        }

        private void BuildExperienceBar(RectTransform panelRect)
        {
            GameObject expContainer = new GameObject("ExperienceBar", typeof(RectTransform));
            RectTransform expRect = expContainer.GetComponent<RectTransform>();
            expRect.SetParent(panelRect, false);
            expRect.anchorMin = new Vector2(0f, 1f);
            expRect.anchorMax = new Vector2(1f, 1f);
            expRect.pivot = new Vector2(0.5f, 1f);
            expRect.anchoredPosition = new Vector2(0f, -38f);
            expRect.sizeDelta = new Vector2(-16f, 12f);

            CreateFullStretchImage(expRect, "Background", expBackgroundColor);
            expFillImage = CreateFullStretchImage(expRect, "Fill", expFillColor);
            expFillImage.sprite = GetFillSprite();
            expFillImage.type = Image.Type.Filled;
            expFillImage.fillMethod = Image.FillMethod.Horizontal;
            expFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            expFillImage.fillAmount = 0f;

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(expRect, false);
            StretchFull(textRect);

            expText = textObject.GetComponent<Text>();
            expText.font = defaultFont;
            expText.fontSize = 11;
            expText.alignment = TextAnchor.MiddleCenter;
            expText.color = Color.white;
        }

        private void BuildLevelBadge(RectTransform panelRect)
        {
            GameObject badge = new GameObject("LevelBadge", typeof(RectTransform), typeof(Image));
            RectTransform badgeRect = badge.GetComponent<RectTransform>();
            badgeRect.SetParent(panelRect, false);
            badgeRect.anchorMin = new Vector2(0f, 1f);
            badgeRect.anchorMax = new Vector2(0f, 1f);
            badgeRect.pivot = new Vector2(0f, 1f);
            badgeRect.anchoredPosition = new Vector2(8f, -8f);
            badgeRect.sizeDelta = new Vector2(26f, 26f);
            badge.GetComponent<Image>().color = expFillColor;

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(badgeRect, false);
            StretchFull(textRect);

            levelText = textObject.GetComponent<Text>();
            levelText.font = defaultFont;
            levelText.fontSize = 13;
            levelText.alignment = TextAnchor.MiddleCenter;
            levelText.color = Color.white;
            levelText.text = "1";
        }

        private Text CreateStatItem(Transform parent, string label, Color accentColor)
        {
            GameObject item = new GameObject(label + "Stat", typeof(RectTransform));
            item.transform.SetParent(parent, false);

            HorizontalLayoutGroup layout = item.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            LayoutElement itemLayoutElement = item.AddComponent<LayoutElement>();
            itemLayoutElement.preferredWidth = 100f;
            itemLayoutElement.preferredHeight = 28f;

            GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(item.transform, false);
            icon.GetComponent<Image>().color = accentColor;
            LayoutElement iconLayoutElement = icon.AddComponent<LayoutElement>();
            iconLayoutElement.preferredWidth = 16f;
            iconLayoutElement.preferredHeight = 16f;

            GameObject textObject = new GameObject("Value", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(item.transform, false);

            Text text = textObject.GetComponent<Text>();
            text.font = defaultFont;
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.text = label;

            LayoutElement textLayoutElement = textObject.AddComponent<LayoutElement>();
            textLayoutElement.preferredWidth = 80f;
            textLayoutElement.preferredHeight = 20f;

            return text;
        }

        private Image CreateFullStretchImage(Transform parent, string objectName, Color color)
        {
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            StretchFull(imageObject.GetComponent<RectTransform>());

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static void StretchFull(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        // Image.Type.Filled only clips correctly when a sprite is assigned - without one, Unity silently
        // falls back to drawing an unclipped quad and fillAmount has no visible effect.
        private static Sprite GetFillSprite()
        {
            if (fillSprite != null)
            {
                return fillSprite;
            }

            Texture2D texture = Texture2D.whiteTexture;
            fillSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return fillSprite;
        }

        private void RefreshStats()
        {
            if (player == null)
            {
                return;
            }

            float fillAmount = player.maxHealth > 0
                ? Mathf.Clamp01((float)player.health / player.maxHealth)
                : 0f;

            if (healthFillImage != null)
            {
                healthFillImage.fillAmount = fillAmount;
            }

            if (healthText != null)
            {
                healthText.text = $"{Mathf.Max(player.health, 0)} / {player.maxHealth}";
            }

            float expFillAmount = player.experienceToNextLevel > 0
                ? Mathf.Clamp01((float)player.experience / player.experienceToNextLevel)
                : 0f;

            if (expFillImage != null)
            {
                expFillImage.fillAmount = expFillAmount;
            }

            if (expText != null)
            {
                expText.text = $"{player.experience} / {player.experienceToNextLevel} XP";
            }

            if (levelText != null)
            {
                levelText.text = player.level.ToString();
            }

            if (pointText != null)
            {
                pointText.text = $"Point: {player.score}";
            }

            if (speedText != null)
            {
                speedText.text = $"Speed: {player.speed:0.0}";
            }

            if (attackText != null)
            {
                attackText.text = $"ATK: {player.attackDamage}";
            }

            if (attackSpeedText != null)
            {
                float attacksPerSecond = basicAttack != null ? basicAttack.AttacksPerSecond : 0f;
                attackSpeedText.text = $"AS: {attacksPerSecond:0.0}/s";
            }
        }
    }
}
