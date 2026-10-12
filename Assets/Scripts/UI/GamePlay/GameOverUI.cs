using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ESS.Objects.Player;

namespace ESS.UI.GamePlay
{
    // Listens for IPlayer.OnDeath and shows a Game Over popup with the final score
    // plus Restart / Back to Selection options. Freezes gameplay via Time.timeScale.
    public class GameOverUI : SystemMonoBehaviour
    {
        private const string GameplaySceneName = "GamePlay";
        private const string SelectionSceneName = "Selection";

        private GameObject popupRoot;

        // SceneManager.LoadScene (Selection -> GamePlay, restart) doesn't re-trigger
        // RuntimeInitializeOnLoadMethod, so hook sceneLoaded instead of checking once at startup.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterBootstrap()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != GameplaySceneName)
            {
                return;
            }

            if (FindObjectOfType<GameOverUI>() != null)
            {
                return;
            }

            new GameObject(nameof(GameOverUI)).AddComponent<GameOverUI>();
        }

        private void OnEnable()
        {
            IPlayer.OnDeath += HandlePlayerDeath;
        }

        private void OnDisable()
        {
            IPlayer.OnDeath -= HandlePlayerDeath;
        }

        private void HandlePlayerDeath(IPlayer player)
        {
            if (popupRoot != null)
            {
                return;
            }

            EnsureEventSystem();
            BuildPopup(player != null ? player.score : 0);
            Time.timeScale = 0f;
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject(
                "EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            DontDestroyOnLoad(eventSystem);
        }

        private void BuildPopup(int score)
        {
            GameObject canvasObject = new GameObject(
                "GameOverCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();

            popupRoot = CreateStretchedImage(
                "Overlay", canvasRect, new Color(0f, 0f, 0f, 0.65f));

            RectTransform panel = CreatePanel(popupRoot.transform);

            CreateText(
                panel, "Title", "GAME OVER", 64, FontStyles.Bold,
                new Vector2(0.06f, 0.68f), new Vector2(0.94f, 0.9f));

            CreateText(
                panel, "Score", $"Score: {score}", 40, FontStyles.Normal,
                new Vector2(0.06f, 0.5f), new Vector2(0.94f, 0.65f));

            CreateButton(
                panel, "RestartButton", "Restart",
                new Vector2(0.1f, 0.14f), new Vector2(0.47f, 0.34f), RestartGame);

            CreateButton(
                panel, "BackButton", "New Character",
                new Vector2(0.53f, 0.14f), new Vector2(0.9f, 0.34f), BackToSelection);
        }

        private RectTransform CreatePanel(Transform parent)
        {
            GameObject panel = new GameObject(
                "Panel", typeof(RectTransform), typeof(Image));

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(720f, 420f);

            panel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f, 0.95f);

            return rect;
        }

        private GameObject CreateStretchedImage(string objectName, Transform parent, Color color)
        {
            GameObject imageObject = new GameObject(
                objectName, typeof(RectTransform), typeof(Image));

            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            imageObject.GetComponent<Image>().color = color;

            return imageObject;
        }

        private TextMeshProUGUI CreateText(
            RectTransform parent,
            string objectName,
            string content,
            float fontSize,
            FontStyles style,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject textObject = new GameObject(
                objectName, typeof(RectTransform), typeof(TextMeshProUGUI));

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            return text;
        }

        private void CreateButton(
            RectTransform parent,
            string objectName,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = new GameObject(
                objectName, typeof(RectTransform), typeof(Image), typeof(Button));

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            buttonObject.GetComponent<Image>().color = new Color(0.85f, 0.58f, 0.18f);

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            CreateText(
                rect, "Label", label, 28, FontStyles.Bold,
                Vector2.zero, Vector2.one);
        }

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
