
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ESS;
using ESS.Objects.Player;
using System.Text;

namespace ESS.UI.GamePlay
{
    public class GameOverUI : SystemMonoBehaviour
    {
        private const string GameplaySceneName = "GamePlay";
        private const string SelectionSceneName = "Selection";

        private GameObject popupRoot;
        private GameObject pausePopupRoot;
        private Button pauseButton;

        private static readonly Color Gold = new Color(0.91f, 0.69f, 0.35f);
        private static readonly Color DarkBrown = new Color(0.20f, 0.12f, 0.07f, 0.98f);
        private static readonly Color LightBrown = new Color(0.29f, 0.19f, 0.12f, 0.98f);
        private static readonly Color White = new Color(0.96f, 0.92f, 0.84f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterBootstrap()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != GameplaySceneName)
                return;

            if (FindObjectOfType<GameOverUI>() != null)
                return;

            new GameObject(nameof(GameOverUI)).AddComponent<GameOverUI>();
        }

        private void OnEnable()
        {
            IPlayer.OnDeath += HandlePlayerDeath;
            HookPauseButton();
        }

        private void OnDisable()
        {
            IPlayer.OnDeath -= HandlePlayerDeath;
            if (pauseButton != null)
                pauseButton.onClick.RemoveListener(ShowPausePopup);
        }

        private void HandlePlayerDeath(IPlayer player)
        {
            if (popupRoot != null)
                return;

            EnsureEventSystem();

            string characterName = SelectedCharacter.Current != null
                ? SelectedCharacter.Current.characterName
                : "Unknown";

            int score = player != null ? player.score : 0;
            int level = player != null ? player.level : 1;

            // Capture before pausing the game.
            float duration = Time.timeSinceLevelLoad;

            GameHistoryStore.RecordRun(
                characterName,
                score,
                level
            );

            ClosePausePopup();
            BuildPopup(characterName, score, level, duration);

            Time.timeScale = 0f;
        }

        private void HookPauseButton()
        {
            GameObject pauseButtonObject = GameObject.Find("PauseGame");
            if (pauseButtonObject == null)
                return;

            pauseButton = pauseButtonObject.GetComponent<Button>();
            if (pauseButton == null)
                return;

            pauseButton.onClick.RemoveListener(ShowPausePopup);
            pauseButton.onClick.AddListener(ShowPausePopup);
        }

        private void ShowPausePopup()
        {
            if (pausePopupRoot != null || popupRoot != null)
                return;

            EnsureEventSystem();
            Time.timeScale = 0f;

            GameObject canvasObject = new GameObject(
                "PauseCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            pausePopupRoot = canvasObject;
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            GameObject overlay = CreateImage(
                "PauseOverlay",
                canvasRect,
                new Color(0.02f, 0.015f, 0.01f, 0.82f),
                Vector2.zero,
                Vector2.one
            );

            RectTransform panel = CreateRect(
                "PausePanel",
                overlay.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(700, 430)
            );
            panel.gameObject.AddComponent<Image>().color = DarkBrown;

            CreateText(
                panel, "PauseTitle", "PAUSED",
                56, FontStyles.Bold,
                new Vector2(0.05f, 0.62f),
                new Vector2(0.95f, 0.9f),
                Gold
            );

            CreateButton(
                panel,
                "ResumeButton",
                "RESUME",
                new Vector2(0.1f, 0.34f),
                new Vector2(0.9f, 0.53f),
                ResumeGame,
                new Color(0.69f, 0.39f, 0.14f)
            );

            CreateButton(
                panel,
                "PauseBackButton",
                "BACK TO SELECTION",
                new Vector2(0.1f, 0.1f),
                new Vector2(0.9f, 0.29f),
                BackToSelection,
                new Color(0.35f, 0.25f, 0.17f)
            );
        }

        private void ResumeGame()
        {
            Time.timeScale = 1f;
            ClosePausePopup();
        }

        private void ClosePausePopup()
        {
            if (pausePopupRoot == null)
                return;

            Destroy(pausePopupRoot);
            pausePopupRoot = null;
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
                return;

            GameObject eventSystem = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule)
            );

            DontDestroyOnLoad(eventSystem);
        }

        private void BuildPopup(
            string characterName,
            int score,
            int level,
            float duration)
        {
            GameObject canvasObject = new GameObject(
                "GameOverCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect =
                canvasObject.GetComponent<RectTransform>();

            popupRoot = CreateImage(
                "Overlay",
                canvasRect,
                new Color(0.02f, 0.015f, 0.01f, 0.82f),
                Vector2.zero,
                Vector2.one
            );

            RectTransform panel = CreateRect(
                "MainPanel",
                popupRoot.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(1500, 850)
            );

            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = DarkBrown;

            // Header
            CreateText(
                panel, "GameOverTitle", "GAME OVER",
                64, FontStyles.Bold,
                new Vector2(0.02f, 0.89f),
                new Vector2(0.98f, 0.99f),
                Gold
            );

            // LEFT: Current run
            RectTransform leftPanel = CreateSection(
                panel, "ResultPanel",
                new Vector2(0.025f, 0.19f),
                new Vector2(0.385f, 0.87f)
            );

            BuildResultPanel(
                leftPanel,
                characterName,
                score,
                level,
                duration
            );

            // RIGHT: Leaderboard
            RectTransform rightPanel = CreateSection(
                panel, "LeaderboardPanel",
                new Vector2(0.405f, 0.19f),
                new Vector2(0.975f, 0.87f)
            );

            BuildLeaderboard(rightPanel);

            // Footer buttons
            CreateButton(
                panel,
                "RestartButton",
                "RESTART",
                new Vector2(0.20f, 0.035f),
                new Vector2(0.48f, 0.145f),
                RestartGame,
                new Color(0.69f, 0.39f, 0.14f)
            );

            CreateButton(
                panel,
                "NewCharacterButton",
                "NEW CHARACTER",
                new Vector2(0.52f, 0.035f),
                new Vector2(0.80f, 0.145f),
                BackToSelection,
                new Color(0.35f, 0.25f, 0.17f)
            );
        }

        // ==========================================
        // LEFT PANEL: CURRENT RUN RESULT
        // ==========================================

        private void BuildResultPanel(
            RectTransform parent,
            string characterName,
            int score,
            int level,
            float duration)
        {
            CreateText(
                parent, "SectionTitle", "YOUR RESULT",
                32, FontStyles.Bold,
                new Vector2(0.05f, 0.88f),
                new Vector2(0.95f, 0.98f),
                Gold
            );

            CreateText(
                parent, "ScoreLabel", "TOTAL SCORE",
                20, FontStyles.Bold,
                new Vector2(0.05f, 0.68f),
                new Vector2(0.95f, 0.83f),
                Gold
            );

            CreateText(
                parent, "ScoreValue", score.ToString("N0"),
                66, FontStyles.Bold,
                new Vector2(0.05f, 0.49f),
                new Vector2(0.95f, 0.70f),
                White
            );

            CreateImage(
                "Divider",
                parent,
                Gold,
                new Vector2(0.10f, 0.46f),
                new Vector2(0.90f, 0.465f)
            );

            CreateStatRow(
                parent, "CHARACTER", characterName, 0.37f
            );

            CreateStatRow(
                parent, "LEVEL REACHED", level.ToString(), 0.25f
            );

            CreateStatRow(
                parent, "SURVIVAL TIME", FormatDuration(duration), 0.13f
            );
        }

        private void CreateStatRow(
            RectTransform parent,
            string label,
            string value,
            float y)
        {
            CreateText(
                parent, label + "Label", label,
                18, FontStyles.Bold,
                new Vector2(0.08f, y),
                new Vector2(0.52f, y + 0.09f),
                Gold,
                TextAlignmentOptions.MidlineLeft
            );

            CreateText(
                parent, label + "Value", value,
                22, FontStyles.Bold,
                new Vector2(0.48f, y),
                new Vector2(0.92f, y + 0.09f),
                White,
                TextAlignmentOptions.MidlineRight
            );
        }

        // ==========================================
        // RIGHT PANEL: LEADERBOARD TABLE
        // ==========================================

        private void BuildLeaderboard(RectTransform parent)
        {
            CreateText(
                parent, "LeaderboardTitle", "LEADERBOARD",
                32, FontStyles.Bold,
                new Vector2(0.03f, 0.88f),
                new Vector2(0.97f, 0.98f),
                Gold
            );

            // Column headers
            float headerTop = 0.83f;
            float headerBottom = 0.72f;

            CreateImage(
                "TableHeaderBackground",
                parent,
                new Color(0.42f, 0.27f, 0.14f),
                new Vector2(0.025f, headerBottom),
                new Vector2(0.975f, headerTop)
            );

            CreateCell(parent, "RankHeader", "#",
                18, FontStyles.Bold, 0.025f, 0.09f,
                headerBottom, headerTop, Gold);

            CreateCell(parent, "CharacterHeader", "CHARACTER",
                18, FontStyles.Bold, 0.09f, 0.39f,
                headerBottom, headerTop, Gold);

            CreateCell(parent, "TimeHeader", "TIME",
                18, FontStyles.Bold, 0.39f, 0.58f,
                headerBottom, headerTop, Gold);

            CreateCell(parent, "ScoreHeader", "SCORE",
                18, FontStyles.Bold, 0.58f, 0.80f,
                headerBottom, headerTop, Gold);

            CreateCell(parent, "LevelHeader", "LV",
                18, FontStyles.Bold, 0.80f, 0.975f,
                headerBottom, headerTop, Gold);

            GameHistoryStore.GameHistoryEntry[] entries =
                GameHistoryStore.GetLeaderboard(5);

            if (entries == null || entries.Length == 0)
            {
                CreateText(
                    parent, "EmptyLeaderboard",
                    "No games recorded yet",
                    22, FontStyles.Italic,
                    new Vector2(0.05f, 0.35f),
                    new Vector2(0.95f, 0.65f),
                    White
                );

                return;
            }

            float rowHeight = 0.115f;
            float startY = 0.705f;

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];

                float top = startY - i * rowHeight;
                float bottom = top - rowHeight + 0.005f;

                Color rowColor = i % 2 == 0
                    ? new Color(0.25f, 0.17f, 0.11f, 0.9f)
                    : new Color(0.17f, 0.12f, 0.08f, 0.9f);

                CreateImage(
                    "RowBackground_" + i,
                    parent,
                    rowColor,
                    new Vector2(0.025f, bottom),
                    new Vector2(0.975f, top)
                );

                string name = string.IsNullOrEmpty(entry.characterName)
                    ? "Unknown"
                    : entry.characterName;

                Color rankColor = i == 0 ? Gold : White;

                CreateCell(parent, "Rank_" + i,
                    (i + 1).ToString(),
                    21, FontStyles.Bold,
                    0.025f, 0.09f, bottom, top, rankColor);

                CreateCell(parent, "Character_" + i,
                    name,
                    19, FontStyles.Bold,
                    0.09f, 0.39f, bottom, top, White,
                    TextAlignmentOptions.MidlineLeft);

                CreateCell(parent, "Time_" + i,
                    entry.playedAtUtc,
                    18, FontStyles.Normal,
                    0.39f, 0.58f, bottom, top, White);

                CreateCell(parent, "Score_" + i,
                    entry.score.ToString("N0"),
                    19, FontStyles.Bold,
                    0.58f, 0.80f, bottom, top, Gold);

                CreateCell(parent, "Level_" + i,
                    entry.level.ToString(),
                    19, FontStyles.Normal,
                    0.80f, 0.975f, bottom, top, White);
            }
        }

        private void CreateCell(
            RectTransform parent,
            string name,
            string value,
            float fontSize,
            FontStyles style,
            float xMin,
            float xMax,
            float yMin,
            float yMax,
            Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            CreateText(
                parent, name, value,
                fontSize, style,
                new Vector2(xMin, yMin),
                new Vector2(xMax, yMax),
                color,
                alignment
            );
        }

        // ==========================================
        // UI HELPERS
        // ==========================================

        private RectTransform CreateSection(
            RectTransform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            RectTransform rect = CreateRect(
                name, parent, anchorMin, anchorMax
            );

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = LightBrown;

            return rect;
        }

        private RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2? size = null)
        {
            GameObject obj = new GameObject(
                name, typeof(RectTransform)
            );

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            if (size.HasValue)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size.Value;
            }

            return rect;
        }

        private GameObject CreateImage(
            string name,
            Transform parent,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject obj = new GameObject(
                name, typeof(RectTransform), typeof(Image)
            );

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            obj.GetComponent<Image>().color = color;

            return obj;
        }

        private TextMeshProUGUI CreateText(
            RectTransform parent,
            string name,
            string content,
            float fontSize,
            FontStyles style,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(5, 0);
            rect.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();

            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Ellipsis;

            return text;
        }

        private void CreateButton(
            RectTransform parent,
            string name,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            UnityEngine.Events.UnityAction onClick,
            Color color)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = obj.GetComponent<Image>();
            image.color = color;

            Button button = obj.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            CreateText(
                rect, "Label", label,
                27, FontStyles.Bold,
                Vector2.zero, Vector2.one,
                White
            );
        }

        private static string FormatDuration(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int minutes = totalSeconds / 60;
            int remainingSeconds = totalSeconds % 60;

            return $"{minutes:00}:{remainingSeconds:00}";
        }

        // ==========================================
        // BUTTON ACTIONS
        // ==========================================

        private void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(GameplaySceneName);
        }

        private void BackToSelection()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SelectionSceneName);
        }
    }
}
