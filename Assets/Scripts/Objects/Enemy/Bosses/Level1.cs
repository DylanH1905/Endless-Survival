using UnityEngine;

namespace ESS.Objects.Enemy.Bosses
{
    public class Level1 : SystemMonoBehaviour
    {
        [SerializeField] private Transform leftWing;
        [SerializeField] private Transform rightWing;
        [SerializeField] private float flapAngle = 30f;
        [SerializeField] private float flapSpeed = 6f;
        [SerializeField] private float leftWingDirection = 1f;
        [SerializeField] private float rightWingDirection = -1f;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Sprite bodySprite;
        [SerializeField] private Sprite biteSprite;
        [SerializeField] private float biteDuration = 0.25f;
        [SerializeField] private float scaleMultiplier = 1f;
        [SerializeField] private bool useWings = true;
        [SerializeField] private Color tint = Color.white;

        private Quaternion leftRestRotation;
        private Quaternion rightRestRotation;
        private Sprite restBodySprite;
        private Boss boss;
        private FocusPlayer focusPlayer;
        private Vector3 baseScale;
        private float biteTimer;
        private bool subscribedToAttack;
        private bool isLevelOne;

        private void Awake()
        {
            boss = GetComponent<Boss>();
            baseScale = transform.localScale;

            if (leftWing == null)
            {
                leftWing = transform.Find("WingLeft");
            }

            if (rightWing == null)
            {
                rightWing = transform.Find("WingRight");
            }

            if (bodyRenderer == null)
            {
                Transform body = transform.Find("Body");
                if (body != null)
                {
                    bodyRenderer = body.GetComponent<SpriteRenderer>();
                }
            }

            if (leftWing != null)
            {
                leftRestRotation = leftWing.localRotation;
            }

            if (rightWing != null)
            {
                rightRestRotation = rightWing.localRotation;
            }

            if (bodyRenderer != null)
            {
                restBodySprite = bodyRenderer.sprite;
            }
        }

        private void OnEnable()
        {
            SubscribeToBossLevel();
            SubscribeToAttack();
        }

        private void Start()
        {
            SubscribeToBossLevel();
            SubscribeToAttack();
        }

        private void OnDisable()
        {
            if (boss != null)
            {
                boss.LevelConfigured -= ConfigureForLevel;
            }

            if (subscribedToAttack && focusPlayer != null)
            {
                focusPlayer.AttackStarted -= PlayBite;
                subscribedToAttack = false;
            }
        }

        private void Update()
        {
            if (!isLevelOne)
            {
                return;
            }

            float flap = Mathf.Sin(Time.time * flapSpeed) * flapAngle;

            if (leftWing != null)
            {
                leftWing.localRotation = leftRestRotation * Quaternion.Euler(0f, 0f, flap * leftWingDirection);
            }

            if (rightWing != null)
            {
                rightWing.localRotation = rightRestRotation * Quaternion.Euler(0f, 0f, flap * rightWingDirection);
            }

            if (biteTimer > 0f)
            {
                biteTimer -= Time.deltaTime;
                if (biteTimer <= 0f && bodyRenderer != null)
                {
                    bodyRenderer.sprite = restBodySprite;
                }
            }
        }

        public void SetSprites(Sprite idleSprite, Sprite attackSprite)
        {
            if (idleSprite != null)
            {
                restBodySprite = idleSprite;
                if (bodyRenderer != null && biteTimer <= 0f)
                {
                    bodyRenderer.sprite = idleSprite;
                }
            }

            biteSprite = attackSprite;
        }

        private void SubscribeToBossLevel()
        {
            if (boss == null)
            {
                boss = GetComponent<Boss>();
            }

            if (boss != null)
            {
                boss.LevelConfigured -= ConfigureForLevel;
                boss.LevelConfigured += ConfigureForLevel;
                ConfigureForLevel(boss.CurrentLevel);
            }
        }

        private void ConfigureForLevel(int level)
        {
            isLevelOne = level == 1;
            if (!isLevelOne)
            {
                SetWingVisibility(leftWing, false);
                SetWingVisibility(rightWing, false);
                return;
            }

            transform.localScale = baseScale * Mathf.Max(0.1f, scaleMultiplier);
            SetWingVisibility(leftWing, useWings);
            SetWingVisibility(rightWing, useWings);

            if (bodyRenderer != null)
            {
                if (bodySprite != null)
                {
                    bodyRenderer.sprite = bodySprite;
                }
                bodyRenderer.color = tint;
            }

            SetSprites(bodySprite, biteSprite);
        }

        private void SetWingVisibility(Transform wing, bool visible)
        {
            if (wing != null)
            {
                wing.gameObject.SetActive(visible);
            }
        }

        private void SubscribeToAttack()
        {
            if (focusPlayer == null)
            {
                focusPlayer = GetComponent<FocusPlayer>();
            }

            if (!subscribedToAttack && focusPlayer != null)
            {
                focusPlayer.AttackStarted += PlayBite;
                subscribedToAttack = true;
            }
        }

        private void PlayBite()
        {
            if (bodyRenderer == null || biteSprite == null)
            {
				Warning("Play Bite but not re-render");
                return;
            }
			Info("Play Bite and re-render");
            bodyRenderer.sprite = biteSprite;
            biteTimer = biteDuration;
        }
    }
}
