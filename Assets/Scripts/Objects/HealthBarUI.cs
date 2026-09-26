using ESS.Objects.Player;
using ESS.Objects.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace ESS.Objects
{
    [RequireComponent(typeof(RectTransform))]
    public class HealthBarUI : SystemMonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private ObjectCharacter target;

        [Header("Size")]
        [SerializeField] private Vector2 playerScreenSize = new Vector2(150f, 15f);
        [SerializeField] private Vector2 enemyScreenSize = new Vector2(60f, 8f);
        [SerializeField] private Vector2 playerWorldSize = new Vector2(1.5f, 0.15f);
        [SerializeField] private Vector2 enemyWorldSize = new Vector2(1.2f, 0.12f);
        [SerializeField, Range(0.1f, 1f)] private float playerSizeMultiplier = 0.5f;

        [Header("Style")]
        [SerializeField] private Color backgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.95f);
        [SerializeField] private Color fillColor = new Color(0.9f, 0.12f, 0.12f, 1f);
        [SerializeField] private float smoothSpeed = 8f;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1f, 0f);
        [SerializeField] private bool useCameraSpaceForEnemy = true;
        [SerializeField] private float cameraPlaneDistance = 10f;

        private Image fillImage;
        private float targetFillAmount;
        private Canvas parentCanvas;
        private RectTransform barRect;

        public float FillAmount => fillImage != null ? fillImage.fillAmount : 0f;

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponentInParent<ObjectCharacter>();
            }

            parentCanvas = GetComponentInParent<Canvas>();
            ConfigureEnemyCanvas();
            barRect = GetBarRect();
            BuildBar();
            UpdateTargetSize();
            UpdateTargetHealth();
        }

        private void Update()
        {
            UpdateTargetHealth();

            if (fillImage != null)
            {
                float nextFillAmount = Mathf.MoveTowards(
                    fillImage.fillAmount,
                    targetFillAmount,
                    smoothSpeed * Time.deltaTime);
                ApplyFillAmount(nextFillAmount);
            }

            FollowTarget();
        }

        public void SetTarget(ObjectCharacter newTarget)
        {
            target = newTarget;
            UpdateTargetSize();
            UpdateTargetHealth();
        }

        private void UpdateTargetHealth()
        {
            if (target == null)
            {
                Info("Target is null, cannot update health bar.");
                return;
            }

            if (target.health <= 0)
            {
                targetFillAmount = 0f;
                return;
            }

            Info($"Updating health bar for target: {target.name}");
            Info($"Target health: {target.health}/{target.maxHealth}");
            targetFillAmount = target.maxHealth > 0
                ? Mathf.Clamp01((float)target.health / target.maxHealth)
                : 0f;
            Info($"Target fill amount set to: {targetFillAmount}");
        }

        public void SetFillAmount(float amount)
        {
            if (fillImage == null)
            {
                BuildBar();
            }

            ApplyFillAmount(amount);
        }

        private void ApplyFillAmount(float amount)
        {
            float clampedAmount = Mathf.Clamp01(amount);
            fillImage.fillAmount = clampedAmount;

            RectTransform fillRect = fillImage.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(clampedAmount, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        private void UpdateTargetSize()
        {
            RectTransform root = barRect != null ? barRect : GetComponent<RectTransform>();
            bool isWorldSpace = parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace;

            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = Vector2.zero;

            if (target is IPlayer)
            {
                Vector2 playerSize = isWorldSpace ? playerWorldSize : playerScreenSize;
                root.sizeDelta = playerSize * playerSizeMultiplier;
            }
            else
            {
                root.sizeDelta = isWorldSpace ? enemyWorldSize : enemyScreenSize;
            }
        }

        private void ConfigureEnemyCanvas()
        {
            if (!useCameraSpaceForEnemy || !(target is IEnemy) || parentCanvas == null)
            {
                return;
            }

            parentCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            parentCanvas.worldCamera = Camera.main;
            parentCanvas.planeDistance = cameraPlaneDistance;
        }

        private void FollowTarget()
        {
            if (target == null || parentCanvas == null)
            {
                return;
            }

            Vector3 worldPosition = target.transform.position + worldOffset;
            RectTransform root = barRect != null ? barRect : GetComponent<RectTransform>();

            if (parentCanvas.renderMode == RenderMode.WorldSpace)
            {
                root.position = worldPosition;
                return;
            }

            Camera canvasCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : parentCanvas.worldCamera != null ? parentCanvas.worldCamera : Camera.main;
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(canvasCamera, worldPosition);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.GetComponent<RectTransform>(),
                screenPosition,
                canvasCamera,
                out Vector2 localPosition))
            {
                root.anchoredPosition = localPosition;
            }
        }

        private void BuildBar()
        {
            if (barRect == null)
            {
                barRect = GetBarRect();
            }

            Image backgroundImage = CreateImage("Background", backgroundColor);
            SetFullStretch(backgroundImage.rectTransform);

            fillImage = CreateImage("Fill", fillColor);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 1f;
            SetFullStretch(fillImage.rectTransform);
        }

        private Image CreateImage(string objectName, Color color)
        {
            Transform existing = barRect.Find(objectName);
            if (existing != null)
            {
                Image existingImage = existing.GetComponent<Image>();
                if (existingImage != null)
                {
                    existingImage.color = color;
                    return existingImage;
                }
            }

            GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(barRect, false);
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private RectTransform GetBarRect()
        {
            if (GetComponent<Canvas>() == null)
            {
                return GetComponent<RectTransform>();
            }

            Transform barTransform = transform.Find("Bar");
            if (barTransform == null)
            {
                GameObject barObject = new GameObject("Bar", typeof(RectTransform));
                barTransform = barObject.transform;
                barTransform.SetParent(transform, false);
            }

            ReparentExistingPart("Background", barTransform);
            ReparentExistingPart("Fill", barTransform);

            return barTransform.GetComponent<RectTransform>();
        }

        private void ReparentExistingPart(string objectName, Transform newParent)
        {
            Transform existingPart = transform.Find(objectName);
            if (existingPart != null)
            {
                existingPart.SetParent(newParent, false);
            }
        }

        private static void SetFullStretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}