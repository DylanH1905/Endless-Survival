using UnityEngine;

namespace ESS.Objects.Enemy.Bosses
{
    public class Level2 : SystemMonoBehaviour
    {
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        [SerializeField] private float walkAngle = 30f;
        [SerializeField] private float walkSpeed = 6f;
        [SerializeField] private float leftLegDirection = 1f;
        [SerializeField] private float rightLegDirection = -1f;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Sprite bodySprite;
        [SerializeField] private Sprite biteSprite;
        [SerializeField] private float biteDuration = 0.25f;
        [SerializeField] private float scaleMultiplier = 1f;
        [SerializeField] private bool useLegs = true;
        [SerializeField] private Color tint = Color.white;

        private Quaternion leftRestRotation;
        private Quaternion rightRestRotation;
        private Sprite restBodySprite;
        private Boss boss;
        private FocusPlayer focusPlayer;
        private Vector3 baseScale;
        private float biteTimer;
        private bool subscribedToAttack;
        private bool isLevelTwo;

        private void Awake()
        {
            boss = GetComponent<Boss>();
            baseScale = transform.localScale;

            if (leftLeg == null)
            {
                leftLeg = transform.Find("LegLeft");
            }

            if (rightLeg == null)
            {
                rightLeg = transform.Find("LegRight");
            }

            if (bodyRenderer == null)
            {
                Transform body = transform.Find("Body");
                if (body != null)
                {
                    bodyRenderer = body.GetComponent<SpriteRenderer>();
                }
            }

            if (leftLeg != null)
            {
                leftRestRotation = leftLeg.localRotation;
            }

            if (rightLeg != null)
            {
                rightRestRotation = rightLeg.localRotation;
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
            if (!isLevelTwo)
            {
                return;
            }

            float walk = Mathf.Sin(Time.time * walkSpeed) * walkAngle;

            if (leftLeg != null)
            {
                leftLeg.localRotation = leftRestRotation * Quaternion.Euler(0f, 0f, walk * leftLegDirection);
            }

            if (rightLeg != null)
            {
                rightLeg.localRotation = rightRestRotation * Quaternion.Euler(0f, 0f, walk * rightLegDirection);
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
            isLevelTwo = level == 2;
            if (!isLevelTwo)
            {
                SetLegVisibility(leftLeg, false);
                SetLegVisibility(rightLeg, false);
                return;
            }

            transform.localScale = baseScale * Mathf.Max(0.1f, scaleMultiplier);
            SetLegVisibility(leftLeg, useLegs);
            SetLegVisibility(rightLeg, useLegs);

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

        private void SetLegVisibility(Transform Leg, bool visible)
        {
            if (Leg != null)
            {
                Leg.gameObject.SetActive(visible);
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
