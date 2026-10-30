using UnityEngine;

namespace ESS.Objects.Enemy
{
    public class BossWingFlap : MonoBehaviour
    {
        [SerializeField] private Transform leftWing;
        [SerializeField] private Transform rightWing;
        [SerializeField] private float flapAngle = 30f;
        [SerializeField] private float flapSpeed = 6f;
        [SerializeField] private float leftWingDirection = 1f;
        [SerializeField] private float rightWingDirection = -1f;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Sprite biteSprite;
        [SerializeField] private float biteDuration = 0.25f;

        private Quaternion leftRestRotation;
        private Quaternion rightRestRotation;
        private Sprite restBodySprite;
        private float biteTimer;

        private void Awake()
        {
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

        private void Update()
        {
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

        public void PlayBite()
        {
            if (bodyRenderer == null || biteSprite == null)
            {
                return;
            }

            bodyRenderer.sprite = biteSprite;
            biteTimer = biteDuration;
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
    }
}