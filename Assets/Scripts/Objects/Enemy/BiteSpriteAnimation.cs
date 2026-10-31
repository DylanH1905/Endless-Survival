using UnityEngine;

namespace ESS.Objects.Enemy
{
    public class BiteSpriteAnimation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite biteSprite;
        [SerializeField] private float biteDuration = 0.2f;

        private Sprite idleSprite;
        private float biteTimer;
        private FocusPlayer focusPlayer;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            }

            if (spriteRenderer != null)
            {
                idleSprite = spriteRenderer.sprite;
            }
        }

        private void Update()
        {
            if (biteTimer <= 0f)
            {
                return;
            }

            biteTimer -= Time.deltaTime;
            if (biteTimer <= 0f && spriteRenderer != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
        }

        private void OnEnable()
        {
            focusPlayer = GetComponent<FocusPlayer>();
            if (focusPlayer != null)
            {
                focusPlayer.AttackStarted += PlayBite;
            }
        }

        private void OnDisable()
        {
            if (focusPlayer != null)
            {
                focusPlayer.AttackStarted -= PlayBite;
                focusPlayer = null;
            }
        }

        public void PlayBite()
        {
            if (spriteRenderer == null || biteSprite == null)
            {
                return;
            }

            spriteRenderer.sprite = biteSprite;
            biteTimer = biteDuration;
        }

        public void SetSprites(Sprite idleSprite, Sprite attackSprite)
        {
            biteSprite = attackSprite;
            if (idleSprite != null)
            {
                this.idleSprite = idleSprite;
                if (spriteRenderer != null && biteTimer <= 0f)
                {
                    spriteRenderer.sprite = idleSprite;
                }
            }
        }
    }
}
