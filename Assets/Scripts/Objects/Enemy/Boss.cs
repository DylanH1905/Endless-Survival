using UnityEngine;
using ESS.Objects.Enemy;
namespace ESS.Objects.Enemy
{
    public class Boss : IEnemy
    {
        [System.Serializable]
        private class BossForm
        {
            public Sprite bodySprite;
            public Sprite biteSprite;
            [Min(0.1f)] public float scaleMultiplier = 1f;
            public bool useWings = true;
            public Color tint = Color.white;
        }

        [SerializeField] private float powerAttackRange = 6f;
        [SerializeField] private float powerProjectileSpeed = 8f;
        [SerializeField] private BossForm[] bossForms;

        private SpriteRenderer bodyRenderer;
        private Vector3 baseScale;
        private int bossLevel = 1;

        private void Awake()
        {
            baseScale = transform.localScale;
            Transform body = transform.Find("Body");
            bodyRenderer = body != null ? body.GetComponent<SpriteRenderer>() : GetComponent<SpriteRenderer>();

            FocusPlayer focusPlayer = GetComponent<FocusPlayer>();
            if (focusPlayer == null)
            {
                focusPlayer = gameObject.AddComponent<FocusPlayer>();
            }

            focusPlayer.ConfigurePowerAttack(powerAttackRange, powerProjectileSpeed);
        }

        public override void InitializeStats()
        {
            maxHealth = Mathf.RoundToInt(5000f * Mathf.Pow(1.65f, bossLevel - 1));
            attackDamage = Mathf.RoundToInt(50f * Mathf.Pow(1.35f, bossLevel - 1));
            speed = 6f + 0.25f * (bossLevel - 1);

            health = maxHealth;
        }

        public void ConfigureBossLevel(int level)
        {
            bossLevel = Mathf.Max(1, level);
            BossForm form = bossForms != null && bossForms.Length >= bossLevel
                ? bossForms[bossLevel - 1]
                : null;

            transform.localScale = baseScale * (form != null
                ? form.scaleMultiplier
                : 1f + 0.2f * (bossLevel - 1));

            if (form == null)
            {
                return;
            }

            if (bodyRenderer != null)
            {
                if (form.bodySprite != null)
                {
                    bodyRenderer.sprite = form.bodySprite;
                }
                bodyRenderer.color = form.tint;
            }

            SetWingVisibility("WingLeft", form.useWings);
            SetWingVisibility("WingRight", form.useWings);

            FocusPlayer focusPlayer = GetComponent<FocusPlayer>();
            if (focusPlayer != null)
            {
                focusPlayer.SetSprites(form.bodySprite, form.biteSprite);
            }

            BossWingFlap wingFlap = GetComponent<BossWingFlap>();
            if (wingFlap != null)
            {
                wingFlap.SetSprites(form.bodySprite, form.biteSprite);
            }
        }

        private void SetWingVisibility(string wingName, bool visible)
        {
            Transform wing = transform.Find(wingName);
            if (wing != null)
            {
                wing.gameObject.SetActive(visible);
            }
        }
    }
}